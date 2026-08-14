import api from './api';
import type {
  ScorePrediction,
  WhatIfResult,
  WhatIfRequest,
  PredictionHistory,
} from '../types';

export const predictionService = {
  async getPrediction(examTypeCode: string): Promise<ScorePrediction> {
    const response = await api.get<ScorePrediction>(`/prediction/${examTypeCode}`);
    return response.data;
  },

  async whatIf(request: WhatIfRequest): Promise<WhatIfResult> {
    const response = await api.post<WhatIfResult>('/prediction/what-if', request);
    return response.data;
  },

  async getHistory(examTypeCode: string, days: number = 30): Promise<PredictionHistory[]> {
    const response = await api.get<PredictionHistory[]>(
      `/prediction/history/${examTypeCode}?days=${days}`
    );
    return response.data;
  },
};
