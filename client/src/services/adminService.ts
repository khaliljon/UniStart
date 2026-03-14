import api from './api';
import type { QuestionListItem, QuestionDetail, QuestionStats, BulkImportResult, AdminUser, AdminUserStats, AdminTopicSummary, AdminDashboard, AdminSection, AdminSkill, PagedResult, TrashSummary } from '../types';

const adminService = {
  // ─── Questions ───────────────────────────────────────────
  getQuestions: (examTypeCode?: string, topic?: string, difficulty?: string, page: number = 1, pageSize: number = 50, section?: string) => {
    const params = new URLSearchParams();
    if (examTypeCode) params.set('examTypeCode', examTypeCode);
    if (topic) params.set('topic', topic);
    if (difficulty) params.set('difficulty', difficulty);
    if (section) params.set('section', section);
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
  getUsers: (role?: string, search?: string, page: number = 1, pageSize: number = 50, includeDeleted: boolean = false) => {
    const params = new URLSearchParams();
    if (role) params.set('role', role);
    if (search) params.set('search', search);
    params.set('page', String(page));
    params.set('pageSize', String(pageSize));
    if (includeDeleted) params.set('includeDeleted', 'true');
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

  // ─── Content Management ──────────────────────────────

  // Lessons
  getLessons: (topicId?: number) =>
    api.get<Array<{ id: number; topicId: number; topicName: string; title: string; videoUrl: string | null; sortOrder: number; stepCount: number }>>('/admin/content/lessons', { params: topicId ? { topicId } : {} }).then(r => r.data),
  createLesson: (data: { topicId: number; title: string; content: string; videoUrl?: string; sortOrder?: number }) =>
    api.post('/admin/content/lessons', data).then(r => r.data),
  updateLesson: (id: number, data: { title?: string; content?: string; videoUrl?: string; sortOrder?: number }) =>
    api.put(`/admin/content/lessons/${id}`, data).then(r => r.data),
  deleteLesson: (id: number) =>
    api.delete(`/admin/content/lessons/${id}`).then(r => r.data),

  // Flashcard Decks
  getDecks: (examTypeCode?: string) =>
    api.get<Array<{ id: number; title: string; description: string | null; examTypeCode: string | null; topicId: number | null; topicName: string | null; isSystem: boolean; cardCount: number; createdAt: string }>>('/admin/content/decks', { params: examTypeCode ? { examTypeCode } : {} }).then(r => r.data),
  createDeck: (data: { title: string; description?: string; examTypeCode?: string; topicId?: number }) =>
    api.post('/admin/content/decks', data).then(r => r.data),
  updateDeck: (id: number, data: { title?: string; description?: string; examTypeCode?: string; topicId?: number }) =>
    api.put(`/admin/content/decks/${id}`, data).then(r => r.data),
  deleteDeck: (id: number) =>
    api.delete(`/admin/content/decks/${id}`).then(r => r.data),

  // Flashcards
  getCards: (deckId: number) =>
    api.get<Array<{ id: number; deckId: number; front: string; back: string; sortOrder: number }>>(`/admin/content/decks/${deckId}/cards`).then(r => r.data),
  createCard: (data: { deckId: number; front: string; back: string; sortOrder?: number }) =>
    api.post('/admin/content/cards', data).then(r => r.data),
  updateCard: (id: number, data: { front?: string; back?: string; sortOrder?: number }) =>
    api.put(`/admin/content/cards/${id}`, data).then(r => r.data),
  deleteCard: (id: number) =>
    api.delete(`/admin/content/cards/${id}`).then(r => r.data),

  // Formulas
  getFormulas: (topicId?: number) =>
    api.get<Array<{ id: number; topicId: number; topicName: string; title: string; formula: string; description: string | null; sortOrder: number }>>('/admin/content/formulas', { params: topicId ? { topicId } : {} }).then(r => r.data),
  createFormula: (data: { topicId: number; title: string; formula: string; description?: string; sortOrder?: number }) =>
    api.post('/admin/content/formulas', data).then(r => r.data),
  updateFormula: (id: number, data: { title?: string; formula?: string; description?: string; sortOrder?: number }) =>
    api.put(`/admin/content/formulas/${id}`, data).then(r => r.data),
  deleteFormula: (id: number) =>
    api.delete(`/admin/content/formulas/${id}`).then(r => r.data),

  // Strategies
  getStrategies: (examTypeCode?: string) =>
    api.get<Array<{ id: number; examTypeCode: string; title: string; summary: string; category: string; estimatedReadMinutes: number; sortOrder: number }>>('/admin/content/strategies', { params: examTypeCode ? { examTypeCode } : {} }).then(r => r.data),
  getStrategy: (id: number) =>
    api.get<{ id: number; examTypeCode: string; title: string; summary: string; content: string; category: string; estimatedReadMinutes: number; sortOrder: number }>(`/admin/content/strategies/${id}`).then(r => r.data),
  createStrategy: (data: { examTypeCode: string; title: string; summary: string; content: string; category: string; estimatedReadMinutes?: number; sortOrder?: number }) =>
    api.post('/admin/content/strategies', data).then(r => r.data),
  updateStrategy: (id: number, data: { title?: string; summary?: string; content?: string; category?: string; estimatedReadMinutes?: number; sortOrder?: number }) =>
    api.put(`/admin/content/strategies/${id}`, data).then(r => r.data),
  deleteStrategy: (id: number) =>
    api.delete(`/admin/content/strategies/${id}`).then(r => r.data),

  // Drill Templates
  getDrills: () =>
    api.get<Array<{ id: number; title: string; description: string | null; drillType: string; examTypeCode: string | null; topicId: number | null; topicName: string | null; questionCount: number; timeLimitMinutes: number | null; isActive: boolean; sortOrder: number }>>('/admin/content/drills').then(r => r.data),
  createDrill: (data: { title: string; description?: string; drillType: string; examTypeCode?: string; topicId?: number; questionCount?: number; timeLimitMinutes?: number; isActive?: boolean; sortOrder?: number }) =>
    api.post('/admin/content/drills', data).then(r => r.data),
  updateDrill: (id: number, data: { title?: string; description?: string; drillType?: string; examTypeCode?: string; topicId?: number; questionCount?: number; timeLimitMinutes?: number; isActive?: boolean; sortOrder?: number }) =>
    api.put(`/admin/content/drills/${id}`, data).then(r => r.data),
  deleteDrill: (id: number) =>
    api.delete(`/admin/content/drills/${id}`).then(r => r.data),

  // ─── Trash / Recycle Bin ─────────────────────────────────
  getTrash: () =>
    api.get<TrashSummary>('/admin/trash').then(r => r.data),

  hardDeleteQuestion: (id: number) =>
    api.delete(`/admin/trash/questions/${id}`),

  hardDeleteUser: (id: number) =>
    api.delete(`/admin/trash/users/${id}`),

  emptyTrash: () =>
    api.delete<{ message: string; count: number }>('/admin/trash').then(r => r.data),
};

export default adminService;
