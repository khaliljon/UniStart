import api from './api';
import type {
  TutorListResult,
  TutorProfileDetail,
  UpdateTutorProfile,
  ScheduleSlotInput,
  CreateReviewRequest,
  StudentInfo,
  PendingRequest,
  AcceptDeclineResult,
  TutorSchoolCard,
  TutorSchoolDetail,
  InviteCodeInfo,
  LinkResult,
  TutorStudentInfo,
  LinkedTutorInfo,
  TutorQuestionsPage,
  TutorQuestionDetail,
  AdminTopicSummary,
  AssignmentListItem,
  AssignmentDetail,
  StudentAssignmentListItem,
  StudentAssignmentDetail,
  SubmitAssignmentAnswerResult,
  SchoolAdmin,
  CreateSchoolRequest,
  UpdateSchoolRequest,
  SchoolBranding,
} from '../types';

export interface TutorListParams {
  search?: string;
  exam?: string;
  sort?: string;
  available?: boolean;
  page?: number;
  pageSize?: number;
  schoolId?: number;
}

export const tutorService = {
  async getTutors(params: TutorListParams = {}): Promise<TutorListResult> {
    const response = await api.get<TutorListResult>('/tutors', { params });
    return response.data;
  },

  async getTutorProfile(userId: number): Promise<TutorProfileDetail> {
    const response = await api.get<TutorProfileDetail>(`/tutors/${userId}`);
    return response.data;
  },

  async updateProfile(data: UpdateTutorProfile): Promise<void> {
    await api.put('/tutors/profile', data);
  },

  async setSchedule(slots: ScheduleSlotInput[]): Promise<void> {
    await api.put('/tutors/schedule', { slots });
  },

  async leaveReview(userId: number, data: CreateReviewRequest): Promise<void> {
    await api.post(`/tutors/${userId}/reviews`, data);
  },

  async getMyStudents(): Promise<StudentInfo[]> {
    const response = await api.get<StudentInfo[]>('/tutors/my-students');
    return response.data;
  },

  async getPendingRequests(): Promise<PendingRequest[]> {
    const response = await api.get<PendingRequest[]>('/tutors/requests/pending');
    return response.data;
  },

  async acceptStudent(conversationId: number): Promise<AcceptDeclineResult> {
    const response = await api.post<AcceptDeclineResult>(`/tutors/requests/${conversationId}/accept`);
    return response.data;
  },

  async declineStudent(conversationId: number, reason?: string): Promise<AcceptDeclineResult> {
    const response = await api.post<AcceptDeclineResult>(`/tutors/requests/${conversationId}/decline`, { reason });
    return response.data;
  },

  async getSchools(): Promise<TutorSchoolCard[]> {
    const response = await api.get<TutorSchoolCard[]>('/tutors/schools');
    return response.data;
  },

  async getSchool(slug: string): Promise<TutorSchoolDetail> {
    const response = await api.get<TutorSchoolDetail>(`/tutors/schools/${encodeURIComponent(slug)}`);
    return response.data;
  },

  // ── School Management (Этап 4) ──

  async createSchool(data: CreateSchoolRequest): Promise<SchoolAdmin> {
    const response = await api.post<SchoolAdmin>('/tutors/schools', data);
    return response.data;
  },

  async getMySchool(): Promise<SchoolAdmin | null> {
    const response = await api.get<SchoolAdmin | { school: null }>('/tutors/my-school');
    if ('school' in response.data && response.data.school === null) return null;
    return response.data as SchoolAdmin;
  },

  async updateMySchool(data: UpdateSchoolRequest): Promise<SchoolAdmin> {
    const response = await api.put<SchoolAdmin>('/tutors/my-school', data);
    return response.data;
  },

  async addTutorToSchool(tutorUserId: number): Promise<LinkResult> {
    const response = await api.post<LinkResult>(`/tutors/my-school/tutors/${tutorUserId}`);
    return response.data;
  },

  async removeTutorFromSchool(tutorUserId: number): Promise<LinkResult> {
    const response = await api.delete<LinkResult>(`/tutors/my-school/tutors/${tutorUserId}`);
    return response.data;
  },

  // ── Invite code & binding ──

  async generateInviteCode(): Promise<InviteCodeInfo> {
    const response = await api.post<InviteCodeInfo>('/tutors/invite-code');
    return response.data;
  },

  async getInviteCode(): Promise<InviteCodeInfo> {
    const response = await api.get<InviteCodeInfo>('/tutors/invite-code');
    return response.data;
  },

  async linkByInviteCode(inviteCode: string): Promise<LinkResult> {
    const response = await api.post<LinkResult>('/tutors/link', { inviteCode });
    return response.data;
  },

  async getLinkedStudents(): Promise<TutorStudentInfo[]> {
    const response = await api.get<TutorStudentInfo[]>('/tutors/linked-students');
    return response.data;
  },

  async unlinkStudent(studentUserId: number): Promise<void> {
    await api.delete(`/tutors/students/${studentUserId}/unlink`);
  },

  async unlinkFromTutor(): Promise<void> {
    await api.delete('/tutors/unlink');
  },

  async getMyTutor(): Promise<LinkedTutorInfo | null> {
    const response = await api.get<{ tutor: LinkedTutorInfo | null }>('/tutors/my-tutor');
    return response.data.tutor;
  },

  // ── Tutor question management (Этап 2) ──

  async getMyQuestions(params: { search?: string; examType?: string; page?: number; pageSize?: number } = {}): Promise<TutorQuestionsPage> {
    const response = await api.get<TutorQuestionsPage>('/tutors/questions', { params });
    return response.data;
  },

  async getQuestion(questionId: number): Promise<TutorQuestionDetail> {
    const response = await api.get<TutorQuestionDetail>(`/tutors/questions/${questionId}`);
    return response.data;
  },

  async createQuestion(data: {
    topicId: number;
    text: string;
    difficulty: string;
    explanation?: string;
    answerOptions: { text: string; isCorrect: boolean }[];
  }): Promise<TutorQuestionDetail> {
    const response = await api.post<TutorQuestionDetail>('/tutors/questions', data);
    return response.data;
  },

  async updateQuestion(questionId: number, data: {
    topicId?: number;
    text?: string;
    difficulty?: string;
    explanation?: string;
    answerOptions?: { text: string; isCorrect: boolean }[];
  }): Promise<TutorQuestionDetail> {
    const response = await api.put<TutorQuestionDetail>(`/tutors/questions/${questionId}`, data);
    return response.data;
  },

  async deleteQuestion(questionId: number): Promise<void> {
    await api.delete(`/tutors/questions/${questionId}`);
  },

  async getTopics(): Promise<AdminTopicSummary[]> {
    const response = await api.get<AdminTopicSummary[]>('/tutors/topics');
    return response.data;
  },

  // ─── Assignments (Этап 3) ──────────────────────────────

  async getAssignments(): Promise<AssignmentListItem[]> {
    const response = await api.get<AssignmentListItem[]>('/tutors/assignments');
    return response.data;
  },

  async getAssignment(id: number): Promise<AssignmentDetail> {
    const response = await api.get<AssignmentDetail>(`/tutors/assignments/${id}`);
    return response.data;
  },

  async createAssignment(data: {
    title: string;
    description?: string;
    deadline?: string;
    questionIds: number[];
    studentUserIds: number[];
  }): Promise<AssignmentDetail> {
    const response = await api.post<AssignmentDetail>('/tutors/assignments', data);
    return response.data;
  },

  async updateAssignment(id: number, data: {
    title?: string;
    description?: string | null;
    deadline?: string | null;
    isActive?: boolean;
  }): Promise<AssignmentDetail> {
    const response = await api.put<AssignmentDetail>(`/tutors/assignments/${id}`, data);
    return response.data;
  },

  async deleteAssignment(id: number): Promise<void> {
    await api.delete(`/tutors/assignments/${id}`);
  },

  // Student-facing assignment methods

  async getMyAssignments(): Promise<StudentAssignmentListItem[]> {
    const response = await api.get<StudentAssignmentListItem[]>('/tutors/my-assignments');
    return response.data;
  },

  async getMyAssignment(id: number): Promise<StudentAssignmentDetail> {
    const response = await api.get<StudentAssignmentDetail>(`/tutors/my-assignments/${id}`);
    return response.data;
  },

  async submitAssignmentAnswer(assignmentId: number, data: {
    questionId: number;
    selectedOptionId: number;
  }): Promise<SubmitAssignmentAnswerResult> {
    const response = await api.post<SubmitAssignmentAnswerResult>(`/tutors/my-assignments/${assignmentId}/answer`, data);
    return response.data;
  },

  // ── School Branding (White Label) ──

  async getSchoolBranding(slug: string): Promise<SchoolBranding | null> {
    try {
      const response = await api.get<SchoolBranding>('/tutors/schools/branding', { params: { slug } });
      return response.data;
    } catch {
      return null;
    }
  },
};
