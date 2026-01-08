import api from './api';

// AI Content Recommendation Types
export interface RecommendedQuiz {
  id: number;
  title: string;
  subject: string;
  difficulty: string;
  recommendationReason: string;
  estimatedDuration: number;
  questionsCount: number;
}

export interface RecommendedExam {
  id: number;
  title: string;
  subject: string;
  difficulty: string;
  recommendationReason: string;
  duration: number;
  questionsCount: number;
}

export interface RecommendedFlashcard {
  id: number;
  title: string;
  subject: string;
  cardsCount: number;
  recommendationReason: string;
  estimatedStudyTime: number;
}

export interface PersonalizedTip {
  topic: string;
  message: string;
  priority: string;
  actionableSteps: string[];
}

export interface NextTopic {
  subject: string;
  topic: string;
  reason: string;
  estimatedStudyTime: number;
}

export interface AIDashboardData {
  recommendedQuizzes: RecommendedQuiz[];
  recommendedExams: RecommendedExam[];
  recommendedFlashcards: RecommendedFlashcard[];
  tips: PersonalizedTip[];
  nextTopic: NextTopic | null;
}

// Adaptive Learning Types
export interface NextReviewPrediction {
  flashcardId: number;
  nextReviewTime: string;
  confidence: number;
  currentInterval: number;
}

export interface StudyPlanItem {
  subject: string;
  topic: string;
  recommendedTime: number;
  priority: string;
  estimatedDifficulty: string;
}

export interface ModelStatus {
  isModelTrained: boolean;
  lastTrainingDate: string | null;
  totalDataPoints: number;
  modelAccuracy: number | null;
}

// University Recommendation Types
export interface UniversityRecommendation {
  universityId: number;
  universityName: string;
  score: number;
  matchReasons: string[];
  country: string;
  tuitionFee: number;
  admissionRequirements: string;
}

// AI Content Generation Types
export interface GeneratedQuestion {
  questionText: string;
  options: string[];
  correctAnswer: string;
  explanation: string;
}

export interface GeneratedExplanation {
  topic: string;
  explanation: string;
  keyPoints: string[];
  examples: string[];
}

export interface GeneratedHint {
  hint: string;
  difficulty: string;
}

export interface GeneratedFlashcardSet {
  title: string;
  flashcards: Array<{
    front: string;
    back: string;
  }>;
}

class AIService {
  // ==================== Content Recommendations ====================
  
  async getRecommendedQuizzes(limit: number = 5): Promise<RecommendedQuiz[]> {
    const { data } = await api.get('/ai/content-recommendations/quizzes/recommended', {
      params: { limit }
    });
    return data;
  }

  async getRecommendedExams(limit: number = 5): Promise<RecommendedExam[]> {
    const { data } = await api.get('/ai/content-recommendations/exams/recommended', {
      params: { limit }
    });
    return data;
  }

  async getRecommendedFlashcards(limit: number = 5): Promise<RecommendedFlashcard[]> {
    const { data } = await api.get('/ai/content-recommendations/flashcards/recommended', {
      params: { limit }
    });
    return data;
  }

  async getNextTopic(): Promise<NextTopic | null> {
    const { data } = await api.get('/ai/content-recommendations/next-topic');
    return data;
  }

  async getPersonalizedTips(): Promise<PersonalizedTip[]> {
    const { data } = await api.get('/ai/content-recommendations/tips');
    return data;
  }

  async getAIDashboard(): Promise<AIDashboardData> {
    const { data } = await api.get('/ai/content-recommendations/dashboard');
    return data;
  }

  // ==================== Adaptive Learning ====================
  
  async predictNextReview(flashcardId: number): Promise<NextReviewPrediction> {
    const { data } = await api.get(`/ai/adaptive-learning/next-review/${flashcardId}`);
    return data;
  }

  async getStudyPlan(): Promise<StudyPlanItem[]> {
    const { data } = await api.get('/ai/adaptive-learning/study-plan');
    return data;
  }

  async retrainModel(): Promise<void> {
    await api.post('/ai/adaptive-learning/retrain');
  }

  async getModelStatus(): Promise<ModelStatus> {
    const { data } = await api.get('/ai/adaptive-learning/model-status');
    return data;
  }

  // ==================== University Recommendations ====================
  
  async getUniversityRecommendations(limit: number = 10): Promise<UniversityRecommendation[]> {
    const { data } = await api.get('/ai/university-recommendations', {
      params: { limit }
    });
    return data;
  }

  async updateUserProfile(profileData: {
    preferredSubjects?: string[];
    careerGoals?: string;
    budgetLimit?: number;
    preferredLocation?: string;
    targetAdmissionScore?: number;
  }): Promise<void> {
    await api.post('/ai/university-recommendations/update-profile', profileData);
  }

  async getRecommendationExplanation(universityId: number): Promise<string> {
    const { data } = await api.get(`/ai/university-recommendations/${universityId}/explanation`);
    return data.explanation;
  }

  // ==================== AI Content Generation ====================
  
  async generateQuestions(params: {
    topic: string;
    difficulty: string;
    count: number;
  }): Promise<GeneratedQuestion[]> {
    const { data } = await api.post('/ai/generator/questions', params);
    return data;
  }

  async generateExplanation(topic: string): Promise<GeneratedExplanation> {
    const { data } = await api.post('/ai/generator/explanation', { topic });
    return data;
  }

  async generateHint(params: {
    question: string;
    difficulty: string;
  }): Promise<GeneratedHint> {
    const { data } = await api.post('/ai/generator/hint', params);
    return data;
  }

  async generateFlashcardSet(params: {
    topic: string;
    count: number;
  }): Promise<GeneratedFlashcardSet> {
    const { data } = await api.post('/ai/generator/flashcards', params);
    return data;
  }

  async generateStudyGuide(topic: string): Promise<string> {
    const { data } = await api.post('/ai/generator/study-guide', { topic });
    return data.guide;
  }
}

export default new AIService();
