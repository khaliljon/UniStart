import api from './api';
import type {
  TutorListResult,
  TutorProfileDetail,
  UpdateTutorProfile,
  ScheduleSlotInput,
  CreateReviewRequest,
  TutorCard,
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

  async getMyStudents(): Promise<TutorCard[]> {
    const response = await api.get<TutorCard[]>('/tutors/my-students');
    return response.data;
  },
};
