using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models.Flashcards;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Learning;

namespace UniStart.Services;

/// <summary>
/// Intelligent recommendation service based on UserPreferences and activity
/// </summary>
public class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(
        ApplicationDbContext context,
        ILogger<RecommendationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<FlashcardSet>> GetRecommendedFlashcardSetsAsync(string userId, int count = 10)
    {
        try
        {
            // 1. Get user preferences
            var preferences = await _context.UserPreferences
                .Include(p => p.InterestedSubjects)
                .Include(p => p.WeakSubjects)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // 2. Get user's completed flashcard sets
            var completedSetIds = await _context.UserFlashcardProgresses
                .Where(p => p.UserId == userId && p.IsMastered)
                .Select(p => p.Flashcard.FlashcardSetId)
                .Distinct()
                .ToListAsync();

            // 3. Build recommendation query
            var query = _context.FlashcardSets
                .Where(fs => fs.IsPublished && fs.IsPublic)
                .Include(fs => fs.Subjects)
                .Include(fs => fs.Flashcards)
                .AsQueryable();

            // 4. Apply preference-based filtering
            if (preferences != null)
            {
                // Prioritize user's preferred difficulty
                var targetDifficulty = GetFlashcardDifficulty(preferences.PreferredDifficulty);
                
                // Get subject IDs from interested and weak subjects
                var interestedSubjectIds = preferences.InterestedSubjects.Select(s => s.Id).ToList();
                var weakSubjectIds = preferences.WeakSubjects.Select(s => s.Id).ToList();
                var allRelevantSubjectIds = interestedSubjectIds.Concat(weakSubjectIds).Distinct().ToList();

                if (allRelevantSubjectIds.Any())
                {
                    query = query.Where(fs => fs.Subjects.Any(s => allRelevantSubjectIds.Contains(s.Id)));
                }
            }

            // 5. Exclude already completed sets
            if (completedSetIds.Any())
            {
                query = query.Where(fs => !completedSetIds.Contains(fs.Id));
            }

            // 6. Get recommendations with scoring
            var sets = await query.ToListAsync();

            var scoredSets = sets.Select(fs => new
            {
                Set = fs,
                Score = CalculateFlashcardSetScore(fs, preferences, userId)
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .Select(x => x.Set)
            .ToList();

            _logger.LogInformation("Generated {Count} flashcard set recommendations for user {UserId}", 
                scoredSets.Count, userId);

            return scoredSets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating flashcard set recommendations for user {UserId}", userId);
            
            // Fallback: return most popular sets
            return await _context.FlashcardSets
                .Where(fs => fs.IsPublished && fs.IsPublic)
                .Include(fs => fs.Flashcards)
                .OrderByDescending(fs => fs.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }

    public async Task<IEnumerable<Quiz>> GetRecommendedQuizzesAsync(string userId, int count = 10)
    {
        try
        {
            // 1. Get user preferences
            var preferences = await _context.UserPreferences
                .Include(p => p.InterestedSubjects)
                .Include(p => p.WeakSubjects)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // 2. Get user's quiz attempts
            var attemptedQuizIds = await _context.UserQuizAttempts
                .Where(qa => qa.UserId == userId && qa.Percentage >= 80) // Passed quizzes
                .Select(qa => qa.QuizId)
                .Distinct()
                .ToListAsync();

            // 3. Build recommendation query
            var query = _context.Quizzes
                .Where(q => q.IsPublished && q.IsPublic)
                .Include(q => q.Subjects)
                .Include(q => q.Questions)
                .AsQueryable();

            // 4. Apply preference-based filtering
            if (preferences != null)
            {
                // Filter by preferred difficulty
                var targetDifficulty = GetDifficulty(preferences.PreferredDifficulty);
                query = query.Where(q => q.Difficulty == targetDifficulty);

                // Filter by subjects
                var interestedSubjectIds = preferences.InterestedSubjects.Select(s => s.Id).ToList();
                var weakSubjectIds = preferences.WeakSubjects.Select(s => s.Id).ToList();
                var allRelevantSubjectIds = interestedSubjectIds.Concat(weakSubjectIds).Distinct().ToList();

                if (allRelevantSubjectIds.Any())
                {
                    query = query.Where(q => q.Subjects.Any(s => allRelevantSubjectIds.Contains(s.Id)));
                }

                // Filter by preferred quiz format
                if (!preferences.PrefersQuizzes)
                {
                    return new List<Quiz>(); // User doesn't prefer quizzes
                }
            }

            // 5. Exclude already passed quizzes
            if (attemptedQuizIds.Any())
            {
                query = query.Where(q => !attemptedQuizIds.Contains(q.Id));
            }

            // 6. Get recommendations with scoring
            var quizzes = await query.ToListAsync();

            var scoredQuizzes = quizzes.Select(q => new
            {
                Quiz = q,
                Score = CalculateQuizScore(q, preferences, userId)
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .Select(x => x.Quiz)
            .ToList();

            _logger.LogInformation("Generated {Count} quiz recommendations for user {UserId}", 
                scoredQuizzes.Count, userId);

            return scoredQuizzes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz recommendations for user {UserId}", userId);
            
            // Fallback: return most popular quizzes
            return await _context.Quizzes
                .Where(q => q.IsPublished && q.IsPublic)
                .Include(q => q.Questions)
                .OrderByDescending(q => q.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }

    public async Task<IEnumerable<Exam>> GetRecommendedExamsAsync(string userId, int count = 10)
    {
        try
        {
            // 1. Get user preferences
            var preferences = await _context.UserPreferences
                .Include(p => p.InterestedSubjects)
                .Include(p => p.TargetUniversity)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // 2. Build recommendation query
            var query = _context.Exams
                .Where(e => e.IsPublic)
                .Include(e => e.Subjects)
                .Include(e => e.Questions)
                .AsQueryable();

            // 3. Apply preference-based filtering
            if (preferences != null)
            {
                // Filter by target university if set
                if (preferences.TargetUniversityId.HasValue)
                {
                    query = query.Where(e => e.UniversityId == preferences.TargetUniversityId);
                }

                // Filter by subjects
                var interestedSubjectIds = preferences.InterestedSubjects.Select(s => s.Id).ToList();
                if (interestedSubjectIds.Any())
                {
                    query = query.Where(e => e.Subjects.Any(s => interestedSubjectIds.Contains(s.Id)));
                }

                // Filter by preferred exam format
                if (!preferences.PrefersExams)
                {
                    // Still show exams if user has specific exam goal
                    if (string.IsNullOrEmpty(preferences.TargetExamType))
                    {
                        return new List<Exam>();
                    }
                }
            }

            // 4. Get recommendations with scoring
            var exams = await query.ToListAsync();

            var scoredExams = exams.Select(e => new
            {
                Exam = e,
                Score = CalculateExamScore(e, preferences, userId)
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .Select(x => x.Exam)
            .ToList();

            _logger.LogInformation("Generated {Count} exam recommendations for user {UserId}", 
                scoredExams.Count, userId);

            return scoredExams;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating exam recommendations for user {UserId}", userId);
            
            // Fallback: return recent exams
            return await _context.Exams
                .Where(e => e.IsPublic)
                .Include(e => e.Questions)
                .OrderByDescending(e => e.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }

    // SCORING ALGORITHMS

    private double CalculateFlashcardSetScore(FlashcardSet set, UserPreferences? preferences, string userId)
    {
        double score = 0;

        if (preferences == null)
        {
            // Base score for new users
            return set.Flashcards.Count * 0.1; // More cards = slightly better
        }

        // Subject match (high weight)
        var setSubjectIds = set.Subjects.Select(s => s.Id).ToList();
        var interestedSubjectIds = preferences.InterestedSubjects.Select(s => s.Id).ToList();
        var weakSubjectIds = preferences.WeakSubjects.Select(s => s.Id).ToList();

        var matchedInterestedSubjects = setSubjectIds.Intersect(interestedSubjectIds).Count();
        var matchedWeakSubjects = setSubjectIds.Intersect(weakSubjectIds).Count();

        score += matchedInterestedSubjects * 50; // Interested subjects: +50 per match
        score += matchedWeakSubjects * 70;       // Weak subjects: +70 per match (prioritize improvement)

        // Card count match to daily study time
        var estimatedMinutes = set.Flashcards.Count * 2; // ~2 min per card
        var timeDifference = Math.Abs(estimatedMinutes - preferences.DailyStudyTimeMinutes);
        score += Math.Max(0, 30 - timeDifference * 0.5); // Prefer sets matching study time

        // Motivation level influence
        score += preferences.MotivationLevel * 5;

        // Recency bonus (newer sets might be better)
        var daysSinceCreation = (DateTime.UtcNow - set.CreatedAt).TotalDays;
        score += Math.Max(0, 20 - daysSinceCreation * 0.1);

        return score;
    }

    private double CalculateQuizScore(Quiz quiz, UserPreferences? preferences, string userId)
    {
        double score = 0;

        if (preferences == null)
        {
            return quiz.Questions.Count * 0.1;
        }

        // Subject match
        var quizSubjectIds = quiz.Subjects.Select(s => s.Id).ToList();
        var interestedSubjectIds = preferences.InterestedSubjects.Select(s => s.Id).ToList();
        var weakSubjectIds = preferences.WeakSubjects.Select(s => s.Id).ToList();

        var matchedInterestedSubjects = quizSubjectIds.Intersect(interestedSubjectIds).Count();
        var matchedWeakSubjects = quizSubjectIds.Intersect(weakSubjectIds).Count();

        score += matchedInterestedSubjects * 60;
        score += matchedWeakSubjects * 80;

        // Difficulty match
        var targetDifficulty = GetDifficulty(preferences.PreferredDifficulty);
        if (quiz.Difficulty == targetDifficulty)
        {
            score += 40; // Perfect difficulty match
        }

        // Learning mode preference
        if (quiz.IsLearningMode)
        {
            score += 20; // Learning mode quizzes are more educational
        }

        // Question count (not too short, not too long)
        var idealQuestionCount = 10;
        var questionDifference = Math.Abs(quiz.Questions.Count - idealQuestionCount);
        score += Math.Max(0, 20 - questionDifference);

        return score;
    }

    private double CalculateExamScore(Exam exam, UserPreferences? preferences, string userId)
    {
        double score = 0;

        if (preferences == null)
        {
            return exam.Questions.Count * 0.05;
        }

        // Target university match
        if (preferences.TargetUniversityId.HasValue && exam.UniversityId == preferences.TargetUniversityId)
        {
            score += 80;
        }

        // Subject match
        var examSubjectIds = exam.Subjects.Select(s => s.Id).ToList();
        var interestedSubjectIds = preferences.InterestedSubjects.Select(s => s.Id).ToList();

        var matchedSubjects = examSubjectIds.Intersect(interestedSubjectIds).Count();
        score += matchedSubjects * 40;

        return score;
    }

    // HELPER METHODS

    private string GetDifficulty(int preferredDifficulty)
    {
        return preferredDifficulty switch
        {
            1 => "Easy",
            2 => "Medium",
            3 => "Hard",
            _ => "Medium"
        };
    }

    private string GetFlashcardDifficulty(int preferredDifficulty)
    {
        // Flashcards don't have explicit difficulty, but we can estimate from card count
        return preferredDifficulty switch
        {
            1 => "Beginner",
            2 => "Intermediate",
            3 => "Advanced",
            _ => "Intermediate"
        };
    }
}
