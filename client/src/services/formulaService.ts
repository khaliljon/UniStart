import api from './api';
import type { FormulaCard } from '../types';

export const formulaService = {
  async getFormulas(examTypeCode: string): Promise<FormulaCard[]> {
    const response = await api.get<FormulaCard[]>('/formulas', { params: { examTypeCode } });
    return response.data;
  },

  async getBookmarks(): Promise<FormulaCard[]> {
    const response = await api.get<FormulaCard[]>('/formulas/bookmarks');
    return response.data;
  },

  async toggleBookmark(formulaId: number): Promise<{ formulaId: number; isBookmarked: boolean }> {
    const response = await api.post<{ formulaId: number; isBookmarked: boolean }>(`/formulas/${formulaId}/bookmark`);
    return response.data;
  },
};
