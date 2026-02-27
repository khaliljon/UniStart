import api from './api';
import type { QuestionListItem, QuestionDetail, QuestionStats, BulkImportResult, AdminUser, AdminUserStats, AdminTopicSummary, AdminDashboard, AdminSection, AdminSkill } from '../types';

const adminService = {
  // ─── Questions ───────────────────────────────────────────
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

  // ─── Users ───────────────────────────────────────────────
  getUsers: (role?: string, search?: string) => {
    const params = new URLSearchParams();
    if (role) params.set('role', role);
    if (search) params.set('search', search);
    const qs = params.toString();
    return api.get<AdminUser[]>(`/admin/users${qs ? '?' + qs : ''}`).then(r => r.data);
  },

  getUser: (id: number) =>
    api.get<AdminUser>(`/admin/users/${id}`).then(r => r.data),

  updateUser: (id: number, data: {
    name?: string;
    email?: string;
    role?: string;
    subscriptionTier?: string;
    subscriptionExpiresAt?: string;
  }) => api.put<AdminUser>(`/admin/users/${id}`, data).then(r => r.data),

  deleteUser: (id: number) =>
    api.delete(`/admin/users/${id}`),

  getUserStats: () =>
    api.get<AdminUserStats>('/admin/users/stats').then(r => r.data),

  // ─── Dashboard & Topics ──────────────────────────────────
  getDashboard: () =>
    api.get<AdminDashboard>('/admin/dashboard').then(r => r.data),

  getTopics: () =>
    api.get<AdminTopicSummary[]>('/admin/topics').then(r => r.data),

  createTopic: (data: { name: string; sectionId: number; skillId: number }) =>
    api.post<AdminTopicSummary>('/admin/topics', data).then(r => r.data),

  getSections: () =>
    api.get<AdminSection[]>('/admin/sections').then(r => r.data),

  getSkills: () =>
    api.get<AdminSkill[]>('/admin/skills').then(r => r.data),
};

export default adminService;
