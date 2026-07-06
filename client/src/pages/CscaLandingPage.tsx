import { useEffect, useMemo, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import { useBranding } from '../contexts/BrandingContext';
import WhiteLabelLanding from './WhiteLabelLanding';
import CscaNav from '../components/csca/CscaNav';
import CscaFooter from '../components/csca/CscaFooter';
import CscaNewsSection from '../components/csca/CscaNewsSection';
import Reveal from '../components/csca/Reveal';
import {
  BrushDivider, MistMountains, SealStamp,
} from '../components/csca/ChineseMotifs';
import {
  CSCA_SUBJECTS, CSCA_PACKAGES, CSCA_STATS, CSCA_BOOK_PRICE,
  getNextSitting,
} from '../cscaConfig';

const CheckIcon = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M20 6L9 17l-5-5" />
  </svg>
);

function useCountdown(targetIso: string) {
  const [now, setNow] = useState(() => new Date());
  useEffect(() => {
    const id = setInterval(() => setNow(new Date()), 60_000);
    return () => clearInterval(id);
  }, []);
  const target = new Date(targetIso).getTime();
  const diff = Math.max(0, target - now.getTime());
  const days = Math.floor(diff / 86_400_000);
  const hours = Math.floor((diff % 86_400_000) / 3_600_000);
  const minutes = Math.floor((diff % 3_600_000) / 60_000);
  return { days, hours, minutes };
}

function CscaLandingPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { locale } = useTranslation();
  const { branding, isWhiteLabel } = useBranding();

  const s = cscaStrings[locale];
  const nextSitting = useMemo(() => getNextSitting(), []);
  const { days, hours, minutes } = useCountdown(nextSitting.date);

  // Scroll to a hash target (e.g. #news) when navigating from another page.
  useEffect(() => {
    if (location.hash) {
      const el = document.getElementById(location.hash.slice(1));
      if (el) setTimeout(() => el.scrollIntoView({ behavior: 'smooth', block: 'start' }), 50);
    }
  }, [location.hash]);

  if (isWhiteLabel && branding) {
    return <WhiteLabelLanding branding={branding} />;
  }

  const goRegister = () => navigate('/register');

  const subjectMeta = {
    chineseTech: { name: s.subjChineseTech, tag: s.subjChineseTechTag },
    chineseHum: { name: s.subjChineseHum, tag: s.subjChineseHumTag },
    math: { name: s.subjMath, tag: s.subjMathTag },
    physics: { name: s.subjPhysics, tag: s.subjPhysicsTag },
    chemistry: { name: s.subjChemistry, tag: s.subjChemistryTag },
  } as const;

  const features = [
    { icon: 'adaptive', t: s.fAdaptiveT, d: s.fAdaptiveD },
    { icon: 'analytics', t: s.fAnalyticsT, d: s.fAnalyticsD },
    { icon: 'mock', t: s.fMockT, d: s.fMockD },
    { icon: 'materials', t: s.fMaterialsT, d: s.fMaterialsD },
    { icon: 'ai', t: s.fAiT, d: s.fAiD },
    { icon: 'plan', t: s.fPlanT, d: s.fPlanD },
  ] as const;

  const packageMeta = {
    start: { name: s.pkgStart, for: s.pkgStartFor, subj: s.oneSubject },
    standard: { name: s.pkgStandard, for: s.pkgStandardFor, subj: s.twoSubjects },
    advanced: { name: s.pkgAdvanced, for: s.pkgAdvancedFor, subj: s.threeSubjects },
    full: { name: s.pkgFull, for: s.pkgFullFor, subj: s.allSubjects },
  } as const;

  const statMeta = {
    students: s.statStudents,
    questions: s.statQuestions,
    answered: s.statAnswered,
    success: s.statSuccess,
  } as const;

  return (
    <div className="csca-landing">
      <CscaNav />

      <div id="top" />

      {/* ═══ Hero ═══ */}
      <header className="csca-hero csca-wrap">
        <div className="csca-hero-hanzi csca-hanzi">学</div>
        <div className="csca-reveal">
          <span className="csca-eyebrow">{s.heroBadge}</span>
          <h1 className="csca-hero-title">
            {s.heroTitle} <span className="accent">{s.heroTitleAccent}</span>
          </h1>
          <p className="csca-hero-sub">{s.heroSub}</p>
          <div className="csca-hero-cta">
            <button className="csca-btn csca-btn-primary" onClick={goRegister}>{s.ctaStart}</button>
            <button className="csca-btn csca-btn-ghost" onClick={() => navigate('/csca/about')}>{s.ctaLearnMore}</button>
          </div>

          {/* Countdown */}
          <div className="csca-countdown">
            <div className="csca-cd-label">
              <b>{s.cdNextExam}</b>
              <span>{s.cdRegOpens}</span>
            </div>
            <div className="csca-cd-unit"><span className="csca-cd-num">{days}</span><span className="csca-cd-cap">{s.cdDays}</span></div>
            <div className="csca-cd-unit"><span className="csca-cd-num">{hours}</span><span className="csca-cd-cap">{s.cdHours}</span></div>
            <div className="csca-cd-unit"><span className="csca-cd-num">{minutes}</span><span className="csca-cd-cap">{s.cdMinutes}</span></div>
          </div>
        </div>

        <MistMountains style={{ position: 'absolute', left: 0, right: 0, bottom: -1, width: '100%', height: 160, color: 'var(--csca-red)', zIndex: 0 }} />
      </header>

      {/* ═══ Stats ═══ */}
      <section className="csca-wrap" style={{ marginTop: '-1rem' }}>
        <Reveal stagger className="csca-stats">
          {CSCA_STATS.map((st) => (
            <div className="csca-stat" key={st.key}>
              <div className="csca-stat-num csca-hanzi">{st.value}</div>
              <div className="csca-stat-cap">{statMeta[st.key]}</div>
            </div>
          ))}
        </Reveal>
      </section>

      {/* ═══ News ═══ */}
      <CscaNewsSection
        id="news"
        section
        title={s.newsTitle}
        lead={s.newsLead}
        readMore={s.newsReadMore}
        emptyText={s.newsEmpty}
        limit={6}
      />

      {/* ═══ About CSCA ═══ */}
      <section id="about" className="csca-section">
        <div className="csca-wrap">
          <div className="csca-section-head">
            <Reveal>
              <BrushDivider className="csca-brush-divider" style={{ maxWidth: 220, margin: '0 auto 1rem' }} />
              <span className="csca-eyebrow">{s.aboutLead}</span>
              <h2 className="csca-h2" style={{ marginTop: '0.8rem' }}>{s.aboutTitle}</h2>
              <p className="csca-lead">{s.aboutBody}</p>
            </Reveal>
          </div>

          {/* Subjects */}
          <div className="csca-section-head" style={{ marginBottom: '1.6rem' }}>
            <h3 className="csca-h2" style={{ fontSize: '1.5rem' }}>{s.subjectsTitle}</h3>
            <p className="csca-lead">{s.subjectsLead}</p>
          </div>
          <Reveal stagger className="csca-grid csca-grid-5">
            {CSCA_SUBJECTS.map((subj) => (
              <div className="csca-card csca-subject" key={subj.key}>
                {subj.required && <span className="csca-subject-badge">{s.required}</span>}
                <span className="csca-subject-hanzi">{subj.hanzi}</span>
                <div className="csca-subject-name">{subjectMeta[subj.key].name}</div>
                <div className="csca-subject-tag">{subjectMeta[subj.key].tag}</div>
              </div>
            ))}
          </Reveal>
          <div style={{ textAlign: 'center', marginTop: '1.6rem' }}>
            <button className="csca-btn csca-btn-ghost" onClick={goRegister}>{s.viewTopics}</button>
          </div>
        </div>
      </section>

      {/* ═══ Features ═══ */}
      <section id="features" className="csca-section" style={{ background: 'rgba(255,255,255,0.5)' }}>
        <div className="csca-wrap">
          <div className="csca-section-head">
            <h2 className="csca-h2">{s.featuresTitle}</h2>
            <p className="csca-lead">{s.featuresLead}</p>
          </div>
          <Reveal stagger className="csca-grid csca-grid-3">
            {features.map((f) => (
              <div className="csca-card" key={f.t}>
                <div className="csca-feature-title">{f.t}</div>
                <div className="csca-feature-desc">{f.d}</div>
              </div>
            ))}
          </Reveal>
        </div>
      </section>

      {/* ═══ Mock exams / packages ═══ */}
      <section id="mocks" className="csca-section">
        <div className="csca-wrap">
          <div className="csca-section-head">
            <h2 className="csca-h2">{s.mocksTitle}</h2>
            <p className="csca-lead">{s.mocksLead}</p>
          </div>

          {/* Free mock banner */}
          <Reveal>
            <div className="csca-card" style={{ display: 'flex', gap: '1.25rem', alignItems: 'center', flexWrap: 'wrap', marginBottom: '1.75rem', background: 'linear-gradient(135deg, rgba(200,16,46,0.06), rgba(201,162,75,0.08))' }}>
              <SealStamp text="免费" size={64} className="csca-seal-anim" />
              <div style={{ flex: 1, minWidth: 240 }}>
                <div className="csca-feature-title" style={{ fontSize: '1.2rem' }}>{s.freeMockTitle}</div>
                <div className="csca-feature-desc">{s.freeMockDesc}</div>
              </div>
              <button className="csca-btn csca-btn-primary" onClick={goRegister}>{s.getFree}</button>
            </div>
          </Reveal>

          <Reveal stagger className="csca-grid csca-grid-4">
            {CSCA_PACKAGES.map((pkg) => (
              <div className={`csca-card csca-price-card ${pkg.featured ? 'featured' : ''}`} key={pkg.key}>
                {pkg.featured && <span className="csca-price-flag">{s.popular}</span>}
                <div className="csca-price-name">{packageMeta[pkg.key].name}</div>
                <div className="csca-price-for">{packageMeta[pkg.key].for}</div>
                <div className="csca-price-amount csca-hanzi">
                  {pkg.price.toLocaleString('ru-RU')} <span className="csca-price-cur">{s.currency}</span>
                </div>
                <ul className="csca-price-list">
                  <li><CheckIcon /> {packageMeta[pkg.key].subj}</li>
                  <li><CheckIcon /> {s.pkgFeatAi}</li>
                  <li><CheckIcon /> {s.pkgFeatAnalytics}</li>
                  <li><CheckIcon /> {s.pkgFeatFull}</li>
                </ul>
                <button
                  className={`csca-btn ${pkg.featured ? 'csca-btn-primary' : 'csca-btn-ghost'}`}
                  style={{ width: '100%', marginTop: 'auto' }}
                  onClick={goRegister}
                >{s.buy}</button>
              </div>
            ))}
          </Reveal>
        </div>
      </section>

      {/* ═══ Materials ═══ */}
      <section id="materials" className="csca-section" style={{ background: 'rgba(255,255,255,0.5)' }}>
        <div className="csca-wrap">
          <div className="csca-section-head">
            <h2 className="csca-h2">{s.materialsTitle}</h2>
            <p className="csca-lead">{s.materialsLead}</p>
          </div>
          <Reveal stagger className="csca-grid csca-grid-5">
            {CSCA_SUBJECTS.map((subj) => (
              <div className="csca-card csca-book" key={subj.key}>
                <div className="csca-book-cover" style={{ background: subj.cover }}>
                  <span className="csca-book-hanzi">{subj.hanzi}</span>
                  <span className="csca-book-label csca-hanzi">CSCA · 备考教材</span>
                </div>
                <div className="csca-subject-name">{subjectMeta[subj.key].name}</div>
                <div className="csca-subject-tag" style={{ marginBottom: '0.75rem' }}>{s.bookLabel}</div>
                <div className="csca-price-amount csca-hanzi" style={{ fontSize: '1.4rem', margin: '0 0 0.6rem' }}>
                  {CSCA_BOOK_PRICE.toLocaleString('ru-RU')} <span className="csca-price-cur">{s.currency}</span>
                </div>
                <button className="csca-btn csca-btn-ghost csca-btn-sm" style={{ width: '100%' }} onClick={goRegister}>{s.addToCart}</button>
              </div>
            ))}
          </Reveal>
          <div className="csca-card" style={{ marginTop: '1.75rem', textAlign: 'center', background: 'linear-gradient(135deg, rgba(200,16,46,0.05), rgba(201,162,75,0.08))' }}>
            <div className="csca-feature-title" style={{ fontSize: '1.15rem' }}>{s.freePdfTitle}</div>
            <div className="csca-feature-desc" style={{ maxWidth: 560, margin: '0.4rem auto 0' }}>{s.freePdfDesc}</div>
          </div>
        </div>
      </section>

      {/* ═══ CTA band ═══ */}
      <section className="csca-wrap csca-section" style={{ paddingTop: '1rem' }}>
        <Reveal variant="zoom">
          <div className="csca-cta-band">
            <span className="csca-hanzi-bg csca-hanzi">越</span>
            <h2>{s.ctaBandTitle}</h2>
            <p>{s.ctaBandDesc}</p>
            <button className="csca-btn" style={{ background: '#fff', color: 'var(--csca-red)' }} onClick={goRegister}>{s.ctaBandBtn}</button>
          </div>
        </Reveal>
      </section>

      {/* ═══ Footer ═══ */}
      <CscaFooter />
    </div>
  );
}

export default CscaLandingPage;
