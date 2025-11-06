import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  Users,
  FileText,
  Award,
  BookOpen,
  ArrowLeft,
  Target,
  BarChart3,
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface OverviewStats {
  totalQuizzes: number;
  publicQuizzes: number;
  privateQuizzes: number;
  totalQuestions: number;
  totalAttempts: number;
  uniqueStudents: number;
  averageStudentScore: number;
}

interface QuizStats {
  quizId: number;
  quizTitle: string;
  questionCount: number;
  totalAttempts: number;
  uniqueUsers: number;
  averageScore: number;
  averagePercentage: number;
  highestScore: number;
  lowestScore: number;
  passRate: number;
}

const TeacherAnalyticsPage = () => {
  const navigate = useNavigate();
  const [overviewStats, setOverviewStats] = useState<OverviewStats | null>(null);
  const [quizzes, setQuizzes] = useState<QuizStats[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadAnalytics();
  }, []);

  const loadAnalytics = async () => {
    try {
      // Загружаем общую статистику
      const overviewResponse = await api.get('/teacher/stats/overview');
      setOverviewStats(overviewResponse.data);

      // Загружаем свои квизы
      const quizzesResponse = await api.get('/teacher/quizzes/my');
      const quizzesData = quizzesResponse.data;

      // Для каждого квиза загружаем детальную статистику
      const quizStatsPromises = quizzesData.map(async (quiz: any) => {
        try {
          const statsResponse = await api.get(`/teacher/quizzes/${quiz.id}/stats`);
          return statsResponse.data;
        } catch (error) {
          console.error(`Ошибка загрузки статистики для квиза ${quiz.id}:`, error);
          return null;
        }
      });

      const quizStats = (await Promise.all(quizStatsPromises)).filter(Boolean);
      setQuizzes(quizStats);
    } catch (error) {
      console.error('Ошибка загрузки аналитики:', error);
    } finally {
      setLoading(false);
    }
  };

  const getPassRateColor = (passRate: number) => {
    if (passRate >= 80) return 'text-green-600';
    if (passRate >= 60) return 'text-yellow-600';
    return 'text-red-600';
  };

  const getScoreColor = (percentage: number) => {
    if (percentage >= 80) return 'bg-green-100 text-green-800';
    if (percentage >= 60) return 'bg-yellow-100 text-yellow-800';
    return 'bg-red-100 text-red-800';
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка аналитики...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 py-8 px-4">
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
            className="mb-4 flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Назад к панели
          </Button>
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-4xl font-bold text-gray-900 mb-2">
                📊 Аналитика и статистика
              </h1>
              <p className="text-gray-600">
                Детальная информация о ваших тестах и успеваемости студентов
              </p>
            </div>
          </div>
        </motion.div>

        {/* Overview Stats */}
        {overviewStats && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8"
          >
            <Card className="p-6 bg-gradient-to-br from-blue-500 to-blue-600 text-white">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-blue-100 text-sm mb-1">Всего тестов</p>
                  <p className="text-3xl font-bold">{overviewStats.totalQuizzes}</p>
                  <p className="text-blue-100 text-xs mt-1">
                    Публичных: {overviewStats.publicQuizzes}
                  </p>
                </div>
                <FileText className="w-12 h-12 text-blue-200" />
              </div>
            </Card>

            <Card className="p-6 bg-gradient-to-br from-green-500 to-green-600 text-white">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-green-100 text-sm mb-1">Студентов</p>
                  <p className="text-3xl font-bold">{overviewStats.uniqueStudents}</p>
                  <p className="text-green-100 text-xs mt-1">
                    Попыток: {overviewStats.totalAttempts}
                  </p>
                </div>
                <Users className="w-12 h-12 text-green-200" />
              </div>
            </Card>

            <Card className="p-6 bg-gradient-to-br from-purple-500 to-purple-600 text-white">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-purple-100 text-sm mb-1">Вопросов создано</p>
                  <p className="text-3xl font-bold">{overviewStats.totalQuestions}</p>
                </div>
                <BookOpen className="w-12 h-12 text-purple-200" />
              </div>
            </Card>

            <Card className="p-6 bg-gradient-to-br from-yellow-500 to-yellow-600 text-white">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-yellow-100 text-sm mb-1">Средний балл</p>
                  <p className="text-3xl font-bold">
                    {overviewStats.averageStudentScore.toFixed(1)}%
                  </p>
                </div>
                <Award className="w-12 h-12 text-yellow-200" />
              </div>
            </Card>
          </motion.div>
        )}

        {/* Quiz Statistics Table */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
        >
          <Card className="p-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                <BarChart3 className="w-6 h-6 text-primary-500" />
                Статистика по тестам
              </h2>
            </div>

            {quizzes.length === 0 ? (
              <div className="text-center py-12">
                <FileText className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                <p className="text-gray-500 text-lg mb-2">
                  У вас пока нет статистики по тестам
                </p>
                <p className="text-gray-400 mb-6">
                  Создайте тесты и дождитесь, пока студенты начнут их проходить
                </p>
                <Button onClick={() => navigate('/quizzes/create')}>
                  Создать тест
                </Button>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Тест
                      </th>
                      <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Попытки
                      </th>
                      <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Студентов
                      </th>
                      <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Средний балл
                      </th>
                      <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Процент сдачи
                      </th>
                      <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Диапазон баллов
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {quizzes.map((quiz) => (
                      <motion.tr
                        key={quiz.quizId}
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        className="hover:bg-gray-50 transition-colors"
                      >
                        <td className="px-6 py-4">
                          <div>
                            <div className="text-sm font-medium text-gray-900">
                              {quiz.quizTitle}
                            </div>
                            <div className="text-xs text-gray-500">
                              Вопросов: {quiz.questionCount}
                            </div>
                          </div>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <span className="text-sm font-medium text-gray-900">
                            {quiz.totalAttempts}
                          </span>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <div className="flex items-center justify-center gap-1">
                            <Users className="w-4 h-4 text-gray-400" />
                            <span className="text-sm font-medium text-gray-900">
                              {quiz.uniqueUsers}
                            </span>
                          </div>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <span
                            className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${getScoreColor(
                              quiz.averagePercentage
                            )}`}
                          >
                            {quiz.averagePercentage.toFixed(1)}%
                          </span>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <span
                            className={`text-sm font-bold ${getPassRateColor(
                              quiz.passRate
                            )}`}
                          >
                            {quiz.passRate.toFixed(0)}%
                          </span>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <div className="text-sm text-gray-600">
                            {quiz.lowestScore} - {quiz.highestScore}
                          </div>
                        </td>
                      </motion.tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </Card>
        </motion.div>

        {/* Quick Actions */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-6"
        >
          <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer">
            <div
              onClick={() => navigate('/teacher/students')}
              className="text-center"
            >
              <Users className="w-12 h-12 text-primary-500 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Студенты
              </h3>
              <p className="text-gray-600 text-sm">
                Просмотр списка студентов и их прогресса
              </p>
            </div>
          </Card>

          <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer">
            <div
              onClick={() => navigate('/teacher/export')}
              className="text-center"
            >
              <FileText className="w-12 h-12 text-green-500 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Экспорт данных
              </h3>
              <p className="text-gray-600 text-sm">
                Скачать результаты в CSV формате
              </p>
            </div>
          </Card>

          <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer">
            <div
              onClick={() => navigate('/quizzes/create')}
              className="text-center"
            >
              <Target className="w-12 h-12 text-blue-500 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 mb-2">
                Создать тест
              </h3>
              <p className="text-gray-600 text-sm">
                Добавить новый тест для студентов
              </p>
            </div>
          </Card>
        </motion.div>
      </div>
    </div>
  );
};

export default TeacherAnalyticsPage;
