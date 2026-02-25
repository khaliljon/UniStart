import { useEffect, useState } from 'react';
import adminService from '../services/adminService';
import type { QuestionStats } from '../types';

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

function AdminStatsPage() {
  const [stats, setStats] = useState<QuestionStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        setIsLoading(true);
        const s = await adminService.getStats();
        setStats(s);
      } catch {
        setError('Ошибка загрузки статистики');
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
        <p style={{ color: 'var(--text-secondary)', marginTop: '1rem' }}>Загрузка…</p>
      </div>
    );
  }

  if (error || !stats) {
    return (
      <div className="container animate-fade-in" style={{ textAlign: 'center', padding: '4rem 1rem' }}>
        <p style={{ color: 'var(--error-color)' }}>{error || 'Нет данных'}</p>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
      <h1 style={{ marginBottom: '0.5rem' }}>📊 Статистика контента</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
        Обзор базы вопросов платформы
      </p>

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
