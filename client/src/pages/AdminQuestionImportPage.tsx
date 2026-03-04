import { useEffect, useState, useCallback, useRef } from 'react';
import { questionImportService } from '../services/questionImportService';
import type { QuestionImportJob, ImportedQuestionDraft, UpdateDraftPayload, FileRole, BatchFileEntry } from '../services/questionImportService';

const EXAM_TYPES = ['SAT', 'TOEFL', 'NUET', 'IELTS', 'CSCA'];

const ROLE_LABELS: Record<FileRole, string> = {
  Questions: 'Вопросы',
  Answers: 'Ответы',
  Mixed: 'Смешанный',
};

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

function AdminQuestionImportPage() {
  const [jobs, setJobs] = useState<QuestionImportJob[]>([]);
  const [selectedJob, setSelectedJob] = useState<QuestionImportJob | null>(null);
  const [drafts, setDrafts] = useState<ImportedQuestionDraft[]>([]);
  const [draftFilter, setDraftFilter] = useState<string>('');
  const [isUploading, setIsUploading] = useState(false);
  const [examTypeCode, setExamTypeCode] = useState('SAT');
  const [error, setError] = useState<string | null>(null);
  const [editingDraft, setEditingDraft] = useState<number | null>(null);
  const [editForm, setEditForm] = useState<UpdateDraftPayload>({});
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Multi-file state
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
    if (selectedJob) loadDrafts(selectedJob.id);
  }, [selectedJob, loadDrafts]);

  // ── Single-file upload ──────────────────────────────────
  const handleUpload = async (file: File) => {
    setIsUploading(true);
    setError(null);
    try {
      const job = await questionImportService.upload(file, examTypeCode);
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

  // ── Multi-file batch upload ─────────────────────────────
  const addBatchFiles = (fileList: FileList) => {
    const newEntries: BatchFileEntry[] = Array.from(fileList).map(f => {
      // Auto-detect role from filename hints
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
      const job = await questionImportService.uploadBatch(batchFiles, examTypeCode, undefined, instructions || undefined);
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

  const startEdit = (draft: ImportedQuestionDraft) => {
    setEditingDraft(draft.id);
    setEditForm({
      questionText: draft.questionText,
      explanation: draft.explanation ?? '',
      difficulty: draft.difficulty,
    });
  };

  // ─── Drop zone handlers ─────────────────────────────────
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
    <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
      <h1 style={{ marginBottom: '0.5rem' }}>Импорт вопросов из файлов</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
        Загрузите файлы — система извлечёт вопросы автоматически. Поддерживается мульти-файл с ответами.
      </p>

      {error && (
        <div className="error-message" style={{ marginBottom: '1rem' }}>{error}</div>
      )}

      {/* Upload Zone */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '2rem' }}>
        <div style={{ display: 'flex', gap: '1rem', alignItems: 'flex-end', flexWrap: 'wrap', marginBottom: '1rem' }}>
          <div>
            <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>
              Экзамен
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
              Режим
            </label>
            <div style={{ display: 'flex', gap: '0.25rem' }}>
              <button
                className={`btn ${uploadMode === 'single' ? 'btn-primary' : 'btn-secondary'}`}
                style={{ fontSize: '0.8rem', padding: '0.45rem 0.75rem' }}
                onClick={() => setUploadMode('single')}
              >
                Один файл
              </button>
              <button
                className={`btn ${uploadMode === 'multi' ? 'btn-primary' : 'btn-secondary'}`}
                style={{ fontSize: '0.8rem', padding: '0.45rem 0.75rem' }}
                onClick={() => setUploadMode('multi')}
              >
                Несколько файлов
              </button>
            </div>
          </div>
        </div>

        {uploadMode === 'single' ? (
          /* ── Single file drop zone ── */
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
              accept=".pdf,.docx,.xlsx,.csv"
              style={{ display: 'none' }}
              onChange={e => {
                const file = e.target.files?.[0];
                if (file) handleUpload(file);
              }}
            />
            {isUploading ? (
              <p style={{ color: 'var(--primary-color)', fontWeight: 600 }}>Обработка файла...</p>
            ) : (
              <>
                <p style={{ fontSize: '1.2rem', marginBottom: '0.5rem', color: 'var(--text-secondary)' }}>PDF / DOCX / XLSX</p>
                <p style={{ fontWeight: 600, color: 'var(--text-primary)' }}>
                  Перетащите файл сюда или нажмите для выбора
                </p>
                <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                  Один файл с вопросами (до 50 МБ)
                </p>
              </>
            )}
          </div>
        ) : (
          /* ── Multi-file mode ── */
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
                Добавить файлы (вопросы, ответы)
              </p>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                Перетащите или нажмите. Роль файла (вопросы/ответы) можно выбрать ниже.
              </p>
            </div>

            {/* File list with role selectors */}
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
                      {(Object.keys(ROLE_LABELS) as FileRole[]).map(r => (
                        <option key={r} value={r}>{ROLE_LABELS[r]}</option>
                      ))}
                    </select>
                    <button
                      onClick={() => removeBatchFile(i)}
                      style={{
                        background: 'transparent', border: 'none', cursor: 'pointer',
                        color: 'var(--error-color)', fontSize: '1.1rem', padding: '0 0.25rem',
                      }}
                      title="Удалить"
                    >
                      x
                    </button>
                  </div>
                ))}
              </div>
            )}

            {/* Instructions textarea */}
            <div style={{ marginBottom: '1rem' }}>
              <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem' }}>
                Контекст / инструкции для парсера (необязательно)
              </label>
              <textarea
                value={instructions}
                onChange={e => setInstructions(e.target.value)}
                placeholder="Например: файл «Questions.pdf» содержит вопросы 1-30, файл «Answers.pdf» — ключи ответов. Разбить по темам: 1-10 алгебра, 11-20 геометрия, 21-30 статистика."
                rows={3}
                style={{
                  width: '100%', padding: '0.5rem 0.75rem', fontSize: '0.9rem',
                  borderRadius: '0.5rem', border: '1px solid var(--border-color)',
                  background: 'var(--card-background)', color: 'var(--text-primary)',
                  fontFamily: 'inherit', resize: 'vertical',
                }}
              />
            </div>

            {/* Upload button */}
            <button
              className="btn btn-primary"
              disabled={batchFiles.length === 0 || isUploading}
              onClick={handleBatchUpload}
              style={{ width: '100%', padding: '0.7rem', fontSize: '0.95rem' }}
            >
              {isUploading ? 'Обработка...' : `Загрузить и обработать (${batchFiles.length} файл${batchFiles.length === 1 ? '' : batchFiles.length < 5 ? 'а' : 'ов'})`}
            </button>
          </div>
        )}
      </div>

      {/* Jobs List */}
      {jobs.length > 0 && (
        <div className="card" style={{ padding: '1.5rem', marginBottom: '2rem' }}>
          <h2 style={{ fontSize: '1.1rem', fontWeight: 600, marginBottom: '1rem' }}>История импортов</h2>
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
                    {job.totalExtracted} extracted · {job.totalApproved} approved
                  </span>
                  <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem' }}>
                    {new Date(job.createdAt).toLocaleDateString()}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Drafts Review */}
      {selectedJob && (
        <div className="card" style={{ padding: '1.5rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem', flexWrap: 'wrap', gap: '0.75rem' }}>
            <h2 style={{ fontSize: '1.1rem', fontWeight: 600 }}>
              Вопросы из «{selectedJob.fileName}» ({drafts.length})
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
            <p style={{ color: 'var(--text-secondary)' }}>Нет вопросов для отображения</p>
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

                      {/* Options */}
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
                            Нет вариантов ответа — заполните вручную перед одобрением
                          </span>
                        )}
                      </div>

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
                            <>💡 {draft.explanation}</>
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

                    {/* Actions */}
                    {draft.status === 'Pending' && (
                      <div style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem', minWidth: '80px' }}>
                        {editingDraft === draft.id ? (
                          <>
                            <button
                              className="btn btn-primary"
                              style={{ fontSize: '0.75rem', padding: '0.3rem 0.5rem' }}
                              onClick={() => handleSaveEdit(draft.id)}
                            >
                              Save
                            </button>
                            <button
                              className="btn btn-secondary"
                              style={{ fontSize: '0.75rem', padding: '0.3rem 0.5rem' }}
                              onClick={() => { setEditingDraft(null); setEditForm({}); }}
                            >
                              Cancel
                            </button>
                          </>
                        ) : (
                          <>
                            <button
                              className="btn btn-primary"
                              style={{ fontSize: '0.75rem', padding: '0.3rem 0.5rem' }}
                              onClick={() => handleApprove(draft.id)}
                            >
                              Approve
                            </button>
                            <button
                              className="btn btn-secondary"
                              style={{ fontSize: '0.75rem', padding: '0.3rem 0.5rem' }}
                              onClick={() => startEdit(draft)}
                            >
                              Edit
                            </button>
                            <button
                              style={{
                                fontSize: '0.75rem', padding: '0.3rem 0.5rem', cursor: 'pointer',
                                background: 'transparent', border: '1px solid var(--error-color)',
                                color: 'var(--error-color)', borderRadius: '0.375rem',
                              }}
                              onClick={() => handleReject(draft.id)}
                            >
                              Reject
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
