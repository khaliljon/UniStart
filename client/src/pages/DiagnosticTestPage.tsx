import { useState, useEffect, useRef } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { diagnosticService } from '../services/diagnosticService';
import axios from 'axios';
import type {
  DiagnosticSession,
  DiagnosticQuestion,
  DiagnosticAnswerResult,
  DiagnosticResult,
} from '../types';

type Phase = 'intro' | 'testing' | 'feedback' | 'results';

const LEVEL_COLORS: Record<string, string> = {
  'Начинающий': '#ef4444',
  'Средний': '#f59e0b',
  'Хороший': '#10b981',
  'Продвинутый': '#6366f1',
  'Отличный': '#8b5cf6',
};

const DIFF_COLORS: Record<string, string> = {
  Easy: '#10b981',
  Medium: '#f59e0b',
  Hard: '#ef4444',
};

function DiagnosticTestPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const examTypeCode = searchParams.get('exam');

  const [phase, setPhase] = useState<Phase>('intro');
  const [session, setSession] = useState<DiagnosticSession | null>(null);
  const [question, setQuestion] = useState<DiagnosticQuestion | null>(null);
  const [selectedOption, setSelectedOption] = useState<number | null>(null);
  const [answerResult, setAnswerResult] = useState<DiagnosticAnswerResult | null>(null);
  const [results, setResults] = useState<DiagnosticResult | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showReview, setShowReview] = useState(false);
  const questionStartRef = useRef<number>(Date.now());

  const handleStart = async () => {
    if (!examTypeCode) return;
    setIsLoading(true);
    setError(null);
    try {
      const sess = await diagnosticService.start(examTypeCode);
      setSession(sess);
      const q = await diagnosticService.getCurrentQuestion(sess.sessionId);
      setQuestion(q);
      questionStartRef.current = Date.now();
      setPhase('testing');
    } catch (err: unknown) {
      setError(axios.isAxiosError(err) ? err.response?.data?.error || 'Не удалось начать диагностику' : 'Не удалось начать диагностику');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSelectOption = (optionId: number) => {
    if (answerResult) return; // already answered
    setSelectedOption(optionId);
  };

  const handleSubmit = async () => {
    if (!session || !question || selectedOption === null) return;
    setIsLoading(true);
    try {
      const timeSpent = Math.round((Date.now() - questionStartRef.current) / 1000);
      const result = await diagnosticService.submitAnswer(
        session.sessionId,
        question.questionId,
        selectedOption,
        timeSpent
      );
      setAnswerResult(result);
      setPhase('feedback');

      if (result.isCompleted) {
        // Auto-load results after a brief delay
        setTimeout(async () => {
          const res = await diagnosticService.getResults(session.sessionId);
          setResults(res);
          setPhase('results');
        }, 1500);
      }
    } catch (err: unknown) {
      setError(axios.isAxiosError(err) ? err.response?.data?.error || 'Ошибка при отправке ответа' : 'Ошибка при отправке ответа');
    } finally {
      setIsLoading(false);
    }
  };

  const handleNext = async () => {
    if (!session) return;
    setSelectedOption(null);
    setAnswerResult(null);
    setIsLoading(true);
    try {
      const q = await diagnosticService.getCurrentQuestion(session.sessionId);
      if (q) {
        setQuestion(q);
        questionStartRef.current = Date.now();
        setPhase('testing');
      } else {
        // all done
        const res = await diagnosticService.getResults(session.sessionId);
        setResults(res);
        setPhase('results');
      }
    } catch (err: unknown) {
      setError(axios.isAxiosError(err) ? err.response?.data?.error || 'Ошибка загрузки вопроса' : 'Ошибка загрузки вопроса');
    } finally {
      setIsLoading(false);
    }
  };

  // Redirect if no exam type specified
  useEffect(() => {
    if (!examTypeCode) {
      navigate('/', { replace: true });
    }
  }, [examTypeCode, navigate]);

  if (!examTypeCode) return null;

  return (
    <div style={{
      minHeight: '100vh',
      background: 'var(--bg-primary, #f9fafb)',
      padding: '2rem',
    }}>
      <div style={{
        maxWidth: '720px',
        margin: '0 auto',
      }}>

        {/* ─── Intro ─── */}
        {phase === 'intro' && (
          <div style={{
            background: 'var(--card-bg, #fff)',
            borderRadius: '1.5rem',
            padding: '3rem',
            textAlign: 'center',
            boxShadow: '0 4px 24px rgba(0,0,0,0.08)',
          }}>
            <h1 style={{ fontSize: '1.75rem', marginBottom: '0.75rem', color: 'var(--text-primary)' }}>
              Диагностический тест
            </h1>
            <div style={{
              display: 'inline-block',
              padding: '0.3rem 1rem',
              borderRadius: '2rem',
              background: 'rgba(99, 102, 241, 0.08)',
              color: 'var(--primary-color)',
              fontWeight: 600,
              fontSize: '0.9rem',
              marginBottom: '1.5rem',
            }}>
              {examTypeCode}
            </div>
            <p style={{
              color: 'var(--text-secondary)',
              fontSize: '1.05rem',
              lineHeight: 1.7,
              marginBottom: '2rem',
              maxWidth: '500px',
              marginLeft: 'auto',
              marginRight: 'auto',
            }}>
              10 вопросов разной сложности из каждой секции экзамена.
              Это поможет определить ваш текущий уровень и спрогнозировать примерный балл.
            </p>

            <div style={{
              background: 'var(--bg-secondary, #f3f4f6)',
              borderRadius: '1rem',
              padding: '1.25rem',
              marginBottom: '2rem',
              textAlign: 'left',
            }}>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.6rem' }}>
                {[
                  { icon: '', text: 'Занимает ~5 минут' },
                  { icon: '', text: 'Вопросы Easy / Medium / Hard из каждой секции' },
                  { icon: '', text: 'Получите прогноз балла и уровень' },
                  { icon: '', text: 'После каждого вопроса — объяснение' },
                ].map((item, i) => (
                  <div key={i} style={{ display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
                    <span style={{ fontSize: '1.1rem' }}>{item.icon}</span>
                    <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{item.text}</span>
                  </div>
                ))}
              </div>
            </div>

            {error && (
              <div style={{
                background: 'var(--error-bg)',
                color: 'var(--error-text)',
                padding: '0.75rem',
                borderRadius: '0.5rem',
                marginBottom: '1rem',
                fontSize: '0.9rem',
              }}>
                {error}
              </div>
            )}

            <button
              onClick={handleStart}
              disabled={isLoading}
              className="btn btn-primary"
              style={{
                width: '100%',
                padding: '1rem',
                fontSize: '1.1rem',
                borderRadius: '0.75rem',
              }}
            >
              {isLoading ? 'Подготовка...' : 'Начать диагностику'}
            </button>

            <button
              onClick={() => navigate('/')}
              style={{
                background: 'none',
                border: 'none',
                color: 'var(--text-secondary)',
                cursor: 'pointer',
                marginTop: '1rem',
                fontSize: '0.9rem',
              }}
            >
              Пропустить и перейти к платформе →
            </button>
          </div>
        )}

        {/* ─── Testing / Feedback ─── */}
        {(phase === 'testing' || phase === 'feedback') && question && (
          <div>
            {/* Progress bar */}
            <div style={{ marginBottom: '1.5rem' }}>
              <div style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: '0.5rem',
              }}>
                <span style={{ fontWeight: 600, color: 'var(--text-primary)' }}>
                  Вопрос {question.index + 1} из {question.totalQuestions}
                </span>
                <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                  <span style={{
                    padding: '0.15rem 0.5rem',
                    borderRadius: '0.25rem',
                    background: `${DIFF_COLORS[question.difficulty] || '#6366f1'}15`,
                    color: DIFF_COLORS[question.difficulty] || '#6366f1',
                    fontSize: '0.75rem',
                    fontWeight: 600,
                  }}>
                    {question.difficulty}
                  </span>
                  <span style={{
                    fontSize: '0.8rem',
                    color: 'var(--text-secondary)',
                  }}>
                    {question.sectionName}
                  </span>
                </div>
              </div>
              <div style={{
                height: '6px',
                background: 'var(--border-color, #e5e7eb)',
                borderRadius: '3px',
                overflow: 'hidden',
              }}>
                <div style={{
                  height: '100%',
                  width: `${((question.index + (phase === 'feedback' ? 1 : 0)) / question.totalQuestions) * 100}%`,
                  background: 'linear-gradient(90deg, #6366f1, #8b5cf6)',
                  borderRadius: '3px',
                  transition: 'width 0.5s ease',
                }} />
              </div>
            </div>

            {/* Question card */}
            <div style={{
              background: 'var(--card-bg, #fff)',
              borderRadius: '1rem',
              padding: '2rem',
              boxShadow: '0 2px 12px rgba(0,0,0,0.06)',
            }}>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>
                {question.topicName}
              </div>
              <h2 style={{
                fontSize: '1.15rem',
                lineHeight: 1.6,
                color: 'var(--text-primary)',
                marginBottom: '1.5rem',
                fontWeight: 500,
              }}>
                {question.text}
              </h2>

              {/* Options */}
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                {question.options.map((opt, i) => {
                  const letter = String.fromCharCode(65 + i);
                  const isSelected = selectedOption === opt.id;
                  const isCorrect = answerResult && opt.id === answerResult.correctOptionId;
                  const isWrong = answerResult && isSelected && !answerResult.isCorrect;

                  let borderColor = 'var(--border-color, #e5e7eb)';
                  let bgColor = 'transparent';
                  if (answerResult) {
                    if (isCorrect) { borderColor = 'var(--success-color)'; bgColor = 'rgba(16, 185, 129, 0.06)'; }
                    else if (isWrong) { borderColor = 'var(--error-color)'; bgColor = 'rgba(239, 68, 68, 0.06)'; }
                  } else if (isSelected) {
                    borderColor = 'var(--primary-color)';
                    bgColor = 'rgba(99, 102, 241, 0.03)';
                  }

                  return (
                    <button
                      key={opt.id}
                      onClick={() => handleSelectOption(opt.id)}
                      disabled={!!answerResult}
                      style={{
                        display: 'flex',
                        alignItems: 'flex-start',
                        gap: '0.75rem',
                        padding: '1rem',
                        border: `2px solid ${borderColor}`,
                        borderRadius: '0.75rem',
                        background: bgColor,
                        cursor: answerResult ? 'default' : 'pointer',
                        textAlign: 'left',
                        transition: 'all 0.2s',
                        width: '100%',
                      }}
                    >
                      <span style={{
                        width: '28px',
                        height: '28px',
                        borderRadius: '50%',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontSize: '0.8rem',
                        fontWeight: 600,
                        flexShrink: 0,
                        background: isSelected && !answerResult ? '#6366f1' : 'var(--bg-secondary, #f3f4f6)',
                        color: isSelected && !answerResult ? '#fff' : 'var(--text-secondary)',
                      }}>
                        {letter}
                      </span>
                      <span style={{ color: 'var(--text-primary)', lineHeight: 1.5 }}>
                        {opt.text}
                      </span>
                    </button>
                  );
                })}
              </div>

              {/* Feedback */}
              {answerResult && (
                <div style={{
                  marginTop: '1.5rem',
                  padding: '1rem 1.25rem',
                  borderRadius: '0.75rem',
                  background: answerResult.isCorrect ? '#10b98110' : '#ef444410',
                  borderLeft: `4px solid ${answerResult.isCorrect ? '#10b981' : '#ef4444'}`,
                }}>
                  <div style={{
                    fontWeight: 600,
                    color: answerResult.isCorrect ? '#10b981' : '#ef4444',
                    marginBottom: '0.5rem',
                    fontSize: '0.95rem',
                  }}>
                    {answerResult.isCorrect ? '✓ Правильно!' : '✗ Неправильно'}
                  </div>
                  {!answerResult.isCorrect && (
                    <div style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '0.5rem' }}>
                      Правильный ответ: <strong>{answerResult.correctOptionText}</strong>
                    </div>
                  )}
                  {answerResult.explanation && (
                    <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', lineHeight: 1.6 }}>
                      {answerResult.explanation}
                    </div>
                  )}
                </div>
              )}

              {/* Actions */}
              <div style={{ marginTop: '1.5rem' }}>
                {!answerResult ? (
                  <button
                    onClick={handleSubmit}
                    disabled={selectedOption === null || isLoading}
                    className="btn btn-primary"
                    style={{
                      width: '100%',
                      padding: '0.85rem',
                      borderRadius: '0.75rem',
                      opacity: selectedOption === null ? 0.5 : 1,
                    }}
                  >
                    {isLoading ? 'Проверяем...' : 'Ответить'}
                  </button>
                ) : !answerResult.isCompleted ? (
                  <button
                    onClick={handleNext}
                    disabled={isLoading}
                    className="btn btn-primary"
                    style={{
                      width: '100%',
                      padding: '0.85rem',
                      borderRadius: '0.75rem',
                    }}
                  >
                    {isLoading ? 'Загрузка...' : 'Следующий вопрос →'}
                  </button>
                ) : (
                  <div style={{
                    textAlign: 'center',
                    padding: '1rem',
                    color: 'var(--primary-color)',
                    fontWeight: 600,
                    fontSize: '1.05rem',
                  }}>
                    Загружаем результаты...
                  </div>
                )}
              </div>
            </div>
          </div>
        )}

        {/* ─── Results ─── */}
        {phase === 'results' && results && (
          <div>
            {/* Header */}
            <div style={{
              background: 'linear-gradient(135deg, #667eea, #764ba2)',
              borderRadius: '1.5rem',
              padding: '2.5rem',
              textAlign: 'center',
              color: '#fff',
              marginBottom: '1.5rem',
            }}>
              <div style={{ fontSize: '1rem', opacity: 0.9, marginBottom: '0.5rem' }}>
                Диагностика {results.examTypeName}
              </div>
              <div style={{ fontSize: '3.5rem', fontWeight: 700, marginBottom: '0.5rem' }}>
                {results.predictedScore}
              </div>
              <div style={{ fontSize: '0.9rem', opacity: 0.8, marginBottom: '1rem' }}>
                прогноз из {results.maxPossibleScore} ({results.predictedScoreMin}–{results.predictedScoreMax})
              </div>
              <div style={{
                display: 'inline-block',
                padding: '0.4rem 1.25rem',
                borderRadius: '2rem',
                background: 'rgba(255,255,255,0.2)',
                fontWeight: 600,
                fontSize: '0.95rem',
              }}>
                Уровень: {results.level}
              </div>
            </div>

            {/* Score breakdown */}
            <div style={{
              display: 'grid',
              gridTemplateColumns: '1fr 1fr 1fr',
              gap: '1rem',
              marginBottom: '1.5rem',
            }}>
              {[
                { label: 'Правильно', value: `${results.correctCount}/${results.totalQuestions}`, color: 'var(--success-color)' },
                { label: 'Точность', value: `${results.scorePercent}%`, color: 'var(--primary-color)' },
                { label: 'Уровень', value: results.level, color: LEVEL_COLORS[results.level] || 'var(--primary-color)' },
              ].map((stat) => (
                <div key={stat.label} style={{
                  background: 'var(--card-bg, #fff)',
                  borderRadius: '1rem',
                  padding: '1.25rem',
                  textAlign: 'center',
                  boxShadow: '0 2px 8px rgba(0,0,0,0.04)',
                }}>
                  <div style={{ fontSize: '1.5rem', fontWeight: 700, color: stat.color }}>
                    {stat.value}
                  </div>
                  <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                    {stat.label}
                  </div>
                </div>
              ))}
            </div>

            {/* Section results */}
            {results.sectionResults.length > 1 && (
              <div style={{
                background: 'var(--card-bg, #fff)',
                borderRadius: '1rem',
                padding: '1.5rem',
                marginBottom: '1.5rem',
                boxShadow: '0 2px 8px rgba(0,0,0,0.04)',
              }}>
                <h3 style={{ margin: '0 0 1rem', fontSize: '1.1rem', color: 'var(--text-primary)' }}>
                  Результаты по секциям
                </h3>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                  {results.sectionResults.map((sr) => (
                    <div key={sr.sectionName}>
                      <div style={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        marginBottom: '0.3rem',
                        fontSize: '0.9rem',
                      }}>
                        <span style={{ color: 'var(--text-primary)' }}>{sr.sectionName}</span>
                        <span style={{ fontWeight: 600, color: sr.scorePercent >= 70 ? '#10b981' : sr.scorePercent >= 40 ? '#f59e0b' : '#ef4444' }}>
                          {sr.correctCount}/{sr.totalQuestions} ({sr.scorePercent}%)
                        </span>
                      </div>
                      <div style={{
                        height: '6px',
                        background: 'var(--border-color, #e5e7eb)',
                        borderRadius: '3px',
                        overflow: 'hidden',
                      }}>
                        <div style={{
                          height: '100%',
                          width: `${sr.scorePercent}%`,
                          background: sr.scorePercent >= 70 ? '#10b981' : sr.scorePercent >= 40 ? '#f59e0b' : '#ef4444',
                          borderRadius: '3px',
                          transition: 'width 0.5s',
                        }} />
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Answer review toggle */}
            <div style={{
              background: 'var(--card-bg, #fff)',
              borderRadius: '1rem',
              padding: '1.5rem',
              marginBottom: '1.5rem',
              boxShadow: '0 2px 8px rgba(0,0,0,0.04)',
            }}>
              <button
                onClick={() => setShowReview(!showReview)}
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  width: '100%',
                  padding: 0,
                }}
              >
                <h3 style={{ margin: 0, fontSize: '1.1rem', color: 'var(--text-primary)' }}>
                  Разбор ответов
                </h3>
                <span style={{ color: 'var(--text-secondary)', fontSize: '1.2rem' }}>
                  {showReview ? '−' : '+'}
                </span>
              </button>

              {showReview && (
                <div style={{ marginTop: '1rem', display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                  {results.answers.map((a, i) => (
                    <div key={a.questionId} style={{
                      padding: '1rem',
                      borderRadius: '0.75rem',
                      border: `1px solid ${a.isCorrect ? '#10b98130' : '#ef444430'}`,
                      background: a.isCorrect ? '#10b98105' : '#ef444405',
                    }}>
                      <div style={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center',
                        marginBottom: '0.5rem',
                      }}>
                        <span style={{
                          fontWeight: 600,
                          fontSize: '0.85rem',
                          color: a.isCorrect ? '#10b981' : '#ef4444',
                        }}>
                          {a.isCorrect ? '✓' : '✗'} Вопрос {i + 1}
                        </span>
                        <div style={{ display: 'flex', gap: '0.5rem' }}>
                          <span style={{
                            fontSize: '0.7rem',
                            padding: '0.1rem 0.4rem',
                            borderRadius: '0.25rem',
                            background: `${DIFF_COLORS[a.difficulty] || '#6366f1'}15`,
                            color: DIFF_COLORS[a.difficulty] || '#6366f1',
                          }}>
                            {a.difficulty}
                          </span>
                          <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                            {a.topicName}
                          </span>
                        </div>
                      </div>
                      <div style={{ fontSize: '0.9rem', color: 'var(--text-primary)', marginBottom: '0.5rem', lineHeight: 1.5 }}>
                        {a.questionText}
                      </div>
                      {!a.isCorrect && (
                        <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                          Ваш ответ: <span style={{ color: 'var(--error-color)' }}>{a.selectedOptionText}</span>
                          {' → '}Правильно: <span style={{ color: 'var(--success-color)', fontWeight: 600 }}>{a.correctOptionText}</span>
                        </div>
                      )}
                      {a.explanation && (
                        <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.4rem', lineHeight: 1.5 }}>
                          {a.explanation}
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* CTA buttons */}
            <div style={{ display: 'flex', gap: '1rem' }}>
              <button
                onClick={() => navigate('/study-plan')}
                className="btn btn-primary"
                style={{
                  flex: 1,
                  padding: '1rem',
                  borderRadius: '0.75rem',
                  fontSize: '1rem',
                }}
              >
                Перейти к плану подготовки
              </button>
              <button
                onClick={() => navigate('/')}
                className="btn btn-outline"
                style={{
                  flex: 1,
                  padding: '1rem',
                  borderRadius: '0.75rem',
                  fontSize: '1rem',
                }}
              >
                На главную
              </button>
            </div>
          </div>
        )}

        {/* Global error */}
        {error && phase !== 'intro' && (
          <div style={{
            background: 'var(--error-bg)',
            color: 'var(--error-text)',
            padding: '0.75rem 1rem',
            borderRadius: '0.5rem',
            marginTop: '1rem',
            fontSize: '0.9rem',
          }}>
            {error}
          </div>
        )}
      </div>
    </div>
  );
}

export default DiagnosticTestPage;
