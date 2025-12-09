import { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { motion } from 'framer-motion';
import { BookOpen, Play, Target, Clock, Plus, Trash2, Edit, TrendingUp, Upload, FileX, CheckCircle, AlertCircle } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import { flashcardService } from '../services/flashcardService';
import { FlashcardSet } from '../types';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';

const FlashcardsPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { isTeacher, isAdmin } = useAuth();
  const [sets, setSets] = useState<FlashcardSet[]>([]);
  const [loading, setLoading] = useState(true);
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    loadSets();
    
    // Показываем сообщение об успешном завершении, если оно есть
    if (location.state?.message) {
      setSuccessMessage(location.state.message);
      setTimeout(() => setSuccessMessage(''), 5000);
    }
  }, [location]);

  const loadSets = async () => {
    try {
      const data = await flashcardService.getSets();
      setSets(data);
    } catch (error) {
      console.error('Ошибка загрузки наборов:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number, title: string) => {
    if (!window.confirm(`Вы уверены, что хотите удалить набор "${title}"? Это действие необратимо.`)) {
      return;
    }

    try {
      await flashcardService.deleteSet(id);
      setSuccessMessage('Набор карточек успешно удален');
      setTimeout(() => setSuccessMessage(''), 3000);
      loadSets();
    } catch (error) {
      console.error('Ошибка удаления набора:', error);
      alert('Ошибка при удалении набора');
    }
  };

  const handlePublish = async (id: number) => {
    if (!window.confirm('Вы уверены, что хотите опубликовать этот набор карточек?')) {
      return;
    }
    try {
      await api.patch(`/flashcards/sets/${id}/publish`);
      setSuccessMessage('Набор успешно опубликован');
      setTimeout(() => setSuccessMessage(''), 3000);
      loadSets();
    } catch (error) {
      console.error('Ошибка публикации:', error);
      alert('Не удалось опубликовать набор');
    }
  };

  const handleUnpublish = async (id: number) => {
    if (!window.confirm('Вы уверены, что хотите снять набор с публикации?')) {
      return;
    }
    try {
      await api.patch(`/flashcards/sets/${id}/unpublish`);
      setSuccessMessage('Набор снят с публикации');
      setTimeout(() => setSuccessMessage(''), 3000);
      loadSets();
    } catch (error) {
      console.error('Ошибка отмены публикации:', error);
      alert('Не удалось снять набор с публикации');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-xl text-gray-600">Загрузка наборов...</div>
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
          className="mb-8 flex justify-between items-start"
        >
          <div>
            <h1 className="text-4xl font-bold text-gray-900 mb-2">
              Интерактивные карточки
            </h1>
            <p className="text-gray-600">
              Изучайте материал с помощью алгоритма интервального повторения
            </p>
          </div>
          
          {(isTeacher || isAdmin) && (
            <Button
              variant="primary"
              onClick={() => navigate('/flashcards/create')}
              className="flex items-center gap-2"
            >
              <Plus className="w-5 h-5" />
              Создать набор
            </Button>
          )}
        </motion.div>

        {/* Сообщение об успехе */}
        {successMessage && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0 }}
            className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg text-green-800"
          >
            {successMessage}
          </motion.div>
        )}

        {/* Список наборов */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {sets.length === 0 ? (
            <div className="col-span-full text-center py-12">
              <p className="text-gray-600 text-lg">
                Пока нет доступных наборов карточек
              </p>
            </div>
          ) : (
            sets.map((set, index) => (
              <motion.div
                key={set.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.1 }}
              >
                <Card className="h-full flex flex-col hover:shadow-lg transition-shadow">
                  {/* Заголовок набора */}
                  <div className="flex justify-between items-start mb-3">
                    <h3 className="text-lg font-semibold text-gray-900 flex-1">
                      {set.title}
                    </h3>
                  </div>
                  
                  {/* Описание */}
                  <p className="text-gray-600 text-sm mb-4 flex-1 line-clamp-2">
                    {set.description}
                  </p>

                  {/* Статистика */}
                  <div className="grid grid-cols-2 gap-3 mb-4">
                    <div className="flex items-center gap-2">
                      <BookOpen className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">
                        {set.totalCards || 0} карточек
                      </span>
                    </div>
                    {(set.cardsToReview || 0) > 0 && (
                      <div className="flex items-center gap-2">
                        <Target className="w-4 h-4 text-gray-400" />
                        <span className="text-sm text-gray-600">
                          {set.cardsToReview} к повторению
                        </span>
                      </div>
                    )}
                    <div className="flex items-center gap-2">
                      <Clock className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">
                        {new Date(set.updatedAt).toLocaleDateString('ru-RU')}
                      </span>
                    </div>
                  </div>

                  {/* Кнопки действий */}
                  <div className="flex flex-col gap-2">
                    {(isTeacher || isAdmin) ? (
                      <>
                        <div className="flex gap-2">
                          <Button
                            onClick={() => navigate(`/flashcards/${set.id}/edit`)}
                            variant="secondary"
                            size="sm"
                            className="flex-1 flex items-center justify-center gap-2"
                          >
                            <Edit className="w-4 h-4" />
                            Редактировать
                          </Button>
                          <Button
                            onClick={() => navigate(`/flashcards/${set.id}/stats`)}
                            variant="primary"
                            size="sm"
                            className="flex-1 flex items-center justify-center gap-2"
                          >
                            <TrendingUp className="w-4 h-4" />
                            Статистика
                          </Button>
                        </div>
                        <div className="flex gap-2">
                          {set.isPublished ? (
                            <Button
                              variant="secondary"
                              size="sm"
                              className="flex-1 flex items-center justify-center gap-2"
                              onClick={() => handleUnpublish(set.id)}
                            >
                              <FileX className="w-4 h-4" />
                              Снять с публикации
                            </Button>
                          ) : (
                            <Button
                              variant="primary"
                              size="sm"
                              className="flex-1 flex items-center justify-center gap-2 bg-green-600 hover:bg-green-700"
                              onClick={() => handlePublish(set.id)}
                            >
                              <Upload className="w-4 h-4" />
                              Опубликовать
                            </Button>
                          )}
                          <Button
                            variant="danger"
                            size="sm"
                            onClick={(e) => {
                              e.stopPropagation();
                              handleDelete(set.id, set.title);
                            }}
                            className="px-4"
                            title="Удалить набор"
                          >
                            <Trash2 className="w-4 h-4" />
                          </Button>
                        </div>
                      </>
                    ) : (
                      <Button
                        onClick={() => navigate(`/flashcards/${set.id}/study`)}
                        variant="primary"
                        size="sm"
                        className="w-full flex items-center justify-center gap-2"
                      >
                        <Play className="w-4 h-4" />
                        Начать изучение
                      </Button>
                    )}
                  </div>

                  {/* Дата и статус публикации */}
                  <div className="mt-4 flex items-center justify-between text-xs text-gray-500 dark:text-gray-400 border-t border-gray-200 dark:border-gray-700 pt-3">
                    {(isTeacher || isAdmin) && (
                      <span className="flex items-center gap-1">
                        {set.isPublished ? (
                          <span className="flex items-center gap-1 text-green-600 dark:text-green-500">
                            <CheckCircle className="w-3 h-3" />
                            Опубликован
                          </span>
                        ) : (
                          <span className="flex items-center gap-1 text-gray-600 dark:text-gray-400">
                            <AlertCircle className="w-3 h-3" />
                            Черновик
                          </span>
                        )}
                      </span>
                    )}
                    {!(isTeacher || isAdmin) && <div />}
                    <span>{new Date(set.updatedAt).toLocaleDateString('ru-RU')}</span>
                  </div>
                </Card>
              </motion.div>
            ))
          )}
        </div>

        {/* Информационная панель */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="mt-12 bg-gradient-to-r from-primary-50 to-blue-50 dark:from-gray-800 dark:to-gray-850 border border-primary-200 dark:border-gray-700 rounded-lg p-6"
        >
          <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-2">
            💡 Как работает интервальное повторение?
          </h3>
          <p className="text-gray-700 dark:text-gray-300 mb-4">
            Мы используем алгоритм SM-2 (SuperMemo 2), который автоматически определяет оптимальное время для повторения каждой карточки на основе вашей оценки.
          </p>
          <ul className="space-y-2 text-sm text-gray-700 dark:text-gray-300">
            <li className="flex items-start gap-2">
              <span className="text-green-600 dark:text-green-400 font-bold">✓</span>
              <span><strong>Легко</strong> — карточка будет показана через большой интервал времени</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-yellow-600 dark:text-yellow-400 font-bold">✓</span>
              <span><strong>Нормально</strong> — карточка будет показана через средний интервал</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-red-600 dark:text-red-400 font-bold">✓</span>
              <span><strong>Сложно</strong> — карточка будет показана снова в ближайшее время</span>
            </li>
          </ul>
        </motion.div>
      </div>
    </div>
  );
}

export default FlashcardsPage
