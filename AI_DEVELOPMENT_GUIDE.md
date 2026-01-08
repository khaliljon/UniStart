# 🔧 Инструкция по дальнейшему развитию AI функционала

## ✅ Что уже реализовано

### 1. ML.NET для адаптивного обучения
- ✅ Сервис `MLPredictionService` с FastTree регрессией
- ✅ Предсказание оптимального времени повторения flashcards
- ✅ Генерация персонального учебного плана
- ✅ Модели данных: `FlashcardReviewData`, `FlashcardReviewPrediction`, `UserLearningPattern`
- ✅ API endpoints в `AdaptiveLearningController`
- ✅ Fallback на SM-2 алгоритм при отсутствии модели

### 2. Рекомендательная система университетов
- ✅ Content-Based Filtering алгоритм
- ✅ Построение профиля пользователя (UserProfile)
- ✅ Расчет Match Score и Admission Probability
- ✅ Кэширование рекомендаций (7 дней)
- ✅ API endpoints в `RecommendationsController`
- ✅ Модель `UniversityRecommendation` с рейтингами

### 3. Интеллектуальные рекомендации контента
- ✅ Рекомендации квизов по слабым сторонам
- ✅ Подбор экзаменов под карьерные цели
- ✅ Рекомендации flashcard наборов
- ✅ Определение следующей темы для изучения
- ✅ Персонализированные советы (tips)
- ✅ Dashboard с комплексными рекомендациями

### 4. Заготовка для AI генерации
- ✅ Интерфейсы и заглушки для OpenAI
- ✅ API endpoints для генерации контента
- ✅ Настройки в appsettings.json
- ⏳ Требуется добавить реальную интеграцию с OpenAI

## 🚀 Следующие шаги для полноценной реализации

### Шаг 1: Накопление данных для ML модели

**Проблема**: ML модель требует минимум 100 примеров для обучения.

**Решение**:
1. Запустите приложение и создайте тестовых пользователей
2. Пусть пользователи проходят квизы, экзамены, повторяют flashcards
3. Через несколько дней накопится достаточно данных
4. Вызовите endpoint переобучения модели:
```bash
POST /api/ai/adaptive-learning/retrain
Authorization: Bearer ADMIN_TOKEN
```

**Альтернатива**: Создайте seed-данные с историческими записями

### Шаг 2: Интеграция с OpenAI API

**Текущий статус**: Заглушка с шаблонными ответами

**Для активации**:

1. Получите API ключ на https://platform.openai.com/
2. Добавьте в `appsettings.json`:
```json
"AI": {
  "OpenAI": {
    "ApiKey": "sk-your-api-key-here",
    "Model": "gpt-4"
  }
}
```

3. Раскомментируйте код в `AIContentGeneratorService.cs`:
```csharp
// Найдите закомментированный метод CallOpenAI
// Раскомментируйте его и добавьте в методы:
// - GenerateQuestions
// - GenerateExplanation
// - GenerateHint
// - GenerateSummary
```

4. Установите NuGet пакет (если нужно):
```bash
dotnet add package Azure.AI.OpenAI
```

### Шаг 3: Улучшение ML модели

**Текущая модель**: FastTree Regression

**Возможные улучшения**:

1. **Добавить больше признаков**:
```csharp
// В FlashcardReviewData.cs добавьте:
public float TimeOfDayPreference { get; set; }     // Предпочтительное время суток
public float SessionLength { get; set; }            // Длина типичной сессии
public float WeekdayVsWeekend { get; set; }        // 0 = будний день, 1 = выходной
public float SubjectDifficulty { get; set; }        // Сложность предмета
```

2. **Эксперимент с другими алгоритмами**:
```csharp
// Попробуйте LightGBM вместо FastTree:
.Append(_mlContext.Regression.Trainers.LightGbm(
    labelColumnName: "OptimalReviewHours",
    numberOfLeaves: 30,
    numberOfIterations: 200
))
```

3. **Cross-validation**:
```csharp
// Добавьте метод валидации модели
var cvResults = _mlContext.Regression.CrossValidate(
    data: trainingData,
    estimator: pipeline,
    numberOfFolds: 5
);

var avgR2 = cvResults.Average(fold => fold.Metrics.RSquared);
_logger.LogInformation("Cross-validation R²: {R2}", avgR2);
```

### Шаг 4: Collaborative Filtering для рекомендаций

**Текущий подход**: Content-Based (только характеристики университетов)

**Добавить**: Collaborative Filtering (что выбрали похожие пользователи)

Создайте новый сервис:
```csharp
// Services/AI/CollaborativeFilteringService.cs
public class CollaborativeFilteringService
{
    // Найти похожих пользователей
    public async Task<List<string>> FindSimilarUsers(string userId, int topN = 10)
    {
        // Косинусное сходство по векторам оценок
        // Векторы: [Math, Physics, Chemistry, ...]
    }
    
    // Рекомендовать на основе выбора похожих пользователей
    public async Task<List<int>> RecommendByCollaborative(string userId, int count = 5)
    {
        var similarUsers = await FindSimilarUsers(userId);
        // Находим университеты, которые выбрали похожие пользователи
    }
}
```

### Шаг 5: A/B тестирование рекомендаций

Добавьте tracking эффективности разных алгоритмов:

```csharp
// Models/Core/RecommendationExperiment.cs
public class RecommendationExperiment
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string AlgorithmVersion { get; set; }  // "v1", "v2", etc.
    public List<int> RecommendedIds { get; set; }
    public int? ChosenId { get; set; }            // Что выбрал пользователь
    public DateTime CreatedAt { get; set; }
}
```

### Шаг 6: Расширенная аналитика

Создайте сервис для анализа эффективности AI:

```csharp
// Services/AI/AIAnalyticsService.cs
public class AIAnalyticsService
{
    // Метрики ML модели
    public async Task<ModelMetrics> GetMLModelMetrics()
    {
        return new ModelMetrics
        {
            Accuracy = 0.85,
            AverageError = 2.5,  // часов
            TotalPredictions = 1000,
            LastTrainingDate = DateTime.UtcNow
        };
    }
    
    // Метрики рекомендаций
    public async Task<RecommendationMetrics> GetRecommendationMetrics()
    {
        // Click-through rate, conversion rate
        // Средний Match Score выбранных университетов
    }
    
    // Анализ пользовательского поведения
    public async Task<UserBehaviorInsights> AnalyzeUserBehavior(string userId)
    {
        // Среднее время сессии, частота посещений
        // Прогресс за период, retention
    }
}
```

### Шаг 7: Background Jobs для автоматического обучения

Используйте Hangfire или Quartz.NET для фоновых задач:

```csharp
// Установка
dotnet add package Hangfire
dotnet add package Hangfire.PostgreSql

// Program.cs
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(connectionString));
builder.Services.AddHangfireServer();

// Регистрация задач
RecurringJob.AddOrUpdate<IMLPredictionService>(
    "retrain-ml-model",
    service => service.RetrainModel(),
    Cron.Weekly  // Еженедельное переобучение
);
```

## 🔍 Как тестировать AI функционал

### 1. Тестирование ML предсказаний

```bash
# 1. Создайте пользователя и авторизуйтесь
POST /api/auth/register
POST /api/auth/login

# 2. Создайте flashcard наборы и повторяйте карточки
GET /api/flashcards/sets
POST /api/flashcards/review

# 3. Получите ML предсказание
GET /api/ai/adaptive-learning/next-review/1
GET /api/ai/adaptive-learning/study-plan

# 4. Проверьте статус модели
GET /api/ai/adaptive-learning/model-status
```

### 2. Тестирование рекомендаций университетов

```bash
# 1. Пройдите несколько квизов
POST /api/quizzes/{id}/attempt

# 2. Установите предпочтения
PUT /api/ai/recommendations/preferences
{
  "preferredCity": "Алматы",
  "maxBudget": 500000,
  "careerGoal": "Программирование"
}

# 3. Получите рекомендации
GET /api/ai/recommendations/universities?topN=10

# 4. Проверьте свой профиль
GET /api/ai/recommendations/profile
```

### 3. Тестирование рекомендаций контента

```bash
# Получите персональные советы
GET /api/ai/content/tips

# Получите рекомендованные квизы
GET /api/ai/content/quizzes/recommended?count=5

# Получите комплексную панель
GET /api/ai/content/dashboard
```

## 📊 Мониторинг производительности

### Важные метрики для отслеживания:

1. **ML модель**:
   - Время обучения модели
   - R² метрика (коэффициент детерминации)
   - Средняя ошибка предсказания (часы)
   - Процент использования ML vs Fallback

2. **Рекомендации**:
   - CTR (Click-Through Rate) рекомендаций
   - Средний Match Score выбранных университетов
   - Время генерации рекомендаций
   - Cache hit rate

3. **Производительность**:
   - Response time API endpoints
   - Database query time
   - ML prediction time

### Добавьте логирование:

```csharp
_logger.LogInformation(
    "ML Prediction: User={UserId}, Card={CardId}, Hours={Hours}, Time={Ms}ms",
    userId, flashcardId, hours, stopwatch.ElapsedMilliseconds
);
```

## 🛡️ Безопасность и ограничения

### Rate Limiting
Добавьте ограничение на AI endpoints:

```csharp
// Install
dotnet add package AspNetCoreRateLimit

// Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*/api/ai/*",
            Limit = 100,
            Period = "1h"
        }
    };
});
```

### Защита от injection
OpenAI вызовы должны санитизировать input:

```csharp
private string SanitizePrompt(string input)
{
    // Удалить потенциально опасные инструкции
    var blocked = new[] { "ignore previous", "system:", "you are now" };
    foreach (var term in blocked)
    {
        input = input.Replace(term, "", StringComparison.OrdinalIgnoreCase);
    }
    return input.Trim();
}
```

## 💡 Идеи для улучшения

### 1. Gamification с AI
- AI подбирает достижения под стиль обучения
- Персональные челленджи на основе слабых сторон
- Прогресс-бары с ML предсказанием времени до мастерства

### 2. Social Learning
- Найти study buddies с похожими целями (ML кластеризация)
- Групповые рекомендации для совместного обучения
- Collaborative фильтрация на основе друзей

### 3. Multimodal AI
- Распознавание рукописных решений (OCR + AI проверка)
- Голосовой ввод вопросов
- Видео-объяснения с AI субтитрами

### 4. Predictive Analytics
- Предсказание финального балла ЕНТ
- Оценка готовности к экзамену
- Рекомендация оптимального времени для сдачи

## 📚 Полезные ресурсы

### ML.NET:
- https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet
- https://docs.microsoft.com/ml-net/

### Рекомендательные системы:
- "Recommender Systems Handbook" (Ricci et al.)
- https://github.com/microsoft/recommenders

### OpenAI:
- https://platform.openai.com/docs
- https://github.com/openai/openai-dotnet

## ❓ FAQ

**Q: Почему ML модель возвращает fallback?**  
A: Модель не обучена. Требуется минимум 100 записей в UserFlashcardProgresses. Используйте POST /api/ai/adaptive-learning/retrain

**Q: Как улучшить точность рекомендаций?**  
A: Пользователь должен пройти больше квизов/экзаменов. Чем больше данных, тем точнее профиль.

**Q: OpenAI генерация не работает?**  
A: Проверьте API ключ в appsettings.json и раскомментируйте код в AIContentGeneratorService.cs

**Q: Как часто переобучать ML модель?**  
A: При активном использовании - раз в неделю. Можно настроить через Hangfire.

---

Удачи в развитии AI функционала! 🚀
