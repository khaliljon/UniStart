import { useEffect, useState, useCallback } from 'react';
import { tutorService } from '../services/tutorService';
import type { TutorQuestionListItem, TutorQuestionDetail, AdminTopicSummary, AdminAnswerOption } from '../types';
import { useTranslation } from '../hooks/useTranslation';

const EXAM_COLORS: Record<string, string> = {
  SAT: '#4f46e5', NUET: '#7c3aed',
};
const DIFF_COLORS: Record<string, string> = {
  Easy: 'var(--success-color)', Medium: 'var(--warning-color)', Hard: 'var(--error-color)',
};

type ModalMode = 'view' | 'edit' | 'create';

const emptyForm = {
  topicId: 0,
  text: '',
  difficulty: 'Medium',
  explanation: '',
  imageUrl: '',
  answerOptions: [
    { text: '', isCorrect: true },
    { text: '', isCorrect: false },
    { text: '', isCorrect: false },
    { text: '', isCorrect: false },
  ],
};

function TutorQuestionsPage() {
  const { t } = useTranslation();

  const [questions, setQuestions] = useState<TutorQuestionListItem[]>([]);
  const [topics, setTopics] = useState<AdminTopicSummary[]>([]);
  const [selected, setSelected] = useState<TutorQuestionDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [filterExam, setFilterExam] = useState('');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [modalMode, setModalMode] = useState<ModalMode>('view');
  const [form, setForm] = useState(emptyForm);
  const [formSectionId, setFormSectionId] = useState<number>(0);
  const [uploadingImage, setUploadingImage] = useState(false);

  // Math keyboard
  const MATH_SYMBOLS: Record<string, string[]> = {
    'αβγ': ['α','β','γ','δ','θ','λ','μ','σ','π','ε','φ','ω','Σ','Δ','Ω','Φ'],
    '+-×÷': ['+','−','×','÷','±','·','/','^','(',')','{','}','[',']'],
    '≤≥≠': ['<','>','≤','≥','≠','≈','≡','→','←','⇒','⇔'],
    'x²ₙ': ['¹','²','³','⁴','⁵','⁻','⁺','ⁿ','₀','₁','₂','₃','ₙ','ₓ'],
    '∫√∑': ['∫','∂','∑','∏','√','∛','∞','lim','log','ln','°'],
    '½¾': ['½','⅓','¼','⅕','⅔','¾'],
  };
  const [showMathKeyboard, setShowMathKeyboard] = useState(false);
  const [mathTab, setMathTab] = useState('αβγ');
  const [mathTarget, setMathTarget] = useState<'text' | 'explanation' | number | null>(null);

  // ── Data loading ──

  const loadQuestions = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const result = await tutorService.getMyQuestions({
        search: search || undefined,
        examType: filterExam || undefined,
        page,
        pageSize: 20,
      });
      setQuestions(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch {
      setError(t.tutor.questionsLoadError);
    } finally {
      setIsLoading(false);
    }
  }, [search, filterExam, page]);

  useEffect(() => { loadQuestions(); }, [loadQuestions]);
  useEffect(() => {
    tutorService.getTopics().then(setTopics).catch(() => {});
  }, []);

  // ── CRUD ──

  const openCreate = () => {
    setForm({ ...emptyForm, answerOptions: emptyForm.answerOptions.map(o => ({ ...o })) });
    setFormSectionId(0);
    setSelected(null);
    setModalMode('create');
    setError(null);
  };

  const openView = async (id: number) => {
    try {
      const q = await tutorService.getQuestion(id);
      setSelected(q);
      setModalMode('view');
    } catch {
      setError(t.tutor.questionsLoadError);
    }
  };

  const openEdit = () => {
    if (!selected) return;
    const sec = topics.find(tp => tp.id === selected.topicId);
    setForm({
      topicId: selected.topicId,
      text: selected.text,
      difficulty: selected.difficulty,
      explanation: selected.explanation || '',
      imageUrl: selected.imageUrl || '',
      answerOptions: selected.answerOptions.length > 0
        ? selected.answerOptions.map(a => ({ text: a.text, isCorrect: a.isCorrect }))
        : emptyForm.answerOptions.map(o => ({ ...o })),
    });
    setFormSectionId(sec ? (topics.find(tp => tp.id === selected.topicId)?.id ?? 0) : 0);
    setModalMode('edit');
    setError(null);
  };

  const saveQuestion = async () => {
    if (!form.topicId) { setError(t.tutor.selectTopic); return; }
    if (!form.text.trim()) { setError(t.tutor.enterQuestionText); return; }
    if (form.answerOptions.filter(o => o.text.trim()).length < 2) { setError(t.tutor.minTwoOptions); return; }
    if (!form.answerOptions.some(o => o.isCorrect && o.text.trim())) { setError(t.tutor.markCorrectOption); return; }

    const options = form.answerOptions.filter(o => o.text.trim());
    try {
      setError(null);
      if (modalMode === 'create') {
        await tutorService.createQuestion({
          topicId: form.topicId,
          text: form.text,
          difficulty: form.difficulty,
          explanation: form.explanation || undefined,
          imageUrl: form.imageUrl || undefined,
          answerOptions: options,
        });
        setSuccess(t.tutor.questionCreated);
      } else if (modalMode === 'edit' && selected) {
        const detail = await tutorService.updateQuestion(selected.id, {
          topicId: form.topicId,
          text: form.text,
          difficulty: form.difficulty,
          explanation: form.explanation || undefined,
          imageUrl: form.imageUrl || undefined,
          answerOptions: options,
        });
        setSelected(detail);
      }
      setModalMode('view');
      loadQuestions();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.tutor.questionSaveError;
      setError(msg);
    }
  };

  const deleteQuestion = async (id: number) => {
    if (!confirm(t.tutor.confirmDeleteQuestion)) return;
    try {
      await tutorService.deleteQuestion(id);
      setSuccess(t.tutor.questionDeleted);
      setSelected(null);
      setModalMode('view');
      loadQuestions();
    } catch {
      setError(t.tutor.questionDeleteError);
    }
  };

  // ── Form helpers ──

  const updateOption = (index: number, field: 'text' | 'isCorrect', value: string | boolean) => {
    setForm(prev => ({
      ...prev,
      answerOptions: prev.answerOptions.map((o, i) =>
        i === index ? { ...o, [field]: value } : field === 'isCorrect' && value ? { ...o, isCorrect: false } : o
      ),
    }));
  };

  const addOption = () => {
    if (form.answerOptions.length >= 6) return;
    setForm(prev => ({ ...prev, answerOptions: [...prev.answerOptions, { text: '', isCorrect: false }] }));
  };

  const removeOption = (index: number) => {
    if (form.answerOptions.length <= 2) return;
    setForm(prev => ({ ...prev, answerOptions: prev.answerOptions.filter((_, i) => i !== index) }));
  };

  const insertMathSymbol = (symbol: string) => {
    if (mathTarget === 'text') setForm(prev => ({ ...prev, text: prev.text + symbol }));
    else if (mathTarget === 'explanation') setForm(prev => ({ ...prev, explanation: prev.explanation + symbol }));
    else if (typeof mathTarget === 'number') updateOption(mathTarget, 'text', form.answerOptions[mathTarget].text + symbol);
  };

  // ── Filtered topics for section selector ──
  const examCodes = [...new Set(topics.map(tp => tp.examTypeCode))];

  // For form: filter by exam code of selected topic or by formSectionId
  const formExam = form.topicId ? topics.find(tp => tp.id === form.topicId)?.examTypeCode : '';
  const formSections = [...new Set(topics.filter(tp => !formExam || tp.examTypeCode === formExam).map(tp => tp.sectionName))];

  // ── UI ──

  const isModal = modalMode === 'create' || modalMode === 'edit' || (modalMode === 'view' && selected);

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h1 style={{ margin: 0 }}>{t.tutor.myQuestions}</h1>
          <p style={{ margin: '0.25rem 0 0', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
            {t.tutor.totalQuestions}: {totalCount}
          </p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>+ {t.tutor.createQuestion}</button>
      </div>

      {success && (
        <div className="alert alert-success" style={{ marginBottom: '1rem' }}>
          {success}
          <button onClick={() => setSuccess(null)} style={{ float: 'right', background: 'none', border: 'none', cursor: 'pointer', fontSize: '1rem' }}>✕</button>
        </div>
      )}

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
        <input
          type="text"
          placeholder={t.tutor.searchQuestions}
          value={search}
          onChange={e => { setSearch(e.target.value); setPage(1); }}
          style={{ padding: '0.5rem 0.75rem', borderRadius: '8px', border: '1px solid var(--border-color)', minWidth: '200px', background: 'var(--card-bg)', color: 'var(--text-primary)' }}
        />
        <select
          value={filterExam}
          onChange={e => { setFilterExam(e.target.value); setPage(1); }}
          style={{ padding: '0.5rem 0.75rem', borderRadius: '8px', border: '1px solid var(--border-color)', background: 'var(--card-bg)', color: 'var(--text-primary)' }}
        >
          <option value="">{t.tutor.allExams}</option>
          {examCodes.map(c => <option key={c} value={c}>{c}</option>)}
        </select>
      </div>

      {/* Table */}
      {isLoading ? (
        <div className="loading"><div className="spinner" /></div>
      ) : error && !isModal ? (
        <div className="alert alert-error">{error}</div>
      ) : questions.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <h3>{t.tutor.noQuestions}</h3>
          <p style={{ color: 'var(--text-secondary)' }}>{t.tutor.noQuestionsDesc}</p>
          <button className="btn btn-primary" onClick={openCreate} style={{ marginTop: '1rem' }}>+ {t.tutor.createQuestion}</button>
        </div>
      ) : (
        <>
          <div className="card" style={{ overflow: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse' }}>
              <thead>
                <tr style={{ borderBottom: '2px solid var(--border-color)' }}>
                  <th style={{ textAlign: 'left', padding: '0.75rem' }}>ID</th>
                  <th style={{ textAlign: 'left', padding: '0.75rem' }}>{t.tutor.questionText}</th>
                  <th style={{ textAlign: 'left', padding: '0.75rem' }}>{t.tutor.topic}</th>
                  <th style={{ textAlign: 'center', padding: '0.75rem' }}>{t.tutor.exam}</th>
                  <th style={{ textAlign: 'center', padding: '0.75rem' }}>{t.tutor.difficulty}</th>
                  <th style={{ textAlign: 'center', padding: '0.75rem' }}>{t.tutor.options}</th>
                  <th style={{ textAlign: 'right', padding: '0.75rem' }}></th>
                </tr>
              </thead>
              <tbody>
                {questions.map(q => (
                  <tr key={q.id} style={{ borderBottom: '1px solid var(--border-color)', cursor: 'pointer' }} onClick={() => openView(q.id)}>
                    <td style={{ padding: '0.75rem', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{q.id}</td>
                    <td style={{ padding: '0.75rem', maxWidth: '300px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{q.text}</td>
                    <td style={{ padding: '0.75rem', fontSize: '0.85rem' }}>{q.topicName}</td>
                    <td style={{ textAlign: 'center', padding: '0.75rem' }}>
                      <span style={{ padding: '0.15rem 0.5rem', borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700, color: '#fff', background: EXAM_COLORS[q.examTypeCode] || '#666' }}>
                        {q.examTypeCode}
                      </span>
                    </td>
                    <td style={{ textAlign: 'center', padding: '0.75rem' }}>
                      <span style={{ color: DIFF_COLORS[q.difficulty] || 'var(--text-primary)', fontWeight: 600, fontSize: '0.85rem' }}>{q.difficulty}</span>
                    </td>
                    <td style={{ textAlign: 'center', padding: '0.75rem', fontSize: '0.85rem' }}>{q.answerCount}</td>
                    <td style={{ textAlign: 'right', padding: '0.75rem' }}>
                      <button
                        className="btn btn-sm"
                        onClick={e => { e.stopPropagation(); deleteQuestion(q.id); }}
                        style={{ fontSize: '0.8rem', padding: '0.25rem 0.5rem', background: 'var(--error-color)', color: '#fff', border: 'none', borderRadius: '6px' }}
                      >✕</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginTop: '1rem' }}>
              <button className="btn btn-sm" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>{t.tutor.prev}</button>
              <span style={{ padding: '0.5rem', fontSize: '0.9rem' }}>{page} / {totalPages}</span>
              <button className="btn btn-sm" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>{t.tutor.next}</button>
            </div>
          )}
        </>
      )}

      {/* Modal: view / edit / create */}
      {isModal && (
        <div
          style={{ position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', justifyContent: 'center', alignItems: 'flex-start', paddingTop: '5vh', zIndex: 1000, overflow: 'auto' }}
          onClick={() => { setSelected(null); setModalMode('view'); setError(null); }}
        >
          <div
            className="card"
            style={{ width: '700px', maxWidth: '95vw', maxHeight: '85vh', overflow: 'auto', padding: '1.5rem' }}
            onClick={e => e.stopPropagation()}
          >
            {/* View mode */}
            {modalMode === 'view' && selected && (
              <>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                  <h2 style={{ margin: 0 }}>#{selected.id}</h2>
                  <div style={{ display: 'flex', gap: '0.5rem' }}>
                    <button className="btn btn-sm" onClick={openEdit}>{t.tutor.edit}</button>
                    <button className="btn btn-sm" style={{ background: 'var(--error-color)', color: '#fff' }} onClick={() => deleteQuestion(selected.id)}>{t.tutor.delete}</button>
                    <button className="btn btn-sm" onClick={() => { setSelected(null); setError(null); }}>✕</button>
                  </div>
                </div>

                <div style={{ marginBottom: '0.75rem' }}>
                  <span style={{ padding: '0.15rem 0.5rem', borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700, color: '#fff', background: EXAM_COLORS[selected.examTypeCode] || '#666', marginRight: '0.5rem' }}>
                    {selected.examTypeCode}
                  </span>
                  <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{selected.sectionName} / {selected.topicName}</span>
                  <span style={{ marginLeft: '0.5rem', color: DIFF_COLORS[selected.difficulty], fontWeight: 600 }}>{selected.difficulty}</span>
                </div>

                <div style={{ padding: '1rem', background: 'var(--bg-secondary)', borderRadius: '8px', marginBottom: '1rem', whiteSpace: 'pre-wrap' }}>
                  {selected.text}
                </div>

                <div style={{ marginBottom: '1rem' }}>
                  {selected.answerOptions.map((opt: AdminAnswerOption) => (
                    <div key={opt.id} style={{
                      padding: '0.5rem 0.75rem', marginBottom: '0.25rem', borderRadius: '6px',
                      background: opt.isCorrect ? 'rgba(34,197,94,0.15)' : 'transparent',
                      border: `1px solid ${opt.isCorrect ? 'var(--success-color)' : 'var(--border-color)'}`,
                    }}>
                      {opt.isCorrect ? '✓ ' : ''}{opt.text}
                    </div>
                  ))}
                </div>

                {selected.explanation && (
                  <div style={{ padding: '0.75rem', background: 'rgba(59,130,246,0.1)', borderRadius: '8px', fontSize: '0.9rem' }}>
                    <strong>{t.tutor.explanation}:</strong> {selected.explanation}
                  </div>
                )}
              </>
            )}

            {/* Create / Edit mode */}
            {(modalMode === 'create' || modalMode === 'edit') && (
              <>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                  <h2 style={{ margin: 0 }}>{modalMode === 'create' ? t.tutor.createQuestion : t.tutor.editQuestion}</h2>
                  <button className="btn btn-sm" onClick={() => { setModalMode('view'); setError(null); if (!selected) { setSelected(null); } }}>✕</button>
                </div>

                {error && <div className="alert alert-error" style={{ marginBottom: '1rem' }}>{error}</div>}

                {/* Exam filter for topic selection */}
                <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '0.75rem', flexWrap: 'wrap' }}>
                  <div style={{ flex: 1, minWidth: '120px' }}>
                    <label style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>{t.tutor.exam}</label>
                    <select
                      value={formExam || ''}
                      onChange={e => {
                        const tp = topics.find(t2 => t2.examTypeCode === e.target.value);
                        if (tp) setForm(prev => ({ ...prev, topicId: tp.id }));
                        setFormSectionId(0);
                      }}
                      style={{ width: '100%', padding: '0.5rem', borderRadius: '8px', border: '1px solid var(--border-color)', background: 'var(--card-bg)', color: 'var(--text-primary)' }}
                    >
                      <option value="">{t.tutor.allExams}</option>
                      {examCodes.map(c => <option key={c} value={c}>{c}</option>)}
                    </select>
                  </div>
                  <div style={{ flex: 1, minWidth: '120px' }}>
                    <label style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>{t.tutor.section}</label>
                    <select
                      value={formSectionId || ''}
                      onChange={e => {
                        setFormSectionId(Number(e.target.value));
                      }}
                      style={{ width: '100%', padding: '0.5rem', borderRadius: '8px', border: '1px solid var(--border-color)', background: 'var(--card-bg)', color: 'var(--text-primary)' }}
                    >
                      <option value="">{t.tutor.allSections}</option>
                      {formSections.map(s => <option key={s} value={topics.find(tp => tp.sectionName === s)?.id || ''}>{s}</option>)}
                    </select>
                  </div>
                </div>

                {/* Topic */}
                <div style={{ marginBottom: '0.75rem' }}>
                  <label style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>{t.tutor.topic} *</label>
                  <select
                    value={form.topicId || ''}
                    onChange={e => setForm(prev => ({ ...prev, topicId: Number(e.target.value) }))}
                    style={{ width: '100%', padding: '0.5rem', borderRadius: '8px', border: '1px solid var(--border-color)', background: 'var(--card-bg)', color: 'var(--text-primary)' }}
                  >
                    <option value="">{t.tutor.selectTopic}</option>
                    {(formExam ? topics.filter(tp => tp.examTypeCode === formExam) : topics).map(tp => (
                      <option key={tp.id} value={tp.id}>{tp.name} ({tp.sectionName})</option>
                    ))}
                  </select>
                </div>

                {/* Difficulty */}
                <div style={{ marginBottom: '0.75rem' }}>
                  <label style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>{t.tutor.difficulty}</label>
                  <select
                    value={form.difficulty}
                    onChange={e => setForm(prev => ({ ...prev, difficulty: e.target.value }))}
                    style={{ width: '100%', padding: '0.5rem', borderRadius: '8px', border: '1px solid var(--border-color)', background: 'var(--card-bg)', color: 'var(--text-primary)' }}
                  >
                    <option value="Easy">Easy</option>
                    <option value="Medium">Medium</option>
                    <option value="Hard">Hard</option>
                  </select>
                </div>

                {/* Question text */}
                <div style={{ marginBottom: '0.75rem' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.25rem' }}>
                    <label style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{t.tutor.questionText} *</label>
                    <button
                      className="btn btn-sm"
                      style={{ fontSize: '0.7rem', padding: '0.15rem 0.4rem' }}
                      onClick={() => { setMathTarget('text'); setShowMathKeyboard(!showMathKeyboard); }}
                    >∑ Math</button>
                  </div>
                  <textarea
                    value={form.text}
                    onChange={e => setForm(prev => ({ ...prev, text: e.target.value }))}
                    rows={4}
                    style={{ width: '100%', padding: '0.5rem', borderRadius: '8px', border: '1px solid var(--border-color)', resize: 'vertical', fontFamily: 'inherit', background: 'var(--card-bg)', color: 'var(--text-primary)' }}
                  />
                </div>

                {/* Math keyboard */}
                {showMathKeyboard && (
                  <div style={{ marginBottom: '0.75rem', padding: '0.75rem', background: 'var(--bg-secondary)', borderRadius: '8px' }}>
                    <div style={{ display: 'flex', gap: '0.25rem', marginBottom: '0.5rem', flexWrap: 'wrap' }}>
                      {Object.keys(MATH_SYMBOLS).map(tab => (
                        <button key={tab} className="btn btn-sm" style={{ fontSize: '0.7rem', padding: '0.2rem 0.4rem', background: tab === mathTab ? 'var(--primary-color)' : 'var(--card-bg)', color: tab === mathTab ? '#fff' : 'var(--text-primary)' }} onClick={() => setMathTab(tab)}>{tab}</button>
                      ))}
                    </div>
                    <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.25rem' }}>
                      {MATH_SYMBOLS[mathTab]?.map(sym => (
                        <button key={sym} onClick={() => insertMathSymbol(sym)} style={{ width: '36px', height: '36px', border: '1px solid var(--border-color)', borderRadius: '6px', background: 'var(--card-bg)', color: 'var(--text-primary)', fontSize: '1rem', cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>{sym}</button>
                      ))}
                    </div>
                    <div style={{ marginTop: '0.5rem', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                      {t.tutor.mathTarget}: {' '}
                      <button className="btn btn-sm" style={{ fontSize: '0.65rem', padding: '0.1rem 0.3rem', background: mathTarget === 'text' ? 'var(--primary-color)' : 'transparent', color: mathTarget === 'text' ? '#fff' : 'var(--text-primary)' }} onClick={() => setMathTarget('text')}>{t.tutor.questionText}</button>
                      <button className="btn btn-sm" style={{ fontSize: '0.65rem', padding: '0.1rem 0.3rem', marginLeft: '0.25rem', background: mathTarget === 'explanation' ? 'var(--primary-color)' : 'transparent', color: mathTarget === 'explanation' ? '#fff' : 'var(--text-primary)' }} onClick={() => setMathTarget('explanation')}>{t.tutor.explanation}</button>
                      {form.answerOptions.map((_, i) => (
                        <button key={i} className="btn btn-sm" style={{ fontSize: '0.65rem', padding: '0.1rem 0.3rem', marginLeft: '0.25rem', background: mathTarget === i ? 'var(--primary-color)' : 'transparent', color: mathTarget === i ? '#fff' : 'var(--text-primary)' }} onClick={() => setMathTarget(i)}>{String.fromCharCode(65 + i)}</button>
                      ))}
                    </div>
                  </div>
                )}

                {/* Answer options */}
                <div style={{ marginBottom: '0.75rem' }}>
                  <label style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>{t.tutor.answerOptions} *</label>
                  {form.answerOptions.map((opt, i) => (
                    <div key={i} style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', marginBottom: '0.25rem' }}>
                      <input
                        type="radio"
                        name="correct"
                        checked={opt.isCorrect}
                        onChange={() => updateOption(i, 'isCorrect', true)}
                        title={t.tutor.markCorrect}
                      />
                      <input
                        value={opt.text}
                        onChange={e => updateOption(i, 'text', e.target.value)}
                        placeholder={`${t.tutor.option} ${String.fromCharCode(65 + i)}`}
                        style={{ flex: 1, padding: '0.4rem 0.6rem', borderRadius: '6px', border: `1px solid ${opt.isCorrect ? 'var(--success-color)' : 'var(--border-color)'}`, background: 'var(--card-bg)', color: 'var(--text-primary)' }}
                      />
                      {form.answerOptions.length > 2 && (
                        <button onClick={() => removeOption(i)} style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--error-color)', fontSize: '1.1rem' }}>✕</button>
                      )}
                    </div>
                  ))}
                  {form.answerOptions.length < 6 && (
                    <button className="btn btn-sm" onClick={addOption} style={{ marginTop: '0.25rem', fontSize: '0.8rem' }}>+ {t.tutor.addOption}</button>
                  )}
                </div>

                {/* Image URL */}
                <div style={{ marginBottom: '0.75rem' }}>
                  <label style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>URL изображения (необязательно)</label>
                  <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                    <input
                      type="url"
                      value={form.imageUrl}
                      onChange={e => setForm(prev => ({ ...prev, imageUrl: e.target.value }))}
                      placeholder="https://..."
                      style={{ flex: 1, padding: '0.5rem', borderRadius: '8px', border: '1px solid var(--border-color)', background: 'var(--card-bg)', color: 'var(--text-primary)' }}
                    />
                    <label style={{ cursor: 'pointer', whiteSpace: 'nowrap' }}>
                      <input
                        type="file"
                        accept="image/jpeg,image/png,image/webp,image/gif"
                        style={{ display: 'none' }}
                        onChange={async e => {
                          const file = e.target.files?.[0];
                          if (!file) return;
                          setUploadingImage(true);
                          try {
                            const url = await tutorService.uploadImage(file);
                            setForm(prev => ({ ...prev, imageUrl: url }));
                          } catch {
                            setError('Не удалось загрузить изображение');
                          } finally {
                            setUploadingImage(false);
                            e.target.value = '';
                          }
                        }}
                      />
                      <span className="btn btn-secondary" style={{ pointerEvents: 'none', fontSize: '0.85rem' }}>
                        {uploadingImage ? 'Загрузка...' : 'С диска'}
                      </span>
                    </label>
                  </div>
                  {form.imageUrl && (
                    <img src={form.imageUrl} alt="preview" style={{ marginTop: '0.5rem', maxWidth: '100%', maxHeight: '200px', borderRadius: '8px', objectFit: 'contain' }} />
                  )}
                </div>

                {/* Explanation */}
                <div style={{ marginBottom: '1rem' }}>
                  <label style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>{t.tutor.explanation}</label>
                  <textarea
                    value={form.explanation}
                    onChange={e => setForm(prev => ({ ...prev, explanation: e.target.value }))}
                    rows={3}
                    style={{ width: '100%', padding: '0.5rem', borderRadius: '8px', border: '1px solid var(--border-color)', resize: 'vertical', fontFamily: 'inherit', background: 'var(--card-bg)', color: 'var(--text-primary)' }}
                    placeholder={t.tutor.explanationPlaceholder}
                  />
                </div>

                {/* Actions */}
                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.5rem' }}>
                  <button className="btn btn-sm" onClick={() => { setModalMode(selected ? 'view' : 'view'); if (!selected) { setSelected(null); } setError(null); }}>{t.tutor.cancel}</button>
                  <button className="btn btn-primary" onClick={saveQuestion}>{modalMode === 'create' ? t.tutor.create : t.tutor.save}</button>
                </div>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

export default TutorQuestionsPage;
