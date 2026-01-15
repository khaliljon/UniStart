using Microsoft.EntityFrameworkCore;
using System.Globalization;
using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Learning;
using UniStart.Repositories;

namespace UniStart.Services.AI;

public interface IMLTrainingDataService
{
    Task<TrainingDataImportResult> AddManualTrainingDataAsync(List<ManualTrainingDataDto> data);
    Task<TrainingDataImportResult> ImportFromCsvAsync(IFormFile file);
    Task<TrainingDataImportResult> GenerateSyntheticDataAsync(int count);
    Task<TrainingStatsDto> GetTrainingStatsAsync();
    Task<int> DeleteSyntheticDataAsync(); // Метод для удаления всех синтетических данных
}

public class MLTrainingDataService : IMLTrainingDataService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MLTrainingDataService> _logger;

    public MLTrainingDataService(
        IUnitOfWork unitOfWork,
        ILogger<MLTrainingDataService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TrainingDataImportResult> AddManualTrainingDataAsync(List<ManualTrainingDataDto> data)
    {
        var result = new TrainingDataImportResult();

        try
        {
            foreach (var item in data)
            {
                // Проверяем существование пользователя и флешкарты
                var userExists = await _unitOfWork.Repository<ApplicationUser>().AnyAsync(u => u.Id == item.UserId);
                var flashcardExists = await _unitOfWork.Repository<Flashcard>().AnyAsync(f => f.Id == item.FlashcardId);

                if (!userExists || !flashcardExists)
                {
                    result.Errors.Add($"Пользователь {item.UserId} или карточка {item.FlashcardId} не найдены");
                    continue;
                }

                // Создаем или обновляем паттерн обучения пользователя
                var pattern = await _unitOfWork.Repository<UserLearningPattern>()
                    .FirstOrDefaultAsync(p => p.UserId == item.UserId);

                if (pattern == null)
                {
                    pattern = new UserLearningPattern
                    {
                        UserId = item.UserId,
                        AverageRetentionRate = item.UserRetentionRate,
                        ForgettingSpeed = item.UserForgettingSpeed,
                        SessionsProcessed = 1,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<UserLearningPattern>().AddAsync(pattern);
                }
                else
                {
                    // Обновляем средние значения
                    pattern.AverageRetentionRate = (pattern.AverageRetentionRate + item.UserRetentionRate) / 2;
                    pattern.ForgettingSpeed = (pattern.ForgettingSpeed + item.UserForgettingSpeed) / 2;
                    pattern.SessionsProcessed++;
                    pattern.UpdatedAt = DateTime.UtcNow;
                }

                // Создаем или обновляем прогресс флешкарты
                var progress = await _unitOfWork.FlashcardProgress.Query()
                    .FirstOrDefaultAsync(p => p.UserId == item.UserId && p.FlashcardId == item.FlashcardId);

                var isNewRecord = progress == null;
                
                if (progress == null)
                {
                    progress = new UserFlashcardProgress
                    {
                        UserId = item.UserId,
                        FlashcardId = item.FlashcardId,
                        EaseFactor = item.EaseFactor,
                        Interval = item.Interval,
                        Repetitions = item.Repetitions,
                        IsMastered = item.IsMastered,
                        LastReviewedAt = DateTime.UtcNow.AddDays(-item.DaysSinceLastReview),
                        IsSyntheticData = true // Помечаем синтетические данные
                    };
                    await _unitOfWork.FlashcardProgress.AddAsync(progress);
                    result.RecordsAdded++; // Увеличиваем только для новых записей
                }
                else
                {
                    progress.EaseFactor = item.EaseFactor;
                    progress.Interval = item.Interval;
                    progress.Repetitions = item.Repetitions;
                    progress.IsMastered = item.IsMastered;
                    progress.LastReviewedAt = DateTime.UtcNow.AddDays(-item.DaysSinceLastReview);
                    // Не увеличиваем RecordsAdded для обновленных записей
                }
            }

            await _unitOfWork.SaveChangesAsync();
            
            // Считаем ПОСЛЕ сохранения, чтобы получить актуальное количество
            result.TotalRecords = await _unitOfWork.FlashcardProgress.CountAsync(p => p.Repetitions > 0);
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении тренировочных данных");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<TrainingDataImportResult> ImportFromCsvAsync(IFormFile file)
    {
        var result = new TrainingDataImportResult();
        var data = new List<ManualTrainingDataDto>();

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            
            // Пропускаем заголовок
            var header = await reader.ReadLineAsync();
            if (header == null)
            {
                result.ErrorMessage = "Файл пуст";
                return result;
            }

            int lineNumber = 1;
            while (!reader.EndOfStream)
            {
                lineNumber++;
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var values = line.Split(',');
                    if (values.Length < 10)
                    {
                        result.Errors.Add($"Строка {lineNumber}: недостаточно столбцов");
                        continue;
                    }

                    var dto = new ManualTrainingDataDto
                    {
                        UserId = values[0].Trim(),
                        FlashcardId = int.Parse(values[1]),
                        EaseFactor = double.Parse(values[2], CultureInfo.InvariantCulture),
                        Interval = int.Parse(values[3]),
                        Repetitions = int.Parse(values[4]),
                        DaysSinceLastReview = double.Parse(values[5], CultureInfo.InvariantCulture),
                        UserRetentionRate = double.Parse(values[6], CultureInfo.InvariantCulture),
                        UserForgettingSpeed = double.Parse(values[7], CultureInfo.InvariantCulture),
                        CorrectAfterBreak = double.Parse(values[8], CultureInfo.InvariantCulture),
                        IsMastered = bool.Parse(values[9]),
                        OptimalReviewHours = double.Parse(values[10], CultureInfo.InvariantCulture)
                    };

                    data.Add(dto);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Строка {lineNumber}: {ex.Message}");
                }
            }

            // Импортируем собранные данные
            var importResult = await AddManualTrainingDataAsync(data);
            result.RecordsAdded = importResult.RecordsAdded;
            result.TotalRecords = importResult.TotalRecords;
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при импорте CSV");
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<TrainingDataImportResult> GenerateSyntheticDataAsync(int count)
    {
        var random = new Random();
        var data = new List<ManualTrainingDataDto>();

        // Получаем ВСЕХ пользователей и ВСЕ флешкарты для максимальных комбинаций
        var users = await _unitOfWork.Repository<ApplicationUser>().Query().Select(u => u.Id).ToListAsync();
        var flashcards = await _unitOfWork.Repository<Flashcard>().Query().Select(f => f.Id).ToListAsync();

        if (!users.Any() || !flashcards.Any())
        {
            return new TrainingDataImportResult
            {
                Success = false,
                ErrorMessage = "Нет пользователей или флешкарт для генерации данных. " +
                              $"Пользователей: {users.Count}, Карточек: {flashcards.Count}"
            };
        }

        var maxPossibleCombinations = users.Count * flashcards.Count;
        
        // Получаем существующие комбинации
        var existingCombinationsList = await _unitOfWork.FlashcardProgress.Query()
            .Select(p => new { p.UserId, p.FlashcardId })
            .ToListAsync();
        
        var existingCombinations = existingCombinationsList
            .Select(x => (x.UserId, x.FlashcardId))
            .ToHashSet();

        var availableSlots = maxPossibleCombinations - existingCombinations.Count;
        
        if (availableSlots == 0)
        {
            return new TrainingDataImportResult
            {
                Success = false,
                ErrorMessage = $"Все возможные комбинации уже существуют! " +
                              $"Всего комбинаций: {maxPossibleCombinations} " +
                              $"({users.Count} пользователей × {flashcards.Count} карточек). " +
                              $"Добавьте больше пользователей или карточек."
            };
        }

        if (count > availableSlots)
        {
            _logger.LogWarning(
                "Запрошено {Requested} записей, но доступно только {Available} уникальных комбинаций. " +
                "Будет сгенерировано максимум {MaxGenerated} записей.",
                count, availableSlots, availableSlots);
            
            count = availableSlots; // Ограничиваем до доступного количества
        }

        var generatedCombinations = new HashSet<(string, int)>();
        var attempts = 0;
        var maxAttempts = count * 10; // Ограничиваем количество попыток

        while (data.Count < count && attempts < maxAttempts)
        {
            attempts++;
            
            var userId = users[random.Next(users.Count)];
            var flashcardId = flashcards[random.Next(flashcards.Count)];
            
            // Проверяем, что такая комбинация еще не существует и не была сгенерирована
            var combination = (userId, flashcardId);
            if (existingCombinations.Any(c => c.UserId == userId && c.FlashcardId == flashcardId) || 
                generatedCombinations.Contains(combination))
            {
                continue; // Пропускаем дубликаты
            }

            generatedCombinations.Add(combination);

            // Генерируем реалистичные параметры
            var repetitions = random.Next(1, 20);
            var easeFactor = 1.3 + random.NextDouble() * 1.2; // 1.3 - 2.5
            var interval = (int)Math.Pow(2, repetitions / 3.0); // Экспоненциальный рост
            var userRetention = 50 + random.NextDouble() * 40; // 50-90%
            var forgettingSpeed = 0.5 + random.NextDouble() * 2; // 0.5-2.5
            var correctAfterBreak = userRetention * (0.7 + random.NextDouble() * 0.3);
            
            // Оптимальное время зависит от интервала и retention rate
            var optimalHours = interval * 24 * (userRetention / 100.0);

            data.Add(new ManualTrainingDataDto
            {
                UserId = userId,
                FlashcardId = flashcardId,
                EaseFactor = easeFactor,
                Interval = Math.Min(interval, 365),
                Repetitions = repetitions,
                DaysSinceLastReview = random.Next(0, interval + 1),
                UserRetentionRate = userRetention,
                UserForgettingSpeed = forgettingSpeed,
                CorrectAfterBreak = correctAfterBreak,
                IsMastered = repetitions > 8 && easeFactor > 2.0,
                OptimalReviewHours = Math.Min(optimalHours, 8760)
            });
        }

        if (data.Count < count)
        {
            _logger.LogWarning(
                "Удалось сгенерировать только {Generated} из {Requested} записей после {Attempts} попыток. " +
                "Максимум уникальных комбинаций: {MaxCombinations} = {Users} пользователей × {Cards} карточек. " +
                "Уже существует: {Existing} записей. Доступно для генерации: {Available}.",
                data.Count, count, attempts, 
                users.Count * flashcards.Count, users.Count, flashcards.Count, 
                existingCombinations.Count, 
                users.Count * flashcards.Count - existingCombinations.Count);
        }
        else
        {
            _logger.LogInformation(
                "Успешно сгенерировано {Generated} новых записей из {Requested}. " +
                "Использовано попыток: {Attempts}/{MaxAttempts}",
                data.Count, count, attempts, maxAttempts);
        }

        return await AddManualTrainingDataAsync(data);
    }

    public async Task<TrainingStatsDto> GetTrainingStatsAsync()
    {
        var now = DateTime.UtcNow;
        var allProgresses = await _unitOfWork.FlashcardProgress.Query()
            .Where(p => p.Repetitions > 0)
            .ToListAsync();

        var patterns = await _unitOfWork.Repository<UserLearningPattern>()
            .Query()
            .ToListAsync();

        return new TrainingStatsDto
        {
            TotalRecords = allProgresses.Count,
            RecordsLast24Hours = allProgresses.Count(p => p.LastReviewedAt >= now.AddDays(-1)),
            RecordsLast7Days = allProgresses.Count(p => p.LastReviewedAt >= now.AddDays(-7)),
            RecordsLast30Days = allProgresses.Count(p => p.LastReviewedAt >= now.AddDays(-30)),
            IsModelTrained = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Models", "flashcard_review_model.zip")),
            LastTrainingDate = patterns.Any() ? patterns.Max(p => p.UpdatedAt) : null,
            UniqueUsers = allProgresses.Select(p => p.UserId).Distinct().Count(),
            UniqueFlashcards = allProgresses.Select(p => p.FlashcardId).Distinct().Count(),
            AverageEaseFactor = allProgresses.Any() ? allProgresses.Average(p => p.EaseFactor) : 0,
            AverageInterval = allProgresses.Any() ? allProgresses.Average(p => p.Interval) : 0,
            AverageRetentionRate = patterns.Any() ? patterns.Average(p => p.AverageRetentionRate) : 0
        };
    }

    public async Task<int> DeleteSyntheticDataAsync()
    {
        try
        {
            var syntheticData = await _unitOfWork.FlashcardProgress.Query()
                .Where(p => p.IsSyntheticData)
                .ToListAsync();

            var count = syntheticData.Count;

            if (count > 0)
            {
                _unitOfWork.FlashcardProgress.RemoveRange(syntheticData);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Удалено {Count} синтетических записей UserFlashcardProgress", count);
            }
            else
            {
                _logger.LogInformation("Синтетические данные не найдены");
            }

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении синтетических данных");
            throw;
        }
    }
}
