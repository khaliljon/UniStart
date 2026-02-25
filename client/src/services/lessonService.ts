import api from './api';
import type { TopicWithLessons, TopicLesson } from '../types';

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
};
