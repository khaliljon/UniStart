import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  LineChart,
  Line,
} from 'recharts';
import { studyPlanService } from '../services/studyPlanService';
import { examService } from '../services/examService';
import type {
  StudyGoal,
  StudyPlan,
  TodayPlan,
  PlanStats,
  StudyPlanEntry,
  ExamType,
} from '../types';

type Tab = 'today' | 'plan' | 'stats';

const TYPE_LABELS: Record<string, string> = {
  New: 'Новая тема',
  Review: 'Повторение',
  Practice: 'Практика',
  Weakness: 'Слабая тема',
};

const TYPE_COLORS: Record<string, string> = {
  New: '#6366f1',
  Review: '#10b981',
  Practice: '#f59e0b',
  Weakness: '#ef4444',
};

function StudyPlanPage() {
  const navigate = useNavigate();
  const [tab, setTab] = useState<Tab>('today');
  const [goal, setGoal] = useState<StudyGoal | null>(null);
  const [todayPlan, setTodayPlan] = useState<TodayPlan | null>(null);
  const [plan, setPlan] = useState<StudyPlan | null>(null);
  const [stats, setStats] = useState<PlanStats | null>(null);
  const [exams, setExams] = useState<ExamType[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Goal form state
  const [showGoalForm, setShowGoalForm] = useState(false);
  const [formExam, setFormExam] = useState('');
  const [formDate, setFormDate] = useState('');
  const [formScore, setFormScore] = useState(80);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Delete confirmation
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const loadData = useCallback(async () => {
    try {
      setIsLoading(true);

      // Auto-complete today's entries from actual answers first
      try {
        const autoCompletedToday = await studyPlanService.autoCompleteToday();
        setTodayPlan(autoCompletedToday);
      } catch { /* ignore — will load normally */ }

      const [goalData, todayData, planData, examData] = await Promise.all([
        studyPlanService.getActiveGoal(),
        studyPlanService.getTodayPlan(),
        studyPlanService.getActivePlan(),
        examService.getExams(),
      ]);
      setGoal(goalData);
      setTodayPlan(todayData);
      setPlan(planData);
      setExams(examData);

      if (planData) {
        const statsData = await studyPlanService.getPlanStats();
        setStats(statsData);
      }
    } catch (err) {
      setError('Ошибка загрузки данных');
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // ─── Handlers ──────────────────────────────────────────

  const handleCreateGoal = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formExam || !formDate) return;

    setIsSubmitting(true);
    try {
      const newGoal = await studyPlanService.createGoal({
        examTypeCode: formExam,
        targetDate: formDate,
        targetScore: formScore,
      });
      setGoal(newGoal);
      setShowGoalForm(false);

      // Generate plan immediately
      const newPlan = await studyPlanService.generatePlan(newGoal.id);
      setPlan(newPlan);

      // Reload today
      const todayData = await studyPlanService.getTodayPlan();
      setTodayPlan(todayData);

      const statsData = await studyPlanService.getPlanStats();
      setStats(statsData);
    } catch (err) {
      setError('Ошибка создания цели');
      console.error(err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRegenerate = async () => {
    try {
      setIsSubmitting(true);
      const newPlan = await studyPlanService.regeneratePlan();
      setPlan(newPlan);
      const todayData = await studyPlanService.getTodayPlan();
      setTodayPlan(todayData);
      const statsData = await studyPlanService.getPlanStats();
      setStats(statsData);
    } catch (err) {
      setError('Ошибка перегенерации плана');
      console.error(err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleStartEntry = (entry: StudyPlanEntry) => {
    // Navigate to practice filtered by this entry's topic
    navigate(`/learn?tab=practice&topicId=${entry.topicId}&planEntryId=${entry.id}`);
  };

  const handleDeleteGoal = async () => {
    if (!goal) return;
    try {
      await studyPlanService.deleteGoal(goal.id);
      setGoal(null);
      setPlan(null);
      setTodayPlan(null);
      setStats(null);
      setShowDeleteConfirm(false);
    } catch (err) {
      console.error(err);
    }
  };

  // ─── Render ────────────────────────────────────────────

  if (isLoading) {
    return (
      <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
        <div className="card" style={{ padding: '3rem', textAlign: 'center' }}>
          <div className="animate-pulse" style={{ fontSize: '1.2rem', color: 'var(--text-secondary)' }}>
            Загрузка плана обучения...
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
        <div className="card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--error-color)' }}>
          {error}
          <button className="btn btn-primary" style={{ marginTop: '1rem' }} onClick={loadData}>
            Попробовать снова
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ padding: '1.5rem 0' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>📚 Учебный план</h1>
        {goal && (
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button className="btn btn-outline" onClick={handleRegenerate} disabled={isSubmitting}>
              🔄 Перегенерировать
            </button>
            <button className="btn btn-outline" onClick={() => setShowGoalForm(true)}>
              ✏️ Изменить цель
            </button>
          </div>
        )}
      </div>

      {/* ─── Goal Card ─── */}
      {goal ? (
        <GoalCard goal={goal} onDelete={() => setShowDeleteConfirm(true)} />
      ) : (
        <div className="card animate-fade-in-up" style={{ padding: '2rem', textAlign: 'center' }}>
          <h2 style={{ marginBottom: '0.5rem' }}>🎯 Установите цель обучения</h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
            Выберите экзамен, дедлайн и целевой балл — мы составим персональный план
          </p>
          <button className="btn btn-primary" onClick={() => setShowGoalForm(true)}>
            Создать цель
          </button>
        </div>
      )}

      {/* ─── Goal Form Modal ─── */}
      {showGoalForm && (
        <GoalFormModal
          exams={exams}
          formExam={formExam}
          formDate={formDate}
          formScore={formScore}
          isSubmitting={isSubmitting}
          onChangeExam={setFormExam}
          onChangeDate={setFormDate}
          onChangeScore={setFormScore}
          onSubmit={handleCreateGoal}
          onClose={() => setShowGoalForm(false)}
        />
      )}

      {/* ─── Tabs ─── */}
      {goal && (
        <>
          <div style={{
            display: 'flex', gap: '0.5rem', marginTop: '1.5rem', marginBottom: '1rem',
            borderBottom: '2px solid var(--border-color)', paddingBottom: '0.5rem'
          }}>
            {([['today', '📅 Сегодня'], ['plan', '📋 Весь план'], ['stats', '📊 Статистика']] as [Tab, string][]).map(
              ([key, label]) => (
                <button
                  key={key}
                  onClick={() => setTab(key)}
                  className={`btn ${tab === key ? 'btn-primary' : 'btn-outline'}`}
                  style={{ fontSize: '0.9rem' }}
                >
                  {label}
                </button>
              )
            )}
          </div>

          {tab === 'today' && todayPlan && (
            <TodayTab todayPlan={todayPlan} onStart={handleStartEntry} />
          )}

          {tab === 'plan' && plan && (
            <PlanTab plan={plan} />
          )}

          {tab === 'stats' && stats && (
            <StatsTab stats={stats} />
          )}
        </>
      )}

      {/* ─── Delete Confirmation Dialog ─── */}
      {showDeleteConfirm && (
        <div style={{
          position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex',
          alignItems: 'center', justifyContent: 'center', zIndex: 1000
        }} onClick={() => setShowDeleteConfirm(false)}>
          <div className="card animate-fade-in-scale" style={{ padding: '2rem', maxWidth: '400px', width: '90%' }}
            onClick={(e) => e.stopPropagation()}>
            <h2 style={{ margin: '0 0 0.5rem' }}>⚠️ Удалить цель?</h2>
            <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem', lineHeight: 1.5 }}>
              Это действие удалит текущую цель и весь учебный план безвозвратно.
              Ваш прогресс по ответам сохранится, но план нужно будет создать заново.
            </p>
            <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
              <button className="btn btn-outline" onClick={() => setShowDeleteConfirm(false)}>
                Отмена
              </button>
              <button
                className="btn"
                style={{ background: 'var(--error-color)', color: '#fff' }}
                onClick={handleDeleteGoal}
              >
                Удалить цель
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════
//  SUB COMPONENTS
// ═══════════════════════════════════════════════════════════

function GoalCard({ goal, onDelete }: { goal: StudyGoal; onDelete: () => void }) {
  const [showMenu, setShowMenu] = useState(false);

  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.25rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <div style={{ flex: 1 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.5rem' }}>
            <h2 style={{ margin: 0 }}>🎯 {goal.examTypeName}</h2>
            <span style={{
              background: 'var(--primary-color)', color: '#fff',
              padding: '0.2rem 0.6rem', borderRadius: '12px', fontSize: '0.8rem'
            }}>
              {goal.examTypeCode}
            </span>
          </div>
          <div style={{
            display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))',
            gap: '1rem', marginTop: '0.75rem'
          }}>
            <StatBox label="Дней до экзамена" value={goal.daysUntilExam} color={
              goal.daysUntilExam < 14 ? 'var(--error-color)' :
              goal.daysUntilExam < 30 ? 'var(--warning-color)' :
              'var(--success-color)'
            } />
            <StatBox label="Целевой балл" value={goal.targetScore} />
            <StatBox label="Рек. часов / день" value={goal.recommendedHoursPerDay} />
            <StatBox label="Дедлайн" value={new Date(goal.targetDate).toLocaleDateString('ru-RU')} />
          </div>
        </div>
        {/* Three-dot menu instead of dangerous X */}
        <div style={{ position: 'relative' }}>
          <button
            onClick={() => setShowMenu(!showMenu)}
            className="btn btn-outline"
            style={{ fontSize: '1rem', padding: '0.3rem 0.6rem', lineHeight: 1 }}
          >
            ⋯
          </button>
          {showMenu && (
            <>
              <div style={{ position: 'fixed', inset: 0, zIndex: 99 }} onClick={() => setShowMenu(false)} />
              <div style={{
                position: 'absolute', right: 0, top: '100%', marginTop: '0.25rem',
                background: 'var(--card-bg)', border: '1px solid var(--border-color)',
                borderRadius: '8px', boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
                zIndex: 100, minWidth: '180px', overflow: 'hidden'
              }}>
                <button
                  onClick={() => { setShowMenu(false); onDelete(); }}
                  style={{
                    width: '100%', padding: '0.6rem 1rem', background: 'none',
                    border: 'none', cursor: 'pointer', textAlign: 'left',
                    color: 'var(--error-color)', fontSize: '0.85rem',
                    display: 'flex', alignItems: 'center', gap: '0.5rem'
                  }}
                  onMouseEnter={(e) => (e.currentTarget.style.background = 'var(--primary-bg)')}
                  onMouseLeave={(e) => (e.currentTarget.style.background = 'none')}
                >
                  🗑️ Удалить цель
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

function StatBox({ label, value, color }: { label: string; value: string | number; color?: string }) {
  return (
    <div>
      <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '0.2rem' }}>{label}</div>
      <div style={{ fontSize: '1.4rem', fontWeight: 700, color: color || 'var(--text-primary)' }}>{value}</div>
    </div>
  );
}

// ─── Today Tab ────────────────────────────────────────────

function TodayTab({ todayPlan, onStart }: { todayPlan: TodayPlan; onStart: (entry: StudyPlanEntry) => void }) {
  const completedCount = todayPlan.entries.filter(e => e.isCompleted).length;
  const totalCount = todayPlan.entries.length;
  const progress = totalCount > 0 ? (completedCount / totalCount) * 100 : 0;

  return (
    <div className="animate-fade-in-up">
      {/* Recommendation */}
      <div className="card card-static" style={{ padding: '1rem', marginBottom: '1rem', background: 'var(--primary-bg)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <span style={{ fontSize: '1.5rem' }}>💡</span>
          <div>
            <div style={{ fontWeight: 600, marginBottom: '0.2rem' }}>{todayPlan.recommendation}</div>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
              ~{todayPlan.totalMinutesToday} мин осталось • {completedCount}/{totalCount} заданий выполнено
            </div>
          </div>
        </div>
        {totalCount > 0 && (
          <div style={{ marginTop: '0.75rem', background: 'var(--border-color)', borderRadius: '8px', height: '8px', overflow: 'hidden' }}>
            <div style={{
              width: `${progress}%`, height: '100%',
              background: progress === 100 ? 'var(--success-color)' : 'var(--primary-color)',
              borderRadius: '8px', transition: 'width 0.5s ease'
            }} />
          </div>
        )}
      </div>

      {/* Entries */}
      {todayPlan.entries.length === 0 ? (
        <div className="card card-static" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
          На сегодня заданий нет
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
          {todayPlan.entries.map((entry) => (
            <EntryCard key={entry.id} entry={entry} onStart={() => onStart(entry)} />
          ))}
        </div>
      )}
    </div>
  );
}

function EntryCard({ entry, onStart }: { entry: StudyPlanEntry; onStart: () => void }) {
  const typeColor = TYPE_COLORS[entry.type] || '#6366f1';
  const accuracy = entry.questionsAnswered > 0
    ? Math.round((entry.correctAnswers / entry.questionsAnswered) * 100)
    : 0;

  return (
    <div className="card card-static" style={{
      padding: '1rem',
      opacity: entry.isCompleted ? 0.85 : 1,
      borderLeft: `4px solid ${typeColor}`,
    }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div style={{ flex: 1 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.25rem' }}>
            <span style={{
              background: typeColor, color: '#fff', padding: '0.15rem 0.5rem',
              borderRadius: '8px', fontSize: '0.7rem', fontWeight: 600
            }}>
              {TYPE_LABELS[entry.type] || entry.type}
            </span>
            {entry.isCompleted && <span style={{ color: 'var(--success-color)', fontWeight: 600 }}>✓ Выполнено</span>}
          </div>
          <div style={{ fontWeight: 600, fontSize: '1rem' }}>{entry.topicName}</div>
          <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
            {entry.recommendedMinutes} мин • {entry.recommendedQuestions} вопросов
            {entry.isCompleted && entry.questionsAnswered > 0 && (
              <> • Результат: <span style={{
                color: accuracy >= 70 ? 'var(--success-color)' : accuracy >= 40 ? 'var(--warning-color)' : 'var(--error-color)',
                fontWeight: 600
              }}>{entry.correctAnswers}/{entry.questionsAnswered} ({accuracy}%)</span></>
            )}
          </div>
        </div>
        {!entry.isCompleted ? (
          <button className="btn btn-primary" style={{ fontSize: '0.85rem', whiteSpace: 'nowrap' }} onClick={onStart}>
            ▶ Начать
          </button>
        ) : (
          <button className="btn btn-outline" style={{ fontSize: '0.8rem', whiteSpace: 'nowrap' }} onClick={onStart}>
            🔄 Ещё
          </button>
        )}
      </div>
    </div>
  );
}

// ─── Plan Tab ─────────────────────────────────────────────

function PlanTab({ plan }: { plan: StudyPlan }) {
  // Group entries by date
  const groupedEntries = new Map<string, StudyPlanEntry[]>();
  for (const entry of plan.entries) {
    const dateKey = entry.date.split('T')[0];
    if (!groupedEntries.has(dateKey)) groupedEntries.set(dateKey, []);
    groupedEntries.get(dateKey)!.push(entry);
  }

  const today = new Date().toISOString().split('T')[0];
  const sortedDates = Array.from(groupedEntries.keys()).sort();

  return (
    <div className="animate-fade-in-up">
      {/* Plan overview */}
      <div className="card card-static" style={{ padding: '1rem', marginBottom: '1rem' }}>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(120px, 1fr))', gap: '1rem' }}>
          <StatBox label="Всего заданий" value={plan.totalEntries} />
          <StatBox label="Выполнено" value={plan.completedEntries} color="var(--success-color)" />
          <StatBox label="Прогресс" value={`${plan.completionPercent}%`} color="var(--primary-color)" />
          <StatBox label="Дней в плане" value={sortedDates.length} />
        </div>
        <div style={{ marginTop: '0.75rem', background: 'var(--border-color)', borderRadius: '8px', height: '8px', overflow: 'hidden' }}>
          <div style={{
            width: `${plan.completionPercent}%`, height: '100%',
            background: 'var(--primary-color)', borderRadius: '8px', transition: 'width 0.5s ease'
          }} />
        </div>
      </div>

      {/* Calendar view */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
        {sortedDates.map((dateStr) => {
          const entries = groupedEntries.get(dateStr) || [];
          const isToday = dateStr === today;
          const isPast = dateStr < today;
          const allCompleted = entries.every(e => e.isCompleted);
          const noneCompleted = entries.every(e => !e.isCompleted);

          return (
            <div key={dateStr} className="card card-static" style={{
              padding: '1rem',
              borderLeft: isToday ? '4px solid var(--primary-color)' :
                          isPast && allCompleted ? '4px solid var(--success-color)' :
                          isPast && noneCompleted ? '4px solid var(--error-color)' :
                          '4px solid var(--border-color)',
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
                <div style={{ fontWeight: 600 }}>
                  {isToday && '📅 '}
                  {new Date(dateStr).toLocaleDateString('ru-RU', { weekday: 'short', day: 'numeric', month: 'short' })}
                  {isToday && ' (Сегодня)'}
                </div>
                <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                  {entries.filter(e => e.isCompleted).length}/{entries.length} • 
                  {entries.reduce((s, e) => s + e.recommendedMinutes, 0)} мин
                </div>
              </div>
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem' }}>
                {entries.map((entry) => (
                  <span key={entry.id} style={{
                    background: entry.isCompleted ? 'var(--success-color)' : TYPE_COLORS[entry.type] || '#6366f1',
                    color: '#fff', padding: '0.2rem 0.5rem', borderRadius: '6px',
                    fontSize: '0.72rem', fontWeight: 500,
                    opacity: entry.isCompleted ? 0.7 : 1,
                    textDecoration: entry.isCompleted ? 'line-through' : 'none'
                  }}>
                    {entry.topicName}
                  </span>
                ))}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

// ─── Stats Tab ────────────────────────────────────────────

function StatsTab({ stats }: { stats: PlanStats }) {
  const weeklyChartData = stats.weeklySummary.map(w => ({
    week: new Date(w.weekStart).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' }),
    planned: w.plannedEntries,
    completed: w.completedEntries,
    accuracy: w.accuracy,
  }));

  return (
    <div className="animate-fade-in-up">
      {/* Summary stats */}
      <div className="card card-static" style={{ padding: '1.25rem', marginBottom: '1rem' }}>
        <h3 style={{ margin: '0 0 1rem' }}>📊 Общая статистика</h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(130px, 1fr))', gap: '1rem' }}>
          <StatBox label="Дней активности" value={stats.completedDays} color="var(--success-color)" />
          <StatBox label="Пропущено дней" value={stats.skippedDays} color="var(--error-color)" />
          <StatBox label="Вопросов отвечено" value={stats.totalQuestionsAnswered} />
          <StatBox
            label="Точность"
            value={`${stats.averageAccuracy}%`}
            color={stats.averageAccuracy >= 70 ? 'var(--success-color)' : 'var(--warning-color)'}
          />
          <StatBox
            label="Приверженность"
            value={`${stats.adherencePercent}%`}
            color={stats.adherencePercent >= 70 ? 'var(--success-color)' : 'var(--warning-color)'}
          />
        </div>
      </div>

      {/* Weekly completion chart */}
      {weeklyChartData.length > 0 && (
        <div className="card card-static" style={{ padding: '1.25rem', marginBottom: '1rem' }}>
          <h3 style={{ margin: '0 0 1rem' }}>📈 Еженедельный прогресс</h3>
          <ResponsiveContainer width="100%" height={260}>
            <BarChart data={weeklyChartData}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" />
              <XAxis dataKey="week" tick={{ fill: 'var(--text-secondary)', fontSize: 12 }} />
              <YAxis tick={{ fill: 'var(--text-secondary)', fontSize: 12 }} />
              <Tooltip
                contentStyle={{
                  background: 'var(--card-bg)', border: '1px solid var(--border-color)',
                  borderRadius: '8px', color: 'var(--text-primary)'
                }}
              />
              <Bar dataKey="planned" name="Запланировано" fill="#6366f1" radius={[4, 4, 0, 0]} />
              <Bar dataKey="completed" name="Выполнено" fill="#10b981" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Accuracy trend */}
      {weeklyChartData.length > 1 && (
        <div className="card card-static" style={{ padding: '1.25rem' }}>
          <h3 style={{ margin: '0 0 1rem' }}>🎯 Динамика точности</h3>
          <ResponsiveContainer width="100%" height={220}>
            <LineChart data={weeklyChartData}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" />
              <XAxis dataKey="week" tick={{ fill: 'var(--text-secondary)', fontSize: 12 }} />
              <YAxis domain={[0, 100]} tick={{ fill: 'var(--text-secondary)', fontSize: 12 }} />
              <Tooltip
                contentStyle={{
                  background: 'var(--card-bg)', border: '1px solid var(--border-color)',
                  borderRadius: '8px', color: 'var(--text-primary)'
                }}
              />
              <Line type="monotone" dataKey="accuracy" name="Точность %" stroke="#f59e0b" strokeWidth={2} dot={{ r: 4 }} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  );
}

// ─── Modals ───────────────────────────────────────────────

function GoalFormModal({
  exams, formExam, formDate, formScore, isSubmitting,
  onChangeExam, onChangeDate, onChangeScore, onSubmit, onClose,
}: {
  exams: ExamType[];
  formExam: string; formDate: string; formScore: number; isSubmitting: boolean;
  onChangeExam: (v: string) => void; onChangeDate: (v: string) => void;
  onChangeScore: (v: number) => void; onSubmit: (e: React.FormEvent) => void;
  onClose: () => void;
}) {
  const minDate = new Date(Date.now() + 7 * 86400000).toISOString().split('T')[0];

  return (
    <div style={{
      position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex',
      alignItems: 'center', justifyContent: 'center', zIndex: 1000
    }} onClick={onClose}>
      <div className="card animate-fade-in-scale" style={{ padding: '2rem', maxWidth: '450px', width: '90%' }}
        onClick={(e) => e.stopPropagation()}>
        <h2 style={{ margin: '0 0 1.5rem' }}>🎯 Установить цель</h2>
        <form onSubmit={onSubmit}>
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.4rem', fontWeight: 600, fontSize: '0.9rem' }}>
              Экзамен
            </label>
            <select
              value={formExam}
              onChange={(e) => onChangeExam(e.target.value)}
              required
              style={{
                width: '100%', padding: '0.6rem', borderRadius: '8px',
                border: '1px solid var(--border-color)', background: 'var(--card-bg)',
                color: 'var(--text-primary)', fontSize: '0.95rem'
              }}
            >
              <option value="">Выберите экзамен...</option>
              {exams.map((e) => (
                <option key={e.code} value={e.code}>{e.name}</option>
              ))}
            </select>
          </div>

          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.4rem', fontWeight: 600, fontSize: '0.9rem' }}>
              Дата экзамена
            </label>
            <input
              type="date"
              value={formDate}
              onChange={(e) => onChangeDate(e.target.value)}
              min={minDate}
              required
              style={{
                width: '100%', padding: '0.6rem', borderRadius: '8px',
                border: '1px solid var(--border-color)', background: 'var(--card-bg)',
                color: 'var(--text-primary)', fontSize: '0.95rem'
              }}
            />
          </div>

          <div style={{ marginBottom: '1.5rem' }}>
            <label style={{ display: 'block', marginBottom: '0.4rem', fontWeight: 600, fontSize: '0.9rem' }}>
              Целевой балл: {formScore}
            </label>
            <input
              type="range"
              min="30"
              max="100"
              value={formScore}
              onChange={(e) => onChangeScore(Number(e.target.value))}
              style={{ width: '100%' }}
            />
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
              <span>30</span>
              <span>100</span>
            </div>
          </div>

          <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
            <button type="button" className="btn btn-outline" onClick={onClose}>Отмена</button>
            <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
              {isSubmitting ? 'Создание...' : 'Создать и сгенерировать план'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default StudyPlanPage;
