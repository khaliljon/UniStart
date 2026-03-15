import { Link } from 'react-router-dom';
import { useTranslation } from '../hooks/useTranslation';

function TermsPage() {
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
        {t.legal.termsTitle}
      </h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem', fontSize: '0.85rem' }}>
        {t.legal.lastUpdated}: {t.legal.termsDate}
      </p>

      <Section title={t.legal.termsIntroTitle}>
        <p>{t.legal.termsIntroText}</p>
      </Section>

      <Section title={t.legal.termsDefinitionsTitle}>
        <ul>
          <li>{t.legal.termsDefPlatform}</li>
          <li>{t.legal.termsDefUser}</li>
          <li>{t.legal.termsDefContent}</li>
          <li>{t.legal.termsDefSubscription}</li>
        </ul>
      </Section>

      <Section title={t.legal.termsRegistrationTitle}>
        <p>{t.legal.termsRegistrationText}</p>
        <ul>
          <li>{t.legal.termsRegAge}</li>
          <li>{t.legal.termsRegAccuracy}</li>
          <li>{t.legal.termsRegSecurity}</li>
          <li>{t.legal.termsRegOneAccount}</li>
        </ul>
      </Section>

      <Section title={t.legal.termsSubscriptionTitle}>
        <p>{t.legal.termsSubscriptionText}</p>
        <ul>
          <li>{t.legal.termsSubFree}</li>
          <li>{t.legal.termsSubTrial}</li>
          <li>{t.legal.termsSubPro}</li>
        </ul>
      </Section>

      <Section title={t.legal.termsPaymentTitle}>
        <p>{t.legal.termsPaymentText}</p>
        <ul>
          <li>{t.legal.termsPayCurrency}</li>
          <li>{t.legal.termsPayRefund}</li>
          <li>{t.legal.termsPayAuto}</li>
        </ul>
      </Section>

      <Section title={t.legal.termsUsageTitle}>
        <p>{t.legal.termsUsageText}</p>
        <ul>
          <li>{t.legal.termsUseNoShare}</li>
          <li>{t.legal.termsUseNoHack}</li>
          <li>{t.legal.termsUseNoAbuse}</li>
          <li>{t.legal.termsUseNoScrape}</li>
        </ul>
      </Section>

      <Section title={t.legal.termsIPTitle}>
        <p>{t.legal.termsIPText}</p>
      </Section>

      <Section title={t.legal.termsLiabilityTitle}>
        <p>{t.legal.termsLiabilityText}</p>
      </Section>

      <Section title={t.legal.termsTerminationTitle}>
        <p>{t.legal.termsTerminationText}</p>
      </Section>

      <Section title={t.legal.termsDisputeTitle}>
        <p>{t.legal.termsDisputeText}</p>
      </Section>

      <Section title={t.legal.termsChangesTitle}>
        <p>{t.legal.termsChangesText}</p>
      </Section>

      <Section title={t.legal.termsContactTitle}>
        <p>{t.legal.termsContactText}</p>
        <p style={{ marginTop: '0.5rem' }}>
          Email: <a href="mailto:support@unistart.kz" style={{ color: 'var(--primary-color)' }}>support@unistart.kz</a>
        </p>
      </Section>

      <div style={{ marginTop: '3rem', textAlign: 'center' }}>
        <Link to="/privacy" style={{ color: 'var(--primary-color)', textDecoration: 'none' }}>
          ← {t.legal.privacyTitle}
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

export default TermsPage;
