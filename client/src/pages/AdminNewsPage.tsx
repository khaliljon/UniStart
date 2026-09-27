import { useEffect, useRef, useState } from 'react';
import { newsService, type NewsItem, type NewsUpsert } from '../services/newsService';
import adminService from '../services/adminService';
import { useToast } from '../components/Toast';

const EMPTY: NewsUpsert = { title: '', summary: '', body: '', titleKz: '', titleEn: '', summaryKz: '', summaryEn: '', bodyKz: '', bodyEn: '', imageUrl: '', isPublished: false, category: 'admission', isFeatured: false };

const CATEGORY_LABELS: Record<string, string> = {
  dates: 'Даты экзамена',
  admission: 'Поступление',
  platform: 'Платформа',
  guide: 'Инструкции',
};

function AdminNewsPage() {
  const { showToast } = useToast();
  const [items, setItems] = useState<NewsItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<NewsUpsert>(EMPTY);
  const [uploading, setUploading] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);

  const uploadFile = async (file: File) => {
    setUploading(true);
    try {
      const url = await adminService.uploadImage(file);
      setForm(prev => ({ ...prev, imageUrl: url }));
      showToast('Изображение загружено', 'success');
    } catch {
      showToast('Ошибка загрузки изображения', 'error');
    } finally {
      setUploading(false);
    }
  };
  const [saving, setSaving] = useState(false);

  const load = () => {
    setLoading(true);
    newsService.listAll()
      .then(setItems)
      .catch(() => showToast('Не удалось загрузить новости', 'error'))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const startCreate = () => { setEditingId(0); setForm(EMPTY); };
  const startEdit = (n: NewsItem) => {
    setEditingId(n.id);
    setForm({
      title: n.title, summary: n.summary, body: n.body,
      titleKz: n.titleKz ?? '', titleEn: n.titleEn ?? '',
      summaryKz: n.summaryKz ?? '', summaryEn: n.summaryEn ?? '',
      bodyKz: n.bodyKz ?? '', bodyEn: n.bodyEn ?? '',
      imageUrl: n.imageUrl ?? '', isPublished: n.isPublished,
      category: n.category ?? 'admission', isFeatured: n.isFeatured ?? false,
    });
  };
  const cancel = () => { setEditingId(null); setForm(EMPTY); };

  const save = async () => {
    if (!form.title.trim()) { showToast('Заголовок обязателен', 'error'); return; }
    setSaving(true);
    try {
      if (editingId && editingId > 0) {
        await newsService.update(editingId, form);
        showToast('Новость обновлена', 'success');
      } else {
        await newsService.create(form);
        showToast('Новость создана', 'success');
      }
      cancel();
      load();
    } catch {
      showToast('Ошибка сохранения', 'error');
    } finally {
      setSaving(false);
    }
  };

  const remove = async (id: number) => {
    if (!window.confirm('Удалить новость?')) return;
    try {
      await newsService.remove(id);
      showToast('Новость удалена', 'success');
      load();
    } catch {
      showToast('Ошибка удаления', 'error');
    }
  };

  const inputStyle: React.CSSProperties = {
    width: '100%', padding: '0.6rem 0.8rem', borderRadius: '0.5rem',
    border: '1px solid var(--border-color)', background: 'var(--input-background, var(--card-background))',
    color: 'var(--text-primary)', boxSizing: 'border-box', fontSize: '0.95rem',
  };

  return (
    <div style={{ maxWidth: 860, margin: '1.5rem auto' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
        <h1 style={{ margin: 0 }}>Новости CSCA</h1>
        {editingId === null && (
          <button className="btn btn-primary" onClick={startCreate}>+ Новая новость</button>
        )}
      </div>

      {editingId !== null && (
        <div className="card" style={{ padding: '1.25rem', marginBottom: '1.5rem', display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <h3 style={{ margin: 0 }}>{editingId && editingId > 0 ? 'Редактировать' : 'Создать'} новость</h3>
          <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>Заголовок
            <input style={inputStyle} value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
          </label>
          <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>Заголовок (KZ)
            <input style={inputStyle} value={form.titleKz ?? ''} onChange={(e) => setForm({ ...form, titleKz: e.target.value })} placeholder="Оставьте пустым — покажется русский" />
          </label>
          <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>Заголовок (EN)
            <input style={inputStyle} value={form.titleEn ?? ''} onChange={(e) => setForm({ ...form, titleEn: e.target.value })} placeholder="Leave empty to fall back to Russian" />
          </label>
          <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>Краткое описание (тизер)
            <textarea style={{ ...inputStyle, minHeight: 60 }} value={form.summary} onChange={(e) => setForm({ ...form, summary: e.target.value })} />
          </label>
          <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>Тизер (KZ)
            <textarea style={{ ...inputStyle, minHeight: 60 }} value={form.summaryKz ?? ''} onChange={(e) => setForm({ ...form, summaryKz: e.target.value })} />
          </label>
          <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>Тизер (EN)
            <textarea style={{ ...inputStyle, minHeight: 60 }} value={form.summaryEn ?? ''} onChange={(e) => setForm({ ...form, summaryEn: e.target.value })} />
          </label>
          <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>Текст новости
            <textarea style={{ ...inputStyle, minHeight: 160 }} value={form.body} onChange={(e) => setForm({ ...form, body: e.target.value })} />
          </label>
          <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>Текст (KZ)
            <textarea style={{ ...inputStyle, minHeight: 160 }} value={form.bodyKz ?? ''} onChange={(e) => setForm({ ...form, bodyKz: e.target.value })} />
          </label>
          <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>Текст (EN)
            <textarea style={{ ...inputStyle, minHeight: 160 }} value={form.bodyEn ?? ''} onChange={(e) => setForm({ ...form, bodyEn: e.target.value })} />
          </label>
          <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>Изображение (необязательно)
            <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', marginTop: '0.4rem', flexWrap: 'wrap' }}>
              <input
                ref={fileRef}
                type="file"
                accept="image/*"
                style={{ display: 'none' }}
                onChange={(e) => { const f = e.target.files?.[0]; if (f) uploadFile(f); e.target.value = ''; }}
              />
              <button
                type="button"
                className="btn btn-outline"
                style={{ flexShrink: 0 }}
                disabled={uploading}
                onClick={() => fileRef.current?.click()}
              >
                {uploading ? 'Загрузка…' : 'Выбрать файл'}
              </button>
              <input
                style={{ ...inputStyle, flex: 1, minWidth: 0 }}
                placeholder="или вставьте URL"
                value={form.imageUrl ?? ''}
                onChange={(e) => setForm({ ...form, imageUrl: e.target.value })}
              />
              {form.imageUrl && (
                <button type="button" className="btn btn-outline" style={{ flexShrink: 0, color: '#ef4444', borderColor: '#ef4444' }} onClick={() => setForm({ ...form, imageUrl: '' })}>×</button>
              )}
            </div>
            {form.imageUrl && (
              <img src={form.imageUrl} alt="" style={{ marginTop: '0.5rem', maxHeight: 120, borderRadius: '0.5rem', objectFit: 'cover' }} />
            )}
          </label>
          <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.9rem' }}>
            Рубрика
            <select value={form.category ?? 'admission'}
                    onChange={(e) => setForm({ ...form, category: e.target.value as NewsUpsert['category'] })}
                    style={{ padding: '0.4rem 0.6rem', border: '1px solid var(--border-color)', borderRadius: 6, background: 'var(--card-background)', color: 'var(--text-primary)' }}>
              {Object.entries(CATEGORY_LABELS).map(([k, label]) => (
                <option key={k} value={k}>{label}</option>
              ))}
            </select>
          </label>
          <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.9rem' }}>
            <input type="checkbox" checked={form.isFeatured ?? false} onChange={(e) => setForm({ ...form, isFeatured: e.target.checked })} />
            Главная новость (крупный блок сверху)
          </label>
          <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.9rem' }}>
            <input type="checkbox" checked={form.isPublished} onChange={(e) => setForm({ ...form, isPublished: e.target.checked })} />
            Опубликовать (видно на лендинге и главной)
          </label>
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button className="btn btn-primary" disabled={saving} onClick={save}>{saving ? '…' : 'Сохранить'}</button>
            <button className="btn btn-outline" onClick={cancel}>Отмена</button>
          </div>
        </div>
      )}

      {loading ? (
        <div className="loading"><div className="spinner" /></div>
      ) : items.length === 0 ? (
        <p style={{ color: 'var(--text-secondary)' }}>Новостей пока нет.</p>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.6rem' }}>
          {items.map((n) => (
            <div key={n.id} className="card" style={{ padding: '0.9rem 1.1rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '1rem' }}>
              <div>
                <div style={{ fontWeight: 700 }}>{n.title}</div>
                <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                  {n.isPublished ? '● Опубликовано' : '○ Черновик'}
                  {n.publishedAt && ` · ${new Date(n.publishedAt).toLocaleDateString('ru-RU')}`}
                </div>
              </div>
              <div style={{ display: 'flex', gap: '0.4rem' }}>
                <button className="btn btn-outline btn-sm" onClick={() => startEdit(n)}>Изменить</button>
                <button className="btn btn-outline btn-sm" style={{ color: '#ef4444', borderColor: '#ef4444' }} onClick={() => remove(n.id)}>Удалить</button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default AdminNewsPage;
