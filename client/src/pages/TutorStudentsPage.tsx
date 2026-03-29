import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { tutorService } from '../services/tutorService';
import { useTranslation } from '../hooks/useTranslation';
import type { StudentInfo, TutorStudentInfo } from '../types';
import { getDateLocale } from '../i18n';

function TutorStudentsPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [students, setStudents] = useState<StudentInfo[]>([]);
  const [linkedStudents, setLinkedStudents] = useState<TutorStudentInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [unlinkingId, setUnlinkingId] = useState<number | null>(null);

  const loadData = useCallback(async () => {
    try {
      const [studentsData, linked] = await Promise.all([
        tutorService.getMyStudents(),
        tutorService.getLinkedStudents().catch(() => []),
      ]);
      setStudents(studentsData);
      setLinkedStudents(linked);
    } catch (err) {
      console.error('Failed to load students:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleUnlinkStudent = async (studentUserId: number) => {
    if (!confirm(t.tutor.confirmUnlinkStudent)) return;
    setUnlinkingId(studentUserId);
    try {
      await tutorService.unlinkStudent(studentUserId);
      setLinkedStudents(prev => prev.filter(s => s.studentUserId !== studentUserId));
    } catch {
      alert('Ошибка отвязки');
    } finally {
      setUnlinkingId(null);
    }
  };

  const formatDate = (dateStr: string) => {
    const d = new Date(dateStr);
    return d.toLocaleDateString(getDateLocale(), { day: 'numeric', month: 'long', year: 'numeric' });
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
      <h1 style={{ marginBottom: '1.5rem' }}>{t.tutor.myStudents}</h1>

      {/* ─── Linked Students (via invite code) ─── */}
      {linkedStudents.length > 0 && (
        <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
          <h2 style={{ margin: '0 0 1rem', fontSize: '1.15rem' }}>
            {t.tutor.linkedStudents}
            <span style={{
              background: 'var(--primary-color)', color: '#fff', borderRadius: '999px',
              padding: '0.15rem 0.5rem', fontSize: '0.75rem', fontWeight: 700, marginLeft: '0.5rem',
            }}>{linkedStudents.length}</span>
          </h2>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {linkedStudents.map(s => (
              <div
                key={s.id}
                style={{
                  padding: '1rem', borderRadius: '10px',
                  background: 'var(--bg-secondary)', border: '1px solid var(--border-color)',
                  display: 'flex', alignItems: 'center', gap: '0.75rem', flexWrap: 'wrap',
                }}
              >
                <div style={{
                  width: '42px', height: '42px', borderRadius: '50%',
                  background: 'linear-gradient(135deg, #10b981, #059669)',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  color: '#fff', fontWeight: 700, fontSize: '0.85rem', flexShrink: 0,
                }}>
                  {getInitials(s.studentName)}
                </div>
                <div style={{ flex: 1, minWidth: '150px' }}>
                  <div style={{ fontWeight: 600 }}>{s.studentName}</div>
                  <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                    {s.studentEmail} · {t.tutor.linkedSince} {formatDate(s.linkedAt)}
                  </div>
                </div>
                <button
                  className="btn btn-outline"
                  onClick={() => handleUnlinkStudent(s.studentUserId)}
                  disabled={unlinkingId === s.studentUserId}
                  style={{ padding: '0.35rem 0.75rem', fontSize: '0.8rem', color: '#ef4444', borderColor: '#ef4444' }}
                >
                  {t.tutor.unlinkStudent}
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* ─── Conversation-based Students ─── */}
      {students.length === 0 && linkedStudents.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <div style={{ fontSize: '3rem', marginBottom: '0.5rem' }}></div>
          <h3 style={{ color: 'var(--text-secondary)' }}>{t.tutor.noLinkedStudents}</h3>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
            {t.tutor.noLinkedStudentsDesc}
          </p>
        </div>
      ) : students.length > 0 && (
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
