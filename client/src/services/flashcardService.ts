import api from './api';
import type {
  FlashcardDeck, FlashcardReview, FlashcardDto,
  CreateDeckRequest, CreateFlashcardRequest, ReviewFlashcardRequest,
} from '../types';

export const flashcardService = {
  async getDecks(examTypeCode?: string): Promise<FlashcardDeck[]> {
    const params = examTypeCode ? { examTypeCode } : {};
    const response = await api.get<FlashcardDeck[]>('/flashcards/decks', { params });
    return response.data;
  },

  async getDueCards(deckId: number, limit = 20): Promise<FlashcardReview[]> {
    const response = await api.get<FlashcardReview[]>(`/flashcards/decks/${deckId}/due`, { params: { limit } });
    return response.data;
  },

  async reviewCard(request: ReviewFlashcardRequest): Promise<void> {
    await api.post('/flashcards/review', request);
  },

  async createDeck(request: CreateDeckRequest): Promise<FlashcardDeck> {
    const response = await api.post<FlashcardDeck>('/flashcards/decks', request);
    return response.data;
  },

  async addCard(request: CreateFlashcardRequest): Promise<FlashcardDto> {
    const response = await api.post<FlashcardDto>('/flashcards/cards', request);
    return response.data;
  },

  async deleteDeck(deckId: number): Promise<void> {
    await api.delete(`/flashcards/decks/${deckId}`);
  },

  async getDueCount(): Promise<number> {
    const response = await api.get<{ dueCount: number }>('/flashcards/due-count');
    return response.data.dueCount;
  },
};
