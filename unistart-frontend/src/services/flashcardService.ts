import api from './api';
import {
  FlashcardSet,
  Flashcard,
  CreateFlashcardSetDto,
  CreateFlashcardDto,
  UpdateFlashcardDto,
  ReviewFlashcardDto,
  ReviewResult,
} from '../types';

export const flashcardService = {
  // FlashcardSets
  async getSets(): Promise<FlashcardSet[]> {
    const { data } = await api.get<FlashcardSet[]>('/flashcards/sets');
    return data;
  },

  async getSet(id: number): Promise<FlashcardSet & { flashcards: Flashcard[] }> {
    const { data } = await api.get(`/flashcards/sets/${id}`);
    return data;
  },

  async createSet(dto: CreateFlashcardSetDto): Promise<FlashcardSet> {
    const { data } = await api.post<FlashcardSet>('/flashcards/sets', dto);
    return data;
  },

  async updateSet(id: number, dto: CreateFlashcardSetDto): Promise<void> {
    await api.put(`/flashcards/sets/${id}`, dto);
  },

  async deleteSet(id: number): Promise<void> {
    await api.delete(`/flashcards/sets/${id}`);
  },

  // Flashcards
  async getCardsForReview(setId: number): Promise<Flashcard[]> {
    const { data } = await api.get<Flashcard[]>(`/flashcards/sets/${setId}/review`);
    return data;
  },

  async getCard(id: number): Promise<Flashcard> {
    const { data } = await api.get<Flashcard>(`/flashcards/cards/${id}`);
    return data;
  },

  async createCard(dto: CreateFlashcardDto): Promise<Flashcard> {
    const { data } = await api.post<Flashcard>('/flashcards/cards', dto);
    return data;
  },

  async updateCard(id: number, dto: UpdateFlashcardDto): Promise<void> {
    await api.put(`/flashcards/cards/${id}`, dto);
  },

  async deleteCard(id: number): Promise<void> {
    await api.delete(`/flashcards/cards/${id}`);
  },

  async reviewCard(dto: ReviewFlashcardDto): Promise<ReviewResult> {
    const { data } = await api.post<ReviewResult>('/flashcards/cards/review', dto);
    return data;
  },
};
