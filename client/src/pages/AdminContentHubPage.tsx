import { useState, useCallback } from 'react';
import { useTranslation } from '../hooks/useTranslation';
import contentPipelineService from '../services/contentPipelineService';
import type { IngestContent, IngestResult, DriveSyncItem } from '../services/contentPipelineService';
import AdminQuestionImportPage from './AdminQuestionImportPage';
import AdminImportPage from './AdminImportPage';

const EXAM_TYPES = ['SAT', 'NUET'];

type Tab = 'pipeline' | 'file' | 'json';

const DRIVE_STATUS_COLORS: Record<string, string> = {
  Synced: 'var(--success-color)',
  SkippedUnchanged: 'var(--text-secondary)',
  Pending: 'var(--warning-color)',
  Failed: 'var(--error-color)',
  Unmapped: 'var(--warning-color)',
  Orphaned: 'var(--text-secondary)',
};

function AdminContentHubPage() {
  const { t } = useTranslation();
  const c = t.admin.contentHub;
  const [tab, setTab] = useState<Tab>('pipeline');

  // ── Pipeline state ──────────────────────────────────────
  const [examType, setExamType] = useState('SAT');
  const [section, setSection] = useState('');
  const [inputMode, setInputMode] = useState<'file' | 'text'>('file');
  const [file, setFile] = useState<File | null>(null);
  const [text, setText] = useState('');
  const [preview, setPreview] = useState<IngestContent | null>(null);
  const [result, setResult] = useState<IngestResult | null>(null);
  const [busy, setBusy] = useState<'' | 'parsing' | 'ingesting' | 'oneshot'>('');
  const [error, setError] = useState<string | null>(null);

  // ── Drive state ─────────────────────────────────────────
  const [folderId, setFolderId] = useState('');
  const [driveBusy, setDriveBusy] = useState(false);
  const [driveMsg, setDriveMsg] = useState<string | null>(null);
  const [items, setItems] = useState<DriveSyncItem[]>([]);

  const validBase = () => {
    if (!section.trim()) { setError(c.sectionRequired); return false; }
    if (inputMode === 'file' && !file) { setError(c.fileRequired); return false; }
    if (inputMode === 'text' && !text.trim()) { setError(c.textRequired); return false; }
    return true;
  };

  const baseInput = () => ({
    examTypeCode: examType,
    examSectionName: section.trim(),
    text: inputMode === 'text' ? text : undefined,
    file: inputMode === 'file' ? file ?? undefined : undefined,
  });

  const errMsg = (e: unknown) => {
    if (e && typeof e === 'object' && 'response' in e) {
      const r = (e as { response?: { data?: { error?: string } } }).response;
      if (r?.data?.error) return r.data.error;
    }
    return e instanceof Error ? e.message : 'Error';
  };

  const doPreview = async () => {
    if (!validBase()) return;
    setBusy('parsing'); setError(null); setResult(null); setPreview(null);
    try {
      setPreview(await contentPipelineService.parse(baseInput()));
    } catch (e) {
      setError(errMsg(e));
    } finally { setBusy(''); }
  };

  const doIngest = async () => {
    if (!preview) return;
    setBusy('ingesting'); setError(null);
    try {
      const res = await contentPipelineService.ingest(preview);
      setResult(res); setPreview(null);
    } catch (e) {
      setError(errMsg(e));
    } finally { setBusy(''); }
  };

  const doOneShot = async () => {
    if (!validBase()) return;
    setBusy('oneshot'); setError(null); setResult(null); setPreview(null);
    try {
      setResult(await contentPipelineService.parseAndIngest(baseInput()));
    } catch (e) {
      setError(errMsg(e));
    } finally { setBusy(''); }
  };

  const loadItems = useCallback(async () => {
    try { setItems(await contentPipelineService.getDriveItems()); }
    catch (e) { setDriveMsg(errMsg(e)); }
  }, []);

  const startSync = async () => {
    if (!folderId.trim()) { setDriveMsg(c.folderRequired); return; }
    if (!section.trim()) { setDriveMsg(c.sectionRequired); return; }
    setDriveBusy(true); setDriveMsg(null);
    try {
      const res = await contentPipelineService.startDriveSync(folderId.trim(), examType, section.trim());
      setDriveMsg(`${c.syncQueued} (${res.jobId})`);
      setTimeout(loadItems, 1500);
    } catch (e) {
      setDriveMsg(errMsg(e));
    } finally { setDriveBusy(false); }
  };

  const totalPreviewQuestions = preview
    ? preview.topics.reduce((sum, tp) => sum + (tp.questions?.length ?? 0), 0)
    : 0;

  const inputStyle: React.CSSProperties = {
    padding: '0.5rem 0.75rem', borderRadius: '0.5rem',
    border: '1px solid var(--border-color)', background: 'var(--card-background)',
    color: 'var(--text-primary)', fontSize: '0.9rem',
  };
  const labelStyle: React.CSSProperties = {
    fontSize: '0.85rem', color: 'var(--text-secondary)', display: 'block', marginBottom: '0.25rem',
  };

  const tabBtn = (id: Tab, label: string) => (
    <button
      onClick={() => setTab(id)}
      style={{
        padding: '0.6rem 1.1rem', fontSize: '0.92rem', fontWeight: tab === id ? 600 : 400,
        background: 'transparent', border: 'none', cursor: 'pointer',
        color: tab === id ? 'var(--primary-color)' : 'var(--text-secondary)',
        borderBottom: `2px solid ${tab === id ? 'var(--primary-color)' : 'transparent'}`,
      }}
    >{label}</button>
  );

  return (
    <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
      <h1 style={{ marginBottom: '0.5rem' }}>{c.title}</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.25rem' }}>{c.subtitle}</p>

      {/* Tab bar */}
      <div style={{ display: 'flex', gap: '0.25rem', borderBottom: '1px solid var(--border-color)', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        {tabBtn('pipeline', c.tabPipeline)}
        {tabBtn('file', c.tabFile)}
        {tabBtn('json', c.tabJson)}
      </div>

      {tab === 'pipeline' && (
        <div>
          {/* Shared exam/section selectors */}
          <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
            <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginBottom: '1rem' }}>
              <div>
                <label style={labelStyle}>{c.examLabel}</label>
                <select value={examType} onChange={e => setExamType(e.target.value)} style={inputStyle}>
                  {EXAM_TYPES.map(code => <option key={code} value={code}>{code}</option>)}
                </select>
              </div>
              <div style={{ flex: 1, minWidth: '200px' }}>
                <label style={labelStyle}>{c.sectionLabel}</label>
                <input
                  value={section}
                  onChange={e => setSection(e.target.value)}
                  placeholder={c.sectionPlaceholder}
                  style={{ ...inputStyle, width: '100%' }}
                />
              </div>
            </div>

            {/* Input mode */}
            <div style={{ display: 'flex', gap: '0.25rem', marginBottom: '1rem' }}>
              <button className={`btn ${inputMode === 'file' ? 'btn-primary' : 'btn-secondary'}`}
                style={{ fontSize: '0.8rem', padding: '0.45rem 0.75rem' }}
                onClick={() => setInputMode('file')}>{c.inputFile}</button>
              <button className={`btn ${inputMode === 'text' ? 'btn-primary' : 'btn-secondary'}`}
                style={{ fontSize: '0.8rem', padding: '0.45rem 0.75rem' }}
                onClick={() => setInputMode('text')}>{c.inputText}</button>
            </div>

            {inputMode === 'file' ? (
              <input
                type="file"
                accept=".pdf,.docx,.xlsx,.md,.markdown,.txt"
                onChange={e => setFile(e.target.files?.[0] ?? null)}
                style={{ display: 'block', marginBottom: '1rem' }}
              />
            ) : (
              <textarea
                value={text}
                onChange={e => setText(e.target.value)}
                rows={10}
                placeholder={c.textPlaceholder}
                style={{ ...inputStyle, width: '100%', fontFamily: 'monospace', fontSize: '0.85rem', resize: 'vertical', marginBottom: '1rem' }}
              />
            )}

            <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
              <button className="btn btn-secondary" disabled={busy !== ''} onClick={doPreview}>
                {busy === 'parsing' ? c.parsing : c.previewBtn}
              </button>
              <button className="btn btn-primary" disabled={busy !== ''} onClick={doOneShot}>
                {busy === 'oneshot' ? c.ingesting : c.oneShotBtn}
              </button>
            </div>

            {error && <div className="error-message" style={{ marginTop: '1rem' }}>{error}</div>}
          </div>

          {/* Preview (parsed, not yet committed) */}
          {preview && (
            <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
              <h3 style={{ marginBottom: '0.75rem' }}>{c.previewTitle}</h3>
              <div style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap', marginBottom: '1rem', fontSize: '0.9rem' }}>
                <div><strong>{c.skillLabel}:</strong> {preview.skillName}</div>
                <div><strong>{c.topicsLabel}:</strong> {preview.topics.length}</div>
                <div><strong>{c.questionsLabel}:</strong> {totalPreviewQuestions}</div>
              </div>
              <ul style={{ margin: '0 0 1rem 1.1rem', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                {preview.topics.map((tp, i) => (
                  <li key={i}>{tp.name} — {tp.questions?.length ?? 0} {c.questionsLabel.toLowerCase()}</li>
                ))}
              </ul>
              <button className="btn btn-primary" disabled={busy !== ''} onClick={doIngest}>
                {busy === 'ingesting' ? c.ingesting : c.ingestBtn}
              </button>
            </div>
          )}

          {/* Result */}
          {result && (
            <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem', border: '1px solid var(--success-color)' }}>
              <h3 style={{ marginBottom: '0.75rem' }}>{c.resultTitle}</h3>
              <div style={{ fontSize: '0.9rem', lineHeight: 1.8 }}>
                <div><strong>{c.skillLabel}:</strong> {result.skillName} {result.skillCreated ? `(${c.created})` : ''}</div>
                <div>{c.topicsLabel}: +{result.topicsCreated} / ~{result.topicsUpdated}</div>
                <div>{c.questionsLabel}: +{result.questionsCreated}, {c.duplicates}: {result.questionsSkippedDuplicate}</div>
                <div>{c.lessonsLabel}: +{result.lessonsCreated} / ~{result.lessonsUpdated} · {c.formulasLabel}: +{result.formulasCreated}</div>
              </div>
              {result.warnings.length > 0 && (
                <div style={{ marginTop: '0.75rem', fontSize: '0.85rem', color: 'var(--warning-color)' }}>
                  {result.warnings.map((w, i) => <div key={i}>⚠ {w}</div>)}
                </div>
              )}
            </div>
          )}

          {/* Google Drive sync */}
          <div className="card" style={{ padding: '1.5rem' }}>
            <h3 style={{ marginBottom: '0.5rem' }}>{c.driveTitle}</h3>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '1rem' }}>{c.driveHint}</p>
            <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap', alignItems: 'flex-end', marginBottom: '1rem' }}>
              <div style={{ flex: 1, minWidth: '240px' }}>
                <label style={labelStyle}>{c.folderLabel}</label>
                <input
                  value={folderId}
                  onChange={e => setFolderId(e.target.value)}
                  placeholder={c.folderPlaceholder}
                  style={{ ...inputStyle, width: '100%' }}
                />
              </div>
              <button className="btn btn-primary" disabled={driveBusy} onClick={startSync}>
                {driveBusy ? c.syncing : c.syncBtn}
              </button>
              <button className="btn btn-secondary" onClick={loadItems}>{c.refreshItems}</button>
            </div>
            {driveMsg && <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '1rem' }}>{driveMsg}</div>}

            {items.length > 0 && (
              <div style={{ overflowX: 'auto' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.83rem' }}>
                  <thead>
                    <tr style={{ textAlign: 'left', color: 'var(--text-secondary)' }}>
                      <th style={{ padding: '0.4rem' }}>{c.colName}</th>
                      <th style={{ padding: '0.4rem' }}>{c.colStatus}</th>
                      <th style={{ padding: '0.4rem' }}>{c.colSkill}</th>
                      <th style={{ padding: '0.4rem' }}>{c.colError}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {items.map(it => (
                      <tr key={it.id} style={{ borderTop: '1px solid var(--border-color)' }}>
                        <td style={{ padding: '0.4rem' }}>{it.name}</td>
                        <td style={{ padding: '0.4rem', color: DRIVE_STATUS_COLORS[it.status] ?? 'var(--text-primary)', fontWeight: 600 }}>{it.status}</td>
                        <td style={{ padding: '0.4rem' }}>{it.mappedSkillName ?? '—'}</td>
                        <td style={{ padding: '0.4rem', color: 'var(--error-color)' }}>{it.errorMessage ?? ''}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}

      {tab === 'file' && <AdminQuestionImportPage embedded />}
      {tab === 'json' && <AdminImportPage embedded />}
    </div>
  );
}

export default AdminContentHubPage;
