import api from './api';

export interface LegalDocument {
  slug: string;
  title: string;
  lastUpdatedLabel: string;
  content: string;
  updatedAt: string | null;
  titleKz?: string | null;
  titleEn?: string | null;
  contentKz?: string | null;
  contentEn?: string | null;
  lastUpdatedLabelKz?: string | null;
  lastUpdatedLabelEn?: string | null;
}

export interface UpdateLegalDocument {
  title: string;
  lastUpdatedLabel: string;
  content: string;
  titleKz?: string | null;
  titleEn?: string | null;
  contentKz?: string | null;
  contentEn?: string | null;
  lastUpdatedLabelKz?: string | null;
  lastUpdatedLabelEn?: string | null;
}

const legalService = {
  getAll: () => api.get<LegalDocument[]>('/legal').then((r) => r.data),

  getBySlug: (slug: string) =>
    api.get<LegalDocument>(`/legal/${slug}`).then((r) => r.data),

  update: (slug: string, data: UpdateLegalDocument) =>
    api.put<LegalDocument>(`/legal/${slug}`, data).then((r) => r.data),
};

export default legalService;
