// Auth types
export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  name: string;
  role: string;
  hasCompletedOnboarding: boolean;
  subscriptionTier: string;
  subscriptionExpiresAt: string | null;
  emailVerified: boolean;
  phoneNumber?: string | null;
  createdAt: string;
}

export interface AuthResponse {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  name: string;
  role: string;
  hasCompletedOnboarding: boolean;
  subscriptionTier: string;
  subscriptionExpiresAt: string | null;
  emailVerified: boolean;
  token: string;
  expiresAt: string;
  schoolSubdomain: string | null;
  phoneNumber?: string | null;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
  phoneNumber: string;
  role?: 'Student' | 'Tutor' | 'SchoolAdmin';
  schoolSlug?: string;
  applyToSchoolId?: number;
  schoolName?: string;
  referralCode?: string;
}

export interface VerifyEmailRequest {
  email: string;
  code: string;
}

export interface ResendCodeRequest {
  email: string;
}

export interface GoogleLoginRequest {
  idToken: string;
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
  hasHint?: boolean;
  imageUrl?: string | null;
}

export interface NextQuestionResponse {
  question: Question | null;
  testCompleted: boolean;
  questionsAnswered: number;
  totalQuestions: number;
  topicMastery: number;
  masteryReached: boolean;
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
  sectionIds?: number[];
  topicId?: number;
}

// Topic progress for learning features
export interface TopicProgress {
  topicId: number;
  topicName: string;
  totalQuestions: number;
  correctAnswers: number;
  incorrectAnswers: number;
  masteryPercentage: number;
  lessonCount?: number;
  hasVideoLessons?: boolean;
  irtLevel?: number;
  sectionId?: number;
  sectionName?: string;
  examTypeCode?: string;
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

// Stage 6: Study Plan Types
export interface StudyGoal {
  id: number;
  examTypeCode: string;
  examTypeName: string;
  targetDate: string;
  targetScore: number;
  isActive: boolean;
  daysUntilExam: number;
  recommendedHoursPerDay: number;
  createdAt: string;
  sectionIds?: number[];
}

export interface CreateStudyGoalRequest {
  examTypeCode: string;
  targetDate: string;
  targetScore: number;
  sectionIds?: number[];
  hoursPerDay?: number;
}

export interface UpdateStudyGoalRequest {
  targetDate?: string;
  targetScore?: number;
  isActive?: boolean;
}

export interface StudyPlan {
  id: number;
  goalId: number;
  examTypeCode: string;
  examTypeName: string;
  generatedAt: string;
  isActive: boolean;
  totalEntries: number;
  completedEntries: number;
  completionPercent: number;
  entries: StudyPlanEntry[];
}

export interface StudyPlanEntry {
  id: number;
  topicId: number;
  topicName: string;
  sectionId: number | null;
  date: string;
  recommendedMinutes: number;
  type: 'New' | 'Review' | 'Practice' | 'Weakness';
  recommendedQuestions: number;
  isCompleted: boolean;
  completedAt: string | null;
  questionsAnswered: number;
  correctAnswers: number;
}

export interface TodayPlan {
  date: string;
  hasGoal: boolean;
  examTypeCode: string | null;
  examTypeName: string | null;
  daysUntilExam: number;
  totalMinutesToday: number;
  entries: StudyPlanEntry[];
  recommendation: string;
}

export interface CompleteEntryRequest {
  questionsAnswered: number;
  correctAnswers: number;
}

export interface PlanStats {
  totalDays: number;
  completedDays: number;
  skippedDays: number;
  averageAccuracy: number;
  totalQuestionsAnswered: number;
  adherencePercent: number;
  weeklySummary: WeekSummary[];
}

export interface WeekSummary {
  weekStart: string;
  plannedEntries: number;
  completedEntries: number;
  totalMinutesPlanned: number;
  accuracy: number;
}

// ═══════════════════════════════════════════════════════
//  SCORE PREDICTION
// ═══════════════════════════════════════════════════════

export interface ScorePrediction {
  examTypeCode: string;
  examName: string;
  predictedScore: number;
  minPossibleScore: number;
  maxPossibleScore: number;
  confidenceLow: number;
  confidenceHigh: number;
  confidencePercent: number;
  targetScore: number | null;
  gapToTarget: number | null;
  sections: SectionPrediction[];
  improvementTips: ImprovementTip[];
  calculatedAt: string;
  answersCount: number;
  isReliable: boolean;
}

export interface SectionPrediction {
  sectionId: number;
  sectionName: string;
  theta: number;
  thetaSE: number;
  predictedScore: number;
  minScore: number;
  maxScore: number;
  confidenceLow: number;
  confidenceHigh: number;
  accuracy: number;
  strength: 'strong' | 'average' | 'weak' | 'critical' | 'insufficient';
  answersCount: number;
  isReliable: boolean;
}

export interface ImprovementTip {
  topicId: number;
  topicName: string;
  sectionName: string;
  currentTheta: number;
  currentLevel: number;
  potentialScoreGain: number;
  recommendation: string;
}

export interface WhatIfResult {
  topicName: string;
  currentLevel: number;
  improvedLevel: number;
  currentPredictedTotal: number;
  improvedPredictedTotal: number;
  scoreGain: number;
}

export interface WhatIfRequest {
  examTypeCode: string;
  topicId: number;
  improvedLevel: number;
}

export interface PredictionHistory {
  date: string;
  predictedScore: number;
  confidenceLow: number;
  confidenceHigh: number;
}

// ─── Recommendations (Stage 8) ──────────────────────────

export interface Recommendation {
  type: 'after_session' | 'daily' | 'mode' | 'milestone' | 'streak';
  priority: 'high' | 'medium' | 'low';
  title: string;
  description: string;
  icon: string | null;
  actionLabel: string | null;
  actionUrl: string | null;
  metadata: Record<string, unknown> | null;
}

export interface Streak {
  currentStreak: number;
  longestStreak: number;
  studiedToday: boolean;
  lastStudyDate: string | null;
  totalStudyDays: number;
}

export interface Milestone {
  id: number;
  code: string;
  title: string;
  description: string;
  icon: string;
  achievedAt: string;
  isNew: boolean;
}

export interface DailySummary {
  questionsAnswered: number;
  correctAnswers: number;
  accuracy: number;
  minutesSpent: number;
  topicsStudied: number;
}

export interface DailyBriefing {
  currentStreak: number;
  longestStreak: number;
  recommendations: Recommendation[];
  recentMilestones: Milestone[];
  streak: Streak;
  yesterdaySummary: DailySummary | null;
}

export interface AfterSession {
  recommendations: Recommendation[];
}

// ─── Admin (Stage 9) ────────────────────────────────────

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface QuestionListItem {
  id: number;
  topicName: string;
  sectionName: string;
  examTypeCode: string;
  text: string;
  difficulty: string;
  difficultyParam: number;
  discriminationParam: number;
  answerCount: number;
  createdAt: string;
}

export interface QuestionDetail {
  id: number;
  topicId: number;
  topicName: string;
  sectionName: string;
  examTypeCode: string;
  text: string;
  difficulty: string;
  explanation: string | null;
  imageUrl: string | null;
  difficultyParam: number;
  discriminationParam: number;
  guessParam: number;
  createdAt: string;
  answerOptions: AdminAnswerOption[];
}

export interface AdminAnswerOption {
  id: number;
  text: string;
  isCorrect: boolean;
}

export interface QuestionStats {
  totalQuestions: number;
  byExam: Record<string, number>;
  byDifficulty: Record<string, number>;
  byTopic: Record<string, number>;
  topicsWithQuestions: number;
  topicsWithoutQuestions: number;
  topicsWithoutQuestionsList: string[];
}

export interface BulkImportResult {
  total: number;
  imported: number;
  failed: number;
  errors: string[];
}

// ─── Admin User Management ──────────────────────────────

export interface AdminUser {
  id: number;
  email: string;
  name: string;
  role: string;
  subscriptionTier: string;
  subscriptionExpiresAt: string | null;
  hasCompletedOnboarding: boolean;
  isBlocked: boolean;
  blockedAt: string | null;
  blockReason: string | null;
  isDeleted: boolean;
  deletedAt: string | null;
  createdAt: string;
  updatedAt: string | null;
  totalAnswers: number;
  correctAnswers: number;
  testSessions: number;
  schoolId: number | null;
  schoolName: string | null;
  phoneNumber: string | null;
}

export interface AdminUserStats {
  totalUsers: number;
  students: number;
  tutors: number;
  admins: number;
  proUsers: number;
  activeLast7Days: number;
}

export interface AdminTopicSummary {
  id: number;
  name: string;
  sectionName: string;
  examTypeCode: string;
  questionCount: number;
}

export interface AdminSection {
  id: number;
  name: string;
  examTypeCode: string;
}

export interface AdminSkill {
  id: number;
  code: string;
  name: string;
}

export interface AdminDashboard {
  questionStats: QuestionStats;
  userStats: AdminUserStats;
  topics: AdminTopicSummary[];
}

// ─── Trash / Recycle Bin ────────────────────────────────

export interface TrashItem {
  id: number;
  entityType: 'User' | 'Question';
  displayName: string;
  detail: string | null;
  deletedAt: string | null;
  deletedBy: string | null;
  daysUntilPurge: number;
}

export interface TrashSummary {
  totalUsers: number;
  totalQuestions: number;
  items: TrashItem[];
}

// ─── Stage 9.2: Learning Materials ──────────────────────

export interface TopicLessonSummary {
  id: number;
  title: string;
  videoUrl: string | null;
  sortOrder: number;
}

export interface TopicWithLessons {
  topicId: number;
  topicName: string;
  lessonCount: number;
  lessons: TopicLessonSummary[];
}

export interface TopicLesson {
  id: number;
  topicId: number;
  topicName: string;
  title: string;
  content: string;
  videoUrl: string | null;
  sortOrder: number;
}

// ─── Stage 9.3: Mock Exams ─────────────────────────────

export interface MockExamListItem {
  id: number;
  examTypeCode: string;
  examTypeName: string;
  title: string;
  description: string;
  totalTimeMinutes: number;
  sectionCount: number;
  totalQuestions: number;
  bestScore: number | null;
  attemptCount: number;
  runsRemaining: number;
  freeAvailable: boolean;
}

export interface MockExamDetail {
  id: number;
  examTypeCode: string;
  examTypeName: string;
  title: string;
  description: string;
  totalTimeMinutes: number;
  sections: MockExamSectionInfo[];
}

export interface MockExamSectionInfo {
  id: number;
  name: string;
  timeLimitMinutes: number;
  questionCount: number;
  sortOrder: number;
  instructions: string | null;
}

export interface MockExamAttempt {
  attemptId: number;
  mockExamId: number;
  examTitle: string;
  status: string;
  currentSectionIndex: number;
  totalSections: number;
  startedAt: string;
  totalTimeMinutes: number;
  sectionNames?: string[];
}

export interface MockExamSectionState {
  sectionIndex: number;
  sectionName: string;
  timeLimitMinutes: number;
  instructions: string | null;
  questions: MockExamQuestion[];
  totalQuestions: number;
  answeredCount: number;
}

export interface MockExamQuestion {
  questionId: number;
  text: string;
  difficulty: string;
  topicName: string;
  options: MockExamOption[];
  selectedOptionId: number | null;
  readingPassageId: number | null;
  passageTitle: string | null;
  passageContent: string | null;
  imageUrl: string | null;
  isMultipleChoice: boolean;
  selectedOptionIds: number[];
}

export interface MockExamOption {
  id: number;
  text: string;
}

export interface MockExamSectionResult {
  sectionIndex: number;
  sectionName: string;
  totalQuestions: number;
  correctCount: number;
  unansweredCount: number;
  accuracy: number;
  timeLimitMinutes: number;
}

export interface MockExamResult {
  attemptId: number;
  mockExamId: number;
  examTitle: string;
  examTypeCode: string;
  totalScore: number;
  totalCorrect: number;
  totalQuestions: number;
  overallAccuracy: number;
  totalTimeMinutes: number;
  startedAt: string;
  completedAt: string | null;
  sectionResults: MockExamSectionResult[];
  answerReview: MockExamAnswerReview[];
}

export interface MockExamAnswerReview {
  questionId: number;
  questionText: string;
  topicName: string;
  difficulty: string;
  sectionName: string;
  selectedOptionId: number | null;
  selectedOptionText: string | null;
  correctOptionId: number;
  correctOptionText: string;
  isCorrect: boolean;
  isUnanswered: boolean;
  explanation: string | null;
  isMultipleChoice: boolean;
  selectedOptionIds: number[];
  correctOptionIds: number[];
}

export interface MockExamHistoryItem {
  attemptId: number;
  mockExamId: number;
  examTitle: string;
  examTypeCode: string;
  status: string;
  totalScore: number | null;
  startedAt: string;
  completedAt: string | null;
}

// Onboarding types
export interface OnboardingStatus {
  hasCompletedOnboarding: boolean;
  examTypeCode: string | null;
  examTypeName: string | null;
  targetDate: string | null;
  targetScore: number | null;
}

export interface ExamTypeInfo {
  code: string;
  name: string;
  minScore: number;
  maxScore: number;
  description: string;
  sections: ExamSectionInfo[];
}

export interface ExamSectionInfo {
  id: number;
  name: string;
  minScore: number;
  maxScore: number;
}

export interface CompleteOnboardingRequest {
  examTypeCode: string;
  targetDate: string;
  targetScore: number;
}

// Diagnostic test types
export interface DiagnosticSession {
  sessionId: number;
  examTypeCode: string;
  examTypeName: string;
  totalQuestions: number;
  currentIndex: number;
  isCompleted: boolean;
}

export interface DiagnosticQuestion {
  index: number;
  totalQuestions: number;
  questionId: number;
  text: string;
  difficulty: string;
  topicName: string;
  sectionName: string;
  options: AnswerOption[];
}

export interface DiagnosticAnswerResult {
  isCorrect: boolean;
  correctOptionId: number;
  correctOptionText: string;
  explanation: string | null;
  currentIndex: number;
  totalQuestions: number;
  isCompleted: boolean;
}

export interface DiagnosticResult {
  sessionId: number;
  examTypeCode: string;
  examTypeName: string;
  totalQuestions: number;
  correctCount: number;
  scorePercent: number;
  predictedScore: number;
  predictedScoreMin: number;
  predictedScoreMax: number;
  maxPossibleScore: number;
  level: string;
  sectionResults: DiagnosticSectionResult[];
  answers: DiagnosticAnswerReview[];
}

export interface DiagnosticSectionResult {
  sectionName: string;
  totalQuestions: number;
  correctCount: number;
  scorePercent: number;
}

export interface DiagnosticAnswerReview {
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
}

// Subscription types
export interface SubscriptionStatus {
  tier: string;
  isPro: boolean;
  expiresAt: string | null;
  dailyUsage: DailyUsage;
  limits: TierLimits;
  freeMockAvailable: boolean;
  hasTutorDiscount: boolean;
  linkedTutorName: string | null;
}

export interface DailyUsage {
  questionsAnswered: number;
  questionsLimit: number;
  questionsRemaining: number;
  lessonsViewed: number;
  lessonsLimit: number;
  lessonsRemaining: number;
  isLimitReached: boolean;
  serverTimeUtc: string;
}

export interface TierLimits {
  questionsPerDay: number;
  lessonsPerDay: number;
  mockExamsEnabled: boolean;
  fullAnalytics: boolean;
  fullStudyPlan: boolean;
  realtimePrediction: boolean;
}

export interface UpgradeResponse {
  success: boolean;
  tier: string;
  expiresAt: string | null;
  message: string;
}

// Notification types
export interface NotificationPreferences {
  welcomeEmail: boolean;
  streakReminder: boolean;
  weeklyDigest: boolean;
  studyPlanReminder: boolean;
  achievementNotification: boolean;
}

export interface UpdateNotificationPreferences {
  welcomeEmail?: boolean;
  streakReminder?: boolean;
  weeklyDigest?: boolean;
  studyPlanReminder?: boolean;
  achievementNotification?: boolean;
}

// ─── Tutor types ──────────────────────────────────────
export interface TutorCard {
  userId: number;
  name: string;
  headline: string;
  bio: string;
  specializations: string[];
  averageRating: number;
  totalReviews: number;
  totalStudents: number;
  isAvailable: boolean;
  isVerified: boolean;
  hourlyRate: number | null;
  avatarUrl: string | null;
}

export interface TutorListResult {
  items: TutorCard[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface TutorProfileDetail {
  userId: number;
  name: string;
  email: string;
  headline: string;
  bio: string;
  experience: string;
  specializations: string[];
  averageRating: number;
  totalReviews: number;
  totalStudents: number;
  isAvailable: boolean;
  isVerified: boolean;
  hourlyRate: number | null;
  avatarUrl: string | null;
  contactPreference: string;
  createdAt: string;
  schedule: ScheduleSlot[];
  recentReviews: TutorReview[];
  verificationRequestedAt: string | null;
  hasPaidSubscription: boolean;
  schoolId: number | null;
  teachingSections: string[] | null;
  subscriptionExpiresAt: string | null;
  schoolHasActiveSubscription: boolean;
}

export interface ScheduleSlot {
  id: number;
  dayOfWeek: number;
  dayName: string;
  startTime: string;
  endTime: string;
}

export interface TutorReview {
  id: number;
  studentId: number;
  studentName: string;
  rating: number;
  comment: string | null;
  createdAt: string;
}

export interface UpdateTutorProfile {
  headline?: string;
  bio?: string;
  experience?: string;
  specializations?: string[];
  teachingSections?: string[];
  hourlyRate?: number;
  isAvailable?: boolean;
  contactPreference?: string;
}

export interface ScheduleSlotInput {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
}

export interface CreateReviewRequest {
  rating: number;
  comment?: string;
}

// ─── Messaging types ──────────────────────────────────
export interface Conversation {
  id: number;
  otherUserId: number;
  otherUserName: string;
  otherUserRole: string;
  lastMessagePreview: string | null;
  lastMessageAt: string | null;
  unreadCount: number;
  status: string;
  requestMessage: string | null;
}

export interface Message {
  id: number;
  conversationId: number;
  senderId: number;
  senderName: string;
  text: string;
  sentAt: string;
  readAt: string | null;
  isEdited: boolean;
  type: string;
  isMine: boolean;
}

export interface MessagesPage {
  items: Message[];
  totalCount: number;
  hasMore: boolean;
}

export interface PendingRequest {
  conversationId: number;
  studentId: number;
  studentName: string;
  studentEmail: string;
  requestMessage: string | null;
  requestedAt: string;
}

export interface StudentInfo {
  userId: number;
  name: string;
  email: string;
  conversationStartedAt: string;
  lastMessageAt: string | null;
  lastMessagePreview: string | null;
}

export interface AcceptDeclineResult {
  conversationId: number;
  status: string;
  systemMessage: string | null;
}

// ─── Learning v2 types ────────────────────────────────

// Lesson Steps (TH-1)
export interface LessonStep {
  id: number;
  lessonId: number;
  title: string;
  content: string;
  stepType: 'Theory' | 'Example' | 'Quiz' | 'Summary';
  quizQuestionId: number | null;
  sortOrder: number;
}

export interface LessonWithSteps {
  id: number;
  topicId: number;
  topicName: string;
  title: string;
  videoUrl: string | null;
  steps: LessonStep[];
  completedSteps: number;
  totalSteps: number;
}

// Formula Cards (TH-2)
export interface FormulaCard {
  id: number;
  topicId: number;
  topicName: string;
  examTypeCode: string | null;
  title: string;
  formula: string;
  description: string | null;
  isBookmarked: boolean;
}

// Flashcards (TH-3)
export interface FlashcardDeck {
  id: number;
  title: string;
  description: string | null;
  examTypeCode: string | null;
  topicId: number | null;
  isSystem: boolean;
  totalCards: number;
  dueCards: number;
  masteredCards: number;
}

export interface FlashcardReview {
  id: number;
  front: string;
  back: string;
  currentInterval: number | null;
  easeFactor: number | null;
}

export interface FlashcardDto {
  id: number;
  deckId: number;
  front: string;
  back: string;
  sortOrder: number;
}

export interface CreateDeckRequest {
  title: string;
  description?: string;
  examTypeCode?: string;
  topicId?: number;
}

export interface CreateFlashcardRequest {
  deckId: number;
  front: string;
  back: string;
}

export interface ReviewFlashcardRequest {
  flashcardId: number;
  quality: number; // 0-5
}

// Timed Drills (TH-4)
export interface StartDrillRequest {
  drillType: 'Speed' | 'Marathon' | 'Streak';
  examTypeCodes?: string[];
  topicId?: number;
  timeLimitMinutes?: number;
}

export interface DrillQuestion {
  questionId: number;
  text: string;
  topicName: string | null;
  difficulty: string;
  options: DrillAnswerOption[];
}

export interface DrillAnswerOption {
  id: number;
  text: string;
}

export interface SubmitDrillAnswerRequest {
  drillResultId: number;
  questionId: number;
  answerOptionId: number;
  timeSpentSeconds: number;
}

export interface DrillAnswerResult {
  isCorrect: boolean;
  correctOptionId: number | null;
  explanation: string | null;
  currentStreak: number;
  totalCorrect: number;
  totalAnswered: number;
  drillEnded: boolean;
}

export interface DrillResult {
  id: number;
  drillType: string;
  questionsAnswered: number;
  correctAnswers: number;
  totalTimeSeconds: number;
  averageTimeSeconds: number;
  bestStreak: number;
  accuracyPercent: number;
  completedAt: string;
}

export interface PersonalBest {
  drillType: string;
  bestScore: number | null;
  bestStreak: number | null;
  bestAverageTime: number | null;
  achievedAt: string | null;
}

// Strategy Guides (TH-5)
export interface StrategyGuide {
  id: number;
  examTypeCode: string;
  title: string;
  summary: string;
  content: string;
  category: string;
  estimatedReadMinutes: number;
  isRead: boolean;
}

export interface StrategyGuideSummary {
  id: number;
  examTypeCode: string;
  title: string;
  summary: string;
  category: string;
  estimatedReadMinutes: number;
  isRead: boolean;
}

// Mistake Journal (TH-6)
export interface MistakeEntry {
  userAnswerId: number;
  questionId: number;
  questionText: string;
  topicName: string;
  difficulty: string;
  userAnswerText: string | null;
  correctAnswerText: string | null;
  explanation: string | null;
  answeredAt: string;
  errorType: string | null;
  noteText: string | null;
}

export interface ErrorPattern {
  errorType: string;
  count: number;
  percentage: number;
}

export interface TopicMistake {
  topicId: number;
  topicName: string;
  mistakeCount: number;
}

export interface MistakeAnalysis {
  totalMistakes: number;
  errorPatterns: ErrorPattern[];
  topicBreakdown: TopicMistake[];
}

// ─── Tutor-Student Binding ───────────────────────────────
export interface TutorStudentInfo {
  id: number;
  studentUserId: number;
  studentName: string;
  studentEmail: string;
  status: string;
  linkedAt: string;
  revokedAt: string | null;
}

export interface LinkedTutorInfo {
  tutorUserId: number;
  tutorName: string;
  headline: string;
  specializations: string[];
  averageRating: number;
  avatarUrl: string | null;
  linkedAt: string;
}

export interface LinkResult {
  success: boolean;
  message: string;
}


// ─── Tutor Questions (Этап 2) ───────────────────────────
export interface TutorQuestionListItem {
  id: number;
  text: string;
  difficulty: string;
  topicName: string;
  sectionName: string;
  examTypeCode: string;
  answerCount: number;
  createdAt: string;
}

export interface TutorQuestionDetail {
  id: number;
  topicId: number;
  topicName: string;
  sectionName: string;
  examTypeCode: string;
  text: string;
  difficulty: string;
  explanation: string | null;
  imageUrl: string | null;
  createdAt: string;
  answerOptions: AdminAnswerOption[];
}

export interface TutorQuestionsPage {
  items: TutorQuestionListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ─── Tutor Schools ──────────────────────────────────────
export interface TutorSchoolCard {
  id: number;
  name: string;
  slug: string;
  description: string;
  logoUrl: string | null;
  instagramUrl: string | null;
  telegramUrl: string | null;
  websiteUrl: string | null;
  specializations: string[];
  isPartner: boolean;
  tutorCount: number;
}

export interface ClaimableSchool {
  id: number;
  name: string;
  subdomain: string | null;
}

export interface TutorSchoolDetail {
  id: number;
  name: string;
  slug: string;
  description: string;
  logoUrl: string | null;
  instagramUrl: string | null;
  telegramUrl: string | null;
  websiteUrl: string | null;
  specializations: string[];
  isPartner: boolean;
  ownerUserId: number | null;
  tutors: TutorCard[];
}

// ─── School Admin (Этап 4) ─────────────────────────────
export interface SchoolAdmin {
  id: number;
  name: string;
  slug: string;
  description: string;
  descriptionEn: string | null;
  descriptionKz: string | null;
  logoUrl: string | null;
  instagramUrl: string | null;
  telegramUrl: string | null;
  websiteUrl: string | null;
  specializations: string[];
  isPartner: boolean;
  isActive: boolean;
  ownerUserId: number | null;
  tutorCount: number;
  createdAt: string;
  subdomain: string | null;
  primaryColor: string | null;
  primaryHoverColor: string | null;
  accentColor: string | null;
  navbarTitle: string | null;
}

export interface CreateSchoolRequest {
  name: string;
  description?: string;
  logoUrl?: string;
  websiteUrl?: string;
  instagramUrl?: string;
  telegramUrl?: string;
  specializations?: string;
}

export interface UpdateSchoolRequest {
  name?: string;
  description?: string;
  descriptionEn?: string;
  descriptionKz?: string;
  logoUrl?: string;
  websiteUrl?: string;
  instagramUrl?: string;
  telegramUrl?: string;
  specializations?: string;
  subdomain?: string;
  primaryColor?: string;
  primaryHoverColor?: string;
  accentColor?: string;
  navbarTitle?: string;
}

// ─── School Branding (White Label) ─────────────────────
export interface SchoolBranding {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  descriptionEn: string | null;
  descriptionKz: string | null;
  logoUrl: string | null;
  primaryColor: string | null;
  primaryHoverColor: string | null;
  accentColor: string | null;
  navbarTitle: string | null;
  specializations: string[];
}

// ─── Assignments (Этап 3) ──────────────────────────────

export interface AssignmentListItem {
  id: number;
  title: string;
  description: string | null;
  deadline: string | null;
  isActive: boolean;
  questionCount: number;
  studentCount: number;
  completedCount: number;
  createdAt: string;
}

export interface AssignmentDetail {
  id: number;
  title: string;
  description: string | null;
  deadline: string | null;
  isActive: boolean;
  createdAt: string;
  questions: AssignmentQuestionInfo[];
  students: AssignmentStudentProgress[];
}

export interface AssignmentQuestionInfo {
  questionId: number;
  text: string;
  difficulty: string;
  topicName: string;
  orderIndex: number;
}

export interface AssignmentStudentProgress {
  studentUserId: number;
  studentName: string;
  status: string;
  answeredCount: number;
  correctCount: number;
  totalQuestions: number;
  startedAt: string | null;
  completedAt: string | null;
  score: number | null;
}

export interface StudentAssignmentListItem {
  id: number;
  title: string;
  description: string | null;
  deadline: string | null;
  tutorName: string;
  status: string;
  totalQuestions: number;
  answeredCount: number;
  correctCount: number;
  score: number | null;
  createdAt: string;
}

export interface StudentAssignmentDetail {
  id: number;
  title: string;
  description: string | null;
  deadline: string | null;
  tutorName: string;
  status: string;
  totalQuestions: number;
  answeredCount: number;
  questions: StudentAssignmentQuestion[];
}

export interface StudentAssignmentQuestion {
  questionId: number;
  text: string;
  difficulty: string;
  options: AdminAnswerOption[];
  selectedOptionId: number | null;
  isCorrect: boolean | null;
  explanation: string | null;
}

export interface SubmitAssignmentAnswerResult {
  isCorrect: boolean;
  correctOptionId: number;
  explanation: string | null;
  answeredCount: number;
  totalQuestions: number;
  isCompleted: boolean;
  score: number | null;
}
