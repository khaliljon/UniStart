// Синхронизировано с Backend DTOs!

// ============ AUTH ============
export interface User {
  id: string;
  email: string;
  emailConfirmed?: boolean;
  userName?: string;
  firstName: string;
  lastName: string;
  
  // Статистика по карточкам (ОБНОВЛЕНО)
  totalCardsStudied?: number; // Всего изучено карточек
  completedFlashcardSets?: number; // Полностью завершенных наборов
  reviewedCards?: number; // Карточек просмотрено хотя бы раз
  masteredCards?: number; // Карточек полностью освоено
  
  // Статистика по квизам
  totalQuizzesTaken?: number; // Уникальные квизы
  totalQuizAttempts?: number; // Все попытки
  averageScore?: number; // Средний балл
  
  // Статистика по экзаменам
  totalExamsTaken?: number;
  averageExamScore?: number;
  
  // Метаданные
  lastActivityDate?: string; // Последняя активность (квизы/экзамены/карточки)
  createdAt: string;
  lastLoginAt?: string;
  
  roles?: string[]; // Роли пользователя: Admin, Teacher, Student
  lockoutEnd?: string | null;
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
  subject?: string;
  isPublic?: boolean;
  isPublished?: boolean;
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
  maxScore: number;
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
  maxScore: number;
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

// ============ STUDENT STATISTICS ============
export interface StudentStats {
  userId: string;
  email: string;
  userName: string;
  firstName?: string;
  lastName?: string;
  
  // Статистика по карточкам
  completedFlashcardSets?: number;
  reviewedCards?: number;
  masteredCards?: number;
  
  // Статистика по квизам
  totalAttempts: number;
  quizzesTaken: number;
  averageScore: number;
  bestScore?: number;
  
  // Статистика по экзаменам
  examsTaken?: number;
  
  // Активность
  lastAttemptDate?: string;
  lastActivityDate?: string;
}

export interface FlashcardSetDetail {
  setId: number;
  setTitle: string;
  setSubject: string;
  totalCards: number;
  studiedCards: number;
  progressPercentage: number;
  isCompleted: boolean;
  completedAt?: string;
  firstAccessedAt: string;
  lastAccessedAt?: string;
  accessCount: number;
}

// Детализированный прогресс по набору карточек (для админа/учителя)
export interface FlashcardSetProgressDetail {
  setId: number;
  setTitle: string;
  totalCards: number;
  reviewedCards: number; // Просмотрено хотя бы раз
  masteredCards: number; // Полностью освоено
  isCompleted: boolean;
  lastAccessed: string;
}

export interface FlashcardProgress {
  setsAccessed: number;
  setsCompleted: number;
  totalCardsReviewed: number;
  masteredCards: number; // ИСПРАВЛЕНО: было totalCardsMastered
  setDetails: FlashcardSetProgressDetail[]; // ИСПРАВЛЕНО: было FlashcardSetDetail
}

export interface StudentDetailedStats extends StudentStats {
  // Детальная статистика по квизам
  totalQuizAttempts: number;
  averageQuizScore: number;
  bestQuizScore: number;
  attempts?: any[];
  
  // Детальная статистика по экзаменам
  totalExamAttempts: number;
  averageExamScore: number;
  bestExamScore: number;
  examAttempts?: any[];
  
  // Детальная статистика по карточкам (ДОПОЛНЕНО)
  completedFlashcardSets: number;
  reviewedCards: number;
  masteredCards: number;
  flashcardProgress?: FlashcardProgress;
  
  // Общая статистика
  averageScore: number;
}

export interface SubjectProgress {
  subject: string;
  quizzesTaken: number;
  averageScore: number;
  cardsStudied: number;
  masteredCards: number;
}

// ============ UTILITY TYPES ============
export interface ApiError {
  message: string;
  errors?: string[];
}

export type DifficultyLevel = 'Easy' | 'Medium' | 'Hard';
export type QuestionType = 'SingleChoice' | 'MultipleChoice' | 'TrueFalse';
