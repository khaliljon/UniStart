import api from './api';

export interface SpecialtyTrack {
  id: number;
  name: string;
  nameKz?: string | null;
  nameEn?: string | null;
  subjects: string[];
  conditionalChinese: boolean;
  sortOrder: number;
  isActive: boolean;
}

export type SaveSpecialtyTrack = Omit<SpecialtyTrack, 'id'>;

export const specialtyTrackService = {
  list: (): Promise<SpecialtyTrack[]> =>
    api.get<SpecialtyTrack[]>('/specialty-tracks').then((r) => r.data),

  adminList: (): Promise<SpecialtyTrack[]> =>
    api.get<SpecialtyTrack[]>('/specialty-tracks/admin/all').then((r) => r.data),

  create: (dto: SaveSpecialtyTrack): Promise<SpecialtyTrack> =>
    api.post<SpecialtyTrack>('/specialty-tracks/admin', dto).then((r) => r.data),

  update: (id: number, dto: SaveSpecialtyTrack): Promise<SpecialtyTrack> =>
    api.put<SpecialtyTrack>(`/specialty-tracks/admin/${id}`, dto).then((r) => r.data),

  remove: (id: number): Promise<void> =>
    api.delete(`/specialty-tracks/admin/${id}`).then(() => undefined),
};
