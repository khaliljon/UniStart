import { useEffect, useState, useRef, useCallback } from 'react';
import { mockExamService } from '../services/mockExamService';
import { useAppSelector } from '../hooks/useAppSelector';
import { ProGate } from '../components/ProGate';
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
  | 'list'        // Choose a mock exam
  | 'detail'      // View exam info + sections before starting
  | 'instructions' // Section instructions before starting section
  | 'section'     // Answering questions in a section
  | 'section-break' // Between sections
  | 'results';    // Final results

function MockExamPage() {
  // Phase
  const [phase, setPhase] = useState<Phase>('list');
  const { user } = useAppSelector((state) => state.auth);
  const isPro = user?.subscriptionTier === 'Pro' || user?.role === 'Admin';

  // List phase
  const [mockExams, setMockExams] = useState<MockExamListItem[]>([]);
  const [history, setHistory] = useState<MockExamHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);

  // Detail phase
  const [examDetail, setExamDetail] = useState<MockExamDetail | null>(null);

  // Active attempt
  const [attempt, setAttempt] = useState<MockExamAttempt | null>(null);
  const [sectionState, setSectionState] = useState<MockExamSectionState | null>(null);
  const [currentQIndex, setCurrentQIndex] = useState(0);

  // Timer
  const [timeLeft, setTimeLeft] = useState(0);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  // Results
  const [results, setResults] = useState<MockExamResult | null>(null);
  const [showReview, setShowReview] = useState(false);
  const [reviewFilter, setReviewFilter] = useState<'all' | 'incorrect' | 'unanswered'>('all');

  // ── Load mock exams list ──────────────────────────────
  const loadExams = useCallback(async () => {
    setLoading(true);
    try {
      const [exams, hist] = await Promise.all([
        mockExamService.getAvailableMockExams(),
        mockExamService.getHistory(),
      ]);
      setMockExams(exams);
      setHistory(hist);
    } catch (e) { console.error(e); }
    setLoading(false);
  }, []);

  useEffect(() => { loadExams(); }, [loadExams]);

  // ── Timer logic ───────────────────────────────────────
  useEffect(() => {
    if (phase === 'section' && timeLeft > 0) {
      timerRef.current = setInterval(() => {
        setTimeLeft(prev => {
          if (prev <= 1) {
            // Time's up — auto-complete section
            handleCompleteSection();
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
    }
    return () => { if (timerRef.current) clearInterval(timerRef.current); };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [phase, timeLeft > 0]);

  const formatTime = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, '0')}`;
  };

  // ── Select exam to view detail ────────────────────────
  const handleSelectExam = async (examId: number) => {
    setLoading(true);
    try {
      const detail = await mockExamService.getMockExamDetail(examId);
      setExamDetail(detail);
      setPhase('detail');
    } catch (e) { console.error(e); }
    setLoading(false);
  };

  // ── Start exam ────────────────────────────────────────
  const handleStartExam = async () => {
    if (!examDetail) return;
    setLoading(true);
    try {
      const att = await mockExamService.startMockExam(examDetail.id);
      setAttempt(att);
      // Load first section
      const section = await mockExamService.getCurrentSection(att.attemptId);
      setSectionState(section);
      setCurrentQIndex(0);
      setTimeLeft(section.timeLimitMinutes * 60);
      setPhase('instructions');
    } catch (e) { console.error(e); }
    setLoading(false);
  };

  // ── Begin section (after instructions) ────────────────
  const handleBeginSection = () => {
    setPhase('section');
  };

  // ── Select answer ─────────────────────────────────────
  const handleSelectOption = async (questionId: number, optionId: number) => {
    if (!attempt || !sectionState) return;
    try {
      await mockExamService.submitAnswer(attempt.attemptId, questionId, optionId);
      // Update local state
      setSectionState(prev => {
        if (!prev) return prev;
        const updatedQuestions = prev.questions.map(q =>
          q.questionId === questionId ? { ...q, selectedOptionId: optionId } : q
        );
        const answeredCount = updatedQuestions.filter(q => q.selectedOptionId !== null).length;
        return { ...prev, questions: updatedQuestions, answeredCount };
      });
    } catch (e) { console.error(e); }
  };

  // ── Complete section ──────────────────────────────────
  const handleCompleteSection = async () => {
    if (!attempt) return;
    if (timerRef.current) clearInterval(timerRef.current);
    setLoading(true);
    try {
      const updated = await mockExamService.completeSection(attempt.attemptId);
      setAttempt(updated);

      if (updated.status === 'completed') {
        // Exam is done — load results
        const res = await mockExamService.getResults(updated.attemptId);
        setResults(res);
        setPhase('results');
      } else {
        // Move to next section
        const nextSection = await mockExamService.getCurrentSection(updated.attemptId);
        setSectionState(nextSection);
        setCurrentQIndex(0);
        setTimeLeft(nextSection.timeLimitMinutes * 60);
        setPhase('instructions');
      }
    } catch (e) { console.error(e); }
    setLoading(false);
  };

  // ── Abandon attempt ───────────────────────────────────
  const handleAbandon = async () => {
    if (!attempt) return;
    if (!window.confirm('Are you sure you want to abandon this exam? Your progress will be lost.')) return;
    if (timerRef.current) clearInterval(timerRef.current);
    try {
      await mockExamService.abandonAttempt(attempt.attemptId);
    } catch (e) { console.error(e); }
    resetToList();
  };

  // ── Back to list ──────────────────────────────────────
  const resetToList = async () => {
    setPhase('list');
    setAttempt(null);
    setSectionState(null);
    setExamDetail(null);
    setResults(null);
    setShowReview(false);
    setCurrentQIndex(0);
    await loadExams();
  };

  // ── Current question ──────────────────────────────────
  const currentQuestion: MockExamQuestion | null =
    sectionState?.questions?.[currentQIndex] ?? null;

  // ── Timer color ───────────────────────────────────────
  const timerColor = timeLeft < 60 ? '#e74c3c' : timeLeft < 180 ? '#f39c12' : 'var(--text-primary)';

  // ══════════════════════════════════════════════════════
  //  RENDER PHASE: LIST
  // ══════════════════════════════════════════════════════
  if (phase === 'list') {
    if (loading) return <div className="loading"><div className="spinner" /></div>;

    return (
      <ProGate hasAccess={isPro} featureName="Mock Exams">
      <div style={{ maxWidth: 900, margin: '0 auto' }}>
        <h1 style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: '0.5rem' }}>
          📝 Mock Exams
        </h1>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem' }}>
          Take a full-length practice test under real exam conditions with timed sections
        </p>

        {/* Exam cards */}
        <div style={{ display: 'grid', gap: '1rem', marginBottom: '2rem' }}>
          {mockExams.map(exam => (
            <div key={exam.id} className="card" style={{ cursor: 'pointer', transition: 'transform 0.15s' }}
                 onClick={() => handleSelectExam(exam.id)}
                 onMouseEnter={e => (e.currentTarget.style.transform = 'translateY(-2px)')}
                 onMouseLeave={e => (e.currentTarget.style.transform = 'none')}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: '1rem', flexWrap: 'wrap' }}>
                <div style={{ flex: 1, minWidth: 200 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                    <span style={{ background: examBadgeColor(exam.examTypeCode), color: '#fff', padding: '2px 10px', borderRadius: 12, fontSize: '0.75rem', fontWeight: 600 }}>
                      {exam.examTypeCode}
                    </span>
                    <h3 style={{ margin: 0, fontSize: '1.1rem' }}>{exam.title}</h3>
                  </div>
                  <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', margin: '0.25rem 0' }}>{exam.description}</p>
                </div>
                <div style={{ display: 'flex', gap: '1.5rem', alignItems: 'center', flexWrap: 'wrap' }}>
                  <Stat label="Questions" value={exam.totalQuestions} />
                  <Stat label="Duration" value={`${exam.totalTimeMinutes}m`} />
                  <Stat label="Sections" value={exam.sectionCount} />
                  {exam.bestScore !== null && <Stat label="Best" value={`${exam.bestScore}%`} />}
                  {exam.attemptCount > 0 && <Stat label="Attempts" value={exam.attemptCount} />}
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* History */}
        {history.length > 0 && (
          <>
            <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>📊 Recent Attempts</h2>
            <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.85rem' }}>
                <thead>
                  <tr style={{ background: 'var(--bg-secondary)', textAlign: 'left' }}>
                    <th style={{ padding: '0.5rem 1rem' }}>Exam</th>
                    <th style={{ padding: '0.5rem' }}>Score</th>
                    <th style={{ padding: '0.5rem' }}>Status</th>
                    <th style={{ padding: '0.5rem' }}>Date</th>
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
                          {h.status === 'completed' ? '✅ Completed' : '⏳ In progress'}
                        </span>
                      </td>
                      <td style={{ padding: '0.5rem', color: 'var(--text-secondary)' }}>
                        {new Date(h.startedAt).toLocaleDateString()}
                      </td>
                      <td style={{ padding: '0.5rem' }}>
                        {h.status === 'completed' && (
                          <button className="btn btn-outline" style={{ padding: '2px 12px', fontSize: '0.75rem' }}
                                  onClick={async (e) => { e.stopPropagation(); const r = await mockExamService.getResults(h.attemptId); setResults(r); setShowReview(false); setPhase('results'); }}>
                            View
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
      </ProGate>
    );
  }

  // ══════════════════════════════════════════════════════
  //  RENDER PHASE: DETAIL (exam info before starting)
  // ══════════════════════════════════════════════════════
  if (phase === 'detail' && examDetail) {
    return (
      <div style={{ maxWidth: 700, margin: '0 auto' }}>
        <button className="btn btn-outline" style={{ marginBottom: '1rem' }} onClick={() => setPhase('list')}>
          ← Back
        </button>

        <div className="card" style={{ textAlign: 'center', marginBottom: '1.5rem' }}>
          <span style={{ background: examBadgeColor(examDetail.examTypeCode), color: '#fff', padding: '3px 14px', borderRadius: 14, fontSize: '0.8rem', fontWeight: 600 }}>
            {examDetail.examTypeCode}
          </span>
          <h1 style={{ fontSize: '1.5rem', margin: '0.75rem 0 0.25rem' }}>{examDetail.title}</h1>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{examDetail.description}</p>
          <div style={{ display: 'flex', justifyContent: 'center', gap: '2rem', marginTop: '1rem' }}>
            <Stat label="Total Time" value={`${examDetail.totalTimeMinutes} min`} />
            <Stat label="Sections" value={examDetail.sections.length} />
          </div>
        </div>

        <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>📋 Sections</h2>
        {examDetail.sections.map((s, i) => (
          <div key={s.id} className="card" style={{ marginBottom: '0.75rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div>
                <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>Section {i + 1}</span>
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
          <button className="btn btn-primary" style={{ padding: '0.75rem 3rem', fontSize: '1rem' }} onClick={handleStartExam} disabled={loading}>
            {loading ? 'Starting...' : '🚀 Start Exam'}
          </button>
        </div>
      </div>
    );
  }

  // ══════════════════════════════════════════════════════
  //  RENDER PHASE: INSTRUCTIONS (before each section)
  // ══════════════════════════════════════════════════════
  if (phase === 'instructions' && sectionState && attempt) {
    return (
      <div style={{ maxWidth: 600, margin: '0 auto', textAlign: 'center' }}>
        <div className="card" style={{ padding: '2rem' }}>
          <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
            Section {sectionState.sectionIndex + 1} of {attempt.totalSections}
          </span>
          <h1 style={{ fontSize: '1.5rem', margin: '0.5rem 0' }}>{sectionState.sectionName}</h1>

          <div style={{ display: 'flex', justifyContent: 'center', gap: '2rem', margin: '1.5rem 0' }}>
            <Stat label="Questions" value={sectionState.totalQuestions} />
            <Stat label="Time Limit" value={`${sectionState.timeLimitMinutes} min`} />
          </div>

          {sectionState.instructions && (
            <div style={{ background: 'var(--bg-secondary)', borderRadius: 8, padding: '1rem', margin: '1rem 0', textAlign: 'left', lineHeight: 1.6, fontSize: '0.9rem' }}>
              {sectionState.instructions}
            </div>
          )}

          <div style={{ marginTop: '1.5rem', display: 'flex', gap: '1rem', justifyContent: 'center' }}>
            <button className="btn btn-outline" onClick={handleAbandon}>
              Abandon Exam
            </button>
            <button className="btn btn-primary" style={{ padding: '0.75rem 2.5rem' }} onClick={handleBeginSection}>
              ▶ Begin Section
            </button>
          </div>
        </div>
      </div>
    );
  }

  // ══════════════════════════════════════════════════════
  //  RENDER PHASE: SECTION (answering questions)
  // ══════════════════════════════════════════════════════
  if (phase === 'section' && sectionState && currentQuestion && attempt) {
    const questions = sectionState.questions;
    const hasPassage = !!currentQuestion.passageContent;

    return (
      <div style={{ maxWidth: 1100, margin: '0 auto' }}>
        {/* Header bar: timer + section info + progress */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem', flexWrap: 'wrap', gap: '0.5rem' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <span style={{ background: 'var(--bg-secondary)', padding: '4px 12px', borderRadius: 8, fontWeight: 600, fontSize: '0.85rem' }}>
              {sectionState.sectionName}
            </span>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
              Section {sectionState.sectionIndex + 1}/{attempt.totalSections}
            </span>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '1.5rem' }}>
            <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
              {sectionState.answeredCount}/{sectionState.totalQuestions} answered
            </span>
            {/* Timer */}
            <div style={{ background: 'var(--bg-secondary)', padding: '6px 16px', borderRadius: 8, fontWeight: 700, fontSize: '1.1rem', fontFamily: 'monospace', color: timerColor, minWidth: 80, textAlign: 'center' }}>
              ⏱ {formatTime(timeLeft)}
            </div>
          </div>
        </div>

        {/* Question navigator grid */}
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

        {/* Main content area: passage (left) + question (right) */}
        <div style={{ display: 'flex', gap: '1rem', alignItems: hasPassage ? 'stretch' : 'flex-start' }}>
          {/* Reading passage panel */}
          {hasPassage && (
            <div className="card" style={{ flex: 1, maxHeight: 600, overflowY: 'auto', fontSize: '0.88rem', lineHeight: 1.7 }}>
              <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem' }}>
                📖 {currentQuestion.passageTitle}
              </h3>
              {currentQuestion.passageContent?.split('\n\n').map((p, i) => (
                <p key={i} style={{ margin: '0 0 0.75rem', textAlign: 'justify' }}>{p}</p>
              ))}
            </div>
          )}

          {/* Question panel */}
          <div className="card" style={{ flex: 1, minWidth: hasPassage ? 350 : 'auto' }}>
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

            {/* Options */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
              {currentQuestion.options.map((opt, i) => {
                const isSelected = currentQuestion.selectedOptionId === opt.id;
                const letter = String.fromCharCode(65 + i);
                return (
                  <button
                    key={opt.id}
                    onClick={() => handleSelectOption(currentQuestion.questionId, opt.id)}
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
                      width: 28, height: 28, borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center',
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

            {/* Navigation */}
            <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '1.5rem', paddingTop: '1rem', borderTop: '1px solid var(--border-color)' }}>
              <button
                className="btn btn-outline"
                onClick={() => setCurrentQIndex(Math.max(0, currentQIndex - 1))}
                disabled={currentQIndex === 0}
              >
                ← Previous
              </button>

              {currentQIndex < questions.length - 1 ? (
                <button
                  className="btn btn-primary"
                  onClick={() => setCurrentQIndex(currentQIndex + 1)}
                >
                  Next →
                </button>
              ) : (
                <button
                  className="btn btn-primary"
                  style={{ background: '#27ae60' }}
                  onClick={handleCompleteSection}
                >
                  {sectionState.sectionIndex + 1 < attempt.totalSections
                    ? '✅ Complete Section'
                    : '🏁 Finish Exam'}
                </button>
              )}
            </div>
          </div>
        </div>

        {/* Abandon button */}
        <div style={{ textAlign: 'center', marginTop: '1.5rem' }}>
          <button className="btn btn-outline" style={{ color: '#e74c3c', borderColor: '#e74c3c', fontSize: '0.8rem' }} onClick={handleAbandon}>
            Abandon Exam
          </button>
        </div>
      </div>
    );
  }

  // ══════════════════════════════════════════════════════
  //  RENDER PHASE: RESULTS
  // ══════════════════════════════════════════════════════
  if (phase === 'results' && results) {
    const filteredReview = results.answerReview.filter(a => {
      if (reviewFilter === 'incorrect') return !a.isCorrect && !a.isUnanswered;
      if (reviewFilter === 'unanswered') return a.isUnanswered;
      return true;
    });

    return (
      <div style={{ maxWidth: 900, margin: '0 auto' }}>
        <button className="btn btn-outline" style={{ marginBottom: '1rem' }} onClick={resetToList}>
          ← Back to Mock Exams
        </button>

        {/* Score header */}
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

        {/* Section breakdown */}
        <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>📊 Section Results</h2>
        <div style={{ display: 'grid', gap: '0.75rem', marginBottom: '1.5rem', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))' }}>
          {results.sectionResults.map(sr => (
            <div key={sr.sectionIndex} className="card">
              <h3 style={{ margin: '0 0 0.5rem', fontSize: '0.95rem' }}>{sr.sectionName}</h3>
              <div style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: '0.5rem', color: sr.accuracy >= 80 ? '#27ae60' : sr.accuracy >= 60 ? '#f39c12' : '#e74c3c' }}>
                {sr.accuracy}%
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                <span>✅ {sr.correctCount} correct</span>
                <span>❌ {sr.totalQuestions - sr.correctCount - sr.unansweredCount} wrong</span>
                {sr.unansweredCount > 0 && <span>⬜ {sr.unansweredCount} skipped</span>}
              </div>
              {/* Progress bar */}
              <div style={{ marginTop: '0.5rem', height: 6, borderRadius: 3, background: 'var(--bg-secondary)', overflow: 'hidden' }}>
                <div style={{ width: `${sr.accuracy}%`, height: '100%', borderRadius: 3, background: sr.accuracy >= 80 ? '#27ae60' : sr.accuracy >= 60 ? '#f39c12' : '#e74c3c', transition: 'width 0.5s' }} />
              </div>
            </div>
          ))}
        </div>

        {/* Answer review toggle */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
          <h2 style={{ fontSize: '1.1rem', margin: 0 }}>📝 Answer Review</h2>
          <button className="btn btn-outline" style={{ fontSize: '0.8rem' }} onClick={() => setShowReview(!showReview)}>
            {showReview ? 'Hide Review' : 'Show Review'}
          </button>
        </div>

        {showReview && (
          <>
            {/* Filter tabs */}
            <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
              {(['all', 'incorrect', 'unanswered'] as const).map(f => (
                <button
                  key={f}
                  className={`btn ${reviewFilter === f ? 'btn-primary' : 'btn-outline'}`}
                  style={{ padding: '4px 14px', fontSize: '0.8rem' }}
                  onClick={() => setReviewFilter(f)}
                >
                  {f === 'all' ? `All (${results.answerReview.length})` :
                   f === 'incorrect' ? `Incorrect (${results.answerReview.filter(a => !a.isCorrect && !a.isUnanswered).length})` :
                   `Unanswered (${results.answerReview.filter(a => a.isUnanswered).length})`}
                </button>
              ))}
            </div>

            {/* Review list */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              {filteredReview.map((a, i) => (
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
                        {a.isCorrect ? '✅ Correct' : a.isUnanswered ? '⬜ Skipped' : '❌ Wrong'}
                      </span>
                    </div>
                  </div>
                  <p style={{ fontWeight: 500, marginBottom: '0.5rem', fontSize: '0.9rem' }}>
                    {i + 1}. {a.questionText}
                  </p>
                  {!a.isUnanswered && !a.isCorrect && (
                    <p style={{ color: '#e74c3c', fontSize: '0.85rem', margin: '0.25rem 0' }}>
                      Your answer: {a.selectedOptionText}
                    </p>
                  )}
                  <p style={{ color: '#27ae60', fontSize: '0.85rem', margin: '0.25rem 0' }}>
                    Correct: {a.correctOptionText}
                  </p>
                  {a.explanation && (
                    <p style={{ color: 'var(--text-secondary)', fontSize: '0.8rem', marginTop: '0.5rem', fontStyle: 'italic', lineHeight: 1.5 }}>
                      💡 {a.explanation}
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

  // ── Fallback loading ──────────────────────────────────
  return <div className="loading"><div className="spinner" /></div>;
}

// ── Helper components ─────────────────────────────────────

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
    case 'TOEFL': return '#9b59b6';
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
