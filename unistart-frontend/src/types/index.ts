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
export enum FlashcardType {
  SingleChoice = 0,
  FillInTheBlank = 1,
  Matching = 2,
  Sequencing = 3
}

export interface FlashcardSet {
  id: number;
  title: string;
  description: string;
  subject?: string;
  isPublic?: boolean;
  isPublished?: boolean;
  createdAt: string;
  updatedAt: string;
  totalCards: number;
  cardsToReview: number;
  flashcards?: Flashcard[]; // Массив карточек (когда загружается детально)
}

export interface Flashcard {
  id: number;
  type: FlashcardType;
  question: string;
  answer: string;
  optionsJson?: string;
  matchingPairsJson?: string;
  sequenceJson?: string;
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
  type: FlashcardType;
  question: string;
  answer: string;
  optionsJson?: string;
  matchingPairsJson?: string;
  sequenceJson?: string;
  explanation: string;
  flashcardSetId: number;
}

export interface UpdateFlashcardDto {
  type: FlashcardType;
  question: string;
  answer: string;
  optionsJson?: string;
  matchingPairsJson?: string;
  sequenceJson?: string;
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
  isLearningMode?: boolean;
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
  isLearningMode?: boolean;
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

// ============ EXAMS ============
export interface Exam {
  id: number;
  title: string;
  description: string;
  subject: string;
  difficulty: string;
  timeLimit: number;
  strictTiming?: boolean;
  questionCount: number;
  totalPoints: number;
  maxAttempts: number;
  passingScore: number;
  shuffleQuestions?: boolean;
  shuffleAnswers?: boolean;
  isProctored: boolean;
  isPublished?: boolean;
  startDate?: string;
  endDate?: string;
  createdAt: string;
}

// ============ UTILITY TYPES ============
export interface ApiError {
  message: string;
  errors?: string[];
}

export type DifficultyLevel = 'Easy' | 'Medium' | 'Hard';
export type QuestionType = 'SingleChoice' | 'MultipleChoice' | 'TrueFalse';
