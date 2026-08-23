import { useEffect, useState, useRef, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { mockExamService } from '../services/mockExamService';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTranslation } from '../hooks/useTranslation';
import { pickLocalized } from '../utils/localize';
import { moks } from '../utils/plural';
import { cscaStrings } from '../i18n/csca';
import { shortDateLocalized } from '../utils/dates';
import type {
  MockExamListItem,
  MockExamDetail,
  MockExamAttempt,
  MockExamSectionState,
  MockExamQuestion,
  MockExamResult,
  MockExamHistoryItem,
} from '../types';

type Phase =
  | 'list'
  | 'detail'
  | 'instructions'
  | 'section'
  | 'section-break'
  | 'results';

function MockExamPage() {
  const [phase, setPhase] = useState<Phase>('list');
  const { user } = useAppSelector((state) => state.auth);
  const isPro = user?.role === 'Admin';
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  const [mockExams, setMockExams] = useState<MockExamListItem[]>([]);
  const [history, setHistory] = useState<MockExamHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);

  const [examDetail, setExamDetail] = useState<MockExamDetail | null>(null);

  const [attempt, setAttempt] = useState<MockExamAttempt | null>(null);
  const [sectionState, setSectionState] = useState<MockExamSectionState | null>(null);
  const [currentQIndex, setCurrentQIndex] = useState(0);

  const [activeAttempt, setActiveAttempt] = useState<MockExamAttempt | null>(null);

  const [timeLeft, setTimeLeft] = useState(0);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const [results, setResults] = useState<MockExamResult | null>(null);
  const [showReview, setShowReview] = useState(false);
  const [reviewFilter, setReviewFilter] = useState<'all' | 'correct' | 'incorrect' | 'unanswered'>('all');
  const [showFinishConfirm, setShowFinishConfirm] = useState(false);

  const loadExams = useCallback(async () => {
    setLoading(true);
    try {
      const [exams, hist, active] = await Promise.all([
        mockExamService.getAvailableMockExams(),
        mockExamService.getHistory(),
        mockExamService.getActiveAttempt(),
      ]);
      setMockExams(exams);
      setHistory(hist);
      setActiveAttempt(active);
    } catch (e) { console.error(e); }
    setLoading(false);
  }, []);

  useEffect(() => { loadExams(); }, [loadExams]);

  const { attemptId } = useParams();
  useEffect(() => {
    if (!attemptId) return;
    const id = Number(attemptId);
    if (!Number.isFinite(id) || id <= 0) return;
    mockExamService.getResults(id)
      .then((r) => { if (r) { setResults(r); setShowReview(false); setPhase('results'); } })
      .catch(() => {});
  }, [attemptId]);

  useEffect(() => {
    if (phase === 'section' && timeLeft > 0) {
      timerRef.current = setInterval(() => {
        setTimeLeft(prev => {
          if (prev <= 1) {
            handleCompleteSection();
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
    }
    return () => { if (timerRef.current) clearInterval(timerRef.current); };
  }, [phase, timeLeft > 0]);

  const formatTime = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, '0')}`;
  };

  const handleResume = async (att: MockExamAttempt) => {
    setLoading(true);
    try {
      const freshAttempt = await mockExamService.getActiveAttempt();
      const effectiveAtt = (freshAttempt && freshAttempt.attemptId === att.attemptId) ? freshAttempt : att;
      setAttempt(effectiveAtt);
      const section = await mockExamService.getCurrentSection(effectiveAtt.attemptId);
      if (!section) {
        const res = await mockExamService.getResults(effectiveAtt.attemptId);
        if (res) { setResults(res); setPhase('results'); }
        else { resetToList(); }
        setLoading(false);
        return;
      }
      setSectionState(section);
      setCurrentQIndex(0);
      const elapsed = Math.floor((Date.now() - new Date(effectiveAtt.startedAt).getTime()) / 1000);
      const totalSec = effectiveAtt.totalTimeMinutes * 60;
      const remaining = Math.max(0, totalSec - elapsed);
      setTimeLeft(remaining > 0 ? remaining : section.timeLimitMinutes * 60);
      setActiveAttempt(null);
      setPhase('section');
    } catch (e) { console.error(e); }
    setLoading(false);
  };

  const handleSelectExam = async (examId: number) => {
    setLoading(true);
    try {
      const detail = await mockExamService.getMockExamDetail(examId);
      setExamDetail(detail);
      setPhase('detail');
    } catch (e) { console.error(e); }
    setLoading(false);
  };

  const handleStartExam = async () => {
    if (!examDetail) return;
    setLoading(true);
    try {
      const att = await mockExamService.startMockExam(examDetail.id);
      setAttempt(att);
      const section = await mockExamService.getCurrentSection(att.attemptId);
      setSectionState(section);
      setCurrentQIndex(0);
      setTimeLeft(att.totalTimeMinutes * 60);
      setPhase('instructions');
    } catch (e) {
      const status = (e as { response?: { status?: number } })?.response?.status;
      if (status === 403) {
        navigate('/');
        return;
      }
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  const handleBeginSection = () => {
    setPhase('section');
  };

  const handleSwitchSection = async (sectionIndex: number) => {
    if (!attempt) return;
    if (sectionState && sectionState.sectionIndex === sectionIndex) return;
    setLoading(true);
    try {
      const section = await mockExamService.getSection(attempt.attemptId, sectionIndex);
      setSectionState(section);
      setCurrentQIndex(0);
    } catch (e) { console.error(e); }
    setLoading(false);
  };

  const handleSelectOption = async (question: MockExamQuestion, optionId: number) => {
    if (!attempt || !sectionState) return;
    const current = question.selectedOptionIds && question.selectedOptionIds.length > 0
      ? question.selectedOptionIds
      : (question.selectedOptionId != null ? [question.selectedOptionId] : []);
    const nextIds = question.isMultipleChoice
      ? (current.includes(optionId) ? current.filter(id => id !== optionId) : [...current, optionId])
      : [optionId];
    const primary = nextIds.length > 0 ? nextIds[0] : 0;
    try {
      await mockExamService.submitAnswer(attempt.attemptId, question.questionId, primary, undefined, nextIds);
      setSectionState(prev => {
        if (!prev) return prev;
        const updatedQuestions = prev.questions.map(q =>
          q.questionId === question.questionId
            ? { ...q, selectedOptionIds: nextIds, selectedOptionId: nextIds.length > 0 ? nextIds[0] : null }
            : q
        );
        const answeredCount = updatedQuestions.filter(q => (q.selectedOptionIds?.length ?? (q.selectedOptionId !== null ? 1 : 0)) > 0).length;
        return { ...prev, questions: updatedQuestions, answeredCount };
      });
    } catch (e) { console.error(e); }
  };

  const handleCompleteSection = async () => {
    if (!attempt) return;
    if (timerRef.current) clearInterval(timerRef.current);
    setLoading(true);
    try {
      const updated = await mockExamService.completeExam(attempt.attemptId);
      setAttempt(updated);
      const res = await mockExamService.getResults(updated.attemptId);
      setResults(res);
      setPhase('results');
    } catch (e) { console.error(e); }
    setLoading(false);
  };


  const resetToList = async () => {
    setPhase('list');
    setAttempt(null);
    setSectionState(null);
    setExamDetail(null);
    setResults(null);
    setShowReview(false);
    setCurrentQIndex(0);
    setActiveAttempt(null);
    await loadExams();
  };

  const currentQuestion: MockExamQuestion | null =
    sectionState?.questions?.[currentQIndex] ?? null;

  const timerColor = timeLeft < 60 ? '#e74c3c' : timeLeft < 180 ? '#f39c12' : 'var(--text-primary)';

  if (phase === 'list') {
    if (loading) return <div className="loading"><div className="spinner" /></div>;

    return (
      <div style={{ maxWidth: 900, margin: '0 auto' }}>
        <h1 style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: '0.5rem' }}>
          {s.mocksTitle}
        </h1>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
          {s.mocksFullLead}
        </p>

        {!isPro && mockExams.some(m => m.freeAvailable) && (
          <div className="card" style={{ marginBottom: '1.5rem', border: '2px dashed var(--primary-color)', background: 'var(--bg-secondary)' }}>
            <div style={{ fontWeight: 700, marginBottom: '0.2rem' }}>{s.firstMockFree}</div>
            <div style={{ color: 'var(--text-secondary)', fontSize: '0.88rem' }}>
              {s.freeMockBannerDesc}
            </div>
          </div>
        )}

        {activeAttempt && (
          <div className="card" style={{ marginBottom: '1.5rem', border: '2px solid var(--primary-color)', background: 'var(--bg-secondary)' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '0.75rem' }}>
              <div>
                <div style={{ fontWeight: 700, fontSize: '1rem', marginBottom: '0.25rem' }}>
                  {s.examInProgress}
                </div>
                <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                  {activeAttempt.examTitle} — {s.sectionLabel} {activeAttempt.currentSectionIndex + 1}/{activeAttempt.totalSections}
                </div>
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" style={{ fontSize: '0.85rem' }}
                        onClick={() => handleResume(activeAttempt)}>
                  ▶ {s.resumeBtn}
                </button>
              </div>
            </div>
          </div>
        )}

        {(() => {
          const visible = mockExams.filter(m => isPro || m.runsRemaining > 0 || m.attemptCount > 0 || m.freeAvailable);
          if (visible.length === 0) {
            return (
              <div className="card" style={{ textAlign: 'center', padding: '2rem', marginBottom: '2rem' }}>
                <p style={{ color: 'var(--text-secondary)', marginBottom: '1rem' }}>
                  {s.noMocksAvailable}
                </p>
                <button className="btn btn-primary" onClick={() => navigate('/')}>{s.buyOnHome}</button>
              </div>
            );
          }
          return (
        <div style={{ display: 'grid', gap: '1rem', marginBottom: '2rem' }}>
          {visible.map(exam => {
            const isFreeStart = !isPro && exam.runsRemaining === 0 && exam.freeAvailable;
            return (
            <div key={exam.id} className="card">
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: '1rem', flexWrap: 'wrap' }}>
                <div style={{ flex: 1, minWidth: 200 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem', flexWrap: 'wrap' }}>
                    <span style={{ background: examBadgeColor(exam.examTypeCode), color: '#fff', padding: '2px 10px', borderRadius: 12, fontSize: '0.75rem', fontWeight: 600 }}>
                      {exam.examTypeCode}
                    </span>
                    <h3 style={{ margin: 0, fontSize: '1.1rem' }}>{pickLocalized(exam.title, exam.titleKz, exam.titleEn, locale)}</h3>
                    {exam.runsRemaining > 0 && (
                      <span style={{ background: 'rgba(16,185,129,0.12)', color: '#10b981', padding: '2px 8px', borderRadius: 10, fontSize: '0.7rem', fontWeight: 700 }}>
                        {s.mockRemaining}: {moks(exam.runsRemaining, locale)}
                      </span>
                    )}
                    {isFreeStart && (
                      <span style={{ background: 'rgba(200,16,46,0.1)', color: 'var(--csca-red, #C8102E)', padding: '2px 8px', borderRadius: 10, fontSize: '0.7rem', fontWeight: 700 }}>{s.mockFreeBadge}</span>
                    )}
                  </div>
                  <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', margin: '0.25rem 0' }}>{pickLocalized(exam.description, exam.descriptionKz, exam.descriptionEn, locale)}</p>
                  <div style={{ display: 'flex', gap: '1.25rem', flexWrap: 'wrap', marginTop: '0.5rem' }}>
                    <Stat label={s.mockQuestions} value={exam.totalQuestions} />
                    <Stat label={s.mockTime} value={`${exam.totalTimeMinutes}м`} />
                    {exam.bestScore !== null && <Stat label={s.mockBest} value={`${exam.bestScore}%`} />}
                    {exam.attemptCount > 0 && <Stat label={s.mockSessions} value={exam.attemptCount} />}
                  </div>
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', alignItems: 'stretch', minWidth: 180 }}>
                  {(isPro || exam.runsRemaining > 0) ? (
                    <button className="btn btn-primary" onClick={() => handleSelectExam(exam.id)}>
                      {exam.runsRemaining > 0 ? `${s.mockSolvePaid} (−1 ${moks(1, locale).replace(/^\d+\s*/, '')})` : s.mockSolvePaid}
                    </button>
                  ) : exam.freeAvailable ? (
                    <button className="btn btn-primary" onClick={() => handleSelectExam(exam.id)}>
                      {s.mockSolveFree}
                    </button>
                  ) : (
                    <button className="btn btn-outline" onClick={() => navigate('/')}>
                      {s.mockRunsOut}
                    </button>
                  )}
                </div>
              </div>
            </div>
            );
          })}
        </div>
          );
        })()}

        {history.length > 0 && (
          <>
            <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>{s.recentAttempts}</h2>
            <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.85rem' }}>
                <thead>
                  <tr style={{ background: 'var(--bg-secondary)', textAlign: 'left' }}>
                    <th style={{ padding: '0.5rem 1rem' }}>{s.thExam}</th>
                    <th style={{ padding: '0.5rem' }}>{s.thScore}</th>
                    <th style={{ padding: '0.5rem' }}>{s.thStatus}</th>
                    <th style={{ padding: '0.5rem' }}>{s.thDate}</th>
                    <th style={{ padding: '0.5rem' }}></th>
                  </tr>
                </thead>
                <tbody>
                  {history.slice(0, 10).map(h => (
                    <tr key={h.attemptId} style={{ borderTop: '1px solid var(--border-color)' }}>
                      <td style={{ padding: '0.5rem 1rem' }}>
                        <span style={{ background: examBadgeColor(h.examTypeCode), color: '#fff', padding: '1px 8px', borderRadius: 10, fontSize: '0.7rem', marginRight: 6 }}>
                          {h.examTypeCode}
                        </span>
                        {h.examTitle}
                      </td>
                      <td style={{ padding: '0.5rem', fontWeight: 600 }}>
                        {h.totalScore !== null ? `${h.totalScore}%` : '—'}
                      </td>
                      <td style={{ padding: '0.5rem' }}>
                        <span style={{ color: h.status === 'completed' ? '#27ae60' : '#e67e22', fontWeight: 500 }}>
                          {h.status === 'completed' ? `✓ ${s.statusCompleted}` : s.statusInProgress}
                        </span>
                      </td>
                      <td style={{ padding: '0.5rem', color: 'var(--text-secondary)' }}>
                        {shortDateLocalized(h.startedAt, locale)}
                      </td>
                      <td style={{ padding: '0.5rem' }}>
                        {h.status === 'completed' && (
                          <button className="btn btn-outline" style={{ padding: '2px 12px', fontSize: '0.75rem' }}
                                  onClick={async (e) => { e.stopPropagation(); const r = await mockExamService.getResults(h.attemptId); setResults(r); setShowReview(false); setPhase('results'); }}>
                            {s.viewBtn}
                          </button>
                        )}
                        {h.status === 'in_progress' && (
                          <button className="btn btn-primary" style={{ padding: '2px 12px', fontSize: '0.75rem' }}
                                  onClick={(e) => { e.stopPropagation(); handleResume({ attemptId: h.attemptId, mockExamId: h.mockExamId, examTitle: h.examTitle, status: h.status, currentSectionIndex: 0, totalSections: 0, startedAt: h.startedAt, totalTimeMinutes: 0 }); }}>
                            {s.resumeBtn}
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </>
        )}
      </div>
    );
  }

  if (phase === 'detail' && examDetail) {
    const totalTime = examDetail.sections.reduce((sum, s) => sum + s.timeLimitMinutes, 0);
    const totalQuestions = examDetail.sections.reduce((sum, s) => sum + s.questionCount, 0);

    return (
      <div style={{ maxWidth: 700, margin: '0 auto' }}>
        <button className="btn btn-outline" style={{ marginBottom: '1rem' }} onClick={() => setPhase('list')}>
          ← Back
        </button>

        <div className="card" style={{ textAlign: 'center', marginBottom: '1.5rem' }}>
          <span style={{ background: examBadgeColor(examDetail.examTypeCode), color: '#fff', padding: '3px 14px', borderRadius: 14, fontSize: '0.8rem', fontWeight: 600 }}>
            {examDetail.examTypeCode}
          </span>
          <h1 style={{ fontSize: '1.5rem', margin: '0.75rem 0 0.25rem' }}>{pickLocalized(examDetail.title, examDetail.titleKz, examDetail.titleEn, locale)}</h1>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{pickLocalized(examDetail.description, examDetail.descriptionKz, examDetail.descriptionEn, locale)}</p>
          <div style={{ display: 'flex', justifyContent: 'center', gap: '2rem', marginTop: '1rem' }}>
            <Stat label="Total Time" value={`${totalTime} min`} />
            <Stat label="Sections" value={examDetail.sections.length} />
            <Stat label="Questions" value={totalQuestions} />
          </div>
        </div>

        <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>Sections</h2>
        {examDetail.sections.map((s, i) => (
          <div key={s.id} className="card" style={{ marginBottom: '0.75rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>{`Section ${i + 1}`}</span>
                <h3 style={{ margin: '0.25rem 0', fontSize: '1rem' }}>{s.name}</h3>
              </div>
              <div style={{ display: 'flex', gap: '1.5rem' }}>
                <Stat label="Questions" value={s.questionCount} />
                <Stat label="Time" value={`${s.timeLimitMinutes}m`} />
              </div>
            </div>
            {s.instructions && (
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.8rem', marginTop: '0.5rem', lineHeight: 1.5 }}>
                {s.instructions}
              </p>
            )}
          </div>
        ))}

        <div style={{ textAlign: 'center', marginTop: '1.5rem' }}>
          <button
            className="btn btn-primary"
            style={{ padding: '0.75rem 2rem', fontSize: '1rem', width: '100%', maxWidth: '320px' }}
            onClick={handleStartExam}
            disabled={loading}
          >
            {loading ? 'Starting...' : 'Start Exam'}
          </button>
        </div>
      </div>
    );
  }

  if (phase === 'instructions' && sectionState && attempt) {
    const sectionNames = attempt.sectionNames ?? [];
    return (
      <div style={{ maxWidth: 600, margin: '0 auto', textAlign: 'center' }}>
        <div className="card" style={{ padding: '2rem' }}>
          <h1 style={{ fontSize: '1.5rem', margin: '0.5rem 0' }}>{attempt.examTitle}</h1>

          <div style={{ display: 'flex', justifyContent: 'center', gap: '2rem', margin: '1.5rem 0' }}>
            <Stat label="Sections" value={attempt.totalSections} />
            <Stat label="Total Time" value={`${attempt.totalTimeMinutes} min`} />
          </div>

          {sectionNames.length > 0 && (
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem', justifyContent: 'center', margin: '1rem 0' }}>
              {sectionNames.map((name, i) => (
                <span key={i} style={{ background: 'var(--bg-secondary)', padding: '4px 12px', borderRadius: 8, fontSize: '0.8rem' }}>
                  {name}
                </span>
              ))}
            </div>
          )}

          {sectionState.instructions && (
            <div style={{ background: 'var(--bg-secondary)', borderRadius: 8, padding: '1rem', margin: '1rem 0', textAlign: 'left', lineHeight: 1.6, fontSize: '0.9rem' }}>
              {sectionState.instructions}
            </div>
          )}

          <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', margin: '1rem 0' }}>
            You can freely switch between sections during the exam.
          </p>

          <div style={{ marginTop: '1.5rem', display: 'flex', gap: '1rem', justifyContent: 'center' }}>
            <button className="btn btn-primary" style={{ padding: '0.75rem 2rem' }} onClick={handleBeginSection}>
              ▶ Begin Exam
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (phase === 'section' && sectionState && currentQuestion && attempt) {
    const questions = sectionState.questions;
    const hasPassage = !!currentQuestion.passageContent;
    const sectionNames = attempt.sectionNames ?? [];

    return (
      <div style={{ maxWidth: 1100, margin: '0 auto' }}>
        {sectionNames.length > 1 && (
          <div style={{ display: 'flex', gap: '0.25rem', marginBottom: '0.75rem', flexWrap: 'wrap' }}>
            {sectionNames.map((name, i) => (
              <button
                key={i}
                onClick={() => handleSwitchSection(i)}
                style={{
                  padding: '6px 14px', borderRadius: 8, border: '2px solid',
                  borderColor: i === sectionState.sectionIndex ? 'var(--primary-color)' : 'var(--border-color)',
                  background: i === sectionState.sectionIndex ? 'var(--primary-color)' : 'transparent',
                  color: i === sectionState.sectionIndex ? '#fff' : 'var(--text-secondary)',
                  cursor: 'pointer', fontSize: '0.8rem', fontWeight: i === sectionState.sectionIndex ? 600 : 400,
                  transition: 'all 0.15s',
                }}
              >
                {name}
              </button>
            ))}
          </div>
        )}

        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem', flexWrap: 'wrap', gap: '0.5rem' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <span style={{ background: 'var(--bg-secondary)', padding: '4px 12px', borderRadius: 8, fontWeight: 600, fontSize: '0.85rem' }}>
              {sectionState.sectionName}
            </span>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
              {sectionState.answeredCount}/{sectionState.totalQuestions} answered
            </span>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem' }}>
            <button
              className="btn btn-primary"
              style={{ background: 'var(--success-color)', fontSize: '0.8rem', padding: '6px 16px' }}
              onClick={() => setShowFinishConfirm(true)}
            >
              Finish Exam
            </button>
            <div style={{ background: 'var(--bg-secondary)', padding: '6px 16px', borderRadius: 8, fontWeight: 700, fontSize: '1.1rem', fontFamily: 'monospace', color: timerColor, minWidth: 80, textAlign: 'center' }}>
              {formatTime(timeLeft)}
            </div>
          </div>
        </div>

        {showFinishConfirm && (
          <div
            onClick={() => setShowFinishConfirm(false)}
            style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 2000, padding: '1rem' }}
          >
            <div onClick={e => e.stopPropagation()} style={{ background: 'var(--card-background)', borderRadius: 12, padding: '1.5rem', maxWidth: 360, width: '100%', boxShadow: '0 10px 40px rgba(0,0,0,0.3)' }}>
              <h3 style={{ margin: '0 0 0.5rem' }}>Finish the exam?</h3>
              <p style={{ margin: '0 0 1.25rem', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>Unanswered questions will count as skipped.</p>
              <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end' }}>
                <button className="btn btn-secondary" onClick={() => setShowFinishConfirm(false)}>Cancel</button>
                <button
                  className="btn btn-primary"
                  style={{ background: 'var(--success-color)' }}
                  onClick={() => { setShowFinishConfirm(false); handleCompleteSection(); }}
                >
                  Finish Exam
                </button>
              </div>
            </div>
          </div>
        )}

        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 4, marginBottom: '1rem' }}>
          {questions.map((q, i) => (
            <button
              key={q.questionId}
              onClick={() => setCurrentQIndex(i)}
              style={{
                width: 32, height: 32, borderRadius: 6, border: 'none', cursor: 'pointer',
                fontSize: '0.75rem', fontWeight: i === currentQIndex ? 700 : 400,
                background: i === currentQIndex
                  ? 'var(--primary-color)'
                  : q.selectedOptionId !== null
                    ? 'var(--success-color, #27ae60)'
                    : 'var(--bg-secondary)',
                color: (i === currentQIndex || q.selectedOptionId !== null) ? '#fff' : 'var(--text-primary)',
                transition: 'all 0.15s',
              }}
            >
              {i + 1}
            </button>
          ))}
        </div>

        <div style={{ display: 'flex', gap: '1rem', alignItems: hasPassage ? 'stretch' : 'flex-start', flexWrap: 'wrap' }}>
          {hasPassage && (
            <div className="card" style={{ flex: '1 1 300px', maxHeight: 600, overflowY: 'auto', fontSize: '0.88rem', lineHeight: 1.7 }}>
              <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem' }}>
                {currentQuestion.passageTitle}
              </h3>
              {currentQuestion.passageContent?.split('\n\n').map((p, i) => (
                <p key={i} style={{ margin: '0 0 0.75rem', textAlign: 'justify' }}>{p}</p>
              ))}
            </div>
          )}

          <div className="card" style={{ flex: '1 1 300px', minWidth: 0 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
              <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>
                Question {currentQIndex + 1} of {questions.length}
              </span>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <span style={{ background: difficultyColor(currentQuestion.difficulty), color: '#fff', padding: '1px 8px', borderRadius: 10, fontSize: '0.7rem' }}>
                  {currentQuestion.difficulty}
                </span>
                <span style={{ background: 'var(--bg-secondary)', padding: '1px 8px', borderRadius: 10, fontSize: '0.7rem' }}>
                  {currentQuestion.topicName}
                </span>
              </div>
            </div>

            <p style={{ fontSize: '1rem', lineHeight: 1.6, marginBottom: '1.25rem', fontWeight: 500 }}>
              {currentQuestion.text}
            </p>

            {currentQuestion.imageUrl && (
              <img
                src={currentQuestion.imageUrl}
                alt="question"
                style={{ maxWidth: '100%', maxHeight: '360px', objectFit: 'contain', borderRadius: 8, marginBottom: '1.25rem', display: 'block' }}
              />
            )}

            {currentQuestion.isMultipleChoice && (
              <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', marginBottom: '0.4rem' }}>
                Можно выбрать несколько вариантов
              </div>
            )}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
              {currentQuestion.options.map((opt, i) => {
                const selIds = currentQuestion.selectedOptionIds && currentQuestion.selectedOptionIds.length > 0
                  ? currentQuestion.selectedOptionIds
                  : (currentQuestion.selectedOptionId != null ? [currentQuestion.selectedOptionId] : []);
                const isSelected = selIds.includes(opt.id);
                const letter = String.fromCharCode(65 + i);
                return (
                  <button
                    key={opt.id}
                    onClick={() => handleSelectOption(currentQuestion, opt.id)}
                    style={{
                      display: 'flex', alignItems: 'center', gap: '0.75rem',
                      padding: '0.75rem 1rem', borderRadius: 8, border: '2px solid',
                      borderColor: isSelected ? 'var(--primary-color)' : 'var(--border-color)',
                      background: isSelected ? 'color-mix(in srgb, var(--primary-color) 10%, transparent)' : 'transparent',
                      cursor: 'pointer', textAlign: 'left', fontSize: '0.9rem',
                      transition: 'all 0.15s', color: 'var(--text-primary)',
                    }}
                  >
                    <span style={{
                      width: 28, height: 28, borderRadius: currentQuestion.isMultipleChoice ? '6px' : '50%', display: 'flex', alignItems: 'center', justifyContent: 'center',
                      background: isSelected ? 'var(--primary-color)' : 'var(--bg-secondary)',
                      color: isSelected ? '#fff' : 'var(--text-primary)',
                      fontWeight: 600, fontSize: '0.8rem', flexShrink: 0,
                    }}>
                      {letter}
                    </span>
                    <span>{opt.text}</span>
                  </button>
                );
              })}
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '1.5rem', paddingTop: '1rem', borderTop: '1px solid var(--border-color)' }}>
              <button
                className="btn btn-outline"
                onClick={() => setCurrentQIndex(Math.max(0, currentQIndex - 1))}
                disabled={currentQIndex === 0}
              >
                ← Previous
              </button>

              <button
                className="btn btn-primary"
                onClick={() => setCurrentQIndex(Math.min(questions.length - 1, currentQIndex + 1))}
                disabled={currentQIndex >= questions.length - 1}
              >
                Next →
              </button>
            </div>
          </div>
        </div>

      </div>
    );
  }

  if (phase === 'results' && results) {
    const filteredReview = results.answerReview.filter(a => {
      if (reviewFilter === 'correct') return a.isCorrect;
      if (reviewFilter === 'incorrect') return !a.isCorrect && !a.isUnanswered;
      if (reviewFilter === 'unanswered') return a.isUnanswered;
      return true;
    });

    return (
      <div style={{ maxWidth: 900, margin: '0 auto' }}>
        <button className="btn btn-outline" style={{ marginBottom: '1rem' }} onClick={resetToList}>
          ← Back to Mock Exams
        </button>

        <div className="card" style={{ textAlign: 'center', marginBottom: '1.5rem', background: results.overallAccuracy >= 80 ? 'linear-gradient(135deg, #27ae6020, #2ecc7120)' : results.overallAccuracy >= 60 ? 'linear-gradient(135deg, #f39c1220, #e67e2220)' : 'linear-gradient(135deg, #e74c3c20, #c0392b20)' }}>
          <span style={{ background: examBadgeColor(results.examTypeCode), color: '#fff', padding: '3px 14px', borderRadius: 14, fontSize: '0.8rem', fontWeight: 600 }}>
            {results.examTypeCode}
          </span>
          <h1 style={{ fontSize: '2.5rem', margin: '0.5rem 0 0.25rem', fontWeight: 800 }}>
            {results.totalScore}%
          </h1>
          <p style={{ color: 'var(--text-secondary)', margin: 0 }}>
            {results.totalCorrect} / {results.totalQuestions} correct
          </p>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginTop: '0.5rem' }}>
            {results.examTitle} • {results.completedAt ? new Date(results.completedAt).toLocaleDateString() : ''}
          </p>
        </div>

        <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>Section Results</h2>
        <div style={{ display: 'grid', gap: '0.75rem', marginBottom: '1.5rem', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))' }}>
          {results.sectionResults.map(sr => (
            <div key={sr.sectionIndex} className="card">
              <h3 style={{ margin: '0 0 0.5rem', fontSize: '0.95rem' }}>{sr.sectionName}</h3>
              <div style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: '0.5rem', color: sr.accuracy >= 80 ? '#27ae60' : sr.accuracy >= 60 ? '#f39c12' : '#e74c3c' }}>
                {sr.accuracy}%
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                <span>✓ {sr.correctCount} correct</span>
                <span>✕ {sr.totalQuestions - sr.correctCount - sr.unansweredCount} wrong</span>
                {sr.unansweredCount > 0 && <span>{sr.unansweredCount} skipped</span>}
              </div>
              <div style={{ marginTop: '0.5rem', height: 6, borderRadius: 3, background: 'var(--bg-secondary)', overflow: 'hidden' }}>
                <div style={{ width: `${sr.accuracy}%`, height: '100%', borderRadius: 3, background: sr.accuracy >= 80 ? '#27ae60' : sr.accuracy >= 60 ? '#f39c12' : '#e74c3c', transition: 'width 0.5s' }} />
              </div>
            </div>
          ))}
        </div>

        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
          <h2 style={{ fontSize: '1.1rem', margin: 0 }}>Answer Review</h2>
          <button className="btn btn-outline" style={{ fontSize: '0.8rem' }} onClick={() => setShowReview(!showReview)}>
            {showReview ? 'Hide Review' : 'Show Review'}
          </button>
        </div>

        {showReview && (
          <>
            <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
              {(['all', 'correct', 'incorrect', 'unanswered'] as const).map(f => (
                <button
                  key={f}
                  className={`btn ${reviewFilter === f ? 'btn-primary' : 'btn-outline'}`}
                  style={{ padding: '4px 14px', fontSize: '0.8rem' }}
                  onClick={() => setReviewFilter(f)}
                >
                  {f === 'all' ? `All (${results.answerReview.length})` :
                   f === 'correct' ? `Correct (${results.answerReview.filter(a => a.isCorrect).length})` :
                   f === 'incorrect' ? `Incorrect (${results.answerReview.filter(a => !a.isCorrect && !a.isUnanswered).length})` :
                   `Unanswered (${results.answerReview.filter(a => a.isUnanswered).length})`}
                </button>
              ))}
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              {filteredReview.map((a) => (
                <div key={a.questionId} className="card" style={{
                  borderLeft: `4px solid ${a.isCorrect ? '#27ae60' : a.isUnanswered ? '#95a5a6' : '#e74c3c'}`
                }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
                    <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                      {a.sectionName} • {a.topicName}
                    </span>
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                      <span style={{ background: difficultyColor(a.difficulty), color: '#fff', padding: '1px 8px', borderRadius: 10, fontSize: '0.7rem' }}>
                        {a.difficulty}
                      </span>
                      <span style={{
                        color: a.isCorrect ? '#27ae60' : a.isUnanswered ? '#95a5a6' : '#e74c3c',
                        fontWeight: 600, fontSize: '0.8rem'
                      }}>
                        {a.isCorrect ? '✓ Correct' : a.isUnanswered ? 'Skipped' : '✕ Wrong'}
                      </span>
                    </div>
                  </div>
                  <p style={{ fontWeight: 500, marginBottom: '0.5rem', fontSize: '0.9rem' }}>
                    {results.answerReview.indexOf(a) + 1}. {a.questionText}
                  </p>
                  {a.imageUrl && (
                    <img src={a.imageUrl} alt="" style={{ maxWidth: '100%', maxHeight: 280, objectFit: 'contain', borderRadius: 8, margin: '0 0 0.5rem', display: 'block' }} />
                  )}
                  {!a.isUnanswered && !a.isCorrect && (
                    <p style={{ color: 'var(--error-color)', fontSize: '0.85rem', margin: '0.25rem 0' }}>
                      Your answer: {a.selectedOptionText}
                    </p>
                  )}
                  <p style={{ color: 'var(--success-color)', fontSize: '0.85rem', margin: '0.25rem 0' }}>
                    Correct: {a.correctOptionText}
                  </p>
                  {a.explanation && (
                    <p style={{ color: 'var(--text-secondary)', fontSize: '0.8rem', marginTop: '0.5rem', fontStyle: 'italic', lineHeight: 1.5 }}>
                      {a.explanation}
                    </p>
                  )}
                </div>
              ))}
            </div>
          </>
        )}

        <div style={{ textAlign: 'center', marginTop: '2rem' }}>
          <button className="btn btn-primary" onClick={resetToList}>
            Back to Mock Exams
          </button>
        </div>
      </div>
    );
  }

  return <div className="loading"><div className="spinner" /></div>;
}


function Stat({ label, value }: { label: string; value: string | number }) {
  return (
    <div style={{ textAlign: 'center' }}>
      <div style={{ fontSize: '1.1rem', fontWeight: 700 }}>{value}</div>
      <div style={{ fontSize: '0.7rem', color: 'var(--text-secondary)', textTransform: 'uppercase' }}>{label}</div>
    </div>
  );
}

function examBadgeColor(code: string): string {
  switch (code) {
    case 'SAT': return '#3498db';
    case 'NUET': return '#e67e22';
    default: return '#95a5a6';
  }
}

function difficultyColor(d: string): string {
  switch (d) {
    case 'Easy': return '#27ae60';
    case 'Medium': return '#f39c12';
    case 'Hard': return '#e74c3c';
    default: return '#95a5a6';
  }
}

export default MockExamPage;
