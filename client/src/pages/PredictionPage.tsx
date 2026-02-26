import { useEffect, useState, useCallback } from 'react';
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Line,
  Area,
  AreaChart,
  RadarChart,
  PolarGrid,
  PolarAngleAxis,
  PolarRadiusAxis,
  Radar,
} from 'recharts';
import { predictionService } from '../services/predictionService';
import { examService } from '../services/examService';
import { testService } from '../services/testService';
import type {
  ScorePrediction,
  SectionPrediction,
  PredictionHistory,
  WhatIfResult,
  ExamType,
  TopicProgress,
} from '../types';

const STRENGTH_CONFIG: Record<string, { label: string; color: string; emoji: string }> = {
  strong: { label: 'Сильная', color: '#10b981', emoji: '💪' },
  average: { label: 'Средняя', color: '#f59e0b', emoji: '📊' },
  weak: { label: 'Слабая', color: '#ef4444', emoji: '⚠️' },
  critical: { label: 'Критическая', color: '#dc2626', emoji: '🚨' },
};

function PredictionPage() {
  const [exams, setExams] = useState<ExamType[]>([]);
  const [selectedExam, setSelectedExam] = useState<string>('');
  const [prediction, setPrediction] = useState<ScorePrediction | null>(null);
  const [history, setHistory] = useState<PredictionHistory[]>([]);
  const [topics, setTopics] = useState<TopicProgress[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // What-if state
  const [whatIfTopic, setWhatIfTopic] = useState<number | null>(null);
  const [whatIfLevel, setWhatIfLevel] = useState(80);
  const [whatIfResult, setWhatIfResult] = useState<WhatIfResult | null>(null);
  const [whatIfLoading, setWhatIfLoading] = useState(false);

  // Load exams on mount
  useEffect(() => {
    (async () => {
      try {
        const data = await examService.getExams();
        setExams(data);
        if (data.length > 0) {
          setSelectedExam(data[0].code);
        }
      } catch {
        setError('Ошибка загрузки экзаменов');
      } finally {
        setIsLoading(false);
      }
    })();
  }, []);

  // Load prediction when exam changes
  const loadPrediction = useCallback(async () => {
    if (!selectedExam) return;
    try {
      setIsLoading(true);
      setError(null);
      const [pred, hist, topicsData] = await Promise.all([
        predictionService.getPrediction(selectedExam),
        predictionService.getHistory(selectedExam, 60),
        testService.getTopicsWithProgress([selectedExam]),
      ]);
      setPrediction(pred);
      setHistory(hist);
      setTopics(topicsData);
      setWhatIfResult(null);
      setWhatIfTopic(topicsData.length > 0 ? topicsData[0].topicId : null);
    } catch (err) {
      console.error(err);
      setError('Ошибка загрузки прогноза');
    } finally {
      setIsLoading(false);
    }
  }, [selectedExam]);

  useEffect(() => {
    loadPrediction();
  }, [loadPrediction]);

  // What-if handler
  const handleWhatIf = async () => {
    if (!whatIfTopic || !selectedExam) return;
    try {
      setWhatIfLoading(true);
      const result = await predictionService.whatIf({
        examTypeCode: selectedExam,
        topicId: whatIfTopic,
        improvedLevel: whatIfLevel,
      });
      setWhatIfResult(result);
    } catch (err) {
      console.error(err);
    } finally {
      setWhatIfLoading(false);
    }
  };

  // ─── Loading / Error ────────────────────────────────────

  if (isLoading && !prediction) {
    return (
      <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
        <div className="card" style={{ padding: '3rem', textAlign: 'center' }}>
          <div className="animate-pulse" style={{ fontSize: '1.2rem', color: 'var(--text-secondary)' }}>
            Загрузка прогноза...
          </div>
        </div>
      </div>
    );
  }

  if (error && !prediction) {
    return (
      <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
        <div className="card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--error-color)' }}>
          {error}
          <button className="btn btn-primary" style={{ marginTop: '1rem' }} onClick={loadPrediction}>
            Попробовать снова
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ padding: '1.5rem 0' }}>
      {/* Header with exam selector */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '1rem' }}>
        <h1 style={{ margin: 0 }}>🔮 Прогноз результата</h1>
        <select
          value={selectedExam}
          onChange={(e) => setSelectedExam(e.target.value)}
          style={{
            padding: '0.5rem 1rem', borderRadius: '8px',
            border: '1px solid var(--border-color)', background: 'var(--card-bg)',
            color: 'var(--text-primary)', fontSize: '0.95rem', fontWeight: 600,
          }}
        >
          {exams.map((e) => (
            <option key={e.code} value={e.code}>{e.name}</option>
          ))}
        </select>
      </div>

      {prediction && (
        <>
          {/* ─── Main Score Card ─── */}
          <ScoreCard prediction={prediction} />

          {/* ─── Section Breakdown ─── */}
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginTop: '1rem' }}>
            <SectionsBarChart sections={prediction.sections} />
            <SectionsRadar sections={prediction.sections} />
          </div>

          {/* ─── Section Details ─── */}
          <SectionDetails sections={prediction.sections} />

          {/* ─── Improvement Tips ─── */}
          {prediction.improvementTips.length > 0 && (
            <ImprovementTips tips={prediction.improvementTips} />
          )}

          {/* ─── What-If Scenario ─── */}
          <WhatIfSection
            topics={topics}
            whatIfTopic={whatIfTopic}
            whatIfLevel={whatIfLevel}
            whatIfResult={whatIfResult}
            whatIfLoading={whatIfLoading}
            onChangeTopic={setWhatIfTopic}
            onChangeLevel={setWhatIfLevel}
            onCalculate={handleWhatIf}
          />

          {/* ─── History Chart ─── */}
          {history.length > 1 && <HistoryChart history={history} prediction={prediction} />}
        </>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════
//  SCORE CARD
// ═══════════════════════════════════════════════════════════

function ScoreCard({ prediction }: { prediction: ScorePrediction }) {
  const scorePercent = ((prediction.predictedScore - prediction.minPossibleScore) /
    (prediction.maxPossibleScore - prediction.minPossibleScore)) * 100;

  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.5rem' }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '2rem', flexWrap: 'wrap' }}>
        {/* Big Score */}
        <div style={{ textAlign: 'center', minWidth: '160px' }}>
          <div style={{ fontSize: '3.5rem', fontWeight: 800, color: 'var(--primary-color)', lineHeight: 1 }}>
            {prediction.predictedScore}
          </div>
          <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
            из {prediction.maxPossibleScore}
          </div>
          <div style={{
            fontSize: '0.8rem', marginTop: '0.5rem', padding: '0.2rem 0.8rem',
            borderRadius: '12px', display: 'inline-block',
            background: 'var(--primary-bg)', color: 'var(--primary-color)', fontWeight: 600,
          }}>
            {prediction.confidencePercent}% CI: {prediction.confidenceLow} – {prediction.confidenceHigh}
          </div>
        </div>

        {/* Score Bar */}
        <div style={{ flex: 1, minWidth: '200px' }}>
          <div style={{
            display: 'flex', justifyContent: 'space-between', marginBottom: '0.3rem',
            fontSize: '0.8rem', color: 'var(--text-secondary)',
          }}>
            <span>{prediction.minPossibleScore}</span>
            <span>{prediction.maxPossibleScore}</span>
          </div>
          <div style={{
            background: 'var(--border-color)', borderRadius: '10px', height: '18px',
            overflow: 'hidden', position: 'relative',
          }}>
            {/* Confidence interval band */}
            <div style={{
              position: 'absolute',
              left: `${((prediction.confidenceLow - prediction.minPossibleScore) / (prediction.maxPossibleScore - prediction.minPossibleScore)) * 100}%`,
              width: `${((prediction.confidenceHigh - prediction.confidenceLow) / (prediction.maxPossibleScore - prediction.minPossibleScore)) * 100}%`,
              height: '100%',
              background: 'var(--primary-color)',
              opacity: 0.2,
              borderRadius: '10px',
            }} />
            {/* Score position */}
            <div style={{
              width: `${scorePercent}%`, height: '100%',
              background: 'var(--primary-color)', borderRadius: '10px',
              transition: 'width 1s ease',
            }} />
            {/* Target marker */}
            {prediction.targetScore && (
              <div style={{
                position: 'absolute', top: 0, bottom: 0,
                left: `${((prediction.targetScore - prediction.minPossibleScore) / (prediction.maxPossibleScore - prediction.minPossibleScore)) * 100}%`,
                width: '3px', background: 'var(--error-color)', borderRadius: '2px',
              }}>
                <div style={{
                  position: 'absolute', top: '-18px', left: '50%', transform: 'translateX(-50%)',
                  fontSize: '0.65rem', color: 'var(--error-color)', fontWeight: 700, whiteSpace: 'nowrap',
                }}>
                  🎯 {prediction.targetScore}
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Target Gap */}
        {prediction.targetScore && prediction.gapToTarget != null && (
          <div style={{ textAlign: 'center', minWidth: '120px' }}>
            <div style={{
              fontSize: '2rem', fontWeight: 700,
              color: prediction.gapToTarget === 0 ? 'var(--success-color)' : 'var(--warning-color)',
            }}>
              {prediction.gapToTarget === 0 ? '✅' : `−${prediction.gapToTarget}`}
            </div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
              {prediction.gapToTarget === 0 ? 'Цель достигнута!' : 'до цели'}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════
//  SECTION CHARTS
// ═══════════════════════════════════════════════════════════

function SectionsBarChart({ sections }: { sections: SectionPrediction[] }) {
  const data = sections.map((s) => ({
    name: s.sectionName.length > 15 ? s.sectionName.slice(0, 14) + '…' : s.sectionName,
    score: s.predictedScore,
    max: s.maxScore - s.predictedScore,
    confLow: s.confidenceLow,
    confHigh: s.confidenceHigh,
  }));

  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.25rem' }}>
      <h3 style={{ margin: '0 0 1rem' }}>📊 Прогноз по секциям</h3>
      <ResponsiveContainer width="100%" height={220}>
        <BarChart data={data} layout="vertical">
          <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" />
          <XAxis type="number" tick={{ fill: 'var(--text-secondary)', fontSize: 11 }} />
          <YAxis type="category" dataKey="name" width={110} tick={{ fill: 'var(--text-secondary)', fontSize: 11 }} />
          <Tooltip
            contentStyle={{
              background: 'var(--card-bg)', border: '1px solid var(--border-color)',
              borderRadius: '8px', color: 'var(--text-primary)',
            }}
          />
          <Bar dataKey="score" name="Прогноз" stackId="a" fill="#6366f1" radius={[0, 4, 4, 0]} />
          <Bar dataKey="max" name="До макс." stackId="a" fill="var(--border-color)" radius={[0, 4, 4, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}

function SectionsRadar({ sections }: { sections: SectionPrediction[] }) {
  const radarData = sections.map((s) => ({
    section: s.sectionName.length > 12 ? s.sectionName.slice(0, 11) + '…' : s.sectionName,
    level: Math.round(((s.predictedScore - s.minScore) / (s.maxScore - s.minScore)) * 100),
    fullMark: 100,
  }));

  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.25rem' }}>
      <h3 style={{ margin: '0 0 1rem' }}>🎯 Профиль по секциям</h3>
      <ResponsiveContainer width="100%" height={220}>
        <RadarChart data={radarData}>
          <PolarGrid stroke="var(--border-color)" />
          <PolarAngleAxis dataKey="section" tick={{ fill: 'var(--text-secondary)', fontSize: 10 }} />
          <PolarRadiusAxis angle={90} domain={[0, 100]} tick={{ fontSize: 9 }} />
          <Radar name="Уровень" dataKey="level" stroke="#6366f1" fill="#6366f1" fillOpacity={0.3} />
        </RadarChart>
      </ResponsiveContainer>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════
//  SECTION DETAIL CARDS
// ═══════════════════════════════════════════════════════════

function SectionDetails({ sections }: { sections: SectionPrediction[] }) {
  return (
    <div style={{ marginTop: '1rem' }}>
      <h3 style={{ marginBottom: '0.75rem' }}>📋 Детали по секциям</h3>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '0.75rem' }}>
        {sections.map((s) => {
          const cfg = STRENGTH_CONFIG[s.strength] || STRENGTH_CONFIG.average;
          const percent = ((s.predictedScore - s.minScore) / (s.maxScore - s.minScore)) * 100;

          return (
            <div key={s.sectionId} className="card card-static animate-fade-in-up" style={{
              padding: '1rem', borderLeft: `4px solid ${cfg.color}`,
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                <div style={{ fontWeight: 600 }}>{s.sectionName}</div>
                <span style={{
                  fontSize: '0.72rem', fontWeight: 600, padding: '0.15rem 0.5rem',
                  borderRadius: '8px', background: cfg.color + '20', color: cfg.color,
                }}>
                  {cfg.emoji} {cfg.label}
                </span>
              </div>

              <div style={{ fontSize: '1.8rem', fontWeight: 700, color: cfg.color }}>
                {s.predictedScore}
                <span style={{ fontSize: '0.85rem', fontWeight: 400, color: 'var(--text-secondary)' }}>
                  {' '}/ {s.maxScore}
                </span>
              </div>

              <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', marginTop: '0.3rem' }}>
                CI: {s.confidenceLow} – {s.confidenceHigh} • θ = {s.theta} (SE {s.thetaSE})
              </div>

              {/* Mini progress bar */}
              <div style={{
                marginTop: '0.5rem', background: 'var(--border-color)',
                borderRadius: '6px', height: '6px', overflow: 'hidden',
              }}>
                <div style={{
                  width: `${percent}%`, height: '100%',
                  background: cfg.color, borderRadius: '6px',
                  transition: 'width 0.8s ease',
                }} />
              </div>

              {s.accuracy > 0 && (
                <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '0.3rem' }}>
                  Точность ответов: {s.accuracy}%
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════
//  IMPROVEMENT TIPS
// ═══════════════════════════════════════════════════════════

function ImprovementTips({ tips }: { tips: ScorePrediction['improvementTips'] }) {
  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.25rem', marginTop: '1rem' }}>
      <h3 style={{ margin: '0 0 1rem' }}>💡 Рекомендации по улучшению</h3>
      <div style={{ display: 'flex', flexDirection: 'column', gap: '0.6rem' }}>
        {tips.map((tip, i) => (
          <div key={i} style={{
            display: 'flex', alignItems: 'center', gap: '1rem', padding: '0.75rem',
            background: 'var(--primary-bg)', borderRadius: '10px',
          }}>
            <div style={{
              minWidth: '48px', height: '48px', borderRadius: '12px',
              background: 'var(--primary-color)', color: '#fff',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              fontWeight: 700, fontSize: '1.1rem',
            }}>
              +{tip.potentialScoreGain}
            </div>
            <div style={{ flex: 1 }}>
              <div style={{ fontWeight: 600, marginBottom: '0.2rem' }}>
                {tip.topicName}
                <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginLeft: '0.5rem' }}>
                  ({tip.sectionName})
                </span>
              </div>
              <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
                {tip.recommendation}
              </div>
              <div style={{ fontSize: '0.72rem', color: 'var(--text-secondary)', marginTop: '0.2rem' }}>
                Текущий уровень: {tip.currentLevel}% • θ = {tip.currentTheta}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════
//  WHAT-IF SCENARIO
// ═══════════════════════════════════════════════════════════

function WhatIfSection({
  topics, whatIfTopic, whatIfLevel, whatIfResult, whatIfLoading,
  onChangeTopic, onChangeLevel, onCalculate,
}: {
  topics: TopicProgress[];
  whatIfTopic: number | null;
  whatIfLevel: number;
  whatIfResult: WhatIfResult | null;
  whatIfLoading: boolean;
  onChangeTopic: (id: number | null) => void;
  onChangeLevel: (level: number) => void;
  onCalculate: () => void;
}) {
  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.25rem', marginTop: '1rem' }}>
      <h3 style={{ margin: '0 0 1rem' }}>🧪 Что если...?</h3>
      <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '1rem' }}>
        Узнайте, как улучшение конкретной темы повлияет на прогнозируемый балл
      </p>

      <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
        <div style={{ flex: '1 1 200px' }}>
          <label style={{ display: 'block', marginBottom: '0.3rem', fontWeight: 600, fontSize: '0.85rem' }}>
            Тема
          </label>
          <select
            value={whatIfTopic ?? ''}
            onChange={(e) => onChangeTopic(Number(e.target.value) || null)}
            style={{
              width: '100%', padding: '0.5rem', borderRadius: '8px',
              border: '1px solid var(--border-color)', background: 'var(--card-bg)',
              color: 'var(--text-primary)', fontSize: '0.9rem',
            }}
          >
            {topics.map((t) => (
              <option key={t.topicId} value={t.topicId}>
                {t.topicName} ({t.masteryPercentage}%)
              </option>
            ))}
          </select>
        </div>

        <div style={{ flex: '0 0 160px' }}>
          <label style={{ display: 'block', marginBottom: '0.3rem', fontWeight: 600, fontSize: '0.85rem' }}>
            Уровень: {whatIfLevel}%
          </label>
          <input
            type="range" min="10" max="99" value={whatIfLevel}
            onChange={(e) => onChangeLevel(Number(e.target.value))}
            style={{ width: '100%' }}
          />
        </div>

        <button
          className="btn btn-primary"
          onClick={onCalculate}
          disabled={!whatIfTopic || whatIfLoading}
          style={{ height: '38px' }}
        >
          {whatIfLoading ? '...' : 'Рассчитать'}
        </button>
      </div>

      {/* Result */}
      {whatIfResult && (
        <div style={{
          marginTop: '1rem', padding: '1rem', borderRadius: '10px',
          background: whatIfResult.scoreGain > 0 ? 'rgba(16,185,129,0.08)' : 'var(--primary-bg)',
          display: 'flex', alignItems: 'center', gap: '1.5rem', flexWrap: 'wrap',
        }}>
          <div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>Тема</div>
            <div style={{ fontWeight: 600 }}>{whatIfResult.topicName}</div>
          </div>
          <div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>Уровень</div>
            <div style={{ fontWeight: 600 }}>
              {whatIfResult.currentLevel}% → {whatIfResult.improvedLevel}%
            </div>
          </div>
          <div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>Прогноз</div>
            <div style={{ fontWeight: 600 }}>
              {whatIfResult.currentPredictedTotal} → {whatIfResult.improvedPredictedTotal}
            </div>
          </div>
          <div style={{
            fontSize: '2rem', fontWeight: 800,
            color: whatIfResult.scoreGain > 0 ? 'var(--success-color)' : 'var(--text-secondary)',
          }}>
            {whatIfResult.scoreGain > 0 ? '+' : ''}{whatIfResult.scoreGain}
          </div>
        </div>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════
//  HISTORY CHART
// ═══════════════════════════════════════════════════════════

function HistoryChart({ history, prediction }: { history: PredictionHistory[]; prediction: ScorePrediction }) {
  const data = history.map((h) => ({
    date: new Date(h.date).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' }),
    score: h.predictedScore,
    low: h.confidenceLow,
    high: h.confidenceHigh,
    target: prediction.targetScore ?? undefined,
  }));

  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.25rem', marginTop: '1rem' }}>
      <h3 style={{ margin: '0 0 1rem' }}>📈 История прогноза</h3>
      <ResponsiveContainer width="100%" height={280}>
        <AreaChart data={data}>
          <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" />
          <XAxis dataKey="date" tick={{ fill: 'var(--text-secondary)', fontSize: 11 }} />
          <YAxis
            domain={['auto', 'auto']}
            tick={{ fill: 'var(--text-secondary)', fontSize: 11 }}
          />
          <Tooltip
            contentStyle={{
              background: 'var(--card-bg)', border: '1px solid var(--border-color)',
              borderRadius: '8px', color: 'var(--text-primary)',
            }}
          />
          {/* Confidence band */}
          <Area type="monotone" dataKey="high" stackId="ci" stroke="none" fill="#6366f1" fillOpacity={0.1} name="Верхняя граница" />
          <Area type="monotone" dataKey="low" stackId="ci" stroke="none" fill="#fff" fillOpacity={0} name="Нижняя граница" />
          {/* Main prediction line */}
          <Line type="monotone" dataKey="score" stroke="#6366f1" strokeWidth={2.5} dot={{ r: 3 }} name="Прогноз" />
          {/* Target line */}
          {prediction.targetScore && (
            <Line type="monotone" dataKey="target" stroke="#ef4444" strokeWidth={1.5} strokeDasharray="5 5" dot={false} name="Цель" />
          )}
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}

export default PredictionPage;
