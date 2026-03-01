import { useState, useEffect, useCallback } from 'react';
import adminService from '../services/adminService';

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

function AdminTutorsPage() {
  const [tutors, setTutors] = useState<TutorItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<'all' | 'verified' | 'unverified' | 'blocked'>('all');
  const [actionLoading, setActionLoading] = useState<number | null>(null);

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

  const handleVerify = async (t: TutorItem) => {
    setActionLoading(t.tutorProfileId);
    try {
      if (t.isVerified) {
        await adminService.unverifyTutor(t.tutorProfileId);
        setTutors(prev => prev.map(x => x.tutorProfileId === t.tutorProfileId ? { ...x, isVerified: false } : x));
      } else {
        await adminService.verifyTutor(t.tutorProfileId);
        setTutors(prev => prev.map(x => x.tutorProfileId === t.tutorProfileId ? { ...x, isVerified: true } : x));
      }
    } catch {
      alert('Не удалось изменить статус верификации');
    } finally {
      setActionLoading(null);
    }
  };

  const handleBlock = async (t: TutorItem) => {
    setActionLoading(t.tutorProfileId);
    try {
      if (t.isBlocked) {
        await adminService.unblockUser(t.userId);
        setTutors(prev => prev.map(x => x.tutorProfileId === t.tutorProfileId ? { ...x, isBlocked: false, blockReason: null } : x));
      } else {
        const reason = prompt('Причина блокировки (необязательно):');
        await adminService.blockUser(t.userId, reason ?? undefined);
        setTutors(prev => prev.map(x => x.tutorProfileId === t.tutorProfileId ? { ...x, isBlocked: true, blockReason: reason } : x));
      }
    } catch {
      alert('Не удалось изменить статус блокировки');
    } finally {
      setActionLoading(null);
    }
  };

  const filteredTutors = tutors.filter(t => {
    if (filter === 'verified') return t.isVerified;
    if (filter === 'unverified') return !t.isVerified;
    if (filter === 'blocked') return t.isBlocked;
    return true;
  });

  const formatDate = (d: string) => new Date(d).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', year: 'numeric' });

  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ fontSize: '1.5rem', fontWeight: 700 }}>Управление тьюторами</h1>
        <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
          Всего: {tutors.length} | Верифицированы: {tutors.filter(t => t.isVerified).length}
        </span>
      </div>

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
            {f === 'all' ? 'Все' : f === 'verified' ? 'Верифицированы' : f === 'unverified' ? 'Не верифицированы' : 'Заблокированы'}
            {' '}({tutors.filter(t => {
              if (f === 'verified') return t.isVerified;
              if (f === 'unverified') return !t.isVerified;
              if (f === 'blocked') return t.isBlocked;
              return true;
            }).length})
          </button>
        ))}
      </div>

      {loading ? (
        <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
      ) : filteredTutors.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
          Нет тьюторов в этой категории
        </div>
      ) : (
        <div style={{ display: 'grid', gap: '0.75rem' }}>
          {filteredTutors.map(t => (
            <div
              key={t.tutorProfileId}
              className="card"
              style={{
                padding: '1rem 1.25rem',
                display: 'flex', alignItems: 'center', gap: '1rem',
                borderLeft: t.isBlocked
                  ? '4px solid #ef4444'
                  : t.isVerified
                    ? '4px solid #22c55e'
                    : '4px solid #f59e0b',
              }}
            >
              {/* Avatar */}
              <div style={{
                width: '48px', height: '48px', borderRadius: '50%', flexShrink: 0,
                background: t.isBlocked
                  ? '#fca5a5'
                  : 'linear-gradient(135deg, var(--primary-color), var(--primary-hover))',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                color: '#fff', fontWeight: 700, fontSize: '1rem',
              }}>
                {t.name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2)}
              </div>

              {/* Info */}
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', flexWrap: 'wrap' }}>
                  <span style={{ fontWeight: 700, fontSize: '1rem' }}>{t.name}</span>
                  {t.isVerified && (
                    <span style={{
                      background: '#dcfce7', color: '#16a34a', padding: '0.1rem 0.5rem',
                      borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700,
                    }}>Верифицирован</span>
                  )}
                  {t.isBlocked && (
                    <span style={{
                      background: '#fef2f2', color: '#dc2626', padding: '0.1rem 0.5rem',
                      borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700,
                    }}>Заблокирован</span>
                  )}
                  {!t.isAvailable && (
                    <span style={{
                      background: '#fef9c3', color: '#ca8a04', padding: '0.1rem 0.5rem',
                      borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700,
                    }}>Недоступен</span>
                  )}
                </div>
                <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '0.15rem' }}>
                  {t.email} · {t.headline || 'Без заголовка'}
                </div>
                <div style={{ display: 'flex', gap: '1rem', marginTop: '0.3rem', fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
                  <span>★ {t.averageRating.toFixed(1)} ({t.totalReviews})</span>
                  <span>{t.totalStudents} учеников</span>
                  {t.hourlyRate && <span>{t.hourlyRate}₸/ч</span>}
                  <span>{formatDate(t.createdAt)}</span>
                  {t.specializations && <span>{t.specializations}</span>}
                </div>
                {t.isBlocked && t.blockReason && (
                  <div style={{ fontSize: '0.8rem', color: '#dc2626', marginTop: '0.25rem' }}>
                    Причина: {t.blockReason}
                  </div>
                )}
              </div>

              {/* Actions */}
              <div style={{ display: 'flex', gap: '0.5rem', flexShrink: 0 }}>
                <button
                  onClick={() => handleVerify(t)}
                  disabled={actionLoading === t.tutorProfileId}
                  className="btn"
                  style={{
                    padding: '0.4rem 0.75rem', fontSize: '0.82rem',
                    background: t.isVerified ? '#fef2f2' : '#dcfce7',
                    color: t.isVerified ? '#dc2626' : '#16a34a',
                    border: 'none',
                  }}
                  title={t.isVerified ? 'Снять верификацию' : 'Верифицировать'}
                >
                  {actionLoading === t.tutorProfileId ? '...' : t.isVerified ? 'Снять ✓' : 'Верифицировать'}
                </button>
                <button
                  onClick={() => handleBlock(t)}
                  disabled={actionLoading === t.tutorProfileId}
                  className="btn"
                  style={{
                    padding: '0.4rem 0.75rem', fontSize: '0.82rem',
                    background: t.isBlocked ? '#dcfce7' : '#fef2f2',
                    color: t.isBlocked ? '#16a34a' : '#dc2626',
                    border: 'none',
                  }}
                  title={t.isBlocked ? 'Разблокировать' : 'Заблокировать'}
                >
                  {actionLoading === t.tutorProfileId ? '...' : t.isBlocked ? 'Разблокировать' : 'Заблокировать'}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default AdminTutorsPage;
