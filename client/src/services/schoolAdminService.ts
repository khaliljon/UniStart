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
};
