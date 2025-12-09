import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { ArrowLeft } from 'lucide-react';
import Button from '../components/common/Button';
import InteractiveFlashcard from '../components/flashcards/InteractiveFlashcard';
import { flashcardService } from '../services/flashcardService';
import { Flashcard, FlashcardSet } from '../types';

type Quality = 0 | 1 | 2 | 3 | 4 | 5;

const FlashcardStudyPage = () => {
  const { setId } = useParams<{ setId: string }>();
  const navigate = useNavigate();
  
  const [flashcardSet, setFlashcardSet] = useState<FlashcardSet | null>(null);
  const [cards, setCards] = useState<Flashcard[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [loading, setLoading] = useState(true);
  const [studiedCount, setStudiedCount] = useState(0);
  const [sessionStats, setSessionStats] = useState({
    correct: 0,
    incorrect: 0,
  });

  useEffect(() => {
    loadFlashcardSet();
  }, [setId]);

  const loadFlashcardSet = async () => {
    try {
      const data = await flashcardService.getSet(Number(setId));
      console.log('Loaded flashcard set:', data);
      console.log('Flashcards in set:', data.flashcards);
      console.log('Flashcards count:', data.flashcards?.length || 0);
      setFlashcardSet(data);
      setCards(data.flashcards || []);
    } catch (error) {
      console.error('Ошибка загрузки набора:', error);
      navigate('/flashcards');
    } finally {
      setLoading(false);
    }
  };

  const handleInteractiveAnswer = async (isCorrect: boolean) => {
    if (!cards[currentIndex]) return;

    const currentCard = cards[currentIndex];
    const quality: Quality = isCorrect ? 5 : 2; // 5 = легко, 2 = сложно

    try {
      await flashcardService.reviewCard({
        flashcardId: currentCard.id,
        quality,
      });

      // Обновляем статистику
      setSessionStats((prev) => ({
        correct: isCorrect ? prev.correct + 1 : prev.correct,
        incorrect: isCorrect ? prev.incorrect : prev.incorrect + 1,
      }));

      setStudiedCount((prev) => prev + 1);

      // Переход к следующей карточке через 2 секунды
      setTimeout(() => {
        if (currentIndex < cards.length - 1) {
          setCurrentIndex((prev) => prev + 1);
        } else {
          navigate('/flashcards', {
            state: { message: 'Поздравляем! Вы завершили изучение набора!' },
          });
        }
      }, 2000);
    } catch (error) {
      console.error('Ошибка отправки оценки:', error);
    }
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
      <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 py-8 px-4">
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
        <div className="grid grid-cols-2 gap-4 mb-8">
          <div className="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg p-4 text-center">
            <div className="text-2xl font-bold text-green-600 dark:text-green-400">
              {sessionStats.correct}
            </div>
            <div className="text-sm text-green-700 dark:text-green-300">Правильно</div>
          </div>
          <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4 text-center">
            <div className="text-2xl font-bold text-red-600 dark:text-red-400">
              {sessionStats.incorrect}
            </div>
            <div className="text-sm text-red-700 dark:text-red-300">Неправильно</div>
          </div>
        </div>

        {/* Интерактивная карточка */}
        <motion.div
          key={currentIndex}
          initial={{ opacity: 0, x: 100 }}
          animate={{ opacity: 1, x: 0 }}
          exit={{ opacity: 0, x: -100 }}
          transition={{ duration: 0.3 }}
          className="bg-white dark:bg-gray-850 rounded-2xl shadow-2xl p-8 mb-8 border-2 border-gray-200 dark:border-gray-700"
        >
          <div className="mb-4 text-sm text-gray-600 dark:text-gray-400">
            Карточка {currentIndex + 1} из {cards.length}
          </div>
          <InteractiveFlashcard
            flashcard={currentCard}
            onAnswer={handleInteractiveAnswer}
          />
        </motion.div>
      </div>
    </div>
  );
};

export default FlashcardStudyPage;
