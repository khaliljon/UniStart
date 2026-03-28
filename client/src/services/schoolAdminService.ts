import api from './api';

export interface SchoolDashboard {
  schoolId: number;
  schoolName: string;
  totalStudents: number;
  totalTutors: number;
  activeStudentsLast7Days: number;
  averageAccuracy: number;
  recentStudents: SchoolStudent[];
}

export interface SchoolStudent {
  userId: number;
  name: string;
  email: string;
  subscriptionTier: string;
  createdAt: string;
  lastSeenAt: string | null;
  linkedTutorId: number | null;
  linkedTutorName: string | null;
}

export interface SchoolTutor {
  userId: number;
  name: string;
  email: string;
  headline: string;
  specializations: string[];
  isVerified: boolean;
  isAvailable: boolean;
  totalStudents: number;
  averageRating: number;
  createdAt: string;
}

export interface StudentAnalytics {
  student: { id: number; name: string; email: string; createdAt: string; lastSeenAt: string | null };
  recentSessions: Array<{
    id: number; examTypeId: number; totalQuestions: number; correctAnswers: number;
    accuracy: number; completedAt: string;
  }>;
  skills: Array<{ name: string; proficiencyLevel: number; questionsAnswered: number }>;
}

export interface TutorContentAssignment {
  id: number;
  title: string;
  description: string | null;
  deadline: string | null;
  isActive: boolean;
  createdAt: string;
  tutorName: string;
  tutorUserId: number;
  questionCount: number;
  studentCount: number;
  completedCount: number;
}

export interface TutorContentQuestion {
  id: number;
  text: string;
  difficulty: string;
  isPrivate: boolean;
  createdAt: string;
  tutorName: string;
  tutorUserId: number;
  topicName: string;
  examTypeCode: string;
}

export interface TutorContentResult {
  assignments: { items: TutorContentAssignment[]; total: number };
  questions: { items: TutorContentQuestion[]; total: number };
  page: number;
  pageSize: number;
}

export const schoolAdminService = {
  async getDashboard(): Promise<SchoolDashboard> {
    const { data } = await api.get<SchoolDashboard>('/school-admin/dashboard');
    return data;
  },

  async getStudents(page = 1, pageSize = 20): Promise<{ items: SchoolStudent[]; total: number }> {
    const { data } = await api.get('/school-admin/students', { params: { page, pageSize } });
    return data;
  },

  async getTutors(): Promise<SchoolTutor[]> {
    const { data } = await api.get<SchoolTutor[]>('/school-admin/tutors');
    return data;
  },

  async getStudentAnalytics(userId: number): Promise<StudentAnalytics> {
    const { data } = await api.get<StudentAnalytics>(`/school-admin/students/${userId}/analytics`);
    return data;
  },

  async getTutorContent(params: { tutorId?: number; page?: number; pageSize?: number } = {}): Promise<TutorContentResult> {
    const { data } = await api.get<TutorContentResult>('/school-admin/tutor-content', { params });
    return data;
  },

  async generateInviteCode(): Promise<{ inviteCode: string }> {
    const { data } = await api.post<{ inviteCode: string }>('/school-admin/invite-code');
    return data;
  },

  async getInviteCode(): Promise<{ inviteCode: string | null; requireApproval: boolean }> {
    const { data } = await api.get<{ inviteCode: string | null; requireApproval: boolean }>('/school-admin/invite-code');
    return data;
  },

  async toggleApproval(requireApproval: boolean): Promise<void> {
    await api.put('/school-admin/invite-code/approval', { requireApproval });
  },

  async getSubscription(): Promise<{ schoolName: string; subscriptionExpiresAt: string | null; subscriptionPaidAt: string | null; isActive: boolean }> {
    const { data } = await api.get('/school-admin/subscription');
    return data;
  },
};
