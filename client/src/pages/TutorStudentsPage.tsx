import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { tutorService } from '../services/tutorService';
import type { StudentInfo } from '../types';

function TutorStudentsPage() {
  const navigate = useNavigate();
  const [students, setStudents] = useState<StudentInfo[]>([]);
  const [loading, setLoading] = useState(true);

  const loadStudents = useCallback(async () => {
    try {
      const data = await tutorService.getMyStudents();
      setStudents(data);
    } catch (err) {
      console.error('Failed to load students:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadStudents();
  }, [loadStudents]);

  const formatDate = (dateStr: string) => {
    const d = new Date(dateStr);
    return d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' });
  };

  const getInitials = (name: string) =>
    name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2);

  const timeSince = (dateStr: string | null) => {
    if (!dateStr) return 'нет сообщений';
    const d = new Date(dateStr);
    const now = new Date();
    const diff = Math.floor((now.getTime() - d.getTime()) / 1000);
    if (diff < 60) return 'только что';
    if (diff < 3600) return `${Math.floor(diff / 60)} мин. назад`;
    if (diff < 86400) return `${Math.floor(diff / 3600)} ч. назад`;
    if (diff < 604800) return `${Math.floor(diff / 86400)} дн. назад`;
    return formatDate(dateStr);
  };

  if (loading) {
    return <div className="animate-fade-in" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>Загрузка...</div>;
  }

  return (
    <div className="animate-fade-in">
      <h1 style={{ marginBottom: '1.5rem' }}>Мои ученики</h1>

      {students.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <div style={{ fontSize: '3rem', marginBottom: '0.5rem' }}></div>
          <h3 style={{ color: 'var(--text-secondary)' }}>Пока нет учеников</h3>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
            Когда студент отправит вам заявку и вы её примете, он появится здесь.
          </p>
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1rem' }}>
          {students.map(s => (
            <div
              key={s.userId}
              className="card"
              style={{ cursor: 'pointer', transition: 'transform 0.15s, box-shadow 0.15s' }}
              onMouseEnter={e => { (e.currentTarget as HTMLElement).style.transform = 'translateY(-2px)'; }}
              onMouseLeave={e => { (e.currentTarget as HTMLElement).style.transform = 'none'; }}
              onClick={() => navigate('/messages')}
            >
              <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center' }}>
                <div style={{
                  width: '48px', height: '48px', borderRadius: '50%',
                  background: 'linear-gradient(135deg, var(--primary-color), var(--primary-hover))',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  color: '#fff', fontWeight: 700, fontSize: '1rem', flexShrink: 0,
                }}>
                  {getInitials(s.name)}
                </div>
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: 600, fontSize: '1rem' }}>{s.name}</div>
                  <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>{s.email}</div>
                </div>
              </div>

              <div style={{ marginTop: '0.75rem', display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                <div>С {formatDate(s.conversationStartedAt)}</div>
                <div>{timeSince(s.lastMessageAt)}</div>
              </div>

              {s.lastMessagePreview && (
                <div style={{
                  marginTop: '0.5rem', fontSize: '0.82rem', color: 'var(--text-secondary)',
                  overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
                  fontStyle: 'italic',
                }}>
                  "{s.lastMessagePreview}"
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default TutorStudentsPage;
