import api from './api';
import type { QuestionListItem, QuestionDetail, QuestionStats, BulkImportResult, AdminUser, AdminUserStats, AdminTopicSummary, AdminDashboard, AdminSection, AdminSkill, PagedResult } from '../types';

const adminService = {
  // ─── Questions ───────────────────────────────────────────
  getQuestions: (examTypeCode?: string, topic?: string, difficulty?: string, page: number = 1, pageSize: number = 50) => {
    const params = new URLSearchParams();
    if (examTypeCode) params.set('examTypeCode', examTypeCode);
    if (topic) params.set('topic', topic);
    if (difficulty) params.set('difficulty', difficulty);
    params.set('page', String(page));
    params.set('pageSize', String(pageSize));
    const qs = params.toString();
    return api.get<PagedResult<QuestionListItem>>(`/admin/questions${qs ? '?' + qs : ''}`).then(r => r.data);
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
  getUsers: (role?: string, search?: string, page: number = 1, pageSize: number = 50) => {
    const params = new URLSearchParams();
    if (role) params.set('role', role);
    if (search) params.set('search', search);
    params.set('page', String(page));
    params.set('pageSize', String(pageSize));
    const qs = params.toString();
    return api.get<PagedResult<AdminUser>>(`/admin/users${qs ? '?' + qs : ''}`).then(r => r.data);
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

  // ─── Block / Unblock (OP-14) ─────────────────────────────
  blockUser: (id: number, reason?: string) =>
    api.post<AdminUser>(`/admin/users/${id}/block`, { reason }).then(r => r.data),

  unblockUser: (id: number) =>
    api.post<AdminUser>(`/admin/users/${id}/unblock`).then(r => r.data),

  // ─── Restore (OP-9) ──────────────────────────────────────
  restoreQuestion: (id: number) =>
    api.post(`/admin/questions/${id}/restore`).then(r => r.data),

  restoreUser: (id: number) =>
    api.post(`/admin/users/${id}/restore`).then(r => r.data),
  // ─── CSV Export (OP-18) ─────────────────────────────────────────
  exportQuestionsCsv: (examTypeCode?: string, difficulty?: string) => {
    const params = new URLSearchParams();
    if (examTypeCode) params.set('examTypeCode', examTypeCode);
    if (difficulty) params.set('difficulty', difficulty);
    const qs = params.toString();
    return api.get(`/admin/questions/export${qs ? '?' + qs : ''}`, { responseType: 'blob' }).then(r => {
      const url = URL.createObjectURL(r.data);
      const a = document.createElement('a');
      a.href = url; a.download = 'questions.csv'; a.click();
      URL.revokeObjectURL(url);
    });
  },

  exportUsersCsv: (role?: string) => {
    const params = new URLSearchParams();
    if (role) params.set('role', role);
    const qs = params.toString();
    return api.get(`/admin/users/export${qs ? '?' + qs : ''}`, { responseType: 'blob' }).then(r => {
      const url = URL.createObjectURL(r.data);
      const a = document.createElement('a');
      a.href = url; a.download = 'users.csv'; a.click();
      URL.revokeObjectURL(url);
    });
  },

  // ─── Audit Logs (OP-7 / OP-23) ───────────────────────────
  getAuditLogs: (filters: {
    action?: string;
    entityType?: string;
    userId?: number;
    from?: string;
    to?: string;
    page?: number;
    pageSize?: number;
  } = {}) => {
    const params = new URLSearchParams();
    if (filters.action) params.set('action', filters.action);
    if (filters.entityType) params.set('entityType', filters.entityType);
    if (filters.userId) params.set('userId', String(filters.userId));
    if (filters.from) params.set('from', filters.from);
    if (filters.to) params.set('to', filters.to);
    params.set('page', String(filters.page || 1));
    params.set('pageSize', String(filters.pageSize || 50));
    const qs = params.toString();
    return api.get<{
      items: Array<{
        id: number;
        userId: number;
        userEmail: string;
        action: string;
        entityType: string;
        entityId: string | null;
        oldValues: string | null;
        newValues: string | null;
        ipAddress: string | null;
        timestamp: string;
      }>;
      totalCount: number;
      page: number;
      pageSize: number;
      totalPages: number;
    }>(`/admin/audit-logs?${qs}`).then(r => r.data);
  },

  // ─── System Health (OP-23) ────────────────────
  getSystemHealth: () =>
    api.get<{
      status: string;
      healthChecks: Array<{
        name: string;
        status: string;
        duration: number;
        error: string | null;
      }>;
      database: {
        totalUsers: number;
        totalQuestions: number;
        totalAnswers: number;
        totalSessions: number;
        activeUsersToday: number;
        answersToday: number;
      };
      recurringJobs: Array<{
        id: string;
        cron: string;
        lastExecution: string | null;
        nextExecution: string | null;
        lastJobState: string | null;
      }> | null;
      system: {
        environment: string;
        dotnetVersion: string;
        machineName: string;
        uptime: number;
        memoryMB: number;
        threadCount: number;
        serverTime: string;
      };
    }>('/admin/system/health').then(r => r.data),

  // ─── Jobs (OP-12) ────────────────────
  getJobsStatus: () =>
    api.get<{ jobs: Array<{
      id: string; cron: string; queue: string;
      lastExecution: string | null; nextExecution: string | null;
      lastJobId: string | null; lastJobState: string | null;
      createdAt: string | null;
    }> }>('/admin/jobs/status').then(r => r.data),

  triggerJob: (jobId: string) =>
    api.post<{ message: string }>(`/admin/jobs/${jobId}/trigger`).then(r => r.data),

  // ─── User Activity (OP-23) ────────────────────
  getUserActivity: (userId: number, page = 1, pageSize = 30) =>
    api.get<{
      user: {
        id: number; name: string; email: string; role: string;
        subscriptionTier: string; createdAt: string;
        isBlocked: boolean; blockReason: string | null;
      };
      summary: {
        totalAnswers: number; correctAnswers: number; accuracy: number;
        totalSessions: number; currentStreak: number;
        lastActivity: string | null;
      };
      skills: Array<{
        skillName: string; theta: number; thetaSE: number;
        level: string; lastUpdated: string;
      }>;
      dailyActivity: Array<{ date: string; count: number }>;
      sessions: {
        items: Array<{
          id: number; startedAt: string; completedAt: string | null;
          totalQuestions: number; correctCount: number;
          isCompleted: boolean; examTypeCode: string;
        }>;
        totalCount: number; page: number; pageSize: number; totalPages: number;
      };
    }>(`/admin/users/${userId}/activity?page=${page}&pageSize=${pageSize}`).then(r => r.data),

  // ─── Tutor Moderation (T-10) ──────────────────────────────
  getTutors: () =>
    api.get<Array<{
      tutorProfileId: number;
      userId: number;
      name: string;
      email: string;
      headline: string;
      specializations: string;
      isAvailable: boolean;
      isVerified: boolean;
      isBlocked: boolean;
      blockReason: string | null;
      averageRating: number;
      totalReviews: number;
      totalStudents: number;
      hourlyRate: number | null;
      createdAt: string;
    }>>('/admin/tutors').then(r => r.data),

  verifyTutor: (tutorProfileId: number) =>
    api.post<{ verified: boolean; tutorProfileId: number }>(`/admin/tutors/${tutorProfileId}/verify`).then(r => r.data),

  unverifyTutor: (tutorProfileId: number) =>
    api.post<{ verified: boolean; tutorProfileId: number }>(`/admin/tutors/${tutorProfileId}/unverify`).then(r => r.data),
};

export default adminService;
