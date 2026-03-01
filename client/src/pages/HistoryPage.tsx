import { useEffect, useState } from 'react';
import { analyticsService } from '../services/analyticsService';
import type { TestSessionSummary, TestSessionDetail } from '../types';

function HistoryPage() {
  const [sessions, setSessions] = useState<TestSessionSummary[]>([]);
  const [selectedSession, setSelectedSession] = useState<TestSessionDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isDetailLoading, setIsDetailLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);

  useEffect(() => {
    const fetchSessions = async () => {
      try {
        setIsLoading(true);
        const data = await analyticsService.getSessions(page, 10);
        setSessions(data);
      } catch (err) {
        setError('Failed to load test history');
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchSessions();
  }, [page]);

  const handleViewDetail = async (sessionId: number) => {
    if (selectedSession?.id === sessionId) {
      setSelectedSession(null);
      return;
    }
    try {
      setIsDetailLoading(true);
      const detail = await analyticsService.getSessionDetail(sessionId);
      setSelectedSession(detail);
    } catch (err) {
      console.error(err);
    } finally {
      setIsDetailLoading(false);
    }
  };

  const formatDate = (dateStr: string) => {
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getModeIcon = (mode: string) => (mode === 'exam' ? '' : '');
  const getScoreColor = (score: number | null) => {
    if (score === null) return 'var(--text-secondary)';
    if (score >= 80) return 'var(--success-color)';
    if (score >= 50) return 'var(--warning-color)';
    return 'var(--error-color)';
  };

  if (isLoading) {
    return (
      <div className="animate-fade-in" style={{ maxWidth: '800px', margin: '0 auto' }}>
        <h1 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '2rem' }}>Test History</h1>
        {[1, 2, 3].map((i) => (
          <div key={i} className="card skeleton" style={{ height: '80px', marginBottom: '1rem' }} />
        ))}
      </div>
    );
  }

  if (error) return <p className="error-message animate-fade-in">{error}</p>;

  return (
    <div className="animate-fade-in" style={{ maxWidth: '800px', margin: '0 auto' }}>
      <h1 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '2rem' }}>Test History</h1>

      {sessions.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <p style={{ fontSize: '1.125rem', color: 'var(--text-secondary)' }}>
            No test sessions yet. Start a test to see your history here!
          </p>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {sessions.map((session) => (
            <div key={session.id}>
              <button
                onClick={() => handleViewDetail(session.id)}
                className="card"
                style={{
                  width: '100%',
                  textAlign: 'left',
                  cursor: 'pointer',
                  border: selectedSession?.id === session.id ? '2px solid var(--primary-color)' : '1px solid var(--border-color)',
                  transition: 'all 0.2s ease',
                  backgroundColor: 'var(--card-background)',
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                    <span style={{ fontSize: '1.5rem' }}>{getModeIcon(session.mode)}</span>
                    <div>
                      <div style={{ fontWeight: '600', color: 'var(--text-primary)' }}>
                        {session.examTypeName}
                      </div>
                      <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                        {formatDate(session.startedAt)} · {session.mode === 'exam' ? 'Exam' : 'Practice'} mode
                      </div>
                    </div>
                  </div>
                  <div style={{ textAlign: 'right' }}>
                    <div style={{ fontSize: '1.25rem', fontWeight: '700', color: getScoreColor(session.score) }}>
                      {session.score !== null ? `${session.score}%` : '—'}
                    </div>
                    <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                      {session.correctCount}/{session.totalQuestions} correct
                    </div>
                  </div>
                </div>
              </button>

              {/* Session Detail Expansion */}
              {selectedSession?.id === session.id && !isDetailLoading && (
                <div className="card animate-fade-in" style={{ marginTop: '0.5rem', borderLeft: '3px solid var(--primary-color)' }}>
                  <h3 style={{ fontSize: '1rem', fontWeight: '600', marginBottom: '1rem' }}>
                    Session Details
                  </h3>
                  {selectedSession.answers.length === 0 ? (
                    <p style={{ color: 'var(--text-secondary)' }}>No answers recorded for this session.</p>
                  ) : (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                      {selectedSession.answers.map((answer, i) => (
                        <div
                          key={i}
                          style={{
                            padding: '0.75rem',
                            borderRadius: '0.5rem',
                            backgroundColor: answer.isCorrect
                              ? 'rgba(16, 185, 129, 0.08)'
                              : 'rgba(239, 68, 68, 0.08)',
                            border: `1px solid ${answer.isCorrect ? 'rgba(16, 185, 129, 0.2)' : 'rgba(239, 68, 68, 0.2)'}`,
                          }}
                        >
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: '1rem' }}>
                            <div style={{ flex: 1 }}>
                              <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>
                                {answer.topicName} · {answer.difficulty}
                                {answer.timeSpentSeconds !== null && ` · ${answer.timeSpentSeconds}s`}
                              </div>
                              <div style={{ fontSize: '0.9rem', color: 'var(--text-primary)', marginBottom: '0.5rem' }}>
                                {answer.questionText}
                              </div>
                              <div style={{ fontSize: '0.85rem' }}>
                                <span style={{ color: answer.isCorrect ? 'var(--success-color)' : 'var(--error-color)' }}>
                                  Your answer: {answer.selectedOptionText}
                                </span>
                                {!answer.isCorrect && (
                                  <span style={{ color: 'var(--success-color)', marginLeft: '1rem' }}>
                                    Correct: {answer.correctOptionText}
                                  </span>
                                )}
                              </div>
                              {answer.explanation && (
                                <div style={{
                                  marginTop: '0.5rem',
                                  padding: '0.5rem',
                                  backgroundColor: 'rgba(99, 102, 241, 0.08)',
                                  borderRadius: '0.25rem',
                                  fontSize: '0.85rem',
                                  color: 'var(--text-secondary)',
                                }}>
                                  {answer.explanation}
                                </div>
                              )}
                            </div>
                            <span style={{ fontSize: '1.25rem', flexShrink: 0 }}>
                              {answer.isCorrect ? '✓' : '✕'}
                            </span>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}
              {selectedSession?.id === session.id && isDetailLoading && (
                <div className="card" style={{ marginTop: '0.5rem', textAlign: 'center', padding: '2rem' }}>
                  <div className="spinner" />
                </div>
              )}
            </div>
          ))}

          {/* Pagination */}
          <div style={{ display: 'flex', justifyContent: 'center', gap: '1rem', marginTop: '1rem' }}>
            <button
              className="btn btn-secondary"
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
            >
              ← Previous
            </button>
            <span style={{ display: 'flex', alignItems: 'center', color: 'var(--text-secondary)' }}>
              Page {page}
            </span>
            <button
              className="btn btn-secondary"
              onClick={() => setPage((p) => p + 1)}
              disabled={sessions.length < 10}
            >
              Next →
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default HistoryPage;
