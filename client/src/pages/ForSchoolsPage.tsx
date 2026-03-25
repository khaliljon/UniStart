import { Link } from 'react-router-dom';
import { useTranslation } from '../hooks/useTranslation';

const checkIcon = (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#10b981" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><polyline points="20 6 9 17 4 12" /></svg>
);

function ForSchoolsPage() {
  const { t } = useTranslation();
  const s = t.forSchools;

  const benefits = [
    { icon: '🎓', title: s.benefitBrandTitle, desc: s.benefitBrandDesc },
    { icon: '📊', title: s.benefitAnalyticsTitle, desc: s.benefitAnalyticsDesc },
    { icon: '📝', title: s.benefitContentTitle, desc: s.benefitContentDesc },
    { icon: '🔗', title: s.benefitSubdomainTitle, desc: s.benefitSubdomainDesc },
    { icon: '👨‍🏫', title: s.benefitTutorsTitle, desc: s.benefitTutorsDesc },
    { icon: '📱', title: s.benefitMobileTitle, desc: s.benefitMobileDesc },
  ];

  const steps = [s.step1, s.step2, s.step3, s.step4];

  const included = [
    s.includedWhiteLabel,
    s.includedAnalytics,
    s.includedContent,
    s.includedTutorPanel,
    s.includedStudents,
    s.includedSupport,
  ];

  return (
    <div style={{ background: 'var(--background-color)', minHeight: '100vh' }}>
      {/* Navbar */}
      <nav style={{
        display: 'flex', justifyContent: 'space-between', alignItems: 'center',
        padding: '1rem 2rem', maxWidth: 1200, margin: '0 auto',
      }}>
        <Link to="/landing" style={{ fontWeight: 700, fontSize: '1.3rem', color: 'var(--text-primary)', textDecoration: 'none' }}>
          ← UniStart
        </Link>
        <Link to="/register" className="btn btn-primary">{s.getStarted}</Link>
      </nav>

      {/* Hero */}
      <section style={{
        textAlign: 'center', padding: '4rem 2rem 3rem', maxWidth: 800, margin: '0 auto',
      }}>
        <h1 style={{ fontSize: 'clamp(1.8rem, 4vw, 2.5rem)', fontWeight: 800, lineHeight: 1.2, color: 'var(--text-primary)' }}>
          {s.heroTitle}
        </h1>
        <p style={{ fontSize: '1.1rem', color: 'var(--text-secondary)', marginTop: '1rem', maxWidth: 600, margin: '1rem auto 0' }}>
          {s.heroDesc}
        </p>
        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center', marginTop: '2rem', flexWrap: 'wrap' }}>
          <a
            href="mailto:unistart.kz@gmail.com?subject=Партнёрство%20—%20размещение%20школы%20на%20UniStart"
            className="btn btn-primary"
            style={{ padding: '0.85rem 2rem', fontSize: '1rem' }}
          >
            {s.contactUs}
          </a>
          <a href="#benefits" style={{
            padding: '0.85rem 2rem', borderRadius: '0.5rem', border: '2px solid var(--border-color)',
            color: 'var(--text-primary)', textDecoration: 'none', fontWeight: 500, fontSize: '1rem',
            display: 'inline-flex', alignItems: 'center',
          }}>
            {s.learnMore}
          </a>
        </div>
      </section>

      {/* Benefits Grid */}
      <section id="benefits" style={{ padding: '3rem 2rem', maxWidth: 1100, margin: '0 auto' }}>
        <h2 style={{ fontSize: '1.8rem', fontWeight: 700, textAlign: 'center', marginBottom: '0.5rem', color: 'var(--text-primary)' }}>
          {s.benefitsTitle}
        </h2>
        <p style={{ textAlign: 'center', color: 'var(--text-secondary)', marginBottom: '2.5rem' }}>
          {s.benefitsDesc}
        </p>
        <div style={{
          display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
          gap: '1.5rem',
        }}>
          {benefits.map((b, i) => (
            <div key={i} style={{
              background: 'var(--card-bg)', border: '1px solid var(--border-color)',
              borderRadius: '1rem', padding: '1.5rem',
            }}>
              <div style={{ fontSize: '2rem', marginBottom: '0.75rem' }}>{b.icon}</div>
              <h3 style={{ fontSize: '1.1rem', fontWeight: 600, marginBottom: '0.5rem', color: 'var(--text-primary)' }}>{b.title}</h3>
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', lineHeight: 1.5 }}>{b.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* How it works */}
      <section style={{ padding: '3rem 2rem', maxWidth: 800, margin: '0 auto' }}>
        <h2 style={{ fontSize: '1.8rem', fontWeight: 700, textAlign: 'center', marginBottom: '2rem', color: 'var(--text-primary)' }}>
          {s.howItWorksTitle}
        </h2>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
          {steps.map((step, i) => (
            <div key={i} style={{ display: 'flex', gap: '1rem', alignItems: 'flex-start' }}>
              <div style={{
                width: 36, height: 36, borderRadius: '50%', background: 'var(--primary-color)',
                color: '#fff', display: 'flex', alignItems: 'center', justifyContent: 'center',
                fontWeight: 700, fontSize: '0.9rem', flexShrink: 0,
              }}>
                {i + 1}
              </div>
              <p style={{ fontSize: '1rem', color: 'var(--text-primary)', lineHeight: 1.5, paddingTop: '0.35rem' }}>{step}</p>
            </div>
          ))}
        </div>
      </section>

      {/* What's included */}
      <section style={{
        padding: '3rem 2rem', maxWidth: 700, margin: '0 auto',
        background: 'var(--card-bg)', border: '1px solid var(--border-color)', borderRadius: '1rem',
        marginBottom: '3rem',
      }}>
        <h2 style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: '1.5rem', textAlign: 'center', color: 'var(--text-primary)' }}>
          {s.includedTitle}
        </h2>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {included.map((item, i) => (
            <div key={i} style={{ display: 'flex', gap: '0.75rem', alignItems: 'center' }}>
              {checkIcon}
              <span style={{ color: 'var(--text-primary)' }}>{item}</span>
            </div>
          ))}
        </div>
        <div style={{ textAlign: 'center', marginTop: '2rem' }}>
          <a
            href="mailto:unistart.kz@gmail.com?subject=Партнёрство%20—%20размещение%20школы%20на%20UniStart"
            className="btn btn-primary"
            style={{ padding: '0.85rem 2.5rem', fontSize: '1rem' }}
          >
            {s.contactUs}
          </a>
        </div>
      </section>

      {/* Footer */}
      <footer style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
        © {new Date().getFullYear()} UniStart. {t.landing.allRights}
      </footer>
    </div>
  );
}

export default ForSchoolsPage;
