import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { newsService, type NewsItem, type NewsCategory } from '../../services/newsService';
import { useTranslation } from '../../hooks/useTranslation';
import { cscaStrings } from '../../i18n/csca';
import { pickLocalized } from '../../utils/localize';
import { fullDateLocalized } from '../../utils/dates';

interface Props {
  title: string;
  lead?: string;
  readMore: string;
  readLess?: string;
  emptyText: string;
  limit?: number;
  section?: boolean;
  appTheme?: boolean;
  id?: string;
  /** Full portal chrome: lead story, category chips and search. */
  portal?: boolean;
}

const CATEGORY_ORDER: NewsCategory[] = ['dates', 'admission', 'guide', 'platform'];

/** Rough reading time from the body length. */
const readMinutes = (text: string) => Math.max(1, Math.round(text.trim().split(/\s+/).length / 180));

export default function CscaNewsSection({ title, lead, readMore, emptyText, limit = 6, section = false, appTheme = false, id, portal = false }: Props) {
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [items, setItems] = useState<NewsItem[] | null>(null);
  const [cat, setCat] = useState<NewsCategory | 'all'>('all');
  const [q, setQ] = useState('');

  useEffect(() => {
    let alive = true;
    newsService.listPublished(limit)
      .then((data) => { if (alive) setItems(data); })
      .catch(() => { if (alive) setItems([]); });
    return () => { alive = false; };
  }, [limit]);

  const catLabel: Record<NewsCategory, string> = {
    dates: s.newsCatDates,
    admission: s.newsCatAdmission,
    platform: s.newsCatPlatform,
    guide: s.newsCatGuide,
  };

  const localized = useMemo(() => (items ?? []).map((n) => ({
    raw: n,
    title: pickLocalized(n.title, n.titleKz, n.titleEn, locale),
    summary: pickLocalized(n.summary, n.summaryKz, n.summaryEn, locale),
    body: pickLocalized(n.body, n.bodyKz, n.bodyEn, locale),
  })), [items, locale]);

  const filtered = useMemo(() => {
    const needle = q.trim().toLowerCase();
    return localized.filter((n) => {
      if (cat !== 'all' && n.raw.category !== cat) return false;
      if (!needle) return true;
      return n.title.toLowerCase().includes(needle) || n.summary.toLowerCase().includes(needle);
    });
  }, [localized, cat, q]);

  const featured = portal && cat === 'all' && !q.trim()
    ? filtered.find((n) => n.raw.isFeatured) ?? null
    : null;
  const rest = featured ? filtered.filter((n) => n.raw.id !== featured.raw.id) : filtered;

  const cardClass = appTheme ? 'card' : 'csca-card';
  const subColor = appTheme ? 'var(--text-secondary)' : 'var(--csca-ink-soft, var(--text-secondary))';
  const href = (n: NewsItem) => `/csca/news/${n.slug ?? n.id}`;

  const card = (n: (typeof filtered)[number], big = false) => (
    <article className={`${cardClass} csca-news-card${big ? ' csca-news-card-lead' : ''}`} key={n.raw.id}>
      {n.raw.imageUrl && (
        <Link to={href(n.raw)} className="csca-news-media">
          <img src={n.raw.imageUrl} alt="" loading="lazy" />
        </Link>
      )}
      <div className="csca-news-body">
        <div className="csca-news-meta">
          <span className={`csca-news-cat csca-news-cat-${n.raw.category}`}>{catLabel[n.raw.category] ?? ''}</span>
          <span style={{ color: subColor }}>{n.raw.publishedAt ? fullDateLocalized(n.raw.publishedAt, locale) : ''}</span>
          {n.body && <span style={{ color: subColor }}>· {readMinutes(n.body)} {s.newsMinRead}</span>}
        </div>
        <Link to={href(n.raw)} className="csca-news-title">{n.title}</Link>
        <p className="csca-news-sum" style={{ color: subColor }}>{n.summary}</p>
        <Link to={href(n.raw)} className={appTheme ? 'btn btn-outline' : 'csca-btn csca-btn-ghost csca-btn-sm'} style={{ alignSelf: 'flex-start' }}>
          {readMore}
        </Link>
      </div>
    </article>
  );

  const body = (
    <>
      {portal && (
        <div className="csca-news-toolbar">
          <div className="csca-news-chips">
            <button type="button" className={`csca-news-chip${cat === 'all' ? ' is-active' : ''}`} onClick={() => setCat('all')}>
              {s.newsAll}
            </button>
            {CATEGORY_ORDER.map((c) => (
              <button type="button" key={c} className={`csca-news-chip${cat === c ? ' is-active' : ''}`} onClick={() => setCat(c)}>
                {catLabel[c]}
              </button>
            ))}
          </div>
          <input className="csca-news-search" type="search" placeholder={s.newsSearch}
                 value={q} onChange={(e) => setQ(e.target.value)} />
        </div>
      )}

      {items && filtered.length === 0 ? (
        <p className={appTheme ? undefined : 'csca-lead'} style={{ textAlign: 'center', color: appTheme ? 'var(--text-secondary)' : undefined }}>
          {q.trim() || cat !== 'all' ? s.newsNothingFound : emptyText}
        </p>
      ) : (
        <>
          {featured && <div className="csca-news-lead-wrap">{card(featured, true)}</div>}
          <div className="csca-news-grid">{rest.map((n) => card(n))}</div>
        </>
      )}
    </>
  );

  if (!section) {
    return (
      <div id={id}>
        {title && <h2 className={appTheme ? undefined : 'csca-h2'} style={{ marginBottom: lead ? '0.4rem' : '1.2rem', ...(appTheme ? { color: 'var(--text-primary)', fontSize: '1.3rem' } : {}) }}>{title}</h2>}
        {lead && <p className={appTheme ? undefined : 'csca-lead'} style={{ marginBottom: '1.4rem', ...(appTheme ? { color: 'var(--text-secondary)' } : {}) }}>{lead}</p>}
        {body}
      </div>
    );
  }

  return (
    <section id={id} className="csca-section">
      <div className="csca-wrap">
        <div className="csca-section-head">
          <h2 className="csca-h2">{title}</h2>
          {lead && <p className="csca-lead">{lead}</p>}
        </div>
        {body}
      </div>
    </section>
  );
}
