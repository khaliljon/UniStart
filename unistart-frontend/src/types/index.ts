// Синхронизировано с Backend DTOs!

// ============ AUTH ============
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  totalCardsStudied: number;
  totalQuizzesTaken: number;
  createdAt: string;
  roles?: string[]; // Роли пользователя: Admin, Teacher, Student
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  expiresAt: string;
}

// ============ FLASHCARDS ============
export interface FlashcardSet {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  updatedAt: string;
  totalCards: number;
  cardsToReview: number;
}

export interface Flashcard {
  id: number;
  question: string;
  answer: string;
  explanation: string;
  orderIndex: number;
  nextReviewDate?: string;
  lastReviewedAt?: string;
  isDueForReview: boolean;
}

export interface CreateFlashcardSetDto {
  title: string;
  description: string;
}

export interface CreateFlashcardDto {
  question: string;
  answer: string;
  explanation: string;
  flashcardSetId: number;
}

export interface UpdateFlashcardDto {
  question: string;
  answer: string;
  explanation: string;
}

export interface ReviewFlashcardDto {
  flashcardId: number;
  quality: number; // 0-5 по SM-2 алгоритму
}

export interface ReviewResult {
  flashcardId: number;
  nextReviewDate: string;
  intervalDays: number;
  message: string;
}

// ============ QUIZZES ============
export interface Quiz {
  id: number;
  title: string;
  description: string;
  timeLimit: number;
  subject: string;
  difficulty: string;
  questionCount: number;
  totalPoints: number;
}

export interface QuizDetail {
  id: number;
  title: string;
  description: string;
  timeLimit: number;
  subject: string;
  difficulty: string;
  questions: Question[];
}

export interface Question {
  id: number;
  text: string;
  questionType: string; // "SingleChoice" | "MultipleChoice"
  points: number;
  imageUrl?: string;
  answers: Answer[];
}

export interface Answer {
  id: number;
  text: string;
  isCorrect?: boolean; // null при отображении пользователю
}

export interface SubmitQuizDto {
  quizId: number;
  timeSpentSeconds: number;
  userAnswers: Record<number, number[]>; // questionId -> answerIds[]
}

export interface QuizResult {
  score: number;
  maxScore: number;
  percentage: number;
  timeSpentSeconds: number;
  questionResults: QuestionResult[];
}

export interface QuestionResult {
  questionId: number;
  questionText: string;
  isCorrect: boolean;
  pointsEarned: number;
  maxPoints: number;
  correctAnswerIds: number[];
  userAnswerIds: number[];
  explanation?: string;
}

export interface UserQuizAttempt {
  id: number;
  userId: string;
  quizId: number;
  score: number;
  maxScore: number;
  percentage: number;
  timeSpentSeconds: number;
  completedAt: string;
}

// ============ UTILITY TYPES ============
export interface ApiError {
  message: string;
  errors?: string[];
}

export type DifficultyLevel = 'Easy' | 'Medium' | 'Hard';
export type QuestionType = 'SingleChoice' | 'MultipleChoice' | 'TrueFalse';
