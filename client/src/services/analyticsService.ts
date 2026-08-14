import api from './api';
import type { SkillAnalytics, UserSkillProfile, Dashboard, TestSessionSummary, TestSessionDetail, SkillHistoryPoint, DailyActivity, DifficultyStats } from '../types';

export const analyticsService = {
  async getAnalytics(): Promise<SkillAnalytics> {
    const response = await api.get<SkillAnalytics>('/analytics');
    return response.data;
  },

  async getSkills(): Promise<UserSkillProfile[]> {
    const response = await api.get<UserSkillProfile[]>('/analytics/skills');
    return response.data;
  },

  async getDashboard(): Promise<Dashboard> {
    const response = await api.get<Dashboard>('/analytics/dashboard');
    return response.data;
  },

  async getSkillHistory(days: number = 30): Promise<SkillHistoryPoint[]> {
    const response = await api.get<SkillHistoryPoint[]>('/analytics/skill-history', { params: { days } });
    return response.data;
  },

  async getActivity(days: number = 90): Promise<DailyActivity[]> {
    const response = await api.get<DailyActivity[]>('/analytics/activity', { params: { days } });
    return response.data;
  },

  async getDifficultyBreakdown(): Promise<DifficultyStats[]> {
    const response = await api.get<DifficultyStats[]>('/analytics/difficulty');
    return response.data;
  },

  async startSession(examTypeCode: string, mode: string = 'practice'): Promise<TestSessionSummary> {
    const response = await api.post<TestSessionSummary>('/analytics/sessions', { examTypeCode, mode });
    return response.data;
  },

  async completeSession(sessionId: number): Promise<TestSessionSummary> {
    const response = await api.post<TestSessionSummary>(`/analytics/sessions/${sessionId}/complete`);
    return response.data;
  },

  async getSessions(page: number = 1, pageSize: number = 10): Promise<TestSessionSummary[]> {
    const response = await api.get<TestSessionSummary[]>('/analytics/sessions', { params: { page, pageSize } });
    return response.data;
  },

  async getSessionDetail(sessionId: number): Promise<TestSessionDetail> {
    const response = await api.get<TestSessionDetail>(`/analytics/sessions/${sessionId}`);
    return response.data;
  },
};
