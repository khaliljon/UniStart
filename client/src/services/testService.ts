import api from './api';
import type {
  NextQuestionResponse,
  SubmitAnswerRequest,
  AnswerResult,
  StartTestRequest,
  UserSkillProfile,
  Question,
  TopicProgress,
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

  async getWeakQuestions(examTypeCodes?: string[], count?: number): Promise<Question[]> {
    const params: Record<string, unknown> = {};
    if (examTypeCodes?.length) params.examTypeCodes = examTypeCodes;
    if (count && count > 0) params.count = count;
    const response = await api.get<Question[]>('/test/weak-questions', { params });
    return response.data;
  },

  async getTopicsWithProgress(examTypeCodes?: string[], sectionIds?: number[]): Promise<TopicProgress[]> {
    const params: Record<string, unknown> = {};
    if (examTypeCodes?.length) params.examTypeCodes = examTypeCodes;
    if (sectionIds?.length) params.sectionIds = sectionIds;
    const response = await api.get<TopicProgress[]>('/test/topics', { params });
    return response.data;
  },

  async getQuestionsByTopic(topicId: number): Promise<Question[]> {
    const response = await api.get<Question[]>(`/test/topics/${topicId}/questions`);
    return response.data;
  },
};
