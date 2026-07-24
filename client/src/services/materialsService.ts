import api from './api';

export interface StudyMaterial {
  id: number;
  subjectKey: string;
  title: string;
  description?: string | null;
  titleKz?: string | null;
  titleEn?: string | null;
  descriptionKz?: string | null;
  descriptionEn?: string | null;
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
  titleKz?: string | null;
  titleEn?: string | null;
  descriptionKz?: string | null;
  descriptionEn?: string | null;
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

  /**
   * Uploads a PDF straight to Cloudflare R2 using a short-lived presigned URL.
   * The file never passes through our API server, so large files don't consume
   * server memory/bandwidth. Returns the final public URL to store on the material.
   */
  uploadPdf: async (file: File, onProgress?: (percent: number) => void): Promise<string> => {
    // 1) Ask our API for a presigned direct-to-R2 upload target.
    const { data } = await api.post<{ uploadUrl: string; publicUrl: string }>(
      '/materials/admin/presign-pdf'
    );

    // 2) PUT the file directly to R2. Use XHR so we can report upload progress.
    await new Promise<void>((resolve, reject) => {
      const xhr = new XMLHttpRequest();
      xhr.open('PUT', data.uploadUrl, true);
      xhr.setRequestHeader('Content-Type', 'application/pdf');
      xhr.upload.onprogress = (e) => {
        if (onProgress && e.lengthComputable) onProgress(Math.round((e.loaded / e.total) * 100));
      };
      xhr.onload = () => {
        if (xhr.status >= 200 && xhr.status < 300) resolve();
        else reject(new Error(`Загрузка в хранилище не удалась (${xhr.status}).`));
      };
      xhr.onerror = () => reject(new Error('Ошибка сети при загрузке файла в хранилище.'));
      xhr.send(file);
    });

    // 3) The object is now in R2 — return its public URL.
    return data.publicUrl;
  },
};
