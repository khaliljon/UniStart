import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  Download,
  FileText,
  Calendar,
  CheckCircle,
  ArrowLeft,
  Loader,
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface Quiz {
  id: number;
  title: string;
  subject: string;
  questionCount: number;
  isPublic: boolean;
  createdAt: string;
}

const TeacherExportPage = () => {
  const navigate = useNavigate();
  const [quizzes, setQuizzes] = useState<Quiz[]>([]);
  const [loading, setLoading] = useState(true);
  const [exportingQuizId, setExportingQuizId] = useState<number | null>(null);
  const [exportSuccess, setExportSuccess] = useState<number | null>(null);

  useEffect(() => {
    loadQuizzes();
  }, []);

  const loadQuizzes = async () => {
    try {
      const response = await api.get('/teacher/quizzes/my');
      setQuizzes(response.data);
    } catch (error) {
      console.error('Ошибка загрузки тестов:', error);
    } finally {
      setLoading(false);
    }
  };

  const exportQuizResults = async (quizId: number, quizTitle: string) => {
    setExportingQuizId(quizId);
    setExportSuccess(null);

    try {
      const response = await api.get(`/teacher/quizzes/${quizId}/export-results`, {
        responseType: 'blob',
      });

      // Создаём ссылку для скачивания
      const blob = new Blob([response.data], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);
      
      const fileName = `Quiz_${quizId}_${quizTitle.replace(/\s+/g, '_')}_Results_${new Date().toISOString().split('T')[0]}.csv`;
      
      link.setAttribute('href', url);
      link.setAttribute('download', fileName);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);

      setExportSuccess(quizId);
      setTimeout(() => setExportSuccess(null), 3000);
    } catch (error: any) {
      console.error('Ошибка экспорта результатов:', error);
      if (error.response?.status === 404) {
        alert('Тест не найден или у вас нет доступа');
      } else {
        alert('Не удалось экспортировать результаты. Попробуйте позже.');
      }
    } finally {
      setExportingQuizId(null);
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('ru-RU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
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
            className="mb-4 flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Назад к панели
          </Button>
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-4xl font-bold text-gray-900 mb-2">
                📥 Экспорт результатов
              </h1>
              <p className="text-gray-600">
                Скачайте результаты тестов в формате CSV для анализа
              </p>
            </div>
          </div>
        </motion.div>

        {/* Info Card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="mb-8"
        >
          <Card className="p-6 bg-blue-50 border-blue-200">
            <div className="flex items-start gap-4">
              <FileText className="w-8 h-8 text-blue-500 flex-shrink-0" />
              <div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  Формат экспорта
                </h3>
                <p className="text-gray-600 text-sm mb-2">
                  CSV файл будет содержать следующие колонки:
                </p>
                <ul className="text-sm text-gray-600 space-y-1">
                  <li>• Email студента</li>
                  <li>• Имя пользователя</li>
                  <li>• Набранные баллы</li>
                  <li>• Максимальные баллы</li>
                  <li>• Процент правильных ответов</li>
                  <li>• Время выполнения (в секундах)</li>
                  <li>• Дата и время завершения</li>
                </ul>
              </div>
            </div>
          </Card>
        </motion.div>

        {/* Quizzes List */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
        >
          <Card className="p-6">
            <h2 className="text-2xl font-bold text-gray-900 mb-6">
              Ваши тесты
            </h2>

            {quizzes.length === 0 ? (
              <div className="text-center py-12">
                <FileText className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                <p className="text-gray-500 text-lg mb-2">
                  У вас пока нет тестов
                </p>
                <p className="text-gray-400 mb-6">
                  Создайте тест, чтобы начать собирать результаты
                </p>
                <Button onClick={() => navigate('/quizzes/create')}>
                  Создать тест
                </Button>
              </div>
            ) : (
              <div className="grid grid-cols-1 gap-4">
                {quizzes.map((quiz) => (
                  <motion.div
                    key={quiz.id}
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    className="border border-gray-200 rounded-lg p-6 hover:shadow-md transition-shadow"
                  >
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <div className="flex items-center gap-3 mb-2">
                          <h3 className="text-lg font-semibold text-gray-900">
                            {quiz.title}
                          </h3>
                          {quiz.isPublic && (
                            <span className="px-2 py-1 bg-green-100 text-green-800 text-xs rounded-full">
                              Публичный
                            </span>
                          )}
                        </div>
                        <div className="flex items-center gap-6 text-sm text-gray-600">
                          <div className="flex items-center gap-1">
                            <FileText className="w-4 h-4" />
                            <span>Предмет: {quiz.subject}</span>
                          </div>
                          <div className="flex items-center gap-1">
                            <FileText className="w-4 h-4" />
                            <span>Вопросов: {quiz.questionCount}</span>
                          </div>
                          <div className="flex items-center gap-1">
                            <Calendar className="w-4 h-4" />
                            <span>{formatDate(quiz.createdAt)}</span>
                          </div>
                        </div>
                      </div>

                      <div className="flex items-center gap-3">
                        {exportSuccess === quiz.id && (
                          <motion.div
                            initial={{ scale: 0 }}
                            animate={{ scale: 1 }}
                            className="flex items-center gap-2 text-green-600"
                          >
                            <CheckCircle className="w-5 h-5" />
                            <span className="text-sm font-medium">Скачано</span>
                          </motion.div>
                        )}
                        
                        <Button
                          onClick={() => exportQuizResults(quiz.id, quiz.title)}
                          disabled={exportingQuizId === quiz.id}
                          className="flex items-center gap-2"
                        >
                          {exportingQuizId === quiz.id ? (
                            <>
                              <Loader className="w-4 h-4 animate-spin" />
                              <span>Экспорт...</span>
                            </>
                          ) : (
                            <>
                              <Download className="w-4 h-4" />
                              <span>Экспорт CSV</span>
                            </>
                          )}
                        </Button>
                      </div>
                    </div>
                  </motion.div>
                ))}
              </div>
            )}
          </Card>
        </motion.div>

        {/* Additional Info */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="mt-8"
        >
          <Card className="p-6 bg-gray-50">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              💡 Советы по использованию
            </h3>
            <ul className="space-y-2 text-gray-600">
              <li className="flex items-start gap-2">
                <span className="text-primary-500 font-bold">•</span>
                <span>
                  CSV файлы можно открыть в Excel, Google Sheets или любом текстовом
                  редакторе
                </span>
              </li>
              <li className="flex items-start gap-2">
                <span className="text-primary-500 font-bold">•</span>
                <span>
                  Используйте экспорт для создания детальных отчётов и графиков
                </span>
              </li>
              <li className="flex items-start gap-2">
                <span className="text-primary-500 font-bold">•</span>
                <span>
                  Файлы сохраняются с текущей датой в имени для удобной организации
                </span>
              </li>
              <li className="flex items-start gap-2">
                <span className="text-primary-500 font-bold">•</span>
                <span>
                  Экспортируйте данные регулярно для отслеживания прогресса во времени
                </span>
              </li>
            </ul>
          </Card>
        </motion.div>
      </div>
    </div>
  );
};

export default TeacherExportPage;
