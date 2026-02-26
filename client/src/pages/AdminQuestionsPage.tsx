import { useEffect, useState, useCallback } from 'react';
import adminService from '../services/adminService';
import type { QuestionListItem, QuestionDetail } from '../types';

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

function AdminQuestionsPage() {
  const [questions, setQuestions] = useState<QuestionListItem[]>([]);
  const [selected, setSelected] = useState<QuestionDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [filterExam, setFilterExam] = useState<string>('');
  const [filterDiff, setFilterDiff] = useState<string>('');
  const [filterTopic, setFilterTopic] = useState<string>('');

  const load = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const q = await adminService.getQuestions(filterExam || undefined, filterTopic || undefined, filterDiff || undefined);
      setQuestions(q);
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

  if (isLoading && questions.length === 0) {
    return (
      <div className="container animate-fade-in" style={{ textAlign: 'center', padding: '4rem 1rem' }}>
        <div className="loading-spinner" />
        <p style={{ color: 'var(--text-secondary)', marginTop: '1rem' }}>Загрузка…</p>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
      <h1 style={{ marginBottom: '0.5rem' }}>📋 Управление вопросами</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
        {questions.length} вопросов найдено
      </p>

      {error && (
        <div style={{ color: 'var(--error-color)', marginBottom: '1rem' }}>
          {error}
          <button className="btn btn-secondary" style={{ marginLeft: '1rem', fontSize: '0.8rem' }} onClick={() => setError(null)}>✕</button>
        </div>
      )}

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
        <select value={filterExam} onChange={e => setFilterExam(e.target.value)}
          style={{ padding: '0.5rem' }}>
          <option value="">Все экзамены</option>
          <option value="SAT">SAT</option>
          <option value="TOEFL">TOEFL</option>
          <option value="NUET">NUET</option>
        </select>
        <select value={filterDiff} onChange={e => setFilterDiff(e.target.value)}
          style={{ padding: '0.5rem' }}>
          <option value="">Все уровни</option>
          <option value="Easy">Easy</option>
          <option value="Medium">Medium</option>
          <option value="Hard">Hard</option>
        </select>
        <input placeholder="Тема…" value={filterTopic} onChange={e => setFilterTopic(e.target.value)}
          className="form-input"
          style={{ flex: 1, minWidth: '120px' }} />
      </div>

      {/* Table */}
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

      {questions.length === 0 && !isLoading && (
        <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
          Нет вопросов по выбранным фильтрам
        </div>
      )}

      {/* Detail Modal */}
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

export default AdminQuestionsPage;
