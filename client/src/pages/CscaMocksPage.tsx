import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell, { CscaPageHero } from '../components/csca/CscaPageShell';
import { SealStamp } from '../components/csca/ChineseMotifs';
import { mockCatalogService, type MockCatalog, type MockTemplate } from '../services/mockCatalogService';
import { useAppSelector } from '../hooks/useAppSelector';
import { moks } from '../utils/plural';
import { pickLocalized } from '../utils/localize';

const CheckIcon = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M20 6L9 17l-5-5" />
  </svg>
);

function CscaMocksPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const { isAuthenticated } = useAppSelector((st) => st.auth);

  const [catalog, setCatalog] = useState<MockCatalog | null>(null);
  useEffect(() => {
    mockCatalogService.getCatalog()
      .then(setCatalog)
      .catch(() => setCatalog({ freeRunAvailable: false, templates: [], packages: [] }));
  }, []);

  const subjectsLabel = (pickCount: number) =>
    pickCount === 0 ? s.allSubjects : pickCount === 1 ? s.oneSubject : pickCount === 2 ? s.twoSubjects : s.threeSubjects;

  const buyMockTier = (tpl: MockTemplate) => {
    sessionStorage.setItem('focusMock', String(tpl.mockExamId));
    navigate(isAuthenticated ? '/' : '/register');
  };

  return (
    <CscaPageShell>
      <CscaPageHero eyebrow={s.navMocks} title={s.mocksTitle} lead={s.mocksLead} />

      <section className="csca-wrap csca-section" style={{ paddingTop: '1.5rem' }}>
        {/* Free mock banner */}
        <div className="csca-card" style={{ display: 'flex', gap: '1.25rem', alignItems: 'center', flexWrap: 'wrap', marginBottom: '1.75rem', background: 'linear-gradient(135deg, rgba(200,16,46,0.06), rgba(201,162,75,0.08))' }}>
          <SealStamp text="免费" size={64} />
          <div style={{ flex: 1, minWidth: 240 }}>
            <div className="csca-feature-title" style={{ fontSize: '1.2rem' }}>{s.freeMockTitle}</div>
            <div className="csca-feature-desc">{s.freeMockDesc}</div>
          </div>
          <button className="csca-btn csca-btn-primary" onClick={() => navigate('/register')}>{s.getFree}</button>
        </div>

        {catalog === null ? (
          <div className="loading"><div className="spinner" /></div>
        ) : catalog.packages.length === 0 && catalog.templates.length === 0 ? (
          <div className="csca-card" style={{ textAlign: 'center' }}>
            <p className="csca-lead" style={{ margin: 0 }}>Пробники скоро появятся — мы работаем над этим.</p>
          </div>
        ) : (
        <>
        {/* Individual mocks per subject */}
        {catalog.templates.length > 0 && (
          <>
            <h3 className="csca-h3" style={{ fontSize: '1.15rem', margin: '0 0 1rem' }}>{s.singleMocksTitle}</h3>
            <div className="csca-grid" style={{ gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', marginBottom: '2rem' }}>
              {catalog.templates.map((tpl) => (
                <div className="csca-card" key={tpl.mockExamId} style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                  <div className="csca-price-name" style={{ fontSize: '1.05rem' }}>{pickLocalized(tpl.title, tpl.titleKz, tpl.titleEn, locale)}</div>
                  <div className="csca-subject-tag" style={{ marginBottom: '0.35rem' }}>{tpl.totalQuestions} {s.mockQuestions.toLowerCase()} · {tpl.totalTimeMinutes} {s.minShort}</div>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem', marginTop: 'auto' }}>
                    {tpl.tiers.map((tier) => (
                      <button key={tier.id} className="csca-btn csca-btn-ghost csca-btn-sm"
                              style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '0.5rem' }}
                              onClick={() => buyMockTier(tpl)}>
                        <span>{moks(tier.runs, locale)}</span>
                        <span style={{ fontWeight: 800, color: 'var(--csca-red, #C8102E)', whiteSpace: 'nowrap' }}>{tier.price.toLocaleString('ru-RU')} {tier.currency}</span>
                      </button>
                    ))}
                  </div>
                </div>
              ))}
            </div>
            <h3 className="csca-h3" style={{ fontSize: '1.15rem', margin: '0 0 1rem' }}>{s.discountPackages}</h3>
          </>
        )}
        <div className="csca-grid csca-grid-4">
          {catalog.packages.map((pkg) => (
            <div className="csca-card csca-price-card" key={pkg.key}>
              <div className="csca-price-name">{pickLocalized(pkg.name, pkg.nameKz, pkg.nameEn, locale)}</div>
              <div className="csca-price-for">{subjectsLabel(pkg.pickCount)} × {moks(pkg.runsEach, locale)}</div>
              <div className="csca-price-amount csca-hanzi">
                {pkg.price.toLocaleString('ru-RU')} <span className="csca-price-cur">{pkg.currency}</span>
              </div>
              <ul className="csca-price-list">
                <li><CheckIcon /> {subjectsLabel(pkg.pickCount)}</li>
                <li><CheckIcon /> {moks(pkg.runsEach, locale)} {s.perSubject}</li>
                <li><CheckIcon /> {s.pkgFeatAi}</li>
              </ul>
              <button
                className="csca-btn csca-btn-primary"
                style={{ width: '100%', marginTop: 'auto' }}
                onClick={() => navigate('/register')}
              >{s.buy}</button>
            </div>
          ))}
        </div>
        </>
        )}
      </section>
    </CscaPageShell>
  );
}

export default CscaMocksPage;
