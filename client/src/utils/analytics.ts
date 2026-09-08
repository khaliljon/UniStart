// Centralized, best-effort analytics. Never throws, never blocks the app.
// GA4 (gtag) + Yandex.Metrika. Active on production hosts only; on other hosts
// events are no-ops (optionally console-logged when analytics_debug is set).

declare global {
  interface Window {
    gtag?: (...args: unknown[]) => void;
    ym?: (...args: unknown[]) => void;
    dataLayer?: unknown[];
  }
}

export interface AnalyticsItem {
  item_id: string;
  item_name: string;
  item_category?: string;
  item_variant?: string;
  price?: number;
  quantity?: number;
}

export interface AnalyticsEventMap {
  sign_up: { method: 'email' | 'google' };
  login: { method: 'email' | 'google' };
  select_subject: { subject: string };
  mock_start: { subject: string; mock_id: number; access_type?: 'free' | 'purchased' | 'full_access' };
  mock_complete: { subject: string; mock_id: number; score: number; duration_seconds: number; access_type?: 'free' | 'purchased' | 'full_access' };
  view_results: { subject: string; mock_id: number; score: number };
  view_explanation: { subject: string; topic: string; question_id: number };
  begin_checkout: { currency: string; value: number; items: AnalyticsItem[] };
  purchase: { transaction_id: string; currency: string; value: number; items: AnalyticsItem[] };
}

const YM_ID = 11074859;
const PROD_HOSTS = ['unistart.kz', 'www.unistart.kz'];

// Allowlist: only these non-PII marketing params are ever forwarded to analytics.
// Everything else (ids, flags, tokens, unknown params) is dropped, so no unknown
// or sensitive value can leak into page_view. UTM is kept for GA4 attribution.
const ALLOWED_QUERY_KEYS = new Set([
  'utm_source', 'utm_medium', 'utm_campaign', 'utm_content', 'utm_term', 'utm_id',
  'gclid', 'gbraid', 'wbraid', 'yclid', 'fbclid', 'msclkid', 'ttclid',
]);

// Events mirrored to Yandex.Metrika as goals (minimal, business-critical set).
const YM_GOALS: Partial<Record<keyof AnalyticsEventMap, string>> = {
  sign_up: 'sign_up',
  mock_complete: 'mock_complete',
  begin_checkout: 'begin_checkout',
  purchase: 'purchase',
};

function isProdHost(): boolean {
  return typeof window !== 'undefined' && PROD_HOSTS.includes(window.location.hostname);
}

function isDebug(): boolean {
  try {
    return typeof window !== 'undefined'
      && !isProdHost()
      && window.localStorage.getItem('analytics_debug') === '1';
  } catch {
    return false;
  }
}

// Real events are only emitted on production hosts (keeps dev/staging clean).
function isEnabled(): boolean {
  return isProdHost();
}

export function sanitizePathSearch(pathname: string, search: string): string {
  try {
    const src = new URLSearchParams(search || '');
    const out = new URLSearchParams();
    for (const [key, value] of src.entries()) {
      if (ALLOWED_QUERY_KEYS.has(key.toLowerCase())) out.append(key, value);
    }
    const qs = out.toString();
    return pathname + (qs ? `?${qs}` : '');
  } catch {
    return pathname;
  }
}

export function trackEvent<K extends keyof AnalyticsEventMap>(name: K, params: AnalyticsEventMap[K]): void {
  try {
    if (isDebug()) console.debug('[analytics] event', name, params);
    if (!isEnabled()) return;
    window.gtag?.('event', name, params as Record<string, unknown>);
    const goal = YM_GOALS[name];
    if (goal) window.ym?.(YM_ID, 'reachGoal', goal);
  } catch {
    /* analytics is best-effort — never break the app */
  }
}

export function trackPageView(sanitizedUrl: string, isFirst: boolean): void {
  try {
    if (isDebug()) console.debug('[analytics] page_view', sanitizedUrl, { isFirst });
    if (!isEnabled()) return;
    window.gtag?.('event', 'page_view', {
      page_path: sanitizedUrl,
      page_location: window.location.origin + sanitizedUrl,
      page_title: document.title,
    });
    // Yandex.Metrika already sends the initial hit on init(); only send SPA hits.
    if (!isFirst) window.ym?.(YM_ID, 'hit', window.location.origin + sanitizedUrl);
  } catch {
    /* best-effort */
  }
}

export function setUserProperties(props: Record<string, string | number | boolean | undefined>): void {
  try {
    if (!isEnabled()) return;
    window.gtag?.('set', 'user_properties', props);
  } catch {
    /* best-effort */
  }
}
