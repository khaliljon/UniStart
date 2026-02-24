import { useEffect, useState } from 'react';
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  RadarChart,
  PolarGrid,
  PolarAngleAxis,
  PolarRadiusAxis,
  Radar,
  BarChart,
  Bar,
  Cell,
} from 'recharts';
import { analyticsService } from '../services/analyticsService';
import { AnalyticsSkeleton } from '../components/Skeleton';
import type { Dashboard } from '../types';

const SKILL_COLORS = [
  '#6366f1', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6',
  '#06b6d4', '#ec4899', '#14b8a6',
];

const DIFFICULTY_COLORS: Record<string, string> = {
  Easy: '#10b981',
  Medium: '#f59e0b',
  Hard: '#ef4444',
};

function AnalyticsPage() {
  const [dashboard, setDashboard] = useState<Dashboard | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchDashboard = async () => {
      try {
        const data = await analyticsService.getDashboard();
        setDashboard(data);
      } catch (err) {
        setError('Failed to load analytics');
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchDashboard();
  }, []);

  if (isLoading) return <AnalyticsSkeleton />;
  if (error) return <p className="error-message animate-fade-in">{error}</p>;
  if (!dashboard) return <p className="animate-fade-in">No analytics data available</p>;

  const getSkillLevelColor = (level: number) => {
    if (level >= 70) return 'var(--success-color)';
    if (level >= 40) return 'var(--warning-color)';
    return 'var(--error-color)';
  };

  // Prepare line chart data — pivot skill history into { date, skill1, skill2, ... }
  const lineChartData = (() => {
    const byDate: Record<string, Record<string, number>> = {};
    dashboard.skillHistory.forEach((p) => {
      const d = new Date(p.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
      if (!byDate[d]) byDate[d] = {};
      byDate[d][p.skillName] = p.level;
    });
    return Object.entries(byDate).map(([date, skills]) => ({ date, ...skills }));
  })();

  const uniqueSkills = [...new Set(dashboard.skillHistory.map((p) => p.skillName))];

  // Radar chart data
  const radarData = dashboard.skillProfiles.map((s) => ({
    skill: s.skillName,
    level: s.level,
    fullMark: 100,
  }));

  // Difficulty bar chart
  const difficultyData = dashboard.difficultyBreakdown.map((d) => ({
    ...d,
    incorrectCount: d.totalAnswered - d.correctCount,
  }));

  // Activity heatmap — last 12 weeks grid
  const activityWeeks = (() => {
    const weeks: { date: string; count: number; level: number }[][] = [];
    const today = new Date();
    const map = new Map(dashboard.activityHeatmap.map((a) => [new Date(a.date).toISOString().slice(0, 10), a.questionsAnswered]));
    
    // Build 12 weeks × 7 days grid
    for (let w = 11; w >= 0; w--) {
      const week: { date: string; count: number; level: number }[] = [];
      for (let d = 0; d < 7; d++) {
        const date = new Date(today);
        date.setDate(date.getDate() - (w * 7 + (6 - d)));
        const key = date.toISOString().slice(0, 10);
        const count = map.get(key) || 0;
        week.push({
          date: key,
          count,
          level: count === 0 ? 0 : count <= 3 ? 1 : count <= 8 ? 2 : count <= 15 ? 3 : 4,
        });
      }
      weeks.push(week);
    }
    return weeks;
  })();

  const heatmapColors = ['var(--border-color)', '#9be9a8', '#40c463', '#30a14e', '#216e39'];

  return (
    <div className="animate-fade-in" style={{ maxWidth: '1000px', margin: '0 auto' }}>
      <h1 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '2rem' }}>
        Your Progress Analytics
      </h1>

      {/* Stats Overview */}
      <div className="stats-grid">
        <div className="card stat-card animate-fade-in-up" style={{ animationDelay: '0.1s' }}>
          <div className="stat-value">{dashboard.totalQuestionsAnswered}</div>
          <div className="stat-label">Questions Answered</div>
        </div>
        <div className="card stat-card animate-fade-in-up" style={{ animationDelay: '0.2s' }}>
          <div className="stat-value">{dashboard.correctAnswers}</div>
          <div className="stat-label">Correct Answers</div>
        </div>
        <div className="card stat-card animate-fade-in-up" style={{ animationDelay: '0.3s' }}>
          <div className="stat-value">{dashboard.overallAccuracy}%</div>
          <div className="stat-label">Accuracy</div>
        </div>
        <div className="card stat-card animate-fade-in-up" style={{ animationDelay: '0.4s' }}>
          <div className="stat-value" style={{ display: 'flex', alignItems: 'center', gap: '0.25rem', justifyContent: 'center' }}>
            🔥 {dashboard.currentStreak}
          </div>
          <div className="stat-label">Day Streak (Best: {dashboard.bestStreak})</div>
        </div>
      </div>

      {/* Skill Levels — Radar Chart */}
      {radarData.length > 0 && (
        <div className="card card-static animate-fade-in-up" style={{ marginTop: '2rem', animationDelay: '0.5s' }}>
          <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1rem' }}>
            Skill Profile
          </h2>
          <ResponsiveContainer width="100%" height={300}>
            <RadarChart data={radarData} cx="50%" cy="50%" outerRadius="75%">
              <PolarGrid stroke="var(--border-color)" />
              <PolarAngleAxis dataKey="skill" tick={{ fill: 'var(--text-secondary)', fontSize: 12 }} />
              <PolarRadiusAxis angle={30} domain={[0, 100]} tick={{ fill: 'var(--text-secondary)', fontSize: 10 }} />
              <Radar name="Skill Level" dataKey="level" stroke="#6366f1" fill="#6366f1" fillOpacity={0.3} />
            </RadarChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Skill Progress Over Time — Line Chart */}
      {lineChartData.length > 1 && (
        <div className="card card-static animate-fade-in-up" style={{ marginTop: '2rem', animationDelay: '0.6s' }}>
          <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1rem' }}>
            Skill Progress Over Time
          </h2>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={lineChartData}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" />
              <XAxis dataKey="date" tick={{ fill: 'var(--text-secondary)', fontSize: 12 }} />
              <YAxis domain={[0, 100]} tick={{ fill: 'var(--text-secondary)', fontSize: 12 }} />
              <Tooltip
                contentStyle={{
                  backgroundColor: 'var(--card-background)',
                  border: '1px solid var(--border-color)',
                  borderRadius: '0.5rem',
                  color: 'var(--text-primary)',
                }}
              />
              <Legend />
              {uniqueSkills.map((skill, i) => (
                <Line
                  key={skill}
                  type="monotone"
                  dataKey={skill}
                  stroke={SKILL_COLORS[i % SKILL_COLORS.length]}
                  strokeWidth={2}
                  dot={{ r: 3 }}
                  connectNulls
                />
              ))}
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Difficulty Breakdown — Bar Chart */}
      {difficultyData.length > 0 && (
        <div className="card card-static animate-fade-in-up" style={{ marginTop: '2rem', animationDelay: '0.7s' }}>
          <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1rem' }}>
            Accuracy by Difficulty
          </h2>
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={difficultyData} barGap={8}>
              <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" />
              <XAxis dataKey="difficulty" tick={{ fill: 'var(--text-secondary)', fontSize: 12 }} />
              <YAxis tick={{ fill: 'var(--text-secondary)', fontSize: 12 }} />
              <Tooltip
                contentStyle={{
                  backgroundColor: 'var(--card-background)',
                  border: '1px solid var(--border-color)',
                  borderRadius: '0.5rem',
                  color: 'var(--text-primary)',
                }}
              />
              <Legend />
              <Bar dataKey="correctCount" name="Correct" stackId="a" radius={[0, 0, 0, 0]}>
                {difficultyData.map((entry) => (
                  <Cell key={entry.difficulty} fill={DIFFICULTY_COLORS[entry.difficulty] || '#6366f1'} />
                ))}
              </Bar>
              <Bar dataKey="incorrectCount" name="Incorrect" stackId="a" fill="#e5e7eb" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
          <div style={{ display: 'flex', gap: '2rem', justifyContent: 'center', marginTop: '0.75rem' }}>
            {difficultyData.map((d) => (
              <span key={d.difficulty} style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                {d.difficulty}: <strong style={{ color: DIFFICULTY_COLORS[d.difficulty] }}>{d.accuracy}%</strong>
              </span>
            ))}
          </div>
        </div>
      )}

      {/* Activity Heatmap */}
      <div className="card card-static animate-fade-in-up" style={{ marginTop: '2rem', animationDelay: '0.8s' }}>
        <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1rem' }}>
          Activity (Last 12 Weeks)
        </h2>
        <div style={{ display: 'flex', gap: '3px', justifyContent: 'center', flexWrap: 'wrap' }}>
          {activityWeeks.map((week, wi) => (
            <div key={wi} style={{ display: 'flex', flexDirection: 'column', gap: '3px' }}>
              {week.map((day) => (
                <div
                  key={day.date}
                  title={`${day.date}: ${day.count} questions`}
                  style={{
                    width: '14px',
                    height: '14px',
                    borderRadius: '2px',
                    backgroundColor: heatmapColors[day.level],
                    cursor: 'default',
                  }}
                />
              ))}
            </div>
          ))}
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: '4px', justifyContent: 'flex-end', marginTop: '0.5rem', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
          <span>Less</span>
          {heatmapColors.map((c, i) => (
            <div key={i} style={{ width: '12px', height: '12px', borderRadius: '2px', backgroundColor: c }} />
          ))}
          <span>More</span>
        </div>
      </div>

      {/* Skill Bars (detailed) */}
      <div className="card card-static animate-fade-in-up" style={{ marginTop: '2rem', animationDelay: '0.9s' }}>
        <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1.5rem' }}>
          Skill Levels
        </h2>
        {dashboard.skillProfiles.length === 0 ? (
          <p style={{ color: 'var(--text-secondary)' }}>
            No skill data yet. Start taking tests to see your progress!
          </p>
        ) : (
          <div className="skills-list">
            {dashboard.skillProfiles.map((skill, index) => (
              <div
                key={skill.skillId}
                className="skill-item animate-slide-in"
                style={{ animationDelay: `${1.0 + index * 0.1}s` }}
              >
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

      {/* Legend */}
      <div className="card" style={{ marginTop: '2rem' }}>
        <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1rem' }}>
          Understanding Your Skill Levels
        </h2>
        <div style={{ display: 'flex', gap: '2rem', flexWrap: 'wrap' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div style={{ width: '1rem', height: '1rem', borderRadius: '50%', backgroundColor: 'var(--error-color)' }} />
            <span style={{ fontSize: '0.875rem' }}>0-39: Beginner (Easy questions)</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div style={{ width: '1rem', height: '1rem', borderRadius: '50%', backgroundColor: 'var(--warning-color)' }} />
            <span style={{ fontSize: '0.875rem' }}>40-69: Intermediate (Medium questions)</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div style={{ width: '1rem', height: '1rem', borderRadius: '50%', backgroundColor: 'var(--success-color)' }} />
            <span style={{ fontSize: '0.875rem' }}>70-100: Advanced (Hard questions)</span>
          </div>
        </div>
      </div>
    </div>
  );
}

export default AnalyticsPage;
