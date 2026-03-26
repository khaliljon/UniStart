import { useState, useEffect, useCallback } from 'react';
import adminService from '../services/adminService';
import { useTranslation } from '../hooks/useTranslation';
import { getDateLocale } from '../i18n';

interface TutorItem {
  tutorProfileId: number;
  userId: number;
  name: string;
  email: string;
  headline: string;
  specializations: string;
  isAvailable: boolean;
  isVerified: boolean;
  isBlocked: boolean;
  blockReason: string | null;
  averageRating: number;
  totalReviews: number;
  totalStudents: number;
  hourlyRate: number | null;
  createdAt: string;
}

interface ContentAssignment {
  id: number; title: string; description: string | null; deadline: string | null;
  isActive: boolean; createdAt: string; tutorName: string; tutorUserId: number;
  questionCount: number; studentCount: number; completedCount: number;
}

interface ContentQuestion {
  id: number; text: string; difficulty: string; isPrivate: boolean; createdAt: string;
  tutorName: string; tutorUserId: number; topicName: string; examTypeCode: string;
}

function AdminTutorsPage() {
  const { t } = useTranslation();
  const [tab, setTab] = useState<'tutors' | 'content'>('tutors');
  const [tutors, setTutors] = useState<TutorItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<'all' | 'verified' | 'unverified' | 'blocked'>('all');
  const [actionLoading, setActionLoading] = useState<number | null>(null);

  // Content state
  const [contentAssignments, setContentAssignments] = useState<ContentAssignment[]>([]);
  const [contentQuestions, setContentQuestions] = useState<ContentQuestion[]>([]);
  const [contentLoading, setContentLoading] = useState(false);
  const [contentFilter, setContentFilter] = useState<number | undefined>();

  const loadTutors = useCallback(async () => {
    try {
      const data = await adminService.getTutors();
      setTutors(data);
    } catch (err) {
      console.error('Failed to load tutors:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadTutors(); }, [loadTutors]);

  const loadContent = useCallback(async () => {
    setContentLoading(true);
    try {
      const data = await adminService.getTutorContent({ tutorId: contentFilter, pageSize: 50 });
      setContentAssignments(data.assignments.items);
      setContentQuestions(data.questions.items);
    } catch { /* */ } finally { setContentLoading(false); }
  }, [contentFilter]);

  useEffect(() => { if (tab === 'content') loadContent(); }, [tab, loadContent]);

  const handleVerify = async (tutor: TutorItem) => {
    setActionLoading(tutor.tutorProfileId);
    try {
      if (tutor.isVerified) {
        await adminService.unverifyTutor(tutor.tutorProfileId);
        setTutors(prev => prev.map(x => x.tutorProfileId === tutor.tutorProfileId ? { ...x, isVerified: false } : x));
      } else {
        await adminService.verifyTutor(tutor.tutorProfileId);
        setTutors(prev => prev.map(x => x.tutorProfileId === tutor.tutorProfileId ? { ...x, isVerified: true } : x));
      }
    } catch {
      alert(t.admin.tutors.verifyError);
    } finally {
      setActionLoading(null);
    }
  };

  const handleBlock = async (tutor: TutorItem) => {
    setActionLoading(tutor.tutorProfileId);
    try {
      if (tutor.isBlocked) {
        await adminService.unblockUser(tutor.userId);
        setTutors(prev => prev.map(x => x.tutorProfileId === tutor.tutorProfileId ? { ...x, isBlocked: false, blockReason: null } : x));
      } else {
        const reason = prompt(t.admin.tutors.blockReasonPrompt);
        await adminService.blockUser(tutor.userId, reason ?? undefined);
        setTutors(prev => prev.map(x => x.tutorProfileId === tutor.tutorProfileId ? { ...x, isBlocked: true, blockReason: reason } : x));
      }
    } catch {
      alert(t.admin.tutors.blockError);
    } finally {
      setActionLoading(null);
    }
  };

  const filteredTutors = tutors.filter(tutor => {
    if (filter === 'verified') return tutor.isVerified;
    if (filter === 'unverified') return !tutor.isVerified;
    if (filter === 'blocked') return tutor.isBlocked;
    return true;
  });

  const formatDate = (d: string) => new Date(d).toLocaleDateString(getDateLocale(), { day: 'numeric', month: 'short', year: 'numeric' });

  const tabStyle = (active: boolean): React.CSSProperties => ({
    padding: '0.5rem 1.25rem', cursor: 'pointer', fontSize: '0.9rem', fontWeight: 600,
    borderBottom: active ? '2px solid var(--primary-color)' : '2px solid transparent',
    color: active ? 'var(--primary-color)' : 'var(--text-secondary)',
    background: 'none', border: 'none', borderBottomWidth: '2px', borderBottomStyle: 'solid',
  });

  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h1 style={{ fontSize: '1.5rem', fontWeight: 700 }}>{t.admin.tutors.title}</h1>
        <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
          {t.admin.tutors.totalLabel} {tutors.length} | {t.admin.tutors.verifiedLabel} {tutors.filter(tutor => tutor.isVerified).length}
        </span>
      </div>

      <div style={{ display: 'flex', gap: '0.25rem', borderBottom: '1px solid var(--border-color)', marginBottom: '1rem' }}>
        <button style={tabStyle(tab === 'tutors')} onClick={() => setTab('tutors')}>
          {t.admin.tutors.title}
        </button>
        <button style={tabStyle(tab === 'content')} onClick={() => setTab('content')}>
          Контент тьюторов
        </button>
      </div>

      {tab === 'tutors' && (<>
      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
        {(['all', 'verified', 'unverified', 'blocked'] as const).map(f => (
          <button
            key={f}
            onClick={() => setFilter(f)}
            className="btn"
            style={{
              padding: '0.4rem 0.8rem', fontSize: '0.85rem',
              background: filter === f ? 'var(--primary-color)' : undefined,
              color: filter === f ? '#fff' : undefined,
            }}
          >
            {f === 'all' ? t.admin.tutors.filterAll : f === 'verified' ? t.admin.tutors.filterVerified : f === 'unverified' ? t.admin.tutors.filterUnverified : t.admin.tutors.filterBlocked}
            {' '}({tutors.filter(tutor => {
              if (f === 'verified') return tutor.isVerified;
              if (f === 'unverified') return !tutor.isVerified;
              if (f === 'blocked') return tutor.isBlocked;
              return true;
            }).length})
          </button>
        ))}
      </div>

      {loading ? (
        <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>{t.admin.common.loading}</div>
      ) : filteredTutors.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
          {t.admin.tutors.noTutors}
        </div>
      ) : (
        <div style={{ display: 'grid', gap: '0.75rem' }}>
          {filteredTutors.map(tutor => (
            <div
              key={tutor.tutorProfileId}
              className="card"
              style={{
                padding: '1rem 1.25rem',
                display: 'flex', alignItems: 'center', gap: '1rem',
                borderLeft: tutor.isBlocked
                  ? '4px solid #ef4444'
                  : tutor.isVerified
                    ? '4px solid #22c55e'
                    : '4px solid #f59e0b',
              }}
            >
              {/* Avatar */}
              <div style={{
                width: '48px', height: '48px', borderRadius: '50%', flexShrink: 0,
                background: tutor.isBlocked
                  ? '#fca5a5'
                  : 'linear-gradient(135deg, var(--primary-color), var(--primary-hover))',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                color: '#fff', fontWeight: 700, fontSize: '1rem',
              }}>
                {tutor.name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2)}
              </div>

              {/* Info */}
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', flexWrap: 'wrap' }}>
                  <span style={{ fontWeight: 700, fontSize: '1rem' }}>{tutor.name}</span>
                  {tutor.isVerified && (
                    <span style={{
                      background: '#dcfce7', color: '#16a34a', padding: '0.1rem 0.5rem',
                      borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700,
                    }}>{t.admin.tutors.verified}</span>
                  )}
                  {tutor.isBlocked && (
                    <span style={{
                      background: '#fef2f2', color: '#dc2626', padding: '0.1rem 0.5rem',
                      borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700,
                    }}>{t.admin.tutors.blocked}</span>
                  )}
                  {!tutor.isAvailable && (
                    <span style={{
                      background: '#fef9c3', color: '#ca8a04', padding: '0.1rem 0.5rem',
                      borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700,
                    }}>{t.admin.tutors.unavailable}</span>
                  )}
                </div>
                <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '0.15rem' }}>
                  {tutor.email} · {tutor.headline || t.admin.tutors.noHeadline}
                </div>
                <div style={{ display: 'flex', gap: '1rem', marginTop: '0.3rem', fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
                  <span>★ {tutor.averageRating.toFixed(1)} ({tutor.totalReviews})</span>
                  <span>{tutor.totalStudents} {t.admin.tutors.studentsCount}</span>
                  {tutor.hourlyRate && <span>{tutor.hourlyRate}₸/ч</span>}
                  <span>{formatDate(tutor.createdAt)}</span>
                  {tutor.specializations && <span>{tutor.specializations}</span>}
                </div>
                {tutor.isBlocked && tutor.blockReason && (
                  <div style={{ fontSize: '0.8rem', color: '#dc2626', marginTop: '0.25rem' }}>
                    {t.admin.tutors.reason} {tutor.blockReason}
                  </div>
                )}
              </div>

              {/* Actions */}
              <div style={{ display: 'flex', gap: '0.5rem', flexShrink: 0 }}>
                <button
                  onClick={() => handleVerify(tutor)}
                  disabled={actionLoading === tutor.tutorProfileId}
                  className="btn"
                  style={{
                    padding: '0.4rem 0.75rem', fontSize: '0.82rem',
                    background: tutor.isVerified ? '#fef2f2' : '#dcfce7',
                    color: tutor.isVerified ? '#dc2626' : '#16a34a',
                    border: 'none',
                  }}
                  title={tutor.isVerified ? t.admin.tutors.unverify : t.admin.tutors.verify}
                >
                  {actionLoading === tutor.tutorProfileId ? '...' : tutor.isVerified ? t.admin.tutors.unverifyShort : t.admin.tutors.verify}
                </button>
                <button
                  onClick={() => handleBlock(tutor)}
                  disabled={actionLoading === tutor.tutorProfileId}
                  className="btn"
                  style={{
                    padding: '0.4rem 0.75rem', fontSize: '0.82rem',
                    background: tutor.isBlocked ? '#dcfce7' : '#fef2f2',
                    color: tutor.isBlocked ? '#16a34a' : '#dc2626',
                    border: 'none',
                  }}
                  title={tutor.isBlocked ? t.admin.tutors.unblock : t.admin.tutors.block}
                >
                  {actionLoading === tutor.tutorProfileId ? '...' : tutor.isBlocked ? t.admin.tutors.unblock : t.admin.tutors.block}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
      </>)}

      {tab === 'content' && (
        <div>
          {tutors.length > 0 && (
            <div style={{ marginBottom: '1rem' }}>
              <select className="form-input" style={{ maxWidth: '300px' }} value={contentFilter ?? ''} onChange={e => setContentFilter(e.target.value ? Number(e.target.value) : undefined)}>
                <option value="">Все тьюторы</option>
                {tutors.map(tr => <option key={tr.userId} value={tr.userId}>{tr.name}</option>)}
              </select>
            </div>
          )}

          {contentLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
              <div>
                <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Задания ({contentAssignments.length})</h3>
                {contentAssignments.length === 0 ? (
                  <div className="card" style={{ padding: '1.5rem', textAlign: 'center', color: 'var(--text-secondary)' }}>Заданий пока нет</div>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                    {contentAssignments.map(a => (
                      <div key={a.id} className="card" style={{ padding: '1rem' }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                          <div>
                            <strong>{a.title}</strong>
                            <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginLeft: '0.5rem' }}>{a.tutorName}</span>
                          </div>
                          <span style={{ fontSize: '0.72rem', color: a.isActive ? 'var(--success-color)' : 'var(--text-secondary)' }}>
                            {a.isActive ? 'Активно' : 'Неактивно'}
                          </span>
                        </div>
                        {a.description && <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '0.35rem' }}>{a.description}</div>}
                        <div style={{ display: 'flex', gap: '1rem', marginTop: '0.5rem', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                          <span>Вопросов: {a.questionCount}</span>
                          <span>Учеников: {a.studentCount}</span>
                          <span>Выполнили: {a.completedCount}</span>
                          {a.deadline && <span>Дедлайн: {new Date(a.deadline).toLocaleDateString()}</span>}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <div>
                <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Вопросы ({contentQuestions.length})</h3>
                {contentQuestions.length === 0 ? (
                  <div className="card" style={{ padding: '1.5rem', textAlign: 'center', color: 'var(--text-secondary)' }}>Вопросов пока нет</div>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                    {contentQuestions.map(q => (
                      <div key={q.id} className="card" style={{ padding: '1rem' }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                          <div style={{ flex: 1, minWidth: 0 }}>
                            <div style={{ fontWeight: 600, fontSize: '0.9rem' }}>{q.text.slice(0, 120)}{q.text.length > 120 ? '...' : ''}</div>
                            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                              {q.tutorName} · {q.topicName} · {q.examTypeCode} · {q.difficulty}
                            </div>
                          </div>
                          {q.isPrivate && (
                            <span style={{ fontSize: '0.7rem', padding: '0.15rem 0.4rem', borderRadius: '4px', background: 'var(--bg-secondary)', color: 'var(--text-secondary)', whiteSpace: 'nowrap' }}>
                              Приватный
                            </span>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default AdminTutorsPage;
