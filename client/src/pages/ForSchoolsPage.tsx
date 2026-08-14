import { useState, FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from '../hooks/useTranslation';
import api from '../services/api';

const checkIcon = (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#10b981" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><polyline points="20 6 9 17 4 12" /></svg>
);

function ForSchoolsPage() {
  const { t } = useTranslation();
  const s = t.forSchools;

  const [form, setForm] = useState({ contactName: '', email: '', phone: '', schoolName: '', message: '' });
  const [submitting, setSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [formError, setFormError] = useState('');

  const benefits = [
    { title: s.benefitBrandTitle, desc: s.benefitBrandDesc },
    { title: s.benefitAnalyticsTitle, desc: s.benefitAnalyticsDesc },
    { title: s.benefitContentTitle, desc: s.benefitContentDesc },
    { title: s.benefitSubdomainTitle, desc: s.benefitSubdomainDesc },
    { title: s.benefitTutorsTitle, desc: s.benefitTutorsDesc },
    { title: s.benefitMobileTitle, desc: s.benefitMobileDesc },
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

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError('');
    setSubmitting(true);
    try {
      await api.post('/applications/school', form);
      setSubmitted(true);
    } catch {
      setFormError(s.formError);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div style={{ background: 'var(--background-color)', minHeight: '100vh' }}>
      <nav style={{
        display: 'flex', justifyContent: 'space-between', alignItems: 'center',
        padding: '1rem 2rem', maxWidth: 1200, margin: '0 auto',
      }}>
        <Link to="/landing" style={{ fontWeight: 700, fontSize: '1.3rem', color: 'var(--text-primary)', textDecoration: 'none' }}>
          ← UniStart
        </Link>
        <a href="#apply" className="btn btn-primary">{s.applyNow}</a>
      </nav>

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
          <a href="#apply" className="btn btn-primary" style={{ padding: '0.85rem 2rem', fontSize: '1rem' }}>
            {s.applyNow}
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

      <section style={{ padding: '2rem 2rem 3rem', maxWidth: 800, margin: '0 auto' }}>
        <div style={{
          display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
          gap: '1.5rem', textAlign: 'center',
        }}>
          <div style={{
            background: 'var(--card-bg)', border: '1px solid var(--border-color)',
            borderRadius: '1rem', padding: '2rem 1.5rem',
          }}>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>{s.pricingSchoolLabel}</div>
            <div style={{ fontSize: '2rem', fontWeight: 800, color: 'var(--primary-color)' }}>{s.pricingSchool}</div>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{s.pricingPerYear}</div>
          </div>
          <div style={{
            background: 'var(--card-bg)', border: '1px solid var(--border-color)',
            borderRadius: '1rem', padding: '2rem 1.5rem',
          }}>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>{s.pricingTutorLabel}</div>
            <div style={{ fontSize: '2rem', fontWeight: 800, color: 'var(--primary-color)' }}>{s.pricingTutor}</div>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{s.pricingPerYear}</div>
          </div>
          <div style={{
            background: 'var(--card-bg)', border: '1px solid var(--border-color)',
            borderRadius: '1rem', padding: '2rem 1.5rem',
          }}>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>{s.pricingStudentLabel}</div>
            <div style={{ fontSize: '2rem', fontWeight: 800, color: 'var(--primary-color)' }}>{s.pricingStudent}</div>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{s.pricingPerMonth}</div>
          </div>
        </div>
      </section>

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
              <h3 style={{ fontSize: '1.1rem', fontWeight: 600, marginBottom: '0.5rem', color: 'var(--text-primary)' }}>{b.title}</h3>
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', lineHeight: 1.5 }}>{b.desc}</p>
            </div>
          ))}
        </div>
      </section>

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
      </section>

      <section id="apply" style={{
        padding: '3rem 2rem', maxWidth: 600, margin: '0 auto 3rem',
        background: 'var(--card-bg)', border: '1px solid var(--border-color)', borderRadius: '1rem',
      }}>
        <h2 style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: '0.5rem', textAlign: 'center', color: 'var(--text-primary)' }}>
          {s.formTitle}
        </h2>
        <p style={{ textAlign: 'center', color: 'var(--text-secondary)', marginBottom: '1.5rem', fontSize: '0.9rem' }}>
          {s.formDesc}
        </p>

        {submitted ? (
          <div style={{ textAlign: 'center', padding: '2rem 0' }}>
            <div style={{ fontSize: '1.5rem', marginBottom: '0.5rem', color: 'var(--success-color)' }}>✓</div>
            <p style={{ fontSize: '1.1rem', fontWeight: 600, color: 'var(--text-primary)' }}>{s.formSuccess}</p>
            <p style={{ color: 'var(--text-secondary)', marginTop: '0.5rem', fontSize: '0.9rem' }}>{s.formSuccessDesc}</p>
          </div>
        ) : (
          <form onSubmit={handleSubmit}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
              <div className="form-group">
                <label className="form-label">{s.fieldName}</label>
                <input
                  type="text"
                  className="form-input"
                  value={form.contactName}
                  onChange={e => setForm({ ...form, contactName: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label className="form-label">{s.fieldSchool}</label>
                <input
                  type="text"
                  className="form-input"
                  value={form.schoolName}
                  onChange={e => setForm({ ...form, schoolName: e.target.value })}
                  required
                />
              </div>
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
              <div className="form-group">
                <label className="form-label">Email</label>
                <input
                  type="email"
                  className="form-input"
                  value={form.email}
                  onChange={e => setForm({ ...form, email: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label className="form-label">{s.fieldPhone}</label>
                <input
                  type="tel"
                  className="form-input"
                  value={form.phone}
                  onChange={e => setForm({ ...form, phone: e.target.value })}
                />
              </div>
            </div>
            <div className="form-group">
              <label className="form-label">{s.fieldMessage}</label>
              <textarea
                className="form-input"
                rows={3}
                value={form.message}
                onChange={e => setForm({ ...form, message: e.target.value })}
                style={{ resize: 'vertical' }}
              />
            </div>

            {formError && <p className="error-message">{formError}</p>}

            <button
              type="submit"
              className="btn btn-primary"
              style={{ width: '100%', marginTop: '0.5rem' }}
              disabled={submitting}
            >
              {submitting ? t.common.loading : s.formSubmit}
            </button>
          </form>
        )}
      </section>

      <footer style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
        © {new Date().getFullYear()} UniStart. {t.landing.allRights}
      </footer>
    </div>
  );
}

export default ForSchoolsPage;
