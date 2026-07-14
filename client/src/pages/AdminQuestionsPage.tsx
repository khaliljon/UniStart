import { useEffect, useState, useCallback } from 'react';
import adminService from '../services/adminService';
import type { QuestionListItem, QuestionDetail, AdminTopicSummary, AdminSection } from '../types';
import { useTranslation } from '../hooks/useTranslation';

const EXAM_COLORS: Record<string, string> = {
  SAT: '#4f46e5',
  NUET: '#7c3aed',
};

const DIFF_COLORS: Record<string, string> = {
  Easy: 'var(--success-color)',
  Medium: 'var(--warning-color)',
  Hard: 'var(--error-color)',
};

type ViewMode = 'table' | 'topics' | 'sections' | 'examTypes';
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
  // IRT advanced params (null = auto from difficulty)
  difficultyParam: null as number | null,
  discriminationParam: null as number | null,
  guessParam: null as number | null,
};

const IRT_PRESETS = [
  { label: 'Авто (Easy)',      b: -1.0, a: 0.8,  c: 0.25 },
  { label: 'Авто (Medium)',    b:  0.0, a: 1.0,  c: 0.25 },
  { label: 'Авто (Hard)',      b:  1.5, a: 1.2,  c: 0.25 },
  { label: 'Очень легкий',     b: -2.0, a: 0.7,  c: 0.25 },
  { label: 'Слабая дискр.',  b:  0.0, a: 0.5,  c: 0.33 },
  { label: 'Высокая дискр.', b:  1.0, a: 1.5,  c: 0.20 },
  { label: 'Очень сложный',   b:  2.5, a: 1.3,  c: 0.20 },
];

function AdminQuestionsPage() {
  const { t } = useTranslation();
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
  const [uploadingImage, setUploadingImage] = useState(false);

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
  const [topicForm, setTopicForm] = useState({ name: '', sectionId: 0 });

  // Section management
  const [showSectionModal, setShowSectionModal] = useState(false);
  const [sectionForm, setSectionForm] = useState({ name: '', examTypeCode: '' });
  const [editingSectionId, setEditingSectionId] = useState<number | null>(null);

  // Math keyboard
  const [showMathKeyboard, setShowMathKeyboard] = useState(false);
  const [mathTarget, setMathTarget] = useState<'text' | 'explanation' | number | null>(null);

  // Inline topic rename
  const [editingTopicId, setEditingTopicId] = useState<number | null>(null);
  const [editingTopicName, setEditingTopicName] = useState('');
  const [showTopicRenameModal, setShowTopicRenameModal] = useState(false);

  // Section view
  const [sectionPage, setSectionPage] = useState(1);
  const SECTIONS_PER_PAGE = 10;
  const [sectionFilterExam, setSectionFilterExam] = useState('');

  // Section selector in question edit/create
  const [formSectionId, setFormSectionId] = useState<number>(0);

  // Advanced IRT parameters toggle
  const [showIrtParams, setShowIrtParams] = useState(false);

  // Bulk selection (table view)
  const [selectedQuestions, setSelectedQuestions] = useState<Set<number>>(new Set());
  const [selectionMode, setSelectionMode] = useState(false);

  // Exam types CRUD
  const [examTypes, setExamTypes] = useState<{ code: string; name: string }[]>([]);
  const [examTypesLoading, setExamTypesLoading] = useState(false);
  const [showExamTypeForm, setShowExamTypeForm] = useState(false);
  const [examTypeForm, setExamTypeForm] = useState({ code: '', name: '' });
  const [editingExamTypeCode, setEditingExamTypeCode] = useState<string | null>(null);

  // ─── Load ────────────────────

  const loadQuestions = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const result = await adminService.getQuestions(filterExam || undefined, filterTopic || undefined, filterDiff || undefined, page, 50, filterSection || undefined);
      setQuestions(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch {
      setError(t.admin.common.loadError);
    } finally {
      setIsLoading(false);
    }
  }, [filterExam, filterDiff, filterTopic, filterSection, page]);

  const loadTopics = async () => {
    try {
      const tp = await adminService.getTopics();
      setTopics(tp);
    } catch { /* ignore */ }
  };

  const loadSectionsAndSkills = async () => {
    try {
      setSections(await adminService.getSections());
    } catch { /* ignore */ }
  };

  const loadExamTypes = async () => {
    setExamTypesLoading(true);
    try { setExamTypes(await adminService.getExamTypes()); }
    catch { setError(t.admin.common.loadError); }
    finally { setExamTypesLoading(false); }
  };

  const saveExamType = async () => {
    if (!examTypeForm.name.trim()) { setError('Введите название'); return; }
    try {
      if (editingExamTypeCode) {
        await adminService.updateExamType(editingExamTypeCode, { name: examTypeForm.name });
        setSuccess('Тип экзамена обновлён');
      } else {
        if (!examTypeForm.code.trim()) { setError('Введите код (напр. SAT)'); return; }
        await adminService.createExamType({ code: examTypeForm.code.trim().toUpperCase(), name: examTypeForm.name });
        setSuccess('Тип экзамена создан');
      }
      setShowExamTypeForm(false);
      setEditingExamTypeCode(null);
      setExamTypeForm({ code: '', name: '' });
      loadExamTypes();
      loadSectionsAndSkills();
    } catch { setError(t.admin.common.saveError || 'Ошибка сохранения'); }
  };

  const handleDeleteExamType = async (code: string) => {
    if (!confirm(`Удалить тип экзамена "${code}"? Все секции, темы и вопросы этого типа будут удалены.`)) return;
    try {
      await adminService.deleteExamType(code);
      setSuccess(`Тип экзамена ${code} удалён`);
      loadExamTypes();
      loadSectionsAndSkills();
    } catch { setError(t.admin.common.deleteError); }
  };

  useEffect(() => { loadQuestions(); }, [loadQuestions]);
  useEffect(() => { loadTopics(); loadSectionsAndSkills(); loadExamTypes(); }, []);

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
    if (viewMode === 'examTypes') loadExamTypes();
    setSelectedQuestions(new Set());
    setSelectionMode(false);
  }, [viewMode, loadTopicViewQuestions]);

  // ─── Inline Topic Rename ─────

  const startTopicRename = (topicId: number, currentName: string) => {
    setEditingTopicId(topicId);
    setEditingTopicName(currentName);
  };

  const saveTopicRename = async () => {
    if (!editingTopicId || !editingTopicName.trim()) return;
    try {
      await adminService.updateTopic(editingTopicId, { name: editingTopicName.trim() });
      setEditingTopicId(null);
      setSuccess(t.admin.questions.topicNameUpdated);
      loadTopics();
    } catch {
      setError(t.admin.questions.topicNameUpdateError);
    }
  };

  // ─── Topic Actions ───────────

  const openTopicModal = () => {
    setTopicForm({ name: '', sectionId: 0 });
    setError(null);
    if (sections.length === 0) loadSectionsAndSkills();
    setShowTopicModal(true);
  };

  const saveTopic = async () => {
    if (!topicForm.name.trim()) { setError(t.admin.questions.enterTopicName); return; }
    if (!topicForm.sectionId) { setError(t.admin.questions.selectSection); return; }
    try {
      setError(null);
      await adminService.createTopic(topicForm);
      setShowTopicModal(false);
      setSuccess(t.admin.questions.topicCreated);
      loadTopics();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.admin.questions.topicCreateError;
      setError(msg);
    }
  };

  // ─── Section Actions ─────────

  const openSectionModal = (section?: AdminSection) => {
    if (section) {
      setEditingSectionId(section.id);
      setSectionForm({ name: section.name, examTypeCode: section.examTypeCode });
    } else {
      setEditingSectionId(null);
      setSectionForm({ name: '', examTypeCode: '' });
    }
    setError(null);
    setShowSectionModal(true);
  };

  const saveSection = async () => {
    if (!sectionForm.name.trim()) { setError(t.admin.questions.enterSectionName); return; }
    try {
      setError(null);
      if (editingSectionId) {
        await adminService.updateSection(editingSectionId, { name: sectionForm.name });
        setSuccess(t.admin.questions.sectionUpdated);
      } else {
        if (!sectionForm.examTypeCode) { setError(t.admin.questions.selectExamType); return; }
        await adminService.createSection(sectionForm);
        setSuccess(t.admin.questions.sectionCreated);
      }
      setShowSectionModal(false);
      loadSectionsAndSkills();
      loadTopics();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error
        || (editingSectionId ? t.admin.questions.sectionUpdateError : t.admin.questions.sectionCreateError);
      setError(msg);
    }
  };

  // ─── Math keyboard ──────────

  const MATH_SYMBOLS: Record<string, string[]> = {
    '123': ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ',', '%', '='],
    '+-×÷': ['+', '−', '×', '÷', '±', '·', '/', '^', '(', ')', '[', ']', '{', '}'],
    'αβγ': ['α', 'β', 'γ', 'δ', 'θ', 'λ', 'μ', 'σ', 'π', 'ε', 'φ', 'ω', 'Σ', 'Δ', 'Ω', 'Φ'],
    '∈∪⊆': ['∈', '∉', '⊆', '⊇', '⊂', '⊃', '∪', '∩', '∅', '∀', '∃', '∄', 'ℝ', 'ℤ', 'ℕ', 'ℚ'],
    '∫∂√': ['∫', '∮', '∂', '∑', '∏', '√', '∛', '∞', 'lim', 'log', 'ln', '∇', '°', '′', '″', '‰'],
    '≤≥≠': ['<', '>', '≤', '≥', '≠', '≈', '≡', '≪', '≫', '⇒', '⇔', '¬', '∧', '∨', '→', '←'],
    'x²ₙ': ['¹', '²', '³', '⁴', '⁵', '⁻', '⁺', 'ⁿ', '⁰', '₀', '₁', '₂', '₃', '₄', 'ₙ', 'ₓ'],
    '½¾': ['½', '⅓', '¼', '⅕', '⅙', '⅛', '⅔', '¾', '⅖', '⅗', '⅘', '⅜', '⅝', '⅞'],
    '△∠⊥': ['∠', '⊥', '∥', '△', '□', '○', '⃗', '→', '↑', '↓', '↔', '⌊', '⌋', '⌈', '⌉', '∘'],
  };
  const [mathTab, setMathTab] = useState<string>('αβγ');

  const insertMathSymbol = (symbol: string) => {
    if (mathTarget === 'text') {
      setForm(prev => ({ ...prev, text: prev.text + symbol }));
    } else if (mathTarget === 'explanation') {
      setForm(prev => ({ ...prev, explanation: prev.explanation + symbol }));
    } else if (typeof mathTarget === 'number') {
      updateOption(mathTarget, 'text', form.answerOptions[mathTarget].text + symbol);
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
      setError(t.admin.questions.questionLoadError);
    }
  };

  const startEdit = () => {
    if (!selected) return;
    setForm({
      topicId: selected.topicId,
      text: selected.text,
      difficulty: selected.difficulty,
      explanation: selected.explanation || '',
      imageUrl: selected.imageUrl || '',
      answerOptions: selected.answerOptions.map(o => ({ text: o.text, isCorrect: o.isCorrect })),
      difficultyParam: selected.difficultyParam ?? null,
      discriminationParam: selected.discriminationParam ?? null,
      guessParam: selected.guessParam ?? null,
    });
    // Find section for the selected topic
    const tp = topics.find(t => t.id === selected.topicId);
    const sec = tp ? sections.find(s => s.name === tp.sectionName && s.examTypeCode === tp.examTypeCode) : null;
    setFormSectionId(sec?.id || 0);
    setShowIrtParams(false);
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
    setFormSectionId(0);
    setShowIrtParams(false);
    setModalMode('create');
    setSuccess(null);
    setError(null);
  };

  const saveEdit = async () => {
    if (!selected) return;
    try {
      setError(null);
      const updated = await adminService.updateQuestion(selected.id, {
        topicId: form.topicId !== selected.topicId ? form.topicId : undefined,
        text: form.text,
        difficulty: form.difficulty,
        explanation: form.explanation || undefined,
        imageUrl: form.imageUrl || undefined,
        difficultyParam: showIrtParams ? form.difficultyParam ?? undefined : undefined,
        discriminationParam: showIrtParams ? form.discriminationParam ?? undefined : undefined,
        guessParam: showIrtParams ? form.guessParam ?? undefined : undefined,
        answerOptions: form.answerOptions.filter(o => o.text.trim()),
      });
      setSelected(updated);
      setModalMode('view');
      setSuccess(t.admin.questions.questionUpdated);
      loadQuestions();
      if (viewMode === 'topics') loadTopicViewQuestions();
    } catch (err: unknown) {
      const data = (err as { response?: { data?: { error?: string; errors?: Record<string, string[]>; title?: string } } })?.response?.data;
      const msg = data?.error
        || (data?.errors ? Object.values(data.errors).flat().join('; ') : undefined)
        || data?.title
        || t.admin.questions.questionSaveError;
      setError(msg);
    }
  };

  const saveCreate = async () => {
    const validOptions = form.answerOptions.filter(o => o.text.trim());
    if (!form.text.trim()) { setError(t.admin.questions.enterQuestion); return; }
    if (validOptions.length < 2) { setError(t.admin.questions.minTwoOptions); return; }
    if (!validOptions.some(o => o.isCorrect)) { setError(t.admin.questions.markCorrectOption); return; }
    if (!form.topicId) { setError(t.admin.questions.selectTopic); return; }

    try {
      setError(null);
      const created = await adminService.createQuestion({
        topicId: form.topicId,
        text: form.text,
        difficulty: form.difficulty,
        explanation: form.explanation || undefined,
        imageUrl: form.imageUrl || undefined,
        difficultyParam: showIrtParams ? form.difficultyParam ?? undefined : undefined,
        discriminationParam: showIrtParams ? form.discriminationParam ?? undefined : undefined,
        guessParam: showIrtParams ? form.guessParam ?? undefined : undefined,
        answerOptions: validOptions,
      });
      setSelected(created);
      setModalMode('view');
      setSuccess(t.admin.questions.questionCreated);
      loadQuestions();
      loadTopics();
      if (viewMode === 'topics') loadTopicViewQuestions();
    } catch (err: unknown) {
      const data = (err as { response?: { data?: { error?: string; errors?: Record<string, string[]>; title?: string } } })?.response?.data;
      const msg = data?.error
        || (data?.errors ? Object.values(data.errors).flat().join('; ') : undefined)
        || data?.title
        || t.admin.questions.questionCreateError;
      setError(msg);
    }
  };

  const deleteQuestion = async (id: number) => {
    if (!confirm(t.admin.questions.questionDeleteConfirm)) return;
    try {
      await adminService.deleteQuestion(id);
      setSelected(null);
      setModalMode('view');
      setSuccess(t.admin.questions.questionDeleted);
      loadQuestions();
      loadTopics();
      if (viewMode === 'topics') loadTopicViewQuestions();
    } catch {
      setError(t.admin.questions.questionDeleteError);
    }
  };

  const closeModal = () => {
    setSelected(null);
    setModalMode('view');
  };

  const bulkDeleteQuestions = async () => {
    const ids = [...selectedQuestions];
    if (!ids.length || !confirm(`Удалить ${ids.length} вопросов? Это действие нельзя отменить.`)) return;
    try {
      await Promise.all(ids.map(id => adminService.deleteQuestion(id)));
      setSelectedQuestions(new Set());
      setSelectionMode(false);
      setSuccess(t.admin.questions.questionDeleted);
      loadQuestions();
      loadTopics();
      if (viewMode === 'topics') loadTopicViewQuestions();
    } catch { setError(t.admin.questions.questionDeleteError); }
  };

  const deleteTopic = async (topicId: number) => {
    if (!confirm('Удалить эту тему и все её вопросы? Это действие нельзя отменить.')) return;
    try {
      await adminService.deleteTopic(topicId);
      setSuccess('Тема удалена');
      loadTopics();
      if (viewMode === 'topics') loadTopicViewQuestions();
    } catch { setError(t.admin.common.deleteError); }
  };

  const clearTopicQuestions = async (topicId: number) => {
    if (!confirm('Удалить все вопросы этой темы? Сама тема останется. Это действие нельзя отменить.')) return;
    try {
      const { removed } = await adminService.clearTopicQuestions(topicId);
      setSuccess(`Удалено вопросов: ${removed}`);
      loadTopics();
      if (viewMode === 'topics') loadTopicViewQuestions();
    } catch { setError(t.admin.common.deleteError); }
  };

  const deleteSectionWithCascade = async (sectionId: number) => {
    if (!confirm('Удалить эту секцию, все её темы и вопросы? Это действие нельзя отменить.')) return;
    try {
      await adminService.deleteSection(sectionId);
      setSuccess('Секция удалена');
      loadSectionsAndSkills();
      loadTopics();
    } catch { setError(t.admin.common.deleteError); }
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
    if (topicFilterExam) filtered = filtered.filter(tp => tp.examTypeCode === topicFilterExam);
    if (topicFilterSection) filtered = filtered.filter(tp => tp.sectionName === topicFilterSection);
    if (topicFilterTopic) {
      const q = topicFilterTopic.toLowerCase();
      filtered = filtered.filter(tp => tp.name.toLowerCase().includes(q));
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
      .map(tp => ({
        topic: tp,
        questions: qMap.get(`${tp.examTypeCode}|${tp.name}`) ?? [],
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
        <p style={{ color: 'var(--text-secondary)', marginTop: '1rem' }}>{t.admin.common.loading}</p>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem', flexWrap: 'wrap', gap: '0.75rem' }}>
        <div>
          <h1 style={{ margin: 0 }}>{t.admin.questions.title}</h1>
          <p style={{ color: 'var(--text-secondary)', margin: '0.25rem 0 0' }}>
            {totalCount} {t.admin.questions.questionsCount} • {(() => {
              const activeSec = filterSection || topicFilterSection;
              const filteredTopicCount = activeSec
                ? topics.filter(tp => tp.sectionName === activeSec).length
                : topics.length;
              return `${filteredTopicCount} ${t.admin.questions.topicsCount}`;
            })()}{totalPages > 1 ? ` • ${t.admin.common.page} ${page}/${totalPages}` : ''}
          </p>
        </div>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button className="btn btn-primary" onClick={() => startCreate()} style={{ fontSize: '0.9rem' }}>
            {t.admin.questions.newQuestion}
          </button>
          <button className="btn btn-outline" onClick={openTopicModal} style={{ fontSize: '0.9rem' }}>
            {t.admin.questions.newTopic}
          </button>
          <button className="btn btn-outline" onClick={() => openSectionModal()} style={{ fontSize: '0.9rem' }}>
            {t.admin.questions.newSection}
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
          >{t.admin.questions.tableView}</button>
          <button
            onClick={() => setViewMode('topics')}
            style={{
              padding: '0.4rem 0.75rem', border: 'none', cursor: 'pointer', fontSize: '0.85rem',
              borderLeft: '1px solid var(--border-color)',
              background: viewMode === 'topics' ? 'var(--primary-color)' : 'var(--card-background)',
              color: viewMode === 'topics' ? '#fff' : 'var(--text-secondary)',
            }}
          >{t.admin.questions.topicsView}</button>
          <button
            onClick={() => setViewMode('sections')}
            style={{
              padding: '0.4rem 0.75rem', border: 'none', cursor: 'pointer', fontSize: '0.85rem',
              borderLeft: '1px solid var(--border-color)',
              background: viewMode === 'sections' ? 'var(--primary-color)' : 'var(--card-background)',
              color: viewMode === 'sections' ? '#fff' : 'var(--text-secondary)',
            }}
          >{t.admin.questions.sectionsView}</button>
          <button
            onClick={() => setViewMode('examTypes')}
            style={{
              padding: '0.4rem 0.75rem', border: 'none', cursor: 'pointer', fontSize: '0.85rem',
              borderLeft: '1px solid var(--border-color)',
              background: viewMode === 'examTypes' ? 'var(--primary-color)' : 'var(--card-background)',
              color: viewMode === 'examTypes' ? '#fff' : 'var(--text-secondary)',
            }}
          >Типы экзаменов</button>
        </div>
      </div>

      {/* Table Filters */}
      {viewMode === 'table' && (
        <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.25rem', flexWrap: 'wrap', alignItems: 'center' }}>
          <select value={filterExam} onChange={e => { setFilterExam(e.target.value); setFilterSection(''); setPage(1); }} style={{ padding: '0.5rem' }}>
            <option value="">{t.admin.common.allExams}</option>
            {examTypes.map(et => <option key={et.code} value={et.code}>{et.code}</option>)}
          </select>
          <select value={filterSection} onChange={e => { setFilterSection(e.target.value); setPage(1); }} style={{ padding: '0.5rem' }}>
            <option value="">{t.admin.common.allSections}</option>
            {sections
              .filter(s => !filterExam || s.examTypeCode === filterExam)
              .map(s => <option key={s.id} value={s.name}>{s.name} [{s.examTypeCode}]</option>)}
          </select>
          <select value={filterDiff} onChange={e => { setFilterDiff(e.target.value); setPage(1); }} style={{ padding: '0.5rem' }}>
            <option value="">{t.admin.common.allLevels}</option>
            <option value="Easy">Easy</option>
            <option value="Medium">Medium</option>
            <option value="Hard">Hard</option>
          </select>
          <input
            placeholder={t.admin.questions.searchTopic}
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
            <option value="">{t.admin.common.allExams}</option>
            {examTypes.map(et => <option key={et.code} value={et.code}>{et.code}</option>)}
          </select>
          <select value={topicFilterSection} onChange={e => { setTopicFilterSection(e.target.value); setTopicPage(1); }} style={{ padding: '0.5rem' }}>
            <option value="">{t.admin.common.allSections}</option>
            {sections
              .filter(s => !topicFilterExam || s.examTypeCode === topicFilterExam)
              .map(s => <option key={s.id} value={s.name}>{s.name} [{s.examTypeCode}]</option>)}
          </select>
          <input
            placeholder={t.admin.questions.searchTopic}
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
          {selectionMode && selectedQuestions.size > 0 && (
            <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', padding: '0.5rem 0.75rem', marginBottom: '0.5rem', background: 'var(--primary-color)', borderRadius: '8px', color: '#fff', fontSize: '0.85rem' }}>
              <span>Выбрано: {selectedQuestions.size}</span>
              <button className="btn" style={{ fontSize: '0.8rem', padding: '0.2rem 0.6rem', background: 'rgba(255,255,255,0.2)', border: '1px solid rgba(255,255,255,0.5)', color: '#fff', cursor: 'pointer' }} onClick={bulkDeleteQuestions}>Удалить выбранные</button>
              <button style={{ marginLeft: 'auto', background: 'none', border: 'none', color: '#fff', cursor: 'pointer', fontSize: '1rem' }} onClick={() => { setSelectedQuestions(new Set()); setSelectionMode(false); }}>✕</button>
            </div>
          )}
          {!selectionMode && (
            <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: '0.5rem' }}>
              <button className="btn btn-outline" style={{ fontSize: '0.85rem' }} onClick={() => setSelectionMode(true)}>Выбрать несколько</button>
            </div>
          )}
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
            <thead>
              <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                {selectionMode && <th style={{ padding: '0.5rem', width: '2rem' }}>
                  <input type="checkbox" checked={questions.length > 0 && questions.every(q => selectedQuestions.has(q.id))} onChange={e => setSelectedQuestions(e.target.checked ? new Set(questions.map(q => q.id)) : new Set())} />
                </th>}
                <th style={thStyle}>ID</th>
                <th style={thStyle}>{t.admin.questions.examSection}</th>
                <th style={thStyle}>{t.admin.questions.topicName}</th>
                <th style={thStyle}>{t.admin.questions.questionText}</th>
                <th style={thStyle}>{t.admin.questions.difficulty}</th>
                <th style={thStyle}>{t.admin.questions.answers}</th>
                <th style={thStyle}>b</th>
              </tr>
            </thead>
            <tbody>
              {questions.map(q => (
                <tr key={q.id}
                  onClick={() => openDetail(q.id)}
                  style={{ borderBottom: '1px solid var(--border-color)', cursor: 'pointer', transition: 'background 0.15s' }}
                  onMouseEnter={e => e.currentTarget.style.background = 'var(--bg-secondary)'}
                  onMouseLeave={e => e.currentTarget.style.background = ''}
                >
                  <td style={{ ...tdStyle, width: '2rem' }} onClick={e => e.stopPropagation()}>
                    {selectionMode && <input type="checkbox" checked={selectedQuestions.has(q.id)} onChange={e => { e.stopPropagation(); setSelectedQuestions(prev => { const s = new Set(prev); e.target.checked ? s.add(q.id) : s.delete(q.id); return s; }); }} />}
                  </td>
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
              {t.admin.questions.noQuestionsFilter}
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
              {t.admin.questions.loadingTopics}
            </div>
          )}
          {groupedByTopic.length === 0 && !topicViewLoading && (
            <div style={{ textAlign: 'center', padding: '2.5rem', color: 'var(--text-secondary)' }}>
              {t.admin.questions.noTopicsFilter}
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
                    {t.admin.questions.showingTopics} {sliced.length} {t.admin.common.of} {groupedByTopic.length} {t.admin.questions.topicsCount} ({t.admin.common.page} {topicPage}/{topicTotalPages})
                  </div>
                )}
                {sliced.map(({ topic: tp, questions: qs }) => (
                    <div key={`${tp.examTypeCode}-${tp.id}`} className="card" style={{ padding: '1rem 1.25rem' }}>
                      {/* Topic header */}
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: qs.length > 0 ? '0.75rem' : 0 }}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
                          <Badge bg={EXAM_COLORS[tp.examTypeCode]}>{tp.examTypeCode}</Badge>
                          <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{tp.sectionName} →</span>
                          <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem', fontFamily: 'monospace' }}>#{tp.id}</span>
                          <span
                            style={{ fontWeight: 600, fontSize: '1rem', cursor: 'pointer', borderBottom: '1px dashed var(--text-secondary)' }}
                            onClick={() => { startTopicRename(tp.id, tp.name); setShowTopicRenameModal(true); }}
                            title={t.admin.questions.clickToEditTopic}
                          >{tp.name}</span>
                          <span style={{ color: tp.questionCount > 0 ? 'var(--text-secondary)' : 'var(--error-color)', fontSize: '0.8rem' }}>
                            ({tp.questionCount} {tp.questionCount === 0 ? t.admin.questions.noQuestions : t.admin.questions.questionsShort})
                          </span>
                        </div>
                        <button
                          className="btn btn-outline"
                          style={{ fontSize: '0.8rem', padding: '0.3rem 0.7rem' }}
                          onClick={() => startCreate(tp.id)}
                        >
                          {t.admin.questions.newQuestion}
                        </button>
                        {tp.questionCount > 0 && (
                          <button
                            className="btn btn-outline"
                            style={{ fontSize: '0.8rem', padding: '0.3rem 0.7rem', color: 'var(--error-color)' }}
                            onClick={() => clearTopicQuestions(tp.id)}
                            title="Удалить все вопросы темы (тема останется)"
                          >
                            Очистить вопросы
                          </button>
                        )}
                        <button
                          className="btn btn-outline"
                          style={{ fontSize: '0.8rem', padding: '0.3rem 0.7rem', color: 'var(--error-color)' }}
                          onClick={() => deleteTopic(tp.id)}
                          title="Удалить тему и все вопросы"
                        >
                          ✕
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
                            <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{q.answerCount} {t.admin.questions.optionsShort}</span>
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

      {/* ─── SECTIONS VIEW ─── */}
      {viewMode === 'sections' && (
        <>
          <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.25rem', flexWrap: 'wrap', alignItems: 'center' }}>
            <select value={sectionFilterExam} onChange={e => { setSectionFilterExam(e.target.value); setSectionPage(1); }} style={{ padding: '0.5rem' }}>
              <option value="">{t.admin.common.allExams}</option>
              {examTypes.map(et => <option key={et.code} value={et.code}>{et.code}</option>)}
            </select>
          </div>
          {(() => {
            const filteredSections = sectionFilterExam
              ? sections.filter(s => s.examTypeCode === sectionFilterExam)
              : sections;
            const sectionTotalPages = Math.ceil(filteredSections.length / SECTIONS_PER_PAGE);
            const slicedSections = filteredSections.slice(
              (sectionPage - 1) * SECTIONS_PER_PAGE,
              sectionPage * SECTIONS_PER_PAGE
            );
            return (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                {filteredSections.length > 0 && (
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                    {t.admin.questions.showingSections} {slicedSections.length} {t.admin.common.of} {filteredSections.length} {t.admin.questions.sectionsCount}
                  </div>
                )}
                {slicedSections.length === 0 && (
                  <div style={{ textAlign: 'center', padding: '2.5rem', color: 'var(--text-secondary)' }}>
                    {t.admin.questions.noSectionsFilter}
                  </div>
                )}
                {slicedSections.map(s => {
                  const sectionTopics = topics.filter(tp => tp.sectionName === s.name && tp.examTypeCode === s.examTypeCode);
                  const totalQ = sectionTopics.reduce((sum, tp) => sum + tp.questionCount, 0);
                  return (
                    <div key={s.id} className="card" style={{ padding: '1rem 1.25rem' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: sectionTopics.length > 0 ? '0.75rem' : 0 }}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
                          <Badge bg={EXAM_COLORS[s.examTypeCode]}>{s.examTypeCode}</Badge>
                          {editingSectionId === s.id && showSectionModal ? (
                            <span style={{ fontWeight: 600, fontSize: '1.05rem' }}>{s.name}</span>
                          ) : (
                            <span
                              style={{ fontWeight: 600, fontSize: '1.05rem', cursor: 'pointer', borderBottom: '1px dashed var(--text-secondary)' }}
                              onClick={() => openSectionModal(s)}
                              title={t.admin.questions.editSection}
                            >{s.name}</span>
                          )}
                          <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>
                            ({sectionTopics.length} {t.admin.questions.topicsCount} • {totalQ} {t.admin.questions.questionsShort})
                          </span>
                        </div>
                        <button
                          className="btn btn-outline"
                          style={{ fontSize: '0.8rem', padding: '0.3rem 0.7rem', color: 'var(--error-color)', flexShrink: 0 }}
                          onClick={() => deleteSectionWithCascade(s.id)}
                          title="Удалить секцию, все темы и вопросы"
                        >
                          ✕
                        </button>
                      </div>
                      {sectionTopics.length > 0 && (
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.3rem' }}>
                          {sectionTopics.map(tp => (
                            <div key={tp.id} style={{
                              display: 'flex', alignItems: 'center', gap: '0.6rem',
                              padding: '0.4rem 0.6rem', borderRadius: '6px',
                              border: '1px solid var(--border-color)', fontSize: '0.85rem',
                            }}>
                              {editingTopicId === tp.id ? (
                                <span style={{ display: 'flex', alignItems: 'center', gap: '0.3rem', flex: 1 }}>
                                  <input
                                    className="form-input"
                                    value={editingTopicName}
                                    onChange={e => setEditingTopicName(e.target.value)}
                                    onKeyDown={e => { if (e.key === 'Enter') saveTopicRename(); if (e.key === 'Escape') setEditingTopicId(null); }}
                                    autoFocus
                                    style={{ padding: '0.15rem 0.4rem', fontSize: '0.85rem', flex: 1 }}
                                  />
                                  <button className="btn btn-primary" onClick={saveTopicRename} style={{ padding: '0.15rem 0.4rem', fontSize: '0.7rem' }}>✓</button>
                                  <button className="btn btn-outline" onClick={() => setEditingTopicId(null)} style={{ padding: '0.15rem 0.4rem', fontSize: '0.7rem' }}>✕</button>
                                </span>
                              ) : (
                                <span
                                  style={{ flex: 1, cursor: 'pointer', borderBottom: '1px dashed transparent' }}
                                  onClick={() => startTopicRename(tp.id, tp.name)}
                                  onMouseEnter={e => (e.currentTarget.style.borderBottomColor = 'var(--text-secondary)')}
                                  onMouseLeave={e => (e.currentTarget.style.borderBottomColor = 'transparent')}
                                  title={t.admin.questions.clickToEditTopic}
                                >
                                  {tp.name}
                                </span>
                              )}
                              <span style={{ color: tp.questionCount > 0 ? 'var(--text-secondary)' : 'var(--error-color)', fontSize: '0.8rem', flexShrink: 0 }}>
                                {tp.questionCount} {t.admin.questions.questionsShort}
                              </span>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  );
                })}
                {sectionTotalPages > 1 && (
                  <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '0.5rem', padding: '1rem 0' }}>
                    <button className="btn btn-outline" disabled={sectionPage <= 1} onClick={() => setSectionPage(1)} style={{ fontSize: '0.85rem' }}>«</button>
                    <button className="btn btn-outline" disabled={sectionPage <= 1} onClick={() => setSectionPage(p => p - 1)} style={{ fontSize: '0.85rem' }}>‹</button>
                    <span style={{ padding: '0 0.75rem', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
                      {sectionPage} / {sectionTotalPages}
                    </span>
                    <button className="btn btn-outline" disabled={sectionPage >= sectionTotalPages} onClick={() => setSectionPage(p => p + 1)} style={{ fontSize: '0.85rem' }}>›</button>
                    <button className="btn btn-outline" disabled={sectionPage >= sectionTotalPages} onClick={() => setSectionPage(sectionTotalPages)} style={{ fontSize: '0.85rem' }}>»</button>
                  </div>
                )}
              </div>
            );
          })()}
        </>
      )}

      {/* ═══ EXAM TYPES VIEW ═══ */}
      {viewMode === 'examTypes' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{examTypes.length} типов экзаменов</span>
            <button className="btn btn-primary" onClick={() => { setEditingExamTypeCode(null); setExamTypeForm({ code: '', name: '' }); setShowExamTypeForm(true); }}>
              + Тип экзамена
            </button>
          </div>

          {showExamTypeForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingExamTypeCode ? 'Редактировать тип' : 'Новый тип экзамена'}</h3>
              {!editingExamTypeCode && (
                <div style={{ marginBottom: '0.75rem' }}>
                  <div style={{ fontSize: '0.85rem', fontWeight: 500, color: 'var(--text-secondary)', marginBottom: '0.4rem' }}>Код (напр. SAT, NUET) *</div>
                  <input className="form-input" value={examTypeForm.code} onChange={e => setExamTypeForm(p => ({ ...p, code: e.target.value }))} placeholder="SAT" style={{ textTransform: 'uppercase' }} />
                </div>
              )}
              <div style={{ marginBottom: '0.75rem' }}>
                <div style={{ fontSize: '0.85rem', fontWeight: 500, color: 'var(--text-secondary)', marginBottom: '0.4rem' }}>Название *</div>
                <input className="form-input" value={examTypeForm.name} onChange={e => setExamTypeForm(p => ({ ...p, name: e.target.value }))} placeholder="Scholastic Assessment Test" />
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveExamType}>{editingExamTypeCode ? 'Сохранить' : 'Создать'}</button>
                <button className="btn btn-outline" onClick={() => { setShowExamTypeForm(false); setEditingExamTypeCode(null); }}>Отмена</button>
              </div>
            </div>
          )}

          {examTypesLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>{t.admin.common.loading}</div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              {examTypes.map(et => (
                <div key={et.code} className="card" style={{ padding: '1rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <div>
                    <div style={{ fontWeight: 700, fontSize: '1rem' }}>{et.code}</div>
                    <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{et.name}</div>
                    <div style={{ fontSize: '0.8rem', color: 'var(--text-muted, var(--text-secondary))', marginTop: '0.25rem' }}>
                      {sections.filter(s => s.examTypeCode === et.code).length} секций
                    </div>
                  </div>
                  <div style={{ display: 'flex', gap: '0.5rem' }}>
                    <button className="btn btn-outline" style={{ fontSize: '0.8rem' }} onClick={() => { setEditingExamTypeCode(et.code); setExamTypeForm({ code: et.code, name: et.name }); setShowExamTypeForm(true); }}>✎</button>
                    <button className="btn btn-outline" style={{ fontSize: '0.8rem', color: 'var(--error-color)' }} onClick={() => handleDeleteExamType(et.code)}>✕</button>
                  </div>
                </div>
              ))}
              {examTypes.length === 0 && <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Нет типов экзаменов</div>}
            </div>
          )}
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
                    <strong>{t.admin.questions.explanationLabel}</strong> {selected.explanation}
                  </div>
                )}

                <div style={{ display: 'flex', gap: '1rem', fontSize: '0.8rem', color: 'var(--text-secondary)', marginBottom: '1.25rem' }}>
                  <span>b = {selected.difficultyParam}</span>
                  <span>a = {selected.discriminationParam}</span>
                  <span>c = {selected.guessParam}</span>
                </div>

                <div style={{ display: 'flex', gap: '0.75rem' }}>
                  <button className="btn btn-primary" style={{ fontSize: '0.85rem' }} onClick={startEdit}>
                    {t.admin.common.edit}
                  </button>
                  <button className="btn btn-outline" style={{ fontSize: '0.85rem', color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                    onClick={() => deleteQuestion(selected.id)}>
                    {t.admin.common.delete}
                  </button>
                </div>
              </>
            )}

            {/* ─── EDIT / CREATE MODE ─── */}
            {(modalMode === 'edit' || modalMode === 'create') && (
              <>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
                  <h2 style={{ margin: 0, fontSize: '1.15rem' }}>
                    {modalMode === 'create' ? t.admin.questions.createQuestion : t.admin.questions.editQuestion}
                  </h2>
                  <button onClick={closeModal} style={closeBtn}>✕</button>
                </div>

                {error && (
                  <div style={{ color: 'var(--error-color)', marginBottom: '1rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>
                    {error}
                  </div>
                )}

                <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                  {/* Section selector (for create AND edit) */}
                  {(modalMode === 'create' || modalMode === 'edit') && (
                    <FormField label={t.admin.questions.selectSectionForQuestion}>
                      <select
                        value={formSectionId}
                        onChange={e => { setFormSectionId(Number(e.target.value)); setForm({ ...form, topicId: 0 }); }}
                        style={{ width: '100%' }}
                      >
                        <option value={0}>{t.admin.questions.selectSectionForQuestion}...</option>
                        {sections.map(s => (
                          <option key={s.id} value={s.id}>
                            {s.name} [{s.examTypeCode}]
                          </option>
                        ))}
                      </select>
                    </FormField>
                  )}

                  {/* Topic selector (for create AND edit) */}
                  {(modalMode === 'create' || modalMode === 'edit') && (
                    <FormField label={t.admin.questions.selectTopic}>
                      <select
                        value={form.topicId}
                        onChange={e => setForm({ ...form, topicId: Number(e.target.value) })}
                        style={{ width: '100%' }}
                      >
                        <option value={0}>{t.admin.questions.selectTopic}...</option>
                        {(formSectionId
                          ? topics.filter(tp => { const sec = sections.find(s => s.id === formSectionId); return sec && tp.sectionName === sec.name && tp.examTypeCode === sec.examTypeCode; })
                          : topics
                        ).map(tp => (
                          <option key={tp.id} value={tp.id}>
                            [{tp.examTypeCode}] {tp.sectionName} → {tp.name} ({tp.questionCount} {t.admin.questions.questionsShort})
                          </option>
                        ))}
                      </select>
                    </FormField>
                  )}

                  {/* Question text */}
                  <FormField label={t.admin.questions.questionText}>
                    <textarea
                      value={form.text}
                      onChange={e => setForm({ ...form, text: e.target.value })}
                      onFocus={() => setMathTarget('text')}
                      className="form-input"
                      rows={3}
                      style={{ width: '100%', resize: 'vertical' }}
                      placeholder={t.admin.questions.enterQuestion}
                    />
                  </FormField>

                  {/* Math keyboard toggle */}
                  <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                    <button
                      type="button"
                      className="btn btn-outline"
                      onClick={() => setShowMathKeyboard(!showMathKeyboard)}
                      style={{ fontSize: '0.8rem', padding: '0.3rem 0.7rem' }}
                    >
                      {showMathKeyboard ? '✕' : '∑'} {t.admin.questions.mathKeyboard}
                    </button>
                  </div>

                  {/* Math keyboard panel — Photomath style */}
                  {showMathKeyboard && (
                    <div style={{
                      borderRadius: '12px',
                      border: '1px solid var(--border-color)',
                      background: 'var(--bg-secondary)',
                      overflow: 'hidden',
                      boxShadow: '0 2px 12px rgba(0,0,0,0.08)',
                    }}>
                      {/* Target selector — pill buttons */}
                      <div style={{ display: 'flex', gap: '0.35rem', padding: '0.6rem 0.75rem', flexWrap: 'wrap', borderBottom: '1px solid var(--border-color)' }}>
                        {[
                          { key: 'text', label: t.admin.questions.questionText },
                          { key: 'explanation', label: t.admin.questions.explanation },
                          ...form.answerOptions.map((_, i) => ({ key: String(i), label: String.fromCharCode(65 + i) })),
                        ].map(target => (
                          <button
                            key={target.key}
                            type="button"
                            onClick={() => setMathTarget(target.key === 'text' ? 'text' : target.key === 'explanation' ? 'explanation' : Number(target.key))}
                            style={{
                              padding: '0.25rem 0.65rem', borderRadius: '999px', border: 'none',
                              background: (mathTarget === target.key || mathTarget === Number(target.key)) ? 'var(--primary-color)' : 'var(--card-background)',
                              color: (mathTarget === target.key || mathTarget === Number(target.key)) ? '#fff' : 'var(--text-secondary)',
                              cursor: 'pointer', fontSize: '0.75rem', fontWeight: 600,
                              boxShadow: '0 1px 3px rgba(0,0,0,0.06)',
                            }}
                          >
                            {target.label}
                          </button>
                        ))}
                      </div>
                      {/* Category tabs */}
                      <div style={{
                        display: 'flex', gap: 0, overflowX: 'auto',
                        borderBottom: '1px solid var(--border-color)', background: 'var(--card-background)',
                      }}>
                        {Object.keys(MATH_SYMBOLS).map(tab => (
                          <button
                            key={tab}
                            type="button"
                            onClick={() => setMathTab(tab)}
                            style={{
                              flex: '1 0 auto', padding: '0.45rem 0.7rem', border: 'none',
                              borderBottom: mathTab === tab ? '2px solid var(--primary-color)' : '2px solid transparent',
                              background: 'transparent',
                              color: mathTab === tab ? 'var(--primary-color)' : 'var(--text-secondary)',
                              cursor: 'pointer', fontSize: '0.85rem', fontWeight: mathTab === tab ? 700 : 500,
                              whiteSpace: 'nowrap', transition: 'all 0.15s',
                            }}
                          >
                            {tab}
                          </button>
                        ))}
                      </div>
                      {/* Symbol grid */}
                      <div style={{
                        display: 'grid',
                        gridTemplateColumns: 'repeat(auto-fill, minmax(38px, 1fr))',
                        gap: '4px', padding: '0.6rem',
                      }}>
                        {MATH_SYMBOLS[mathTab]?.map(sym => (
                          <button
                            key={sym}
                            type="button"
                            onClick={() => insertMathSymbol(sym)}
                            style={{
                              height: '38px', border: 'none',
                              borderRadius: '8px', background: 'var(--card-background)',
                              color: 'var(--text-primary)', cursor: 'pointer',
                              display: 'flex', alignItems: 'center', justifyContent: 'center',
                              fontSize: sym.length > 2 ? '0.65rem' : '1rem', fontWeight: 500,
                              boxShadow: '0 1px 3px rgba(0,0,0,0.08)',
                              transition: 'transform 0.1s, box-shadow 0.1s',
                            }}
                            onMouseDown={e => { e.currentTarget.style.transform = 'scale(0.92)'; }}
                            onMouseUp={e => { e.currentTarget.style.transform = 'scale(1)'; }}
                            onMouseLeave={e => { e.currentTarget.style.transform = 'scale(1)'; }}
                            title={sym}
                          >
                            {sym}
                          </button>
                        ))}
                      </div>
                    </div>
                  )}

                  {/* Difficulty */}
                  <FormField label={t.admin.questions.difficulty}>
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
                  <FormField label={`${t.admin.questions.answerOptions} (${t.admin.questions.answerOptionsHint})`}>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
                      {form.answerOptions.map((opt, i) => (
                        <div key={i} style={{ display: 'flex', gap: '0.4rem', alignItems: 'center' }}>
                          <button
                            type="button"
                            onClick={() => updateOption(i, 'isCorrect', true)}
                            title={opt.isCorrect ? t.admin.questions.correctAnswer : t.admin.questions.markCorrect}
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
                            placeholder={`${t.admin.questions.optionPlaceholder} ${String.fromCharCode(65 + i)}...`}
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
                              title={t.admin.questions.removeOption}
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
                          {t.admin.questions.addOption}
                        </button>
                      )}
                    </div>
                  </FormField>

                  {/* Image URL */}
                  <FormField label="URL изображения (необязательно)">
                    <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                      <input
                        type="url"
                        value={form.imageUrl}
                        onChange={e => setForm({ ...form, imageUrl: e.target.value })}
                        className="form-input"
                        style={{ flex: 1 }}
                        placeholder="https://..."
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
                              const url = await adminService.uploadImage(file);
                              setForm(f => ({ ...f, imageUrl: url }));
                            } catch {
                              setError('Не удалось загрузить изображение');
                            } finally {
                              setUploadingImage(false);
                              e.target.value = '';
                            }
                          }}
                        />
                        <span className="btn btn-secondary" style={{ pointerEvents: 'none' }}>
                          {uploadingImage ? 'Загрузка...' : 'С диска'}
                        </span>
                      </label>
                    </div>
                    {form.imageUrl && (
                      <img src={form.imageUrl} alt="preview" style={{ marginTop: '0.5rem', maxWidth: '100%', maxHeight: '200px', borderRadius: '0.5rem', objectFit: 'contain' }} />
                    )}
                  </FormField>

                  {/* Explanation */}
                  <FormField label={t.admin.questions.explanationOptional}>
                    <textarea
                      value={form.explanation}
                      onChange={e => setForm({ ...form, explanation: e.target.value })}
                      className="form-input"
                      rows={2}
                      style={{ width: '100%', resize: 'vertical' }}
                      placeholder={t.admin.questions.explanationPlaceholder}
                    />
                  </FormField>

                  {/* Advanced IRT Parameters */}
                  <div style={{ marginTop: '0.5rem' }}>
                    <button
                      type="button"
                      onClick={() => setShowIrtParams(v => !v)}
                      style={{
                        background: 'none', border: 'none', cursor: 'pointer',
                        fontSize: '0.82rem', color: 'var(--text-secondary)',
                        padding: '0.25rem 0', display: 'flex', alignItems: 'center', gap: '0.4rem'
                      }}
                    >
                      <span style={{ fontSize: '0.7rem' }}>{showIrtParams ? '▼' : '▶'}</span>
                      Расширенные IRT-параметры
                    </button>

                    {showIrtParams && (
                      <div style={{
                        marginTop: '0.5rem', padding: '0.75rem', borderRadius: '0.5rem',
                        background: 'var(--bg-secondary)', border: '1px solid var(--border-color)',
                        display: 'flex', flexDirection: 'column', gap: '0.75rem'
                      }}>
                        {/* Presets */}
                        <div>
                          <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', marginBottom: '0.35rem' }}>
                            Пресеты
                          </div>
                          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.35rem' }}>
                            {IRT_PRESETS.map(p => (
                              <button
                                key={p.label}
                                type="button"
                                className="btn btn-outline"
                                style={{ fontSize: '0.75rem', padding: '0.2rem 0.6rem' }}
                                onClick={() => setForm(f => ({ ...f, difficultyParam: p.b, discriminationParam: p.a, guessParam: p.c }))}
                              >
                                {p.label}
                              </button>
                            ))}
                          </div>
                        </div>

                        {/* Numeric inputs */}
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '0.5rem' }}>
                          {[
                            { key: 'difficultyParam' as const, label: 'b (сложность)', min: -4, max: 4, step: 0.1 },
                            { key: 'discriminationParam' as const, label: 'a (дискриминация)', min: 0.3, max: 3, step: 0.05 },
                            { key: 'guessParam' as const, label: 'c (угадываемость)', min: 0, max: 0.5, step: 0.01 },
                          ].map(({ key, label, min, max, step }) => {
                            const val = form[key];
                            const invalid = val !== null && (val < min || val > max);
                            return (
                              <div key={key}>
                                <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '0.2rem' }}>{label}</div>
                                <input
                                  type="number"
                                  className="form-input"
                                  style={{ width: '100%', borderColor: invalid ? 'var(--error-color)' : undefined }}
                                  min={min} max={max} step={step}
                                  value={val ?? ''}
                                  onChange={e => {
                                    const v = e.target.value === '' ? null : parseFloat(e.target.value);
                                    setForm(f => ({ ...f, [key]: v }));
                                  }}
                                  placeholder="авто"
                                />
                                {invalid && (
                                  <div style={{ fontSize: '0.7rem', color: 'var(--error-color)', marginTop: '0.15rem' }}>
                                    Диапазон: {min} … {max}
                                  </div>
                                )}
                              </div>
                            );
                          })}
                        </div>

                        <div style={{ fontSize: '0.72rem', color: 'var(--text-secondary)', fontStyle: 'italic' }}>
                          ⓘ Пустое поле = автоматически из выбранной сложности. Изменяйте только если уверены.
                        </div>
                      </div>
                    )}
                  </div>

                  {/* Actions */}
                  <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem' }}>
                    <button className="btn btn-primary" onClick={modalMode === 'create' ? saveCreate : saveEdit} style={{ flex: 1 }}>
                      {modalMode === 'create' ? t.admin.questions.createQuestion : t.admin.common.save}
                    </button>
                    <button className="btn btn-outline" onClick={modalMode === 'edit' ? () => setModalMode('view') : closeModal} style={{ flex: 1 }}>
                      {t.admin.common.cancel}
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
              <h2 style={{ margin: 0, fontSize: '1.15rem' }}>{t.admin.questions.newTopic}</h2>
              <button onClick={() => setShowTopicModal(false)} style={closeBtn}>✕</button>
            </div>

            {error && (
              <div style={{ color: 'var(--error-color)', marginBottom: '1rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>
                {error}
              </div>
            )}

            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <FormField label={t.admin.questions.topicName}>
                <input
                  className="form-input"
                  value={topicForm.name}
                  onChange={e => setTopicForm({ ...topicForm, name: e.target.value })}
                  placeholder={t.admin.questions.enterTopicName}
                  style={{ width: '100%' }}
                  autoFocus
                />
              </FormField>

              <FormField label={t.admin.questions.examSection}>
                <select
                  value={topicForm.sectionId}
                  onChange={e => setTopicForm({ ...topicForm, sectionId: Number(e.target.value) })}
                  style={{ width: '100%' }}
                >
                  <option value={0}>{t.admin.questions.selectSection}...</option>
                  {sections.map(s => (
                    <option key={s.id} value={s.id}>
                      [{s.examTypeCode}] {s.name}
                    </option>
                  ))}
                </select>
              </FormField>

              <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveTopic} style={{ flex: 1 }}>
                  {t.admin.questions.createTopic}
                </button>
                <button className="btn btn-outline" onClick={() => setShowTopicModal(false)} style={{ flex: 1 }}>
                  {t.admin.common.cancel}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
      {/* ═══ SECTION CREATION/EDIT MODAL ═══ */}
      {showSectionModal && (
        <div
          style={{
            position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            zIndex: 1001, padding: '1rem',
          }}
          onClick={() => setShowSectionModal(false)}
        >
          <div
            className="card"
            style={{ maxWidth: '500px', width: '100%', padding: '2rem' }}
            onClick={e => e.stopPropagation()}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
              <h2 style={{ margin: 0, fontSize: '1.15rem' }}>
                {editingSectionId ? t.admin.questions.editSection : t.admin.questions.newSection}
              </h2>
              <button onClick={() => setShowSectionModal(false)} style={closeBtn}>✕</button>
            </div>

            {error && (
              <div style={{ color: 'var(--error-color)', marginBottom: '1rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>
                {error}
              </div>
            )}

            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <FormField label={t.admin.questions.sectionName}>
                <input
                  className="form-input"
                  value={sectionForm.name}
                  onChange={e => setSectionForm({ ...sectionForm, name: e.target.value })}
                  placeholder={t.admin.questions.enterSectionName}
                  style={{ width: '100%' }}
                  autoFocus
                />
              </FormField>

              {!editingSectionId && (
                <FormField label={t.admin.questions.selectExamType}>
                  <select
                    value={sectionForm.examTypeCode}
                    onChange={e => setSectionForm({ ...sectionForm, examTypeCode: e.target.value })}
                    style={{ width: '100%' }}
                  >
                    <option value="">{t.admin.questions.selectExamType}...</option>
                    {examTypes.map(et => <option key={et.code} value={et.code}>{et.code}</option>)}
                  </select>
                </FormField>
              )}

              {/* Existing sections list for editing */}
              {!editingSectionId && sections.length > 0 && (
                <div style={{ marginTop: '0.5rem' }}>
                  <label style={{ display: 'block', marginBottom: '0.35rem', fontWeight: 600, fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                    {t.admin.questions.editSection}
                  </label>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.3rem', maxHeight: '200px', overflow: 'auto' }}>
                    {sections.map(s => (
                      <div key={s.id} style={{
                        display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                        padding: '0.4rem 0.6rem', borderRadius: '6px',
                        border: '1px solid var(--border-color)', fontSize: '0.85rem',
                      }}>
                        <span><Badge bg={EXAM_COLORS[s.examTypeCode]}>{s.examTypeCode}</Badge> {s.name}</span>
                        <button
                          type="button"
                          className="btn btn-outline"
                          style={{ fontSize: '0.75rem', padding: '0.2rem 0.5rem' }}
                          onClick={() => {
                            setShowSectionModal(false);
                            setTimeout(() => openSectionModal(s), 100);
                          }}
                        >
                          {t.admin.common.edit}
                        </button>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveSection} style={{ flex: 1 }}>
                  {editingSectionId ? t.admin.common.save : t.admin.common.create}
                </button>
                <button className="btn btn-outline" onClick={() => setShowSectionModal(false)} style={{ flex: 1 }}>
                  {t.admin.common.cancel}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
      {/* ═══ TOPIC RENAME MODAL ═══ */}
      {showTopicRenameModal && editingTopicId && (
        <div
          style={{
            position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            zIndex: 1001, padding: '1rem',
          }}
          onClick={() => { setShowTopicRenameModal(false); setEditingTopicId(null); }}
        >
          <div
            className="card"
            style={{ maxWidth: '500px', width: '100%', padding: '2rem' }}
            onClick={e => e.stopPropagation()}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
              <h2 style={{ margin: 0, fontSize: '1.15rem' }}>{t.admin.questions.editTopicName}</h2>
              <button onClick={() => { setShowTopicRenameModal(false); setEditingTopicId(null); }} style={closeBtn}>✕</button>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <FormField label={t.admin.questions.topicName}>
                <input
                  className="form-input"
                  value={editingTopicName}
                  onChange={e => setEditingTopicName(e.target.value)}
                  onKeyDown={e => { if (e.key === 'Enter') { saveTopicRename(); setShowTopicRenameModal(false); } }}
                  autoFocus
                  style={{ width: '100%' }}
                />
              </FormField>
              <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem' }}>
                <button className="btn btn-primary" onClick={() => { saveTopicRename(); setShowTopicRenameModal(false); }} style={{ flex: 1 }}>
                  {t.admin.common.save}
                </button>
                <button className="btn btn-outline" onClick={() => { setShowTopicRenameModal(false); setEditingTopicId(null); }} style={{ flex: 1 }}>
                  {t.admin.common.cancel}
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
