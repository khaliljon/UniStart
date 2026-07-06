import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell, { CscaPageHero } from '../components/csca/CscaPageShell';
import { BrushDivider } from '../components/csca/ChineseMotifs';
import { CSCA_STATS } from '../cscaConfig';

function AboutUsPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  const statMeta = {
    students: s.statStudents, questions: s.statQuestions, answered: s.statAnswered, success: s.statSuccess,
  } as const;

  return (
    <CscaPageShell>
      <CscaPageHero eyebrow="UNISTART" title={s.aboutUsTitle} lead={s.aboutUsLead} />

      <section className="csca-wrap" style={{ paddingBottom: '1rem' }}>
        <BrushDivider className="csca-brush-divider" style={{ maxWidth: 220, margin: '0 auto 1.5rem' }} />
        <p className="csca-lead" style={{ textAlign: 'center', maxWidth: 760, margin: '0 auto' }}>{s.aboutUsMission}</p>
      </section>

      {/* Goal highlight */}
      <section className="csca-wrap csca-section" style={{ paddingTop: '1rem' }}>
        <div className="csca-cta-band" style={{ textAlign: 'center' }}>
          <span className="csca-hanzi-bg csca-hanzi">一</span>
          <h2 style={{ maxWidth: 640, margin: '0 auto' }}>{s.aboutUsGoal}</h2>
        </div>
      </section>

      {/* Stats */}
      <section className="csca-wrap csca-section" style={{ paddingTop: 0 }}>
        <div className="csca-stats">
          {CSCA_STATS.map((st) => (
            <div className="csca-stat" key={st.key}>
              <div className="csca-stat-num csca-hanzi">{st.value}</div>
              <div className="csca-stat-cap">{statMeta[st.key]}</div>
            </div>
          ))}
        </div>
        <div style={{ textAlign: 'center', marginTop: '2rem' }}>
          <button className="csca-btn csca-btn-primary" onClick={() => navigate('/register')}>{s.ctaStart}</button>
        </div>
      </section>
    </CscaPageShell>
  );
}

export default AboutUsPage;
