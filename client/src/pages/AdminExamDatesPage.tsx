import { useEffect, useState } from 'react';
import { examSittingsService, type ExamSitting } from '../services/examSittingsService';

/** Admin CRUD for CSCA exam sitting dates (shown on the landing / About CSCA page). */
function AdminExamDatesPage() {
  const [items, setItems] = useState<ExamSitting[]>([]);
  const [msg, setMsg] = useState('');
  const [loading, setLoading] = useState(true);

  const load = () => {
    setLoading(true);
    examSittingsService.adminList()
      .then(setItems)
      .catch(() => setItems([]))
      .finally(() => setLoading(false));
  };
  useEffect(() => { load(); }, []);

  const flash = (m: string) => { setMsg(m); setTimeout(() => setMsg(''), 2500); };

  const patch = (id: number, p: Partial<ExamSitting>) =>
    setItems((prev) => prev.map((it) => (it.id === id ? { ...it, ...p } : it)));

  const save = async (it: ExamSitting) => {
    try {
      const dto = { date: it.date, isActive: it.isActive, sortOrder: it.sortOrder };
      if (it.id < 0) {
        const created = await examSittingsService.create(dto);
        setItems((prev) => prev.map((x) => (x.id === it.id ? created : x)));
      } else {
        await examSittingsService.update(it.id, dto);
      }
      flash('Сохранено');
    } catch { flash('Ошибка сохранения'); }
  };

  const remove = async (it: ExamSitting) => {
    if (it.id < 0) { setItems((prev) => prev.filter((x) => x.id !== it.id)); return; }
    if (!confirm('Удалить дату экзамена?')) return;
    try {
      await examSittingsService.remove(it.id);
      setItems((prev) => prev.filter((x) => x.id !== it.id));
      flash('Удалено');
    } catch { flash('Ошибка удаления'); }
  };

  const addRow = () => {
    const tempId = -Date.now();
    const nextOrder = items.reduce((m, x) => Math.max(m, x.sortOrder), 0) + 1;
    setItems((prev) => [...prev, { id: tempId, date: '2026-01-01', isActive: true, sortOrder: nextOrder }]);
  };

  const input: React.CSSProperties = { padding: '0.4rem 0.5rem', border: '1px solid var(--border-color)', borderRadius: 6, background: 'var(--card-background)', color: 'var(--text-primary)' };
  const th: React.CSSProperties = { padding: '0.5rem 0.75rem', textAlign: 'left', fontSize: '0.8rem', color: 'var(--text-secondary)' };
  const td: React.CSSProperties = { padding: '0.5rem 0.75rem', borderTop: '1px solid var(--border-color)' };

  return (
    <div style={{ maxWidth: 820, margin: '1.5rem auto', padding: '0 1rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h1 style={{ fontSize: '1.6rem', fontWeight: 700, margin: 0 }}>Даты экзаменов</h1>
        {msg && <span style={{ color: 'var(--success-color, #10b981)', fontSize: '0.85rem' }}>{msg}</span>}
      </div>
      <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '1rem' }}>
        Даты показываются на лендинге и странице «О CSCA». Месяц подставляется автоматически из даты.
        Неактивные даты скрыты от пользователей.
      </p>

      {loading ? (
        <div className="loading"><div className="spinner" /></div>
      ) : (
        <div className="card" style={{ padding: 0, overflow: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr>
                <th style={th}>Дата</th>
                <th style={th}>Порядок</th>
                <th style={th}>Активна</th>
                <th style={th}></th>
              </tr>
            </thead>
            <tbody>
              {items.map((it) => (
                <tr key={it.id}>
                  <td style={td}>
                    <input type="date" value={it.date} style={input}
                           onChange={(e) => patch(it.id, { date: e.target.value })} />
                  </td>
                  <td style={td}>
                    <input type="number" value={it.sortOrder} style={{ ...input, width: 70 }}
                           onChange={(e) => patch(it.id, { sortOrder: Number(e.target.value) })} />
                  </td>
                  <td style={td}>
                    <input type="checkbox" checked={it.isActive}
                           onChange={(e) => patch(it.id, { isActive: e.target.checked })} />
                  </td>
                  <td style={{ ...td, whiteSpace: 'nowrap' }}>
                    <button className="btn btn-primary" style={{ fontSize: '0.8rem', padding: '0.3rem 0.6rem', marginRight: '0.4rem' }}
                            onClick={() => save(it)}>Сохранить</button>
                    <button className="btn btn-outline" style={{ fontSize: '0.8rem', padding: '0.3rem 0.6rem', color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                            onClick={() => remove(it)}>Удалить</button>
                  </td>
                </tr>
              ))}
              {items.length === 0 && (
                <tr><td style={td} colSpan={4}><span style={{ color: 'var(--text-secondary)' }}>Дат пока нет.</span></td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      <button className="btn btn-outline" style={{ marginTop: '1rem' }} onClick={addRow}>+ Добавить дату</button>
    </div>
  );
}

export default AdminExamDatesPage;
