import { useEffect, useState } from 'react';
import { newsService, type NewsItem } from '../../services/newsService';

interface Props {
  title: string;
  lead?: string;
  readMore: string;
  readLess?: string;
  emptyText: string;
  limit?: number;
  /** Wrap in the marketing section chrome (used on the landing). */
  section?: boolean;
  /** Use theme-aware colors (for the in-app dashboard, which supports dark mode). */
  appTheme?: boolean;
  id?: string;
}

/** Renders published CSCA news. Used on the landing and the student dashboard. */
export default function CscaNewsSection({ title, lead, readMore, readLess, emptyText, limit = 6, section = false, appTheme = false, id }: Props) {
  const [items, setItems] = useState<NewsItem[] | null>(null);
  const [openId, setOpenId] = useState<number | null>(null);

  useEffect(() => {
    let alive = true;
    newsService.listPublished(limit)
      .then((data) => { if (alive) setItems(data); })
      .catch(() => { if (alive) setItems([]); });
    return () => { alive = false; };
  }, [limit]);

  const fmtDate = (iso: string | null) =>
    iso ? new Date(iso).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' }) : '';

  const cardClass = appTheme ? 'card' : 'csca-card';
  const subColor = appTheme ? 'var(--text-secondary)' : 'var(--csca-ink-soft, var(--text-secondary))';

  const grid = (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
      {(items ?? []).map((n) => {
        const expanded = openId === n.id;
        return (
          <article className={cardClass} key={n.id}>
            {n.imageUrl && (
              <img src={n.imageUrl} alt="" style={{ width: '100%', maxHeight: 320, objectFit: 'contain', background: 'transparent', borderRadius: '0.75rem', marginBottom: '1rem', display: 'block' }} />
            )}
            <div style={{ fontSize: '0.78rem', color: subColor, marginBottom: '0.35rem' }}>{fmtDate(n.publishedAt)}</div>
            <div className={appTheme ? undefined : 'csca-feature-title'} style={appTheme ? { fontSize: '1.15rem', fontWeight: 700, color: 'var(--text-primary)' } : { fontSize: '1.15rem' }}>{n.title}</div>
            <div className={appTheme ? undefined : 'csca-feature-desc'} style={{ marginTop: '0.45rem', whiteSpace: 'pre-wrap', color: appTheme ? 'var(--text-secondary)' : undefined }}>
              {expanded ? n.body : n.summary}
            </div>
            {n.body && n.body !== n.summary && (
              <button
                type="button"
                className={appTheme ? 'btn btn-outline' : 'csca-btn csca-btn-ghost csca-btn-sm'}
                style={{ marginTop: '0.75rem' }}
                onClick={() => setOpenId(expanded ? null : n.id)}
              >
                {expanded ? (readLess ?? readMore) : readMore}
              </button>
            )}
          </article>
        );
      })}
    </div>
  );

  const body = items && items.length === 0
    ? <p className={appTheme ? undefined : 'csca-lead'} style={{ textAlign: 'center', color: appTheme ? 'var(--text-secondary)' : undefined }}>{emptyText}</p>
    : grid;

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
