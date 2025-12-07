import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  FileText,
  Clock,
  Award,
  Trash2,
  Eye,
  CheckCircle,
  XCircle,
  User,
  Search,
  ArrowLeft,
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface Quiz {
  id: number;
  title: string;
  description: string;
  subject: string;
  difficulty: string;
  questionCount: number;
  totalPoints: number;
  isPublished: boolean;
  userId: string;
  userName: string;
  createdAt: string;
}

const AdminQuizzesPage = () => {
  const navigate = useNavigate();
  const [quizzes, setQuizzes] = useState<Quiz[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');

  useEffect(() => {
    loadQuizzes();
  }, []);

  const loadQuizzes = async () => {
    try {
      setLoading(true);
      const response = await api.get('/admin/quizzes');
      setQuizzes(response.data);
    } catch (error) {
      console.error('Ошибка загрузки квизов:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Вы уверены, что хотите удалить этот квиз?')) return;

    try {
      await api.delete(`/quizzes/${id}`);
      setQuizzes(quizzes.filter(q => q.id !== id));
    } catch (error) {
      console.error('Ошибка удаления квиза:', error);
      alert('Не удалось удалить квиз');
    }
  };

  const handleTogglePublish = async (id: number, currentStatus: boolean) => {
    try {
      await api.patch(`/quizzes/${id}/publish`, !currentStatus);
      setQuizzes(quizzes.map(q => 
        q.id === id ? { ...q, isPublished: !currentStatus } : q
      ));
    } catch (error) {
      console.error('Ошибка изменения статуса публикации:', error);
      alert('Не удалось изменить статус публикации');
    }
  };

  const filteredQuizzes = quizzes.filter(quiz =>
    quiz.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
    quiz.description.toLowerCase().includes(searchQuery.toLowerCase()) ||
    quiz.userName.toLowerCase().includes(searchQuery.toLowerCase())
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
      <Button 
        variant="secondary" 
        className="mb-4 flex items-center gap-2"
        onClick={() => navigate('/dashboard')}
      >
        <ArrowLeft className="w-4 h-4" />
        Назад к панели
      </Button>

      {/* Заголовок */}
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
            <FileText className="w-8 h-8 text-primary-500" />
            Управление квизами
          </h1>
          <p className="text-gray-600 mt-2">
            Все квизы в системе • Всего: {quizzes.length}
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

      {/* Список квизов */}
      {filteredQuizzes.length === 0 ? (
        <Card className="text-center py-12">
          <FileText className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-700 mb-2">
            {searchQuery ? 'Квизы не найдены' : 'Нет квизов в системе'}
          </h3>
          <p className="text-gray-600">
            {searchQuery ? 'Попробуйте изменить параметры поиска' : 'Квизы будут отображаться здесь после создания'}
          </p>
        </Card>
      ) : (
        <div className="space-y-4">
          {filteredQuizzes.map((quiz, index) => (
            <motion.div
              key={quiz.id}
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
                        {quiz.title}
                      </h3>
                      <span
                        className={`px-2 py-1 rounded-full text-xs font-medium ${getDifficultyColor(
                          quiz.difficulty
                        )}`}
                      >
                        {getDifficultyLabel(quiz.difficulty)}
                      </span>
                      {quiz.isPublished ? (
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
                    <p className="text-gray-600 text-sm mb-3">{quiz.description}</p>

                    {/* Метаданные */}
                    <div className="flex flex-wrap items-center gap-4 text-sm text-gray-600">
                      <div className="flex items-center gap-1">
                        <User className="w-4 h-4" />
                        <span>{quiz.userName}</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <FileText className="w-4 h-4" />
                        <span>{quiz.questionCount} вопросов</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <Award className="w-4 h-4" />
                        <span>{quiz.totalPoints} баллов</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <Clock className="w-4 h-4" />
                        <span>{new Date(quiz.createdAt).toLocaleDateString('ru-RU')}</span>
                      </div>
                    </div>
                  </div>

                  {/* Кнопки действий */}
                  <div className="flex items-center gap-2 ml-4">
                    <Button
                      onClick={() => navigate(`/quizzes/${quiz.id}/stats`)}
                      variant="secondary"
                      className="px-3 py-2"
                      title="Статистика"
                    >
                      <Eye className="w-4 h-4" />
                    </Button>
                    <Button
                      onClick={() => handleTogglePublish(quiz.id, quiz.isPublished)}
                      variant={quiz.isPublished ? 'secondary' : 'success'}
                      className="px-3 py-2"
                      title={quiz.isPublished ? 'Снять с публикации' : 'Опубликовать'}
                    >
                      {quiz.isPublished ? <XCircle className="w-4 h-4" /> : <CheckCircle className="w-4 h-4" />}
                    </Button>
                    <Button
                      onClick={() => handleDelete(quiz.id)}
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

export default AdminQuizzesPage;
