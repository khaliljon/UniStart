import { useEffect, useState } from 'react';
import { supportService, type SupportTicketSummary, type SupportTicketDetail } from '../services/supportService';
import { useToast } from '../components/Toast';

function AdminSupportPage() {
  const { showToast } = useToast();
  const [tickets, setTickets] = useState<SupportTicketSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [active, setActive] = useState<SupportTicketDetail | null>(null);
  const [reply, setReply] = useState('');
  const [sending, setSending] = useState(false);

  const loadTickets = () => {
    setLoading(true);
    supportService.listTickets()
      .then(setTickets)
      .catch(() => showToast('Не удалось загрузить обращения', 'error'))
      .finally(() => setLoading(false));
  };

  useEffect(loadTickets, []);

  const openTicket = (id: number) => {
    supportService.getTicket(id).then(setActive).catch(() => showToast('Ошибка загрузки', 'error'));
  };

  const send = async () => {
    if (!active || !reply.trim()) return;
    setSending(true);
    try {
      await supportService.reply(active.id, reply.trim());
      setReply('');
      openTicket(active.id);
      loadTickets();
      showToast('Ответ отправлен', 'success');
    } catch {
      showToast('Не удалось отправить ответ', 'error');
    } finally {
      setSending(false);
    }
  };

  const fmt = (iso: string) => new Date(iso).toLocaleString('ru-RU', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });
  const name = (t: { firstName?: string | null; username?: string | null; telegramUserId: number }) =>
    t.firstName || (t.username ? `@${t.username}` : `id ${t.telegramUserId}`);

  return (
    <div style={{ maxWidth: 1000, margin: '1.5rem auto' }}>
      <h1 style={{ marginBottom: '1.25rem' }}>Поддержка (Telegram)</h1>

      <div style={{ display: 'grid', gridTemplateColumns: 'minmax(240px, 320px) 1fr', gap: '1rem', alignItems: 'start' }}>
        {/* Ticket list */}
        <div className="card" style={{ padding: '0.5rem' }}>
          {loading ? (
            <div className="loading"><div className="spinner" /></div>
          ) : tickets.length === 0 ? (
            <p style={{ color: 'var(--text-secondary)', padding: '1rem' }}>Обращений пока нет.</p>
          ) : (
            tickets.map((t) => (
              <button
                key={t.id}
                onClick={() => openTicket(t.id)}
                style={{
                  display: 'block', width: '100%', textAlign: 'left', padding: '0.6rem 0.8rem',
                  border: 'none', borderRadius: '0.5rem', cursor: 'pointer', marginBottom: '0.2rem',
                  background: active?.id === t.id ? 'var(--bg-secondary)' : 'transparent',
                  color: 'var(--text-primary)',
                }}
              >
                <div style={{ fontWeight: 600, fontSize: '0.9rem' }}>{name(t)}</div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                  {fmt(t.lastMessageAt)} · {t.messageCount} сообщ.
                </div>
              </button>
            ))
          )}
        </div>

        {/* Thread */}
        <div className="card" style={{ padding: '1rem', minHeight: 320 }}>
          {!active ? (
            <p style={{ color: 'var(--text-secondary)' }}>Выберите обращение слева.</p>
          ) : (
            <>
              <div style={{ fontWeight: 700, marginBottom: '0.75rem' }}>
                {name(active)} {active.username && <span style={{ color: 'var(--text-secondary)', fontWeight: 400 }}>@{active.username}</span>}
              </div>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', marginBottom: '1rem', maxHeight: 420, overflowY: 'auto' }}>
                {active.messages.map((m) => (
                  <div
                    key={m.id}
                    style={{
                      alignSelf: m.direction === 'Out' ? 'flex-end' : 'flex-start',
                      maxWidth: '80%', padding: '0.5rem 0.8rem', borderRadius: '0.75rem',
                      background: m.direction === 'Out' ? 'var(--primary-color)' : 'var(--bg-secondary)',
                      color: m.direction === 'Out' ? '#fff' : 'var(--text-primary)',
                      whiteSpace: 'pre-wrap',
                    }}
                  >
                    <div style={{ fontSize: '0.9rem' }}>{m.text}</div>
                    <div style={{ fontSize: '0.68rem', opacity: 0.7, marginTop: '0.2rem' }}>{fmt(m.createdAt)}</div>
                  </div>
                ))}
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <input
                  value={reply}
                  onChange={(e) => setReply(e.target.value)}
                  onKeyDown={(e) => { if (e.key === 'Enter') send(); }}
                  placeholder="Ответ пользователю…"
                  style={{
                    flex: 1, padding: '0.6rem 0.8rem', borderRadius: '0.5rem',
                    border: '1px solid var(--border-color)', background: 'var(--card-background)',
                    color: 'var(--text-primary)',
                  }}
                />
                <button className="btn btn-primary" disabled={sending || !reply.trim()} onClick={send}>
                  {sending ? '…' : 'Отправить'}
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default AdminSupportPage;
