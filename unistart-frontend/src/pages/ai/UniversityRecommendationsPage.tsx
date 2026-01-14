import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  GraduationCap,
  MapPin,
  DollarSign,
  TrendingUp,
  Star,
  ArrowLeft,
  ExternalLink,
  CheckCircle,
  Sparkles,
  Target,
  Award,
} from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import aiService, { UniversityRecommendation } from '../../services/aiService';

const UniversityRecommendationsPage = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [recommendations, setRecommendations] = useState<UniversityRecommendation[]>([]);
  const [selectedUniversity, setSelectedUniversity] = useState<number | null>(null);
  const [explanation, setExplanation] = useState<string>('');
  const [loadingExplanation, setLoadingExplanation] = useState(false);

  useEffect(() => {
    loadRecommendations();
  }, []);

  const loadRecommendations = async () => {
    try {
      setLoading(true);
      const data: any = await aiService.getUniversityRecommendations(10);
      // API returns { total, recommendations }
      const recs = Array.isArray(data) ? data : (data?.recommendations || []);
      setRecommendations(recs);
    } catch (error) {
      console.error('Ошибка загрузки рекомендаций:', error);
      setRecommendations([]);
    } finally {
      setLoading(false);
    }
  };

  const handleShowExplanation = async (universityId: number) => {
    try {
      if (selectedUniversity === universityId && explanation) {
        // Скрыть объяснение если уже открыто
        setSelectedUniversity(null);
        setExplanation('');
        return;
      }
      
      setLoadingExplanation(true);
      setSelectedUniversity(universityId);
      const exp = await aiService.getRecommendationExplanation(universityId);
      setExplanation(exp);
    } catch (error: any) {
      console.error('Ошибка загрузки объяснения:', error);
      const errorMessage = error?.response?.data?.message || 'Не удалось загрузить объяснение';
      alert(errorMessage);
      setSelectedUniversity(null);
    } finally {
      setLoadingExplanation(false);
    }
  };

  const handleOpenUniversityPage = (universityId: number, universityName: string) => {
    // Поскольку страница университета еще не реализована, показываем уведомление
    alert(`Страница университета "${universityName}" находится в разработке. ID: ${universityId}`);
    // TODO: Когда страница будет готова, раскомментировать:
    // navigate(`/universities/${universityId}`);
  };

  const getScoreColor = (score: number) => {
    if (score >= 80) return 'text-green-600 bg-green-50';
    if (score >= 60) return 'text-blue-600 bg-blue-50';
    if (score >= 40) return 'text-yellow-600 bg-yellow-50';
    return 'text-orange-600 bg-orange-50';
  };

  const getScoreStars = (score: number) => {
    const stars = Math.round((score / 100) * 5);
    return Array(5).fill(0).map((_, i) => (
      <Star
        key={i}
        className={`w-5 h-5 ${i < stars ? 'fill-yellow-400 text-yellow-400' : 'text-gray-300'}`}
      />
    ));
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <GraduationCap className="w-16 h-16 mx-auto mb-4 text-blue-600 animate-pulse" />
          <p className="text-gray-600">Подбираем университеты...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Заголовок */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <Button
            variant="ghost"
            onClick={() => navigate('/ai/dashboard')}
            className="mb-4"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Назад к AI панели
          </Button>

          <div className="flex items-center gap-3 mb-2">
            <div className="p-3 bg-gradient-to-br from-blue-500 to-purple-600 rounded-xl">
              <GraduationCap className="w-8 h-8 text-white" />
            </div>
            <div>
              <h1 className="text-4xl font-bold text-gray-900">
                Рекомендации университетов
              </h1>
              <p className="text-gray-600">
                Персонализированный подбор на основе ваших интересов и достижений
              </p>
            </div>
          </div>
        </motion.div>

        {/* Информационная панель */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-6"
        >
          <Card className="bg-gradient-to-r from-indigo-500 to-purple-600 text-white">
            <div className="flex items-start gap-4">
              <Sparkles className="w-6 h-6 flex-shrink-0" />
              <div>
                <h3 className="font-bold text-lg mb-2">Как работает AI подбор?</h3>
                <p className="text-sm opacity-90">
                  Алгоритм анализирует ваши предпочтения, успеваемость по предметам, карьерные цели,
                  бюджет и желаемое местоположение для подбора наиболее подходящих университетов.
                </p>
              </div>
            </div>
          </Card>
        </motion.div>

        {/* Список рекомендаций */}
        {recommendations.length === 0 ? (
          <Card>
            <div className="text-center py-12">
              <Target className="w-16 h-16 mx-auto mb-4 text-gray-400" />
              <h3 className="text-xl font-bold text-gray-900 mb-2">
                Рекомендаций пока нет
              </h3>
              <p className="text-gray-600 mb-4">
                Заполните профиль и пройдите несколько квизов или экзаменов для получения персональных рекомендаций
              </p>
              <Button variant="primary" onClick={() => navigate('/dashboard')}>
                Перейти в профиль
              </Button>
            </div>
          </Card>
        ) : (
          <div className="space-y-4">
            {recommendations.map((rec, index) => (
              <motion.div
                key={rec.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.05 }}
              >
                <Card className="hover:shadow-xl transition-shadow">
                  <div className="flex items-start justify-between gap-4">
                    {/* Основная информация */}
                    <div className="flex-1">
                      <div className="flex items-start gap-4 mb-4">
                        <div className="flex-shrink-0 w-16 h-16 bg-gradient-to-br from-blue-500 to-purple-600 rounded-xl flex items-center justify-center text-white font-bold text-2xl">
                          {index + 1}
                        </div>
                        <div className="flex-1">
                          <div className="flex items-start justify-between mb-2">
                            <div>
                              <h3 className="text-2xl font-bold text-gray-900 mb-1">
                                {rec.university.name}
                              </h3>
                              <div className="flex items-center gap-2 text-gray-600">
                                <MapPin className="w-4 h-4" />
                                <span>{rec.university.city}, {rec.university.country}</span>
                              </div>
                            </div>
                            <div className={`px-4 py-2 rounded-lg font-bold text-lg ${getScoreColor(rec.matchScore)}`}>
                              {rec.matchScore}%
                            </div>
                          </div>

                          {/* Звездный рейтинг */}
                          <div className="flex items-center gap-1 mb-3">
                            {getScoreStars(rec.matchScore)}
                            <span className="ml-2 text-sm text-gray-600">
                              Совпадение: {rec.matchScore}%
                            </span>
                          </div>

                          {/* Стоимость обучения */}
                          {rec.university.tuitionFee && (
                            <div className="flex items-center gap-2 mb-4">
                              <DollarSign className="w-5 h-5 text-green-600" />
                              <span className="text-gray-900 font-semibold">
                                ${rec.university.tuitionFee.toLocaleString()} / год
                              </span>
                            </div>
                          )}

                          {/* Причины рекомендации */}
                          {rec.reasons && rec.reasons.length > 0 && (
                            <div className="mb-4">
                              <p className="text-sm font-semibold text-gray-700 mb-2">
                                Почему мы рекомендуем:
                              </p>
                              <div className="space-y-1">
                                {rec.reasons.map((reason, idx) => (
                                  <div key={idx} className="flex items-start gap-2">
                                    <CheckCircle className="w-4 h-4 text-green-600 flex-shrink-0 mt-0.5" />
                                    <span className="text-sm text-gray-700">{reason}</span>
                                  </div>
                                ))}
                              </div>
                            </div>
                          )}

                          {/* Требования к поступлению */}
                          {(rec.university.minScore || rec.admissionProbability) && (
                            <div className="p-3 bg-blue-50 rounded-lg mb-4">
                              <div className="flex items-start gap-2">
                                <Award className="w-5 h-5 text-blue-600 flex-shrink-0 mt-0.5" />
                                <div>
                                  <p className="text-sm font-semibold text-gray-900 mb-1">
                                    Требования к поступлению:
                                  </p>
                                  <p className="text-sm text-gray-700">
                                    {rec.university.minScore && `Минимальный балл: ${rec.university.minScore}`}
                                    {rec.admissionProbability && ` • Вероятность поступления: ${rec.admissionProbability}%`}
                                  </p>
                                </div>
                              </div>
                            </div>
                          )}

                          {/* Кнопки действий */}
                          <div className="flex items-center gap-3">
                            <Button
                              variant={selectedUniversity === rec.university.id ? "secondary" : "primary"}
                              size="sm"
                              onClick={() => handleShowExplanation(rec.university.id)}
                              disabled={loadingExplanation}
                            >
                              {loadingExplanation && selectedUniversity === rec.university.id ? (
                                <>
                                  <div className="w-4 h-4 mr-2 border-2 border-white border-t-transparent rounded-full animate-spin" />
                                  Загрузка...
                                </>
                              ) : (
                                <>
                                  <Sparkles className="w-4 h-4 mr-2" />
                                  {selectedUniversity === rec.university.id ? 'Скрыть объяснение' : 'Подробное объяснение'}
                                </>
                              )}
                            </Button>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleOpenUniversityPage(rec.university.id, rec.university.name)}
                            >
                              <ExternalLink className="w-4 h-4 mr-2" />
                              Страница вуза
                            </Button>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Развернутое объяснение */}
                  {selectedUniversity === rec.university.id && explanation && (
                    <motion.div
                      initial={{ opacity: 0, height: 0 }}
                      animate={{ opacity: 1, height: 'auto' }}
                      className="mt-4 p-4 bg-gradient-to-r from-blue-50 to-purple-50 rounded-lg border-t border-gray-200"
                    >
                      <div className="flex items-start gap-2">
                        <TrendingUp className="w-5 h-5 text-blue-600 flex-shrink-0 mt-1" />
                        <div>
                          <p className="font-semibold text-gray-900 mb-2">
                            Детальный анализ совпадения:
                          </p>
                          <p className="text-sm text-gray-700 whitespace-pre-line">
                            {explanation}
                          </p>
                        </div>
                      </div>
                    </motion.div>
                  )}
                </Card>
              </motion.div>
            ))}
          </div>
        )}

        {/* Обновить профиль */}
        {recommendations.length > 0 && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ delay: 0.3 }}
            className="mt-8"
          >
            <Card className="bg-gradient-to-r from-orange-100 to-pink-100">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-bold text-gray-900 mb-1">
                    Хотите улучшить рекомендации?
                  </h3>
                  <p className="text-sm text-gray-600">
                    Обновите свои предпочтения и цели в профиле
                  </p>
                </div>
                <Button variant="primary" onClick={() => navigate('/dashboard')}>
                  Обновить профиль
                </Button>
              </div>
            </Card>
          </motion.div>
        )}
      </div>
    </div>
  );
};

export default UniversityRecommendationsPage;
