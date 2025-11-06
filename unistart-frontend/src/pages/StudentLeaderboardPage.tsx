import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  Trophy,
  Medal,
  Award,
  TrendingUp,
  Target,
  Crown,
  Star,
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface LeaderboardEntry {
  rank: number;
  userId: string;
  userName: string;
  email: string;
  totalPoints: number;
  quizzesTaken: number;
  averageScore: number;
  achievementsUnlocked: number;
  isCurrentUser: boolean;
}

const StudentLeaderboardPage = () => {
  const navigate = useNavigate();
  const [leaderboard, setLeaderboard] = useState<LeaderboardEntry[]>([]);
  const [currentUser, setCurrentUser] = useState<LeaderboardEntry | null>(null);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<'all' | 'week' | 'month'>('all');

  useEffect(() => {
    loadLeaderboard();
  }, [filter]);

  const loadLeaderboard = async () => {
    try {
      setLoading(true);
      const response = await api.get(`/student/leaderboard?period=${filter === 'all' ? 'all-time' : filter}&top=50`);
      
      // Форматируем данные под наш интерфейс
      const formattedLeaderboard = response.data.leaderboard.map((entry: any) => ({
        rank: entry.rank,
        userId: entry.userId || '0',
        userName: entry.userName,
        email: entry.email || '',
        totalPoints: entry.totalPoints,
        quizzesTaken: entry.totalAttempts,
        averageScore: entry.averageScore,
        achievementsUnlocked: 0, // TODO: добавить когда будут достижения
        isCurrentUser: false // TODO: определить текущего пользователя
      }));
      
      setLeaderboard(formattedLeaderboard);
    } catch (error) {
      console.error('Ошибка загрузки таблицы лидеров:', error);
      // Моковые данные
      const mockLeaderboard: LeaderboardEntry[] = [
        {
          rank: 1,
          userId: '1',
          userName: 'Алия Нурмуханова',
          email: 'aliya@example.com',
          totalPoints: 2850,
          quizzesTaken: 25,
          averageScore: 92.5,
          achievementsUnlocked: 18,
          isCurrentUser: false,
        },
        {
          rank: 2,
          userId: '2',
          userName: 'Данияр Сериков',
          email: 'daniyar@example.com',
          totalPoints: 2720,
          quizzesTaken: 23,
          averageScore: 89.2,
          achievementsUnlocked: 16,
          isCurrentUser: false,
        },
        {
          rank: 3,
          userId: '3',
          userName: 'Айгерим Кенжебаева',
          email: 'aigerim@example.com',
          totalPoints: 2650,
          quizzesTaken: 22,
          averageScore: 87.8,
          achievementsUnlocked: 15,
          isCurrentUser: false,
        },
        {
          rank: 4,
          userId: '4',
          userName: 'Вы',
          email: 'student@unistart.kz',
          totalPoints: 2400,
          quizzesTaken: 20,
          averageScore: 85.0,
          achievementsUnlocked: 12,
          isCurrentUser: true,
        },
        {
          rank: 5,
          userId: '5',
          userName: 'Нурлан Жумабеков',
          email: 'nurlan@example.com',
          totalPoints: 2150,
          quizzesTaken: 18,
          averageScore: 82.3,
          achievementsUnlocked: 11,
          isCurrentUser: false,
        },
      ];
      setLeaderboard(mockLeaderboard);
      setCurrentUser(mockLeaderboard.find((e) => e.isCurrentUser) || null);
    } finally {
      setLoading(false);
    }
  };

  const getRankIcon = (rank: number) => {
    switch (rank) {
      case 1:
        return <Crown className="w-8 h-8 text-yellow-500" />;
      case 2:
        return <Medal className="w-8 h-8 text-gray-400" />;
      case 3:
        return <Medal className="w-8 h-8 text-orange-400" />;
      default:
        return (
          <div className="w-8 h-8 flex items-center justify-center">
            <span className="text-gray-600 font-bold">{rank}</span>
          </div>
        );
    }
  };

  const getRankColor = (rank: number) => {
    switch (rank) {
      case 1:
        return 'bg-gradient-to-r from-yellow-400 to-yellow-500';
      case 2:
        return 'bg-gradient-to-r from-gray-300 to-gray-400';
      case 3:
        return 'bg-gradient-to-r from-orange-400 to-orange-500';
      default:
        return 'bg-white';
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка таблицы лидеров...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-purple-50 py-8 px-4">
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
                🏆 Таблица лидеров
              </h1>
              <p className="text-gray-600">
                Соревнуйтесь с другими студентами и стремитесь к вершине
              </p>
            </div>
          </div>
        </motion.div>

        {/* Current User Card */}
        {currentUser && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="mb-8"
          >
            <Card className="p-6 bg-gradient-to-r from-primary-500 to-purple-600 text-white">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-4">
                  <div className="bg-white bg-opacity-20 p-4 rounded-lg">
                    <Trophy className="w-10 h-10" />
                  </div>
                  <div>
                    <p className="text-primary-100 text-sm mb-1">
                      Ваша позиция
                    </p>
                    <p className="text-3xl font-bold mb-1">
                      #{currentUser.rank}
                    </p>
                    <p className="text-primary-100 text-sm">
                      {currentUser.totalPoints} баллов
                    </p>
                  </div>
                </div>
                <div className="text-right">
                  <p className="text-primary-100 text-sm mb-1">Средний балл</p>
                  <p className="text-2xl font-bold">
                    {currentUser.averageScore}%
                  </p>
                </div>
              </div>
            </Card>
          </motion.div>
        )}

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
            Все время
          </Button>
          <Button
            variant={filter === 'month' ? 'primary' : 'secondary'}
            onClick={() => setFilter('month')}
          >
            Этот месяц
          </Button>
          <Button
            variant={filter === 'week' ? 'primary' : 'secondary'}
            onClick={() => setFilter('week')}
          >
            Эта неделя
          </Button>
        </motion.div>

        {/* Leaderboard */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
        >
          <Card className="p-6">
            <h2 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
              <TrendingUp className="w-6 h-6 text-primary-500" />
              Топ студентов
            </h2>

            <div className="space-y-4">
              {leaderboard.map((entry, index) => (
                <motion.div
                  key={entry.userId}
                  initial={{ opacity: 0, x: -20 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: 0.3 + index * 0.05 }}
                  className={`${getRankColor(entry.rank)} ${
                    entry.isCurrentUser
                      ? 'ring-2 ring-primary-500'
                      : 'hover:shadow-md'
                  } p-6 rounded-xl transition-all ${
                    entry.rank <= 3 ? 'text-white' : ''
                  }`}
                >
                  <div className="flex items-center gap-6">
                    {/* Rank */}
                    <div className="flex-shrink-0">{getRankIcon(entry.rank)}</div>

                    {/* Avatar */}
                    <div className="flex-shrink-0">
                      <div
                        className={`w-14 h-14 rounded-full flex items-center justify-center text-xl font-bold ${
                          entry.rank <= 3
                            ? 'bg-white bg-opacity-30'
                            : 'bg-primary-500 text-white'
                        }`}
                      >
                        {entry.userName.charAt(0).toUpperCase()}
                      </div>
                    </div>

                    {/* Info */}
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <h3
                          className={`text-lg font-bold ${
                            entry.rank <= 3 ? '' : 'text-gray-900'
                          }`}
                        >
                          {entry.userName}
                        </h3>
                        {entry.isCurrentUser && (
                          <span className="px-2 py-1 bg-primary-500 text-white text-xs rounded-full">
                            Вы
                          </span>
                        )}
                      </div>
                      <div
                        className={`flex items-center gap-4 text-sm ${
                          entry.rank <= 3 ? 'text-opacity-90' : 'text-gray-600'
                        }`}
                      >
                        <span className="flex items-center gap-1">
                          <Target className="w-4 h-4" />
                          {entry.quizzesTaken} тестов
                        </span>
                        <span className="flex items-center gap-1">
                          <Award className="w-4 h-4" />
                          {entry.achievementsUnlocked} достижений
                        </span>
                      </div>
                    </div>

                    {/* Stats */}
                    <div className="text-right flex-shrink-0">
                      <p
                        className={`text-2xl font-bold mb-1 ${
                          entry.rank <= 3 ? '' : 'text-gray-900'
                        }`}
                      >
                        {entry.totalPoints}
                      </p>
                      <p
                        className={`text-sm ${
                          entry.rank <= 3 ? 'text-opacity-90' : 'text-gray-600'
                        }`}
                      >
                        баллов
                      </p>
                      <div
                        className={`flex items-center justify-end gap-1 mt-2 text-sm ${
                          entry.rank <= 3 ? '' : 'text-green-600'
                        }`}
                      >
                        <Star
                          className={`w-4 h-4 ${
                            entry.rank <= 3 ? '' : 'fill-current'
                          }`}
                        />
                        <span className="font-medium">
                          {entry.averageScore}%
                        </span>
                      </div>
                    </div>
                  </div>
                </motion.div>
              ))}
            </div>
          </Card>
        </motion.div>

        {/* Motivation Card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}
          className="mt-8"
        >
          <Card className="p-6 bg-gradient-to-r from-green-500 to-teal-500 text-white">
            <div className="flex items-center gap-4">
              <TrendingUp className="w-12 h-12" />
              <div>
                <h3 className="text-xl font-bold mb-1">
                  Продолжайте учиться!
                </h3>
                <p className="text-green-100">
                  Чем больше тестов вы проходите и чем выше ваши баллы, тем выше
                  ваша позиция в рейтинге. Удачи!
                </p>
              </div>
            </div>
          </Card>
        </motion.div>
      </div>
    </div>
  );
};

export default StudentLeaderboardPage;
