import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import { purchaseService, type Purchase } from '../services/purchaseService';
import { mockCatalogService, type MockTemplate } from '../services/mockCatalogService';
import { cartService } from '../services/cartService';

function PurchasesPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [items, setItems] = useState<Purchase[] | null>(null);
  const [runs, setRuns] = useState<MockTemplate[]>([]);
  const [paid, setPaid] = useState(false);

  useEffect(() => {
    // Returning from a successful Polar payment — clear the cart, show a banner.
    if (new URLSearchParams(window.location.search).get('paid') === '1') {
      cartService.clear();
      setPaid(true);
    }
    purchaseService.list().then(setItems).catch(() => setItems([]));
    mockCatalogService.getCatalog()
      .then((c) => setRuns(c.templates.filter((t) => t.runsRemaining > 0)))
      .catch(() => {});
  }, []);

  const fmt = (iso: string) =>
    new Date(iso).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' });

  return (
    <div style={{ maxWidth: 720, margin: '1.5rem auto' }}>
      <h1 className="csca-h2" style={{ fontSize: '1.6rem', marginBottom: '1.25rem' }}>{s.purchasesTitle}</h1>

      {paid && (
        <div className="card" style={{ marginBottom: '1.25rem', border: '2px solid #10b981', background: 'rgba(16,185,129,0.08)' }}>
          <div style={{ fontWeight: 700, color: '#10b981' }}>✓ Оплата прошла</div>
          <div style={{ fontSize: '0.88rem', color: 'var(--text-secondary)' }}>
            Доступ начислен. Запуски пробников — во вкладке «Пробные экзамены», учебники — в «Материалах».
          </div>
        </div>
      )}

      {/* My runs */}
      {runs.length > 0 && (
        <div className="card" style={{ marginBottom: '1.25rem' }}>
          <div style={{ fontWeight: 700, marginBottom: '0.5rem' }}>Мои запуски</div>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem' }}>
            {runs.map((t) => (
              <span key={t.mockExamId} style={{ fontSize: '0.85rem', padding: '0.3rem 0.7rem', borderRadius: '999px', background: 'rgba(16,185,129,0.12)', color: '#10b981', fontWeight: 700 }}>
                {t.title}: {t.runsRemaining}
              </span>
            ))}
          </div>
          <button className="btn btn-primary" style={{ marginTop: '0.75rem', fontSize: '0.85rem' }} onClick={() => navigate('/learn?tab=mock')}>
            Перейти к решению
          </button>
        </div>
      )}

      {items === null ? (
        <div className="loading"><div className="spinner" /></div>
      ) : items.length === 0 ? (
        <div className="csca-card" style={{ textAlign: 'center' }}>
          <p className="csca-lead">{s.purchasesEmpty}</p>
          <button className="csca-btn csca-btn-primary" onClick={() => navigate('/')}>{s.purchasesBrowse}</button>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {items.map((p) => (
            <div key={p.id} className="csca-card" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '1rem' }}>
              <div>
                <div style={{ fontWeight: 700 }}>{p.title}</div>
                <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)' }}>{fmt(p.purchasedAt)}</div>
                {p.subjects && (
                  <div style={{ marginTop: '0.4rem', display: 'flex', flexWrap: 'wrap', gap: '0.35rem' }}>
                    {p.subjects.split(',').filter(Boolean).map((sub) => (
                      <span key={sub} style={{ fontSize: '0.75rem', padding: '0.15rem 0.5rem', borderRadius: '1rem', background: 'var(--bg-secondary, #f3f4f6)', color: 'var(--text-secondary)' }}>{sub}</span>
                    ))}
                  </div>
                )}
              </div>
              <div style={{ textAlign: 'right', whiteSpace: 'nowrap' }}>
                <div style={{ fontWeight: 800, color: 'var(--csca-red, #C8102E)' }}>
                  {p.amount.toLocaleString('ru-RU')} {p.currency}
                </div>
                <div style={{ fontSize: '0.75rem', color: '#10b981', fontWeight: 700 }}>{p.status}</div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default PurchasesPage;
