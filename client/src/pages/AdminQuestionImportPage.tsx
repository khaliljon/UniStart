import { useEffect, useState, useCallback, useRef } from 'react';
import { questionImportService } from '../services/questionImportService';
import type { QuestionImportJob, ImportedQuestionDraft, UpdateDraftPayload, FileRole, BatchFileEntry, DraftOption } from '../services/questionImportService';
import adminService from '../services/adminService';
import type { AdminSection, AdminTopicSummary } from '../types';
import { isChineseOnlySubject } from '../utils/subject';
import { useTranslation } from '../hooks/useTranslation';

const EXAM_TYPES = ['CSCA'];

const STATUS_COLORS: Record<string, string> = {
  Pending: 'var(--warning-color)',
  Processing: 'var(--primary-color)',
  Completed: 'var(--success-color)',
  Failed: 'var(--error-color)',
  PartiallyCompleted: 'var(--warning-color)',
};

const DRAFT_STATUS_COLORS: Record<string, string> = {
  Pending: 'var(--warning-color)',
  Approved: 'var(--success-color)',
  Rejected: 'var(--error-color)',
};

function AdminQuestionImportPage({ embedded = false }: { embedded?: boolean } = {}) {
  const { t } = useTranslation();
  const roleLabels: Record<string, string> = { Questions: t.admin.questionImport.roleQuestions, Answers: t.admin.questionImport.roleAnswers, Mixed: t.admin.questionImport.roleMixed };

  const [jobs, setJobs] = useState<QuestionImportJob[]>([]);
  const [selectedJob, setSelectedJob] = useState<QuestionImportJob | null>(null);
  const [drafts, setDrafts] = useState<ImportedQuestionDraft[]>([]);
  const [draftFilter, setDraftFilter] = useState<string>('');
  const [isUploading, setIsUploading] = useState(false);
  const [examTypeCode, setExamTypeCode] = useState('CSCA');
  const [language, setLanguage] = useState<'en' | 'zh'>('en');
  const [contentType, setContentType] = useState<'questions' | 'theory'>('questions');
  const [mode, setMode] = useState<'ai' | 'strict'>('ai');
  const [error, setError] = useState<string | null>(null);
  const [editingDraft, setEditingDraft] = useState<number | null>(null);
  const [editForm, setEditForm] = useState<UpdateDraftPayload>({});
  const [editMulti, setEditMulti] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [sections, setSections] = useState<AdminSection[]>([]);
  const [topics, setTopics] = useState<AdminTopicSummary[]>([]);
  const [sectionId, setSectionId] = useState<number | ''>('');
  const [topicId, setTopicId] = useState<number | ''>('');

  const [batchFiles, setBatchFiles] = useState<BatchFileEntry[]>([]);
  const [instructions, setInstructions] = useState('');
  const batchInputRef = useRef<HTMLInputElement>(null);

  const loadJobs = useCallback(async () => {
    try {
      const data = await questionImportService.getJobs();
      setJobs(data);
    } catch {
      setError('Failed to load import jobs');
    }
  }, []);

  const loadDrafts = useCallback(async (jobId: number) => {
    try {
      const data = await questionImportService.getDrafts(jobId, draftFilter || undefined);
      setDrafts(data);
    } catch {
      setError('Failed to load drafts');
    }
  }, [draftFilter]);

  useEffect(() => {
    loadJobs();
  }, [loadJobs]);

  useEffect(() => {
    adminService.getSections().then(setSections).catch(() => {});
    adminService.getTopics().then(setTopics).catch(() => {});
  }, []);

  useEffect(() => {
    setSectionId('');
    setTopicId('');
  }, [examTypeCode]);

  const sectionsForExam = sections.filter(s => s.examTypeCode === examTypeCode);
  const selectedSection = sections.find(s => s.id === sectionId);
  const chineseOnly = isChineseOnlySubject(selectedSection?.name);
  const topicsForSection = selectedSection
    ? topics.filter(tp => tp.examTypeCode === examTypeCode && tp.sectionName === selectedSection.name)
    : [];

  useEffect(() => {
    if (chineseOnly) setLanguage('zh');
  }, [chineseOnly]);

  useEffect(() => {
    if (selectedJob) loadDrafts(selectedJob.id);
  }, [selectedJob, loadDrafts]);

  const handleUpload = async (file: File) => {
    setIsUploading(true);
    setError(null);
    try {
      const job = await questionImportService.upload(
        file,
        examTypeCode,
        sectionId === '' ? undefined : sectionId,
        topicId === '' ? undefined : topicId,
        instructions || undefined,
        contentType,
        contentType === 'theory' ? undefined : mode,
        language,
      );
      await loadJobs();
      setSelectedJob(job);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Upload failed';
      setError(msg);
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const addBatchFiles = (fileList: FileList) => {
    const newEntries: BatchFileEntry[] = Array.from(fileList).map(f => {
      const lower = f.name.toLowerCase();
      let role: FileRole = 'Questions';
      if (lower.includes('answer') || lower.includes('ответ') || lower.includes('key') || lower.includes('ключ'))
        role = 'Answers';
      return { file: f, role };
    });
    setBatchFiles(prev => [...prev, ...newEntries]);
  };

  const removeBatchFile = (index: number) => {
    setBatchFiles(prev => prev.filter((_, i) => i !== index));
  };

  const updateBatchRole = (index: number, role: FileRole) => {
    setBatchFiles(prev => prev.map((entry, i) => i === index ? { ...entry, role } : entry));
  };

  const handleBatchUpload = async () => {
    if (batchFiles.length === 0) return;
    setIsUploading(true);
    setError(null);
    try {
      const job = await questionImportService.uploadBatch(
        batchFiles,
        examTypeCode,
        sectionId === '' ? undefined : sectionId,
        topicId === '' ? undefined : topicId,
        instructions || undefined,
        language,
      );
      await loadJobs();
      setSelectedJob(job);
      setBatchFiles([]);
      setInstructions('');
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Batch upload failed';
      setError(msg);
    } finally {
      setIsUploading(false);
      if (batchInputRef.current) batchInputRef.current.value = '';
    }
  };

  const handleApprove = async (draftId: number) => {
    try {
      await questionImportService.approveDraft(draftId);
      if (selectedJob) await loadDrafts(selectedJob.id);
      await loadJobs();
    } catch {
      setError('Failed to approve draft');
    }
  };

  const handleReject = async (draftId: number) => {
    try {
      await questionImportService.rejectDraft(draftId);
      if (selectedJob) await loadDrafts(selectedJob.id);
      await loadJobs();
    } catch {
      setError('Failed to reject draft');
    }
  };

  const handleApproveAll = async () => {
    if (!selectedJob) return;
    try {
      const result = await questionImportService.approveAll(selectedJob.id);
      setError(null);
      alert(`${result.approved} questions approved and created!`);
      await loadDrafts(selectedJob.id);
      await loadJobs();
    } catch {
      setError('Failed to approve all');
    }
  };

  const handleDeleteJob = async (jobId: number, e: React.MouseEvent) => {
    e.stopPropagation();
    if (!confirm(t.admin.questionImport.deleteImportConfirm)) return;
    try {
      await questionImportService.deleteJob(jobId);
      if (selectedJob?.id === jobId) { setSelectedJob(null); setDrafts([]); }
      await loadJobs();
    } catch {
      setError('Failed to delete job');
    }
  };

  const handleDeleteAllJobs = async () => {
    if (!confirm(t.admin.questionImport.clearAllConfirm)) return;
    try {
      await questionImportService.deleteAllJobs();
      setSelectedJob(null);
      setDrafts([]);
      await loadJobs();
    } catch {
      setError('Failed to delete all jobs');
    }
  };

  const handleSaveEdit = async (draftId: number) => {
    try {
      await questionImportService.updateDraft(draftId, editForm);
      setEditingDraft(null);
      setEditForm({});
      if (selectedJob) await loadDrafts(selectedJob.id);
    } catch {
      setError('Failed to save edit');
    }
  };

  const handleDraftImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    try {
      const url = await adminService.uploadImage(file);
      setEditForm((f) => ({ ...f, imageUrl: url }));
    } catch {
      setError('Failed to upload image');
    }
    e.target.value = '';
  };

  const startEdit = (draft: ImportedQuestionDraft) => {
    setEditingDraft(draft.id);
    setEditMulti(draft.options.filter((o) => o.isCorrect).length > 1);
    setEditForm({
      questionText: draft.questionText,
      explanation: draft.explanation ?? '',
      difficulty: draft.difficulty,
      imageUrl: draft.imageUrl ?? '',
      options: draft.options.map((o) => ({ text: o.text, isCorrect: o.isCorrect })),
    });
  };

  const setOptions = (opts: DraftOption[]) => setEditForm((f) => ({ ...f, options: opts }));
  const updateOptionText = (i: number, text: string) =>
    setOptions((editForm.options ?? []).map((o, j) => (j === i ? { ...o, text } : o)));
  const toggleOptionCorrect = (i: number) => {
    const opts = editForm.options ?? [];
    setOptions(editMulti
      ? opts.map((o, j) => (j === i ? { ...o, isCorrect: !o.isCorrect } : o))
      : opts.map((o, j) => ({ ...o, isCorrect: j === i })));
  };
  const deleteOption = (i: number) => setOptions((editForm.options ?? []).filter((_, j) => j !== i));
  const addOption = () => setOptions([...(editForm.options ?? []), { text: '', isCorrect: false }]);
  const switchMode = (multi: boolean) => {
    setEditMulti(multi);
    if (!multi) {
      const opts = editForm.options ?? [];
      const firstCorrect = opts.findIndex((o) => o.isCorrect);
      setOptions(opts.map((o, j) => ({ ...o, isCorrect: j === firstCorrect })));
    }
  };

  const [isDragOver, setIsDragOver] = useState(false);
  const [uploadMode, setUploadMode] = useState<'single' | 'multi'>('single');

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragOver(false);
    if (uploadMode === 'multi') {
      addBatchFiles(e.dataTransfer.files);
    } else {
      const file = e.dataTransfer.files[0];
      if (file) handleUpload(file);
    }
  };

  return (
    <div className="animate-fade-in" style={{ padding: embedded ? 0 : '2rem 0' }}>
      {!embedded && (
        <>
          <h1 style={{ marginBottom: '0.5rem' }}>{t.admin.questionImport.title}</h1>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
            {t.admin.questionImport.subtitle}
          </p>
        </>
      )}

      {error && (
        <div className="error-message" style={{ marginBottom: '1rem' }}>{error}</div>
      )}

      <div className="card" style={{ padding: '1.5rem', marginBottom: '2rem' }}>
        <div style={{ display: 'flex', gap: '1rem', alignItems: 'flex-end', flexWrap: 'wrap', marginBottom: '1rem' }}>
          <div>
            <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>
              {t.admin.questionImport.contentTypeLabel}
            </label>
            <div style={{ display: 'flex', gap: '0.25rem' }}>
              <button
                className={`btn ${contentType === 'questions' ? 'btn-primary' : 'btn-secondary'}`}
                style={{ fontSize: '0.8rem', padding: '0.45rem 0.75rem' }}
                onClick={() => setContentType('questions')}
              >
                {t.admin.questionImport.contentTypeQuestions}
              </button>
              <button
                className={`btn ${contentType === 'theory' ? 'btn-primary' : 'btn-secondary'}`}
                style={{ fontSize: '0.8rem', padding: '0.45rem 0.75rem' }}
                onClick={() => setContentType('theory')}
              >
                {t.admin.questionImport.contentTypeTheory}
              </button>
            </div>
          </div>

          {contentType !== 'theory' && (
            <div>
              <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>
                Режим распознавания
              </label>
              <div style={{ display: 'flex', gap: '0.25rem' }}>
                <button
                  className={`btn ${mode === 'ai' ? 'btn-primary' : 'btn-secondary'}`}
                  style={{ fontSize: '0.8rem', padding: '0.45rem 0.75rem' }}
                  onClick={() => setMode('ai')}
                >
                  ИИ-распознавание
                </button>
                <button
                  className={`btn ${mode === 'strict' ? 'btn-primary' : 'btn-secondary'}`}
                  style={{ fontSize: '0.8rem', padding: '0.45rem 0.75rem' }}
                  onClick={() => setMode('strict')}
                  title="Детерминированный разбор .docx по фиксированному шаблону (без ИИ)"
                >
                  Строгий шаблон (.docx)
                </button>
              </div>
            </div>
          )}
          <div>
            <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>
              {t.admin.questionImport.examLabel}
            </label>
            <select
              value={examTypeCode}
              onChange={e => setExamTypeCode(e.target.value)}
              style={{
                padding: '0.5rem 0.75rem', borderRadius: '0.5rem',
                border: '1px solid var(--border-color)', background: 'var(--card-background)',
                color: 'var(--text-primary)', fontSize: '0.9rem'
              }}
            >
              {EXAM_TYPES.map(code => (
                <option key={code} value={code}>{code}</option>
              ))}
            </select>
          </div>
          <div>
            <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>
              Язык вопросов
            </label>
            {chineseOnly ? (
              <div style={{
                padding: '0.5rem 0.75rem', borderRadius: '0.5rem',
                border: '1px solid var(--border-color)', background: 'var(--bg-secondary, #f3f4f6)',
                color: 'var(--text-secondary)', fontSize: '0.9rem'
              }}>中文 (zh)</div>
            ) : (
              <select
                value={language}
                onChange={e => setLanguage(e.target.value as 'en' | 'zh')}
                style={{
                  padding: '0.5rem 0.75rem', borderRadius: '0.5rem',
                  border: '1px solid var(--border-color)', background: 'var(--card-background)',
                  color: 'var(--text-primary)', fontSize: '0.9rem'
                }}
              >
                <option value="en">English (en)</option>
                <option value="zh">中文 (zh)</option>
              </select>
            )}
          </div>
          <div>
            <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>
              {t.admin.questionImport.sectionLabel}
            </label>
            <select
              value={sectionId}
              onChange={e => { setSectionId(e.target.value === '' ? '' : Number(e.target.value)); setTopicId(''); }}
              style={{
                padding: '0.5rem 0.75rem', borderRadius: '0.5rem',
                border: '1px solid var(--border-color)', background: 'var(--card-background)',
                color: 'var(--text-primary)', fontSize: '0.9rem', minWidth: '160px',
              }}
            >
              <option value="">{t.admin.questionImport.sectionAuto}</option>
              {sectionsForExam.map(s => (
                <option key={s.id} value={s.id}>{s.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>
              {t.admin.questionImport.topicLabel}
            </label>
            <select
              value={topicId}
              onChange={e => setTopicId(e.target.value === '' ? '' : Number(e.target.value))}
              disabled={sectionId === ''}
              style={{
                padding: '0.5rem 0.75rem', borderRadius: '0.5rem',
                border: '1px solid var(--border-color)', background: 'var(--card-background)',
                color: 'var(--text-primary)', fontSize: '0.9rem', minWidth: '180px',
                opacity: sectionId === '' ? 0.5 : 1,
              }}
            >
              <option value="">{t.admin.questionImport.topicAuto}</option>
              {topicsForSection.map(tp => (
                <option key={tp.id} value={tp.id}>#{tp.id} · {tp.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>
              {t.admin.questionImport.modeLabel}
            </label>
            <div style={{ display: 'flex', gap: '0.25rem', opacity: contentType === 'theory' ? 0.4 : 1, pointerEvents: contentType === 'theory' ? 'none' : 'auto' }}>
              <button
                className={`btn ${uploadMode === 'single' ? 'btn-primary' : 'btn-secondary'}`}
                style={{ fontSize: '0.8rem', padding: '0.45rem 0.75rem' }}
                onClick={() => setUploadMode('single')}
              >
                {t.admin.questionImport.singleFile}
              </button>
              <button
                className={`btn ${uploadMode === 'multi' ? 'btn-primary' : 'btn-secondary'}`}
                style={{ fontSize: '0.8rem', padding: '0.45rem 0.75rem' }}
                onClick={() => setUploadMode('multi')}
              >
                {t.admin.questionImport.multiFile}
              </button>
            </div>
          </div>
        </div>

        <div style={{ marginBottom: '1rem' }}>
          <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>
            {t.admin.questionImport.contextLabel}
          </label>
          <textarea
            value={instructions}
            onChange={e => setInstructions(e.target.value)}
            placeholder={t.admin.questionImport.contextPlaceholder}
            rows={3}
            style={{
              width: '100%', padding: '0.5rem 0.75rem', fontSize: '0.9rem',
              borderRadius: '0.5rem', border: '1px solid var(--border-color)',
              background: 'var(--card-background)', color: 'var(--text-primary)',
              fontFamily: 'inherit', resize: 'vertical',
            }}
          />
          <p style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', marginTop: '0.35rem', lineHeight: 1.4 }}>
            {t.admin.questionImport.contextHint}
          </p>
        </div>

        {uploadMode === 'single' || contentType === 'theory' ? (
          <div
            onDragOver={e => { e.preventDefault(); setIsDragOver(true); }}
            onDragLeave={() => setIsDragOver(false)}
            onDrop={handleDrop}
            onClick={() => fileInputRef.current?.click()}
            style={{
              border: `2px dashed ${isDragOver ? 'var(--primary-color)' : 'var(--border-color)'}`,
              borderRadius: '0.75rem',
              padding: '2.5rem 1rem',
              textAlign: 'center',
              cursor: isUploading ? 'wait' : 'pointer',
              background: isDragOver ? 'rgba(99, 102, 241, 0.05)' : 'transparent',
              transition: 'all 0.2s',
            }}
          >
            <input
              ref={fileInputRef}
              type="file"
              accept={contentType === 'theory' ? '.pdf,.docx,.md,.markdown,.txt' : '.pdf,.docx,.xlsx,.csv,.md,.markdown,.txt'}
              style={{ display: 'none' }}
              onChange={e => {
                const file = e.target.files?.[0];
                if (file) handleUpload(file);
              }}
            />
            {isUploading ? (
              <p style={{ color: 'var(--primary-color)', fontWeight: 600 }}>{t.admin.questionImport.processing}</p>
            ) : (
              <>
                <p style={{ fontSize: '1.2rem', marginBottom: '0.5rem', color: 'var(--text-secondary)' }}>{contentType === 'theory' ? 'MD / DOCX / PDF' : 'PDF / DOCX / XLSX / MD'}</p>
                <p style={{ fontWeight: 600, color: 'var(--text-primary)' }}>
                  {t.admin.questionImport.dropzone}
                </p>
                <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                  {contentType === 'theory' ? t.admin.questionImport.theoryHint : t.admin.questionImport.dropzoneHint}
                </p>
              </>
            )}
          </div>
        ) : (
          <div>
            <div
              onDragOver={e => { e.preventDefault(); setIsDragOver(true); }}
              onDragLeave={() => setIsDragOver(false)}
              onDrop={handleDrop}
              onClick={() => batchInputRef.current?.click()}
              style={{
                border: `2px dashed ${isDragOver ? 'var(--primary-color)' : 'var(--border-color)'}`,
                borderRadius: '0.75rem',
                padding: '1.5rem 1rem',
                textAlign: 'center',
                cursor: isUploading ? 'wait' : 'pointer',
                background: isDragOver ? 'rgba(99, 102, 241, 0.05)' : 'transparent',
                transition: 'all 0.2s',
                marginBottom: '1rem',
              }}
            >
              <input
                ref={batchInputRef}
                type="file"
                accept=".pdf,.docx,.xlsx,.csv"
                multiple
                style={{ display: 'none' }}
                onChange={e => {
                  if (e.target.files) addBatchFiles(e.target.files);
                  if (batchInputRef.current) batchInputRef.current.value = '';
                }}
              />
              <p style={{ fontSize: '1.2rem', marginBottom: '0.25rem', color: 'var(--text-secondary)' }}>+</p>
              <p style={{ fontWeight: 600, color: 'var(--text-primary)', fontSize: '0.9rem' }}>
                {t.admin.questionImport.addFiles}
              </p>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                {t.admin.questionImport.addFilesHint}
              </p>
            </div>

            {batchFiles.length > 0 && (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', marginBottom: '1rem' }}>
                {batchFiles.map((entry, i) => (
                  <div
                    key={`${entry.file.name}-${i}`}
                    style={{
                      display: 'flex', alignItems: 'center', gap: '0.75rem',
                      padding: '0.5rem 0.75rem', borderRadius: '0.5rem',
                      background: 'var(--bg-secondary)', border: '1px solid var(--border-color)',
                    }}
                  >
                    <span style={{ flex: 1, fontWeight: 500, fontSize: '0.9rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {entry.file.name}
                    </span>
                    <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', whiteSpace: 'nowrap' }}>
                      {(entry.file.size / 1024).toFixed(0)} KB
                    </span>
                    <select
                      value={entry.role}
                      onChange={e => updateBatchRole(i, e.target.value as FileRole)}
                      style={{
                        padding: '0.3rem 0.5rem', borderRadius: '0.375rem', fontSize: '0.8rem',
                        border: '1px solid var(--border-color)', background: 'var(--card-background)',
                        color: entry.role === 'Questions' ? 'var(--primary-color)' :
                               entry.role === 'Answers' ? 'var(--success-color)' : 'var(--text-primary)',
                        fontWeight: 600,
                      }}
                    >
                      {(Object.keys(roleLabels) as FileRole[]).map(r => (
                        <option key={r} value={r}>{roleLabels[r]}</option>
                      ))}
                    </select>
                    <button
                      onClick={() => removeBatchFile(i)}
                      style={{
                        background: 'transparent', border: 'none', cursor: 'pointer',
                        color: 'var(--error-color)', fontSize: '1.1rem', padding: '0 0.25rem',
                      }}
                      title={t.admin.questionImport.removeFile}
                    >
                      x
                    </button>
                  </div>
                ))}
              </div>
            )}

            <button
              className="btn btn-primary"
              disabled={batchFiles.length === 0 || isUploading}
              onClick={handleBatchUpload}
              style={{ width: '100%', padding: '0.7rem', fontSize: '0.95rem' }}
            >
              {isUploading ? t.admin.questionImport.processBtn : `${t.admin.questionImport.processFiles} (${batchFiles.length})`}
            </button>
          </div>
        )}
      </div>

      {jobs.length > 0 && (
        <div className="card" style={{ padding: '1.5rem', marginBottom: '2rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
            <h2 style={{ fontSize: '1.1rem', fontWeight: 600 }}>{t.admin.questionImport.historyTitle}</h2>
            <button
              onClick={handleDeleteAllJobs}
              style={{
                background: 'transparent', border: '1px solid var(--error-color)',
                color: 'var(--error-color)', borderRadius: '0.375rem',
                fontSize: '0.8rem', padding: '0.3rem 0.75rem', cursor: 'pointer',
              }}
            >
              {t.admin.questionImport.clearAll}
            </button>
          </div>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {jobs.map(job => (
              <div
                key={job.id}
                onClick={() => setSelectedJob(job)}
                className="card"
                style={{
                  padding: '0.75rem 1rem',
                  cursor: 'pointer',
                  background: selectedJob?.id === job.id ? 'rgba(99, 102, 241, 0.1)' : 'var(--bg-secondary)',
                  border: selectedJob?.id === job.id ? '1px solid var(--primary-color)' : '1px solid transparent',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  flexWrap: 'wrap',
                  gap: '0.5rem',
                }}
              >
                <div>
                  <span style={{ fontWeight: 600 }}>{job.fileName}</span>
                  <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginLeft: '0.75rem' }}>
                    {job.examTypeCode} · {job.fileType}
                    {job.files && job.files.length > 0 && ` · ${job.files.length} files`}
                  </span>
                  {job.instructions && (
                    <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem', marginLeft: '0.5rem', fontStyle: 'italic' }}>
                      — {job.instructions.length > 60 ? job.instructions.slice(0, 60) + '...' : job.instructions}
                    </span>
                  )}
                </div>
                <div style={{ display: 'flex', gap: '1rem', alignItems: 'center', fontSize: '0.85rem' }}>
                  <span style={{ color: STATUS_COLORS[job.status] ?? 'var(--text-secondary)' }}>
                    {job.status}
                  </span>
                  <span style={{ color: 'var(--text-secondary)' }}>
                    {job.totalExtracted} {t.admin.questionImport.extracted} · {job.totalApproved} {t.admin.questionImport.approved}
                  </span>
                  <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>
                    {new Date(job.createdAt).toLocaleDateString()}
                  </span>
                  <button
                    onClick={(e) => handleDeleteJob(job.id, e)}
                    title={t.admin.questionImport.removeFile}
                    style={{
                      background: 'transparent', border: 'none', cursor: 'pointer',
                      color: 'var(--error-color)', fontSize: '1rem', padding: '0 0.25rem',
                      opacity: 0.6,
                    }}
                    onMouseEnter={e => (e.currentTarget.style.opacity = '1')}
                    onMouseLeave={e => (e.currentTarget.style.opacity = '0.6')}
                  >
                    x
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {selectedJob && selectedJob.contentType === 'Theory' && (
        <div className="card" style={{ padding: '1.5rem', borderLeft: `3px solid var(--success-color)` }}>
          <h2 style={{ fontSize: '1.1rem', fontWeight: 600, marginBottom: '0.5rem' }}>
            {t.admin.questionImport.theoryResultTitle} «{selectedJob.fileName}»
          </h2>
          {selectedJob.status === 'Failed' ? (
            <p style={{ color: 'var(--error-color)' }}>{selectedJob.errorMessage}</p>
          ) : (
            <>
              <p style={{ color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>{t.admin.questionImport.theoryResultHint}</p>
              <p style={{ fontWeight: 600 }}>{selectedJob.resultSummary || '—'}</p>
            </>
          )}
        </div>
      )}

      {selectedJob && selectedJob.contentType !== 'Theory' && (
        <div className="card" style={{ padding: '1.5rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem', flexWrap: 'wrap', gap: '0.75rem' }}>
            <h2 style={{ fontSize: '1.1rem', fontWeight: 600 }}>
              {t.admin.questionImport.questionsFrom} «{selectedJob.fileName}» ({drafts.length})
            </h2>
            <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
              <select
                value={draftFilter}
                onChange={e => setDraftFilter(e.target.value)}
                style={{
                  padding: '0.4rem 0.6rem', borderRadius: '0.5rem', fontSize: '0.85rem',
                  border: '1px solid var(--border-color)', background: 'var(--card-background)',
                  color: 'var(--text-primary)',
                }}
              >
                <option value="">Все</option>
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Rejected">Rejected</option>
              </select>
              <button className="btn btn-primary" style={{ fontSize: '0.85rem', padding: '0.4rem 0.8rem' }} onClick={handleApproveAll}>
                Approve All Pending
              </button>
            </div>
          </div>

          {drafts.length === 0 ? (
            <p style={{ color: 'var(--text-secondary)' }}>{t.admin.questionImport.noQuestions}</p>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              {drafts.map((draft, idx) => (
                <div
                  key={draft.id}
                  className="card"
                  style={{
                    padding: '1rem',
                    background: 'var(--bg-secondary)',
                    borderLeft: `3px solid ${DRAFT_STATUS_COLORS[draft.status] ?? 'var(--border-color)'}`,
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: '0.5rem' }}>
                    <div style={{ flex: 1 }}>
                      <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>#{idx + 1}</span>
                      {editingDraft === draft.id ? (
                        <textarea
                          value={editForm.questionText ?? ''}
                          onChange={e => setEditForm({ ...editForm, questionText: e.target.value })}
                          rows={3}
                          style={{
                            width: '100%', padding: '0.5rem', fontFamily: 'inherit', fontSize: '0.9rem',
                            borderRadius: '0.375rem', border: '1px solid var(--border-color)',
                            background: 'var(--card-background)', color: 'var(--text-primary)',
                            marginTop: '0.25rem',
                          }}
                        />
                      ) : (
                        <p style={{ fontWeight: 500, margin: '0.25rem 0 0.5rem' }}>{draft.questionText}</p>
                      )}

                      {editingDraft === draft.id ? (
                        <div style={{ margin: '0 0 0.5rem' }}>
                          {editForm.imageUrl ? (
                            <div style={{ position: 'relative', display: 'inline-block' }}>
                              <img src={editForm.imageUrl} alt="" style={{ maxWidth: '100%', maxHeight: 200, borderRadius: 6, display: 'block' }} />
                              <button
                                onClick={() => setEditForm({ ...editForm, imageUrl: '' })}
                                title="Удалить изображение"
                                style={{ position: 'absolute', top: 4, right: 4, background: 'rgba(0,0,0,0.55)', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: '0.8rem', lineHeight: 1, padding: '0.15rem 0.35rem' }}
                              >✕</button>
                            </div>
                          ) : null}
                          <label className="btn btn-secondary" style={{ fontSize: '0.72rem', padding: '0.2rem 0.55rem', cursor: 'pointer', marginTop: '0.35rem', display: 'inline-block' }}>
                            {editForm.imageUrl ? 'Заменить изображение' : 'Добавить изображение'}
                            <input type="file" accept="image/*" style={{ display: 'none' }} onChange={handleDraftImageUpload} />
                          </label>
                        </div>
                      ) : (
                        draft.imageUrl && (
                          <img src={draft.imageUrl} alt="" style={{ maxWidth: '100%', maxHeight: 260, borderRadius: 6, margin: '0.25rem 0 0.5rem', display: 'block' }} />
                        )
                      )}

                      {editingDraft === draft.id ? (
                        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem', marginBottom: '0.5rem' }}>
                          <div style={{ display: 'flex', gap: '0.4rem', alignItems: 'center', marginTop: '0.25rem' }}>
                            <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Тип ответа:</span>
                            <button
                              className={`btn ${!editMulti ? 'btn-primary' : 'btn-secondary'}`}
                              style={{ fontSize: '0.72rem', padding: '0.2rem 0.55rem' }}
                              onClick={() => switchMode(false)}
                            >Одиночный</button>
                            <button
                              className={`btn ${editMulti ? 'btn-primary' : 'btn-secondary'}`}
                              style={{ fontSize: '0.72rem', padding: '0.2rem 0.55rem' }}
                              onClick={() => switchMode(true)}
                            >Множественный</button>
                          </div>
                          {(editForm.options ?? []).map((opt, oi) => (
                            <div key={oi} style={{ display: 'flex', gap: '0.4rem', alignItems: 'center' }}>
                              <input
                                type={editMulti ? 'checkbox' : 'radio'}
                                checked={opt.isCorrect}
                                onChange={() => toggleOptionCorrect(oi)}
                                title="Правильный вариант"
                              />
                              <span style={{ fontSize: '0.8rem', minWidth: '1.1rem', color: 'var(--text-secondary)' }}>{String.fromCharCode(65 + oi)}.</span>
                              <input
                                value={opt.text}
                                onChange={e => updateOptionText(oi, e.target.value)}
                                placeholder="Текст варианта"
                                style={{
                                  flex: 1, padding: '0.3rem 0.4rem', fontSize: '0.85rem',
                                  borderRadius: '0.375rem', border: '1px solid var(--border-color)',
                                  background: 'var(--card-background)', color: 'var(--text-primary)',
                                }}
                              />
                              <button
                                onClick={() => deleteOption(oi)}
                                title="Удалить вариант"
                                style={{ background: 'transparent', border: 'none', color: 'var(--error-color)', cursor: 'pointer', fontSize: '1rem', lineHeight: 1 }}
                              >✕</button>
                            </div>
                          ))}
                          <button
                            className="btn btn-secondary"
                            style={{ fontSize: '0.75rem', padding: '0.25rem 0.6rem', alignSelf: 'flex-start' }}
                            onClick={addOption}
                          >+ Добавить вариант</button>
                          {editMulti && (
                            <span style={{ fontSize: '0.72rem', color: 'var(--text-secondary)' }}>
                              Отметьте все правильные варианты — вопрос сохранится как множественный выбор.
                            </span>
                          )}
                        </div>
                      ) : (
                      <div style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem', marginBottom: '0.5rem' }}>
                        {draft.options.length > 0 ? draft.options.map((opt, oi) => (
                          <span key={oi} style={{
                            fontSize: '0.85rem',
                            color: opt.isCorrect ? 'var(--success-color)' : 'var(--text-secondary)',
                            fontWeight: opt.isCorrect ? 600 : 400,
                          }}>
                            {String.fromCharCode(65 + oi)}. {opt.text} {opt.isCorrect && '✓'}
                          </span>
                        )) : (
                          <span style={{
                            fontSize: '0.8rem', fontStyle: 'italic',
                            color: 'var(--warning-color, #f59e0b)', padding: '0.25rem 0',
                          }}>
                            {t.admin.questionImport.noOptionsHint}
                          </span>
                        )}
                      </div>
                      )}

                      {draft.explanation && (
                        <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', fontStyle: 'italic' }}>
                          {editingDraft === draft.id ? (
                            <input
                              value={editForm.explanation ?? ''}
                              onChange={e => setEditForm({ ...editForm, explanation: e.target.value })}
                              style={{
                                width: '100%', padding: '0.3rem', fontSize: '0.85rem',
                                borderRadius: '0.375rem', border: '1px solid var(--border-color)',
                                background: 'var(--card-background)', color: 'var(--text-primary)',
                              }}
                            />
                          ) : (
                            <>{draft.explanation}</>
                          )}
                        </p>
                      )}

                      <div style={{ display: 'flex', gap: '0.75rem', fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '0.5rem' }}>
                        <span>Difficulty: {draft.difficulty}</span>
                        <span>Source: {draft.source}</span>
                        {draft.topicName && <span>Topic: {draft.topicName}</span>}
                        <span style={{ color: DRAFT_STATUS_COLORS[draft.status] }}>{draft.status}</span>
                      </div>
                    </div>

                    {draft.status === 'Pending' && (
                      <div style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem', minWidth: '80px' }}>
                        {editingDraft === draft.id ? (
                          <>
                            <button
                              className="btn btn-primary"
                              style={{ fontSize: '0.75rem', padding: '0.3rem 0.5rem' }}
                              onClick={() => handleSaveEdit(draft.id)}
                            >
                              {t.admin.questionImport.saveEdit}
                            </button>
                            <button
                              className="btn btn-secondary"
                              style={{ fontSize: '0.75rem', padding: '0.3rem 0.5rem' }}
                              onClick={() => { setEditingDraft(null); setEditForm({}); }}
                            >
                              {t.admin.questionImport.cancelEdit}
                            </button>
                          </>
                        ) : (
                          <>
                            <button
                              className="btn btn-primary"
                              style={{ fontSize: '0.75rem', padding: '0.3rem 0.5rem' }}
                              onClick={() => handleApprove(draft.id)}
                            >
                              {t.admin.questionImport.approve}
                            </button>
                            <button
                              className="btn btn-secondary"
                              style={{ fontSize: '0.75rem', padding: '0.3rem 0.5rem' }}
                              onClick={() => startEdit(draft)}
                            >
                              {t.admin.questionImport.editBtn}
                            </button>
                            <button
                              style={{
                                fontSize: '0.75rem', padding: '0.3rem 0.5rem', cursor: 'pointer',
                                background: 'transparent', border: '1px solid var(--error-color)',
                                color: 'var(--error-color)', borderRadius: '0.375rem',
                              }}
                              onClick={() => handleReject(draft.id)}
                            >
                              {t.admin.questionImport.reject}
                            </button>
                          </>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default AdminQuestionImportPage;
