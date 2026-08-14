namespace UniStart.Application.DTOs;


public record PredictionRequest(
    string ExamTypeCode
);

public record ScorePredictionDto(
    string ExamTypeCode,
    string ExamName,
    int PredictedScore,
    int MinPossibleScore,
    int MaxPossibleScore,
    int ConfidenceLow,
    int ConfidenceHigh,
    double ConfidencePercent,
    int? TargetScore,
    int? GapToTarget,
    IEnumerable<SectionPredictionDto> Sections,
    IEnumerable<ImprovementTipDto> ImprovementTips,
    DateTime CalculatedAt,
    int AnswersCount = 0,
    bool IsReliable = false
);

public record SectionPredictionDto(
    int SectionId,
    string SectionName,
    double Theta,
    double ThetaSE,
    int PredictedScore,
    int MinScore,
    int MaxScore,
    int ConfidenceLow,
    int ConfidenceHigh,
    double Accuracy,
    string Strength,
    int AnswersCount = 0,
    bool IsReliable = false
);

public record ImprovementTipDto(
    int TopicId,
    string TopicName,
    string SectionName,
    double CurrentTheta,
    int CurrentLevel,
    int PotentialScoreGain,
    string Recommendation
);

public record WhatIfResultDto(
    string TopicName,
    int CurrentLevel,
    int ImprovedLevel,
    int CurrentPredictedTotal,
    int ImprovedPredictedTotal,
    int ScoreGain
);

public record PredictionHistoryDto(
    DateTime Date,
    int PredictedScore,
    int ConfidenceLow,
    int ConfidenceHigh
);
