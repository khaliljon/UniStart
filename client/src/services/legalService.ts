import api from './api';

export interface LegalDocument {
  slug: string;
  title: string;
  lastUpdatedLabel: string;
  content: string;
  updatedAt: string | null;
}

const legalService = {
  getAll: () => api.get<LegalDocument[]>('/legal').then((r) => r.data),

  getBySlug: (slug: string) =>
    api.get<LegalDocument>(`/legal/${slug}`).then((r) => r.data),

  update: (slug: string, data: { title: string; lastUpdatedLabel: string; content: string }) =>
    api.put<LegalDocument>(`/legal/${slug}`, data).then((r) => r.data),
};

export default legalService;
