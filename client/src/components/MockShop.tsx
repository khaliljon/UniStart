import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { mockCatalogService, type MockCatalog, type MockTemplate } from '../services/mockCatalogService';
import { cartService } from '../services/cartService';

/**
 * Storefront for run-based mocks: per-subject run tiers (1/3/5) and discounted
 * packages with subject selection. Used on the Home page. Adds items to the cart.
 */
function MockShop() {
  const navigate = useNavigate();
  const [catalog, setCatalog] = useState<MockCatalog | null>(null);
  const [pkgPicker, setPkgPicker] = useState<string | null>(null);
  const [pkgChosen, setPkgChosen] = useState<number[]>([]);

  useEffect(() => {
    mockCatalogService.getCatalog().then(setCatalog).catch(() => {});
  }, []);

  const addTierToCart = (tpl: MockTemplate, runs: number, price: number, currency: string) => {
    cartService.add({
      itemType: 'mock',
      itemCode: String(tpl.mockExamId),
      title: `${tpl.title} · ${runs} зап.`,
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
      <h2 style={{ fontSize: '1.1rem', marginBottom: '0.75rem' }}>Пробные экзамены</h2>

      {catalog.freeRunAvailable && (
        <div className="card" style={{ marginBottom: '1rem', border: '2px dashed var(--primary-color)', background: 'var(--bg-secondary)' }}>
          <div style={{ fontWeight: 700, marginBottom: '0.2rem' }}>🎁 Первый запуск — бесплатно</div>
          <div style={{ color: 'var(--text-secondary)', fontSize: '0.88rem' }}>
            Начните любой пробник бесплатно во вкладке «Пробные экзамены». Дальше — покупка запусков поштучно или пакетом.
          </div>
        </div>
      )}

      {/* Per-subject run tiers */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '0.75rem', marginBottom: '1.25rem' }}>
        {catalog.templates.map((tpl) => (
          <div key={tpl.mockExamId} className="card" style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', flexWrap: 'wrap' }}>
              <div style={{ fontWeight: 700 }}>{tpl.title}</div>
              {tpl.runsRemaining > 0 && (
                <span style={{ background: 'rgba(16,185,129,0.12)', color: '#10b981', padding: '1px 7px', borderRadius: 10, fontSize: '0.7rem', fontWeight: 700 }}>
                  Запусков: {tpl.runsRemaining}
                </span>
              )}
            </div>
            <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
              {tpl.totalQuestions} вопросов · {tpl.totalTimeMinutes} мин
            </div>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem', marginTop: 'auto' }}>
              {tpl.tiers.length === 0 ? (
                <span style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>Цены не заданы</span>
              ) : tpl.tiers.map((tier) => (
                <button key={tier.id} className="btn btn-outline" style={{ fontSize: '0.8rem', flex: '1 1 auto', whiteSpace: 'nowrap' }}
                        onClick={() => addTierToCart(tpl, tier.runs, tier.price, tier.currency)}>
                  {tier.runs} зап. · {tier.price.toLocaleString('ru-RU')} {tier.currency}
                </button>
              ))}
            </div>
          </div>
        ))}
      </div>

      {/* Packages */}
      {catalog.packages.length > 0 && (
        <>
          <h3 style={{ fontSize: '1rem', marginBottom: '0.5rem' }}>Пакеты со скидкой</h3>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '0.75rem' }}>
            {catalog.packages.map((pkg) => {
              const picking = pkgPicker === pkg.key;
              const allSubjects = pkg.pickCount === 0;
              return (
                <div key={pkg.key} className="card" style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                  <div style={{ fontWeight: 800, fontSize: '1.05rem' }}>{pkg.name}</div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.82rem' }}>
                    {allSubjects ? 'Все предметы' : `Любые ${pkg.pickCount} предмета`} × {pkg.runsEach} запусков
                  </div>
                  <div style={{ fontWeight: 800, fontSize: '1.2rem', color: 'var(--csca-red, #C8102E)' }}>
                    {pkg.price.toLocaleString('ru-RU')} {pkg.currency}
                  </div>

                  {picking && !allSubjects && (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.3rem', margin: '0.4rem 0' }}>
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
                    <button className="btn btn-primary" style={{ marginTop: 'auto' }}
                            onClick={() => addPackageToCart(pkg.key, pkg.name, pkg.price, pkg.currency, [])}>
                      В корзину
                    </button>
                  ) : picking ? (
                    <button className="btn btn-primary" style={{ marginTop: 'auto' }}
                            disabled={pkgChosen.length !== pkg.pickCount}
                            onClick={() => addPackageToCart(pkg.key, pkg.name, pkg.price, pkg.currency, pkgChosen)}>
                      В корзину ({pkgChosen.length}/{pkg.pickCount})
                    </button>
                  ) : (
                    <button className="btn btn-outline" style={{ marginTop: 'auto' }}
                            onClick={() => { setPkgPicker(pkg.key); setPkgChosen([]); }}>
                      Выбрать предметы
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
