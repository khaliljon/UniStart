import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import {
  fetchNextQuestion,
  submitAnswer,
  selectAnswer,
  clearAnswerResult,
  resetTest,
  setTestSessionId,
} from '../store/slices/testSlice';
import { analyticsService } from '../services/analyticsService';
import { lessonService } from '../services/lessonService';
import { subscriptionService } from '../services/subscriptionService';
import { studyPlanService } from '../services/studyPlanService';
import { UpgradeBanner } from '../components/UpgradeBanner';
import { DailyLimitModal } from '../components/DailyLimitModal';
import { useTranslation } from '../hooks/useTranslation';
import type { DailyUsage } from '../types';

function TestPage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();
  const { selectedExams, selectedSectionIds } = useAppSelector((state) => state.exam);

  const topicIdParam = searchParams.get('topicId');
  const planEntryIdParam = searchParams.get('planEntryId');
  const topicId = topicIdParam ? parseInt(topicIdParam, 10) : undefined;
  const planEntryId = planEntryIdParam ? parseInt(planEntryIdParam, 10) : undefined;
  const {
    currentQuestion,
    selectedAnswer,
    answerResult,
    questionsAnswered,
    totalQuestions,
    topicMastery,
    masteryReached,
    testCompleted,
    isLoading,
    error,
    questionStartTime,
    testSessionId,
  } = useAppSelector((state) => state.test);

  const [showFeedback, setShowFeedback] = useState(false);
  const [hintText, setHintText] = useState<string | null>(null);
  const [hintLoading, setHintLoading] = useState(false);
  const [showHint, setShowHint] = useState(false);
  const [showQuitConfirm, setShowQuitConfirm] = useState(false);
  const [practiceStarted, setPracticeStarted] = useState(false);
  const [sessionAnswered, setSessionAnswered] = useState(0);
  const [, setMasteryShown] = useState(false);

  const [dailyUsage, setDailyUsage] = useState<DailyUsage | null>(null);
  const [showLimitModal, setShowLimitModal] = useState(false);
  const { user } = useAppSelector((state) => state.auth);
  const isPro = user?.subscriptionTier === 'Pro';

  useEffect(() => {
    if (!isPro) {
      subscriptionService.getDailyUsage().then(setDailyUsage).catch(() => {});
    }
  }, [isPro, questionsAnswered]);

  useEffect(() => {
    if (selectedExams.length === 0 && !topicId) {
      navigate('/');
    }
  }, [selectedExams, navigate, topicId]);

  const handleStartPractice = async () => {
    if (!isPro) {
      try {
        const usage = await subscriptionService.getDailyUsage();
        setDailyUsage(usage);
        if (usage.isLimitReached) {
          setShowLimitModal(true);
          return;
        }
      } catch {}
    }
    setPracticeStarted(true);
    try {
      const session = await analyticsService.startSession(selectedExams[0], 'practice');
      dispatch(setTestSessionId(session.id));
    } catch (err) {
      console.error('Failed to start session:', err);
    }
    dispatch(fetchNextQuestion({ examTypeCodes: selectedExams, sectionIds: topicId ? undefined : selectedSectionIds.length > 0 ? selectedSectionIds : undefined, topicId }));
  };

  const handleOptionClick = (optionId: number) => {
    if (!answerResult && !isLoading) {
      dispatch(selectAnswer(optionId));
    }
  };

  const handleSubmit = async () => {
    if (selectedAnswer && currentQuestion) {
      const timeSpentSeconds = questionStartTime
        ? Math.round((Date.now() - questionStartTime) / 1000)
        : undefined;
      await dispatch(
        submitAnswer({
          questionId: currentQuestion.id,
          answerOptionId: selectedAnswer,
          timeSpentSeconds,
          testSessionId: testSessionId ?? undefined,
        })
      );
      setShowFeedback(true);
    }
  };

  const handleNextQuestion = async () => {
    if (!isPro) {
      try {
        const usage = await subscriptionService.getDailyUsage();
        setDailyUsage(usage);
        if (usage.isLimitReached) {
          setShowLimitModal(true);
          return;
        }
      } catch {}
    }
    setShowFeedback(false);
    setHintText(null);
    setShowHint(false);
    setSessionAnswered(prev => prev + 1);
    dispatch(clearAnswerResult());
    dispatch(fetchNextQuestion({ examTypeCodes: selectedExams, sectionIds: topicId ? undefined : selectedSectionIds.length > 0 ? selectedSectionIds : undefined, topicId }));
  };

  const handleFinishTest = async () => {
    if (testSessionId) {
      try {
        await analyticsService.completeSession(testSessionId);
      } catch (err) {
        console.error('Failed to complete session:', err);
      }
    }
    if (planEntryId) {
      try {
        await studyPlanService.autoCompleteToday();
      } catch {}
      navigate('/plan');
      return;
    }
    navigate('/progress');
  };

  const handleQuitTest = async () => {
    if (testSessionId) {
      try {
        await analyticsService.completeSession(testSessionId);
      } catch (err) {
        console.error('Failed to complete session:', err);
      }
    }
    if (planEntryId) {
      try {
        await studyPlanService.autoCompleteToday();
      } catch {}
    }
    dispatch(resetTest());
    setPracticeStarted(false);
    setSessionAnswered(0);
    setShowQuitConfirm(false);
    if (planEntryId) {
      const newParams = new URLSearchParams(searchParams);
      newParams.delete('topicId');
      newParams.delete('planEntryId');
      setSearchParams(newParams, { replace: true });
      navigate('/plan');
    }
  };

  const handleRequestHint = async () => {
    if (!currentQuestion) return;
    if (hintText) {
      setShowHint(!showHint);
      return;
    }
    setHintLoading(true);
    try {
      const hint = await lessonService.getQuestionHint(currentQuestion.id);
      setHintText(hint);
      setShowHint(true);
    } catch {
      setHintText(null);
    } finally {
      setHintLoading(false);
    }
  };

  useEffect(() => {
    if (topicId && selectedExams.length > 0) {
      dispatch(resetTest());
      setPracticeStarted(false);
      setSessionAnswered(0);
      handleStartPractice();
    }
  }, [topicId]);

  if (selectedExams.length === 0 && !topicId) {
    return null;
  }

  if (!practiceStarted && !currentQuestion && !testCompleted) {
    return (
      <div className="test-container">
        <DailyLimitModal isOpen={showLimitModal} onClose={() => setShowLimitModal(false)} />
        {!isPro && dailyUsage && (
          <div style={{ maxWidth: '600px', margin: '0 auto 0' }}>

            <UpgradeBanner
              questionsRemaining={dailyUsage.questionsRemaining}
              questionsLimit={dailyUsage.questionsLimit}
            />
          </div>
        )}
        <div className="card" style={{ maxWidth: '500px', margin: '0 auto', padding: '2.5rem', textAlign: 'center' }}>
          <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '0.5rem', color: 'var(--text-primary)' }}>
            {topicId ? t.studyPlan.planAssignment : t.studyPlan.adaptivePractice}
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '0.5rem', lineHeight: '1.5' }}>
            {topicId
              ? t.studyPlan.planAssignmentDesc
              : t.studyPlan.adaptivePracticeDesc
            }
          </p>
          <div style={{
            display: 'flex',
            gap: '0.5rem',
            justifyContent: 'center',
            flexWrap: 'wrap',
            margin: '1rem 0 1.5rem'
          }}>
            {selectedExams.map(code => (
              <span key={code} style={{
                padding: '0.25rem 0.75rem',
                borderRadius: '1rem',
                fontSize: '0.75rem',
                fontWeight: '600',
                backgroundColor: 'rgba(99, 102, 241, 0.1)',
                color: 'var(--primary-color)',
              }}>
                {code}
              </span>
            ))}
          </div>
          <button
            onClick={handleStartPractice}
            className="btn btn-primary"
            style={{ padding: '0.75rem 2rem', fontSize: '1rem' }}
          >
            ▶ {t.studyPlan.startPractice}
          </button>
        </div>
      </div>
    );
  }

  if (testCompleted && totalQuestions === 0 && sessionAnswered === 0) {
    return (
      <div className="test-container">
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <div style={{
            width: '64px', height: '64px', borderRadius: '50%',
            backgroundColor: 'rgba(99, 102, 241, 0.1)', display: 'flex',
            alignItems: 'center', justifyContent: 'center',
            margin: '0 auto 1rem', fontSize: '2rem'
          }}>
            📝
          </div>
          <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '1rem' }}>
            {t.practice.noQuestionsYet}
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem', maxWidth: '420px', margin: '0 auto 2rem', lineHeight: 1.5 }}>
            {t.practice.noQuestionsYetDesc}
          </p>
          <button onClick={() => { dispatch(resetTest()); navigate('/learn?tab=topics'); }} className="btn btn-primary">
            {t.practice.chooseDifferent}
          </button>
        </div>
      </div>
    );
  }

  if (testCompleted) {
    const isMastered = masteryReached || topicMastery >= 80;
    return (
      <div className="test-container">
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          {isMastered && (
            <div style={{
              width: '64px', height: '64px', borderRadius: '50%',
              backgroundColor: 'rgba(16, 185, 129, 0.1)', display: 'flex',
              alignItems: 'center', justifyContent: 'center',
              margin: '0 auto 1rem', fontSize: '2rem'
            }}>
              ✓
            </div>
          )}
          <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '1rem' }}>
            {planEntryId
              ? t.studyPlan.taskComplete
              : isMastered
                ? t.studyPlan.topicMastered
                : t.studyPlan.practiceComplete}
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1rem' }}>
            {planEntryId
              ? t.studyPlan.taskCompleteDesc
              : isMastered
                ? t.studyPlan.topicMasteredDesc
                : t.studyPlan.practiceCompleteDesc
            }
          </p>
          {topicMastery > 0 && (
            <div style={{ marginBottom: '1.5rem' }}>
              <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>
                {t.studyPlan.topicMastery}
              </div>
              <div style={{
                width: '200px', height: '8px', backgroundColor: 'var(--border-color)',
                borderRadius: '4px', overflow: 'hidden', margin: '0 auto 0.5rem'
              }}>
                <div style={{
                  width: `${topicMastery}%`, height: '100%',
                  backgroundColor: topicMastery >= 80 ? 'var(--success-color)' : 'var(--primary-color)',
                  borderRadius: '4px', transition: 'width 0.3s ease'
                }} />
              </div>
              <span style={{
                fontSize: '1.25rem', fontWeight: '700',
                color: topicMastery >= 80 ? 'var(--success-color)' : 'var(--primary-color)'
              }}>
                {topicMastery}%
              </span>
            </div>
          )}
          <p style={{ marginBottom: '2rem', fontSize: '0.95rem', color: 'var(--text-secondary)' }}>
            {t.studyPlan.sessionAnswered}: <strong>{sessionAnswered}</strong>
          </p>
          <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
            <button onClick={handleFinishTest} className="btn btn-primary">
              {planEntryId ? t.studyPlan.backToPlan : t.studyPlan.analytics}
            </button>
            {!planEntryId && (
              <button onClick={() => {
                setMasteryShown(true);
                dispatch(fetchNextQuestion({ examTypeCodes: selectedExams, sectionIds: topicId ? undefined : selectedSectionIds.length > 0 ? selectedSectionIds : undefined, topicId }));
              }} className="btn btn-secondary" disabled={isLoading}>
                {t.studyPlan.continuePractice}
              </button>
            )}
          </div>
        </div>
      </div>
    );
  }

  if (isLoading && !currentQuestion) {
    return (
      <div className="loading">
        <div className="spinner"></div>
      </div>
    );
  }

  if (error) {
    const is429 = error.includes('429');
    return (
      <div className="test-container">
        {is429 && <DailyLimitModal isOpen={showLimitModal} onClose={() => setShowLimitModal(false)} />}
        <div className="card" style={{ textAlign: 'center', padding: '2rem' }}>
          {is429 ? (
            <>
              <h2 style={{ fontSize: '1.25rem', fontWeight: 700, marginBottom: '0.75rem', color: 'var(--text-primary)' }}>
                {t.limits.limitReachedTitle}
              </h2>
              <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem', lineHeight: 1.5 }}>
                {t.limits.exhaustedDesc}
              </p>
              <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'center', flexWrap: 'wrap' }}>
                <button
                  onClick={() => setShowLimitModal(true)}
                  className="btn btn-primary"
                >
                  {t.limits.upgradePro}
                </button>
                <button
                  onClick={() => { dispatch(resetTest()); navigate('/'); }}
                  className="btn btn-secondary"
                >
                  {t.practice.backToExams}
                </button>
              </div>
            </>
          ) : (
            <>
              <p className="error-message">{error}</p>
              <button onClick={() => { dispatch(resetTest()); navigate('/'); }} className="btn btn-primary" style={{ marginTop: '1rem' }}>
                {t.practice.backToExams}
              </button>
            </>
          )}
        </div>
      </div>
    );
  }

  if (!currentQuestion) {
    return (
      <div className="test-container">
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '1rem' }}>
            {t.practice.noQuestions}
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem' }}>
            {t.practice.noQuestionsDesc}
          </p>
          <button onClick={() => navigate('/')} className="btn btn-primary">
            {t.practice.chooseDifferent}
          </button>
        </div>
      </div>
    );
  }

  const getDifficultyClass = (difficulty: string) => {
    switch ((difficulty ?? '').toLowerCase()) {
      case 'easy':
        return 'difficulty-easy';
      case 'medium':
        return 'difficulty-medium';
      case 'hard':
        return 'difficulty-hard';
      default:
        return '';
    }
  };

  const getOptionClass = (optionId: number) => {
    if (!answerResult) {
      return selectedAnswer === optionId ? 'selected' : '';
    }
    if (optionId === answerResult.correctOptionId) {
      return 'correct';
    }
    if (optionId === selectedAnswer && !answerResult.isCorrect) {
      return 'incorrect';
    }
    return '';
  };

  return (
    <div className="test-container">
      {!isPro && dailyUsage && dailyUsage.questionsRemaining >= 0 && dailyUsage.questionsRemaining <= 5 && (
        <UpgradeBanner
          questionsRemaining={dailyUsage.questionsRemaining}
          questionsLimit={dailyUsage.questionsLimit}
        />
      )}

      <DailyLimitModal isOpen={showLimitModal} onClose={() => setShowLimitModal(false)} />

      <div className="question-card card">
        <div style={{ 
          display: 'flex', 
          justifyContent: 'space-between', 
          alignItems: 'center',
          marginBottom: '1rem',
          padding: '0.5rem 0',
          borderBottom: '1px solid var(--border-color)'
        }}>
          <span style={{
            padding: '0.25rem 0.75rem',
            borderRadius: '1rem',
            fontSize: '0.75rem',
            fontWeight: '600',
            textTransform: 'uppercase',
            backgroundColor: 'rgba(16, 185, 129, 0.1)',
            color: 'var(--success-color)',
          }}>
            Practice
          </span>
          <button
            onClick={() => setShowQuitConfirm(true)}
            style={{
              padding: '0.25rem 0.75rem',
              borderRadius: '0.5rem',
              fontSize: '0.75rem',
              fontWeight: '500',
              border: '1px solid rgba(239, 68, 68, 0.3)',
              backgroundColor: 'rgba(239, 68, 68, 0.05)',
              color: 'var(--error-color)',
              cursor: 'pointer',
              transition: 'all 0.2s ease',
            }}
            onMouseOver={(e) => { e.currentTarget.style.backgroundColor = 'rgba(239, 68, 68, 0.15)'; }}
            onMouseOut={(e) => { e.currentTarget.style.backgroundColor = 'rgba(239, 68, 68, 0.05)'; }}
          >
            ✕ {t.practice.quit}
          </button>
        </div>

        {showQuitConfirm && (
          <div style={{
            marginBottom: '1rem',
            padding: '1rem',
            borderRadius: '0.75rem',
            backgroundColor: 'rgba(239, 68, 68, 0.08)',
            border: '1px solid rgba(239, 68, 68, 0.2)',
          }}>
            <p style={{ margin: '0 0 0.75rem', fontSize: '0.9rem', color: 'var(--text-primary)' }}>
              {planEntryId
                ? t.practice.quitConfirmPlan
                : t.practice.quitConfirmPractice
              }
            </p>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button
                onClick={handleQuitTest}
                className="btn"
                style={{
                  padding: '0.375rem 1rem',
                  fontSize: '0.8rem',
                  backgroundColor: 'var(--error-color)',
                  color: '#fff',
                  border: 'none',
                  borderRadius: '0.5rem',
                  cursor: 'pointer',
                }}
              >
                {t.practice.yesQuit}
              </button>
              <button
                onClick={() => setShowQuitConfirm(false)}
                className="btn btn-secondary"
                style={{ padding: '0.375rem 1rem', fontSize: '0.8rem' }}
              >
                {t.practice.continue}
              </button>
            </div>
          </div>
        )}

        {(() => {
          const hasTopic = !!topicId;
          const mastery = topicMastery;
          const total = totalQuestions > 0 ? totalQuestions : 1;
          const isRecycling = sessionAnswered >= total;
          const masteryColor = mastery >= 80 ? 'var(--success-color)' : mastery >= 50 ? 'rgb(245, 158, 11)' : 'var(--primary-color)';

          return (
            <div className="progress-container" style={{ marginBottom: '1.5rem' }}>
              <div style={{ 
                display: 'flex', 
                justifyContent: 'space-between', 
                alignItems: 'center',
                marginBottom: '0.5rem'
              }}>
                <span style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                  {isRecycling
                    ? `${t.practice.solved}: ${sessionAnswered} (${totalQuestions} ${t.practice.inTopic})`
                    : !isPro && dailyUsage
                      ? `${dailyUsage.questionsAnswered} / ${dailyUsage.questionsLimit}`
                      : `${sessionAnswered + 1} / ${totalQuestions}`}
                </span>
                {hasTopic && (
                  <span style={{
                    display: 'flex', alignItems: 'center', gap: '0.375rem',
                    fontSize: '0.8rem', fontWeight: '600', color: masteryColor,
                  }}>
                    {t.practice.mastery}: {mastery}%
                  </span>
                )}
                {!hasTopic && (
                  <span style={{ fontSize: '0.875rem', fontWeight: '600', color: 'var(--primary-color)' }}>
                    {!isPro && dailyUsage
                      ? `${Math.min(Math.round((dailyUsage.questionsAnswered / dailyUsage.questionsLimit) * 100), 100)}%`
                      : `${totalQuestions > 0 ? Math.min(Math.round((sessionAnswered / total) * 100), 100) : 0}%`}
                  </span>
                )}
              </div>
              {hasTopic ? (
                <div style={{
                  width: '100%', height: '8px', backgroundColor: 'var(--border-color)',
                  borderRadius: '4px', overflow: 'hidden'
                }}>
                  <div style={{
                    width: `${mastery}%`, height: '100%',
                    backgroundColor: masteryColor,
                    borderRadius: '4px', transition: 'width 0.5s ease'
                  }} />
                </div>
              ) : (
                <div style={{
                  width: '100%', height: '8px', backgroundColor: 'var(--border-color)',
                  borderRadius: '4px', overflow: 'hidden'
                }}>
                  <div style={{
                    width: `${!isPro && dailyUsage
                      ? Math.min((dailyUsage.questionsAnswered / dailyUsage.questionsLimit) * 100, 100)
                      : totalQuestions > 0 ? Math.min((sessionAnswered / total) * 100, 100) : 0}%`,
                    height: '100%', backgroundColor: 'var(--primary-color)',
                    borderRadius: '4px', transition: 'width 0.3s ease'
                  }} />
                </div>
              )}
            </div>
          );
        })()}

        <div className="question-header">
          <span className="question-number">
            {currentQuestion.topicName}
          </span>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            {currentQuestion.hasHint && !answerResult && (
              <button
                onClick={handleRequestHint}
                disabled={hintLoading}
                style={{
                  padding: '0.25rem 0.75rem',
                  borderRadius: '9999px',
                  fontSize: '0.75rem',
                  fontWeight: '500',
                  border: '1px solid rgba(245, 158, 11, 0.3)',
                  backgroundColor: showHint ? 'rgba(245, 158, 11, 0.15)' : 'rgba(245, 158, 11, 0.05)',
                  color: 'var(--warning-color)',
                  cursor: 'pointer',
                  transition: 'all 0.2s ease'
                }}
              >
                {hintLoading ? '...' : showHint ? t.practice.hideHint : t.practice.hint}
              </button>
            )}
            <span className={`difficulty-badge ${getDifficultyClass(currentQuestion.difficulty)}`}>
              {currentQuestion.difficulty}
            </span>
          </div>
        </div>

        {showHint && hintText && (
          <div style={{
            margin: '0.75rem 0',
            padding: '0.75rem 1rem',
            borderRadius: '0.75rem',
            backgroundColor: 'rgba(245, 158, 11, 0.08)',
            border: '1px solid rgba(245, 158, 11, 0.2)',
            fontSize: '0.85rem',
            color: 'var(--text-secondary)',
            lineHeight: '1.5'
          }}>
            <span style={{ fontWeight: '600', color: 'var(--warning-color)' }}>Hint: </span>
            {hintText}
          </div>
        )}

        <p className="question-text">{currentQuestion.text}</p>

        {currentQuestion.imageUrl && <img src={currentQuestion.imageUrl} alt="question" style={{ maxWidth: "100%", borderRadius: "8px", margin: "12px 0" }} />}

        <div className="options-list">
          {currentQuestion.options.map((option) => (
            <button
              key={option.id}
              className={`option-btn ${getOptionClass(option.id)}`}
              onClick={() => handleOptionClick(option.id)}
              disabled={!!answerResult}
            >
              {option.text}
            </button>
          ))}
        </div>

        {showFeedback && answerResult && (
          <div
            className="feedback-card"
            style={{
              marginTop: '1.5rem',
              padding: '1.25rem',
              borderRadius: '0.75rem',
              backgroundColor: answerResult.isCorrect
                ? 'rgba(16, 185, 129, 0.1)'
                : 'rgba(239, 68, 68, 0.1)',
              border: `1px solid ${answerResult.isCorrect ? 'rgba(16, 185, 129, 0.3)' : 'rgba(239, 68, 68, 0.3)'}`,
            }}
          >
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.75rem' }}>
              <span style={{ fontSize: '1.5rem' }}>
                {answerResult.isCorrect ? '✓' : '✕'}
              </span>
              <p
                style={{
                  fontWeight: '700',
                  fontSize: '1.125rem',
                  color: answerResult.isCorrect ? 'var(--success-color)' : 'var(--error-color)',
                  margin: 0,
                }}
              >
                {answerResult.isCorrect ? t.practice.correct : t.practice.incorrect}
              </p>
            </div>

            {!answerResult.isCorrect && (
              <div style={{ 
                marginBottom: '0.75rem',
                padding: '0.75rem',
                backgroundColor: 'rgba(16, 185, 129, 0.15)',
                borderRadius: '0.5rem',
                border: '1px solid rgba(16, 185, 129, 0.3)'
              }}>
                <p style={{ margin: 0, fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                  {t.practice.correctAnswer}
                </p>
                <p style={{ margin: '0.25rem 0 0 0', fontWeight: '600', color: 'var(--success-color)' }}>
                  {answerResult.correctOptionText}
                </p>
              </div>
            )}

            {answerResult.explanation && (
              <div style={{
                marginBottom: '0.75rem',
                padding: '0.75rem',
                backgroundColor: 'rgba(99, 102, 241, 0.1)',
                borderRadius: '0.5rem',
                border: '1px solid rgba(99, 102, 241, 0.2)'
              }}>
                <p style={{ margin: 0, fontSize: '0.75rem', fontWeight: '600', color: 'var(--primary-color)', textTransform: 'uppercase' }}>
                  {t.practice.explanation}
                </p>
                <p style={{ margin: '0.5rem 0 0 0', fontSize: '0.9rem', lineHeight: '1.5' }}>
                  {answerResult.explanation}
                </p>
              </div>
            )}

            <div style={{ 
              display: 'flex', 
              alignItems: 'center', 
              gap: '1rem',
              paddingTop: '0.5rem',
              borderTop: '1px solid rgba(0,0,0,0.1)',
              fontSize: '0.875rem',
              color: 'var(--text-secondary)',
              flexWrap: 'wrap',
            }}>
              <span>
                {t.practice.skill}: {answerResult.skillChange > 0 ? '+' : ''}{answerResult.skillChange}
              </span>
              <span>•</span>
              <span>{t.practice.level}: {answerResult.newSkillLevel}</span>
              {answerResult.confidenceLow != null && answerResult.confidenceHigh != null && (
                <>
                  <span>•</span>
                  <span style={{ fontSize: '0.8rem' }}>
                    95% CI: {answerResult.confidenceLow}–{answerResult.confidenceHigh}
                  </span>
                </>
              )}
            </div>
          </div>
        )}

        <div style={{ marginTop: '1.5rem', display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
          {!answerResult ? (
            <button
              onClick={handleSubmit}
              className="btn btn-primary"
              disabled={!selectedAnswer || isLoading}
            >
              {isLoading ? t.practice.submitting : t.practice.submitAnswer}
            </button>
          ) : (
            <button onClick={handleNextQuestion} className="btn btn-primary">
              {t.practice.nextQuestion}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export default TestPage;
