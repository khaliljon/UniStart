import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import LanguageSwitcher from '../components/LanguageSwitcher';

function LandingPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const FEATURES = [
    { icon: '', title: t.landing.featureAdaptiveTitle, desc: t.landing.featureAdaptiveDesc },
    { icon: '', title: t.landing.featurePredictionTitle, desc: t.landing.featurePredictionDesc },
    { icon: '', title: t.landing.featurePlanTitle, desc: t.landing.featurePlanDesc },
    { icon: '', title: t.landing.featureMockTitle, desc: t.landing.featureMockDesc },
    { icon: '', title: t.landing.featureDiagnosticTitle, desc: t.landing.featureDiagnosticDesc },
    { icon: '', title: t.landing.featureExplanationsTitle, desc: t.landing.featureExplanationsDesc },
  ];

  const PLANS = [
    {
      name: 'Free',
      price: '$0',
      period: t.landing.forever,
      highlight: false,
      features: [
        t.landing.freeQuestions,
        t.landing.freeLesson,
        t.landing.freeAnalytics,
        t.landing.freeDiagnostic,
        t.landing.freeWeeklyForecast,
        t.landing.freeBasicPlan,
      ],
      cta: t.landing.startWithFree,
    },
    {
      name: 'Pro',
      price: '$9.99',
      period: t.landing.perMonth,
      highlight: true,
      features: [
        t.landing.proUnlimited,
        t.landing.proAllLessons,
        t.landing.proMockExams,
        t.landing.proFullAnalytics,
        t.landing.proRealtimePrediction,
        t.landing.proFullPlan,
        t.landing.proPriority,
      ],
      cta: t.landing.startWithPro,
    },
  ];

  const TESTIMONIALS = [
    { name: 'Айдана К.', exam: 'SAT', score: '1480', text: 'За 2 месяца подготовки на UniStart мой балл вырос с 1200 до 1480. Адаптивные тесты — это совсем другой уровень.' },
    { name: 'Тимур Б.', exam: 'TOEFL', score: '108', text: 'Удобный план подготовки и прогноз балла мотивировали заниматься каждый день. Получил 108 при цели 100.' },
    { name: 'Дана М.', exam: 'NUET', score: '156', text: 'UniStart идеально подготовил к NUET. Mock exams были максимально близки к реальному экзамену.' },
  ];

  const STATS = [
    { value: '10 000+', label: t.landing.statsQuestions },
    { value: '3', label: t.landing.statsExams },
    { value: '95%', label: t.landing.statsAccuracy },
    { value: '~30%', label: t.landing.statsGrowth },
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
      }}>
        <div style={{ fontWeight: 800, fontSize: '1.5rem', color: '#6366f1' }}>
          UniStart
        </div>
        <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center' }}>
          <LanguageSwitcher />
          <button
            onClick={() => navigate('/login')}
            style={{
              padding: '0.5rem 1.25rem',
              border: '1px solid var(--border-color)',
              borderRadius: '0.5rem',
              background: 'transparent',
              cursor: 'pointer',
              fontWeight: 500,
              color: 'var(--text-primary)',
            }}
          >
            {t.landing.login}
          </button>
          <button
            onClick={() => navigate('/register')}
            style={{
              padding: '0.5rem 1.25rem',
              border: 'none',
              borderRadius: '0.5rem',
              background: '#6366f1',
              color: '#fff',
              cursor: 'pointer',
              fontWeight: 600,
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
          SAT • TOEFL • NUET
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
      </section>

      {/* ═══ Stats ═══ */}
      <section style={{
        background: 'var(--bg-secondary)',
        padding: '3rem 2rem',
      }}>
        <div style={{
          maxWidth: '900px',
          margin: '0 auto',
          display: 'grid',
          gridTemplateColumns: 'repeat(4, 1fr)',
          gap: '2rem',
          textAlign: 'center',
        }}>
          {STATS.map((s) => (
            <div key={s.label}>
              <div style={{ fontSize: '2rem', fontWeight: 800, color: '#6366f1' }}>{s.value}</div>
              <div style={{ fontSize: '0.9rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>{s.label}</div>
            </div>
          ))}
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
        <div style={{ maxWidth: '800px', margin: '0 auto' }}>
          <div style={{ textAlign: 'center', marginBottom: '3rem' }}>
            <h2 style={{ fontSize: '2rem', fontWeight: 700, marginBottom: '0.75rem' }}>
              {t.landing.pricingTitle}
            </h2>
            <p style={{ color: 'var(--text-secondary)', fontSize: '1.05rem' }}>
              {t.landing.pricingDesc}
            </p>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem' }}>
            {PLANS.map((plan) => (
              <div key={plan.name} style={{
                padding: '2rem',
                borderRadius: '1.25rem',
                border: plan.highlight ? '2px solid #6366f1' : '1px solid var(--border-color)',
                background: 'var(--card-bg)',
                position: 'relative',
                boxShadow: plan.highlight ? '0 8px 30px rgba(99,102,241,0.15)' : 'none',
              }}>
                {plan.highlight && (
                  <div style={{
                    position: 'absolute',
                    top: '-12px',
                    right: '1.25rem',
                    background: '#6366f1',
                    color: '#fff',
                    padding: '0.2rem 0.8rem',
                    borderRadius: '1rem',
                    fontSize: '0.75rem',
                    fontWeight: 600,
                  }}>
                    {t.landing.popular}
                  </div>
                )}
                <div style={{ fontWeight: 700, fontSize: '1.25rem', marginBottom: '0.25rem', color: plan.highlight ? '#6366f1' : 'var(--text-primary)' }}>
                  {plan.name}
                </div>
                <div style={{ display: 'flex', alignItems: 'baseline', gap: '0.25rem', marginBottom: '0.25rem' }}>
                  <span style={{ fontSize: '2.5rem', fontWeight: 800 }}>{plan.price}</span>
                  <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{plan.period}</span>
                </div>

                <ul style={{ listStyle: 'none', padding: 0, margin: '1.5rem 0', display: 'flex', flexDirection: 'column', gap: '0.6rem' }}>
                  {plan.features.map((f) => (
                    <li key={f} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.9rem', color: 'var(--text-primary)' }}>
                      <span style={{ color: plan.highlight ? '#6366f1' : '#10b981', flexShrink: 0 }}>✓</span>
                      {f}
                    </li>
                  ))}
                </ul>

                <button
                  onClick={() => navigate('/register')}
                  style={{
                    width: '100%',
                    padding: '0.75rem',
                    borderRadius: '0.75rem',
                    border: plan.highlight ? 'none' : '2px solid var(--border-color)',
                    background: plan.highlight ? '#6366f1' : 'transparent',
                    color: plan.highlight ? '#fff' : 'var(--text-primary)',
                    fontWeight: 600,
                    cursor: 'pointer',
                    fontSize: '1rem',
                  }}
                >
                  {plan.cta}
                </button>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ═══ Testimonials ═══ */}
      <section style={{
        padding: '5rem 2rem',
        maxWidth: '1000px',
        margin: '0 auto',
      }}>
        <div style={{ textAlign: 'center', marginBottom: '3rem' }}>
          <h2 style={{ fontSize: '2rem', fontWeight: 700, marginBottom: '0.75rem' }}>
            {t.landing.testimonialsTitle}
          </h2>
        </div>

        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
          gap: '1.5rem',
        }}>
          {TESTIMONIALS.map((item) => (
            <div key={item.name} style={{
              padding: '1.5rem',
              borderRadius: '1rem',
              border: '1px solid var(--border-color)',
              background: 'var(--card-bg)',
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                <div>
                  <div style={{ fontWeight: 600, fontSize: '0.95rem' }}>{item.name}</div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>{item.exam}</div>
                </div>
                <div style={{
                  background: '#6366f115',
                  color: '#6366f1',
                  fontWeight: 700,
                  padding: '0.25rem 0.75rem',
                  borderRadius: '0.5rem',
                  fontSize: '0.9rem',
                }}>
                  {item.score}
                </div>
              </div>
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', lineHeight: 1.6, margin: 0 }}>
                "{item.text}"
              </p>
            </div>
          ))}
        </div>
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
              <span>support@unistart.kz</span>
              <span>Алматы, Казахстан</span>
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
          © {new Date().getFullYear()} UniStart. {t.landing.allRights}
        </div>
      </footer>
    </div>
  );
}

export default LandingPage;
