import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell, { CscaPageHero } from '../components/csca/CscaPageShell';
import { SealStamp } from '../components/csca/ChineseMotifs';
import { mockCatalogService, type MockCatalog } from '../services/mockCatalogService';
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

  const [catalog, setCatalog] = useState<MockCatalog | null>(null);
  useEffect(() => {
    mockCatalogService.getCatalog()
      .then(setCatalog)
      .catch(() => setCatalog({ freeRunAvailable: false, templates: [], packages: [] }));
  }, []);

  const subjectsLabel = (pickCount: number) =>
    pickCount === 0 ? s.allSubjects : pickCount === 1 ? s.oneSubject : pickCount === 2 ? s.twoSubjects : s.threeSubjects;

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
        ) : catalog.packages.length === 0 ? (
          <div className="csca-card" style={{ textAlign: 'center' }}>
            <p className="csca-lead" style={{ margin: 0 }}>Пробники скоро появятся — мы работаем над этим.</p>
          </div>
        ) : (
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
        )}
      </section>
    </CscaPageShell>
  );
}

export default CscaMocksPage;
