import { downloadIcs, googleCalendarUrl, type CalendarEvent } from '../../utils/calendar';

interface Props {
  event: CalendarEvent;
  /** Kept for API compatibility; not rendered (buttons are shown inline). */
  label?: string;
  googleLabel: string;
  appleLabel: string;
  fileName?: string;
}

/**
 * Two inline "add to calendar" actions (no dropdown, so nothing gets clipped):
 *  - Google Calendar (opens web — saves to the user's Google account on phone & PC)
 *  - Apple / Outlook via .ics download (opens the native calendar app)
 */
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
