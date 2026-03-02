import api from './api';
import type { MistakeEntry, MistakeAnalysis } from '../types';

export const mistakeService = {
  async getMistakes(params?: {
    examTypeCode?: string;
    topicId?: number;
    errorType?: string;
    page?: number;
    pageSize?: number;
  }): Promise<MistakeEntry[]> {
    const response = await api.get<MistakeEntry[]>('/mistakes', { params });
    return response.data;
  },

  async setErrorType(userAnswerId: number, errorType: string | null): Promise<void> {
    await api.post('/mistakes/error-type', { userAnswerId, errorType });
  },

  async setNote(userAnswerId: number, noteText: string): Promise<void> {
    await api.post('/mistakes/note', { userAnswerId, noteText });
  },

  async getAnalysis(examTypeCode?: string): Promise<MistakeAnalysis> {
    const params = examTypeCode ? { examTypeCode } : {};
    const response = await api.get<MistakeAnalysis>('/mistakes/analysis', { params });
    return response.data;
  },

  async getMistakeCount(): Promise<number> {
    const response = await api.get<{ count: number }>('/mistakes/count');
    return response.data.count;
  },
};
