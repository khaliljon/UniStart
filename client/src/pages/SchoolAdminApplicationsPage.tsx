import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from '../i18n';
import api from '../services/api';

type TutorApp = { id: number; userId: number; userName: string; userEmail: string; status: string; message: string | null; createdAt: string; reviewedAt: string | null };

function SchoolAdminApplicationsPage() {
  const { t } = useTranslation();
  const [apps, setApps] = useState<TutorApp[]>([]);
  const [loading, setLoading] = useState(true);

  const loadApps = useCallback(async () => {
    setLoading(true);
    try {
      const { data } = await api.get<TutorApp[]>('/tutor-school-applications/school');
      setApps(data);
    } catch { /* */ } finally { setLoading(false); }
  }, []);

  useEffect(() => { loadApps(); }, [loadApps]);

  return (
    <div>
      <h1 style={{ marginBottom: '1.5rem' }}>{t.schoolAdmin.applications}</h1>
      <div className="card" style={{ padding: '1rem', overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.88rem' }}>
          <thead>
            <tr style={{ borderBottom: '2px solid var(--border-color)' }}>
              <th style={th}>{t.schoolAdmin.name}</th><th style={th}>{t.schoolAdmin.email}</th><th style={th}>{t.schoolAdmin.message}</th>
              <th style={th}>{t.schoolAdmin.date}</th><th style={th}>{t.schoolAdmin.status}</th><th style={th}>{t.schoolAdmin.actions}</th>
            </tr>
          </thead>
          <tbody>
            {apps.map(a => (
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
                            loadApps();
                          } catch { /* */ }
                        }}>{t.schoolAdmin.approve}</button>
                      <button className="btn btn-outline" style={{ fontSize: '0.75rem', padding: '0.25rem 0.6rem', color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                        onClick={async () => {
                          try {
                            await api.put(`/tutor-school-applications/${a.id}/status`, { status: 'Rejected' });
                            loadApps();
                          } catch { /* */ }
                        }}>{t.schoolAdmin.reject}</button>
                    </div>
                  )}
                </td>
              </tr>
            ))}
            {apps.length === 0 && !loading && (
              <tr><td colSpan={6} style={{ ...td, textAlign: 'center', color: 'var(--text-secondary)' }}>{t.schoolAdmin.noApplications}</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

const th: React.CSSProperties = { padding: '0.5rem 0.75rem', textAlign: 'left', fontWeight: 600 };
const td: React.CSSProperties = { padding: '0.5rem 0.75rem' };

export default SchoolAdminApplicationsPage;
