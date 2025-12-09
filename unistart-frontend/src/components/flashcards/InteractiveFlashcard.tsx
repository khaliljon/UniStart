import { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Check, X, ChevronUp, ChevronDown } from 'lucide-react';
import { Flashcard, FlashcardType } from '../../types';
import Button from '../common/Button';

interface InteractiveFlashcardProps {
  flashcard: Flashcard;
  onAnswer: (isCorrect: boolean) => void;
}

interface MatchingPair {
  left: string;
  right: string;
}

const InteractiveFlashcard = ({ flashcard, onAnswer }: InteractiveFlashcardProps) => {
  const [userAnswer, setUserAnswer] = useState<string>('');
  const [selectedOption, setSelectedOption] = useState<string>('');
  const [matchingSelections, setMatchingSelections] = useState<{ [key: string]: string }>({});
  const [sequenceItems, setSequenceItems] = useState<string[]>([]);
  const [isAnswered, setIsAnswered] = useState(false);
  const [isCorrect, setIsCorrect] = useState(false);

  // Парсинг JSON данных
  const options = flashcard.optionsJson ? JSON.parse(flashcard.optionsJson) : [];
  const matchingPairs: MatchingPair[] = flashcard.matchingPairsJson 
    ? JSON.parse(flashcard.matchingPairsJson) 
    : [];
  const correctSequence = flashcard.sequenceJson ? JSON.parse(flashcard.sequenceJson) : [];

  useEffect(() => {
    if (flashcard.type === FlashcardType.Sequencing) {
      // Перемешиваем последовательность при загрузке
      setSequenceItems([...correctSequence].sort(() => Math.random() - 0.5));
    }
  }, [flashcard.id]);

  const handleSubmit = () => {
    let correct = false;

    switch (flashcard.type) {
      case FlashcardType.SingleChoice:
        correct = selectedOption === flashcard.answer;
        break;
      
      case FlashcardType.FillInTheBlank:
        correct = userAnswer.trim().toLowerCase() === flashcard.answer.toLowerCase();
        break;
      
      case FlashcardType.Matching:
        correct = matchingPairs.every(pair => matchingSelections[pair.left] === pair.right);
        break;
      
      case FlashcardType.Sequencing:
        correct = JSON.stringify(sequenceItems) === JSON.stringify(correctSequence);
        break;
    }

    setIsCorrect(correct);
    setIsAnswered(true);
    onAnswer(correct);
  };

  const moveSequenceItem = (index: number, direction: 'up' | 'down') => {
    const newItems = [...sequenceItems];
    const newIndex = direction === 'up' ? index - 1 : index + 1;
    
    if (newIndex >= 0 && newIndex < newItems.length) {
      [newItems[index], newItems[newIndex]] = [newItems[newIndex], newItems[index]];
      setSequenceItems(newItems);
    }
  };

  const renderCardContent = () => {
    switch (flashcard.type) {
      case FlashcardType.SingleChoice:
        return (
          <div className="space-y-3">
            <p className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4">
              {flashcard.question}
            </p>
            <div className="space-y-2">
              {options.map((option: string, index: number) => (
                <motion.button
                  key={index}
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                  onClick={() => {
                    if (!isAnswered) {
                      setSelectedOption(option);
                      // Немедленно показываем результат
                      const correct = option === flashcard.answer;
                      setIsCorrect(correct);
                      setIsAnswered(true);
                      onAnswer(correct);
                    }
                  }}
                  disabled={isAnswered}
                  className={`w-full p-4 text-left rounded-lg border-2 transition-all ${
                    isAnswered
                      ? option === flashcard.answer
                        ? 'border-green-500 bg-green-50 dark:bg-green-900/20'
                        : option === selectedOption
                        ? 'border-red-500 bg-red-50 dark:bg-red-900/20'
                        : 'border-gray-200 dark:border-gray-700'
                      : selectedOption === option
                      ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20'
                      : 'border-gray-200 dark:border-gray-700 hover:border-indigo-300 dark:hover:border-indigo-700'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <span className="text-gray-900 dark:text-gray-100">{option}</span>
                    {isAnswered && option === flashcard.answer && (
                      <Check className="w-5 h-5 text-green-600" />
                    )}
                    {isAnswered && option === selectedOption && option !== flashcard.answer && (
                      <X className="w-5 h-5 text-red-600" />
                    )}
                  </div>
                </motion.button>
              ))}
            </div>
          </div>
        );

      case FlashcardType.FillInTheBlank:
        return (
          <div className="space-y-4">
            <p className="text-lg font-medium text-gray-900 dark:text-gray-100">
              {flashcard.question}
            </p>
            <input
              type="text"
              value={userAnswer}
              onChange={(e) => !isAnswered && setUserAnswer(e.target.value)}
              disabled={isAnswered}
              placeholder="Введите ответ..."
              className={`w-full px-4 py-3 border-2 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 dark:bg-gray-850 dark:text-gray-100 ${
                isAnswered
                  ? isCorrect
                    ? 'border-green-500 bg-green-50 dark:bg-green-900/20'
                    : 'border-red-500 bg-red-50 dark:bg-red-900/20'
                  : 'border-gray-300 dark:border-gray-600'
              }`}
            />
            {isAnswered && !isCorrect && (
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Правильный ответ: <span className="font-semibold text-green-600 dark:text-green-400">{flashcard.answer}</span>
              </p>
            )}
          </div>
        );

      case FlashcardType.Matching:
        const leftItems = matchingPairs.map(p => p.left);
        const rightItems = [...matchingPairs.map(p => p.right)].sort(() => Math.random() - 0.5);
        
        return (
          <div className="space-y-4">
            <p className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4">
              Сопоставьте термины с определениями
            </p>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                {leftItems.map((item, index) => (
                  <div key={index} className="p-3 bg-gray-100 dark:bg-gray-800 rounded-lg">
                    <p className="font-medium text-gray-900 dark:text-gray-100">{item}</p>
                  </div>
                ))}
              </div>
              <div className="space-y-2">
                {leftItems.map((leftItem, index) => (
                  <select
                    key={index}
                    value={matchingSelections[leftItem] || ''}
                    onChange={(e) => !isAnswered && setMatchingSelections({ ...matchingSelections, [leftItem]: e.target.value })}
                    disabled={isAnswered}
                    className={`w-full p-3 border-2 rounded-lg dark:bg-gray-850 dark:text-gray-100 ${
                      isAnswered
                        ? matchingSelections[leftItem] === matchingPairs.find(p => p.left === leftItem)?.right
                          ? 'border-green-500 bg-green-50 dark:bg-green-900/20'
                          : 'border-red-500 bg-red-50 dark:bg-red-900/20'
                        : 'border-gray-300 dark:border-gray-600'
                    }`}
                  >
                    <option value="">Выберите...</option>
                    {rightItems.map((rightItem, idx) => (
                      <option key={idx} value={rightItem}>{rightItem}</option>
                    ))}
                  </select>
                ))}
              </div>
            </div>
          </div>
        );

      case FlashcardType.Sequencing:
        return (
          <div className="space-y-4">
            <p className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4">
              Расставьте элементы в правильном порядке
            </p>
            <div className="space-y-2">
              {sequenceItems.map((item, index) => (
                <div
                  key={index}
                  className={`flex items-center gap-2 p-3 rounded-lg border-2 ${
                    isAnswered
                      ? correctSequence[index] === item
                        ? 'border-green-500 bg-green-50 dark:bg-green-900/20'
                        : 'border-red-500 bg-red-50 dark:bg-red-900/20'
                      : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-850'
                  }`}
                >
                  <span className="font-semibold text-gray-500 dark:text-gray-400 w-6">
                    {index + 1}.
                  </span>
                  <span className="flex-1 text-gray-900 dark:text-gray-100">{item}</span>
                  {!isAnswered && (
                    <div className="flex gap-1">
                      <button
                        onClick={() => moveSequenceItem(index, 'up')}
                        disabled={index === 0}
                        className="p-1 hover:bg-gray-200 dark:hover:bg-gray-700 rounded disabled:opacity-50"
                      >
                        <ChevronUp className="w-5 h-5" />
                      </button>
                      <button
                        onClick={() => moveSequenceItem(index, 'down')}
                        disabled={index === sequenceItems.length - 1}
                        className="p-1 hover:bg-gray-200 dark:hover:bg-gray-700 rounded disabled:opacity-50"
                      >
                        <ChevronDown className="w-5 h-5" />
                      </button>
                    </div>
                  )}
                  {isAnswered && correctSequence[index] === item && (
                    <Check className="w-5 h-5 text-green-600" />
                  )}
                  {isAnswered && correctSequence[index] !== item && (
                    <X className="w-5 h-5 text-red-600" />
                  )}
                </div>
              ))}
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="space-y-6">
      {renderCardContent()}

      {!isAnswered && flashcard.type !== FlashcardType.SingleChoice && (
        <Button
          onClick={handleSubmit}
          disabled={
            (flashcard.type === FlashcardType.FillInTheBlank && !userAnswer.trim()) ||
            (flashcard.type === FlashcardType.Matching && Object.keys(matchingSelections).length < matchingPairs.length)
          }
          className="w-full"
        >
          Проверить ответ
        </Button>
      )}

      <AnimatePresence>
        {isAnswered && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            className={`p-4 rounded-lg border-2 ${
              isCorrect
                ? 'border-green-500 bg-green-50 dark:bg-green-900/20'
                : 'border-red-500 bg-red-50 dark:bg-red-900/20'
            }`}
          >
            <div className="flex items-start gap-3">
              {isCorrect ? (
                <Check className="w-6 h-6 text-green-600 flex-shrink-0 mt-0.5" />
              ) : (
                <X className="w-6 h-6 text-red-600 flex-shrink-0 mt-0.5" />
              )}
              <div className="flex-1">
                <p className={`font-semibold mb-2 ${isCorrect ? 'text-green-900 dark:text-green-100' : 'text-red-900 dark:text-red-100'}`}>
                  {isCorrect ? 'Правильно!' : 'Неправильно'}
                </p>
                {flashcard.explanation && (
                  <p className="text-sm text-gray-700 dark:text-gray-300">
                    {flashcard.explanation}
                  </p>
                )}
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default InteractiveFlashcard;
