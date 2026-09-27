import api from './api';

export type NewsCategory = 'dates' | 'admission' | 'platform' | 'guide';

export interface NewsItem {
  id: number;
  title: string;
  summary: string;
  body: string;
  titleKz?: string | null;
  titleEn?: string | null;
  summaryKz?: string | null;
  summaryEn?: string | null;
  bodyKz?: string | null;
  bodyEn?: string | null;
  imageUrl?: string | null;
  isPublished: boolean;
  publishedAt: string | null;
  createdAt: string;
  updatedAt: string;
  slug?: string | null;
  category: NewsCategory;
  isFeatured: boolean;
}

export interface NewsUpsert {
  title: string;
  summary: string;
  body: string;
  titleKz?: string | null;
  titleEn?: string | null;
  summaryKz?: string | null;
  summaryEn?: string | null;
  bodyKz?: string | null;
  bodyEn?: string | null;
  imageUrl?: string | null;
  isPublished: boolean;
  category?: NewsCategory;
  isFeatured?: boolean;
}

export const newsService = {
  async listPublished(limit?: number): Promise<NewsItem[]> {
    const res = await api.get<NewsItem[]>('/news', { params: limit ? { limit } : undefined });
    return res.data;
  },

  async getById(id: number): Promise<NewsItem> {
    const res = await api.get<NewsItem>(`/news/${id}`);
    return res.data;
  },

  async getBySlug(slug: string): Promise<NewsItem> {
    const res = await api.get<NewsItem>(`/news/by-slug/${encodeURIComponent(slug)}`);
    return res.data;
  },

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
