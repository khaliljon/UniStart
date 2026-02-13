import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import type { RootState } from '../store';
import type { Question, AnswerResult } from '../types';
import { testService } from '../services/testService';

export default function ReviewPage() {
  const navigate = useNavigate();
  const { selectedExams } = useSelector((state: RootState) => state.exam);
  
  const [questions, setQuestions] = useState<Question[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [selectedOption, setSelectedOption] = useState<number | null>(null);
  const [answerResult, setAnswerResult] = useState<AnswerResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    loadWeakQuestions();
  }, []);

  const loadWeakQuestions = async () => {
    try {
      setLoading(true);
      const data = await testService.getWeakQuestions(selectedExams);
      setQuestions(data);
    } catch (error) {
      console.error('Failed to load weak questions:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async () => {
    if (selectedOption === null) return;
    
    const currentQuestion = questions[currentIndex];
    setSubmitting(true);
    
    try {
      const result = await testService.submitAnswer({
        questionId: currentQuestion.id,
        answerOptionId: selectedOption,
      });
      setAnswerResult(result);
    } catch (error) {
      console.error('Failed to submit answer:', error);
    } finally {
      setSubmitting(false);
    }
  };

  const handleNext = () => {
    if (currentIndex < questions.length - 1) {
      setCurrentIndex(currentIndex + 1);
      setSelectedOption(null);
      setAnswerResult(null);
    } else {
      navigate('/analytics');
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto" />
          <p className="mt-4 text-gray-600">Загрузка вопросов для повторения...</p>
        </div>
      </div>
    );
  }

  if (questions.length === 0) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-8">
        <div className="max-w-2xl mx-auto bg-white rounded-2xl shadow-xl p-8 text-center">
          <div className="text-6xl mb-4">🎉</div>
          <h2 className="text-2xl font-bold text-gray-800 mb-4">Отлично!</h2>
          <p className="text-gray-600 mb-6">
            У вас нет ошибок для повторения. Продолжайте практиковаться!
          </p>
          <button
            onClick={() => navigate('/test')}
            className="px-6 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
          >
            Начать новый тест
          </button>
        </div>
      </div>
    );
  }

  const currentQuestion = questions[currentIndex];

  return (
    <div className="min-h-screen bg-gradient-to-br from-orange-50 to-amber-100 p-4 md:p-8">
      <div className="max-w-3xl mx-auto">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <div className="flex items-center gap-3">
            <span className="px-3 py-1 bg-orange-100 text-orange-700 rounded-full text-sm font-medium">
              🔄 Повторение ошибок
            </span>
          </div>
          <button
            onClick={() => navigate('/test')}
            className="text-gray-600 hover:text-gray-800"
          >
            ← Назад
          </button>
        </div>

        {/* Progress */}
        <div className="bg-white rounded-xl p-4 mb-6">
          <div className="flex justify-between text-sm text-gray-600 mb-2">
            <span>Вопрос {currentIndex + 1} из {questions.length}</span>
            <span>{currentQuestion.topicName}</span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2">
            <div
              className="bg-orange-500 h-2 rounded-full transition-all duration-300"
              style={{ width: `${((currentIndex + 1) / questions.length) * 100}%` }}
            />
          </div>
        </div>

        {/* Question Card */}
        <div className="bg-white rounded-2xl shadow-xl p-6 md:p-8">
          <div className="flex items-center gap-2 mb-4">
            <span className={`px-3 py-1 rounded-full text-xs font-medium ${
              currentQuestion.difficulty === 'Easy' ? 'bg-green-100 text-green-700' :
              currentQuestion.difficulty === 'Medium' ? 'bg-yellow-100 text-yellow-700' :
              'bg-red-100 text-red-700'
            }`}>
              {currentQuestion.difficulty === 'Easy' ? 'Легкий' :
               currentQuestion.difficulty === 'Medium' ? 'Средний' : 'Сложный'}
            </span>
          </div>

          <h2 className="text-xl font-semibold text-gray-800 mb-6">
            {currentQuestion.text}
          </h2>

          <div className="space-y-3">
            {currentQuestion.options.map((option) => {
              let optionClass = 'border-2 border-gray-200 hover:border-indigo-300';
              
              if (answerResult) {
                if (option.id === answerResult.correctOptionId) {
                  optionClass = 'border-2 border-green-500 bg-green-50';
                } else if (option.id === selectedOption && !answerResult.isCorrect) {
                  optionClass = 'border-2 border-red-500 bg-red-50';
                }
              } else if (selectedOption === option.id) {
                optionClass = 'border-2 border-indigo-500 bg-indigo-50';
              }

              return (
                <button
                  key={option.id}
                  onClick={() => !answerResult && setSelectedOption(option.id)}
                  disabled={answerResult !== null}
                  className={`w-full p-4 rounded-xl text-left transition-all ${optionClass} disabled:cursor-default`}
                >
                  {option.text}
                </button>
              );
            })}
          </div>

          {/* Result Feedback */}
          {answerResult && (
            <div className={`mt-6 p-4 rounded-xl ${
              answerResult.isCorrect ? 'bg-green-50 border border-green-200' : 'bg-red-50 border border-red-200'
            }`}>
              <div className="flex items-center gap-2 mb-2">
                <span className="text-2xl">{answerResult.isCorrect ? '✅' : '❌'}</span>
                <span className={`font-semibold ${answerResult.isCorrect ? 'text-green-700' : 'text-red-700'}`}>
                  {answerResult.isCorrect ? 'Правильно!' : 'Неправильно'}
                </span>
              </div>
              
              {!answerResult.isCorrect && (
                <p className="text-gray-700 mb-2">
                  <span className="font-medium">Правильный ответ: </span>
                  {answerResult.correctOptionText}
                </p>
              )}
              
              {answerResult.explanation && (
                <div className="mt-3 pt-3 border-t border-gray-200">
                  <p className="text-sm text-gray-600">
                    <span className="font-medium">Объяснение: </span>
                    {answerResult.explanation}
                  </p>
                </div>
              )}
            </div>
          )}

          {/* Actions */}
          <div className="mt-6">
            {!answerResult ? (
              <button
                onClick={handleSubmit}
                disabled={selectedOption === null || submitting}
                className="w-full py-3 bg-orange-600 text-white rounded-xl font-semibold hover:bg-orange-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
              >
                {submitting ? 'Проверка...' : 'Ответить'}
              </button>
            ) : (
              <button
                onClick={handleNext}
                className="w-full py-3 bg-indigo-600 text-white rounded-xl font-semibold hover:bg-indigo-700 transition-colors"
              >
                {currentIndex < questions.length - 1 ? 'Следующий вопрос' : 'Завершить'}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
