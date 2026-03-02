import api from './api';
import type { StrategyGuide, StrategyGuideSummary } from '../types';

export const strategyService = {
  async getGuides(examTypeCode: string): Promise<StrategyGuideSummary[]> {
    const response = await api.get<StrategyGuideSummary[]>('/strategies', { params: { examTypeCode } });
    return response.data;
  },

  async getGuide(guideId: number): Promise<StrategyGuide> {
    const response = await api.get<StrategyGuide>(`/strategies/${guideId}`);
    return response.data;
  },

  async markRead(guideId: number): Promise<void> {
    await api.post(`/strategies/${guideId}/read`);
  },
};
