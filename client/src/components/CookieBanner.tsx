import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from '../hooks/useTranslation';

const COOKIE_CONSENT_KEY = 'unistart_cookie_consent';

function CookieBanner() {
  const { t } = useTranslation();
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    const consent = localStorage.getItem(COOKIE_CONSENT_KEY);
    if (!consent) {
      setVisible(true);
    }
  }, []);

  const handleAccept = () => {
    localStorage.setItem(COOKIE_CONSENT_KEY, 'accepted');
    setVisible(false);
  };

  const handleDecline = () => {
    localStorage.setItem(COOKIE_CONSENT_KEY, 'declined');
    setVisible(false);
  };

  if (!visible) return null;

  return (
    <div style={{
      position: 'fixed',
      bottom: 0,
      left: 0,
      right: 0,
      zIndex: 9999,
      background: 'var(--card-bg, #1a1a2e)',
      borderTop: '1px solid var(--border-color, #374151)',
      padding: '1rem 1.5rem',
      boxShadow: '0 -4px 20px rgba(0, 0, 0, 0.15)',
    }}>
      <div style={{
        maxWidth: '1000px',
        margin: '0 auto',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: '1rem',
        flexWrap: 'wrap',
      }}>
        <p style={{
          margin: 0,
          fontSize: '0.85rem',
          color: 'var(--text-secondary)',
          lineHeight: 1.5,
          flex: 1,
          minWidth: '250px',
        }}>
          {t.legal.cookieText}{' '}
          <Link to="/privacy" style={{ color: 'var(--primary-color)', textDecoration: 'underline' }}>
            {t.legal.privacyTitle}
          </Link>
        </p>
        <div style={{ display: 'flex', gap: '0.5rem', flexShrink: 0 }}>
          <button
            onClick={handleDecline}
            style={{
              padding: '0.5rem 1rem',
              borderRadius: '6px',
              border: '1px solid var(--border-color, #374151)',
              background: 'transparent',
              color: 'var(--text-secondary)',
              cursor: 'pointer',
              fontSize: '0.85rem',
            }}
          >
            {t.legal.cookieDecline}
          </button>
          <button
            onClick={handleAccept}
            style={{
              padding: '0.5rem 1rem',
              borderRadius: '6px',
              border: 'none',
              background: 'var(--primary-color)',
              color: '#fff',
              cursor: 'pointer',
              fontSize: '0.85rem',
              fontWeight: 600,
            }}
          >
            {t.legal.cookieAccept}
          </button>
        </div>
      </div>
    </div>
  );
}

export default CookieBanner;
