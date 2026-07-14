import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import { purchaseService, type Purchase } from '../services/purchaseService';
import { cartService } from '../services/cartService';

function PurchasesPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [items, setItems] = useState<Purchase[] | null>(null);

  useEffect(() => {
    // Returning from a successful Polar payment — clear the cart.
    if (new URLSearchParams(window.location.search).get('paid') === '1') {
      cartService.clear();
    }
    purchaseService.list().then(setItems).catch(() => setItems([]));
  }, []);

  const fmt = (iso: string) =>
    new Date(iso).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' });

  return (
    <div style={{ maxWidth: 720, margin: '1.5rem auto' }}>
      <h1 className="csca-h2" style={{ fontSize: '1.6rem', marginBottom: '1.25rem' }}>{s.purchasesTitle}</h1>

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
