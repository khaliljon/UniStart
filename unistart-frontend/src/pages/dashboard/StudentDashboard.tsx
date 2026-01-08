import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { BookOpen, TrendingUp, Award, Calendar, Clock, Target } from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import AIStatsWidget from '../../components/AIStatsWidget';
import { flashcardService } from '../../services/flashcardService';
import { quizService } from '../../services/quizService';
import { FlashcardSet, Quiz } from '../../types';

interface Stats {
  totalCardsStudied: number;
  totalQuizzesTaken: number;
  cardsToReviewToday: number;
  averageScore: number;
}

const StudentDashboard = () => {
  const navigate = useNavigate();
  const [flashcardSets, setFlashcardSets] = useState<FlashcardSet[]>([]);
  const [quizzes, setQuizzes] = useState<Quiz[]>([]);
  const [stats, setStats] = useState<Stats>({
    totalCardsStudied: 0,
    totalQuizzesTaken: 0,
    cardsToReviewToday: 0,
    averageScore: 85,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      const [setsData, quizzesData] = await Promise.all([
        flashcardService.getSets(),
        quizService.getQuizzes(),
      ]);

      setFlashcardSets(Array.isArray(setsData) ? setsData.slice(0, 3) : []);
      setQuizzes(Array.isArray(quizzesData) ? quizzesData.slice(0, 3) : []);

      // Подсчитываем карточки для повторения
      const cardsToReview = Array.isArray(setsData) 
        ? setsData.reduce((sum: number, set: FlashcardSet) => sum + (set.cardsToReview || 0), 0)
        : 0;

      setStats((prev) => ({
        ...prev,
        cardsToReviewToday: cardsToReview,
      }));
    } catch (error) {
      console.error('Ошибка загрузки данных:', error);
    } finally {
      setLoading(false);
    }
  };

  const statCards = [
    {
      icon: BookOpen,
      label: 'Карточек изучено',
      value: stats.totalCardsStudied,
      color: 'bg-blue-500',
    },
    {
      icon: Award,
      label: 'Квизов пройдено',
      value: stats.totalQuizzesTaken,
      color: 'bg-green-500',
    },
    {
      icon: Calendar,
      label: 'К повторению сегодня',
      value: stats.cardsToReviewToday,
      color: 'bg-orange-500',
    },
    {
      icon: TrendingUp,
      label: 'Средний балл',
      value: `${stats.averageScore}%`,
      color: 'bg-purple-500',
    },
  ];

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-xl text-gray-600">Загрузка...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Заголовок */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <h1 className="text-4xl font-bold text-gray-900 mb-2">
            Панель управления
          </h1>
          <p className="text-gray-600">
            Добро пожаловать! Вот ваш прогресс обучения
          </p>
        </motion.div>

        {/* Статистика */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          {statCards.map((stat, index) => (
            <motion.div
              key={stat.label}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.1 }}
            >
              <Card className="p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600 mb-1">{stat.label}</p>
                    <p className="text-3xl font-bold text-gray-900">
                      {stat.value}
                    </p>
                  </div>
                  <div className={`${stat.color} p-4 rounded-lg`}>
                    <stat.icon className="w-8 h-8 text-white" />
                  </div>
                </div>
              </Card>
            </motion.div>
          ))}
        </div>

        {/* Карточки для повторения */}
        {stats.cardsToReviewToday > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="mb-12"
          >
            <Card className="p-6 bg-gradient-to-r from-orange-500 to-red-500 text-white">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-4">
                  <Clock className="w-12 h-12" />
                  <div>
                    <h3 className="text-2xl font-bold mb-1">
                      {stats.cardsToReviewToday} карточек ждут повторения!
                    </h3>
                    <p className="opacity-90">
                      Повторите сегодня, чтобы не забыть материал
                    </p>
                  </div>
                </div>
                <Button
                  variant="secondary"
                  onClick={() => navigate('/flashcards')}
                  className="bg-white text-orange-500 hover:bg-gray-100"
                >
                  Начать повторение
                </Button>
              </div>
            </Card>
          </motion.div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Наборы карточек */}
          <motion.div
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.2 }}
            className="lg:col-span-2"
          >
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                <BookOpen className="w-6 h-6 text-primary-500" />
                Наборы карточек
              </h2>
              <Button
                variant="secondary"
                onClick={() => navigate('/flashcards')}
              >
                Все наборы
              </Button>
            </div>

            <div className="space-y-4">
              {flashcardSets.length === 0 ? (
                <Card className="p-8 text-center">
                  <p className="text-gray-600">
                    Пока нет доступных наборов карточек
                  </p>
                </Card>
              ) : (
                flashcardSets.map((set, index) => (
                  <motion.div
                    key={set.id}
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.3 + index * 0.1 }}
                  >
                    <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer">
                      <div className="flex items-start justify-between">
                        <div className="flex-1">
                          <h3 className="text-lg font-semibold text-gray-900 mb-2">
                            {set.title}
                          </h3>
                          <p className="text-sm text-gray-600 mb-4">
                            {set.description}
                          </p>
                          <div className="flex items-center gap-4 text-sm">
                            <div className="flex items-center gap-1 text-gray-600">
                              <BookOpen className="w-4 h-4" />
                              <span>{set.totalCards || 0} карточек</span>
                            </div>
                            {(set.cardsToReview || 0) > 0 && (
                              <div className="flex items-center gap-1 text-orange-500 font-medium">
                                <Target className="w-4 h-4" />
                                <span>{set.cardsToReview} на повторение</span>
                              </div>
                            )}
                          </div>
                        </div>
                        <Button
                          onClick={() => navigate(`/flashcards/${set.id}/study`)}
                          className="ml-4"
                        >
                          Учить
                        </Button>
                      </div>
                    </Card>
                  </motion.div>
                ))
              )}
            </div>
          </motion.div>

          {/* AI виджет и рекомендованные квизы */}
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.2 }}
            className="space-y-6"
          >
            {/* AI статистика */}
            <AIStatsWidget />

            {/* Рекомендованные квизы */}
            <div>
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-xl font-bold text-gray-900 flex items-center gap-2">
                  <Award className="w-5 h-5 text-primary-500" />
                  Квизы
                </h2>
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={() => navigate('/quizzes')}
                >
                  Все
                </Button>
              </div>

              <div className="space-y-3">
              {quizzes.length === 0 ? (
                <Card className="p-8 text-center">
                  <p className="text-gray-600">Пока нет доступных квизов</p>
                </Card>
              ) : (
                quizzes.map((quiz, index) => (
                  <motion.div
                    key={quiz.id}
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.3 + index * 0.1 }}
                  >
                    <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer">
                      <div className="flex items-start justify-between">
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-2">
                            <h3 className="text-lg font-semibold text-gray-900">
                              {quiz.title}
                            </h3>
                            <span
                              className={`px-2 py-1 rounded text-xs font-medium ${
                                quiz.difficulty === 'Easy'
                                  ? 'bg-green-100 text-green-700'
                                  : quiz.difficulty === 'Medium'
                                  ? 'bg-yellow-100 text-yellow-700'
                                  : 'bg-red-100 text-red-700'
                              }`}
                            >
                              {quiz.difficulty === 'Easy'
                                ? 'Легко'
                                : quiz.difficulty === 'Medium'
                                ? 'Средне'
                                : 'Сложно'}
                            </span>
                          </div>
                          <p className="text-sm text-gray-600 mb-4">
                            {quiz.description}
                          </p>
                          <div className="flex items-center gap-4 text-sm text-gray-600">
                            <div className="flex items-center gap-1">
                              <BookOpen className="w-4 h-4" />
                              <span>{quiz.subject}</span>
                            </div>
                            {quiz.timeLimit && (
                              <div className="flex items-center gap-1">
                                <Clock className="w-4 h-4" />
                                <span>{quiz.timeLimit} мин</span>
                              </div>
                            )}
                          </div>
                        </div>
                        <Button
                          onClick={() => navigate(`/quizzes/${quiz.id}`)}
                          className="ml-4"
                        >
                          Начать
                        </Button>
                      </div>
                    </Card>
                  </motion.div>
                ))
              )}
              </div>
            </div>
          </motion.div>
        </div>

        {/* График прогресса (заглушка) */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="mt-12"
        >
          <h2 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
            <TrendingUp className="w-6 h-6 text-primary-500" />
            Мой прогресс
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer">
              <div
                onClick={() => navigate('/student/progress')}
                className="text-center"
              >
                <TrendingUp className="w-12 h-12 text-green-500 mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  Детальный прогресс
                </h3>
                <p className="text-gray-600 text-sm">
                  Смотрите статистику по предметам и активности
                </p>
              </div>
            </Card>

            <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer">
              <div
                onClick={() => navigate('/student/achievements')}
                className="text-center"
              >
                <Award className="w-12 h-12 text-yellow-500 mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  Мои достижения
                </h3>
                <p className="text-gray-600 text-sm">
                  Разблокируйте награды за успехи в обучении
                </p>
              </div>
            </Card>

            <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer">
              <div
                onClick={() => navigate('/student/leaderboard')}
                className="text-center"
              >
                <Target className="w-12 h-12 text-purple-500 mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  Таблица лидеров
                </h3>
                <p className="text-gray-600 text-sm">
                  Соревнуйтесь с другими студентами
                </p>
              </div>
            </Card>
          </div>
        </motion.div>
      </div>
    </div>
  );
}

export default StudentDashboard
