import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell, { CscaPageHero } from '../components/csca/CscaPageShell';
import { SOCIAL_LINKS } from '../socialLinks';

function ContactsPage() {
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  const contacts = [
    { label: 'Email', value: SOCIAL_LINKS.email, href: SOCIAL_LINKS.emailCompose, hanzi: '邮' },
    { label: s.socialSupportBot, value: '@unistart_support_bot', href: SOCIAL_LINKS.supportBot, hanzi: '助' },
    { label: s.socialTelegram, value: '@unistart_csca', href: SOCIAL_LINKS.telegramChannel, hanzi: '电' },
    { label: 'Instagram', value: '@unistartkz', href: SOCIAL_LINKS.instagram, hanzi: '图' },
    { label: 'TikTok', value: '@unistartkz', href: SOCIAL_LINKS.tiktok, hanzi: '视' },
  ];

  return (
    <CscaPageShell>
      <CscaPageHero eyebrow={s.navContacts} title={s.contactsTitle} lead={s.contactsLead} />

      <section className="csca-wrap csca-section" style={{ paddingTop: '1.5rem' }}>
        <div className="csca-grid csca-grid-5" style={{ marginBottom: '2rem' }}>
          {contacts.map((c) => (
            <a key={c.label} className="csca-card" href={c.href} target="_blank" rel="noopener noreferrer"
               style={{ textAlign: 'center', textDecoration: 'none', color: 'inherit' }}>
              <div className="csca-subject-hanzi" style={{ fontSize: '2rem' }}>{c.hanzi}</div>
              <div className="csca-feature-title">{c.label}</div>
              <div className="csca-feature-desc">{c.value}</div>
            </a>
          ))}
        </div>
      </section>
    </CscaPageShell>
  );
}

export default ContactsPage;
