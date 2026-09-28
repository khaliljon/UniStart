import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaNewsSection from '../components/csca/CscaNewsSection';

/** News portal inside the authenticated app shell (same data as the landing). */
function AppNewsPage() {
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  return (
    <div style={{ maxWidth: 1000, margin: '1.5rem auto', padding: '0 1rem' }}>
      <h1 style={{ fontSize: '1.6rem', fontWeight: 700, marginBottom: '0.35rem' }}>{s.newsTitle}</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.25rem' }}>{s.newsLead}</p>

      <CscaNewsSection
        title=""
        readMore={s.newsReadMore}
        readLess={s.newsReadLess}
        emptyText={s.newsEmpty}
        limit={24}
        appTheme
        portal
        basePath="/news"
      />
    </div>
  );
}

export default AppNewsPage;
