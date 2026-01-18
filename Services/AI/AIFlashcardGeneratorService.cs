using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using AnthropicMessage = Anthropic.SDK.Messaging.Message;
using Microsoft.Extensions.Options;
using System.Text.Json;
using UniStart.DTOs;
using Mscc.GenerativeAI;

namespace UniStart.Services.AI;

public interface IAIFlashcardGeneratorService
{
    Task<GenerateFlashcardsResponse> GenerateFlashcardsAsync(GenerateFlashcardsRequest request);
    Task<bool> IsConfiguredAsync();
    Task<List<string>> GetAvailableModelsAsync();
}

public class AIFlashcardGeneratorService : IAIFlashcardGeneratorService
{
    private readonly AIServiceSettings _settings;
    private readonly ILogger<AIFlashcardGeneratorService> _logger;
    private readonly AnthropicClient? _anthropicClient;
    private readonly GoogleAI? _geminiClient;

    public AIFlashcardGeneratorService(
        IOptions<AIServiceSettings> settings,
        ILogger<AIFlashcardGeneratorService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Инициализируем Claude клиент, если есть API ключ
        if (!string.IsNullOrEmpty(_settings.AnthropicApiKey))
        {
            _anthropicClient = new AnthropicClient(new APIAuthentication(_settings.AnthropicApiKey));
            _logger.LogInformation("✅ Claude Sonnet 4.5 клиент инициализирован");
        }

        // Инициализируем Gemini клиент (БЕСПЛАТНЫЙ fallback - 1500 запросов/день!)
        if (!string.IsNullOrEmpty(_settings.GoogleAIApiKey))
        {
            _geminiClient = new GoogleAI(_settings.GoogleAIApiKey);
            _logger.LogInformation("✅ Gemini 2.0 Flash клиент инициализирован (БЕСПЛАТНО: 1500 запросов/день)");
        }

        if (_anthropicClient == null && _geminiClient == null)
        {
            _logger.LogWarning("⚠️ Ни один AI API ключ не настроен. Генерация флешкарт недоступна.");
        }
    }

    public async Task<GenerateFlashcardsResponse> GenerateFlashcardsAsync(GenerateFlashcardsRequest request)
    {
        try
        {
            // Проверяем наличие AI сервисов (приоритет: Claude → Gemini)
            if (_anthropicClient == null && _geminiClient == null)
            {
                return new GenerateFlashcardsResponse
                {
                    Success = false,
                    ErrorMessage = "AI сервис не настроен. Добавьте Gemini API ключ (БЕСПЛАТНО!) или Claude API ключ в appsettings.json"
                };
            }

            // Ограничиваем размер входного текста (максимум ~8000 слов / ~50000 символов)
            if (request.SourceText.Length > 50000)
            {
                return new GenerateFlashcardsResponse
                {
                    Success = false,
                    ErrorMessage = "Текст слишком длинный. Максимум 50000 символов (примерно 8000 слов). Разбейте на части."
                };
            }

            _logger.LogInformation("Генерация {Count} flashcards из текста длиной {Length} символов", 
                request.Count, request.SourceText.Length);

            // Создаем промпты
            var systemPrompt = CreateSystemPrompt(request);
            var userPrompt = CreateUserPrompt(request);

            // Используем Claude если доступен, иначе Gemini (бесплатный!)
            if (_anthropicClient != null)
            {
                return await GenerateWithClaudeAsync(request, systemPrompt, userPrompt);
            }
            else
            {
                return await GenerateWithGeminiAsync(request, systemPrompt, userPrompt);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации flashcards через AI");
            
            return new GenerateFlashcardsResponse
            {
                Success = false,
                ErrorMessage = $"Ошибка генерации: {ex.Message}"
            };
        }
    }

    private async Task<GenerateFlashcardsResponse> GenerateWithClaudeAsync(
        GenerateFlashcardsRequest request, string systemPrompt, string userPrompt)
    {
        var messages = new List<AnthropicMessage>
        {
            new AnthropicMessage(RoleType.User, userPrompt)
        };

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = _settings.MaxTokens,
            Model = AnthropicModels.Claude35Sonnet,
            Temperature = (decimal)_settings.Temperature,
            System = new List<SystemMessage> { new SystemMessage(systemPrompt) }
        };

        var response = await _anthropicClient!.Messages.GetClaudeMessageAsync(parameters);
        var flashcards = ParseClaudeResponse(response.Content.ToString() ?? "");

        return new GenerateFlashcardsResponse
        {
            Flashcards = flashcards,
            ModelUsed = "claude-3.5-sonnet",
            TokensUsed = response.Usage.InputTokens + response.Usage.OutputTokens,
            Success = true
        };
    }

    private async Task<GenerateFlashcardsResponse> GenerateWithGeminiAsync(
        GenerateFlashcardsRequest request, string systemPrompt, string userPrompt)
    {
        try
        {
            _logger.LogInformation("🚀 Начало генерации через Gemini 2.5 Flash");
            
            // Используем gemini-2.5-flash - новейшая быстрая модель (ноябрь 2025)
            var model = _geminiClient!.GenerativeModel(model: "gemini-2.5-flash");
            var fullPrompt = $"{systemPrompt}\n\n{userPrompt}";
            
            _logger.LogInformation("📝 Промпт подготовлен. Длина: {Length} символов", fullPrompt.Length);

            // Настройки генерации с увеличенным timeout
            var generationConfig = new GenerationConfig
            {
                Temperature = (float)_settings.Temperature,
                MaxOutputTokens = _settings.MaxTokens
            };

            var requestOptions = new RequestOptions(
                retry: null, 
                timeout: TimeSpan.FromMinutes(3) // 3 минуты для длинных текстов
            );

            _logger.LogInformation("⏳ Отправка запроса в Gemini API...");
            
            var response = await model.GenerateContent(
                prompt: fullPrompt, 
                generationConfig: generationConfig,
                requestOptions: requestOptions
            );
            
            _logger.LogInformation("✅ Получен ответ от Gemini. Candidates: {Count}", response.Candidates?.Count ?? 0);
            
            if (response.Candidates == null || !response.Candidates.Any())
            {
                var blockReason = response.PromptFeedback?.BlockReason.ToString() ?? "unknown";
                _logger.LogError("❌ Gemini не вернул candidates. BlockReason: {Reason}", blockReason);
                throw new Exception($"Gemini API вернул пустой ответ. Причина: {blockReason}");
            }
            
            // Получаем текст из первого Part первого Candidate
            string? contentText = null;
            
            if (response.Candidates?.Count > 0 && 
                response.Candidates[0]?.Content?.Parts?.Count > 0)
            {
                var firstPart = response.Candidates[0].Content.Parts[0];
                contentText = firstPart?.Text;
                
                _logger.LogInformation("📄 Текст из Parts[0]: {Length} символов. Part Type: {Type}", 
                    contentText?.Length ?? 0, 
                    firstPart?.GetType().Name ?? "null");
            }
            else
            {
                // Fallback на response.Text если есть
                contentText = response.Text;
                _logger.LogWarning("⚠️ Используем response.Text fallback. Длина: {Length}", contentText?.Length ?? 0);
            }
            
            if (string.IsNullOrEmpty(contentText))
            {
                _logger.LogError("❌ Текст ответа пустой. response.Text: {ResponseText}, Parts: {Parts}", 
                    response.Text?.Length ?? 0,
                    response.Candidates?[0]?.Content?.Parts?.Count ?? 0);
                throw new Exception("Empty response from Gemini API");
            }

            var flashcards = ParseClaudeResponse(contentText); // Тот же парсер работает!

            _logger.LogInformation("✨ Gemini сгенерировал {Count} карточек", flashcards.Count);

            return new GenerateFlashcardsResponse
            {
                Flashcards = flashcards,
                ModelUsed = "gemini-2.5-flash",
                TokensUsed = 0, // Gemini не возвращает точный подсчёт в бесплатной версии
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при генерации через Gemini");
            throw;
        }
    }

    public Task<bool> IsConfiguredAsync()
    {
        return Task.FromResult(_anthropicClient != null || _geminiClient != null);
    }

    public Task<List<string>> GetAvailableModelsAsync()
    {
        var models = new List<string>();
        
        if (_anthropicClient != null)
            models.Add("claude-3.5-sonnet");
        
        if (_geminiClient != null)
            models.Add("gemini-2.5-flash");
        
        if (!string.IsNullOrEmpty(_settings.OpenAIApiKey))
            models.Add("gpt-4o");

        return Task.FromResult(models);
    }

    private string CreateSystemPrompt(GenerateFlashcardsRequest request)
    {
        var difficultyDesc = request.Difficulty switch
        {
            "easy" => "простые вопросы для начинающих",
            "hard" => "сложные вопросы для углубленного изучения",
            _ => "вопросы среднего уровня сложности"
        };

        return $@"Ты — эксперт по созданию образовательных flashcards (карточек для запоминания).

Твоя задача:
1. Создать {request.Count} качественных flashcards из предоставленного текста
2. Каждая карточка должна иметь вопрос и 4 варианта ответа (multiple choice)
3. Вопросы должны быть конкретными и {difficultyDesc}
4. Один из 4 вариантов - правильный ответ, остальные 3 - правдоподобные неправильные
5. Добавить краткое объяснение, если это помогает пониманию
6. Язык: {request.Language}

Формат ответа - СТРОГО JSON массив:
[
  {{
    ""question"": ""Вопрос здесь?"",
    ""answer"": ""Правильный ответ"",
    ""options"": [
      ""Правильный ответ"",
      ""Неправильный 1"",
      ""Неправильный 2"",
      ""Неправильный 3""
    ],
    ""explanation"": ""Дополнительное объяснение (опционально)"",
    ""difficultyLevel"": 1-5,
    ""tags"": [""тег1"", ""тег2""]
  }}
]

ВАЖНО: 
- Первый элемент в массиве options ДОЛЖЕН быть правильным ответом (совпадать с answer)
- Возвращай ТОЛЬКО валидный JSON массив, без дополнительного текста!";
    }

    private string CreateUserPrompt(GenerateFlashcardsRequest request)
    {
        var subjectContext = string.IsNullOrEmpty(request.Subject) 
            ? "" 
            : $"Предмет/тема: {request.Subject}\n\n";

        return $@"{subjectContext}Исходный текст для создания flashcards:

{request.SourceText}

Создай {request.Count} flashcards уровня ""{request.Difficulty}"" из этого текста.";
    }

    private List<GeneratedFlashcard> ParseClaudeResponse(string responseText)
    {
        try
        {
            _logger.LogInformation("🔍 Парсинг ответа AI. Длина: {Length} символов", responseText.Length);
            
            // Извлекаем JSON из ответа (Claude иногда добавляет текст вокруг)
            var jsonStart = responseText.IndexOf('[');
            var jsonEnd = responseText.LastIndexOf(']');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonText = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                _logger.LogInformation("📝 Извлеченный JSON: {Json}", jsonText.Length > 500 ? jsonText.Substring(0, 500) + "..." : jsonText);
                
                var flashcards = JsonSerializer.Deserialize<List<GeneratedFlashcard>>(jsonText, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                _logger.LogInformation("✅ Успешно распарсено {Count} карточек", flashcards?.Count ?? 0);
                
                if (flashcards != null && flashcards.Count > 0)
                {
                    _logger.LogInformation("📊 Первая карточка: Q={Question}, A={Answer}, Options={OptionsCount}", 
                        flashcards[0].Question, flashcards[0].Answer, flashcards[0].Options?.Count ?? 0);
                }
                
                return flashcards ?? new List<GeneratedFlashcard>();
            }

            _logger.LogWarning("⚠️ Не удалось найти JSON массив в ответе AI. Ответ: {Response}", 
                responseText.Length > 200 ? responseText.Substring(0, 200) + "..." : responseText);
            return new List<GeneratedFlashcard>();
        }
        catch (Exception ex)
        {
            var preview = responseText.Length > 200 ? "..." + responseText.Substring(responseText.Length - 200) : responseText;
            _logger.LogError(ex, "❌ Ошибка парсинга ответа от AI. Длина: {Length} символов. Конец ответа: {Preview}", 
                responseText.Length, preview);
            return new List<GeneratedFlashcard>();
        }
    }
}
