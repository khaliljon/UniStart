import { useEffect, useState, useCallback } from 'react';
import adminService from '../services/adminService';
import { lessonService } from '../services/lessonService';
import { materialsService, type StudyMaterialAdmin } from '../services/materialsService';
import type { AdminTopicSummary } from '../types';
import { useTranslation } from '../hooks/useTranslation';

type ContentTab = 'lessons' | 'flashcards' | 'formulas' | 'strategies' | 'drills' | 'materials';

type Lesson = { id: number; topicId: number; topicName: string; title: string; videoUrl: string | null; sortOrder: number; stepCount: number };
type Deck = { id: number; title: string; description: string | null; examTypeCode: string | null; topicId: number | null; topicName: string | null; isSystem: boolean; cardCount: number; createdAt: string };
type Card = { id: number; deckId: number; front: string; back: string; sortOrder: number };
type Formula = { id: number; topicId: number; topicName: string; title: string; formula: string; description: string | null; sortOrder: number };
type Strategy = { id: number; examTypeCode: string; title: string; summary: string; category: string; estimatedReadMinutes: number; sortOrder: number };
type DrillTemplate = { id: number; title: string; description: string | null; drillType: string; examTypeCode: string | null; topicId: number | null; topicName: string | null; questionCount: number; timeLimitMinutes: number | null; isActive: boolean; sortOrder: number };

const TABS: ContentTab[] = ['lessons', 'flashcards', 'formulas', 'strategies', 'drills', 'materials'];

export default function AdminContentPage() {
  const { t, locale } = useTranslation();
  const isEn = locale === 'en';
  const isKz = locale === 'kz';
  const tabLabels: Record<ContentTab, string> = {
    lessons: t.admin.content.lessons,
    flashcards: t.admin.content.flashcards,
    formulas: t.admin.content.formulas,
    strategies: t.admin.content.strategies,
    drills: t.admin.content.drillsTab,
    materials: isEn ? 'PDF Books' : isKz ? 'PDF кітаптар' : 'Учебники (PDF)',
  };
  const [tab, setTab] = useState<ContentTab>('lessons');
  const [topics, setTopics] = useState<AdminTopicSummary[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [filterExam, setFilterExam] = useState<string>('all');

  const [lessons, setLessons] = useState<Lesson[]>([]);
  const [lessonsLoading, setLessonsLoading] = useState(false);
  const [showLessonForm, setShowLessonForm] = useState(false);
  const [lessonForm, setLessonForm] = useState({ topicId: 0, title: '', content: '', videoUrl: '' });
  const [editingLessonId, setEditingLessonId] = useState<number | null>(null);

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

  const [formulas, setFormulas] = useState<Formula[]>([]);
  const [formulasLoading, setFormulasLoading] = useState(false);
  const [showFormulaForm, setShowFormulaForm] = useState(false);
  const [formulaForm, setFormulaForm] = useState({ topicId: 0, title: '', formula: '', description: '' });
  const [editingFormulaId, setEditingFormulaId] = useState<number | null>(null);

  const [strategies, setStrategies] = useState<Strategy[]>([]);
  const [strategiesLoading, setStrategiesLoading] = useState(false);
  const [showStrategyForm, setShowStrategyForm] = useState(false);
  const [strategyForm, setStrategyForm] = useState({ examTypeCode: '', title: '', summary: '', content: '', category: 'test-taking', estimatedReadMinutes: 5 });
  const [editingStrategyId, setEditingStrategyId] = useState<number | null>(null);

  const [drills, setDrills] = useState<DrillTemplate[]>([]);
  const [drillsLoading, setDrillsLoading] = useState(false);
  const [showDrillForm, setShowDrillForm] = useState(false);
  const [drillForm, setDrillForm] = useState({ title: '', description: '', drillType: 'Speed', examTypeCode: '', topicId: 0, questionCount: 10, timeLimitMinutes: 0, isActive: true, sortOrder: 0 });
  const [editingDrillId, setEditingDrillId] = useState<number | null>(null);

  const [materials, setMaterials] = useState<StudyMaterialAdmin[]>([]);
  const [materialsLoading, setMaterialsLoading] = useState(false);
  const [showMaterialForm, setShowMaterialForm] = useState(false);
  const [materialForm, setMaterialForm] = useState({ subjectKey: 'math', title: '', titleKz: '', titleEn: '', description: '', descriptionKz: '', descriptionEn: '', pdfUrl: '', price: 6990, isActive: true });
  const [editingMaterialId, setEditingMaterialId] = useState<number | null>(null);
  const [uploadingPdf, setUploadingPdf] = useState(false);
  const [uploadPdfPct, setUploadPdfPct] = useState(0);

  const [selectedLessons, setSelectedLessons] = useState<Set<number>>(new Set());
  const [selectedFormulas, setSelectedFormulas] = useState<Set<number>>(new Set());
  const [selectedStrategies, setSelectedStrategies] = useState<Set<number>>(new Set());
  const [selectedDrills, setSelectedDrills] = useState<Set<number>>(new Set());
  const [selectedDecks, setSelectedDecks] = useState<Set<number>>(new Set());
  const [selectedMaterials, setSelectedMaterials] = useState<Set<number>>(new Set());
  const [selectionMode, setSelectionMode] = useState(false);

  const [examTypeOptions, setExamTypeOptions] = useState<{ code: string; name: string }[]>([]);

  useEffect(() => {
    adminService.getTopics().then(setTopics).catch(() => {});
    adminService.getExamTypes().then(setExamTypeOptions).catch(() => {});
  }, []);

  const loadLessons = useCallback(async () => {
    setLessonsLoading(true);
    try { setLessons(await adminService.getLessons()); } catch { setError(t.admin.common.loadError); }
    finally { setLessonsLoading(false); }
  }, []);

  const loadDecks = useCallback(async () => {
    setDecksLoading(true);
    try { setDecks(await adminService.getDecks()); } catch { setError(t.admin.common.loadError); }
    finally { setDecksLoading(false); }
  }, []);

  const loadFormulas = useCallback(async () => {
    setFormulasLoading(true);
    try { setFormulas(await adminService.getFormulas()); } catch { setError(t.admin.common.loadError); }
    finally { setFormulasLoading(false); }
  }, []);

  const loadStrategies = useCallback(async () => {
    setStrategiesLoading(true);
    try { setStrategies(await adminService.getStrategies()); } catch { setError(t.admin.common.loadError); }
    finally { setStrategiesLoading(false); }
  }, []);

  const loadDrills = useCallback(async () => {
    setDrillsLoading(true);
    try { setDrills(await adminService.getDrills()); } catch { setError(t.admin.common.loadError); }
    finally { setDrillsLoading(false); }
  }, []);

  const loadMaterials = useCallback(async () => {
    setMaterialsLoading(true);
    try { setMaterials(await materialsService.adminList()); } catch { setError(t.admin.common.loadError); }
    finally { setMaterialsLoading(false); }
  }, []);

  useEffect(() => {
    setError(null);
    setSuccess(null);
    setSelectedLessons(new Set());
    setSelectedFormulas(new Set());
    setSelectedStrategies(new Set());
    setSelectedDrills(new Set());
    setSelectedDecks(new Set());
    setSelectedMaterials(new Set());
    setSelectionMode(false);
    if (tab === 'lessons') loadLessons();
    else if (tab === 'flashcards') loadDecks();
    else if (tab === 'formulas') loadFormulas();
    else if (tab === 'strategies') loadStrategies();
    else if (tab === 'drills') loadDrills();
    else if (tab === 'materials') loadMaterials();
  }, [tab, loadLessons, loadDecks, loadFormulas, loadStrategies, loadDrills, loadMaterials]);

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
    if (!lessonForm.title.trim()) { setError(t.admin.content.enterName); return; }
    try {
      if (editingLessonId) {
        await adminService.updateLesson(editingLessonId, { title: lessonForm.title, content: lessonForm.content, videoUrl: lessonForm.videoUrl || undefined });
        setSuccess(t.admin.content.lessonUpdated);
      } else {
        if (!lessonForm.topicId) { setError(t.admin.content.selectTopic); return; }
        if (!lessonForm.content.trim()) { setError(t.admin.content.fillContent); return; }
        await adminService.createLesson({ ...lessonForm, videoUrl: lessonForm.videoUrl || undefined });
        setSuccess(t.admin.content.lessonCreated);
      }
      setShowLessonForm(false);
      setEditingLessonId(null);
      loadLessons();
    } catch { setError(t.admin.content.lessonSaveError); }
  };

  const deleteLesson = async (id: number) => {
    if (!confirm(t.admin.content.lessonDeleteConfirm)) return;
    try { await adminService.deleteLesson(id); setSuccess(t.admin.content.lessonDeleted); loadLessons(); }
    catch { setError(t.admin.common.deleteError); }
  };

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
    if (!deckForm.title.trim()) { setError(t.admin.content.deckName); return; }
    try {
      if (editingDeckId) {
        await adminService.updateDeck(editingDeckId, { title: deckForm.title, description: deckForm.description || undefined, examTypeCode: deckForm.examTypeCode || undefined });
        setSuccess(t.admin.content.deckUpdated);
      } else {
        await adminService.createDeck({ title: deckForm.title, description: deckForm.description || undefined, examTypeCode: deckForm.examTypeCode || undefined });
        setSuccess(t.admin.content.deckCreated);
      }
      setShowDeckForm(false);
      setEditingDeckId(null);
      loadDecks();
    } catch { setError(t.admin.content.deckSaveError); }
  };

  const deleteDeck = async (id: number) => {
    if (!confirm(t.admin.content.deckDeleteConfirm)) return;
    try { await adminService.deleteDeck(id); setExpandedDeck(null); setSuccess(t.admin.content.deckDeleted); loadDecks(); }
    catch { setError(t.admin.common.deleteError); }
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
    if (!cardForm.front.trim() || !cardForm.back.trim()) { setError(t.admin.content.fillBothSides); return; }
    try {
      if (editingCardId) {
        await adminService.updateCard(editingCardId, { front: cardForm.front, back: cardForm.back });
        setSuccess(t.admin.content.cardUpdated);
      } else {
        if (!expandedDeck) return;
        await adminService.createCard({ deckId: expandedDeck, front: cardForm.front, back: cardForm.back });
        setSuccess(t.admin.content.cardCreated);
      }
      setShowCardForm(false);
      setEditingCardId(null);
      if (expandedDeck) setCards(await adminService.getCards(expandedDeck));
      loadDecks();
    } catch { setError(t.admin.content.cardSaveError); }
  };

  const deleteCard = async (id: number) => {
    if (!confirm(t.admin.content.cardDeleteConfirm)) return;
    try {
      await adminService.deleteCard(id);
      if (expandedDeck) setCards(await adminService.getCards(expandedDeck));
      loadDecks();
    } catch { setError(t.admin.common.deleteError); }
  };

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
    if (!formulaForm.title.trim() || !formulaForm.formula.trim()) { setError(t.admin.content.fillNameFormula); return; }
    try {
      if (editingFormulaId) {
        await adminService.updateFormula(editingFormulaId, { title: formulaForm.title, formula: formulaForm.formula, description: formulaForm.description || undefined });
        setSuccess(t.admin.content.formulaUpdated);
      } else {
        if (!formulaForm.topicId) { setError(t.admin.content.selectTopic); return; }
        await adminService.createFormula({ ...formulaForm, description: formulaForm.description || undefined });
        setSuccess(t.admin.content.formulaCreated);
      }
      setShowFormulaForm(false);
      setEditingFormulaId(null);
      loadFormulas();
    } catch { setError(t.admin.content.formulaSaveError); }
  };

  const deleteFormula = async (id: number) => {
    if (!confirm(t.admin.content.formulaDeleteConfirm)) return;
    try { await adminService.deleteFormula(id); setSuccess(t.admin.content.formulaDeleted); loadFormulas(); }
    catch { setError(t.admin.common.deleteError); }
  };

  const openStrategyCreate = () => {
    setEditingStrategyId(null);
    setStrategyForm({ examTypeCode: examTypeOptions[0]?.code || '', title: '', summary: '', content: '', category: 'test-taking', estimatedReadMinutes: 5 });
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
    if (!strategyForm.title.trim() || !strategyForm.content.trim()) { setError(t.admin.content.fillNameContent); return; }
    try {
      if (editingStrategyId) {
        await adminService.updateStrategy(editingStrategyId, { title: strategyForm.title, summary: strategyForm.summary, content: strategyForm.content, category: strategyForm.category, estimatedReadMinutes: strategyForm.estimatedReadMinutes });
        setSuccess(t.admin.content.strategyUpdated);
      } else {
        if (!strategyForm.examTypeCode) { setError(t.admin.content.selectExam); return; }
        await adminService.createStrategy(strategyForm);
        setSuccess(t.admin.content.strategyCreated);
      }
      setShowStrategyForm(false);
      setEditingStrategyId(null);
      loadStrategies();
    } catch { setError(t.admin.content.strategySaveError); }
  };

  const deleteStrategy = async (id: number) => {
    if (!confirm(t.admin.content.strategyDeleteConfirm)) return;
    try { await adminService.deleteStrategy(id); setSuccess(t.admin.content.strategyDeleted); loadStrategies(); }
    catch { setError(t.admin.common.deleteError); }
  };

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
    if (!drillForm.title.trim()) { setError(t.admin.content.enterDrillName); return; }
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
        setSuccess(t.admin.content.drillUpdated);
      } else {
        await adminService.createDrill(payload);
        setSuccess(t.admin.content.drillCreated);
      }
      setShowDrillForm(false);
      setEditingDrillId(null);
      loadDrills();
    } catch { setError(t.admin.content.drillSaveError); }
  };

  const deleteDrill = async (id: number) => {
    if (!confirm(t.admin.content.drillDeleteConfirm)) return;
    try { await adminService.deleteDrill(id); setSuccess(t.admin.content.drillDeleted); loadDrills(); }
    catch { setError(t.admin.common.deleteError); }
  };

  const bulkDeleteLessons = async () => {
    const ids = [...selectedLessons];
    if (!ids.length || !confirm(`Удалить ${ids.length} уроков?`)) return;
    try { await Promise.all(ids.map(id => adminService.deleteLesson(id))); setSelectedLessons(new Set()); setSelectionMode(false); loadLessons(); }
    catch { setError(t.admin.common.deleteError); }
  };

  const bulkDeleteFormulas = async () => {
    const ids = [...selectedFormulas];
    if (!ids.length || !confirm(`Удалить ${ids.length} формул?`)) return;
    try { await Promise.all(ids.map(id => adminService.deleteFormula(id))); setSelectedFormulas(new Set()); setSelectionMode(false); loadFormulas(); }
    catch { setError(t.admin.common.deleteError); }
  };

  const bulkDeleteStrategies = async () => {
    const ids = [...selectedStrategies];
    if (!ids.length || !confirm(`Удалить ${ids.length} стратегий?`)) return;
    try { await Promise.all(ids.map(id => adminService.deleteStrategy(id))); setSelectedStrategies(new Set()); setSelectionMode(false); loadStrategies(); }
    catch { setError(t.admin.common.deleteError); }
  };

  const bulkDeleteDrills = async () => {
    const ids = [...selectedDrills];
    if (!ids.length || !confirm(`Удалить ${ids.length} тренировок?`)) return;
    try { await Promise.all(ids.map(id => adminService.deleteDrill(id))); setSelectedDrills(new Set()); setSelectionMode(false); loadDrills(); }
    catch { setError(t.admin.common.deleteError); }
  };

  const bulkDeleteDecks = async () => {
    const ids = [...selectedDecks];
    if (!ids.length || !confirm(`Удалить ${ids.length} колод?`)) return;
    try { await Promise.all(ids.map(id => adminService.deleteDeck(id))); setSelectedDecks(new Set()); setSelectionMode(false); loadDecks(); }
    catch { setError(t.admin.common.deleteError); }
  };

  const openMaterialCreate = () => {
    setEditingMaterialId(null);
    setMaterialForm({ subjectKey: 'math', title: '', titleKz: '', titleEn: '', description: '', descriptionKz: '', descriptionEn: '', pdfUrl: '', price: 6990, isActive: true });
    setShowMaterialForm(true);
    setError(null);
  };

  const openMaterialEdit = (m: StudyMaterialAdmin) => {
    setEditingMaterialId(m.id);
    setMaterialForm({
      subjectKey: m.subjectKey,
      title: m.title,
      titleKz: m.titleKz || '',
      titleEn: m.titleEn || '',
      description: m.description || '',
      descriptionKz: m.descriptionKz || '',
      descriptionEn: m.descriptionEn || '',
      pdfUrl: m.pdfUrl || '',
      price: Number(m.price),
      isActive: m.isActive,
    });
    setShowMaterialForm(true);
    setError(null);
  };

  const saveMaterial = async () => {
    if (!materialForm.title.trim()) { setError('Введите название'); return; }
    if (!materialForm.pdfUrl.trim()) { setError('Загрузите PDF файл — без него сохранить нельзя'); return; }
    try {
      const payload = {
        subjectKey: materialForm.subjectKey,
        title: materialForm.title,
        titleKz: materialForm.titleKz || null,
        titleEn: materialForm.titleEn || null,
        description: materialForm.description || null,
        descriptionKz: materialForm.descriptionKz || null,
        descriptionEn: materialForm.descriptionEn || null,
        pdfUrl: materialForm.pdfUrl || null,
        price: materialForm.price,
        isActive: materialForm.isActive,
      };
      if (editingMaterialId) {
        await materialsService.update(editingMaterialId, payload);
        setSuccess('Учебник обновлен');
      } else {
        await materialsService.create(payload);
        setSuccess('Учебник добавлен');
      }
      setShowMaterialForm(false);
      setEditingMaterialId(null);
      loadMaterials();
    } catch {
      setError('Не удалось сохранить учебник');
    }
  };

  const deleteMaterial = async (id: number) => {
    if (!confirm('Вы уверены, что хотите удалить этот учебник?')) return;
    try {
      await materialsService.remove(id);
      setSuccess('Учебник удален');
      loadMaterials();
    } catch {
      setError(t.admin.common.deleteError);
    }
  };

  const handleUploadPdf = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploadingPdf(true);
    setUploadPdfPct(0);
    setError(null);
    try {
      const url = await materialsService.uploadPdf(file, setUploadPdfPct);
      setMaterialForm(p => ({ ...p, pdfUrl: url }));
      setSuccess('PDF файл успешно загружен');
    } catch (err: any) {
      setError(err?.response?.data?.error || err?.message || 'Ошибка при загрузке PDF');
    } finally {
      setUploadingPdf(false);
    }
  };

  const bulkDeleteMaterials = async () => {
    const ids = [...selectedMaterials];
    if (!ids.length || !confirm(`Удалить ${ids.length} учебников?`)) return;
    try {
      await Promise.all(ids.map(id => materialsService.remove(id)));
      setSelectedMaterials(new Set());
      setSelectionMode(false);
      loadMaterials();
    } catch {
      setError(t.admin.common.deleteError);
    }
  };

  const examCodes = [...new Set(topics.map(tp => tp.examTypeCode))].sort();
  const topicExamMap = new Map(topics.map(tp => [tp.id, tp.examTypeCode]));

  const filteredLessons = filterExam === 'all' ? lessons : lessons.filter(l => topicExamMap.get(l.topicId) === filterExam);
  const filteredFormulas = filterExam === 'all' ? formulas : formulas.filter(f => topicExamMap.get(f.topicId) === filterExam);
  const filteredStrategies = filterExam === 'all' ? strategies : strategies.filter(s => s.examTypeCode === filterExam);
  const filteredDecks = filterExam === 'all' ? decks : decks.filter(d => d.examTypeCode === filterExam);
  const filteredDrills = filterExam === 'all' ? drills : drills.filter(d => d.examTypeCode === filterExam);
  const filteredMaterials = materials;

  const formRow: React.CSSProperties = { display: 'flex', flexDirection: 'column', gap: '0.5rem', marginBottom: '0.75rem' };
  const label: React.CSSProperties = { fontSize: '0.85rem', fontWeight: 500, color: 'var(--text-secondary)' };
  const editBtn: React.CSSProperties = { fontSize: '0.75rem', padding: '0.25rem 0.5rem', cursor: 'pointer' };

  return (
    <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
      <h1 style={{ margin: '0 0 0.25rem' }}>{t.admin.content.title}</h1>
      <p style={{ margin: '0 0 1.5rem', color: 'var(--text-secondary)' }}>{t.admin.content.subtitle}</p>

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

      <div style={{ display: 'flex', gap: '1rem', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        <div style={{ display: 'flex', borderRadius: '8px', overflow: 'hidden', border: '1px solid var(--border-color)', width: 'fit-content' }}>
          {TABS.map((tabKey, i) => (
            <button
              key={tabKey}
              onClick={() => setTab(tabKey)}
              style={{
                padding: '0.5rem 1rem', border: 'none', cursor: 'pointer', fontSize: '0.85rem',
                background: tab === tabKey ? 'var(--primary-color)' : 'var(--card-background)',
                color: tab === tabKey ? '#fff' : 'var(--text-secondary)',
                borderLeft: i > 0 ? '1px solid var(--border-color)' : 'none',
              }}
            >
              {tabLabels[tabKey]}
            </button>
          ))}
        </div>
        <select
          value={filterExam}
          onChange={e => setFilterExam(e.target.value)}
          style={{ padding: '0.5rem 0.75rem', borderRadius: '8px', border: '1px solid var(--border-color)', fontSize: '0.85rem', background: 'var(--card-background)', color: 'var(--text-primary)' }}
        >
          <option value="all">{t.admin.common.allExams}</option>
          {examCodes.map(code => <option key={code} value={code}>{code}</option>)}
        </select>
      </div>

      {tab === 'lessons' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{filteredLessons.length} {t.admin.content.lessonsCount}</span>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button className="btn btn-outline" style={{ fontSize: '0.85rem' }} onClick={() => { setSelectionMode(m => !m); setSelectedLessons(new Set()); }}>
                {selectionMode ? 'Отменить' : 'Выбрать несколько'}
              </button>
              <button className="btn btn-primary" onClick={openLessonCreate}>{t.admin.content.addLesson}</button>
            </div>
          </div>

          {showLessonForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingLessonId ? t.admin.content.editLesson : t.admin.content.newLesson}</h3>
              {!editingLessonId && (
                <div style={formRow}>
                  <span style={label}>{t.admin.content.topic} *</span>
                  <select value={lessonForm.topicId} onChange={e => setLessonForm(p => ({ ...p, topicId: Number(e.target.value) }))} style={{ padding: '0.5rem' }}>
                    <option value={0}>{t.admin.content.selectTopic}</option>
                    {topics.map(tp => <option key={tp.id} value={tp.id}>{tp.examTypeCode} — {tp.name}</option>)}
                  </select>
                </div>
              )}
              <div style={formRow}>
                <span style={label}>{t.admin.content.name} *</span>
                <input className="form-input" value={lessonForm.title} onChange={e => setLessonForm(p => ({ ...p, title: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>{t.admin.content.contentMd}</span>
                <textarea className="form-input" rows={6} value={lessonForm.content} onChange={e => setLessonForm(p => ({ ...p, content: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>{t.admin.content.videoUrl}</span>
                <input className="form-input" value={lessonForm.videoUrl} onChange={e => setLessonForm(p => ({ ...p, videoUrl: e.target.value }))} placeholder="https://..." />
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveLesson}>{editingLessonId ? t.admin.common.save : t.admin.common.create}</button>
                <button className="btn btn-outline" onClick={() => { setShowLessonForm(false); setEditingLessonId(null); }}>{t.admin.common.cancel}</button>
              </div>
            </div>
          )}

          {lessonsLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>{t.admin.common.loading}</div>
          ) : (
            <>
              {selectionMode && selectedLessons.size > 0 && (
                <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', padding: '0.5rem 0.75rem', marginBottom: '0.5rem', background: 'var(--primary-color)', borderRadius: '8px', color: '#fff', fontSize: '0.85rem' }}>
                  <span>Выбрано: {selectedLessons.size}</span>
                  <button className="btn" style={{ fontSize: '0.8rem', padding: '0.2rem 0.6rem', background: 'rgba(255,255,255,0.2)', border: '1px solid rgba(255,255,255,0.5)', color: '#fff', cursor: 'pointer' }} onClick={bulkDeleteLessons}>Удалить выбранные</button>
                  <button style={{ marginLeft: 'auto', background: 'none', border: 'none', color: '#fff', cursor: 'pointer', fontSize: '1rem' }} onClick={() => { setSelectedLessons(new Set()); setSelectionMode(false); }}>✕</button>
                </div>
              )}
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
                <thead>
                  <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                    {selectionMode && <th style={{ padding: '0.5rem', width: '2rem' }}>
                      <input type="checkbox" checked={filteredLessons.length > 0 && filteredLessons.every(l => selectedLessons.has(l.id))} onChange={e => setSelectedLessons(e.target.checked ? new Set(filteredLessons.map(l => l.id)) : new Set())} />
                    </th>}
                    <th style={{ padding: '0.5rem' }}>ID</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.exam}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.topic}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.name}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.steps}</th>
                    <th style={{ padding: '0.5rem' }}></th>
                  </tr>
                </thead>
                <tbody>
                  {filteredLessons.map(l => (
                    <tr key={l.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                      {selectionMode && <td style={{ padding: '0.5rem' }}>
                        <input type="checkbox" checked={selectedLessons.has(l.id)} onChange={e => setSelectedLessons(prev => { const s = new Set(prev); e.target.checked ? s.add(l.id) : s.delete(l.id); return s; })} />
                      </td>}
                      <td style={{ padding: '0.5rem', color: 'var(--text-secondary)' }}>#{l.id}</td>
                      <td style={{ padding: '0.5rem', fontSize: '0.8rem' }}>{topicExamMap.get(l.topicId) || '—'}</td>
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
            </>
          )}
        </div>
      )}

      {tab === 'flashcards' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{filteredDecks.length} {t.admin.content.decksCount}</span>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button className="btn btn-outline" style={{ fontSize: '0.85rem' }} onClick={() => { setSelectionMode(m => !m); setSelectedDecks(new Set()); }}>
                {selectionMode ? 'Отменить' : 'Выбрать несколько'}
              </button>
              <button className="btn btn-primary" onClick={openDeckCreate}>{t.admin.content.addDeck}</button>
            </div>
          </div>

          {showDeckForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingDeckId ? t.admin.content.editDeck : t.admin.content.newDeck}</h3>
              <div style={formRow}>
                <span style={label}>{t.admin.content.name} *</span>
                <input className="form-input" value={deckForm.title} onChange={e => setDeckForm(p => ({ ...p, title: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>{t.admin.content.description}</span>
                <input className="form-input" value={deckForm.description} onChange={e => setDeckForm(p => ({ ...p, description: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>{t.admin.content.exam}</span>
                <select value={deckForm.examTypeCode} onChange={e => setDeckForm(p => ({ ...p, examTypeCode: e.target.value }))} style={{ padding: '0.5rem' }}>
                  <option value="">{t.admin.content.noBinding}</option>
                  {examTypeOptions.map(et => <option key={et.code} value={et.code}>{et.code}</option>)}
                </select>
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveDeck}>{editingDeckId ? t.admin.common.save : t.admin.common.create}</button>
                <button className="btn btn-outline" onClick={() => { setShowDeckForm(false); setEditingDeckId(null); }}>{t.admin.common.cancel}</button>
              </div>
            </div>
          )}

          {decksLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>{t.admin.common.loading}</div>
          ) : (
            <>
              {selectionMode && selectedDecks.size > 0 && (
                <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', padding: '0.5rem 0.75rem', marginBottom: '0.5rem', background: 'var(--primary-color)', borderRadius: '8px', color: '#fff', fontSize: '0.85rem' }}>
                  <span>Выбрано: {selectedDecks.size}</span>
                  <button className="btn" style={{ fontSize: '0.8rem', padding: '0.2rem 0.6rem', background: 'rgba(255,255,255,0.2)', border: '1px solid rgba(255,255,255,0.5)', color: '#fff', cursor: 'pointer' }} onClick={bulkDeleteDecks}>Удалить выбранные</button>
                  <button style={{ marginLeft: 'auto', background: 'none', border: 'none', color: '#fff', cursor: 'pointer', fontSize: '1rem' }} onClick={() => { setSelectedDecks(new Set()); setSelectionMode(false); }}>✕</button>
                </div>
              )}
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                {filteredDecks.map(d => (
                  <div key={d.id} className="card" style={{ padding: '1rem' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', cursor: 'pointer' }} onClick={() => toggleDeck(d.id)}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                        {selectionMode && <input type="checkbox" checked={selectedDecks.has(d.id)} onChange={e => { e.stopPropagation(); setSelectedDecks(prev => { const s = new Set(prev); e.target.checked ? s.add(d.id) : s.delete(d.id); return s; }); }} onClick={e => e.stopPropagation()} />}
                        <div>
                          <div style={{ fontWeight: 600 }}>{d.title}</div>
                          <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                            {d.examTypeCode || '—'} • {d.cardCount} {t.admin.content.cardsLabel} {d.topicName ? `• ${d.topicName}` : ''}
                          </div>
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
                        <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{t.admin.content.cardsLabel} ({cards.length})</span>
                        <button className="btn btn-outline" style={{ fontSize: '0.8rem' }} onClick={openCardCreate}>{t.admin.content.addCard}</button>
                      </div>

                      {showCardForm && (
                        <div style={{ marginBottom: '1rem', padding: '1rem', background: 'var(--bg-secondary)', borderRadius: '8px' }}>
                          <h4 style={{ margin: '0 0 0.75rem', fontSize: '0.9rem' }}>{editingCardId ? t.admin.content.editCard : t.admin.content.newCard}</h4>
                          <div style={formRow}>
                            <span style={label}>{t.admin.content.cardFront}</span>
                            <input className="form-input" value={cardForm.front} onChange={e => setCardForm(p => ({ ...p, front: e.target.value }))} />
                          </div>
                          <div style={formRow}>
                            <span style={label}>{t.admin.content.cardBack}</span>
                            <textarea className="form-input" rows={3} value={cardForm.back} onChange={e => setCardForm(p => ({ ...p, back: e.target.value }))} />
                          </div>
                          <div style={{ display: 'flex', gap: '0.5rem' }}>
                            <button className="btn btn-primary" style={{ fontSize: '0.8rem' }} onClick={saveCard}>{editingCardId ? t.admin.common.save : t.admin.common.create}</button>
                            <button className="btn btn-outline" style={{ fontSize: '0.8rem' }} onClick={() => { setShowCardForm(false); setEditingCardId(null); }}>{t.admin.common.cancel}</button>
                          </div>
                        </div>
                      )}

                      {cards.length === 0 ? (
                        <div style={{ textAlign: 'center', padding: '1rem', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{t.admin.content.noCards}</div>
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
            </>
          )}
        </div>
      )}

      {tab === 'formulas' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{filteredFormulas.length} {t.admin.content.formulasCount}</span>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button className="btn btn-outline" style={{ fontSize: '0.85rem' }} onClick={() => { setSelectionMode(m => !m); setSelectedFormulas(new Set()); }}>
                {selectionMode ? 'Отменить' : 'Выбрать несколько'}
              </button>
              <button className="btn btn-primary" onClick={openFormulaCreate}>{t.admin.content.addFormula}</button>
            </div>
          </div>

          {showFormulaForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingFormulaId ? t.admin.content.editFormula : t.admin.content.newFormula}</h3>
              {!editingFormulaId && (
                <div style={formRow}>
                  <span style={label}>{t.admin.content.topic} *</span>
                  <select value={formulaForm.topicId} onChange={e => setFormulaForm(p => ({ ...p, topicId: Number(e.target.value) }))} style={{ padding: '0.5rem' }}>
                    <option value={0}>{t.admin.content.selectTopic}</option>
                    {topics.map(tp => <option key={tp.id} value={tp.id}>{tp.examTypeCode} — {tp.name}</option>)}
                  </select>
                </div>
              )}
              <div style={formRow}>
                <span style={label}>{t.admin.content.name} *</span>
                <input className="form-input" value={formulaForm.title} onChange={e => setFormulaForm(p => ({ ...p, title: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>{t.admin.content.formulaKatex}</span>
                <input className="form-input" value={formulaForm.formula} onChange={e => setFormulaForm(p => ({ ...p, formula: e.target.value }))} placeholder="E = mc^2" />
              </div>
              <div style={formRow}>
                <span style={label}>{t.admin.content.description}</span>
                <textarea className="form-input" rows={3} value={formulaForm.description} onChange={e => setFormulaForm(p => ({ ...p, description: e.target.value }))} />
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveFormula}>{editingFormulaId ? t.admin.common.save : t.admin.common.create}</button>
                <button className="btn btn-outline" onClick={() => { setShowFormulaForm(false); setEditingFormulaId(null); }}>{t.admin.common.cancel}</button>
              </div>
            </div>
          )}

          {formulasLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>{t.admin.common.loading}</div>
          ) : (
            <>
              {selectionMode && selectedFormulas.size > 0 && (
                <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', padding: '0.5rem 0.75rem', marginBottom: '0.5rem', background: 'var(--primary-color)', borderRadius: '8px', color: '#fff', fontSize: '0.85rem' }}>
                  <span>Выбрано: {selectedFormulas.size}</span>
                  <button className="btn" style={{ fontSize: '0.8rem', padding: '0.2rem 0.6rem', background: 'rgba(255,255,255,0.2)', border: '1px solid rgba(255,255,255,0.5)', color: '#fff', cursor: 'pointer' }} onClick={bulkDeleteFormulas}>Удалить выбранные</button>
                  <button style={{ marginLeft: 'auto', background: 'none', border: 'none', color: '#fff', cursor: 'pointer', fontSize: '1rem' }} onClick={() => { setSelectedFormulas(new Set()); setSelectionMode(false); }}>✕</button>
                </div>
              )}
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
                <thead>
                  <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                    {selectionMode && <th style={{ padding: '0.5rem', width: '2rem' }}>
                      <input type="checkbox" checked={filteredFormulas.length > 0 && filteredFormulas.every(f => selectedFormulas.has(f.id))} onChange={e => setSelectedFormulas(e.target.checked ? new Set(filteredFormulas.map(f => f.id)) : new Set())} />
                    </th>}
                    <th style={{ padding: '0.5rem' }}>ID</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.exam}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.topic}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.name}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.formulaCol}</th>
                    <th style={{ padding: '0.5rem' }}></th>
                  </tr>
                </thead>
                <tbody>
                  {filteredFormulas.map(f => (
                    <tr key={f.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                      {selectionMode && <td style={{ padding: '0.5rem' }}>
                        <input type="checkbox" checked={selectedFormulas.has(f.id)} onChange={e => setSelectedFormulas(prev => { const s = new Set(prev); e.target.checked ? s.add(f.id) : s.delete(f.id); return s; })} />
                      </td>}
                      <td style={{ padding: '0.5rem', color: 'var(--text-secondary)' }}>#{f.id}</td>
                      <td style={{ padding: '0.5rem', fontSize: '0.8rem' }}>{topicExamMap.get(f.topicId) || '—'}</td>
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
            </>
          )}
        </div>
      )}

      {tab === 'strategies' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{filteredStrategies.length} {t.admin.content.strategiesCount}</span>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button className="btn btn-outline" style={{ fontSize: '0.85rem' }} onClick={() => { setSelectionMode(m => !m); setSelectedStrategies(new Set()); }}>
                {selectionMode ? 'Отменить' : 'Выбрать несколько'}
              </button>
              <button className="btn btn-primary" onClick={openStrategyCreate}>{t.admin.content.addStrategy}</button>
            </div>
          </div>

          {showStrategyForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingStrategyId ? t.admin.content.editStrategy : t.admin.content.newStrategy}</h3>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div style={formRow}>
                  <span style={label}>{t.admin.content.exam} *</span>
                  <select value={strategyForm.examTypeCode} onChange={e => setStrategyForm(p => ({ ...p, examTypeCode: e.target.value }))} style={{ padding: '0.5rem' }} disabled={!!editingStrategyId}>
                    {examTypeOptions.map(et => <option key={et.code} value={et.code}>{et.code}</option>)}
                  </select>
                </div>
                <div style={formRow}>
                  <span style={label}>{t.admin.content.category}</span>
                  <select value={strategyForm.category} onChange={e => setStrategyForm(p => ({ ...p, category: e.target.value }))} style={{ padding: '0.5rem' }}>
                    <option value="test-taking">{t.admin.content.categoryTestTaking}</option>
                    <option value="time-management">{t.admin.content.categoryTimeManagement}</option>
                    <option value="section-specific">{t.admin.content.categorySections}</option>
                    <option value="mental">{t.admin.content.categoryPsychology}</option>
                  </select>
                </div>
              </div>
              <div style={formRow}>
                <span style={label}>{t.admin.content.name} *</span>
                <input className="form-input" value={strategyForm.title} onChange={e => setStrategyForm(p => ({ ...p, title: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>{t.admin.content.briefDesc}</span>
                <input className="form-input" value={strategyForm.summary} onChange={e => setStrategyForm(p => ({ ...p, summary: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>{t.admin.content.contentMd}</span>
                <textarea className="form-input" rows={8} value={strategyForm.content} onChange={e => setStrategyForm(p => ({ ...p, content: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>{t.admin.content.readTime}</span>
                <input type="number" className="form-input" value={strategyForm.estimatedReadMinutes} onChange={e => setStrategyForm(p => ({ ...p, estimatedReadMinutes: Number(e.target.value) || 5 }))} style={{ width: '100px' }} />
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveStrategy}>{editingStrategyId ? t.admin.common.save : t.admin.common.create}</button>
                <button className="btn btn-outline" onClick={() => { setShowStrategyForm(false); setEditingStrategyId(null); }}>{t.admin.common.cancel}</button>
              </div>
            </div>
          )}

          {strategiesLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>{t.admin.common.loading}</div>
          ) : (
            <>
              {selectionMode && selectedStrategies.size > 0 && (
                <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', padding: '0.5rem 0.75rem', marginBottom: '0.5rem', background: 'var(--primary-color)', borderRadius: '8px', color: '#fff', fontSize: '0.85rem' }}>
                  <span>Выбрано: {selectedStrategies.size}</span>
                  <button className="btn" style={{ fontSize: '0.8rem', padding: '0.2rem 0.6rem', background: 'rgba(255,255,255,0.2)', border: '1px solid rgba(255,255,255,0.5)', color: '#fff', cursor: 'pointer' }} onClick={bulkDeleteStrategies}>Удалить выбранные</button>
                  <button style={{ marginLeft: 'auto', background: 'none', border: 'none', color: '#fff', cursor: 'pointer', fontSize: '1rem' }} onClick={() => { setSelectedStrategies(new Set()); setSelectionMode(false); }}>✕</button>
                </div>
              )}
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
                <thead>
                  <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                    {selectionMode && <th style={{ padding: '0.5rem', width: '2rem' }}>
                      <input type="checkbox" checked={filteredStrategies.length > 0 && filteredStrategies.every(s => selectedStrategies.has(s.id))} onChange={e => setSelectedStrategies(e.target.checked ? new Set(filteredStrategies.map(s => s.id)) : new Set())} />
                    </th>}
                    <th style={{ padding: '0.5rem' }}>ID</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.exam}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.category}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.name}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.min}</th>
                    <th style={{ padding: '0.5rem' }}></th>
                  </tr>
                </thead>
                <tbody>
                  {filteredStrategies.map(s => (
                    <tr key={s.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                      {selectionMode && <td style={{ padding: '0.5rem' }}>
                        <input type="checkbox" checked={selectedStrategies.has(s.id)} onChange={e => setSelectedStrategies(prev => { const st = new Set(prev); e.target.checked ? st.add(s.id) : st.delete(s.id); return st; })} />
                      </td>}
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
            </>
          )}
        </div>
      )}

      {tab === 'drills' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{filteredDrills.length} {t.admin.content.drillsCount}</span>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button className="btn btn-outline" style={{ fontSize: '0.85rem' }} onClick={() => { setSelectionMode(m => !m); setSelectedDrills(new Set()); }}>
                {selectionMode ? 'Отменить' : 'Выбрать несколько'}
              </button>
              <button className="btn btn-primary" onClick={openDrillCreate}>{t.admin.content.addDrill}</button>
            </div>
          </div>

          {showDrillForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingDrillId ? t.admin.content.editDrill : t.admin.content.newDrill}</h3>
              <div style={formRow}>
                <span style={label}>{t.admin.content.name} *</span>
                <input className="form-input" value={drillForm.title} onChange={e => setDrillForm(p => ({ ...p, title: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>{t.admin.content.description}</span>
                <textarea className="form-input" rows={3} value={drillForm.description} onChange={e => setDrillForm(p => ({ ...p, description: e.target.value }))} />
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div style={formRow}>
                  <span style={label}>{t.admin.content.drillType} *</span>
                  <select value={drillForm.drillType} onChange={e => setDrillForm(p => ({ ...p, drillType: e.target.value }))} style={{ padding: '0.5rem' }}>
                    <option value="Speed">{t.admin.content.drillTypeSpeed}</option>
                    <option value="Marathon">{t.admin.content.drillTypeMarathon}</option>
                    <option value="Streak">{t.admin.content.drillTypeStreak}</option>
                  </select>
                </div>
                <div style={formRow}>
                  <span style={label}>{t.admin.content.exam}</span>
                  <select value={drillForm.examTypeCode} onChange={e => setDrillForm(p => ({ ...p, examTypeCode: e.target.value }))} style={{ padding: '0.5rem' }}>
                    <option value="">{t.admin.content.noBinding}</option>
                    {examTypeOptions.map(et => <option key={et.code} value={et.code}>{et.code}</option>)}
                  </select>
                </div>
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '0.75rem' }}>
                <div style={formRow}>
                  <span style={label}>{t.admin.content.topic}</span>
                  <select value={drillForm.topicId} onChange={e => setDrillForm(p => ({ ...p, topicId: Number(e.target.value) }))} style={{ padding: '0.5rem' }}>
                    <option value={0}>{t.admin.content.noBinding}</option>
                    {topics.map(tp => <option key={tp.id} value={tp.id}>{tp.examTypeCode} — {tp.name}</option>)}
                  </select>
                </div>
                <div style={formRow}>
                  <span style={label}>{t.admin.content.drillQuestionCount}</span>
                  <input type="number" className="form-input" value={drillForm.questionCount} onChange={e => setDrillForm(p => ({ ...p, questionCount: Number(e.target.value) || 10 }))} />
                </div>
                <div style={formRow}>
                  <span style={label}>{t.admin.content.drillTimeLimit}</span>
                  <input type="number" className="form-input" value={drillForm.timeLimitMinutes || ''} onChange={e => setDrillForm(p => ({ ...p, timeLimitMinutes: Number(e.target.value) || 0 }))} placeholder={t.admin.content.noTimeLimit} />
                </div>
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                <div style={formRow}>
                  <span style={label}>{t.admin.content.sortOrder}</span>
                  <input type="number" className="form-input" value={drillForm.sortOrder} onChange={e => setDrillForm(p => ({ ...p, sortOrder: Number(e.target.value) || 0 }))} />
                </div>
                <div style={{ ...formRow, flexDirection: 'row', alignItems: 'center', gap: '0.5rem' }}>
                  <input type="checkbox" id="drillActive" checked={drillForm.isActive} onChange={e => setDrillForm(p => ({ ...p, isActive: e.target.checked }))} />
                  <label htmlFor="drillActive" style={label}>{t.admin.content.active}</label>
                </div>
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" onClick={saveDrill}>{editingDrillId ? t.admin.common.save : t.admin.common.create}</button>
                <button className="btn btn-outline" onClick={() => { setShowDrillForm(false); setEditingDrillId(null); }}>{t.admin.common.cancel}</button>
              </div>
            </div>
          )}

          {drillsLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>{t.admin.common.loading}</div>
          ) : (
            <>
              {selectionMode && selectedDrills.size > 0 && (
                <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', padding: '0.5rem 0.75rem', marginBottom: '0.5rem', background: 'var(--primary-color)', borderRadius: '8px', color: '#fff', fontSize: '0.85rem' }}>
                  <span>Выбрано: {selectedDrills.size}</span>
                  <button className="btn" style={{ fontSize: '0.8rem', padding: '0.2rem 0.6rem', background: 'rgba(255,255,255,0.2)', border: '1px solid rgba(255,255,255,0.5)', color: '#fff', cursor: 'pointer' }} onClick={bulkDeleteDrills}>Удалить выбранные</button>
                  <button style={{ marginLeft: 'auto', background: 'none', border: 'none', color: '#fff', cursor: 'pointer', fontSize: '1rem' }} onClick={() => { setSelectedDrills(new Set()); setSelectionMode(false); }}>✕</button>
                </div>
              )}
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
                <thead>
                  <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                    {selectionMode && <th style={{ padding: '0.5rem', width: '2rem' }}>
                      <input type="checkbox" checked={filteredDrills.length > 0 && filteredDrills.every(d => selectedDrills.has(d.id))} onChange={e => setSelectedDrills(e.target.checked ? new Set(filteredDrills.map(d => d.id)) : new Set())} />
                    </th>}
                    <th style={{ padding: '0.5rem' }}>ID</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.name}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.drillType}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.exam}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.questionsCount}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.min}</th>
                    <th style={{ padding: '0.5rem' }}>{t.admin.content.active}</th>
                    <th style={{ padding: '0.5rem' }}></th>
                  </tr>
                </thead>
                <tbody>
                  {filteredDrills.map(d => (
                    <tr key={d.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                      {selectionMode && <td style={{ padding: '0.5rem' }}>
                        <input type="checkbox" checked={selectedDrills.has(d.id)} onChange={e => setSelectedDrills(prev => { const s = new Set(prev); e.target.checked ? s.add(d.id) : s.delete(d.id); return s; })} />
                      </td>}
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
            </>
          )}
        </div>
      )}

      {tab === 'materials' && (
        <div>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{filteredMaterials.length} учебников</span>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button className="btn btn-outline" style={{ fontSize: '0.85rem' }} onClick={() => { setSelectionMode(m => !m); setSelectedMaterials(new Set()); }}>
                {selectionMode ? 'Отменить' : 'Выбрать несколько'}
              </button>
              <button className="btn btn-primary" onClick={openMaterialCreate}>Добавить учебник</button>
            </div>
          </div>

          {showMaterialForm && (
            <div className="card" style={{ marginBottom: '1rem', padding: '1.5rem' }}>
              <h3 style={{ margin: '0 0 1rem' }}>{editingMaterialId ? 'Редактировать учебник' : 'Новый учебник'}</h3>
              <div style={formRow}>
                <span style={label}>Предмет *</span>
                <select value={materialForm.subjectKey} onChange={e => setMaterialForm(p => ({ ...p, subjectKey: e.target.value }))} style={{ padding: '0.5rem', width: '100%', borderRadius: '4px', border: '1px solid var(--border-color)', background: 'var(--card-background)', color: 'var(--text-primary)' }}>
                  <option value="math">Математика</option>
                  <option value="physics">Физика</option>
                  <option value="chemistry">Химия</option>
                  <option value="chineseTech">Технический китайский</option>
                  <option value="chineseHum">Гуманитарный китайский</option>
                </select>
              </div>
              <div style={formRow}>
                <span style={label}>Название учебника *</span>
                <input className="form-input" value={materialForm.title} onChange={e => setMaterialForm(p => ({ ...p, title: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Название (KZ)</span>
                <input className="form-input" value={materialForm.titleKz} onChange={e => setMaterialForm(p => ({ ...p, titleKz: e.target.value }))} placeholder="Оставьте пустым — покажется русское" />
              </div>
              <div style={formRow}>
                <span style={label}>Название (EN)</span>
                <input className="form-input" value={materialForm.titleEn} onChange={e => setMaterialForm(p => ({ ...p, titleEn: e.target.value }))} placeholder="Leave empty to fall back to Russian" />
              </div>
              <div style={formRow}>
                <span style={label}>Описание</span>
                <textarea className="form-input" rows={3} value={materialForm.description} onChange={e => setMaterialForm(p => ({ ...p, description: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Описание (KZ)</span>
                <textarea className="form-input" rows={3} value={materialForm.descriptionKz} onChange={e => setMaterialForm(p => ({ ...p, descriptionKz: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Описание (EN)</span>
                <textarea className="form-input" rows={3} value={materialForm.descriptionEn} onChange={e => setMaterialForm(p => ({ ...p, descriptionEn: e.target.value }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Цена (₸) *</span>
                <input type="number" className="form-input" value={materialForm.price} onChange={e => setMaterialForm(p => ({ ...p, price: Number(e.target.value) }))} />
              </div>
              <div style={formRow}>
                <span style={label}>Загрузить PDF файл</span>
                <input type="file" accept=".pdf" onChange={handleUploadPdf} disabled={uploadingPdf} style={{ marginBottom: '0.5rem' }} />
                {uploadingPdf && <span style={{ fontSize: '0.8rem', color: 'var(--primary-color)' }}>Загрузка файла в хранилище… {uploadPdfPct}%</span>}
                <span style={label}>URL PDF файла</span>
                <input className="form-input" value={materialForm.pdfUrl} onChange={e => setMaterialForm(p => ({ ...p, pdfUrl: e.target.value }))} placeholder="https://..." />
                {!materialForm.pdfUrl.trim() && (
                  <span style={{ fontSize: '0.8rem', color: '#ef4444', marginTop: '0.35rem' }}>
                    ⚠ Загрузите PDF файл — без него сохранить нельзя.
                  </span>
                )}
              </div>
              <div style={formRow}>
                <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer', fontSize: '0.9rem' }}>
                  <input type="checkbox" checked={materialForm.isActive} onChange={e => setMaterialForm(p => ({ ...p, isActive: e.target.checked }))} />
                  Активен (виден пользователям)
                </label>
              </div>
              <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem' }}>
                <button className="btn btn-primary" onClick={saveMaterial} disabled={uploadingPdf || !materialForm.pdfUrl.trim()}>{editingMaterialId ? 'Сохранить' : 'Создать'}</button>
                <button className="btn btn-outline" onClick={() => { setShowMaterialForm(false); setEditingMaterialId(null); }}>Отмена</button>
              </div>
            </div>
          )}

          {materialsLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
          ) : (
            <>
              {selectionMode && selectedMaterials.size > 0 && (
                <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', padding: '0.5rem 0.75rem', marginBottom: '0.5rem', background: 'var(--primary-color)', borderRadius: '8px', color: '#fff', fontSize: '0.85rem' }}>
                  <span>Выбрано: {selectedMaterials.size}</span>
                  <button className="btn" style={{ fontSize: '0.8rem', padding: '0.2rem 0.6rem', background: 'rgba(255,255,255,0.2)', border: '1px solid rgba(255,255,255,0.5)', color: '#fff', cursor: 'pointer' }} onClick={bulkDeleteMaterials}>Удалить выбранные</button>
                  <button style={{ marginLeft: 'auto', background: 'none', border: 'none', color: '#fff', cursor: 'pointer', fontSize: '1rem' }} onClick={() => { setSelectedMaterials(new Set()); setSelectionMode(false); }}>✕</button>
                </div>
              )}
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
                <thead>
                  <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                    {selectionMode && <th style={{ padding: '0.5rem', width: '2rem' }}>
                      <input type="checkbox" checked={filteredMaterials.length > 0 && filteredMaterials.every(m => selectedMaterials.has(m.id))} onChange={e => setSelectedMaterials(e.target.checked ? new Set(filteredMaterials.map(m => m.id)) : new Set())} />
                    </th>}
                    <th style={{ padding: '0.5rem' }}>ID</th>
                    <th style={{ padding: '0.5rem' }}>Предмет</th>
                    <th style={{ padding: '0.5rem' }}>Название</th>
                    <th style={{ padding: '0.5rem' }}>Цена</th>
                    <th style={{ padding: '0.5rem' }}>PDF Файл</th>
                    <th style={{ padding: '0.5rem' }}>Активен</th>
                    <th style={{ padding: '0.5rem' }}></th>
                  </tr>
                </thead>
                <tbody>
                  {filteredMaterials.map(m => (
                    <tr key={m.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                      {selectionMode && <td style={{ padding: '0.5rem' }}>
                        <input type="checkbox" checked={selectedMaterials.has(m.id)} onChange={e => setSelectedMaterials(prev => { const s = new Set(prev); e.target.checked ? s.add(m.id) : s.delete(m.id); return s; })} />
                      </td>}
                      <td style={{ padding: '0.5rem', color: 'var(--text-secondary)' }}>#{m.id}</td>
                      <td style={{ padding: '0.5rem', fontWeight: 600 }}>{m.subjectKey}</td>
                      <td style={{ padding: '0.5rem' }}>{m.title}</td>
                      <td style={{ padding: '0.5rem' }}>{Number(m.price).toLocaleString('ru-RU')} ₸</td>
                      <td style={{ padding: '0.5rem' }}>
                        {m.pdfUrl ? (
                          <a href={m.pdfUrl} target="_blank" rel="noreferrer" style={{ color: 'var(--primary-color)', textDecoration: 'underline' }}>PDF файл</a>
                        ) : '—'}
                      </td>
                      <td style={{ padding: '0.5rem' }}>{m.isActive ? '✓' : '✕'}</td>
                      <td style={{ padding: '0.5rem' }}>
                        <div style={{ display: 'flex', gap: '0.25rem' }}>
                          <button className="btn btn-outline" style={editBtn} onClick={() => openMaterialEdit(m)}>✎</button>
                          <button className="btn btn-outline" style={{ ...editBtn, color: 'var(--error-color)' }} onClick={() => deleteMaterial(m.id)}>✕</button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </>
          )}
        </div>
      )}
    </div>
  );
}
