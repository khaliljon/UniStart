using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using UniStart.Data;
using UniStart.Models.Core;
using UniStart.Models.Learning;

namespace UniStart.Services.AI;

/// <summary>
/// ML.NET сервис для предсказания оптимального времени повторения карточек
/// Использует FastTree регрессию для обучения на истории пользователей
/// </summary>
public class MLPredictionService : IMLPredictionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MLPredictionService> _logger;
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private readonly string _modelPath;
    private readonly object _modelLock = new();

    public MLPredictionService(
        ApplicationDbContext context,
        ILogger<MLPredictionService> logger,
        IWebHostEnvironment env)
    {
        _context = context;
        _logger = logger;
        _mlContext = new MLContext(seed: 42);
        _modelPath = Path.Combine(env.ContentRootPath, "Models", "flashcard_review_model.zip");
        
        LoadModel();
    }

    public bool IsModelTrained()
    {
        return _model != null;
    }

    public async Task<FlashcardReviewPrediction?> PredictNextReviewTime(string userId, int flashcardId)
    {
        try
        {
            // Если модель не обучена, используем fallback на старый SM-2
            if (_model == null)
            {
                _logger.LogWarning("ML модель не обучена, используется fallback на SM-2");
                return await FallbackToStaticAlgorithm(userId, flashcardId);
            }

            // Получаем текущий прогресс пользователя по карточке
            var progress = await _context.UserFlashcardProgresses
                .Include(p => p.Flashcard)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.FlashcardId == flashcardId);

            if (progress == null)
            {
                _logger.LogWarning("Прогресс не найден для User={UserId}, Flashcard={FlashcardId}", userId, flashcardId);
                return null;
            }

            // Получаем паттерны обучения пользователя
            var learningPattern = await _context.UserLearningPatterns
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (learningPattern == null)
            {
                // Создаем паттерн с дефолтными значениями
                learningPattern = new UserLearningPattern
                {
                    UserId = userId,
                    AverageRetentionRate = 70.0,
                    ForgettingSpeed = 1.0,
                    SessionsProcessed = 0
                };
            }

            // Подготавливаем данные для предсказания
            var inputData = new FlashcardReviewData
            {
                EaseFactor = (float)progress.EaseFactor,
                Interval = progress.Interval,
                Repetitions = progress.Repetitions,
                DaysSinceLastReview = progress.LastReviewedAt.HasValue 
                    ? (float)(DateTime.UtcNow - progress.LastReviewedAt.Value).TotalDays 
                    : 0f,
                UserRetentionRate = (float)learningPattern.AverageRetentionRate,
                UserForgettingSpeed = (float)learningPattern.ForgettingSpeed,
                CorrectAfterBreak = CalculateCorrectAfterBreak(userId, flashcardId),
                IsMastered = progress.IsMastered,
                OptimalReviewHours = 0 // Это предсказываемое значение
            };

            // Делаем предсказание
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<FlashcardReviewData, FlashcardReviewPredictionResult>(_model);
            var prediction = predictionEngine.Predict(inputData);

            // Ограничиваем предсказание разумными пределами (1 час - 1 год)
            var optimalHours = Math.Max(1, Math.Min(8760, (int)prediction.Score));

            // Рассчитываем уверенность (упрощенная версия)
            var confidence = learningPattern.SessionsProcessed > 10 ? 0.85f : 0.5f;

            var result = new FlashcardReviewPrediction
            {
                FlashcardId = flashcardId,
                UserId = userId,
                OptimalReviewHours = optimalHours,
                Confidence = confidence,
                RecommendedReviewDate = DateTime.UtcNow.AddHours(optimalHours),
                Reason = GenerateReason(optimalHours, inputData),
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("ML предсказание: User={UserId}, Card={CardId}, Hours={Hours}, Confidence={Conf}",
                userId, flashcardId, optimalHours, confidence);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при ML предсказании для User={UserId}, Flashcard={FlashcardId}", userId, flashcardId);
            return await FallbackToStaticAlgorithm(userId, flashcardId);
        }
    }

    public async Task<List<FlashcardReviewPrediction>> GenerateStudyPlan(string userId)
    {
        try
        {
            _logger.LogInformation("Генерация учебного плана для User={UserId}", userId);

            // Получаем все карточки пользователя, которые нужно повторить
            var now = DateTime.UtcNow;
            var dueTodayProgresses = await _context.UserFlashcardProgresses
                .Where(p => p.UserId == userId && p.NextReviewDate <= now.AddHours(24))
                .OrderBy(p => p.NextReviewDate)
                .Take(50) // Ограничиваем максимум 50 карточками в день
                .ToListAsync();

            var studyPlan = new List<FlashcardReviewPrediction>();

            foreach (var progress in dueTodayProgresses)
            {
                var prediction = await PredictNextReviewTime(userId, progress.FlashcardId);
                if (prediction != null)
                {
                    studyPlan.Add(prediction);
                }
            }

            // Сортируем по приоритету: сначала просроченные, потом по оптимальному времени
            studyPlan = studyPlan
                .OrderBy(p => p.OptimalReviewHours)
                .ToList();

            _logger.LogInformation("Сгенерирован план обучения на {Count} карточек для User={UserId}", 
                studyPlan.Count, userId);

            return studyPlan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации плана обучения для User={UserId}", userId);
            return new List<FlashcardReviewPrediction>();
        }
    }

    public async Task<bool> RetrainModel()
    {
        try
        {
            _logger.LogInformation("Начинается переобучение ML модели...");

            // Собираем тренировочные данные из истории всех пользователей
            var trainingData = await PrepareTrainingData();

            if (trainingData.Count < 100)
            {
                _logger.LogWarning("Недостаточно данных для обучения ({Count} записей), требуется минимум 100", trainingData.Count);
                return false;
            }

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Определяем pipeline обучения
            var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(FlashcardReviewData.EaseFactor),
                    nameof(FlashcardReviewData.Interval),
                    nameof(FlashcardReviewData.Repetitions),
                    nameof(FlashcardReviewData.DaysSinceLastReview),
                    nameof(FlashcardReviewData.UserRetentionRate),
                    nameof(FlashcardReviewData.UserForgettingSpeed),
                    nameof(FlashcardReviewData.CorrectAfterBreak))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Regression.Trainers.FastTree(
                    labelColumnName: nameof(FlashcardReviewData.OptimalReviewHours),
                    numberOfLeaves: 20,
                    numberOfTrees: 100,
                    minimumExampleCountPerLeaf: 10));

            // Обучаем модель
            _logger.LogInformation("Обучение модели на {Count} примерах...", trainingData.Count);
            var trainedModel = pipeline.Fit(dataView);

            // Сохраняем модель
            lock (_modelLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);
                _mlContext.Model.Save(trainedModel, dataView.Schema, _modelPath);
                _model = trainedModel;
            }

            _logger.LogInformation("ML модель успешно переобучена и сохранена в {Path}", _modelPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при переобучении ML модели");
            return false;
        }
    }

    private void LoadModel()
    {
        try
        {
            if (File.Exists(_modelPath))
            {
                lock (_modelLock)
                {
                    _model = _mlContext.Model.Load(_modelPath, out _);
                }
                _logger.LogInformation("ML модель загружена из {Path}", _modelPath);
            }
            else
            {
                _logger.LogWarning("ML модель не найдена по пути {Path}. Требуется первичное обучение.", _modelPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке ML модели");
        }
    }

    private async Task<List<FlashcardReviewData>> PrepareTrainingData()
    {
        // Берем историю за последние 90 дней для актуальности
        var cutoffDate = DateTime.UtcNow.AddDays(-90);

        var progresses = await _context.UserFlashcardProgresses
            .Where(p => p.LastReviewedAt >= cutoffDate && p.Repetitions > 0)
            .Include(p => p.User)
            .ToListAsync();

        var userPatterns = await _context.UserLearningPatterns
            .ToDictionaryAsync(p => p.UserId);

        var trainingData = new List<FlashcardReviewData>();

        foreach (var progress in progresses)
        {
            var pattern = userPatterns.GetValueOrDefault(progress.UserId);
            if (pattern == null) continue;

            // Рассчитываем фактическое оптимальное время (основываясь на Interval)
            var optimalHours = progress.Interval * 24; // Конвертируем дни в часы

            trainingData.Add(new FlashcardReviewData
            {
                EaseFactor = (float)progress.EaseFactor,
                Interval = progress.Interval,
                Repetitions = progress.Repetitions,
                DaysSinceLastReview = progress.LastReviewedAt.HasValue
                    ? (float)(DateTime.UtcNow - progress.LastReviewedAt.Value).TotalDays
                    : 0f,
                UserRetentionRate = (float)pattern.AverageRetentionRate,
                UserForgettingSpeed = (float)pattern.ForgettingSpeed,
                CorrectAfterBreak = CalculateCorrectAfterBreak(progress.UserId, progress.FlashcardId),
                IsMastered = progress.IsMastered,
                OptimalReviewHours = optimalHours
            });
        }

        return trainingData;
    }

    private float CalculateCorrectAfterBreak(string userId, int flashcardId)
    {
        // Упрощенная метрика: количество успешных повторений после пропусков
        var progress = _context.UserFlashcardProgresses
            .FirstOrDefault(p => p.UserId == userId && p.FlashcardId == flashcardId);

        return progress?.Repetitions ?? 0;
    }

    private string GenerateReason(int hours, FlashcardReviewData data)
    {
        if (data.IsMastered)
            return "Карточка освоена, редкие повторения для закрепления";

        if (data.Repetitions < 3)
            return "Начальная стадия изучения, частые повторения";

        if (data.UserRetentionRate > 80)
            return "Высокая скорость запоминания, интервал увеличен";

        if (data.UserForgettingSpeed > 2)
            return "Быстрое забывание, требуется чаще повторять";

        return "Оптимальный интервал на основе вашей истории обучения";
    }

    private async Task<FlashcardReviewPrediction?> FallbackToStaticAlgorithm(string userId, int flashcardId)
    {
        // Fallback на старый SM-2 алгоритм
        var progress = await _context.UserFlashcardProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.FlashcardId == flashcardId);

        if (progress == null) return null;

        var hours = progress.Interval * 24; // Дни в часы

        return new FlashcardReviewPrediction
        {
            FlashcardId = flashcardId,
            UserId = userId,
            OptimalReviewHours = hours,
            Confidence = 0.3f, // Низкая уверенность для fallback
            RecommendedReviewDate = DateTime.UtcNow.AddHours(hours),
            Reason = "Стандартный алгоритм повторения (SM-2)",
            CreatedAt = DateTime.UtcNow
        };
    }
}
