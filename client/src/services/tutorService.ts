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
} from '../types';

export interface TutorListParams {
  search?: string;
  examType?: string;
  sortBy?: string;
  onlyAvailable?: boolean;
  page?: number;
  pageSize?: number;
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

  async getMyTutor(): Promise<LinkedTutorInfo> {
    const response = await api.get<LinkedTutorInfo>('/tutors/my-tutor');
    return response.data;
  },
};
