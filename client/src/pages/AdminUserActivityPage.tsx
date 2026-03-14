import { useState, useEffect, useCallback } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import adminService from '../services/adminService';
import { useTranslation } from '../hooks/useTranslation';
import { getDateLocale } from '../i18n';

type ActivityData = Awaited<ReturnType<typeof adminService.getUserActivity>>;

function formatDate(iso: string | null): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString(getDateLocale(), {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });
}

const LEVEL_COLORS: Record<string, string> = {
  Novice: '#6b7280', Beginner: '#3b82f6', Intermediate: '#f59e0b',
  Advanced: '#22c55e', Expert: '#a855f7', Master: '#ef4444'
};

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
            <SummaryCard label="Streak" value={data.summary.currentStreak} suffix={` ${t.admin.activity.daysShort}`} />
            <SummaryCard label={t.admin.activity.lastActivity} text={formatDate(data.summary.lastActivity)} />
          </div>

          {/* Two-column: Skills + Activity Heatmap */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24, marginBottom: 24 }}>
            {/* Skills */}
            <div style={{ background: 'var(--bg-secondary)', borderRadius: 12, padding: 20 }}>
              <h3 style={{ margin: '0 0 12px' }}>{t.admin.activity.skills}</h3>
              {data.skills.length === 0 ? (
                <div style={{ color: 'var(--text-secondary)' }}>{t.admin.common.noData}</div>
              ) : data.skills.map(s => (
                <div key={s.skillName} style={{
                  display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                  padding: '8px 0', borderBottom: '1px solid var(--border)'
                }}>
                  <div>
                    <div style={{ fontWeight: 600, fontSize: 14 }}>{s.skillName}</div>
                    <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>
                      θ = {s.theta.toFixed(2)} ± {s.thetaSE.toFixed(2)}
                    </div>
                  </div>
                  <span style={{
                    padding: '2px 10px', borderRadius: 12, fontSize: 12, fontWeight: 600,
                    color: '#fff', background: LEVEL_COLORS[s.level] || '#6b7280'
                  }}>{s.level}</span>
                </div>
              ))}
            </div>

            {/* Daily Activity Chart (simple bar chart) */}
            <div style={{ background: 'var(--bg-secondary)', borderRadius: 12, padding: 20 }}>
              <h3 style={{ margin: '0 0 12px' }}>{t.admin.activity.activity30days}</h3>
              {data.dailyActivity.length === 0 ? (
                <div style={{ color: 'var(--text-secondary)' }}>{t.admin.common.noData}</div>
              ) : (
                <div style={{ display: 'flex', alignItems: 'flex-end', gap: 2, height: 120 }}>
                  {data.dailyActivity.map(d => {
                    const max = Math.max(...data.dailyActivity.map(x => x.count));
                    const h = max > 0 ? (d.count / max) * 100 : 0;
                    return (
                      <div key={d.date} title={`${new Date(d.date).toLocaleDateString(getDateLocale())}: ${d.count}`}
                        style={{
                          flex: 1, minWidth: 4, borderRadius: '4px 4px 0 0',
                          height: `${Math.max(h, 4)}%`,
                          background: d.count > 0 ? 'var(--accent-color)' : 'var(--border)',
                          opacity: d.count > 0 ? 0.6 + (h / 250) : 0.3
                        }}
                      />
                    );
                  })}
                </div>
              )}
            </div>
          </div>

          {/* Sessions Table */}
          <div style={{ background: 'var(--bg-secondary)', borderRadius: 12, padding: 20 }}>
            <h3 style={{ margin: '0 0 12px' }}>
              {t.admin.activity.sessions} ({data.sessions.totalCount})
            </h3>
            {data.sessions.items.length === 0 ? (
              <div style={{ color: 'var(--text-secondary)' }}>{t.admin.activity.noSessions}</div>
            ) : (
              <>
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                  <thead>
                    <tr style={{ borderBottom: '2px solid var(--border)' }}>
                      {['ID', t.admin.activity.examCol, t.admin.activity.startCol, t.admin.activity.endCol, t.admin.activity.questionsCol, t.admin.activity.correctCol, t.admin.activity.statusCol].map(h => (
                        <th key={h} style={{ padding: '8px 10px', textAlign: 'left', fontSize: 13, color: 'var(--text-secondary)' }}>{h}</th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {data.sessions.items.map(s => (
                      <tr key={s.id} style={{ borderBottom: '1px solid var(--border)' }}>
                        <td style={{ padding: '8px 10px', fontFamily: 'monospace', fontSize: 13 }}>{s.id}</td>
                        <td style={{ padding: '8px 10px', fontWeight: 600 }}>{s.examTypeCode}</td>
                        <td style={{ padding: '8px 10px', fontSize: 13 }}>{formatDate(s.startedAt)}</td>
                        <td style={{ padding: '8px 10px', fontSize: 13 }}>{formatDate(s.completedAt)}</td>
                        <td style={{ padding: '8px 10px' }}>{s.totalQuestions}</td>
                        <td style={{ padding: '8px 10px' }}>
                          {s.correctCount}/{s.totalQuestions}
                          {s.totalQuestions > 0 && (
                            <span style={{ color: 'var(--text-secondary)', marginLeft: 4, fontSize: 12 }}>
                              ({Math.round(s.correctCount / s.totalQuestions * 100)}%)
                            </span>
                          )}
                        </td>
                        <td style={{ padding: '8px 10px' }}>
                          <span style={{
                            padding: '2px 10px', borderRadius: 12, fontSize: 12, fontWeight: 600,
                            color: '#fff',
                            background: s.isCompleted ? '#22c55e' : '#f59e0b'
                          }}>
                            {s.isCompleted ? t.admin.activity.completed : t.admin.activity.inProgress}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>

                {/* Pagination */}
                {data.sessions.totalPages > 1 && (
                  <div style={{ display: 'flex', justifyContent: 'center', gap: 8, marginTop: 16 }}>
                    <button onClick={() => setPage(1)} disabled={page <= 1}
                      style={{ padding: '4px 10px', borderRadius: 6, border: '1px solid var(--border)', background: 'var(--bg-primary)', cursor: 'pointer', color: 'var(--text-primary)' }}>«</button>
                    <button onClick={() => setPage(p => p - 1)} disabled={page <= 1}
                      style={{ padding: '4px 10px', borderRadius: 6, border: '1px solid var(--border)', background: 'var(--bg-primary)', cursor: 'pointer', color: 'var(--text-primary)' }}>‹</button>
                    <span style={{ padding: '4px 12px', color: 'var(--text-secondary)' }}>
                      {page} / {data.sessions.totalPages}
                    </span>
                    <button onClick={() => setPage(p => p + 1)} disabled={page >= data.sessions.totalPages}
                      style={{ padding: '4px 10px', borderRadius: 6, border: '1px solid var(--border)', background: 'var(--bg-primary)', cursor: 'pointer', color: 'var(--text-primary)' }}>›</button>
                    <button onClick={() => setPage(data.sessions.totalPages)} disabled={page >= data.sessions.totalPages}
                      style={{ padding: '4px 10px', borderRadius: 6, border: '1px solid var(--border)', background: 'var(--bg-primary)', cursor: 'pointer', color: 'var(--text-primary)' }}>»</button>
                  </div>
                )}
              </>
            )}
          </div>
        </>
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
