import { useEffect, useState, useCallback } from 'react';
import adminService from '../services/adminService';
import { lessonService } from '../services/lessonService';
import type { AdminTopicSummary } from '../types';

type ContentTab = 'lessons' | 'flashcards' | 'formulas' | 'strategies' | 'drills';

type Lesson = { id: number; topicId: number; topicName: string; title: string; videoUrl: string | null; sortOrder: number; stepCount: number };
type Deck = { id: number; title: string; description: string | null; examTypeCode: string | null; topicId: number | null; topicName: string | null; isSystem: boolean; cardCount: number; createdAt: string };
type Card = { id: number; deckId: number; front: string; back: string; sortOrder: number };
type Formula = { id: number; topicId: number; topicName: string; title: string; formula: string; description: string | null; sortOrder: number };
type Strategy = { id: number; examTypeCode: string; title: string; summary: string; category: string; estimatedReadMinutes: number; sortOrder: number };
type DrillTemplate = { id: number; title: string; description: string | null; drillType: string; examTypeCode: string | null; topicId: number | null; topicName: string | null; questionCount: number; timeLimitMinutes: number | null; isActive: boolean; sortOrder: number };

const TABS: { key: ContentTab; label: string }[] = [
  { key: 'lessons', label: 'Уроки' },
  { key: 'flashcards', label: 'Флэшкарты' },
  { key: 'formulas', label: 'Формулы' },
  { key: 'strategies', label: 'Стратегии' },
  { key: 'drills', label: 'Дриллы' },
];

export default function AdminContentPage() {
  const [tab, setTab] = useState<ContentTab>('lessons');
  const [topics, setTopics] = useState<AdminTopicSummary[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Lessons
  const [lessons, setLessons] = useState<Lesson[]>([]);
  const [lessonsLoading, setLessonsLoading] = useState(false);
  const [showLessonForm, setShowLessonForm] = useState(false);
  const [lessonForm, setLessonForm] = useState({ topicId: 0, title: '', content: '', videoUrl: '' });
  const [editingLessonId, setEditingLessonId] = useState<number | null>(null);

  // Flashcards
  const [decks, setDecks] = useState<Deck[]>([]);
  const [decksLoading, setDecksLoading] = useState(false);
  const [showDeckForm, setShowDeckForm] = useState(false);
  const [deckForm, setDeckForm] = useState({ title: '', description: '', examTypeCode: '' });
  const [editingDeckId, setEditingDeckId] = useState<number | null>(null);
  const [expandedDeck, setExpandedDeck] = useState<number | null>(null);
  const [cards, setCards] = useState<Card[]>([]);
  const [showCardForm, setShowCardForm] = useState(false);
  const [cardForm, setCardForm] = useState({ front: '', back: '' });
  const [editingCardId, setEditingCardId] = useState<number | null>(null);

  // Formulas
  const [formulas, setFormulas] = useState<Formula[]>([]);
  const [formulasLoading, setFormulasLoading] = useState(false);
  const [showFormulaForm, setShowFormulaForm] = useState(false);
  const [formulaForm, setFormulaForm] = useState({ topicId: 0, title: '', formula: '', description: '' });
  const [editingFormulaId, setEditingFormulaId] = useState<number | null>(null);

  // Strategies
  const [strategies, setStrategies] = useState<Strategy[]>([]);
  const [strategiesLoading, setStrategiesLoading] = useState(false);
  const [showStrategyForm, setShowStrategyForm] = useState(false);
  const [strategyForm, setStrategyForm] = useState({ examTypeCode: '', title: '', summary: '', content: '', category: 'test-taking', estimatedReadMinutes: 5 });
  const [editingStrategyId, setEditingStrategyId] = useState<number | null>(null);

  // Drills
  const [drills, setDrills] = useState<DrillTemplate[]>([]);
  const [drillsLoading, setDrillsLoading] = useState(false);
  const [showDrillForm, setShowDrillForm] = useState(false);
  const [drillForm, setDrillForm] = useState({ title: '', description: '', drillType: 'Speed', examTypeCode: '', topicId: 0, questionCount: 10, timeLimitMinutes: 0, isActive: true, sortOrder: 0 });
  const [editingDrillId, setEditingDrillId] = useState<number | null>(null);

  // Load topics on mount
  useEffect(() => {
    adminService.getTopics().then(setTopics).catch(() => {});
  }, []);

  // Load data on tab change
  const loadLessons = useCallback(async () => {
    setLessonsLoading(true);
    try { setLessons(await adminService.getLessons()); } catch { setError('Ошибка загрузки уроков'); }
    finally { setLessonsLoading(false); }
  }, []);

  const loadDecks = useCallback(async () => {
    setDecksLoading(true);
    try { setDecks(await adminService.getDecks()); } catch { setError('Ошибка загрузки колод'); }
    finally { setDecksLoading(false); }
  }, []);

  const loadFormulas = useCallback(async () => {
    setFormulasLoading(true);
    try { setFormulas(await adminService.getFormulas()); } catch { setError('Ошибка загрузки формул'); }
    finally { setFormulasLoading(false); }
  }, []);

  const loadStrategies = useCallback(async () => {
    setStrategiesLoading(true);
    try { setStrategies(await adminService.getStrategies()); } catch { setError('Ошибка загрузки стратегий'); }
    finally { setStrategiesLoading(false); }
  }, []);

  const loadDrills = useCallback(async () => {
    setDrillsLoading(true);
    try { setDrills(await adminService.getDrills()); } catch { setError('Ошибка загрузки дриллов'); }
    finally { setDrillsLoading(false); }
  }, []);

  useEffect(() => {
    setError(null);
    setSuccess(null);
    if (tab === 'lessons') loadLessons();
    else if (tab === 'flashcards') loadDecks();
    else if (tab === 'formulas') loadFormulas();
    else if (tab === 'strategies') loadStrategies();
    else if (tab === 'drills') loadDrills();
  }, [tab, loadLessons, loadDecks, loadFormulas, loadStrategies, loadDrills]);

  // ─── Lesson actions ──────────────────────
  const openLessonCreate = () => {
    setEditingLessonId(null);
    setLessonForm({ topicId: topics[0]?.id || 0, title: '', content: '', videoUrl: '' });
    setShowLessonForm(true);
    setError(null);
  };

  const openLessonEdit = async (l: Lesson) => {
    setEditingLessonId(l.id);
    try {
      const full = await lessonService.getLesson(l.id);
      setLessonForm({ topicId: l.topicId, title: l.title, content: full.content || '', videoUrl: l.videoUrl || '' });
    } catch {
      setLessonForm({ topicId: l.topicId, title: l.title, content: '', videoUrl: l.videoUrl || '' });
    }
    setShowLessonForm(true);
    setError(null);
  };

  const saveLesson = async () => {
    if (!lessonForm.title.trim()) { setError('Введите название'); return; }
    try {
      if (editingLessonId) {
        await adminService.updateLesson(editingLessonId, { title: lessonForm.title, content: lessonForm.content, videoUrl: lessonForm.videoUrl || undefined });
        setSuccess('Урок обновлён');
      } else {
        if (!lessonForm.topicId) { setError('Выберите тему'); return; }
        if (!lessonForm.content.trim()) { setError('Заполните содержание'); return; }
        await adminService.createLesson({ ...lessonForm, videoUrl: lessonForm.videoUrl || undefined });
        setSuccess('Урок создан');
      }
      setShowLessonForm(false);
      setEditingLessonId(null);
      loadLessons();
    } catch { setError('Ошибка сохранения урока'); }
  };

  const deleteLesson = async (id: number) => {
    if (!confirm('Удалить урок?')) return;
    try { await adminService.deleteLesson(id); setSuccess('Урок удалён'); loadLessons(); }
    catch { setError('Ошибка удаления'); }
  };

  // ─── Deck actions ────────────────────────
  const openDeckCreate = () => {
    setEditingDeckId(null);
    setDeckForm({ title: '', description: '', examTypeCode: '' });
    setShowDeckForm(true);
    setError(null);
  };

  const openDeckEdit = (d: Deck) => {
    setEditingDeckId(d.id);
    setDeckForm({ title: d.title, description: d.description || '', examTypeCode: d.examTypeCode || '' });
    setShowDeckForm(true);
    setError(null);
  };

  const saveDeck = async () => {
    if (!deckForm.title.trim()) { setError('Введите название колоды'); return; }
    try {
      if (editingDeckId) {
        await adminService.updateDeck(editingDeckId, { title: deckForm.title, description: deckForm.description || undefined, examTypeCode: deckForm.examTypeCode || undefined });
        setSuccess('Колода обновлена');
      } else {
        await adminService.createDeck({ title: deckForm.title, description: deckForm.description || undefined, examTypeCode: deckForm.examTypeCode || undefined });
        setSuccess('Колода создана');
      }
      setShowDeckForm(false);
      setEditingDeckId(null);
      loadDecks();
    } catch { setError('Ошибка сохранения колоды'); }
  };

  const deleteDeck = async (id: number) => {
    if (!confirm('Удалить колоду?')) return;
    try { await adminService.deleteDeck(id); setExpandedDeck(null); setSuccess('Колода удалена'); loadDecks(); }
    catch { setError('Ошибка удаления'); }
  };

  const toggleDeck = async (deckId: number) => {
    if (expandedDeck === deckId) { setExpandedDeck(null); return; }
    setExpandedDeck(deckId);
    try { setCards(await adminService.getCards(deckId)); } catch { setCards([]); }
  };

  const openCardCreate = () => {
    setEditingCardId(null);
    setCardForm({ front: '', back: '' });
    setShowCardForm(true);
  };

  const openCardEdit = (c: Card) => {
    setEditingCardId(c.id);
    setCardForm({ front: c.front, back: c.back });
    setShowCardForm(true);
  };

  const saveCard = async () => {
    if (!cardForm.front.trim() || !cardForm.back.trim()) { setError('Заполните обе стороны карточки'); return; }
    try {
      if (editingCardId) {
        await adminService.updateCard(editingCardId, { front: cardForm.front, back: cardForm.back });
        setSuccess('Карточка обновлена');
      } else {
        if (!expandedDeck) return;
        await adminService.createCard({ deckId: expandedDeck, front: cardForm.front, back: cardForm.back });
        setSuccess('Карточка создана');
      }
      setShowCardForm(false);
      setEditingCardId(null);
      if (expandedDeck) setCards(await adminService.getCards(expandedDeck));
      loadDecks();
    } catch { setError('Ошибка сохранения карточки'); }
  };

  const deleteCard = async (id: number) => {
    if (!confirm('Удалить карточку?')) return;
    try {
      await adminService.deleteCard(id);
      if (expandedDeck) setCards(await adminService.getCards(expandedDeck));
      loadDecks();
    } catch { setError('Ошибка удаления'); }
  };

  // ─── Formula actions ─────────────────────
  const openFormulaCreate = () => {
    setEditingFormulaId(null);
    setFormulaForm({ topicId: topics[0]?.id || 0, title: '', formula: '', description: '' });
    setShowFormulaForm(true);
    setError(null);
  };

  const openFormulaEdit = (f: Formula) => {
    setEditingFormulaId(f.id);
    setFormulaForm({ topicId: f.topicId, title: f.title, formula: f.formula, description: f.description || '' });
    setShowFormulaForm(true);
    setError(null);
  };

  const saveFormula = async () => {
    if (!formulaForm.title.trim() || !formulaForm.formula.trim()) { setError('Заполните название и формулу'); return; }
    try {
      if (editingFormulaId) {
        await adminService.updateFormula(editingFormulaId, { title: formulaForm.title, formula: formulaForm.formula, description: formulaForm.description || undefined });
        setSuccess('Формула обновлена');
      } else {
        if (!formulaForm.topicId) { setError('Выберите тему'); return; }
        await adminService.createFormula({ ...formulaForm, description: formulaForm.description || undefined });
        setSuccess('Формула создана');
      }
      setShowFormulaForm(false);
      setEditingFormulaId(null);
      loadFormulas();
    } catch { setError('Ошибка сохранения формулы'); }
  };

  const deleteFormula = async (id: number) => {
    if (!confirm('Удалить формулу?')) return;
    try { await adminService.deleteFormula(id); setSuccess('Формула удалена'); loadFormulas(); }
    catch { setError('Ошибка удаления'); }
  };

  // ─── Strategy actions ────────────────────
  const openStrategyCreate = () => {
    setEditingStrategyId(null);
    setStrategyForm({ examTypeCode: 'SAT', title: '', summary: '', content: '', category: 'test-taking', estimatedReadMinutes: 5 });
    setShowStrategyForm(true);
    setError(null);
  };

  const openStrategyEdit = async (s: Strategy) => {
    setEditingStrategyId(s.id);
    try {
      const full = await adminService.getStrategy(s.id);
      setStrategyForm({ examTypeCode: s.examTypeCode, title: s.title, summary: s.summary, content: full.content || '', category: s.category, estimatedReadMinutes: s.estimatedReadMinutes });
    } catch {
      setStrategyForm({ examTypeCode: s.examTypeCode, title: s.title, summary: s.summary, content: '', category: s.category, estimatedReadMinutes: s.estimatedReadMinutes });
    }
    setShowStrategyForm(true);
    setError(null);
  };

  const saveStrategy = async () => {
    if (!strategyForm.title.trim() || !strategyForm.content.trim()) { setError('Заполните название и содержание'); return; }
    try {
      if (editingStrategyId) {
        await adminService.updateStrategy(editingStrategyId, { title: strategyForm.title, summary: strategyForm.summary, content: strategyForm.content, category: strategyForm.category, estimatedReadMinutes: strategyForm.estimatedReadMinutes });
        setSuccess('Стратегия обновлена');
      } else {
        if (!strategyForm.examTypeCode) { setError('Выберите экзамен'); return; }
        await adminService.createStrategy(strategyForm);
        setSuccess('Стратегия создана');
      }
      setShowStrategyForm(false);
      setEditingStrategyId(null);
      loadStrategies();
    } catch { setError('Ошибка сохранения стратегии'); }
  };

  const deleteStrategy = async (id: number) => {
    if (!confirm('Удалить стратегию?')) return;
    try { await adminService.deleteStrategy(id); setSuccess('Стратегия удалена'); loadStrategies(); }
    catch { setError('Ошибка удаления'); }
  };

  // ─── Drill actions ───────────────────────
  const openDrillCreate = () => {
    setEditingDrillId(null);
    setDrillForm({ title: '', description: '', drillType: 'Speed', examTypeCode: '', topicId: 0, questionCount: 10, timeLimitMinutes: 0, isActive: true, sortOrder: 0 });
    setShowDrillForm(true);
    setError(null);
  };

  const openDrillEdit = (d: DrillTemplate) => {
    setEditingDrillId(d.id);
    setDrillForm({
      title: d.title,
      description: d.description || '',
      drillType: d.drillType,
      examTypeCode: d.examTypeCode || '',
      topicId: d.topicId || 0,
      questionCount: d.questionCount,
      timeLimitMinutes: d.timeLimitMinutes || 0,
      isActive: d.isActive,
      sortOrder: d.sortOrder,
    });
    setShowDrillForm(true);
    setError(null);
  };

  const saveDrill = async () => {
    if (!drillForm.title.trim()) { setError('Введите название'); return; }
    try {
      const payload = {
        title: drillForm.title,
        description: drillForm.description || undefined,
        drillType: drillForm.drillType,
        examTypeCode: drillForm.examTypeCode || undefined,
        topicId: drillForm.topicId || undefined,
        questionCount: drillForm.questionCount,
        timeLimitMinutes: drillForm.timeLimitMinutes || undefined,
        isActive: drillForm.isActive,
        sortOrder: drillForm.sortOrder,
      };
      if (editingDrillId) {
        await adminService.updateDrill(editingDrillId, payload);
        setSuccess('Дрилл обновлён');
      } else {
        await adminService.createDrill(payload);
        setSuccess('Дрилл создан');
      }
      setShowDrillForm(false);
      setEditingDrillId(null);
      loadDrills();
    } catch { setError('Ошибка сохранения дрилла'); }
  };

  const deleteDrill = async (id: number) => {
    if (!confirm('Удалить дрилл?')) return;
    try { await adminService.deleteDrill(id); setSuccess('Дрилл удалён'); loadDrills(); }
    catch { setError('Ошибка удаления'); }
  };

  // ─── Common styles ───────────────────────
  const formRow: React.CSSProperties = { display: 'flex', flexDirection: 'column', gap: '0.5rem', marginBottom: '0.75rem' };
  const label: React.CSSProperties = { fontSize: '0.85rem', fontWeight: 500, color: 'var(--text-secondary)' };
  const editBtn: React.CSSProperties = { fontSize: '0.75rem', padding: '0.25rem 0.5rem', cursor: 'pointer' };

  return (
    <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
      <h1 style={{ margin: '0 0 0.25rem' }}>Контент</h1>
      <p style={{ margin: '0 0 1.5rem', color: 'var(--text-secondary)' }}>Управление учебными материалами</p>

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

      {/* Tabs */}
      <div style={{ display: 'flex', borderRadius: '8px', overflow: 'hidden', border: '1px solid var(--border-color)', marginBottom: '1.5rem', width: 'fit-content' }}>
        {TABS.map((t, i) => (
          <button
            key={t.key}
            onClick={() => setTab(t.key)}
            style={{
              padding: '0.5rem 1rem', border: 'none', cursor: 'pointer', fontSize: '0.85rem',
              background: tab === t.key ? 'var(--primary-color)' : 'var(--card-background)',
              color: tab === t.key ? '#fff' : 'var(--text-secondary)',
              borderLeft: i > 0 ? '1px solid var(--border-color)' : 'none',
            }}
          >
            {t.label}
          </button>
        ))}
      </div>

      {/* ═══════ LESSONS TAB ═══════ */}
      {tab === 'lessons' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{lessons.length} уроков</span>
            <button className="btn btn-primary" onClick={openLessonCreate}>+ Урок</button>
          </div>

          {showLessonForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingLessonId ? 'Редактировать урок' : 'Новый урок'}</h3>
              {!editingLessonId && (
                <div style={formRow}>
                  <span style={label}>Тема *</span>
                  <select value={lessonForm.topicId} onChange={e => setLessonForm(p => ({ ...p, topicId: Number(e.target.value) }))} style={{ padding: '0.5rem' }}>
                    <option value={0}>Выберите тему</option>
                    {topics.map(t => <option key={t.id} value={t.id}>{t.examTypeCode} — {t.name}</option>)}
                  </select>
                </div>
              )}
              <div style={formRow}>
                <span style={label}>Название *</span>
                <input className="form-input" value={lessonForm.title} onChange={e => setLessonForm(p => ({ ...p, title: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Содержание (Markdown) *</span>
                <textarea className="form-input" rows={6} value={lessonForm.content} onChange={e => setLessonForm(p => ({ ...p, content: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>URL видео</span>
                <input className="form-input" value={lessonForm.videoUrl} onChange={e => setLessonForm(p => ({ ...p, videoUrl: e.target.value }))} placeholder="https://..." />
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveLesson}>{editingLessonId ? 'Сохранить' : 'Создать'}</button>
                <button className="btn btn-outline" onClick={() => { setShowLessonForm(false); setEditingLessonId(null); }}>Отмена</button>
              </div>
            </div>
          )}

          {lessonsLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
              <thead>
                <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                  <th style={{ padding: '0.5rem' }}>ID</th>
                  <th style={{ padding: '0.5rem' }}>Тема</th>
                  <th style={{ padding: '0.5rem' }}>Название</th>
                  <th style={{ padding: '0.5rem' }}>Шагов</th>
                  <th style={{ padding: '0.5rem' }}></th>
                </tr>
              </thead>
              <tbody>
                {lessons.map(l => (
                  <tr key={l.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                    <td style={{ padding: '0.5rem', color: 'var(--text-secondary)' }}>#{l.id}</td>
                    <td style={{ padding: '0.5rem' }}>{l.topicName}</td>
                    <td style={{ padding: '0.5rem', fontWeight: 500 }}>{l.title}</td>
                    <td style={{ padding: '0.5rem' }}>{l.stepCount}</td>
                    <td style={{ padding: '0.5rem', display: 'flex', gap: '0.25rem' }}>
                      <button className="btn btn-outline" style={editBtn} onClick={() => openLessonEdit(l)}>✎</button>
                      <button className="btn btn-outline" style={{ ...editBtn, color: 'var(--error-color)' }} onClick={() => deleteLesson(l.id)}>✕</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* ═══════ FLASHCARDS TAB ═══════ */}
      {tab === 'flashcards' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{decks.length} колод</span>
            <button className="btn btn-primary" onClick={openDeckCreate}>+ Колода</button>
          </div>

          {showDeckForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingDeckId ? 'Редактировать колоду' : 'Новая колода'}</h3>
              <div style={formRow}>
                <span style={label}>Название *</span>
                <input className="form-input" value={deckForm.title} onChange={e => setDeckForm(p => ({ ...p, title: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Описание</span>
                <input className="form-input" value={deckForm.description} onChange={e => setDeckForm(p => ({ ...p, description: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Экзамен</span>
                <select value={deckForm.examTypeCode} onChange={e => setDeckForm(p => ({ ...p, examTypeCode: e.target.value }))} style={{ padding: '0.5rem' }}>
                  <option value="">Без привязки</option>
                  <option value="SAT">SAT</option>
                  <option value="TOEFL">TOEFL</option>
                  <option value="NUET">NUET</option>
                  <option value="IELTS">IELTS</option>
                  <option value="CSCA">CSCA</option>
                </select>
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveDeck}>{editingDeckId ? 'Сохранить' : 'Создать'}</button>
                <button className="btn btn-outline" onClick={() => { setShowDeckForm(false); setEditingDeckId(null); }}>Отмена</button>
              </div>
            </div>
          )}

          {decksLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              {decks.map(d => (
                <div key={d.id} className="card" style={{ padding: '1rem' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', cursor: 'pointer' }} onClick={() => toggleDeck(d.id)}>
                    <div>
                      <div style={{ fontWeight: 600 }}>{d.title}</div>
                      <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                        {d.examTypeCode || '—'} • {d.cardCount} карт {d.topicName ? `• ${d.topicName}` : ''}
                      </div>
                    </div>
                    <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                      <span style={{ fontSize: '0.9rem' }}>{expandedDeck === d.id ? '▼' : '▶'}</span>
                      <button className="btn btn-outline" style={editBtn} onClick={e => { e.stopPropagation(); openDeckEdit(d); }}>✎</button>
                      <button className="btn btn-outline" style={{ ...editBtn, color: 'var(--error-color)' }} onClick={e => { e.stopPropagation(); deleteDeck(d.id); }}>✕</button>
                    </div>
                  </div>

                  {expandedDeck === d.id && (
                    <div style={{ marginTop: '1rem', paddingTop: '1rem', borderTop: '1px solid var(--border-color)' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
                        <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>Карточки ({cards.length})</span>
                        <button className="btn btn-outline" style={{ fontSize: '0.8rem' }} onClick={openCardCreate}>+ Карточка</button>
                      </div>

                      {showCardForm && (
                        <div style={{ marginBottom: '1rem', padding: '1rem', background: 'var(--bg-secondary)', borderRadius: '8px' }}>
                          <h4 style={{ margin: '0 0 0.75rem', fontSize: '0.9rem' }}>{editingCardId ? 'Редактировать карточку' : 'Новая карточка'}</h4>
                          <div style={formRow}>
                            <span style={label}>Лицевая сторона *</span>
                            <input className="form-input" value={cardForm.front} onChange={e => setCardForm(p => ({ ...p, front: e.target.value }))} />
                          </div>
                          <div style={formRow}>
                            <span style={label}>Обратная сторона *</span>
                            <textarea className="form-input" rows={3} value={cardForm.back} onChange={e => setCardForm(p => ({ ...p, back: e.target.value }))} />
                          </div>
                          <div style={{ display: 'flex', gap: '0.5rem' }}>
                            <button className="btn btn-primary" style={{ fontSize: '0.8rem' }} onClick={saveCard}>{editingCardId ? 'Сохранить' : 'Добавить'}</button>
                            <button className="btn btn-outline" style={{ fontSize: '0.8rem' }} onClick={() => { setShowCardForm(false); setEditingCardId(null); }}>Отмена</button>
                          </div>
                        </div>
                      )}

                      {cards.length === 0 ? (
                        <div style={{ textAlign: 'center', padding: '1rem', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Карточек пока нет</div>
                      ) : (
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                          {cards.map(c => (
                            <div key={c.id} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '0.5rem 0.75rem', background: 'var(--bg-secondary)', borderRadius: '6px', fontSize: '0.85rem' }}>
                              <div style={{ flex: 1 }}>
                                <strong>{c.front}</strong>
                                <span style={{ color: 'var(--text-secondary)', margin: '0 0.5rem' }}>→</span>
                                {c.back}
                              </div>
                              <div style={{ display: 'flex', gap: '0.25rem', flexShrink: 0 }}>
                                <button style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--primary-color)', fontSize: '0.9rem' }} onClick={() => openCardEdit(c)}>✎</button>
                                <button style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--error-color)', fontSize: '0.9rem' }} onClick={() => deleteCard(c.id)}>✕</button>
                              </div>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* ═══════ FORMULAS TAB ═══════ */}
      {tab === 'formulas' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{formulas.length} формул</span>
            <button className="btn btn-primary" onClick={openFormulaCreate}>+ Формула</button>
          </div>

          {showFormulaForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingFormulaId ? 'Редактировать формулу' : 'Новая формула'}</h3>
              {!editingFormulaId && (
                <div style={formRow}>
                  <span style={label}>Тема *</span>
                  <select value={formulaForm.topicId} onChange={e => setFormulaForm(p => ({ ...p, topicId: Number(e.target.value) }))} style={{ padding: '0.5rem' }}>
                    <option value={0}>Выберите тему</option>
                    {topics.map(t => <option key={t.id} value={t.id}>{t.examTypeCode} — {t.name}</option>)}
                  </select>
                </div>
              )}
              <div style={formRow}>
                <span style={label}>Название *</span>
                <input className="form-input" value={formulaForm.title} onChange={e => setFormulaForm(p => ({ ...p, title: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Формула (KaTeX) *</span>
                <input className="form-input" value={formulaForm.formula} onChange={e => setFormulaForm(p => ({ ...p, formula: e.target.value }))} placeholder="E = mc^2" />
              </div>
              <div style={formRow}>
                <span style={label}>Описание</span>
                <textarea className="form-input" rows={3} value={formulaForm.description} onChange={e => setFormulaForm(p => ({ ...p, description: e.target.value }))} />
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveFormula}>{editingFormulaId ? 'Сохранить' : 'Создать'}</button>
                <button className="btn btn-outline" onClick={() => { setShowFormulaForm(false); setEditingFormulaId(null); }}>Отмена</button>
              </div>
            </div>
          )}

          {formulasLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
              <thead>
                <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                  <th style={{ padding: '0.5rem' }}>ID</th>
                  <th style={{ padding: '0.5rem' }}>Тема</th>
                  <th style={{ padding: '0.5rem' }}>Название</th>
                  <th style={{ padding: '0.5rem' }}>Формула</th>
                  <th style={{ padding: '0.5rem' }}></th>
                </tr>
              </thead>
              <tbody>
                {formulas.map(f => (
                  <tr key={f.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                    <td style={{ padding: '0.5rem', color: 'var(--text-secondary)' }}>#{f.id}</td>
                    <td style={{ padding: '0.5rem' }}>{f.topicName}</td>
                    <td style={{ padding: '0.5rem', fontWeight: 500 }}>{f.title}</td>
                    <td style={{ padding: '0.5rem', fontFamily: 'monospace', fontSize: '0.8rem' }}>{f.formula}</td>
                    <td style={{ padding: '0.5rem' }}>
                      <div style={{ display: 'flex', gap: '0.25rem' }}>
                        <button className="btn btn-outline" style={editBtn} onClick={() => openFormulaEdit(f)}>✎</button>
                        <button className="btn btn-outline" style={{ ...editBtn, color: 'var(--error-color)' }} onClick={() => deleteFormula(f.id)}>✕</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* ═══════ STRATEGIES TAB ═══════ */}
      {tab === 'strategies' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{strategies.length} стратегий</span>
            <button className="btn btn-primary" onClick={openStrategyCreate}>+ Стратегия</button>
          </div>

          {showStrategyForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingStrategyId ? 'Редактировать стратегию' : 'Новая стратегия'}</h3>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div style={formRow}>
                  <span style={label}>Экзамен *</span>
                  <select value={strategyForm.examTypeCode} onChange={e => setStrategyForm(p => ({ ...p, examTypeCode: e.target.value }))} style={{ padding: '0.5rem' }} disabled={!!editingStrategyId}>
                    <option value="SAT">SAT</option>
                    <option value="TOEFL">TOEFL</option>
                    <option value="NUET">NUET</option>
                    <option value="IELTS">IELTS</option>
                    <option value="CSCA">CSCA</option>
                  </select>
                </div>
                <div style={formRow}>
                  <span style={label}>Категория</span>
                  <select value={strategyForm.category} onChange={e => setStrategyForm(p => ({ ...p, category: e.target.value }))} style={{ padding: '0.5rem' }}>
                    <option value="test-taking">Сдача тестов</option>
                    <option value="time-management">Тайм-менеджмент</option>
                    <option value="section-specific">По секциям</option>
                    <option value="mental">Психология</option>
                  </select>
                </div>
              </div>
              <div style={formRow}>
                <span style={label}>Название *</span>
                <input className="form-input" value={strategyForm.title} onChange={e => setStrategyForm(p => ({ ...p, title: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Краткое описание *</span>
                <input className="form-input" value={strategyForm.summary} onChange={e => setStrategyForm(p => ({ ...p, summary: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Содержание (Markdown) *</span>
                <textarea className="form-input" rows={8} value={strategyForm.content} onChange={e => setStrategyForm(p => ({ ...p, content: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Время чтения (мин)</span>
                <input type="number" className="form-input" value={strategyForm.estimatedReadMinutes} onChange={e => setStrategyForm(p => ({ ...p, estimatedReadMinutes: Number(e.target.value) || 5 }))} style={{ width: '100px' }} />
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveStrategy}>{editingStrategyId ? 'Сохранить' : 'Создать'}</button>
                <button className="btn btn-outline" onClick={() => { setShowStrategyForm(false); setEditingStrategyId(null); }}>Отмена</button>
              </div>
            </div>
          )}

          {strategiesLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
              <thead>
                <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                  <th style={{ padding: '0.5rem' }}>ID</th>
                  <th style={{ padding: '0.5rem' }}>Экзамен</th>
                  <th style={{ padding: '0.5rem' }}>Категория</th>
                  <th style={{ padding: '0.5rem' }}>Название</th>
                  <th style={{ padding: '0.5rem' }}>Мин</th>
                  <th style={{ padding: '0.5rem' }}></th>
                </tr>
              </thead>
              <tbody>
                {strategies.map(s => (
                  <tr key={s.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                    <td style={{ padding: '0.5rem', color: 'var(--text-secondary)' }}>#{s.id}</td>
                    <td style={{ padding: '0.5rem' }}>{s.examTypeCode}</td>
                    <td style={{ padding: '0.5rem', fontSize: '0.8rem' }}>{s.category}</td>
                    <td style={{ padding: '0.5rem', fontWeight: 500 }}>{s.title}</td>
                    <td style={{ padding: '0.5rem' }}>{s.estimatedReadMinutes}</td>
                    <td style={{ padding: '0.5rem' }}>
                      <div style={{ display: 'flex', gap: '0.25rem' }}>
                        <button className="btn btn-outline" style={editBtn} onClick={() => openStrategyEdit(s)}>✎</button>
                        <button className="btn btn-outline" style={{ ...editBtn, color: 'var(--error-color)' }} onClick={() => deleteStrategy(s.id)}>✕</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* ═══════ DRILLS TAB ═══════ */}
      {tab === 'drills' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{drills.length} дриллов</span>
            <button className="btn btn-primary" onClick={openDrillCreate}>+ Дрилл</button>
          </div>

          {showDrillForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingDrillId ? 'Редактировать дрилл' : 'Новый дрилл'}</h3>
              <div style={formRow}>
                <span style={label}>Название *</span>
                <input className="form-input" value={drillForm.title} onChange={e => setDrillForm(p => ({ ...p, title: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Описание</span>
                <textarea className="form-input" rows={3} value={drillForm.description} onChange={e => setDrillForm(p => ({ ...p, description: e.target.value }))} />
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div style={formRow}>
                  <span style={label}>Тип дрилла *</span>
                  <select value={drillForm.drillType} onChange={e => setDrillForm(p => ({ ...p, drillType: e.target.value }))} style={{ padding: '0.5rem' }}>
                    <option value="Speed">Speed (кол-во вопросов)</option>
                    <option value="Marathon">Marathon (время)</option>
                    <option value="Streak">Streak (без ошибок)</option>
                  </select>
                </div>
                <div style={formRow}>
                  <span style={label}>Экзамен</span>
                  <select value={drillForm.examTypeCode} onChange={e => setDrillForm(p => ({ ...p, examTypeCode: e.target.value }))} style={{ padding: '0.5rem' }}>
                    <option value="">Без привязки</option>
                    <option value="SAT">SAT</option>
                    <option value="TOEFL">TOEFL</option>
                    <option value="NUET">NUET</option>
                    <option value="IELTS">IELTS</option>
                    <option value="CSCA">CSCA</option>
                  </select>
                </div>
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '0.75rem' }}>
                <div style={formRow}>
                  <span style={label}>Тема</span>
                  <select value={drillForm.topicId} onChange={e => setDrillForm(p => ({ ...p, topicId: Number(e.target.value) }))} style={{ padding: '0.5rem' }}>
                    <option value={0}>Без привязки</option>
                    {topics.map(t => <option key={t.id} value={t.id}>{t.examTypeCode} — {t.name}</option>)}
                  </select>
                </div>
                <div style={formRow}>
                  <span style={label}>Кол-во вопросов</span>
                  <input type="number" className="form-input" value={drillForm.questionCount} onChange={e => setDrillForm(p => ({ ...p, questionCount: Number(e.target.value) || 10 }))} />
                </div>
                <div style={formRow}>
                  <span style={label}>Лимит времени (мин)</span>
                  <input type="number" className="form-input" value={drillForm.timeLimitMinutes || ''} onChange={e => setDrillForm(p => ({ ...p, timeLimitMinutes: Number(e.target.value) || 0 }))} placeholder="0 = без лимита" />
                </div>
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div style={formRow}>
                  <span style={label}>Порядок сортировки</span>
                  <input type="number" className="form-input" value={drillForm.sortOrder} onChange={e => setDrillForm(p => ({ ...p, sortOrder: Number(e.target.value) || 0 }))} />
                </div>
                <div style={{ ...formRow, flexDirection: 'row', alignItems: 'center', gap: '0.5rem' }}>
                  <input type="checkbox" id="drillActive" checked={drillForm.isActive} onChange={e => setDrillForm(p => ({ ...p, isActive: e.target.checked }))} />
                  <label htmlFor="drillActive" style={label}>Активен</label>
                </div>
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveDrill}>{editingDrillId ? 'Сохранить' : 'Создать'}</button>
                <button className="btn btn-outline" onClick={() => { setShowDrillForm(false); setEditingDrillId(null); }}>Отмена</button>
              </div>
            </div>
          )}

          {drillsLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
              <thead>
                <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                  <th style={{ padding: '0.5rem' }}>ID</th>
                  <th style={{ padding: '0.5rem' }}>Название</th>
                  <th style={{ padding: '0.5rem' }}>Тип</th>
                  <th style={{ padding: '0.5rem' }}>Экзамен</th>
                  <th style={{ padding: '0.5rem' }}>Вопросов</th>
                  <th style={{ padding: '0.5rem' }}>Мин</th>
                  <th style={{ padding: '0.5rem' }}>Активен</th>
                  <th style={{ padding: '0.5rem' }}></th>
                </tr>
              </thead>
              <tbody>
                {drills.map(d => (
                  <tr key={d.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                    <td style={{ padding: '0.5rem', color: 'var(--text-secondary)' }}>#{d.id}</td>
                    <td style={{ padding: '0.5rem', fontWeight: 500 }}>{d.title}</td>
                    <td style={{ padding: '0.5rem' }}>{d.drillType}</td>
                    <td style={{ padding: '0.5rem' }}>{d.examTypeCode || '—'}</td>
                    <td style={{ padding: '0.5rem' }}>{d.questionCount}</td>
                    <td style={{ padding: '0.5rem' }}>{d.timeLimitMinutes ?? '—'}</td>
                    <td style={{ padding: '0.5rem' }}>{d.isActive ? '✓' : '✕'}</td>
                    <td style={{ padding: '0.5rem' }}>
                      <div style={{ display: 'flex', gap: '0.25rem' }}>
                        <button className="btn btn-outline" style={editBtn} onClick={() => openDrillEdit(d)}>✎</button>
                        <button className="btn btn-outline" style={{ ...editBtn, color: 'var(--error-color)' }} onClick={() => deleteDrill(d.id)}>✕</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}
    </div>
  );
}
