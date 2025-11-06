import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  ClipboardCheck,
  Clock,
  Award,
  Trash2,
  Eye,
  CheckCircle,
  XCircle,
  User,
  Search,
  AlertCircle,
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface Test {
  id: number;
  title: string;
  description: string;
  subject: string;
  difficulty: string;
  questionCount: number;
  totalPoints: number;
  maxAttempts: number;
  passingScore: number;
  isPublished: boolean;
  userId: string;
  userName: string;
  createdAt: string;
}

const AdminTestsPage = () => {
  const navigate = useNavigate();
  const [tests, setTests] = useState<Test[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');

  useEffect(() => {
    loadTests();
  }, []);

  const loadTests = async () => {
    try {
      setLoading(true);
      const response = await api.get('/admin/tests');
      setTests(response.data);
    } catch (error) {
      console.error('Ошибка загрузки тестов:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Вы уверены, что хотите удалить этот тест?')) return;

    try {
      await api.delete(`/tests/${id}`);
      setTests(tests.filter(t => t.id !== id));
    } catch (error) {
      console.error('Ошибка удаления теста:', error);
      alert('Не удалось удалить тест');
    }
  };

  const handleTogglePublish = async (id: number, currentStatus: boolean) => {
    try {
      await api.patch(`/tests/${id}/publish`);
      setTests(tests.map(t => 
        t.id === id ? { ...t, isPublished: !currentStatus } : t
      ));
    } catch (error) {
      console.error('Ошибка изменения статуса публикации:', error);
      alert('Не удалось изменить статус публикации');
    }
  };

  const filteredTests = tests.filter(test =>
    test.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    test.description.toLowerCase().includes(searchQuery.toLowerCase()) ||
    test.userName.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'Easy': return 'text-green-600 bg-green-100';
      case 'Medium': return 'text-yellow-600 bg-yellow-100';
      case 'Hard': return 'text-red-600 bg-red-100';
      default: return 'text-gray-600 bg-gray-100';
    }
  };

  const getDifficultyLabel = (difficulty: string) => {
    switch (difficulty) {
      case 'Easy': return 'Легкий';
      case 'Medium': return 'Средний';
      case 'Hard': return 'Сложный';
      default: return difficulty;
    }
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
      {/* Заголовок */}
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
            <ClipboardCheck className="w-8 h-8 text-primary-500" />
            Управление тестами
          </h1>
          <p className="text-gray-600 mt-2">
            Все тесты в системе • Всего: {tests.length}
          </p>
        </div>
      </div>

      {/* Поиск */}
      <Card className="mb-6">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
          <input
            type="text"
            placeholder="Поиск по названию, описанию или автору..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          />
        </div>
      </Card>

      {/* Список тестов */}
      {filteredTests.length === 0 ? (
        <Card className="text-center py-12">
          <ClipboardCheck className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-700 mb-2">
            {searchQuery ? 'Тесты не найдены' : 'Нет тестов в системе'}
          </h3>
          <p className="text-gray-600">
            {searchQuery ? 'Попробуйте изменить параметры поиска' : 'Тесты будут отображаться здесь после создания'}
          </p>
        </Card>
      ) : (
        <div className="space-y-4">
          {filteredTests.map((test, index) => (
            <motion.div
              key={test.id}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.05 }}
            >
              <Card className="hover:shadow-lg transition-shadow">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    {/* Заголовок и статус */}
                    <div className="flex items-start gap-3 mb-2">
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
                      {test.isPublished ? (
                        <span className="flex items-center gap-1 text-green-600 text-sm">
                          <CheckCircle className="w-4 h-4" />
                          Опубликован
                        </span>
                      ) : (
                        <span className="flex items-center gap-1 text-gray-600 text-sm">
                          <XCircle className="w-4 h-4" />
                          Черновик
                        </span>
                      )}
                    </div>

                    {/* Описание */}
                    <p className="text-gray-600 text-sm mb-3">{test.description}</p>

                    {/* Метаданные */}
                    <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                      <div className="flex items-center gap-1">
                        <User className="w-4 h-4" />
                        <span>{test.userName}</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <ClipboardCheck className="w-4 h-4" />
                        <span>{test.questionCount} вопросов</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <Award className="w-4 h-4" />
                        <span>{test.totalPoints} баллов</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <AlertCircle className="w-4 h-4" />
                        <span>{test.maxAttempts} попытки, {test.passingScore}% проходной</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <Clock className="w-4 h-4" />
                        <span>{new Date(test.createdAt).toLocaleDateString('ru-RU')}</span>
                      </div>
                    </div>
                  </div>

                  {/* Кнопки действий */}
                  <div className="flex items-center gap-2 ml-4">
                    <Button
                      onClick={() => navigate(`/tests/${test.id}/stats`)}
                      variant="secondary"
                      className="px-3 py-2"
                      title="Статистика"
                    >
                      <Eye className="w-4 h-4" />
                    </Button>
                    <Button
                      onClick={() => handleTogglePublish(test.id, test.isPublished)}
                      variant={test.isPublished ? 'secondary' : 'success'}
                      className="px-3 py-2"
                      title={test.isPublished ? 'Снять с публикации' : 'Опубликовать'}
                    >
                      {test.isPublished ? <XCircle className="w-4 h-4" /> : <CheckCircle className="w-4 h-4" />}
                    </Button>
                    <Button
                      onClick={() => handleDelete(test.id)}
                      variant="danger"
                      className="px-3 py-2"
                      title="Удалить"
                    >
                      <Trash2 className="w-4 h-4" />
                    </Button>
                  </div>
                </div>
              </Card>
            </motion.div>
          ))}
        </div>
      )}
    </div>
  );
};

export default AdminTestsPage;
