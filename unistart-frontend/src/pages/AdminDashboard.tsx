import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Users, TrendingUp, Award, AlertCircle, FileText, Activity, BarChart3, Download, BookOpen, Globe, Building2, ClipboardList } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface AdminStats {
  totalUsers: number;
  totalQuizzes: number;
  totalTests: number;
  totalFlashcardSets: number;
  activeToday: number;
  activeThisWeek: number;
  activeThisMonth: number;
}

const AdminDashboard = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState<AdminStats>({
    totalUsers: 0,
    totalQuizzes: 0,
    totalTests: 0,
    totalFlashcardSets: 0,
    activeToday: 0,
    activeThisWeek: 0,
    activeThisMonth: 0,
  });
  const [users, setUsers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadAdminData();

    // Перезагружаем данные при возврате на страницу (когда окно получает фокус)
    const handleVisibilityChange = () => {
      if (!document.hidden) {
        loadAdminData();
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, []);

  const loadAdminData = async () => {
    try {
      // Загружаем аналитику и список пользователей
      const [analyticsResponse, usersResponse] = await Promise.allSettled([
        api.get('/admin/analytics'),
        api.get('/admin/users'),
      ]);

      console.log('Analytics response:', analyticsResponse);
      console.log('Users response:', usersResponse);

      // Обработка аналитики
      let analyticsData: any = {};
      if (analyticsResponse.status === 'fulfilled') {
        const data = analyticsResponse.value.data;
        // API может вернуть stats или Stats
        analyticsData = data.stats || data.Stats || data;
      }

      // Обработка пользователей
      let usersArray: any[] = [];
      if (usersResponse.status === 'fulfilled') {
        const data = usersResponse.value.data;
        usersArray = Array.isArray(data) 
          ? data 
          : (data.users || data.Users || []);
      }

      // Обновляем статистику из аналитики
      setStats({
        totalUsers: analyticsData.totalUsers || 0,
        totalQuizzes: analyticsData.totalQuizzes || 0,
        totalTests: analyticsData.totalTests || 0,
        totalFlashcardSets: analyticsData.totalFlashcardSets || 0,
        activeToday: analyticsData.activeToday || 0,
        activeThisWeek: analyticsData.activeThisWeek || 0,
        activeThisMonth: analyticsData.activeThisMonth || 0,
      });

      setUsers(usersArray.slice(0, 5)); // Показываем только 5 последних пользователей
    } catch (error) {
      console.error('Ошибка загрузки данных админа:', error);
      // Даже при ошибке показываем 0, а не падаем
      setStats({
        totalUsers: 0,
        totalQuizzes: 0,
        totalTests: 0,
        totalFlashcardSets: 0,
        activeToday: 0,
        activeThisWeek: 0,
        activeThisMonth: 0,
      });
      setUsers([]);
    } finally {
      setLoading(false);
    }
  };

  const statCards = [
    {
      icon: Users,
      label: 'Всего пользователей',
      value: stats.totalUsers,
      color: 'bg-blue-500',
    },
    {
      icon: FileText,
      label: 'Квизов создано',
      value: stats.totalQuizzes,
      color: 'bg-green-500',
    },
    {
      icon: FileText,
      label: 'Тестов создано',
      value: stats.totalTests,
      color: 'bg-indigo-500',
    },
    {
      icon: Award,
      label: 'Наборов карточек',
      value: stats.totalFlashcardSets,
      color: 'bg-purple-500',
    },
    {
      icon: Activity,
      label: 'Активных сегодня',
      value: stats.activeToday,
      color: 'bg-orange-500',
    },
  ];

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-xl text-gray-600 dark:text-gray-400">Загрузка...</div>
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
          <div className="flex items-center gap-3 mb-2">
            <div className="bg-red-500 p-2 rounded-lg">
              <AlertCircle className="w-6 h-6 text-white" />
            </div>
            <h1 className="text-4xl font-bold text-gray-900 dark:text-white">
              Панель Администратора
            </h1>
          </div>
          <p className="text-gray-600 dark:text-gray-400">
            Управление платформой и мониторинг активности
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
                    <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">{stat.label}</p>
                    <p className="text-3xl font-bold text-gray-900 dark:text-white">
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

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Последние пользователи */}
          <motion.div
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.2 }}
          >
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6 flex items-center gap-2">
              <Users className="w-6 h-6 text-primary-500" />
              Последние пользователи
            </h2>

            <Card className="p-6">
              <div className="space-y-4">
                {users.length > 0 ? (
                  users.map((user) => (
                    <div
                      key={user.id}
                      className="flex items-center justify-between py-3 border-b last:border-0"
                    >
                      <div>
                        <p className="font-medium text-gray-900 dark:text-white">
                          {user.firstName} {user.lastName}
                        </p>
                        <p className="text-sm text-gray-600 dark:text-gray-400">{user.email}</p>
                      </div>
                      <Button 
                        variant="secondary" 
                        size="sm"
                        onClick={() => navigate('/admin/users')}
                      >
                        Управление
                      </Button>
                    </div>
                  ))
                ) : (
                  <div className="text-center py-8 text-gray-500">
                    <Users className="w-12 h-12 mx-auto mb-2 text-gray-400" />
                    <p>Пользователи не найдены</p>
                  </div>
                )}
              </div>
            </Card>
          </motion.div>

          {/* Системные действия */}
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.2 }}
          >
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6 flex items-center gap-2">
              <TrendingUp className="w-6 h-6 text-primary-500" />
              Быстрые действия
            </h2>

            <Card className="p-6">
              <div className="space-y-3">
                <Button 
                  variant="primary" 
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/admin/users')}
                >
                  <Users className="w-4 h-4" />
                  Просмотреть всех пользователей
                </Button>
                <Button 
                  variant="secondary" 
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/admin/subjects')}
                >
                  <BookOpen className="w-4 h-4" />
                  Управление предметами
                </Button>
                <Button 
                  variant="secondary" 
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/admin/quizzes')}
                >
                  <FileText className="w-4 h-4" />
                  Управление квизами
                </Button>
                <Button 
                  variant="secondary" 
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/admin/exams')}
                >
                  <FileText className="w-4 h-4" />
                  Управление экзаменами
                </Button>
                <Button 
                  variant="secondary" 
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/admin/analytics')}
                >
                  <BarChart3 className="w-4 h-4" />
                  Аналитика платформы
                </Button>
                <Button 
                  variant="secondary" 
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/admin/export')}
                >
                  <Download className="w-4 h-4" />
                  Экспорт данных (CSV)
                </Button>
                <Button 
                  variant="secondary" 
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/admin/achievements')}
                >
                  <Award className="w-4 h-4" />
                  Управление достижениями
                </Button>
                <Button 
                  variant="secondary" 
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/admin/countries')}
                >
                  <Globe className="w-4 h-4" />
                  Управление странами
                </Button>
                <Button 
                  variant="secondary" 
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/admin/universities')}
                >
                  <Building2 className="w-4 h-4" />
                  Управление вузами
                </Button>
                <Button 
                  variant="secondary" 
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/admin/exam-types')}
                >
                  <ClipboardList className="w-4 h-4" />
                  Типы экзаменов
                </Button>
              </div>
            </Card>
          </motion.div>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;
