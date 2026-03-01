import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { tutorService } from '../services/tutorService';
import { messageService } from '../services/messageService';
import type { TutorProfileDetail, PendingRequest } from '../types';

function TutorHomePage() {
  const navigate = useNavigate();
  const [profile, setProfile] = useState<TutorProfileDetail | null>(null);
  const [pending, setPending] = useState<PendingRequest[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<number | null>(null);

  const loadData = useCallback(async () => {
    try {
      const [pendingReqs, unread] = await Promise.all([
        tutorService.getPendingRequests(),
        messageService.getUnreadCount(),
      ]);
      setPending(pendingReqs);
      setUnreadCount(unread);

      // Load own profile — get userId from first pending or from profile endpoint
      try {
        // We'll try to load own tutor profile via the current user's ID
        // Since we don't have it directly, let's just show fallback on failure
      } catch { /* ignore */ }
    } catch (err) {
      console.error('Failed to load dashboard data:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  // Load tutor profile separately — need current user ID
  useEffect(() => {
    const loadProfile = async () => {
      try {
        const userStr = localStorage.getItem('user');
        if (userStr) {
          const user = JSON.parse(userStr);
          const data = await tutorService.getTutorProfile(user.id);
          setProfile(data);
        }
      } catch { /* ignore — profile might not exist yet */ }
    };
    loadProfile();
    loadData();
  }, [loadData]);

  const handleAccept = async (conversationId: number) => {
    setActionLoading(conversationId);
    try {
      await tutorService.acceptStudent(conversationId);
      setPending(prev => prev.filter(p => p.conversationId !== conversationId));
    } catch {
      alert('Не удалось принять заявку');
    } finally {
      setActionLoading(null);
    }
  };

  const handleDecline = async (conversationId: number) => {
    const reason = prompt('Причина отклонения (необязательно):');
    setActionLoading(conversationId);
    try {
      await tutorService.declineStudent(conversationId, reason || undefined);
      setPending(prev => prev.filter(p => p.conversationId !== conversationId));
    } catch {
      alert('Не удалось отклонить заявку');
    } finally {
      setActionLoading(null);
    }
  };

  const formatDate = (dateStr: string) => {
    const d = new Date(dateStr);
    return d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });
  };

  if (loading) {
    return <div className="animate-fade-in" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>Загрузка...</div>;
  }

  return (
    <div className="animate-fade-in">
      <h1 style={{ marginBottom: '1.5rem' }}>Главная</h1>

      {/* Stat cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '1rem', marginBottom: '1.5rem' }}>
        {[
          { label: 'Новые заявки', value: pending.length, icon: '', color: '#f59e0b', onClick: () => {} },
          { label: 'Непрочитанные', value: unreadCount, icon: '', color: '#3b82f6', onClick: () => navigate('/messages') },
          { label: 'Рейтинг', value: profile ? profile.averageRating.toFixed(1) : '—', icon: '', color: '#10b981', onClick: () => navigate('/reviews') },
          { label: 'Учеников', value: profile?.totalStudents ?? 0, icon: '', color: '#8b5cf6', onClick: () => navigate('/students') },
        ].map((stat, i) => (
          <div
            key={i}
            className="card"
            onClick={stat.onClick}
            style={{ cursor: 'pointer', textAlign: 'center', padding: '1.25rem' }}
          >
            <div style={{ fontSize: '2rem', marginBottom: '0.3rem' }}>{stat.icon}</div>
            <div style={{ fontSize: '1.5rem', fontWeight: 700, color: stat.color }}>{stat.value}</div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{stat.label}</div>
          </div>
        ))}
      </div>

      {/* Pending requests */}
      <div className="card" style={{ marginBottom: '1rem' }}>
        <h2 style={{ margin: '0 0 1rem', fontSize: '1.15rem' }}>
          Новые заявки {pending.length > 0 && <span style={{
            background: '#f59e0b', color: '#fff', borderRadius: '999px',
            padding: '0.15rem 0.5rem', fontSize: '0.75rem', fontWeight: 700, marginLeft: '0.5rem',
          }}>{pending.length}</span>}
        </h2>

        {pending.length === 0 ? (
          <p style={{ color: 'var(--text-secondary)', margin: 0 }}>Нет новых заявок</p>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {pending.map(req => (
              <div
                key={req.conversationId}
                style={{
                  padding: '1rem', borderRadius: '10px',
                  background: 'var(--bg-secondary)', border: '1px solid var(--border-color)',
                  display: 'flex', flexWrap: 'wrap', gap: '0.75rem', alignItems: 'center',
                }}
              >
                <div style={{
                  width: '42px', height: '42px', borderRadius: '50%',
                  background: 'linear-gradient(135deg, #f59e0b, #d97706)',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  color: '#fff', fontWeight: 700, fontSize: '0.85rem', flexShrink: 0,
                }}>
                  {req.studentName.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2)}
                </div>

                <div style={{ flex: 1, minWidth: '150px' }}>
                  <div style={{ fontWeight: 600 }}>{req.studentName}</div>
                  <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                    {req.studentEmail} · {formatDate(req.requestedAt)}
                  </div>
                  {req.requestMessage && (
                    <div style={{
                      marginTop: '0.3rem', fontSize: '0.85rem',
                      color: 'var(--text-primary)', fontStyle: 'italic',
                      background: 'var(--bg-primary)', padding: '0.4rem 0.6rem',
                      borderRadius: '6px', borderLeft: '3px solid var(--primary-color)',
                    }}>
                      "{req.requestMessage}"
                    </div>
                  )}
                </div>

                <div style={{ display: 'flex', gap: '0.5rem', flexShrink: 0 }}>
                  <button
                    className="btn btn-primary"
                    onClick={() => handleAccept(req.conversationId)}
                    disabled={actionLoading === req.conversationId}
                    style={{ padding: '0.4rem 1rem', fontSize: '0.85rem' }}
                  >
                    Принять
                  </button>
                  <button
                    className="btn btn-outline"
                    onClick={() => handleDecline(req.conversationId)}
                    disabled={actionLoading === req.conversationId}
                    style={{ padding: '0.4rem 1rem', fontSize: '0.85rem' }}
                  >
                    Отклонить
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Quick info */}
      {profile && (
        <div className="card">
          <h2 style={{ margin: '0 0 0.75rem', fontSize: '1.15rem' }}>Мой профиль</h2>
          <div style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap', fontSize: '0.9rem' }}>
            <div><strong>Заголовок:</strong> {profile.headline}</div>
            <div><strong>Статус:</strong> {profile.isAvailable ? '● Доступен' : '○ Недоступен'}</div>
            {profile.isVerified && <div>✓ Верифицирован</div>}
          </div>
          <button
            className="btn btn-outline"
            onClick={() => navigate('/my-profile')}
            style={{ marginTop: '0.75rem', fontSize: '0.85rem' }}
          >
            Редактировать профиль
          </button>
        </div>
      )}
    </div>
  );
}

export default TutorHomePage;
