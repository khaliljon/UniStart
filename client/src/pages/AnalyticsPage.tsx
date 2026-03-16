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
} from 'recharts';
import { analyticsService } from '../services/analyticsService';
import { subscriptionService } from '../services/subscriptionService';
import { AnalyticsSkeleton } from '../components/Skeleton';
import { ProGate } from '../components/ProGate';
import { useTranslation } from '../i18n';
import { useAppSelector } from '../hooks/useAppSelector';
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

// Maps each exam to the skill codes it uses
const EXAM_SKILL_MAP: Record<string, string[]> = {
  SAT: ['SK_READ', 'SK_WRITE', 'SK_MATH'],
  TOEFL: ['SK_READ', 'SK_WRITE', 'SK_LISTEN', 'SK_SPEAK'],
  NUET: ['SK_MATH', 'SK_CRIT'],
  IELTS: ['SK_READ', 'SK_WRITE', 'SK_LISTEN', 'SK_SPEAK'],
  CSCA: ['SK_MATH', 'SK_PHYS', 'SK_CHEM', 'SK_MATH_CN', 'SK_PHYS_CN', 'SK_CHEM_CN', 'SK_CN_TECH', 'SK_CN_HUM'],
};

const EXAM_LABELS: Record<string, string> = {
  SAT: 'SAT',
  TOEFL: 'TOEFL',
  NUET: 'NUET',
  IELTS: 'IELTS',
  CSCA: 'CSCA',
};

function AnalyticsPage() {
  const [dashboard, setDashboard] = useState<Dashboard | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [hasFullAnalytics, setHasFullAnalytics] = useState(true);
  const { selectedExams } = useAppSelector((state) => state.exam);
  const defaultExam = selectedExams.length > 0 && EXAM_LABELS[selectedExams[0]] ? selectedExams[0] : 'SAT';
  const [selectedExam, setSelectedExam] = useState<string>(defaultExam);
  const { t } = useTranslation();

  useEffect(() => {
    const fetchDashboard = async () => {
      try {
        const data = await analyticsService.getDashboard();
        setDashboard(data);
      } catch (err) {
        setError(t.progress.failedToLoad);
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };
    fetchDashboard();
    subscriptionService.getStatus().then((s) => {
      setHasFullAnalytics(s.isPro || s.limits.fullAnalytics);
    }).catch(() => {});
  }, []);

  if (isLoading) return <AnalyticsSkeleton />;
  if (error) return <p className="error-message animate-fade-in">{error}</p>;
  if (!dashboard) return <p className="animate-fade-in">{t.progress.noAnalyticsData}</p>;

  const getSkillLevelColor = (level: number) => {
    if (level >= 70) return 'var(--success-color)';
    if (level >= 40) return 'var(--warning-color)';
    return 'var(--error-color)';
  };

  // Format skill name with exam type prefix
  const getSkillLabel = (skillName: string, _skillCode: string) => {
    const label = EXAM_LABELS[selectedExam];
    return label ? `${label} — ${skillName}` : skillName;
  };

  // Filter skill profiles and history by selected exam
  const allowedSkillCodes = EXAM_SKILL_MAP[selectedExam] ?? [];
  const filteredProfiles = dashboard.skillProfiles.filter((s) => allowedSkillCodes.includes(s.skillCode));
  const filteredHistory = dashboard.skillHistory.filter((p) => allowedSkillCodes.includes(p.skillCode));

  // Prepare line chart data — pivot skill history into { date, skill1, skill2, ... }
  const lineChartData = (() => {
    const byDate: Record<string, Record<string, number>> = {};
    filteredHistory.forEach((p) => {
      const d = new Date(p.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
      if (!byDate[d]) byDate[d] = {};
      const label = getSkillLabel(p.skillName, p.skillCode);
      byDate[d][label] = p.level;
    });
    return Object.entries(byDate).map(([date, skills]) => ({ date, ...skills }));
  })();

  const uniqueSkills = [...new Set(filteredHistory.map((p) => getSkillLabel(p.skillName, p.skillCode)))];

  // Radar chart data
  const radarData = filteredProfiles.map((s) => ({
    skill: getSkillLabel(s.skillName, s.skillCode),
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
        {t.progress.title}
      </h1>

      {/* Stats Overview */}
      <div className="stats-grid">
        <div className="card stat-card animate-fade-in-up" style={{ animationDelay: '0.1s' }}>
          <div className="stat-value">{dashboard.totalQuestionsAnswered}</div>
          <div className="stat-label">{t.progress.questionsAnswered}</div>
        </div>
        <div className="card stat-card animate-fade-in-up" style={{ animationDelay: '0.2s' }}>
          <div className="stat-value">{dashboard.correctAnswers}</div>
          <div className="stat-label">{t.progress.correctAnswers}</div>
        </div>
        <div className="card stat-card animate-fade-in-up" style={{ animationDelay: '0.3s' }}>
          <div className="stat-value">{dashboard.overallAccuracy}%</div>
          <div className="stat-label">{t.progress.accuracy}</div>
        </div>
        <div className="card stat-card animate-fade-in-up" style={{ animationDelay: '0.4s' }}>
          <div className="stat-value" style={{ display: 'flex', alignItems: 'center', gap: '0.25rem', justifyContent: 'center' }}>
            {dashboard.currentStreak}
          </div>
          <div className="stat-label">{t.progress.dayStreak} ({t.progress.best}: {dashboard.bestStreak})</div>
        </div>
      </div>

      {/* Exam Filter Tabs */}
      <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginTop: '2rem' }}>
        {Object.keys(EXAM_LABELS).map((code) => (
          <button
            key={code}
            className={selectedExam === code ? 'btn btn-primary' : 'btn btn-secondary'}
            style={{ padding: '0.4rem 0.9rem', fontSize: '0.85rem' }}
            onClick={() => setSelectedExam(code)}
          >
            {EXAM_LABELS[code]}
          </button>
        ))}
      </div>

      {/* Pro-gated analytics charts */}
      <ProGate hasAccess={hasFullAnalytics} featureName={t.progress.skillProfile}>
      <>
      {/* Skill Levels — Radar Chart (or Bar Chart for ≤2 skills) */}
      {radarData.length > 0 && (
        <div className="card card-static animate-fade-in-up" style={{ marginTop: '2rem', animationDelay: '0.5s' }}>
          <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1rem' }}>
            {t.progress.skillProfile}
          </h2>
          {radarData.length <= 2 ? (
            <ResponsiveContainer width="100%" height={160}>
              <BarChart data={radarData} layout="vertical" barSize={24}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" />
                <XAxis type="number" domain={[0, 100]} tick={{ fill: 'var(--text-secondary)', fontSize: 12 }} />
                <YAxis type="category" dataKey="skill" width={180} tick={{ fill: 'var(--text-secondary)', fontSize: 12 }} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: 'var(--card-background)',
                    border: '1px solid var(--border-color)',
                    borderRadius: '0.5rem',
                  }}
                  itemStyle={{ color: 'var(--text-primary)' }}
                  labelStyle={{ color: 'var(--text-primary)', fontWeight: 600 }}
                />
                <Bar dataKey="level" name={t.progress.skillLevels} fill="#6366f1" radius={[0, 6, 6, 0]} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <ResponsiveContainer width="100%" height={380}>
              <RadarChart data={radarData} cx="50%" cy="50%" outerRadius="70%">
                <PolarGrid stroke="var(--text-secondary)" strokeOpacity={0.4} />
                <PolarAngleAxis dataKey="skill" tick={{ fill: 'var(--text-secondary)', fontSize: 13 }} />
                <PolarRadiusAxis angle={30} domain={[0, 100]} tick={{ fill: 'var(--text-secondary)', fontSize: 10 }} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: 'var(--card-background)',
                    border: '1px solid var(--border-color)',
                    borderRadius: '0.5rem',
                  }}
                  itemStyle={{ color: 'var(--text-primary)' }}
                  labelStyle={{ color: 'var(--text-primary)', fontWeight: 600 }}
                />
                <Radar name={t.progress.skillLevels} dataKey="level" stroke="#6366f1" fill="#6366f1" fillOpacity={0.3} />
              </RadarChart>
            </ResponsiveContainer>
          )}
        </div>
      )}

      {/* Skill Progress Over Time — Line Chart */}
      {lineChartData.length > 1 && (
        <div className="card card-static animate-fade-in-up" style={{ marginTop: '2rem', animationDelay: '0.6s' }}>
          <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1rem' }}>
            {t.progress.skillProgressOverTime}
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
                }}
                itemStyle={{ color: 'var(--text-primary)' }}
                labelStyle={{ color: 'var(--text-primary)', fontWeight: 600 }}
              />
              <Legend wrapperStyle={{ color: 'var(--text-primary)' }} />
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
            {t.progress.accuracyByDifficulty}
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
                }}
                itemStyle={{ color: 'var(--text-primary)' }}
                labelStyle={{ color: 'var(--text-primary)', fontWeight: 600 }}
              />
              <Legend wrapperStyle={{ color: 'var(--text-primary)' }} />
              <Bar dataKey="correctCount" name={t.progress.correct} stackId="a" fill="#10b981" radius={[0, 0, 0, 0]} />
              <Bar dataKey="incorrectCount" name={t.progress.incorrect} stackId="a" fill="#f87171" radius={[4, 4, 0, 0]} />
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
          <span>{t.progress.less}</span>
          {heatmapColors.map((c, i) => (
            <div key={i} style={{ width: '12px', height: '12px', borderRadius: '2px', backgroundColor: c }} />
          ))}
          <span>{t.progress.more}</span>
        </div>
      </div>
      </>
      </ProGate>

      {/* Skill Bars (detailed, with confidence intervals) */}
      <div className="card card-static animate-fade-in-up" style={{ marginTop: '2rem', animationDelay: '0.9s' }}>
        <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1.5rem' }}>
            {t.progress.skillLevels}
          </h2>
        {filteredProfiles.length === 0 ? (
          <p style={{ color: 'var(--text-secondary)' }}>
            {t.progress.noSkillData}
          </p>
        ) : (
          <div className="skills-list">
            {filteredProfiles.map((skill, index) => {
              const hasConfidence = skill.confidenceLow != null && skill.confidenceHigh != null;
              return (
                <div
                  key={skill.skillId}
                  className="skill-item animate-slide-in"
                  style={{ animationDelay: `${1.0 + index * 0.1}s` }}
                >
                  <span className="skill-name">{getSkillLabel(skill.skillName, skill.skillCode)}</span>
                  <div className="skill-bar" style={{ position: 'relative' }}>
                    {/* Confidence interval band */}
                    {hasConfidence && (
                      <div
                        style={{
                          position: 'absolute',
                          left: `${skill.confidenceLow}%`,
                          width: `${Math.max(0, (skill.confidenceHigh ?? 0) - (skill.confidenceLow ?? 0))}%`,
                          height: '100%',
                          backgroundColor: getSkillLevelColor(skill.level),
                          opacity: 0.15,
                          borderRadius: '0.5rem',
                          zIndex: 0,
                        }}
                        title={`95% confidence: ${skill.confidenceLow}–${skill.confidenceHigh}`}
                      />
                    )}
                    <div
                      className="skill-bar-fill"
                      style={{
                        width: `${skill.level}%`,
                        backgroundColor: getSkillLevelColor(skill.level),
                        position: 'relative',
                        zIndex: 1,
                      }}
                    />
                  </div>
                  <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', minWidth: '3.5rem' }}>
                    <span className="skill-level" style={{ color: getSkillLevelColor(skill.level) }}>
                      {skill.level}
                    </span>
                    {hasConfidence && (
                      <span style={{ fontSize: '0.65rem', color: 'var(--text-secondary)', whiteSpace: 'nowrap' }}>
                        ±{Math.round(((skill.confidenceHigh ?? 0) - (skill.confidenceLow ?? 0)) / 2)}
                      </span>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* Legend */}
      <div className="card" style={{ marginTop: '2rem' }}>
        <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1rem' }}>
          {t.progress.understanding}
        </h2>
        <div style={{ display: 'flex', gap: '2rem', flexWrap: 'wrap' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div style={{ width: '1rem', height: '1rem', borderRadius: '50%', backgroundColor: 'var(--error-color)' }} />
            <span style={{ fontSize: '0.875rem' }}>0-39: {t.progress.beginner}</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div style={{ width: '1rem', height: '1rem', borderRadius: '50%', backgroundColor: 'var(--warning-color)' }} />
            <span style={{ fontSize: '0.875rem' }}>40-69: {t.progress.intermediate}</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <div style={{ width: '1rem', height: '1rem', borderRadius: '50%', backgroundColor: 'var(--success-color)' }} />
            <span style={{ fontSize: '0.875rem' }}>70-100: {t.progress.advanced}</span>
          </div>
        </div>
        <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.75rem', lineHeight: '1.4' }}>
          {t.progress.irtExplanation}
        </p>
      </div>
    </div>
  );
}

export default AnalyticsPage;
