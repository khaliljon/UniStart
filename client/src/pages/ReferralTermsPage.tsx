import { useTranslation } from '../hooks/useTranslation';
import LegalDocumentView from '../components/LegalDocumentView';

function ReferralTermsPage() {
  const { t } = useTranslation();
  return (
    <LegalDocumentView
      slug="referral"
      fallbackTitle={t.legal.referralTermsTitle}
      footerLink={{ to: '/terms', label: t.legal.termsTitle }}
    />
  );
}

export default ReferralTermsPage;
