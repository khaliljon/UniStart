import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { cscaStrings } from '../i18n/csca';
import { cartService, type CartItem } from '../services/cartService';
import { mockCatalogService, type CheckoutLine, type MockTemplate, type MockPackage } from '../services/mockCatalogService';
import { materialsService, type StudyMaterial } from '../services/materialsService';
import { purchaseService } from '../services/purchaseService';
import { paymentsService } from '../services/paymentsService';
import { pickLocalized } from '../utils/localize';
import { moks } from '../utils/plural';

function CartPage() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [items, setItems] = useState<CartItem[]>([]);
  const [templates, setTemplates] = useState<MockTemplate[]>([]);
  const [packages, setPackages] = useState<MockPackage[]>([]);
  const [materials, setMaterials] = useState<StudyMaterial[]>([]);
  const [processing, setProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const refresh = () => setItems(cartService.list());
    refresh();
    window.addEventListener(cartService.eventName, refresh);
    // Remove any book the user already owns (e.g. left in the cart before purchase)
    // so it can never be paid for twice.
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

  // Catalog is used to turn selected mock IDs (stored on package cart items) into
  // human-readable, localized names for the chips, and to localize item titles.
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

  // Re-derive a localized title from the catalog so it follows the language
  // switch, instead of the snapshot stored when the item was added.
  const displayTitle = (item: CartItem): string => {
    if (item.itemType === 'mock') {
      const tpl = templates.find((t) => String(t.mockExamId) === item.itemCode);
      if (tpl) return `${pickLocalized(tpl.title, tpl.titleKz, tpl.titleEn, locale)} · ${moks(item.runs ?? 1, locale)}`;
    } else if (item.itemType === 'package') {
      const pkg = packages.find((p) => p.key === item.itemCode);
      if (pkg) return pickLocalized(pkg.name, pkg.nameKz, pkg.nameEn, locale);
    } else if (item.itemType === 'book') {
      const mat = materials.find((m) => String(m.id) === item.itemCode);
      if (mat) return pickLocalized(mat.title, mat.titleKz, mat.titleEn, locale);
    }
    return item.title;
  };

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

          <button className="btn btn-primary" style={{ padding: '0.75rem' }} onClick={checkout} disabled={processing}>
            {processing ? '…' : s.cartCheckout}
          </button>
          <button className="btn btn-outline" style={{ padding: '0.75rem' }} onClick={() => navigate('/')}>
            {s.continueShopping}
          </button>
        </div>
      )}
    </div>
  );
}

export default CartPage;
