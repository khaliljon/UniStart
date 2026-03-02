import api from './api';
import type { TopicWithLessons, TopicLesson, LessonWithSteps } from '../types';

export const lessonService = {
  async getTopicLessons(examTypeCodes?: string[]): Promise<TopicWithLessons[]> {
    const params = examTypeCodes?.length ? { examTypeCodes } : {};
    const response = await api.get<TopicWithLessons[]>('/lessons', { params });
    return response.data;
  },

  async getLessonsByTopic(topicId: number): Promise<TopicLesson[]> {
    const response = await api.get<TopicLesson[]>(`/lessons/topic/${topicId}`);
    return response.data;
  },

  async getLesson(lessonId: number): Promise<TopicLesson> {
    const response = await api.get<TopicLesson>(`/lessons/${lessonId}`);
    return response.data;
  },

  async getQuestionHint(questionId: number): Promise<string | null> {
    try {
      const response = await api.get<{ hint: string }>(`/lessons/hint/${questionId}`);
      return response.data.hint;
    } catch {
      return null;
    }
  },

  // Step-based lessons (TH-1)
  async getLessonWithSteps(lessonId: number): Promise<LessonWithSteps> {
    const response = await api.get<LessonWithSteps>(`/lessons/${lessonId}/steps`);
    return response.data;
  },

  async markStepCompleted(stepId: number): Promise<void> {
    await api.post(`/lessons/steps/${stepId}/complete`);
  },

  async getLessonProgress(lessonId: number): Promise<number> {
    const response = await api.get<{ lessonId: number; progressPercent: number }>(`/lessons/${lessonId}/progress`);
    return response.data.progressPercent;
  },
};
