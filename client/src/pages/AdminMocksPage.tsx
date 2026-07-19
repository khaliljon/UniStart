import { useEffect, useState, useCallback } from 'react';
import mockAdminService, {
  type MockExamListItem,
  type MockSectionInput,
  type ExamTypeOption,
  type ExamSectionOption,
} from '../services/mockAdminService';
import { pricingService } from '../services/pricingService';
import { useTranslation } from '../hooks/useTranslation';

function AdminMocksPage() {
  const { locale } = useTranslation();
  const [exams, setExams] = useState<MockExamListItem[]>([]);
  const [examTypes, setExamTypes] = useState<ExamTypeOption[]>([]);
  const [examSections, setExamSections] = useState<ExamSectionOption[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Global pricing (mock price is uniform, admin-editable)
  const [mockPrice, setMockPrice] = useState<number>(0);
  const [materialPrice, setMaterialPrice] = useState<number>(0);
  const [priceCurrency, setPriceCurrency] = useState<string>('KZT');
  const [savingPrice, setSavingPrice] = useState(false);

  // Form states
  const [editingId, setEditingId] = useState<number | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({
    examTypeCode: '',
    title: '',
    description: '',
    titleKz: '',
    titleEn: '',
    descriptionKz: '',
    descriptionEn: '',
    totalTimeMinutes: 120,
    isActive: true,
    sections: [] as MockSectionInput[],
  });

  const loadExams = useCallback(async () => {
    setLoading(true);
    try {
      const list = await mockAdminService.list();
      setExams(list);
    } catch {
      setError('Не удалось загрузить пробные экзамены');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadExams();
    mockAdminService.getExamTypes().then(setExamTypes).catch(() => {});
  }, [loadExams]);

  useEffect(() => {
    pricingService.get().then((p) => {
      setMockPrice(p.mockPrice);
      setMaterialPrice(p.materialPrice);
      setPriceCurrency(p.currency);
    }).catch(() => {});
  }, []);

  const savePricing = async () => {
    setSavingPrice(true);
    setError(null);
    setSuccess(null);
    try {
      const saved = await pricingService.update({ mockPrice, materialPrice, currency: priceCurrency });
      setMockPrice(saved.mockPrice);
      setMaterialPrice(saved.materialPrice);
      setPriceCurrency(saved.currency);
      setSuccess('Цены сохранены');
    } catch {
      setError('Не удалось сохранить цены');
    } finally {
      setSavingPrice(false);
    }
  };

  useEffect(() => {
    if (form.examTypeCode) {      mockAdminService.getExamSections(form.examTypeCode)
        .then(setExamSections)
        .catch(() => setExamSections([]));
    } else {
      setExamSections([]);
    }
  }, [form.examTypeCode]);

  const openCreate = () => {
    setEditingId(null);
    setForm({
      examTypeCode: examTypes[0]?.code || '',
      title: '',
      description: '',
      titleKz: '',
      titleEn: '',
      descriptionKz: '',
      descriptionEn: '',
      totalTimeMinutes: 120,
      isActive: true,
      sections: [],
    });
    setShowForm(true);
    setError(null);
    setSuccess(null);
  };

  const openEdit = async (id: number) => {
    setEditingId(id);
    setLoading(true);
    try {
      const detail = await mockAdminService.get(id);
      setForm({
        examTypeCode: detail.examTypeCode,
        title: detail.title,
        description: detail.description,
        titleKz: detail.titleKz || '',
        titleEn: detail.titleEn || '',
        descriptionKz: detail.descriptionKz || '',
        descriptionEn: detail.descriptionEn || '',
        totalTimeMinutes: detail.totalTimeMinutes,
        isActive: detail.isActive,
        sections: detail.sections.map((s) => ({
          id: s.id,
          examSectionId: s.examSectionId,
          name: s.name,
          timeLimitMinutes: s.timeLimitMinutes,
          questionCount: s.questionCount,
          sortOrder: s.sortOrder,
          instructions: s.instructions,
        })),
      });
      setShowForm(true);
      setError(null);
      setSuccess(null);
    } catch {
      setError('Не удалось загрузить детали экзамена');
    } finally {
      setLoading(false);
    }
  };

  const toggleActive = async (id: number, currentActive: boolean) => {
    try {
      await mockAdminService.toggleActive(id, !currentActive);
      setExams((prev) =>
        prev.map((e) => (e.id === id ? { ...e, isActive: !currentActive } : e))
      );
      setSuccess('Статус изменен');
    } catch {
      setError('Не удалось переключить статус');
    }
  };

  const handleRemove = async (id: number) => {
    if (!confirm('Удалить пробный экзамен со всеми секциями? Внимание: история попыток также будет удалена.')) return;
    try {
      await mockAdminService.remove(id);
      setSuccess('Экзамен удален');
      loadExams();
    } catch {
      setError('Ошибка при удалении');
    }
  };

  const addSection = () => {
    setForm((prev) => ({
      ...prev,
      sections: [
        ...prev.sections,
        {
          examSectionId: examSections[0]?.id || null,
          name: `Секция ${prev.sections.length + 1}`,
          timeLimitMinutes: 30,
          questionCount: 20,
          sortOrder: prev.sections.length,
          instructions: '',
        },
      ],
    }));
  };

  const removeSection = (index: number) => {
    setForm((prev) => ({
      ...prev,
      sections: prev.sections.filter((_, i) => i !== index),
    }));
  };

  const updateSection = (index: number, fields: Partial<MockSectionInput>) => {
    setForm((prev) => ({
      ...prev,
      sections: prev.sections.map((sec, i) => (i === index ? { ...sec, ...fields } : sec)),
    }));
  };

  const save = async () => {
    if (!form.title.trim()) {
      setError('Введите название пробника');
      return;
    }
    if (form.sections.length === 0) {
      setError('Добавьте хотя бы одну секцию');
      return;
    }

    try {
      const payload = {
        examTypeCode: form.examTypeCode,
        title: form.title,
        description: form.description,
        titleKz: form.titleKz || null,
        titleEn: form.titleEn || null,
        descriptionKz: form.descriptionKz || null,
        descriptionEn: form.descriptionEn || null,
        totalTimeMinutes: form.totalTimeMinutes,
        isActive: form.isActive,
        sections: form.sections,
      };

      if (editingId) {
        await mockAdminService.update(editingId, payload);
        setSuccess('Пробный экзамен успешно обновлен');
      } else {
        await mockAdminService.create(payload);
        setSuccess('Пробный экзамен успешно создан');
      }
      setShowForm(false);
      setEditingId(null);
      loadExams();
    } catch {
      setError('Не удалось сохранить изменения');
    }
  };

  const isEn = locale === 'en';
  const isKz = locale === 'kz';

  const headingText = isEn
    ? 'Mock Exams Management'
    : isKz
    ? 'Сынақ емтихандарын басқару'
    : 'Управление пробными экзаменами';

  return (
    <div style={{ maxWidth: '1000px', margin: '1.5rem auto', padding: '0 1rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ fontSize: '1.75rem', fontWeight: 700, margin: 0 }}>{headingText}</h1>
        {!showForm && (
          <button className="btn btn-primary" onClick={openCreate}>
            + Создать пробник
          </button>
        )}
      </div>

      {error && (
        <div style={{ padding: '0.75rem 1rem', background: 'var(--error-bg)', color: 'var(--error-color)', borderRadius: '8px', marginBottom: '1rem', fontSize: '0.9rem' }}>
          ✕ {error}
        </div>
      )}
      {success && (
        <div style={{ padding: '0.75rem 1rem', background: 'rgba(16,185,129,0.08)', color: 'var(--success-color)', borderRadius: '8px', marginBottom: '1rem', fontSize: '0.9rem' }}>
          ✓ {success}
        </div>
      )}

      {!showForm && (
        <div className="card" style={{ padding: '1.25rem', marginBottom: '1.5rem', display: 'flex', flexWrap: 'wrap', alignItems: 'flex-end', gap: '1rem' }}>
          <div style={{ flex: '1 1 100%' }}>
            <h2 style={{ fontSize: '1.1rem', margin: 0 }}>Цена учебника</h2>
            <p style={{ margin: '0.25rem 0 0', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
              Цены на моки и пакеты — во вкладке «Цены».
            </p>
          </div>
          <label style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem', fontSize: '0.85rem' }}>
            Цена за учебник
            <input
              type="number"
              min={0}
              value={materialPrice}
              onChange={(e) => setMaterialPrice(Number(e.target.value))}
              style={{ padding: '0.5rem', border: '1px solid var(--border-color)', borderRadius: '6px', width: '140px' }}
            />
          </label>
          <label style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem', fontSize: '0.85rem' }}>
            Валюта
            <input
              type="text"
              value={priceCurrency}
              onChange={(e) => setPriceCurrency(e.target.value.toUpperCase())}
              maxLength={8}
              style={{ padding: '0.5rem', border: '1px solid var(--border-color)', borderRadius: '6px', width: '90px' }}
            />
          </label>
          <button className="btn btn-primary" onClick={savePricing} disabled={savingPrice}>
            {savingPrice ? 'Сохранение…' : 'Сохранить'}
          </button>
        </div>
      )}

      {showForm ? (
        <div className="card" style={{ padding: '1.5rem', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <h2 style={{ fontSize: '1.3rem', margin: 0 }}>
            {editingId ? 'Редактировать пробный экзамен' : 'Новый пробный экзамен'}
          </h2>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
            <label style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
              <span style={{ fontWeight: 600, fontSize: '0.85rem' }}>Тип экзамена *</span>
              <select
                value={form.examTypeCode}
                onChange={(e) => setForm((p) => ({ ...p, examTypeCode: e.target.value }))}
                style={selectStyle}
              >
                {examTypes.map((et) => (
                  <option key={et.code} value={et.code}>
                    {et.code}
                  </option>
                ))}
              </select>
            </label>

            <label style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
              <span style={{ fontWeight: 600, fontSize: '0.85rem' }}>Общее время (минут) *</span>
              <input
                type="number"
                value={form.totalTimeMinutes}
                onChange={(e) => setForm((p) => ({ ...p, totalTimeMinutes: Number(e.target.value) }))}
                style={inputStyle}
              />
            </label>
          </div>

          <label style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
            <span style={{ fontWeight: 600, fontSize: '0.85rem' }}>Название пробника *</span>
            <input
              type="text"
              value={form.title}
              onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))}
              style={inputStyle}
              placeholder="CSCA Мок Математика (Вариант А)"
            />
          </label>

          <label style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
            <span style={{ fontWeight: 600, fontSize: '0.85rem' }}>Название (KZ)</span>
            <input
              type="text"
              value={form.titleKz}
              onChange={(e) => setForm((p) => ({ ...p, titleKz: e.target.value }))}
              style={inputStyle}
              placeholder="Оставьте пустым — покажется русское"
            />
          </label>

          <label style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
            <span style={{ fontWeight: 600, fontSize: '0.85rem' }}>Название (EN)</span>
            <input
              type="text"
              value={form.titleEn}
              onChange={(e) => setForm((p) => ({ ...p, titleEn: e.target.value }))}
              style={inputStyle}
              placeholder="Leave empty to fall back to Russian"
            />
          </label>

          <label style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
            <span style={{ fontWeight: 600, fontSize: '0.85rem' }}>Описание</span>
            <textarea
              value={form.description}
              onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
              rows={3}
              style={{ ...inputStyle, resize: 'vertical' }}
            />
          </label>

          <label style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
            <span style={{ fontWeight: 600, fontSize: '0.85rem' }}>Описание (KZ)</span>
            <textarea
              value={form.descriptionKz}
              onChange={(e) => setForm((p) => ({ ...p, descriptionKz: e.target.value }))}
              rows={3}
              style={{ ...inputStyle, resize: 'vertical' }}
            />
          </label>

          <label style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
            <span style={{ fontWeight: 600, fontSize: '0.85rem' }}>Описание (EN)</span>
            <textarea
              value={form.descriptionEn}
              onChange={(e) => setForm((p) => ({ ...p, descriptionEn: e.target.value }))}
              rows={3}
              style={{ ...inputStyle, resize: 'vertical' }}
            />
          </label>

          <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer', margin: '0.5rem 0' }}>
            <input
              type="checkbox"
              checked={form.isActive}
              onChange={(e) => setForm((p) => ({ ...p, isActive: e.target.checked }))}
            />
            <span style={{ fontSize: '0.9rem' }}>Активен (виден студентам для прохождения)</span>
          </label>

          <hr style={{ border: 'none', borderTop: '1px solid var(--border-color)', margin: '1rem 0' }} />

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <h3 style={{ fontSize: '1.1rem', margin: 0 }}>Секции экзамена ({form.sections.length})</h3>
            <button className="btn btn-outline" style={{ fontSize: '0.8rem' }} onClick={addSection}>
              + Добавить секцию
            </button>
          </div>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', marginTop: '0.5rem' }}>
            {form.sections.map((sec, i) => (
              <div
                key={i}
                className="card"
                style={{
                  background: 'var(--bg-secondary)',
                  padding: '1rem',
                  display: 'flex',
                  flexDirection: 'column',
                  gap: '0.75rem',
                  border: '1px solid var(--border-color)',
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontWeight: 700, fontSize: '0.9rem' }}>Секция #{i + 1}</span>
                  <button
                    className="btn btn-outline"
                    style={{ padding: '0.2rem 0.5rem', color: 'var(--error-color)', fontSize: '0.75rem' }}
                    onClick={() => removeSection(i)}
                  >
                    Удалить
                  </button>
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: '1.5fr 1fr 1fr 1fr', gap: '0.75rem' }}>
                  <label style={subLabelStyle}>
                    <span>Название секции *</span>
                    <input
                      type="text"
                      value={sec.name}
                      onChange={(e) => updateSection(i, { name: e.target.value })}
                      style={smallInputStyle}
                    />
                  </label>

                  <label style={subLabelStyle}>
                    <span>Время (минут) *</span>
                    <input
                      type="number"
                      value={sec.timeLimitMinutes}
                      onChange={(e) => updateSection(i, { timeLimitMinutes: Number(e.target.value) })}
                      style={smallInputStyle}
                    />
                  </label>

                  <label style={subLabelStyle}>
                    <span>Кол-во вопросов *</span>
                    <input
                      type="number"
                      value={sec.questionCount}
                      onChange={(e) => updateSection(i, { questionCount: Number(e.target.value) })}
                      style={smallInputStyle}
                    />
                    {(() => {
                      const avail = examSections.find((x) => x.id === sec.examSectionId)?.availableQuestions;
                      if (avail === undefined) return null;
                      const short = sec.questionCount > avail;
                      return (
                        <span style={{ fontSize: '0.75rem', color: short ? 'var(--error-color, #ef4444)' : 'var(--text-secondary)' }}>
                          {short
                            ? `⚠ В банке только ${avail} вопросов — мок стартует с ${avail}`
                            : `В банке: ${avail} вопросов`}
                        </span>
                      );
                    })()}
                  </label>

                  <label style={subLabelStyle}>
                    <span>Привязка к разделу *</span>
                    <select
                      value={sec.examSectionId || ''}
                      onChange={(e) => updateSection(i, { examSectionId: Number(e.target.value) || null })}
                      style={smallSelectStyle}
                    >
                      {examSections.map((exs) => (
                        <option key={exs.id} value={exs.id}>
                          {exs.name}
                        </option>
                      ))}
                    </select>
                  </label>
                </div>

                <label style={subLabelStyle}>
                  <span>Инструкции для секции</span>
                  <input
                    type="text"
                    value={sec.instructions || ''}
                    onChange={(e) => updateSection(i, { instructions: e.target.value })}
                    style={smallInputStyle}
                    placeholder="Инструкции перед стартом этой секции..."
                  />
                </label>
              </div>
            ))}
          </div>

          <div style={{ display: 'flex', gap: '0.75rem', marginTop: '1.5rem' }}>
            <button className="btn btn-primary" onClick={save}>
              Сохранить
            </button>
            <button
              className="btn btn-outline"
              onClick={() => {
                setShowForm(false);
                setEditingId(null);
              }}
            >
              Отмена
            </button>
          </div>
        </div>
      ) : loading ? (
        <div className="loading">
          <div className="spinner" />
        </div>
      ) : exams.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '2rem' }}>
          <p style={{ color: 'var(--text-secondary)' }}>Пробные экзамены еще не созданы.</p>
          <button className="btn btn-primary" onClick={openCreate}>
            Создать первый пробник
          </button>
        </div>
      ) : (
        <div className="card" style={{ padding: '0', overflow: 'hidden' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
            <thead>
              <tr style={{ background: 'var(--bg-secondary)', borderBottom: '1px solid var(--border-color)', textAlign: 'left' }}>
                <th style={thStyle}>ID</th>
                <th style={thStyle}>Экзамен</th>
                <th style={thStyle}>Название</th>
                <th style={thStyle}>Время</th>
                <th style={thStyle}>Секций</th>
                <th style={thStyle}>Вопросов</th>
                <th style={thStyle}>Попыток</th>
                <th style={thStyle}>Активен</th>
                <th style={thStyle}></th>
              </tr>
            </thead>
            <tbody>
              {exams.map((e) => (
                <tr key={e.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                  <td style={tdStyle}>#{e.id}</td>
                  <td style={tdStyle}>
                    <span style={{ padding: '0.2rem 0.5rem', borderRadius: '4px', background: '#c8102e14', color: 'var(--primary-color)', fontSize: '0.75rem', fontWeight: 600 }}>
                      {e.examTypeCode}
                    </span>
                  </td>
                  <td style={{ ...tdStyle, fontWeight: 600 }}>{e.title}</td>
                  <td style={tdStyle}>{e.totalTimeMinutes} мин</td>
                  <td style={tdStyle}>{e.sectionCount}</td>
                  <td style={tdStyle}>{e.questionCount}</td>
                  <td style={tdStyle}>{e.attemptCount}</td>
                  <td style={tdStyle}>
                    <button
                      onClick={() => toggleActive(e.id, e.isActive)}
                      style={{
                        border: 'none',
                        background: 'none',
                        cursor: 'pointer',
                        color: e.isActive ? 'var(--success-color)' : 'var(--text-secondary)',
                        fontWeight: 600,
                      }}
                    >
                      {e.isActive ? '✓ Активен' : '✕ Неактивен'}
                    </button>
                  </td>
                  <td style={{ ...tdStyle, textAlign: 'right' }}>
                    <div style={{ display: 'flex', gap: '0.4rem', justifyContent: 'flex-end' }}>
                      <button className="btn btn-outline" style={{ padding: '0.25rem 0.5rem', fontSize: '0.8rem' }} onClick={() => openEdit(e.id)}>
                        ✎
                      </button>
                      <button
                        className="btn btn-outline"
                        style={{ padding: '0.25rem 0.5rem', fontSize: '0.8rem', color: 'var(--error-color)' }}
                        onClick={() => handleRemove(e.id)}
                      >
                        ✕
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

// ─── Inline Styles ────────────────────────────────────────

const thStyle: React.CSSProperties = {
  padding: '0.75rem 1rem',
  fontWeight: 600,
  color: 'var(--text-secondary)',
};

const tdStyle: React.CSSProperties = {
  padding: '0.75rem 1rem',
  verticalAlign: 'middle',
};

const inputStyle: React.CSSProperties = {
  padding: '0.5rem 0.75rem',
  borderRadius: '0.375rem',
  border: '1px solid var(--border-color)',
  background: 'var(--card-background)',
  color: 'var(--text-primary)',
  fontSize: '0.9rem',
};

const selectStyle: React.CSSProperties = {
  ...inputStyle,
  cursor: 'pointer',
  height: '2.4rem',
};

const subLabelStyle: React.CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: '0.25rem',
  fontSize: '0.8rem',
  color: 'var(--text-secondary)',
};

const smallInputStyle: React.CSSProperties = {
  ...inputStyle,
  padding: '0.4rem 0.6rem',
  fontSize: '0.85rem',
};

const smallSelectStyle: React.CSSProperties = {
  ...selectStyle,
  padding: '0.4rem 0.6rem',
  fontSize: '0.85rem',
  height: '2.1rem',
};

export default AdminMocksPage;
