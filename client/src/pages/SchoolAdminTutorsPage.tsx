import { useState, useEffect, useCallback } from 'react';
import { schoolAdminService } from '../services/schoolAdminService';
import { useTranslation } from '../i18n';
import api from '../services/api';
import type { SchoolTutor, TutorContentAssignment, TutorContentQuestion } from '../services/schoolAdminService';

function SchoolAdminTutorsPage() {
  const { t } = useTranslation();
  const [tab, setTab] = useState<'tutors' | 'content'>('tutors');
  const [tutors, setTutors] = useState<SchoolTutor[]>([]);
  const [loading, setLoading] = useState(true);

  // Content state
  const [assignments, setAssignments] = useState<TutorContentAssignment[]>([]);
  const [questions, setQuestions] = useState<TutorContentQuestion[]>([]);
  const [contentLoading, setContentLoading] = useState(false);
  const [filterTutor, setFilterTutor] = useState<number | undefined>();

  const loadTutors = useCallback(async () => {
    setLoading(true);
    try {
      setTutors(await schoolAdminService.getTutors());
    } catch { /* */ } finally { setLoading(false); }
  }, []);

  const loadContent = useCallback(async () => {
    setContentLoading(true);
    try {
      const data = await schoolAdminService.getTutorContent({ tutorId: filterTutor, pageSize: 50 });
      setAssignments(data.assignments.items);
      setQuestions(data.questions.items);
    } catch { /* */ } finally { setContentLoading(false); }
  }, [filterTutor]);

  useEffect(() => { loadTutors(); }, [loadTutors]);
  useEffect(() => { if (tab === 'content') loadContent(); }, [tab, loadContent]);

  const tabStyle = (active: boolean): React.CSSProperties => ({
    padding: '0.5rem 1.25rem', cursor: 'pointer', fontSize: '0.9rem', fontWeight: 600,
    borderBottom: active ? '2px solid var(--primary-color)' : '2px solid transparent',
    color: active ? 'var(--primary-color)' : 'var(--text-secondary)',
    background: 'none', border: 'none', borderBottomWidth: '2px', borderBottomStyle: 'solid',
  });

  return (
    <div>
      <h1 style={{ marginBottom: '1rem' }}>{t.schoolAdmin.tutors}</h1>

      <div style={{ display: 'flex', gap: '0.25rem', borderBottom: '1px solid var(--border-color)', marginBottom: '1.5rem' }}>
        <button style={tabStyle(tab === 'tutors')} onClick={() => setTab('tutors')}>
          {t.schoolAdmin.tutors}
        </button>
        <button style={tabStyle(tab === 'content')} onClick={() => setTab('content')}>
          Контент
        </button>
      </div>

      {tab === 'tutors' && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1rem' }}>
          {tutors.map(tr => (
            <div key={tr.userId} className="card" style={{ padding: '1.25rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                <div>
                  <strong>{tr.name}</strong>
                  {tr.isVerified && <span style={{ color: 'var(--success-color)', marginLeft: '0.5rem' }}>&#10003;</span>}
                </div>
                <span style={{ fontSize: '0.8rem', color: tr.isAvailable ? 'var(--success-color)' : 'var(--text-secondary)' }}>
                  {tr.isAvailable ? t.schoolAdmin.available : t.schoolAdmin.unavailable}
                </span>
              </div>
              <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', margin: '0.5rem 0' }}>{tr.headline}</div>
              <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{tr.email}</div>
              <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.75rem', fontSize: '0.8rem' }}>
                <span>&#9733; {tr.averageRating.toFixed(1)}</span>
                <span>{tr.totalStudents} {t.schoolAdmin.students.toLowerCase()}</span>
                <span>{tr.specializations.join(', ')}</span>
              </div>
              <div style={{ marginTop: '0.75rem' }}>
                {tr.isVerified ? (
                  <button className="btn btn-outline" style={{ fontSize: '0.75rem', padding: '0.25rem 0.7rem', color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                    onClick={async () => {
                      try {
                        await api.post(`/tutor-school-applications/unverify/${tr.userId}`);
                        loadTutors();
                      } catch { /* */ }
                    }}>{t.schoolAdmin.unverify}</button>
                ) : (
                  <button className="btn btn-primary" style={{ fontSize: '0.75rem', padding: '0.25rem 0.7rem' }}
                    onClick={async () => {
                      try {
                        await api.post(`/tutor-school-applications/verify/${tr.userId}`);
                        loadTutors();
                      } catch { /* */ }
                    }}>{t.schoolAdmin.verify}</button>
                )}
              </div>
            </div>
          ))}
          {tutors.length === 0 && !loading && (
            <div className="card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
              {t.schoolAdmin.noTutors}
            </div>
          )}
        </div>
      )}

      {tab === 'content' && (
        <div>
          {/* Tutor filter */}
          {tutors.length > 0 && (
            <div style={{ marginBottom: '1rem' }}>
              <select className="form-input" style={{ maxWidth: '300px' }} value={filterTutor ?? ''} onChange={e => setFilterTutor(e.target.value ? Number(e.target.value) : undefined)}>
                <option value="">Все тьюторы</option>
                {tutors.map(tr => <option key={tr.userId} value={tr.userId}>{tr.name}</option>)}
              </select>
            </div>
          )}

          {contentLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
              {/* Assignments section */}
              <div>
                <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Задания ({assignments.length})</h3>
                {assignments.length === 0 ? (
                  <div className="card" style={{ padding: '1.5rem', textAlign: 'center', color: 'var(--text-secondary)' }}>Заданий пока нет</div>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                    {assignments.map(a => (
                      <div key={a.id} className="card" style={{ padding: '1rem' }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                          <div>
                            <strong>{a.title}</strong>
                            <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginLeft: '0.5rem' }}>
                              {a.tutorName}
                            </span>
                          </div>
                          <span style={{ fontSize: '0.72rem', color: a.isActive ? 'var(--success-color)' : 'var(--text-secondary)' }}>
                            {a.isActive ? 'Активно' : 'Неактивно'}
                          </span>
                        </div>
                        {a.description && <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '0.35rem' }}>{a.description}</div>}
                        <div style={{ display: 'flex', gap: '1rem', marginTop: '0.5rem', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                          <span>Вопросов: {a.questionCount}</span>
                          <span>Учеников: {a.studentCount}</span>
                          <span>Выполнили: {a.completedCount}</span>
                          {a.deadline && <span>Дедлайн: {new Date(a.deadline).toLocaleDateString()}</span>}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              {/* Questions section */}
              <div>
                <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Вопросы ({questions.length})</h3>
                {questions.length === 0 ? (
                  <div className="card" style={{ padding: '1.5rem', textAlign: 'center', color: 'var(--text-secondary)' }}>Вопросов пока нет</div>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                    {questions.map(q => (
                      <div key={q.id} className="card" style={{ padding: '1rem' }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                          <div style={{ flex: 1, minWidth: 0 }}>
                            <div style={{ fontWeight: 600, fontSize: '0.9rem' }}>{q.text.slice(0, 120)}{q.text.length > 120 ? '...' : ''}</div>
                            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                              {q.tutorName} · {q.topicName} · {q.examTypeCode} · {q.difficulty}
                            </div>
                          </div>
                          {q.isPrivate && (
                            <span style={{ fontSize: '0.7rem', padding: '0.15rem 0.4rem', borderRadius: '4px', background: 'var(--bg-secondary)', color: 'var(--text-secondary)', whiteSpace: 'nowrap' }}>
                              Приватный
                            </span>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default SchoolAdminTutorsPage;
