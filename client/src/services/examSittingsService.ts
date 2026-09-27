import api from './api';

export interface ExamSitting {
  id: number;
  date: string;
  endDate?: string | null;
  isActive: boolean;
  sortOrder: number;
}

export interface SaveExamSitting {
  date: string;
  endDate?: string | null;
  isActive: boolean;
  sortOrder: number;
}

export const examSittingsService = {
  list: (): Promise<ExamSitting[]> =>
    api.get<ExamSitting[]>('/exam-sittings').then((r) => r.data),

  adminList: (): Promise<ExamSitting[]> =>
    api.get<ExamSitting[]>('/exam-sittings/admin/all').then((r) => r.data),

  create: (dto: SaveExamSitting): Promise<ExamSitting> =>
    api.post<ExamSitting>('/exam-sittings/admin', dto).then((r) => r.data),

  update: (id: number, dto: SaveExamSitting): Promise<ExamSitting> =>
    api.put<ExamSitting>(`/exam-sittings/admin/${id}`, dto).then((r) => r.data),

  remove: (id: number): Promise<void> =>
    api.delete(`/exam-sittings/admin/${id}`).then(() => undefined),
};
