import { useState, useEffect, useCallback, useRef } from 'react';
import { tutorService } from '../services/tutorService';
import { useTranslation } from '../i18n';
import api from '../services/api';
import { useAppSelector } from '../hooks/useAppSelector';
import type { SchoolAdmin, UpdateSchoolRequest, TutorSchoolCard } from '../types';

const EXAM_OPTIONS = ['SAT', 'NUET'];

function TutorSchoolManagePage() {
  const { user } = useAppSelector((state) => state.auth);
  const canBrand = user?.role === 'SchoolAdmin' || user?.role === 'Admin';
  const [school, setSchool] = useState<SchoolAdmin | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const [error, setError] = useState('');

  // Form fields
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [descriptionEn, setDescriptionEn] = useState('');
  const [descriptionKz, setDescriptionKz] = useState('');
  const [logoUrl, setLogoUrl] = useState('');
  const [websiteUrl, setWebsiteUrl] = useState('');
  const [instagramUrl, setInstagramUrl] = useState('');
  const [telegramUrl, setTelegramUrl] = useState('');
  const [specializations, setSpecializations] = useState<string[]>([]);

  // White-label branding
  const [subdomain, setSubdomain] = useState('');
  const [primaryColor, setPrimaryColor] = useState('');
  const [primaryHoverColor, setPrimaryHoverColor] = useState('');
  const [accentColor, setAccentColor] = useState('');
  const [navbarTitle, setNavbarTitle] = useState('');

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
    setDescriptionEn(s.descriptionEn || '');
    setDescriptionKz(s.descriptionKz || '');
    setLogoUrl(s.logoUrl || '');
    setWebsiteUrl(s.websiteUrl || '');
    setInstagramUrl(s.instagramUrl || '');
    setTelegramUrl(s.telegramUrl || '');
    setSpecializations(s.specializations || []);
    setSubdomain(s.subdomain || '');
    setPrimaryColor(s.primaryColor || '');
    setPrimaryHoverColor(s.primaryHoverColor || '');
    setAccentColor(s.accentColor || '');
    setNavbarTitle(s.navbarTitle || '');
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
        descriptionEn: descriptionEn.trim() || undefined,
        descriptionKz: descriptionKz.trim() || undefined,
        logoUrl: logoUrl.trim() || undefined,
        websiteUrl: websiteUrl.trim() || undefined,
        instagramUrl: instagramUrl.trim() || undefined,
        telegramUrl: telegramUrl.trim() || undefined,
        specializations: specializations.length > 0 ? specializations.join(',') : undefined,
        // White-label branding is restricted to SchoolAdmin / platform Admin.
        ...(canBrand ? {
          subdomain: subdomain.trim().toLowerCase(),
          primaryColor: primaryColor.trim(),
          primaryHoverColor: primaryHoverColor.trim(),
          accentColor: accentColor.trim(),
          navbarTitle: navbarTitle.trim(),
        } : {}),
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

  // ─── No school yet — browse schools ───
  if (!school) {
    return <SchoolBrowser />;
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
              descriptionEn={descriptionEn} setDescriptionEn={setDescriptionEn}
              descriptionKz={descriptionKz} setDescriptionKz={setDescriptionKz}
              logoUrl={logoUrl} setLogoUrl={setLogoUrl}
              websiteUrl={websiteUrl} setWebsiteUrl={setWebsiteUrl}
              instagramUrl={instagramUrl} setInstagramUrl={setInstagramUrl}
              telegramUrl={telegramUrl} setTelegramUrl={setTelegramUrl}
              specializations={specializations} toggleSpec={toggleSpec}
              onSaved={() => setSaved(false)}
            />
          </div>

          {/* White-label branding */}
          {canBrand && (
          <div className="card">
            <h3 style={{ margin: '0 0 0.25rem', fontSize: '1rem' }}>Брендинг (White Label)</h3>
            <p style={{ margin: '0 0 0.75rem', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
              Поддомен и фирменные цвета для страницы вашей школы (например <code>school.unistart.kz</code>). Цвета задаются в формате HEX, например <code>#c0392b</code>.
            </p>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>
                Поддомен
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.25rem', marginTop: '0.25rem' }}>
                  <input
                    className="form-input"
                    type="text"
                    placeholder="school"
                    value={subdomain}
                    onChange={e => { setSubdomain(e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, '')); setSaved(false); }}
                    style={{ flex: 1 }}
                  />
                  <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', whiteSpace: 'nowrap' }}>.unistart.kz</span>
                </div>
              </label>
              <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>
                Название в шапке
                <input
                  className="form-input"
                  type="text"
                  placeholder={name}
                  value={navbarTitle}
                  onChange={e => { setNavbarTitle(e.target.value); setSaved(false); }}
                  style={{ marginTop: '0.25rem' }}
                />
              </label>
              <ColorField label="Основной цвет" value={primaryColor} onChange={v => { setPrimaryColor(v); setSaved(false); }} placeholder="#c0392b" />
              <ColorField label="Цвет при наведении" value={primaryHoverColor} onChange={v => { setPrimaryHoverColor(v); setSaved(false); }} placeholder="#e74c3c" />
              <ColorField label="Акцентный цвет" value={accentColor} onChange={v => { setAccentColor(v); setSaved(false); }} placeholder="#d4a437" />
            </div>
          </div>
          )}
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
  descriptionEn: string; setDescriptionEn: (v: string) => void;
  descriptionKz: string; setDescriptionKz: (v: string) => void;
  logoUrl: string; setLogoUrl: (v: string) => void;
  websiteUrl: string; setWebsiteUrl: (v: string) => void;
  instagramUrl: string; setInstagramUrl: (v: string) => void;
  telegramUrl: string; setTelegramUrl: (v: string) => void;
  specializations: string[]; toggleSpec: (s: string) => void;
  onSaved: () => void;
}

function ColorField({ label, value, onChange, placeholder }: { label: string; value: string; onChange: (v: string) => void; placeholder: string }) {
  const valid = /^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$/.test(value.trim());
  return (
    <label style={{ fontSize: '0.85rem', fontWeight: 600 }}>
      {label}
      <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginTop: '0.25rem' }}>
        <input
          type="color"
          value={valid ? value.trim() : '#6366f1'}
          onChange={e => onChange(e.target.value)}
          style={{ width: '2.5rem', height: '2.25rem', padding: 0, border: '1px solid var(--border-color)', borderRadius: '0.4rem', cursor: 'pointer', background: 'none' }}
          aria-label={label}
        />
        <input
          className="form-input"
          type="text"
          placeholder={placeholder}
          value={value}
          onChange={e => onChange(e.target.value)}
          style={{ flex: 1 }}
        />
      </div>
    </label>
  );
}

function SchoolForm({ name, setName, description, setDescription, descriptionEn, setDescriptionEn, descriptionKz, setDescriptionKz, logoUrl, setLogoUrl, websiteUrl, setWebsiteUrl, instagramUrl, setInstagramUrl, telegramUrl, setTelegramUrl, specializations, toggleSpec, onSaved }: SchoolFormProps) {
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
        <label className="form-label">Description (English)</label>
        <textarea className="form-input" rows={2} value={descriptionEn} onChange={e => { setDescriptionEn(e.target.value); onSaved(); }} placeholder="School description in English..." maxLength={2000} />
      </div>

      <div className="form-group" style={{ marginBottom: '0.75rem' }}>
        <label className="form-label">Сипаттама (Қазақша)</label>
        <textarea className="form-input" rows={2} value={descriptionKz} onChange={e => { setDescriptionKz(e.target.value); onSaved(); }} placeholder="Мектептің қысқаша сипаттамасы..." maxLength={2000} />
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

// ─── School Browser for independent tutors ───
function SchoolBrowser() {
  const { t } = useTranslation();
  const [schools, setSchools] = useState<TutorSchoolCard[]>([]);
  const [apps, setApps] = useState<{ id: number; schoolId: number; schoolName: string; status: string }[]>([]);
  const [applyingTo, setApplyingTo] = useState<number | null>(null);
  const [applyMsg, setApplyMsg] = useState('');
  const [applyError, setApplyError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      tutorService.getSchools(),
      api.get('/tutor-school-applications/my').then(r => r.data),
    ]).then(([s, a]) => {
      setSchools(s);
      setApps(a);
    }).catch(() => {}).finally(() => setLoading(false));
  }, []);

  const handleApply = async (schoolId: number) => {
    setApplyError(null);
    try {
      await api.post('/tutor-school-applications', { schoolId, message: applyMsg || undefined });
      setApplyingTo(null);
      setApplyMsg('');
      const r = await api.get('/tutor-school-applications/my');
      setApps(r.data);
    } catch (e: any) {
      setApplyError(e.response?.data?.error || 'Error');
    }
  };

  const appliedSchoolIds = new Set(apps.map(a => a.schoolId));
  const availableSchools = schools.filter(s => !appliedSchoolIds.has(s.id));

  if (loading) {
    return <div className="animate-fade-in" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>...</div>;
  }

  return (
    <div className="animate-fade-in" style={{ maxWidth: '700px', margin: '0 auto' }}>
      <h1 style={{ marginBottom: '0.5rem' }}>{t.tutorGate.availableSchools}</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
        {t.tutorGate.description}
      </p>

      {/* My applications */}
      {apps.length > 0 && (
        <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
          <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>{t.tutorGate.myApps}</h3>
          {apps.map(a => (
            <div key={a.id} style={{
              display: 'flex', justifyContent: 'space-between', alignItems: 'center',
              padding: '0.6rem 0.75rem', borderRadius: '8px', background: 'var(--bg-secondary)', marginBottom: '0.4rem',
            }}>
              <span style={{ fontWeight: 500 }}>{a.schoolName}</span>
              <span style={{
                fontSize: '0.75rem', fontWeight: 600, padding: '0.15rem 0.5rem', borderRadius: '999px',
                background: a.status === 'Pending' ? '#f59e0b22' : a.status === 'Approved' ? '#10b98122' : '#ef444422',
                color: a.status === 'Pending' ? '#f59e0b' : a.status === 'Approved' ? '#10b981' : '#ef4444',
              }}>
                {a.status === 'Pending' ? t.tutorGate.statusPending : a.status === 'Approved' ? t.tutorGate.statusApproved : t.tutorGate.statusRejected}
              </span>
            </div>
          ))}
        </div>
      )}

      {/* Available schools */}
      {availableSchools.length > 0 && (
        <div className="card" style={{ padding: '1.5rem' }}>
          {availableSchools.map(s => (
            <div key={s.id} style={{
              display: 'flex', justifyContent: 'space-between', alignItems: 'center',
              padding: '0.6rem 0.75rem', borderRadius: '8px', background: 'var(--bg-secondary)', marginBottom: '0.4rem',
            }}>
              <div style={{ flex: 1 }}>
                <span style={{ fontWeight: 500 }}>{s.name}</span>
                {s.description && <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '0.15rem' }}>{s.description.slice(0, 100)}{s.description.length > 100 ? '...' : ''}</div>}
              </div>
              {applyingTo === s.id ? (
                <div style={{ display: 'flex', gap: '0.3rem', alignItems: 'center', flexShrink: 0 }}>
                  <input
                    value={applyMsg}
                    onChange={e => setApplyMsg(e.target.value)}
                    placeholder={t.tutorGate.messagePlaceholder}
                    style={{ fontSize: '0.8rem', padding: '0.3rem 0.5rem', borderRadius: '6px', border: '1px solid var(--border)', width: '140px' }}
                  />
                  <button className="btn btn-primary" style={{ fontSize: '0.75rem', padding: '0.3rem 0.6rem' }} onClick={() => handleApply(s.id)}>
                    {t.tutorGate.send}
                  </button>
                  <button className="btn" style={{ fontSize: '0.75rem', padding: '0.3rem 0.6rem' }} onClick={() => { setApplyingTo(null); setApplyMsg(''); }}>
                    &times;
                  </button>
                </div>
              ) : (
                <button className="btn btn-primary" style={{ fontSize: '0.75rem', padding: '0.3rem 0.8rem', flexShrink: 0 }} onClick={() => setApplyingTo(s.id)}>
                  {t.tutorGate.applyBtn}
                </button>
              )}
            </div>
          ))}
          {applyError && <p style={{ color: '#ef4444', fontSize: '0.8rem', marginTop: '0.5rem' }}>{applyError}</p>}
        </div>
      )}

      {availableSchools.length === 0 && apps.length === 0 && (
        <div className="card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
          Нет доступных школ
        </div>
      )}
    </div>
  );
}

export default TutorSchoolManagePage;
