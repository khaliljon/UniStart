import { useEffect, useState, useCallback } from 'react';
import { Clock, AlertCircle } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

interface ExamTimerProps {
  timeLimit: number; // в минутах
  strictTiming: boolean;
  onTimeUp: () => void;
}

const ExamTimer = ({ timeLimit, strictTiming, onTimeUp }: ExamTimerProps) => {
  const [timeRemaining, setTimeRemaining] = useState(timeLimit * 60); // конвертируем в секунды
  const [isWarning, setIsWarning] = useState(false);
  const [isCritical, setIsCritical] = useState(false);

  useEffect(() => {
    if (timeRemaining <= 0) {
      if (strictTiming) {
        onTimeUp();
      }
      return;
    }

    const timer = setInterval(() => {
      setTimeRemaining((prev) => {
        const newTime = prev - 1;
        
        // Предупреждение за 5 минут
        if (newTime === 300) {
          setIsWarning(true);
        }
        
        // Критическое предупреждение за 1 минуту
        if (newTime === 60) {
          setIsCritical(true);
        }
        
        return newTime;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [timeRemaining, strictTiming, onTimeUp]);

  const formatTime = useCallback((seconds: number) => {
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    
    if (hrs > 0) {
      return `${hrs}:${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    }
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }, []);

  const getTimerColor = () => {
    if (isCritical) return 'text-red-600 dark:text-red-400';
    if (isWarning) return 'text-yellow-600 dark:text-yellow-400';
    return 'text-gray-700 dark:text-gray-300';
  };

  const getBackgroundColor = () => {
    if (isCritical) return 'bg-red-50 dark:bg-red-900/20 border-red-300 dark:border-red-700';
    if (isWarning) return 'bg-yellow-50 dark:bg-yellow-900/20 border-yellow-300 dark:border-yellow-700';
    return 'bg-white dark:bg-gray-850 border-gray-200 dark:border-gray-700';
  };

  const progress = (timeRemaining / (timeLimit * 60)) * 100;

  return (
    <div className={`fixed top-20 right-4 z-50 p-4 rounded-lg border-2 shadow-lg ${getBackgroundColor()} transition-all duration-300`}>
      <div className="flex items-center gap-3">
        <Clock className={`w-6 h-6 ${getTimerColor()}`} />
        <div>
          <div className={`text-2xl font-bold font-mono ${getTimerColor()}`}>
            {formatTime(timeRemaining)}
          </div>
          <div className="text-xs text-gray-600 dark:text-gray-400">
            {strictTiming ? 'Автозавершение' : 'Осталось времени'}
          </div>
        </div>
      </div>

      {/* Прогресс-бар */}
      <div className="mt-3 w-48 h-1.5 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
        <motion.div
          className={`h-full ${
            isCritical 
              ? 'bg-red-500' 
              : isWarning 
              ? 'bg-yellow-500' 
              : 'bg-indigo-500'
          }`}
          initial={{ width: '100%' }}
          animate={{ width: `${progress}%` }}
          transition={{ duration: 0.5 }}
        />
      </div>

      {/* Предупреждения */}
      <AnimatePresence>
        {isCritical && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            className="mt-2 flex items-center gap-2 text-xs text-red-700 dark:text-red-300"
          >
            <AlertCircle className="w-4 h-4" />
            <span className="font-semibold">Осталась 1 минута!</span>
          </motion.div>
        )}
        {isWarning && !isCritical && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            className="mt-2 flex items-center gap-2 text-xs text-yellow-700 dark:text-yellow-300"
          >
            <AlertCircle className="w-4 h-4" />
            <span>Осталось 5 минут</span>
          </motion.div>
        )}
      </AnimatePresence>

      {strictTiming && timeRemaining <= 10 && (
        <div className="mt-2 text-xs text-red-700 dark:text-red-300 font-semibold animate-pulse">
          Экзамен завершится автоматически!
        </div>
      )}
    </div>
  );
};

export default ExamTimer;
