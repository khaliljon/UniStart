import { useState, useCallback, useEffect } from 'react';
import { useTranslation } from '../hooks/useTranslation';
import contentPipelineService from '../services/contentPipelineService';
import type {
  IngestContent, IngestResult, DriveSyncItem, AdminSkillSummary,
  DriveSyncPlan, ContentMappingRule, ContentMappingRuleInput,
} from '../services/contentPipelineService';
import adminService from '../services/adminService';
import type { AdminSection } from '../types';
import AdminQuestionImportPage from './AdminQuestionImportPage';
import AdminImportPage from './AdminImportPage';

const EXAM_TYPES = ['SAT', 'NUET'];

type Tab = 'pipeline' | 'file' | 'json' | 'cleanup' | 'mappings';

const EMPTY_RULE: ContentMappingRuleInput = {
  examSectionName: '', matchType: 'FolderSegment', pattern: '',
  skillName: '', glossary: '', sortOrder: 0, isActive: true, isIgnore: false,
};

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
  const [sections, setSections] = useState<AdminSection[]>([]);
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
  const [itemsBusy, setItemsBusy] = useState(false);
  const [itemsLoaded, setItemsLoaded] = useState(false);

  // ── Cleanup state ─────────────────────────────────
  const [skills, setSkills] = useState<AdminSkillSummary[]>([]);
  const [skillsBusy, setSkillsBusy] = useState(false);
  const [skillsLoaded, setSkillsLoaded] = useState(false);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const [cleanupMsg, setCleanupMsg] = useState<string | null>(null);
  // ── Dry-run preview state ─────────────────
  const [plan, setPlan] = useState<DriveSyncPlan | null>(null);
  const [planBusy, setPlanBusy] = useState(false);

  // ── Mapping rules state ─────────────────
  const [rules, setRules] = useState<ContentMappingRule[]>([]);
  const [rulesBusy, setRulesBusy] = useState(false);
  const [rulesLoaded, setRulesLoaded] = useState(false);
  const [ruleForm, setRuleForm] = useState<ContentMappingRuleInput>(EMPTY_RULE);
  const [editingRuleId, setEditingRuleId] = useState<number | null>(null);
  const [rulesMsg, setRulesMsg] = useState<string | null>(null);
  // Load existing exam sections once for the dropdown.
  useEffect(() => {
    adminService.getSections().then(setSections).catch(() => { /* non-fatal */ });
  }, []);

  const sectionsForExam = sections.filter(s => s.examTypeCode === examType);

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
    setItemsBusy(true);
    try { setItems(await contentPipelineService.getDriveItems()); setItemsLoaded(true); }
    catch (e) { setDriveMsg(errMsg(e)); }
    finally { setItemsBusy(false); }
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

  const doDrivePreview = async () => {
    if (!folderId.trim()) { setDriveMsg(c.folderRequired); return; }
    if (!section.trim()) { setDriveMsg(c.sectionRequired); return; }
    setPlanBusy(true); setDriveMsg(null); setPlan(null);
    try {
      setPlan(await contentPipelineService.previewDriveSync(folderId.trim(), examType, section.trim()));
    } catch (e) {
      setDriveMsg(errMsg(e));
    } finally { setPlanBusy(false); }
  };

  // ── Mapping rules ───────────────────────────────────────
  const loadRules = useCallback(async () => {
    setRulesBusy(true); setRulesMsg(null);
    try { setRules(await contentPipelineService.getMappings()); setRulesLoaded(true); }
    catch (e) { setRulesMsg(errMsg(e)); }
    finally { setRulesBusy(false); }
  }, []);

  const submitRule = async () => {
    if (!ruleForm.pattern.trim() || (!ruleForm.isIgnore && !ruleForm.skillName.trim())) { setRulesMsg(c.mapRequired); return; }
    setRulesBusy(true); setRulesMsg(null);
    try {
      if (editingRuleId !== null) {
        const updated = await contentPipelineService.updateMapping(editingRuleId, ruleForm);
        setRules(prev => prev.map(r => (r.id === editingRuleId ? updated : r)));
      } else {
        const created = await contentPipelineService.createMapping(ruleForm);
        setRules(prev => [...prev, created]);
      }
      setRuleForm(EMPTY_RULE); setEditingRuleId(null);
    } catch (e) {
      setRulesMsg(errMsg(e));
    } finally { setRulesBusy(false); }
  };

  const editRule = (r: ContentMappingRule) => {
    setEditingRuleId(r.id);
    setRuleForm({
      examSectionName: r.examSectionName ?? '', matchType: r.matchType, pattern: r.pattern,
      skillName: r.skillName, glossary: r.glossary ?? '', sortOrder: r.sortOrder, isActive: r.isActive, isIgnore: r.isIgnore,
    });
  };

  const cancelRuleEdit = () => { setRuleForm(EMPTY_RULE); setEditingRuleId(null); };

  const removeRule = async (r: ContentMappingRule) => {
    if (!window.confirm(`${r.pattern} → ${r.skillName}\n\n${c.mapDeleteConfirm}`)) return;
    setRulesBusy(true); setRulesMsg(null);
    try {
      await contentPipelineService.deleteMapping(r.id);
      setRules(prev => prev.filter(x => x.id !== r.id));
    } catch (e) {
      setRulesMsg(errMsg(e));
    } finally { setRulesBusy(false); }
  };

  const totalPreviewQuestions = preview
    ? preview.topics.reduce((sum, tp) => sum + (tp.questions?.length ?? 0), 0)
    : 0;

  const loadSkills = useCallback(async () => {
    setSkillsBusy(true); setCleanupMsg(null);
    try { setSkills(await contentPipelineService.getSkills()); setSkillsLoaded(true); }
    catch (e) { setCleanupMsg(errMsg(e)); }
    finally { setSkillsBusy(false); }
  }, []);

  const deleteSkill = async (s: AdminSkillSummary) => {
    const msg = s.hasStudentActivity ? c.deleteForceConfirm : c.deleteConfirm;
    if (!window.confirm(`${s.name}\n\n${msg}`)) return;
    setDeletingId(s.id); setCleanupMsg(null);
    try {
      await contentPipelineService.deleteSkill(s.id, s.hasStudentActivity);
      setSkills(prev => prev.filter(x => x.id !== s.id));
      setCleanupMsg(`${c.deleted}: ${s.name}`);
    } catch (e) {
      setCleanupMsg(errMsg(e));
    } finally { setDeletingId(null); }
  };

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
        {tabBtn('mappings', c.tabMappings)}
        {tabBtn('cleanup', c.tabCleanup)}
      </div>

      {tab === 'pipeline' && (
        <div>
          {/* Shared exam/section selectors */}
          <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
            <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginBottom: '1rem' }}>
              <div>
                <label style={labelStyle}>{c.examLabel}</label>
                <select value={examType} onChange={e => { setExamType(e.target.value); setSection(''); }} style={inputStyle}>
                  {EXAM_TYPES.map(code => <option key={code} value={code}>{code}</option>)}
                </select>
              </div>
              <div style={{ flex: 1, minWidth: '200px' }}>
                <label style={labelStyle}>{c.sectionLabel}</label>
                <input
                  value={section}
                  onChange={e => setSection(e.target.value)}
                  placeholder={c.sectionPlaceholder}
                  list="content-hub-sections"
                  style={{ ...inputStyle, width: '100%' }}
                />
                <datalist id="content-hub-sections">
                  {sectionsForExam.map(s => <option key={s.id} value={s.name} />)}
                </datalist>
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
              <button className="btn btn-secondary" disabled={planBusy} onClick={doDrivePreview}>
                {planBusy ? c.previewing : c.previewPlanBtn}
              </button>
              <button className="btn btn-secondary" disabled={itemsBusy} onClick={loadItems}>{itemsBusy ? c.refreshing : c.refreshItems}</button>
            </div>
            {driveMsg && <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '1rem' }}>{driveMsg}</div>}

            {/* Dry-run plan (no LLM, no DB writes) */}
            {plan && (
              <div className="card" style={{ padding: '1rem', marginBottom: '1rem', background: 'var(--background-secondary, transparent)' }}>
                <div style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap', marginBottom: '0.75rem', fontSize: '0.85rem' }}>
                  <div><strong>{c.planFiles}:</strong> {plan.totalFiles}</div>
                  <div><strong>{c.planIngestible}:</strong> {plan.ingestibleCount}</div>
                  <div><strong>{c.planChanged}:</strong> {plan.changedCount}</div>
                  <div><strong>{c.planUnits}:</strong> {plan.unitNames.join(', ') || '—'}</div>
                </div>
                {plan.warnings.length > 0 && (
                  <div style={{ marginBottom: '0.75rem', fontSize: '0.82rem', color: 'var(--warning-color)' }}>
                    {plan.warnings.map((w, i) => <div key={i}>⚠ {w}</div>)}
                  </div>
                )}
                <div style={{ overflowX: 'auto' }}>
                  <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.8rem' }}>
                    <thead>
                      <tr style={{ textAlign: 'left', color: 'var(--text-secondary)' }}>
                        <th style={{ padding: '0.35rem' }}>{c.colName}</th>
                        <th style={{ padding: '0.35rem' }}>{c.colFolder}</th>
                        <th style={{ padding: '0.35rem' }}>{c.colSkill}</th>
                        <th style={{ padding: '0.35rem' }}>{c.colChanged}</th>
                      </tr>
                    </thead>
                    <tbody>
                      {plan.normal.map(p => (
                        <tr key={p.driveFileId} style={{ borderTop: '1px solid var(--border-color)' }}>
                          <td style={{ padding: '0.35rem' }}>{p.name}</td>
                          <td style={{ padding: '0.35rem', color: 'var(--text-secondary)' }}>{p.folderPath || '—'}</td>
                          <td style={{ padding: '0.35rem', color: p.mappedSkillName ? 'var(--text-primary)' : 'var(--warning-color)' }}>
                            {p.mappedSkillName ?? c.planSkipped}{p.matchedRule ? ` (⚙ ${p.matchedRule})` : ''}
                          </td>
                          <td style={{ padding: '0.35rem' }}>{p.changed ? c.planYes : c.planNo}</td>
                        </tr>
                      ))}
                      {plan.tsaPairs.map((tp, i) => (
                        <tr key={`tsa-${i}`} style={{ borderTop: '1px solid var(--border-color)' }}>
                          <td style={{ padding: '0.35rem' }}>TSA: {tp.questionsName}</td>
                          <td style={{ padding: '0.35rem', color: 'var(--text-secondary)' }}>{tp.answersName ?? c.planNoAnswer}</td>
                          <td style={{ padding: '0.35rem', color: 'var(--text-secondary)' }}>{c.planTsaDistribute}</td>
                          <td style={{ padding: '0.35rem' }}>{tp.changed ? c.planYes : c.planNo}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}
            {itemsLoaded && items.length === 0 && (
              <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '1rem' }}>{c.noItems}</div>
            )}

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

      {tab === 'mappings' && (
        <div className="card" style={{ padding: '1.5rem' }}>
          <h3 style={{ marginBottom: '0.5rem' }}>{c.mapTitle}</h3>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '1rem' }}>{c.mapHint}</p>

          {/* Editor form */}
          <div style={{ display: 'flex', gap: '0.75rem', flexWrap: 'wrap', alignItems: 'flex-end', marginBottom: '1rem' }}>
            <div>
              <label style={labelStyle}>{c.mapSection}</label>
              <input value={ruleForm.examSectionName ?? ''} onChange={e => setRuleForm({ ...ruleForm, examSectionName: e.target.value })}
                placeholder={c.mapSectionPlaceholder} style={{ ...inputStyle, width: '160px' }} />
            </div>
            <div>
              <label style={labelStyle}>{c.mapMatchType}</label>
              <select value={ruleForm.matchType} onChange={e => setRuleForm({ ...ruleForm, matchType: e.target.value })} style={inputStyle}>
                <option value="FolderSegment">{c.mapFolder}</option>
                <option value="FileName">{c.mapFileName}</option>
              </select>
            </div>
            <div>
              <label style={labelStyle}>{c.mapPattern}</label>
              <input value={ruleForm.pattern} onChange={e => setRuleForm({ ...ruleForm, pattern: e.target.value })}
                placeholder={c.mapPatternPlaceholder} style={{ ...inputStyle, width: '160px' }} />
            </div>
            <div>
              <label style={labelStyle}>{c.mapSkill}</label>
              <input value={ruleForm.skillName} onChange={e => setRuleForm({ ...ruleForm, skillName: e.target.value })}
                placeholder={c.mapSkillPlaceholder} style={{ ...inputStyle, width: '160px' }} />
            </div>
            <div>
              <label style={labelStyle}>{c.mapOrder}</label>
              <input type="number" value={ruleForm.sortOrder} onChange={e => setRuleForm({ ...ruleForm, sortOrder: Number(e.target.value) })}
                style={{ ...inputStyle, width: '80px' }} />
            </div>
            <label style={{ ...labelStyle, display: 'flex', alignItems: 'center', gap: '0.35rem', marginBottom: '0.6rem' }}>
              <input type="checkbox" checked={ruleForm.isActive} onChange={e => setRuleForm({ ...ruleForm, isActive: e.target.checked })} />
              {c.mapActive}
            </label>
            <label style={{ ...labelStyle, display: 'flex', alignItems: 'center', gap: '0.35rem', marginBottom: '0.6rem' }}>
              <input type="checkbox" checked={ruleForm.isIgnore} onChange={e => setRuleForm({ ...ruleForm, isIgnore: e.target.checked })} />
              {c.mapIgnore}
            </label>
          </div>
          <div style={{ marginBottom: '1rem' }}>
            <label style={labelStyle}>{c.mapGlossary}</label>
            <textarea value={ruleForm.glossary ?? ''} onChange={e => setRuleForm({ ...ruleForm, glossary: e.target.value })}
              rows={2} placeholder={c.mapGlossaryPlaceholder}
              style={{ ...inputStyle, width: '100%', resize: 'vertical', fontSize: '0.85rem' }} />
          </div>
          <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
            <button className="btn btn-primary" disabled={rulesBusy} onClick={submitRule}>
              {editingRuleId !== null ? c.mapSave : c.mapAdd}
            </button>
            {editingRuleId !== null && (
              <button className="btn btn-secondary" disabled={rulesBusy} onClick={cancelRuleEdit}>{c.mapCancel}</button>
            )}
            <button className="btn btn-secondary" disabled={rulesBusy} onClick={loadRules}>
              {rulesBusy ? c.refreshing : c.mapLoad}
            </button>
          </div>
          {rulesMsg && <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '1rem' }}>{rulesMsg}</div>}
          {rulesLoaded && rules.length === 0 && (
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{c.mapEmpty}</div>
          )}
          {rules.length > 0 && (
            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.83rem' }}>
                <thead>
                  <tr style={{ textAlign: 'left', color: 'var(--text-secondary)' }}>
                    <th style={{ padding: '0.4rem' }}>{c.mapSection}</th>
                    <th style={{ padding: '0.4rem' }}>{c.mapMatchType}</th>
                    <th style={{ padding: '0.4rem' }}>{c.mapPattern}</th>
                    <th style={{ padding: '0.4rem' }}>{c.mapSkill}</th>
                    <th style={{ padding: '0.4rem' }}>{c.mapOrder}</th>
                    <th style={{ padding: '0.4rem' }}>{c.mapActive}</th>
                    <th style={{ padding: '0.4rem' }}>{c.mapIgnore}</th>
                    <th style={{ padding: '0.4rem' }}>{c.colActions}</th>
                  </tr>
                </thead>
                <tbody>
                  {rules.map(r => (
                    <tr key={r.id} style={{ borderTop: '1px solid var(--border-color)', opacity: r.isActive ? 1 : 0.5 }}>
                      <td style={{ padding: '0.4rem' }}>{r.examSectionName || '*'}</td>
                      <td style={{ padding: '0.4rem' }}>{r.matchType === 'FileName' ? c.mapFileName : c.mapFolder}</td>
                      <td style={{ padding: '0.4rem' }}>{r.pattern}</td>
                      <td style={{ padding: '0.4rem' }}>{r.isIgnore ? '—' : r.skillName}</td>
                      <td style={{ padding: '0.4rem' }}>{r.sortOrder}</td>
                      <td style={{ padding: '0.4rem' }}>{r.isActive ? '✓' : '—'}</td>
                      <td style={{ padding: '0.4rem' }}>{r.isIgnore ? '🚫' : '—'}</td>
                      <td style={{ padding: '0.4rem', display: 'flex', gap: '0.3rem' }}>
                        <button className="btn btn-secondary" style={{ fontSize: '0.78rem', padding: '0.3rem 0.6rem' }}
                          disabled={rulesBusy} onClick={() => editRule(r)}>{c.mapEdit}</button>
                        <button className="btn btn-secondary" style={{ fontSize: '0.78rem', padding: '0.3rem 0.6rem', color: 'var(--error-color)' }}
                          disabled={rulesBusy} onClick={() => removeRule(r)}>{c.deleteBtn}</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {tab === 'cleanup' && (
        <div className="card" style={{ padding: '1.5rem' }}>
          <h3 style={{ marginBottom: '0.5rem' }}>{c.cleanupTitle}</h3>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '1rem' }}>{c.cleanupHint}</p>
          <button className="btn btn-secondary" disabled={skillsBusy} onClick={loadSkills} style={{ marginBottom: '1rem' }}>
            {skillsBusy ? c.loadingSkills : c.loadSkills}
          </button>
          {cleanupMsg && <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '1rem' }}>{cleanupMsg}</div>}
          {skillsLoaded && skills.length === 0 && (
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{c.noSkills}</div>
          )}
          {skills.length > 0 && (
            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.85rem' }}>
                <thead>
                  <tr style={{ textAlign: 'left', color: 'var(--text-secondary)' }}>
                    <th style={{ padding: '0.4rem' }}>{c.skillLabel}</th>
                    <th style={{ padding: '0.4rem' }}>{c.colTopics}</th>
                    <th style={{ padding: '0.4rem' }}>{c.questionsLabel}</th>
                    <th style={{ padding: '0.4rem' }}>{c.colActivity}</th>
                    <th style={{ padding: '0.4rem' }}>{c.colActions}</th>
                  </tr>
                </thead>
                <tbody>
                  {skills.map(s => (
                    <tr key={s.id} style={{ borderTop: '1px solid var(--border-color)' }}>
                      <td style={{ padding: '0.4rem' }}>{s.name}</td>
                      <td style={{ padding: '0.4rem' }}>{s.topicCount}</td>
                      <td style={{ padding: '0.4rem' }}>{s.questionCount}</td>
                      <td style={{ padding: '0.4rem', color: s.hasStudentActivity ? 'var(--warning-color)' : 'var(--text-secondary)' }}>
                        {s.hasStudentActivity ? c.hasActivity : c.noActivity}
                      </td>
                      <td style={{ padding: '0.4rem' }}>
                        <button
                          className="btn btn-secondary"
                          style={{ fontSize: '0.78rem', padding: '0.3rem 0.6rem', color: 'var(--error-color)' }}
                          disabled={deletingId !== null}
                          onClick={() => deleteSkill(s)}
                        >{deletingId === s.id ? c.deleting : c.deleteBtn}</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default AdminContentHubPage;
