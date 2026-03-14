import { useEffect, useState } from 'react';
import adminService from '../services/adminService';
import { useTranslation } from '../hooks/useTranslation';
import type { QuestionStats } from '../types';

const EXAM_COLORS: Record<string, string> = {
  SAT: '#4f46e5',
  TOEFL: '#0891b2',
  NUET: '#7c3aed',
  IELTS: '#059669',
  CSCA: '#dc2626',
};

const DIFF_COLORS: Record<string, string> = {
  Easy: 'var(--success-color)',
  Medium: 'var(--warning-color)',
  Hard: 'var(--error-color)',
};

function AdminStatsPage() {
  const { t } = useTranslation();
  const [stats, setStats] = useState<QuestionStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showUncovered, setShowUncovered] = useState(false);
  const [topicPage, setTopicPage] = useState(0);
  const TOPICS_PER_PAGE = 20;

  useEffect(() => {
    const load = async () => {
      try {
        setIsLoading(true);
        const s = await adminService.getStats();
        setStats(s);
      } catch {
        setError(t.admin.common.loadError);
      } finally {
        setIsLoading(false);
      }
    };
    load();
  }, []);

  if (isLoading) {
    return (
      <div className="container animate-fade-in" style={{ textAlign: 'center', padding: '4rem 1rem' }}>
        <div className="loading-spinner" />
        <p style={{ color: 'var(--text-secondary)', marginTop: '1rem' }}>{t.admin.common.loading}</p>
      </div>
    );
  }

  if (error || !stats) {
    return (
      <div className="container animate-fade-in" style={{ textAlign: 'center', padding: '4rem 1rem' }}>
        <p style={{ color: 'var(--error-color)' }}>{error || t.admin.common.noData}</p>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
      <h1 style={{ marginBottom: '0.5rem' }}>{t.admin.stats.title}</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
        {t.admin.stats.subtitle}
      </p>

      {/* Top counters */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))', gap: '1rem', marginBottom: '1.5rem' }}>
        <StatCard label={t.admin.stats.totalQuestions} value={stats.totalQuestions} icon="" />
        <StatCard label={t.admin.stats.totalTopics} value={`${stats.topicsWithQuestions}/${stats.topicsWithQuestions + stats.topicsWithoutQuestions}`} icon="" />
        <div
          className="card"
          onClick={() => stats.topicsWithoutQuestions > 0 && setShowUncovered(!showUncovered)}
          style={{ padding: '1.25rem', textAlign: 'center', cursor: stats.topicsWithoutQuestions > 0 ? 'pointer' : 'default' }}
        >
          <div style={{ fontSize: '1.75rem', fontWeight: 700, color: stats.topicsWithoutQuestions > 0 ? 'var(--error-color)' : 'var(--success-color)' }}>
            {stats.topicsWithoutQuestions}
          </div>
          <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
            {t.admin.stats.withoutQuestions} {stats.topicsWithoutQuestions > 0 ? (showUncovered ? '▲' : '▼') : ''}
          </div>
        </div>
      </div>

      {/* Uncovered topics list */}
      {showUncovered && stats.topicsWithoutQuestionsList.length > 0 && (
        <div className="card" style={{ padding: '1.25rem', marginBottom: '1rem' }}>
          <h3 style={{ marginBottom: '0.75rem' }}>{t.admin.stats.topicsNoQuestions} ({stats.topicsWithoutQuestionsList.length})</h3>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', gap: '0.4rem' }}>
            {stats.topicsWithoutQuestionsList.map(name => (
              <div key={name} style={{
                padding: '0.4rem 0.75rem', borderRadius: '0.375rem',
                background: 'var(--background-color)', fontSize: '0.85rem',
                color: 'var(--text-secondary)', borderLeft: '3px solid var(--error-color)',
              }}>
                {name}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* By exam */}
      <div className="card" style={{ padding: '1.25rem', marginBottom: '1rem' }}>
        <h3 style={{ marginBottom: '1rem' }}>{t.admin.stats.byExam}</h3>
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
        <h3 style={{ marginBottom: '1rem' }}>{t.admin.stats.byDifficulty}</h3>
        <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
          {['Easy', 'Medium', 'Hard'].map(d => (
            <DiffBadge key={d} label={d} count={stats.byDifficulty[d] || 0} />
          ))}
        </div>
      </div>

      {/* By topic */}
      <div className="card" style={{ padding: '1.25rem' }}>
        <h3 style={{ marginBottom: '1rem' }}>{t.admin.stats.byTopic}</h3>
        {(() => {
          const sorted = Object.entries(stats.byTopic).sort((a, b) => b[1] - a[1]);
          const totalPages = Math.ceil(sorted.length / TOPICS_PER_PAGE);
          const page = sorted.slice(topicPage * TOPICS_PER_PAGE, (topicPage + 1) * TOPICS_PER_PAGE);
          return (
            <>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: '0.5rem' }}>
                {page.map(([topic, count]) => (
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
              {totalPages > 1 && (
                <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '0.5rem', marginTop: '1rem' }}>
                  <button
                    className="btn btn-secondary"
                    disabled={topicPage === 0}
                    onClick={() => setTopicPage(p => p - 1)}
                    style={{ fontSize: '0.8rem', padding: '0.3rem 0.75rem' }}
                  >
                    &larr; {t.admin.common.back.replace('← ', '')}
                  </button>
                  <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                    {topicPage + 1} / {totalPages}
                  </span>
                  <button
                    className="btn btn-secondary"
                    disabled={topicPage >= totalPages - 1}
                    onClick={() => setTopicPage(p => p + 1)}
                    style={{ fontSize: '0.8rem', padding: '0.3rem 0.75rem' }}
                  >
                    {t.admin.common.forward.replace(' →', '')} &rarr;
                  </button>
                </div>
              )}
            </>
          );
        })()}
      </div>
    </div>
  );
}

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

export default AdminStatsPage;
