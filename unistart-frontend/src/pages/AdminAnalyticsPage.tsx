import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  Users,
  FileText,
  BookOpen,
  Activity,
  TrendingUp,
  Award,
  ArrowLeft,
  Calendar,
  BarChart3,
  Target,
  Settings,
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface PlatformStats {
  totalUsers: number;
  totalQuizzes: number;
  totalTests: number;
  totalFlashcardSets: number;
  totalFlashcards: number;
  totalAttempts: number;
  activeToday: number;
  activeThisWeek: number;
  activeThisMonth: number;
  averageQuizScore: number;
  totalAchievements: number;
}

const AdminAnalyticsPage = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState<PlatformStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadAnalytics();
  }, []);

  const loadAnalytics = async () => {
    try {
      setLoading(true);
      const response = await api.get('/admin/analytics');
      console.log('=== Analytics API Response ===');
      console.log('Full response:', JSON.stringify(response.data, null, 2));
      
      // Данные возвращаются в объекте stats (camelCase) - JSON сериализатор преобразует Stats -> stats
      // response.data = { stats: { totalUsers: 4, ... } }
      const statsData = response.data?.stats || response.data?.Stats || response.data;
      console.log('Stats data object:', statsData);
      console.log('TotalUsers:', statsData?.totalUsers, statsData?.TotalUsers);
      
      if (!statsData) {
        console.error('Stats data is null or undefined!');
        throw new Error('Stats data не найдены в ответе API');
      }
      
      // Читаем данные в camelCase (как они приходят с бэкенда)
      const newStats: PlatformStats = {
        totalUsers: Number(statsData.totalUsers ?? statsData.TotalUsers ?? 0),
        totalQuizzes: Number(statsData.totalQuizzes ?? statsData.TotalQuizzes ?? 0),
        totalTests: Number(statsData.totalExams ?? statsData.TotalExams ?? 0),
        totalFlashcardSets: Number(statsData.totalFlashcardSets ?? statsData.TotalFlashcardSets ?? 0),
        totalFlashcards: Number(statsData.totalFlashcards ?? statsData.TotalFlashcards ?? 0),
        totalAttempts: Number(statsData.totalAttempts ?? statsData.TotalAttempts ?? 0),
        activeToday: Number(statsData.activeToday ?? statsData.ActiveToday ?? 0),
        activeThisWeek: Number(statsData.activeThisWeek ?? statsData.ActiveThisWeek ?? 0),
        activeThisMonth: Number(statsData.activeThisMonth ?? statsData.ActiveThisMonth ?? 0),
        averageQuizScore: Number(statsData.averageQuizScore ?? statsData.AverageQuizScore ?? 0),
        totalAchievements: Number(statsData.totalAchievements ?? statsData.TotalAchievements ?? 0),
      };
      
      console.log('Final parsed stats:', newStats);
      setStats(newStats);
    } catch (error: any) {
      console.error('Ошибка загрузки аналитики:', error);
      console.error('Error details:', error.response?.data);
      console.error('Error status:', error.response?.status);
      // Показываем сообщение об ошибке, но не обнуляем все данные
      alert(`Ошибка загрузки аналитики: ${error.response?.data?.message || error.message}`);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка аналитики...</div>
      </div>
    );
  }

  if (!stats) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Нет данных для отображения</div>
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
            className="mb-4 flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Назад к панели
          </Button>
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-4xl font-bold text-gray-900 mb-2">
                📊 Аналитика платформы
              </h1>
              <p className="text-gray-600">
                Полная статистика использования UniStart
              </p>
            </div>
          </div>
        </motion.div>

        {/* Main Stats Grid */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6 mb-8"
        >
          <Card className="p-6 bg-gradient-to-br from-blue-500 to-blue-600 text-white">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-blue-100 text-sm mb-1">Всего пользователей</p>
                <p className="text-3xl font-bold">{stats.totalUsers}</p>
                <p className="text-blue-100 text-xs mt-1">
                  Активных сегодня: {stats.activeToday}
                </p>
              </div>
              <Users className="w-12 h-12 text-blue-200" />
            </div>
          </Card>

          <Card className="p-6 bg-gradient-to-br from-green-500 to-green-600 text-white">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-green-100 text-sm mb-1">Квизов</p>
                <p className="text-3xl font-bold">{stats.totalQuizzes}</p>
              </div>
              <FileText className="w-12 h-12 text-green-200" />
            </div>
          </Card>

          <Card className="p-6 bg-gradient-to-br from-indigo-500 to-indigo-600 text-white">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-indigo-100 text-sm mb-1">Экзаменов</p>
                <p className="text-3xl font-bold">{stats.totalTests}</p>
              </div>
              <ClipboardCheck className="w-12 h-12 text-indigo-200" />
            </div>
          </Card>

          <Card className="p-6 bg-gradient-to-br from-purple-500 to-purple-600 text-white">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-purple-100 text-sm mb-1">Наборов карточек</p>
                <p className="text-3xl font-bold">{stats.totalFlashcardSets}</p>
                <p className="text-purple-100 text-xs mt-1">
                  Карточек: {stats.totalFlashcards}
                </p>
              </div>
              <BookOpen className="w-12 h-12 text-purple-200" />
            </div>
          </Card>

          <Card className="p-6 bg-gradient-to-br from-orange-500 to-orange-600 text-white">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-orange-100 text-sm mb-1">Достижений</p>
                <p className="text-3xl font-bold">{stats.totalAchievements}</p>
                <p className="text-orange-100 text-xs mt-1">
                  Попыток прохождения: {stats.totalAttempts}
                </p>
              </div>
              <Award className="w-12 h-12 text-orange-200" />
            </div>
          </Card>
        </motion.div>

        {/* Activity Stats */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
          className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8"
        >
          <Card className="p-6">
            <div className="flex items-center gap-4">
              <div className="bg-blue-100 dark:bg-blue-900/30 p-3 rounded-lg">
                <Activity className="w-6 h-6 text-blue-600 dark:text-blue-400" />
              </div>
              <div>
                <p className="text-gray-600 dark:text-gray-400 text-sm">Активны сегодня</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                  {stats.activeToday}
                </p>
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <div className="flex items-center gap-4">
              <div className="bg-green-100 dark:bg-green-900/30 p-3 rounded-lg">
                <Calendar className="w-6 h-6 text-green-600 dark:text-green-400" />
              </div>
              <div>
                <p className="text-gray-600 dark:text-gray-400 text-sm">Активны за неделю</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                  {stats.activeThisWeek}
                </p>
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <div className="flex items-center gap-4">
              <div className="bg-purple-100 dark:bg-purple-900/30 p-3 rounded-lg">
                <TrendingUp className="w-6 h-6 text-purple-600 dark:text-purple-400" />
              </div>
              <div>
                <p className="text-gray-600 dark:text-gray-400 text-sm">Активны за месяц</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                  {stats.activeThisMonth}
                </p>
              </div>
            </div>
          </Card>
        </motion.div>

        {/* Content Stats */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8"
        >
          <Card className="p-6">
            <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center gap-2">
              <BarChart3 className="w-6 h-6 text-primary-500" />
              Статистика контента
            </h3>
            <div className="space-y-4">
              <div className="flex items-center justify-between py-3 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-center gap-3">
                  <FileText className="w-5 h-5 text-green-500" />
                  <span className="text-gray-700 dark:text-gray-300">Всего квизов</span>
                </div>
                <span className="text-xl font-bold text-gray-900 dark:text-white">
                  {stats.totalQuizzes}
                </span>
              </div>
              <div className="flex items-center justify-between py-3 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-center gap-3">
                  <FileText className="w-5 h-5 text-blue-500" />
                  <span className="text-gray-700 dark:text-gray-300">Всего экзаменов</span>
                </div>
                <span className="text-xl font-bold text-gray-900 dark:text-white">
                  {stats.totalTests}
                </span>
              </div>
              <div className="flex items-center justify-between py-3 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-center gap-3">
                  <BookOpen className="w-5 h-5 text-purple-500" />
                  <span className="text-gray-700 dark:text-gray-300">Наборов карточек</span>
                </div>
                <span className="text-xl font-bold text-gray-900 dark:text-white">
                  {stats.totalFlashcardSets}
                </span>
              </div>
              <div className="flex items-center justify-between py-3">
                <div className="flex items-center gap-3">
                  <BookOpen className="w-5 h-5 text-indigo-500" />
                  <span className="text-gray-700 dark:text-gray-300">Всего карточек</span>
                </div>
                <span className="text-xl font-bold text-gray-900 dark:text-white">
                  {stats.totalFlashcards}
                </span>
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-6 flex items-center gap-2">
              <TrendingUp className="w-6 h-6 text-primary-500" />
              Активность пользователей
            </h3>
            <div className="space-y-4">
              <div className="flex items-center justify-between py-3 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-center gap-3">
                  <Users className="w-5 h-5 text-blue-500" />
                  <span className="text-gray-700 dark:text-gray-300">Всего пользователей</span>
                </div>
                <span className="text-xl font-bold text-gray-900 dark:text-white">
                  {stats.totalUsers}
                </span>
              </div>
              <div className="flex items-center justify-between py-3 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-center gap-3">
                  <Activity className="w-5 h-5 text-green-500" />
                  <span className="text-gray-700 dark:text-gray-300">Попыток прохождения</span>
                </div>
                <span className="text-xl font-bold text-gray-900 dark:text-white">
                  {stats.totalAttempts}
                </span>
              </div>
              <div className="flex items-center justify-between py-3 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-center gap-3">
                  <Award className="w-5 h-5 text-yellow-500" />
                  <span className="text-gray-700 dark:text-gray-300">Средний балл</span>
                </div>
                <span className="text-xl font-bold text-gray-900 dark:text-white">
                  {stats.averageQuizScore.toFixed(1)}%
                </span>
              </div>
              <div className="flex items-center justify-between py-3">
                <div className="flex items-center gap-3">
                  <Award className="w-5 h-5 text-orange-500" />
                  <span className="text-gray-700 dark:text-gray-300">Достижений создано</span>
                </div>
                <span className="text-xl font-bold text-gray-900 dark:text-white">
                  {stats.totalAchievements}
                </span>
              </div>
            </div>
          </Card>
        </motion.div>

        {/* Quick Actions */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="grid grid-cols-1 md:grid-cols-3 gap-6"
        >
          <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer">
            <div
              onClick={() => navigate('/admin/users')}
              className="text-center"
            >
              <Users className="w-12 h-12 text-blue-500 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                Управление пользователями
              </h3>
              <p className="text-gray-600 dark:text-gray-400 text-sm">
                Просмотр и управление всеми пользователями
              </p>
            </div>
          </Card>

          <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer">
            <div
              onClick={() => navigate('/admin/export')}
              className="text-center"
            >
              <FileText className="w-12 h-12 text-green-500 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                Экспорт данных
              </h3>
              <p className="text-gray-600 dark:text-gray-400 text-sm">
                Скачать все данные платформы в CSV
              </p>
            </div>
          </Card>

          <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer">
            <div
              onClick={() => navigate('/admin/settings')}
              className="text-center"
            >
              <Settings className="w-12 h-12 text-purple-500 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                Настройки системы
              </h3>
              <p className="text-gray-600 dark:text-gray-400 text-sm">
                Конфигурация и параметры платформы
              </p>
            </div>
          </Card>
        </motion.div>
      </div>
    </div>
  );
};

export default AdminAnalyticsPage;
