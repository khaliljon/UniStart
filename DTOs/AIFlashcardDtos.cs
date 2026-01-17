namespace UniStart.DTOs;

/// <summary>
/// Запрос на генерацию flashcards из текста через AI
/// </summary>
public class GenerateFlashcardsRequest
{
    /// <summary>
    /// Исходный текст для генерации flashcards (лекция, учебник, статья)
    /// </summary>
    public string SourceText { get; set; } = string.Empty;
    
    /// <summary>
    /// Количество flashcards для генерации
    /// </summary>
    public int Count { get; set; } = 10;
    
    /// <summary>
    /// Уровень сложности: easy, medium, hard
    /// </summary>
    public string Difficulty { get; set; } = "medium";
    
    /// <summary>
    /// Язык flashcards (ru, en, etc.)
    /// </summary>
    public string Language { get; set; } = "ru";
    
    /// <summary>
    /// ID набора флешкарт, в который добавить (опционально)
    /// </summary>
    public int? FlashcardSetId { get; set; }
    
    /// <summary>
    /// Название нового набора (если FlashcardSetId не указан)
    /// </summary>
    public string? NewSetTitle { get; set; }
    
    /// <summary>
    /// Предмет/тема (опционально, для контекста)
    /// </summary>
    public string? Subject { get; set; }
}

/// <summary>
/// Результат генерации flashcards
/// </summary>
public class GenerateFlashcardsResponse
{
    /// <summary>
    /// Сгенерированные flashcards
    /// </summary>
    public List<GeneratedFlashcard> Flashcards { get; set; } = new();
    
    /// <summary>
    /// ID созданного набора (если был создан новый)
    /// </summary>
    public int? FlashcardSetId { get; set; }
    
    /// <summary>
    /// Использованная AI модель
    /// </summary>
    public string ModelUsed { get; set; } = string.Empty;
    
    /// <summary>
    /// Количество использованных токенов
    /// </summary>
    public int TokensUsed { get; set; }
    
    /// <summary>
    /// Успешность генерации
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Сообщение об ошибке (если есть)
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Сгенерированная flashcard
/// </summary>
public class GeneratedFlashcard
{
    /// <summary>
    /// Вопрос
    /// </summary>
    public string Question { get; set; } = string.Empty;
    
    /// <summary>
    /// Ответ
    /// </summary>
    public string Answer { get; set; } = string.Empty;
    
    /// <summary>
    /// Варианты ответов для multiple choice (4 варианта, первый - правильный)
    /// </summary>
    public List<string> Options { get; set; } = new();
    
    /// <summary>
    /// Дополнительное объяснение (опционально)
    /// </summary>
    public string? Explanation { get; set; }
    
    /// <summary>
    /// Уровень сложности (1-5)
    /// </summary>
    public int DifficultyLevel { get; set; } = 3;
    
    /// <summary>
    /// Теги/категории
    /// </summary>
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Настройки AI сервиса
/// </summary>
public class AIServiceSettings
{
    /// <summary>
    /// Anthropic API ключ (Claude)
    /// </summary>
    public string? AnthropicApiKey { get; set; }
    
    /// <summary>
    /// OpenAI API ключ (GPT)
    /// </summary>
    public string? OpenAIApiKey { get; set; }
    
    /// <summary>
    /// Google AI API ключ (Gemini)
    /// </summary>
    public string? GoogleAIApiKey { get; set; }
    
    /// <summary>
    /// Предпочитаемая модель: claude-sonnet-4.5, gpt-4o, gemini-2.0-flash
    /// </summary>
    public string PreferredModel { get; set; } = "claude-sonnet-4.5";
    
    /// <summary>
    /// Fallback модель, если предпочитаемая недоступна
    /// </summary>
    public string? FallbackModel { get; set; } = "gpt-4o";
    
    /// <summary>
    /// Максимальное количество токенов в ответе
    /// </summary>
    public int MaxTokens { get; set; } = 4096;
    
    /// <summary>
    /// Температура генерации (0.0 - 1.0)
    /// </summary>
    public double Temperature { get; set; } = 0.7;
}
