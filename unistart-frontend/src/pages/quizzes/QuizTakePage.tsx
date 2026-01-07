import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { ArrowLeft, Clock, CheckCircle, XCircle, AlertCircle, Trophy } from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import api from '../../services/api';

interface Answer {
  id: number;
  text: string;
  isCorrect?: boolean; // Может быть null для обычного режима
}

interface Question {
  id: number;
  text: string;
  points: number;
  explanation?: string;
  answers: Answer[];
}

interface Quiz {
  id: number;
  title: string;
  description: string;
  subject: string;
  difficulty: string;
  timeLimit: number;
  isLearningMode: boolean;
  questions: Question[];
}

const QuizTakePage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [quiz, setQuiz] = useState<Quiz | null>(null);
  const [loading, setLoading] = useState(true);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [userAnswers, setUserAnswers] = useState<Map<number, number[]>>(new Map());
  const [selectedAnswerIds, setSelectedAnswerIds] = useState<number[]>([]);
  const [showFeedback, setShowFeedback] = useState(false);
  const [timeRemaining, setTimeRemaining] = useState(0);
  const [quizStartTime] = useState(Date.now());
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [attemptId, setAttemptId] = useState<number | null>(null);
  const [quizCompleted, setQuizCompleted] = useState(false);

  useEffect(() => {
    loadQuiz();
  }, [id]);

  // Restore selected answers when changing question
  useEffect(() => {
    if (quiz && quiz.questions[currentQuestionIndex]) {
      const currentQuestion = quiz.questions[currentQuestionIndex];
      const savedAnswers = userAnswers.get(currentQuestion.id);
      if (savedAnswers) {
        setSelectedAnswerIds(savedAnswers);
      } else {
        setSelectedAnswerIds([]);
      }
      setShowFeedback(false);
    }
  }, [currentQuestionIndex, quiz]);

  useEffect(() => {
    if (quiz && timeRemaining > 0 && !quizCompleted) {
      const timer = setInterval(() => {
        setTimeRemaining((prev) => {
          if (prev <= 1) {
            handleSubmitQuiz();
            return 0;
          }
          return prev - 1;
        });
      }, 1000);

      return () => clearInterval(timer);
    }
  }, [quiz, timeRemaining, quizCompleted]);

  const loadQuiz = async () => {
    try {
      setLoading(true);
      console.log('Loading quiz:', id);
      const response = await api.get(`/quizzes/${id}`);
      const quizData = response.data;
      console.log('Quiz data loaded:', quizData);
      
      setQuiz(quizData);
      setTimeRemaining(quizData.timeLimit * 60); // Convert minutes to seconds
      
      // Start quiz attempt
      console.log('Starting quiz attempt...');
      const attemptResponse = await api.post(`/quizzes/${id}/attempts/start`);
      console.log('Attempt response:', attemptResponse.data);
      console.log('Attempt ID:', attemptResponse.data.attemptId);
      setAttemptId(attemptResponse.data.attemptId);
      console.log('Attempt ID set to:', attemptResponse.data.attemptId);
    } catch (error: any) {
      console.error('Ошибка загрузки квиза:', error);
      console.error('Error response:', error.response);
      alert(error.response?.data?.message || 'Не удалось загрузить квиз');
      navigate('/quizzes');
    } finally {
      setLoading(false);
    }
  };

  const handleAnswerSelect = (answerId: number) => {
    if (showFeedback) return; // Don't allow changing answer after feedback is shown
    
    if (!quiz) return;
    
    const currentQuestion = quiz.questions[currentQuestionIndex];
    const isMultipleChoice = currentQuestion.answers.filter(a => a.isCorrect).length > 1;
    
    let newSelectedIds: number[];
    
    if (isMultipleChoice) {
      // Toggle answer for multiple choice
      if (selectedAnswerIds.includes(answerId)) {
        newSelectedIds = selectedAnswerIds.filter(id => id !== answerId);
      } else {
        newSelectedIds = [...selectedAnswerIds, answerId];
      }
    } else {
      // Replace for single choice
      newSelectedIds = [answerId];
      // Show feedback immediately for single choice
      setShowFeedback(true);
    }
    
    setSelectedAnswerIds(newSelectedIds);
    
    // Save answer immediately to userAnswers Map
    const newUserAnswers = new Map(userAnswers);
    if (newSelectedIds.length > 0) {
      newUserAnswers.set(currentQuestion.id, newSelectedIds);
    } else {
      newUserAnswers.delete(currentQuestion.id);
    }
    setUserAnswers(newUserAnswers);
  };

  const handleNextQuestion = () => {
    console.log('handleNextQuestion called');
    console.log('Quiz:', quiz);
    console.log('Current question index:', currentQuestionIndex);
    
    if (!quiz) {
      console.log('No quiz, returning');
      return;
    }

    const currentQuestion = quiz.questions[currentQuestionIndex];
    console.log('Current question:', currentQuestion);
    
    // Check if current question has been answered
    const hasAnswered = userAnswers.has(currentQuestion.id) && 
                       userAnswers.get(currentQuestion.id)!.length > 0;
    
    console.log('Has answered:', hasAnswered);
    console.log('User answers:', userAnswers);
    
    if (!hasAnswered) {
      console.log('Not answered, returning');
      return;
    }

    const isMultipleChoice = currentQuestion.answers.filter(a => a.isCorrect).length > 1;
    const isLastQuestion = currentQuestionIndex >= quiz.questions.length - 1;
    
    console.log('Is multiple choice:', isMultipleChoice);
    console.log('Is last question:', isLastQuestion);
    console.log('Show feedback:', showFeedback);
    
    // On last question, submit immediately (ignore feedback state)
    if (isLastQuestion) {
      console.log('Last question - calling handleSubmitQuiz');
      handleSubmitQuiz();
      return;
    }
    
    // For multiple choice (not last question), show feedback when clicking Next if not already shown
    if (isMultipleChoice && !showFeedback) {
      console.log('Showing feedback for multiple choice');
      setShowFeedback(true);
      return;
    }

    // Reset for next question
    console.log('Moving to next question');
    setShowFeedback(false);
    setSelectedAnswerIds([]);
    setCurrentQuestionIndex(currentQuestionIndex + 1);
  };

  const handleSubmitQuiz = async () => {
    console.log('handleSubmitQuiz called');
    console.log('isSubmitting:', isSubmitting);
    console.log('attemptId:', attemptId);
    
    if (isSubmitting || !quiz || !attemptId) {
      console.log('Blocked - isSubmitting:', isSubmitting, 'quiz:', !!quiz, 'attemptId:', attemptId);
      return;
    }
    
    console.log('Starting quiz submission...');
    setIsSubmitting(true);
    setQuizCompleted(true); // Останавливаем таймер
    
    try {
      const timeSpent = Math.floor((Date.now() - quizStartTime) / 1000);
      
      console.log('User answers Map:', userAnswers);
      
      // Convert Map to Dictionary<int, List<int>> format
      const userAnswersDict: Record<number, number[]> = {};
      userAnswers.forEach((answerIds, questionId) => {
        userAnswersDict[questionId] = answerIds;
      });

      console.log('Submitting quiz with answers:', userAnswersDict);
      console.log('Total answered questions:', userAnswers.size);
      console.log('Total questions:', quiz.questions.length);

      const response = await api.post(`/quizzes/${id}/attempts/${attemptId}/submit`, {
        quizId: parseInt(id!),
        timeSpentSeconds: timeSpent,
        userAnswers: userAnswersDict,
      });

      console.log('Server response:', response.data);

      navigate(`/quizzes/${id}/results`, {
        state: {
          score: response.data.score,
          maxScore: response.data.maxScore,
          percentage: response.data.percentage,
          passed: response.data.passed,
          totalQuestions: response.data.totalQuestions,
          correctAnswers: response.data.correctQuestions,
          timeSpent,
          userAnswers: userAnswersDict,
          quiz,
        },
      });
    } catch (error: any) {
      console.error('Ошибка отправки квиза:', error);
      alert(error.response?.data?.message || 'Не удалось отправить результаты');
      setIsSubmitting(false);
    }
  };

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800">
        <div className="text-xl text-gray-600 dark:text-gray-300">Загрузка квиза...</div>
      </div>
    );
  }

  if (!quiz) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800">
        <div className="text-xl text-gray-600 dark:text-gray-300">Квиз не найден</div>
      </div>
    );
  }

  const currentQuestion = quiz.questions[currentQuestionIndex];
  const progress = ((currentQuestionIndex + 1) / quiz.questions.length) * 100;

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <Button
            variant="secondary"
            onClick={() => {
              if (window.confirm('Вы уверены, что хотите выйти? Прогресс будет потерян.')) {
                navigate('/quizzes');
              }
            }}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Выход
          </Button>

          <div className="flex items-center gap-4">
            <div className={`flex items-center gap-2 px-4 py-2 rounded-lg ${
              timeRemaining < 60 ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300' :
              timeRemaining < 300 ? 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300' :
              'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300'
            }`}>
              <Clock className="w-5 h-5" />
              <span className="font-mono font-semibold">{formatTime(timeRemaining)}</span>
            </div>
          </div>
        </div>

        {/* Quiz Info */}
        <Card className="p-6 mb-6">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">{quiz.title}</h1>
          <p className="text-gray-600 dark:text-gray-400 mb-4">{quiz.description}</p>
          
          {quiz.isLearningMode && (
            <div className="flex items-center gap-2 text-sm text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/30 px-3 py-2 rounded-lg">
              <AlertCircle className="w-4 h-4" />
              <span>Режим обучения: вы увидите объяснение после каждого ответа</span>
            </div>
          )}

          {/* Progress Bar */}
          <div className="mt-4">
            <div className="flex justify-between text-sm text-gray-600 dark:text-gray-400 mb-2">
              <span>Вопрос {currentQuestionIndex + 1} из {quiz.questions.length}</span>
              <span>{Math.round(progress)}%</span>
            </div>
            <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
              <div
                className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                style={{ width: `${progress}%` }}
              />
            </div>
          </div>
        </Card>

        {/* Question Card */}
        <AnimatePresence mode="wait">
          <motion.div
            key={currentQuestionIndex}
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            transition={{ duration: 0.3 }}
          >
            <Card className="p-6 mb-6">
              <div className="mb-6">
                <h2 className="text-xl font-semibold text-gray-900 dark:text-white mb-4">
                  {currentQuestion.text}
                </h2>
                <div className="text-sm text-gray-500 dark:text-gray-400">
                  {currentQuestion.points} {currentQuestion.points === 1 ? 'балл' : 'балла'}
                </div>
              </div>

              {/* Answers */}
              <div className="space-y-3">
                {currentQuestion.answers.map((answer) => {
                  const isMultipleChoice = currentQuestion.answers.filter(a => a.isCorrect).length > 1;
                  const isSelected = selectedAnswerIds.includes(answer.id);
                  const showCorrect = showFeedback && quiz.isLearningMode;
                  
                  return (
                    <button
                      key={answer.id}
                      onClick={() => handleAnswerSelect(answer.id)}
                      disabled={showFeedback && quiz.isLearningMode}
                      className={`w-full p-4 rounded-lg border-2 transition-all text-left ${
                        showCorrect
                          ? answer.isCorrect
                            ? 'border-green-500 bg-green-50 dark:bg-green-900/30'
                            : isSelected
                            ? 'border-red-500 bg-red-50 dark:bg-red-900/30'
                            : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800'
                          : isSelected
                          ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/30'
                          : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 hover:border-blue-300 hover:bg-blue-50 dark:hover:bg-blue-900/10'
                      }`}
                    >
                      <div className="flex items-center gap-3">
                        {isMultipleChoice ? (
                          <input
                            type="checkbox"
                            checked={isSelected}
                            onChange={() => {}}
                            className="w-5 h-5 text-blue-600 rounded"
                          />
                        ) : (
                          <input
                            type="radio"
                            checked={isSelected}
                            onChange={() => {}}
                            className="w-5 h-5 text-blue-600"
                          />
                        )}
                        <span className="flex-1 text-gray-900 dark:text-white">{answer.text}</span>
                        {showCorrect && (
                          <>
                            {answer.isCorrect && (
                              <CheckCircle className="w-5 h-5 text-green-600" />
                            )}
                            {!answer.isCorrect && isSelected && (
                              <XCircle className="w-5 h-5 text-red-600" />
                            )}
                          </>
                        )}
                      </div>
                    </button>
                  );
                })}
              </div>

              {/* Feedback in Learning Mode */}
              {showFeedback && quiz.isLearningMode && selectedAnswerIds.length > 0 && (() => {
                const correctAnswerIds = currentQuestion.answers.filter(a => a.isCorrect).map(a => a.id).sort();
                const sortedSelected = [...selectedAnswerIds].sort();
                const isCorrect = correctAnswerIds.length === sortedSelected.length && 
                                 correctAnswerIds.every((id, index) => id === sortedSelected[index]);
                
                return (
                  <motion.div
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    className={`mt-4 p-4 rounded-lg ${
                      isCorrect
                        ? 'bg-green-50 dark:bg-green-900/30 border border-green-200 dark:border-green-700'
                        : 'bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-700'
                    }`}
                  >
                    <div className="flex items-start gap-2">
                      {isCorrect ? (
                        <CheckCircle className="w-5 h-5 text-green-600 flex-shrink-0 mt-0.5" />
                      ) : (
                        <XCircle className="w-5 h-5 text-red-600 flex-shrink-0 mt-0.5" />
                      )}
                      <div>
                        <p className={`font-semibold mb-1 ${
                          isCorrect
                            ? 'text-green-800 dark:text-green-300'
                            : 'text-red-800 dark:text-red-300'
                        }`}>
                          {isCorrect ? 'Правильно!' : 'Неправильно'}
                        </p>
                        {currentQuestion.explanation && (
                          <p className="text-gray-700 dark:text-gray-300 text-sm">
                            {currentQuestion.explanation}
                          </p>
                        )}
                      </div>
                    </div>
                  </motion.div>
                );
              })()}
            </Card>
          </motion.div>
        </AnimatePresence>

        {/* Navigation */}
        <div className="flex justify-between items-center">
          {(() => {
            const isMultipleChoice = quiz.questions[currentQuestionIndex].answers.filter(a => a.isCorrect).length > 1;
            return isMultipleChoice && !showFeedback ? (
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Выберите все правильные варианты
              </p>
            ) : <div />;
          })()}
          <Button
            variant="primary"
            onClick={handleNextQuestion}
            disabled={
              !userAnswers.has(quiz.questions[currentQuestionIndex].id) || 
              userAnswers.get(quiz.questions[currentQuestionIndex].id)!.length === 0 || 
              isSubmitting
            }
            className="flex items-center gap-2"
          >
            {currentQuestionIndex < quiz.questions.length - 1 ? (
              <>Следующий вопрос</>
            ) : (
              <>
                <Trophy className="w-5 h-5" />
                Завершить квиз
              </>
            )}
          </Button>
        </div>
      </div>
    </div>
  );
};

export default QuizTakePage;
