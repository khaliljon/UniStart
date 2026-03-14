import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { setOnboardingComplete } from '../store/slices/authSlice';
import { setSelectedExams } from '../store/slices/examSlice';
import { onboardingService } from '../services/onboardingService';
import { useTranslation } from '../i18n';
import LanguageSwitcher from '../components/LanguageSwitcher';
import axios from 'axios';
import type { ExamTypeInfo } from '../types';

type Step = 'welcome' | 'exam' | 'target' | 'ready';

const EXAM_ICONS: Record<string, string> = {
  SAT: '',
  TOEFL: '',
  NUET: '',
  IELTS: '',
  CSCA: '',
};

const EXAM_COLORS: Record<string, string> = {
  SAT: '#6366f1',
  TOEFL: '#8b5cf6',
  NUET: '#f59e0b',
  IELTS: '#ef4444',
  CSCA: '#10b981',
};

function OnboardingPage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { t, dateLocale } = useTranslation();

  // ── Restore wizard state from sessionStorage ──────────
  const saved = sessionStorage.getItem('onboarding');
  const restored = saved ? JSON.parse(saved) as { step?: Step; exam?: ExamTypeInfo; date?: string; score?: number; sections?: string[] } : null;
  const validSteps: Step[] = ['welcome', 'exam', 'target'];
  const restoredStep = restored?.step && validSteps.includes(restored.step) ? restored.step : 'welcome';

  const [step, _setStep] = useState<Step>(restoredStep);
  const [examTypes, setExamTypes] = useState<ExamTypeInfo[]>([]);
  const [selectedExam, _setSelectedExam] = useState<ExamTypeInfo | null>(restored?.exam ?? null);
  const [targetDate, _setTargetDate] = useState(restored?.date ?? '');
  const [targetScore, _setTargetScore] = useState(restored?.score ?? 0);
  const [selectedSections, _setSelectedSections] = useState<string[]>(restored?.sections ?? []);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Wrapped setters that also persist to sessionStorage
  const persist = (patch: Partial<{ step: Step; exam: ExamTypeInfo | null; date: string; score: number; sections: string[] }>) => {
    const prev = JSON.parse(sessionStorage.getItem('onboarding') ?? '{}');
    sessionStorage.setItem('onboarding', JSON.stringify({ ...prev, ...patch }));
  };
  const setStep = (s: Step) => { _setStep(s); persist({ step: s }); };
  const setSelectedExam = (e: ExamTypeInfo | null) => { _setSelectedExam(e); persist({ exam: e }); };
  const setTargetDate = (d: string) => { _setTargetDate(d); persist({ date: d }); };
  const setTargetScore = (s: number) => { _setTargetScore(s); persist({ score: s }); };
  const setSelectedSections = (ss: string[]) => { _setSelectedSections(ss); persist({ sections: ss }); };

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
      setError(t.common.error);
    }
  };

  const handleExamSelect = (exam: ExamTypeInfo) => {
    setSelectedExam(exam);
    // Auto-select all sections
    setSelectedSections(exam.sections.map(s => s.name));
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
      sessionStorage.removeItem('onboarding');
      dispatch(setOnboardingComplete());
      dispatch(setSelectedExams([selectedExam.code]));
      setStep('ready');
    } catch (err: unknown) {
      setError(axios.isAxiosError(err) ? err.response?.data?.error || t.common.error : t.common.error);
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

        {/* Language switcher */}
        <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: '1rem' }}>
          <LanguageSwitcher />
        </div>

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
            <h1 style={{ fontSize: '2rem', marginBottom: '0.75rem', color: 'var(--text-primary)' }}>
              {t.onboarding.welcome}, {user?.name}!
            </h1>
            <p style={{ color: 'var(--text-secondary)', fontSize: '1.1rem', lineHeight: 1.6, marginBottom: '2rem' }}>
              {t.onboarding.welcomeDesc}
            </p>
            <div style={{
              background: 'var(--bg-secondary, #f9fafb)',
              borderRadius: '1rem',
              padding: '1.5rem',
              marginBottom: '2rem',
              textAlign: 'left',
            }}>
              <h3 style={{ marginBottom: '1rem', color: 'var(--text-primary)' }}>{t.onboarding.whatAwaits}</h3>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                {[
                  { icon: '', text: t.onboarding.featureAdaptive },
                  { icon: '', text: t.onboarding.featurePrediction },
                  { icon: '', text: t.onboarding.featurePlan },
                  { icon: '', text: t.onboarding.featureMockExam },
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
              {t.onboarding.startSetup}
            </button>
          </div>
        )}

        {/* Skip onboarding link — always visible on welcome */}
        {step === 'welcome' && (
          <div style={{ textAlign: 'center', marginTop: '1rem' }}>
            <button
              onClick={async () => {
                setIsLoading(true);
                try {
                  await onboardingService.complete({ examTypeCode: 'SAT', targetDate: new Date(Date.now() + 90 * 86400000).toISOString().split('T')[0], targetScore: 1200 });
                  sessionStorage.removeItem('onboarding');
                  dispatch(setOnboardingComplete());
                  navigate('/', { replace: true });
                } catch {
                  sessionStorage.removeItem('onboarding');
                  dispatch(setOnboardingComplete());
                  navigate('/', { replace: true });
                } finally {
                  setIsLoading(false);
                }
              }}
              disabled={isLoading}
              style={{
                background: 'none',
                border: 'none',
                color: 'var(--text-secondary)',
                cursor: 'pointer',
                fontSize: '0.9rem',
                textDecoration: 'underline',
              }}
            >
              {t.onboarding.skipAndStart}
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
              {t.onboarding.back}
            </button>
            <h2 style={{ fontSize: '1.5rem', marginBottom: '0.5rem', textAlign: 'center', color: 'var(--text-primary)' }}>
              {t.onboarding.examQuestion}
            </h2>
            <p style={{ color: 'var(--text-secondary)', textAlign: 'center', marginBottom: '2rem' }}>
              {t.onboarding.examDesc}
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
              {t.onboarding.back}
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
                {t.onboarding.targetTitle}
              </h2>
              <p style={{ color: 'var(--text-secondary)' }}>
                {t.onboarding.targetDesc}
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
              {/* Section selection (for exams with multiple sections like CSCA) */}
              {selectedExam.sections.length > 1 && (
                <div>
                  <label style={{
                    display: 'block',
                    marginBottom: '0.5rem',
                    fontWeight: 600,
                    color: 'var(--text-primary)',
                    fontSize: '0.95rem',
                  }}>
                    {t.onboarding.selectSections}
                  </label>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                    {selectedExam.sections.map((s) => {
                      const checked = selectedSections.includes(s.name);
                      return (
                        <label
                          key={s.name}
                          style={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: '0.75rem',
                            padding: '0.75rem 1rem',
                            borderRadius: '0.75rem',
                            border: checked
                              ? `2px solid ${EXAM_COLORS[selectedExam.code]}`
                              : '2px solid var(--border-color, #e5e7eb)',
                            background: checked
                              ? `${EXAM_COLORS[selectedExam.code]}08`
                              : 'transparent',
                            cursor: 'pointer',
                            transition: 'all 0.2s',
                          }}
                        >
                          <input
                            type="checkbox"
                            checked={checked}
                            onChange={() => {
                              const next = checked
                                ? selectedSections.filter(n => n !== s.name)
                                : [...selectedSections, s.name];
                              if (next.length === 0) return; // at least one must be selected
                              setSelectedSections(next);
                              // Recalculate target score range
                              const activeSections = selectedExam.sections.filter(sec => next.includes(sec.name));
                              const newMax = activeSections.reduce((sum, sec) => sum + sec.maxScore, 0);
                              const newMin = activeSections.reduce((sum, sec) => sum + sec.minScore, 0);
                              if (targetScore > newMax) setTargetScore(newMax);
                              if (targetScore < newMin) setTargetScore(Math.round((newMin + newMax) / 2));
                            }}
                            style={{ accentColor: EXAM_COLORS[selectedExam.code] }}
                          />
                          <div style={{ flex: 1 }}>
                            <span style={{ fontWeight: 600, color: 'var(--text-primary)', fontSize: '0.9rem' }}>
                              {s.name}
                            </span>
                            <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem', marginLeft: '0.5rem' }}>
                              ({s.minScore}–{s.maxScore})
                            </span>
                          </div>
                        </label>
                      );
                    })}
                  </div>
                </div>
              )}

              {/* Target Date */}
              <div>
                <label style={{
                  display: 'block',
                  marginBottom: '0.5rem',
                  fontWeight: 600,
                  color: 'var(--text-primary)',
                  fontSize: '0.95rem',
                }}>
                  {t.onboarding.examDate}
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
                      if (days <= 0) return t.onboarding.dateMustBeFuture;
                      const weeks = Math.floor(days / 7);
                      return `${t.onboarding.daysUntilExam}: ${days} (${weeks})`;
                    })()}
                  </p>
                )}
              </div>

              {/* Target Score */}
              {(() => {
                const activeSections = selectedSections.length > 0 && selectedExam.sections.length > 1
                  ? selectedExam.sections.filter(s => selectedSections.includes(s.name))
                  : selectedExam.sections;
                const effectiveMin = activeSections.reduce((sum, s) => sum + s.minScore, 0);
                const effectiveMax = activeSections.reduce((sum, s) => sum + s.maxScore, 0);
                return (
              <div>
                <label style={{
                  display: 'block',
                  marginBottom: '0.5rem',
                  fontWeight: 600,
                  color: 'var(--text-primary)',
                  fontSize: '0.95rem',
                }}>
                  {t.onboarding.targetScore}
                </label>
                <div style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '1rem',
                }}>
                  <input
                    type="range"
                    min={effectiveMin}
                    max={effectiveMax}
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
                  <span>{effectiveMin}</span>
                  <span>{effectiveMax}</span>
                </div>
                <div style={{
                  marginTop: '1rem',
                  display: 'flex',
                  gap: '0.5rem',
                  flexWrap: 'wrap',
                }}>
                  {(() => {
                    const range = effectiveMax - effectiveMin;
                    const presets = [
                      { label: t.onboarding.presetAverage, value: Math.round(effectiveMin + range * 0.4) },
                      { label: t.onboarding.presetGood, value: Math.round(effectiveMin + range * 0.6) },
                      { label: t.onboarding.presetExcellent, value: Math.round(effectiveMin + range * 0.8) },
                      { label: t.onboarding.presetMax, value: effectiveMax },
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
                );
              })()}

              {/* Section breakdown */}
              <div style={{
                background: 'var(--bg-secondary, #f9fafb)',
                borderRadius: '0.75rem',
                padding: '1rem 1.25rem',
              }}>
                <h4 style={{ margin: '0 0 0.75rem', color: 'var(--text-primary)', fontSize: '0.9rem' }}>
                  {t.onboarding.sections}
                </h4>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                  {selectedExam.sections
                    .filter(s => selectedSections.length === 0 || selectedSections.includes(s.name))
                    .map((s) => (
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
                {isLoading ? t.onboarding.setting : t.onboarding.createPlan}
              </button>
            </div>
          </div>
        )}

        {/* ─── Step 4: Ready! ─── */}
        {step === 'ready' && selectedExam && (
          <div style={{ textAlign: 'center' }}>
            <h2 style={{ fontSize: '1.75rem', marginBottom: '0.75rem', color: 'var(--text-primary)' }}>
              {t.onboarding.readyTitle}
            </h2>
            <p style={{ color: 'var(--text-secondary)', fontSize: '1.05rem', lineHeight: 1.6, marginBottom: '2rem' }}>
              {t.onboarding.readyDesc}
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
                    {t.onboarding.examLabel}
                  </div>
                  <div style={{ fontWeight: 600, color: 'var(--text-primary)', fontSize: '1.1rem' }}>
                    {EXAM_ICONS[selectedExam.code]} {selectedExam.code}
                  </div>
                </div>
                <div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '0.25rem' }}>
                    {t.onboarding.targetScore}
                  </div>
                  <div style={{ fontWeight: 600, color: EXAM_COLORS[selectedExam.code], fontSize: '1.1rem' }}>
                    {targetScore} / {selectedSections.length > 0 && selectedExam.sections.length > 1
                      ? selectedExam.sections.filter(s => selectedSections.includes(s.name)).reduce((sum, s) => sum + s.maxScore, 0)
                      : selectedExam.maxScore}
                  </div>
                </div>
                <div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '0.25rem' }}>
                    {t.onboarding.examDate}
                  </div>
                  <div style={{ fontWeight: 600, color: 'var(--text-primary)', fontSize: '1.1rem' }}>
                    {new Date(targetDate).toLocaleDateString(dateLocale, {
                      day: 'numeric',
                      month: 'long',
                      year: 'numeric',
                    })}
                  </div>
                </div>
                <div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '0.25rem' }}>
                    {t.onboarding.daysUntilExam}
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
                {t.onboarding.recommendStart}
              </h4>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                {[
                  { icon: '1.', text: t.onboarding.rec1 },
                  { icon: '2.', text: t.onboarding.rec2 },
                  { icon: '3.', text: t.onboarding.rec3 },
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
              {t.onboarding.diagnosticTest}
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
              {t.onboarding.skipAndStart}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

export default OnboardingPage;
