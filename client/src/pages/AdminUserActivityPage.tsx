import { useState, useEffect, useCallback } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import adminService from '../services/adminService';
import { useTranslation } from '../hooks/useTranslation';
import { getDateLocale } from '../i18n';

type ActivityData = Awaited<ReturnType<typeof adminService.getUserActivity>>;

interface MockReviewAnswer {
  questionId: number;
  questionText: string;
  topicName: string;
  sectionName: string;
  selectedOptionText: string | null;
  correctOptionText: string;
  isCorrect: boolean;
  isUnanswered: boolean;
  explanation: string | null;
}
interface MockReview {
  examTitle: string;
  totalScore: number;
  totalCorrect: number;
  totalQuestions: number;
  answerReview: MockReviewAnswer[];
}

function formatDate(iso: string | null): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString(getDateLocale(), {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });
}

export default function AdminUserActivityPage() {
  const { t } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();
  const [userId, setUserId] = useState(Number(searchParams.get('id')) || 0);
  const [inputId, setInputId] = useState(searchParams.get('id') || '');
  const [data, setData] = useState<ActivityData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const [review, setReview] = useState<MockReview | null>(null);
  const [reviewLoading, setReviewLoading] = useState(false);

  const openReview = async (attemptId: number) => {
    setReviewLoading(true);
    try {
      const r = await adminService.getMockAttemptReview(attemptId) as MockReview;
      setReview(r);
    } catch {
      setReview(null);
    } finally {
      setReviewLoading(false);
    }
  };

  const load = useCallback(async (uid: number, p: number) => {
    if (!uid) return;
    setLoading(true);
    setError('');
    try {
      const result = await adminService.getUserActivity(uid, p);
      setData(result);
    } catch {
      setError(t.admin.activity.userNotFound);
      setData(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (userId > 0) load(userId, page);
  }, [userId, page, load]);

  const handleSearch = () => {
    const id = parseInt(inputId);
    if (id > 0) {
      setUserId(id);
      setPage(1);
      setSearchParams({ id: String(id) });
    }
  };

  return (
    <div style={{ maxWidth: 1100, margin: '0 auto' }}>
      <h2 style={{ marginBottom: 16 }}>{t.admin.activity.title}</h2>

      {/* Search bar */}
      <div style={{ display: 'flex', gap: 8, marginBottom: 24 }}>
        <input
          type="number"
          placeholder="User ID"
          value={inputId}
          onChange={e => setInputId(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && handleSearch()}
          style={{
            padding: '8px 16px', borderRadius: 8, border: '1px solid var(--border)',
            background: 'var(--bg-secondary)', color: 'var(--text-primary)', width: 160
          }}
        />
        <button onClick={handleSearch} style={{
          padding: '8px 20px', borderRadius: 8, border: 'none',
          background: 'var(--accent-color)', color: '#fff', cursor: 'pointer', fontWeight: 600
        }}>{t.admin.activity.findBtn}</button>
        {data && (
          <button onClick={() => navigate(`/users`)} style={{
            padding: '8px 16px', borderRadius: 8, border: '1px solid var(--border)',
            background: 'var(--bg-secondary)', color: 'var(--text-primary)', cursor: 'pointer'
          }}>{t.admin.activity.backToList}</button>
        )}
      </div>

      {loading && <div style={{ textAlign: 'center', padding: 32 }}>{t.admin.common.loading}</div>}
      {error && <div style={{ color: '#ef4444', padding: 16 }}>{error}</div>}

      {data && !loading && (
        <>
          {/* User Info Header */}
          <div style={{
            background: 'var(--bg-secondary)', borderRadius: 12, padding: 20, marginBottom: 24,
            display: 'flex', justifyContent: 'space-between', alignItems: 'center'
          }}>
            <div>
              <div style={{ fontSize: 20, fontWeight: 700 }}>
                {data.user.name}
                {data.user.isBlocked && <span style={{ color: '#ef4444', marginLeft: 8 }}>{t.admin.activity.blockedBadge}</span>}
              </div>
              <div style={{ color: 'var(--text-secondary)', marginTop: 4 }}>
                {data.user.email} · {data.user.role} · {data.user.subscriptionTier}
              </div>
              <div style={{ color: 'var(--text-secondary)', fontSize: 13, marginTop: 2 }}>
                {t.admin.activity.registeredAt} {formatDate(data.user.createdAt)}
                {data.user.blockReason && ` · ${t.admin.activity.blockReason} ${data.user.blockReason}`}
              </div>
            </div>
            <div style={{ textAlign: 'right' }}>
              <div style={{ fontSize: 28, fontWeight: 700, color: 'var(--accent-color)' }}>
                {data.summary.accuracy}%
              </div>
              <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{t.admin.activity.accuracy}</div>
            </div>
          </div>

          {/* Summary Cards */}
          <div style={{
            display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))',
            gap: 12, marginBottom: 24
          }}>
            <SummaryCard label={t.admin.activity.totalAnswers} value={data.summary.totalAnswers} />
            <SummaryCard label={t.admin.activity.correctAnswers} value={data.summary.correctAnswers} />
            <SummaryCard label={t.admin.activity.sessionsCount} value={data.summary.totalSessions} />
            <SummaryCard label={t.admin.activity.lastActivity} text={formatDate(data.summary.lastActivity)} />
          </div>

          {/* Sessions (mock attempts) with review */}
          <div style={{ background: 'var(--bg-secondary)', borderRadius: 12, padding: 20, marginTop: 20 }}>
            <h3 style={{ margin: '0 0 12px' }}>
              {t.admin.activity.sessions} ({data.mockSessions.length})
            </h3>
            {data.mockSessions.length === 0 ? (
              <div style={{ color: 'var(--text-secondary)' }}>{t.admin.activity.noSessions}</div>
            ) : (
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ borderBottom: '2px solid var(--border)' }}>
                    {['ID', t.admin.activity.examCol, t.admin.activity.startCol, t.admin.activity.scoreCol, t.admin.activity.statusCol, ''].map((h, i) => (
                      <th key={i} style={{ padding: '8px 10px', textAlign: 'left', fontSize: 13, color: 'var(--text-secondary)' }}>{h}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {data.mockSessions.map(m => (
                    <tr key={m.id} style={{ borderBottom: '1px solid var(--border)' }}>
                      <td style={{ padding: '8px 10px', fontFamily: 'monospace', fontSize: 13 }}>{m.id}</td>
                      <td style={{ padding: '8px 10px', fontWeight: 600 }}>{m.title || m.examTypeCode}</td>
                      <td style={{ padding: '8px 10px', fontSize: 13 }}>{formatDate(m.startedAt)}</td>
                      <td style={{ padding: '8px 10px' }}>{m.totalScore != null ? `${m.totalScore}%` : '—'}</td>
                      <td style={{ padding: '8px 10px' }}>
                        <span style={{
                          padding: '2px 10px', borderRadius: 12, fontSize: 12, fontWeight: 600, color: '#fff',
                          background: m.status === 'completed' ? '#22c55e' : '#f59e0b'
                        }}>
                          {m.status === 'completed' ? t.admin.activity.completed : t.admin.activity.inProgress}
                        </span>
                      </td>
                      <td style={{ padding: '8px 10px', textAlign: 'right' }}>
                        {m.status === 'completed' && (
                          <button className="btn btn-outline" style={{ fontSize: 12, padding: '2px 12px' }} onClick={() => openReview(m.id)}>
                            {t.admin.activity.reviewBtn}
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </>
      )}

      {/* Review modal */}
      {(review || reviewLoading) && (
        <div onClick={() => setReview(null)} style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'flex-start', justifyContent: 'center', zIndex: 1000, padding: '2rem 1rem', overflowY: 'auto' }}>
          <div onClick={e => e.stopPropagation()} style={{ background: 'var(--card-background)', borderRadius: 12, maxWidth: 800, width: '100%', padding: '1.5rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
              <h3 style={{ margin: 0 }}>{t.admin.activity.reviewTitle}</h3>
              <button onClick={() => setReview(null)} style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '1.2rem', color: 'var(--text-muted)' }}>✕</button>
            </div>
            {reviewLoading || !review ? (
              <div style={{ color: 'var(--text-secondary)' }}>{t.admin.common?.loading ?? '...'}</div>
            ) : (
              <>
                <div style={{ marginBottom: '1rem', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
                  {review.examTitle} — {review.totalScore}% ({review.totalCorrect}/{review.totalQuestions})
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                  {review.answerReview.map((a, i) => (
                    <div key={a.questionId} className="card" style={{ borderLeft: `4px solid ${a.isCorrect ? '#27ae60' : a.isUnanswered ? '#95a5a6' : '#e74c3c'}`, padding: '0.75rem' }}>
                      <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>
                        {i + 1}. {a.sectionName} • {a.topicName} — {a.isCorrect ? '✓' : a.isUnanswered ? '—' : '✕'}
                      </div>
                      <p style={{ fontWeight: 500, margin: '0 0 0.5rem', fontSize: '0.9rem' }}>{a.questionText}</p>
                      {!a.isUnanswered && !a.isCorrect && (
                        <p style={{ color: 'var(--error-color)', fontSize: '0.85rem', margin: '0.15rem 0' }}>{a.selectedOptionText}</p>
                      )}
                      <p style={{ color: 'var(--success-color)', fontSize: '0.85rem', margin: '0.15rem 0' }}>{a.correctOptionText}</p>
                      {a.explanation && (
                        <p style={{ color: 'var(--text-secondary)', fontSize: '0.8rem', marginTop: '0.4rem', fontStyle: 'italic' }}>{a.explanation}</p>
                      )}
                    </div>
                  ))}
                </div>
              </>
            )}
          </div>
        </div>
      )}

      {!data && !loading && !error && (
        <div style={{ textAlign: 'center', padding: 48, color: 'var(--text-secondary)' }}>
          {t.admin.activity.enterUserIdPrompt}
        </div>
      )}
    </div>
  );
}

function SummaryCard({ label, value, text, suffix }: {
  label: string; value?: number; text?: string; suffix?: string;
}) {
  return (
    <div style={{ background: 'var(--bg-secondary)', borderRadius: 10, padding: '14px 18px' }}>
      <div style={{ fontSize: 12, color: 'var(--text-secondary)', marginBottom: 4 }}>{label}</div>
      <div style={{ fontWeight: 700, fontSize: 20 }}>
        {text ?? `${(value ?? 0).toLocaleString(getDateLocale())}${suffix || ''}`}
      </div>
    </div>
  );
}
