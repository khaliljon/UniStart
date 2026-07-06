import api from './api';

export interface NewsItem {
  id: number;
  title: string;
  summary: string;
  body: string;
  imageUrl?: string | null;
  isPublished: boolean;
  publishedAt: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface NewsUpsert {
  title: string;
  summary: string;
  body: string;
  imageUrl?: string | null;
  isPublished: boolean;
}

export const newsService = {
  /** Public: published news, newest first. */
  async listPublished(limit?: number): Promise<NewsItem[]> {
    const res = await api.get<NewsItem[]>('/news', { params: limit ? { limit } : undefined });
    return res.data;
  },

  async getById(id: number): Promise<NewsItem> {
    const res = await api.get<NewsItem>(`/news/${id}`);
    return res.data;
  },

  // ─── Admin ───────────────────────────────────────────────
  async listAll(): Promise<NewsItem[]> {
    const res = await api.get<NewsItem[]>('/news/all');
    return res.data;
  },

  async create(dto: NewsUpsert): Promise<NewsItem> {
    const res = await api.post<NewsItem>('/news', dto);
    return res.data;
  },

  async update(id: number, dto: NewsUpsert): Promise<NewsItem> {
    const res = await api.put<NewsItem>(`/news/${id}`, dto);
    return res.data;
  },

  async remove(id: number): Promise<void> {
    await api.delete(`/news/${id}`);
  },
};
