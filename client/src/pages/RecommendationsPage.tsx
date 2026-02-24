import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import recommendationService from '../services/recommendationService';
import type { DailyBriefing, Milestone, Recommendation, Streak } from '../types';

function RecommendationsPage() {
  const navigate = useNavigate();
  const [briefing, setBriefing] = useState<DailyBriefing | null>(null);
  const [allMilestones, setAllMilestones] = useState<Milestone[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'daily' | 'milestones'>('daily');

  const load = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const [daily, milestones] = await Promise.all([
        recommendationService.getDailyBriefing(),
        recommendationService.getMilestones(),
      ]);
      setBriefing(daily);
      setAllMilestones(milestones);
    } catch {
      setError('Ошибка загрузки рекомендаций');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  if (isLoading) {
    return (
      <div className="container animate-fade-in" style={{ textAlign: 'center', padding: '4rem 1rem' }}>
        <div className="loading-spinner" />
        <p style={{ color: 'var(--text-secondary)', marginTop: '1rem' }}>Загрузка рекомендаций…</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container animate-fade-in" style={{ textAlign: 'center', padding: '4rem 1rem' }}>
        <p style={{ color: 'var(--error-color)' }}>{error}</p>
        <button className="btn btn-primary" style={{ marginTop: '1rem' }} onClick={load}>
          Повторить
        </button>
      </div>
    );
  }

  const streak: Streak | null = briefing?.streak ?? null;

  return (
    <div className="container animate-fade-in" style={{ padding: '2rem 1rem' }}>
      <h1 style={{ marginBottom: '0.5rem' }}>🧭 Рекомендации</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
        Персональные советы на основе вашего прогресса
      </p>

      {/* ─── Streak Card ──────────────────────────────── */}
      {streak && (
        <div
          className="card animate-slide-up"
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '1.5rem',
            padding: '1.5rem',
            marginBottom: '1.5rem',
            background: streak.currentStreak >= 7
              ? 'linear-gradient(135deg, #f97316 0%, #ef4444 100%)'
              : streak.currentStreak >= 3
                ? 'linear-gradient(135deg, #f59e0b 0%, #f97316 100%)'
                : 'var(--card-background)',
            color: streak.currentStreak >= 3 ? '#fff' : 'var(--text-primary)',
          }}
        >
          <div style={{ fontSize: '3rem' }}>
            {streak.currentStreak >= 7 ? '🔥' : streak.currentStreak >= 3 ? '⚡' : streak.studiedToday ? '✅' : '📅'}
          </div>
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: '1.5rem', fontWeight: 700 }}>
              {streak.currentStreak > 0 ? `${streak.currentStreak} дней подряд!` : 'Начните серию!'}
            </div>
            <div style={{ opacity: 0.85, fontSize: '0.9rem', marginTop: '0.25rem' }}>
              {streak.studiedToday
                ? 'Вы уже позанимались сегодня 🎉'
                : streak.currentStreak > 0
                  ? 'Не забудьте позаниматься сегодня!'
                  : 'Ответьте хотя бы на несколько вопросов, чтобы начать серию'}
            </div>
          </div>
          <div style={{ textAlign: 'center' }}>
            <div style={{ fontSize: '0.75rem', textTransform: 'uppercase', opacity: 0.7, letterSpacing: '0.05em' }}>
              Рекорд
            </div>
            <div style={{ fontSize: '1.75rem', fontWeight: 700 }}>
              {streak.longestStreak}
            </div>
            <div style={{ fontSize: '0.75rem', opacity: 0.7 }}>дней</div>
          </div>
          <div style={{ textAlign: 'center' }}>
            <div style={{ fontSize: '0.75rem', textTransform: 'uppercase', opacity: 0.7, letterSpacing: '0.05em' }}>
              Всего
            </div>
            <div style={{ fontSize: '1.75rem', fontWeight: 700 }}>
              {streak.totalStudyDays}
            </div>
            <div style={{ fontSize: '0.75rem', opacity: 0.7 }}>дней</div>
          </div>
        </div>
      )}

      {/* ─── Yesterday Summary ────────────────────────── */}
      {briefing?.yesterdaySummary && (
        <div className="card animate-slide-up" style={{ padding: '1.25rem', marginBottom: '1.5rem' }}>
          <h3 style={{ marginBottom: '1rem', fontSize: '1rem' }}>📊 Вчерашний итог</h3>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(120px, 1fr))', gap: '1rem' }}>
            <StatBox label="Вопросов" value={briefing.yesterdaySummary.questionsAnswered} />
            <StatBox label="Верных" value={briefing.yesterdaySummary.correctAnswers} />
            <StatBox label="Точность" value={`${briefing.yesterdaySummary.accuracy}%`} />
            <StatBox label="Минут" value={briefing.yesterdaySummary.minutesSpent} />
            <StatBox label="Тем" value={briefing.yesterdaySummary.topicsStudied} />
          </div>
        </div>
      )}

      {/* ─── Tabs ─────────────────────────────────────── */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1.5rem' }}>
        <button
          className={`btn ${activeTab === 'daily' ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setActiveTab('daily')}
        >
          💡 Советы дня ({briefing?.recommendations.length ?? 0})
        </button>
        <button
          className={`btn ${activeTab === 'milestones' ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setActiveTab('milestones')}
        >
          🏆 Достижения ({allMilestones.length})
        </button>
      </div>

      {/* ─── Tab: Daily Recommendations ───────────────── */}
      {activeTab === 'daily' && (
        <div className="animate-fade-in">
          {(!briefing?.recommendations || briefing.recommendations.length === 0) ? (
            <div className="card" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
              <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>🎉</div>
              <p>На сегодня рекомендаций нет — вы на правильном пути!</p>
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              {briefing.recommendations.map((rec, i) => (
                <RecommendationCard key={i} rec={rec} onAction={(url) => navigate(url)} />
              ))}
            </div>
          )}
        </div>
      )}

      {/* ─── Tab: Milestones ──────────────────────────── */}
      {activeTab === 'milestones' && (
        <div className="animate-fade-in">
          {allMilestones.length === 0 ? (
            <div className="card" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
              <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>🎯</div>
              <p>Начните заниматься, чтобы получить первые достижения!</p>
            </div>
          ) : (
            <div style={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fill, minmax(260px, 1fr))',
              gap: '1rem',
            }}>
              {allMilestones.map((m) => (
                <MilestoneCard key={m.id} milestone={m} />
              ))}
            </div>
          )}
        </div>
      )}

      {/* ─── Recent Milestones (new) ──────────────────── */}
      {briefing?.recentMilestones && briefing.recentMilestones.filter(m => m.isNew).length > 0 && (
        <div
          className="card animate-slide-up"
          style={{
            marginTop: '1.5rem', padding: '1.25rem',
            border: '2px solid var(--warning-color)',
          }}
        >
          <h3 style={{ marginBottom: '1rem' }}>🎊 Новые достижения!</h3>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.75rem' }}>
            {briefing.recentMilestones.filter(m => m.isNew).map(m => (
              <div
                key={m.id}
                style={{
                  padding: '0.5rem 1rem',
                  borderRadius: '999px',
                  background: 'var(--warning-color)',
                  color: '#fff',
                  fontWeight: 600,
                  fontSize: '0.9rem',
                }}
              >
                {m.icon} {m.title}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════
//  Sub-components
// ═══════════════════════════════════════════════════════

function StatBox({ label, value }: { label: string; value: string | number }) {
  return (
    <div style={{ textAlign: 'center' }}>
      <div style={{ fontSize: '1.5rem', fontWeight: 700, color: 'var(--primary-color)' }}>
        {value}
      </div>
      <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{label}</div>
    </div>
  );
}

const PRIORITY_STYLES: Record<string, { border: string; bg: string }> = {
  high: { border: 'var(--error-color)', bg: 'rgba(239,68,68,0.06)' },
  medium: { border: 'var(--warning-color)', bg: 'rgba(245,158,11,0.06)' },
  low: { border: 'var(--success-color)', bg: 'rgba(16,185,129,0.06)' },
};

function RecommendationCard({ rec, onAction }: { rec: Recommendation; onAction: (url: string) => void }) {
  const style = PRIORITY_STYLES[rec.priority] ?? PRIORITY_STYLES.medium;

  return (
    <div
      className="card"
      style={{
        display: 'flex',
        alignItems: 'center',
        gap: '1rem',
        padding: '1rem 1.25rem',
        borderLeft: `4px solid ${style.border}`,
        background: style.bg,
      }}
    >
      <div style={{ fontSize: '1.75rem', flexShrink: 0 }}>{rec.icon ?? '💡'}</div>
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ fontWeight: 600, marginBottom: '0.25rem' }}>{rec.title}</div>
        <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{rec.description}</div>
      </div>
      {rec.actionUrl && rec.actionLabel && (
        <button
          className="btn btn-primary"
          style={{ flexShrink: 0, fontSize: '0.85rem', padding: '0.4rem 1rem' }}
          onClick={() => onAction(rec.actionUrl!)}
        >
          {rec.actionLabel}
        </button>
      )}
    </div>
  );
}

function MilestoneCard({ milestone }: { milestone: Milestone }) {
  const dateStr = new Date(milestone.achievedAt).toLocaleDateString('ru-RU', {
    day: 'numeric', month: 'short', year: 'numeric',
  });

  return (
    <div
      className="card"
      style={{
        padding: '1.25rem',
        textAlign: 'center',
        transition: 'transform 0.2s',
      }}
      onMouseEnter={(e) => (e.currentTarget.style.transform = 'translateY(-3px)')}
      onMouseLeave={(e) => (e.currentTarget.style.transform = 'translateY(0)')}
    >
      <div style={{ fontSize: '2.5rem', marginBottom: '0.75rem' }}>{milestone.icon}</div>
      <div style={{ fontWeight: 700, fontSize: '1.05rem', marginBottom: '0.25rem' }}>
        {milestone.title}
      </div>
      <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '0.75rem' }}>
        {milestone.description}
      </div>
      <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', opacity: 0.7 }}>
        {dateStr}
      </div>
    </div>
  );
}

export default RecommendationsPage;
