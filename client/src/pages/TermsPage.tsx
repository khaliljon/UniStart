import { useTranslation } from '../hooks/useTranslation';
import LegalDocumentView from '../components/LegalDocumentView';

function TermsPage() {
  const { t } = useTranslation();
  return (
    <LegalDocumentView
      slug="terms"
      fallbackTitle={t.legal.termsTitle}
      footerLink={{ to: '/privacy', label: t.legal.privacyTitle }}
    />
  );
}

export default TermsPage;
