import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import type { RootState } from '../store';
import type { TopicProgress, Question, AnswerResult } from '../types';
import { testService } from '../services/testService';

type ViewMode = 'topics' | 'practice';

export default function TopicsPage() {
  const navigate = useNavigate();
  const { selectedExams } = useSelector((state: RootState) => state.exam);
  
  const [topics, setTopics] = useState<TopicProgress[]>([]);
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState<ViewMode>('topics');
  
  // Practice mode state
  const [currentTopic, setCurrentTopic] = useState<TopicProgress | null>(null);
  const [questions, setQuestions] = useState<Question[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [selectedOption, setSelectedOption] = useState<number | null>(null);
  const [answerResult, setAnswerResult] = useState<AnswerResult | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    loadTopics();
  }, []);

  const loadTopics = async () => {
    try {
      setLoading(true);
      const data = await testService.getTopicsWithProgress(selectedExams);
      setTopics(data);
    } catch (error) {
      console.error('Failed to load topics:', error);
    } finally {
      setLoading(false);
    }
  };

  const startTopicPractice = async (topic: TopicProgress) => {
    try {
      setLoading(true);
      const data = await testService.getQuestionsByTopic(topic.topicId);
      setQuestions(data);
      setCurrentTopic(topic);
      setCurrentIndex(0);
      setSelectedOption(null);
      setAnswerResult(null);
      setViewMode('practice');
    } catch (error) {
      console.error('Failed to load topic questions:', error);
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
      setViewMode('topics');
      loadTopics(); // Refresh progress
    }
  };

  const getMasteryColor = (mastery: number) => {
    if (mastery >= 80) return 'bg-green-500';
    if (mastery >= 50) return 'bg-yellow-500';
    if (mastery >= 20) return 'bg-orange-500';
    return 'bg-red-500';
  };

  const getMasteryBgColor = (mastery: number) => {
    if (mastery >= 80) return 'bg-green-50 border-green-200';
    if (mastery >= 50) return 'bg-yellow-50 border-yellow-200';
    if (mastery >= 20) return 'bg-orange-50 border-orange-200';
    return 'bg-red-50 border-red-200';
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto" />
          <p className="mt-4 text-gray-600">Загрузка тем...</p>
        </div>
      </div>
    );
  }

  // Topics List View
  if (viewMode === 'topics') {
    return (
      <div className="min-h-screen bg-gradient-to-br from-purple-50 to-indigo-100 p-4 md:p-8">
        <div className="max-w-4xl mx-auto">
          {/* Header */}
          <div className="flex justify-between items-center mb-6">
            <h1 className="text-2xl font-bold text-gray-800">📚 Темы</h1>
            <button
              onClick={() => navigate('/test')}
              className="text-gray-600 hover:text-gray-800"
            >
              ← Назад
            </button>
          </div>

          {/* Stats Summary */}
          <div className="bg-white rounded-xl p-6 mb-6 shadow-lg">
            <div className="grid grid-cols-3 gap-4 text-center">
              <div>
                <div className="text-3xl font-bold text-indigo-600">{topics.length}</div>
                <div className="text-sm text-gray-600">Всего тем</div>
              </div>
              <div>
                <div className="text-3xl font-bold text-green-600">
                  {topics.filter(t => t.masteryPercentage >= 80).length}
                </div>
                <div className="text-sm text-gray-600">Освоено</div>
              </div>
              <div>
                <div className="text-3xl font-bold text-orange-600">
                  {topics.filter(t => t.masteryPercentage < 50).length}
                </div>
                <div className="text-sm text-gray-600">Требуют внимания</div>
              </div>
            </div>
          </div>

          {/* Topics Grid */}
          <div className="grid gap-4">
            {topics.map((topic) => (
              <div
                key={topic.topicId}
                className={`bg-white rounded-xl p-5 shadow-lg border-l-4 ${getMasteryBgColor(topic.masteryPercentage)} cursor-pointer hover:shadow-xl transition-shadow`}
                onClick={() => startTopicPractice(topic)}
              >
                <div className="flex justify-between items-start mb-3">
                  <div>
                    <h3 className="font-semibold text-gray-800">{topic.topicName}</h3>
                    <p className="text-sm text-gray-600">
                      {topic.totalQuestions} вопросов
                    </p>
                  </div>
                  <span className={`px-3 py-1 rounded-full text-sm font-medium ${
                    topic.masteryPercentage >= 80 ? 'bg-green-100 text-green-700' :
                    topic.masteryPercentage >= 50 ? 'bg-yellow-100 text-yellow-700' :
                    topic.masteryPercentage >= 20 ? 'bg-orange-100 text-orange-700' :
                    'bg-red-100 text-red-700'
                  }`}>
                    {topic.masteryPercentage}%
                  </span>
                </div>

                {/* Progress Bar */}
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div
                    className={`h-2 rounded-full transition-all duration-300 ${getMasteryColor(topic.masteryPercentage)}`}
                    style={{ width: `${topic.masteryPercentage}%` }}
                  />
                </div>

                <div className="mt-2 flex justify-between text-xs text-gray-500">
                  <span>✅ {topic.correctAnswers} правильно</span>
                  <span>❌ {topic.incorrectAnswers} ошибок</span>
                </div>
              </div>
            ))}
          </div>

          {topics.length === 0 && (
            <div className="text-center py-12">
              <div className="text-6xl mb-4">📭</div>
              <p className="text-gray-600">Темы не найдены для выбранных экзаменов</p>
            </div>
          )}
        </div>
      </div>
    );
  }

  // Practice Mode View
  const currentQuestion = questions[currentIndex];

  return (
    <div className="min-h-screen bg-gradient-to-br from-purple-50 to-indigo-100 p-4 md:p-8">
      <div className="max-w-3xl mx-auto">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <div className="flex items-center gap-3">
            <span className="px-3 py-1 bg-purple-100 text-purple-700 rounded-full text-sm font-medium">
              📚 {currentTopic?.topicName}
            </span>
          </div>
          <button
            onClick={() => setViewMode('topics')}
            className="text-gray-600 hover:text-gray-800"
          >
            ← К темам
          </button>
        </div>

        {/* Progress */}
        <div className="bg-white rounded-xl p-4 mb-6">
          <div className="flex justify-between text-sm text-gray-600 mb-2">
            <span>Вопрос {currentIndex + 1} из {questions.length}</span>
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2">
            <div
              className="bg-purple-500 h-2 rounded-full transition-all duration-300"
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
              let optionClass = 'border-2 border-gray-200 hover:border-purple-300';
              
              if (answerResult) {
                if (option.id === answerResult.correctOptionId) {
                  optionClass = 'border-2 border-green-500 bg-green-50';
                } else if (option.id === selectedOption && !answerResult.isCorrect) {
                  optionClass = 'border-2 border-red-500 bg-red-50';
                }
              } else if (selectedOption === option.id) {
                optionClass = 'border-2 border-purple-500 bg-purple-50';
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
                className="w-full py-3 bg-purple-600 text-white rounded-xl font-semibold hover:bg-purple-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
              >
                {submitting ? 'Проверка...' : 'Ответить'}
              </button>
            ) : (
              <button
                onClick={handleNext}
                className="w-full py-3 bg-indigo-600 text-white rounded-xl font-semibold hover:bg-indigo-700 transition-colors"
              >
                {currentIndex < questions.length - 1 ? 'Следующий вопрос' : 'Завершить тему'}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
