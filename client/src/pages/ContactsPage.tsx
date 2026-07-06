import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell, { CscaPageHero } from '../components/csca/CscaPageShell';
import ContactForm from '../components/ContactForm';

function ContactsPage() {
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  const contacts = [
    { label: 'Email', value: 'unistart.kz@gmail.com', href: 'mailto:unistart.kz@gmail.com', hanzi: '邮' },
    { label: 'Telegram', value: '@unistart', href: 'https://t.me/unistart', hanzi: '电' },
    { label: 'Instagram', value: '@unistart', href: 'https://instagram.com/unistart', hanzi: '图' },
  ];

  return (
    <CscaPageShell>
      <CscaPageHero eyebrow={s.navContacts} title={s.contactsTitle} lead={s.contactsLead} />

      <section className="csca-wrap csca-section" style={{ paddingTop: '1.5rem' }}>
        <div className="csca-grid csca-grid-3" style={{ marginBottom: '2rem' }}>
          {contacts.map((c) => (
            <a key={c.label} className="csca-card" href={c.href} target="_blank" rel="noopener noreferrer"
               style={{ textAlign: 'center', textDecoration: 'none', color: 'inherit' }}>
              <div className="csca-subject-hanzi" style={{ fontSize: '2rem' }}>{c.hanzi}</div>
              <div className="csca-feature-title">{c.label}</div>
              <div className="csca-feature-desc">{c.value}</div>
            </a>
          ))}
        </div>

        <div className="csca-card" style={{ maxWidth: 640, margin: '0 auto' }}>
          <div className="csca-feature-title" style={{ fontSize: '1.2rem', marginBottom: '1rem', textAlign: 'center' }}>{s.contactsWriteUs}</div>
          <ContactForm />
        </div>
      </section>
    </CscaPageShell>
  );
}

export default ContactsPage;
