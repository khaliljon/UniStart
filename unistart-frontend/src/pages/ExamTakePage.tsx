import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { ArrowLeft, AlertTriangle, Trophy, CheckCircle, Clock } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import ExamTimer from '../components/exams/ExamTimer';
import api from '../services/api';

interface Answer {
  id: number;
  text: string;
  isCorrect: boolean;
}

interface Question {
  id: number;
  text: string;
  points: number;
  explanation: string;
  questionType: string;
  answers: Answer[];
}

interface Exam {
  id: number;
  title: string;
  description: string;
  subject: string;
  difficulty: string;
  timeLimit: number;
  passingScore: number;
  strictTiming: boolean;
  shuffleQuestions: boolean;
  shuffleAnswers: boolean;
  questions: Question[];
}

const ExamTakePage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  
  const [exam, setExam] = useState<Exam | null>(null);
  const [loading, setLoading] = useState(true);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [userAnswers, setUserAnswers] = useState<Map<number, number[]>>(new Map());
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [attemptId, setAttemptId] = useState<number | null>(null);
  const [examStartTime] = useState(Date.now());
  const autoSubmitRef = useRef(false);

  useEffect(() => {
    loadExam();
  }, [id]);

  const loadExam = async () => {
    try {
      setLoading(true);
      console.log('Loading exam with ID:', id);
      const response = await api.get(`/exams/${id}/take`);
      console.log('Exam loaded:', response.data);
      let examData = response.data;
      
      // Shuffle questions if needed
      if (examData.shuffleQuestions) {
        examData.questions = [...examData.questions].sort(() => Math.random() - 0.5);
      }

      // Shuffle answers if needed
      if (examData.shuffleAnswers) {
        examData.questions = examData.questions.map((q: Question) => ({
          ...q,
          answers: [...q.answers].sort(() => Math.random() - 0.5),
        }));
      }

      setExam(examData);
      
      // Start exam attempt
      console.log('Starting exam attempt...');
      const attemptResponse = await api.post(`/exams/${id}/attempts/start`);
      console.log('Attempt started:', attemptResponse.data);
      setAttemptId(attemptResponse.data.id);
    } catch (error: any) {
      console.error('Ошибка загрузки экзамена:', error);
      console.error('Error details:', error.response);
      const errorMessage = error.response?.data?.message || error.response?.data || 'Не удалось загрузить экзамен';
      alert(typeof errorMessage === 'string' ? errorMessage : 'Не удалось загрузить экзамен');
      navigate('/exams');
    } finally {
      setLoading(false);
    }
  };

  const handleAnswerSelect = (questionId: number, answerId: number, isMultipleChoice: boolean) => {
    const newAnswers = new Map(userAnswers);
    
    if (isMultipleChoice) {
      // For multiple choice, toggle the answer
      const current = newAnswers.get(questionId) || [];
      if (current.includes(answerId)) {
        newAnswers.set(questionId, current.filter(id => id !== answerId));
      } else {
        newAnswers.set(questionId, [...current, answerId]);
      }
    } else {
      // For single choice, replace the answer
      newAnswers.set(questionId, [answerId]);
    }
    
    setUserAnswers(newAnswers);
  };

  const handleTimeExpired = () => {
    if (!autoSubmitRef.current) {
      autoSubmitRef.current = true;
      handleSubmitExam(true);
    }
  };

  const handleSubmitExam = async (autoSubmit = false) => {
    if (isSubmitting || !exam || !attemptId) return;
    
    // Check if all questions are answered
    if (!autoSubmit) {
      const unansweredQuestions = exam.questions.filter(q => !userAnswers.has(q.id));
      if (unansweredQuestions.length > 0) {
        const confirm = window.confirm(
          `У вас есть ${unansweredQuestions.length} неотвеченных вопросов. Вы уверены, что хотите завершить экзамен?`
        );
        if (!confirm) return;
      }
    }
    
    setIsSubmitting(true);
    
    try {
      const timeSpent = Math.floor((Date.now() - examStartTime) / 1000);
      
      // Calculate score
      let totalPoints = 0;
      let earnedPoints = 0;

      exam.questions.forEach(question => {
        totalPoints += question.points;
        const userAnswerIds = (userAnswers.get(question.id) || []).sort((a, b) => a - b);
        const correctAnswerIds = question.answers.filter(a => a.isCorrect).map(a => a.id).sort((a, b) => a - b);
        
        // Check if answer is correct - both arrays must be identical
        const isCorrect = 
          userAnswerIds.length === correctAnswerIds.length &&
          userAnswerIds.every((id, index) => id === correctAnswerIds[index]);
        
        if (isCorrect) {
          earnedPoints += question.points;
        }
      });

      const response = await api.post(`/exams/${id}/attempts/${attemptId}/submit`, {
        answers: Array.from(userAnswers.entries()).map(([questionId, answerIds]) => ({
          questionId,
          answerIds,
        })),
        score: 0, // Will be calculated on backend
        timeSpent,
      });
      
      console.log('Exam submission response:', response.data);

      navigate(`/exams/${id}/results`, {
        state: {
          score: response.data.score,
          totalPoints: response.data.totalPoints,
          earnedPoints: response.data.earnedPoints,
          passingScore: exam.passingScore,
          passed: response.data.passed,
          timeSpent,
          totalQuestions: exam.questions.length,
          answeredQuestions: userAnswers.size,
          autoSubmitted: autoSubmit,
        },
      });
    } catch (error: any) {
      console.error('Ошибка отправки экзамена:', error);
      alert(error.response?.data?.message || 'Не удалось отправить результаты');
      setIsSubmitting(false);
      autoSubmitRef.current = false;
    }
  };

  const goToQuestion = (index: number) => {
    setCurrentQuestionIndex(index);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-gray-50 to-purple-50 dark:from-gray-900 dark:to-gray-800">
        <div className="text-xl text-gray-600 dark:text-gray-300">Загрузка экзамена...</div>
      </div>
    );
  }

  if (!exam) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gradient-to-br from-gray-50 to-purple-50 dark:from-gray-900 dark:to-gray-800">
        <div className="text-xl text-gray-600 dark:text-gray-300">Экзамен не найден</div>
      </div>
    );
  }

  const currentQuestion = exam.questions[currentQuestionIndex];
  const isMultipleChoice = currentQuestion.questionType === 'MultipleChoice';
  const selectedAnswers = userAnswers.get(currentQuestion.id) || [];
  const progress = (userAnswers.size / exam.questions.length) * 100;

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-purple-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-7xl mx-auto">
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          {/* Main Content */}
          <div className="lg:col-span-3">
            {/* Header */}
            <div className="mb-6 flex items-center justify-between">
              <Button
                variant="secondary"
                onClick={() => {
                  if (window.confirm('Вы уверены, что хотите выйти? Прогресс будет потерян.')) {
                    navigate('/exams');
                  }
                }}
                className="flex items-center gap-2"
              >
                <ArrowLeft className="w-4 h-4" />
                Выход
              </Button>

              {exam.strictTiming && (
                <ExamTimer
                  timeLimit={exam.timeLimit}
                  strictTiming={exam.strictTiming}
                  onTimeUp={handleTimeExpired}
                />
              )}
            </div>

            {/* Exam Info */}
            <Card className="p-6 mb-6">
              <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">{exam.title}</h1>
              <p className="text-gray-600 dark:text-gray-400 mb-4">{exam.description}</p>
              
              <div className="flex items-center gap-4 text-sm">
                <span className="text-gray-600 dark:text-gray-400">
                  Проходной балл: <span className="font-semibold text-purple-600 dark:text-purple-400">{exam.passingScore}%</span>
                </span>
                <span className="text-gray-600 dark:text-gray-400">
                  Всего вопросов: <span className="font-semibold">{exam.questions.length}</span>
                </span>
              </div>

              {exam.strictTiming && (
                <div className="mt-4 flex items-center gap-2 text-sm text-orange-600 dark:text-orange-400 bg-orange-50 dark:bg-orange-900/30 px-3 py-2 rounded-lg">
                  <AlertTriangle className="w-4 h-4" />
                  <span>Строгий контроль времени: экзамен будет автоматически завершен при истечении времени</span>
                </div>
              )}
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
                    <div className="flex items-start justify-between mb-4">
                      <h2 className="text-xl font-semibold text-gray-900 dark:text-white flex-1">
                        {currentQuestion.text}
                      </h2>
                      <span className="ml-4 px-3 py-1 bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300 rounded-full text-sm font-medium">
                        {currentQuestion.points} {currentQuestion.points === 1 ? 'балл' : 'балла'}
                      </span>
                    </div>
                    
                    {isMultipleChoice && (
                      <p className="text-sm text-gray-500 dark:text-gray-400 flex items-center gap-2">
                        <AlertTriangle className="w-4 h-4" />
                        Можно выбрать несколько вариантов ответа
                      </p>
                    )}
                  </div>

                  {/* Answers */}
                  <div className="space-y-3">
                    {currentQuestion.answers.map((answer) => {
                      const isSelected = selectedAnswers.includes(answer.id);
                      
                      return (
                        <button
                          key={answer.id}
                          onClick={() => handleAnswerSelect(currentQuestion.id, answer.id, isMultipleChoice)}
                          className={`w-full p-4 rounded-lg border-2 transition-all text-left ${
                            isSelected
                              ? 'border-purple-500 bg-purple-50 dark:bg-purple-900/30'
                              : 'border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 hover:border-purple-300 hover:bg-purple-50 dark:hover:bg-purple-900/10'
                          }`}
                        >
                          <div className="flex items-center gap-3">
                            {isMultipleChoice ? (
                              <input
                                type="checkbox"
                                checked={isSelected}
                                onChange={() => {}}
                                className="w-5 h-5 text-purple-600 rounded"
                              />
                            ) : (
                              <input
                                type="radio"
                                checked={isSelected}
                                onChange={() => {}}
                                className="w-5 h-5 text-purple-600"
                              />
                            )}
                            <span className="text-gray-900 dark:text-white flex-1">{answer.text}</span>
                          </div>
                        </button>
                      );
                    })}
                  </div>
                </Card>
              </motion.div>
            </AnimatePresence>

            {/* Navigation */}
            <div className="flex justify-between">
              <Button
                variant="secondary"
                onClick={() => setCurrentQuestionIndex(Math.max(0, currentQuestionIndex - 1))}
                disabled={currentQuestionIndex === 0}
              >
                Предыдущий
              </Button>
              
              {currentQuestionIndex < exam.questions.length - 1 ? (
                <Button
                  variant="primary"
                  onClick={() => setCurrentQuestionIndex(currentQuestionIndex + 1)}
                >
                  Следующий
                </Button>
              ) : (
                <Button
                  variant="primary"
                  onClick={() => handleSubmitExam(false)}
                  disabled={isSubmitting}
                  className="flex items-center gap-2"
                >
                  <Trophy className="w-5 h-5" />
                  {isSubmitting ? 'Отправка...' : 'Завершить экзамен'}
                </Button>
              )}
            </div>
          </div>

          {/* Sidebar - Question Navigator */}
          <div className="lg:col-span-1">
            <Card className="p-6 sticky top-6">
              <h3 className="font-semibold text-gray-900 dark:text-white mb-4">Навигация</h3>
              
              {/* Progress */}
              <div className="mb-4">
                <div className="flex justify-between text-sm text-gray-600 dark:text-gray-400 mb-2">
                  <span>Отвечено</span>
                  <span>{userAnswers.size} / {exam.questions.length}</span>
                </div>
                <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                  <div
                    className="bg-purple-600 h-2 rounded-full transition-all duration-300"
                    style={{ width: `${progress}%` }}
                  />
                </div>
              </div>

              {/* Question Grid */}
              <div className="grid grid-cols-5 gap-2">
                {exam.questions.map((question, index) => {
                  const isAnswered = userAnswers.has(question.id);
                  const isCurrent = index === currentQuestionIndex;
                  
                  return (
                    <button
                      key={question.id}
                      onClick={() => goToQuestion(index)}
                      className={`aspect-square rounded-lg flex items-center justify-center text-sm font-medium transition-all ${
                        isCurrent
                          ? 'bg-purple-600 text-white ring-2 ring-purple-400 ring-offset-2 dark:ring-offset-gray-800'
                          : isAnswered
                          ? 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300 hover:bg-green-200 dark:hover:bg-green-900/50'
                          : 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-600'
                      }`}
                    >
                      {isAnswered && !isCurrent ? (
                        <CheckCircle className="w-4 h-4" />
                      ) : (
                        index + 1
                      )}
                    </button>
                  );
                })}
              </div>

              {/* Time info */}
              {!exam.strictTiming && exam.timeLimit > 0 && (
                <div className="mt-4 text-sm text-gray-500 dark:text-gray-400 flex items-center gap-2">
                  <Clock className="w-4 h-4" />
                  <span>Рекомендуемое время: {exam.timeLimit} мин</span>
                </div>
              )}
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ExamTakePage;
