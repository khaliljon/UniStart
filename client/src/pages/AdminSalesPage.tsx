import { useEffect, useState } from 'react';
import { purchaseService, type AdminSales } from '../services/purchaseService';

function AdminSalesPage() {
  const [data, setData] = useState<AdminSales | null>(null);
  const [loading, setLoading] = useState(true);
  const [status, setStatus] = useState<string>('');
  const [itemType, setItemType] = useState<string>('');
  const [from, setFrom] = useState<string>('');
  const [to, setTo] = useState<string>('');

  const params = {
    status: status || undefined,
    itemType: itemType || undefined,
    from: from || undefined,
    to: to || undefined,
  };

  useEffect(() => {
    setLoading(true);
    purchaseService
      .adminList(params)
      .then(setData)
      .catch(() => setData({ count: 0, totalRevenue: 0, currency: 'KZT', items: [] }))
      .finally(() => setLoading(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [status, itemType, from, to]);

  const exportCsv = async () => {
    try {
      const blob = await purchaseService.adminExportCsv(params);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `sales-${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
      URL.revokeObjectURL(url);
    } catch { /* ignore */ }
  };

  const fmt = (iso: string) =>
    new Date(iso).toLocaleString('ru-RU', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });

  const th: React.CSSProperties = { padding: '0.5rem 0.75rem', textAlign: 'left', fontSize: '0.8rem', color: 'var(--text-secondary)', whiteSpace: 'nowrap' };
  const td: React.CSSProperties = { padding: '0.5rem 0.75rem', fontSize: '0.85rem', borderTop: '1px solid var(--border-color)' };

  const typeLabel = (t: string) =>
    t === 'mock' ? 'Пробник' : t === 'book' ? 'Учебник' : t === 'package' ? 'Пакет' : t;

  return (
    <div style={{ maxWidth: 1000, margin: '1.5rem auto', padding: '0 1rem' }}>
      <h1 style={{ fontSize: '1.75rem', fontWeight: 700, marginBottom: '1rem' }}>Продажи</h1>

      <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginBottom: '1.25rem' }}>
        <div className="card" style={{ padding: '1rem 1.25rem', flex: '1 1 180px' }}>
          <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>Выручка (оплачено)</div>
          <div style={{ fontSize: '1.5rem', fontWeight: 800, color: 'var(--csca-red, #C8102E)' }}>
            {(data?.totalRevenue ?? 0).toLocaleString('ru-RU')} {data?.currency ?? 'KZT'}
          </div>
        </div>
        <div className="card" style={{ padding: '1rem 1.25rem', flex: '1 1 180px' }}>
          <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>Всего заказов</div>
          <div style={{ fontSize: '1.5rem', fontWeight: 800 }}>{data?.count ?? 0}</div>
        </div>
      </div>

      <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap', marginBottom: '1rem' }}>
        <select value={itemType} onChange={(e) => setItemType(e.target.value)}
                style={{ padding: '0.4rem 0.6rem', border: '1px solid var(--border-color)', borderRadius: '6px' }}>
          <option value="">Все типы</option>
          <option value="mock">Пробники</option>
          <option value="book">Учебники</option>
          <option value="package">Пакеты</option>
        </select>
        <select value={status} onChange={(e) => setStatus(e.target.value)}
                style={{ padding: '0.4rem 0.6rem', border: '1px solid var(--border-color)', borderRadius: '6px' }}>
          <option value="">Все статусы</option>
          <option value="Paid">Оплачено</option>
          <option value="Pending">Ожидает</option>
          <option value="Cancelled">Отменено</option>
        </select>
        <label style={{ display: 'flex', alignItems: 'center', gap: '0.35rem', fontSize: '0.85rem' }}>
          с
          <input type="date" value={from} onChange={(e) => setFrom(e.target.value)}
                 style={{ padding: '0.35rem 0.5rem', border: '1px solid var(--border-color)', borderRadius: '6px' }} />
        </label>
        <label style={{ display: 'flex', alignItems: 'center', gap: '0.35rem', fontSize: '0.85rem' }}>
          по
          <input type="date" value={to} onChange={(e) => setTo(e.target.value)}
                 style={{ padding: '0.35rem 0.5rem', border: '1px solid var(--border-color)', borderRadius: '6px' }} />
        </label>
        <button className="btn btn-outline" onClick={exportCsv} disabled={!data || data.items.length === 0}>
          ↓ CSV
        </button>
      </div>

      {loading ? (
        <div className="loading"><div className="spinner" /></div>
      ) : !data || data.items.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
          Пока нет продаж.
        </div>
      ) : (
        <div className="card" style={{ padding: 0, overflow: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr>
                <th style={th}>Дата</th>
                <th style={th}>Покупатель</th>
                <th style={th}>Тип</th>
                <th style={th}>Товар</th>
                <th style={th}>Сумма</th>
                <th style={th}>Статус</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((p) => (
                <tr key={p.id}>
                  <td style={td}>{fmt(p.purchasedAt)}</td>
                  <td style={td}>
                    <div style={{ fontWeight: 600 }}>{p.userName}</div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{p.userEmail}</div>
                  </td>
                  <td style={td}>{typeLabel(p.itemType)}</td>
                  <td style={td}>{p.title}</td>
                  <td style={{ ...td, fontWeight: 700, whiteSpace: 'nowrap' }}>
                    {p.amount.toLocaleString('ru-RU')} {p.currency}
                  </td>
                  <td style={{ ...td, color: p.status === 'Paid' ? '#10b981' : 'var(--text-secondary)', fontWeight: 600 }}>
                    {p.status}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default AdminSalesPage;
