import api from './api';
import type {
  StartDrillRequest, DrillQuestion, SubmitDrillAnswerRequest,
  DrillAnswerResult, DrillResult, PersonalBest,
} from '../types';

export const drillService = {
  async startDrill(request: StartDrillRequest): Promise<DrillResult> {
    const response = await api.post<DrillResult>('/drills/start', request);
    return response.data;
  },

  async getNextQuestion(drillId: number): Promise<DrillQuestion | null> {
    const response = await api.get<DrillQuestion>(`/drills/${drillId}/next`);
    return response.status === 204 ? null : response.data;
  },

  async submitAnswer(request: SubmitDrillAnswerRequest): Promise<DrillAnswerResult> {
    const response = await api.post<DrillAnswerResult>('/drills/answer', request);
    return response.data;
  },

  async completeDrill(drillId: number): Promise<DrillResult> {
    const response = await api.post<DrillResult>(`/drills/${drillId}/complete`);
    return response.data;
  },

  async getPersonalBests(): Promise<PersonalBest[]> {
    const response = await api.get<PersonalBest[]>('/drills/personal-bests');
    return response.data;
  },

  async getHistory(limit = 20): Promise<DrillResult[]> {
    const response = await api.get<DrillResult[]>('/drills/history', { params: { limit } });
    return response.data;
  },

  async getTemplates(): Promise<Array<{ id: number; title: string; description: string | null; drillType: string; examTypeCode: string | null; topicId: number | null; questionCount: number; timeLimitMinutes: number | null; sortOrder: number }>> {
    const response = await api.get('/drills/templates');
    return response.data;
  },
};
