import api from './api';
import type { ExamType, ExamSection } from '../types';

export const examService = {
  async getExams(): Promise<ExamType[]> {
    const response = await api.get<ExamType[]>('/exams');
    return response.data;
  },

  async getExamSections(examCode: string): Promise<ExamSection[]> {
    const response = await api.get<ExamSection[]>(`/exams/${examCode}/sections`);
    return response.data;
  },
};
