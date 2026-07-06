import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell, { CscaPageHero } from '../components/csca/CscaPageShell';

function CscaCoursesPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  const features = [
    { t: s.fAdaptiveT, d: s.fAdaptiveD },
    { t: s.fAnalyticsT, d: s.fAnalyticsD },
    { t: s.fMockT, d: s.fMockD },
    { t: s.fMaterialsT, d: s.fMaterialsD },
    { t: s.fAiT, d: s.fAiD },
    { t: s.fPlanT, d: s.fPlanD },
  ];

  return (
    <CscaPageShell>
      <CscaPageHero eyebrow={s.navCourses} title={s.featuresTitle} lead={s.coursesLead2} />

      <section className="csca-wrap csca-section" style={{ paddingTop: '1.5rem' }}>
        <div className="csca-grid csca-grid-2" style={{ marginBottom: '2rem' }}>
          {[{ title: s.coursesFree, cta: s.getFree, note: '' }, { title: s.coursesPaid, cta: s.buy, note: s.coursesPriceNote }].map((c) => (
            <div className="csca-card" key={c.title} style={{ textAlign: 'center', position: 'relative' }}>
              <span className="csca-subject-badge">{s.coursesComingSoon}</span>
              <div className="csca-subject-hanzi" style={{ fontSize: '2rem' }}>{c.title === s.coursesFree ? '免费' : '课程'}</div>
              <div className="csca-feature-title" style={{ fontSize: '1.2rem' }}>{c.title}</div>
              {c.note && <div className="csca-feature-desc" style={{ maxWidth: 360, margin: '0.4rem auto 0' }}>{c.note}</div>}
              <button className="csca-btn csca-btn-ghost csca-btn-sm" style={{ marginTop: '0.8rem' }} onClick={() => navigate('/register')}>{c.cta}</button>
            </div>
          ))}
        </div>

        <div className="csca-grid csca-grid-3">
          {features.map((f) => (
            <div className="csca-card" key={f.t}>
              <div className="csca-feature-title">{f.t}</div>
              <div className="csca-feature-desc">{f.d}</div>
            </div>
          ))}
        </div>
      </section>
    </CscaPageShell>
  );
}

export default CscaCoursesPage;
