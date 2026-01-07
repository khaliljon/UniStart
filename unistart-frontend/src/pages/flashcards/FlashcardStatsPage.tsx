import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { ArrowLeft, BookOpen, Users, Edit, XCircle } from 'lucide-react';
import Button from '../../components/common/Button';
import { flashcardService } from '../../services/flashcardService';

interface FlashcardSetStats {
  id: number;
  title: string;
  description: string;
  subject: string;
  isPublic: boolean;
  createdAt: string;
  updatedAt: string;
  totalCards: number;
  uniqueStudents: number;
  averageProgress: number;
  completedSetsCount: number;
}

const FlashcardStatsPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [stats, setStats] = useState<FlashcardSetStats | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadStats();
  }, [id]);

  const loadStats = async () => {
    try {
      const data = await flashcardService.getSetStats(Number(id));
      console.log('📊 Статистика набора:', data);
      setStats(data);
      setError(null);
    } catch (error) {
      console.error('Ошибка загрузки статистики:', error);
      setError('Не удалось загрузить статистику');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 flex items-center justify-center">
        <div className="text-white text-xl">Загрузка статистики...</div>
      </div>
    );
  }

  if (error || !stats) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 flex items-center justify-center">
        <div className="text-center">
          <XCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h2 className="text-white text-2xl mb-4">{error || 'Статистика не найдена'}</h2>
          <button
            onClick={() => navigate(-1)}
            className="px-6 py-2 bg-white/10 hover:bg-white/20 text-white rounded-lg transition-colors"
          >
            Вернуться назад
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 py-8 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8 flex items-center justify-between"
        >
          <div>
            <button
              onClick={() => navigate(-1)}
              className="flex items-center gap-2 text-white/70 hover:text-white mb-4 transition-colors"
            >
              <ArrowLeft className="w-5 h-5" />
              Назад
            </button>
            <h1 className="text-4xl font-bold text-white mb-2">{stats.title}</h1>
            <p className="text-white/60">Статистика и аналитика набора карточек</p>
          </div>
          <Button
            onClick={() => navigate(`/flashcards/${id}/edit`)}
            variant="secondary"
            className="flex items-center gap-2"
          >
            <Edit className="w-4 h-4" />
            Редактировать
          </Button>
        </motion.div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20"
          >
            <div className="flex items-center gap-3 mb-2">
              <BookOpen className="w-8 h-8 text-blue-400" />
              <div>
                <p className="text-white/60 text-sm">Всего карточек</p>
                <p className="text-3xl font-bold text-white">{stats.totalCards}</p>
              </div>
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20"
          >
            <div className="flex items-center gap-3 mb-2">
              <Users className="w-8 h-8 text-green-400" />
              <div>
                <p className="text-white/60 text-sm">Изучающих</p>
                <p className="text-3xl font-bold text-white">{stats.uniqueStudents}</p>
              </div>
            </div>
          </motion.div>
        </div>

        {/* Additional Info */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.5 }}
            className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20"
          >
            <h3 className="text-xl font-bold text-white mb-4">Информация о наборе</h3>
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-white/60">Предмет:</span>
                <span className="text-white font-medium">{stats.subject || 'Не указан'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-white/60">Доступ:</span>
                <span className="text-white font-medium">{stats.isPublic ? 'Публичный' : 'Приватный'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-white/60">Создан:</span>
                <span className="text-white font-medium">
                  {new Date(stats.createdAt).toLocaleDateString('ru-RU')}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-white/60">Обновлен:</span>
                <span className="text-white font-medium">
                  {new Date(stats.updatedAt).toLocaleDateString('ru-RU')}
                </span>
              </div>
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.6 }}
            className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20"
          >
            <h3 className="text-xl font-bold text-white mb-4">Активность</h3>
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-white/60">Завершили набор:</span>
                <span className="text-white font-medium text-blue-400">{stats.completedSetsCount} из {stats.uniqueStudents} студентов</span>
              </div>
              <div className="flex justify-between">
                <span className="text-white/60">Средний прогресс:</span>
                <span className="text-white font-medium text-purple-400">{stats.averageProgress.toFixed(1)}%</span>
              </div>
            </div>
          </motion.div>
        </div>

        {/* Note */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.7 }}
          className="bg-blue-500/10 border border-blue-500/30 rounded-xl p-6"
        >
          <p className="text-blue-200">
            💡 <strong>Как это работает:</strong>
          </p>
          <ul className="text-blue-200 mt-2 space-y-1 ml-4">
            <li>• <strong>Изучающих:</strong> студенты, открывшие набор хотя бы раз</li>
            <li>• <strong>Средний прогресс:</strong> процент студентов, завершивших набор полностью</li>
          </ul>
        </motion.div>
      </div>
    </div>
  );
};

export default FlashcardStatsPage;
