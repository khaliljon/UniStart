import { Fragment, useEffect, useState } from 'react';
import { purchaseService, type AdminSales, type AdminPurchase } from '../services/purchaseService';
import { adminPaymentsService, type KaspiPendingOrder, type KaspiNotification } from '../services/adminPaymentsService';

const KASPI_FEE_RATE = 0.0095;

// Calm, non-alarming status labels so unmatched/old emails don't look like required tasks.
const notifStatusLabel = (s: string): string =>
  s === 'Matched' ? 'Готово к подтверждению'
  : s === 'RequiresReview' ? 'Не сопоставлен'
  : s === 'Processed' ? 'Обработан'
  : s === 'Rejected' ? 'Игнорируется'
  : s;

const friendlyReason = (msg?: string | null): string => {
  if (!msg) return 'не удалось сопоставить автоматически';
  const m = msg.toLowerCase();
  if (m.includes('order code')) return 'отсутствует или неверный код заказа';
  if (m.includes('order not found')) return 'заказ не найден';
  if (m.includes('amount mismatch')) return 'сумма не совпадает';
  if (m.includes('currency')) return 'валюта не KZT';
  if (m.includes('order already')) return 'заказ уже обработан';
  if (m.includes('payment id already') || m.includes('already used') || m.includes('already matched')) return 'этот платёж уже использован';
  if (m.includes('authentication')) return 'письмо не прошло проверку подлинности';
  if (m.includes('missing required')) return 'в письме не хватает данных';
  return msg;
};

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
  const [notifications, setNotifications] = useState<KaspiNotification[]>([]);
  const [confirmingNotif, setConfirmingNotif] = useState<number | null>(null);
  const [notifError, setNotifError] = useState<Record<number, string>>({});

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

  const loadNotifications = () => {
    adminPaymentsService.listKaspiNotifications().then(setNotifications).catch(() => setNotifications([]));
  };

  useEffect(loadSales, [status, itemType, provider, from, to]);
  useEffect(loadPending, []);
  useEffect(loadNotifications, []);

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

  const confirmNotification = async (n: KaspiNotification) => {
    if (!window.confirm(`Подтвердить платёж по заказу ${n.orderCode ?? '—'} и выдать доступ?`)) return;
    setConfirmingNotif(n.id);
    setNotifError((p) => ({ ...p, [n.id]: '' }));
    try {
      await adminPaymentsService.confirmKaspiNotification(n.id);
      loadNotifications();
      loadPending();
      loadSales();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setNotifError((p) => ({ ...p, [n.id]: err.response?.data?.error ?? 'Не удалось подтвердить платёж.' }));
    } finally {
      setConfirmingNotif(null);
    }
  };

  const rejectNotification = async (n: KaspiNotification) => {
    if (!window.confirm(`Игнорировать это письмо (${n.orderCode ?? 'без кода заказа'})? Доступ выдан не будет.`)) return;
    setConfirmingNotif(n.id);
    setNotifError((p) => ({ ...p, [n.id]: '' }));
    try {
      await adminPaymentsService.rejectKaspiNotification(n.id);
      loadNotifications();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setNotifError((p) => ({ ...p, [n.id]: err.response?.data?.error ?? 'Не удалось изменить статус.' }));
    } finally {
      setConfirmingNotif(null);
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
        Обнаруженные платежи Kaspi {notifications.length > 0 && <span style={{ color: 'var(--csca-red, #C8102E)' }}>({notifications.length})</span>}
      </h2>
      {notifications.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '1.5rem', color: 'var(--text-secondary)' }}>
          Нет обнаруженных платежей, требующих внимания.
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {notifications.map((n) => {
            const matched = n.status === 'Matched';
            const hasAmounts = n.amount != null && n.expectedAmount != null;
            const delta = hasAmounts ? (n.amount as number) - (n.expectedAmount as number) : 0;
            const isAmountMismatch = hasAmounts && delta !== 0;
            return (
              <div key={n.id} className="card" style={{ padding: '1rem 1.25rem', display: 'flex', flexDirection: 'column', gap: '0.6rem', border: `1px solid ${matched ? '#10b981' : '#f59e0b'}` }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: '0.5rem' }}>
                  <div>
                    <div style={{ fontWeight: 800, fontSize: '1.05rem', letterSpacing: '0.03em' }}>
                      {n.orderCode ?? '—'}{' '}
                      <span style={{ fontSize: '0.72rem', fontWeight: 700, color: matched ? '#10b981' : '#b45309', padding: '0.1rem 0.45rem', borderRadius: '999px', background: matched ? 'rgba(16,185,129,0.12)' : 'rgba(245,158,11,0.15)' }}>
                        {notifStatusLabel(n.status)}
                      </span>
                    </div>
                    <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                      {n.userName ? `${n.userName} · ${n.userEmail}` : 'Заказ не сопоставлен'}
                    </div>
                  </div>
                  <div style={{ textAlign: 'right', fontSize: '0.8rem' }}>
                    <div style={{ fontWeight: 800, fontSize: '1rem' }}>
                      {n.amount != null ? `${n.amount.toLocaleString('ru-RU')} ${n.currency}` : '—'}
                    </div>
                    {n.expectedAmount != null && (
                      <div style={{ color: 'var(--text-secondary)' }}>ожидалось {n.expectedAmount.toLocaleString('ru-RU')} {n.currency}</div>
                    )}
                  </div>
                </div>

                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '1rem', fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                  <span>Kaspi id: {n.kaspiPaymentId ?? '—'}</span>
                  <span>оплата: {n.paidAt ? fmt(n.paidAt) : '—'}</span>
                  <span>письмо: {fmt(n.receivedAt)}</span>
                </div>

                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '1rem', fontSize: '0.8rem' }}>
                  <span style={{ color: n.orderFound ? '#10b981' : '#ef4444' }}>{n.orderFound ? '✓' : '✕'} заказ найден</span>
                  <span style={{ color: n.amountMatches ? '#10b981' : '#ef4444' }}>{n.amountMatches ? '✓' : '✕'} сумма совпадает</span>
                  <span style={{ color: n.paymentIdUnique ? '#10b981' : '#ef4444' }}>{n.paymentIdUnique ? '✓' : '✕'} payment id уникален</span>
                </div>

                {matched ? (
                  <div>
                    <button className="btn btn-primary" style={{ background: '#10b981', borderColor: '#10b981' }}
                            onClick={() => confirmNotification(n)} disabled={confirmingNotif === n.id}>
                      {confirmingNotif === n.id ? '…' : 'Подтвердить и выдать доступ'}
                    </button>
                    {notifError[n.id] && (
                      <div style={{ color: 'var(--error-color, #ef4444)', fontSize: '0.8rem', marginTop: '0.4rem' }}>{notifError[n.id]}</div>
                    )}
                  </div>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                    {isAmountMismatch ? (
                      <div style={{ display: 'flex', flexDirection: 'column', gap: '0.2rem', fontSize: '0.82rem' }}>
                        <div>Ожидалось: <b>{(n.expectedAmount as number).toLocaleString('ru-RU')} {n.currency}</b></div>
                        <div>Фактически оплачено: <b>{(n.amount as number).toLocaleString('ru-RU')} {n.currency}</b></div>
                        <div style={{ color: '#ef4444', fontWeight: 700 }}>
                          {delta < 0 ? 'Недоплата' : 'Переплата'}: {Math.abs(delta).toLocaleString('ru-RU')} {n.currency}
                        </div>
                        <div style={{ color: '#b45309' }}>Причина: сумма не совпадает</div>
                      </div>
                    ) : (
                      <div style={{ fontSize: '0.82rem', color: '#b45309' }}>
                        Причина: {friendlyReason(n.errorMessage)}
                      </div>
                    )}
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                      <button className="btn btn-outline" style={{ color: 'var(--text-secondary)' }}
                              onClick={() => rejectNotification(n)} disabled={confirmingNotif === n.id}>
                        {confirmingNotif === n.id ? '…' : 'Игнорировать'}
                      </button>
                    </div>
                    {notifError[n.id] && (
                      <div style={{ color: 'var(--error-color, #ef4444)', fontSize: '0.8rem' }}>{notifError[n.id]}</div>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      <h2 style={{ fontSize: '1.1rem', fontWeight: 700, margin: '2rem 0 0.5rem', color: 'var(--text-secondary)' }}>
        Созданные Kaspi-заказы {pending.length > 0 && <span>({pending.length})</span>}
      </h2>
      <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginBottom: '0.6rem' }}>
        Технические попытки checkout. Создание заказа не является подтверждением оплаты.
      </div>
      {pending.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '1.25rem', color: 'var(--text-secondary)' }}>
          Нет созданных Kaspi-заказов.
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
          {pending.map((o) => (
            <div key={o.orderCode} className="card" style={{ padding: '0.75rem 1rem', display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: '0.5rem' }}>
              <div>
                <div style={{ fontWeight: 700, letterSpacing: '0.03em' }}>{o.orderCode}</div>
                <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>{o.userName} · {o.email}</div>
              </div>
              <div style={{ textAlign: 'right' }}>
                <div style={{ fontWeight: 700 }}>{o.amount.toLocaleString('ru-RU')} {o.currency}</div>
                <div style={{ fontSize: '0.72rem', color: 'var(--text-secondary)' }}>
                  {fmt(o.createdAt)} · {ageLabel(o.createdAt)} · Оплата не подтверждена
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default AdminSalesPage;
