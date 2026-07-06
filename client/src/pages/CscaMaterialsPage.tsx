import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell, { CscaPageHero } from '../components/csca/CscaPageShell';
import { CSCA_SUBJECTS, CSCA_BOOK_PRICE } from '../cscaConfig';

function CscaMaterialsPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  const subjectName = {
    chinese: s.subjChinese, math: s.subjMath, physics: s.subjPhysics, chemistry: s.subjChemistry,
  } as const;

  return (
    <CscaPageShell>
      <CscaPageHero eyebrow={s.navMaterials} title={s.materialsTitle} lead={s.materialsLead} />

      <section className="csca-wrap csca-section" style={{ paddingTop: '1.5rem' }}>
        <div className="csca-grid csca-grid-4">
          {CSCA_SUBJECTS.map((subj) => (
            <div className="csca-card csca-book" key={subj.key}>
              <div className="csca-book-cover" style={{ background: subj.cover }}>
                <span className="csca-book-hanzi">{subj.hanzi}</span>
                <span className="csca-book-label csca-hanzi">CSCA · 备考教材</span>
              </div>
              <div className="csca-subject-name">{subjectName[subj.key]}</div>
              <div className="csca-subject-tag" style={{ marginBottom: '0.75rem' }}>{s.bookLabel}</div>
              <div className="csca-price-amount csca-hanzi" style={{ fontSize: '1.4rem', margin: '0 0 0.6rem' }}>
                {CSCA_BOOK_PRICE.toLocaleString('ru-RU')} <span className="csca-price-cur">{s.currency}</span>
              </div>
              <button className="csca-btn csca-btn-ghost csca-btn-sm" style={{ width: '100%' }} onClick={() => navigate('/register')}>{s.addToCart}</button>
            </div>
          ))}
        </div>

        <div className="csca-card" style={{ marginTop: '1.75rem', textAlign: 'center', background: 'linear-gradient(135deg, rgba(200,16,46,0.05), rgba(201,162,75,0.08))' }}>
          <div className="csca-feature-title" style={{ fontSize: '1.15rem' }}>{s.freePdfTitle}</div>
          <div className="csca-feature-desc" style={{ maxWidth: 560, margin: '0.4rem auto 1rem' }}>{s.freePdfDesc}</div>
          <button className="csca-btn csca-btn-primary" onClick={() => navigate('/register')}>{s.getFree}</button>
        </div>
      </section>
    </CscaPageShell>
  );
}

export default CscaMaterialsPage;
