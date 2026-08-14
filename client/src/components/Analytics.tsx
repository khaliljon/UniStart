import { useEffect, useRef } from 'react';
import { useLocation } from 'react-router-dom';

const YM_ID = 11074859;

declare global {
  interface Window {
    gtag?: (...args: unknown[]) => void;
    ym?: (...args: unknown[]) => void;
  }
}

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
