import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import type { RootState } from '../store';
import type { Question, AnswerResult } from '../types';
import { testService } from '../services/testService';
import { QuestionSkeleton } from '../components/Skeleton';

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
    if (selectedExams.length === 0) {
      navigate('/');
      return;
    }
    loadWeakQuestions();
  }, [selectedExams, navigate]);

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
      <div style={{ maxWidth: '700px', margin: '0 auto' }}>
        <div style={{ marginBottom: '1.5rem' }}>
          <div className="skeleton" style={{ height: '2rem', width: '200px', marginBottom: '1rem' }} />
        </div>
        <QuestionSkeleton />
      </div>
    );
  }

  if (questions.length === 0) {
    return (
      <div className="card animate-fade-in-scale" style={{ maxWidth: '500px', margin: '2rem auto', textAlign: 'center', padding: '3rem' }}>
        <div style={{ fontSize: '4rem', marginBottom: '1rem' }}></div>
        <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '1rem', color: 'var(--text-primary)' }}>Отлично!</h2>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
          У вас нет ошибок для повторения. Продолжайте практиковаться!
        </p>
        <button onClick={() => navigate('/test')} className="btn btn-primary">
          Начать новый тест
        </button>
      </div>
    );
  }

  const currentQuestion = questions[currentIndex];

  const getDifficultyStyle = (difficulty: string) => {
    switch(difficulty) {
      case 'Easy': return { backgroundColor: 'rgba(16, 185, 129, 0.1)', color: 'var(--success-color)' };
      case 'Medium': return { backgroundColor: 'rgba(245, 158, 11, 0.1)', color: 'var(--warning-color)' };
      default: return { backgroundColor: 'rgba(239, 68, 68, 0.1)', color: 'var(--error-color)' };
    }
  };

  return (
    <div className="animate-fade-in" style={{ maxWidth: '700px', margin: '0 auto' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <span style={{ 
          padding: '0.375rem 0.75rem', 
          backgroundColor: 'rgba(245, 158, 11, 0.1)', 
          color: 'var(--warning-color)',
          borderRadius: '9999px',
          fontSize: '0.875rem',
          fontWeight: '500'
        }}>
          Повторение ошибок
        </span>
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

      {/* Progress */}
      <div className="card" style={{ padding: '1rem', marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.875rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>
          <span>Вопрос {currentIndex + 1} из {questions.length}</span>
          <span>{currentQuestion.topicName}</span>
        </div>
        <div style={{ width: '100%', backgroundColor: 'var(--border-color)', borderRadius: '9999px', height: '0.5rem' }}>
          <div style={{ 
            width: `${((currentIndex + 1) / questions.length) * 100}%`,
            backgroundColor: 'var(--warning-color)',
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
              <span style={{ fontSize: '1.5rem' }}>{answerResult.isCorrect ? '✓' : '✕'}</span>
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
              {currentIndex < questions.length - 1 ? 'Следующий вопрос' : 'Завершить'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
