using System.Text.Json;

namespace UniStart.Services.AI;

/// <summary>
/// Заглушка для AI генерации контента
/// В будущем можно интегрировать OpenAI API, Azure OpenAI или другие LLM
/// </summary>
public class AIContentGeneratorService : IAIContentGeneratorService
{
    private readonly ILogger<AIContentGeneratorService> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _isConfigured;

    public AIContentGeneratorService(
        ILogger<AIContentGeneratorService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Проверяем, настроен ли API ключ для OpenAI или другого сервиса
        var apiKey = _configuration["AI:OpenAI:ApiKey"];
        _isConfigured = !string.IsNullOrEmpty(apiKey);
        
        if (!_isConfigured)
        {
            _logger.LogWarning("AI генерация контента не настроена. Добавьте AI:OpenAI:ApiKey в appsettings.json");
        }
    }

    public bool IsAvailable()
    {
        return _isConfigured;
    }

    public async Task<List<GeneratedQuestion>> GenerateQuestions(string subject, string difficulty, int count = 5)
    {
        try
        {
            if (!_isConfigured)
            {
                _logger.LogWarning("AI сервис не настроен, возвращаем шаблонные вопросы");
                return GenerateTemplateQuestions(subject, difficulty, count);
            }

            // TODO: Интеграция с OpenAI API
            // var prompt = $"Generate {count} {difficulty} level multiple choice questions about {subject}";
            // var response = await CallOpenAI(prompt);
            
            _logger.LogInformation("Генерация {Count} вопросов по теме {Subject}, уровень {Difficulty}", 
                count, subject, difficulty);

            // Временная заглушка
            await Task.Delay(100); // Имитация API вызова
            return GenerateTemplateQuestions(subject, difficulty, count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации вопросов");
            return new List<GeneratedQuestion>();
        }
    }

    public async Task<string> GenerateExplanation(string question, string correctAnswer)
    {
        try
        {
            if (!_isConfigured)
            {
                return $"Правильный ответ: {correctAnswer}. Для получения подробного объяснения настройте AI сервис.";
            }

            // TODO: Интеграция с OpenAI API
            // var prompt = $"Explain why '{correctAnswer}' is the correct answer to: {question}";
            // return await CallOpenAI(prompt);

            _logger.LogInformation("Генерация объяснения для вопроса");
            await Task.Delay(50);
            
            return $"Правильный ответ: {correctAnswer}. " +
                   $"Это объяснение будет сгенерировано AI после настройки OpenAI API.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации объяснения");
            return "Объяснение временно недоступно";
        }
    }

    public async Task<string> GenerateHint(string question, string userAnswer)
    {
        try
        {
            if (!_isConfigured)
            {
                return "Подумайте еще раз и попробуйте другой вариант ответа.";
            }

            // TODO: Интеграция с OpenAI API
            // var prompt = $"User answered '{userAnswer}' to question: {question}. Give a helpful hint without revealing the answer.";
            
            _logger.LogInformation("Генерация подсказки");
            await Task.Delay(50);
            
            return "Проанализируйте вопрос внимательнее. Обратите внимание на ключевые слова.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации подсказки");
            return "Попробуйте еще раз";
        }
    }

    public async Task<string> GenerateSummary(string topic, string content)
    {
        try
        {
            if (!_isConfigured)
            {
                return $"Краткое резюме по теме '{topic}' будет доступно после настройки AI сервиса.";
            }

            // TODO: Интеграция с OpenAI API
            // var prompt = $"Summarize the following content about {topic}: {content}";
            
            _logger.LogInformation("Генерация резюме для темы {Topic}", topic);
            await Task.Delay(50);
            
            return $"Резюме по теме '{topic}': материал охватывает основные концепции и практические примеры.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации резюме");
            return "Резюме недоступно";
        }
    }

    /// <summary>
    /// Генерирует шаблонные вопросы для демонстрации (без AI)
    /// </summary>
    private List<GeneratedQuestion> GenerateTemplateQuestions(string subject, string difficulty, int count)
    {
        var questions = new List<GeneratedQuestion>();
        
        for (int i = 1; i <= count; i++)
        {
            questions.Add(new GeneratedQuestion
            {
                Question = $"[{difficulty}] Вопрос {i} по предмету {subject}",
                Options = new List<string>
                {
                    "Вариант А",
                    "Вариант Б",
                    "Вариант В",
                    "Вариант Г"
                },
                CorrectAnswer = "Вариант А",
                Explanation = "Это шаблонный вопрос. Настройте OpenAI API для генерации реальных вопросов."
            });
        }

        return questions;
    }

    // Для будущей интеграции с OpenAI
    /*
    private async Task<string> CallOpenAI(string prompt)
    {
        var apiKey = _configuration["AI:OpenAI:ApiKey"];
        var endpoint = _configuration["AI:OpenAI:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
        
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        
        var requestBody = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "system", content = "You are an educational AI assistant helping students prepare for university entrance exams." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 1000
        };
        
        var response = await httpClient.PostAsJsonAsync(endpoint, requestBody);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>();
        return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
    }
    */
}
