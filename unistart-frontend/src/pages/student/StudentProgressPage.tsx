import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  TrendingUp,
  Award,
  BookOpen,
  Target,
  Clock,
  BarChart3,
  CheckCircle,
} from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import api from '../../services/api';

interface ProgressStats {
  totalCardsStudied: number; // ReviewedCards
  masteredCards: number; // НОВОЕ: освоенные карточки
  completedFlashcardSets: number; // НОВОЕ: завершенные наборы
  totalQuizzesTaken: number;
  averageQuizScore: number;
  totalTimeSpent: number;
  currentStreak: number;
  longestStreak: number;
  totalAchievements: number;
}

interface RecentActivity {
  id: number;
  type: 'quiz' | 'flashcard' | 'achievement';
  title: string;
  score?: number;
  date: string;
}

interface SubjectProgress {
  subject: string;
  quizzesTaken: number;
  averageScore: number;
  cardsStudied: number;
  masteredCards?: number; // НОВОЕ: освоенные карточки
}

const StudentProgressPage = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState<ProgressStats | null>(null);
  const [recentActivity, setRecentActivity] = useState<RecentActivity[]>([]);
  const [subjectProgress, setSubjectProgress] = useState<SubjectProgress[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadProgress();
  }, []);

  const loadProgress = async () => {
    try {
      const response = await api.get('/student/progress');
      setStats(response.data.stats);
      setRecentActivity(response.data.recentActivity || []);
      setSubjectProgress(response.data.subjectProgress || []);
    } catch (error) {
      console.error('Ошибка загрузки прогресса:', error);
      // Моковые данные
      setStats({
        totalCardsStudied: 125,
        masteredCards: 45,
        completedFlashcardSets: 3,
        totalQuizzesTaken: 8,
        averageQuizScore: 78.5,
        totalTimeSpent: 3600,
        currentStreak: 3,
        longestStreak: 7,
        totalAchievements: 4,
      });
      setRecentActivity([
        {
          id: 1,
          type: 'quiz',
          title: 'Математика - Алгебра',
          score: 85,
          date: new Date().toISOString(),
        },
        {
          id: 2,
          type: 'flashcard',
          title: 'Изучено 15 карточек по Физике',
          date: new Date(Date.now() - 86400000).toISOString(),
        },
        {
          id: 3,
          type: 'achievement',
          title: 'Получено достижение "Новичок"',
          date: new Date(Date.now() - 172800000).toISOString(),
        },
      ]);
      setSubjectProgress([
        { subject: 'Математика', quizzesTaken: 3, averageScore: 82, cardsStudied: 45 },
        { subject: 'Физика', quizzesTaken: 2, averageScore: 75, cardsStudied: 38 },
        { subject: 'Химия', quizzesTaken: 2, averageScore: 76, cardsStudied: 28 },
        { subject: 'Биология', quizzesTaken: 1, averageScore: 85, cardsStudied: 14 },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const formatTime = (seconds: number) => {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    return `${hours}ч ${minutes}м`;
  };

  const getScoreColor = (score: number) => {
    if (score >= 80) return 'text-green-600';
    if (score >= 60) return 'text-yellow-600';
    return 'text-red-600';
  };

  const getScoreBgColor = (score: number) => {
    if (score >= 80) return 'bg-green-100';
    if (score >= 60) return 'bg-yellow-100';
    return 'bg-red-100';
  };

  const getActivityIcon = (type: string) => {
    switch (type) {
      case 'quiz':
        return Target;
      case 'flashcard':
        return BookOpen;
      case 'achievement':
        return Award;
      default:
        return CheckCircle;
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка прогресса...</div>
      </div>
    );
  }

  if (!stats) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Нет данных о прогрессе</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <Button
            variant="secondary"
            onClick={() => navigate('/dashboard')}
            className="mb-4"
          >
            ← Назад к панели
          </Button>
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-4xl font-bold text-gray-900 mb-2">
                📈 Мой прогресс
              </h1>
              <p className="text-gray-600">
                Отслеживайте свои достижения и улучшайте результаты
              </p>
            </div>
          </div>
        </motion.div>

        {/* Main Stats */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8"
        >
            <Card className="p-6 bg-gradient-to-br from-blue-500 to-blue-600 text-white">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-blue-100 text-sm mb-1">Просмотрено карточек</p>
                  <p className="text-3xl font-bold">{stats.totalCardsStudied}</p>
                  <p className="text-xs text-blue-200 mt-1">Освоено: {stats.masteredCards}</p>
                </div>
                <BookOpen className="w-12 h-12 text-blue-200" />
              </div>
            </Card>

          <Card className="p-6 bg-gradient-to-br from-green-500 to-green-600 text-white">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-green-100 text-sm mb-1">Пройдено тестов</p>
                <p className="text-3xl font-bold">{stats.totalQuizzesTaken}</p>
              </div>
              <Target className="w-12 h-12 text-green-200" />
            </div>
          </Card>

          <Card className="p-6 bg-gradient-to-br from-purple-500 to-purple-600 text-white">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-purple-100 text-sm mb-1">Средний балл</p>
                <p className="text-3xl font-bold">
                  {stats.averageQuizScore.toFixed(0)}%
                </p>
              </div>
              <TrendingUp className="w-12 h-12 text-purple-200" />
            </div>
          </Card>

          <Card className="p-6 bg-gradient-to-br from-orange-500 to-orange-600 text-white">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-orange-100 text-sm mb-1">Достижений</p>
                <p className="text-3xl font-bold">{stats.totalAchievements}</p>
              </div>
              <Award className="w-12 h-12 text-orange-200" />
            </div>
          </Card>
        </motion.div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Left Column */}
          <div className="lg:col-span-2 space-y-8">
            {/* Subject Progress */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.2 }}
            >
              <Card className="p-6">
                <h2 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
                  <BarChart3 className="w-6 h-6 text-primary-500" />
                  Прогресс по предметам
                </h2>
                <div className="space-y-6">
                  {subjectProgress.map((subject, index) => (
                    <motion.div
                      key={subject.subject}
                      initial={{ opacity: 0, x: -20 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: 0.2 + index * 0.1 }}
                    >
                      <div className="flex items-center justify-between mb-2">
                        <h3 className="font-semibold text-gray-900">
                          {subject.subject}
                        </h3>
                        <span
                          className={`text-sm font-bold ${getScoreColor(
                            subject.averageScore
                          )}`}
                        >
                          {subject.averageScore}%
                        </span>
                      </div>
                      <div className="flex items-center gap-4 text-sm text-gray-600 mb-2">
                        <span>📝 Тестов: {subject.quizzesTaken}</span>
                        <span>📚 Карточек: {subject.cardsStudied}</span>
                        {subject.masteredCards !== undefined && (
                          <span>✅ Освоено: {subject.masteredCards}</span>
                        )}
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-3">
                        <motion.div
                          initial={{ width: 0 }}
                          animate={{ width: `${subject.averageScore}%` }}
                          transition={{ duration: 1, delay: 0.3 + index * 0.1 }}
                          className={`${getScoreBgColor(
                            subject.averageScore
                          )} h-3 rounded-full`}
                        />
                      </div>
                    </motion.div>
                  ))}
                </div>
              </Card>
            </motion.div>

            {/* Recent Activity */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3 }}
            >
              <Card className="p-6">
                <h2 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
                  <Clock className="w-6 h-6 text-primary-500" />
                  Недавняя активность
                </h2>
                <div className="space-y-4">
                  {recentActivity.map((activity, index) => {
                    const ActivityIcon = getActivityIcon(activity.type);
                    return (
                      <motion.div
                        key={activity.id}
                        initial={{ opacity: 0, x: -20 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ delay: 0.3 + index * 0.1 }}
                        className="flex items-start gap-4 pb-4 border-b last:border-0"
                      >
                        <div className="bg-primary-100 p-3 rounded-lg">
                          <ActivityIcon className="w-5 h-5 text-primary-600" />
                        </div>
                        <div className="flex-1">
                          <p className="font-medium text-gray-900">
                            {activity.title}
                          </p>
                          <div className="flex items-center gap-3 mt-1">
                            <span className="text-xs text-gray-500">
                              {new Date(activity.date).toLocaleDateString(
                                'ru-RU'
                              )}
                            </span>
                            {activity.score && (
                              <span
                                className={`text-xs font-bold ${getScoreColor(
                                  activity.score
                                )}`}
                              >
                                {activity.score}%
                              </span>
                            )}
                          </div>
                        </div>
                      </motion.div>
                    );
                  })}
                </div>
              </Card>
            </motion.div>
          </div>

          {/* Right Column */}
          <div className="space-y-8">
            {/* Streak */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.4 }}
            >
              <Card className="p-6 bg-gradient-to-br from-red-500 to-orange-500 text-white">
                <div className="text-center">
                  <div className="text-6xl mb-4">🔥</div>
                  <p className="text-orange-100 text-sm mb-1">
                    Текущая серия
                  </p>
                  <p className="text-4xl font-bold mb-2">
                    {stats.currentStreak} дней
                  </p>
                  <p className="text-orange-100 text-xs">
                    Рекорд: {stats.longestStreak} дней
                  </p>
                </div>
              </Card>
            </motion.div>

            {/* Time Spent */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.5 }}
            >
              <Card className="p-6">
                <div className="flex items-center gap-4 mb-4">
                  <div className="bg-blue-100 p-3 rounded-lg">
                    <Clock className="w-6 h-6 text-blue-600" />
                  </div>
                  <div>
                    <p className="text-gray-600 text-sm">Время обучения</p>
                    <p className="text-2xl font-bold text-gray-900">
                      {formatTime(stats.totalTimeSpent)}
                    </p>
                  </div>
                </div>
              </Card>
            </motion.div>

            {/* Quick Actions */}
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.6 }}
            >
              <Card className="p-6">
                <h3 className="text-lg font-bold text-gray-900 mb-4">
                  Быстрые действия
                </h3>
                <div className="space-y-3">
                  <Button
                    variant="primary"
                    className="w-full"
                    onClick={() => navigate('/quizzes')}
                  >
                    Пройти тест
                  </Button>
                  <Button
                    variant="secondary"
                    className="w-full"
                    onClick={() => navigate('/flashcards')}
                  >
                    Изучить карточки
                  </Button>
                  <Button
                    variant="secondary"
                    className="w-full"
                    onClick={() => navigate('/achievements')}
                  >
                    Мои достижения
                  </Button>
                </div>
              </Card>
            </motion.div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default StudentProgressPage;
