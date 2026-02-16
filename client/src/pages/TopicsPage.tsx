import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import type { RootState } from '../store';
import type { TopicProgress, Question, AnswerResult } from '../types';
import { testService } from '../services/testService';
import { TopicsSkeleton, QuestionSkeleton } from '../components/Skeleton';

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
    if (selectedExams.length === 0) {
      navigate('/');
      return;
    }
    loadTopics();
  }, [selectedExams, navigate]);

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
      loadTopics();
    }
  };

  const getMasteryColor = (mastery: number) => {
    if (mastery >= 80) return 'var(--success-color)';
    if (mastery >= 50) return 'var(--warning-color)';
    return 'var(--error-color)';
  };

  const getMasteryBadgeStyle = (mastery: number): React.CSSProperties => {
    if (mastery >= 80) return { backgroundColor: 'rgba(16, 185, 129, 0.1)', color: 'var(--success-color)' };
    if (mastery >= 50) return { backgroundColor: 'rgba(245, 158, 11, 0.1)', color: 'var(--warning-color)' };
    return { backgroundColor: 'rgba(239, 68, 68, 0.1)', color: 'var(--error-color)' };
  };

  const getDifficultyStyle = (difficulty: string) => {
    switch(difficulty) {
      case 'Easy': return { backgroundColor: 'rgba(16, 185, 129, 0.1)', color: 'var(--success-color)' };
      case 'Medium': return { backgroundColor: 'rgba(245, 158, 11, 0.1)', color: 'var(--warning-color)' };
      default: return { backgroundColor: 'rgba(239, 68, 68, 0.1)', color: 'var(--error-color)' };
    }
  };

  if (loading && viewMode === 'topics') {
    return (
      <div style={{ maxWidth: '800px', margin: '0 auto' }}>
        <div className="skeleton" style={{ height: '2rem', width: '150px', marginBottom: '1.5rem' }} />
        <TopicsSkeleton />
      </div>
    );
  }

  if (loading && viewMode === 'practice') {
    return (
      <div style={{ maxWidth: '700px', margin: '0 auto' }}>
        <QuestionSkeleton />
      </div>
    );
  }

  // Topics List View
  if (viewMode === 'topics') {
    return (
      <div className="animate-fade-in" style={{ maxWidth: '800px', margin: '0 auto' }}>
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <h1 style={{ fontSize: '1.5rem', fontWeight: '700', color: 'var(--text-primary)' }}>📚 Темы</h1>
          <button onClick={() => navigate('/test')} className="btn btn-outline">
            ← Назад
          </button>
        </div>

        {/* Selected Exams */}
        {selectedExams.length > 0 && (
          <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginBottom: '1.5rem' }}>
            {selectedExams.map(exam => (
              <span key={exam} style={{
                padding: '0.25rem 0.75rem',
                backgroundColor: 'var(--primary-color)',
                color: 'white',
                borderRadius: '9999px',
                fontSize: '0.75rem',
                fontWeight: '500'
              }}>
                {exam}
              </span>
            ))}
          </div>
        )}

        {/* Stats Summary */}
        <div className="card animate-fade-in-up" style={{ marginBottom: '1.5rem', padding: '1.5rem' }}>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '1rem', textAlign: 'center' }}>
            <div>
              <div style={{ fontSize: '1.75rem', fontWeight: '700', color: 'var(--primary-color)' }}>{topics.length}</div>
              <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Всего тем</div>
            </div>
            <div>
              <div style={{ fontSize: '1.75rem', fontWeight: '700', color: 'var(--success-color)' }}>
                {topics.filter(t => t.masteryPercentage >= 80).length}
              </div>
              <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Освоено</div>
            </div>
            <div>
              <div style={{ fontSize: '1.75rem', fontWeight: '700', color: 'var(--warning-color)' }}>
                {topics.filter(t => t.masteryPercentage < 50).length}
              </div>
              <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Требуют внимания</div>
            </div>
          </div>
        </div>

        {/* Topics Grid */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {topics.map((topic, index) => (
            <div
              key={topic.topicId}
              className="card animate-fade-in-up"
              style={{ 
                cursor: 'pointer', 
                padding: '1.25rem',
                borderLeft: `4px solid ${getMasteryColor(topic.masteryPercentage)}`,
                animationDelay: `${index * 0.05}s`
              }}
              onClick={() => startTopicPractice(topic)}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                <div>
                  <h3 style={{ fontWeight: '600', color: 'var(--text-primary)', marginBottom: '0.25rem' }}>{topic.topicName}</h3>
                  <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                    {topic.totalQuestions} {topic.totalQuestions === 1 ? 'вопрос' : 'вопросов'} в теме
                  </p>
                </div>
                <span style={{ 
                  padding: '0.25rem 0.75rem', 
                  borderRadius: '9999px', 
                  fontSize: '0.875rem', 
                  fontWeight: '500',
                  ...getMasteryBadgeStyle(topic.masteryPercentage)
                }}>
                  {topic.masteryPercentage}%
                </span>
              </div>

              {/* Progress Bar */}
              <div style={{ width: '100%', backgroundColor: 'var(--border-color)', borderRadius: '9999px', height: '0.5rem' }}>
                <div style={{ 
                  width: `${Math.min(topic.masteryPercentage, 100)}%`,
                  backgroundColor: getMasteryColor(topic.masteryPercentage),
                  height: '0.5rem',
                  borderRadius: '9999px',
                  transition: 'width 0.3s ease'
                }} />
              </div>

              <div style={{ marginTop: '0.5rem', display: 'flex', justifyContent: 'space-between', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                <span>✅ {topic.correctAnswers} верных ответов</span>
                <span>❌ {topic.incorrectAnswers} ошибок</span>
              </div>
            </div>
          ))}
        </div>

        {topics.length === 0 && (
          <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
            <div style={{ fontSize: '4rem', marginBottom: '1rem' }}>📭</div>
            <p style={{ color: 'var(--text-secondary)' }}>Темы не найдены для выбранных экзаменов</p>
          </div>
        )}
      </div>
    );
  }

  // Practice Mode View
  const currentQuestion = questions[currentIndex];

  return (
    <div className="animate-fade-in" style={{ maxWidth: '700px', margin: '0 auto' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <span style={{ 
          padding: '0.375rem 0.75rem', 
          backgroundColor: 'rgba(79, 70, 229, 0.1)', 
          color: 'var(--primary-color)',
          borderRadius: '9999px',
          fontSize: '0.875rem',
          fontWeight: '500'
        }}>
          📚 {currentTopic?.topicName}
        </span>
        <button onClick={() => setViewMode('topics')} className="btn btn-outline">
          ← К темам
        </button>
      </div>

      {/* Progress */}
      <div className="card" style={{ padding: '1rem', marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.875rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>
          <span>Вопрос {currentIndex + 1} из {questions.length}</span>
        </div>
        <div style={{ width: '100%', backgroundColor: 'var(--border-color)', borderRadius: '9999px', height: '0.5rem' }}>
          <div style={{ 
            width: `${((currentIndex + 1) / questions.length) * 100}%`,
            backgroundColor: 'var(--primary-color)',
            height: '0.5rem',
            borderRadius: '9999px',
            transition: 'width 0.3s ease'
          }} />
        </div>
      </div>

      {/* Question Card */}
      <div className="card card-static animate-fade-in-up" key={currentQuestion.id}>
        <div style={{ marginBottom: '1rem' }}>
          <span style={{ 
            padding: '0.25rem 0.75rem', 
            borderRadius: '9999px', 
            fontSize: '0.75rem', 
            fontWeight: '500',
            ...getDifficultyStyle(currentQuestion.difficulty)
          }}>
            {currentQuestion.difficulty === 'Easy' ? 'Легкий' :
             currentQuestion.difficulty === 'Medium' ? 'Средний' : 'Сложный'}
          </span>
        </div>

        <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1.5rem', color: 'var(--text-primary)' }}>
          {currentQuestion.text}
        </h2>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {currentQuestion.options.map((option) => {
            let style: React.CSSProperties = { 
              border: '2px solid var(--border-color)',
              backgroundColor: 'var(--card-background)'
            };
            
            if (answerResult) {
              if (option.id === answerResult.correctOptionId) {
                style = { border: '2px solid var(--success-color)', backgroundColor: 'rgba(16, 185, 129, 0.1)' };
              } else if (option.id === selectedOption && !answerResult.isCorrect) {
                style = { border: '2px solid var(--error-color)', backgroundColor: 'rgba(239, 68, 68, 0.1)' };
              }
            } else if (selectedOption === option.id) {
              style = { border: '2px solid var(--primary-color)', backgroundColor: 'rgba(79, 70, 229, 0.1)' };
            }

            return (
              <button
                key={option.id}
                onClick={() => !answerResult && setSelectedOption(option.id)}
                disabled={answerResult !== null}
                style={{
                  ...style,
                  width: '100%',
                  padding: '1rem',
                  borderRadius: '0.75rem',
                  textAlign: 'left',
                  cursor: answerResult ? 'default' : 'pointer',
                  transition: 'all 0.2s ease',
                  color: 'var(--text-primary)',
                  fontSize: '0.875rem'
                }}
              >
                {option.text}
              </button>
            );
          })}
        </div>

        {/* Result Feedback */}
        {answerResult && (
          <div className="animate-fade-in-up" style={{
            marginTop: '1.5rem',
            padding: '1rem',
            borderRadius: '0.75rem',
            backgroundColor: answerResult.isCorrect ? 'rgba(16, 185, 129, 0.1)' : 'rgba(239, 68, 68, 0.1)',
            border: `1px solid ${answerResult.isCorrect ? 'var(--success-color)' : 'var(--error-color)'}`
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
              <span style={{ fontSize: '1.5rem' }}>{answerResult.isCorrect ? '✅' : '❌'}</span>
              <span style={{ fontWeight: '600', color: answerResult.isCorrect ? 'var(--success-color)' : 'var(--error-color)' }}>
                {answerResult.isCorrect ? 'Правильно!' : 'Неправильно'}
              </span>
            </div>
            
            {!answerResult.isCorrect && (
              <p style={{ color: 'var(--text-primary)', marginBottom: '0.5rem' }}>
                <span style={{ fontWeight: '500' }}>Правильный ответ: </span>
                {answerResult.correctOptionText}
              </p>
            )}
            
            {answerResult.explanation && (
              <div style={{ marginTop: '0.75rem', paddingTop: '0.75rem', borderTop: '1px solid var(--border-color)' }}>
                <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                  <span style={{ fontWeight: '500' }}>Объяснение: </span>
                  {answerResult.explanation}
                </p>
              </div>
            )}
          </div>
        )}

        {/* Actions */}
        <div style={{ marginTop: '1.5rem' }}>
          {!answerResult ? (
            <button
              onClick={handleSubmit}
              disabled={selectedOption === null || submitting}
              className="btn btn-primary"
              style={{ width: '100%' }}
            >
              {submitting ? 'Проверка...' : 'Ответить'}
            </button>
          ) : (
            <button
              onClick={handleNext}
              className="btn btn-primary"
              style={{ width: '100%' }}
            >
              {currentIndex < questions.length - 1 ? 'Следующий вопрос' : 'Завершить тему'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
