import api from './api';
import { DailyBriefing, AfterSession, Streak, Milestone } from '../types';

const recommendationService = {
  getDailyBriefing: (sectionIds?: number[]) => {
    const params: Record<string, string> = {};
    if (sectionIds && sectionIds.length > 0) params.sectionIds = sectionIds.join(',');
    return api.get<DailyBriefing>('/recommendations/daily', { params }).then(r => r.data);
  },

  getAfterSession: (sessionId: number) =>
    api.get<AfterSession>(`/recommendations/after-session/${sessionId}`).then(r => r.data),

  getStreak: () =>
    api.get<Streak>('/recommendations/streak').then(r => r.data),

  getMilestones: () =>
    api.get<Milestone[]>('/recommendations/milestones').then(r => r.data),

  checkMilestones: () =>
    api.post<Milestone[]>('/recommendations/check-milestones').then(r => r.data),
};

export default recommendationService;
