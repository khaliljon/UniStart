import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell, { CscaPageHero } from '../components/csca/CscaPageShell';
import { BrushDivider } from '../components/csca/ChineseMotifs';
import { CSCA_SUBJECTS, CSCA_EXAM_SITTINGS } from '../cscaConfig';
import { examSittingsService } from '../services/examSittingsService';
import AddToCalendarButton from '../components/csca/AddToCalendarButton';

function AboutCscaPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const now = new Date();

  const subjectMeta = {
    chineseTech: { name: s.subjChineseTech, tag: s.subjChineseTechTag },
    chineseHum: { name: s.subjChineseHum, tag: s.subjChineseHumTag },
    math: { name: s.subjMath, tag: s.subjMathTag },
    physics: { name: s.subjPhysics, tag: s.subjPhysicsTag },
    chemistry: { name: s.subjChemistry, tag: s.subjChemistryTag },
  } as const;

  const monthLabel: Record<string, string> = {
    january: s.monthJanuary, march: s.monthMarch, june: s.monthJune,
    september: s.monthSeptember, november: s.monthNovember,
  };
  void monthLabel;

  const localeTag = locale === 'en' ? 'en-US' : locale === 'kz' ? 'kk-KZ' : 'ru-RU';
  const [sittings, setSittings] = useState<{ date: string }[]>(CSCA_EXAM_SITTINGS.map((x) => ({ date: x.date })));
  useEffect(() => {
    examSittingsService.list()
      .then((list) => { if (list.length > 0) setSittings(list.map((x) => ({ date: x.date }))); })
      .catch(() => {});
  }, []);
  const monthName = (iso: string) => {
    const m = new Date(iso).toLocaleDateString(localeTag, { month: 'long' });
    return m.charAt(0).toUpperCase() + m.slice(1);
  };

  return (
    <CscaPageShell>
      <CscaPageHero eyebrow={s.aboutLead} title={s.aboutTitle} />

      {/* About body */}
      <section className="csca-wrap" style={{ paddingBottom: '1rem' }}>
        <BrushDivider className="csca-brush-divider" style={{ maxWidth: 220, margin: '0 auto 1.5rem' }} />
        <p className="csca-lead" style={{ textAlign: 'center', maxWidth: 760, margin: '0 auto' }}>{s.aboutBody}</p>
      </section>

      {/* Subjects */}
      <section className="csca-section">
        <div className="csca-wrap">
          <div className="csca-section-head">
            <h2 className="csca-h2">{s.subjectsTitle}</h2>
            <p className="csca-lead">{s.subjectsLead}</p>
          </div>
          <div className="csca-grid csca-grid-5">
            {CSCA_SUBJECTS.map((subj) => (
              <div className="csca-card csca-subject" key={subj.key}>
                {subj.required && <span className="csca-subject-badge">{s.required}</span>}
                <span className="csca-subject-hanzi">{subj.hanzi}</span>
                <div className="csca-subject-name">{subjectMeta[subj.key].name}</div>
                <div className="csca-subject-tag">{subjectMeta[subj.key].tag}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Exam dates */}
      <section className="csca-section" style={{ background: 'rgba(255,255,255,0.5)' }}>
        <div className="csca-wrap">
          <div className="csca-section-head">
            <h2 className="csca-h2">{s.examDatesTitle}</h2>
            <p className="csca-lead">{s.examDatesLead}</p>
          </div>
          <div style={{ maxWidth: 640, margin: '0 auto', display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {sittings.map((sit) => {
              const d = new Date(sit.date);
              const past = d.getTime() < now.getTime();
              return (
                <div key={sit.date} className="csca-card" style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '1rem 1.25rem' }}>
                  <div>
                    <div style={{ fontWeight: 700, color: 'var(--csca-ink)' }}>{monthName(sit.date)}</div>
                    <div style={{ fontSize: '0.85rem', color: 'var(--csca-ink-soft)' }}>
                      {d.toLocaleDateString(localeTag, { day: 'numeric', month: 'long', year: 'numeric' })}
                    </div>
                  </div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
                    {!past && (
                      <AddToCalendarButton
                        label={s.addToCalendar}
                        googleLabel={s.calGoogle}
                        appleLabel={s.calApple}
                        fileName={`csca-${sit.date}.ics`}
                        event={{
                          title: `CSCA — ${monthName(sit.date)}`,
                          description: s.examDatesLead,
                          date: sit.date,
                        }}
                      />
                    )}
                    <span style={{
                      fontSize: '0.78rem', fontWeight: 700, padding: '0.28rem 0.8rem', borderRadius: 999,
                      color: past ? 'var(--csca-ink-soft)' : '#fff',
                      background: past ? 'var(--csca-cloud)' : 'var(--csca-red)',
                    }}>
                      {past ? s.statusCompleted : s.statusUpcoming}
                    </span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="csca-wrap csca-section">
        <div className="csca-cta-band">
          <span className="csca-hanzi-bg csca-hanzi">越</span>
          <h2>{s.ctaBandTitle}</h2>
          <p>{s.ctaBandDesc}</p>
          <button className="csca-btn" style={{ background: '#fff', color: 'var(--csca-red)' }} onClick={() => navigate('/register')}>{s.ctaBandBtn}</button>
        </div>
      </section>
    </CscaPageShell>
  );
}

export default AboutCscaPage;
