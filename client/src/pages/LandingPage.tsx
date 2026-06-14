import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import LanguageSwitcher from '../components/LanguageSwitcher';
import { useBranding } from '../contexts/BrandingContext';
import WhiteLabelLanding from './WhiteLabelLanding';

function LandingPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { branding, isWhiteLabel } = useBranding();

  if (isWhiteLabel && branding) {
    return <WhiteLabelLanding branding={branding} />;
  }
  const FEATURES = [
    { icon: '', title: t.landing.featureAdaptiveTitle, desc: t.landing.featureAdaptiveDesc },
    { icon: '', title: t.landing.featurePredictionTitle, desc: t.landing.featurePredictionDesc },
    { icon: '', title: t.landing.featurePlanTitle, desc: t.landing.featurePlanDesc },
    { icon: '', title: t.landing.featureMockTitle, desc: t.landing.featureMockDesc },
    { icon: '', title: t.landing.featureDiagnosticTitle, desc: t.landing.featureDiagnosticDesc },
    { icon: '', title: t.landing.featureExplanationsTitle, desc: t.landing.featureExplanationsDesc },
  ];

  const FREE_FEATURES = [
    t.landing.freeQuestions,
    t.landing.freeAnalytics,
    t.landing.freeDiagnostic,
    t.landing.freeWeeklyForecast,
    t.landing.freeBasicPlan,
    t.landing.freeMockExam,
  ];

  const PRO_FEATURES = [
    t.landing.proUnlimited,
    t.landing.proMockExams,
    t.landing.proFullAnalytics,
    t.landing.proRealtimePrediction,
    t.landing.proFullPlan,
    t.landing.proReviewMistakes,
    t.landing.proPriority,
  ];

  return (
    <div style={{ background: 'var(--background-color)', color: 'var(--text-primary)', minHeight: '100vh' }}>

      {/* ═══ Navbar ═══ */}
      <nav style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: '1rem 2rem',
        maxWidth: '1200px',
        margin: '0 auto',
        gap: '0.5rem',
      }}>
        <div style={{ fontWeight: 800, fontSize: '1.5rem', color: '#6366f1', flexShrink: 0 }}>
          UniStart
        </div>
        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', flexShrink: 0 }}>
          <LanguageSwitcher />
          <button
            onClick={() => navigate('/login')}
            style={{
              padding: '0.4rem 0.7rem',
              border: '1px solid var(--border-color)',
              borderRadius: '0.5rem',
              background: 'transparent',
              cursor: 'pointer',
              fontWeight: 500,
              color: 'var(--text-primary)',
              fontSize: '0.8rem',
              whiteSpace: 'nowrap',
            }}
          >
            {t.landing.login}
          </button>
          <button
            onClick={() => navigate('/register')}
            style={{
              padding: '0.4rem 0.7rem',
              border: 'none',
              borderRadius: '0.5rem',
              background: '#6366f1',
              color: '#fff',
              cursor: 'pointer',
              fontWeight: 600,
              fontSize: '0.8rem',
              whiteSpace: 'nowrap',
            }}
          >
            {t.landing.register}
          </button>
        </div>
      </nav>

      {/* ═══ Hero ═══ */}
      <section style={{
        textAlign: 'center',
        padding: '5rem 2rem 4rem',
        maxWidth: '800px',
        margin: '0 auto',
      }}>
        <div style={{
          display: 'inline-block',
          padding: '0.3rem 1rem',
          borderRadius: '2rem',
          background: '#6366f115',
          color: '#6366f1',
          fontWeight: 600,
          fontSize: '0.85rem',
          marginBottom: '1.5rem',
        }}>
          SAT • NUET
        </div>
        <h1 style={{
          fontSize: 'clamp(2rem, 5vw, 3.5rem)',
          fontWeight: 800,
          lineHeight: 1.2,
          marginBottom: '1.25rem',
          background: 'linear-gradient(135deg, #6366f1, #8b5cf6)',
          WebkitBackgroundClip: 'text',
          WebkitTextFillColor: 'transparent',
        }}>
          {t.landing.heroTitle}
        </h1>
        <p style={{
          fontSize: '1.2rem',
          color: 'var(--text-secondary)',
          lineHeight: 1.7,
          maxWidth: '600px',
          margin: '0 auto 2.5rem',
        }}>
          {t.landing.heroSubtitle}
        </p>
        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center', flexWrap: 'wrap' }}>
          <button
            onClick={() => navigate('/register')}
            style={{
              padding: '0.9rem 2.5rem',
              border: 'none',
              borderRadius: '0.75rem',
              background: 'linear-gradient(135deg, #6366f1, #8b5cf6)',
              color: '#fff',
              fontSize: '1.1rem',
              fontWeight: 600,
              cursor: 'pointer',
              boxShadow: '0 4px 15px rgba(99,102,241,0.35)',
            }}
          >
            {t.landing.startFree}
          </button>
          <button
            onClick={() => {
              document.getElementById('features')?.scrollIntoView({ behavior: 'smooth' });
            }}
            style={{
              padding: '0.9rem 2rem',
              border: '2px solid var(--border-color)',
              borderRadius: '0.75rem',
              background: 'transparent',
              color: 'var(--text-primary)',
              fontSize: '1.1rem',
              fontWeight: 500,
              cursor: 'pointer',
            }}
          >
            {t.landing.learnMore}
          </button>
        </div>

        {/* ═══ Linhao partnership — prominent, right under the CTA ═══ */}
        <div style={{ marginTop: '2.5rem', display: 'flex', justifyContent: 'center' }}>
          <div
            style={{
              display: 'flex',
              flexDirection: 'column',
              gap: '0.85rem',
              padding: '1rem 1.25rem',
              maxWidth: '560px',
              width: '100%',
              borderRadius: '1rem',
              background: 'linear-gradient(135deg, rgba(99,102,241,0.10), rgba(139,92,246,0.10))',
              border: '2px solid #6366f1',
              boxShadow: '0 8px 28px rgba(99,102,241,0.22)',
              textAlign: 'left',
              transition: 'transform 0.2s, box-shadow 0.2s',
            }}
            onMouseEnter={(e) => { e.currentTarget.style.transform = 'translateY(-2px)'; e.currentTarget.style.boxShadow = '0 12px 34px rgba(99,102,241,0.30)'; }}
            onMouseLeave={(e) => { e.currentTarget.style.transform = 'none'; e.currentTarget.style.boxShadow = '0 8px 28px rgba(99,102,241,0.22)'; }}
          >
            <a
              href="https://linhao.unistart.kz"
              target="_blank"
              rel="noopener noreferrer"
              style={{ display: 'flex', alignItems: 'center', gap: '1rem', textDecoration: 'none' }}
            >
              <div style={{ width: '56px', height: '56px', borderRadius: '50%', overflow: 'hidden', flexShrink: 0 }}>
                <img src="/linhao-logo.png" alt="LINHAO.CHINESE" style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
              </div>
              <div style={{ flex: 1, minWidth: 0 }}>
                <span style={{
                  display: 'inline-block', fontSize: '0.68rem', fontWeight: 800, letterSpacing: '0.08em',
                  color: '#fff', textTransform: 'uppercase', marginBottom: '0.3rem',
                  padding: '0.15rem 0.55rem', borderRadius: '999px',
                  background: 'linear-gradient(135deg, #6366f1, #8b5cf6)',
                }}>
                  {t.landing.partnerTitle}
                </span>
                <div style={{ fontWeight: 700, fontSize: '1.05rem', color: 'var(--text-primary)' }}>
                  {t.landing.partnerLinHaoName}
                </div>
                <div style={{ color: 'var(--text-secondary)', fontSize: '0.82rem', lineHeight: 1.4 }}>
                  {t.landing.partnerLinHaoDesc}
                </div>
              </div>
              <span style={{ color: '#6366f1', fontWeight: 700, fontSize: '1.4rem', flexShrink: 0 }}>→</span>
            </a>

            {/* Social links */}
            <div style={{ display: 'flex', gap: '0.6rem', paddingLeft: 'calc(56px + 1rem)' }}>
              <a
                href="https://www.instagram.com/linhao.chinese/"
                target="_blank"
                rel="noopener noreferrer"
                style={{
                  display: 'inline-flex', alignItems: 'center', gap: '0.35rem',
                  fontSize: '0.78rem', fontWeight: 600, textDecoration: 'none',
                  color: '#fff', padding: '0.3rem 0.7rem', borderRadius: '999px',
                  background: 'linear-gradient(135deg, #f58529, #dd2a7b, #8134af)',
                }}
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <rect x="2" y="2" width="20" height="20" rx="5" ry="5" />
                  <path d="M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z" />
                  <line x1="17.5" y1="6.5" x2="17.51" y2="6.5" />
                </svg>
                Instagram
              </a>
              <a
                href="https://t.me/linhao_chinese"
                target="_blank"
                rel="noopener noreferrer"
                style={{
                  display: 'inline-flex', alignItems: 'center', gap: '0.35rem',
                  fontSize: '0.78rem', fontWeight: 600, textDecoration: 'none',
                  color: '#fff', padding: '0.3rem 0.7rem', borderRadius: '999px',
                  background: '#229ED9',
                }}
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M21.94 4.6 18.9 19c-.23 1.02-.83 1.27-1.68.79l-4.65-3.43-2.24 2.16c-.25.25-.46.46-.93.46l.33-4.73 8.6-7.77c.37-.33-.08-.52-.58-.19L7.4 13.2l-4.58-1.43c-1-.31-1.02-1 .21-1.48l17.9-6.9c.83-.31 1.56.19 1.29 1.46z" />
                </svg>
                Telegram
              </a>
            </div>
          </div>
        </div>
      </section>

      {/* ═══ Features ═══ */}
      <section id="features" style={{
        padding: '5rem 2rem',
        maxWidth: '1100px',
        margin: '0 auto',
      }}>
        <div style={{ textAlign: 'center', marginBottom: '3rem' }}>
          <h2 style={{ fontSize: '2rem', fontWeight: 700, marginBottom: '0.75rem' }}>
            {t.landing.featuresTitle}
          </h2>
          <p style={{ color: 'var(--text-secondary)', fontSize: '1.05rem', maxWidth: '550px', margin: '0 auto' }}>
            {t.landing.featuresDesc}
          </p>
        </div>

        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
          gap: '1.5rem',
        }}>
          {FEATURES.map((f) => (
            <div key={f.title} style={{
              padding: '1.75rem',
              borderRadius: '1rem',
              border: '1px solid var(--border-color)',
              background: 'var(--card-bg)',
              transition: 'box-shadow 0.2s, transform 0.2s',
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.boxShadow = '0 8px 30px rgba(0,0,0,0.08)';
              e.currentTarget.style.transform = 'translateY(-2px)';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.boxShadow = 'none';
              e.currentTarget.style.transform = 'none';
            }}
            >
              <div style={{ fontSize: '2rem', marginBottom: '0.75rem' }}>{f.icon}</div>
              <h3 style={{ fontSize: '1.1rem', fontWeight: 600, marginBottom: '0.5rem' }}>{f.title}</h3>
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', lineHeight: 1.6 }}>{f.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* ═══ Pricing ═══ */}
      <section style={{
        padding: '5rem 2rem',
        background: 'var(--bg-secondary)',
      }}>
        <div style={{ maxWidth: '1100px', margin: '0 auto' }}>
          <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
            <h2 style={{ fontSize: '2rem', fontWeight: 700, marginBottom: '0.75rem' }}>
              {t.landing.pricingTitle}
            </h2>
            <p style={{ color: 'var(--text-secondary)', fontSize: '1.05rem' }}>
              {t.landing.pricingDesc}
            </p>
          </div>

          <div className="pricing-grid">
            {/* Free */}
            <div style={{
              padding: '2rem',
              borderRadius: '1.25rem',
              border: '1px solid var(--border-color)',
              background: 'var(--card-bg)',
            }}>
              <div style={{ fontWeight: 700, fontSize: '1.25rem', marginBottom: '0.25rem', color: 'var(--text-primary)' }}>
                Free
              </div>
              <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.25rem', marginBottom: '0.25rem' }}>
                <span style={{ fontSize: '2.5rem', fontWeight: 800 }}>0 ₸</span>
                <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{t.landing.forever}</span>
              </div>
              <ul style={{ listStyle: 'none', padding: 0, margin: '1.5rem 0', display: 'flex', flexDirection: 'column', gap: '0.6rem' }}>
                {FREE_FEATURES.map((f) => (
                  <li key={f} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.9rem', color: 'var(--text-primary)' }}>
                    <span style={{ color: '#10b981', flexShrink: 0 }}>✓</span>
                    {f}
                  </li>
                ))}
              </ul>
              <button
                onClick={() => navigate('/register')}
                style={{
                  width: '100%', padding: '0.75rem', borderRadius: '0.75rem',
                  border: '2px solid var(--border-color)', background: 'transparent',
                  color: 'var(--text-primary)', fontWeight: 600, cursor: 'pointer', fontSize: '1rem',
                }}
              >
                {t.landing.startWithFree}
              </button>
            </div>

            {/* Pro Monthly */}
            <div style={{
              padding: '2rem',
              borderRadius: '1.25rem',
              border: '2px solid #6366f1',
              background: 'var(--card-bg)',
              position: 'relative',
              boxShadow: '0 8px 30px rgba(99,102,241,0.15)',
            }}>
              <div style={{
                position: 'absolute', top: '-12px', right: '1.25rem',
                background: '#6366f1', color: '#fff',
                padding: '0.2rem 0.8rem', borderRadius: '1rem', fontSize: '0.75rem', fontWeight: 600,
              }}>
                {t.landing.popular}
              </div>
              <div style={{ fontWeight: 700, fontSize: '1.25rem', marginBottom: '0.25rem', color: '#6366f1' }}>
                Pro
              </div>
              <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.25rem', marginBottom: '0.25rem' }}>
                <span style={{ fontSize: '2.5rem', fontWeight: 800 }}>
                  {t.landing.proMonthlyPrice}
                </span>
                <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
                  {t.landing.perMonth}
                </span>
              </div>
              <ul style={{ listStyle: 'none', padding: 0, margin: '1.5rem 0', display: 'flex', flexDirection: 'column', gap: '0.6rem' }}>
                {PRO_FEATURES.map((f) => (
                  <li key={f} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.9rem', color: 'var(--text-primary)' }}>
                    <span style={{ color: '#6366f1', flexShrink: 0 }}>✓</span>
                    {f}
                  </li>
                ))}
              </ul>
              <button
                onClick={() => navigate('/register')}
                style={{
                  width: '100%', padding: '0.75rem', borderRadius: '0.75rem',
                  border: 'none', background: '#6366f1',
                  color: '#fff', fontWeight: 600, cursor: 'pointer', fontSize: '1rem',
                }}
              >
                {t.landing.startWithPro}
              </button>
            </div>

            {/* Pro Yearly */}
            <div style={{
              padding: '2rem',
              borderRadius: '1.25rem',
              border: '2px solid #8b5cf6',
              background: 'var(--card-bg)',
              position: 'relative',
            }}>
              <div style={{
                position: 'absolute', top: '-12px', right: '1.25rem',
                background: 'linear-gradient(135deg, #10b981, #059669)', color: '#fff',
                padding: '0.2rem 0.8rem', borderRadius: '1rem', fontSize: '0.75rem', fontWeight: 600,
              }}>
                {t.landing.yearlyDiscount}
              </div>
              <div style={{ fontWeight: 700, fontSize: '1.25rem', marginBottom: '0.25rem', color: '#8b5cf6' }}>
                Pro
              </div>
              <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.25rem', marginBottom: '0.25rem' }}>
                <span style={{ fontSize: '2.5rem', fontWeight: 800 }}>
                  {t.landing.proYearlyPrice}
                </span>
                <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
                  {t.landing.perYear}
                </span>
              </div>
              <ul style={{ listStyle: 'none', padding: 0, margin: '1.5rem 0', display: 'flex', flexDirection: 'column', gap: '0.6rem' }}>
                {PRO_FEATURES.map((f) => (
                  <li key={f} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.9rem', color: 'var(--text-primary)' }}>
                    <span style={{ color: '#8b5cf6', flexShrink: 0 }}>✓</span>
                    {f}
                  </li>
                ))}
              </ul>
              <button
                onClick={() => navigate('/register')}
                style={{
                  width: '100%', padding: '0.75rem', borderRadius: '0.75rem',
                  border: 'none', background: 'linear-gradient(135deg, #8b5cf6, #6366f1)',
                  color: '#fff', fontWeight: 600, cursor: 'pointer', fontSize: '1rem',
                }}
              >
                {t.landing.startWithPro}
              </button>
            </div>
          </div>
        </div>
      </section>

      {/* ═══ Referral Program ═══ */}
      <section style={{
        padding: '4rem 2rem',
        maxWidth: '800px',
        margin: '0 auto',
        textAlign: 'center',
      }}>
        <h2 style={{ fontSize: '2rem', fontWeight: 700, marginBottom: '0.75rem' }}>
          {t.landing.referralTitle}
        </h2>
        <p style={{ color: 'var(--text-secondary)', fontSize: '1.05rem', marginBottom: '2rem', lineHeight: 1.6 }}>
          {t.landing.referralDesc}
        </p>
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
          gap: '1.5rem',
          marginBottom: '2rem',
        }}>
          <div style={{
            padding: '1.5rem',
            borderRadius: '1rem',
            border: '1px solid var(--border-color)',
            background: 'var(--card-bg)',
          }}>
            <p style={{ fontWeight: 600, fontSize: '0.95rem' }}>{t.landing.referralStudents}</p>
          </div>
          <div style={{
            padding: '1.5rem',
            borderRadius: '1rem',
            border: '1px solid var(--border-color)',
            background: 'var(--card-bg)',
          }}>
            <p style={{ fontWeight: 600, fontSize: '0.95rem' }}>{t.landing.referralTutors}</p>
          </div>
        </div>
        <button
          onClick={() => navigate('/register')}
          style={{
            padding: '0.85rem 2rem',
            borderRadius: '0.75rem',
            border: '2px solid var(--primary-color)',
            background: 'transparent',
            color: 'var(--primary-color)',
            fontWeight: 600,
            fontSize: '0.95rem',
            cursor: 'pointer',
            transition: 'all 0.2s ease',
          }}
          onMouseEnter={e => { e.currentTarget.style.background = 'var(--primary-color)'; e.currentTarget.style.color = '#fff'; }}
          onMouseLeave={e => { e.currentTarget.style.background = 'transparent'; e.currentTarget.style.color = 'var(--primary-color)'; }}
        >
          {t.landing.referralCta}
        </button>
      </section>

      {/* ═══ Become a Tutor ═══ */}
      <section style={{
        padding: '3rem 2rem',
        maxWidth: '800px',
        margin: '0 auto',
        textAlign: 'center',
      }}>
        <h2 style={{ fontSize: '1.8rem', fontWeight: 700, marginBottom: '0.75rem' }}>
          {t.landing.becomeTutorTitle}
        </h2>
        <p style={{
          color: 'var(--text-secondary)',
          fontSize: '1rem',
          lineHeight: 1.6,
          marginBottom: '1.5rem',
          maxWidth: '600px',
          margin: '0 auto 1.5rem',
        }}>
          {t.landing.becomeTutorDesc}
        </p>
        <button
          onClick={() => navigate('/register?role=Tutor')}
          style={{
            padding: '0.85rem 2rem',
            borderRadius: '0.75rem',
            border: '2px solid #8b5cf6',
            background: 'transparent',
            color: '#8b5cf6',
            fontWeight: 600,
            fontSize: '0.95rem',
            cursor: 'pointer',
            transition: 'all 0.2s ease',
          }}
          onMouseEnter={e => { e.currentTarget.style.background = '#8b5cf6'; e.currentTarget.style.color = '#fff'; }}
          onMouseLeave={e => { e.currentTarget.style.background = 'transparent'; e.currentTarget.style.color = '#8b5cf6'; }}
        >
          {t.landing.becomeTutorBtn}
        </button>
      </section>

      {/* ═══ CTA ═══ */}
      <section style={{
        padding: '4rem 2rem',
        background: 'linear-gradient(135deg, #6366f1, #8b5cf6)',
        textAlign: 'center',
        color: '#fff',
      }}>
        <h2 style={{ fontSize: '2rem', fontWeight: 700, marginBottom: '0.75rem' }}>
          {t.landing.ctaTitle}
        </h2>
        <p style={{ opacity: 0.9, fontSize: '1.05rem', marginBottom: '2rem', maxWidth: '500px', margin: '0 auto 2rem' }}>
          {t.landing.ctaDesc}
        </p>
        <button
          onClick={() => navigate('/register')}
          style={{
            padding: '1rem 3rem',
            border: 'none',
            borderRadius: '0.75rem',
            background: '#fff',
            color: '#6366f1',
            fontSize: '1.1rem',
            fontWeight: 700,
            cursor: 'pointer',
            boxShadow: '0 4px 15px rgba(0,0,0,0.15)',
          }}
        >
          {t.landing.createAccountFree}
        </button>
      </section>

      {/* ═══ Footer ═══ */}
      <footer style={{
        padding: '3rem 2rem',
        background: '#1a1a2e',
        color: '#9ca3af',
      }}>
        <div style={{
          maxWidth: '1000px',
          margin: '0 auto',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'flex-start',
          flexWrap: 'wrap',
          gap: '2rem',
        }}>
          <div>
            <div style={{ fontWeight: 800, fontSize: '1.25rem', color: '#fff', marginBottom: '0.5rem' }}>
              UniStart
            </div>
            <p style={{ fontSize: '0.85rem', maxWidth: '300px', lineHeight: 1.6 }}>
              {t.landing.footerDesc}
            </p>
          </div>

          <div>
            <div style={{ fontWeight: 600, color: '#fff', marginBottom: '0.75rem', fontSize: '0.9rem' }}>
              {t.landing.platform}
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem', fontSize: '0.85rem' }}>
              <span style={{ cursor: 'pointer' }} onClick={() => navigate('/register')}>{t.landing.register}</span>
              <span style={{ cursor: 'pointer' }} onClick={() => navigate('/login')}>{t.landing.login}</span>
              <span style={{ cursor: 'pointer' }} onClick={() => document.getElementById('features')?.scrollIntoView({ behavior: 'smooth' })}>{t.landing.capabilities}</span>
              <span style={{ cursor: 'pointer' }} onClick={() => navigate('/privacy')}>{t.legal.privacyTitle}</span>
              <span style={{ cursor: 'pointer' }} onClick={() => navigate('/terms')}>{t.legal.termsTitle}</span>
            </div>
          </div>

          <div>
            <div style={{ fontWeight: 600, color: '#fff', marginBottom: '0.75rem', fontSize: '0.9rem' }}>
              {t.landing.faq}
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem', fontSize: '0.85rem' }}>
              <span>{t.landing.faqExams}</span>
              <span>{t.landing.faqExamsAnswer}</span>
              <span>{t.landing.faqPrice}</span>
              <span>{t.landing.faqPriceAnswer}</span>
            </div>
          </div>

          <div>
            <div style={{ fontWeight: 600, color: '#fff', marginBottom: '0.75rem', fontSize: '0.9rem' }}>
              {t.landing.contacts}
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem', fontSize: '0.85rem' }}>
              <span>unistart.kz@gmail.com</span>
              <span>Астана, Казахстан</span>
            </div>
          </div>
        </div>

        <div style={{
          maxWidth: '1000px',
          margin: '2rem auto 0',
          paddingTop: '1.5rem',
          borderTop: '1px solid #374151',
          textAlign: 'center',
          fontSize: '0.8rem',
        }}>
          © {new Date().getFullYear()} UniStart. {t.landing.allRights} |{' '}
          <span style={{ cursor: 'pointer', color: '#9ca3af' }} onClick={() => navigate('/privacy')}>{t.legal.privacyTitle}</span>{' | '}
          <span style={{ cursor: 'pointer', color: '#9ca3af' }} onClick={() => navigate('/terms')}>{t.legal.termsTitle}</span>
        </div>
      </footer>
    </div>
  );
}

export default LandingPage;
