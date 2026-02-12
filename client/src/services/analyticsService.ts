import api from './api';
import type { SkillAnalytics, UserSkillProfile } from '../types';

export const analyticsService = {
  async getAnalytics(): Promise<SkillAnalytics> {
    const response = await api.get<SkillAnalytics>('/analytics');
    return response.data;
  },

  async getSkills(): Promise<UserSkillProfile[]> {
    const response = await api.get<UserSkillProfile[]>('/analytics/skills');
    return response.data;
  },
};
