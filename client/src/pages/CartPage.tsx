import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import { cartService, type CartItem } from '../services/cartService';
import { mockCatalogService, type CheckoutLine, type MockTemplate, type MockPackage } from '../services/mockCatalogService';
import { materialsService, type StudyMaterial } from '../services/materialsService';
import { purchaseService } from '../services/purchaseService';
import { paymentsService, type CheckoutProvider, type KaspiCheckoutResponse } from '../services/paymentsService';
import { trackEvent } from '../utils/analytics';
import { pickLocalized } from '../utils/localize';
import { moks } from '../utils/plural';

// Signature of the cart contents; when it changes, a cached Kaspi order is invalidated.
const cartSignature = (list: CartItem[]) =>
  JSON.stringify(list.map((i) => [i.itemType, i.itemCode, i.language ?? '', i.runs ?? 0, i.amount, (i.selectedMockIds ?? []).join('|')]));

// Kaspi checkout is cached for the current browser session only (survives refresh, not a new session).
const KASPI_CHECKOUT_KEY = 'kaspi_checkout';
type StoredKaspiOrder = KaspiCheckoutResponse & { sig: string };

const loadStoredKaspi = (): StoredKaspiOrder | null => {
  try {
    const raw = sessionStorage.getItem(KASPI_CHECKOUT_KEY);
    if (!raw) return null;
    const o = JSON.parse(raw);
    if (o && o.provider === 'kaspi' && typeof o.orderCode === 'string' && typeof o.paymentUrl === 'string'
        && typeof o.amount === 'number' && typeof o.currency === 'string' && typeof o.sig === 'string') {
      return o as StoredKaspiOrder;
    }
    sessionStorage.removeItem(KASPI_CHECKOUT_KEY); // corrupt / incomplete
    return null;
  } catch {
    sessionStorage.removeItem(KASPI_CHECKOUT_KEY);
    return null;
  }
};

function CartPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [items, setItems] = useState<CartItem[]>(() => cartService.list());
  const [templates, setTemplates] = useState<MockTemplate[]>([]);
  const [packages, setPackages] = useState<MockPackage[]>([]);
  const [materials, setMaterials] = useState<StudyMaterial[]>([]);
  const [processing, setProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [kaspiOrder, setKaspiOrder] = useState<StoredKaspiOrder | null>(() => loadStoredKaspi());
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    const refresh = () => setItems(cartService.list());
    refresh();
    window.addEventListener(cartService.eventName, refresh);
    purchaseService.list()
      .then((ps) => {
        const owned = new Set(ps.filter((p) => p.itemType === 'book').map((p) => p.itemCode));
        cartService.list().forEach((i) => {
          if (i.itemType === 'book' && owned.has(i.itemCode)) cartService.remove(i.itemType, i.itemCode);
        });
      })
      .catch(() => {});
    return () => window.removeEventListener(cartService.eventName, refresh);
  }, []);

  // Invalidate a cached Kaspi checkout when the cart contents change.
  useEffect(() => {
    setKaspiOrder((cur) => (cur && cur.sig !== cartSignature(items) ? null : cur));
  }, [items]);

  // Keep the session-scoped Kaspi checkout in sync with state (save on create, clear on reset).
  useEffect(() => {
    if (kaspiOrder) sessionStorage.setItem(KASPI_CHECKOUT_KEY, JSON.stringify(kaspiOrder));
    else sessionStorage.removeItem(KASPI_CHECKOUT_KEY);
  }, [kaspiOrder]);

  useEffect(() => {
    mockCatalogService.getCatalog().then((c) => { setTemplates(c.templates); setPackages(c.packages); }).catch(() => {});
    materialsService.list().then(setMaterials).catch(() => {});
  }, []);

  const nameById = new Map(
    templates.map((t) => [String(t.mockExamId), pickLocalized(t.title, t.titleKz, t.titleEn, locale)])
  );
  const subjectName: Record<string, string> = {
    math: s.subjMath,
    physics: s.subjPhysics,
    chemistry: s.subjChemistry,
    chineseTech: s.subjChineseTech,
    chineseHum: s.subjChineseHum,
  };
  const chipLabel = (sub: string) => nameById.get(sub) ?? subjectName[sub] ?? sub;

  const displayTitle = (item: CartItem): string => {
    const langSuffix = item.language ? ` · ${item.language.toUpperCase()}` : '';
    if (item.itemType === 'mock') {
      const tpl = templates.find((t) => String(t.mockExamId) === item.itemCode);
      if (tpl) return `${pickLocalized(tpl.title, tpl.titleKz, tpl.titleEn, locale)} · ${moks(item.runs ?? 1, locale)}${langSuffix}`;
    } else if (item.itemType === 'package') {
      const pkg = packages.find((p) => p.key === item.itemCode);
      if (pkg) return `${pickLocalized(pkg.name, pkg.nameKz, pkg.nameEn, locale)}${langSuffix}`;
    } else if (item.itemType === 'book') {
      const mat = materials.find((m) => String(m.id) === item.itemCode);
      if (mat) return pickLocalized(mat.title, mat.titleKz, mat.titleEn, locale);
    }
    return item.title;
  };

  const total = items.reduce((sum, i) => sum + i.amount, 0);
  const currency = items[0]?.currency ?? s.currency;

  const remove = (item: CartItem) => cartService.remove(item.itemType, item.itemCode, item.language);

  const checkout = async (provider: CheckoutProvider) => {
    if (items.length === 0) return;
    setProcessing(true);
    setError(null);
    try {
      const lines: CheckoutLine[] = items.map((i) => {
        if (i.itemType === 'mock') return { kind: 'mock', mockExamId: Number(i.itemCode), runs: i.runs ?? 1, language: i.language };
        if (i.itemType === 'package') return { kind: 'package', packageKey: i.itemCode, selectedMockIds: i.selectedMockIds ?? [], language: i.language };
        return { kind: 'book', bookMaterialId: Number(i.itemCode) };
      });

      const res = await paymentsService.createCheckout(lines, provider);
      trackEvent('begin_checkout', {
        currency,
        value: total,
        items: items.map((i) => ({
          item_id: i.itemCode,
          item_name: displayTitle(i),
          item_category: i.itemType,
          item_variant: i.language ?? undefined,
          price: i.amount,
          quantity: 1,
        })),
      });

      if (res.provider === 'kaspi') {
        // Do NOT clear the cart or mark as paid — access is granted only after
        // an admin confirms the Kaspi payment. Cache the order so re-clicking just
        // reopens the same paymentUrl instead of creating a new PaymentOrder.
        setKaspiOrder({ ...res, sig: cartSignature(items) });
      } else {
        window.location.href = res.url;
      }
    } catch {
      setError(s.checkoutError);
    } finally {
      setProcessing(false);
    }
  };

  const copyOrderCode = async () => {
    if (!kaspiOrder) return;
    try {
      await navigator.clipboard.writeText(kaspiOrder.orderCode);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch { /* clipboard unavailable */ }
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
            <div key={`${item.itemType}:${item.itemCode}:${item.language ?? ''}`} className="csca-card" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '1rem' }}>
              <div>
                <div style={{ fontWeight: 700 }}>{displayTitle(item)}</div>
                {item.subjects && (
                  <div style={{ marginTop: '0.4rem', display: 'flex', flexWrap: 'wrap', gap: '0.35rem' }}>
                    {item.subjects.split(',').filter(Boolean).map((sub) => (
                      <span key={sub} style={{ fontSize: '0.75rem', padding: '0.15rem 0.5rem', borderRadius: '1rem', background: 'var(--bg-secondary, #f3f4f6)', color: 'var(--text-secondary)' }}>{chipLabel(sub)}</span>
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

          {kaspiOrder ? (
            <div className="csca-card" style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', border: '2px solid #F14635' }}>
              <div style={{ fontWeight: 800, fontSize: '1.1rem' }}>{s.kaspiTitle}</div>
              <div style={{ fontSize: '0.9rem', color: 'var(--text-secondary)' }}>{s.kaspiOrderCodeHint}</div>

              <div>
                <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>{s.kaspiCourseFieldName}</div>
                <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                  <code style={{ flex: 1, fontSize: '1.1rem', fontWeight: 800, letterSpacing: '0.05em', padding: '0.5rem 0.75rem', borderRadius: '0.5rem', background: 'var(--bg-secondary, #f3f4f6)' }}>
                    {kaspiOrder.orderCode}
                  </code>
                  <button className="btn btn-outline" style={{ whiteSpace: 'nowrap' }} onClick={copyOrderCode}>
                    {copied ? s.kaspiCopied : s.kaspiCopy}
                  </button>
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontWeight: 700 }}>{s.kaspiAmountLabel}</span>
                <span style={{ fontWeight: 800, fontSize: '1.2rem', color: 'var(--csca-red, #C8102E)' }}>
                  {kaspiOrder.amount.toLocaleString('ru-RU')} {kaspiOrder.currency}
                </span>
              </div>

              <a className="btn btn-primary" style={{ padding: '0.75rem', textAlign: 'center', background: '#F14635', borderColor: '#F14635' }}
                 href={kaspiOrder.paymentUrl} target="_blank" rel="noopener noreferrer">
                {s.payWithKaspi}
              </a>

              <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)' }}>{s.kaspiPendingNote}</div>

              <button className="btn btn-outline" style={{ padding: '0.6rem' }} onClick={() => setKaspiOrder(null)}>
                {s.kaspiBack}
              </button>
            </div>
          ) : (
            <>
              <div style={{ fontWeight: 700, fontSize: '0.95rem' }}>{s.choosePaymentMethod}</div>

              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.3rem' }}>
                <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{s.kaspiRecommendedNote}</div>
                <button className="btn btn-primary" style={{ padding: '0.75rem', background: '#F14635', borderColor: '#F14635' }} onClick={() => checkout('kaspi')} disabled={processing}>
                  {processing ? '…' : s.payWithKaspi}
                </button>
              </div>

              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.3rem' }}>
                <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{s.polarInternationalNote}</div>
                <button className="btn btn-outline" style={{ padding: '0.75rem' }} onClick={() => checkout('polar')} disabled={processing}>
                  {processing ? '…' : s.payWithPolar}
                </button>
              </div>

              <button className="btn btn-outline" style={{ padding: '0.75rem' }} onClick={() => navigate('/')}>
                {s.continueShopping}
              </button>
            </>
          )}
        </div>
      )}
    </div>
  );
}

export default CartPage;
