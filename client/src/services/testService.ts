import api from './api';
import type {
  NextQuestionResponse,
  SubmitAnswerRequest,
  AnswerResult,
  StartTestRequest,
  UserSkillProfile,
} from '../types';

export const testService = {
  async getNextQuestion(request: StartTestRequest): Promise<NextQuestionResponse> {
    const response = await api.post<NextQuestionResponse>('/test/next-question', request);
    return response.data;
  },

  async submitAnswer(request: SubmitAnswerRequest): Promise<AnswerResult> {
    const response = await api.post<AnswerResult>('/test/submit-answer', request);
    return response.data;
  },

  async getSkillProfiles(): Promise<UserSkillProfile[]> {
    const response = await api.get<UserSkillProfile[]>('/test/skill-profiles');
    return response.data;
  },

  async resetProgress(): Promise<void> {
    await api.post('/test/reset');
  },
};
