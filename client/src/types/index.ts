// Auth types
export interface User {
  id: number;
  email: string;
  name: string;
  role: string;
  createdAt: string;
}

export interface AuthResponse {
  userId: number;
  email: string;
  name: string;
  role: string;
  token: string;
  expiresAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  name: string;
  password: string;
}

// Exam types
export interface ExamType {
  code: string;
  name: string;
}

export interface ExamSection {
  id: number;
  examTypeCode: string;
  name: string;
  minScore: number;
  maxScore: number;
}

// Question types
export interface AnswerOption {
  id: number;
  text: string;
}

export interface Question {
  id: number;
  text: string;
  difficulty: 'Easy' | 'Medium' | 'Hard';
  topicId: number;
  topicName: string;
  options: AnswerOption[];
}

export interface NextQuestionResponse {
  question: Question | null;
  testCompleted: boolean;
  questionsAnswered: number;
  totalQuestions: number;
}

export interface SubmitAnswerRequest {
  questionId: number;
  answerOptionId: number;
}

export interface AnswerResult {
  isCorrect: boolean;
  correctOptionId: number;
  newSkillLevel: number;
  skillChange: number;
}

// Skill types
export interface UserSkillProfile {
  skillId: number;
  skillName: string;
  skillCode: string;
  level: number;
  lastUpdated: string;
}

export interface SkillAnalytics {
  totalQuestionsAnswered: number;
  correctAnswers: number;
  overallAccuracy: number;
  skillProfiles: UserSkillProfile[];
  recentProgress: SkillProgress[];
}

export interface SkillProgress {
  skillName: string;
  oldLevel: number;
  newLevel: number;
  date: string;
}

// Test session types
export interface StartTestRequest {
  examTypeCodes: string[];
  sectionId?: number;
}
