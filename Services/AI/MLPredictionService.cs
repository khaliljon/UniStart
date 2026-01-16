using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using UniStart.Repositories;
using UniStart.Models.Core;
using UniStart.Models.Learning;

namespace UniStart.Services.AI;

/// <summary>
/// ML.NET сервис для предсказания оптимального времени повторения карточек
/// Использует FastTree регрессию для обучения на истории пользователей
/// </summary>
public class MLPredictionService : IMLPredictionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MLPredictionService> _logger;
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<FlashcardReviewData, FlashcardReviewPredictionResult>? _predictionEngine;
    private readonly string _modelPath;
    private readonly object _modelLock = new();

    public MLPredictionService(
        IUnitOfWork unitOfWork,
        ILogger<MLPredictionService> logger,
        IWebHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
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
            var progress = await _unitOfWork.FlashcardProgress.Query()
                .Include(p => p.Flashcard)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.FlashcardId == flashcardId);

            if (progress == null)
            {
                _logger.LogWarning("Прогресс не найден для User={UserId}, Flashcard={FlashcardId}", userId, flashcardId);
                return null;
            }

            // Получаем паттерны обучения пользователя
            var learningPattern = await _unitOfWork.Repository<UserLearningPattern>()
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
                OptimalReviewHours = 0
            };

            // Валидируем входные данные
            if (!ValidateMLInput(inputData))
            {
                _logger.LogWarning(
                    "ML входные данные некорректны для User={UserId}, Card={CardId}. Fallback на SM-2",
                    userId, flashcardId);
                return await FallbackToStaticAlgorithm(userId, flashcardId);
            }

            // Делаем предсказание с кэшированным PredictionEngine
            if (_predictionEngine == null)
            {
                _logger.LogError("PredictionEngine не инициализирован");
                return await FallbackToStaticAlgorithm(userId, flashcardId);
            }

            var prediction = _predictionEngine.Predict(inputData);

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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "ML модель не готова для User={UserId}, Flashcard={FlashcardId}. Переход на статический алгоритм", userId, flashcardId);
            return await FallbackToStaticAlgorithm(userId, flashcardId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Некорректные входные данные для ML предсказания User={UserId}, Flashcard={FlashcardId}", userId, flashcardId);
            return await FallbackToStaticAlgorithm(userId, flashcardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при ML предсказании User={UserId}, Flashcard={FlashcardId}", userId, flashcardId);
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
            var dueTodayProgresses = await _unitOfWork.FlashcardProgress.Query()
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
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Некорректный userId для генерации плана обучения");
            return new List<FlashcardReviewPrediction>();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка БД при генерации плана обучения для User={UserId}", userId);
            return new List<FlashcardReviewPrediction>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при генерации плана обучения для User={UserId}", userId);
            return new List<FlashcardReviewPrediction>();
        }
    }

    public async Task<bool> RetrainModelAsync()
    {
        return await RetrainModel();
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

            // Разделяем данные на train (80%) и validation (20%)
            var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2, seed: 42);
            var trainSet = trainTestSplit.TrainSet;
            var validationSet = trainTestSplit.TestSet;

            _logger.LogInformation(
                "Данные разделены: {Train} train, {Val} validation", 
                trainingData.Count * 0.8, 
                trainingData.Count * 0.2
            );

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

            // Обучаем модель на train set
            _logger.LogInformation("Обучение модели на {Count} примерах...", (int)(trainingData.Count * 0.8));
            var trainedModel = pipeline.Fit(trainSet);

            // Оценка модели на validation set
            var predictions = trainedModel.Transform(validationSet);
            var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: nameof(FlashcardReviewData.OptimalReviewHours));

            _logger.LogInformation(
                "Метрики модели на validation set: MAE={MAE:F2}, RMSE={RMSE:F2}, R²={R2:F3}",
                metrics.MeanAbsoluteError,
                metrics.RootMeanSquaredError,
                metrics.RSquared
            );

            // Проверяем качество модели (R² должен быть > 0.5 для production)
            if (metrics.RSquared < 0.5)
            {
                _logger.LogWarning(
                    "Модель имеет низкое качество (R²={R2:F3} < 0.5). Рекомендуется больше данных для обучения.",
                    metrics.RSquared
                );
            }

            // Сохраняем модель и пересоздаём PredictionEngine
            lock (_modelLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);
                _mlContext.Model.Save(trainedModel, dataView.Schema, _modelPath);
                _model = trainedModel;
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<FlashcardReviewData, FlashcardReviewPredictionResult>(_model);
            }

            _logger.LogInformation("ML модель и PredictionEngine успешно переобучены и сохранены в {Path}", _modelPath);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка конфигурации ML pipeline при переобучении модели");
            return false;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Некорректные данные для обучения ML модели");
            return false;
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Ошибка ввода-вывода при сохранении ML модели в {Path}", _modelPath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при переобучении ML модели");
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
                    _model = _mlContext.Model.Load(_modelPath, out var schema);
                    _predictionEngine = _mlContext.Model.CreatePredictionEngine<FlashcardReviewData, FlashcardReviewPredictionResult>(_model);
                }
                _logger.LogInformation("ML модель и PredictionEngine загружены из {Path}", _modelPath);
            }
            else
            {
                _logger.LogWarning("ML модель не найдена по пути {Path}. Требуется первичное обучение.", _modelPath);
            }
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "Файл ML модели не найден: {Path}", _modelPath);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Некорректный формат ML модели в {Path}", _modelPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при загрузке ML модели из {Path}", _modelPath);
        }
    }

    private async Task<List<FlashcardReviewData>> PrepareTrainingData()
    {
        // Берем историю за последний год для достаточного количества данных
        var cutoffDate = DateTime.UtcNow.AddDays(-365);

        var progresses = await _unitOfWork.FlashcardProgress.Query()
            .Where(p => p.LastReviewedAt >= cutoffDate && p.Repetitions > 0)
            .Include(p => p.User)
            .ToListAsync();

        var userPatterns = await _unitOfWork.Repository<UserLearningPattern>()
            .Query()
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
        var progress = _unitOfWork.FlashcardProgress.Query()
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
        var progress = await _unitOfWork.FlashcardProgress.Query()
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

    /// <summary>
    /// Валидация ML входных данных на наличие NaN/Infinity/негативных значений
    /// </summary>
    private bool ValidateMLInput(FlashcardReviewData input)
    {
        // Проверка EaseFactor (1.3 - 5.0)
        if (float.IsNaN(input.EaseFactor) || float.IsInfinity(input.EaseFactor) || input.EaseFactor < 1.3f || input.EaseFactor > 5.0f)
        {
            _logger.LogWarning("Некорректное значение EaseFactor: {Value}", input.EaseFactor);
            return false;
        }

        // Проверка Interval (не может быть отрицательным)
        if (input.Interval < 0)
        {
            _logger.LogWarning("Негативный Interval: {Value}", input.Interval);
            return false;
        }

        // Проверка Repetitions (не может быть отрицательным)
        if (input.Repetitions < 0)
        {
            _logger.LogWarning("Негативный Repetitions: {Value}", input.Repetitions);
            return false;
        }

        // Проверка DaysSinceLastReview
        if (float.IsNaN(input.DaysSinceLastReview) || float.IsInfinity(input.DaysSinceLastReview) || input.DaysSinceLastReview < 0)
        {
            _logger.LogWarning("Некорректное значение DaysSinceLastReview: {Value}", input.DaysSinceLastReview);
            return false;
        }

        // Проверка UserRetentionRate (0.0 - 1.0)
        if (float.IsNaN(input.UserRetentionRate) || float.IsInfinity(input.UserRetentionRate) || input.UserRetentionRate < 0 || input.UserRetentionRate > 1.0f)
        {
            _logger.LogWarning("Некорректное значение UserRetentionRate: {Value}", input.UserRetentionRate);
            return false;
        }

        // Проверка UserForgettingSpeed (0.0 - 1.0)
        if (float.IsNaN(input.UserForgettingSpeed) || float.IsInfinity(input.UserForgettingSpeed) || input.UserForgettingSpeed < 0 || input.UserForgettingSpeed > 1.0f)
        {
            _logger.LogWarning("Некорректное значение UserForgettingSpeed: {Value}", input.UserForgettingSpeed);
            return false;
        }

        // Проверка CorrectAfterBreak (0.0 - 1.0)
        if (float.IsNaN(input.CorrectAfterBreak) || float.IsInfinity(input.CorrectAfterBreak) || input.CorrectAfterBreak < 0 || input.CorrectAfterBreak > 1.0f)
        {
            _logger.LogWarning("Некорректное значение CorrectAfterBreak: {Value}", input.CorrectAfterBreak);
            return false;
        }

        return true;
    }
}
