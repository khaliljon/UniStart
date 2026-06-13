import { useNavigate } from 'react-router-dom';
import { useEffect } from 'react';
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
  const localizedDesc = (locale === 'en' ? branding.descriptionEn
    : locale === 'kz' ? branding.descriptionKz
    : branding.description) || branding.description;

  // Dynamic SEO meta tags for White Label subdomain
  useEffect(() => {
    const origin = window.location.origin;
    document.title = `${brandName} — UniStart`;

    const setMeta = (attr: string, key: string, content: string) => {
      let el = document.querySelector(`meta[${attr}="${key}"]`) as HTMLMetaElement | null;
      if (!el) { el = document.createElement('meta'); el.setAttribute(attr, key); document.head.appendChild(el); }
      el.content = content;
    };

    setMeta('name', 'description', localizedDesc || `${brandName} — подготовка к экзаменам на платформе UniStart`);
    setMeta('property', 'og:title', `${brandName} — UniStart`);
    setMeta('property', 'og:description', localizedDesc || `${brandName} — подготовка к экзаменам`);
    setMeta('property', 'og:url', origin);
    setMeta('property', 'og:type', 'website');
    setMeta('property', 'og:site_name', brandName);
    if (branding.logoUrl) setMeta('property', 'og:image', branding.logoUrl);

    let canonical = document.querySelector('link[rel="canonical"]') as HTMLLinkElement | null;
    if (!canonical) { canonical = document.createElement('link'); canonical.rel = 'canonical'; document.head.appendChild(canonical); }
    canonical.href = origin;
  }, [brandName, localizedDesc, branding.logoUrl]);

  const features = [
    { title: t.wl.featureAdaptive, desc: t.wl.featureAdaptiveDesc },
    { title: t.wl.featurePrediction, desc: t.wl.featurePredictionDesc },
    { title: t.wl.featurePlan, desc: t.wl.featurePlanDesc },
    { title: t.wl.featureMock, desc: t.wl.featureMockDesc },
    { title: t.wl.featureAnalytics, desc: t.wl.featureAnalyticsDesc },
    { title: t.wl.featureTutors, desc: t.wl.featureTutorsDesc },
  ];

  const mockPackages = [
    { label: t.wl.mockPack1, price: '2 000 ₸', desc: '', highlight: false },
    { label: t.wl.mockPack3, price: '4 000 ₸', desc: '', highlight: true },
    { label: t.wl.mockPack5, price: '6 000 ₸', desc: '', highlight: false },
    { label: t.wl.mockPackFull, price: '10 000 ₸', desc: t.wl.mockPackFullDesc, highlight: false },
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
        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', flexWrap: 'wrap', marginLeft: 'auto' }}>
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

      {/* Mock exam packages */}
      <section style={{ padding: '3rem 2rem', maxWidth: '1000px', margin: '0 auto' }}>
        <div style={{ textAlign: 'center', marginBottom: '2.5rem' }}>
          <h2 style={{ fontWeight: 700, fontSize: '1.75rem', marginBottom: '0.5rem' }}>
            {t.wl.mockTitle}
          </h2>
          <p style={{ color: 'var(--text-secondary)', fontSize: '1rem' }}>
            {t.wl.mockSubtitle}
          </p>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1.25rem' }}>
          {mockPackages.map((p, i) => (
            <div
              key={i}
              className="card"
              style={{
                padding: '1.75rem 1.5rem',
                textAlign: 'center',
                position: 'relative',
                border: p.highlight ? `2px solid ${primary}` : '1px solid var(--border-color)',
                boxShadow: p.highlight ? `0 8px 28px ${primary}22` : 'none',
              }}
            >
              {p.highlight && (
                <div style={{
                  position: 'absolute', top: '-12px', left: '50%', transform: 'translateX(-50%)',
                  background: primary, color: '#fff', padding: '0.2rem 0.8rem',
                  borderRadius: '999px', fontSize: '0.72rem', fontWeight: 700, whiteSpace: 'nowrap',
                }}>
                  {t.wl.mockPopular}
                </div>
              )}
              <div style={{ fontWeight: 700, fontSize: '1.05rem', marginBottom: '0.5rem' }}>{p.label}</div>
              <div style={{ fontSize: '1.9rem', fontWeight: 800, color: primary, marginBottom: '0.25rem' }}>{p.price}</div>
              {p.desc && (
                <div style={{ color: 'var(--text-secondary)', fontSize: '0.82rem', marginBottom: '1rem' }}>{p.desc}</div>
              )}
              <button
                onClick={() => navigate('/register')}
                style={{
                  marginTop: p.desc ? 0 : '1rem',
                  width: '100%', padding: '0.65rem', borderRadius: '0.6rem',
                  border: p.highlight ? 'none' : `2px solid ${primary}`,
                  background: p.highlight ? primary : 'transparent',
                  color: p.highlight ? '#fff' : primary,
                  fontWeight: 600, cursor: 'pointer', fontSize: '0.9rem',
                }}
              >
                {t.wl.mockBuy}
              </button>
            </div>
          ))}
        </div>
      </section>

      {/* Become a tutor */}
      <section style={{
        padding: '3rem 2rem', maxWidth: '700px', margin: '0 auto', textAlign: 'center',
      }}>
        <div className="card" style={{
          padding: '2.5rem', borderTop: `3px solid ${primary}`,
          background: 'var(--card-bg)',
        }}>
          <h2 style={{ fontWeight: 700, fontSize: '1.35rem', marginBottom: '0.75rem' }}>
            {t.wl.becomeTutorTitle}
          </h2>
          <p style={{
            color: 'var(--text-secondary)', fontSize: '0.95rem', lineHeight: 1.6,
            marginBottom: '1.5rem', maxWidth: '500px', margin: '0 auto 1.5rem',
          }}>
            {t.wl.becomeTutorDesc}
          </p>
          <button onClick={() => navigate('/register?role=Tutor')}
            style={{
              padding: '0.75rem 2rem', background: 'transparent',
              color: primary, border: `2px solid ${primary}`,
              borderRadius: '0.75rem', fontSize: '0.95rem',
              fontWeight: 600, cursor: 'pointer',
            }}>
            {t.wl.becomeTutorBtn}
          </button>
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
        {t.wl.footerBrand.split('{brand}')[0]}
        <a href="https://unistart.kz" target="_blank" rel="noopener noreferrer"
          style={{ color: '#6366f1', fontWeight: 600, textDecoration: 'none' }}>UniStart</a>
        {t.wl.footerBrand.split('{brand}')[1]} &middot; {new Date().getFullYear()}
      </footer>
    </div>
  );
}

export default WhiteLabelLanding;
