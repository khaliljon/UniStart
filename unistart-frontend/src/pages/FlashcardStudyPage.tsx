import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { ArrowLeft, RotateCw, Check, X } from 'lucide-react';
import Button from '../components/common/Button';
import { flashcardService } from '../services/flashcardService';
import { Flashcard, FlashcardSet } from '../types';

type Quality = 0 | 1 | 2 | 3 | 4 | 5;

const FlashcardStudyPage = () => {
  const { setId } = useParams<{ setId: string }>();
  const navigate = useNavigate();
  
  const [flashcardSet, setFlashcardSet] = useState<FlashcardSet | null>(null);
  const [cards, setCards] = useState<Flashcard[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isFlipped, setIsFlipped] = useState(false);
  const [loading, setLoading] = useState(true);
  const [studiedCount, setStudiedCount] = useState(0);
  const [sessionStats, setSessionStats] = useState({
    easy: 0,
    good: 0,
    hard: 0,
  });

  useEffect(() => {
    loadFlashcardSet();
  }, [setId]);

  const loadFlashcardSet = async () => {
    try {
      const data = await flashcardService.getSet(Number(setId));
      setFlashcardSet(data);
      setCards(data.flashcards || []);
    } catch (error) {
      console.error('Ошибка загрузки набора:', error);
      navigate('/flashcards');
    } finally {
      setLoading(false);
    }
  };

  const handleAnswer = async (quality: Quality) => {
    if (!cards[currentIndex]) return;

    const currentCard = cards[currentIndex];

    // Отправляем оценку на backend
    try {
      await flashcardService.reviewCard({
        flashcardId: currentCard.id,
        quality,
      });

      // Обновляем статистику сессии
      if (quality >= 4) {
        setSessionStats((prev) => ({ ...prev, easy: prev.easy + 1 }));
      } else if (quality === 3) {
        setSessionStats((prev) => ({ ...prev, good: prev.good + 1 }));
      } else {
        setSessionStats((prev) => ({ ...prev, hard: prev.hard + 1 }));
      }

      setStudiedCount((prev) => prev + 1);

      // Переход к следующей карточке
      if (currentIndex < cards.length - 1) {
        setCurrentIndex((prev) => prev + 1);
        setIsFlipped(false);
      } else {
        // Завершили изучение всех карточек
        navigate('/flashcards', {
          state: { message: 'Поздравляем! Вы завершили изучение набора!' },
        });
      }
    } catch (error) {
      console.error('Ошибка отправки оценки:', error);
    }
  };

  const handleFlip = () => {
    setIsFlipped(!isFlipped);
  };

  const currentCard = cards[currentIndex];
  const progress = cards.length > 0 ? (studiedCount / cards.length) * 100 : 0;

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <div className="text-xl text-gray-600">Загрузка карточек...</div>
      </div>
    );
  }

  if (!flashcardSet || cards.length === 0) {
    return (
      <div className="max-w-2xl mx-auto p-8 text-center">
        <h2 className="text-2xl font-bold text-gray-900 mb-4">
          Карточки не найдены
        </h2>
        <Button onClick={() => navigate('/flashcards')}>
          Вернуться к наборам
        </Button>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 py-8 px-4">
      <div className="max-w-4xl mx-auto">
        {/* Заголовок и навигация */}
        <div className="flex items-center justify-between mb-8">
          <Button
            variant="secondary"
            onClick={() => navigate('/flashcards')}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Назад
          </Button>
          <h1 className="text-2xl font-bold text-gray-900">
            {flashcardSet.title}
          </h1>
          <div className="w-24" /> {/* Spacer для центрирования */}
        </div>

        {/* Прогресс-бар */}
        <div className="mb-8">
          <div className="flex items-center justify-between mb-2">
            <span className="text-sm font-medium text-gray-700">
              Прогресс изучения
            </span>
            <span className="text-sm font-medium text-gray-700">
              {studiedCount} / {cards.length}
            </span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-3 overflow-hidden">
            <motion.div
              className="h-full bg-gradient-to-r from-primary-500 to-primary-600"
              initial={{ width: 0 }}
              animate={{ width: `${progress}%` }}
              transition={{ duration: 0.5 }}
            />
          </div>
        </div>

        {/* Статистика сессии */}
        <div className="grid grid-cols-3 gap-4 mb-8">
          <div className="bg-green-50 border border-green-200 rounded-lg p-4 text-center">
            <div className="text-2xl font-bold text-green-600">
              {sessionStats.easy}
            </div>
            <div className="text-sm text-green-700">Легко</div>
          </div>
          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 text-center">
            <div className="text-2xl font-bold text-yellow-600">
              {sessionStats.good}
            </div>
            <div className="text-sm text-yellow-700">Нормально</div>
          </div>
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center">
            <div className="text-2xl font-bold text-red-600">
              {sessionStats.hard}
            </div>
            <div className="text-sm text-red-700">Сложно</div>
          </div>
        </div>

        {/* Карточка с 3D flip */}
        <div className="perspective-1000 mb-8">
          <AnimatePresence mode="wait">
            <motion.div
              key={currentIndex}
              initial={{ opacity: 0, scale: 0.8 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.8 }}
              transition={{ duration: 0.3 }}
              className="relative"
            >
              <div
                className="relative w-full h-96 cursor-pointer"
                onClick={handleFlip}
                style={{ perspective: '1000px' }}
              >
                <motion.div
                  className="w-full h-full relative"
                  animate={{ rotateY: isFlipped ? 180 : 0 }}
                  transition={{ duration: 0.6, type: 'spring' }}
                  style={{
                    transformStyle: 'preserve-3d',
                  }}
                >
                  {/* Лицевая сторона (вопрос) */}
                  <div
                    className="absolute w-full h-full bg-white rounded-2xl shadow-2xl p-8 flex flex-col items-center justify-center border-2 border-primary-200"
                    style={{
                      backfaceVisibility: 'hidden',
                    }}
                  >
                    <div className="text-sm text-primary-600 font-medium mb-4">
                      ВОПРОС
                    </div>
                    <div className="text-2xl font-bold text-gray-900 text-center mb-8">
                      {currentCard.question}
                    </div>
                    <div className="flex items-center gap-2 text-gray-500 text-sm">
                      <RotateCw className="w-4 h-4" />
                      <span>Нажмите, чтобы перевернуть</span>
                    </div>
                  </div>

                  {/* Обратная сторона (ответ) */}
                  <div
                    className="absolute w-full h-full bg-gradient-to-br from-primary-500 to-primary-600 rounded-2xl shadow-2xl p-8 flex flex-col items-center justify-center text-white"
                    style={{
                      backfaceVisibility: 'hidden',
                      transform: 'rotateY(180deg)',
                    }}
                  >
                    <div className="text-sm font-medium mb-4 opacity-90">
                      ОТВЕТ
                    </div>
                    <div className="text-2xl font-bold text-center mb-4">
                      {currentCard.answer}
                    </div>
                    {currentCard.explanation && (
                      <div className="text-sm opacity-90 text-center max-w-lg mt-4 p-4 bg-white/10 rounded-lg">
                        {currentCard.explanation}
                      </div>
                    )}
                  </div>
                </motion.div>
              </div>
            </motion.div>
          </AnimatePresence>
        </div>

        {/* Кнопки оценки */}
        {isFlipped && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="grid grid-cols-3 gap-4"
          >
            <Button
              onClick={() => handleAnswer(1)}
              className="bg-red-500 hover:bg-red-600 text-white py-4 text-lg font-semibold flex items-center justify-center gap-2"
            >
              <X className="w-5 h-5" />
              Сложно
            </Button>
            <Button
              onClick={() => handleAnswer(3)}
              className="bg-yellow-500 hover:bg-yellow-600 text-white py-4 text-lg font-semibold"
            >
              Нормально
            </Button>
            <Button
              onClick={() => handleAnswer(5)}
              className="bg-green-500 hover:bg-green-600 text-white py-4 text-lg font-semibold flex items-center justify-center gap-2"
            >
              <Check className="w-5 h-5" />
              Легко
            </Button>
          </motion.div>
        )}

        {/* Подсказка */}
        {!isFlipped && (
          <div className="text-center text-gray-500 text-sm mt-8">
            💡 Переверните карточку, чтобы увидеть ответ и оценить, насколько хорошо вы знаете материал
          </div>
        )}
      </div>
    </div>
  );
}

export default FlashcardStudyPage
