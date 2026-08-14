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
import { useTranslation } from '../hooks/useTranslation';
import { useAppSelector } from '../hooks/useAppSelector';
import { studyPlanService } from '../services/studyPlanService';
import { examService } from '../services/examService';
import { subscriptionService } from '../services/subscriptionService';
import { ProGate } from '../components/ProGate';
import type {
  StudyGoal,
  StudyPlan,
  TodayPlan,
  PlanStats,
  StudyPlanEntry,
  ExamType,
  ExamSection,
} from '../types';

type Tab = 'today' | 'plan' | 'stats';

const TYPE_COLORS: Record<string, string> = {
  New: '#6366f1',
  Review: '#10b981',
  Practice: '#f59e0b',
  Weakness: '#ef4444',
};

function StudyPlanPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { selectedSectionIds: profileSectionIds } = useAppSelector((state) => state.exam);
  const [tab, setTab] = useState<Tab>('today');
  const [goal, setGoal] = useState<StudyGoal | null>(null);
  const [todayPlan, setTodayPlan] = useState<TodayPlan | null>(null);
  const [plan, setPlan] = useState<StudyPlan | null>(null);
  const [stats, setStats] = useState<PlanStats | null>(null);
  const [exams, setExams] = useState<ExamType[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [showGoalForm, setShowGoalForm] = useState(false);
  const [formExam, setFormExam] = useState('');
  const [formDate, setFormDate] = useState('');
  const [formScore, setFormScore] = useState(80);
  const [formHoursPerDay, setFormHoursPerDay] = useState<number | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formSections, setFormSections] = useState<ExamSection[]>([]);
  const [selectedSectionIds, setSelectedSectionIds] = useState<number[]>([]);

  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [hasFullStudyPlan, setHasFullStudyPlan] = useState(true);

  const loadData = useCallback(async () => {
    try {
      setIsLoading(true);

      try {
        const autoCompletedToday = await studyPlanService.autoCompleteToday();
        setTodayPlan(autoCompletedToday);
      } catch {}

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
      setError(t.studyPlan.loadError);
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
    subscriptionService.getStatus().then((s) => {
      setHasFullStudyPlan(s.isPro || s.limits.fullStudyPlan);
    }).catch(() => {});
  }, [loadData]);

  useEffect(() => {
    if (!formExam) { setFormSections([]); setSelectedSectionIds([]); return; }
    let cancelled = false;
    examService.getExamSections(formExam).then((sections) => {
      if (!cancelled) {
        setFormSections(sections);
        setSelectedSectionIds(sections.map((s) => s.id));
      }
    }).catch(() => { if (!cancelled) setFormSections([]); });
    return () => { cancelled = true; };
  }, [formExam]);


  const handleCreateGoal = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formExam || !formDate) return;

    setIsSubmitting(true);
    try {
      const newGoal = await studyPlanService.createGoal({
        examTypeCode: formExam,
        targetDate: formDate,
        targetScore: formScore,
        sectionIds: selectedSectionIds.length > 0 && selectedSectionIds.length < formSections.length
          ? selectedSectionIds
          : undefined,
        hoursPerDay: formHoursPerDay ?? undefined,
      });
      setGoal(newGoal);
      setShowGoalForm(false);

      const newPlan = await studyPlanService.generatePlan(newGoal.id);
      setPlan(newPlan);

      const todayData = await studyPlanService.getTodayPlan();
      setTodayPlan(todayData);

      const statsData = await studyPlanService.getPlanStats();
      setStats(statsData);
    } catch (err) {
      setError(t.studyPlan.createError);
      console.error(err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleStartEntry = (entry: StudyPlanEntry) => {
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


  if (isLoading) {
    return (
      <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
        <div className="card" style={{ padding: '3rem', textAlign: 'center' }}>
          <div className="animate-pulse" style={{ fontSize: '1.2rem', color: 'var(--text-secondary)' }}>
            {t.studyPlan.loading}
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
            {t.studyPlan.tryAgain}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ padding: '1.5rem 0' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>{t.studyPlan.title}</h1>
        {goal && (
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button className="btn btn-outline" onClick={() => setShowGoalForm(true)}>
              {t.studyPlan.changeGoal}
            </button>
          </div>
        )}
      </div>

      {goal ? (
        <GoalCard goal={goal} onDelete={() => setShowDeleteConfirm(true)} />
      ) : (
        <div className="card animate-fade-in-up" style={{ padding: '2rem', textAlign: 'center' }}>
          <h2 style={{ marginBottom: '0.5rem' }}>{t.studyPlan.setGoal}</h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
            {t.studyPlan.setGoalDesc}
          </p>
          <button className="btn btn-primary" onClick={() => setShowGoalForm(true)}>
            {t.studyPlan.createGoal}
          </button>
        </div>
      )}

      {showGoalForm && (
        <GoalFormModal
          exams={exams}
          formExam={formExam}
          formDate={formDate}
          formScore={formScore}
          formHoursPerDay={formHoursPerDay}
          isSubmitting={isSubmitting}
          sections={formSections}
          selectedSectionIds={selectedSectionIds}
          onChangeExam={setFormExam}
          onChangeDate={setFormDate}
          onChangeScore={setFormScore}
          onChangeHoursPerDay={setFormHoursPerDay}
          onChangeSections={setSelectedSectionIds}
          onSubmit={handleCreateGoal}
          onClose={() => setShowGoalForm(false)}
        />
      )}

      {goal && (
        <>
          <div style={{
            display: 'flex', gap: '0.5rem', marginTop: '1.5rem', marginBottom: '1rem',
            borderBottom: '2px solid var(--border-color)', paddingBottom: '0.5rem'
          }}>
            {([['today', t.studyPlan.tabToday], ['plan', t.studyPlan.tabPlan], ['stats', t.studyPlan.tabStats]] as [Tab, string][]).map(
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
            <TodayTab
              todayPlan={profileSectionIds.length > 0 ? {
                ...todayPlan,
                entries: todayPlan.entries.filter(e => e.sectionId == null || profileSectionIds.includes(e.sectionId)),
                totalMinutesToday: todayPlan.entries
                  .filter(e => !e.isCompleted && (e.sectionId == null || profileSectionIds.includes(e.sectionId)))
                  .reduce((s, e) => s + e.recommendedMinutes, 0),
              } : todayPlan}
              onStart={handleStartEntry}
            />
          )}

          {tab === 'plan' && plan && (
            <ProGate hasAccess={hasFullStudyPlan} featureName={t.studyPlan.tabPlan}>
              <PlanTab plan={profileSectionIds.length > 0 ? {
                ...plan,
                entries: plan.entries.filter(e => e.sectionId == null || profileSectionIds.includes(e.sectionId)),
              } : plan} />
            </ProGate>
          )}

          {tab === 'stats' && stats && (
            <ProGate hasAccess={hasFullStudyPlan} featureName={t.studyPlan.tabStats}>
              <StatsTab stats={stats} />
            </ProGate>
          )}
        </>
      )}

      {showDeleteConfirm && (
        <div style={{
          position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex',
          alignItems: 'center', justifyContent: 'center', zIndex: 1000
        }} onClick={() => setShowDeleteConfirm(false)}>
          <div className="card animate-fade-in-scale" style={{ padding: '2rem', maxWidth: '400px', width: '90%' }}
            onClick={(e) => e.stopPropagation()}>
            <h2 style={{ margin: '0 0 0.5rem' }}>{t.studyPlan.deleteGoalTitle}</h2>
            <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem', lineHeight: 1.5 }}>
              {t.studyPlan.deleteGoalDesc}
            </p>
            <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
              <button className="btn btn-outline" onClick={() => setShowDeleteConfirm(false)}>
                {t.common.cancel}
              </button>
              <button
                className="btn"
                style={{ background: 'var(--error-color)', color: '#fff' }}
                onClick={handleDeleteGoal}
              >
                {t.studyPlan.deleteGoal}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}


function GoalCard({ goal, onDelete }: { goal: StudyGoal; onDelete: () => void }) {
  const { t, dateLocale } = useTranslation();
  const [showMenu, setShowMenu] = useState(false);

  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.25rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: '0.5rem' }}>
        <div style={{ flex: 1, minWidth: 0 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.5rem', flexWrap: 'wrap' }}>
            <h2 style={{ margin: 0, wordBreak: 'break-word' }}>{goal.examTypeName}</h2>
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
            <StatBox label={t.studyPlan.daysToExam} value={goal.daysUntilExam} color={
              goal.daysUntilExam < 14 ? 'var(--error-color)' :
              goal.daysUntilExam < 30 ? 'var(--warning-color)' :
              'var(--success-color)'
            } />
            <StatBox label={t.studyPlan.targetScoreLabel} value={goal.targetScore} />
            <StatBox label={t.studyPlan.recHoursPerDay} value={goal.recommendedHoursPerDay} />
            <StatBox label={t.studyPlan.deadline} value={new Date(goal.targetDate).toLocaleDateString(dateLocale)} />
          </div>
        </div>
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
                  {t.studyPlan.deleteGoal}
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


function TodayTab({ todayPlan, onStart }: { todayPlan: TodayPlan; onStart: (entry: StudyPlanEntry) => void }) {
  const { t } = useTranslation();
  const completedCount = todayPlan.entries.filter(e => e.isCompleted).length;
  const totalCount = todayPlan.entries.length;
  const progress = totalCount > 0 ? (completedCount / totalCount) * 100 : 0;

  return (
    <div className="animate-fade-in-up">
      <div className="card card-static" style={{ padding: '1rem', marginBottom: '1rem', background: 'var(--primary-bg)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <div>
            <div style={{ fontWeight: 600, marginBottom: '0.2rem' }}>{todayPlan.recommendation}</div>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
              ~{todayPlan.totalMinutesToday} {t.studyPlan.minLeft} • {completedCount}/{totalCount} {t.studyPlan.tasksDone}
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

      {todayPlan.entries.length === 0 ? (
        <div className="card card-static" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
          {t.studyPlan.noTasksToday}
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
  const { t } = useTranslation();
  const TYPE_LABELS: Record<string, string> = {
    New: t.studyPlan.typeNew,
    Review: t.studyPlan.typeReview,
    Practice: t.studyPlan.typePractice,
    Weakness: t.studyPlan.typeWeakness,
  };
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
            {entry.isCompleted && <span style={{ color: 'var(--success-color)', fontWeight: 600 }}>✓ {t.studyPlan.done}</span>}
          </div>
          <div style={{ fontWeight: 600, fontSize: '1rem' }}>{entry.topicName}</div>
          <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
            {entry.recommendedMinutes} {t.studyPlan.min} • {entry.recommendedQuestions} {t.studyPlan.questions}
            {entry.isCompleted && entry.questionsAnswered > 0 && (
              <> • {t.studyPlan.result}: <span style={{
                color: accuracy >= 70 ? 'var(--success-color)' : accuracy >= 40 ? 'var(--warning-color)' : 'var(--error-color)',
                fontWeight: 600
              }}>{entry.correctAnswers}/{entry.questionsAnswered} ({accuracy}%)</span></>
            )}
          </div>
        </div>
        {!entry.isCompleted ? (
          <button className="btn btn-primary" style={{ fontSize: '0.85rem', whiteSpace: 'nowrap' }} onClick={onStart}>
            ▶ {t.studyPlan.start}
          </button>
        ) : (
          <button className="btn btn-outline" style={{ fontSize: '0.8rem', whiteSpace: 'nowrap' }} onClick={onStart}>
            {t.studyPlan.more}
          </button>
        )}
      </div>
    </div>
  );
}


function PlanTab({ plan }: { plan: StudyPlan }) {
  const { t, dateLocale } = useTranslation();
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
      <div className="card card-static" style={{ padding: '1rem', marginBottom: '1rem' }}>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(120px, 1fr))', gap: '1rem' }}>
          <StatBox label={t.studyPlan.totalTasks} value={plan.totalEntries} />
          <StatBox label={t.studyPlan.completedTasks} value={plan.completedEntries} color="var(--success-color)" />
          <StatBox label={t.studyPlan.progressLabel} value={`${plan.completionPercent}%`} color="var(--primary-color)" />
          <StatBox label={t.studyPlan.daysInPlan} value={sortedDates.length} />
        </div>
        <div style={{ marginTop: '0.75rem', background: 'var(--border-color)', borderRadius: '8px', height: '8px', overflow: 'hidden' }}>
          <div style={{
            width: `${plan.completionPercent}%`, height: '100%',
            background: 'var(--primary-color)', borderRadius: '8px', transition: 'width 0.5s ease'
          }} />
        </div>
      </div>

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
                  {new Date(dateStr).toLocaleDateString(dateLocale, { weekday: 'short', day: 'numeric', month: 'short' })}
                  {isToday && ` (${t.studyPlan.todayLabel})`}
                </div>
                <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                  {entries.filter(e => e.isCompleted).length}/{entries.length} • 
                  {entries.reduce((s, e) => s + e.recommendedMinutes, 0)} {t.studyPlan.min}
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


function StatsTab({ stats }: { stats: PlanStats }) {
  const { t, dateLocale } = useTranslation();
  const weeklyChartData = stats.weeklySummary.map(w => ({
    week: new Date(w.weekStart).toLocaleDateString(dateLocale, { day: 'numeric', month: 'short' }),
    planned: w.plannedEntries,
    completed: w.completedEntries,
    accuracy: w.accuracy,
  }));

  return (
    <div className="animate-fade-in-up">
      <div className="card card-static" style={{ padding: '1.25rem', marginBottom: '1rem' }}>
        <h3 style={{ margin: '0 0 1rem' }}>{t.studyPlan.generalStats}</h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(130px, 1fr))', gap: '1rem' }}>
          <StatBox label={t.studyPlan.activeDays} value={stats.completedDays} color="var(--success-color)" />
          <StatBox label={t.studyPlan.skippedDays} value={stats.skippedDays} color="var(--error-color)" />
          <StatBox label={t.studyPlan.questionsAnswered} value={stats.totalQuestionsAnswered} />
          <StatBox
            label={t.studyPlan.accuracyLabel}
            value={`${stats.averageAccuracy}%`}
            color={stats.averageAccuracy >= 70 ? 'var(--success-color)' : 'var(--warning-color)'}
          />
          <StatBox
            label={t.studyPlan.adherence}
            value={`${stats.adherencePercent}%`}
            color={stats.adherencePercent >= 70 ? 'var(--success-color)' : 'var(--warning-color)'}
          />
        </div>
      </div>

      {weeklyChartData.length > 0 && (
        <div className="card card-static" style={{ padding: '1.25rem', marginBottom: '1rem' }}>
          <h3 style={{ margin: '0 0 1rem' }}>{t.studyPlan.weeklyProgress}</h3>
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
              <Bar dataKey="planned" name={t.studyPlan.planned} fill="#6366f1" radius={[4, 4, 0, 0]} />
              <Bar dataKey="completed" name={t.studyPlan.completed} fill="#10b981" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {weeklyChartData.length > 1 && (
        <div className="card card-static" style={{ padding: '1.25rem' }}>
          <h3 style={{ margin: '0 0 1rem' }}>{t.studyPlan.accuracyTrend}</h3>
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
              <Line type="monotone" dataKey="accuracy" name={t.studyPlan.accuracyPercent} stroke="#f59e0b" strokeWidth={2} dot={{ r: 4 }} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  );
}


const EXAM_SCORE_CONFIG: Record<string, { min: number; max: number; step: number; default: number }> = {
  SAT:  { min: 400, max: 1600, step: 10, default: 1200 },
  NUET: { min: 0,   max: 200,  step: 1,  default: 150 },
};
const DEFAULT_SCORE_CONFIG = { min: 0, max: 100, step: 1, default: 70 };

function GoalFormModal({
  exams, formExam, formDate, formScore, formHoursPerDay, isSubmitting,
  sections, selectedSectionIds,
  onChangeExam, onChangeDate, onChangeScore, onChangeHoursPerDay, onChangeSections, onSubmit, onClose,
}: {
  exams: ExamType[];
  formExam: string; formDate: string; formScore: number; formHoursPerDay: number | null; isSubmitting: boolean;
  sections: ExamSection[]; selectedSectionIds: number[];
  onChangeExam: (v: string) => void; onChangeDate: (v: string) => void;
  onChangeScore: (v: number) => void; onChangeHoursPerDay: (v: number | null) => void;
  onChangeSections: (ids: number[]) => void;
  onSubmit: (e: React.FormEvent) => void; onClose: () => void;
}) {
  const minDate = new Date(Date.now() + 7 * 86400000).toISOString().split('T')[0];
  const { t } = useTranslation();
  const cfg = EXAM_SCORE_CONFIG[formExam] ?? DEFAULT_SCORE_CONFIG;

  const selectedSections = sections.filter((s) => selectedSectionIds.includes(s.id));
  const hasSectionScores = sections.length > 0 && sections.some((s) => s.maxScore > 0);
  const dynamicMax = hasSectionScores && selectedSections.length > 0
    ? selectedSections.reduce((sum, s) => sum + s.maxScore, 0)
    : cfg.max;
  const dynamicMin = hasSectionScores && selectedSections.length > 0
    ? selectedSections.reduce((sum, s) => sum + s.minScore, 0)
    : cfg.min;

  const handleExamChange = (code: string) => {
    onChangeExam(code);
    const c = EXAM_SCORE_CONFIG[code] ?? DEFAULT_SCORE_CONFIG;
    onChangeScore(c.default);
  };

  const toggleSection = (id: number) => {
    const next = selectedSectionIds.includes(id)
      ? selectedSectionIds.filter((x) => x !== id)
      : [...selectedSectionIds, id];
    onChangeSections(next);
    if (hasSectionScores && next.length > 0) {
      const newMax = sections.filter((s) => next.includes(s.id)).reduce((sum, s) => sum + s.maxScore, 0);
      if (formScore > newMax) onChangeScore(newMax);
    }
  };

  const effectiveMax = dynamicMax;
  const effectiveMin = dynamicMin;

  return (
    <div style={{
      position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex',
      alignItems: 'center', justifyContent: 'center', zIndex: 1000
    }} onClick={onClose}>
      <div className="card animate-fade-in-scale" style={{ padding: '2rem', maxWidth: '450px', width: '90%' }}
        onClick={(e) => e.stopPropagation()}>
        <h2 style={{ margin: '0 0 1.5rem' }}>{t.studyPlan.goalFormTitle}</h2>
        <form onSubmit={onSubmit}>
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.4rem', fontWeight: 600, fontSize: '0.9rem' }}>
              {t.studyPlan.exam}
            </label>
            <select
              value={formExam}
              onChange={(e) => handleExamChange(e.target.value)}
              required
              style={{
                width: '100%', padding: '0.6rem', borderRadius: '8px',
                border: '1px solid var(--border-color)', background: 'var(--card-bg)',
                color: 'var(--text-primary)', fontSize: '0.95rem'
              }}
            >
              <option value="">{t.studyPlan.selectExam}...</option>
              {exams.map((e) => (
                <option key={e.code} value={e.code}>{e.name}</option>
              ))}
            </select>
          </div>

          {sections.length > 1 && (
            <div style={{ marginBottom: '1rem' }}>
              <label style={{ display: 'block', marginBottom: '0.4rem', fontWeight: 600, fontSize: '0.9rem' }}>
                {t.studyPlan.sections}
              </label>
              <div style={{
                display: 'flex', flexDirection: 'column', gap: '0.35rem',
                padding: '0.6rem', borderRadius: '8px', border: '1px solid var(--border-color)',
                background: 'var(--card-bg)', maxHeight: '160px', overflowY: 'auto',
              }}>
                {sections.map((s) => (
                  <label key={s.id} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer', fontSize: '0.9rem' }}>
                    <input
                      type="checkbox"
                      checked={selectedSectionIds.includes(s.id)}
                      onChange={() => toggleSection(s.id)}
                    />
                    <span>{s.name}</span>
                    {s.maxScore > 0 && (
                      <span style={{ marginLeft: 'auto', color: 'var(--text-secondary)', fontSize: '0.8rem' }}>
                        {s.minScore}–{s.maxScore}
                      </span>
                    )}
                  </label>
                ))}
              </div>
            </div>
          )}

          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.4rem', fontWeight: 600, fontSize: '0.9rem' }}>
              {t.studyPlan.examDate}
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
              {t.studyPlan.targetScoreValue}: {formScore}
            </label>
            <input
              type="range"
              min={effectiveMin}
              max={effectiveMax}
              step={cfg.step}
              value={formScore}
              onChange={(e) => onChangeScore(Number(e.target.value))}
              style={{ width: '100%' }}
            />
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
              <span>{effectiveMin}</span>
              <span>{effectiveMax}</span>
            </div>
          </div>

          <div style={{ marginBottom: '1.5rem' }}>
            <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.4rem', fontWeight: 600, fontSize: '0.9rem' }}>
              <input
                type="checkbox"
                checked={formHoursPerDay !== null}
                onChange={(e) => onChangeHoursPerDay(e.target.checked ? 2 : null)}
              />
              {t.studyPlan.hoursPerDayLabel}: {formHoursPerDay !== null ? formHoursPerDay : t.studyPlan.hoursPerDayAuto}
            </label>
            {formHoursPerDay !== null && (
              <>
                <input
                  type="range"
                  min={0.5}
                  max={6}
                  step={0.5}
                  value={formHoursPerDay}
                  onChange={(e) => onChangeHoursPerDay(Number(e.target.value))}
                  style={{ width: '100%' }}
                />
                <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                  <span>0.5</span>
                  <span>6</span>
                </div>
              </>
            )}
          </div>

          <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
            <button type="button" className="btn btn-outline" onClick={onClose}>{t.common.cancel}</button>
            <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
              {isSubmitting ? t.studyPlan.creating : t.studyPlan.createAndGenerate}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default StudyPlanPage;
