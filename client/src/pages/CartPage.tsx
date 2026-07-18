import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import { cartService, type CartItem } from '../services/cartService';
import { type CheckoutLine } from '../services/mockCatalogService';
import { paymentsService } from '../services/paymentsService';

function CartPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [items, setItems] = useState<CartItem[]>([]);
  const [processing, setProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const refresh = () => setItems(cartService.list());
    refresh();
    window.addEventListener(cartService.eventName, refresh);
    return () => window.removeEventListener(cartService.eventName, refresh);
  }, []);

  const total = items.reduce((sum, i) => sum + i.amount, 0);
  const currency = items[0]?.currency ?? s.currency;

  const remove = (item: CartItem) => cartService.remove(item.itemType, item.itemCode);

  const checkout = async () => {
    if (items.length === 0) return;
    setProcessing(true);
    setError(null);
    try {
      // All paid items (mocks, packages, books) go through Polar. Server prices them.
      const lines: CheckoutLine[] = items.map((i) => {
        if (i.itemType === 'mock') return { kind: 'mock', mockExamId: Number(i.itemCode), runs: i.runs ?? 1 };
        if (i.itemType === 'package') return { kind: 'package', packageKey: i.itemCode, selectedMockIds: i.selectedMockIds ?? [] };
        return { kind: 'book', bookMaterialId: Number(i.itemCode) };
      });

      const { url } = await paymentsService.createCheckout(lines);
      window.location.href = url;
    } catch {
      setError(s.checkoutError);
    } finally {
      setProcessing(false);
    }
  };

  return (
    <div style={{ maxWidth: 720, margin: '1.5rem auto' }}>
      <h1 className="csca-h2" style={{ fontSize: '1.6rem', marginBottom: '1.25rem' }}>{s.cartTitle}</h1>

      {items.length === 0 ? (
        <div className="csca-card" style={{ textAlign: 'center' }}>
          <p className="csca-lead">{s.cartEmpty}</p>
          <button className="btn btn-primary" onClick={() => navigate('/')}>{s.cartBrowse}</button>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {items.map((item) => (
            <div key={`${item.itemType}:${item.itemCode}`} className="csca-card" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '1rem' }}>
              <div>
                <div style={{ fontWeight: 700 }}>{item.title}</div>
                {item.subjects && (
                  <div style={{ marginTop: '0.4rem', display: 'flex', flexWrap: 'wrap', gap: '0.35rem' }}>
                    {item.subjects.split(',').filter(Boolean).map((sub) => (
                      <span key={sub} style={{ fontSize: '0.75rem', padding: '0.15rem 0.5rem', borderRadius: '1rem', background: 'var(--bg-secondary, #f3f4f6)', color: 'var(--text-secondary)' }}>{sub}</span>
                    ))}
                  </div>
                )}
              </div>
              <div style={{ textAlign: 'right', whiteSpace: 'nowrap' }}>
                <div style={{ fontWeight: 800, color: 'var(--csca-red, #C8102E)' }}>
                  {item.amount.toLocaleString('ru-RU')} {item.currency}
                </div>
                <button
                  className="btn btn-outline"
                  style={{ marginTop: '0.4rem', fontSize: '0.78rem', padding: '0.2rem 0.6rem' }}
                  onClick={() => remove(item)}
                >
                  {s.cartRemove}
                </button>
              </div>
            </div>
          ))}

          <div className="csca-card" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '1rem' }}>
            <div style={{ fontWeight: 700, fontSize: '1.1rem' }}>{s.cartTotal}</div>
            <div style={{ fontWeight: 800, fontSize: '1.2rem', color: 'var(--csca-red, #C8102E)' }}>
              {total.toLocaleString('ru-RU')} {currency}
            </div>
          </div>

          {error && (
            <div style={{ color: 'var(--error-color, #ef4444)', fontSize: '0.9rem' }}>{error}</div>
          )}

          <button className="btn btn-primary" style={{ padding: '0.75rem' }} onClick={checkout} disabled={processing}>
            {processing ? '…' : s.cartCheckout}
          </button>
          <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', textAlign: 'center', margin: 0 }}>
            {s.checkoutStubNote}
          </p>
        </div>
      )}
    </div>
  );
}

export default CartPage;
