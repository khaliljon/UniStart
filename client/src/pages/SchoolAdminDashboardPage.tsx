import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { schoolAdminService } from '../services/schoolAdminService';
import { useTranslation } from '../i18n';
import { getDateLocale } from '../i18n';
import type { SchoolDashboard } from '../services/schoolAdminService';
import ContactForm from '../components/ContactForm';

function SchoolAdminDashboardPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [dashboard, setDashboard] = useState<SchoolDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [subscription, setSubscription] = useState<{ subscriptionExpiresAt: string | null; isActive: boolean } | null>(null);

  const loadExtras = useCallback(async () => {
    try {
      const sub = await schoolAdminService.getSubscription().catch(() => null);
      if (sub) setSubscription(sub);
    } catch { /* ignore */ }
  }, []);

  useEffect(() => {
    schoolAdminService.getDashboard()
      .then(setDashboard)
      .catch(() => {})
      .finally(() => setLoading(false));
    loadExtras();
  }, [loadExtras]);

  if (loading && !dashboard) return <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>{t.schoolAdmin.loading}</div>;
  if (!dashboard) return <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>{t.schoolAdmin.noSchool}</div>;

  return (
    <div>
      <h1 style={{ marginBottom: '1.5rem' }}>{dashboard.schoolName}</h1>

      {/* Primary action: manage mock exams */}
      <div
        className="card"
        onClick={() => navigate('/mocks')}
        style={{
          padding: '1.5rem 1.75rem', marginBottom: '2rem', cursor: 'pointer',
          display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '1rem',
          border: '2px solid var(--primary-color)',
          background: 'linear-gradient(135deg, var(--primary-color)11, transparent)',
        }}
      >
        <div>
          <div style={{ fontWeight: 700, fontSize: '1.15rem', marginBottom: '0.25rem' }}>
            {t.schoolAdmin.mocks.title}
          </div>
          <div style={{ fontSize: '0.88rem', color: 'var(--text-secondary)' }}>
            {t.schoolAdmin.mocks.subtitle}
          </div>
        </div>
        <span className="btn btn-primary" style={{ whiteSpace: 'nowrap' }}>
          {t.schoolAdmin.mocks.nav} &rarr;
        </span>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem', marginBottom: '2rem' }}>
        <StatCard label={t.schoolAdmin.students} value={dashboard.totalStudents} />
        <StatCard label={t.schoolAdmin.tutors} value={dashboard.totalTutors} />
        <StatCard label={t.schoolAdmin.active7d} value={dashboard.activeStudentsLast7Days} />
        <StatCard label={t.schoolAdmin.avgAccuracy} value={`${dashboard.averageAccuracy}%`} />
      </div>

      {/* Subscription status */}
      {subscription && (
        <div className="card" style={{
          padding: '1rem 1.25rem', marginBottom: '1rem',
          border: '1px solid',
          borderColor: subscription.isActive ? '#22c55e44' : '#f59e0b44',
          background: subscription.isActive ? '#22c55e11' : '#f59e0b11',
        }}>
          <div style={{ fontWeight: 600, marginBottom: '0.25rem' }}>
            {t.schoolAdmin.subscriptionTitle}
          </div>
          <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
            {subscription.isActive
              ? `${t.schoolAdmin.subscriptionActive} ${subscription.subscriptionExpiresAt ? new Date(subscription.subscriptionExpiresAt).toLocaleDateString(getDateLocale()) : ''}`
              : t.schoolAdmin.subscriptionInactive}
          </div>
          {!subscription.isActive && (
            <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', marginTop: '0.5rem' }}>
              <strong>{t.schoolAdmin.pricingInfo}</strong>
              <div>{t.schoolAdmin.paymentInstructions}</div>
            </div>
          )}
        </div>
      )}

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

      <div style={{ marginTop: '1.5rem' }}>
        <ContactForm />
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
