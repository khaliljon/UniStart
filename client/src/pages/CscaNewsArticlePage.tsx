import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell from '../components/csca/CscaPageShell';
import { newsService, type NewsItem } from '../services/newsService';
import { pickLocalized } from '../utils/localize';
import { fullDateLocalized } from '../utils/dates';

function CscaNewsArticlePage({ appTheme = false }: { appTheme?: boolean }) {
  const { slug = '' } = useParams();
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [item, setItem] = useState<NewsItem | null>(null);
  const [notFound, setNotFound] = useState(false);
  const basePath = appTheme ? '/news' : '/csca/news';

  useEffect(() => {
    let alive = true;
    setItem(null);
    setNotFound(false);
    newsService.getBySlug(slug)
      .then((n) => {
        if (!alive) return;
        setItem(n);
        // Retired slug (or numeric id) → land on the canonical address.
        if (n.slug && n.slug !== slug) navigate(`${basePath}/${n.slug}`, { replace: true });
      })
      .catch(() => { if (alive) setNotFound(true); });
    return () => { alive = false; };
  }, [slug, navigate]);

  const catLabel: Record<string, string> = {
    dates: s.newsCatDates,
    admission: s.newsCatAdmission,
    platform: s.newsCatPlatform,
    guide: s.newsCatGuide,
  };

  const title = item ? pickLocalized(item.title, item.titleKz, item.titleEn, locale) : '';
  const body = item ? pickLocalized(item.body, item.bodyKz, item.bodyEn, locale) : '';
  const summary = item ? pickLocalized(item.summary, item.summaryKz, item.summaryEn, locale) : '';

  useEffect(() => {
    if (title) document.title = `${title} — UniStart`;
  }, [title]);

  // Canonical + hreflang always point at the current slug, never a retired one.
  useEffect(() => {
    if (!item?.slug) return;
    const href = `${window.location.origin}/csca/news/${item.slug}`;
    const tags: HTMLLinkElement[] = [];

    const add = (rel: string, hreflang?: string) => {
      const el = document.createElement('link');
      el.rel = rel;
      el.href = href;
      if (hreflang) el.hreflang = hreflang;
      document.head.appendChild(el);
      tags.push(el);
    };

    add('canonical');
    ['ru', 'kk', 'en', 'x-default'].forEach((lang) => add('alternate', lang));

    const desc = document.createElement('meta');
    desc.name = 'description';
    desc.content = summary.slice(0, 300);
    document.head.appendChild(desc);

    return () => {
      tags.forEach((el) => el.remove());
      desc.remove();
    };
  }, [item?.slug, summary]);

  const content = (
    <article className={appTheme ? 'csca-article' : 'csca-wrap csca-section csca-article'}
             style={appTheme ? { maxWidth: 820, margin: '1.5rem auto', padding: '0 1rem' } : undefined}>
      <Link to={basePath} className={appTheme ? 'btn btn-outline' : 'csca-btn csca-btn-ghost csca-btn-sm'}
            style={{ marginBottom: '1.5rem', display: 'inline-flex' }}>
        ← {s.newsBack}
      </Link>

      {notFound ? (
        <p className="csca-lead" style={{ textAlign: 'center' }}>{s.newsEmpty}</p>
      ) : !item ? (
        <div className="loading"><div className="spinner" /></div>
      ) : (
        <>
          <div className="csca-news-meta" style={{ marginBottom: '0.75rem' }}>
            <span className={`csca-news-cat csca-news-cat-${item.category}`}>{catLabel[item.category] ?? ''}</span>
            <span style={{ color: 'var(--csca-ink-soft)' }}>
              {item.publishedAt ? fullDateLocalized(item.publishedAt, locale) : ''}
            </span>
          </div>

          <h1 className="csca-h2" style={{ fontSize: '2rem', marginBottom: '0.75rem' }}>{title}</h1>
          {summary && <p className="csca-lead" style={{ marginBottom: '1.5rem' }}>{summary}</p>}

          {item.imageUrl && (
            <img src={item.imageUrl} alt="" className="csca-article-cover" />
          )}

          <div className="csca-article-body">{body}</div>
        </>
      )}
    </article>
  );

  if (appTheme) return content;

  return (
    <CscaPageShell>
      {content}
    </CscaPageShell>
  );
}

export default CscaNewsArticlePage;
