namespace UniStart.Application.DTOs;

// ─── Requests ────────────────────────────────────────────────

public record PredictionRequest(
    string ExamTypeCode
);

// ─── Responses ───────────────────────────────────────────────

/// <summary>
/// Full exam score prediction with section breakdown
/// </summary>
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
    DateTime CalculatedAt
);

/// <summary>
/// Per-section predicted score
/// </summary>
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
    string Strength
);

/// <summary>
/// Recommendation to improve score
/// </summary>
public record ImprovementTipDto(
    int TopicId,
    string TopicName,
    string SectionName,
    double CurrentTheta,
    int CurrentLevel,
    int PotentialScoreGain,
    string Recommendation
);

/// <summary>
/// What-if scenario result
/// </summary>
public record WhatIfResultDto(
    string TopicName,
    int CurrentLevel,
    int ImprovedLevel,
    int CurrentPredictedTotal,
    int ImprovedPredictedTotal,
    int ScoreGain
);

/// <summary>
/// Prediction history point for charting
/// </summary>
public record PredictionHistoryDto(
    DateTime Date,
    int PredictedScore,
    int ConfidenceLow,
    int ConfidenceHigh
);
