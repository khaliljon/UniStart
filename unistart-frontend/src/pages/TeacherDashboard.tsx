import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { BookOpen, Users, TrendingUp, FileText, Plus, BarChart3, Download } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface TeacherStats {
  myQuizzes: number;
  myFlashcardSets: number;
  totalStudents: number;
  averageScore: number;
}

const TeacherDashboard = () => {
  const navigate = useNavigate();
  const [stats, setStats] = useState<TeacherStats>({
    myQuizzes: 0,
    myFlashcardSets: 0,
    totalStudents: 0,
    averageScore: 0,
  });
  const [myQuizzes, setMyQuizzes] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadTeacherData();

    // Перезагружаем данные при возврате на страницу
    const handleVisibilityChange = () => {
      if (!document.hidden) {
        loadTeacherData();
      }
    };

    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, []);

  const loadTeacherData = async () => {
    try {
      const [quizzesData, flashcardsData] = await Promise.allSettled([
        api.get('/quizzes/my'),         // Получаем свои квизы
        api.get('/flashcards/sets'),    // Получаем свои наборы карточек
      ]);

      console.log('Teacher quizzes:', quizzesData);
      console.log('Teacher flashcards:', flashcardsData);

      // Обработка квизов
      let quizzes: any[] = [];
      if (quizzesData.status === 'fulfilled') {
        const data = quizzesData.value.data;
        quizzes = Array.isArray(data) ? data : (data.quizzes || data.Quizzes || []);
      }

      // Обработка карточек
      let flashcardSets: any[] = [];
      if (flashcardsData.status === 'fulfilled') {
        const data = flashcardsData.value.data;
        flashcardSets = Array.isArray(data) ? data : (data.flashcardSets || data.FlashcardSets || []);
      }

      setStats({
        myQuizzes: quizzes.length,
        myFlashcardSets: flashcardSets.length,
        totalStudents: 0,  // TODO: добавить подсчет студентов
        averageScore: 0,   // TODO: добавить подсчет среднего балла
      });

      setMyQuizzes(quizzes.slice(0, 5));
    } catch (error) {
      console.error('Ошибка загрузки данных учителя:', error);
      setStats({
        myQuizzes: 0,
        myFlashcardSets: 0,
        totalStudents: 0,
        averageScore: 0,
      });
      setMyQuizzes([]);
    } finally {
      setLoading(false);
    }
  };

  const statCards = [
    {
      icon: FileText,
      label: 'Мои квизы',
      value: stats.myQuizzes,
      color: 'bg-blue-500',
    },
    {
      icon: BookOpen,
      label: 'Мои наборы карточек',
      value: stats.myFlashcardSets,
      color: 'bg-green-500',
    },
    {
      icon: Users,
      label: 'Студентов',
      value: stats.totalStudents,
      color: 'bg-purple-500',
    },
    {
      icon: TrendingUp,
      label: 'Средний балл',
      value: `${stats.averageScore}%`,
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
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Заголовок */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-4xl font-bold text-gray-900 mb-2">
                👨‍🏫 Панель Преподавателя
              </h1>
              <p className="text-gray-600">
                Управление курсами и мониторинг прогресса студентов
              </p>
            </div>
            <div className="flex gap-3">
              <Button
                variant="primary"
                onClick={() => navigate('/quizzes/create')}
                className="flex items-center gap-2"
              >
                <Plus className="w-5 h-5" />
                Создать квиз
              </Button>
              <Button
                variant="secondary"
                onClick={() => navigate('/flashcards/create')}
                className="flex items-center gap-2"
              >
                <Plus className="w-5 h-5" />
                Создать набор карточек
              </Button>
            </div>
          </div>
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
          {/* Мои квизы */}
          <motion.div
            initial={{ opacity: 0, x: -20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.2 }}
          >
            <h2 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
              <FileText className="w-6 h-6 text-primary-500" />
              Мои квизы
            </h2>

            <Card className="p-6">
              {myQuizzes.length === 0 ? (
                <div className="text-center py-8">
                  <p className="text-gray-600 mb-4">
                    У вас пока нет созданных квизов
                  </p>
                  <Button
                    variant="primary"
                    onClick={() => navigate('/quizzes/create')}
                  >
                    Создать первый квиз
                  </Button>
                </div>
              ) : (
                <div className="space-y-4">
                  {myQuizzes.map((quiz) => (
                    <div
                      key={quiz.id}
                      className="flex items-center justify-between py-3 border-b last:border-0"
                    >
                      <div className="flex-1">
                        <p className="font-medium text-gray-900">{quiz.title}</p>
                        <p className="text-sm text-gray-600">
                          {quiz.subject} · {quiz.difficulty}
                        </p>
                      </div>
                      <div className="flex gap-2">
                        <Button
                          variant="secondary"
                          size="sm"
                          onClick={() => navigate(`/quizzes/${quiz.id}/stats`)}
                          title="Статистика квиза"
                        >
                          <BarChart3 className="w-4 h-4" />
                        </Button>
                        <Button
                          variant="secondary"
                          size="sm"
                          onClick={() => navigate(`/quizzes/${quiz.id}/edit`)}
                        >
                          Редактировать
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </Card>
          </motion.div>

          {/* Статистика студентов */}
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ delay: 0.2 }}
          >
            <h2 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
              <Users className="w-6 h-6 text-primary-500" />
              Студенты
            </h2>

            <Card className="p-6">
              <div className="space-y-3">
                <Button
                  variant="primary"
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/teacher/students')}
                >
                  <Users className="w-4 h-4" />
                  Просмотреть всех студентов
                </Button>
                <Button
                  variant="secondary"
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/teacher/analytics')}
                >
                  <BarChart3 className="w-4 h-4" />
                  Детальная аналитика
                </Button>
                <Button
                  variant="secondary"
                  className="w-full flex items-center justify-center gap-2"
                  onClick={() => navigate('/teacher/export')}
                >
                  <Download className="w-4 h-4" />
                  Экспорт результатов
                </Button>
              </div>
            </Card>
          </motion.div>
        </div>
      </div>
    </div>
  );
};

export default TeacherDashboard;
