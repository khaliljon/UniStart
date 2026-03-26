import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import type { SchoolBranding } from '../types';
import LanguageSwitcher from '../components/LanguageSwitcher';

interface Props {
  branding: SchoolBranding;
}

function WhiteLabelLanding({ branding }: Props) {
  const navigate = useNavigate();
  const { t, locale } = useTranslation();
  const brandName = branding.navbarTitle || branding.name;
  const primary = branding.primaryColor || '#6366f1';
  const localizedDesc = locale === 'en' ? branding.descriptionEn
    : locale === 'kz' ? branding.descriptionKz
    : branding.description;

  const features = [
    { title: t.wl.featureAdaptive, desc: t.wl.featureAdaptiveDesc },
    { title: t.wl.featurePrediction, desc: t.wl.featurePredictionDesc },
    { title: t.wl.featurePlan, desc: t.wl.featurePlanDesc },
    { title: t.wl.featureMock, desc: t.wl.featureMockDesc },
    { title: t.wl.featureAnalytics, desc: t.wl.featureAnalyticsDesc },
    { title: t.wl.featureTutors, desc: t.wl.featureTutorsDesc },
  ];

  return (
    <div style={{ background: 'var(--background-color)', color: 'var(--text-primary)', minHeight: '100vh' }}>

      {/* Navbar */}
      <nav style={{
        display: 'flex', justifyContent: 'space-between', alignItems: 'center',
        padding: '1rem 2rem', maxWidth: '1200px', margin: '0 auto', flexWrap: 'wrap', gap: '0.5rem',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          {branding.logoUrl && (
            <img src={branding.logoUrl} alt={brandName}
              style={{ height: 36, borderRadius: '50%', objectFit: 'cover' }} />
          )}
          <span style={{ fontWeight: 800, fontSize: '1.5rem', color: primary }}>{brandName}</span>
        </div>
        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', flexWrap: 'wrap' }}>
          <LanguageSwitcher />
          <button onClick={() => navigate('/login')}
            style={{
              padding: '0.4rem 0.9rem', border: '1px solid var(--border-color)',
              borderRadius: '0.5rem', background: 'transparent', color: 'var(--text-primary)',
              cursor: 'pointer', fontSize: '0.9rem',
            }}>
            {t.wl.login}
          </button>
          <button onClick={() => navigate('/register')}
            style={{
              padding: '0.4rem 0.9rem', border: 'none', borderRadius: '0.5rem',
              background: primary, color: '#fff', cursor: 'pointer',
              fontSize: '0.9rem', fontWeight: 600,
            }}>
            {t.wl.register}
          </button>
        </div>
      </nav>

      {/* Hero */}
      <section style={{ textAlign: 'center', padding: '5rem 2rem 3rem', maxWidth: '800px', margin: '0 auto' }}>
        {branding.specializations.length > 0 && (
          <div style={{
            display: 'inline-block', padding: '0.3rem 1.2rem', borderRadius: '999px',
            background: `${primary}22`, color: primary, fontSize: '0.85rem', fontWeight: 600,
            marginBottom: '1.5rem',
          }}>
            {branding.specializations.join(' \u00b7 ')}
          </div>
        )}
        <h1 style={{
          fontSize: 'clamp(2rem, 5vw, 3.5rem)', fontWeight: 800,
          lineHeight: 1.15, color: primary, margin: '0 0 1.5rem',
        }}>
          {t.wl.heroTitle}
          <br />
          <span style={{ color: 'var(--text-primary)' }}>{t.wl.heroWith} {brandName}</span>
        </h1>
        {localizedDesc && (
          <p style={{ fontSize: '1.1rem', lineHeight: 1.7, color: 'var(--text-secondary)', maxWidth: '600px', margin: '0 auto 2rem' }}>
            {localizedDesc}
          </p>
        )}
        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center', flexWrap: 'wrap' }}>
          <button onClick={() => navigate('/register')}
            style={{
              padding: '0.85rem 2rem', background: primary, color: '#fff',
              border: 'none', borderRadius: '0.75rem', fontSize: '1rem',
              fontWeight: 600, cursor: 'pointer',
            }}>
            {t.wl.getStarted}
          </button>
          <button onClick={() => navigate('/login')}
            style={{
              padding: '0.85rem 2rem', background: 'transparent',
              color: 'var(--text-primary)', border: '1px solid var(--border-color)',
              borderRadius: '0.75rem', fontSize: '1rem', cursor: 'pointer',
            }}>
            {t.wl.login}
          </button>
        </div>
      </section>

      {/* Features */}
      <section style={{ padding: '3rem 2rem', maxWidth: '1000px', margin: '0 auto' }}>
        <h2 style={{ textAlign: 'center', fontWeight: 700, fontSize: '1.75rem', marginBottom: '2rem' }}>
          {t.wl.whyTitle} {brandName}?
        </h2>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', gap: '1.5rem' }}>
          {features.map((f, i) => (
            <div key={i} className="card" style={{ padding: '1.5rem', borderLeft: `3px solid ${primary}` }}>
              <h3 style={{ margin: '0 0 0.5rem', fontSize: '1.05rem' }}>{f.title}</h3>
              <p style={{ margin: 0, fontSize: '0.9rem', color: 'var(--text-secondary)', lineHeight: 1.6 }}>{f.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* CTA */}
      <section style={{ textAlign: 'center', padding: '3rem 2rem 5rem' }}>
        <h2 style={{ fontWeight: 700, fontSize: '1.5rem', marginBottom: '1rem' }}>
          {t.wl.ctaTitle}
        </h2>
        <button onClick={() => navigate('/register')}
          style={{
            padding: '0.85rem 2.5rem', background: primary, color: '#fff',
            border: 'none', borderRadius: '0.75rem', fontSize: '1rem',
            fontWeight: 600, cursor: 'pointer',
          }}>
          {t.wl.ctaButton}
        </button>
      </section>

      {/* Footer */}
      <footer style={{
        textAlign: 'center', padding: '1.5rem 2rem', borderTop: '1px solid var(--border-color)',
        fontSize: '0.8rem', color: 'var(--text-secondary)',
      }}>
        {t.wl.poweredBy} <span style={{ color: '#6366f1', fontWeight: 600 }}>UniStart</span> \u00b7 {new Date().getFullYear()}
      </footer>
    </div>
  );
}

export default WhiteLabelLanding;
