import { useEffect, useRef } from 'react';
import { useLocation } from 'react-router-dom';
import { trackPageView, sanitizePathSearch, setUserProperties } from '../utils/analytics';
import { captureFirstTouch, getFirstTouch } from '../utils/attribution';

export default function Analytics() {
  const location = useLocation();
  const isFirst = useRef(true);

  // First-touch attribution + GA4 user properties (once per app load).
  useEffect(() => {
    captureFirstTouch();
    const ft = getFirstTouch();
    if (ft) {
      const props: Record<string, string> = {};
      if (ft.source) props.first_source = ft.source;
      if (ft.medium) props.first_medium = ft.medium;
      if (ft.campaign) props.first_campaign = ft.campaign;
      if (Object.keys(props).length > 0) setUserProperties(props);
    }
  }, []);

  // Exactly one page_view per real navigation (initial + SPA), sanitized URL.
  useEffect(() => {
    const url = sanitizePathSearch(location.pathname, location.search);
    trackPageView(url, isFirst.current);
    isFirst.current = false;
  }, [location]);

  return null;
}
