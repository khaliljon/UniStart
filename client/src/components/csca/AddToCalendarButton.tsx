import { useEffect, useRef, useState } from 'react';
import { downloadIcs, googleCalendarUrl, type CalendarEvent } from '../../utils/calendar';

interface Props {
  event: CalendarEvent;
  label: string;
  googleLabel: string;
  appleLabel: string;
  fileName?: string;
}

/**
 * "Add to calendar" button with a small menu:
 *  - Google Calendar (opens web — saves to the user's Google account on phone & PC)
 *  - Apple / Outlook via .ics download (opens the native calendar app)
 */
export default function AddToCalendarButton({ event, label, googleLabel, appleLabel, fileName }: Props) {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const menuItem: React.CSSProperties = {
    display: 'block', width: '100%', textAlign: 'left', padding: '0.5rem 0.9rem',
    fontSize: '0.85rem', background: 'none', border: 'none', cursor: 'pointer',
    color: 'var(--csca-ink, var(--text-primary))', textDecoration: 'none', whiteSpace: 'nowrap',
  };

  return (
    <div ref={ref} style={{ position: 'relative', display: 'inline-block' }}>
      <button type="button" className="csca-btn csca-btn-ghost csca-btn-sm" onClick={() => setOpen((o) => !o)}>
        {label}
      </button>
      {open && (
        <div style={{
          position: 'absolute', top: '100%', right: 0, marginTop: '0.35rem', zIndex: 30,
          background: '#fff', borderRadius: '0.6rem', boxShadow: '0 10px 30px rgba(0,0,0,0.15)',
          border: '1px solid rgba(26,26,26,0.08)', overflow: 'hidden', minWidth: 190,
        }}>
          <a
            style={menuItem}
            href={googleCalendarUrl(event)}
            target="_blank"
            rel="noopener noreferrer"
            onClick={() => setOpen(false)}
          >
            {googleLabel}
          </a>
          <button
            type="button"
            style={menuItem}
            onClick={() => { downloadIcs(event, fileName); setOpen(false); }}
          >
            {appleLabel}
          </button>
        </div>
      )}
    </div>
  );
}
