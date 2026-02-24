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

// Test Mode
export type TestMode = 'practice' | 'exam';

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

export interface AnswerResult {
  isCorrect: boolean;
  correctOptionId: number;
  correctOptionText: string;
  explanation: string | null;
  newSkillLevel: number;
  skillChange: number;
  // IRT fields
  theta?: number;
  thetaSE?: number;
  confidenceLow?: number;
  confidenceHigh?: number;
}

// Skill types
export interface UserSkillProfile {
  skillId: number;
  skillName: string;
  skillCode: string;
  level: number;
  lastUpdated: string;
  // IRT fields
  theta?: number;
  thetaSE?: number;
  confidenceLow?: number;
  confidenceHigh?: number;
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

// Topic progress for learning features
export interface TopicProgress {
  topicId: number;
  topicName: string;
  totalQuestions: number;
  correctAnswers: number;
  incorrectAnswers: number;
  masteryPercentage: number;
}

// ─── Stage 4: Enhanced Analytics Types ───────────────────────────

export interface Dashboard {
  totalQuestionsAnswered: number;
  correctAnswers: number;
  overallAccuracy: number;
  currentStreak: number;
  bestStreak: number;
  skillProfiles: UserSkillProfile[];
  skillHistory: SkillHistoryPoint[];
  activityHeatmap: DailyActivity[];
  difficultyBreakdown: DifficultyStats[];
}

export interface SkillHistoryPoint {
  skillName: string;
  skillCode: string;
  level: number;
  date: string;
}

export interface DailyActivity {
  date: string;
  questionsAnswered: number;
  correctCount: number;
}

export interface DifficultyStats {
  difficulty: string;
  totalAnswered: number;
  correctCount: number;
  accuracy: number;
}

export interface TestSessionSummary {
  id: number;
  examTypeCode: string;
  examTypeName: string;
  mode: string;
  startedAt: string;
  completedAt: string | null;
  totalQuestions: number;
  correctCount: number;
  score: number | null;
}

export interface TestSessionDetail extends TestSessionSummary {
  answers: SessionAnswer[];
}

export interface SessionAnswer {
  questionId: number;
  questionText: string;
  topicName: string;
  difficulty: string;
  selectedOptionId: number;
  selectedOptionText: string;
  correctOptionId: number;
  correctOptionText: string;
  isCorrect: boolean;
  explanation: string | null;
  timeSpentSeconds: number | null;
}

export interface SubmitAnswerRequest {
  questionId: number;
  answerOptionId: number;
  timeSpentSeconds?: number;
  testSessionId?: number;
}
