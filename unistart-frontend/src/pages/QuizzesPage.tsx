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
      
      // Учителя и админы видят свои квизы, студенты - все опубликованные
      const endpoint = (isTeacher || isAdmin) ? '/quizzes/my' : '/quizzes';
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
                📝 Тесты
              </h1>
              <p className="text-gray-600">
                {(isTeacher || isAdmin)
                  ? 'Управляйте своими тестами и отслеживайте результаты студентов'
                  : 'Проверьте свои знания и улучшайте результаты'}
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
              Тесты не найдены
            </h3>
            <p className="text-gray-500 mb-4">
              {(isTeacher || isAdmin)
                ? 'Создайте свой первый тест, чтобы начать'
                : 'Пока нет доступных тестов'}
            </p>
            {(isTeacher || isAdmin) && (
              <Button
                variant="primary"
                onClick={() => navigate('/quizzes/create')}
              >
                Создать тест
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
                <Card className="p-6 hover:shadow-lg transition-all duration-300 border-2 border-transparent hover:border-primary-200">
                  <div className="flex items-start justify-between mb-4">
                    <div className="flex-1">
                      <h3 className="text-xl font-bold text-gray-900 mb-2">
                        {quiz.title}
                      </h3>
                      <p className="text-sm text-gray-600 line-clamp-2">
                        {quiz.description}
                      </p>
                    </div>
                  </div>

                  <div className="space-y-3 mb-4">
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-gray-600">Предмет:</span>
                      <span className="font-medium text-gray-900">
                        {quiz.subject}
                      </span>
                    </div>

                    <div className="flex items-center justify-between text-sm">
                      <span className="text-gray-600">Сложность:</span>
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${getDifficultyColor(
                          quiz.difficulty
                        )}`}
                      >
                        {getDifficultyLabel(quiz.difficulty)}
                      </span>
                    </div>

                    <div className="flex items-center justify-between text-sm">
                      <span className="text-gray-600 flex items-center gap-1">
                        <Target className="w-4 h-4" />
                        Вопросов:
                      </span>
                      <span className="font-medium text-gray-900">
                        {quiz.questionCount}
                      </span>
                    </div>

                    <div className="flex items-center justify-between text-sm">
                      <span className="text-gray-600 flex items-center gap-1">
                        <Clock className="w-4 h-4" />
                        Время:
                      </span>
                      <span className="font-medium text-gray-900">
                        {quiz.timeLimit} мин
                      </span>
                    </div>

                    <div className="flex items-center justify-between text-sm">
                      <span className="text-gray-600 flex items-center gap-1">
                        <Award className="w-4 h-4" />
                        Баллов:
                      </span>
                      <span className="font-medium text-gray-900">
                        {quiz.totalPoints}
                      </span>
                    </div>
                  </div>

                  {/* Actions */}
                  <div className="flex gap-2">
                    {(isTeacher || isAdmin) ? (
                      <>
                        <Button
                          variant="primary"
                          size="sm"
                          className="flex-1 flex items-center justify-center gap-2"
                          onClick={() => navigate(`/quizzes/${quiz.id}/stats`)}
                        >
                          <TrendingUp className="w-4 h-4" />
                          Статистика
                        </Button>
                        {quiz.isPublished === false && (
                          <span className="text-xs text-orange-600 bg-orange-100 px-2 py-1 rounded-full">
                            Черновик
                          </span>
                        )}
                      </>
                    ) : (
                      <Button
                        variant="primary"
                        size="sm"
                        className="w-full flex items-center justify-center gap-2"
                        onClick={() => navigate(`/quizzes/${quiz.id}/take`)}
                      >
                        <Play className="w-4 h-4" />
                        Начать тест
                      </Button>
                    )}
                  </div>
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

