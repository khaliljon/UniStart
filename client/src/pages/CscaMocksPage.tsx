import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell, { CscaPageHero } from '../components/csca/CscaPageShell';
import { SealStamp } from '../components/csca/ChineseMotifs';
import { CSCA_PACKAGES } from '../cscaConfig';

const CheckIcon = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M20 6L9 17l-5-5" />
  </svg>
);

function CscaMocksPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  const packageMeta = {
    start: { name: s.pkgStart, for: s.pkgStartFor, subj: s.oneSubject },
    standard: { name: s.pkgStandard, for: s.pkgStandardFor, subj: s.twoSubjects },
    advanced: { name: s.pkgAdvanced, for: s.pkgAdvancedFor, subj: s.threeSubjects },
    full: { name: s.pkgFull, for: s.pkgFullFor, subj: s.allSubjects },
  } as const;

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

        <div className="csca-grid csca-grid-4">
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
                onClick={() => navigate('/register')}
              >{s.buy}</button>
            </div>
          ))}
        </div>
      </section>
    </CscaPageShell>
  );
}

export default CscaMocksPage;
