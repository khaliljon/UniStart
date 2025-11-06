import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  Award,
  TrendingUp,
  Star,
  Trophy,
  Target,
  Zap,
  Lock,
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface Achievement {
  id: number;
  name: string;
  description: string;
  icon: string;
  requiredValue: number;
  category: string;
  isUnlocked: boolean;
  unlockedAt?: string;
  progress: number;
}

const StudentAchievementsPage = () => {
  const navigate = useNavigate();
  const [achievements, setAchievements] = useState<Achievement[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<'all' | 'unlocked' | 'locked'>('all');

  useEffect(() => {
    loadAchievements();
  }, []);

  const loadAchievements = async () => {
    try {
      setLoading(true);
      const response = await api.get('/achievements');
      
      // Форматируем данные под наш интерфейс
      const formattedAchievements = response.data.map((ach: any) => ({
        id: ach.id,
        name: ach.name,
        description: ach.description,
        icon: ach.icon || '🏆',
        requiredValue: ach.pointsRequired || 0,
        category: ach.category || 'Общее',
        isUnlocked: ach.isUnlocked,
        unlockedAt: ach.unlockedAt,
        progress: ach.progress || 0,
      }));
      
      setAchievements(formattedAchievements);
    } catch (error) {
      console.error('Ошибка загрузки достижений:', error);
      // Моковые данные
      setAchievements([
        {
          id: 1,
          name: 'Первые шаги',
          description: 'Зарегистрируйтесь на платформе',
          icon: '🎯',
          requiredValue: 1,
          category: 'Начало',
          isUnlocked: true,
          unlockedAt: new Date().toISOString(),
          progress: 100,
        },
        {
          id: 2,
          name: 'Новичок',
          description: 'Пройдите первый тест',
          icon: '🌟',
          requiredValue: 1,
          category: 'Тесты',
          isUnlocked: false,
          progress: 0,
        },
        {
          id: 3,
          name: 'Студент',
          description: 'Изучите 50 карточек',
          icon: '📚',
          requiredValue: 50,
          category: 'Карточки',
          isUnlocked: false,
          progress: 20,
        },
        {
          id: 4,
          name: 'Эксперт',
          description: 'Наберите 90%+ в 5 тестах',
          icon: '🏆',
          requiredValue: 5,
          category: 'Тесты',
          isUnlocked: false,
          progress: 40,
        },
        {
          id: 5,
          name: 'Марафонец',
          description: 'Занимайтесь 7 дней подряд',
          icon: '🔥',
          requiredValue: 7,
          category: 'Активность',
          isUnlocked: false,
          progress: 57,
        },
        {
          id: 6,
          name: 'Перфекционист',
          description: 'Получите 100% в тесте',
          icon: '💎',
          requiredValue: 1,
          category: 'Тесты',
          isUnlocked: false,
          progress: 0,
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const getCategoryIcon = (category: string) => {
    const icons: Record<string, any> = {
      'Начало': Target,
      'Тесты': Trophy,
      'Карточки': Star,
      'Активность': Zap,
    };
    return icons[category] || Award;
  };

  const getCategoryColor = (category: string) => {
    const colors: Record<string, string> = {
      'Начало': 'bg-blue-500',
      'Тесты': 'bg-yellow-500',
      'Карточки': 'bg-purple-500',
      'Активность': 'bg-red-500',
    };
    return colors[category] || 'bg-gray-500';
  };

  const filteredAchievements = achievements.filter((achievement) => {
    if (filter === 'unlocked') return achievement.isUnlocked;
    if (filter === 'locked') return !achievement.isUnlocked;
    return true;
  });

  const unlockedCount = achievements.filter((a) => a.isUnlocked).length;
  const totalCount = achievements.length;
  const completionPercentage = (unlockedCount / totalCount) * 100;

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка достижений...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-yellow-50 py-8 px-4">
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
                🏆 Мои достижения
              </h1>
              <p className="text-gray-600">
                Отслеживайте свои успехи и получайте награды
              </p>
            </div>
          </div>
        </motion.div>

        {/* Progress Card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="mb-8"
        >
          <Card className="p-6 bg-gradient-to-r from-yellow-400 to-orange-500 text-white">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h2 className="text-2xl font-bold mb-1">
                  {unlockedCount} из {totalCount} достижений
                </h2>
                <p className="text-yellow-100">
                  {completionPercentage.toFixed(0)}% завершено
                </p>
              </div>
              <Trophy className="w-16 h-16 text-yellow-100" />
            </div>
            <div className="w-full bg-yellow-200 rounded-full h-4">
              <motion.div
                initial={{ width: 0 }}
                animate={{ width: `${completionPercentage}%` }}
                transition={{ duration: 1, delay: 0.3 }}
                className="bg-white rounded-full h-4"
              />
            </div>
          </Card>
        </motion.div>

        {/* Filters */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
          className="mb-6 flex gap-4"
        >
          <Button
            variant={filter === 'all' ? 'primary' : 'secondary'}
            onClick={() => setFilter('all')}
          >
            Все ({totalCount})
          </Button>
          <Button
            variant={filter === 'unlocked' ? 'primary' : 'secondary'}
            onClick={() => setFilter('unlocked')}
          >
            Открытые ({unlockedCount})
          </Button>
          <Button
            variant={filter === 'locked' ? 'primary' : 'secondary'}
            onClick={() => setFilter('locked')}
          >
            Закрытые ({totalCount - unlockedCount})
          </Button>
        </motion.div>

        {/* Achievements Grid */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6"
        >
          {filteredAchievements.map((achievement, index) => {
            const CategoryIcon = getCategoryIcon(achievement.category);
            const categoryColor = getCategoryColor(achievement.category);

            return (
              <motion.div
                key={achievement.id}
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.3 + index * 0.05 }}
              >
                <Card
                  className={`p-6 relative overflow-hidden ${
                    achievement.isUnlocked
                      ? 'border-2 border-yellow-400 shadow-lg'
                      : 'opacity-75'
                  }`}
                >
                  {achievement.isUnlocked && (
                    <div className="absolute top-2 right-2">
                      <div className="bg-yellow-400 text-white p-1 rounded-full">
                        <Star className="w-4 h-4 fill-current" />
                      </div>
                    </div>
                  )}

                  <div className="flex items-start gap-4 mb-4">
                    <div
                      className={`${categoryColor} p-4 rounded-lg text-white text-3xl flex-shrink-0`}
                    >
                      {achievement.isUnlocked ? achievement.icon : '🔒'}
                    </div>
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <h3 className="text-lg font-bold text-gray-900">
                          {achievement.name}
                        </h3>
                        {!achievement.isUnlocked && (
                          <Lock className="w-4 h-4 text-gray-400" />
                        )}
                      </div>
                      <div className="flex items-center gap-2 text-xs text-gray-500">
                        <CategoryIcon className="w-3 h-3" />
                        <span>{achievement.category}</span>
                      </div>
                    </div>
                  </div>

                  <p className="text-gray-600 text-sm mb-4">
                    {achievement.description}
                  </p>

                  {achievement.isUnlocked ? (
                    <div className="flex items-center gap-2 text-green-600">
                      <Award className="w-5 h-5" />
                      <span className="text-sm font-medium">
                        Разблокировано{' '}
                        {achievement.unlockedAt &&
                          new Date(achievement.unlockedAt).toLocaleDateString(
                            'ru-RU'
                          )}
                      </span>
                    </div>
                  ) : (
                    <div>
                      <div className="flex items-center justify-between text-sm text-gray-600 mb-2">
                        <span>Прогресс</span>
                        <span className="font-medium">
                          {achievement.progress}%
                        </span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-2">
                        <div
                          className="bg-primary-500 rounded-full h-2 transition-all"
                          style={{ width: `${achievement.progress}%` }}
                        />
                      </div>
                    </div>
                  )}
                </Card>
              </motion.div>
            );
          })}
        </motion.div>

        {/* Motivation */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}
          className="mt-8"
        >
          <Card className="p-6 bg-gradient-to-r from-purple-500 to-pink-500 text-white">
            <div className="flex items-center gap-4">
              <TrendingUp className="w-12 h-12" />
              <div>
                <h3 className="text-xl font-bold mb-1">
                  Продолжайте в том же духе!
                </h3>
                <p className="text-purple-100">
                  Вы уже разблокировали {unlockedCount} достижений. Осталось
                  всего {totalCount - unlockedCount}!
                </p>
              </div>
            </div>
          </Card>
        </motion.div>
      </div>
    </div>
  );
};

export default StudentAchievementsPage;
