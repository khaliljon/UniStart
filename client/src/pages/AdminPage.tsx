import { useEffect, useState, useCallback } from 'react';
import adminService from '../services/adminService';
import type { QuestionListItem, QuestionDetail, QuestionStats } from '../types';

const EXAM_COLORS: Record<string, string> = {
  SAT: '#4f46e5',
  TOEFL: '#0891b2',
  NUET: '#7c3aed',
};

const DIFF_COLORS: Record<string, string> = {
  Easy: 'var(--success-color)',
  Medium: 'var(--warning-color)',
  Hard: 'var(--error-color)',
};

function AdminPage() {
  const [questions, setQuestions] = useState<QuestionListItem[]>([]);
  const [stats, setStats] = useState<QuestionStats | null>(null);
  const [selected, setSelected] = useState<QuestionDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Filters
  const [filterExam, setFilterExam] = useState<string>('');
  const [filterDiff, setFilterDiff] = useState<string>('');
  const [filterTopic, setFilterTopic] = useState<string>('');

  // Tabs
  const [tab, setTab] = useState<'list' | 'stats' | 'import'>('stats');

  // Import
  const [importJson, setImportJson] = useState('');
  const [importResult, setImportResult] = useState<{ imported: number; failed: number; errors: string[] } | null>(null);
  const [importLoading, setImportLoading] = useState(false);

  const load = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const [q, s] = await Promise.all([
        adminService.getQuestions(filterExam || undefined, filterTopic || undefined, filterDiff || undefined),
        adminService.getStats(),
      ]);
      setQuestions(q);
      setStats(s);
    } catch {
      setError('Ошибка загрузки');
    } finally {
      setIsLoading(false);
    }
  }, [filterExam, filterDiff, filterTopic]);

  useEffect(() => { load(); }, [load]);

  const openDetail = async (id: number) => {
    try {
      const detail = await adminService.getQuestion(id);
      setSelected(detail);
    } catch {
      setError('Ошибка загрузки вопроса');
    }
  };

  const deleteQuestion = async (id: number) => {
    if (!confirm('Удалить вопрос?')) return;
    try {
      await adminService.deleteQuestion(id);
      setSelected(null);
      await load();
    } catch {
      setError('Ошибка удаления');
    }
  };

  const handleImport = async () => {
    try {
      setImportLoading(true);
      setImportResult(null);
      const parsed = JSON.parse(importJson);
      const arr = Array.isArray(parsed) ? parsed : parsed.questions;
      const result = await adminService.bulkImport(arr);
      setImportResult(result);
      if (result.imported > 0) await load();
    } catch (e) {
      setImportResult({ imported: 0, failed: 0, errors: [`JSON parse error: ${e}`] });
    } finally {
      setImportLoading(false);
    }
  };

  if (isLoading && !stats) {
    return (
      <div className="container animate-fade-in" style={{ textAlign: 'center', padding: '4rem 1rem' }}>
        <div className="loading-spinner" />
        <p style={{ color: 'var(--text-secondary)', marginTop: '1rem' }}>Загрузка…</p>
      </div>
    );
  }

  return (
    <div className="container animate-fade-in" style={{ padding: '2rem 1rem' }}>
      <h1 style={{ marginBottom: '0.5rem' }}>⚙️ Админ-панель</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
        Управление базой вопросов • {stats?.totalQuestions ?? 0} вопросов
      </p>

      {error && (
        <div style={{ color: 'var(--error-color)', marginBottom: '1rem' }}>
          {error}
          <button className="btn btn-secondary" style={{ marginLeft: '1rem', fontSize: '0.8rem' }} onClick={() => setError(null)}>✕</button>
        </div>
      )}

      {/* ─── Tabs ─────────────────────────────────────── */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1.5rem' }}>
        <button className={`btn ${tab === 'stats' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setTab('stats')}>
          📊 Статистика
        </button>
        <button className={`btn ${tab === 'list' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setTab('list')}>
          📋 Вопросы ({questions.length})
        </button>
        <button className={`btn ${tab === 'import' ? 'btn-primary' : 'btn-secondary'}`} onClick={() => setTab('import')}>
          📥 Импорт
        </button>
      </div>

      {/* ═══ STATS TAB ═══════════════════════════════════ */}
      {tab === 'stats' && stats && (
        <div className="animate-fade-in">
          {/* Top counters */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))', gap: '1rem', marginBottom: '1.5rem' }}>
            <StatCard label="Всего вопросов" value={stats.totalQuestions} icon="📝" />
            <StatCard label="Тем покрыто" value={`${stats.topicsWithQuestions}/${stats.topicsWithQuestions + stats.topicsWithoutQuestions}`} icon="🗂️" />
            <StatCard label="Без вопросов" value={stats.topicsWithoutQuestions} icon={stats.topicsWithoutQuestions === 0 ? '✅' : '⚠️'} />
          </div>

          {/* By exam */}
          <div className="card" style={{ padding: '1.25rem', marginBottom: '1rem' }}>
            <h3 style={{ marginBottom: '1rem' }}>По экзаменам</h3>
            <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
              {Object.entries(stats.byExam).map(([exam, count]) => (
                <div key={exam} style={{
                  padding: '0.75rem 1.5rem', borderRadius: '0.75rem', fontWeight: 700,
                  background: EXAM_COLORS[exam] || 'var(--primary-color)', color: '#fff',
                  minWidth: '100px', textAlign: 'center'
                }}>
                  <div style={{ fontSize: '1.5rem' }}>{count}</div>
                  <div style={{ fontSize: '0.8rem', opacity: 0.85 }}>{exam}</div>
                </div>
              ))}
            </div>
          </div>

          {/* By difficulty */}
          <div className="card" style={{ padding: '1.25rem', marginBottom: '1rem' }}>
            <h3 style={{ marginBottom: '1rem' }}>По сложности</h3>
            <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
              {['Easy', 'Medium', 'Hard'].map(d => (
                <DiffBadge key={d} label={d} count={stats.byDifficulty[d] || 0} />
              ))}
            </div>
          </div>

          {/* By topic */}
          <div className="card" style={{ padding: '1.25rem' }}>
            <h3 style={{ marginBottom: '1rem' }}>По темам</h3>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: '0.5rem' }}>
              {Object.entries(stats.byTopic)
                .sort((a, b) => b[1] - a[1])
                .map(([topic, count]) => (
                  <div key={topic} style={{
                    display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                    padding: '0.5rem 0.75rem', borderRadius: '0.5rem',
                    background: 'var(--background-color)', fontSize: '0.9rem'
                  }}>
                    <span style={{ color: 'var(--text-primary)' }}>{topic}</span>
                    <span style={{
                      fontWeight: 700,
                      color: count >= 7 ? 'var(--success-color)' : count >= 4 ? 'var(--warning-color)' : 'var(--error-color)'
                    }}>{count}</span>
                  </div>
                ))}
            </div>
          </div>
        </div>
      )}

      {/* ═══ LIST TAB ════════════════════════════════════ */}
      {tab === 'list' && (
        <div className="animate-fade-in">
          {/* Filters */}
          <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
            <select value={filterExam} onChange={e => setFilterExam(e.target.value)}
              style={{ padding: '0.5rem', borderRadius: '0.5rem', border: '1px solid var(--border-color)', background: 'var(--card-background)', color: 'var(--text-primary)' }}>
              <option value="">Все экзамены</option>
              <option value="SAT">SAT</option>
              <option value="TOEFL">TOEFL</option>
              <option value="NUET">NUET</option>
            </select>
            <select value={filterDiff} onChange={e => setFilterDiff(e.target.value)}
              style={{ padding: '0.5rem', borderRadius: '0.5rem', border: '1px solid var(--border-color)', background: 'var(--card-background)', color: 'var(--text-primary)' }}>
              <option value="">Все уровни</option>
              <option value="Easy">Easy</option>
              <option value="Medium">Medium</option>
              <option value="Hard">Hard</option>
            </select>
            <input placeholder="Тема…" value={filterTopic} onChange={e => setFilterTopic(e.target.value)}
              style={{ padding: '0.5rem', borderRadius: '0.5rem', border: '1px solid var(--border-color)', background: 'var(--card-background)', color: 'var(--text-primary)', flex: 1, minWidth: '120px' }} />
          </div>

          {/* Question table */}
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
              <thead>
                <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                  <th style={{ padding: '0.75rem 0.5rem' }}>ID</th>
                  <th style={{ padding: '0.75rem 0.5rem' }}>Экзамен</th>
                  <th style={{ padding: '0.75rem 0.5rem' }}>Тема</th>
                  <th style={{ padding: '0.75rem 0.5rem' }}>Вопрос</th>
                  <th style={{ padding: '0.75rem 0.5rem' }}>Уровень</th>
                  <th style={{ padding: '0.75rem 0.5rem' }}>b</th>
                </tr>
              </thead>
              <tbody>
                {questions.map(q => (
                  <tr key={q.id}
                    onClick={() => openDetail(q.id)}
                    style={{
                      borderBottom: '1px solid var(--border-color)',
                      cursor: 'pointer',
                      transition: 'background 0.15s',
                    }}
                    onMouseEnter={e => e.currentTarget.style.background = 'var(--background-color)'}
                    onMouseLeave={e => e.currentTarget.style.background = 'transparent'}
                  >
                    <td style={{ padding: '0.6rem 0.5rem', color: 'var(--text-secondary)' }}>{q.id}</td>
                    <td style={{ padding: '0.6rem 0.5rem' }}>
                      <span style={{
                        padding: '0.15rem 0.5rem', borderRadius: '999px', fontSize: '0.75rem', fontWeight: 600,
                        background: EXAM_COLORS[q.examTypeCode] || '#666', color: '#fff'
                      }}>{q.examTypeCode}</span>
                    </td>
                    <td style={{ padding: '0.6rem 0.5rem' }}>{q.topicName}</td>
                    <td style={{ padding: '0.6rem 0.5rem', maxWidth: '300px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {q.text}
                    </td>
                    <td style={{ padding: '0.6rem 0.5rem' }}>
                      <span style={{ color: DIFF_COLORS[q.difficulty] || 'inherit', fontWeight: 600 }}>{q.difficulty}</span>
                    </td>
                    <td style={{ padding: '0.6rem 0.5rem', color: 'var(--text-secondary)' }}>{q.difficultyParam}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {questions.length === 0 && (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
              Нет вопросов по выбранным фильтрам
            </div>
          )}
        </div>
      )}

      {/* ═══ IMPORT TAB ══════════════════════════════════ */}
      {tab === 'import' && (
        <div className="animate-fade-in">
          <div className="card" style={{ padding: '1.5rem' }}>
            <h3 style={{ marginBottom: '0.5rem' }}>📥 Bulk Import (JSON)</h3>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '1rem' }}>
              Вставьте массив вопросов в формате JSON. Каждый вопрос должен содержать: topicId, text, difficulty, answerOptions[].
            </p>
            <textarea
              value={importJson}
              onChange={e => setImportJson(e.target.value)}
              rows={12}
              placeholder={`[\n  {\n    "topicId": 1,\n    "text": "What is 2+2?",\n    "difficulty": "Easy",\n    "explanation": "Basic addition",\n    "answerOptions": [\n      { "text": "3", "isCorrect": false },\n      { "text": "4", "isCorrect": true },\n      { "text": "5", "isCorrect": false },\n      { "text": "6", "isCorrect": false }\n    ]\n  }\n]`}
              style={{
                width: '100%', fontFamily: 'monospace', fontSize: '0.85rem', padding: '0.75rem',
                borderRadius: '0.5rem', border: '1px solid var(--border-color)',
                background: 'var(--background-color)', color: 'var(--text-primary)',
                resize: 'vertical'
              }}
            />
            <button
              className="btn btn-primary"
              style={{ marginTop: '1rem' }}
              disabled={importLoading || !importJson.trim()}
              onClick={handleImport}
            >
              {importLoading ? 'Импорт…' : 'Импортировать'}
            </button>

            {importResult && (
              <div style={{
                marginTop: '1rem', padding: '1rem', borderRadius: '0.5rem',
                background: importResult.failed > 0 ? 'rgba(239,68,68,0.08)' : 'rgba(16,185,129,0.08)',
                border: `1px solid ${importResult.failed > 0 ? 'var(--error-color)' : 'var(--success-color)'}`
              }}>
                <div style={{ fontWeight: 600, marginBottom: '0.5rem' }}>
                  ✅ Импортировано: {importResult.imported} / {importResult.imported + importResult.failed}
                </div>
                {importResult.errors.length > 0 && (
                  <div style={{ fontSize: '0.85rem', color: 'var(--error-color)' }}>
                    {importResult.errors.map((e, i) => <div key={i}>• {e}</div>)}
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      )}

      {/* ═══ Question Detail Modal ═══════════════════════ */}
      {selected && (
        <div
          style={{
            position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            zIndex: 1000, padding: '1rem'
          }}
          onClick={() => setSelected(null)}
        >
          <div
            className="card"
            style={{ maxWidth: '700px', width: '100%', maxHeight: '80vh', overflow: 'auto', padding: '2rem' }}
            onClick={e => e.stopPropagation()}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1rem' }}>
              <div>
                <span style={{
                  padding: '0.15rem 0.5rem', borderRadius: '999px', fontSize: '0.75rem', fontWeight: 600,
                  background: EXAM_COLORS[selected.examTypeCode] || '#666', color: '#fff', marginRight: '0.5rem'
                }}>{selected.examTypeCode}</span>
                <span style={{ color: DIFF_COLORS[selected.difficulty], fontWeight: 600 }}>{selected.difficulty}</span>
              </div>
              <button onClick={() => setSelected(null)} style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '1.25rem', color: 'var(--text-secondary)' }}>✕</button>
            </div>

            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginBottom: '0.75rem' }}>
              {selected.sectionName} → {selected.topicName} • ID: {selected.id}
            </div>

            <div style={{ fontSize: '1rem', fontWeight: 600, marginBottom: '1rem', lineHeight: 1.5 }}>
              {selected.text}
            </div>

            <div style={{ marginBottom: '1rem' }}>
              {selected.answerOptions.map((opt, i) => (
                <div key={opt.id} style={{
                  padding: '0.5rem 0.75rem', marginBottom: '0.4rem', borderRadius: '0.5rem',
                  border: `1px solid ${opt.isCorrect ? 'var(--success-color)' : 'var(--border-color)'}`,
                  background: opt.isCorrect ? 'rgba(16,185,129,0.08)' : 'transparent',
                  display: 'flex', alignItems: 'center', gap: '0.5rem'
                }}>
                  <span style={{ fontWeight: 600, color: 'var(--text-secondary)', minWidth: '1.2rem' }}>{String.fromCharCode(65 + i)}</span>
                  <span>{opt.text}</span>
                  {opt.isCorrect && <span style={{ marginLeft: 'auto', color: 'var(--success-color)', fontWeight: 600 }}>✓</span>}
                </div>
              ))}
            </div>

            {selected.explanation && (
              <div style={{
                padding: '0.75rem', borderRadius: '0.5rem',
                background: 'rgba(79,70,229,0.06)', border: '1px solid var(--primary-color)',
                fontSize: '0.9rem', marginBottom: '1rem'
              }}>
                <strong>Объяснение:</strong> {selected.explanation}
              </div>
            )}

            <div style={{ display: 'flex', gap: '1rem', fontSize: '0.8rem', color: 'var(--text-secondary)', marginBottom: '1rem' }}>
              <span>b = {selected.difficultyParam}</span>
              <span>a = {selected.discriminationParam}</span>
              <span>c = {selected.guessParam}</span>
            </div>

            <button className="btn btn-secondary" style={{ fontSize: '0.85rem', color: 'var(--error-color)' }}
              onClick={() => deleteQuestion(selected.id)}>
              🗑️ Удалить
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

// ─── Sub-components ──────────────────────────────────────

function StatCard({ label, value, icon }: { label: string; value: string | number; icon: string }) {
  return (
    <div className="card" style={{ padding: '1.25rem', textAlign: 'center' }}>
      <div style={{ fontSize: '1.75rem', marginBottom: '0.25rem' }}>{icon}</div>
      <div style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--primary-color)' }}>{value}</div>
      <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{label}</div>
    </div>
  );
}

function DiffBadge({ label, count }: { label: string; count: number }) {
  return (
    <div style={{
      display: 'flex', alignItems: 'center', gap: '0.5rem',
      padding: '0.5rem 1rem', borderRadius: '999px',
      background: 'var(--background-color)', fontWeight: 600
    }}>
      <span style={{ color: DIFF_COLORS[label] }}>●</span>
      <span>{label}</span>
      <span style={{ color: 'var(--text-secondary)' }}>{count}</span>
    </div>
  );
}

export default AdminPage;
