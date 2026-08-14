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

    private const double ThetaMin = -3.0;
    private const double ThetaMax = 3.0;

    private const int SectionReliableThreshold = 10;

    public ScorePredictionService(UniStartDbContext db, ILogger<ScorePredictionService> logger)
    {
        _db = db;
        _logger = logger;
    }


    public async Task<ScorePredictionDto> PredictScoreAsync(int userId, string examTypeCode)
    {
        var exam = await _db.ExamTypes.FirstOrDefaultAsync(e => e.Code == examTypeCode)
            ?? throw new InvalidOperationException($"Exam type '{examTypeCode}' not found");

        var sections = await _db.ExamSections
            .Where(s => s.ExamTypeCode == examTypeCode)
            .ToListAsync();

        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SectionId);

        var topicsPerSection = await _db.Topics
            .Where(t => t.SectionId != null && t.Section!.ExamTypeCode == examTypeCode)
            .GroupBy(t => t.SectionId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());

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

        var sectionPredictions = new List<SectionPredictionDto>();
        int totalPredicted = 0;
        int totalMin = 0;
        int totalMax = 0;
        int totalConfLow = 0;
        int totalConfHigh = 0;

        foreach (var section in sections)
        {
            var (theta, thetaSE) = GetSectionTheta(section.Id, profiles);
            
            var predicted = ThetaToScore(theta, section.MinScore, section.MaxScore);
            var confLow = ThetaToScore(theta - 1.645 * thetaSE, section.MinScore, section.MaxScore);
            var confHigh = ThetaToScore(theta + 1.645 * thetaSE, section.MinScore, section.MaxScore);

            var stats = answerStats.GetValueOrDefault(section.Id);
            var accuracy = stats != null && stats.Total > 0
                ? Math.Round((double)stats.Correct / stats.Total * 100, 1)
                : 0;

            var sectionAnswers = stats?.Total ?? 0;
            var sectionReliable = sectionAnswers >= SectionReliableThreshold;

            var strength = !sectionReliable
                ? "insufficient"
                : theta switch
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
                Strength: strength,
                AnswersCount: sectionAnswers,
                IsReliable: sectionReliable
            ));

            totalPredicted += predicted;
            totalMin += section.MinScore;
            totalMax += section.MaxScore;
            totalConfLow += confLow;
            totalConfHigh += confHigh;
        }

        var goal = await _db.StudyGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive && g.ExamTypeCode == examTypeCode);

        var tips = await GenerateImprovementTips(userId, examTypeCode, sections, profiles, topicsPerSection);

        var totalAnswers = sectionPredictions.Sum(s => s.AnswersCount);
        var reliableSections = sectionPredictions.Count(s => s.IsReliable);
        var overallReliable = totalAnswers >= SectionReliableThreshold
            && (sectionPredictions.Count == 0 || reliableSections * 2 >= sectionPredictions.Count);

        return new ScorePredictionDto(
            ExamTypeCode: examTypeCode,
            ExamName: exam.Name,
            PredictedScore: totalPredicted,
            MinPossibleScore: totalMin,
            MaxPossibleScore: totalMax,
            ConfidenceLow: totalConfLow,
            ConfidenceHigh: totalConfHigh,
            ConfidencePercent: 90,
            TargetScore: goal?.TargetScore,
            GapToTarget: goal != null ? Math.Max(0, goal.TargetScore - totalPredicted) : null,
            Sections: sectionPredictions,
            ImprovementTips: tips,
            CalculatedAt: DateTime.UtcNow,
            AnswersCount: totalAnswers,
            IsReliable: overallReliable
        );
    }


    public async Task<WhatIfResultDto> WhatIfAsync(int userId, string examTypeCode, int topicId, int improvedLevel)
    {
        var topic = await _db.Topics
            .Include(t => t.Section)
            .FirstOrDefaultAsync(t => t.Id == topicId)
            ?? throw new InvalidOperationException("Topic not found");

        var sectionId = topic.SectionId
            ?? throw new InvalidOperationException("Topic has no section");

        var current = await PredictScoreAsync(userId, examTypeCode);

        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SectionId);

        var currentProfile = profiles.GetValueOrDefault(sectionId);
        var currentLevel = currentProfile != null ? IrtMath.ThetaToLevel(currentProfile.Theta) : 50;
        var improvedTheta = IrtMath.LevelToTheta(Math.Clamp(improvedLevel, 1, 99));

        var sections = await _db.ExamSections
            .Where(s => s.ExamTypeCode == examTypeCode)
            .ToListAsync();

        var modifiedProfiles = new Dictionary<int, UserSkillProfile>(profiles);
        modifiedProfiles[sectionId] = new UserSkillProfile
        {
            UserId = userId,
            SectionId = sectionId,
            Level = improvedLevel,
            Theta = improvedTheta,
            ThetaSE = currentProfile != null ? currentProfile.ThetaSE * 0.8 : 0.5,
            LastUpdated = DateTime.UtcNow
        };

        int improvedTotal = 0;
        foreach (var section in sections)
        {
            var (theta, _) = GetSectionTheta(section.Id, modifiedProfiles);
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


    public async Task<IEnumerable<PredictionHistoryDto>> GetPredictionHistoryAsync(
        int userId, string examTypeCode, int days = 30)
    {
        var sections = await _db.ExamSections
            .Where(s => s.ExamTypeCode == examTypeCode)
            .ToListAsync();

        var startDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-days), DateTimeKind.Utc);
        var answers = await _db.UserAnswers
            .Where(a => a.UserId == userId
                && a.Question!.Topic!.Section != null
                && a.Question.Topic.Section.ExamTypeCode == examTypeCode
                && a.AnsweredAt >= startDate)
            .OrderBy(a => a.AnsweredAt)
            .Select(a => new { a.AnsweredAt, IsCorrect = a.AnswerOption!.IsCorrect, SectionId = a.Question!.Topic!.SectionId!.Value })
            .ToListAsync();

        if (!answers.Any())
            return Enumerable.Empty<PredictionHistoryDto>();

        var answersByDate = answers
            .GroupBy(a => a.AnsweredAt.Date)
            .OrderBy(g => g.Key)
            .ToList();

        var profiles = await _db.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.SectionId);

        var history = new List<PredictionHistoryDto>();

        var cumulativeCorrect = new Dictionary<int, (int correct, int total)>();

        foreach (var dayGroup in answersByDate)
        {
            foreach (var a in dayGroup)
            {
                if (!cumulativeCorrect.ContainsKey(a.SectionId))
                    cumulativeCorrect[a.SectionId] = (0, 0);

                var (c, t) = cumulativeCorrect[a.SectionId];
                cumulativeCorrect[a.SectionId] = (c + (a.IsCorrect ? 1 : 0), t + 1);
            }

            var tempProfiles = new Dictionary<int, UserSkillProfile>(profiles);
            foreach (var (sectionId, (correct, total)) in cumulativeCorrect)
            {
                if (total >= 2)
                {
                    var accuracy = (double)correct / total;
                    var approxTheta = AccuracyToTheta(accuracy);
                    var approxSE = 1.0 / Math.Sqrt(total);
                    tempProfiles[sectionId] = new UserSkillProfile
                    {
                        UserId = userId, SectionId = sectionId,
                        Theta = approxTheta, ThetaSE = approxSE,
                        Level = IrtMath.ThetaToLevel(approxTheta),
                        LastUpdated = dayGroup.Key
                    };
                }
            }

            int predicted = 0, confLow = 0, confHigh = 0;
            foreach (var section in sections)
            {
                var (theta, se) = GetSectionTheta(section.Id, tempProfiles);
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
            if (topic.SectionId is not int tSecId) continue;

            var profile = profiles.GetValueOrDefault(tSecId);
            var theta = profile?.Theta ?? 0.0;
            var currentLevel = profile != null ? IrtMath.ThetaToLevel(theta) : 50;

            if (currentLevel >= 80) continue;

            var improvedTheta = IrtMath.LevelToTheta(Math.Min(currentLevel + 20, 95));
            int currentTotal = 0, improvedTotal = 0;

            foreach (var section in sections)
            {
                var (sTheta, _) = GetSectionTheta(section.Id, profiles);
                currentTotal += ThetaToScore(sTheta, section.MinScore, section.MaxScore);

                if (section.Id == tSecId)
                {
                    var tempProfiles = new Dictionary<int, UserSkillProfile>(profiles);
                    tempProfiles[tSecId] = new UserSkillProfile
                    {
                        UserId = userId, SectionId = tSecId,
                        Theta = improvedTheta, ThetaSE = 0.5,
                        Level = currentLevel + 20, LastUpdated = DateTime.UtcNow
                    };
                    var (iTheta, _) = GetSectionTheta(section.Id, tempProfiles);
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

        return Task.FromResult(tips.OrderByDescending(t => t.PotentialScoreGain)
            .DistinctBy(t => t.TopicName)
            .Take(5)
            .ToList());
    }

    private static (double theta, double se) GetSectionTheta(
        int sectionId,
        Dictionary<int, UserSkillProfile> profiles)
    {
        if (profiles.TryGetValue(sectionId, out var p))
            return (p.Theta, p.ThetaSE > 0 ? p.ThetaSE : 1.0);
        return (0.0, 1.5);
    }

    private static int ThetaToScore(double theta, int minScore, int maxScore)
    {
        var proportion = 1.0 / (1.0 + Math.Exp(-1.2 * theta));
        var score = minScore + proportion * (maxScore - minScore);
        return (int)Math.Round(Math.Clamp(score, minScore, maxScore));
    }

    private static double AccuracyToTheta(double accuracy)
    {
        accuracy = Math.Clamp(accuracy, 0.05, 0.95);
        return Math.Log(accuracy / (1.0 - accuracy)) / 1.2;
    }
}
