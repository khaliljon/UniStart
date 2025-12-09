import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Trophy, XCircle, Clock, CheckCircle, ArrowRight, RotateCcw } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';

const QuizResultsPage = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { id } = useParams();
  const resultData = location.state;

  if (!resultData) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800">
        <div className="text-center">
          <p className="text-xl text-gray-600 dark:text-gray-300 mb-4">Результаты не найдены</p>
          <Button variant="primary" onClick={() => navigate('/quizzes')}>
            Вернуться к квизам
          </Button>
        </div>
      </div>
    );
  }

  const { score, totalQuestions, correctAnswers, timeSpent, quiz } = resultData;
  const isPassed = score >= 70; // Можно сделать настраиваемым

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-12 px-4">
      <div className="max-w-3xl mx-auto">
        {/* Result Header */}
        <motion.div
          initial={{ scale: 0 }}
          animate={{ scale: 1 }}
          transition={{ type: 'spring', duration: 0.5 }}
          className="text-center mb-8"
        >
          {isPassed ? (
            <div className="inline-flex items-center justify-center w-24 h-24 bg-green-100 dark:bg-green-900/30 rounded-full mb-4">
              <Trophy className="w-12 h-12 text-green-600 dark:text-green-400" />
            </div>
          ) : (
            <div className="inline-flex items-center justify-center w-24 h-24 bg-red-100 dark:bg-red-900/30 rounded-full mb-4">
              <XCircle className="w-12 h-12 text-red-600 dark:text-red-400" />
            </div>
          )}
          
          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-2">
            {isPassed ? 'Поздравляем!' : 'Попробуйте еще раз'}
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            {isPassed ? 'Вы успешно прошли квиз!' : 'Нужно еще немного практики'}
          </p>
        </motion.div>

        {/* Score Card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
        >
          <Card className="p-8 mb-6">
            <div className="text-center mb-6">
              <div className="text-6xl font-bold mb-2" style={{ 
                color: isPassed ? 'rgb(34, 197, 94)' : 'rgb(239, 68, 68)' 
              }}>
                {score}%
              </div>
              <p className="text-gray-600 dark:text-gray-400">Ваш результат</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="text-center p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
                <div className="flex items-center justify-center mb-2">
                  <CheckCircle className="w-5 h-5 text-green-600 mr-2" />
                  <span className="text-2xl font-bold text-gray-900 dark:text-white">
                    {correctAnswers}
                  </span>
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Правильных ответов</p>
              </div>

              <div className="text-center p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
                <div className="flex items-center justify-center mb-2">
                  <XCircle className="w-5 h-5 text-red-600 mr-2" />
                  <span className="text-2xl font-bold text-gray-900 dark:text-white">
                    {totalQuestions - correctAnswers}
                  </span>
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Неправильных ответов</p>
              </div>

              <div className="text-center p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
                <div className="flex items-center justify-center mb-2">
                  <Clock className="w-5 h-5 text-blue-600 mr-2" />
                  <span className="text-2xl font-bold text-gray-900 dark:text-white">
                    {formatTime(timeSpent)}
                  </span>
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Затраченное время</p>
              </div>
            </div>
          </Card>
        </motion.div>

        {/* Quiz Info */}
        {quiz && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
          >
            <Card className="p-6 mb-6">
              <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                {quiz.title}
              </h2>
              <p className="text-gray-600 dark:text-gray-400 mb-4">{quiz.description}</p>
              
              <div className="flex items-center gap-4 text-sm text-gray-600 dark:text-gray-400">
                <span className="px-3 py-1 bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 rounded-full">
                  {quiz.subject}
                </span>
                <span className="px-3 py-1 bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300 rounded-full">
                  {quiz.difficulty}
                </span>
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
          {!isPassed && (
            <Button
              variant="primary"
              onClick={() => navigate(`/quizzes/${id}/take`)}
              className="flex items-center justify-center gap-2"
            >
              <RotateCcw className="w-5 h-5" />
              Попробовать снова
            </Button>
          )}
          
          <Button
            variant={isPassed ? 'primary' : 'secondary'}
            onClick={() => navigate('/quizzes')}
            className="flex items-center justify-center gap-2"
          >
            {isPassed ? (
              <>
                <ArrowRight className="w-5 h-5" />
                Продолжить обучение
              </>
            ) : (
              'Вернуться к квизам'
            )}
          </Button>
        </motion.div>
      </div>
    </div>
  );
};

export default QuizResultsPage;
