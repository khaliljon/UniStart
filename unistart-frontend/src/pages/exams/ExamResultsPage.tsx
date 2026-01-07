import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Trophy, XCircle, Clock, CheckCircle, ArrowRight, RotateCcw, AlertTriangle, Award } from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';

const ExamResultsPage = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { id } = useParams();
  const resultData = location.state;

  if (!resultData) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-gray-50 to-purple-50 dark:from-gray-900 dark:to-gray-800">
        <div className="text-center">
          <p className="text-xl text-gray-600 dark:text-gray-300 mb-4">Результаты не найдены</p>
          <Button variant="primary" onClick={() => navigate('/exams')}>
            Вернуться к экзаменам
          </Button>
        </div>
      </div>
    );
  }

  const { 
    score, 
    totalPoints, 
    earnedPoints, 
    passingScore, 
    passed, 
    timeSpent, 
    totalQuestions, 
    answeredQuestions,
    autoSubmitted 
  } = resultData;

  const formatTime = (seconds: number) => {
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    
    if (hrs > 0) {
      return `${hrs}:${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
    }
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-purple-50 dark:from-gray-900 dark:to-gray-800 py-12 px-4">
      <div className="max-w-4xl mx-auto">
        {/* Result Header */}
        <motion.div
          initial={{ scale: 0 }}
          animate={{ scale: 1 }}
          transition={{ type: 'spring', duration: 0.5 }}
          className="text-center mb-8"
        >
          {passed ? (
            <div className="inline-flex items-center justify-center w-32 h-32 bg-gradient-to-br from-green-400 to-green-600 rounded-full mb-4 shadow-2xl">
              <Trophy className="w-16 h-16 text-white" />
            </div>
          ) : (
            <div className="inline-flex items-center justify-center w-32 h-32 bg-gradient-to-br from-red-400 to-red-600 rounded-full mb-4 shadow-2xl">
              <XCircle className="w-16 h-16 text-white" />
            </div>
          )}
          
          <h1 className="text-5xl font-bold text-gray-900 dark:text-white mb-3">
            {passed ? 'Экзамен сдан!' : 'Экзамен не сдан'}
          </h1>
          <p className="text-xl text-gray-600 dark:text-gray-400">
            {passed 
              ? 'Отличная работа! Вы прошли проходной балл.' 
              : 'Не расстраивайтесь, попробуйте еще раз.'}
          </p>
          
          {autoSubmitted && (
            <div className="mt-4 inline-flex items-center gap-2 px-4 py-2 bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-300 rounded-lg">
              <AlertTriangle className="w-5 h-5" />
              <span className="text-sm">Экзамен был автоматически завершен по истечении времени</span>
            </div>
          )}
        </motion.div>

        {/* Score Card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
        >
          <Card className="p-8 mb-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-8">
              <div className="text-center">
                <div className="text-7xl font-bold mb-2" style={{ 
                  color: passed ? 'rgb(34, 197, 94)' : 'rgb(239, 68, 68)' 
                }}>
                  {score}%
                </div>
                <p className="text-gray-600 dark:text-gray-400 text-lg">Ваш результат</p>
              </div>

              <div className="flex flex-col justify-center">
                <div className="flex items-center justify-between mb-3">
                  <span className="text-gray-600 dark:text-gray-400">Проходной балл:</span>
                  <span className="text-xl font-semibold text-purple-600 dark:text-purple-400">{passingScore}%</span>
                </div>
                <div className="flex items-center justify-between mb-3">
                  <span className="text-gray-600 dark:text-gray-400">Набрано баллов:</span>
                  <span className="text-xl font-semibold text-gray-900 dark:text-white">{earnedPoints} / {totalPoints}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-gray-600 dark:text-gray-400">Статус:</span>
                  <span className={`text-xl font-semibold ${passed ? 'text-green-600' : 'text-red-600'}`}>
                    {passed ? 'Сдано ✓' : 'Не сдано ✗'}
                  </span>
                </div>
              </div>
            </div>

            {/* Progress Bar */}
            <div className="mb-8">
              <div className="flex justify-between text-sm text-gray-600 dark:text-gray-400 mb-2">
                <span>Прогресс</span>
                <span>{score}%</span>
              </div>
              <div className="relative w-full bg-gray-200 dark:bg-gray-700 rounded-full h-4">
                <div
                  className={`h-4 rounded-full transition-all duration-1000 ${
                    passed ? 'bg-gradient-to-r from-green-400 to-green-600' : 'bg-gradient-to-r from-red-400 to-red-600'
                  }`}
                  style={{ width: `${score}%` }}
                />
                {/* Passing score marker */}
                <div 
                  className="absolute top-0 h-4 w-1 bg-purple-500"
                  style={{ left: `${passingScore}%` }}
                >
                  <div className="absolute -top-6 left-1/2 transform -translate-x-1/2 text-xs text-purple-600 dark:text-purple-400 whitespace-nowrap">
                    Проходной
                  </div>
                </div>
              </div>
            </div>

            {/* Statistics */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="text-center p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
                <div className="flex items-center justify-center mb-2">
                  <CheckCircle className="w-5 h-5 text-green-600 mr-2" />
                  <span className="text-2xl font-bold text-gray-900 dark:text-white">
                    {answeredQuestions}
                  </span>
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Отвечено</p>
              </div>

              <div className="text-center p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
                <div className="flex items-center justify-center mb-2">
                  <XCircle className="w-5 h-5 text-red-600 mr-2" />
                  <span className="text-2xl font-bold text-gray-900 dark:text-white">
                    {totalQuestions - answeredQuestions}
                  </span>
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Пропущено</p>
              </div>

              <div className="text-center p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
                <div className="flex items-center justify-center mb-2">
                  <Clock className="w-5 h-5 text-blue-600 mr-2" />
                  <span className="text-2xl font-bold text-gray-900 dark:text-white">
                    {formatTime(timeSpent)}
                  </span>
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Время</p>
              </div>

              <div className="text-center p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
                <div className="flex items-center justify-center mb-2">
                  <Award className="w-5 h-5 text-purple-600 mr-2" />
                  <span className="text-2xl font-bold text-gray-900 dark:text-white">
                    {earnedPoints}
                  </span>
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Баллов</p>
              </div>
            </div>
          </Card>
        </motion.div>

        {/* Feedback */}
        {passed && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
          >
            <Card className="p-6 mb-6 bg-gradient-to-r from-green-50 to-emerald-50 dark:from-green-900/20 dark:to-emerald-900/20 border-2 border-green-200 dark:border-green-700">
              <div className="flex items-start gap-4">
                <div className="flex-shrink-0">
                  <Trophy className="w-8 h-8 text-green-600 dark:text-green-400" />
                </div>
                <div>
                  <h3 className="text-lg font-semibold text-green-900 dark:text-green-100 mb-2">
                    Поздравляем с успешной сдачей экзамена!
                  </h3>
                  <p className="text-green-800 dark:text-green-200">
                    Вы показали отличные знания и прошли проходной балл. Продолжайте в том же духе!
                  </p>
                </div>
              </div>
            </Card>
          </motion.div>
        )}

        {!passed && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
          >
            <Card className="p-6 mb-6 bg-gradient-to-r from-red-50 to-orange-50 dark:from-red-900/20 dark:to-orange-900/20 border-2 border-red-200 dark:border-red-700">
              <div className="flex items-start gap-4">
                <div className="flex-shrink-0">
                  <AlertTriangle className="w-8 h-8 text-red-600 dark:text-red-400" />
                </div>
                <div>
                  <h3 className="text-lg font-semibold text-red-900 dark:text-red-100 mb-2">
                    Рекомендации для улучшения результата
                  </h3>
                  <ul className="list-disc list-inside text-red-800 dark:text-red-200 space-y-1">
                    <li>Повторите материал по темам, где были ошибки</li>
                    <li>Используйте флешкарточки для закрепления знаний</li>
                    <li>Пройдите тренировочные квизы перед повторной попыткой</li>
                    <li>Обратитесь к преподавателю за дополнительными материалами</li>
                  </ul>
                </div>
              </div>
            </Card>
          </motion.div>
        )}

        {/* Actions */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="flex flex-col sm:flex-row gap-4 justify-center"
        >
          {!passed && (
            <Button
              variant="primary"
              onClick={() => navigate(`/exams/${id}/take`)}
              className="flex items-center justify-center gap-2"
            >
              <RotateCcw className="w-5 h-5" />
              Попробовать снова
            </Button>
          )}
          
          <Button
            variant={passed ? 'primary' : 'secondary'}
            onClick={() => navigate('/exams')}
            className="flex items-center justify-center gap-2"
          >
            {passed ? (
              <>
                <ArrowRight className="w-5 h-5" />
                К другим экзаменам
              </>
            ) : (
              'Вернуться к экзаменам'
            )}
          </Button>

          <Button
            variant="secondary"
            onClick={() => navigate('/dashboard')}
            className="flex items-center justify-center gap-2"
          >
            На главную
          </Button>
        </motion.div>
      </div>
    </div>
  );
};

export default ExamResultsPage;
