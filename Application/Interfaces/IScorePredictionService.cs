using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IScorePredictionService
{
    /// <summary>
    /// Predict the user's exam score based on their current θ values per section
    /// </summary>
    Task<ScorePredictionDto> PredictScoreAsync(int userId, string examTypeCode);

    /// <summary>
    /// Run a what-if scenario: "If topic X improves from Y to Z, how does total change?"
    /// </summary>
    Task<WhatIfResultDto> WhatIfAsync(int userId, string examTypeCode, int topicId, int improvedLevel);

    /// <summary>
    /// Get prediction history (recalculated from answer history snapshots)
    /// </summary>
    Task<IEnumerable<PredictionHistoryDto>> GetPredictionHistoryAsync(int userId, string examTypeCode, int days = 30);
}
