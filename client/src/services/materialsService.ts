import api from './api';

export interface StudyMaterial {
  id: number;
  subjectKey: string;
  title: string;
  description?: string | null;
  price: number;
}

export interface StudyMaterialAdmin extends StudyMaterial {
  pdfUrl?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface SaveStudyMaterialDto {
  subjectKey: string;
  title: string;
  description?: string | null;
  pdfUrl?: string | null;
  price: number;
  isActive: boolean;
}

export const materialsService = {
  /** Public list of active materials (no PDF URLs). */
  list: (): Promise<StudyMaterial[]> =>
    api.get<StudyMaterial[]>('/materials').then((r) => r.data),

  /** Download link for a purchased material (authenticated, purchase-gated by material id). */
  download: (id: number): Promise<{ pdfUrl: string }> =>
    api.get<{ pdfUrl: string }>(`/materials/${id}/download`).then((r) => r.data),

  // ── Admin ──────────────────────────────────────────────

  adminList: (): Promise<StudyMaterialAdmin[]> =>
    api.get<StudyMaterialAdmin[]>('/materials/admin/all').then((r) => r.data),

  create: (dto: SaveStudyMaterialDto): Promise<StudyMaterialAdmin> =>
    api.post<StudyMaterialAdmin>('/materials/admin', dto).then((r) => r.data),

  update: (id: number, dto: SaveStudyMaterialDto): Promise<StudyMaterialAdmin> =>
    api.put<StudyMaterialAdmin>(`/materials/admin/${id}`, dto).then((r) => r.data),

  remove: (id: number): Promise<void> =>
    api.delete(`/materials/admin/${id}`).then(() => undefined),

  /** Upload a PDF to R2 and returns the public URL. */
  uploadPdf: async (file: File): Promise<string> => {
    const form = new FormData();
    form.append('file', file);
    const res = await api.post<{ url: string }>('/materials/admin/upload-pdf', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return res.data.url;
  },
};
