import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { Users, TrendingUp, Award, AlertCircle, FileText, Activity } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface AdminStats {
  totalUsers: number;
  totalQuizzes: number;
  totalFlashcardSets: number;
  activeToday: number;
}

const AdminDashboard = () => {
  const [stats, setStats] = useState<AdminStats>({
    totalUsers: 0,
    totalQuizzes: 0,
    totalFlashcardSets: 0,
    activeToday: 0,
  });
  const [users, setUsers] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadAdminData();
  }, []);

  const loadAdminData = async () => {
    try {
      const [statsData, usersData] = await Promise.all([
        api.get('/admin/stats'),
        api.get('/admin/users'),
      ]);

      setStats(statsData.data);
      setUsers(usersData.data.slice(0, 5)); // Показываем только 5 последних пользователей
    } catch (error) {
      console.error('Ошибка загрузки данных админа:', error);
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
        <div className="text-xl text-gray-600">Загрузка...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 py-8 px-4">
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
            <h1 className="text-4xl font-bold text-gray-900">
              Панель Администратора
            </h1>
          </div>
          <p className="text-gray-600">
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

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Последние пользователи */}
          <motion.div
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.2 }}
          >
            <h2 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
              <Users className="w-6 h-6 text-primary-500" />
              Последние пользователи
            </h2>

            <Card className="p-6">
              <div className="space-y-4">
                {users.map((user) => (
                  <div
                    key={user.id}
                    className="flex items-center justify-between py-3 border-b last:border-0"
                  >
                    <div>
                      <p className="font-medium text-gray-900">
                        {user.firstName} {user.lastName}
                      </p>
                      <p className="text-sm text-gray-600">{user.email}</p>
                    </div>
                    <Button variant="secondary" size="sm">
                      Управление
                    </Button>
                  </div>
                ))}
              </div>
            </Card>
          </motion.div>

          {/* Системные действия */}
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.2 }}
          >
            <h2 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
              <TrendingUp className="w-6 h-6 text-primary-500" />
              Быстрые действия
            </h2>

            <Card className="p-6">
              <div className="space-y-3">
                <Button variant="primary" className="w-full">
                  Просмотреть всех пользователей
                </Button>
                <Button variant="secondary" className="w-full">
                  Аналитика платформы
                </Button>
                <Button variant="secondary" className="w-full">
                  Экспорт данных (CSV)
                </Button>
                <Button variant="secondary" className="w-full">
                  Создать достижение
                </Button>
                <Button variant="secondary" className="w-full">
                  Управление ролями
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
