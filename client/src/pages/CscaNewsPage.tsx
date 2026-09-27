import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell, { CscaPageHero } from '../components/csca/CscaPageShell';
import CscaNewsSection from '../components/csca/CscaNewsSection';

function CscaNewsPage() {
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  return (
    <CscaPageShell>
      <CscaPageHero eyebrow={s.navNews} title={s.newsTitle} lead={s.newsLead} />

      <section className="csca-wrap csca-section" style={{ paddingTop: '1rem' }}>
        <CscaNewsSection
          title=""
          readMore={s.newsReadMore}
          readLess={s.newsReadLess}
          emptyText={s.newsEmpty}
          limit={24}
          portal
        />
      </section>
    </CscaPageShell>
  );
}

export default CscaNewsPage;
