import { useEffect, useState } from 'react';
import { useTranslation } from '../../i18n';
import { cscaStrings } from '../../i18n/csca';
import { CSCA_FEE, type CscaSubjectKey } from '../../cscaConfig';
import { specialtyTrackService, type SpecialtyTrack } from '../../services/specialtyTrackService';
import { pickLocalized } from '../../utils/localize';
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
  const [tracks, setTracks] = useState<SpecialtyTrack[]>([]);

  useEffect(() => {
    let alive = true;
    specialtyTrackService.list()
      .then((data) => { if (alive) setTracks(data); })
      .catch(() => { if (alive) setTracks([]); });
    return () => { alive = false; };
  }, []);

  const subjectName: Record<CscaSubjectKey, string> = {
    math: s.subjMath,
    physics: s.subjPhysics,
    chemistry: s.subjChemistry,
    chineseTech: s.subjChineseTech,
    chineseHum: s.subjChineseHum,
  };

  if (tracks.length === 0) return null;

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
        {tracks.map((track) => (
          <div className="csca-card csca-track" key={track.id}>
            <div className="csca-track-name">{pickLocalized(track.name, track.nameKz, track.nameEn, locale)}</div>
            <div className="csca-track-subjects">
              {track.subjects.map((k) => (
                <span className="csca-track-chip" key={k}>{subjectName[k as CscaSubjectKey] ?? k}</span>
              ))}
            </div>
            {track.conditionalChinese && <div className="csca-track-note">{s.trackIfChinese}</div>}
          </div>
        ))}
      </div>

      <div className="csca-track-footer">
        <div className="csca-fee">
          <span className="csca-fee-title">{s.feeTitle}</span>
          <span><b>{CSCA_FEE.single} ¥</b> — {s.feeSingle}</span>
          <span><b>{CSCA_FEE.multiple} ¥</b> — {s.feeMultiple}</span>
        </div>
        <p className="csca-track-disclaimer">{s.tracksNote}</p>
      </div>
    </div>
  );

  if (!section) return inner;
  return <section className="csca-section" style={background ? { background } : undefined}>{inner}</section>;
}
