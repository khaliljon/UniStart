import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { mockCatalogService, type MockCatalog, type MockTemplate } from '../services/mockCatalogService';
import { cartService } from '../services/cartService';
import { moks } from '../utils/plural';
import { pickLocalized } from '../utils/localize';
import { useTranslation } from '../hooks/useTranslation';
import { cscaStrings } from '../i18n/csca';

/**
 * Storefront for run-based mocks: per-subject run tiers (1/3/5) and discounted
 * packages with subject selection. Used on the Home page. Adds items to the cart.
 */
function MockShop() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [catalog, setCatalog] = useState<MockCatalog | null>(null);
  const [pkgPicker, setPkgPicker] = useState<string | null>(null);
  const [pkgChosen, setPkgChosen] = useState<number[]>([]);
  const cardRefs = useRef<Record<string, HTMLDivElement | null>>({});

  useEffect(() => {
    mockCatalogService.getCatalog().then(setCatalog).catch(() => {});
  }, []);

  const addTierToCart = (tpl: MockTemplate, runs: number, price: number, currency: string) => {
    cartService.add({
      itemType: 'mock',
      itemCode: String(tpl.mockExamId),
      title: `${pickLocalized(tpl.title, tpl.titleKz, tpl.titleEn, locale)} · ${moks(runs, locale)}`,
      amount: price,
      currency,
      runs,
    });
    navigate('/cart');
  };

  const addPackageToCart = (key: string, name: string, price: number, currency: string, selectedMockIds: number[]) => {
    cartService.add({
      itemType: 'package',
      itemCode: key,
      title: name,
      amount: price,
      currency,
      selectedMockIds,
      subjects: selectedMockIds.join(','),
    });
    setPkgPicker(null);
    setPkgChosen([]);
    navigate('/cart');
  };

  // If the user clicked "Buy" on a landing package, focus its card here (open the
  // subject picker for pick packages) once the catalog is loaded — never straight to cart.
  const [highlight, setHighlight] = useState<string | null>(null);
  useEffect(() => {
    if (!catalog) return;
    const key = sessionStorage.getItem('buyPackage');
    if (!key) return;
    sessionStorage.removeItem('buyPackage');
    const pkg = catalog.packages.find((p) => p.key === key);
    if (!pkg) return;
    if (pkg.pickCount > 0) setPkgPicker(pkg.key);
    setTimeout(() => cardRefs.current[pkg.key]?.scrollIntoView({ behavior: 'smooth', block: 'center' }), 120);
    setHighlight(pkg.key);
    setTimeout(() => setHighlight((h) => (h === pkg.key ? null : h)), 2400);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [catalog]);

  // If the user clicked "Buy" on a specific mock on the landing, scroll to that
  // template card here and highlight it briefly (the tier is chosen here).
  useEffect(() => {
    if (!catalog) return;
    const id = sessionStorage.getItem('focusMock');
    if (!id) return;
    sessionStorage.removeItem('focusMock');
    const key = `mock:${id}`;
    setTimeout(() => cardRefs.current[key]?.scrollIntoView({ behavior: 'smooth', block: 'center' }), 120);
    setHighlight(key);
    const t = setTimeout(() => setHighlight(null), 2400);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [catalog]);

  if (!catalog) return null;

  const isEmpty = catalog.templates.length === 0 && catalog.packages.length === 0;
  if (isEmpty) {
    return (
      <div style={{ marginBottom: '1.75rem' }}>
        <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>Пробные экзамены</h2>
        <div className="card" style={{ textAlign: 'center', padding: '1.5rem', color: 'var(--text-secondary)' }}>
          Пробники скоро появятся — мы работаем над этим.
        </div>
      </div>
    );
  }

  return (
    <div style={{ marginBottom: '1.75rem' }}>
      <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>{s.mocksTitle}</h2>

      {catalog.freeRunAvailable && (
        <div className="card" style={{ marginBottom: '1rem', border: '2px dashed var(--primary-color)', background: 'var(--bg-secondary)' }}>
          <div style={{ fontWeight: 700, marginBottom: '0.2rem' }}>{s.firstMockFree}</div>
          <div style={{ color: 'var(--text-secondary)', fontSize: '0.88rem' }}>
            {s.firstMockFreeDesc}
          </div>
        </div>
      )}

      {/* Per-subject run tiers */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '0.75rem', marginBottom: '1.25rem' }}>
        {catalog.templates.map((tpl) => (
          <div key={tpl.mockExamId} ref={(el) => { cardRefs.current[`mock:${tpl.mockExamId}`] = el; }} className="card" style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem', outline: highlight === `mock:${tpl.mockExamId}` ? '2px solid var(--primary-color)' : 'none', outlineOffset: 2, transition: 'outline-color 0.3s' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', flexWrap: 'wrap' }}>
              <div style={{ fontWeight: 700 }}>{pickLocalized(tpl.title, tpl.titleKz, tpl.titleEn, locale)}</div>
              {tpl.runsRemaining > 0 && (
                <span style={{ background: 'rgba(16,185,129,0.12)', color: '#10b981', padding: '1px 7px', borderRadius: 10, fontSize: '0.7rem', fontWeight: 700 }}>
                  {s.mockRemaining}: {moks(tpl.runsRemaining, locale)}
                </span>
              )}
            </div>
            <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
              {tpl.totalQuestions} {s.mockQuestions.toLowerCase()} · {tpl.totalTimeMinutes} {s.minShort}
            </div>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem', marginTop: 'auto' }}>
              {tpl.tiers.length === 0 ? (
                <span style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>Цены не заданы</span>
              ) : tpl.tiers.map((tier) => (
                <button key={tier.id} className="btn btn-outline" style={{ fontSize: '0.85rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '0.5rem', padding: '0.55rem 0.9rem' }}
                        onClick={() => addTierToCart(tpl, tier.runs, tier.price, tier.currency)}>
                  <span style={{ fontWeight: 700 }}>{moks(tier.runs, locale)}</span>
                  <span style={{ color: 'var(--primary-color)', fontWeight: 800, whiteSpace: 'nowrap' }}>{tier.price.toLocaleString('ru-RU')} {tier.currency}</span>
                </button>
              ))}
            </div>
          </div>
        ))}
      </div>

      {/* Packages */}
      {catalog.packages.length > 0 && (
        <>
          <h3 style={{ fontSize: '1rem', marginBottom: '0.5rem' }}>{s.discountPackages}</h3>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '0.75rem' }}>
            {catalog.packages.map((pkg) => {
              const picking = pkgPicker === pkg.key;
              const allSubjects = pkg.pickCount === 0;
              const featured = pkg.key === 'standard';
              const subjLabel = allSubjects ? s.allSubjects
                : pkg.pickCount === 1 ? s.oneSubject
                : pkg.pickCount === 2 ? s.twoSubjects
                : pkg.pickCount === 3 ? s.threeSubjects
                : `${pkg.pickCount}`;
              return (
                <div key={pkg.key} className="card" ref={(el) => { cardRefs.current[pkg.key] = el; }} style={{ position: 'relative', display: 'flex', flexDirection: 'column', gap: '0.5rem', padding: '1.25rem', borderRadius: 16, border: featured ? '2px solid var(--primary-color)' : '1px solid var(--border-color)', overflow: 'hidden', outline: highlight === pkg.key ? '2px solid var(--primary-color)' : 'none', outlineOffset: 2, transition: 'outline-color 0.3s' }}>
                  {featured && (
                    <span style={{ position: 'absolute', top: 0, right: 0, background: 'var(--primary-color)', color: '#fff', fontSize: '0.68rem', fontWeight: 700, padding: '3px 12px', borderBottomLeftRadius: 10 }}>{s.popular}</span>
                  )}
                  <div style={{ fontWeight: 800, fontSize: '1.15rem' }}>{pickLocalized(pkg.name, pkg.nameKz, pkg.nameEn, locale)}</div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.82rem' }}>
                    {subjLabel} × {moks(pkg.runsEach, locale)}
                  </div>
                  <div style={{ fontWeight: 900, fontSize: '1.5rem', color: 'var(--csca-red, #C8102E)', margin: '0.15rem 0' }}>
                    {pkg.price.toLocaleString('ru-RU')} <span style={{ fontSize: '0.9rem', fontWeight: 700 }}>{pkg.currency}</span>
                  </div>
                  <ul style={{ listStyle: 'none', padding: 0, margin: '0 0 0.3rem', display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
                    {[subjLabel, `${moks(pkg.runsEach, locale)} ${s.perSubject}`, s.pkgFeatAi].map((f, i) => (
                      <li key={i} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
                        <Check /> {f}
                      </li>
                    ))}
                  </ul>

                  {picking && !allSubjects && (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.3rem', margin: '0.2rem 0', padding: '0.5rem', background: 'var(--bg-secondary)', borderRadius: 10 }}>
                      {catalog.templates.map((t) => {
                        const checked = pkgChosen.includes(t.mockExamId);
                        const disabled = !checked && pkgChosen.length >= pkg.pickCount;
                        return (
                          <label key={t.mockExamId} style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', fontSize: '0.82rem', opacity: disabled ? 0.5 : 1 }}>
                            <input type="checkbox" checked={checked} disabled={disabled}
                                   onChange={() => setPkgChosen((prev) => checked ? prev.filter((x) => x !== t.mockExamId) : [...prev, t.mockExamId])} />
                            {t.title}
                          </label>
                        );
                      })}
                    </div>
                  )}

                  {allSubjects ? (
                    <button className={`btn ${featured ? 'btn-primary' : 'btn-outline'}`} style={{ marginTop: 'auto', width: '100%' }}
                            onClick={() => addPackageToCart(pkg.key, pkg.name, pkg.price, pkg.currency, [])}>
                      {s.addToCart}
                    </button>
                  ) : picking ? (
                    <button className="btn btn-primary" style={{ marginTop: 'auto', width: '100%' }}
                            disabled={pkgChosen.length !== pkg.pickCount}
                            onClick={() => addPackageToCart(pkg.key, pkg.name, pkg.price, pkg.currency, pkgChosen)}>
                      {s.addToCart} ({pkgChosen.length}/{pkg.pickCount})
                    </button>
                  ) : (
                    <button className={`btn ${featured ? 'btn-primary' : 'btn-outline'}`} style={{ marginTop: 'auto', width: '100%' }}
                            onClick={() => { setPkgPicker(pkg.key); setPkgChosen([]); }}>
                      {s.pickSubjects}
                    </button>
                  )}
                </div>
              );
            })}
          </div>
        </>
      )}
    </div>
  );
}

export default MockShop;

function Check() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="var(--primary-color)" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" style={{ flexShrink: 0 }}>
      <path d="M20 6L9 17l-5-5" />
    </svg>
  );
}
