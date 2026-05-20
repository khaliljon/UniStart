import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from '../i18n';
import {
  AdvisorConfig,
  AdvisorExam,
  AdvisorUniversity,
  loadAdvisorConfig,
  saveAdvisorConfig,
} from '../advisorConfig';

// ── Predefined lists ───────────────────────────────────────────────────────────

const WORLD_COUNTRIES = [
  'Japan', 'Kazakhstan', 'USA', 'Canada', 'UK', 'Germany', 'France', 'Australia',
  'South Korea', 'China', 'Singapore', 'Hong Kong', 'UAE', 'Qatar', 'Netherlands',
  'Switzerland', 'Sweden', 'Russia', 'Italy', 'Spain', 'Turkey', 'India', 'Malaysia',
  'New Zealand', 'Austria', 'Belgium', 'Czech Republic', 'Denmark', 'Finland',
  'Poland', 'Portugal', 'Norway', 'Ireland', 'Hungary', 'Estonia', 'Lithuania',
  'Romania', 'Bulgaria', 'Greece', 'Taiwan', 'Thailand', 'Vietnam', 'Brazil',
  'Argentina', 'Mexico', 'Europe', 'CIS', 'Asia',
];

const WORLD_LANGUAGES = [
  'English', 'Japanese', 'Russian', 'Korean', 'Chinese', 'French', 'German',
  'Spanish', 'Arabic', 'Dutch', 'Portuguese', 'Italian', 'Turkish',
  'English / Japanese', 'English / Russian', 'Other',
];

const MASTER_SPECIALTIES = [
  'International Relations', 'Global Studies', 'Journalism', 'Economics',
  'Management', 'Marketing', 'Accounting', 'Entrepreneurship', 'Business', 'Law',
  'Math', 'Physics', 'Chemistry', 'Biology', 'Computer Science',
  'Information and Communication', 'Computing', 'Engineering', 'Sciences',
  'Mechanical Engineering', 'Electrical Engineering', 'Civil Engineering',
  'Environmental Design', 'Applied Biological Sciences', 'Environmental Sciences',
  'Bioresource Sciences', 'Agriculture', 'Medicine', 'Pharmacy', 'Nursing',
  'Dentistry', 'Tourism', 'Sustainability', 'English Language Teaching',
  'Education', 'Culture', 'Media', 'Japan Studies', 'Arts', 'Humanities',
  'Linguistics', 'Philosophy', 'History', 'Literature', 'Sociology',
  'Anthropology', 'Psychology',
];

const LEVELS = ['Несложно', 'Средне', 'Сложно', 'Очень сложно'];

// ── Helpers ────────────────────────────────────────────────────────────────────

function createEmptyUniversity(): AdvisorUniversity {
  return {
    id: `advisor-${Date.now()}`,
    name: '',
    city: '',
    country: '',
    exam: 'SAT',
    minScore: null,
    level: '',
    specialties: [],
    language: '',
    grants: '',
    plus: '',
    minus: '',
    acceptance: '',
    deadlines: '',
    ranking: '',
    procedure: '',
    videoUrl: '',
    photoUrl: '',
    description: '',
  };
}

// ── Tag button component ───────────────────────────────────────────────────────

function TagBtn({
  label,
  active,
  onClick,
}: {
  label: string;
  active: boolean;
  onClick: () => void;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      style={{
        padding: '0.28rem 0.75rem',
        borderRadius: '999px',
        border: `1px solid ${active ? 'var(--accent-color)' : 'var(--border-color)'}`,
        background: active ? 'var(--accent-color)' : 'transparent',
        color: active ? '#fff' : 'var(--text-secondary)',
        cursor: 'pointer',
        fontSize: '0.82rem',
        transition: 'all 0.15s',
        whiteSpace: 'nowrap',
      }}
    >
      {label}
    </button>
  );
}

// ── Main page ─────────────────────────────────────────────────────────────────

function AdminAdvisorConfigPage() {
  const { t } = useTranslation();

  const [config, setConfig] = useState<AdvisorConfig>(loadAdvisorConfig());
  const [selectedUniId, setSelectedUniId] = useState<string | null>(
    config.universities[0]?.id ?? null,
  );
  const [editUni, setEditUni] = useState<AdvisorUniversity | null>(
    config.universities[0] ? { ...config.universities[0] } : null,
  );
  const [customCountry, setCustomCountry] = useState('');
  const [newCatKey, setNewCatKey] = useState('');
  const [newCatLabel, setNewCatLabel] = useState('');
  const [message, setMessage] = useState<string | null>(null);
  const [uniSearch, setUniSearch] = useState('');

  const notify = (msg: string) => {
    setMessage(msg);
    window.setTimeout(() => setMessage(null), 2500);
  };

  // ── Countries ─────────────────────────────────────────────────────────────

  const toggleCountry = (country: string) => {
    setConfig((prev) => ({
      ...prev,
      countries: prev.countries.includes(country)
        ? prev.countries.filter((c) => c !== country)
        : [...prev.countries, country],
    }));
  };

  const addCustomCountry = () => {
    const trimmed = customCountry.trim();
    if (!trimmed || config.countries.includes(trimmed)) return;
    setConfig((prev) => ({ ...prev, countries: [...prev.countries, trimmed] }));
    setCustomCountry('');
  };

  // ── Languages ────────────────────────────────────────────────────────────

  const toggleLanguage = (lang: string) => {
    setConfig((prev) => ({
      ...prev,
      languages: prev.languages.includes(lang)
        ? prev.languages.filter((l) => l !== lang)
        : [...prev.languages, lang],
    }));
  };

  // ── Categories ───────────────────────────────────────────────────────────

  const toggleCategorySpecialty = (catIdx: number, sp: string) => {
    setConfig((prev) => {
      const cats = [...prev.categories];
      const cat = { ...cats[catIdx] };
      cat.specialties = cat.specialties.includes(sp)
        ? cat.specialties.filter((s) => s !== sp)
        : [...cat.specialties, sp];
      cats[catIdx] = cat;
      return { ...prev, categories: cats };
    });
  };

  const updateCategoryField = (
    catIdx: number,
    field: 'key' | 'label',
    value: string,
  ) => {
    setConfig((prev) => {
      const cats = [...prev.categories];
      cats[catIdx] = { ...cats[catIdx], [field]: value };
      return { ...prev, categories: cats };
    });
  };

  const deleteCategory = (catIdx: number) => {
    setConfig((prev) => ({
      ...prev,
      categories: prev.categories.filter((_, i) => i !== catIdx),
    }));
  };

  const addCategory = () => {
    if (!newCatKey.trim() || !newCatLabel.trim()) {
      notify('Заполните ключ и название');
      return;
    }
    if (config.categories.some((c) => c.key === newCatKey.trim())) {
      notify('Ключ уже используется');
      return;
    }
    setConfig((prev) => ({
      ...prev,
      categories: [
        ...prev.categories,
        { key: newCatKey.trim(), label: newCatLabel.trim(), specialties: [] },
      ],
    }));
    setNewCatKey('');
    setNewCatLabel('');
  };

  // ── Universities ─────────────────────────────────────────────────────────

  const selectUni = (id: string) => {
    const uni = config.universities.find((u) => u.id === id);
    if (uni) {
      setSelectedUniId(id);
      setEditUni({ ...uni });
    }
  };

  const addUni = () => {
    const newUni = createEmptyUniversity();
    setConfig((prev) => ({
      ...prev,
      universities: [...prev.universities, newUni],
    }));
    setSelectedUniId(newUni.id);
    setEditUni(newUni);
  };

  const deleteUni = (id: string) => {
    setConfig((prev) => {
      const unis = prev.universities.filter((u) => u.id !== id);
      return { ...prev, universities: unis };
    });
    const remaining = config.universities.filter((u) => u.id !== id);
    setSelectedUniId(remaining[0]?.id ?? null);
    setEditUni(remaining[0] ? { ...remaining[0] } : null);
  };

  const updateEditUni = (patch: Partial<AdvisorUniversity>) => {
    setEditUni((prev) => (prev ? { ...prev, ...patch } : null));
  };

  const toggleUniSpecialty = (sp: string) => {
    if (!editUni) return;
    const specialties = editUni.specialties.includes(sp)
      ? editUni.specialties.filter((s) => s !== sp)
      : [...editUni.specialties, sp];
    updateEditUni({ specialties });
  };

  const saveEditUni = () => {
    if (!editUni) return;
    setConfig((prev) => ({
      ...prev,
      universities: prev.universities.map((u) =>
        u.id === editUni.id ? editUni : u,
      ),
    }));
    notify('Вуз обновлён');
  };

  // sync editUni when config.universities changes externally
  useEffect(() => {
    if (!selectedUniId) return;
    const uni = config.universities.find((u) => u.id === selectedUniId);
    if (!uni) return;
    // only sync if editUni is not "dirty" (same id but already in editUni state)
  }, [config.universities, selectedUniId]);

  const filteredUnis = useMemo(
    () =>
      config.universities.filter(
        (u) =>
          !uniSearch ||
          u.name.toLowerCase().includes(uniSearch.toLowerCase()) ||
          u.country.toLowerCase().includes(uniSearch.toLowerCase()) ||
          u.city.toLowerCase().includes(uniSearch.toLowerCase()),
      ),
    [config.universities, uniSearch],
  );

  // ── Save / reset ─────────────────────────────────────────────────────────

  const saveConfig = () => {
    const finalConfig =
      editUni
        ? {
            ...config,
            universities: config.universities.map((u) =>
              u.id === editUni.id ? editUni : u,
            ),
          }
        : config;
    saveAdvisorConfig(finalConfig);
    notify('Конфигурация сохранена');
  };

  const reloadConfig = () => {
    const loaded = loadAdvisorConfig();
    setConfig(loaded);
    const first = loaded.universities[0];
    setSelectedUniId(first?.id ?? null);
    setEditUni(first ? { ...first } : null);
    notify('Загружено из кэша');
  };

  // ── Styles ───────────────────────────────────────────────────────────────

  const cardStyle: React.CSSProperties = {
    background: 'var(--card-background)',
    border: '1px solid var(--border-color)',
    borderRadius: '12px',
    padding: '1.25rem',
    marginBottom: '1.5rem',
  };

  const sectionTitle: React.CSSProperties = {
    margin: '0 0 1rem',
    fontSize: '1.1rem',
    fontWeight: 700,
  };

  // ── Render ────────────────────────────────────────────────────────────────

  return (
    <div style={{ maxWidth: 1240, margin: '0 auto' }}>
      {/* Header */}
      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          marginBottom: '1.5rem',
          flexWrap: 'wrap',
          gap: '0.75rem',
        }}
      >
        <h1 style={{ margin: 0 }}>Конфигурация советника</h1>
        <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center' }}>
          {message && (
            <span style={{ color: 'var(--success-color)', fontSize: '0.9rem' }}>
              {message}
            </span>
          )}
          <button className="btn" onClick={reloadConfig}>
            {t.common.reset}
          </button>
          <button className="btn btn-primary" onClick={saveConfig}>
            {t.common.save}
          </button>
        </div>
      </div>

      {/* ── Countries ──────────────────────────────────────────────────────── */}
      <div style={cardStyle}>
        <h2 style={sectionTitle}>Страны ({config.countries.length} выбрано)</h2>
        <p style={{ margin: '0 0 0.75rem', color: 'var(--text-secondary)', fontSize: '0.88rem' }}>
          Нажмите на страну чтобы добавить или убрать её из списка фильтрации.
        </p>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem', marginBottom: '0.75rem' }}>
          {WORLD_COUNTRIES.map((country) => (
            <TagBtn
              key={country}
              label={country}
              active={config.countries.includes(country)}
              onClick={() => toggleCountry(country)}
            />
          ))}
        </div>
        {/* Custom countries not in predefined list */}
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem', marginTop: '0.4rem' }}>
          {config.countries
            .filter((c) => !WORLD_COUNTRIES.includes(c))
            .map((c) => (
              <TagBtn
                key={c}
                label={c}
                active={true}
                onClick={() => toggleCountry(c)}
              />
            ))}
        </div>
        <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem' }}>
          <input
            className="form-input"
            style={{ maxWidth: 240 }}
            value={customCountry}
            onChange={(e) => setCustomCountry(e.target.value)}
            placeholder="Добавить другую страну..."
            onKeyDown={(e) => e.key === 'Enter' && addCustomCountry()}
          />
          <button className="btn" type="button" onClick={addCustomCountry}>
            Добавить
          </button>
        </div>
      </div>

      {/* ── Languages ──────────────────────────────────────────────────────── */}
      <div style={cardStyle}>
        <h2 style={sectionTitle}>Языки обучения ({config.languages.length} выбрано)</h2>
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem' }}>
          {WORLD_LANGUAGES.map((lang) => (
            <TagBtn
              key={lang}
              label={lang}
              active={config.languages.includes(lang)}
              onClick={() => toggleLanguage(lang)}
            />
          ))}
        </div>
      </div>

      {/* ── Categories ─────────────────────────────────────────────────────── */}
      <div style={cardStyle}>
        <h2 style={sectionTitle}>Категории специальностей</h2>

        {config.categories.map((cat, idx) => (
          <div
            key={cat.key}
            style={{
              marginBottom: '1rem',
              padding: '1rem',
              border: '1px solid var(--border-color)',
              borderRadius: '8px',
            }}
          >
            <div
              style={{
                display: 'flex',
                gap: '0.5rem',
                alignItems: 'center',
                marginBottom: '0.75rem',
                flexWrap: 'wrap',
              }}
            >
              <input
                className="form-input"
                style={{ maxWidth: 160 }}
                value={cat.key}
                onChange={(e) => updateCategoryField(idx, 'key', e.target.value)}
                placeholder="Ключ"
              />
              <input
                className="form-input"
                style={{ flex: '1 1 180px' }}
                value={cat.label}
                onChange={(e) => updateCategoryField(idx, 'label', e.target.value)}
                placeholder="Название"
              />
              <button
                className="btn"
                type="button"
                style={{ color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                onClick={() => {
                  if (window.confirm(`Удалить категорию «${cat.label}»?`)) {
                    deleteCategory(idx);
                  }
                }}
              >
                Удалить
              </button>
            </div>
            <p style={{ margin: '0 0 0.5rem', fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
              Специальности ({cat.specialties.length} выбрано):
            </p>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem' }}>
              {MASTER_SPECIALTIES.map((sp) => {
                const active = cat.specialties.includes(sp);
                return (
                  <label
                    key={sp}
                    style={{
                      display: 'inline-flex',
                      alignItems: 'center',
                      gap: '0.3rem',
                      cursor: 'pointer',
                      fontSize: '0.8rem',
                      padding: '0.2rem 0.4rem',
                      borderRadius: '4px',
                      background: active ? 'rgba(99,102,241,0.12)' : 'transparent',
                      transition: 'background 0.15s',
                    }}
                  >
                    <input
                      type="checkbox"
                      checked={active}
                      onChange={() => toggleCategorySpecialty(idx, sp)}
                    />
                    {sp}
                  </label>
                );
              })}
            </div>
          </div>
        ))}

        {/* Add category */}
        <div
          style={{
            padding: '0.75rem',
            border: '1px dashed var(--border-color)',
            borderRadius: '8px',
          }}
        >
          <p style={{ margin: '0 0 0.5rem', fontWeight: 600, fontSize: '0.9rem' }}>
            Добавить категорию
          </p>
          <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
            <input
              className="form-input"
              style={{ maxWidth: 160 }}
              value={newCatKey}
              onChange={(e) => setNewCatKey(e.target.value)}
              placeholder="Ключ (уникальный)"
            />
            <input
              className="form-input"
              style={{ flex: '1 1 180px' }}
              value={newCatLabel}
              onChange={(e) => setNewCatLabel(e.target.value)}
              placeholder="Название"
            />
            <button className="btn btn-primary" type="button" onClick={addCategory}>
              Добавить
            </button>
          </div>
        </div>
      </div>

      {/* ── Universities ───────────────────────────────────────────────────── */}
      <div style={cardStyle}>
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            marginBottom: '1rem',
            flexWrap: 'wrap',
            gap: '0.5rem',
          }}
        >
          <h2 style={{ margin: 0 }}>
            Университеты ({config.universities.length})
          </h2>
          <button className="btn btn-primary" type="button" onClick={addUni}>
            + Добавить вуз
          </button>
        </div>

        <div
          style={{
            display: 'grid',
            gridTemplateColumns: '260px 1fr',
            gap: '1rem',
            alignItems: 'start',
          }}
        >
          {/* Left list */}
          <div>
            <input
              className="form-input"
              value={uniSearch}
              onChange={(e) => setUniSearch(e.target.value)}
              placeholder="Поиск..."
              style={{ marginBottom: '0.5rem' }}
            />
            <div style={{ maxHeight: 560, overflowY: 'auto' }}>
              {filteredUnis.map((uni) => (
                <div
                  key={uni.id}
                  onClick={() => selectUni(uni.id)}
                  style={{
                    padding: '0.55rem 0.75rem',
                    borderRadius: '6px',
                    cursor: 'pointer',
                    background:
                      selectedUniId === uni.id
                        ? 'rgba(99,102,241,0.15)'
                        : 'transparent',
                    border: `1px solid ${selectedUniId === uni.id ? 'var(--accent-color)' : 'var(--border-color)'}`,
                    marginBottom: '0.4rem',
                    transition: 'background 0.1s',
                  }}
                >
                  <div style={{ fontWeight: 600, fontSize: '0.88rem' }}>
                    {uni.name || '(без названия)'}
                  </div>
                  <div style={{ fontSize: '0.76rem', color: 'var(--text-secondary)' }}>
                    {uni.city ? `${uni.city}, ` : ''}
                    {uni.country} · {uni.exam}
                  </div>
                </div>
              ))}
              {filteredUnis.length === 0 && (
                <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                  Нет результатов
                </p>
              )}
            </div>
          </div>

          {/* Right editor */}
          {editUni ? (
            <div>
              {/* Row 1: Name / City */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem', marginBottom: '0.75rem' }}>
                <div className="form-group">
                  <label>Название</label>
                  <input
                    className="form-input"
                    value={editUni.name}
                    onChange={(e) => updateEditUni({ name: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>Город</label>
                  <input
                    className="form-input"
                    value={editUni.city}
                    onChange={(e) => updateEditUni({ city: e.target.value })}
                  />
                </div>
              </div>

              {/* Row 2: Country / Exam */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem', marginBottom: '0.75rem' }}>
                <div className="form-group">
                  <label>Страна</label>
                  <select
                    className="form-input"
                    value={editUni.country}
                    onChange={(e) => updateEditUni({ country: e.target.value })}
                  >
                    <option value="">Выберите страну</option>
                    {config.countries.map((c) => (
                      <option key={c} value={c}>
                        {c}
                      </option>
                    ))}
                    {editUni.country && !config.countries.includes(editUni.country) && (
                      <option value={editUni.country}>{editUni.country} (не в списке)</option>
                    )}
                  </select>
                </div>
                <div className="form-group">
                  <label>Экзамен</label>
                  <select
                    className="form-input"
                    value={editUni.exam}
                    onChange={(e) =>
                      updateEditUni({ exam: e.target.value as AdvisorExam })
                    }
                  >
                    <option value="SAT">SAT</option>
                    <option value="NUET">NUET</option>
                  </select>
                </div>
              </div>

              {/* Row 3: Min score / Level */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem', marginBottom: '0.75rem' }}>
                <div className="form-group">
                  <label>Мин. балл (пусто = не указан)</label>
                  <input
                    className="form-input"
                    type="number"
                    value={editUni.minScore ?? ''}
                    onChange={(e) =>
                      updateEditUni({
                        minScore: e.target.value ? Number(e.target.value) : null,
                      })
                    }
                  />
                </div>
                <div className="form-group">
                  <label>Сложность поступления</label>
                  <select
                    className="form-input"
                    value={editUni.level}
                    onChange={(e) => updateEditUni({ level: e.target.value })}
                  >
                    <option value="">Не указана</option>
                    {LEVELS.map((l) => (
                      <option key={l} value={l}>
                        {l}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              {/* Row 4: Language / Lat+Lng */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '0.75rem', marginBottom: '0.75rem' }}>
                <div className="form-group">
                  <label>Язык обучения</label>
                  <select
                    className="form-input"
                    value={editUni.language}
                    onChange={(e) => updateEditUni({ language: e.target.value })}
                  >
                    <option value="">Выберите язык</option>
                    {config.languages.map((l) => (
                      <option key={l} value={l}>
                        {l}
                      </option>
                    ))}
                    {editUni.language &&
                      !config.languages.includes(editUni.language) && (
                        <option value={editUni.language}>
                          {editUni.language} (не в списке)
                        </option>
                      )}
                  </select>
                </div>
                <div className="form-group">
                  <label>Широта (lat)</label>
                  <input
                    className="form-input"
                    type="number"
                    step="0.0001"
                    value={editUni.lat ?? ''}
                    onChange={(e) =>
                      updateEditUni({
                        lat: e.target.value ? Number(e.target.value) : undefined,
                      })
                    }
                  />
                </div>
                <div className="form-group">
                  <label>Долгота (lng)</label>
                  <input
                    className="form-input"
                    type="number"
                    step="0.0001"
                    value={editUni.lng ?? ''}
                    onChange={(e) =>
                      updateEditUni({
                        lng: e.target.value ? Number(e.target.value) : undefined,
                      })
                    }
                  />
                </div>
              </div>

              {/* Map preview */}
              {editUni.lat != null && editUni.lng != null && (
                <div style={{ marginBottom: '0.75rem' }}>
                  <p style={{ margin: '0 0 0.4rem', fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
                    Предпросмотр на карте
                  </p>
                  <iframe
                    key={`${editUni.lat},${editUni.lng}`}
                    title="map-preview"
                    src={`https://www.openstreetmap.org/export/embed.html?bbox=${editUni.lng - 0.5}%2C${editUni.lat - 0.3}%2C${editUni.lng + 0.5}%2C${editUni.lat + 0.3}&layer=mapnik&marker=${editUni.lat}%2C${editUni.lng}`}
                    style={{ width: '100%', height: 240, border: '1px solid var(--border-color)', borderRadius: '8px' }}
                    loading="lazy"
                  />
                </div>
              )}

              {/* Specialties */}
              <div className="form-group" style={{ marginBottom: '0.75rem' }}>
                <label>
                  Специальности ({editUni.specialties.length} выбрано)
                </label>
                <div
                  style={{
                    border: '1px solid var(--border-color)',
                    borderRadius: '8px',
                    padding: '0.75rem',
                    maxHeight: 240,
                    overflowY: 'auto',
                  }}
                >
                  {config.categories.map((cat) => (
                    <div key={cat.key} style={{ marginBottom: '0.75rem' }}>
                      <p
                        style={{
                          margin: '0 0 0.4rem',
                          fontWeight: 600,
                          fontSize: '0.8rem',
                          color: 'var(--text-secondary)',
                          textTransform: 'uppercase',
                          letterSpacing: '0.05em',
                        }}
                      >
                        {cat.label}
                      </p>
                      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem' }}>
                        {cat.specialties.map((sp) => (
                          <label
                            key={sp}
                            style={{
                              display: 'inline-flex',
                              alignItems: 'center',
                              gap: '0.3rem',
                              cursor: 'pointer',
                              fontSize: '0.8rem',
                              padding: '0.2rem 0.4rem',
                              borderRadius: '4px',
                              background: editUni.specialties.includes(sp)
                                ? 'rgba(99,102,241,0.12)'
                                : 'transparent',
                            }}
                          >
                            <input
                              type="checkbox"
                              checked={editUni.specialties.includes(sp)}
                              onChange={() => toggleUniSpecialty(sp)}
                            />
                            {sp}
                          </label>
                        ))}
                        {cat.specialties.length === 0 && (
                          <span style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                            Специальностей нет
                          </span>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Text fields */}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem', marginBottom: '0.75rem' }}>
                <div className="form-group">
                  <label>Гранты</label>
                  <input
                    className="form-input"
                    value={editUni.grants}
                    onChange={(e) => updateEditUni({ grants: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>Рейтинг</label>
                  <input
                    className="form-input"
                    value={editUni.ranking ?? ''}
                    onChange={(e) => updateEditUni({ ranking: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>Плюсы</label>
                  <input
                    className="form-input"
                    value={editUni.plus}
                    onChange={(e) => updateEditUni({ plus: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>Минусы</label>
                  <input
                    className="form-input"
                    value={editUni.minus}
                    onChange={(e) => updateEditUni({ minus: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>Сроки подачи</label>
                  <input
                    className="form-input"
                    value={editUni.deadlines ?? ''}
                    onChange={(e) => updateEditUni({ deadlines: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>Процедура подачи</label>
                  <input
                    className="form-input"
                    value={editUni.procedure ?? ''}
                    onChange={(e) => updateEditUni({ procedure: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>URL фото</label>
                  <input
                    className="form-input"
                    value={editUni.photoUrl ?? ''}
                    onChange={(e) => updateEditUni({ photoUrl: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>URL видео</label>
                  <input
                    className="form-input"
                    value={editUni.videoUrl ?? ''}
                    onChange={(e) => updateEditUni({ videoUrl: e.target.value })}
                  />
                </div>
              </div>
              <div className="form-group" style={{ marginBottom: '1rem' }}>
                <label>Описание</label>
                <textarea
                  className="form-input"
                  rows={3}
                  value={editUni.description ?? ''}
                  onChange={(e) => updateEditUni({ description: e.target.value })}
                />
              </div>

              {/* Actions */}
              <div style={{ display: 'flex', gap: '0.75rem' }}>
                <button className="btn btn-primary" type="button" onClick={saveEditUni}>
                  Сохранить вуз
                </button>
                <button
                  className="btn"
                  type="button"
                  style={{ color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                  onClick={() => {
                    if (window.confirm('Удалить этот вуз?')) {
                      deleteUni(editUni.id);
                    }
                  }}
                >
                  Удалить вуз
                </button>
              </div>
            </div>
          ) : (
            <p style={{ color: 'var(--text-secondary)' }}>
              Выберите вуз из списка или добавьте новый
            </p>
          )}
        </div>
      </div>
    </div>
  );
}

export default AdminAdvisorConfigPage;
