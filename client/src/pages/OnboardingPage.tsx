import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { setOnboardingComplete } from '../store/slices/authSlice';
import { onboardingService } from '../services/onboardingService';
import type { ExamTypeInfo } from '../types';

type Step = 'welcome' | 'exam' | 'target' | 'ready';

const EXAM_ICONS: Record<string, string> = {
  SAT: '🎓',
  TOEFL: '🌍',
  NUET: '🇰🇿',
};

const EXAM_COLORS: Record<string, string> = {
  SAT: '#6366f1',
  TOEFL: '#8b5cf6',
  NUET: '#f59e0b',
};

function OnboardingPage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);

  const [step, setStep] = useState<Step>('welcome');
  const [examTypes, setExamTypes] = useState<ExamTypeInfo[]>([]);
  const [selectedExam, setSelectedExam] = useState<ExamTypeInfo | null>(null);
  const [targetDate, setTargetDate] = useState('');
  const [targetScore, setTargetScore] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Redirect if already onboarded
  useEffect(() => {
    if (user?.hasCompletedOnboarding) {
      navigate('/', { replace: true });
    }
  }, [user, navigate]);

  // Load exam types when entering exam step
  useEffect(() => {
    if (step === 'exam' && examTypes.length === 0) {
      loadExamTypes();
    }
  }, [step]);

  const loadExamTypes = async () => {
    try {
      const types = await onboardingService.getExamTypes();
      setExamTypes(types);
    } catch {
      setError('Не удалось загрузить типы экзаменов');
    }
  };

  const handleExamSelect = (exam: ExamTypeInfo) => {
    setSelectedExam(exam);
    // Set defaults
    const defaultDate = new Date();
    defaultDate.setMonth(defaultDate.getMonth() + 3);
    setTargetDate(defaultDate.toISOString().split('T')[0]);
    setTargetScore(Math.round((exam.minScore + exam.maxScore) / 2));
    setStep('target');
  };

  const handleComplete = async () => {
    if (!selectedExam) return;
    setIsLoading(true);
    setError(null);
    try {
      await onboardingService.complete({
        examTypeCode: selectedExam.code,
        targetDate: targetDate,
        targetScore: targetScore,
      });
      dispatch(setOnboardingComplete());
      setStep('ready');
    } catch (err: any) {
      setError(err.response?.data?.error || 'Произошла ошибка');
    } finally {
      setIsLoading(false);
    }
  };

  const handleStart = () => {
    navigate('/', { replace: true });
  };

  // Helper: min date = tomorrow
  const minDate = new Date();
  minDate.setDate(minDate.getDate() + 1);
  const minDateStr = minDate.toISOString().split('T')[0];

  // Helper: max date = 2 years from now
  const maxDate = new Date();
  maxDate.setFullYear(maxDate.getFullYear() + 2);
  const maxDateStr = maxDate.toISOString().split('T')[0];

  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: '2rem',
    }}>
      <div style={{
        background: 'var(--card-bg, #fff)',
        borderRadius: '1.5rem',
        padding: '3rem',
        maxWidth: step === 'exam' ? '800px' : '560px',
        width: '100%',
        boxShadow: '0 25px 50px rgba(0,0,0,0.25)',
        transition: 'max-width 0.3s ease',
      }}>

        {/* Progress indicator */}
        {step !== 'ready' && (
          <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginBottom: '2rem' }}>
            {(['welcome', 'exam', 'target'] as Step[]).map((s, i) => (
              <div key={s} style={{
                width: '60px',
                height: '4px',
                borderRadius: '2px',
                background: (['welcome', 'exam', 'target'].indexOf(step) >= i)
                  ? '#6366f1'
                  : 'var(--border-color, #e5e7eb)',
                transition: 'background 0.3s',
              }} />
            ))}
          </div>
        )}

        {/* ─── Step 1: Welcome ─── */}
        {step === 'welcome' && (
          <div style={{ textAlign: 'center' }}>
            <div style={{ fontSize: '4rem', marginBottom: '1rem' }}>🚀</div>
            <h1 style={{ fontSize: '2rem', marginBottom: '0.75rem', color: 'var(--text-primary)' }}>
              Добро пожаловать, {user?.name}!
            </h1>
            <p style={{ color: 'var(--text-secondary)', fontSize: '1.1rem', lineHeight: 1.6, marginBottom: '2rem' }}>
              UniStart — ваш адаптивный помощник для подготовки к экзаменам.
              Давайте настроим платформу под ваши цели за несколько шагов.
            </p>
            <div style={{
              background: 'var(--bg-secondary, #f9fafb)',
              borderRadius: '1rem',
              padding: '1.5rem',
              marginBottom: '2rem',
              textAlign: 'left',
            }}>
              <h3 style={{ marginBottom: '1rem', color: 'var(--text-primary)' }}>Что вас ждёт:</h3>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                {[
                  { icon: '🎯', text: 'Адаптивные тесты, подстраивающиеся под ваш уровень' },
                  { icon: '📊', text: 'Детальная аналитика и прогноз баллов' },
                  { icon: '📅', text: 'Персональный план подготовки' },
                  { icon: '📝', text: 'Пробные экзамены в реальном формате' },
                ].map((item, i) => (
                  <div key={i} style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                    <span style={{ fontSize: '1.3rem' }}>{item.icon}</span>
                    <span style={{ color: 'var(--text-secondary)' }}>{item.text}</span>
                  </div>
                ))}
              </div>
            </div>
            <button
              onClick={() => setStep('exam')}
              className="btn btn-primary"
              style={{
                width: '100%',
                padding: '1rem',
                fontSize: '1.1rem',
                borderRadius: '0.75rem',
              }}
            >
              Начать настройку →
            </button>
          </div>
        )}

        {/* ─── Step 2: Exam Selection ─── */}
        {step === 'exam' && (
          <div>
            <button
              onClick={() => setStep('welcome')}
              style={{
                background: 'none',
                border: 'none',
                color: 'var(--text-secondary)',
                cursor: 'pointer',
                fontSize: '0.9rem',
                marginBottom: '1rem',
                padding: 0,
              }}
            >
              ← Назад
            </button>
            <h2 style={{ fontSize: '1.5rem', marginBottom: '0.5rem', textAlign: 'center', color: 'var(--text-primary)' }}>
              Какой экзамен вы готовите?
            </h2>
            <p style={{ color: 'var(--text-secondary)', textAlign: 'center', marginBottom: '2rem' }}>
              Выберите экзамен, к которому вы хотите подготовиться
            </p>

            {error && (
              <div style={{
                background: 'var(--error-bg)',
                color: 'var(--error-text)',
                padding: '0.75rem 1rem',
                borderRadius: '0.5rem',
                marginBottom: '1rem',
                fontSize: '0.9rem',
              }}>
                {error}
              </div>
            )}

            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              {examTypes.map((exam) => (
                <button
                  key={exam.code}
                  onClick={() => handleExamSelect(exam)}
                  style={{
                    display: 'flex',
                    alignItems: 'flex-start',
                    gap: '1.25rem',
                    padding: '1.5rem',
                    border: '2px solid var(--border-color, #e5e7eb)',
                    borderRadius: '1rem',
                    background: 'var(--card-bg, #fff)',
                    cursor: 'pointer',
                    textAlign: 'left',
                    transition: 'all 0.2s',
                    width: '100%',
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.borderColor = EXAM_COLORS[exam.code] || '#6366f1';
                    e.currentTarget.style.transform = 'translateY(-2px)';
                    e.currentTarget.style.boxShadow = `0 8px 24px ${EXAM_COLORS[exam.code]}22`;
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.borderColor = 'var(--border-color, #e5e7eb)';
                    e.currentTarget.style.transform = 'translateY(0)';
                    e.currentTarget.style.boxShadow = 'none';
                  }}
                >
                  <div style={{
                    width: '56px',
                    height: '56px',
                    borderRadius: '1rem',
                    background: `${EXAM_COLORS[exam.code]}15`,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontSize: '1.8rem',
                    flexShrink: 0,
                  }}>
                    {EXAM_ICONS[exam.code]}
                  </div>
                  <div style={{ flex: 1 }}>
                    <div style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: '0.75rem',
                      marginBottom: '0.4rem',
                    }}>
                      <h3 style={{ margin: 0, fontSize: '1.2rem', color: 'var(--text-primary)' }}>
                        {exam.name}
                      </h3>
                      <span style={{
                        fontSize: '0.75rem',
                        padding: '0.15rem 0.5rem',
                        borderRadius: '0.25rem',
                        background: `${EXAM_COLORS[exam.code]}15`,
                        color: EXAM_COLORS[exam.code],
                        fontWeight: 600,
                      }}>
                        {exam.minScore}–{exam.maxScore}
                      </span>
                    </div>
                    <p style={{ margin: '0 0 0.75rem', color: 'var(--text-secondary)', fontSize: '0.9rem', lineHeight: 1.5 }}>
                      {exam.description}
                    </p>
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem' }}>
                      {exam.sections.map((s) => (
                        <span key={s.name} style={{
                          fontSize: '0.75rem',
                          padding: '0.2rem 0.6rem',
                          borderRadius: '1rem',
                          background: 'var(--bg-secondary, #f3f4f6)',
                          color: 'var(--text-secondary)',
                        }}>
                          {s.name} ({s.minScore}–{s.maxScore})
                        </span>
                      ))}
                    </div>
                  </div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '1.5rem', alignSelf: 'center' }}>
                    →
                  </div>
                </button>
              ))}
            </div>
          </div>
        )}

        {/* ─── Step 3: Target Setup ─── */}
        {step === 'target' && selectedExam && (
          <div>
            <button
              onClick={() => setStep('exam')}
              style={{
                background: 'none',
                border: 'none',
                color: 'var(--text-secondary)',
                cursor: 'pointer',
                fontSize: '0.9rem',
                marginBottom: '1rem',
                padding: 0,
              }}
            >
              ← Назад
            </button>
            <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
              <div style={{
                display: 'inline-flex',
                alignItems: 'center',
                gap: '0.5rem',
                padding: '0.4rem 1rem',
                borderRadius: '2rem',
                background: `${EXAM_COLORS[selectedExam.code]}15`,
                color: EXAM_COLORS[selectedExam.code],
                fontWeight: 600,
                fontSize: '0.9rem',
                marginBottom: '1rem',
              }}>
                {EXAM_ICONS[selectedExam.code]} {selectedExam.code}
              </div>
              <h2 style={{ fontSize: '1.5rem', marginBottom: '0.5rem', color: 'var(--text-primary)' }}>
                Настройте вашу цель
              </h2>
              <p style={{ color: 'var(--text-secondary)' }}>
                Укажите дату экзамена и желаемый балл
              </p>
            </div>

            {error && (
              <div style={{
                background: 'var(--error-bg)',
                color: 'var(--error-text)',
                padding: '0.75rem 1rem',
                borderRadius: '0.5rem',
                marginBottom: '1rem',
                fontSize: '0.9rem',
              }}>
                {error}
              </div>
            )}

            <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
              {/* Target Date */}
              <div>
                <label style={{
                  display: 'block',
                  marginBottom: '0.5rem',
                  fontWeight: 600,
                  color: 'var(--text-primary)',
                  fontSize: '0.95rem',
                }}>
                  📅 Дата экзамена
                </label>
                <input
                  type="date"
                  value={targetDate}
                  min={minDateStr}
                  max={maxDateStr}
                  onChange={(e) => setTargetDate(e.target.value)}
                  className="form-input"
                  style={{
                    width: '100%',
                    padding: '0.75rem 1rem',
                    borderRadius: '0.75rem',
                    border: '2px solid var(--border-color, #e5e7eb)',
                    fontSize: '1rem',
                    boxSizing: 'border-box',
                  }}
                />
                {targetDate && (
                  <p style={{ margin: '0.5rem 0 0', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                    {(() => {
                      const days = Math.ceil((new Date(targetDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24));
                      if (days <= 0) return 'Дата должна быть в будущем';
                      const weeks = Math.floor(days / 7);
                      return `До экзамена: ${days} дней (${weeks} недель)`;
                    })()}
                  </p>
                )}
              </div>

              {/* Target Score */}
              <div>
                <label style={{
                  display: 'block',
                  marginBottom: '0.5rem',
                  fontWeight: 600,
                  color: 'var(--text-primary)',
                  fontSize: '0.95rem',
                }}>
                  🎯 Целевой балл
                </label>
                <div style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '1rem',
                }}>
                  <input
                    type="range"
                    min={selectedExam.minScore}
                    max={selectedExam.maxScore}
                    step={selectedExam.code === 'SAT' ? 10 : 1}
                    value={targetScore}
                    onChange={(e) => setTargetScore(Number(e.target.value))}
                    style={{ flex: 1, accentColor: EXAM_COLORS[selectedExam.code] }}
                  />
                  <div style={{
                    minWidth: '70px',
                    textAlign: 'center',
                    fontWeight: 700,
                    fontSize: '1.5rem',
                    color: EXAM_COLORS[selectedExam.code],
                  }}>
                    {targetScore}
                  </div>
                </div>
                <div style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  marginTop: '0.25rem',
                  fontSize: '0.8rem',
                  color: 'var(--text-secondary)',
                }}>
                  <span>{selectedExam.minScore}</span>
                  <span>{selectedExam.maxScore}</span>
                </div>
                <div style={{
                  marginTop: '1rem',
                  display: 'flex',
                  gap: '0.5rem',
                  flexWrap: 'wrap',
                }}>
                  {(() => {
                    const range = selectedExam.maxScore - selectedExam.minScore;
                    const presets = [
                      { label: 'Средний', value: Math.round(selectedExam.minScore + range * 0.4) },
                      { label: 'Хороший', value: Math.round(selectedExam.minScore + range * 0.6) },
                      { label: 'Отличный', value: Math.round(selectedExam.minScore + range * 0.8) },
                      { label: 'Максимум', value: selectedExam.maxScore },
                    ];
                    // Round SAT presets to nearest 10
                    if (selectedExam.code === 'SAT') {
                      presets.forEach(p => p.value = Math.round(p.value / 10) * 10);
                    }
                    return presets.map((p) => (
                      <button
                        key={p.label}
                        onClick={() => setTargetScore(p.value)}
                        style={{
                          padding: '0.4rem 0.8rem',
                          borderRadius: '0.5rem',
                          border: targetScore === p.value
                            ? `2px solid ${EXAM_COLORS[selectedExam.code]}`
                            : '2px solid var(--border-color, #e5e7eb)',
                          background: targetScore === p.value
                            ? `${EXAM_COLORS[selectedExam.code]}10`
                            : 'transparent',
                          color: targetScore === p.value
                            ? EXAM_COLORS[selectedExam.code]
                            : 'var(--text-secondary)',
                          cursor: 'pointer',
                          fontSize: '0.85rem',
                          fontWeight: targetScore === p.value ? 600 : 400,
                          transition: 'all 0.2s',
                        }}
                      >
                        {p.label}: {p.value}
                      </button>
                    ));
                  })()}
                </div>
              </div>

              {/* Section breakdown */}
              <div style={{
                background: 'var(--bg-secondary, #f9fafb)',
                borderRadius: '0.75rem',
                padding: '1rem 1.25rem',
              }}>
                <h4 style={{ margin: '0 0 0.75rem', color: 'var(--text-primary)', fontSize: '0.9rem' }}>
                  Секции экзамена
                </h4>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                  {selectedExam.sections.map((s) => (
                    <div key={s.name} style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      fontSize: '0.85rem',
                    }}>
                      <span style={{ color: 'var(--text-secondary)' }}>{s.name}</span>
                      <span style={{ fontWeight: 600, color: 'var(--text-primary)' }}>
                        {s.minScore}–{s.maxScore}
                      </span>
                    </div>
                  ))}
                </div>
              </div>

              <button
                onClick={handleComplete}
                disabled={isLoading || !targetDate || !targetScore}
                className="btn btn-primary"
                style={{
                  width: '100%',
                  padding: '1rem',
                  fontSize: '1.1rem',
                  borderRadius: '0.75rem',
                  opacity: isLoading ? 0.7 : 1,
                }}
              >
                {isLoading ? 'Настраиваем...' : 'Создать план подготовки →'}
              </button>
            </div>
          </div>
        )}

        {/* ─── Step 4: Ready! ─── */}
        {step === 'ready' && selectedExam && (
          <div style={{ textAlign: 'center' }}>
            <div style={{ fontSize: '4rem', marginBottom: '1rem' }}>🎉</div>
            <h2 style={{ fontSize: '1.75rem', marginBottom: '0.75rem', color: 'var(--text-primary)' }}>
              Всё готово!
            </h2>
            <p style={{ color: 'var(--text-secondary)', fontSize: '1.05rem', lineHeight: 1.6, marginBottom: '2rem' }}>
              Ваш персональный план подготовки к {selectedExam.code} создан.
              Платформа адаптируется под ваш уровень по мере обучения.
            </p>

            <div style={{
              background: 'var(--bg-secondary, #f9fafb)',
              borderRadius: '1rem',
              padding: '1.5rem',
              marginBottom: '2rem',
              textAlign: 'left',
            }}>
              <div style={{
                display: 'grid',
                gridTemplateColumns: '1fr 1fr',
                gap: '1rem',
              }}>
                <div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '0.25rem' }}>
                    Экзамен
                  </div>
                  <div style={{ fontWeight: 600, color: 'var(--text-primary)', fontSize: '1.1rem' }}>
                    {EXAM_ICONS[selectedExam.code]} {selectedExam.code}
                  </div>
                </div>
                <div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '0.25rem' }}>
                    Целевой балл
                  </div>
                  <div style={{ fontWeight: 600, color: EXAM_COLORS[selectedExam.code], fontSize: '1.1rem' }}>
                    {targetScore} / {selectedExam.maxScore}
                  </div>
                </div>
                <div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '0.25rem' }}>
                    Дата экзамена
                  </div>
                  <div style={{ fontWeight: 600, color: 'var(--text-primary)', fontSize: '1.1rem' }}>
                    {new Date(targetDate).toLocaleDateString('ru-RU', {
                      day: 'numeric',
                      month: 'long',
                      year: 'numeric',
                    })}
                  </div>
                </div>
                <div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '0.25rem' }}>
                    Дней до экзамена
                  </div>
                  <div style={{ fontWeight: 600, color: 'var(--text-primary)', fontSize: '1.1rem' }}>
                    {Math.ceil((new Date(targetDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24))}
                  </div>
                </div>
              </div>
            </div>

            <div style={{
              background: 'linear-gradient(135deg, #667eea15, #764ba215)',
              borderRadius: '1rem',
              padding: '1.25rem',
              marginBottom: '2rem',
              textAlign: 'left',
            }}>
              <h4 style={{ margin: '0 0 0.75rem', color: 'var(--text-primary)' }}>
                Рекомендуем начать с:
              </h4>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                {[
                  { icon: '1️⃣', text: 'Пройдите диагностический тест, чтобы определить уровень' },
                  { icon: '2️⃣', text: 'Изучите план подготовки на сегодня' },
                  { icon: '3️⃣', text: 'Просмотрите свою аналитику после первых тестов' },
                ].map((item, i) => (
                  <div key={i} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <span>{item.icon}</span>
                    <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{item.text}</span>
                  </div>
                ))}
              </div>
            </div>

            <button
              onClick={() => navigate(`/diagnostic?exam=${selectedExam.code}`, { replace: true })}
              className="btn btn-primary"
              style={{
                width: '100%',
                padding: '1rem',
                fontSize: '1.1rem',
                borderRadius: '0.75rem',
                marginBottom: '0.75rem',
              }}
            >
              🩺 Пройти диагностический тест
            </button>

            <button
              onClick={handleStart}
              style={{
                width: '100%',
                padding: '0.85rem',
                fontSize: '1rem',
                borderRadius: '0.75rem',
                background: 'none',
                border: '1px solid var(--border-color, #e5e7eb)',
                color: 'var(--text-secondary)',
                cursor: 'pointer',
              }}
            >
              Пропустить и начать подготовку →
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

export default OnboardingPage;
