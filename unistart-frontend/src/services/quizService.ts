import api from './api';
import {
  Quiz,
  QuizDetail,
  SubmitQuizDto,
  QuizResult,
  UserQuizAttempt,
} from '../types';

export const quizService = {
  async getQuizzes(filters?: { subject?: string; difficulty?: string }): Promise<Quiz[]> {
    const { data } = await api.get<Quiz[]>('/quizzes', { params: filters });
    return data;
  },

  async getQuiz(id: number): Promise<QuizDetail> {
    const { data } = await api.get<QuizDetail>(`/quizzes/${id}`);
    return data;
  },

  async submitQuiz(dto: SubmitQuizDto): Promise<QuizResult> {
    const { data } = await api.post<QuizResult>('/quizzes/submit', dto);
    return data;
  },

  async getAttempts(quizId: number): Promise<UserQuizAttempt[]> {
    const { data } = await api.get<UserQuizAttempt[]>(`/quizzes/${quizId}/attempts`);
    return data;
  },

  async getStatistics(quizId: number): Promise<any> {
    const { data } = await api.get(`/quizzes/${quizId}/statistics`);
    return data;
  },
};
