import { useState } from 'react';
import api from '../services/api';
import { useTranslation } from '../hooks/useTranslation';

function ContactForm() {
  const { t } = useTranslation();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [sending, setSending] = useState(false);
  const [sent, setSent] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim() || !email.trim() || !message.trim()) return;
    setSending(true);
    setError(null);
    try {
      await api.post('/contact', { name: name.trim(), email: email.trim(), message: message.trim() });
      setSent(true);
      setName(''); setEmail(''); setMessage('');
    } catch {
      setError(t.contact?.error || 'Ошибка отправки');
    } finally {
      setSending(false);
    }
  };

  if (sent) {
    return (
      <div style={{
        padding: '1rem 1.25rem', borderRadius: '0.75rem',
        background: '#22c55e11', border: '1px solid #22c55e44',
        textAlign: 'center', fontSize: '0.9rem', color: '#22c55e',
      }}>
        {t.contact?.sent || 'Сообщение отправлено!'} ✓
      </div>
    );
  }

  return (
    <div style={{
      padding: '1rem 1.25rem', borderRadius: '0.75rem',
      background: 'var(--bg-secondary)', border: '1px solid var(--border-color)',
    }}>
      <div style={{ fontWeight: 600, marginBottom: '0.75rem', fontSize: '0.95rem' }}>
        {t.contact?.title || 'Обратная связь'}
      </div>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
        <input
          type="text"
          className="form-input"
          placeholder={t.contact?.namePlaceholder || 'Ваше имя'}
          value={name}
          onChange={e => setName(e.target.value)}
          required
          maxLength={200}
          style={{ fontSize: '0.85rem' }}
        />
        <input
          type="email"
          className="form-input"
          placeholder="Email"
          value={email}
          onChange={e => setEmail(e.target.value)}
          required
          maxLength={200}
          style={{ fontSize: '0.85rem' }}
        />
        <textarea
          className="form-input"
          placeholder={t.contact?.messagePlaceholder || 'Ваше сообщение...'}
          value={message}
          onChange={e => setMessage(e.target.value)}
          required
          maxLength={5000}
          rows={3}
          style={{ fontSize: '0.85rem', resize: 'vertical' }}
        />
        {error && <div style={{ color: '#dc2626', fontSize: '0.8rem' }}>{error}</div>}
        <button
          type="submit"
          className="btn btn-primary"
          disabled={sending}
          style={{ alignSelf: 'flex-start', padding: '0.4rem 1.2rem', fontSize: '0.85rem' }}
        >
          {sending ? '...' : (t.contact?.send || 'Отправить')}
        </button>
      </form>
      <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', marginTop: '0.5rem' }}>
        {t.contact?.emailDirect || 'Или напишите нам: '}<a href="mailto:unistart.kz@gmail.com" style={{ color: 'var(--primary-color)' }}>unistart.kz@gmail.com</a>
      </div>
    </div>
  );
}

export default ContactForm;
