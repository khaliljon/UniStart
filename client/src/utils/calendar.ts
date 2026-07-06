// Helpers to add an event to the user's calendar (works on desktop & mobile).
// Downloading a .ics file opens the native calendar app on both platforms.

export interface CalendarEvent {
  title: string;
  description?: string;
  location?: string;
  /** All-day event date in YYYY-MM-DD. */
  date: string;
}

function toIcsDate(date: string): string {
  // All-day event: YYYYMMDD
  return date.replace(/-/g, '');
}

function addOneDay(date: string): string {
  const d = new Date(date + 'T00:00:00Z');
  d.setUTCDate(d.getUTCDate() + 1);
  return d.toISOString().slice(0, 10).replace(/-/g, '');
}

function escapeIcs(text: string): string {
  return text.replace(/\\/g, '\\\\').replace(/;/g, '\\;').replace(/,/g, '\\,').replace(/\n/g, '\\n');
}

/** Builds an iCalendar (.ics) string for a single all-day event. */
export function buildIcs(event: CalendarEvent): string {
  const uid = `${toIcsDate(event.date)}-${Math.random().toString(36).slice(2)}@unistart.kz`;
  const stamp = new Date().toISOString().replace(/[-:]/g, '').slice(0, 15) + 'Z';
  return [
    'BEGIN:VCALENDAR',
    'VERSION:2.0',
    'PRODID:-//UniStart//CSCA//RU',
    'CALSCALE:GREGORIAN',
    'METHOD:PUBLISH',
    'BEGIN:VEVENT',
    `UID:${uid}`,
    `DTSTAMP:${stamp}`,
    `DTSTART;VALUE=DATE:${toIcsDate(event.date)}`,
    `DTEND;VALUE=DATE:${addOneDay(event.date)}`,
    `SUMMARY:${escapeIcs(event.title)}`,
    event.description ? `DESCRIPTION:${escapeIcs(event.description)}` : '',
    event.location ? `LOCATION:${escapeIcs(event.location)}` : '',
    'END:VEVENT',
    'END:VCALENDAR',
  ].filter(Boolean).join('\r\n');
}

/** Triggers a download of an .ics file that opens the device's calendar app. */
export function downloadIcs(event: CalendarEvent, fileName = 'csca-exam.ics'): void {
  const blob = new Blob([buildIcs(event)], { type: 'text/calendar;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

/** Google Calendar "add event" URL (opens in a new tab). */
export function googleCalendarUrl(event: CalendarEvent): string {
  const start = toIcsDate(event.date);
  const end = addOneDay(event.date);
  const params = new URLSearchParams({
    action: 'TEMPLATE',
    text: event.title,
    dates: `${start}/${end}`,
  });
  if (event.description) params.set('details', event.description);
  if (event.location) params.set('location', event.location);
  return `https://calendar.google.com/calendar/render?${params.toString()}`;
}
