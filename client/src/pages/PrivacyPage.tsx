import { Link } from 'react-router-dom';
import { useTranslation } from '../hooks/useTranslation';

function PrivacyPage() {
  const { t } = useTranslation();

  return (
    <div style={{
      maxWidth: '800px',
      margin: '0 auto',
      padding: '2rem 1.5rem 4rem',
      color: 'var(--text-primary)',
    }}>
      <Link to="/landing" style={{ color: 'var(--primary-color)', textDecoration: 'none', fontSize: '0.9rem' }}>
        ← {t.legal.backToHome}
      </Link>

      <h1 style={{ margin: '1.5rem 0 0.5rem', fontSize: '2rem', fontWeight: 700 }}>
        {t.legal.privacyTitle}
      </h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem', fontSize: '0.85rem' }}>
        {t.legal.lastUpdated}: {t.legal.privacyDate}
      </p>

      <Section title={t.legal.privacyIntroTitle}>
        <p>{t.legal.privacyIntroText}</p>
      </Section>

      <Section title={t.legal.privacyDataCollectedTitle}>
        <p>{t.legal.privacyDataCollectedText}</p>
        <ul>
          <li>{t.legal.privacyDataName}</li>
          <li>{t.legal.privacyDataEmail}</li>
          <li>{t.legal.privacyDataPassword}</li>
          <li>{t.legal.privacyDataUsage}</li>
          <li>{t.legal.privacyDataDevice}</li>
        </ul>
      </Section>

      <Section title={t.legal.privacyPurposeTitle}>
        <p>{t.legal.privacyPurposeText}</p>
        <ul>
          <li>{t.legal.privacyPurposeAuth}</li>
          <li>{t.legal.privacyPurposeAdaptive}</li>
          <li>{t.legal.privacyPurposeAnalytics}</li>
          <li>{t.legal.privacyPurposeSupport}</li>
          <li>{t.legal.privacyPurposeImprove}</li>
        </ul>
      </Section>

      <Section title={t.legal.privacyStorageTitle}>
        <p>{t.legal.privacyStorageText}</p>
      </Section>

      <Section title={t.legal.privacyCookiesTitle}>
        <p>{t.legal.privacyCookiesText}</p>
        <ul>
          <li>{t.legal.privacyCookieAuth}</li>
          <li>{t.legal.privacyCookiePrefs}</li>
          <li>{t.legal.privacyCookieAnalytics}</li>
        </ul>
      </Section>

      <Section title={t.legal.privacyThirdPartyTitle}>
        <p>{t.legal.privacyThirdPartyText}</p>
      </Section>

      <Section title={t.legal.privacyRightsTitle}>
        <p>{t.legal.privacyRightsText}</p>
        <ul>
          <li>{t.legal.privacyRightAccess}</li>
          <li>{t.legal.privacyRightCorrect}</li>
          <li>{t.legal.privacyRightDelete}</li>
          <li>{t.legal.privacyRightExport}</li>
          <li>{t.legal.privacyRightWithdraw}</li>
        </ul>
      </Section>

      <Section title={t.legal.privacyChildrenTitle}>
        <p>{t.legal.privacyChildrenText}</p>
      </Section>

      <Section title={t.legal.privacyChangesTitle}>
        <p>{t.legal.privacyChangesText}</p>
      </Section>

      <Section title={t.legal.privacyContactTitle}>
        <p>{t.legal.privacyContactText}</p>
        <p style={{ marginTop: '0.5rem' }}>
          Email: <a href="mailto:support@unistart.kz" style={{ color: 'var(--primary-color)' }}>support@unistart.kz</a>
        </p>
      </Section>

      <div style={{ marginTop: '3rem', textAlign: 'center' }}>
        <Link to="/terms" style={{ color: 'var(--primary-color)', textDecoration: 'none' }}>
          {t.legal.termsTitle} →
        </Link>
      </div>
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section style={{ marginBottom: '2rem' }}>
      <h2 style={{ fontSize: '1.25rem', fontWeight: 600, marginBottom: '0.75rem' }}>{title}</h2>
      <div style={{
        lineHeight: 1.7,
        fontSize: '0.95rem',
        color: 'var(--text-secondary)',
      }}>
        {children}
      </div>
      <style>{`
        section ul { padding-left: 1.5rem; margin-top: 0.5rem; }
        section li { margin-bottom: 0.3rem; }
      `}</style>
    </section>
  );
}

export default PrivacyPage;
