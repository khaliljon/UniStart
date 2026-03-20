import { useState, useEffect, useCallback, useRef } from 'react';
import { tutorService } from '../services/tutorService';
import type { SchoolAdmin, CreateSchoolRequest, UpdateSchoolRequest } from '../types';

const EXAM_OPTIONS = ['SAT', 'TOEFL', 'IELTS', 'NUET', 'CSCA'];

function TutorSchoolManagePage() {
  const [school, setSchool] = useState<SchoolAdmin | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const [error, setError] = useState('');

  // Form fields
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [logoUrl, setLogoUrl] = useState('');
  const [websiteUrl, setWebsiteUrl] = useState('');
  const [instagramUrl, setInstagramUrl] = useState('');
  const [telegramUrl, setTelegramUrl] = useState('');
  const [specializations, setSpecializations] = useState<string[]>([]);

  // Tutor management
  const [tutorIdInput, setTutorIdInput] = useState('');
  const [removeTutorIdInput, setRemoveTutorIdInput] = useState('');
  const [tutorAction, setTutorAction] = useState('');

  const savedTimerRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  useEffect(() => {
    return () => { clearTimeout(savedTimerRef.current); };
  }, []);

  const populateForm = (s: SchoolAdmin) => {
    setName(s.name);
    setDescription(s.description || '');
    setLogoUrl(s.logoUrl || '');
    setWebsiteUrl(s.websiteUrl || '');
    setInstagramUrl(s.instagramUrl || '');
    setTelegramUrl(s.telegramUrl || '');
    setSpecializations(s.specializations || []);
  };

  const loadSchool = useCallback(async () => {
    try {
      const data = await tutorService.getMySchool();
      setSchool(data);
      if (data) populateForm(data);
    } catch {
      console.error('Failed to load school');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadSchool();
  }, [loadSchool]);

  const handleCreate = async () => {
    if (!name.trim()) {
      setError('Введите название школы');
      return;
    }
    setSaving(true);
    setError('');
    try {
      const data: CreateSchoolRequest = {
        name: name.trim(),
        description: description.trim() || undefined,
        logoUrl: logoUrl.trim() || undefined,
        websiteUrl: websiteUrl.trim() || undefined,
        instagramUrl: instagramUrl.trim() || undefined,
        telegramUrl: telegramUrl.trim() || undefined,
        specializations: specializations.length > 0 ? specializations.join(',') : undefined,
      };
      const created = await tutorService.createSchool(data);
      setSchool(created);
      populateForm(created);
      setSaved(true);
      clearTimeout(savedTimerRef.current);
      savedTimerRef.current = setTimeout(() => setSaved(false), 3000);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Не удалось создать школу';
      setError(msg);
    } finally {
      setSaving(false);
    }
  };

  const handleUpdate = async () => {
    if (!name.trim()) {
      setError('Введите название школы');
      return;
    }
    setSaving(true);
    setError('');
    try {
      const data: UpdateSchoolRequest = {
        name: name.trim(),
        description: description.trim() || undefined,
        logoUrl: logoUrl.trim() || undefined,
        websiteUrl: websiteUrl.trim() || undefined,
        instagramUrl: instagramUrl.trim() || undefined,
        telegramUrl: telegramUrl.trim() || undefined,
        specializations: specializations.length > 0 ? specializations.join(',') : undefined,
      };
      const updated = await tutorService.updateMySchool(data);
      setSchool(updated);
      populateForm(updated);
      setSaved(true);
      clearTimeout(savedTimerRef.current);
      savedTimerRef.current = setTimeout(() => setSaved(false), 3000);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Не удалось обновить школу';
      setError(msg);
    } finally {
      setSaving(false);
    }
  };

  const handleAddTutor = async () => {
    const id = Number(tutorIdInput.trim());
    if (!id || id <= 0) {
      setError('Введите корректный ID преподавателя');
      return;
    }
    setTutorAction('adding');
    setError('');
    try {
      await tutorService.addTutorToSchool(id);
      setTutorIdInput('');
      await loadSchool();
      setSaved(true);
      clearTimeout(savedTimerRef.current);
      savedTimerRef.current = setTimeout(() => setSaved(false), 3000);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Не удалось добавить преподавателя';
      setError(msg);
    } finally {
      setTutorAction('');
    }
  };

  const handleRemoveTutor = async () => {
    const id = Number(removeTutorIdInput.trim());
    if (!id || id <= 0) {
      setError('Введите корректный ID преподавателя');
      return;
    }
    if (!confirm('Удалить преподавателя из школы?')) return;
    setTutorAction('removing');
    setError('');
    try {
      await tutorService.removeTutorFromSchool(id);
      setRemoveTutorIdInput('');
      await loadSchool();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Не удалось удалить преподавателя';
      setError(msg);
    } finally {
      setTutorAction('');
    }
  };

  const toggleSpec = (spec: string) => {
    setSpecializations(prev =>
      prev.includes(spec) ? prev.filter(s => s !== spec) : [...prev, spec]
    );
    setSaved(false);
  };

  if (loading) {
    return <div className="animate-fade-in" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>Загрузка...</div>;
  }

  // ─── No school yet — creation form ───
  if (!school) {
    return (
      <div className="animate-fade-in">
        <h1 style={{ marginBottom: '1.5rem' }}>Создать школу</h1>

        {error && (
          <div style={{ background: '#fef2f2', color: '#dc2626', padding: '0.75rem 1rem', borderRadius: '0.5rem', marginBottom: '1rem', fontSize: '0.85rem' }}>
            {error}
          </div>
        )}

        <div className="card" style={{ maxWidth: '600px' }}>
          <SchoolForm
            name={name} setName={setName}
            description={description} setDescription={setDescription}
            logoUrl={logoUrl} setLogoUrl={setLogoUrl}
            websiteUrl={websiteUrl} setWebsiteUrl={setWebsiteUrl}
            instagramUrl={instagramUrl} setInstagramUrl={setInstagramUrl}
            telegramUrl={telegramUrl} setTelegramUrl={setTelegramUrl}
            specializations={specializations} toggleSpec={toggleSpec}
            onSaved={() => setSaved(false)}
          />
          <button className="btn btn-primary" onClick={handleCreate} disabled={saving} style={{ marginTop: '1rem', width: '100%' }}>
            {saving ? 'Создание...' : 'Создать школу'}
          </button>
        </div>
      </div>
    );
  }

  // ─── School exists — edit + manage ───
  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '0.5rem' }}>
        <h1 style={{ margin: 0 }}>Моя школа</h1>
        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
          {saved && <span style={{ color: '#10b981', fontSize: '0.85rem' }}>Сохранено</span>}
          <button className="btn btn-primary" onClick={handleUpdate} disabled={saving}>
            {saving ? 'Сохранение...' : 'Сохранить'}
          </button>
        </div>
      </div>

      {error && (
        <div style={{ background: '#fef2f2', color: '#dc2626', padding: '0.75rem 1rem', borderRadius: '0.5rem', marginBottom: '1rem', fontSize: '0.85rem' }}>
          {error}
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(350px, 1fr))', gap: '1rem' }}>
        {/* Left: Edit form */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <div className="card">
            <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Информация о школе</h3>
            <SchoolForm
              name={name} setName={setName}
              description={description} setDescription={setDescription}
              logoUrl={logoUrl} setLogoUrl={setLogoUrl}
              websiteUrl={websiteUrl} setWebsiteUrl={setWebsiteUrl}
              instagramUrl={instagramUrl} setInstagramUrl={setInstagramUrl}
              telegramUrl={telegramUrl} setTelegramUrl={setTelegramUrl}
              specializations={specializations} toggleSpec={toggleSpec}
              onSaved={() => setSaved(false)}
            />
          </div>
        </div>

        {/* Right: Info & Tutor Management */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {/* School info card */}
          <div className="card">
            <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Сведения</h3>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
              <div><strong>Slug:</strong> {school.slug}</div>
              <div><strong>Публичная ссылка:</strong>{' '}
                <a href={`/schools/${school.slug}`} target="_blank" rel="noopener noreferrer" style={{ color: 'var(--primary)' }}>
                  /schools/{school.slug}
                </a>
              </div>
              <div><strong>Преподавателей:</strong> {school.tutorCount}</div>
              <div><strong>Партнёр:</strong> {school.isPartner ? 'Да' : 'Нет'}</div>
              <div><strong>Создана:</strong> {new Date(school.createdAt).toLocaleDateString('ru-RU')}</div>
            </div>
          </div>

          {/* Tutor management card */}
          <div className="card">
            <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Управление преподавателями</h3>

            <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
              <input
                className="form-input"
                type="number"
                min={1}
                placeholder="ID преподавателя"
                value={tutorIdInput}
                onChange={e => setTutorIdInput(e.target.value)}
                style={{ flex: 1 }}
              />
              <button
                className="btn btn-primary"
                onClick={handleAddTutor}
                disabled={tutorAction === 'adding'}
                style={{ whiteSpace: 'nowrap' }}
              >
                {tutorAction === 'adding' ? '...' : 'Добавить'}
              </button>
            </div>

            <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', margin: '0 0 1rem' }}>
              Введите ID пользователя-преподавателя, чтобы добавить его в школу.
            </p>

            <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
              <input
                className="form-input"
                type="number"
                min={1}
                placeholder="ID для удаления"
                value={removeTutorIdInput}
                onChange={e => setRemoveTutorIdInput(e.target.value)}
                style={{ flex: 1 }}
              />
              <button
                className="btn btn-outline"
                onClick={handleRemoveTutor}
                disabled={tutorAction === 'removing'}
                style={{ whiteSpace: 'nowrap', color: '#dc2626', borderColor: '#dc2626' }}
              >
                {tutorAction === 'removing' ? '...' : 'Удалить'}
              </button>
            </div>

            {school.tutorCount > 0 ? (
              <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                Преподавателей в школе: <strong>{school.tutorCount}</strong>
              </div>
            ) : (
              <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                В школе пока нет преподавателей.
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

// ─── Shared Form Fields ─────────────────────────────────

interface SchoolFormProps {
  name: string; setName: (v: string) => void;
  description: string; setDescription: (v: string) => void;
  logoUrl: string; setLogoUrl: (v: string) => void;
  websiteUrl: string; setWebsiteUrl: (v: string) => void;
  instagramUrl: string; setInstagramUrl: (v: string) => void;
  telegramUrl: string; setTelegramUrl: (v: string) => void;
  specializations: string[]; toggleSpec: (s: string) => void;
  onSaved: () => void;
}

function SchoolForm({ name, setName, description, setDescription, logoUrl, setLogoUrl, websiteUrl, setWebsiteUrl, instagramUrl, setInstagramUrl, telegramUrl, setTelegramUrl, specializations, toggleSpec, onSaved }: SchoolFormProps) {
  return (
    <>
      <div className="form-group" style={{ marginBottom: '0.75rem' }}>
        <label className="form-label">Название *</label>
        <input className="form-input" value={name} onChange={e => { setName(e.target.value); onSaved(); }} placeholder="Название школы" maxLength={200} />
      </div>

      <div className="form-group" style={{ marginBottom: '0.75rem' }}>
        <label className="form-label">Описание</label>
        <textarea className="form-input" rows={3} value={description} onChange={e => { setDescription(e.target.value); onSaved(); }} placeholder="Краткое описание школы..." maxLength={2000} />
        <div style={{ fontSize: '0.72rem', color: 'var(--text-secondary)', textAlign: 'right' }}>{description.length}/2000</div>
      </div>

      <div className="form-group" style={{ marginBottom: '0.75rem' }}>
        <label className="form-label">Логотип (URL)</label>
        <input className="form-input" value={logoUrl} onChange={e => { setLogoUrl(e.target.value); onSaved(); }} placeholder="https://example.com/logo.png" />
      </div>

      <div className="form-group" style={{ marginBottom: '0.75rem' }}>
        <label className="form-label">Веб-сайт</label>
        <input className="form-input" value={websiteUrl} onChange={e => { setWebsiteUrl(e.target.value); onSaved(); }} placeholder="https://school.kz" />
      </div>

      <div className="form-group" style={{ marginBottom: '0.75rem' }}>
        <label className="form-label">Instagram</label>
        <input className="form-input" value={instagramUrl} onChange={e => { setInstagramUrl(e.target.value); onSaved(); }} placeholder="https://instagram.com/school" />
      </div>

      <div className="form-group" style={{ marginBottom: '0.75rem' }}>
        <label className="form-label">Telegram</label>
        <input className="form-input" value={telegramUrl} onChange={e => { setTelegramUrl(e.target.value); onSaved(); }} placeholder="https://t.me/school" />
      </div>

      <div className="form-group" style={{ marginBottom: '0' }}>
        <label className="form-label">Специализации</label>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem' }}>
          {EXAM_OPTIONS.map(spec => (
            <button
              key={spec}
              type="button"
              className={`btn ${specializations.includes(spec) ? 'btn-primary' : 'btn-outline'}`}
              onClick={() => toggleSpec(spec)}
              style={{ padding: '0.35rem 0.75rem', fontSize: '0.82rem' }}
            >
              {spec}
            </button>
          ))}
        </div>
      </div>
    </>
  );
}

export default TutorSchoolManagePage;
