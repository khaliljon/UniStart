import { useEffect, useRef } from 'react';
import { useLocation } from 'react-router-dom';

const YM_ID = 11074859;

declare global {
  interface Window {
    gtag?: (...args: unknown[]) => void;
    ym?: (...args: unknown[]) => void;
  }
}

/**
 * Sends SPA page views to Google Analytics 4 and Yandex.Metrika on every
 * client-side route change. The initial page view is already sent by the
 * inline snippets in index.html, so the first render is skipped here.
 * On non-production hosts the trackers are not loaded, so the calls no-op.
 */
export default function Analytics() {
  const location = useLocation();
  const first = useRef(true);

  useEffect(() => {
    if (first.current) {
      first.current = false;
      return;
    }
    const url = location.pathname + location.search;
    window.gtag?.('event', 'page_view', {
      page_path: url,
      page_location: window.location.href,
      page_title: document.title,
    });
    window.ym?.(YM_ID, 'hit', url);
  }, [location]);

  return null;
}
