using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class ScorePredictionService : IScorePredictionService
{
    private readonly UniStartDbContext _db;
    private readonly ILogger<ScorePredictionService> _logger;

    // θ range for mapping: practical limits
    private const double ThetaMin = -3.0;
    private const double ThetaMax = 3.0;

    public ScorePredictionService(UniStartDbContext db, ILogger<ScorePredictionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════
    //  PREDICT SCORE
    // ═══════════════════════════════════════════════════════

    public async Task<ScorePredictionDto> PredictScoreAsync(int userId, string examTypeCode)
    {
        var exam = await _db.ExamTypes.FirstOrDefaultAsync(e => e.Code == examTypeCode)
            ?? throw new InvalidOperationException($"Exam type '{examTypeCode}' not found");

        // Load sections for this exam
        var sections = await _db.ExamSections
            .Where(s => s.ExamTypeCode == examTypeCode)
            .ToListAsync();

        // Load user skill profiles
        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SkillId);

        // Load topics per section to map skills → sections
        var topicsPerSection = await _db.Topics
            .Where(t => t.SectionId != null && t.Section!.ExamTypeCode == examTypeCode)
            .Include(t => t.Skill)
            .GroupBy(t => t.SectionId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        // Load user answer stats for accuracy
        var answerStats = await _db.UserAnswers
            .Where(a => a.UserId == userId && a.Question!.Topic!.Section != null
                && a.Question.Topic.Section.ExamTypeCode == examTypeCode)
            .GroupBy(a => a.Question!.Topic!.SectionId)
            .Select(g => new
            {
                SectionId = g.Key,
                Total = g.Count(),
                Correct = g.Count(a => a.AnswerOption!.IsCorrect)
            })
            .ToDictionaryAsync(x => x.SectionId ?? 0);

        // Calculate per-section predictions
        var sectionPredictions = new List<SectionPredictionDto>();
        int totalPredicted = 0;
        int totalMin = 0;
        int totalMax = 0;
        int totalConfLow = 0;
        int totalConfHigh = 0;

        foreach (var section in sections)
        {
            var topics = topicsPerSection.GetValueOrDefault(section.Id, new List<Topic>());
            var (theta, thetaSE) = GetSectionTheta(topics, profiles);
            
            var predicted = ThetaToScore(theta, section.MinScore, section.MaxScore);
            var confLow = ThetaToScore(theta - 1.645 * thetaSE, section.MinScore, section.MaxScore);
            var confHigh = ThetaToScore(theta + 1.645 * thetaSE, section.MinScore, section.MaxScore);

            var stats = answerStats.GetValueOrDefault(section.Id);
            var accuracy = stats != null && stats.Total > 0
                ? Math.Round((double)stats.Correct / stats.Total * 100, 1)
                : 0;

            var strength = theta switch
            {
                >= 1.0 => "strong",
                >= 0.0 => "average",
                >= -1.0 => "weak",
                _ => "critical"
            };

            sectionPredictions.Add(new SectionPredictionDto(
                SectionId: section.Id,
                SectionName: section.Name,
                Theta: Math.Round(theta, 2),
                ThetaSE: Math.Round(thetaSE, 2),
                PredictedScore: predicted,
                MinScore: section.MinScore,
                MaxScore: section.MaxScore,
                ConfidenceLow: confLow,
                ConfidenceHigh: confHigh,
                Accuracy: accuracy,
                Strength: strength
            ));

            totalPredicted += predicted;
            totalMin += section.MinScore;
            totalMax += section.MaxScore;
            totalConfLow += confLow;
            totalConfHigh += confHigh;
        }

        // Get target score from active goal
        var goal = await _db.StudyGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive && g.ExamTypeCode == examTypeCode);

        // Generate improvement tips (top weaknesses sorted by ROI)
        var tips = await GenerateImprovementTips(userId, examTypeCode, sections, profiles, topicsPerSection);

        return new ScorePredictionDto(
            ExamTypeCode: examTypeCode,
            ExamName: exam.Name,
            PredictedScore: totalPredicted,
            MinPossibleScore: totalMin,
            MaxPossibleScore: totalMax,
            ConfidenceLow: totalConfLow,
            ConfidenceHigh: totalConfHigh,
            ConfidencePercent: 90, // 1.645σ = 90% CI
            TargetScore: goal?.TargetScore,
            GapToTarget: goal != null ? Math.Max(0, goal.TargetScore - totalPredicted) : null,
            Sections: sectionPredictions,
            ImprovementTips: tips,
            CalculatedAt: DateTime.UtcNow
        );
    }

    // ═══════════════════════════════════════════════════════
    //  WHAT-IF SCENARIO
    // ═══════════════════════════════════════════════════════

    public async Task<WhatIfResultDto> WhatIfAsync(int userId, string examTypeCode, int topicId, int improvedLevel)
    {
        var topic = await _db.Topics
            .Include(t => t.Skill)
            .Include(t => t.Section)
            .FirstOrDefaultAsync(t => t.Id == topicId)
            ?? throw new InvalidOperationException("Topic not found");

        // Current prediction
        var current = await PredictScoreAsync(userId, examTypeCode);

        // Calculate what would happen with improved theta for this topic's skill
        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SkillId);

        var currentProfile = profiles.GetValueOrDefault(topic.SkillId);
        var currentLevel = currentProfile != null ? IrtMath.ThetaToLevel(currentProfile.Theta) : 50;
        var currentTheta = currentProfile?.Theta ?? 0.0;
        var improvedTheta = IrtMath.LevelToTheta(Math.Clamp(improvedLevel, 1, 99));

        // Temporarily compute improved score
        var sections = await _db.ExamSections
            .Where(s => s.ExamTypeCode == examTypeCode)
            .ToListAsync();

        var topicsPerSection = await _db.Topics
            .Where(t => t.SectionId != null && t.Section!.ExamTypeCode == examTypeCode)
            .Include(t => t.Skill)
            .GroupBy(t => t.SectionId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        // Clone profiles and override the target skill
        var modifiedProfiles = new Dictionary<int, UserSkillProfile>(profiles);
        if (modifiedProfiles.ContainsKey(topic.SkillId))
        {
            // Create a modified copy
            var orig = modifiedProfiles[topic.SkillId];
            modifiedProfiles[topic.SkillId] = new UserSkillProfile
            {
                UserId = orig.UserId,
                SkillId = orig.SkillId,
                Level = improvedLevel,
                Theta = improvedTheta,
                ThetaSE = orig.ThetaSE * 0.8, // Assume SE decreases with practice
                LastUpdated = orig.LastUpdated
            };
        }
        else
        {
            modifiedProfiles[topic.SkillId] = new UserSkillProfile
            {
                UserId = userId,
                SkillId = topic.SkillId,
                Level = improvedLevel,
                Theta = improvedTheta,
                ThetaSE = 0.5,
                LastUpdated = DateTime.UtcNow
            };
        }

        int improvedTotal = 0;
        foreach (var section in sections)
        {
            var topics = topicsPerSection.GetValueOrDefault(section.Id, new List<Topic>());
            var (theta, _) = GetSectionTheta(topics, modifiedProfiles);
            improvedTotal += ThetaToScore(theta, section.MinScore, section.MaxScore);
        }

        return new WhatIfResultDto(
            TopicName: topic.Name,
            CurrentLevel: currentLevel,
            ImprovedLevel: improvedLevel,
            CurrentPredictedTotal: current.PredictedScore,
            ImprovedPredictedTotal: improvedTotal,
            ScoreGain: improvedTotal - current.PredictedScore
        );
    }

    // ═══════════════════════════════════════════════════════
    //  PREDICTION HISTORY
    // ═══════════════════════════════════════════════════════

    public async Task<IEnumerable<PredictionHistoryDto>> GetPredictionHistoryAsync(
        int userId, string examTypeCode, int days = 30)
    {
        var sections = await _db.ExamSections
            .Where(s => s.ExamTypeCode == examTypeCode)
            .ToListAsync();

        var topicsPerSection = await _db.Topics
            .Where(t => t.SectionId != null && t.Section!.ExamTypeCode == examTypeCode)
            .Include(t => t.Skill)
            .GroupBy(t => t.SectionId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        // Get the user's answer history dates
        var startDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-days), DateTimeKind.Utc);
        var answers = await _db.UserAnswers
            .Where(a => a.UserId == userId
                && a.Question!.Topic!.Section != null
                && a.Question.Topic.Section.ExamTypeCode == examTypeCode
                && a.AnsweredAt >= startDate)
            .OrderBy(a => a.AnsweredAt)
            .Select(a => new { a.AnsweredAt, IsCorrect = a.AnswerOption!.IsCorrect, SkillId = a.Question!.Topic!.SkillId })
            .ToListAsync();

        if (!answers.Any())
            return Enumerable.Empty<PredictionHistoryDto>();

        // Group by date and reconstruct progressive skill estimates
        var answersByDate = answers
            .GroupBy(a => a.AnsweredAt.Date)
            .OrderBy(g => g.Key)
            .ToList();

        // Load current profiles as baseline
        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SkillId);

        var history = new List<PredictionHistoryDto>();

        // For each date, compute running accuracy → approximate theta → score
        var cumulativeCorrect = new Dictionary<int, (int correct, int total)>();

        foreach (var dayGroup in answersByDate)
        {
            foreach (var a in dayGroup)
            {
                if (!cumulativeCorrect.ContainsKey(a.SkillId))
                    cumulativeCorrect[a.SkillId] = (0, 0);

                var (c, t) = cumulativeCorrect[a.SkillId];
                cumulativeCorrect[a.SkillId] = (c + (a.IsCorrect ? 1 : 0), t + 1);
            }

            // Build temporary profiles based on cumulative accuracy
            var tempProfiles = new Dictionary<int, UserSkillProfile>(profiles);
            foreach (var (skillId, (correct, total)) in cumulativeCorrect)
            {
                if (total >= 2) // Need at least 2 answers for meaningful estimate
                {
                    var accuracy = (double)correct / total;
                    // Map accuracy (0-1) to approximate theta (-3 to +3)
                    var approxTheta = AccuracyToTheta(accuracy);
                    var approxSE = 1.0 / Math.Sqrt(total); // SE decreases with more data

                    if (tempProfiles.ContainsKey(skillId))
                    {
                        var orig = tempProfiles[skillId];
                        tempProfiles[skillId] = new UserSkillProfile
                        {
                            UserId = userId, SkillId = skillId,
                            Theta = approxTheta, ThetaSE = approxSE,
                            Level = IrtMath.ThetaToLevel(approxTheta),
                            LastUpdated = dayGroup.Key
                        };
                    }
                    else
                    {
                        tempProfiles[skillId] = new UserSkillProfile
                        {
                            UserId = userId, SkillId = skillId,
                            Theta = approxTheta, ThetaSE = approxSE,
                            Level = IrtMath.ThetaToLevel(approxTheta),
                            LastUpdated = dayGroup.Key
                        };
                    }
                }
            }

            // Compute prediction for this date
            int predicted = 0, confLow = 0, confHigh = 0;
            foreach (var section in sections)
            {
                var topics = topicsPerSection.GetValueOrDefault(section.Id, new List<Topic>());
                var (theta, se) = GetSectionTheta(topics, tempProfiles);
                predicted += ThetaToScore(theta, section.MinScore, section.MaxScore);
                confLow += ThetaToScore(theta - 1.645 * se, section.MinScore, section.MaxScore);
                confHigh += ThetaToScore(theta + 1.645 * se, section.MinScore, section.MaxScore);
            }

            history.Add(new PredictionHistoryDto(
                Date: DateTime.SpecifyKind(dayGroup.Key, DateTimeKind.Utc),
                PredictedScore: predicted,
                ConfidenceLow: confLow,
                ConfidenceHigh: confHigh
            ));
        }

        return history;
    }

    // ═══════════════════════════════════════════════════════
    //  IMPROVEMENT TIPS GENERATION
    // ═══════════════════════════════════════════════════════

    private Task<List<ImprovementTipDto>> GenerateImprovementTips(
        int userId,
        string examTypeCode,
        List<ExamSection> sections,
        Dictionary<int, UserSkillProfile> profiles,
        Dictionary<int, List<Topic>> topicsPerSection)
    {
        var tips = new List<ImprovementTipDto>();
        var allTopics = topicsPerSection.Values.SelectMany(t => t).ToList();

        foreach (var topic in allTopics)
        {
            var profile = profiles.GetValueOrDefault(topic.SkillId);
            var theta = profile?.Theta ?? 0.0;
            var currentLevel = profile != null ? IrtMath.ThetaToLevel(theta) : 50;

            // Only suggest improvement for topics below mastery
            if (currentLevel >= 80) continue;

            // Calculate potential score gain if this skill improves by 20 level points
            var improvedTheta = IrtMath.LevelToTheta(Math.Min(currentLevel + 20, 95));
            int currentTotal = 0, improvedTotal = 0;

            foreach (var section in sections)
            {
                var sectionTopics = topicsPerSection.GetValueOrDefault(section.Id, new List<Topic>());
                var (sTheta, _) = GetSectionTheta(sectionTopics, profiles);
                currentTotal += ThetaToScore(sTheta, section.MinScore, section.MaxScore);

                // If this topic belongs to this section, use improved theta
                if (sectionTopics.Any(t => t.SkillId == topic.SkillId))
                {
                    var tempProfiles = new Dictionary<int, UserSkillProfile>(profiles);
                    tempProfiles[topic.SkillId] = new UserSkillProfile
                    {
                        UserId = userId, SkillId = topic.SkillId,
                        Theta = improvedTheta, ThetaSE = 0.5,
                        Level = currentLevel + 20, LastUpdated = DateTime.UtcNow
                    };
                    var (iTheta, _) = GetSectionTheta(sectionTopics, tempProfiles);
                    improvedTotal += ThetaToScore(iTheta, section.MinScore, section.MaxScore);
                }
                else
                {
                    improvedTotal += ThetaToScore(sTheta, section.MinScore, section.MaxScore);
                }
            }

            var gain = improvedTotal - currentTotal;
            if (gain <= 0) continue;

            var sectionName = allTopics.FirstOrDefault(t => t.Id == topic.Id)?.Section?.Name ?? "";
            var recommendation = currentLevel switch
            {
                < 30 => $"Начните с основ {topic.Name} — потенциал роста очень высокий",
                < 50 => $"Усильте практику {topic.Name} — значительный резерв для улучшения",
                < 70 => $"Продолжайте тренировать {topic.Name} — почти на хорошем уровне",
                _ => $"Доведите {topic.Name} до мастерства для максимального результата"
            };

            tips.Add(new ImprovementTipDto(
                TopicId: topic.Id,
                TopicName: topic.Name,
                SectionName: sectionName,
                CurrentTheta: Math.Round(theta, 2),
                CurrentLevel: currentLevel,
                PotentialScoreGain: gain,
                Recommendation: recommendation
            ));
        }

        // Sort by ROI: highest potential score gain first
        return Task.FromResult(tips.OrderByDescending(t => t.PotentialScoreGain)
            .DistinctBy(t => t.TopicName)
            .Take(5)
            .ToList());
    }

    // ═══════════════════════════════════════════════════════
    //  CORE MATH
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Get the aggregate θ for a section by averaging thetas of related skills.
    /// Returns (theta, se).
    /// </summary>
    private static (double theta, double se) GetSectionTheta(
        List<Topic> sectionTopics,
        Dictionary<int, UserSkillProfile> profiles)
    {
        if (!sectionTopics.Any())
            return (0.0, 1.5);

        var skillIds = sectionTopics.Select(t => t.SkillId).Distinct().ToList();
        var thetas = new List<double>();
        var ses = new List<double>();

        foreach (var skillId in skillIds)
        {
            if (profiles.TryGetValue(skillId, out var p))
            {
                thetas.Add(p.Theta);
                ses.Add(p.ThetaSE > 0 ? p.ThetaSE : 1.0);
            }
            else
            {
                thetas.Add(0.0); // Default: average ability
                ses.Add(1.5);    // High uncertainty
            }
        }

        // Weighted average: weight by inverse SE (more certain = more weight)
        var weights = ses.Select(se => 1.0 / (se * se)).ToList();
        var totalWeight = weights.Sum();
        var avgTheta = thetas.Zip(weights, (t, w) => t * w).Sum() / totalWeight;
        var avgSE = Math.Sqrt(1.0 / totalWeight); // Combined SE

        return (avgTheta, avgSE);
    }

    /// <summary>
    /// Map θ (latent ability) to a score within [minScore, maxScore] using logistic mapping.
    /// </summary>
    private static int ThetaToScore(double theta, int minScore, int maxScore)
    {
        // Logistic mapping: maps θ ∈ [-3, 3] → proportion ∈ [0, 1]
        var proportion = 1.0 / (1.0 + Math.Exp(-1.2 * theta));
        var score = minScore + proportion * (maxScore - minScore);
        return (int)Math.Round(Math.Clamp(score, minScore, maxScore));
    }

    /// <summary>
    /// Approximate θ from accuracy (for history reconstruction).
    /// </summary>
    private static double AccuracyToTheta(double accuracy)
    {
        // Inverse logistic: accuracy ∈ (0, 1) → θ
        accuracy = Math.Clamp(accuracy, 0.05, 0.95);
        return Math.Log(accuracy / (1.0 - accuracy)) / 1.2;
    }
}
