import { useState, useEffect } from 'react';
import { schoolAdminService } from '../services/schoolAdminService';
import { useTranslation } from '../i18n';
import type { SchoolDashboard } from '../services/schoolAdminService';

function SchoolAdminDashboardPage() {
  const { t } = useTranslation();
  const [dashboard, setDashboard] = useState<SchoolDashboard | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    schoolAdminService.getDashboard()
      .then(setDashboard)
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  if (loading && !dashboard) return <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>{t.schoolAdmin.loading}</div>;
  if (!dashboard) return <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>{t.schoolAdmin.noSchool}</div>;

  return (
    <div>
      <h1 style={{ marginBottom: '1.5rem' }}>{dashboard.schoolName}</h1>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem', marginBottom: '2rem' }}>
        <StatCard label={t.schoolAdmin.students} value={dashboard.totalStudents} />
        <StatCard label={t.schoolAdmin.tutors} value={dashboard.totalTutors} />
        <StatCard label={t.schoolAdmin.active7d} value={dashboard.activeStudentsLast7Days} />
        <StatCard label={t.schoolAdmin.avgAccuracy} value={`${dashboard.averageAccuracy}%`} />
      </div>

      <h3>{t.schoolAdmin.recentStudents}</h3>
      <div className="card" style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.88rem' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid var(--border-color)' }}>
              <th style={th}>{t.schoolAdmin.name}</th><th style={th}>{t.schoolAdmin.email}</th><th style={th}>{t.schoolAdmin.plan}</th><th style={th}>{t.schoolAdmin.joined}</th><th style={th}>{t.schoolAdmin.lastOnline}</th>
            </tr>
          </thead>
          <tbody>
            {dashboard.recentStudents.map(s => (
              <tr key={s.userId} style={{ borderBottom: '1px solid var(--border-color)' }}>
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

export default SchoolAdminDashboardPage;
