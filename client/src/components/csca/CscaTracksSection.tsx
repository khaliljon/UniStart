import { useTranslation } from '../../i18n';
import { cscaStrings } from '../../i18n/csca';
import { CSCA_TRACKS, CSCA_FEE, type CscaSubjectKey } from '../../cscaConfig';
import { BrushDivider } from './ChineseMotifs';
import Reveal from './Reveal';

interface Props {
  /** Render inside its own <section> with the standard CSCA spacing. */
  section?: boolean;
  background?: string;
}

export default function CscaTracksSection({ section = true, background }: Props) {
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  const subjectName: Record<CscaSubjectKey, string> = {
    math: s.subjMath,
    physics: s.subjPhysics,
    chemistry: s.subjChemistry,
    chineseTech: s.subjChineseTech,
    chineseHum: s.subjChineseHum,
  };

  const trackName: Record<(typeof CSCA_TRACKS)[number]['key'], string> = {
    it: s.trackIt,
    engineering: s.trackEngineering,
    chemistry: s.trackChemistry,
    medicine: s.trackMedicine,
    economics: s.trackEconomics,
    international: s.trackInternational,
    humanities: s.trackHumanities,
    architecture: s.trackArchitecture,
    mechanical: s.trackMechanical,
  };

  const inner = (
    <div className="csca-wrap">
      <div className="csca-section-head">
        <Reveal>
          <BrushDivider className="csca-brush-divider" style={{ maxWidth: 220, margin: '0 auto 1rem' }} />
          <h2 className="csca-h2">{s.tracksTitle}</h2>
          <p className="csca-lead">{s.tracksLead}</p>
        </Reveal>
      </div>

      <div className="csca-tracks-grid">
        {CSCA_TRACKS.map((track) => (
          <div className="csca-card csca-track" key={track.key}>
            <span className="csca-track-icon" aria-hidden="true">{track.icon}</span>
            <div className="csca-track-name">{trackName[track.key]}</div>
            <div className="csca-track-subjects">
              {track.subjects.map((k) => (
                <span className="csca-track-chip" key={k}>{subjectName[k]}</span>
              ))}
            </div>
            {track.conditional && <div className="csca-track-note">{s.trackIfChinese}</div>}
          </div>
        ))}
      </div>

      <div className="csca-track-footer">
        <div className="csca-fee">
          <span className="csca-fee-title">{s.feeTitle}</span>
          <span><b>{CSCA_FEE.single} ¥</b> — {s.feeSingle}</span>
          <span><b>{CSCA_FEE.multiple} ¥</b> — {s.feeMultiple}</span>
        </div>
        <p className="csca-track-disclaimer">⚠️ {s.tracksNote}</p>
      </div>
    </div>
  );

  if (!section) return inner;
  return <section className="csca-section" style={background ? { background } : undefined}>{inner}</section>;
}
