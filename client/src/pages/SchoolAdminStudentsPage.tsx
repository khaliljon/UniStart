import { useState, useEffect, useCallback } from 'react';
import { schoolAdminService } from '../services/schoolAdminService';
import { useTranslation } from '../i18n';
import type { SchoolStudent, StudentAnalytics } from '../services/schoolAdminService';

function SchoolAdminStudentsPage() {
  const { t } = useTranslation();
  const [students, setStudents] = useState<SchoolStudent[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [selectedStudent, setSelectedStudent] = useState<StudentAnalytics | null>(null);

  const loadStudents = useCallback(async (p = 1) => {
    setLoading(true);
    try {
      const data = await schoolAdminService.getStudents(p);
      setStudents(data.items);
      setTotal(data.total);
      setPage(p);
    } catch { /* */ } finally { setLoading(false); }
  }, []);

  useEffect(() => { loadStudents(); }, [loadStudents]);

  const openDetail = async (userId: number) => {
    setLoading(true);
    try {
      setSelectedStudent(await schoolAdminService.getStudentAnalytics(userId));
    } catch { /* */ } finally { setLoading(false); }
  };

  if (selectedStudent) {
    return (
      <div>
        <button className="btn btn-outline" onClick={() => setSelectedStudent(null)} style={{ marginBottom: '1rem' }}>
          &larr; {t.schoolAdmin.back}
        </button>
        <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
          <h2 style={{ margin: '0 0 0.5rem' }}>{selectedStudent.student.name}</h2>
          <div style={{ fontSize: '0.9rem', color: 'var(--text-secondary)' }}>
            {selectedStudent.student.email} — {t.schoolAdmin.joined} {new Date(selectedStudent.student.createdAt).toLocaleDateString()}
            {selectedStudent.student.lastSeenAt && ` — ${t.schoolAdmin.lastOnline} ${timeAgo(selectedStudent.student.lastSeenAt)}`}
          </div>
        </div>
        {selectedStudent.skills.length > 0 && (
          <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
            <h3 style={{ margin: '0 0 1rem' }}>{t.schoolAdmin.skills}</h3>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: '0.75rem' }}>
              {selectedStudent.skills.map(sk => (
                <div key={sk.name} style={{ display: 'flex', justifyContent: 'space-between', padding: '0.5rem', borderRadius: '8px', background: 'var(--background-secondary)' }}>
                  <span style={{ fontSize: '0.85rem' }}>{sk.name}</span>
                  <span style={{ fontWeight: 600, color: 'var(--primary-color)' }}>{sk.proficiencyLevel.toFixed(1)}</span>
                </div>
              ))}
            </div>
          </div>
        )}
        <div className="card" style={{ padding: '1.5rem', overflowX: 'auto' }}>
          <h3 style={{ margin: '0 0 1rem' }}>{t.schoolAdmin.recentSessions}</h3>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.88rem' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--border-color)' }}>
                <th style={th}>{t.schoolAdmin.date}</th><th style={th}>{t.schoolAdmin.questions}</th><th style={th}>{t.schoolAdmin.correct}</th><th style={th}>{t.schoolAdmin.accuracy}</th>
              </tr>
            </thead>
            <tbody>
              {selectedStudent.recentSessions.map(s => (
                <tr key={s.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                  <td style={td}>{s.completedAt ? new Date(s.completedAt).toLocaleDateString() : '—'}</td>
                  <td style={td}>{s.totalQuestions}</td>
                  <td style={td}>{s.correctAnswers}</td>
                  <td style={td}><span style={{ fontWeight: 600, color: s.accuracy >= 70 ? 'var(--success-color)' : 'var(--error-color)' }}>{s.accuracy.toFixed(0)}%</span></td>
                </tr>
              ))}
              {selectedStudent.recentSessions.length === 0 && (
                <tr><td colSpan={4} style={{ ...td, textAlign: 'center', color: 'var(--text-secondary)' }}>{t.schoolAdmin.noSessions}</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    );
  }

  return (
    <div>
      <h1 style={{ marginBottom: '1.5rem' }}>{t.schoolAdmin.students}</h1>
      <div className="card" style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.88rem' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid var(--border-color)' }}>
              <th style={th}>{t.schoolAdmin.name}</th><th style={th}>{t.schoolAdmin.email}</th><th style={th}>{t.schoolAdmin.plan}</th><th style={th}>{t.schoolAdmin.joined}</th><th style={th}>{t.schoolAdmin.lastOnline}</th><th style={th}>{t.schoolAdmin.tutor}</th>
            </tr>
          </thead>
          <tbody>
            {students.map(s => (
              <tr key={s.userId} onClick={() => openDetail(s.userId)}
                style={{ cursor: 'pointer', borderBottom: '1px solid var(--border-color)' }}>
                <td style={td}>{s.name}</td>
                <td style={td}>{s.email}</td>
                <td style={td}><span style={badge(s.subscriptionTier)}>{s.subscriptionTier}</span></td>
                <td style={td}>{new Date(s.createdAt).toLocaleDateString()}</td>
                <td style={td}>{s.lastSeenAt ? timeAgo(s.lastSeenAt) : '—'}</td>
                <td style={td}>{s.linkedTutorName || '—'}</td>
              </tr>
            ))}
            {students.length === 0 && !loading && (
              <tr><td colSpan={6} style={{ ...td, textAlign: 'center', color: 'var(--text-secondary)' }}>{t.schoolAdmin.noStudents}</td></tr>
            )}
          </tbody>
        </table>
      </div>
      {total > 20 && (
        <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginTop: '1rem' }}>
          <button className="btn btn-outline" disabled={page <= 1} onClick={() => loadStudents(page - 1)}>&larr;</button>
          <span style={{ padding: '0.5rem' }}>{page} / {Math.ceil(total / 20)}</span>
          <button className="btn btn-outline" disabled={page * 20 >= total} onClick={() => loadStudents(page + 1)}>&rarr;</button>
        </div>
      )}
    </div>
  );
}

function timeAgo(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 60) return `${mins}m`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h`;
  return `${Math.floor(hours / 24)}d`;
}

const th: React.CSSProperties = { padding: '0.5rem 0.75rem', textAlign: 'left', fontWeight: 600 };
const td: React.CSSProperties = { padding: '0.5rem 0.75rem' };
function badge(tier: string): React.CSSProperties {
  return {
    padding: '0.15rem 0.5rem', borderRadius: '999px', fontSize: '0.72rem', fontWeight: 600,
    background: tier === 'Pro' ? 'var(--primary-color)' : 'var(--background-secondary)',
    color: tier === 'Pro' ? '#fff' : 'var(--text-secondary)',
  };
}

export default SchoolAdminStudentsPage;
