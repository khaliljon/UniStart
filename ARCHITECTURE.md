# 🏗️ UniStart - Архитектурные решения

## 📐 ТЕКУЩАЯ АРХИТЕКТУРА

```
UniStart/
├── Controllers/          # API Endpoints (Presentation Layer)
├── Services/            # Business Logic
├── Models/              # Domain Entities
├── DTOs/                # Data Transfer Objects
├── Data/                # Database Context
└── Migrations/          # EF Core Migrations
```

---

## ✅ ЧТО СДЕЛАНО ПРАВИЛЬНО

### 1. Separation of Concerns
✅ **DTOs отделены от Models** - клиент не получает внутренние данные
✅ **Services для бизнес-логики** - контроллеры тонкие
✅ **Dependency Injection** - легко тестировать и заменять реализации

### 2. Database Design
✅ **Правильные связи One-to-Many**
✅ **Cascade Delete настроен**
✅ **DateTime в UTC** - нет проблем с часовыми поясами
✅ **Индексы для производительности**

### 3. Security
✅ **JWT Authentication** - stateless, масштабируемый
✅ **ASP.NET Core Identity** - проверенное решение
✅ **Password hashing** - автоматически через Identity

### 4. Spaced Repetition Algorithm
✅ **SM-2 алгоритм** - научно обоснованный подход
✅ **Правильная реализация** - EaseFactor, Interval, Repetitions
✅ **Гибкость** - можно легко заменить на SM-17 или другой алгоритм

---

## 🔄 РЕКОМЕНДУЕМЫЕ УЛУЧШЕНИЯ

### 1. Repository Pattern

**Зачем:** Абстракция доступа к данным, легче тестировать

**Создайте:**

```csharp
// IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

// FlashcardRepository.cs
public class FlashcardRepository : IRepository<Flashcard>
{
    private readonly ApplicationDbContext _context;
    
    public FlashcardRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Специфичные методы для Flashcard
    public async Task<List<Flashcard>> GetDueForReviewAsync(int setId)
    {
        return await _context.Flashcards
            .Where(f => f.FlashcardSetId == setId)
            .Where(f => f.NextReviewDate == null || f.NextReviewDate <= DateTime.UtcNow)
            .ToListAsync();
    }
}
```

### 2. Result Pattern (вместо исключений)

**Зачем:** Явная обработка ошибок, лучше для API

```csharp
// Result.cs
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    
    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}

// Использование в контроллере
[HttpGet("{id}")]
public async Task<ActionResult<FlashcardSet>> GetFlashcardSet(int id)
{
    var result = await _flashcardService.GetByIdAsync(id);
    
    if (!result.IsSuccess)
        return NotFound(result.Error);
        
    return Ok(result.Data);
}
```

### 3. FluentValidation для DTOs

**Зачем:** Централизованная валидация, повторное использование

```csharp
// Install: dotnet add package FluentValidation.AspNetCore

public class CreateFlashcardDtoValidator : AbstractValidator<CreateFlashcardDto>
{
    public CreateFlashcardDtoValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Вопрос обязателен")
            .MaximumLength(500).WithMessage("Максимум 500 символов");
            
        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Ответ обязателен")
            .MaximumLength(1000);
            
        RuleFor(x => x.FlashcardSetId)
            .GreaterThan(0).WithMessage("Неверный ID набора");
    }
}

// Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateFlashcardDtoValidator>();
```

### 4. Global Exception Handler

**Зачем:** Единообразная обработка ошибок

```csharp
// Middleware/GlobalExceptionHandler.cs
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Произошла ошибка: {Message}", exception.Message);

        var response = new
        {
            status = StatusCodes.Status500InternalServerError,
            message = "Внутренняя ошибка сервера",
            detail = exception.Message
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}

// Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
app.UseExceptionHandler(_ => { });
```

### 5. Response Wrapper

**Зачем:** Единый формат ответов API

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }
    
    public static ApiResponse<T> ErrorResponse(string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = new List<string> { error }
        };
    }
}

// Использование
return Ok(ApiResponse<FlashcardSet>.SuccessResponse(set, "Набор успешно создан"));
```

---

## 🗄️ ОПТИМИЗАЦИЯ БАЗЫ ДАННЫХ

### Добавьте составные индексы

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Для быстрого поиска карточек для повторения
    modelBuilder.Entity<Flashcard>()
        .HasIndex(f => new { f.FlashcardSetId, f.NextReviewDate });
        
    // Для фильтрации тестов
    modelBuilder.Entity<Quiz>()
        .HasIndex(q => new { q.Subject, q.Difficulty, q.IsPublished });
        
    // Для истории попыток пользователя
    modelBuilder.Entity<UserQuizAttempt>()
        .HasIndex(ua => new { ua.UserId, ua.QuizId, ua.CompletedAt });
}
```

### Добавьте UserId к FlashcardSet

```csharp
public class FlashcardSet
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Владелец набора
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public bool IsPublic { get; set; } = false; // Могут ли другие видеть
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public List<Flashcard> Flashcards { get; set; } = new();
}

// Миграция
public partial class AddUserIdToFlashcardSet : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "UserId",
            table: "FlashcardSets",
            type: "text",
            nullable: false,
            defaultValue: "");
            
        migrationBuilder.AddColumn<bool>(
            name: "IsPublic",
            table: "FlashcardSets",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_FlashcardSets_UserId",
            table: "FlashcardSets",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_FlashcardSets_AspNetUsers_UserId",
            table: "FlashcardSets",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
```

---

## 🔒 БЕЗОПАСНОСТЬ

### 1. Добавьте авторизацию к контроллерам

```csharp
[Authorize] // Требует авторизации для всех методов
[ApiController]
[Route("api/[controller]")]
public class FlashcardsController : ControllerBase
{
    // Только владелец может удалить
    [HttpDelete("sets/{id}")]
    public async Task<IActionResult> DeleteFlashcardSet(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var set = await _context.FlashcardSets.FindAsync(id);
        
        if (set == null)
            return NotFound();
            
        if (set.UserId != userId)
            return Forbid(); // 403 Forbidden
            
        _context.FlashcardSets.Remove(set);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
```

### 2. Rate Limiting

```csharp
// Install: dotnet add package AspNetCoreRateLimit

// Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        }
    };
});
builder.Services.AddInMemoryRateLimiting();

app.UseIpRateLimiting();
```

### 3. Refresh Tokens

```csharp
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}

// TokenService
public string GenerateRefreshToken()
{
    var randomNumber = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomNumber);
    return Convert.ToBase64String(randomNumber);
}
```

---

## 📊 МОНИТОРИНГ И ЛОГИРОВАНИЕ

### Serilog Setup

```csharp
// Install: 
// dotnet add package Serilog.AspNetCore
// dotnet add package Serilog.Sinks.Console
// dotnet add package Serilog.Sinks.File

// Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/unistart-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Использование в контроллере
private readonly ILogger<FlashcardsController> _logger;

public FlashcardsController(ILogger<FlashcardsController> logger)
{
    _logger = logger;
}

[HttpPost("cards/review")]
public async Task<ActionResult> ReviewFlashcard(ReviewFlashcardDto dto)
{
    _logger.LogInformation("Пользователь повторяет карточку {CardId} с качеством {Quality}", 
        dto.FlashcardId, dto.Quality);
    // ...
}
```

---

## 🚀 ПРОИЗВОДИТЕЛЬНОСТЬ

### 1. Response Caching

```csharp
// Program.cs
builder.Services.AddResponseCaching();
app.UseResponseCaching();

// Контроллер
[HttpGet]
[ResponseCache(Duration = 60)] // Кеш на 60 секунд
public async Task<ActionResult<List<QuizDto>>> GetQuizzes()
{
    // ...
}
```

### 2. Async всюду

✅ **Уже используете!** Все методы async/await

### 3. Projection вместо Include (когда возможно)

```csharp
// ❌ Плохо - загружает всю сущность
var sets = await _context.FlashcardSets
    .Include(fs => fs.Flashcards)
    .ToListAsync();

// ✅ Хорошо - загружает только нужные поля
var sets = await _context.FlashcardSets
    .Select(fs => new FlashcardSetDto
    {
        Id = fs.Id,
        Title = fs.Title,
        TotalCards = fs.Flashcards.Count
    })
    .ToListAsync();
```

---

## 🧪 ТЕСТИРОВАНИЕ

### Unit Test пример

```csharp
// Install: dotnet add package xUnit
// Install: dotnet add package Moq

public class SpacedRepetitionServiceTests
{
    [Fact]
    public void UpdateFlashcard_Quality5_IncreasesInterval()
    {
        // Arrange
        var service = new SpacedRepetitionService();
        var card = new Flashcard { Repetitions = 2, Interval = 6 };
        
        // Act
        service.UpdateFlashcard(card, 5);
        
        // Assert
        Assert.True(card.Interval > 6);
        Assert.Equal(3, card.Repetitions);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void UpdateFlashcard_LowQuality_ResetsProgress(int quality)
    {
        // Arrange
        var service = new SpacedRepetitionService();
        var card = new Flashcard { Repetitions = 5, Interval = 30 };
        
        // Act
        service.UpdateFlashcard(card, quality);
        
        // Assert
        Assert.Equal(0, card.Repetitions);
        Assert.Equal(0, card.Interval);
    }
}
```

---

## 📱 FRONTEND АРХИТЕКТУРА (Рекомендации)

### Структура React проекта

```
frontend/
├── src/
│   ├── components/          # Переиспользуемые компоненты
│   │   ├── common/         # Button, Input, Card
│   │   ├── flashcards/     # FlashcardItem, FlashcardList
│   │   └── quiz/           # QuizCard, QuestionItem
│   ├── pages/              # Страницы (routes)
│   │   ├── Home.tsx
│   │   ├── Login.tsx
│   │   ├── Dashboard.tsx
│   │   └── FlashcardStudy.tsx
│   ├── services/           # API calls
│   │   ├── api.ts          # Axios instance
│   │   ├── authService.ts
│   │   └── flashcardService.ts
│   ├── hooks/              # Custom hooks
│   │   ├── useAuth.ts
│   │   └── useFlashcards.ts
│   ├── context/            # React Context
│   │   └── AuthContext.tsx
│   ├── types/              # TypeScript types
│   │   └── index.ts
│   └── utils/              # Helpers
│       └── dateFormatter.ts
└── package.json
```

### TypeScript типы (синхронизируйте с DTOs!)

```typescript
// types/index.ts
export interface FlashcardSetDto {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  updatedAt: string;
  totalCards: number;
  cardsToReview: number;
}

export interface CreateFlashcardDto {
  question: string;
  answer: string;
  explanation: string;
  flashcardSetId: number;
}

// Можно автоматически генерировать из C# DTOs!
// Используйте: NSwag, Swagger Codegen, или TypeGen
```

---

## 🎯 ИТОГОВЫЕ РЕКОМЕНДАЦИИ

### Немедленно (сегодня-завтра):
1. ✅ Запустите проект и протестируйте все API
2. ✅ Добавьте UserId к FlashcardSet
3. ✅ Добавьте авторизацию ([Authorize]) к контроллерам

### На этой неделе:
1. Внедрите FluentValidation
2. Добавьте Global Exception Handler
3. Настройте Serilog
4. Создайте seed data для тестирования

### На следующей неделе:
1. Начните React проект
2. Реализуйте аутентификацию на фронте
3. Интегрируйте с Backend API

---

**Ваш проект уже на очень хорошем уровне! 🎉**  
Продолжайте в том же духе, и UniStart станет отличной платформой!
