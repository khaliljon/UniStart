import { useEffect, useState } from 'react';
import { specialtyTrackService, type SpecialtyTrack } from '../services/specialtyTrackService';

const SUBJECTS: { key: string; label: string }[] = [
  { key: 'math', label: 'Математика' },
  { key: 'physics', label: 'Физика' },
  { key: 'chemistry', label: 'Химия' },
  { key: 'chineseTech', label: 'Техн. китайский' },
  { key: 'chineseHum', label: 'Гуман. китайский' },
];

function AdminSpecialtyTracksPage() {
  const [items, setItems] = useState<SpecialtyTrack[]>([]);
  const [msg, setMsg] = useState('');
  const [loading, setLoading] = useState(true);

  const load = () => {
    setLoading(true);
    specialtyTrackService.adminList()
      .then(setItems)
      .catch(() => setItems([]))
      .finally(() => setLoading(false));
  };
  useEffect(() => { load(); }, []);

  const flash = (m: string) => { setMsg(m); setTimeout(() => setMsg(''), 2500); };

  const patch = (id: number, p: Partial<SpecialtyTrack>) =>
    setItems((prev) => prev.map((it) => (it.id === id ? { ...it, ...p } : it)));

  const toggleSubject = (it: SpecialtyTrack, key: string) => {
    const next = it.subjects.includes(key)
      ? it.subjects.filter((x) => x !== key)
      : [...it.subjects, key];
    patch(it.id, { subjects: next });
  };

  const save = async (it: SpecialtyTrack) => {
    if (!it.name.trim()) { flash('Укажите название'); return; }
    const dto = {
      name: it.name, nameKz: it.nameKz ?? null, nameEn: it.nameEn ?? null,
      subjects: it.subjects, conditionalChinese: it.conditionalChinese,
      sortOrder: it.sortOrder, isActive: it.isActive,
    };
    try {
      if (it.id < 0) {
        const created = await specialtyTrackService.create(dto);
        setItems((prev) => prev.map((x) => (x.id === it.id ? created : x)));
      } else {
        await specialtyTrackService.update(it.id, dto);
      }
      flash('Сохранено');
    } catch { flash('Ошибка сохранения'); }
  };

  const remove = async (it: SpecialtyTrack) => {
    if (it.id < 0) { setItems((prev) => prev.filter((x) => x.id !== it.id)); return; }
    if (!confirm('Удалить направление?')) return;
    try {
      await specialtyTrackService.remove(it.id);
      setItems((prev) => prev.filter((x) => x.id !== it.id));
      flash('Удалено');
    } catch { flash('Ошибка удаления'); }
  };

  const addRow = () => {
    const tempId = -Date.now();
    const nextOrder = items.reduce((m, x) => Math.max(m, x.sortOrder), 0) + 1;
    setItems((prev) => [...prev, {
      id: tempId, name: '', nameKz: '', nameEn: '',
      subjects: ['math'], conditionalChinese: false, sortOrder: nextOrder, isActive: true,
    }]);
  };

  const input: React.CSSProperties = { padding: '0.4rem 0.5rem', border: '1px solid var(--border-color)', borderRadius: 6, background: 'var(--card-background)', color: 'var(--text-primary)', width: '100%' };

  return (
    <div style={{ maxWidth: 1000, margin: '1.5rem auto', padding: '0 1rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h1 style={{ fontSize: '1.6rem', fontWeight: 700, margin: 0 }}>Специальности и предметы</h1>
        {msg && <span style={{ color: 'var(--success-color, #10b981)', fontSize: '0.85rem' }}>{msg}</span>}
      </div>
      <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '1rem' }}>
        Блок «Какие предметы сдавать» на лендинге и странице «О CSCA». Отметьте предметы для каждого направления.
        Флаг «только на китайском» добавляет пометку, что предмет нужен лишь для программ на китайском языке.
      </p>

      {loading ? (
        <div className="loading"><div className="spinner" /></div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {items.map((it) => (
            <div key={it.id} className="card" style={{ padding: '1rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '0.6rem', marginBottom: '0.6rem' }}>
                <label style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                  Название (RU)
                  <input value={it.name} style={input} onChange={(e) => patch(it.id, { name: e.target.value })} />
                </label>
                <label style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                  KZ
                  <input value={it.nameKz ?? ''} style={input} onChange={(e) => patch(it.id, { nameKz: e.target.value })} />
                </label>
                <label style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                  EN
                  <input value={it.nameEn ?? ''} style={input} onChange={(e) => patch(it.id, { nameEn: e.target.value })} />
                </label>
                <label style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                  Порядок
                  <input type="number" value={it.sortOrder} style={input} onChange={(e) => patch(it.id, { sortOrder: Number(e.target.value) })} />
                </label>
              </div>

              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem', marginBottom: '0.6rem' }}>
                {SUBJECTS.map((s) => (
                  <label key={s.key} style={{ display: 'flex', alignItems: 'center', gap: '0.3rem', fontSize: '0.85rem' }}>
                    <input type="checkbox" checked={it.subjects.includes(s.key)} onChange={() => toggleSubject(it, s.key)} />
                    {s.label}
                  </label>
                ))}
              </div>

              <div style={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: '1rem' }}>
                <label style={{ display: 'flex', alignItems: 'center', gap: '0.3rem', fontSize: '0.85rem' }}>
                  <input type="checkbox" checked={it.conditionalChinese} onChange={(e) => patch(it.id, { conditionalChinese: e.target.checked })} />
                  только для программ на китайском
                </label>
                <label style={{ display: 'flex', alignItems: 'center', gap: '0.3rem', fontSize: '0.85rem' }}>
                  <input type="checkbox" checked={it.isActive} onChange={(e) => patch(it.id, { isActive: e.target.checked })} />
                  Активно
                </label>
                <div style={{ marginLeft: 'auto', whiteSpace: 'nowrap' }}>
                  <button className="btn btn-primary" style={{ fontSize: '0.8rem', padding: '0.3rem 0.6rem', marginRight: '0.4rem' }}
                          onClick={() => save(it)}>Сохранить</button>
                  <button className="btn btn-outline" style={{ fontSize: '0.8rem', padding: '0.3rem 0.6rem', color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                          onClick={() => remove(it)}>Удалить</button>
                </div>
              </div>
            </div>
          ))}
          {items.length === 0 && (
            <div className="card" style={{ padding: '1.5rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
              Направлений пока нет.
            </div>
          )}
        </div>
      )}

      <button className="btn btn-outline" style={{ marginTop: '1rem' }} onClick={addRow}>+ Добавить направление</button>
    </div>
  );
}

export default AdminSpecialtyTracksPage;
