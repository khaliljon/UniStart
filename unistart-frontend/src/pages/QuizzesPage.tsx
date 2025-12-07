import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  Target,
  Clock,
  Award,
  Play,
  Plus,
  Search,
  TrendingUp,
  BookOpen,
  Edit,
  Trash2,
  Upload,
  FileX,
  CheckCircle,
  AlertCircle,
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';

interface Quiz {
  id: number;
  title: string;
  description: string;
  subject: string;
  difficulty: string;
  timeLimit: number;
  questionCount: number;
  totalPoints: number;
  isPublished?: boolean;
  createdAt: string;
}

const QuizzesPage = () => {
  const navigate = useNavigate();
  const { isTeacher, isAdmin } = useAuth();
  const [quizzes, setQuizzes] = useState<Quiz[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [subjectFilter, setSubjectFilter] = useState<string>('');
  const [difficultyFilter, setDifficultyFilter] = useState<string>('');

  useEffect(() => {
    loadQuizzes();
  }, [subjectFilter, difficultyFilter]);

  const loadQuizzes = async () => {
    try {
      setLoading(true);
      
      // Админы видят все квизы, учителя - свои, студенты - опубликованные
      let endpoint = '/quizzes'; // По умолчанию для студентов
      
      if (isAdmin) {
        endpoint = '/admin/quizzes'; // Админ видит все квизы
      } else if (isTeacher) {
        endpoint = '/quizzes/my'; // Учитель видит свои квизы
      }
      
      const params = new URLSearchParams();
      
      if (subjectFilter) params.append('subject', subjectFilter);
      if (difficultyFilter) params.append('difficulty', difficultyFilter);
      
      const response = await api.get(`${endpoint}?${params.toString()}`);
      setQuizzes(response.data);
    } catch (error) {
      console.error('Ошибка загрузки квизов:', error);
      setQuizzes([]);
    } finally {
      setLoading(false);
    }
  };

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty.toLowerCase()) {
      case 'easy':
        return 'bg-green-100 text-green-800';
      case 'medium':
        return 'bg-yellow-100 text-yellow-800';
      case 'hard':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getDifficultyLabel = (difficulty: string) => {
    switch (difficulty.toLowerCase()) {
      case 'easy':
        return 'Легко';
      case 'medium':
        return 'Средне';
      case 'hard':
        return 'Сложно';
      default:
        return difficulty;
    }
  };

  const handlePublish = async (quizId: number) => {
    if (!confirm('Вы уверены, что хотите опубликовать этот квиз?')) {
      return;
    }
    try {
      await api.patch(`/quizzes/${quizId}/publish`);
      loadQuizzes(); // Перезагружаем список
    } catch (error) {
      console.error('Ошибка публикации квиза:', error);
      alert('Не удалось опубликовать квиз');
    }
  };

  const handleUnpublish = async (quizId: number) => {
    if (!confirm('Вы уверены, что хотите снять квиз с публикации?')) {
      return;
    }
    try {
      await api.patch(`/quizzes/${quizId}/unpublish`);
      loadQuizzes(); // Перезагружаем список
    } catch (error) {
      console.error('Ошибка отмены публикации квиза:', error);
      alert('Не удалось снять квиз с публикации');
    }
  };

  const handleDelete = async (quizId: number) => {
    if (!confirm('Вы уверены, что хотите удалить этот квиз? Это действие нельзя отменить.')) {
      return;
    }
    try {
      await api.delete(`/quizzes/${quizId}`);
      loadQuizzes(); // Перезагружаем список
    } catch (error) {
      console.error('Ошибка удаления квиза:', error);
      alert('Не удалось удалить квиз');
    }
  };

  const filteredQuizzes = quizzes.filter((quiz) =>
    quiz.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    quiz.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const subjects = [...new Set(quizzes.map((q) => q.subject))];

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка тестов...</div>
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
          <div className="flex items-center justify-between mb-4">
            <div>
              <h1 className="text-4xl font-bold text-gray-900 mb-2">
                📝 {isAdmin ? 'Квизы' : (isTeacher ? 'Мои Квизы' : 'Квизы')}
              </h1>
              <p className="text-gray-600">
                {isAdmin
                  ? 'Все квизы в системе'
                  : ((isTeacher)
                    ? 'Управляйте своими квизами и отслеживайте результаты студентов'
                    : 'Проверьте свои знания и улучшайте результаты')}
              </p>
            </div>
            {(isTeacher || isAdmin) && (
              <Button
                variant="primary"
                onClick={() => navigate('/quizzes/create')}
                className="flex items-center gap-2"
              >
                <Plus className="w-5 h-5" />
                Создать квиз
              </Button>
            )}
          </div>

          {/* Search and Filters */}
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                placeholder="Поиск по названию или описанию..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>

            <select
              value={subjectFilter}
              onChange={(e) => setSubjectFilter(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            >
              <option value="">Все предметы</option>
              {subjects.map((subject) => (
                <option key={subject} value={subject}>
                  {subject}
                </option>
              ))}
            </select>

            <select
              value={difficultyFilter}
              onChange={(e) => setDifficultyFilter(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            >
              <option value="">Любая сложность</option>
              <option value="Easy">Легко</option>
              <option value="Medium">Средне</option>
              <option value="Hard">Сложно</option>
            </select>
          </div>
        </motion.div>

        {/* Quizzes Grid */}
        {filteredQuizzes.length === 0 ? (
          <Card className="p-12 text-center">
            <BookOpen className="w-16 h-16 text-gray-400 mx-auto mb-4" />
            <h3 className="text-xl font-semibold text-gray-700 mb-2">
              Квизы не найдены
            </h3>
            <p className="text-gray-500 mb-4">
              {(isTeacher || isAdmin)
                ? 'Создайте свой первый квиз, чтобы начать'
                : 'Пока нет доступных квизов'}
            </p>
            {(isTeacher || isAdmin) && (
              <Button
                variant="primary"
                onClick={() => navigate('/quizzes/create')}
              >
                Создать квиз
              </Button>
            )}
          </Card>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredQuizzes.map((quiz, index) => (
              <motion.div
                key={quiz.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.1 }}
              >
                <Card className="h-full flex flex-col hover:shadow-lg transition-shadow">
                  {/* Заголовок квиза */}
                  <div className="flex justify-between items-start mb-3">
                    <h3 className="text-lg font-semibold text-gray-900 flex-1">
                      {quiz.title}
                    </h3>
                    <span
                      className={`px-2 py-1 rounded-full text-xs font-medium ${getDifficultyColor(
                        quiz.difficulty
                      )}`}
                    >
                      {getDifficultyLabel(quiz.difficulty)}
                    </span>
                  </div>

                  {/* Описание */}
                  <p className="text-gray-600 text-sm mb-4 flex-1">
                    {quiz.description}
                  </p>

                  {/* Предмет */}
                  <div className="flex items-center gap-2 mb-3">
                    <BookOpen className="w-4 h-4 text-gray-400" />
                    <span className="text-sm text-gray-700">{quiz.subject}</span>
                  </div>

                  {/* Статистика */}
                  <div className="grid grid-cols-2 gap-3 mb-4">
                    <div className="flex items-center gap-2">
                      <Target className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">
                        {quiz.questionCount} вопросов
                      </span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Clock className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">{quiz.timeLimit} мин</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Award className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">
                        {quiz.totalPoints} баллов
                      </span>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="flex flex-col gap-2">
                    {(isTeacher || isAdmin) ? (
                      <>
                        <div className="flex gap-2">
                          <Button
                            variant="secondary"
                            size="sm"
                            className="flex-1 flex items-center justify-center gap-2"
                            onClick={() => navigate(`/quizzes/${quiz.id}/edit`)}
                          >
                            <Edit className="w-4 h-4" />
                            Редактировать
                          </Button>
                          <Button
                            variant="primary"
                            size="sm"
                            className="flex-1 flex items-center justify-center gap-2"
                            onClick={() => navigate(`/quizzes/${quiz.id}/stats`)}
                          >
                            <TrendingUp className="w-4 h-4" />
                            Статистика
                          </Button>
                        </div>
                        <div className="flex gap-2">
                          {quiz.isPublished ? (
                            <Button
                              variant="secondary"
                              size="sm"
                              className="flex-1 flex items-center justify-center gap-2"
                              onClick={() => handleUnpublish(quiz.id)}
                            >
                              <FileX className="w-4 h-4" />
                              Снять с публикации
                            </Button>
                          ) : (
                            <Button
                              variant="primary"
                              size="sm"
                              className="flex-1 flex items-center justify-center gap-2 bg-green-600 hover:bg-green-700"
                              onClick={() => handlePublish(quiz.id)}
                            >
                              <Upload className="w-4 h-4" />
                              Опубликовать
                            </Button>
                          )}
                          <Button
                            variant="danger"
                            size="sm"
                            className="px-4 flex items-center justify-center gap-2"
                            onClick={() => handleDelete(quiz.id)}
                          >
                            <Trash2 className="w-4 h-4" />
                          </Button>
                        </div>
                      </>
                    ) : (
                      <Button
                        variant="primary"
                        size="sm"
                        className="w-full flex items-center justify-center gap-2"
                        onClick={() => navigate(`/quizzes/${quiz.id}/take`)}
                      >
                        <Play className="w-4 h-4" />
                        Начать квиз
                      </Button>
                    )}
                  </div>

                  {/* Статус публикации для преподавателей */}
                  {(isTeacher || isAdmin) && (
                    <div className="mt-3 pt-3 border-t border-gray-200 flex items-center justify-between">
                      <span className="text-xs text-gray-500">
                        {quiz.isPublished ? (
                          <span className="flex items-center gap-1 text-green-600">
                            <CheckCircle className="w-3 h-3" />
                            Опубликован
                          </span>
                        ) : (
                          <span className="flex items-center gap-1 text-gray-600">
                            <AlertCircle className="w-3 h-3" />
                            Черновик
                          </span>
                        )}
                      </span>
                      <span className="text-xs text-gray-400">
                        {new Date(quiz.createdAt).toLocaleDateString('ru-RU')}
                      </span>
                    </div>
                  )}
                </Card>
              </motion.div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default QuizzesPage;

