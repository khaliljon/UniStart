import api from './api';

// Types
export interface SubjectDto {
  id: number;
  name: string;
  description?: string;
}

export interface UserPreferencesDto {
  // Цели обучения
  learningGoal: string; // "ENT" | "University" | "SelfStudy" | "Professional"
  targetExamType?: string;
  targetUniversityId?: number;
  careerGoal?: string;
  
  // Географические предпочтения
  preferredCountry?: string;
  preferredCity?: string;
  willingToRelocate: boolean;
  
  // Финансовые предпочтения
  maxBudgetPerYear?: number;
  interestedInScholarships: boolean;
  
  // Языковые предпочтения
  preferredLanguages: string[];
  englishLevel?: string; // "A1" | "A2" | "B1" | "B2" | "C1" | "C2"
  
  // Предметы
  interestedSubjectIds: number[];
  strongSubjectIds: number[];
  weakSubjectIds: number[];
  
  // Формат обучения
  prefersFlashcards: boolean;
  prefersQuizzes: boolean;
  prefersExams: boolean;
  preferredDifficulty: number; // 1=Easy, 2=Medium, 3=Hard
  dailyStudyTimeMinutes: number;
  
  // Расписание
  preferredStudyTime?: string; // "Morning" | "Afternoon" | "Evening" | "Night"
  studyDays: string[]; // ["Mon", "Tue", "Wed", ...]
  
  // Мотивация
  motivationLevel: number; // 1-5
  needsReminders: boolean;
}

export interface UserPreferencesResponseDto extends UserPreferencesDto {
  id: number;
  userId: string;
  targetUniversityName?: string;
  interestedSubjects: SubjectDto[];
  strongSubjects: SubjectDto[];
  weakSubjects: SubjectDto[];
  onboardingCompleted: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface OnboardingStatusResponse {
  onboardingCompleted: boolean;
}

const preferencesService = {
  /**
   * Получить предпочтения текущего пользователя
   */
  async getMyPreferences(): Promise<UserPreferencesResponseDto> {
    const response = await api.get<UserPreferencesResponseDto>('/student/preferences');
    return response.data;
  },

  /**
   * Проверить статус Onboarding
   */
  async checkOnboardingStatus(): Promise<OnboardingStatusResponse> {
    const response = await api.get<OnboardingStatusResponse>('/student/preferences/onboarding/status');
    return response.data;
  },

  /**
   * Создать или обновить предпочтения
   */
  async createOrUpdatePreferences(preferences: UserPreferencesDto): Promise<UserPreferencesResponseDto> {
    const response = await api.post<UserPreferencesResponseDto>('/student/preferences', preferences);
    return response.data;
  },

  /**
   * Завершить Onboarding
   */
  async completeOnboarding(): Promise<{ message: string; onboardingCompleted: boolean }> {
    const response = await api.post('/student/preferences/onboarding/complete');
    return response.data;
  },

  /**
   * Пропустить Onboarding (создаст предпочтения по умолчанию)
   */
  async skipOnboarding(): Promise<{ message: string; onboardingCompleted: boolean; preferences: UserPreferencesResponseDto }> {
    const response = await api.post('/student/preferences/onboarding/skip');
    return response.data;
  },

  /**
   * Получить рекомендуемые предметы
   */
  async getRecommendedSubjects(): Promise<SubjectDto[]> {
    const response = await api.get<SubjectDto[]>('/student/preferences/recommended-subjects');
    return response.data;
  },
};

export default preferencesService;
