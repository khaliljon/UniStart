import { useState, useEffect, useCallback } from 'react';
import mockAdminService from '../services/mockAdminService';
import type {
  MockExamListItem,
  MockSectionInput,
  ExamTypeOption,
  ExamSectionOption,
} from '../services/mockAdminService';
import { useTranslation } from '../i18n';

interface FormState {
  id: number | null;
  examTypeCode: string;
  title: string;
  description: string;
  totalTimeMinutes: number;
  isActive: boolean;
  sections: MockSectionInput[];
}

const emptySection = (sortOrder: number): MockSectionInput => ({
  examSectionId: null,
  name: '',
  timeLimitMinutes: 30,
  questionCount: 10,
  sortOrder,
  instructions: null,
});

function SchoolAdminMocksPage() {
  const { t } = useTranslation();
  const m = t.schoolAdmin.mocks;

  const [mocks, setMocks] = useState<MockExamListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [examTypes, setExamTypes] = useState<ExamTypeOption[]>([]);
  const [poolsByType, setPoolsByType] = useState<Record<string, ExamSectionOption[]>>({});

  const [form, setForm] = useState<FormState | null>(null);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [toast, setToast] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [list, types] = await Promise.all([
        mockAdminService.list(),
        mockAdminService.getExamTypes(),
      ]);
      setMocks(list);
      setExamTypes(types);
    } catch {
      setError(m.loadError);
    } finally {
      setLoading(false);
    }
  }, [m.loadError]);

  useEffect(() => { load(); }, [load]);

  const showToast = (msg: string) => {
    setToast(msg);
    setTimeout(() => setToast(null), 2500);
  };

  const ensurePools = useCallback(async (code: string) => {
    if (!code || poolsByType[code]) return;
    try {
      const pools = await mockAdminService.getExamSections(code);
      setPoolsByType((prev) => ({ ...prev, [code]: pools }));
    } catch { /* ignore */ }
  }, [poolsByType]);

  const startCreate = () => {
    setFormError(null);
    setForm({
      id: null,
      examTypeCode: '',
      title: '',
      description: '',
      totalTimeMinutes: 60,
      isActive: true,
      sections: [emptySection(0)],
    });
  };

  const startEdit = async (id: number) => {
    setFormError(null);
    try {
      const detail = await mockAdminService.get(id);
      await ensurePools(detail.examTypeCode);
      setForm({
        id: detail.id,
        examTypeCode: detail.examTypeCode,
        title: detail.title,
        description: detail.description,
        totalTimeMinutes: detail.totalTimeMinutes,
        isActive: detail.isActive,
        sections: detail.sections
          .slice()
          .sort((a, b) => a.sortOrder - b.sortOrder)
          .map((s) => ({
            id: s.id,
            examSectionId: s.examSectionId,
            name: s.name,
            timeLimitMinutes: s.timeLimitMinutes,
            questionCount: s.questionCount,
            sortOrder: s.sortOrder,
            instructions: s.instructions,
          })),
      });
    } catch {
      showToast(m.loadError);
    }
  };

  const updateForm = (patch: Partial<FormState>) =>
    setForm((f) => (f ? { ...f, ...patch } : f));

  const updateSection = (idx: number, patch: Partial<MockSectionInput>) =>
    setForm((f) => f ? {
      ...f,
      sections: f.sections.map((s, i) => (i === idx ? { ...s, ...patch } : s)),
    } : f);

  const addSection = () =>
    setForm((f) => f ? { ...f, sections: [...f.sections, emptySection(f.sections.length)] } : f);

  const removeSection = (idx: number) =>
    setForm((f) => f ? { ...f, sections: f.sections.filter((_, i) => i !== idx) } : f);

  const onExamTypeChange = async (code: string) => {
    updateForm({ examTypeCode: code });
    await ensurePools(code);
  };

  const save = async () => {
    if (!form) return;
    setSaving(true);
    setFormError(null);
    try {
      const payload = {
        examTypeCode: form.examTypeCode,
        title: form.title,
        description: form.description,
        totalTimeMinutes: form.totalTimeMinutes,
        isActive: form.isActive,
        sections: form.sections.map((s, i) => ({ ...s, sortOrder: i })),
      };
      if (form.id == null) {
        await mockAdminService.create(payload);
      } else {
        await mockAdminService.update(form.id, payload);
      }
      setForm(null);
      showToast(m.saved);
      await load();
    } catch (err) {
      const e = err as { response?: { data?: { error?: string } } };
      setFormError(e.response?.data?.error ?? m.loadError);
    } finally {
      setSaving(false);
    }
  };

  const toggleActive = async (mock: MockExamListItem) => {
    try {
      await mockAdminService.toggleActive(mock.id, !mock.isActive);
      await load();
    } catch { /* ignore */ }
  };

  const remove = async (mock: MockExamListItem) => {
    if (!window.confirm(m.confirmDelete)) return;
    try {
      await mockAdminService.remove(mock.id);
      showToast(m.deleted);
      await load();
    } catch (err) {
      const e = err as { response?: { data?: { error?: string } } };
      showToast(e.response?.data?.error ?? m.loadError);
    }
  };

  // ── Form view ──
  if (form) {
    const pools = poolsByType[form.examTypeCode] ?? [];
    return (
      <div>
        <button className="btn btn-outline" onClick={() => setForm(null)} style={{ marginBottom: '1rem' }}>
          &larr; {m.cancel}
        </button>
        <h1 style={{ marginBottom: '1.5rem' }}>{form.id == null ? m.newMock : m.editMock}</h1>

        {formError && (
          <div className="card" style={{ padding: '0.75rem 1rem', marginBottom: '1rem', color: 'var(--error-color)', borderColor: 'var(--error-color)' }}>
            {formError}
          </div>
        )}

        <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem', display: 'grid', gap: '1rem' }}>
          <label style={labelStyle}>
            {m.fieldTitle}
            <input style={inputStyle} value={form.title} onChange={(e) => updateForm({ title: e.target.value })} />
          </label>

          <label style={labelStyle}>
            {m.fieldDescription}
            <textarea style={{ ...inputStyle, minHeight: '70px', resize: 'vertical' }} value={form.description} onChange={(e) => updateForm({ description: e.target.value })} />
          </label>

          <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr 1fr', gap: '1rem' }}>
            <label style={labelStyle}>
              {m.fieldExamType}
              <select style={inputStyle} value={form.examTypeCode} onChange={(e) => onExamTypeChange(e.target.value)}>
                <option value="">{m.selectExamType}</option>
                {examTypes.map((et) => (
                  <option key={et.code} value={et.code}>{et.name}</option>
                ))}
              </select>
            </label>

            <label style={labelStyle}>
              {m.fieldTotalTime}
              <input type="number" min={1} style={inputStyle} value={form.totalTimeMinutes}
                onChange={(e) => updateForm({ totalTimeMinutes: Number(e.target.value) })} />
            </label>

            <label style={{ ...labelStyle, justifyContent: 'flex-end' }}>
              <span style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginTop: 'auto' }}>
                <input type="checkbox" checked={form.isActive} onChange={(e) => updateForm({ isActive: e.target.checked })} />
                {m.fieldActive}
              </span>
            </label>
          </div>
        </div>

        <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <h3 style={{ margin: 0 }}>{m.sections}</h3>
            <button className="btn btn-outline" onClick={addSection}>+ {m.addSection}</button>
          </div>

          <div style={{ display: 'grid', gap: '1rem' }}>
            {form.sections.map((s, idx) => (
              <div key={idx} style={{ border: '1px solid var(--border-color)', borderRadius: '10px', padding: '1rem' }}>
                <div style={{ display: 'grid', gridTemplateColumns: '2fr 2fr', gap: '0.75rem' }}>
                  <label style={labelStyle}>
                    {m.sectionName}
                    <input style={inputStyle} value={s.name} onChange={(e) => updateSection(idx, { name: e.target.value })} />
                  </label>
                  <label style={labelStyle}>
                    {m.pool}
                    <select
                      style={inputStyle}
                      value={s.examSectionId ?? ''}
                      onChange={(e) => updateSection(idx, { examSectionId: e.target.value ? Number(e.target.value) : null })}
                    >
                      <option value="">{m.noPool}</option>
                      {pools.map((p) => (
                        <option key={p.id} value={p.id}>{p.name}</option>
                      ))}
                    </select>
                  </label>
                </div>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem', marginTop: '0.75rem' }}>
                  <label style={labelStyle}>
                    {m.timeLimit}
                    <input type="number" min={0} style={inputStyle} value={s.timeLimitMinutes}
                      onChange={(e) => updateSection(idx, { timeLimitMinutes: Number(e.target.value) })} />
                  </label>
                  <label style={labelStyle}>
                    {m.questionCount}
                    <input type="number" min={1} style={inputStyle} value={s.questionCount}
                      onChange={(e) => updateSection(idx, { questionCount: Number(e.target.value) })} />
                  </label>
                </div>
                <label style={{ ...labelStyle, marginTop: '0.75rem' }}>
                  {m.instructions}
                  <textarea style={{ ...inputStyle, minHeight: '50px', resize: 'vertical' }}
                    value={s.instructions ?? ''}
                    onChange={(e) => updateSection(idx, { instructions: e.target.value || null })} />
                </label>
                {form.sections.length > 1 && (
                  <button
                    className="btn btn-outline"
                    style={{ marginTop: '0.75rem', color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                    onClick={() => removeSection(idx)}
                  >
                    {m.removeSection}
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>

        <div style={{ display: 'flex', gap: '0.75rem' }}>
          <button className="btn btn-primary" onClick={save} disabled={saving}>
            {saving ? '…' : m.save}
          </button>
          <button className="btn btn-outline" onClick={() => setForm(null)} disabled={saving}>
            {m.cancel}
          </button>
        </div>
      </div>
    );
  }

  // ── List view ──
  return (
    <div>
      {toast && (
        <div style={{
          position: 'fixed', top: '1rem', right: '1rem', zIndex: 1000,
          background: 'var(--primary-color)', color: '#fff', padding: '0.6rem 1rem',
          borderRadius: '8px', fontSize: '0.85rem', boxShadow: '0 4px 12px rgba(0,0,0,0.2)',
        }}>{toast}</div>
      )}

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h1 style={{ margin: '0 0 0.25rem' }}>{m.title}</h1>
          <p style={{ margin: 0, color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{m.subtitle}</p>
        </div>
        <button className="btn btn-primary" onClick={startCreate}>+ {m.createBtn}</button>
      </div>

      {loading ? (
        <div className="card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
          {t.schoolAdmin.loading}
        </div>
      ) : error ? (
        <div className="card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--error-color)' }}>
          {error}
        </div>
      ) : mocks.length === 0 ? (
        <div className="card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
          {m.noMocks}
        </div>
      ) : (
        <div style={{ display: 'grid', gap: '1rem' }}>
          {mocks.map((mock) => (
            <div key={mock.id} className="card" style={{ padding: '1.25rem 1.5rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '1rem', flexWrap: 'wrap' }}>
              <div style={{ flex: 1, minWidth: '220px' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.35rem', flexWrap: 'wrap' }}>
                  <span style={{ fontWeight: 600, fontSize: '1.05rem' }}>{mock.title}</span>
                  <span style={{
                    fontSize: '0.7rem', padding: '0.1rem 0.5rem', borderRadius: '999px', fontWeight: 700,
                    background: mock.isActive ? 'var(--success-color)' : 'var(--border-color)',
                    color: mock.isActive ? '#fff' : 'var(--text-secondary)',
                  }}>
                    {mock.isActive ? m.statusActive : m.statusInactive}
                  </span>
                  <span style={{ fontSize: '0.72rem', color: 'var(--text-secondary)' }}>{mock.examTypeCode}</span>
                </div>
                <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
                  {mock.sectionCount} {m.sectionsCount} · {mock.questionCount} {m.questionsTotal} · {mock.totalTimeMinutes} {m.minutesUnit} · {mock.attemptCount} {m.attempts}
                </div>
              </div>
              <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                <button className="btn btn-outline" onClick={() => toggleActive(mock)}>
                  {mock.isActive ? m.deactivate : m.activate}
                </button>
                <button className="btn btn-outline" onClick={() => startEdit(mock.id)}>
                  {m.editMock}
                </button>
                <button
                  className="btn btn-outline"
                  style={{ color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                  onClick={() => remove(mock)}
                >
                  {m.removeSection}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

const labelStyle: React.CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  gap: '0.35rem',
  fontSize: '0.82rem',
  fontWeight: 600,
  color: 'var(--text-secondary)',
};

const inputStyle: React.CSSProperties = {
  padding: '0.55rem 0.7rem',
  borderRadius: '8px',
  border: '1px solid var(--border-color)',
  background: 'var(--background-color)',
  color: 'var(--text-color)',
  fontSize: '0.9rem',
  fontWeight: 400,
  width: '100%',
};

export default SchoolAdminMocksPage;
