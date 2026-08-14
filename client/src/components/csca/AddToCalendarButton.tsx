import { downloadIcs, googleCalendarUrl, type CalendarEvent } from '../../utils/calendar';

interface Props {
  event: CalendarEvent;
  label?: string;
  googleLabel: string;
  appleLabel: string;
  fileName?: string;
}

export default function AddToCalendarButton({ event, googleLabel, appleLabel, fileName }: Props) {
  return (
    <div style={{ display: 'inline-flex', gap: '0.4rem', flexWrap: 'wrap' }}>
      <a
        className="csca-btn csca-btn-ghost csca-btn-sm"
        href={googleCalendarUrl(event)}
        target="_blank"
        rel="noopener noreferrer"
      >
        {googleLabel}
      </a>
      <button
        type="button"
        className="csca-btn csca-btn-ghost csca-btn-sm"
        onClick={() => downloadIcs(event, fileName)}
      >
        {appleLabel}
      </button>
    </div>
  );
}
