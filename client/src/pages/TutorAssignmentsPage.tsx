import { useEffect, useState, useCallback } from 'react';
import { tutorService } from '../services/tutorService';
import type {
  AssignmentListItem, AssignmentDetail, TutorStudentInfo,
  TutorQuestionsPage as TQPage, AssignmentStudentProgress,
} from '../types';

type ModalMode = 'list' | 'create' | 'detail';

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

export default function TutorAssignmentsPage() {
  const [assignments, setAssignments] = useState<AssignmentListItem[]>([]);
  const [detail, setDetail] = useState<AssignmentDetail | null>(null);
  const [mode, setMode] = useState<ModalMode>('list');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // create form
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [deadline, setDeadline] = useState('');
  const [students, setStudents] = useState<TutorStudentInfo[]>([]);
  const [selectedStudentIds, setSelectedStudentIds] = useState<number[]>([]);
  const [questions, setQuestions] = useState<TQPage['items']>([]);
  const [selectedQuestionIds, setSelectedQuestionIds] = useState<number[]>([]);
  const [qSearch, setQSearch] = useState('');
  const [qPage, setQPage] = useState(1);
  const [qTotal, setQTotal] = useState(0);

  const loadAssignments = useCallback(async () => {
    try {
      setLoading(true);
      const data = await tutorService.getAssignments();
      setAssignments(data);
    } catch { setError('Ошибка загрузки заданий'); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { loadAssignments(); }, [loadAssignments]);

  const loadQuestions = useCallback(async (page: number, search: string) => {
    try {
      const data = await tutorService.getMyQuestions({ page, pageSize: 20, search: search || undefined });
      setQuestions(data.items);
      setQTotal(data.totalPages);
      setQPage(data.page);
    } catch { /* ignore */ }
  }, []);

  const openCreate = async () => {
    setMode('create');
    setTitle('');
    setDescription('');
    setDeadline('');
    setSelectedStudentIds([]);
    setSelectedQuestionIds([]);
    setQSearch('');
    setError('');
    try {
      const [s] = await Promise.all([
        tutorService.getLinkedStudents(),
        loadQuestions(1, ''),
      ]);
      setStudents(s);
    } catch { setError('Ошибка загрузки данных'); }
  };

  const openDetail = async (id: number) => {
    try {
      const d = await tutorService.getAssignment(id);
      setDetail(d);
      setMode('detail');
    } catch { setError('Ошибка загрузки задания'); }
  };

  const handleCreate = async () => {
    if (!title.trim()) { setError('Введите название'); return; }
    if (selectedQuestionIds.length === 0) { setError('Выберите хотя бы один вопрос'); return; }
    if (selectedStudentIds.length === 0) { setError('Выберите хотя бы одного ученика'); return; }
    try {
      setError('');
      await tutorService.createAssignment({
        title: title.trim(),
        description: description.trim() || undefined,
        deadline: deadline || undefined,
        questionIds: selectedQuestionIds,
        studentUserIds: selectedStudentIds,
      });
      setSuccess('Задание создано');
      setMode('list');
      loadAssignments();
      setTimeout(() => setSuccess(''), 3000);
    } catch (e: any) {
      setError(e.response?.data || 'Ошибка создания задания');
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Удалить задание? Все ответы учеников будут потеряны.')) return;
    try {
      await tutorService.deleteAssignment(id);
      setSuccess('Задание удалено');
      if (mode === 'detail') setMode('list');
      loadAssignments();
      setTimeout(() => setSuccess(''), 3000);
    } catch { setError('Ошибка удаления'); }
  };

  const toggleActive = async (a: AssignmentListItem) => {
    try {
      await tutorService.updateAssignment(a.id, { isActive: !a.isActive });
      loadAssignments();
    } catch { setError('Ошибка обновления'); }
  };

  const toggleQuestion = (id: number) => {
    setSelectedQuestionIds(prev =>
      prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
    );
  };

  const toggleStudent = (id: number) => {
    setSelectedStudentIds(prev =>
      prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
    );
  };

  const fmt = (d: string) => new Date(d).toLocaleDateString('ru-RU', {
    day: '2-digit', month: '2-digit', year: 'numeric',
  });

  const fmtDt = (d: string) => new Date(d).toLocaleString('ru-RU', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });

  // ── LIST VIEW ───────────────────────────────────────

  if (mode === 'list') return (
    <div className="page-container" style={{ maxWidth: 960, margin: '0 auto', padding: '2rem 1rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h2 style={{ margin: 0 }}>Задания <span style={{ fontSize: '0.85rem', opacity: 0.6 }}>({assignments.length})</span></h2>
        <button className="btn btn-primary" onClick={openCreate}>+ Создать задание</button>
      </div>

      {success && <div className="alert alert-success" style={{ marginBottom: '1rem' }}>{success}</div>}
      {error && <div className="alert alert-danger" style={{ marginBottom: '1rem' }}>{error}</div>}

      {loading ? (
        <div className="loading"><div className="spinner" /></div>
      ) : assignments.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '3rem 1rem', opacity: 0.6 }}>
          <p style={{ fontSize: '1.1rem' }}>Нет заданий</p>
          <p>Создайте первое домашнее задание для ваших учеников</p>
        </div>
      ) : (
        <div style={{ display: 'grid', gap: '1rem' }}>
          {assignments.map(a => (
            <div
              key={a.id}
              className="card"
              style={{ padding: '1.25rem', cursor: 'pointer', transition: 'box-shadow 0.2s' }}
              onClick={() => openDetail(a.id)}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: '1rem' }}>
                <div style={{ flex: 1 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.5rem' }}>
                    <h4 style={{ margin: 0 }}>{a.title}</h4>
                    {!a.isActive && (
                      <span style={{
                        fontSize: '0.7rem', padding: '0.15rem 0.5rem',
                        borderRadius: '999px', background: '#6b7280', color: '#fff',
                      }}>неактивно</span>
                    )}
                  </div>
                  {a.description && <p style={{ margin: '0 0 0.5rem', opacity: 0.7, fontSize: '0.9rem' }}>{a.description}</p>}
                  <div style={{ display: 'flex', gap: '1.5rem', fontSize: '0.85rem', opacity: 0.7 }}>
                    <span>📝 {a.questionCount} вопросов</span>
                    <span>👤 {a.studentCount} учеников</span>
                    <span>✅ {a.completedCount} выполнено</span>
                    {a.deadline && <span>📅 до {fmt(a.deadline)}</span>}
                  </div>
                </div>
                <div style={{ display: 'flex', gap: '0.5rem' }} onClick={e => e.stopPropagation()}>
                  <button
                    className={`btn btn-sm ${a.isActive ? 'btn-outline' : 'btn-primary'}`}
                    onClick={() => toggleActive(a)}
                    title={a.isActive ? 'Деактивировать' : 'Активировать'}
                    style={{ fontSize: '0.8rem', padding: '0.3rem 0.7rem' }}
                  >{a.isActive ? '⏸' : '▶'}</button>
                  <button
                    className="btn btn-sm btn-danger"
                    onClick={() => handleDelete(a.id)}
                    title="Удалить"
                    style={{ fontSize: '0.8rem', padding: '0.3rem 0.7rem' }}
                  >✕</button>
                </div>
              </div>
              <div style={{ fontSize: '0.75rem', opacity: 0.5, marginTop: '0.5rem' }}>
                Создано {fmt(a.createdAt)}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );

  // ── DETAIL VIEW ─────────────────────────────────────

  if (mode === 'detail' && detail) return (
    <div className="page-container" style={{ maxWidth: 960, margin: '0 auto', padding: '2rem 1rem' }}>
      <button className="btn btn-outline" onClick={() => { setMode('list'); setDetail(null); }} style={{ marginBottom: '1rem' }}>
        ← Назад
      </button>

      {success && <div className="alert alert-success" style={{ marginBottom: '1rem' }}>{success}</div>}
      {error && <div className="alert alert-danger" style={{ marginBottom: '1rem' }}>{error}</div>}

      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <div>
            <h2 style={{ margin: '0 0 0.5rem' }}>{detail.title}</h2>
            {detail.description && <p style={{ opacity: 0.7 }}>{detail.description}</p>}
          </div>
          <button className="btn btn-sm btn-danger" onClick={() => handleDelete(detail.id)}>Удалить</button>
        </div>
        <div style={{ display: 'flex', gap: '1.5rem', fontSize: '0.85rem', opacity: 0.7, marginTop: '0.75rem' }}>
          {detail.deadline && <span>📅 Дедлайн: {fmtDt(detail.deadline)}</span>}
          <span>Создано: {fmt(detail.createdAt)}</span>
          <span>{detail.isActive ? '✅ Активно' : '⏸ Неактивно'}</span>
        </div>
      </div>

      {/* Questions */}
      <h4 style={{ marginBottom: '0.75rem' }}>Вопросы ({detail.questions.length})</h4>
      <div style={{ display: 'grid', gap: '0.5rem', marginBottom: '1.5rem' }}>
        {detail.questions.map((q, i) => (
          <div key={q.questionId} className="card" style={{ padding: '0.75rem 1rem', fontSize: '0.9rem' }}>
            <span style={{ opacity: 0.5, marginRight: '0.75rem' }}>#{i + 1}</span>
            <span>{q.text.length > 80 ? q.text.slice(0, 80) + '…' : q.text}</span>
            <span style={{
              marginLeft: '0.75rem', fontSize: '0.75rem', padding: '0.1rem 0.5rem',
              borderRadius: '999px', background: '#8b5cf620', color: '#8b5cf6',
            }}>{q.topicName}</span>
          </div>
        ))}
      </div>

      {/* Students progress */}
      <h4 style={{ marginBottom: '0.75rem' }}>Прогресс учеников ({detail.students.length})</h4>
      <div className="table-responsive">
        <table className="table">
          <thead>
            <tr>
              <th>Ученик</th>
              <th>Статус</th>
              <th>Прогресс</th>
              <th>Баллы</th>
              <th>Начато</th>
              <th>Завершено</th>
            </tr>
          </thead>
          <tbody>
            {detail.students.map((s: AssignmentStudentProgress) => (
              <tr key={s.studentUserId}>
                <td>{s.studentName}</td>
                <td>
                  <span style={{
                    fontSize: '0.75rem', padding: '0.15rem 0.5rem',
                    borderRadius: '999px', color: '#fff',
                    background: STATUS_COLORS[s.status] || '#6b7280',
                  }}>{STATUS_LABELS[s.status] || s.status}</span>
                </td>
                <td>{s.answeredCount}/{s.totalQuestions} ({s.correctCount} верных)</td>
                <td>{s.score !== null ? `${s.score}%` : '—'}</td>
                <td>{s.startedAt ? fmtDt(s.startedAt) : '—'}</td>
                <td>{s.completedAt ? fmtDt(s.completedAt) : '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );

  // ── CREATE VIEW ─────────────────────────────────────

  return (
    <div className="page-container" style={{ maxWidth: 960, margin: '0 auto', padding: '2rem 1rem' }}>
      <button className="btn btn-outline" onClick={() => setMode('list')} style={{ marginBottom: '1rem' }}>
        ← Назад
      </button>

      <h2 style={{ marginBottom: '1.5rem' }}>Создать задание</h2>

      {error && <div className="alert alert-danger" style={{ marginBottom: '1rem' }}>{error}</div>}

      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
        <div className="form-group" style={{ marginBottom: '1rem' }}>
          <label className="form-label">Название *</label>
          <input className="form-control" value={title} onChange={e => setTitle(e.target.value)}
            placeholder="Например: ДЗ по математике — тема Алгебра" maxLength={200} />
        </div>
        <div className="form-group" style={{ marginBottom: '1rem' }}>
          <label className="form-label">Описание</label>
          <textarea className="form-control" value={description} onChange={e => setDescription(e.target.value)}
            placeholder="Необязательное описание задания" rows={2} maxLength={2000} />
        </div>
        <div className="form-group">
          <label className="form-label">Дедлайн</label>
          <input type="datetime-local" className="form-control" value={deadline}
            onChange={e => setDeadline(e.target.value)} />
        </div>
      </div>

      {/* Questions selection */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
        <h4 style={{ margin: '0 0 1rem' }}>Вопросы ({selectedQuestionIds.length} выбрано)</h4>
        <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
          <input className="form-control" placeholder="Поиск вопросов…"
            value={qSearch} onChange={e => { setQSearch(e.target.value); loadQuestions(1, e.target.value); }} />
        </div>
        <div style={{ maxHeight: 300, overflow: 'auto' }}>
          {questions.map(q => (
            <label key={q.id} style={{
              display: 'flex', alignItems: 'center', gap: '0.75rem',
              padding: '0.5rem 0.75rem', cursor: 'pointer',
              borderBottom: '1px solid var(--border-color, #e5e7eb)',
              background: selectedQuestionIds.includes(q.id) ? 'var(--primary-light, #ede9fe)' : 'transparent',
            }}>
              <input type="checkbox" checked={selectedQuestionIds.includes(q.id)}
                onChange={() => toggleQuestion(q.id)} />
              <span style={{ flex: 1, fontSize: '0.9rem' }}>
                {q.text.length > 70 ? q.text.slice(0, 70) + '…' : q.text}
              </span>
              <span style={{ fontSize: '0.75rem', opacity: 0.6 }}>{q.topicName}</span>
            </label>
          ))}
        </div>
        {qTotal > 1 && (
          <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginTop: '0.75rem' }}>
            <button className="btn btn-sm btn-outline" disabled={qPage <= 1}
              onClick={() => loadQuestions(qPage - 1, qSearch)}>←</button>
            <span style={{ lineHeight: '2rem', fontSize: '0.85rem' }}>{qPage} / {qTotal}</span>
            <button className="btn btn-sm btn-outline" disabled={qPage >= qTotal}
              onClick={() => loadQuestions(qPage + 1, qSearch)}>→</button>
          </div>
        )}
      </div>

      {/* Students selection */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.5rem' }}>
        <h4 style={{ margin: '0 0 1rem' }}>
          Ученики ({selectedStudentIds.length} из {students.length})
          {students.length > 0 && (
            <button className="btn btn-sm btn-outline" style={{ marginLeft: '1rem', fontSize: '0.8rem' }}
              onClick={() => setSelectedStudentIds(
                selectedStudentIds.length === students.length ? [] : students.map(s => s.studentUserId)
              )}>
              {selectedStudentIds.length === students.length ? 'Снять все' : 'Выбрать всех'}
            </button>
          )}
        </h4>
        {students.length === 0 ? (
          <p style={{ opacity: 0.6, fontSize: '0.9rem' }}>Нет привязанных учеников. Сначала привяжите учеников через инвайт-код.</p>
        ) : students.map(s => (
          <label key={s.studentUserId} style={{
            display: 'flex', alignItems: 'center', gap: '0.75rem',
            padding: '0.5rem 0.75rem', cursor: 'pointer',
            borderBottom: '1px solid var(--border-color, #e5e7eb)',
            background: selectedStudentIds.includes(s.studentUserId) ? 'var(--primary-light, #ede9fe)' : 'transparent',
          }}>
            <input type="checkbox" checked={selectedStudentIds.includes(s.studentUserId)}
              onChange={() => toggleStudent(s.studentUserId)} />
            <span>{s.studentName}</span>
            <span style={{ fontSize: '0.75rem', opacity: 0.6 }}>{s.studentEmail}</span>
          </label>
        ))}
      </div>

      <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.75rem' }}>
        <button className="btn btn-outline" onClick={() => setMode('list')}>Отмена</button>
        <button className="btn btn-primary" onClick={handleCreate}>Создать задание</button>
      </div>
    </div>
  );
}
