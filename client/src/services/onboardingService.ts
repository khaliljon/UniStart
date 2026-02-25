import api from './api';
import type {
  OnboardingStatus,
  ExamTypeInfo,
  CompleteOnboardingRequest,
} from '../types';

export const onboardingService = {
  async getStatus(): Promise<OnboardingStatus> {
    const response = await api.get<OnboardingStatus>('/onboarding/status');
    return response.data;
  },

  async getExamTypes(): Promise<ExamTypeInfo[]> {
    const response = await api.get<ExamTypeInfo[]>('/onboarding/exam-types');
    return response.data;
  },

  async complete(dto: CompleteOnboardingRequest): Promise<OnboardingStatus> {
    const response = await api.post<OnboardingStatus>('/onboarding/complete', dto);
    return response.data;
  },
};
