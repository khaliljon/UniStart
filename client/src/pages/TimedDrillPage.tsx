import { useState, useEffect, useRef, useCallback } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { drillService } from '../services/drillService';
import type { DrillQuestion, DrillAnswerResult, DrillResult, PersonalBest, StartDrillRequest } from '../types';
import MathText from '../components/MathRenderer';

type DrillView = 'menu' | 'playing' | 'result' | 'history';

const MARATHON_TIME_LIMIT = 180; // 3 minutes

function TimedDrillPage() {
  const { selectedExams } = useAppSelector((state) => state.exam);
  const [view, setView] = useState<DrillView>('menu');
  const [activeDrill, setActiveDrill] = useState<DrillResult | null>(null);
  const [activeDrillType, setActiveDrillType] = useState<string>('');
  const [question, setQuestion] = useState<DrillQuestion | null>(null);
  const [answerResult, setAnswerResult] = useState<DrillAnswerResult | null>(null);
  const [selectedOption, setSelectedOption] = useState<number | null>(null);
  const [timer, setTimer] = useState(0);
  const [marathonTimeLeft, setMarathonTimeLeft] = useState(MARATHON_TIME_LIMIT);
  const [personalBests, setPersonalBests] = useState<PersonalBest[]>([]);
  const [history, setHistory] = useState<DrillResult[]>([]);
  const [finalResult, setFinalResult] = useState<DrillResult | null>(null);
  const [loading, setLoading] = useState(false);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const marathonTimerRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const activeDrillRef = useRef<DrillResult | null>(null);

  const loadPersonalBests = useCallback(async () => {
    try {
      const data = await drillService.getPersonalBests();
      setPersonalBests(data);
    } catch (err) {
      console.error('Failed to load personal bests:', err);
    }
  }, []);

  useEffect(() => { loadPersonalBests(); }, [loadPersonalBests]);

  useEffect(() => {
    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
      if (marathonTimerRef.current) clearInterval(marathonTimerRef.current);
    };
  }, []);

  // Auto-complete marathon when time runs out
  const completeMarathon = useCallback(async () => {
    if (timerRef.current) clearInterval(timerRef.current);
    if (marathonTimerRef.current) clearInterval(marathonTimerRef.current);
    const drill = activeDrillRef.current;
    if (!drill) return;
    try {
      const final = await drillService.completeDrill(drill.id);
      setFinalResult(final);
      setView('result');
    } catch (err) {
      console.error('Failed to complete marathon:', err);
    }
  }, []);

  const startDrill = async (drillType: 'Speed' | 'Marathon' | 'Streak') => {
    setLoading(true);
    try {
      const request: StartDrillRequest = {
        drillType,
        examTypeCodes: selectedExams,
      };
      const result = await drillService.startDrill(request);
      setActiveDrill(result);
      setActiveDrillType(drillType);
      activeDrillRef.current = result;

      // Start marathon countdown
      if (drillType === 'Marathon') {
        setMarathonTimeLeft(MARATHON_TIME_LIMIT);
        if (marathonTimerRef.current) clearInterval(marathonTimerRef.current);
        marathonTimerRef.current = setInterval(() => {
          setMarathonTimeLeft(prev => {
            if (prev <= 1) {
              completeMarathon();
              return 0;
            }
            return prev - 1;
          });
        }, 1000);
      }

      await loadNextQuestion(result.id);
      setView('playing');
    } catch (err) {
      console.error('Failed to start drill:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadNextQuestion = async (drillId: number) => {
    setAnswerResult(null);
    setSelectedOption(null);
    setTimer(0);

    if (timerRef.current) clearInterval(timerRef.current);
    timerRef.current = setInterval(() => setTimer(t => t + 1), 1000);

    const q = await drillService.getNextQuestion(drillId);
    if (!q) {
      if (timerRef.current) clearInterval(timerRef.current);
      const final = await drillService.completeDrill(drillId);
      setFinalResult(final);
      setView('result');
      return;
    }
    setQuestion(q);
  };

  const handleAnswer = async (optionId: number) => {
    if (!activeDrill || !question || answerResult) return;
    setSelectedOption(optionId);
    if (timerRef.current) clearInterval(timerRef.current);

    try {
      const result = await drillService.submitAnswer({
        drillResultId: activeDrill.id,
        questionId: question.questionId,
        answerOptionId: optionId,
        timeSpentSeconds: timer,
      });
      setAnswerResult(result);

      if (result.drillEnded) {
        setTimeout(async () => {
          const final = await drillService.completeDrill(activeDrill.id);
          setFinalResult(final);
          setView('result');
        }, 1500);
      }
    } catch (err) {
      console.error('Failed to submit answer:', err);
    }
  };

  const handleNext = () => {
    if (activeDrill) loadNextQuestion(activeDrill.id);
  };

  const loadHistory = async () => {
    try {
      const data = await drillService.getHistory();
      setHistory(data);
      setView('history');
    } catch (err) {
      console.error('Failed to load history:', err);
    }
  };

  // Playing view
  if (view === 'playing' && question) {
    return (
      <div className="animate-fade-in">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
            {answerResult ? `${answerResult.totalCorrect} / ${answerResult.totalAnswered}` : '...'}
            {answerResult && answerResult.currentStreak > 1 && ` | Streak: ${answerResult.currentStreak}`}
          </div>
          <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
            {activeDrillType === 'Marathon' && (
              <div style={{
                fontSize: '1.5rem', fontWeight: 700, fontVariantNumeric: 'tabular-nums',
                color: marathonTimeLeft < 30 ? 'var(--error-color)' : marathonTimeLeft < 60 ? 'var(--warning-color)' : 'var(--success-color)'
              }}>
                {Math.floor(marathonTimeLeft / 60)}:{(marathonTimeLeft % 60).toString().padStart(2, '0')}
              </div>
            )}
            <div style={{ fontSize: activeDrillType === 'Marathon' ? '0.85rem' : '1.5rem', fontWeight: 700, fontVariantNumeric: 'tabular-nums', color: 'var(--text-secondary)' }}>
              {timer}s
            </div>
          </div>
        </div>

        <div className="card" style={{ padding: '1.5rem', marginBottom: '1rem' }}>
          {question.topicName && (
            <p style={{ margin: '0 0 0.5rem', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
              {question.topicName} / {question.difficulty}
            </p>
          )}
          <p style={{ margin: 0, fontSize: '1.05rem', lineHeight: 1.6 }}><MathText text={question.text} as="span" /></p>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
          {question.options.map(opt => {
            let bg = 'var(--bg-secondary)';
            if (answerResult) {
              if (opt.id === answerResult.correctOptionId) bg = 'rgba(40, 167, 69, 0.15)';
              else if (opt.id === selectedOption && !answerResult.isCorrect) bg = 'rgba(220, 53, 69, 0.15)';
            } else if (opt.id === selectedOption) {
              bg = 'rgba(0, 123, 255, 0.15)';
            }
            return (
              <button
                key={opt.id}
                className="card"
                style={{
                  padding: '0.75rem 1rem', textAlign: 'left', cursor: answerResult ? 'default' : 'pointer',
                  background: bg, border: 'none', width: '100%'
                }}
                onClick={() => handleAnswer(opt.id)}
                disabled={!!answerResult}
              >
                <MathText text={opt.text} as="span" />
              </button>
            );
          })}
        </div>

        {answerResult && !answerResult.drillEnded && (
          <button className="btn btn-primary" style={{ marginTop: '1rem', width: '100%' }} onClick={handleNext}>
            Next Question
          </button>
        )}

        {answerResult?.explanation && (
          <p style={{ marginTop: '0.75rem', fontSize: '0.85rem', color: 'var(--text-secondary)', fontStyle: 'italic' }}>
            {answerResult.explanation}
          </p>
        )}
      </div>
    );
  }

  // Result view
  if (view === 'result' && finalResult) {
    return (
      <div className="animate-fade-in" style={{ textAlign: 'center' }}>
        <h2>Drill Complete</h2>
        <div className="card" style={{ padding: '2rem', maxWidth: '400px', margin: '0 auto' }}>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', textAlign: 'left' }}>
            <div><span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Type</span><br />{finalResult.drillType}</div>
            <div><span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Accuracy</span><br />{finalResult.accuracyPercent}%</div>
            <div><span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Correct</span><br />{finalResult.correctAnswers} / {finalResult.questionsAnswered}</div>
            <div><span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Best Streak</span><br />{finalResult.bestStreak}</div>
            <div><span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Avg Time</span><br />{finalResult.averageTimeSeconds}s</div>
            <div><span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Total Time</span><br />{finalResult.totalTimeSeconds}s</div>
          </div>
        </div>
        <div style={{ marginTop: '1.5rem', display: 'flex', gap: '0.75rem', justifyContent: 'center' }}>
          <button className="btn btn-primary" onClick={() => { setView('menu'); loadPersonalBests(); }}>
            Back to Menu
          </button>
          <button className="btn btn-secondary" onClick={() => startDrill(finalResult.drillType as 'Speed' | 'Marathon' | 'Streak')}>
            Try Again
          </button>
        </div>
      </div>
    );
  }

  // History view
  if (view === 'history') {
    return (
      <div className="animate-fade-in">
        <button className="btn btn-secondary" onClick={() => setView('menu')} style={{ marginBottom: '1rem' }}>
          Back to Menu
        </button>
        <h3>Drill History</h3>
        {history.length === 0 ? (
          <p style={{ color: 'var(--text-secondary)' }}>No completed drills yet</p>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {history.map(d => (
              <div key={d.id} className="card" style={{ padding: '0.75rem 1rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                  <strong>{d.drillType}</strong>
                  <span style={{ marginLeft: '0.75rem', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                    {d.correctAnswers}/{d.questionsAnswered} ({d.accuracyPercent}%)
                  </span>
                </div>
                <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                  {new Date(d.completedAt).toLocaleDateString()}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    );
  }

  // Menu view
  const drillModes = [
    { type: 'Speed' as const, title: 'Speed Round', desc: '10 questions, answer as fast as you can' },
    { type: 'Marathon' as const, title: 'Marathon', desc: `Answer as many questions as possible in ${MARATHON_TIME_LIMIT / 60} minutes` },
    { type: 'Streak' as const, title: 'Streak Challenge', desc: 'Keep answering correctly until you miss' },
  ];

  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h3 style={{ margin: 0 }}>Timed Drills</h3>
        <button className="btn btn-secondary" onClick={loadHistory}>History</button>
      </div>

      {/* Drill modes */}
      <div style={{ display: 'grid', gap: '0.75rem', gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', marginBottom: '2rem' }}>
        {drillModes.map(mode => (
          <div key={mode.type} className="card" style={{ padding: '1.25rem' }}>
            <h4 style={{ margin: '0 0 0.5rem' }}>{mode.title}</h4>
            <p style={{ margin: '0 0 1rem', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{mode.desc}</p>
            <button className="btn btn-primary" onClick={() => startDrill(mode.type)} disabled={loading} style={{ width: '100%' }}>
              Start
            </button>
          </div>
        ))}
      </div>

      {/* Personal bests */}
      {personalBests.some(pb => pb.bestScore !== null) && (
        <>
          <h4 style={{ marginBottom: '0.75rem', color: 'var(--text-secondary)' }}>Personal Bests</h4>
          <div style={{ display: 'grid', gap: '0.5rem', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))' }}>
            {personalBests.filter(pb => pb.bestScore !== null).map(pb => (
              <div key={pb.drillType} className="card" style={{ padding: '1rem' }}>
                <div style={{ fontWeight: 600, marginBottom: '0.5rem' }}>{pb.drillType}</div>
                <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  Score: {pb.bestScore} | Streak: {pb.bestStreak}
                  {pb.bestAverageTime && <> | Avg: {pb.bestAverageTime}s</>}
                </div>
              </div>
            ))}
          </div>
        </>
      )}
    </div>
  );
}

export default TimedDrillPage;
