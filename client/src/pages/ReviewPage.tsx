import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import type { RootState } from '../store';
import type { Question, AnswerResult, DailyUsage } from '../types';
import { testService } from '../services/testService';
import { subscriptionService } from '../services/subscriptionService';
import { QuestionSkeleton } from '../components/Skeleton';
import { useTranslation } from '../hooks/useTranslation';
import { DailyLimitModal } from '../components/DailyLimitModal';

export default function ReviewPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { selectedExams } = useSelector((state: RootState) => state.exam);
  const { user } = useSelector((state: RootState) => state.auth);
  const isPro = user?.subscriptionTier === 'Pro';
  
  const [questions, setQuestions] = useState<Question[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [selectedOption, setSelectedOption] = useState<number | null>(null);
  const [answerResult, setAnswerResult] = useState<AnswerResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [, setDailyUsage] = useState<DailyUsage | null>(null);
  const [limitBlocked, setLimitBlocked] = useState(false);
  const [showLimitModal, setShowLimitModal] = useState(false);

  useEffect(() => {
    if (selectedExams.length === 0) {
      navigate('/');
      return;
    }
    if (!isPro) {
      subscriptionService.getDailyUsage().then((usage) => {
        setDailyUsage(usage);
        if (usage.isLimitReached) {
          setLimitBlocked(true);
          setLoading(false);
          return;
        }
        const limit = 5;
        loadWeakQuestions(limit);
      }).catch(() => {
        loadWeakQuestions();
      });
    } else {
      loadWeakQuestions();
    }
  }, [selectedExams, navigate, isPro]);

  const loadWeakQuestions = async (count?: number) => {
    try {
      setLoading(true);
      const data = await testService.getWeakQuestions(selectedExams, count);
      setQuestions(data);
    } catch (error) {
      console.error('Failed to load weak questions:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async () => {
    if (selectedOption === null) return;
    
    if (!isPro) {
      try {
        const usage = await subscriptionService.getDailyUsage();
        setDailyUsage(usage);
        if (usage.isLimitReached) {
          setLimitBlocked(true);
          return;
        }
      } catch {}
    }

    const currentQuestion = questions[currentIndex];
    setSubmitting(true);
    
    try {
      const result = await testService.submitAnswer({
        questionId: currentQuestion.id,
        answerOptionId: selectedOption,
      });
      setAnswerResult(result);
    } catch (error: any) {
      if (error?.response?.status === 429) {
        setLimitBlocked(true);
        return;
      }
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

  if (limitBlocked) {
    return (
      <div className="animate-fade-in" style={{ maxWidth: '500px', margin: '2rem auto' }}>
        <DailyLimitModal isOpen={showLimitModal} onClose={() => setShowLimitModal(false)} />
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <div style={{
            width: '64px', height: '64px', borderRadius: '50%',
            backgroundColor: 'rgba(239, 68, 68, 0.1)', display: 'flex',
            alignItems: 'center', justifyContent: 'center',
            margin: '0 auto 1rem', fontSize: '1.5rem', color: 'var(--error-color)',
          }}>
            !
          </div>
          <h2 style={{ fontSize: '1.25rem', fontWeight: 700, marginBottom: '0.75rem', color: 'var(--text-primary)' }}>
            {t.limits.reviewBlocked}
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem', lineHeight: 1.5 }}>
            {t.limits.reviewBlockedDesc}
          </p>
          <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'center', flexWrap: 'wrap' }}>
            <button
              onClick={() => setShowLimitModal(true)}
              className="btn btn-primary"
            >
              {t.limits.upgradePro}
            </button>
            <button
              onClick={() => navigate('/')}
              className="btn btn-secondary"
            >
              {t.practice.backToExams}
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (questions.length === 0) {
    return (
      <div className="card animate-fade-in-scale" style={{ maxWidth: '500px', margin: '2rem auto', textAlign: 'center', padding: '3rem' }}>
        <div style={{ fontSize: '4rem', marginBottom: '1rem' }}></div>
        <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '1rem', color: 'var(--text-primary)' }}>{t.reviewPage.excellent}</h2>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
          {t.reviewPage.noMistakesDesc}
        </p>
        <button onClick={() => navigate('/test')} className="btn btn-primary">
          {t.reviewPage.startNewTest}
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
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <span style={{ 
          padding: '0.375rem 0.75rem', 
          backgroundColor: 'rgba(245, 158, 11, 0.1)', 
          color: 'var(--warning-color)',
          borderRadius: '9999px',
          fontSize: '0.875rem',
          fontWeight: '500'
        }}>
          {t.reviewPage.reviewErrors}
        </span>
        <button onClick={() => navigate('/test')} className="btn btn-outline">
          {t.reviewPage.back}
        </button>
      </div>

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

      <div className="card" style={{ padding: '1rem', marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.875rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>
          <span>{t.reviewPage.questionOf.replace('{n}', String(currentIndex + 1)).replace('{total}', String(questions.length))}</span>
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

      <div className="card card-static animate-fade-in-up" key={currentQuestion.id}>
        <div style={{ marginBottom: '1rem' }}>
          <span style={{ 
            padding: '0.25rem 0.75rem', 
            borderRadius: '9999px', 
            fontSize: '0.75rem', 
            fontWeight: '500',
            ...getDifficultyStyle(currentQuestion.difficulty)
          }}>
            {currentQuestion.difficulty === 'Easy' ? t.practice.easy :
             currentQuestion.difficulty === 'Medium' ? t.practice.medium : t.practice.hard}
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
                {answerResult.isCorrect ? t.reviewPage.correct : t.reviewPage.incorrect}
              </span>
            </div>
            
            {!answerResult.isCorrect && (
              <p style={{ color: 'var(--text-primary)', marginBottom: '0.5rem' }}>
                <span style={{ fontWeight: '500' }}>{t.reviewPage.correctAnswerLabel} </span>
                {answerResult.correctOptionText}
              </p>
            )}
            
            {answerResult.explanation && (
              <div style={{ marginTop: '0.75rem', paddingTop: '0.75rem', borderTop: '1px solid var(--border-color)' }}>
                <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                  <span style={{ fontWeight: '500' }}>{t.reviewPage.explanation}: </span>
                  {answerResult.explanation}
                </p>
              </div>
            )}
          </div>
        )}

        <div style={{ marginTop: '1.5rem' }}>
          {!answerResult ? (
            <button
              onClick={handleSubmit}
              disabled={selectedOption === null || submitting}
              className="btn btn-primary"
              style={{ width: '100%' }}
            >
              {submitting ? t.reviewPage.checking : t.reviewPage.answer}
            </button>
          ) : (
            <button
              onClick={handleNext}
              className="btn btn-primary"
              style={{ width: '100%' }}
            >
              {currentIndex < questions.length - 1 ? t.reviewPage.nextQuestion : t.reviewPage.finish}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
