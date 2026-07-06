import { useMemo, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import { purchaseService } from '../services/purchaseService';
import { useToast } from '../components/Toast';

interface CheckoutState {
  itemType: string;
  itemCode: string;
  title: string;
  subjects?: string;
  amount: number;
  currency?: string;
}

function readState(location: ReturnType<typeof useLocation>): CheckoutState | null {
  const fromRouter = location.state as CheckoutState | null;
  if (fromRouter && fromRouter.itemCode) {
    sessionStorage.setItem('checkout', JSON.stringify(fromRouter));
    return fromRouter;
  }
  const saved = sessionStorage.getItem('checkout');
  return saved ? (JSON.parse(saved) as CheckoutState) : null;
}

function CheckoutPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const { showToast } = useToast();
  const order = useMemo(() => readState(location), [location]);
  const [loading, setLoading] = useState(false);

  if (!order) {
    return (
      <div className="csca-card" style={{ maxWidth: 560, margin: '2rem auto', textAlign: 'center' }}>
        <h2 className="csca-h2" style={{ fontSize: '1.4rem' }}>{s.checkoutTitle}</h2>
        <p className="csca-lead">{s.checkoutEmpty}</p>
        <button className="csca-btn csca-btn-primary" onClick={() => navigate('/')}>{s.checkoutBackHome}</button>
      </div>
    );
  }

  const subjects = order.subjects ? order.subjects.split(',').filter(Boolean) : [];

  const confirm = async () => {
    setLoading(true);
    try {
      await purchaseService.checkout({
        itemType: order.itemType,
        itemCode: order.itemCode,
        title: order.title,
        subjects: order.subjects || null,
        amount: order.amount,
        currency: order.currency || 'KZT',
      });
      sessionStorage.removeItem('checkout');
      showToast(s.checkoutSuccess, 'success');
      navigate('/purchases');
    } catch {
      showToast(s.checkoutError, 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: 620, margin: '1.5rem auto' }}>
      <h1 className="csca-h2" style={{ fontSize: '1.6rem', marginBottom: '1.25rem' }}>{s.checkoutTitle}</h1>

      <div className="csca-card" style={{ marginBottom: '1.25rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', gap: '1rem' }}>
          <div style={{ fontWeight: 700, fontSize: '1.15rem' }}>{order.title}</div>
          <div style={{ fontWeight: 800, fontSize: '1.35rem', color: 'var(--csca-red, #C8102E)', whiteSpace: 'nowrap' }}>
            {order.amount.toLocaleString('ru-RU')} {order.currency || s.currency}
          </div>
        </div>
        {subjects.length > 0 && (
          <div style={{ marginTop: '0.9rem', display: 'flex', flexWrap: 'wrap', gap: '0.4rem' }}>
            {subjects.map((sub) => (
              <span key={sub} style={{
                fontSize: '0.8rem', padding: '0.2rem 0.6rem', borderRadius: '1rem',
                background: 'var(--bg-secondary, #f3f4f6)', color: 'var(--text-secondary)',
              }}>{sub}</span>
            ))}
          </div>
        )}
      </div>

      <div className="csca-card" style={{ marginBottom: '1.25rem', fontSize: '0.9rem', color: 'var(--text-secondary)' }}>
        {s.checkoutStubNote}
      </div>

      <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
        <button className="csca-btn csca-btn-primary" disabled={loading} onClick={confirm}>
          {loading ? '…' : s.checkoutPay}
        </button>
        <button className="csca-btn csca-btn-ghost" onClick={() => navigate('/')}>{s.checkoutBackHome}</button>
      </div>
    </div>
  );
}

export default CheckoutPage;
