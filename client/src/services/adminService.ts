import api from './api';
import type { QuestionListItem, QuestionDetail, QuestionStats, BulkImportResult } from '../types';

const adminService = {
  getQuestions: (examTypeCode?: string, topic?: string, difficulty?: string) => {
    const params = new URLSearchParams();
    if (examTypeCode) params.set('examTypeCode', examTypeCode);
    if (topic) params.set('topic', topic);
    if (difficulty) params.set('difficulty', difficulty);
    const qs = params.toString();
    return api.get<QuestionListItem[]>(`/admin/questions${qs ? '?' + qs : ''}`).then(r => r.data);
  },

  getQuestion: (id: number) =>
    api.get<QuestionDetail>(`/admin/questions/${id}`).then(r => r.data),

  createQuestion: (data: {
    topicId: number;
    text: string;
    difficulty: string;
    explanation?: string;
    difficultyParam?: number;
    discriminationParam?: number;
    guessParam?: number;
    answerOptions: { text: string; isCorrect: boolean }[];
  }) => api.post<QuestionDetail>('/admin/questions', data).then(r => r.data),

  updateQuestion: (id: number, data: {
    text?: string;
    difficulty?: string;
    explanation?: string;
    difficultyParam?: number;
    discriminationParam?: number;
    guessParam?: number;
    answerOptions?: { text: string; isCorrect: boolean }[];
  }) => api.put<QuestionDetail>(`/admin/questions/${id}`, data).then(r => r.data),

  deleteQuestion: (id: number) =>
    api.delete(`/admin/questions/${id}`),

  bulkImport: (questions: {
    topicId: number;
    text: string;
    difficulty: string;
    explanation?: string;
    answerOptions: { text: string; isCorrect: boolean }[];
  }[]) => api.post<BulkImportResult>('/admin/questions/import', { questions }).then(r => r.data),

  getStats: () =>
    api.get<QuestionStats>('/admin/stats').then(r => r.data),
};

export default adminService;
