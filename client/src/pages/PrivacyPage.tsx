import { useTranslation } from '../hooks/useTranslation';
import LegalDocumentView from '../components/LegalDocumentView';

function PrivacyPage() {
  const { t } = useTranslation();
  return (
    <LegalDocumentView
      slug="privacy"
      fallbackTitle={t.legal.privacyTitle}
      footerLink={{ to: '/terms', label: t.legal.termsTitle }}
    />
  );
}

export default PrivacyPage;
