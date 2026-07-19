import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import { useAppSelector } from '../hooks/useAppSelector';
import { materialsService } from '../services/materialsService';
import { mockCatalogService, type MockPackage } from '../services/mockCatalogService';
import { purchaseService } from '../services/purchaseService';
import { examSittingsService } from '../services/examSittingsService';
import { cartService } from '../services/cartService';
import { moks } from '../utils/plural';
import CscaNav from '../components/csca/CscaNav';
import CscaFooter from '../components/csca/CscaFooter';
import Reveal from '../components/csca/Reveal';
import {
  BrushDivider, MistMountains, SealStamp,
} from '../components/csca/ChineseMotifs';
import {
  CSCA_SUBJECTS,
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
  const { isAuthenticated } = useAppSelector((s) => s.auth);
  const [dbMaterials, setDbMaterials] = useState<{ id: number; subjectKey: string; title: string; price: number }[]>([]);
  const [dbPackages, setDbPackages] = useState<MockPackage[] | null>(null);
  const [ownedBooks, setOwnedBooks] = useState<Set<string>>(new Set());

  const s = cscaStrings[locale];
  const [nextDate, setNextDate] = useState<string>(getNextSitting().date);
  const { days, hours, minutes } = useCountdown(nextDate);

  useEffect(() => {
    materialsService.list().then(setDbMaterials).catch(() => {});
    mockCatalogService.getCatalog().then((c) => setDbPackages(c.packages)).catch(() => setDbPackages([]));
    examSittingsService.list().then((list) => {
      if (list.length === 0) return;
      const now = Date.now();
      const upcoming = list.find((x) => new Date(x.date).getTime() >= now) ?? list[list.length - 1];
      setNextDate(upcoming.date);
    }).catch(() => {});
    if (isAuthenticated) {
      purchaseService.list()
        .then((ps) => setOwnedBooks(new Set(ps.filter((p) => p.itemType === 'book').map((p) => p.itemCode))))
        .catch(() => {});
    }
  }, [isAuthenticated]);

  // Scroll to a hash target (e.g. #news) when navigating from another page.
  useEffect(() => {
    if (location.hash) {
      const el = document.getElementById(location.hash.slice(1));
      if (el) setTimeout(() => el.scrollIntoView({ behavior: 'smooth', block: 'start' }), 50);
    }
  }, [location.hash]);

  const goRegister = () => navigate('/register');

  const subjectMeta = {
    chineseTech: { name: s.subjChineseTech, tag: s.subjChineseTechTag },
    chineseHum: { name: s.subjChineseHum, tag: s.subjChineseHumTag },
    math: { name: s.subjMath, tag: s.subjMathTag },
    physics: { name: s.subjPhysics, tag: s.subjPhysicsTag },
    chemistry: { name: s.subjChemistry, tag: s.subjChemistryTag },
  } as const;

  const features = [
    { icon: 'mock', t: s.fMockT, d: s.fMockD },
    { icon: 'materials', t: s.fMaterialsT, d: s.fMaterialsD },
    { icon: 'ai', t: s.fAiT, d: s.fAiD },
  ] as const;

  // Packages may require choosing subjects. We carry the chosen package into the
  // storefront (MockShop on the home page): it opens the subject picker (or adds
  // straight to the cart for "all subjects" packages) and pays via Polar.
  const buyPackage = (pkg: MockPackage) => {
    sessionStorage.setItem('buyPackage', pkg.key);
    navigate(isAuthenticated ? '/' : '/register');
  };

  const buyBook = (mat: { id: number; subjectKey: string; title: string; price: number }) => {
    // Already owned → send to the library instead of charging again.
    if (isAuthenticated && ownedBooks.has(String(mat.id))) {
      navigate('/materials');
      return;
    }
    const item = {
      itemType: 'book',
      itemCode: String(mat.id),
      title: mat.title || `${subjectMeta[mat.subjectKey as keyof typeof subjectMeta]?.name ?? ''} · ${s.bookLabel}`,
      subjects: mat.subjectKey,
      amount: mat.price,
      currency: '₸',
    };
    if (isAuthenticated) {
      cartService.add(item);
      navigate('/cart');
      return;
    }
    // Guest: remember intent, register, then the dashboard adds it to the cart.
    sessionStorage.setItem('checkout', JSON.stringify(item));
    navigate('/register');
  };

  const statMeta = {
    students: s.statStudents,
    questions: s.statQuestions,
    answered: s.statAnswered,
    success: s.statSuccess,
  } as const;
  void statMeta; // kept for future use

  return (
    <div className="csca-landing">
      <CscaNav />

      <div id="top" />

      {/* ═══ Hero ═══ */}
      <header className="csca-hero csca-wrap">
        <div className="csca-hero-hanzi csca-hanzi">学</div>
        <img
          src="/emblem-lg.png"
          alt=""
          aria-hidden="true"
          className="csca-hero-emblem"
          style={{
            position: 'absolute', right: 'clamp(-220px, -14vw, -120px)', top: '50%',
            transform: 'translateY(-50%)', width: 'min(560px, 48vw)', height: 'auto',
            opacity: 0.16, zIndex: 0, mixBlendMode: 'multiply',
            pointerEvents: 'none', userSelect: 'none',
          }}
        />
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

          {dbPackages === null ? (
            <div className="loading"><div className="spinner" /></div>
          ) : dbPackages.length === 0 ? (
            <div className="csca-card" style={{ textAlign: 'center' }}>
              <p className="csca-lead" style={{ margin: 0 }}>Пробники скоро появятся — мы работаем над этим.</p>
            </div>
          ) : (
          <Reveal stagger className="csca-grid csca-grid-4">
            {dbPackages.map((pkg) => {
              const featured = pkg.key === 'standard';
              const subjLabel = pkg.pickCount === 0 ? s.allSubjects
                : pkg.pickCount === 1 ? s.oneSubject
                : pkg.pickCount === 2 ? s.twoSubjects
                : pkg.pickCount === 3 ? s.threeSubjects
                : `${pkg.pickCount}`;
              return (
              <div className={`csca-card csca-price-card ${featured ? 'featured' : ''}`} key={pkg.key}>
                {featured && <span className="csca-price-flag">{s.popular}</span>}
                <div className="csca-price-name">{pkg.name}</div>
                <div className="csca-price-for">{subjLabel} × {moks(pkg.runsEach)}</div>
                <div className="csca-price-amount csca-hanzi">
                  {pkg.price.toLocaleString('ru-RU')} <span className="csca-price-cur">{s.currency}</span>
                </div>
                <ul className="csca-price-list">
                  <li><CheckIcon /> {subjLabel}</li>
                  <li><CheckIcon /> {s.pkgFeatAi}</li>
                </ul>
                <button
                  className={`csca-btn ${featured ? 'csca-btn-primary' : 'csca-btn-ghost'}`}
                  style={{ width: '100%', marginTop: 'auto' }}
                  onClick={() => buyPackage(pkg)}
                >{s.buy}</button>
              </div>
              );
            })}
          </Reveal>
          )}
        </div>
      </section>

      {/* ═══ Materials ═══ */}
      <section id="materials" className="csca-section" style={{ background: 'rgba(255,255,255,0.5)' }}>
        <div className="csca-wrap">
          <div className="csca-section-head">
            <h2 className="csca-h2">{s.materialsTitle}</h2>
            <p className="csca-lead">{s.materialsLead}</p>
          </div>
          {dbMaterials.length === 0 ? (
            <div className="csca-card" style={{ textAlign: 'center' }}>
              <p className="csca-lead" style={{ margin: 0 }}>Учебные материалы скоро появятся — мы работаем над этим.</p>
            </div>
          ) : (
          <Reveal stagger className="csca-grid csca-grid-5">
            {dbMaterials.map((mat) => {
              const subj = CSCA_SUBJECTS.find(cs => cs.key === mat.subjectKey) ?? CSCA_SUBJECTS[0];
              const name = subjectMeta[subj.key as keyof typeof subjectMeta]?.name ?? mat.title;
              return (
                <div className="csca-card csca-book" key={mat.id}>
                  <div className="csca-book-cover" style={{ background: subj.cover }}>
                    <span className="csca-book-hanzi">{subj.hanzi}</span>
                    <span className="csca-book-label csca-hanzi">CSCA · 备考教材</span>
                  </div>
                  <div className="csca-subject-name">{mat.title || name}</div>
                  <div className="csca-subject-tag" style={{ marginBottom: '0.75rem' }}>{s.bookLabel}</div>
                  <div className="csca-price-amount csca-hanzi" style={{ fontSize: '1.4rem', margin: '0 0 0.6rem' }}>
                    {mat.price.toLocaleString('ru-RU')} <span className="csca-price-cur">{s.currency}</span>
                  </div>
                  <button className="csca-btn csca-btn-ghost csca-btn-sm" style={{ width: '100%' }} onClick={() => buyBook(mat)}>{isAuthenticated && ownedBooks.has(String(mat.id)) ? 'Открыть' : s.buy}</button>
                </div>
              );
            })}
          </Reveal>
          )}        </div>
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
