import { useEffect, useState } from 'react';
import { newsService, type NewsItem } from '../../services/newsService';

interface Props {
  title: string;
  lead?: string;
  readMore: string;
  emptyText: string;
  limit?: number;
  /** Wrap in the marketing section chrome (used on the landing). */
  section?: boolean;
  id?: string;
}

/** Renders published CSCA news. Used on the landing and the student dashboard. */
export default function CscaNewsSection({ title, lead, readMore, emptyText, limit = 6, section = false, id }: Props) {
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

  const grid = (
    <div className="csca-grid csca-grid-3">
      {(items ?? []).map((n) => {
        const expanded = openId === n.id;
        return (
          <article className="csca-card" key={n.id} style={{ display: 'flex', flexDirection: 'column' }}>
            {n.imageUrl && (
              <img src={n.imageUrl} alt="" style={{ width: '100%', height: 160, objectFit: 'cover', borderRadius: '0.75rem', marginBottom: '0.9rem' }} />
            )}
            <div style={{ fontSize: '0.78rem', color: 'var(--csca-ink-soft, var(--text-secondary))', marginBottom: '0.35rem' }}>{fmtDate(n.publishedAt)}</div>
            <div className="csca-feature-title" style={{ fontSize: '1.1rem' }}>{n.title}</div>
            <div className="csca-feature-desc" style={{ marginTop: '0.35rem', whiteSpace: 'pre-wrap' }}>
              {expanded ? n.body : n.summary}
            </div>
            {n.body && n.body !== n.summary && (
              <button
                type="button"
                className="csca-btn csca-btn-ghost csca-btn-sm"
                style={{ marginTop: '0.9rem', alignSelf: 'flex-start' }}
                onClick={() => setOpenId(expanded ? null : n.id)}
              >
                {readMore}
              </button>
            )}
          </article>
        );
      })}
    </div>
  );

  const body = items && items.length === 0
    ? <p className="csca-lead" style={{ textAlign: 'center' }}>{emptyText}</p>
    : grid;

  if (!section) {
    return (
      <div id={id}>
        <h2 className="csca-h2" style={{ marginBottom: lead ? '0.4rem' : '1.2rem' }}>{title}</h2>
        {lead && <p className="csca-lead" style={{ marginBottom: '1.4rem' }}>{lead}</p>}
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
