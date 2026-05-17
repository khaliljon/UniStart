import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from '../i18n';
import { AdvisorConfig, AdvisorUniversity, AdvisorCategory, loadAdvisorConfig, saveAdvisorConfig } from '../advisorConfig';

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

function normalizeMultiline(text: string): string[] {
  return text
    .split('\n')
    .map((item) => item.trim())
    .filter(Boolean);
}

function AdminAdvisorConfigPage() {
  const { t } = useTranslation();
  const [config, setConfig] = useState<AdvisorConfig>(loadAdvisorConfig());
  const [selectedUniversityId, setSelectedUniversityId] = useState<string | null>(config.universities[0]?.id ?? null);
  const [editorUniversity, setEditorUniversity] = useState<AdvisorUniversity>(
    config.universities[0] ?? createEmptyUniversity()
  );
  const [newCategory, setNewCategory] = useState({ key: '', label: '', specialties: '' });
  const [message, setMessage] = useState<string | null>(null);

  const selectedUniversity = useMemo(
    () => config.universities.find((uni) => uni.id === selectedUniversityId) ?? null,
    [config.universities, selectedUniversityId]
  );

  useEffect(() => {
    if (selectedUniversity) {
      setEditorUniversity(selectedUniversity);
    } else if (config.universities.length === 0) {
      setEditorUniversity(createEmptyUniversity());
    }
  }, [selectedUniversity, config.universities.length]);

  const saveConfig = () => {
    saveAdvisorConfig(config);
    setMessage('Настройки сохранены');
    window.setTimeout(() => setMessage(null), 2500);
  };

  const reloadConfig = () => {
    const loaded = loadAdvisorConfig();
    setConfig(loaded);
    setSelectedUniversityId(loaded.universities[0]?.id ?? null);
    setMessage('Конфигурация загружена');
    window.setTimeout(() => setMessage(null), 2500);
  };

  const updateCategory = (index: number, patch: Partial<AdvisorCategory>) => {
    setConfig((prev) => {
      const updated = [...prev.categories];
      updated[index] = { ...updated[index], ...patch };
      return { ...prev, categories: updated };
    });
  };

  const deleteCategory = (index: number) => {
    setConfig((prev) => ({
      ...prev,
      categories: prev.categories.filter((_, idx) => idx !== index),
    }));
  };

  const addCategory = () => {
    if (!newCategory.key.trim() || !newCategory.label.trim()) {
      setMessage('Заполните ключ и название категории');
      return;
    }

    if (config.categories.some((category) => category.key === newCategory.key.trim())) {
      setMessage('Ключ категории должен быть уникальным');
      return;
    }

    setConfig((prev) => ({
      ...prev,
      categories: [
        ...prev.categories,
        {
          key: newCategory.key.trim(),
          label: newCategory.label.trim(),
          specialties: normalizeMultiline(newCategory.specialties.replace(/,/g, '\n')),
        },
      ],
    }));
    setNewCategory({ key: '', label: '', specialties: '' });
    setMessage('Категория добавлена');
    window.setTimeout(() => setMessage(null), 2500);
  };

  const handleUniversitySelect = (id: string) => {
    setSelectedUniversityId(id);
  };

  const handleUniversityChange = (patch: Partial<AdvisorUniversity>) => {
    setEditorUniversity((prev) => ({ ...prev, ...patch }));
  };

  const saveUniversity = () => {
    if (!editorUniversity.name.trim()) {
      setMessage('Укажите название университета');
      return;
    }

    setConfig((prev) => {
      const existingIndex = prev.universities.findIndex((item) => item.id === editorUniversity.id);
      if (existingIndex >= 0) {
        const updated = [...prev.universities];
        updated[existingIndex] = editorUniversity;
        return { ...prev, universities: updated };
      }

      return { ...prev, universities: [...prev.universities, editorUniversity] };
    });
    setSelectedUniversityId(editorUniversity.id);
    setMessage('Университет сохранён');
    window.setTimeout(() => setMessage(null), 2500);
  };

  const deleteUniversity = () => {
    if (!selectedUniversityId) return;
    setConfig((prev) => ({
      ...prev,
      universities: prev.universities.filter((uni) => uni.id !== selectedUniversityId),
    }));
    setSelectedUniversityId(null);
    setEditorUniversity(createEmptyUniversity());
    setMessage('Университет удалён');
    window.setTimeout(() => setMessage(null), 2500);
  };

  const startNewUniversity = () => {
    const next = createEmptyUniversity();
    setSelectedUniversityId(null);
    setEditorUniversity(next);
  };

  return (
    <div className="animate-fade-in" style={{ maxWidth: 1100, margin: '0 auto' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        <div>
          <h1 style={{ margin: 0, fontSize: '1.75rem' }}>{t.admin.nav.advisorConfig}</h1>
          <p style={{ margin: '0.75rem 0 0', color: 'var(--text-secondary)', maxWidth: 760 }}>
            Управление данными SAT/NUET-советника: категории, страны, языки и университеты.
          </p>
        </div>
        <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap' }}>
          <button type="button" className="btn" onClick={reloadConfig}>{t.common.reset}</button>
          <button type="button" className="btn btn-primary" onClick={saveConfig}>{t.admin.common.save}</button>
        </div>
      </div>

      {message && (
        <div className="card" style={{ padding: '1rem', marginBottom: '1rem', borderLeft: '4px solid var(--primary-color)' }}>
          {message}
        </div>
      )}

      <div style={{ display: 'grid', gap: '1.5rem' }}>
        <section className="card" style={{ padding: '1.25rem' }}>
          <h2>Страны и языки</h2>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginTop: '1rem' }}>
            <div>
              <label>Страны</label>
              <textarea
                className="form-input"
                rows={6}
                value={config.countries.join('\n')}
                onChange={(event) => setConfig((prev) => ({ ...prev, countries: normalizeMultiline(event.target.value) }))}
              />
              <p style={{ margin: '0.5rem 0 0', color: 'var(--text-secondary)' }}>
                Одна страна на строку.
              </p>
            </div>
            <div>
              <label>Языки</label>
              <textarea
                className="form-input"
                rows={6}
                value={config.languages.join('\n')}
                onChange={(event) => setConfig((prev) => ({ ...prev, languages: normalizeMultiline(event.target.value) }))}
              />
              <p style={{ margin: '0.5rem 0 0', color: 'var(--text-secondary)' }}>
                Один язык на строку.
              </p>
            </div>
          </div>
        </section>

        <section className="card" style={{ padding: '1.25rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '1rem', flexWrap: 'wrap' }}>
            <h2>Категории</h2>
            <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
              <span style={{ color: 'var(--text-secondary)', fontSize: '0.93rem' }}>Добавьте новую категорию с ключом, названием и списком специальностей.</span>
            </div>
          </div>

          <div style={{ marginTop: '1rem', display: 'grid', gap: '1rem' }}>
            {config.categories.map((category, index) => (
              <div key={category.key} style={{ border: '1px solid var(--border-color)', borderRadius: 12, padding: '1rem' }}>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr auto', gap: '1rem', alignItems: 'end' }}>
                  <div>
                    <label>Ключ</label>
                    <input
                      className="form-input"
                      value={category.key}
                      onChange={(event) => updateCategory(index, { key: event.target.value })}
                    />
                  </div>
                  <div>
                    <label>Название</label>
                    <input
                      className="form-input"
                      value={category.label}
                      onChange={(event) => updateCategory(index, { label: event.target.value })}
                    />
                  </div>
                  <div>
                    <label>Специальности</label>
                    <textarea
                      className="form-input"
                      rows={3}
                      value={category.specialties.join('\n')}
                      onChange={(event) => updateCategory(index, { specialties: normalizeMultiline(event.target.value) })}
                    />
                  </div>
                  <button type="button" className="btn btn-outline" onClick={() => deleteCategory(index)} style={{ height: 40 }}>
                    Удалить
                  </button>
                </div>
              </div>
            ))}
          </div>

          <div style={{ marginTop: '1.5rem', borderTop: '1px solid var(--border-color)', paddingTop: '1rem' }}>
            <h3>Новая категория</h3>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr auto', gap: '1rem', alignItems: 'end' }}>
              <div>
                <label>Ключ</label>
                <input
                  className="form-input"
                  value={newCategory.key}
                  onChange={(event) => setNewCategory((prev) => ({ ...prev, key: event.target.value }))}
                />
              </div>
              <div>
                <label>Название</label>
                <input
                  className="form-input"
                  value={newCategory.label}
                  onChange={(event) => setNewCategory((prev) => ({ ...prev, label: event.target.value }))}
                />
              </div>
              <div>
                <label>Специальности</label>
                <textarea
                  className="form-input"
                  rows={3}
                  value={newCategory.specialties}
                  onChange={(event) => setNewCategory((prev) => ({ ...prev, specialties: event.target.value }))}
                />
              </div>
              <button type="button" className="btn btn-primary" onClick={addCategory} style={{ height: 40 }}>
                Добавить
              </button>
            </div>
          </div>
        </section>

        <section className="card" style={{ padding: '1.25rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
            <h2>Университеты</h2>
            <button type="button" className="btn btn-outline" onClick={startNewUniversity}>Создать новый</button>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1.2fr 1fr', gap: '1rem', marginTop: '1rem' }}>
            <div style={{ border: '1px solid var(--border-color)', borderRadius: 12, padding: '1rem' }}>
              {config.universities.length === 0 ? (
                <p style={{ color: 'var(--text-secondary)' }}>Список университетов пуст.</p>
              ) : (
                <div style={{ display: 'grid', gap: '0.5rem' }}>
                  {config.universities.map((uni) => (
                    <button
                      key={uni.id}
                      type="button"
                      onClick={() => handleUniversitySelect(uni.id)}
                      className="btn"
                      style={{
                        justifyContent: 'start',
                        background: uni.id === selectedUniversityId ? 'var(--primary-bg)' : undefined,
                      }}
                    >
                      {uni.name || '(без названия)'} — {uni.city}
                    </button>
                  ))}
                </div>
              )}
            </div>

            <div style={{ display: 'grid', gap: '1rem' }}>
              <div style={{ display: 'grid', gap: '0.75rem' }}>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                  <div>
                    <label>Название</label>
                    <input
                      className="form-input"
                      value={editorUniversity.name}
                      onChange={(event) => handleUniversityChange({ name: event.target.value })}
                    />
                  </div>
                  <div>
                    <label>Город</label>
                    <input
                      className="form-input"
                      value={editorUniversity.city}
                      onChange={(event) => handleUniversityChange({ city: event.target.value })}
                    />
                  </div>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '1rem' }}>
                  <div>
                    <label>Страна</label>
                    <input
                      className="form-input"
                      value={editorUniversity.country}
                      onChange={(event) => handleUniversityChange({ country: event.target.value })}
                    />
                  </div>
                  <div>
                    <label>Экзамен</label>
                    <select
                      className="form-input"
                      value={editorUniversity.exam}
                      onChange={(event) => handleUniversityChange({ exam: event.target.value as 'SAT' | 'NUET' })}
                    >
                      <option value="SAT">SAT</option>
                      <option value="NUET">NUET</option>
                    </select>
                  </div>
                  <div>
                    <label>Минимальный балл</label>
                    <input
                      className="form-input"
                      type="number"
                      min={0}
                      value={editorUniversity.minScore ?? ''}
                      onChange={(event) => handleUniversityChange({ minScore: event.target.value ? Number(event.target.value) : null })}
                    />
                  </div>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                  <div>
                    <label>Уровень</label>
                    <input
                      className="form-input"
                      value={editorUniversity.level}
                      onChange={(event) => handleUniversityChange({ level: event.target.value })}
                    />
                  </div>
                  <div>
                    <label>Язык</label>
                    <input
                      className="form-input"
                      value={editorUniversity.language}
                      onChange={(event) => handleUniversityChange({ language: event.target.value })}
                    />
                  </div>
                </div>

                <div>
                  <label>Специальности</label>
                  <textarea
                    className="form-input"
                    rows={3}
                    value={editorUniversity.specialties.join(', ')}
                    onChange={(event) => handleUniversityChange({ specialties: event.target.value.split(',').map((item) => item.trim()).filter(Boolean) })}
                  />
                </div>
                <div>
                  <label>Описание</label>
                  <textarea
                    className="form-input"
                    rows={3}
                    value={editorUniversity.description ?? ''}
                    onChange={(event) => handleUniversityChange({ description: event.target.value })}
                  />
                </div>
              </div>

              <div style={{ display: 'grid', gap: '0.75rem' }}>
                <label>Фото (URL)</label>
                <input
                  className="form-input"
                  value={editorUniversity.photoUrl ?? ''}
                  onChange={(event) => handleUniversityChange({ photoUrl: event.target.value })}
                />

                <label>Видео (URL)</label>
                <input
                  className="form-input"
                  value={editorUniversity.videoUrl ?? ''}
                  onChange={(event) => handleUniversityChange({ videoUrl: event.target.value })}
                />

                <label>Гранты</label>
                <textarea
                  className="form-input"
                  rows={2}
                  value={editorUniversity.grants}
                  onChange={(event) => handleUniversityChange({ grants: event.target.value })}
                />

                <label>Плюсы</label>
                <textarea
                  className="form-input"
                  rows={2}
                  value={editorUniversity.plus}
                  onChange={(event) => handleUniversityChange({ plus: event.target.value })}
                />

                <label>Минусы</label>
                <textarea
                  className="form-input"
                  rows={2}
                  value={editorUniversity.minus}
                  onChange={(event) => handleUniversityChange({ minus: event.target.value })}
                />

                <label>Приём / сроки</label>
                <textarea
                  className="form-input"
                  rows={2}
                  value={editorUniversity.deadlines}
                  onChange={(event) => handleUniversityChange({ deadlines: event.target.value })}
                />

                <label>Дополнительно</label>
                <textarea
                  className="form-input"
                  rows={2}
                  value={editorUniversity.procedure ?? ''}
                  onChange={(event) => handleUniversityChange({ procedure: event.target.value })}
                />
              </div>
            </div>
          </div>

          <div style={{ display: 'flex', gap: '0.75rem', marginTop: '1rem', flexWrap: 'wrap' }}>
            <button type="button" className="btn btn-primary" onClick={saveUniversity}>
              Сохранить университет
            </button>
            {selectedUniversityId && (
              <button type="button" className="btn btn-outline" onClick={deleteUniversity}>
                Удалить университет
              </button>
            )}
          </div>
        </section>
      </div>
    </div>
  );
}

export default AdminAdvisorConfigPage;
