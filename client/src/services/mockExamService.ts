import api from './api';
import type {
  MockExamListItem,
  MockExamDetail,
  MockExamAttempt,
  MockExamSectionState,
  MockExamResult,
  MockExamHistoryItem,
} from '../types';

export const mockExamService = {
  async getAvailableMockExams(): Promise<MockExamListItem[]> {
    const response = await api.get<MockExamListItem[]>('/mock-exams');
    return response.data;
  },

  async getMockExamDetail(id: number): Promise<MockExamDetail> {
    const response = await api.get<MockExamDetail>(`/mock-exams/${id}`);
    return response.data;
  },

  async startMockExam(id: number): Promise<MockExamAttempt> {
    const response = await api.post<MockExamAttempt>(`/mock-exams/${id}/start`);
    return response.data;
  },

  async getCurrentSection(attemptId: number): Promise<MockExamSectionState> {
    const response = await api.get<MockExamSectionState>(`/mock-exams/attempts/${attemptId}/current-section`);
    return response.data;
  },

  async getSection(attemptId: number, sectionIndex: number): Promise<MockExamSectionState> {
    const response = await api.get<MockExamSectionState>(`/mock-exams/attempts/${attemptId}/sections/${sectionIndex}`);
    return response.data;
  },

  async submitAnswer(attemptId: number, questionId: number, selectedOptionId: number, timeSpentSeconds?: number): Promise<void> {
    await api.post(`/mock-exams/attempts/${attemptId}/answer`, {
      questionId,
      selectedOptionId,
      timeSpentSeconds,
    });
  },

  async completeSection(attemptId: number): Promise<MockExamAttempt> {
    const response = await api.post<MockExamAttempt>(`/mock-exams/attempts/${attemptId}/complete-section`);
    return response.data;
  },

  async getResults(attemptId: number): Promise<MockExamResult> {
    const response = await api.get<MockExamResult>(`/mock-exams/attempts/${attemptId}/results`);
    return response.data;
  },

  async getHistory(): Promise<MockExamHistoryItem[]> {
    const response = await api.get<MockExamHistoryItem[]>('/mock-exams/history');
    return response.data;
  },

  async abandonAttempt(attemptId: number): Promise<void> {
    await api.post(`/mock-exams/attempts/${attemptId}/abandon`);
  },

  async getActiveAttempt(): Promise<MockExamAttempt | null> {
    const response = await api.get('/mock-exams/active-attempt', { validateStatus: s => s === 200 || s === 204 });
    return response.status === 204 ? null : response.data;
  },
};
