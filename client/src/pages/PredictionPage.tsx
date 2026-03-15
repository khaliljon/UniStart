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
  Line,
  Area,
  AreaChart,
} from 'recharts';
import { predictionService } from '../services/predictionService';
import { examService } from '../services/examService';
import { testService } from '../services/testService';
import { subscriptionService } from '../services/subscriptionService';
import { useTranslation } from '../i18n';
import { useAppSelector } from '../hooks/useAppSelector';
import { ProGate } from '../components/ProGate';
import type {
  ScorePrediction,
  SectionPrediction,
  PredictionHistory,
  WhatIfResult,
  ExamType,
  TopicProgress,
} from '../types';

const STRENGTH_COLORS: Record<string, { color: string; emoji: string }> = {
  strong: { color: '#10b981', emoji: '' },
  average: { color: '#f59e0b', emoji: '' },
  weak: { color: '#ef4444', emoji: '' },
  critical: { color: '#dc2626', emoji: '' },
};

function PredictionPage() {
  const { t } = useTranslation();

  const STRENGTH_CONFIG: Record<string, { label: string; color: string; emoji: string }> = {
    strong: { label: t.prediction.strong, ...STRENGTH_COLORS.strong },
    average: { label: t.prediction.average, ...STRENGTH_COLORS.average },
    weak: { label: t.prediction.weak, ...STRENGTH_COLORS.weak },
    critical: { label: t.prediction.critical, ...STRENGTH_COLORS.critical },
  };
  const { selectedExams: userExams } = useAppSelector((state) => state.exam);
  const [exams, setExams] = useState<ExamType[]>([]);
  const [selectedExam, setSelectedExam] = useState<string>('');
  const [selectedSectionIds, setSelectedSectionIds] = useState<number[]>([]);
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
  const [hasPredictionAccess, setHasPredictionAccess] = useState(true);

  // Load exams on mount
  useEffect(() => {
    (async () => {
      try {
        const data = await examService.getExams();
        setExams(data);
        if (data.length > 0) {
          const userMatch = data.find(e => userExams.includes(e.code));
          setSelectedExam(userMatch ? userMatch.code : data[0].code);
        }
      } catch {
        setError(t.prediction.examLoadError);
      } finally {
        setIsLoading(false);
      }
    })();
    subscriptionService.getStatus().then((s) => {
      setHasPredictionAccess(s.isPro || s.isTrial || s.limits.realtimePrediction);
    }).catch(() => {});
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
      setSelectedSectionIds([]);
      setWhatIfResult(null);
      setWhatIfTopic(topicsData.length > 0 ? topicsData[0].topicId : null);
    } catch (err) {
      console.error(err);
      setError(t.prediction.loadError);
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
            {t.prediction.loading}
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
            {t.prediction.tryAgain}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ padding: '1.5rem 0' }}>
      {/* Header with exam selector */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '1rem' }}>
        <h1 style={{ margin: 0 }}>{t.prediction.title}</h1>
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
        <ProGate hasAccess={hasPredictionAccess} featureName={t.prediction.title}>
        <>
          {/* ─── Section Filter (for multi-section exams) ─── */}
          {prediction.sections.length > 1 && (
            <div style={{ display: 'flex', gap: '0.4rem', flexWrap: 'wrap', marginBottom: '1rem' }}>
              <button
                className={selectedSectionIds.length === 0 ? 'btn btn-primary' : 'btn btn-secondary'}
                style={{ padding: '0.3rem 0.8rem', fontSize: '0.8rem' }}
                onClick={() => setSelectedSectionIds([])}
              >
                {t.prediction.allSections}
              </button>
              {prediction.sections.map((s) => (
                <button
                  key={s.sectionId}
                  className={selectedSectionIds.includes(s.sectionId) ? 'btn btn-primary' : 'btn btn-secondary'}
                  style={{ padding: '0.3rem 0.8rem', fontSize: '0.8rem' }}
                  onClick={() => setSelectedSectionIds((prev) => {
                    if (prev.includes(s.sectionId)) {
                      const next = prev.filter((id) => id !== s.sectionId);
                      return next;
                    }
                    return [...prev, s.sectionId];
                  })}
                >
                  {s.sectionName}
                </button>
              ))}
            </div>
          )}

          {/* ─── Main Score Card ─── */}
          {(() => {
            const filtered = selectedSectionIds.length > 0
              ? prediction.sections.filter((s) => selectedSectionIds.includes(s.sectionId))
              : prediction.sections;
            const totalPredicted = filtered.reduce((sum, s) => sum + s.predictedScore, 0);
            const totalMin = filtered.reduce((sum, s) => sum + s.minScore, 0);
            const totalMax = filtered.reduce((sum, s) => sum + s.maxScore, 0);
            const totalConfLow = filtered.reduce((sum, s) => sum + s.confidenceLow, 0);
            const totalConfHigh = filtered.reduce((sum, s) => sum + s.confidenceHigh, 0);
            const scaledTarget = selectedSectionIds.length > 0 && prediction.targetScore != null
              ? Math.round(prediction.targetScore * (totalMax / prediction.maxPossibleScore))
              : prediction.targetScore;
            const gap = scaledTarget != null ? Math.max(0, scaledTarget - totalPredicted) : prediction.gapToTarget;
            const displayPrediction: ScorePrediction = {
              ...prediction,
              predictedScore: totalPredicted,
              minPossibleScore: totalMin,
              maxPossibleScore: totalMax,
              confidenceLow: totalConfLow,
              confidenceHigh: totalConfHigh,
              targetScore: scaledTarget ?? null,
              gapToTarget: gap ?? null,
              sections: filtered,
            };
            return <ScoreCard prediction={displayPrediction} />;
          })()}

          {/* ─── Section Breakdown ─── */}
          <div style={{ marginTop: '1rem' }}>
            <SectionsBarChart sections={prediction.sections} />
          </div>

          {/* ─── Section Details ─── */}
          <SectionDetails sections={prediction.sections} strengthConfig={STRENGTH_CONFIG} />

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
        </ProGate>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════
//  SCORE CARD
// ═══════════════════════════════════════════════════════════

function ScoreCard({ prediction }: { prediction: ScorePrediction }) {
  const { t } = useTranslation();
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
            {t.prediction.ofMax} {prediction.maxPossibleScore}
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
                  {prediction.targetScore}
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
              {prediction.gapToTarget === 0 ? '✓' : `−${prediction.gapToTarget}`}
            </div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
              {prediction.gapToTarget === 0 ? t.prediction.targetReached : t.prediction.toTarget}
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
  const { t } = useTranslation();
  const data = sections.map((s) => ({
    name: s.sectionName.length > 18 ? s.sectionName.slice(0, 17) + '…' : s.sectionName,
    fullName: s.sectionName,
    score: s.predictedScore,
    max: s.maxScore - s.predictedScore,
    maxScore: s.maxScore,
    confLow: s.confidenceLow,
    confHigh: s.confidenceHigh,
  }));

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const renderTooltip = ({ active, payload }: any) => {
    if (!active || !payload?.length) return null;
    const d = payload[0].payload;
    return (
      <div style={{
        background: 'var(--card-bg)', border: '1px solid var(--border-color)',
        borderRadius: '8px', padding: '0.5rem 0.75rem', whiteSpace: 'nowrap',
      }}>
        <div style={{ fontWeight: 600, color: 'var(--text-primary)', marginBottom: '0.25rem' }}>
          {d.fullName}
        </div>
        <div style={{ color: '#6366f1' }}>{t.prediction.predicted} : {d.score}</div>
        <div style={{ color: 'var(--text-primary)' }}>{t.prediction.toMax} : {d.max}</div>
      </div>
    );
  };

  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.25rem' }}>
      <h3 style={{ margin: '0 0 1rem' }}>{t.prediction.sectionForecast}</h3>
      <ResponsiveContainer width="100%" height={Math.max(180, data.length * 44)}>
        <BarChart data={data} layout="vertical">
          <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" />
          <XAxis type="number" tick={{ fill: 'var(--text-secondary)', fontSize: 11 }} />
          <YAxis type="category" dataKey="name" width={140} tick={{ fill: 'var(--text-secondary)', fontSize: 11 }} />
          <Tooltip content={renderTooltip} />
          <Bar dataKey="score" name={t.prediction.predicted} stackId="a" fill="#6366f1" radius={[0, 4, 4, 0]} />
          <Bar dataKey="max" name={t.prediction.toMax} stackId="a" fill="var(--border-color)" radius={[0, 4, 4, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════
//  SECTION DETAIL CARDS
// ═══════════════════════════════════════════════════════════

function SectionDetails({ sections, strengthConfig }: { sections: SectionPrediction[]; strengthConfig: Record<string, { label: string; color: string; emoji: string }> }) {
  const { t } = useTranslation();
  return (
    <div style={{ marginTop: '1rem' }}>
      <h3 style={{ marginBottom: '0.75rem' }}>{t.prediction.sectionDetails}</h3>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '0.75rem' }}>
        {sections.map((s) => {
          const cfg = strengthConfig[s.strength] || strengthConfig.average;
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
                  {t.prediction.accuracyLabel}: {s.accuracy}%
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
  const navigate = useNavigate();
  const { t } = useTranslation();
  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.25rem', marginTop: '1rem' }}>
      <h3 style={{ margin: '0 0 1rem' }}>{t.prediction.improvementTips}</h3>
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
                {t.prediction.currentLevel}: {tip.currentLevel}% | theta = {tip.currentTheta}
              </div>
            </div>
            {tip.topicId > 0 && (
              <button
                className="btn btn-primary"
                style={{ padding: '0.35rem 0.75rem', fontSize: '0.78rem', whiteSpace: 'nowrap' }}
                onClick={() => navigate(`/learn?tab=practice&topicId=${tip.topicId}`)}
              >
                {t.prediction.practice}
              </button>
            )}
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
  const { t } = useTranslation();
  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.25rem', marginTop: '1rem' }}>
      <h3 style={{ margin: '0 0 1rem' }}>{t.prediction.whatIf}</h3>
      <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '1rem' }}>
        {t.prediction.whatIfDesc}
      </p>

      <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
        <div style={{ flex: '1 1 200px' }}>
          <label style={{ display: 'block', marginBottom: '0.3rem', fontWeight: 600, fontSize: '0.85rem' }}>
            {t.prediction.topic}
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
                {t.topicName} ({t.irtLevel ?? t.masteryPercentage}%)
              </option>
            ))}
          </select>
        </div>

        <div style={{ flex: '0 0 160px' }}>
          <label style={{ display: 'block', marginBottom: '0.3rem', fontWeight: 600, fontSize: '0.85rem' }}>
            {t.prediction.level}: {whatIfLevel}%
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
          {whatIfLoading ? '...' : t.prediction.calculate}
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
            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{t.prediction.topic}</div>
            <div style={{ fontWeight: 600 }}>{whatIfResult.topicName}</div>
          </div>
          <div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{t.prediction.level}</div>
            <div style={{ fontWeight: 600 }}>
              {whatIfResult.currentLevel}% → {whatIfResult.improvedLevel}%
            </div>
          </div>
          <div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{t.prediction.forecast}</div>
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
  const { t, dateLocale } = useTranslation();
  const data = history.map((h) => ({
    date: new Date(h.date).toLocaleDateString(dateLocale, { day: 'numeric', month: 'short' }),
    score: h.predictedScore,
    low: h.confidenceLow,
    high: h.confidenceHigh,
    target: prediction.targetScore ?? undefined,
  }));

  return (
    <div className="card card-static animate-fade-in-up" style={{ padding: '1.25rem', marginTop: '1rem' }}>
      <h3 style={{ margin: '0 0 1rem' }}>{t.prediction.forecastHistory}</h3>
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
          <Area type="monotone" dataKey="high" stackId="ci" stroke="none" fill="#6366f1" fillOpacity={0.1} name={t.prediction.upperBound} />
          <Area type="monotone" dataKey="low" stackId="ci" stroke="none" fill="#fff" fillOpacity={0} name={t.prediction.lowerBound} />
          {/* Main prediction line */}
          <Line type="monotone" dataKey="score" stroke="#6366f1" strokeWidth={2.5} dot={{ r: 3 }} name={t.prediction.predicted} />
          {/* Target line */}
          {prediction.targetScore && (
            <Line type="monotone" dataKey="target" stroke="#ef4444" strokeWidth={1.5} strokeDasharray="5 5" dot={false} name={t.prediction.target} />
          )}
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}

export default PredictionPage;
