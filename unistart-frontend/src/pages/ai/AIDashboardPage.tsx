import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  Brain,
  BookOpen,
  Target,
  Lightbulb,
  TrendingUp,
  Clock,
  Award,
  ArrowRight,
  Sparkles,
  Calendar,
  Activity,
} from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import aiService, { 
  AIDashboardData, 
  StudyPlanItem, 
  ModelStatus 
} from '../../services/aiService';

const AIDashboardPage = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [dashboardData, setDashboardData] = useState<AIDashboardData | null>(null);
  const [studyPlan, setStudyPlan] = useState<StudyPlanItem[]>([]);
  const [modelStatus, setModelStatus] = useState<ModelStatus | null>(null);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      const [dashboard, plan, status] = await Promise.all([
        aiService.getAIDashboard(),
        aiService.getStudyPlan(),
        aiService.getModelStatus(),
      ]);
      setDashboardData(dashboard);
      setStudyPlan(plan);
      setModelStatus(status);
    } catch (error) {
      console.error('Ошибка загрузки AI dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleRetrainModel = async () => {
    try {
      await aiService.retrainModel();
      await loadDashboardData();
      alert('Модель успешно переобучена!');
    } catch (error: any) {
      console.error('Ошибка переобучения модели:', error);
      const errorMessage = error?.response?.data?.message || 
        'Ошибка при переобучении модели. Возможно, недостаточно данных (требуется минимум 100 примеров).';
      alert(errorMessage);
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority.toLowerCase()) {
      case 'high':
        return 'text-red-600 bg-red-50';
      case 'medium':
        return 'text-yellow-600 bg-yellow-50';
      case 'low':
        return 'text-green-600 bg-green-50';
      default:
        return 'text-gray-600 bg-gray-50';
    }
  };

  const getDifficultyColor = (difficulty: string | undefined) => {
    if (!difficulty) return 'text-gray-600';
    
    switch (difficulty.toLowerCase()) {
      case 'hard':
        return 'text-purple-600';
      case 'medium':
        return 'text-blue-600';
      case 'easy':
        return 'text-green-600';
      default:
        return 'text-gray-600';
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <Brain className="w-16 h-16 mx-auto mb-4 text-blue-600 animate-pulse" />
          <p className="text-gray-600">Загрузка AI панели...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-purple-50 to-pink-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Заголовок */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <div className="flex items-center gap-3 mb-2">
            <div className="p-3 bg-gradient-to-br from-blue-500 to-purple-600 rounded-xl">
              <Brain className="w-8 h-8 text-white" />
            </div>
            <div>
              <h1 className="text-4xl font-bold text-gray-900">
                AI Ассистент
              </h1>
              <p className="text-gray-600">
                Персонализированные рекомендации и адаптивное обучение
              </p>
            </div>
          </div>
        </motion.div>

        {/* Прогресс адаптации AI */}
        {modelStatus && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="mb-6"
          >
            <Card>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-4 flex-1">
                  {modelStatus.isModelTrained ? (
                    <div className="text-2xl">🌳</div>
                  ) : modelStatus.totalDataPoints >= 50 ? (
                    <div className="text-2xl">🌿</div>
                  ) : (
                    <div className="text-2xl">🌱</div>
                  )}
                  <div className="flex-1">
                    <p className="font-semibold text-gray-900">
                      {modelStatus.isModelTrained 
                        ? '🎉 AI полностью настроен под вас' 
                        : modelStatus.totalDataPoints >= 50
                        ? 'AI адаптируется под ваш темп'
                        : 'AI начинает узнавать вас'}
                    </p>
                    <p className="text-sm text-gray-600">
                      AI изучил {Math.min(100, Math.round((modelStatus.totalDataPoints / 100) * 100))}% вашего стиля обучения
                      {!modelStatus.isModelTrained && modelStatus.totalDataPoints < 100 && 
                        ` • Еще ${100 - modelStatus.totalDataPoints} сессий для полной персонализации`}
                    </p>
                    {modelStatus.isModelTrained && (
                      <div className="mt-2">
                        <div className="w-full bg-gray-200 rounded-full h-2">
                          <div className="bg-green-600 h-2 rounded-full" style={{ width: '100%' }}></div>
                        </div>
                      </div>
                    )}
                    {!modelStatus.isModelTrained && (
                      <div className="mt-2">
                        <div className="w-full bg-gray-200 rounded-full h-2">
                          <div 
                            className="bg-blue-600 h-2 rounded-full transition-all duration-500" 
                            style={{ width: `${Math.min(100, (modelStatus.totalDataPoints / 100) * 100)}%` }}
                          ></div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
                {modelStatus.totalDataPoints >= 100 && !modelStatus.isModelTrained && (
                  <Button onClick={handleRetrainModel} variant="primary">
                    <Sparkles className="w-4 h-4 mr-2" />
                    Активировать персонализацию
                  </Button>
                )}
              </div>
            </Card>
          </motion.div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Левая колонка - Рекомендации */}
          <div className="lg:col-span-2 space-y-6">
            {/* Следующая тема */}
            {dashboardData?.nextTopic && (
              <motion.div
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
              >
                <Card className="bg-gradient-to-br from-blue-500 to-purple-600 text-white">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-2">
                        <Target className="w-6 h-6" />
                        <h3 className="text-xl font-bold">Следующая тема для изучения</h3>
                      </div>
                      <p className="text-lg font-semibold mb-1">{dashboardData.nextTopic.topic}</p>
                      <p className="text-sm opacity-90 mb-2">{dashboardData.nextTopic.subject}</p>
                      <p className="text-sm opacity-80 mb-4">{dashboardData.nextTopic.reason}</p>
                      <div className="flex items-center gap-2 text-sm">
                        <Clock className="w-4 h-4" />
                        <span>Примерное время: {dashboardData.nextTopic.estimatedStudyTime} мин</span>
                      </div>
                    </div>
                    <ArrowRight className="w-6 h-6 flex-shrink-0" />
                  </div>
                </Card>
              </motion.div>
            )}

            {/* Рекомендованные квизы */}
            {dashboardData?.recommendedQuizzes && dashboardData.recommendedQuizzes.length > 0 && (
              <motion.div
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.1 }}
              >
                <Card>
                  <div className="flex items-center justify-between mb-4">
                    <div className="flex items-center gap-2">
                      <BookOpen className="w-6 h-6 text-blue-600" />
                      <h3 className="text-xl font-bold text-gray-900">Рекомендованные квизы</h3>
                    </div>
                    <Button variant="ghost" size="sm" onClick={() => navigate('/quizzes')}>
                      Все квизы
                    </Button>
                  </div>
                  <div className="space-y-3">
                    {dashboardData.recommendedQuizzes.map((quiz, index) => (
                      <div
                        key={quiz.id || `quiz-${index}`}
                        className="p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors cursor-pointer"
                        onClick={() => {
                          if (quiz.id) {
                            navigate(`/quizzes/${quiz.id}/take`);
                          }
                        }}
                      >
                        <div className="flex items-start justify-between mb-2">
                          <div className="flex-1">
                            <p className="font-semibold text-gray-900">{quiz.title}</p>
                            <p className="text-sm text-gray-600">{quiz.subject}</p>
                          </div>
                          <span className={`px-2 py-1 text-xs font-semibold rounded ${getDifficultyColor(quiz.difficulty)}`}>
                            {quiz.difficulty}
                          </span>
                        </div>
                        <p className="text-sm text-blue-600 mb-2">{quiz.recommendationReason}</p>
                        <div className="flex items-center gap-4 text-xs text-gray-500">
                          <span>{quiz.questionsCount} вопросов</span>
                          <span>{quiz.estimatedDuration} мин</span>
                        </div>
                      </div>
                    ))}
                  </div>
                </Card>
              </motion.div>
            )}

            {/* Рекомендованные флешкарты */}
            {dashboardData?.recommendedFlashcards && dashboardData.recommendedFlashcards.length > 0 && (
              <motion.div
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.2 }}
              >
                <Card>
                  <div className="flex items-center justify-between mb-4">
                    <div className="flex items-center gap-2">
                      <Award className="w-6 h-6 text-purple-600" />
                      <h3 className="text-xl font-bold text-gray-900">Рекомендованные карточки</h3>
                    </div>
                    <Button variant="ghost" size="sm" onClick={() => navigate('/flashcards')}>
                      Все карточки
                    </Button>
                  </div>
                  <div className="space-y-3">
                    {dashboardData.recommendedFlashcards.map((flashcard, index) => (
                      <div
                        key={flashcard.id || `flashcard-${index}`}
                        className="p-4 bg-purple-50 rounded-lg hover:bg-purple-100 transition-colors cursor-pointer"
                        onClick={() => {
                          if (flashcard.id) {
                            navigate(`/flashcards/${flashcard.id}/study`);
                          }
                        }}
                      >
                        <p className="font-semibold text-gray-900 mb-1">{flashcard.title}</p>
                        <p className="text-sm text-gray-600 mb-2">{flashcard.subject}</p>
                        <p className="text-sm text-purple-600 mb-2">{flashcard.recommendationReason}</p>
                        <div className="flex items-center gap-4 text-xs text-gray-500">
                          <span>{flashcard.cardsCount} карточек</span>
                          <span>~{flashcard.estimatedStudyTime} мин</span>
                        </div>
                      </div>
                    ))}
                  </div>
                </Card>
              </motion.div>
            )}
          </div>

          {/* Правая колонка - План и советы */}
          <div className="space-y-6">
            {/* План обучения */}
            {studyPlan.length > 0 && (
              <motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
              >
                <Card>
                  <div className="flex items-center gap-2 mb-4">
                    <Calendar className="w-6 h-6 text-green-600" />
                    <h3 className="text-xl font-bold text-gray-900">План обучения</h3>
                  </div>
                  <div className="space-y-3">
                    {studyPlan.map((item, index) => (
                      <div key={`study-${item.topic}-${index}`} className="p-3 bg-green-50 rounded-lg">
                        <div className="flex items-start justify-between mb-2">
                          <div className="flex-1">
                            <p className="font-semibold text-gray-900 text-sm">{item.topic}</p>
                            <p className="text-xs text-gray-600">{item.subject}</p>
                          </div>
                          <span className={`px-2 py-1 text-xs font-semibold rounded ${getPriorityColor(item.priority)}`}>
                            {item.priority}
                          </span>
                        </div>
                        <div className="flex items-center gap-2 text-xs text-gray-500">
                          <Clock className="w-3 h-3" />
                          <span>{item.recommendedTime} мин</span>
                          <span>•</span>
                          <span className={getDifficultyColor(item.estimatedDifficulty)}>
                            {item.estimatedDifficulty}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>
                </Card>
              </motion.div>
            )}

            {/* Персональные советы */}
            {dashboardData?.tips && dashboardData.tips.length > 0 && (
              <motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.1 }}
              >
                <Card>
                  <div className="flex items-center gap-2 mb-4">
                    <Lightbulb className="w-6 h-6 text-yellow-600" />
                    <h3 className="text-xl font-bold text-gray-900">Советы</h3>
                  </div>
                  <div className="space-y-4">
                    {dashboardData.tips.map((tip, index) => (
                      <div key={`tip-${tip.topic}-${index}`} className="p-3 bg-yellow-50 rounded-lg">
                        <div className="flex items-start justify-between mb-2">
                          <p className="font-semibold text-gray-900 text-sm">{tip.topic}</p>
                          <span className={`px-2 py-1 text-xs font-semibold rounded ${getPriorityColor(tip.priority)}`}>
                            {tip.priority}
                          </span>
                        </div>
                        <p className="text-sm text-gray-700 mb-2">{tip.message}</p>
                        {tip.actionableSteps.length > 0 && (
                          <ul className="space-y-1">
                            {tip.actionableSteps.map((step, stepIndex) => (
                              <li key={`step-${index}-${stepIndex}`} className="text-xs text-gray-600 flex items-start gap-2">
                                <TrendingUp className="w-3 h-3 flex-shrink-0 mt-0.5 text-yellow-600" />
                                <span>{step}</span>
                              </li>
                            ))}
                          </ul>
                        )}
                      </div>
                    ))}
                  </div>
                </Card>
              </motion.div>
            )}

            {/* Кнопка рекомендаций университетов */}
            <motion.div
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: 0.2 }}
            >
              <Card className="bg-gradient-to-br from-orange-500 to-red-600 text-white hover:shadow-xl transition-shadow cursor-pointer"
                onClick={() => navigate('/ai/universities')}>
                <div className="flex items-center justify-between">
                  <div>
                    <h3 className="text-xl font-bold mb-2">Рекомендации университетов</h3>
                    <p className="text-sm opacity-90">Персонализированный подбор вузов</p>
                  </div>
                  <ArrowRight className="w-8 h-8" />
                </div>
              </Card>
            </motion.div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AIDashboardPage;
