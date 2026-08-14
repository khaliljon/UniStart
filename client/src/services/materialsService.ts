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
  list: (): Promise<StudyMaterial[]> =>
    api.get<StudyMaterial[]>('/materials').then((r) => r.data),

  download: (id: number): Promise<{ pdfUrl: string }> =>
    api.get<{ pdfUrl: string }>(`/materials/${id}/download`).then((r) => r.data),


  adminList: (): Promise<StudyMaterialAdmin[]> =>
    api.get<StudyMaterialAdmin[]>('/materials/admin/all').then((r) => r.data),

  create: (dto: SaveStudyMaterialDto): Promise<StudyMaterialAdmin> =>
    api.post<StudyMaterialAdmin>('/materials/admin', dto).then((r) => r.data),

  update: (id: number, dto: SaveStudyMaterialDto): Promise<StudyMaterialAdmin> =>
    api.put<StudyMaterialAdmin>(`/materials/admin/${id}`, dto).then((r) => r.data),

  remove: (id: number): Promise<void> =>
    api.delete(`/materials/admin/${id}`).then(() => undefined),

  uploadPdf: async (file: File, onProgress?: (percent: number) => void): Promise<string> => {
    const { data } = await api.post<{ uploadUrl: string; publicUrl: string }>(
      '/materials/admin/presign-pdf'
    );

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

    return data.publicUrl;
  },
};
