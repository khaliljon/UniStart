using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models.Learning;
using UniStart.Repositories;

namespace UniStart.Services.AI;

/// <summary>
/// AI сервис для интеллектуальных рекомендаций контента
/// Анализирует паттерны обучения и предлагает персонализированный контент
/// </summary>
public class ContentRecommendationService : IContentRecommendationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUniversityRecommendationService _universityService;
    private readonly ILogger<ContentRecommendationService> _logger;

    public ContentRecommendationService(
        IUnitOfWork unitOfWork,
        IUniversityRecommendationService universityService,
        ILogger<ContentRecommendationService> logger)
    {
        _unitOfWork = unitOfWork;
        _universityService = universityService;
        _logger = logger;
    }

    public async Task<List<int>> RecommendQuizzesForWeaknesses(string userId, int count = 5)
    {
        try
        {
            // Проверяем, что у пользователя достаточно данных для анализа
            var totalAttempts = await _unitOfWork.QuizAttempts.Query()
                .CountAsync(a => a.UserId == userId) +
                await _unitOfWork.ExamAttempts.Query()
                .CountAsync(a => a.UserId == userId);
            
            // Минимум 5 попыток для рекомендаций
            if (totalAttempts < 5)
            {
                _logger.LogInformation("Недостаточно данных для AI рекомендаций User={UserId}. Попыток: {Attempts}, требуется: 5", 
                    userId, totalAttempts);
                return new List<int>(); // Пустой список - рекомендации не показываем
            }

            // Получаем профиль пользователя для анализа слабостей
            var profile = await _universityService.BuildUserProfile(userId);
            if (profile == null || !profile.Weaknesses.Any() || !profile.SubjectScores.Any())
            {
                _logger.LogWarning("Не удалось определить слабости для User={UserId}", userId);
                return new List<int>(); // Нет данных - не показываем рекомендации
            }

            // Ищем квизы - Subject field removed, используем популярные
            var recommendedQuizzes = await _unitOfWork.Repository<Quiz>()
                .Query()
                .Where(q => q.IsPublished)
                .OrderByDescending(q => q.Attempts.Count) // Популярные квизы
                .Select(q => q.Id)
                .Take(count)
                .ToListAsync();

            if (!recommendedQuizzes.Any())
            {
                _logger.LogInformation("Не найдено квизов по слабым предметам User={UserId}", userId);
                return new List<int>(); // Пустой список - не показываем рекомендации
            }

            _logger.LogInformation("Рекомендовано {Count} квизов для улучшения слабых сторон User={UserId}", 
                recommendedQuizzes.Count, userId);

            return recommendedQuizzes;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Некорректный userId для рекомендации квизов");
            return new List<int>();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения запроса при рекомендации квизов для User={UserId}", userId);
            return new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при рекомендации квизов для User={UserId}", userId);
            return new List<int>();
        }
    }

    public async Task<List<int>> RecommendExamsForGoals(string userId, int count = 3)
    {
        try
        {
            // Проверяем, что у пользователя достаточно данных
            var totalAttempts = await _unitOfWork.QuizAttempts.Query()
                .CountAsync(a => a.UserId == userId) +
                await _unitOfWork.ExamAttempts.Query()
                .CountAsync(a => a.UserId == userId);
            
            if (totalAttempts < 5)
            {
                _logger.LogInformation("Недостаточно данных для AI рекомендаций экзаменов User={UserId}", userId);
                return new List<int>();
            }

            var profile = await _universityService.BuildUserProfile(userId);
            if (profile == null || !profile.SubjectScores.Any())
            {
                return new List<int>();
            }

            // Если есть карьерная цель, ищем релевантные экзамены
            var recommendedExams = await _unitOfWork.Repository<Exam>()
                .Query()
                .Where(e => e.IsPublic && 
                           (!string.IsNullOrEmpty(profile.CareerGoal) && 
                            !string.IsNullOrEmpty(e.Description) &&
                            e.Description.Contains(profile.CareerGoal)))
                .OrderByDescending(e => e.Attempts.Count)
                .Select(e => e.Id)
                .Take(count)
                .ToListAsync();

            if (!recommendedExams.Any())
            {
                _logger.LogInformation("Не найдено экзаменов по целям User={UserId}", userId);
                return new List<int>();
            }

            _logger.LogInformation("Рекомендовано {Count} экзаменов для User={UserId}", 
                recommendedExams.Count, userId);

            return recommendedExams;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Некорректный userId для рекомендации экзаменов");
            return new List<int>();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения запроса при рекомендации экзаменов для User={UserId}", userId);
            return new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при рекомендации экзаменов для User={UserId}", userId);
            return new List<int>();
        }
    }

    public async Task<List<int>> RecommendFlashcardSets(string userId, int count = 5)
    {
        try
        {
            // Проверяем, что у пользователя достаточно данных
            var totalAttempts = await _unitOfWork.QuizAttempts.Query()
                .CountAsync(a => a.UserId == userId) +
                await _unitOfWork.ExamAttempts.Query()
                .CountAsync(a => a.UserId == userId);
            
            if (totalAttempts < 5)
            {
                _logger.LogInformation("Недостаточно данных для AI рекомендаций карточек User={UserId}", userId);
                return new List<int>();
            }

            var profile = await _universityService.BuildUserProfile(userId);
            if (profile == null || !profile.Weaknesses.Any() || !profile.SubjectScores.Any())
            {
                return new List<int>();
            }

            // Ищем наборы карточек по слабым темам
            var weakSubjects = profile.Weaknesses;
            var recommendedSets = await _unitOfWork.Repository<FlashcardSet>()
                .Query()
                .Where(fs => fs.IsPublished && 
                            weakSubjects.Any(ws => fs.Title.Contains(ws) || 
                                                  fs.Description.Contains(ws)))
                .OrderByDescending(fs => fs.Flashcards.Count)
                .Select(fs => fs.Id)
                .Take(count)
                .ToListAsync();

            if (!recommendedSets.Any())
            {
                _logger.LogInformation("Не найдено наборов карточек по слабым темам User={UserId}", userId);
                return new List<int>();
            }

            _logger.LogInformation("Рекомендовано {Count} наборов карточек для User={UserId}", 
                recommendedSets.Count, userId);

            return recommendedSets;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Некорректный userId для рекомендации наборов карточек");
            return new List<int>();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения запроса при рекомендации карточек для User={UserId}", userId);
            return new List<int>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при рекомендации карточек для User={UserId}", userId);
            return new List<int>();
        }
    }

    public async Task<string?> GetNextTopicToStudy(string userId)
    {
        try
        {
            var profile = await _universityService.BuildUserProfile(userId);
            if (profile == null)
                return null;

            // Анализируем паттерны обучения
            var learningPattern = await _unitOfWork.Repository<UserLearningPattern>()
                .Query()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // Subject field removed - нельзя рекомендовать темы
            _logger.LogInformation("Рекомендации тем недоступны - Subject field removed");
            return null;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Некорректный userId для определения следующей темы");
            return null;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения запроса при определении следующей темы для User={UserId}", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при определении следующей темы для User={UserId}", userId);
            return null;
        }
    }

    public async Task<List<string>> GetPersonalizedTips(string userId)
    {
        try
        {
            var tips = new List<string>();
            var profile = await _universityService.BuildUserProfile(userId);
            
            if (profile == null)
            {
                return new List<string> { "Начните с прохождения нескольких квизов для анализа вашего уровня" };
            }

            var learningPattern = await _unitOfWork.Repository<UserLearningPattern>()
                .Query()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // Анализ успеваемости
            if (profile.AverageExamScore < 60)
            {
                tips.Add("💡 Ваши результаты ниже среднего. Рекомендуем уделить больше времени базовым темам");
                tips.Add("📚 Используйте flashcards для закрепления основных концепций");
            }
            else if (profile.AverageExamScore >= 80)
            {
                tips.Add("🌟 Отличные результаты! Продолжайте практиковаться и изучайте более сложные темы");
            }

            // Анализ слабостей
            if (profile.Weaknesses.Any())
            {
                tips.Add($"⚠️ Требуется улучшение по предметам: {string.Join(", ", profile.Weaknesses)}");
            }

            // Анализ прогресса
            if (profile.LearningProgress < 30)
            {
                tips.Add("🎯 Вы только начали обучение. Установите ежедневную цель - 10-15 карточек");
            }
            else if (profile.LearningProgress >= 70)
            {
                tips.Add("🏆 Отличный прогресс в изучении материала! Вы освоили большую часть контента");
            }

            // Анализ паттернов запоминания
            if (learningPattern != null)
            {
                if (learningPattern.ForgettingSpeed > 1.5)
                {
                    tips.Add("🔄 Вы быстро забываете материал. Попробуйте увеличить частоту повторений");
                }
                
                if (learningPattern.AverageRetentionRate > 85)
                {
                    tips.Add("🧠 Высокая скорость запоминания! Вы можете увеличить интервалы между повторениями");
                }
            }

            // Рекомендации по учебному плану
            if (profile.TotalQuizzesTaken + profile.TotalExamsTaken < 5)
            {
                tips.Add("📝 Пройдите больше квизов и экзаменов для более точной оценки ваших знаний");
            }

            // Мотивационные советы
            var user = await _unitOfWork.Repository<ApplicationUser>()
                .Query()
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                var daysSinceStart = (DateTime.UtcNow - user.CreatedAt).Days;
                if (daysSinceStart > 7 && profile.TotalQuizzesTaken == 0 && profile.TotalExamsTaken == 0)
                {
                    tips.Add("⏰ Вы давно не проходили квизы и экзамены. Регулярная практика - ключ к успеху!");
                }
            }

            if (!tips.Any())
            {
                tips.Add("✅ Продолжайте в том же духе! Ваш прогресс стабилен");
            }

            _logger.LogInformation("Сгенерировано {Count} персонализированных советов для User={UserId}", 
                tips.Count, userId);

            return tips;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Некорректный userId для генерации советов");
            return new List<string> { "Продолжайте учиться и практиковаться регулярно" };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения запроса при генерации советов для User={UserId}", userId);
            return new List<string> { "Продолжайте учиться и практиковаться регулярно" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при генерации советов для User={UserId}", userId);
            return new List<string> { "Продолжайте учиться и практиковаться регулярно" };
        }
    }

    // Helper методы
    private async Task<List<int>> GetPopularQuizzes(int count)
    {
        return await _unitOfWork.Repository<Quiz>()
            .Query()
            .Where(q => q.IsPublished)
            .OrderByDescending(q => q.Attempts.Count)
            .Select(q => q.Id)
            .Take(count)
            .ToListAsync();
    }

    private async Task<List<int>> GetPopularExams(int count)
    {
        return await _unitOfWork.Repository<Exam>()
            .Query()
            .Where(e => e.IsPublic)
            .OrderByDescending(e => e.Attempts.Count)
            .Select(e => e.Id)
            .Take(count)
            .ToListAsync();
    }

    private async Task<List<int>> GetPopularFlashcardSets(int count)
    {
        return await _unitOfWork.Repository<FlashcardSet>()
            .Query()
            .Where(fs => fs.IsPublished)
            .OrderByDescending(fs => fs.Flashcards.Count)
            .Select(fs => fs.Id)
            .Take(count)
            .ToListAsync();
    }
}
