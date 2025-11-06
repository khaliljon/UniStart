import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  FileText,
  Clock,
  Award,
  Play,
  Plus,
  Search,
  TrendingUp,
  BookOpen,
  AlertCircle,
  CheckCircle,
  Lock,
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';

interface Test {
  id: number;
  title: string;
  description: string;
  subject: string;
  difficulty: string;
  timeLimit: number;
  questionCount: number;
  totalPoints: number;
  maxAttempts: number;
  passingScore: number;
  isProctored: boolean;
  isPublished?: boolean;
  startDate?: string;
  endDate?: string;
  createdAt: string;
}

const TestsPage = () => {
  const navigate = useNavigate();
  const { isTeacher, isAdmin } = useAuth();
  const [tests, setTests] = useState<Test[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [subjectFilter, setSubjectFilter] = useState<string>('');
  const [difficultyFilter, setDifficultyFilter] = useState<string>('');

  useEffect(() => {
    loadTests();
  }, [subjectFilter, difficultyFilter]);

  const loadTests = async () => {
    try {
      setLoading(true);
      
      // Учителя и админы видят свои тесты, студенты - все опубликованные
      const endpoint = (isTeacher || isAdmin) ? '/tests/my' : '/tests';
      
      const params = new URLSearchParams();
      if (subjectFilter) params.append('subject', subjectFilter);
      if (difficultyFilter) params.append('difficulty', difficultyFilter);
      
      const response = await api.get(`${endpoint}?${params.toString()}`);
      setTests(response.data);
    } catch (error) {
      console.error('Ошибка загрузки тестов:', error);
    } finally {
      setLoading(false);
    }
  };

  const filteredTests = tests.filter(test =>
    test.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    test.description.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'Easy':
        return 'text-green-600 bg-green-100';
      case 'Medium':
        return 'text-yellow-600 bg-yellow-100';
      case 'Hard':
        return 'text-red-600 bg-red-100';
      default:
        return 'text-gray-600 bg-gray-100';
    }
  };

  const getDifficultyLabel = (difficulty: string) => {
    switch (difficulty) {
      case 'Easy':
        return 'Легкий';
      case 'Medium':
        return 'Средний';
      case 'Hard':
        return 'Сложный';
      default:
        return difficulty;
    }
  };

  const isTestAvailable = (test: Test) => {
    const now = new Date();
    if (test.startDate && new Date(test.startDate) > now) return false;
    if (test.endDate && new Date(test.endDate) < now) return false;
    return true;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <motion.div
          animate={{ rotate: 360 }}
          transition={{ duration: 1, repeat: Infinity, ease: 'linear' }}
          className="w-16 h-16 border-4 border-primary-500 border-t-transparent rounded-full"
        />
      </div>
    );
  }

  return (
    <div className="p-6 max-w-7xl mx-auto">
      {/* Заголовок и кнопка создания */}
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
            <FileText className="w-8 h-8 text-primary-500" />
            {(isTeacher || isAdmin) ? 'Мои Тесты' : 'Доступные Тесты'}
          </h1>
          <p className="text-gray-600 mt-2">
            {(isTeacher || isAdmin)
              ? 'Управляйте своими тестами и отслеживайте результаты студентов'
              : 'Проверьте свои знания с помощью тестов'}
          </p>
        </div>
        {(isTeacher || isAdmin) && (
          <Button
            onClick={() => navigate('/tests/create')}
            variant="primary"
            className="flex items-center gap-2"
          >
            <Plus className="w-5 h-5" />
            Создать тест
          </Button>
        )}
      </div>

      {/* Поиск и фильтры */}
      <Card className="mb-6">
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="Поиск тестов..."
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
            <option value="Математика">Математика</option>
            <option value="Физика">Физика</option>
            <option value="Информатика">Информатика</option>
            <option value="История">История</option>
            <option value="Литература">Литература</option>
            <option value="Английский">Английский</option>
          </select>
          <select
            value={difficultyFilter}
            onChange={(e) => setDifficultyFilter(e.target.value)}
            className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          >
            <option value="">Все уровни</option>
            <option value="Easy">Легкий</option>
            <option value="Medium">Средний</option>
            <option value="Hard">Сложный</option>
          </select>
        </div>
      </Card>

      {/* Список тестов */}
      {filteredTests.length === 0 ? (
        <Card className="text-center py-12">
          <FileText className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-700 mb-2">
            {searchQuery || subjectFilter || difficultyFilter
              ? 'Тесты не найдены'
              : 'Нет доступных тестов'}
          </h3>
          <p className="text-gray-600 mb-6">
            {(isTeacher || isAdmin)
              ? 'Создайте свой первый тест для студентов'
              : 'Пока нет доступных тестов для прохождения'}
          </p>
          {(isTeacher || isAdmin) && (
            <Button
              onClick={() => navigate('/tests/create')}
              variant="primary"
              className="inline-flex items-center gap-2"
            >
              <Plus className="w-5 h-5" />
              Создать тест
            </Button>
          )}
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredTests.map((test, index) => {
            const available = isTestAvailable(test);
            
            return (
              <motion.div
                key={test.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.1 }}
              >
                <Card className="h-full flex flex-col hover:shadow-lg transition-shadow">
                  {/* Заголовок теста */}
                  <div className="flex justify-between items-start mb-3">
                    <h3 className="text-lg font-semibold text-gray-900 flex-1">
                      {test.title}
                    </h3>
                    <span
                      className={`px-2 py-1 rounded-full text-xs font-medium ${getDifficultyColor(
                        test.difficulty
                      )}`}
                    >
                      {getDifficultyLabel(test.difficulty)}
                    </span>
                  </div>

                  {/* Описание */}
                  <p className="text-gray-600 text-sm mb-4 flex-1">
                    {test.description}
                  </p>

                  {/* Предмет */}
                  <div className="flex items-center gap-2 mb-3">
                    <BookOpen className="w-4 h-4 text-gray-400" />
                    <span className="text-sm text-gray-700">{test.subject}</span>
                  </div>

                  {/* Статистика */}
                  <div className="grid grid-cols-2 gap-3 mb-4">
                    <div className="flex items-center gap-2">
                      <FileText className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">
                        {test.questionCount} вопросов
                      </span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Clock className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">{test.timeLimit} мин</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Award className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">
                        {test.totalPoints} баллов
                      </span>
                    </div>
                    <div className="flex items-center gap-2">
                      <TrendingUp className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">
                        {test.passingScore}% проходной
                      </span>
                    </div>
                  </div>

                  {/* Особенности теста */}
                  <div className="flex flex-wrap gap-2 mb-4">
                    <span className="px-2 py-1 bg-blue-100 text-blue-700 rounded text-xs flex items-center gap-1">
                      <AlertCircle className="w-3 h-3" />
                      {test.maxAttempts} {test.maxAttempts === 1 ? 'попытка' : 'попытки'}
                    </span>
                    {test.isProctored && (
                      <span className="px-2 py-1 bg-purple-100 text-purple-700 rounded text-xs flex items-center gap-1">
                        <Lock className="w-3 h-3" />
                        С контролем
                      </span>
                    )}
                    {!available && (
                      <span className="px-2 py-1 bg-gray-100 text-gray-700 rounded text-xs flex items-center gap-1">
                        <Clock className="w-3 h-3" />
                        Недоступен
                      </span>
                    )}
                  </div>

                  {/* Кнопки действий */}
                  {(isTeacher || isAdmin) ? (
                    <div className="flex gap-2">
                      <Button
                        onClick={() => navigate(`/tests/${test.id}/stats`)}
                        variant="secondary"
                        className="flex-1 flex items-center justify-center gap-2"
                      >
                        <TrendingUp className="w-4 h-4" />
                        Статистика
                      </Button>
                      <Button
                        onClick={() => navigate(`/tests/${test.id}/edit`)}
                        variant="secondary"
                        className="px-4"
                      >
                        Редактировать
                      </Button>
                    </div>
                  ) : (
                    <Button
                      onClick={() => navigate(`/tests/${test.id}/take`)}
                      variant="primary"
                      className="w-full flex items-center justify-center gap-2"
                      disabled={!available}
                    >
                      {available ? (
                        <>
                          <Play className="w-4 h-4" />
                          Начать тест
                        </>
                      ) : (
                        <>
                          <Lock className="w-4 h-4" />
                          Недоступен
                        </>
                      )}
                    </Button>
                  )}

                  {/* Статус публикации для преподавателей */}
                  {(isTeacher || isAdmin) && (
                    <div className="mt-3 pt-3 border-t border-gray-200 flex items-center justify-between">
                      <span className="text-xs text-gray-500">
                        {test.isPublished ? (
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
                        {new Date(test.createdAt).toLocaleDateString('ru-RU')}
                      </span>
                    </div>
                  )}
                </Card>
              </motion.div>
            );
          })}
        </div>
      )}
    </div>
  );
};

export default TestsPage;
