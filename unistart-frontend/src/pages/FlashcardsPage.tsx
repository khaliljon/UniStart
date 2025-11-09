import { useEffect, useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { motion } from 'framer-motion';
import { BookOpen, Play, Target, Clock, Plus } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import { flashcardService } from '../services/flashcardService';
import { FlashcardSet } from '../types';
import { useAuth } from '../context/AuthContext';

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

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-xl text-gray-600">Загрузка наборов...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 py-8 px-4">
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
                <Card className="h-full hover:shadow-xl transition-all duration-300 overflow-hidden group">
                  {/* Цветная полоска сверху */}
                  <div className="h-2 bg-gradient-to-r from-primary-500 to-primary-600" />
                  
                  <div className="p-6">
                    {/* Заголовок */}
                    <h3 className="text-xl font-bold text-gray-900 mb-2 group-hover:text-primary-600 transition-colors">
                      {set.title}
                    </h3>
                    
                    {/* Описание */}
                    <p className="text-gray-600 text-sm mb-4 line-clamp-2">
                      {set.description}
                    </p>

                    {/* Статистика */}
                    <div className="space-y-2 mb-6">
                      <div className="flex items-center gap-2 text-sm text-gray-600">
                        <BookOpen className="w-4 h-4 text-primary-500" />
                        <span>
                          Всего карточек: <strong>{set.totalCards || 0}</strong>
                        </span>
                      </div>
                      
                      {(set.cardsToReview || 0) > 0 && (
                        <div className="flex items-center gap-2 text-sm">
                          <Target className="w-4 h-4 text-orange-500" />
                          <span className="text-orange-600 font-medium">
                            К повторению: {set.cardsToReview}
                          </span>
                        </div>
                      )}

                      <div className="flex items-center gap-2 text-sm text-gray-500">
                        <Clock className="w-4 h-4" />
                        <span>
                          Обновлено: {new Date(set.updatedAt).toLocaleDateString('ru-RU')}
                        </span>
                      </div>
                    </div>

                    {/* Кнопка изучения */}
                    <Button
                      onClick={() => navigate(`/flashcards/${set.id}/study`)}
                      className="w-full flex items-center justify-center gap-2 group-hover:scale-105 transition-transform"
                    >
                      <Play className="w-4 h-4" />
                      Начать изучение
                    </Button>
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
          className="mt-12 bg-gradient-to-r from-primary-50 to-blue-50 border border-primary-200 rounded-lg p-6"
        >
          <h3 className="text-lg font-bold text-gray-900 mb-2">
            💡 Как работает интервальное повторение?
          </h3>
          <p className="text-gray-700 mb-4">
            Мы используем алгоритм SM-2 (SuperMemo 2), который автоматически определяет оптимальное время для повторения каждой карточки на основе вашей оценки.
          </p>
          <ul className="space-y-2 text-sm text-gray-700">
            <li className="flex items-start gap-2">
              <span className="text-green-600 font-bold">✓</span>
              <span><strong>Легко</strong> — карточка будет показана через большой интервал времени</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-yellow-600 font-bold">✓</span>
              <span><strong>Нормально</strong> — карточка будет показана через средний интервал</span>
            </li>
            <li className="flex items-start gap-2">
              <span className="text-red-600 font-bold">✓</span>
              <span><strong>Сложно</strong> — карточка будет показана снова в ближайшее время</span>
            </li>
          </ul>
        </motion.div>
      </div>
    </div>
  );
}

export default FlashcardsPage
