// First-touch attribution stored client-side (localStorage). Never overwritten
// once set. Contains only marketing signals — no PII, tokens or secrets.

export interface FirstTouch {
  source: string;
  medium: string;
  campaign: string;
  content: string;
  landing_path: string;
  referrer: string;
  timestamp: string;
}

const KEY = 'us_first_touch';

function sanitizeReferrer(ref: string): string {
  try {
    if (!ref) return '';
    const u = new URL(ref);
    // Host + path only — drop any query/hash that could carry sensitive data.
    return u.hostname + (u.pathname === '/' ? '' : u.pathname);
  } catch {
    return '';
  }
}

function inferSource(referrerHost: string): string {
  const r = referrerHost.toLowerCase();
  if (!r) return 'direct';
  if (r.includes('t.me') || r.includes('telegram')) return 'telegram';
  if (r.includes('instagram')) return 'instagram';
  if (r.includes('tiktok')) return 'tiktok';
  if (r.includes('threads')) return 'threads';
  if (r.includes('google')) return 'google';
  if (r.includes('yandex')) return 'yandex';
  if (r.includes('bing')) return 'bing';
  return 'referral';
}

// Capture first-touch on the very first visit that carries a real source signal
// (UTM or an external referrer). Subsequent visits never overwrite it.
export function captureFirstTouch(): void {
  try {
    if (localStorage.getItem(KEY)) return;

    const p = new URLSearchParams(window.location.search);
    const source = (p.get('utm_source') || '').trim().toLowerCase();
    const medium = (p.get('utm_medium') || '').trim().toLowerCase();
    const campaign = (p.get('utm_campaign') || '').trim().toLowerCase();
    const content = (p.get('utm_content') || '').trim().toLowerCase();

    const referrer = sanitizeReferrer(document.referrer);
    const externalRef = !!referrer && !referrer.startsWith(window.location.hostname);
    const hasUtm = !!source;

    if (!hasUtm && !externalRef) return;

    const ft: FirstTouch = {
      source: source || inferSource(referrer),
      medium: medium || (hasUtm ? '' : 'referral'),
      campaign,
      content,
      landing_path: window.location.pathname,
      referrer,
      timestamp: new Date().toISOString(),
    };
    localStorage.setItem(KEY, JSON.stringify(ft));
  } catch {
    /* attribution is best-effort */
  }
}

export function getFirstTouch(): FirstTouch | null {
  try {
    const raw = localStorage.getItem(KEY);
    return raw ? (JSON.parse(raw) as FirstTouch) : null;
  } catch {
    return null;
  }
}
