import api from './api';

export interface Subject {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
}

class SubjectService {
  async getSubjects(): Promise<Subject[]> {
    const response = await api.get('/subjects');
    return response.data;
  }

  async getAllSubjects(): Promise<Subject[]> {
    const response = await api.get('/subjects/all');
    return response.data;
  }

  async getSubject(id: number): Promise<Subject> {
    const response = await api.get(`/subjects/${id}`);
    return response.data;
  }

  async createSubject(data: { name: string; description?: string }): Promise<Subject> {
    const response = await api.post('/subjects', data);
    return response.data;
  }

  async updateSubject(id: number, data: { name: string; description?: string; isActive: boolean }): Promise<Subject> {
    const response = await api.put(`/subjects/${id}`, data);
    return response.data;
  }

  async deleteSubject(id: number): Promise<void> {
    await api.delete(`/subjects/${id}`);
  }
}

export const subjectService = new SubjectService();
