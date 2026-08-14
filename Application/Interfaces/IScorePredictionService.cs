using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IScorePredictionService
{
    Task<ScorePredictionDto> PredictScoreAsync(int userId, string examTypeCode);

    Task<WhatIfResultDto> WhatIfAsync(int userId, string examTypeCode, int topicId, int improvedLevel);

    Task<IEnumerable<PredictionHistoryDto>> GetPredictionHistoryAsync(int userId, string examTypeCode, int days = 30);
}
