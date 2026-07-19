import api from './api';

export interface ExamSitting {
  id: number;
  date: string;        // "YYYY-MM-DD"
  isActive: boolean;
  sortOrder: number;
}

export interface SaveExamSitting {
  date: string;        // "YYYY-MM-DD"
  isActive: boolean;
  sortOrder: number;
}

export const examSittingsService = {
  /** Public: active sittings for landing / about pages. */
  list: (): Promise<ExamSitting[]> =>
    api.get<ExamSitting[]>('/exam-sittings').then((r) => r.data),

  // ── Admin ──────────────────────────────────────────────
  adminList: (): Promise<ExamSitting[]> =>
    api.get<ExamSitting[]>('/exam-sittings/admin/all').then((r) => r.data),

  create: (dto: SaveExamSitting): Promise<ExamSitting> =>
    api.post<ExamSitting>('/exam-sittings/admin', dto).then((r) => r.data),

  update: (id: number, dto: SaveExamSitting): Promise<ExamSitting> =>
    api.put<ExamSitting>(`/exam-sittings/admin/${id}`, dto).then((r) => r.data),

  remove: (id: number): Promise<void> =>
    api.delete(`/exam-sittings/admin/${id}`).then(() => undefined),
};
