import { useEffect, useState, useCallback } from 'react';
import mockAdminService, { type MockExamListItem } from '../services/mockAdminService';
import {
  mockCatalogService,
  type AdminTier,
  type AdminPackage,
} from '../services/mockCatalogService';
import { moks } from '../utils/plural';

const RUN_COLUMNS = [1, 3, 5];

interface PkgDraft extends AdminPackage {
  _new?: boolean;
}

function AdminPricingPage() {
  const [mocks, setMocks] = useState<MockExamListItem[]>([]);
  const [tiers, setTiers] = useState<AdminTier[]>([]);
  const [packages, setPackages] = useState<PkgDraft[]>([]);
  const [currency, setCurrency] = useState('KZT');
  const [loading, setLoading] = useState(true);
  const [msg, setMsg] = useState<string | null>(null);
  const [err, setErr] = useState<string | null>(null);

  // Price inputs keyed by `${mockId}:${runs}`
  const [priceInputs, setPriceInputs] = useState<Record<string, string>>({});

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [ms, ts, ps] = await Promise.all([
        mockAdminService.list(),
        mockCatalogService.adminTiers(),
        mockCatalogService.adminPackages(),
      ]);
      setMocks(ms);
      setTiers(ts);
      setPackages(ps);
      if (ts[0]?.currency) setCurrency(ts[0].currency);
      const inputs: Record<string, string> = {};
      ts.forEach((t) => { inputs[`${t.mockExamId}:${t.runs}`] = String(t.price); });
      setPriceInputs(inputs);
    } catch {
      setErr('Не удалось загрузить данные');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const tierFor = (mockId: number, runs: number) =>
    tiers.find((t) => t.mockExamId === mockId && t.runs === runs);

  const saveTiers = async () => {
    setErr(null); setMsg(null);
    try {
      for (const m of mocks) {
        for (const runs of RUN_COLUMNS) {
          const key = `${m.id}:${runs}`;
          const raw = priceInputs[key];
          const price = raw ? Number(raw) : 0;
          const existing = tierFor(m.id, runs);
          if (price > 0) {
            const dto = { mockExamId: m.id, runs, price, currency, isActive: true };
            if (existing) await mockCatalogService.updateTier(existing.id, dto);
            else await mockCatalogService.createTier(dto);
          } else if (existing) {
            await mockCatalogService.deleteTier(existing.id);
          }
        }
      }
      setMsg('Цены моков сохранены');
      await load();
    } catch {
      setErr('Ошибка сохранения цен');
    }
  };

  // ── Packages ─────────────────────────────────────────
  const addPackage = () => {
    setPackages((prev) => [
      ...prev,
      { id: 0, key: '', name: '', pickCount: 2, runsEach: 3, price: 0, currency, sortOrder: prev.length, isActive: true, _new: true },
    ]);
  };

  const updatePkgField = (idx: number, patch: Partial<PkgDraft>) => {
    setPackages((prev) => prev.map((p, i) => (i === idx ? { ...p, ...patch } : p)));
  };

  const savePackage = async (idx: number) => {
    setErr(null); setMsg(null);
    const p = packages[idx];
    const dto = {
      key: p.key.trim(), name: p.name.trim(),
      nameKz: p.nameKz?.trim() || undefined, nameEn: p.nameEn?.trim() || undefined,
      pickCount: p.pickCount, runsEach: p.runsEach,
      price: p.price, currency, sortOrder: p.sortOrder, isActive: p.isActive,
    };
    if (!dto.key || !dto.name) { setErr('Ключ и название пакета обязательны'); return; }
    try {
      if (p._new) await mockCatalogService.createPackage(dto);
      else await mockCatalogService.updatePackage(p.id, dto);
      setMsg('Пакет сохранён');
      await load();
    } catch {
      setErr('Ошибка сохранения пакета');
    }
  };

  const deletePackage = async (idx: number) => {
    const p = packages[idx];
    if (p._new) { setPackages((prev) => prev.filter((_, i) => i !== idx)); return; }
    if (!confirm('Удалить пакет?')) return;
    try { await mockCatalogService.deletePackage(p.id); await load(); }
    catch { setErr('Ошибка удаления'); }
  };

  const input: React.CSSProperties = { padding: '0.4rem', border: '1px solid var(--border-color)', borderRadius: '6px', width: '100%' };
  const th: React.CSSProperties = { padding: '0.5rem', textAlign: 'left', fontSize: '0.8rem', color: 'var(--text-secondary)' };
  const td: React.CSSProperties = { padding: '0.4rem 0.5rem', borderTop: '1px solid var(--border-color)' };

  if (loading) return <div className="loading"><div className="spinner" /></div>;

  return (
    <div style={{ maxWidth: 1000, margin: '1.5rem auto', padding: '0 1rem' }}>
      <h1 style={{ fontSize: '1.75rem', fontWeight: 700, marginBottom: '0.75rem' }}>Цены</h1>

      {err && <div style={{ padding: '0.6rem 1rem', background: 'var(--error-bg)', color: 'var(--error-color)', borderRadius: 8, marginBottom: '1rem' }}>✕ {err}</div>}
      {msg && <div style={{ padding: '0.6rem 1rem', background: 'rgba(16,185,129,0.08)', color: 'var(--success-color)', borderRadius: 8, marginBottom: '1rem' }}>✓ {msg}</div>}

      <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '1rem' }}>
        <label style={{ fontSize: '0.85rem' }}>Валюта:</label>
        <input value={currency} onChange={(e) => setCurrency(e.target.value.toUpperCase())} maxLength={8} style={{ ...input, width: 90 }} />
      </div>

      {/* Tier matrix */}
      <div className="card" style={{ padding: '1.25rem', marginBottom: '2rem', overflow: 'auto' }}>
        <h2 style={{ fontSize: '1.15rem', marginTop: 0 }}>Цены моков (по предметам)</h2>
        <p style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', marginTop: 0 }}>
          Цена за 1 / 3 / 5 моков каждого пробника. Пустая ячейка — тир не продаётся.
        </p>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr>
              <th style={th}>Пробник</th>
              {RUN_COLUMNS.map((r) => <th key={r} style={th}>{moks(r)}</th>)}
            </tr>
          </thead>
          <tbody>
            {mocks.map((m) => (
              <tr key={m.id}>
                <td style={{ ...td, fontWeight: 600 }}>{m.title}</td>
                {RUN_COLUMNS.map((runs) => {
                  const key = `${m.id}:${runs}`;
                  return (
                    <td key={runs} style={td}>
                      <input type="number" min={0} value={priceInputs[key] ?? ''}
                             placeholder="—"
                             onChange={(e) => setPriceInputs((prev) => ({ ...prev, [key]: e.target.value }))}
                             style={{ ...input, width: 110 }} />
                    </td>
                  );
                })}
              </tr>
            ))}
            {mocks.length === 0 && (
              <tr><td style={td} colSpan={RUN_COLUMNS.length + 1}>Сначала создайте пробники во вкладке «Пробники».</td></tr>
            )}
          </tbody>
        </table>
        <button className="btn btn-primary" style={{ marginTop: '1rem' }} onClick={saveTiers}>Сохранить цены моков</button>
      </div>

      {/* Packages */}
      <div className="card" style={{ padding: '1.25rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ fontSize: '1.15rem', margin: 0 }}>Пакеты со скидкой</h2>
          <button className="btn btn-outline" onClick={addPackage}>+ Пакет</button>
        </div>
        <p style={{ fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
          «Выбор предметов» = сколько предметов выбирает покупатель (0 = все). «Моков» = сколько моков на каждый выбранный предмет.
        </p>
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {packages.map((p, idx) => (
            <div key={p.id || `new-${idx}`} className="card" style={{ padding: '0.85rem', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(110px, 1fr))', gap: '0.6rem', alignItems: 'end' }}>
              <label style={{ fontSize: '0.75rem' }}>Ключ<input value={p.key} onChange={(e) => updatePkgField(idx, { key: e.target.value })} style={input} /></label>
              <label style={{ fontSize: '0.75rem' }}>Название (RU)<input value={p.name} onChange={(e) => updatePkgField(idx, { name: e.target.value })} style={input} /></label>
              <label style={{ fontSize: '0.75rem' }}>Название (KZ)<input value={p.nameKz ?? ''} onChange={(e) => updatePkgField(idx, { nameKz: e.target.value })} style={input} /></label>
              <label style={{ fontSize: '0.75rem' }}>Название (EN)<input value={p.nameEn ?? ''} onChange={(e) => updatePkgField(idx, { nameEn: e.target.value })} style={input} /></label>
              <label style={{ fontSize: '0.75rem' }}>Выбор предм. (0=все)<input type="number" min={0} value={p.pickCount} onChange={(e) => updatePkgField(idx, { pickCount: Number(e.target.value) })} style={input} /></label>
              <label style={{ fontSize: '0.75rem' }}>Моков<input type="number" min={1} value={p.runsEach} onChange={(e) => updatePkgField(idx, { runsEach: Number(e.target.value) })} style={input} /></label>
              <label style={{ fontSize: '0.75rem' }}>Цена<input type="number" min={0} value={p.price} onChange={(e) => updatePkgField(idx, { price: Number(e.target.value) })} style={input} /></label>
              <label style={{ fontSize: '0.75rem' }}>Порядок<input type="number" value={p.sortOrder} onChange={(e) => updatePkgField(idx, { sortOrder: Number(e.target.value) })} style={input} /></label>
              <label style={{ fontSize: '0.75rem', display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                <input type="checkbox" checked={p.isActive} onChange={(e) => updatePkgField(idx, { isActive: e.target.checked })} /> Активен
              </label>
              <div style={{ display: 'flex', gap: '0.4rem' }}>
                <button className="btn btn-primary" style={{ fontSize: '0.8rem' }} onClick={() => savePackage(idx)}>Сохранить</button>
                <button className="btn btn-outline" style={{ fontSize: '0.8rem' }} onClick={() => deletePackage(idx)}>Удалить</button>
              </div>
            </div>
          ))}
          {packages.length === 0 && <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Пакетов пока нет.</div>}
        </div>
      </div>
    </div>
  );
}

export default AdminPricingPage;
