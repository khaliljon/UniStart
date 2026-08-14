import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from '../hooks/useTranslation';
import { pickLocalized } from '../utils/localize';
import legalService, { type LegalDocument } from '../services/legalService';

interface Props {
  slug: string;
  fallbackTitle: string;
  footerLink?: { to: string; label: string };
}

function LegalDocumentView({ slug, fallbackTitle, footerLink }: Props) {
  const { t, locale } = useTranslation();
  const [doc, setDoc] = useState<LegalDocument | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    let active = true;
    setLoading(true);
    setError(false);
    legalService
      .getBySlug(slug)
      .then((d) => {
        if (active) setDoc(d);
      })
      .catch(() => {
        if (active) setError(true);
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, [slug]);

  return (
    <div
      style={{
        maxWidth: '800px',
        margin: '0 auto',
        padding: '2rem 1.5rem 4rem',
        color: 'var(--text-primary)',
      }}
    >
      <Link to="/landing" style={{ color: 'var(--primary-color)', textDecoration: 'none', fontSize: '0.9rem' }}>
        ← {t.legal.backToHome}
      </Link>

      <h1 style={{ margin: '1.5rem 0 0.5rem', fontSize: '2rem', fontWeight: 700 }}>
        {doc ? pickLocalized(doc.title, doc.titleKz, doc.titleEn, locale) : fallbackTitle}
      </h1>

      {doc && pickLocalized(doc.lastUpdatedLabel, doc.lastUpdatedLabelKz, doc.lastUpdatedLabelEn, locale) && (
        <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem', fontSize: '0.85rem' }}>
          {t.legal.lastUpdated}: {pickLocalized(doc.lastUpdatedLabel, doc.lastUpdatedLabelKz, doc.lastUpdatedLabelEn, locale)}
        </p>
      )}

      {loading && <p style={{ color: 'var(--text-secondary)' }}>{t.common.loading}</p>}

      {error && !loading && (
        <p style={{ color: 'var(--text-secondary)' }}>{t.common.error}</p>
      )}

      {doc && !loading && (
        <div
          style={{
            whiteSpace: 'pre-wrap',
            lineHeight: 1.7,
            fontSize: '0.95rem',
            color: 'var(--text-secondary)',
            wordBreak: 'break-word',
          }}
        >
          {pickLocalized(doc.content, doc.contentKz, doc.contentEn, locale)}
        </div>
      )}

      {footerLink && (
        <div style={{ marginTop: '3rem', textAlign: 'center' }}>
          <Link to={footerLink.to} style={{ color: 'var(--primary-color)', textDecoration: 'none' }}>
            {footerLink.label} →
          </Link>
        </div>
      )}
    </div>
  );
}

export default LegalDocumentView;
