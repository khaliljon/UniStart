import { useEffect, useState, useCallback } from 'react';
import { tutorService } from '../services/tutorService';
import type { StudentAssignmentListItem, StudentAssignmentDetail, SubmitAssignmentAnswerResult } from '../types';

const STATUS_LABELS: Record<string, string> = {
  Assigned: 'Назначено',
  InProgress: 'В процессе',
  Completed: 'Выполнено',
  Overdue: 'Просрочено',
};
const STATUS_COLORS: Record<string, string> = {
  Assigned: '#6b7280',
  InProgress: '#3b82f6',
  Completed: '#22c55e',
  Overdue: '#ef4444',
};

export default function StudentAssignmentsPage() {
  const [assignments, setAssignments] = useState<StudentAssignmentListItem[]>([]);
  const [detail, setDetail] = useState<StudentAssignmentDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedOption, setSelectedOption] = useState<number | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [lastResult, setLastResult] = useState<SubmitAssignmentAnswerResult | null>(null);
  const [currentQIdx, setCurrentQIdx] = useState(0);

  const loadList = useCallback(async () => {
    try {
      setLoading(true);
      const data = await tutorService.getMyAssignments();
      setAssignments(data);
    } catch { setError('Ошибка загрузки заданий'); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { loadList(); }, [loadList]);

  const openAssignment = async (id: number) => {
    try {
      setError('');
      setLastResult(null);
      const d = await tutorService.getMyAssignment(id);
      setDetail(d);
      // find first unanswered question
      const firstUnanswered = d.questions.findIndex(q => q.selectedOptionId === null);
      setCurrentQIdx(firstUnanswered >= 0 ? firstUnanswered : 0);
      setSelectedOption(null);
    } catch { setError('Ошибка загрузки задания'); }
  };

  const submitAnswer = async () => {
    if (!detail || selectedOption === null) return;
    const q = detail.questions[currentQIdx];
    if (!q || q.selectedOptionId !== null) return;
    try {
      setSubmitting(true);
      setError('');
      const result = await tutorService.submitAssignmentAnswer(detail.id, {
        questionId: q.questionId,
        selectedOptionId: selectedOption,
      });
      setLastResult(result);
      // update local state
      setDetail(prev => {
        if (!prev) return prev;
        const updated = { ...prev };
        updated.answeredCount = result.answeredCount;
        updated.status = result.isCompleted ? 'Completed' : 'InProgress';
        updated.questions = prev.questions.map(qq =>
          qq.questionId === q.questionId
            ? { ...qq, selectedOptionId: selectedOption, isCorrect: result.isCorrect, explanation: result.explanation }
            : qq
        );
        return updated;
      });
      setSelectedOption(null);
    } catch (e: any) {
      setError(e.response?.data || 'Ошибка отправки ответа');
    } finally { setSubmitting(false); }
  };

  const goNext = () => {
    if (!detail) return;
    setLastResult(null);
    const nextIdx = detail.questions.findIndex((q, i) => i > currentQIdx && q.selectedOptionId === null);
    if (nextIdx >= 0) {
      setCurrentQIdx(nextIdx);
    } else {
      setCurrentQIdx(currentQIdx + 1 < detail.questions.length ? currentQIdx + 1 : currentQIdx);
    }
  };

  const fmt = (d: string) => new Date(d).toLocaleDateString('ru-RU', {
    day: '2-digit', month: '2-digit', year: 'numeric',
  });

  const fmtDt = (d: string) => new Date(d).toLocaleString('ru-RU', {
    day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit',
  });

  // ── LIST ────────────────────────────────────────────

  if (!detail) return (
    <div className="page-container" style={{ maxWidth: 800, margin: '0 auto', padding: '2rem 1rem' }}>
      <h2 style={{ marginBottom: '1.5rem' }}>Мои задания</h2>

      {error && <div className="alert alert-danger" style={{ marginBottom: '1rem' }}>{error}</div>}

      {loading ? (
        <div className="loading"><div className="spinner" /></div>
      ) : assignments.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '3rem 1rem', opacity: 0.6 }}>
          <p style={{ fontSize: '1.1rem' }}>Нет заданий</p>
          <p>Ваш тьютор ещё не назначил заданий</p>
        </div>
      ) : (
        <div style={{ display: 'grid', gap: '1rem' }}>
          {assignments.map(a => (
            <div
              key={a.id}
              className="card"
              style={{ padding: '1.25rem', cursor: 'pointer', transition: 'box-shadow 0.2s' }}
              onClick={() => openAssignment(a.id)}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div style={{ flex: 1 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.5rem' }}>
                    <h4 style={{ margin: 0 }}>{a.title}</h4>
                    <span style={{
                      fontSize: '0.7rem', padding: '0.15rem 0.5rem', borderRadius: '999px',
                      color: '#fff', background: STATUS_COLORS[a.status] || '#6b7280',
                    }}>{STATUS_LABELS[a.status] || a.status}</span>
                  </div>
                  {a.description && <p style={{ margin: '0 0 0.5rem', opacity: 0.7, fontSize: '0.9rem' }}>{a.description}</p>}
                  <div style={{ display: 'flex', gap: '1.5rem', fontSize: '0.85rem', opacity: 0.7 }}>
                    <span>Тьютор: {a.tutorName}</span>
                    <span>📝 {a.answeredCount}/{a.totalQuestions}</span>
                    {a.score !== null && <span>🏆 {a.score}%</span>}
                    {a.deadline && <span>📅 до {fmt(a.deadline)}</span>}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );

  // ── DETAIL / WORK ───────────────────────────────────

  const q = detail.questions[currentQIdx];
  const isCompleted = detail.status === 'Completed';
  const allAnswered = detail.questions.every(qq => qq.selectedOptionId !== null);

  return (
    <div className="page-container" style={{ maxWidth: 800, margin: '0 auto', padding: '2rem 1rem' }}>
      <button className="btn btn-outline" onClick={() => { setDetail(null); setLastResult(null); loadList(); }}
        style={{ marginBottom: '1rem' }}>
        ← Назад к заданиям
      </button>

      {error && <div className="alert alert-danger" style={{ marginBottom: '1rem' }}>{error}</div>}

      <div className="card" style={{ padding: '1.25rem', marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <h3 style={{ margin: '0 0 0.25rem' }}>{detail.title}</h3>
            <span style={{ fontSize: '0.85rem', opacity: 0.6 }}>от {detail.tutorName}</span>
          </div>
          <span style={{
            fontSize: '0.75rem', padding: '0.2rem 0.6rem', borderRadius: '999px',
            color: '#fff', background: STATUS_COLORS[detail.status] || '#6b7280',
          }}>{STATUS_LABELS[detail.status] || detail.status}</span>
        </div>
        {detail.description && <p style={{ margin: '0.75rem 0 0', opacity: 0.7, fontSize: '0.9rem' }}>{detail.description}</p>}
        <div style={{ marginTop: '0.75rem', fontSize: '0.85rem', opacity: 0.6 }}>
          Прогресс: {detail.answeredCount}/{detail.totalQuestions}
          {detail.deadline && ` · Дедлайн: ${fmtDt(detail.deadline)}`}
        </div>
        {/* progress bar */}
        <div style={{ marginTop: '0.5rem', height: 6, borderRadius: 3, background: 'var(--border-color, #e5e7eb)' }}>
          <div style={{
            height: '100%', borderRadius: 3, transition: 'width 0.3s',
            width: `${detail.totalQuestions > 0 ? (detail.answeredCount / detail.totalQuestions) * 100 : 0}%`,
            background: isCompleted ? '#22c55e' : '#3b82f6',
          }} />
        </div>
      </div>

      {/* Question navigation */}
      <div style={{ display: 'flex', gap: '0.35rem', flexWrap: 'wrap', marginBottom: '1rem' }}>
        {detail.questions.map((qq, i) => (
          <button
            key={qq.questionId}
            className={`btn btn-sm ${i === currentQIdx ? 'btn-primary' : 'btn-outline'}`}
            style={{
              minWidth: 36, padding: '0.25rem 0.5rem',
              background: i === currentQIdx ? undefined :
                qq.selectedOptionId !== null ? (qq.isCorrect ? '#dcfce7' : '#fee2e2') : undefined,
              color: i === currentQIdx ? undefined :
                qq.selectedOptionId !== null ? (qq.isCorrect ? '#166534' : '#991b1b') : undefined,
            }}
            onClick={() => { setCurrentQIdx(i); setLastResult(null); setSelectedOption(null); }}
          >
            {i + 1}
          </button>
        ))}
      </div>

      {/* Current question */}
      {q && (
        <div className="card" style={{ padding: '1.5rem' }}>
          <div style={{ marginBottom: '1rem' }}>
            <span style={{ fontSize: '0.8rem', opacity: 0.5 }}>Вопрос {currentQIdx + 1} из {detail.questions.length}</span>
            <span style={{
              marginLeft: '0.75rem', fontSize: '0.7rem', padding: '0.1rem 0.4rem',
              borderRadius: '999px', background: '#8b5cf620', color: '#8b5cf6',
            }}>{q.difficulty}</span>
          </div>
          <p style={{ fontSize: '1.05rem', marginBottom: '1.25rem', lineHeight: 1.6 }}>{q.text}</p>

          <div style={{ display: 'grid', gap: '0.5rem' }}>
            {q.options.map(o => {
              const isAnswered = q.selectedOptionId !== null;
              const isSelected = isAnswered ? o.id === q.selectedOptionId : o.id === selectedOption;
              const showCorrect = isAnswered && o.isCorrect;
              const showWrong = isAnswered && o.id === q.selectedOptionId && !q.isCorrect;

              return (
                <label
                  key={o.id}
                  style={{
                    display: 'flex', alignItems: 'center', gap: '0.75rem',
                    padding: '0.75rem 1rem', borderRadius: '0.5rem', cursor: isAnswered ? 'default' : 'pointer',
                    border: `2px solid ${showCorrect ? '#22c55e' : showWrong ? '#ef4444' : isSelected ? '#8b5cf6' : 'var(--border-color, #e5e7eb)'}`,
                    background: showCorrect ? '#dcfce7' : showWrong ? '#fee2e2' : isSelected ? '#ede9fe' : 'transparent',
                    transition: 'all 0.15s',
                  }}
                >
                  <input
                    type="radio"
                    name="option"
                    checked={isSelected}
                    disabled={isAnswered}
                    onChange={() => setSelectedOption(o.id)}
                  />
                  <span>{o.text}</span>
                </label>
              );
            })}
          </div>

          {/* Result feedback */}
          {lastResult && (
            <div style={{
              marginTop: '1rem', padding: '1rem', borderRadius: '0.5rem',
              background: lastResult.isCorrect ? '#dcfce7' : '#fee2e2',
              border: `1px solid ${lastResult.isCorrect ? '#22c55e' : '#ef4444'}`,
            }}>
              <strong>{lastResult.isCorrect ? '✅ Верно!' : '❌ Неверно'}</strong>
              {lastResult.explanation && (
                <p style={{ margin: '0.5rem 0 0', fontSize: '0.9rem' }}>{lastResult.explanation}</p>
              )}
              {lastResult.isCompleted && (
                <p style={{ margin: '0.5rem 0 0', fontWeight: 600 }}>
                  🎉 Задание завершено! Итоговый балл: {lastResult.score}%
                </p>
              )}
            </div>
          )}

          {/* Explanation for already-answered */}
          {q.selectedOptionId !== null && !lastResult && q.explanation && (
            <div style={{
              marginTop: '1rem', padding: '0.75rem 1rem', borderRadius: '0.5rem',
              background: 'var(--bg-secondary, #f9fafb)',
              border: '1px solid var(--border-color, #e5e7eb)',
              fontSize: '0.9rem',
            }}>
              <strong>Объяснение:</strong> {q.explanation}
            </div>
          )}

          {/* Action buttons */}
          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem', marginTop: '1.25rem' }}>
            {q.selectedOptionId === null && !lastResult && (
              <button className="btn btn-primary" disabled={selectedOption === null || submitting}
                onClick={submitAnswer}>
                {submitting ? 'Отправка…' : 'Ответить'}
              </button>
            )}
            {(lastResult || q.selectedOptionId !== null) && !allAnswered && (
              <button className="btn btn-primary" onClick={goNext}>
                Следующий вопрос →
              </button>
            )}
          </div>
        </div>
      )}

      {/* Completion summary */}
      {allAnswered && isCompleted && (
        <div className="card" style={{ padding: '1.5rem', marginTop: '1.5rem', textAlign: 'center' }}>
          <h3>🎉 Задание выполнено!</h3>
          <div style={{ fontSize: '2rem', fontWeight: 700, color: '#8b5cf6', margin: '0.5rem 0' }}>
            {detail.questions.filter(qq => qq.isCorrect).length}/{detail.totalQuestions}
          </div>
          <p style={{ opacity: 0.7 }}>правильных ответов</p>
        </div>
      )}
    </div>
  );
}
