import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import CscaPageShell from '../components/csca/CscaPageShell';
import { newsService, type NewsItem } from '../services/newsService';
import { pickLocalized } from '../utils/localize';
import { fullDateLocalized } from '../utils/dates';

function CscaNewsArticlePage() {
  const { slug = '' } = useParams();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [item, setItem] = useState<NewsItem | null>(null);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    let alive = true;
    setItem(null);
    setNotFound(false);
    newsService.getBySlug(slug)
      .then((n) => { if (alive) setItem(n); })
      .catch(() => { if (alive) setNotFound(true); });
    return () => { alive = false; };
  }, [slug]);

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

  return (
    <CscaPageShell>
      <article className="csca-wrap csca-section csca-article">
        <Link to="/csca/news" className="csca-btn csca-btn-ghost csca-btn-sm" style={{ marginBottom: '1.5rem', display: 'inline-flex' }}>
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
    </CscaPageShell>
  );
}

export default CscaNewsArticlePage;
