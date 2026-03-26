import { useState, useEffect, useCallback } from 'react';
import { schoolAdminService } from '../services/schoolAdminService';
import api from '../services/api';
import type { SchoolDashboard, SchoolStudent, SchoolTutor, StudentAnalytics } from '../services/schoolAdminService';

type TutorApp = { id: number; userId: number; userName: string; userEmail: string; status: string; message: string | null; createdAt: string; reviewedAt: string | null };
type View = 'dashboard' | 'students' | 'tutors' | 'student-detail' | 'tutor-applications';

function SchoolAdminDashboardPage() {
  const [view, setView] = useState<View>('dashboard');
  const [dashboard, setDashboard] = useState<SchoolDashboard | null>(null);
  const [students, setStudents] = useState<SchoolStudent[]>([]);
  const [tutors, setTutors] = useState<SchoolTutor[]>([]);
  const [studentsTotal, setStudentsTotal] = useState(0);
  const [studentsPage, setStudentsPage] = useState(1);
  const [selectedStudent, setSelectedStudent] = useState<StudentAnalytics | null>(null);
  const [loading, setLoading] = useState(true);
  const [tutorApps, setTutorApps] = useState<TutorApp[]>([]);
  const [pendingCount, setPendingCount] = useState(0);

  useEffect(() => {
    schoolAdminService.getDashboard()
      .then(setDashboard)
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const loadStudents = useCallback(async (page = 1) => {
    setLoading(true);
    try {
      const data = await schoolAdminService.getStudents(page);
      setStudents(data.items);
      setStudentsTotal(data.total);
      setStudentsPage(page);
    } catch { /* */ } finally { setLoading(false); }
  }, []);

  const loadTutors = useCallback(async () => {
    setLoading(true);
    try {
      setTutors(await schoolAdminService.getTutors());
    } catch { /* */ } finally { setLoading(false); }
  }, []);

  const loadTutorApps = useCallback(async () => {
    setLoading(true);
    try {
      const { data } = await api.get<TutorApp[]>('/tutor-school-applications/school');
      setTutorApps(data);
      setPendingCount(data.filter(a => a.status === 'Pending').length);
    } catch { /* */ } finally { setLoading(false); }
  }, []);

  // Load pending count on mount
  useEffect(() => {
    api.get<TutorApp[]>('/tutor-school-applications/school?status=Pending')
      .then(({ data }) => setPendingCount(data.length))
      .catch(() => {});
  }, []);

  const openStudentDetail = async (userId: number) => {
    setLoading(true);
    try {
      const data = await schoolAdminService.getStudentAnalytics(userId);
      setSelectedStudent(data);
      setView('student-detail');
    } catch { /* */ } finally { setLoading(false); }
  };

  const navTo = (v: View) => {
    setView(v);
    if (v === 'students') loadStudents();
    if (v === 'tutors') loadTutors();
    if (v === 'tutor-applications') loadTutorApps();
  };

  if (loading && !dashboard) return <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>Loading...</div>;
  if (!dashboard) return <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>No school found. Contact admin to link your account.</div>;

  return (
    <div>
      {/* Title */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>{dashboard.schoolName}</h1>
      </div>

      {/* Tab Nav */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        {(['dashboard', 'students', 'tutors'] as View[]).map(v => (
          <button key={v} className={view === v ? 'btn btn-primary' : 'btn btn-outline'}
            onClick={() => navTo(v)} style={{ textTransform: 'capitalize' }}>
            {v === 'dashboard' ? 'Dashboard' : v === 'students' ? `Students (${dashboard.totalStudents})` : `Tutors (${dashboard.totalTutors})`}
          </button>
        ))}
        <button
          className={view === 'tutor-applications' ? 'btn btn-primary' : 'btn btn-outline'}
          onClick={() => navTo('tutor-applications')}
          style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}
        >
          Applications{pendingCount > 0 && (
            <span style={{
              background: '#ef4444', color: '#fff', borderRadius: '999px',
              padding: '0.1rem 0.45rem', fontSize: '0.7rem', fontWeight: 700,
            }}>{pendingCount}</span>
          )}
        </button>
        {view === 'student-detail' && (
          <button className="btn btn-outline" onClick={() => navTo('students')}>← Back</button>
        )}
      </div>

      {/* Dashboard view */}
      {view === 'dashboard' && (
        <div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem', marginBottom: '2rem' }}>
            <StatCard label="Students" value={dashboard.totalStudents} />
            <StatCard label="Tutors" value={dashboard.totalTutors} />
            <StatCard label="Active (7d)" value={dashboard.activeStudentsLast7Days} />
            <StatCard label="Avg Accuracy" value={`${dashboard.averageAccuracy}%`} />
          </div>

          <h3>Recent Students</h3>
          <div className="card" style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.88rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid var(--border-color)' }}>
                  <th style={th}>Name</th><th style={th}>Email</th><th style={th}>Plan</th><th style={th}>Joined</th><th style={th}>Last Online</th>
                </tr>
              </thead>
              <tbody>
                {dashboard.recentStudents.map(s => (
                  <tr key={s.userId} onClick={() => openStudentDetail(s.userId)}
                    style={{ cursor: 'pointer', borderBottom: '1px solid var(--border-color)' }}>
                    <td style={td}>{s.name}</td>
                    <td style={td}>{s.email}</td>
                    <td style={td}><span style={badge(s.subscriptionTier)}>{s.subscriptionTier}</span></td>
                    <td style={td}>{new Date(s.createdAt).toLocaleDateString()}</td>
                    <td style={td}>{s.lastSeenAt ? timeAgo(s.lastSeenAt) : '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Students list */}
      {view === 'students' && (
        <div>
          <div className="card" style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.88rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid var(--border-color)' }}>
                  <th style={th}>Name</th><th style={th}>Email</th><th style={th}>Plan</th><th style={th}>Joined</th><th style={th}>Last Online</th><th style={th}>Tutor</th>
                </tr>
              </thead>
              <tbody>
                {students.map(s => (
                  <tr key={s.userId} onClick={() => openStudentDetail(s.userId)}
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
                  <tr><td colSpan={6} style={{ ...td, textAlign: 'center', color: 'var(--text-secondary)' }}>No students yet</td></tr>
                )}
              </tbody>
            </table>
          </div>
          {studentsTotal > 20 && (
            <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginTop: '1rem' }}>
              <button className="btn btn-outline" disabled={studentsPage <= 1} onClick={() => loadStudents(studentsPage - 1)}>←</button>
              <span style={{ padding: '0.5rem' }}>{studentsPage} / {Math.ceil(studentsTotal / 20)}</span>
              <button className="btn btn-outline" disabled={studentsPage * 20 >= studentsTotal} onClick={() => loadStudents(studentsPage + 1)}>→</button>
            </div>
          )}
        </div>
      )}

      {/* Tutors list */}
      {view === 'tutors' && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1rem' }}>
          {tutors.map(t => (
            <div key={t.userId} className="card" style={{ padding: '1.25rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                <div>
                  <strong>{t.name}</strong>
                  {t.isVerified && <span style={{ color: 'var(--success-color)', marginLeft: '0.5rem' }}>&#10003;</span>}
                </div>
                <span style={{ fontSize: '0.8rem', color: t.isAvailable ? 'var(--success-color)' : 'var(--text-secondary)' }}>
                  {t.isAvailable ? 'Available' : 'Unavailable'}
                </span>
              </div>
              <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', margin: '0.5rem 0' }}>{t.headline}</div>
              <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{t.email}</div>
              <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.75rem', fontSize: '0.8rem' }}>
                <span>&#9733; {t.averageRating.toFixed(1)}</span>
                <span>{t.totalStudents} students</span>
                <span>{t.specializations.join(', ')}</span>
              </div>
              <div style={{ marginTop: '0.75rem' }}>
                {t.isVerified ? (
                  <button className="btn btn-outline" style={{ fontSize: '0.75rem', padding: '0.25rem 0.7rem', color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                    onClick={async () => {
                      try {
                        await api.post(`/tutor-school-applications/unverify/${t.userId}`);
                        loadTutors();
                      } catch { /* */ }
                    }}>Unverify</button>
                ) : (
                  <button className="btn btn-primary" style={{ fontSize: '0.75rem', padding: '0.25rem 0.7rem' }}
                    onClick={async () => {
                      try {
                        await api.post(`/tutor-school-applications/verify/${t.userId}`);
                        loadTutors();
                      } catch { /* */ }
                    }}>Verify</button>
                )}
              </div>
            </div>
          ))}
          {tutors.length === 0 && !loading && (
            <div className="card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
              No tutors in your school yet
            </div>
          )}
        </div>
      )}

      {/* Tutor Applications */}
      {view === 'tutor-applications' && (
        <div className="card" style={{ padding: '1rem', overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.88rem' }}>
            <thead>
              <tr style={{ borderBottom: '2px solid var(--border-color)' }}>
                <th style={th}>Name</th><th style={th}>Email</th><th style={th}>Message</th>
                <th style={th}>Date</th><th style={th}>Status</th><th style={th}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {tutorApps.map(a => (
                <tr key={a.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                  <td style={td}>{a.userName}</td>
                  <td style={td}>{a.userEmail}</td>
                  <td style={{ ...td, maxWidth: '200px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{a.message || '—'}</td>
                  <td style={td}>{new Date(a.createdAt).toLocaleDateString()}</td>
                  <td style={td}>
                    <span style={{
                      padding: '0.15rem 0.5rem', borderRadius: '999px', fontSize: '0.75rem', fontWeight: 600, color: '#fff',
                      background: a.status === 'Pending' ? '#f59e0b' : a.status === 'Approved' ? 'var(--success-color)' : 'var(--error-color)',
                    }}>{a.status}</span>
                  </td>
                  <td style={td}>
                    {a.status === 'Pending' && (
                      <div style={{ display: 'flex', gap: '0.35rem' }}>
                        <button className="btn btn-primary" style={{ fontSize: '0.75rem', padding: '0.25rem 0.6rem' }}
                          onClick={async () => {
                            try {
                              await api.put(`/tutor-school-applications/${a.id}/status`, { status: 'Approved' });
                              loadTutorApps();
                            } catch { /* */ }
                          }}>Approve</button>
                        <button className="btn btn-outline" style={{ fontSize: '0.75rem', padding: '0.25rem 0.6rem', color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                          onClick={async () => {
                            try {
                              await api.put(`/tutor-school-applications/${a.id}/status`, { status: 'Rejected' });
                              loadTutorApps();
                            } catch { /* */ }
                          }}>Reject</button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
              {tutorApps.length === 0 && !loading && (
                <tr><td colSpan={6} style={{ ...td, textAlign: 'center', color: 'var(--text-secondary)' }}>No tutor applications yet</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* Student detail */}
      {view === 'student-detail' && selectedStudent && (
        <div>
          <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
            <h2 style={{ margin: '0 0 0.5rem' }}>{selectedStudent.student.name}</h2>
            <div style={{ fontSize: '0.9rem', color: 'var(--text-secondary)' }}>
              {selectedStudent.student.email} — Joined {new Date(selectedStudent.student.createdAt).toLocaleDateString()}
              {selectedStudent.student.lastSeenAt && ` — Last seen ${timeAgo(selectedStudent.student.lastSeenAt)}`}
            </div>
          </div>

          {/* Skills */}
          {selectedStudent.skills.length > 0 && (
            <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>Skills</h3>
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

          {/* Recent sessions */}
          <div className="card" style={{ padding: '1.5rem', overflowX: 'auto' }}>
            <h3 style={{ margin: '0 0 1rem' }}>Recent Sessions</h3>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.88rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid var(--border-color)' }}>
                  <th style={th}>Date</th><th style={th}>Questions</th><th style={th}>Correct</th><th style={th}>Accuracy</th>
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
                  <tr><td colSpan={4} style={{ ...td, textAlign: 'center', color: 'var(--text-secondary)' }}>No sessions yet</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

function StatCard({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="card" style={{ padding: '1.25rem', textAlign: 'center' }}>
      <div style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--primary-color)' }}>{value}</div>
      <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>{label}</div>
    </div>
  );
}

function timeAgo(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  return `${days}d ago`;
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

export default SchoolAdminDashboardPage;
