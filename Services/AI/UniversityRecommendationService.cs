using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UniStart.Repositories;
using UniStart.Models.Core;
using UniStart.Models.Learning;
using UniStart.Models.Reference;

namespace UniStart.Services.AI;

/// <summary>
/// Сервис рекомендаций университетов на основе content-based filtering
/// Анализирует успеваемость пользователя по предметам и сопоставляет с требованиями университетов
/// </summary>
public class UniversityRecommendationService : IUniversityRecommendationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UniversityRecommendationService> _logger;

    public UniversityRecommendationService(
        IUnitOfWork unitOfWork,
        ILogger<UniversityRecommendationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<UniversityRecommendation>> GetRecommendations(
        string userId, 
        int topN = 10, 
        bool forceRefresh = false)
    {
        try
        {
            // Проверяем, есть ли сохраненные рекомендации (не старше 7 дней)
            if (!forceRefresh)
            {
                var cachedRecommendations = await _unitOfWork.Repository<UniversityRecommendation>()
                    .Query()
                    .Where(r => r.UserId == userId && r.CreatedAt > DateTime.UtcNow.AddDays(-7))
                    .Include(r => r.University)
                        .ThenInclude(u => u.Country)
                    .OrderBy(r => r.Rank)
                    .Take(topN)
                    .ToListAsync();

                if (cachedRecommendations.Any())
                {
                    _logger.LogInformation("Возвращены кэшированные рекомендации для User={UserId}", userId);
                    return cachedRecommendations;
                }
            }

            // Строим профиль пользователя
            var userProfile = await BuildUserProfile(userId);
            if (userProfile == null)
            {
                _logger.LogWarning("Невозможно построить профиль для User={UserId}", userId);
                return new List<UniversityRecommendation>();
            }

            // Получаем все активные университеты
            var universities = await _unitOfWork.Repository<University>()
                .Query()
                .Where(u => u.IsActive)
                .Include(u => u.Country)
                .ToListAsync();

            // Рассчитываем оценки для каждого университета
            var scores = new List<(University University, double MatchScore, double AdmissionProb, List<string> Reasons)>();

            foreach (var university in universities)
            {
                var (matchScore, admissionProb, reasons) = CalculateUniversityScore(userProfile, university);
                scores.Add((university, matchScore, admissionProb, reasons));
            }

            // Сортируем по оценке и берем топ-N
            var topUniversities = scores
                .OrderByDescending(s => s.MatchScore)
                .Take(topN)
                .ToList();

            // Удаляем старые рекомендации
            var oldRecommendations = await _unitOfWork.Repository<UniversityRecommendation>()
                .Query()
                .Where(r => r.UserId == userId)
                .ToListAsync();
            _unitOfWork.Repository<UniversityRecommendation>().RemoveRange(oldRecommendations);

            // Создаем новые рекомендации
            var recommendations = new List<UniversityRecommendation>();
            int rank = 1;

            foreach (var (university, matchScore, admissionProb, reasons) in topUniversities)
            {
                var recommendation = new UniversityRecommendation
                {
                    UserId = userId,
                    UniversityId = university.Id,
                    University = university,
                    MatchScore = Math.Round(matchScore, 2),
                    AdmissionProbability = Math.Round(admissionProb, 2),
                    ReasonsJson = JsonSerializer.Serialize(reasons),
                    Rank = rank++,
                    CreatedAt = DateTime.UtcNow,
                    IsViewed = false
                };

                recommendations.Add(recommendation);
                await _unitOfWork.Repository<UniversityRecommendation>().AddAsync(recommendation);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Создано {Count} рекомендаций для User={UserId}", recommendations.Count, userId);
            return recommendations;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Некорректный userId для создания рекомендаций");
            return new List<UniversityRecommendation>();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка БД при сохранении рекомендаций для User={UserId}", userId);
            return new List<UniversityRecommendation>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при создании рекомендаций для User={UserId}", userId);
            return new List<UniversityRecommendation>();
        }
    }

    public async Task<UserProfile?> BuildUserProfile(string userId)
    {
        try
        {
            var user = await _unitOfWork.Repository<ApplicationUser>()
                .Query()
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var profile = new UserProfile
            {
                UserId = userId,
                PreferredCity = user.PreferredCity,
                MaxBudget = user.MaxBudget,
                CareerGoal = user.CareerGoal
            };

            // Собираем баллы из экзаменов  
            var examScores = await _unitOfWork.ExamAttempts.Query()
                .Where(a => a.UserId == userId)
                .Include(a => a.Exam)
                    .ThenInclude(e => e.Subjects)
                .SelectMany(a => a.Exam.Subjects.Select(s => new
                {
                    SubjectName = s.Name,
                    Score = a.Score
                }))
                .GroupBy(x => x.SubjectName)
                .Select(g => new
                {
                    Subject = g.Key,
                    AverageScore = g.Average(x => x.Score)
                })
                .ToListAsync();

            // Объединяем оценки по предметам
            var allScores = examScores
                .GroupBy(s => s.Subject)
                .Select(g => new
                {
                    Subject = g.Key,
                    AverageScore = g.Average(s => s.AverageScore)
                })
                .ToList();

            foreach (var score in allScores)
            {
                profile.SubjectScores[score.Subject] = Math.Round(score.AverageScore, 2);
            }

            // Общая статистика
            profile.TotalQuizzesTaken = await _unitOfWork.QuizAttempts.Query().CountAsync(a => a.UserId == userId);
            profile.TotalExamsTaken = await _unitOfWork.ExamAttempts.Query().CountAsync(a => a.UserId == userId);
            profile.TotalExamsTaken = await _unitOfWork.ExamAttempts.Query().CountAsync(a => a.UserId == userId);
            profile.AverageExamScore = allScores.Any() ? Math.Round(allScores.Average(s => s.AverageScore), 2) : 0;

            // Сильные и слабые стороны
            var orderedScores = allScores.OrderByDescending(s => s.AverageScore).ToList();
            profile.Strengths = orderedScores.Take(3).Select(s => s.Subject).ToList();
            profile.Weaknesses = orderedScores.TakeLast(3).Select(s => s.Subject).ToList();

            // Прогресс обучения
            var totalCards = await _unitOfWork.FlashcardProgress.Query().CountAsync(p => p.UserId == userId);
            var masteredCards = await _unitOfWork.FlashcardProgress.Query().CountAsync(p => p.UserId == userId && p.IsMastered);
            profile.LearningProgress = totalCards > 0 ? Math.Round((double)masteredCards / totalCards * 100, 2) : 0;

            return profile;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Некорректный userId для построения профиля");
            return null;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Ошибка выполнения запроса при построении профиля User={UserId}", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при построении профиля User={UserId}", userId);
            return null;
        }
    }

    public async Task<bool> UpdateUserPreferences(string userId, string? preferredCity, decimal? maxBudget, string? careerGoal)
    {
        try
        {
            var user = await _unitOfWork.Repository<ApplicationUser>()
                .Query()
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            user.PreferredCity = preferredCity;
            user.MaxBudget = maxBudget;
            user.CareerGoal = careerGoal;

            _unitOfWork.Repository<ApplicationUser>().Update(user);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Обновлены предпочтения User={UserId}", userId);

            return true;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Некорректный userId для обновления предпочтений");
            return false;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка БД при обновлении предпочтений User={UserId}", userId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при обновлении предпочтений User={UserId}", userId);
            return false;
        }
    }

    public async Task MarkAsViewed(int recommendationId)
    {
        try
        {
            var recommendation = await _unitOfWork.Repository<UniversityRecommendation>()
                .Query()
                .FirstOrDefaultAsync(r => r.Id == recommendationId);
            if (recommendation != null && !recommendation.IsViewed)
            {
                recommendation.IsViewed = true;
                recommendation.ViewedAt = DateTime.UtcNow;
                _unitOfWork.Repository<UniversityRecommendation>().Update(recommendation);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка БД при отметке рекомендации {Id} как просмотренной", recommendationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при отметке рекомендации {Id} как просмотренной", recommendationId);
        }
    }

    public async Task RateRecommendation(int recommendationId, int rating)
    {
        try
        {
            if (rating < 1 || rating > 5)
            {
                _logger.LogWarning("Некорректная оценка {Rating} для рекомендации {Id}", rating, recommendationId);
                return;
            }

            var recommendation = await _unitOfWork.Repository<UniversityRecommendation>()
                .Query()
                .FirstOrDefaultAsync(r => r.Id == recommendationId);
            if (recommendation != null)
            {
                recommendation.UserRating = rating;
                _unitOfWork.Repository<UniversityRecommendation>().Update(recommendation);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Рекомендация {Id} оценена на {Rating} звезд", recommendationId, rating);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка БД при оценке рекомендации {Id}", recommendationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при оценке рекомендации {Id}", recommendationId);
        }
    }

    /// <summary>
    /// Рассчитывает оценку соответствия университета профилю пользователя
    /// </summary>
    private (double MatchScore, double AdmissionProb, List<string> Reasons) CalculateUniversityScore(
        UserProfile profile, 
        University university)
    {
        double matchScore = 0;
        double admissionProb = 50; // Базовая вероятность
        var reasons = new List<string>();

        // 1. Совпадение по предметам (40% веса)
        var universitySubjects = ParseJsonArray(university.StrongSubjectsJson);
        if (universitySubjects.Any() && profile.SubjectScores.Any())
        {
            var matchedSubjects = universitySubjects
                .Where(s => profile.SubjectScores.ContainsKey(s))
                .ToList();

            if (matchedSubjects.Any())
            {
                var avgSubjectScore = matchedSubjects.Average(s => profile.SubjectScores[s]);
                matchScore += avgSubjectScore * 0.4;
                
                if (avgSubjectScore > 80)
                {
                    reasons.Add($"Отличные результаты по профильным предметам ({string.Join(", ", matchedSubjects)})");
                    admissionProb += 20;
                }
                else if (avgSubjectScore > 60)
                {
                    reasons.Add($"Хорошие результаты по профильным предметам ({string.Join(", ", matchedSubjects)})");
                    admissionProb += 10;
                }
            }
        }

        // 2. Совпадение по карьерной цели (20% веса)
        if (!string.IsNullOrEmpty(profile.CareerGoal) && !string.IsNullOrEmpty(university.ProgramsJson))
        {
            var programs = ParseJsonArray(university.ProgramsJson);
            if (programs.Any(p => p.Contains(profile.CareerGoal, StringComparison.OrdinalIgnoreCase)))
            {
                matchScore += 20;
                reasons.Add($"Университет предлагает программы по направлению '{profile.CareerGoal}'");
                admissionProb += 15;
            }
        }

        // 3. Бюджет (15% веса)
        if (university.TuitionFee.HasValue && profile.MaxBudget.HasValue)
        {
            if (university.TuitionFee <= profile.MaxBudget)
            {
                matchScore += 15;
                reasons.Add($"Стоимость обучения ({university.TuitionFee:N0}) соответствует бюджету");
            }
            else
            {
                // Штраф за превышение бюджета
                var overage = (double)(university.TuitionFee.Value - profile.MaxBudget.Value) / (double)profile.MaxBudget.Value;
                matchScore -= Math.Min(15, overage * 10);
            }
        }

        // 4. Локация (10% веса)
        if (!string.IsNullOrEmpty(profile.PreferredCity))
        {
            if (university.City?.Contains(profile.PreferredCity, StringComparison.OrdinalIgnoreCase) == true)
            {
                matchScore += 10;
                reasons.Add($"Университет находится в желаемом городе ({university.City})");
            }
        }

        // 5. Минимальный проходной балл (15% веса)
        if (university.MinScore.HasValue)
        {
            var scoreDiff = profile.AverageExamScore - university.MinScore.Value;
            if (scoreDiff >= 10)
            {
                matchScore += 15;
                reasons.Add("Ваши результаты значительно выше минимального порога");
                admissionProb += 25;
            }
            else if (scoreDiff >= 0)
            {
                matchScore += 10;
                reasons.Add("Ваши результаты соответствуют требованиям");
                admissionProb += 10;
            }
            else
            {
                matchScore -= Math.Abs(scoreDiff) * 0.5;
                admissionProb -= Math.Min(30, Math.Abs(scoreDiff));
                if (scoreDiff > -10)
                    reasons.Add("Требуется улучшить результаты для гарантированного поступления");
            }
        }

        // Нормализация
        matchScore = Math.Max(0, Math.Min(100, matchScore));
        admissionProb = Math.Max(0, Math.Min(100, admissionProb));

        if (!reasons.Any())
            reasons.Add("Общее соответствие вашему профилю");

        return (matchScore, admissionProb, reasons);
    }

    private List<string> ParseJsonArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
