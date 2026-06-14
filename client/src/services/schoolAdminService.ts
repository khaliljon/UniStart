import api from './api';

export interface SchoolDashboard {
  schoolId: number;
  schoolName: string;
  totalStudents: number;
  totalTutors: number;
  activeStudentsLast7Days: number;
  averageAccuracy: number;
  recentStudents: SchoolStudent[];
  isApproved: boolean;
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
  verificationRequestedAt: string | null;
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

export interface SchoolOwnBranding {
  id: number;
  name: string;
  slug: string;
  subdomain: string | null;
  navbarTitle: string | null;
  logoUrl: string | null;
  websiteUrl: string | null;
  instagramUrl: string | null;
  telegramUrl: string | null;
  primaryColor: string | null;
  primaryHoverColor: string | null;
  accentColor: string | null;
}

export type SchoolOwnUpdateBranding = Pick<
  SchoolOwnBranding,
  'navbarTitle' | 'logoUrl' | 'websiteUrl' | 'instagramUrl' | 'telegramUrl' | 'primaryColor' | 'primaryHoverColor' | 'accentColor'
>;

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

  async getSubscription(): Promise<{ schoolName: string; subscriptionExpiresAt: string | null; subscriptionPaidAt: string | null; isActive: boolean }> {
    const { data } = await api.get('/school-admin/subscription');
    return data;
  },

  async getBranding(): Promise<SchoolOwnBranding> {
    const { data } = await api.get<SchoolOwnBranding>('/school-admin/branding');
    return data;
  },

  async updateBranding(payload: Partial<SchoolOwnUpdateBranding>): Promise<{ updated: boolean; schoolId: number }> {
    const { data } = await api.put('/school-admin/branding', payload);
    return data;
  },

  async uploadImage(file: File): Promise<string> {
    const form = new FormData();
    form.append('file', file);
    const { data } = await api.post<{ url: string }>('/school-admin/upload-image', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return data.url;
  },
};

