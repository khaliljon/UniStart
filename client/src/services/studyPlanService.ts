import api from './api';
import type {
  StudyGoal,
  CreateStudyGoalRequest,
  UpdateStudyGoalRequest,
  StudyPlan,
  TodayPlan,
  StudyPlanEntry,
  CompleteEntryRequest,
  PlanStats,
} from '../types';

export const studyPlanService = {
  // ─── Goals ───────────────────────────────────────────────

  async createGoal(dto: CreateStudyGoalRequest): Promise<StudyGoal> {
    const response = await api.post<StudyGoal>('/study-plan/goals', dto);
    return response.data;
  },

  async getActiveGoal(): Promise<StudyGoal | null> {
    try {
      const response = await api.get<StudyGoal>('/study-plan/goals/active');
      return response.data;
    } catch (err: any) {
      if (err.response?.status === 404) return null;
      throw err;
    }
  },

  async updateGoal(goalId: number, dto: UpdateStudyGoalRequest): Promise<StudyGoal> {
    const response = await api.put<StudyGoal>(`/study-plan/goals/${goalId}`, dto);
    return response.data;
  },

  async deleteGoal(goalId: number): Promise<void> {
    await api.delete(`/study-plan/goals/${goalId}`);
  },

  // ─── Plan ────────────────────────────────────────────────

  async generatePlan(goalId: number): Promise<StudyPlan> {
    const response = await api.post<StudyPlan>(`/study-plan/generate/${goalId}`);
    return response.data;
  },

  async getActivePlan(): Promise<StudyPlan | null> {
    try {
      const response = await api.get<StudyPlan>('/study-plan/active');
      return response.data;
    } catch (err: any) {
      if (err.response?.status === 404) return null;
      throw err;
    }
  },

  async regeneratePlan(): Promise<StudyPlan> {
    const response = await api.post<StudyPlan>('/study-plan/regenerate');
    return response.data;
  },

  // ─── Today ───────────────────────────────────────────────

  async getTodayPlan(): Promise<TodayPlan> {
    const response = await api.get<TodayPlan>('/study-plan/today');
    return response.data;
  },

  // ─── Entry Completion ────────────────────────────────────

  async completeEntry(entryId: number, dto: CompleteEntryRequest): Promise<StudyPlanEntry> {
    const response = await api.post<StudyPlanEntry>(`/study-plan/entries/${entryId}/complete`, dto);
    return response.data;
  },

  // ─── Auto-Complete ───────────────────────────────────────

  async autoCompleteToday(): Promise<TodayPlan> {
    const response = await api.post<TodayPlan>('/study-plan/auto-complete-today');
    return response.data;
  },

  // ─── Stats ───────────────────────────────────────────────

  async getPlanStats(): Promise<PlanStats> {
    const response = await api.get<PlanStats>('/study-plan/stats');
    return response.data;
  },
};
