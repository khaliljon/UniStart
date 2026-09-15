import { Fragment, useEffect, useState } from 'react';
import { purchaseService, type AdminSales, type AdminPurchase } from '../services/purchaseService';
import { adminPaymentsService, type KaspiPendingOrder } from '../services/adminPaymentsService';

const KASPI_FEE_RATE = 0.0095;

function AdminSalesPage() {
  const [data, setData] = useState<AdminSales | null>(null);
  const [loading, setLoading] = useState(true);
  const [status, setStatus] = useState<string>('');
  const [itemType, setItemType] = useState<string>('');
  const [provider, setProvider] = useState<string>('');
  const [from, setFrom] = useState<string>('');
  const [to, setTo] = useState<string>('');
  const [expanded, setExpanded] = useState<Set<number>>(new Set());

  const [pending, setPending] = useState<KaspiPendingOrder[]>([]);
  const [confirmInputs, setConfirmInputs] = useState<Record<string, { paymentId: string; amount: string }>>({});
  const [confirming, setConfirming] = useState<string | null>(null);
  const [confirmError, setConfirmError] = useState<Record<string, string>>({});

  const params = {
    status: status || undefined,
    itemType: itemType || undefined,
    provider: provider || undefined,
    from: from || undefined,
    to: to || undefined,
  };

  const loadSales = () => {
    setLoading(true);
    purchaseService
      .adminList(params)
      .then(setData)
      .catch(() => setData({ totalRevenue: 0, currency: 'KZT', paidOrders: 0, fees: [], items: [] }))
      .finally(() => setLoading(false));
  };

  const loadPending = () => {
    adminPaymentsService.listKaspiPending().then(setPending).catch(() => setPending([]));
  };

  useEffect(loadSales, [status, itemType, provider, from, to]);
  useEffect(loadPending, []);

  const exportCsv = async () => {
    try {
      const blob = await purchaseService.adminExportCsv(params);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `sales-${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
      URL.revokeObjectURL(url);
    } catch {}
  };

  const fmt = (iso: string) =>
    new Date(iso).toLocaleString('ru-RU', { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });

  const ageLabel = (iso: string) => {
    const mins = Math.max(0, Math.floor((Date.now() - new Date(iso).getTime()) / 60000));
    if (mins < 60) return `${mins} мин`;
    const hours = Math.floor(mins / 60);
    if (hours < 24) return `${hours} ч`;
    return `${Math.floor(hours / 24)} дн`;
  };

  const th: React.CSSProperties = { padding: '0.5rem 0.75rem', textAlign: 'left', fontSize: '0.8rem', color: 'var(--text-secondary)', whiteSpace: 'nowrap' };
  const td: React.CSSProperties = { padding: '0.5rem 0.75rem', fontSize: '0.85rem', borderTop: '1px solid var(--border-color)' };

  const typeLabel = (t: string) =>
    t === 'mock' ? 'Пробник' : t === 'book' ? 'Учебник' : t === 'package' ? 'Пакет' : t;

  const providerOf = (p: AdminPurchase): string =>
    p.paymentProvider ?? (p.polarOrderId ? 'Polar' : '—');

  const rowFee = (p: AdminPurchase): string => {
    if (p.status !== 'Paid') return '—';
    const prov = providerOf(p);
    if (prov === 'Kaspi') {
      const fee = Math.round(p.amount * KASPI_FEE_RATE * 100) / 100;
      return `${fee.toLocaleString('ru-RU')} ${p.currency}`;
    }
    if (prov === 'Polar' && p.platformFeeAmount > 0) {
      return `${p.platformFeeAmount.toLocaleString('ru-RU')} ${(p.platformFeeCurrency ?? '').toUpperCase()}`;
    }
    return '—';
  };

  const toggle = (id: number) =>
    setExpanded((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });

  const setConfirmField = (code: string, field: 'paymentId' | 'amount', value: string) =>
    setConfirmInputs((prev) => {
      const cur = prev[code] ?? { paymentId: '', amount: '' };
      return { ...prev, [code]: { ...cur, [field]: value } };
    });

  const submitConfirm = async (o: KaspiPendingOrder) => {
    const input = confirmInputs[o.orderCode] ?? { paymentId: '', amount: '' };
    const paymentId = input.paymentId.trim();
    const paidAmount = Number(input.amount);
    if (!paymentId) {
      setConfirmError((p) => ({ ...p, [o.orderCode]: 'Укажите Kaspi payment id.' }));
      return;
    }
    if (!Number.isFinite(paidAmount) || paidAmount <= 0) {
      setConfirmError((p) => ({ ...p, [o.orderCode]: 'Укажите корректную оплаченную сумму.' }));
      return;
    }
    if (!window.confirm(`Подтвердить оплату заказа ${o.orderCode} на сумму ${paidAmount.toLocaleString('ru-RU')} ${o.currency}?`)) return;

    setConfirming(o.orderCode);
    setConfirmError((p) => ({ ...p, [o.orderCode]: '' }));
    try {
      await adminPaymentsService.confirmKaspi(o.orderCode, { kaspiPaymentId: paymentId, paidAmount });
      loadPending();
      loadSales();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setConfirmError((p) => ({ ...p, [o.orderCode]: err.response?.data?.error ?? 'Не удалось подтвердить оплату.' }));
    } finally {
      setConfirming(null);
    }
  };

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
          <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>Оплаченных заказов</div>
          <div style={{ fontSize: '1.5rem', fontWeight: 800 }}>{data?.paidOrders ?? 0}</div>
        </div>
        <div className="card" style={{ padding: '1rem 1.25rem', flex: '1 1 220px' }}>
          <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>Комиссии платёжных сервисов</div>
          {(data?.fees?.length ?? 0) === 0 ? (
            <div style={{ fontSize: '1.5rem', fontWeight: 800 }}>—</div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.15rem', marginTop: '0.25rem' }}>
              {data!.fees.map((f, i) => (
                <div key={i} style={{ fontSize: '0.95rem', fontWeight: 700 }}>
                  <span style={{ color: 'var(--text-secondary)', fontWeight: 500 }}>{f.provider}: </span>
                  {f.amount.toLocaleString('ru-RU')} {f.currency}
                </div>
              ))}
            </div>
          )}
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
        <select value={provider} onChange={(e) => setProvider(e.target.value)}
                style={{ padding: '0.4rem 0.6rem', border: '1px solid var(--border-color)', borderRadius: '6px' }}>
          <option value="">Все провайдеры</option>
          <option value="polar">Polar</option>
          <option value="kaspi">Kaspi</option>
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
                <th style={th}></th>
                <th style={th}>Дата</th>
                <th style={th}>Покупатель</th>
                <th style={th}>Тип</th>
                <th style={th}>Товар</th>
                <th style={th}>Провайдер</th>
                <th style={th}>Сумма</th>
                <th style={th}>Комиссия</th>
                <th style={th}>Статус</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((p) => (
                <Fragment key={p.id}>
                  <tr>
                    <td style={{ ...td, width: 28, cursor: 'pointer', color: 'var(--text-secondary)' }} onClick={() => toggle(p.id)}>
                      {expanded.has(p.id) ? '▾' : '▸'}
                    </td>
                    <td style={td}>{fmt(p.purchasedAt)}</td>
                    <td style={td}>
                      <div style={{ fontWeight: 600 }}>{p.userName}</div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{p.userEmail}</div>
                    </td>
                    <td style={td}>{typeLabel(p.itemType)}</td>
                    <td style={td}>{p.title}</td>
                    <td style={td}>{providerOf(p)}</td>
                    <td style={{ ...td, fontWeight: 700, whiteSpace: 'nowrap' }}>
                      {p.amount.toLocaleString('ru-RU')} {p.currency}
                    </td>
                    <td style={{ ...td, whiteSpace: 'nowrap', color: 'var(--text-secondary)' }}>{rowFee(p)}</td>
                    <td style={{ ...td, color: p.status === 'Paid' ? '#10b981' : 'var(--text-secondary)', fontWeight: 600 }}>
                      {p.status}
                    </td>
                  </tr>
                  {expanded.has(p.id) && (
                    <tr>
                      <td style={{ ...td, background: 'var(--bg-secondary, #f9fafb)' }} colSpan={9}>
                        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '1.25rem', fontSize: '0.8rem' }}>
                          <div><span style={{ color: 'var(--text-secondary)' }}>Провайдер: </span>{providerOf(p)}</div>
                          <div><span style={{ color: 'var(--text-secondary)' }}>OrderCode: </span>{p.orderCode ?? '—'}</div>
                          <div><span style={{ color: 'var(--text-secondary)' }}>ExternalPaymentId: </span>{p.externalPaymentId ?? '—'}</div>
                          <div><span style={{ color: 'var(--text-secondary)' }}>PolarOrderId: </span>{p.polarOrderId ?? '—'}</div>
                          <div><span style={{ color: 'var(--text-secondary)' }}>CheckoutRef: </span>{p.checkoutRef ?? '—'}</div>
                        </div>
                      </td>
                    </tr>
                  )}
                </Fragment>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <h2 style={{ fontSize: '1.25rem', fontWeight: 700, margin: '2rem 0 0.75rem' }}>
        Ожидают подтверждения Kaspi {pending.length > 0 && <span style={{ color: 'var(--csca-red, #C8102E)' }}>({pending.length})</span>}
      </h2>
      {pending.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '1.5rem', color: 'var(--text-secondary)' }}>
          Нет заказов, ожидающих подтверждения.
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {pending.map((o) => {
            const input = confirmInputs[o.orderCode] ?? { paymentId: '', amount: '' };
            return (
              <div key={o.orderCode} className="card" style={{ padding: '1rem 1.25rem', display: 'flex', flexDirection: 'column', gap: '0.6rem', border: '1px solid #F14635' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: '0.5rem' }}>
                  <div>
                    <div style={{ fontWeight: 800, fontSize: '1.05rem', letterSpacing: '0.03em' }}>{o.orderCode}</div>
                    <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{o.userName} · {o.email}</div>
                  </div>
                  <div style={{ textAlign: 'right' }}>
                    <div style={{ fontWeight: 800 }}>{o.amount.toLocaleString('ru-RU')} {o.currency}</div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                      создан {fmt(o.createdAt)} · в ожидании {ageLabel(o.createdAt)}
                    </div>
                  </div>
                </div>

                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.6rem', alignItems: 'flex-end' }}>
                  <label style={{ display: 'flex', flexDirection: 'column', gap: '0.2rem', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                    Kaspi payment id
                    <input value={input.paymentId} onChange={(e) => setConfirmField(o.orderCode, 'paymentId', e.target.value)}
                           style={{ padding: '0.4rem 0.6rem', border: '1px solid var(--border-color)', borderRadius: '6px', minWidth: 200 }} />
                  </label>
                  <label style={{ display: 'flex', flexDirection: 'column', gap: '0.2rem', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                    Фактически оплачено ({o.currency})
                    <input type="number" value={input.amount} onChange={(e) => setConfirmField(o.orderCode, 'amount', e.target.value)}
                           placeholder={String(o.amount)}
                           style={{ padding: '0.4rem 0.6rem', border: '1px solid var(--border-color)', borderRadius: '6px', minWidth: 160 }} />
                  </label>
                  <button className="btn btn-primary" style={{ background: '#F14635', borderColor: '#F14635' }}
                          onClick={() => submitConfirm(o)} disabled={confirming === o.orderCode}>
                    {confirming === o.orderCode ? '…' : 'Подтвердить оплату'}
                  </button>
                </div>
                {confirmError[o.orderCode] && (
                  <div style={{ color: 'var(--error-color, #ef4444)', fontSize: '0.8rem' }}>{confirmError[o.orderCode]}</div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

export default AdminSalesPage;
