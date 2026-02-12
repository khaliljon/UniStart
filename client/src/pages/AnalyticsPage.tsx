import { useEffect, useState } from 'react';
import { analyticsService } from '../services/analyticsService';
import type { SkillAnalytics } from '../types';

function AnalyticsPage() {
  const [analytics, setAnalytics] = useState<SkillAnalytics | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        const data = await analyticsService.getAnalytics();
        setAnalytics(data);
      } catch (err) {
        setError('Failed to load analytics');
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchAnalytics();
  }, []);

  if (isLoading) {
    return (
      <div className="loading">
        <div className="spinner"></div>
      </div>
    );
  }

  if (error) {
    return <p className="error-message">{error}</p>;
  }

  if (!analytics) {
    return <p>No analytics data available</p>;
  }

  const getSkillLevelColor = (level: number) => {
    if (level >= 70) return 'var(--success-color)';
    if (level >= 40) return 'var(--warning-color)';
    return 'var(--error-color)';
  };

  return (
    <div>
      <h1 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '2rem' }}>
        Your Progress Analytics
      </h1>

      {/* Stats Overview */}
      <div className="stats-grid">
        <div className="card stat-card">
          <div className="stat-value">{analytics.totalQuestionsAnswered}</div>
          <div className="stat-label">Questions Answered</div>
        </div>
        <div className="card stat-card">
          <div className="stat-value">{analytics.correctAnswers}</div>
          <div className="stat-label">Correct Answers</div>
        </div>
        <div className="card stat-card">
          <div className="stat-value">{analytics.overallAccuracy}%</div>
          <div className="stat-label">Accuracy</div>
        </div>
        <div className="card stat-card">
          <div className="stat-value">{analytics.skillProfiles.length}</div>
          <div className="stat-label">Skills Tracked</div>
        </div>
      </div>

      {/* Skills Breakdown */}
      <div className="card" style={{ marginTop: '2rem' }}>
        <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1.5rem' }}>
          Skill Levels
        </h2>

        {analytics.skillProfiles.length === 0 ? (
          <p style={{ color: 'var(--text-secondary)' }}>
            No skill data yet. Start taking tests to see your progress!
          </p>
        ) : (
          <div className="skills-list">
            {analytics.skillProfiles.map((skill) => (
              <div key={skill.skillId} className="skill-item">
                <span className="skill-name">{skill.skillName}</span>
                <div className="skill-bar">
                  <div
                    className="skill-bar-fill"
                    style={{
                      width: `${skill.level}%`,
                      backgroundColor: getSkillLevelColor(skill.level),
                    }}
                  />
                </div>
                <span className="skill-level" style={{ color: getSkillLevelColor(skill.level) }}>
                  {skill.level}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Recent Progress */}
      {analytics.recentProgress.length > 0 && (
        <div className="card" style={{ marginTop: '2rem' }}>
          <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1.5rem' }}>
            Recent Activity
          </h2>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {analytics.recentProgress.slice(0, 5).map((progress, index) => (
              <div
                key={index}
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  padding: '0.75rem',
                  backgroundColor: 'var(--background-color)',
                  borderRadius: '0.5rem',
                }}
              >
                <span>{progress.skillName}</span>
                <span
                  style={{
                    color:
                      progress.newLevel > progress.oldLevel
                        ? 'var(--success-color)'
                        : 'var(--error-color)',
                  }}
                >
                  {progress.oldLevel} → {progress.newLevel}
                </span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Skill Level Legend */}
      <div className="card" style={{ marginTop: '2rem' }}>
        <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1rem' }}>
          Understanding Your Skill Levels
        </h2>
        <div style={{ display: 'flex', gap: '2rem', flexWrap: 'wrap' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div
              style={{
                width: '1rem',
                height: '1rem',
                borderRadius: '50%',
                backgroundColor: 'var(--error-color)',
              }}
            />
            <span style={{ fontSize: '0.875rem' }}>0-39: Beginner (Easy questions)</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div
              style={{
                width: '1rem',
                height: '1rem',
                borderRadius: '50%',
                backgroundColor: 'var(--warning-color)',
              }}
            />
            <span style={{ fontSize: '0.875rem' }}>40-69: Intermediate (Medium questions)</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div
              style={{
                width: '1rem',
                height: '1rem',
                borderRadius: '50%',
                backgroundColor: 'var(--success-color)',
              }}
            />
            <span style={{ fontSize: '0.875rem' }}>70-100: Advanced (Hard questions)</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default AnalyticsPage;
