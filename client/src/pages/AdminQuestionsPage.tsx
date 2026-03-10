import { useEffect, useState, useCallback } from 'react';
import adminService from '../services/adminService';
import type { QuestionListItem, QuestionDetail, AdminTopicSummary, AdminSection, AdminSkill } from '../types';

const EXAM_COLORS: Record<string, string> = {
  SAT: '#4f46e5',
  TOEFL: '#0891b2',
  NUET: '#7c3aed',
  IELTS: '#059669',
  CSCA: '#dc2626',
};

const DIFF_COLORS: Record<string, string> = {
  Easy: 'var(--success-color)',
  Medium: 'var(--warning-color)',
  Hard: 'var(--error-color)',
};

type ViewMode = 'table' | 'topics';
type ModalMode = 'view' | 'edit' | 'create';

const emptyForm = {
  topicId: 0,
  text: '',
  difficulty: 'Medium',
  explanation: '',
  answerOptions: [
    { text: '', isCorrect: true },
    { text: '', isCorrect: false },
    { text: '', isCorrect: false },
    { text: '', isCorrect: false },
  ],
};

function AdminQuestionsPage() {
  const [questions, setQuestions] = useState<QuestionListItem[]>([]);
  const [topics, setTopics] = useState<AdminTopicSummary[]>([]);
  const [selected, setSelected] = useState<QuestionDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [filterExam, setFilterExam] = useState('');
  const [filterDiff, setFilterDiff] = useState('');
  const [filterTopic, setFilterTopic] = useState('');
  const [filterSection, setFilterSection] = useState('');

  const [viewMode, setViewMode] = useState<ViewMode>('table');
  const [modalMode, setModalMode] = useState<ModalMode>('view');
  const [form, setForm] = useState(emptyForm);

  // Pagination (OP-13)
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // Topic view — independent state
  const [topicPage, setTopicPage] = useState(1);
  const TOPICS_PER_PAGE = 10;
  const [topicViewQuestions, setTopicViewQuestions] = useState<QuestionListItem[]>([]);
  const [topicViewLoading, setTopicViewLoading] = useState(false);
  const [topicFilterExam, setTopicFilterExam] = useState('');
  const [topicFilterTopic, setTopicFilterTopic] = useState('');
  const [topicFilterSection, setTopicFilterSection] = useState('');

  // Topic creation
  const [showTopicModal, setShowTopicModal] = useState(false);
  const [sections, setSections] = useState<AdminSection[]>([]);
  const [skills, setSkills] = useState<AdminSkill[]>([]);
  const [topicForm, setTopicForm] = useState({ name: '', sectionId: 0, skillId: 0 });

  // ─── Load ────────────────────

  const loadQuestions = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const result = await adminService.getQuestions(filterExam || undefined, filterTopic || undefined, filterDiff || undefined, page, 50);
      setQuestions(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch {
      setError('Ошибка загрузки');
    } finally {
      setIsLoading(false);
    }
  }, [filterExam, filterDiff, filterTopic, page]);

  const loadTopics = async () => {
    try {
      const t = await adminService.getTopics();
      setTopics(t);
    } catch { /* ignore */ }
  };

  const loadSectionsAndSkills = async () => {
    try {
      const [s, sk] = await Promise.all([adminService.getSections(), adminService.getSkills()]);
      setSections(s);
      setSkills(sk);
    } catch { /* ignore */ }
  };

  useEffect(() => { loadQuestions(); }, [loadQuestions]);
  useEffect(() => { loadTopics(); loadSectionsAndSkills(); }, []);

  // Load topic view questions independently
  const loadTopicViewQuestions = useCallback(async () => {
    try {
      setTopicViewLoading(true);
      const result = await adminService.getQuestions(topicFilterExam || undefined, topicFilterTopic || undefined, undefined, 1, 1000);
      setTopicViewQuestions(result.items);
    } catch { /* ignore */ } finally {
      setTopicViewLoading(false);
    }
  }, [topicFilterExam, topicFilterTopic]);

  useEffect(() => {
    if (viewMode === 'topics') loadTopicViewQuestions();
  }, [viewMode, loadTopicViewQuestions]);

  // ─── Topic Actions ───────────

  const openTopicModal = () => {
    setTopicForm({ name: '', sectionId: 0, skillId: 0 });
    setError(null);
    if (sections.length === 0) loadSectionsAndSkills();
    setShowTopicModal(true);
  };

  const saveTopic = async () => {
    if (!topicForm.name.trim()) { setError('Введите название темы'); return; }
    if (!topicForm.sectionId) { setError('Выберите секцию экзамена'); return; }
    if (!topicForm.skillId) { setError('Выберите навык'); return; }
    try {
      setError(null);
      await adminService.createTopic(topicForm);
      setShowTopicModal(false);
      setSuccess('Тема создана');
      loadTopics();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Ошибка создания темы';
      setError(msg);
    }
  };

  // ─── Actions ─────────────────

  const openDetail = async (id: number) => {
    try {
      const detail = await adminService.getQuestion(id);
      setSelected(detail);
      setModalMode('view');
      setSuccess(null);
    } catch {
      setError('Ошибка загрузки вопроса');
    }
  };

  const startEdit = () => {
    if (!selected) return;
    setForm({
      topicId: selected.topicId,
      text: selected.text,
      difficulty: selected.difficulty,
      explanation: selected.explanation || '',
      answerOptions: selected.answerOptions.map(o => ({ text: o.text, isCorrect: o.isCorrect })),
    });
    setModalMode('edit');
    setSuccess(null);
  };

  const startCreate = (preselectedTopicId?: number) => {
    setSelected(null);
    setForm({
      ...emptyForm,
      topicId: preselectedTopicId || (topics.length > 0 ? topics[0].id : 0),
      answerOptions: [
        { text: '', isCorrect: true },
        { text: '', isCorrect: false },
        { text: '', isCorrect: false },
        { text: '', isCorrect: false },
      ],
    });
    setModalMode('create');
    setSuccess(null);
    setError(null);
  };

  const saveEdit = async () => {
    if (!selected) return;
    try {
      setError(null);
      const updated = await adminService.updateQuestion(selected.id, {
        text: form.text,
        difficulty: form.difficulty,
        explanation: form.explanation || undefined,
        answerOptions: form.answerOptions.filter(o => o.text.trim()),
      });
      setSelected(updated);
      setModalMode('view');
      setSuccess('Вопрос обновлён');
      loadQuestions();
      if (viewMode === 'topics') loadTopicViewQuestions();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Ошибка сохранения';
      setError(msg);
    }
  };

  const saveCreate = async () => {
    const validOptions = form.answerOptions.filter(o => o.text.trim());
    if (!form.text.trim()) { setError('Введите текст вопроса'); return; }
    if (validOptions.length < 2) { setError('Минимум 2 варианта ответа'); return; }
    if (!validOptions.some(o => o.isCorrect)) { setError('Отметьте правильный ответ'); return; }
    if (!form.topicId) { setError('Выберите тему'); return; }

    try {
      setError(null);
      const created = await adminService.createQuestion({
        topicId: form.topicId,
        text: form.text,
        difficulty: form.difficulty,
        explanation: form.explanation || undefined,
        answerOptions: validOptions,
      });
      setSelected(created);
      setModalMode('view');
      setSuccess('Вопрос создан');
      loadQuestions();
      loadTopics();
      if (viewMode === 'topics') loadTopicViewQuestions();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Ошибка создания';
      setError(msg);
    }
  };

  const deleteQuestion = async (id: number) => {
    if (!confirm('Удалить вопрос? Это действие необратимо.')) return;
    try {
      await adminService.deleteQuestion(id);
      setSelected(null);
      setModalMode('view');
      setSuccess('Вопрос удалён');
      loadQuestions();
      loadTopics();
      if (viewMode === 'topics') loadTopicViewQuestions();
    } catch {
      setError('Ошибка удаления');
    }
  };

  const closeModal = () => {
    setSelected(null);
    setModalMode('view');
  };

  // ─── Form helpers ────────────

  const updateOption = (idx: number, field: 'text' | 'isCorrect', value: string | boolean) => {
    setForm(prev => ({
      ...prev,
      answerOptions: prev.answerOptions.map((o, i) => {
        if (field === 'isCorrect') {
          return { ...o, isCorrect: i === idx };
        }
        if (field === 'text' && i === idx) {
          return { ...o, text: value as string };
        }
        return o;
      }),
    }));
  };

  const addOption = () => {
    if (form.answerOptions.length >= 6) return;
    setForm(prev => ({
      ...prev,
      answerOptions: [...prev.answerOptions, { text: '', isCorrect: false }],
    }));
  };

  const removeOption = (idx: number) => {
    if (form.answerOptions.length <= 2) return;
    setForm(prev => ({
      ...prev,
      answerOptions: prev.answerOptions.filter((_, i) => i !== idx),
    }));
  };

  // ─── Group by topic (from full topics list, not just questions) ──────────

  const groupedByTopic = (() => {
    // Use all topics (including those with 0 questions)
    let filtered = topics;
    if (topicFilterExam) filtered = filtered.filter(t => t.examTypeCode === topicFilterExam);
    if (topicFilterSection) filtered = filtered.filter(t => t.sectionName === topicFilterSection);
    if (topicFilterTopic) {
      const q = topicFilterTopic.toLowerCase();
      filtered = filtered.filter(t => t.name.toLowerCase().includes(q));
    }

    // Build questions lookup by topic name + exam
    const qMap = new Map<string, QuestionListItem[]>();
    for (const q of topicViewQuestions) {
      const key = `${q.examTypeCode}|${q.topicName}`;
      if (!qMap.has(key)) qMap.set(key, []);
      qMap.get(key)!.push(q);
    }

    // Natural numeric sort: "1.1.1" < "1.1.2" < "2.1.1" < "10.1.1", "P1.1.1" groups after numbers
    return filtered
      .map(t => ({
        topic: t,
        questions: qMap.get(`${t.examTypeCode}|${t.name}`) ?? [],
      }))
      .sort((a, b) => {
        // Sort by exam first
        const examCmp = a.topic.examTypeCode.localeCompare(b.topic.examTypeCode);
        if (examCmp !== 0) return examCmp;
        // Natural numeric sort on topic name
        const na = a.topic.name.match(/^[P]?(\d+)/);
        const nb = b.topic.name.match(/^[P]?(\d+)/);
        if (na && nb) {
          const diff = parseInt(na[1]) - parseInt(nb[1]);
          if (diff !== 0) return diff;
        }
        return a.topic.name.localeCompare(b.topic.name);
      });
  })();

  // ─── Render ──────────────────

  if (isLoading && questions.length === 0) {
    return (
      <div className="animate-fade-in" style={{ textAlign: 'center', padding: '4rem 1rem' }}>
        <div className="loading-spinner" />
        <p style={{ color: 'var(--text-secondary)', marginTop: '1rem' }}>Загрузка…</p>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem', flexWrap: 'wrap', gap: '0.75rem' }}>
        <div>
          <h1 style={{ margin: 0 }}>Управление вопросами</h1>
          <p style={{ color: 'var(--text-secondary)', margin: '0.25rem 0 0' }}>
            {totalCount} вопросов • {topics.length} тем{totalPages > 1 ? ` • стр. ${page}/${totalPages}` : ''}
          </p>
        </div>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button className="btn btn-primary" onClick={() => startCreate()} style={{ fontSize: '0.9rem' }}>
            Новый вопрос
          </button>
          <button className="btn btn-outline" onClick={openTopicModal} style={{ fontSize: '0.9rem' }}>
            Новая тема
          </button>
          <button className="btn btn-outline" onClick={() => adminService.exportQuestionsCsv(filterExam || undefined, filterDiff || undefined)} style={{ fontSize: '0.9rem' }}>
            CSV
          </button>
        </div>
      </div>

      {error && (
        <div style={{ color: 'var(--error-color)', marginBottom: '1rem', padding: '0.6rem 1rem', background: 'var(--error-bg)', borderRadius: '8px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <span>{error}</span>
          <button style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--error-color)', fontSize: '1rem' }} onClick={() => setError(null)}>✕</button>
        </div>
      )}
      {success && (
        <div style={{ color: 'var(--success-color)', marginBottom: '1rem', padding: '0.6rem 1rem', background: 'rgba(16,185,129,0.08)', borderRadius: '8px' }}>
          ✓ {success}
        </div>
      )}

      {/* View Toggle */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1rem', flexWrap: 'wrap', alignItems: 'center' }}>
        <div style={{ display: 'flex', borderRadius: '8px', overflow: 'hidden', border: '1px solid var(--border-color)' }}>
          <button
            onClick={() => setViewMode('table')}
            style={{
              padding: '0.4rem 0.75rem', border: 'none', cursor: 'pointer', fontSize: '0.85rem',
              background: viewMode === 'table' ? 'var(--primary-color)' : 'var(--card-background)',
              color: viewMode === 'table' ? '#fff' : 'var(--text-secondary)',
            }}
          >Таблица</button>
          <button
            onClick={() => setViewMode('topics')}
            style={{
              padding: '0.4rem 0.75rem', border: 'none', cursor: 'pointer', fontSize: '0.85rem',
              borderLeft: '1px solid var(--border-color)',
              background: viewMode === 'topics' ? 'var(--primary-color)' : 'var(--card-background)',
              color: viewMode === 'topics' ? '#fff' : 'var(--text-secondary)',
            }}
          >По темам</button>
        </div>
      </div>

      {/* Table Filters */}
      {viewMode === 'table' && (
        <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.25rem', flexWrap: 'wrap', alignItems: 'center' }}>
          <select value={filterExam} onChange={e => { setFilterExam(e.target.value); setFilterSection(''); setPage(1); }} style={{ padding: '0.5rem' }}>
            <option value="">Все экзамены</option>
            <option value="SAT">SAT</option>
            <option value="TOEFL">TOEFL</option>
            <option value="NUET">NUET</option>
            <option value="IELTS">IELTS</option>
            <option value="CSCA">CSCA</option>
          </select>
          <select value={filterSection} onChange={e => { setFilterSection(e.target.value); setPage(1); }} style={{ padding: '0.5rem' }}>
            <option value="">Все секции</option>
            {sections
              .filter(s => !filterExam || s.examTypeCode === filterExam)
              .map(s => <option key={s.id} value={s.name}>{s.name}</option>)}
          </select>
          <select value={filterDiff} onChange={e => { setFilterDiff(e.target.value); setPage(1); }} style={{ padding: '0.5rem' }}>
            <option value="">Все уровни</option>
            <option value="Easy">Easy</option>
            <option value="Medium">Medium</option>
            <option value="Hard">Hard</option>
          </select>
          <input
            placeholder="Поиск по теме…"
            value={filterTopic}
            onChange={e => { setFilterTopic(e.target.value); setPage(1); }}
            className="form-input"
            style={{ flex: 1, minWidth: '140px' }}
          />
        </div>
      )}

      {/* Topic View Filters */}
      {viewMode === 'topics' && (
        <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.25rem', flexWrap: 'wrap', alignItems: 'center' }}>
          <select value={topicFilterExam} onChange={e => { setTopicFilterExam(e.target.value); setTopicFilterSection(''); setTopicPage(1); }} style={{ padding: '0.5rem' }}>
            <option value="">Все экзамены</option>
            <option value="SAT">SAT</option>
            <option value="TOEFL">TOEFL</option>
            <option value="NUET">NUET</option>
            <option value="IELTS">IELTS</option>
            <option value="CSCA">CSCA</option>
          </select>
          <select value={topicFilterSection} onChange={e => { setTopicFilterSection(e.target.value); setTopicPage(1); }} style={{ padding: '0.5rem' }}>
            <option value="">Все секции</option>
            {sections
              .filter(s => !topicFilterExam || s.examTypeCode === topicFilterExam)
              .map(s => <option key={s.id} value={s.name}>{s.name}</option>)}
          </select>
          <input
            placeholder="Поиск по теме…"
            value={topicFilterTopic}
            onChange={e => { setTopicFilterTopic(e.target.value); setTopicPage(1); }}
            className="form-input"
            style={{ flex: 1, minWidth: '140px' }}
          />
        </div>
      )}

      {/* ─── TABLE VIEW ─── */}
      {viewMode === 'table' && (
        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
            <thead>
              <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                <th style={thStyle}>ID</th>
                <th style={thStyle}>Экзамен</th>
                <th style={thStyle}>Секция / Тема</th>
                <th style={thStyle}>Вопрос</th>
                <th style={thStyle}>Уровень</th>
                <th style={thStyle}>Ответы</th>
                <th style={thStyle}>b</th>
              </tr>
            </thead>
            <tbody>
              {(filterSection ? questions.filter(q => q.sectionName === filterSection) : questions).map(q => (
                <tr key={q.id}
                  onClick={() => openDetail(q.id)}
                  style={{ borderBottom: '1px solid var(--border-color)', cursor: 'pointer', transition: 'background 0.15s' }}
                  onMouseEnter={e => e.currentTarget.style.background = 'var(--bg-secondary)'}
                  onMouseLeave={e => e.currentTarget.style.background = ''}
                >
                  <td style={tdStyle}><span style={{ color: 'var(--text-muted, var(--text-secondary))' }}>#{q.id}</span></td>
                  <td style={tdStyle}>
                    <Badge bg={EXAM_COLORS[q.examTypeCode]}>{q.examTypeCode}</Badge>
                  </td>
                  <td style={tdStyle}>
                    <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{q.sectionName}</div>
                    <div style={{ fontWeight: 500 }}>{q.topicName}</div>
                  </td>
                  <td style={{ ...tdStyle, maxWidth: '320px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    {q.text}
                  </td>
                  <td style={tdStyle}>
                    <span style={{ color: DIFF_COLORS[q.difficulty] || 'inherit', fontWeight: 600 }}>{q.difficulty}</span>
                  </td>
                  <td style={{ ...tdStyle, textAlign: 'center' }}>{q.answerCount}</td>
                  <td style={{ ...tdStyle, color: 'var(--text-secondary)' }}>{q.difficultyParam}</td>
                </tr>
              ))}
            </tbody>
          </table>
          {questions.length === 0 && !isLoading && (
            <div style={{ textAlign: 'center', padding: '2.5rem', color: 'var(--text-secondary)' }}>
              Нет вопросов по выбранным фильтрам
            </div>
          )}
          {/* Pagination (OP-13) */}
          {totalPages > 1 && (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '0.5rem', padding: '1rem 0' }}>
              <button className="btn btn-outline" disabled={page <= 1} onClick={() => setPage(1)} style={{ fontSize: '0.85rem' }}>«</button>
              <button className="btn btn-outline" disabled={page <= 1} onClick={() => setPage(p => p - 1)} style={{ fontSize: '0.85rem' }}>‹</button>
              <span style={{ padding: '0 0.75rem', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
                {page} / {totalPages}
              </span>
              <button className="btn btn-outline" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)} style={{ fontSize: '0.85rem' }}>›</button>
              <button className="btn btn-outline" disabled={page >= totalPages} onClick={() => setPage(totalPages)} style={{ fontSize: '0.85rem' }}>»</button>
            </div>
          )}
        </div>
      )}

      {/* ─── TOPICS VIEW ─── */}
      {viewMode === 'topics' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          {topicViewLoading && (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
              <div className="loading-spinner" style={{ margin: '0 auto 0.5rem' }} />
              Загрузка тем…
            </div>
          )}
          {groupedByTopic.length === 0 && !topicViewLoading && (
            <div style={{ textAlign: 'center', padding: '2.5rem', color: 'var(--text-secondary)' }}>
              Нет тем по выбранным фильтрам
            </div>
          )}
          {(() => {
            const topicTotalPages = Math.ceil(groupedByTopic.length / TOPICS_PER_PAGE);
            const sliced = groupedByTopic.slice(
              (topicPage - 1) * TOPICS_PER_PAGE,
              topicPage * TOPICS_PER_PAGE
            );
            return (
              <>
                {groupedByTopic.length > 0 && (
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                    Показано {sliced.length} из {groupedByTopic.length} тем (стр. {topicPage}/{topicTotalPages})
                  </div>
                )}
                {sliced.map(({ topic: t, questions: qs }) => (
                    <div key={`${t.examTypeCode}-${t.id}`} className="card" style={{ padding: '1rem 1.25rem' }}>
                      {/* Topic header */}
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: qs.length > 0 ? '0.75rem' : 0 }}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
                          <Badge bg={EXAM_COLORS[t.examTypeCode]}>{t.examTypeCode}</Badge>
                          <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{t.sectionName} →</span>
                          <span style={{ fontWeight: 600, fontSize: '1rem' }}>{t.name}</span>
                          <span style={{ color: t.questionCount > 0 ? 'var(--text-secondary)' : 'var(--error-color)', fontSize: '0.8rem' }}>
                            ({t.questionCount} {t.questionCount === 0 ? 'нет вопросов' : `вопр.`})
                          </span>
                        </div>
                        <button
                          className="btn btn-outline"
                          style={{ fontSize: '0.8rem', padding: '0.3rem 0.7rem' }}
                          onClick={() => startCreate(t.id)}
                        >
                          Добавить
                        </button>
                      </div>
                      {/* Questions list */}
                      {qs.length > 0 && (
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
                          {qs.map(q => (
                          <div
                            key={q.id}
                            onClick={() => openDetail(q.id)}
                            style={{
                              display: 'flex', alignItems: 'center', gap: '0.6rem',
                              padding: '0.5rem 0.6rem', borderRadius: '6px', cursor: 'pointer',
                              transition: 'background 0.15s',
                              border: '1px solid var(--border-color)',
                            }}
                            onMouseEnter={e => e.currentTarget.style.background = 'var(--bg-secondary)'}
                            onMouseLeave={e => e.currentTarget.style.background = ''}
                          >
                            <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem', minWidth: '2.5rem' }}>#{q.id}</span>
                            <DiffDot diff={q.difficulty} />
                            <span style={{ flex: 1, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', fontSize: '0.9rem' }}>
                              {q.text}
                            </span>
                            <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{q.answerCount} вар.</span>
                          </div>
                        ))}
                      </div>
                      )}
                    </div>
                ))}
                {/* Topic Pagination */}
                {topicTotalPages > 1 && (
                  <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '0.5rem', padding: '1rem 0' }}>
                    <button className="btn btn-outline" disabled={topicPage <= 1} onClick={() => setTopicPage(1)} style={{ fontSize: '0.85rem' }}>«</button>
                    <button className="btn btn-outline" disabled={topicPage <= 1} onClick={() => setTopicPage(p => p - 1)} style={{ fontSize: '0.85rem' }}>‹</button>
                    <span style={{ padding: '0 0.75rem', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
                      {topicPage} / {topicTotalPages}
                    </span>
                    <button className="btn btn-outline" disabled={topicPage >= topicTotalPages} onClick={() => setTopicPage(p => p + 1)} style={{ fontSize: '0.85rem' }}>›</button>
                    <button className="btn btn-outline" disabled={topicPage >= topicTotalPages} onClick={() => setTopicPage(topicTotalPages)} style={{ fontSize: '0.85rem' }}>»</button>
                  </div>
                )}
              </>
            );
          })()}
        </div>
      )}

      {/* ═══ MODAL: View / Edit / Create ═══ */}
      {(selected || modalMode === 'create') && (
        <div
          style={{
            position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            zIndex: 1000, padding: '1rem',
          }}
          onClick={closeModal}
        >
          <div
            className="card"
            style={{ maxWidth: '740px', width: '100%', maxHeight: '85vh', overflow: 'auto', padding: '2rem' }}
            onClick={e => e.stopPropagation()}
          >
            {/* ─── VIEW MODE ─── */}
            {modalMode === 'view' && selected && (
              <>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', flexWrap: 'wrap' }}>
                    <Badge bg={EXAM_COLORS[selected.examTypeCode]}>{selected.examTypeCode}</Badge>
                    <span style={{ color: DIFF_COLORS[selected.difficulty], fontWeight: 600 }}>{selected.difficulty}</span>
                    <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>• ID: {selected.id}</span>
                  </div>
                  <button onClick={closeModal} style={closeBtn}>✕</button>
                </div>

                <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '0.75rem' }}>
                  {selected.sectionName} → {selected.topicName}
                </div>

                <div style={{ fontSize: '1.05rem', fontWeight: 600, marginBottom: '1rem', lineHeight: 1.5 }}>
                  {selected.text}
                </div>

                <div style={{ marginBottom: '1rem' }}>
                  {selected.answerOptions.map((opt, i) => (
                    <div key={opt.id} style={{
                      padding: '0.55rem 0.75rem', marginBottom: '0.4rem', borderRadius: '0.5rem',
                      border: `1px solid ${opt.isCorrect ? 'var(--success-color)' : 'var(--border-color)'}`,
                      background: opt.isCorrect ? 'rgba(16,185,129,0.08)' : 'transparent',
                      display: 'flex', alignItems: 'center', gap: '0.5rem',
                    }}>
                      <span style={{ fontWeight: 600, color: 'var(--text-secondary)', minWidth: '1.2rem' }}>{String.fromCharCode(65 + i)}</span>
                      <span style={{ flex: 1 }}>{opt.text}</span>
                      {opt.isCorrect && <span style={{ color: 'var(--success-color)', fontWeight: 700 }}>✓</span>}
                    </div>
                  ))}
                </div>

                {selected.explanation && (
                  <div style={{
                    padding: '0.75rem', borderRadius: '0.5rem',
                    background: 'rgba(79,70,229,0.06)', border: '1px solid var(--primary-color)',
                    fontSize: '0.9rem', marginBottom: '1rem', lineHeight: 1.5,
                  }}>
                    <strong>Объяснение:</strong> {selected.explanation}
                  </div>
                )}

                <div style={{ display: 'flex', gap: '1rem', fontSize: '0.8rem', color: 'var(--text-secondary)', marginBottom: '1.25rem' }}>
                  <span>b = {selected.difficultyParam}</span>
                  <span>a = {selected.discriminationParam}</span>
                  <span>c = {selected.guessParam}</span>
                </div>

                <div style={{ display: 'flex', gap: '0.75rem' }}>
                  <button className="btn btn-primary" style={{ fontSize: '0.85rem' }} onClick={startEdit}>
                    Редактировать
                  </button>
                  <button className="btn btn-outline" style={{ fontSize: '0.85rem', color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                    onClick={() => deleteQuestion(selected.id)}>
                    Удалить
                  </button>
                </div>
              </>
            )}

            {/* ─── EDIT / CREATE MODE ─── */}
            {(modalMode === 'edit' || modalMode === 'create') && (
              <>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
                  <h2 style={{ margin: 0, fontSize: '1.15rem' }}>
                    {modalMode === 'create' ? 'Новый вопрос' : 'Редактирование'}
                  </h2>
                  <button onClick={closeModal} style={closeBtn}>✕</button>
                </div>

                {error && (
                  <div style={{ color: 'var(--error-color)', marginBottom: '1rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>
                    {error}
                  </div>
                )}

                <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                  {/* Topic selector (only for create) */}
                  {modalMode === 'create' && (
                    <FormField label="Тема">
                      <select
                        value={form.topicId}
                        onChange={e => setForm({ ...form, topicId: Number(e.target.value) })}
                        style={{ width: '100%' }}
                      >
                        <option value={0}>Выберите тему...</option>
                        {topics.map(t => (
                          <option key={t.id} value={t.id}>
                            [{t.examTypeCode}] {t.sectionName} → {t.name} ({t.questionCount} вопр.)
                          </option>
                        ))}
                      </select>
                    </FormField>
                  )}

                  {/* Question text */}
                  <FormField label="Текст вопроса">
                    <textarea
                      value={form.text}
                      onChange={e => setForm({ ...form, text: e.target.value })}
                      className="form-input"
                      rows={3}
                      style={{ width: '100%', resize: 'vertical' }}
                      placeholder="Введите текст вопроса..."
                    />
                  </FormField>

                  {/* Difficulty */}
                  <FormField label="Уровень сложности">
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                      {['Easy', 'Medium', 'Hard'].map(d => (
                        <button
                          key={d}
                          onClick={() => setForm({ ...form, difficulty: d })}
                          style={{
                            padding: '0.4rem 1rem', borderRadius: '999px', border: 'none', cursor: 'pointer',
                            fontWeight: 600, fontSize: '0.85rem', transition: 'all 0.15s',
                            background: form.difficulty === d ? DIFF_COLORS[d] : 'var(--bg-secondary)',
                            color: form.difficulty === d ? '#fff' : 'var(--text-secondary)',
                          }}
                        >
                          {d}
                        </button>
                      ))}
                    </div>
                  </FormField>

                  {/* Answer options */}
                  <FormField label="Варианты ответа (нажмите ● для выбора правильного)">
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
                      {form.answerOptions.map((opt, i) => (
                        <div key={i} style={{ display: 'flex', gap: '0.4rem', alignItems: 'center' }}>
                          <button
                            type="button"
                            onClick={() => updateOption(i, 'isCorrect', true)}
                            title={opt.isCorrect ? 'Правильный ответ' : 'Отметить как правильный'}
                            style={{
                              width: '28px', height: '28px', borderRadius: '50%', border: '2px solid',
                              borderColor: opt.isCorrect ? 'var(--success-color)' : 'var(--border-color)',
                              background: opt.isCorrect ? 'var(--success-color)' : 'transparent',
                              color: opt.isCorrect ? '#fff' : 'var(--text-secondary)',
                              cursor: 'pointer', display: 'flex', alignItems: 'center', justifyContent: 'center',
                              fontSize: '0.75rem', fontWeight: 700, flexShrink: 0,
                            }}
                          >
                            {opt.isCorrect ? '✓' : String.fromCharCode(65 + i)}
                          </button>
                          <input
                            className="form-input"
                            value={opt.text}
                            onChange={e => updateOption(i, 'text', e.target.value)}
                            placeholder={`Вариант ${String.fromCharCode(65 + i)}...`}
                            style={{ flex: 1 }}
                          />
                          {form.answerOptions.length > 2 && (
                            <button
                              type="button"
                              onClick={() => removeOption(i)}
                              style={{
                                background: 'none', border: 'none', cursor: 'pointer',
                                color: 'var(--error-color)', fontSize: '1.1rem', flexShrink: 0,
                              }}
                              title="Удалить вариант"
                            >✕</button>
                          )}
                        </div>
                      ))}
                      {form.answerOptions.length < 6 && (
                        <button
                          type="button"
                          onClick={addOption}
                          className="btn btn-outline"
                          style={{ fontSize: '0.8rem', padding: '0.3rem 0.7rem', alignSelf: 'flex-start', marginTop: '0.25rem' }}
                        >
                          + Вариант
                        </button>
                      )}
                    </div>
                  </FormField>

                  {/* Explanation */}
                  <FormField label="Объяснение (необязательно)">
                    <textarea
                      value={form.explanation}
                      onChange={e => setForm({ ...form, explanation: e.target.value })}
                      className="form-input"
                      rows={2}
                      style={{ width: '100%', resize: 'vertical' }}
                      placeholder="Объяснение правильного ответа..."
                    />
                  </FormField>

                  {/* Actions */}
                  <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem' }}>
                    <button className="btn btn-primary" onClick={modalMode === 'create' ? saveCreate : saveEdit} style={{ flex: 1 }}>
                      {modalMode === 'create' ? 'Создать вопрос' : 'Сохранить'}
                    </button>
                    <button className="btn btn-outline" onClick={modalMode === 'edit' ? () => setModalMode('view') : closeModal} style={{ flex: 1 }}>
                      Отмена
                    </button>
                  </div>
                </div>
              </>
            )}
          </div>
        </div>
      )}
      {/* ═══ TOPIC CREATION MODAL ═══ */}
      {showTopicModal && (
        <div
          style={{
            position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            zIndex: 1001, padding: '1rem',
          }}
          onClick={() => setShowTopicModal(false)}
        >
          <div
            className="card"
            style={{ maxWidth: '500px', width: '100%', padding: '2rem' }}
            onClick={e => e.stopPropagation()}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
              <h2 style={{ margin: 0, fontSize: '1.15rem' }}>Новая тема</h2>
              <button onClick={() => setShowTopicModal(false)} style={closeBtn}>✕</button>
            </div>

            {error && (
              <div style={{ color: 'var(--error-color)', marginBottom: '1rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>
                {error}
              </div>
            )}

            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <FormField label="Название темы">
                <input
                  className="form-input"
                  value={topicForm.name}
                  onChange={e => setTopicForm({ ...topicForm, name: e.target.value })}
                  placeholder="Например: Trigonometry"
                  style={{ width: '100%' }}
                  autoFocus
                />
              </FormField>

              <FormField label="Секция экзамена">
                <select
                  value={topicForm.sectionId}
                  onChange={e => setTopicForm({ ...topicForm, sectionId: Number(e.target.value) })}
                  style={{ width: '100%' }}
                >
                  <option value={0}>Выберите секцию...</option>
                  {sections.map(s => (
                    <option key={s.id} value={s.id}>
                      [{s.examTypeCode}] {s.name}
                    </option>
                  ))}
                </select>
              </FormField>

              <FormField label="Навык">
                <select
                  value={topicForm.skillId}
                  onChange={e => setTopicForm({ ...topicForm, skillId: Number(e.target.value) })}
                  style={{ width: '100%' }}
                >
                  <option value={0}>Выберите навык...</option>
                  {skills.map(s => (
                    <option key={s.id} value={s.id}>
                      {s.name}
                    </option>
                  ))}
                </select>
              </FormField>

              <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveTopic} style={{ flex: 1 }}>
                  Создать тему
                </button>
                <button className="btn btn-outline" onClick={() => setShowTopicModal(false)} style={{ flex: 1 }}>
                  Отмена
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ─── Shared sub-components ────────────────────────────────

function Badge({ bg, children }: { bg?: string; children: React.ReactNode }) {
  return (
    <span style={{
      padding: '0.15rem 0.5rem', borderRadius: '999px', fontSize: '0.75rem',
      fontWeight: 600, background: bg || '#666', color: '#fff', whiteSpace: 'nowrap',
    }}>
      {children}
    </span>
  );
}

function DiffDot({ diff }: { diff: string }) {
  return (
    <span style={{
      width: '8px', height: '8px', borderRadius: '50%', flexShrink: 0,
      background: DIFF_COLORS[diff] || 'var(--text-secondary)',
    }} title={diff} />
  );
}

function FormField({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <label style={{ display: 'block', marginBottom: '0.35rem', fontWeight: 600, fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
        {label}
      </label>
      {children}
    </div>
  );
}

const thStyle: React.CSSProperties = { padding: '0.75rem 0.5rem', fontSize: '0.8rem', textTransform: 'uppercase', letterSpacing: '0.03em', color: 'var(--text-secondary)' };
const tdStyle: React.CSSProperties = { padding: '0.6rem 0.5rem' };
const closeBtn: React.CSSProperties = { background: 'none', border: 'none', cursor: 'pointer', fontSize: '1.3rem', color: 'var(--text-secondary)', lineHeight: 1 };

export default AdminQuestionsPage;
