import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import {
  fetchNextQuestion,
  submitAnswer,
  selectAnswer,
  clearAnswerResult,
  resetTestProgress,
  resetTest,
  setTestSessionId,
} from '../store/slices/testSlice';
import { analyticsService } from '../services/analyticsService';
import { lessonService } from '../services/lessonService';
import { subscriptionService } from '../services/subscriptionService';
import { studyPlanService } from '../services/studyPlanService';
import { UpgradeBanner } from '../components/UpgradeBanner';
import { DailyLimitModal } from '../components/DailyLimitModal';
import type { DailyUsage } from '../types';

function TestPage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { selectedExams } = useAppSelector((state) => state.exam);

  // Read plan-related params from URL
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

  // Daily limit state
  const [dailyUsage, setDailyUsage] = useState<DailyUsage | null>(null);
  const [showLimitModal, setShowLimitModal] = useState(false);
  const { user } = useAppSelector((state) => state.auth);
  const isPro = user?.subscriptionTier === 'Pro';

  // Fetch daily usage on mount and after each answer
  useEffect(() => {
    if (!isPro) {
      subscriptionService.getDailyUsage().then(setDailyUsage).catch(() => {});
    }
  }, [isPro, questionsAnswered]);

  // Redirect if no exams selected
  useEffect(() => {
    if (selectedExams.length === 0) {
      navigate('/');
    }
  }, [selectedExams, navigate]);

  const handleStartPractice = async () => {
    // Check daily limit for free users
    if (!isPro) {
      try {
        const usage = await subscriptionService.getDailyUsage();
        setDailyUsage(usage);
        if (usage.isLimitReached) {
          setShowLimitModal(true);
          return;
        }
      } catch { /* proceed if check fails */ }
    }
    setPracticeStarted(true);
    try {
      const session = await analyticsService.startSession(selectedExams[0], 'practice');
      dispatch(setTestSessionId(session.id));
    } catch (err) {
      console.error('Failed to start session:', err);
    }
    dispatch(fetchNextQuestion({ examTypeCodes: selectedExams, topicId }));
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

  const handleNextQuestion = () => {
    setShowFeedback(false);
    setHintText(null);
    setShowHint(false);
    dispatch(clearAnswerResult());
    dispatch(fetchNextQuestion({ examTypeCodes: selectedExams, topicId }));
  };

  const handleFinishTest = async () => {
    if (testSessionId) {
      try {
        await analyticsService.completeSession(testSessionId);
      } catch (err) {
        console.error('Failed to complete session:', err);
      }
    }
    // Auto-complete plan entry if we came from the plan
    if (planEntryId) {
      try {
        await studyPlanService.autoCompleteToday();
      } catch { /* ignore */ }
      // Clear plan params from URL and navigate back to plan
      navigate('/plan');
      return;
    }
    navigate('/progress');
  };

  const handleResetTest = async () => {
    await dispatch(resetTestProgress());
    dispatch(fetchNextQuestion({ examTypeCodes: selectedExams, topicId }));
  };

  const handleQuitTest = async () => {
    // Complete the session if one exists
    if (testSessionId) {
      try {
        await analyticsService.completeSession(testSessionId);
      } catch (err) {
        console.error('Failed to complete session:', err);
      }
    }
    // Auto-complete plan entry if we came from the plan
    if (planEntryId) {
      try {
        await studyPlanService.autoCompleteToday();
      } catch { /* ignore */ }
    }
    dispatch(resetTest());
    setPracticeStarted(false);
    setShowQuitConfirm(false);
    // Navigate back to plan if we came from there
    if (planEntryId) {
      // Remove plan params from URL
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

  // Auto-start when coming from the plan (with topicId)
  useEffect(() => {
    if (topicId && selectedExams.length > 0 && !practiceStarted && !currentQuestion && !testCompleted) {
      handleStartPractice();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [topicId]);

  if (selectedExams.length === 0) {
    return null;
  }

  // Start screen — before practice begins
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
          <span style={{ fontSize: '3rem', display: 'block', marginBottom: '1rem' }}>{topicId ? '📅' : '📚'}</span>
          <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '0.5rem', color: 'var(--text-primary)' }}>
            {topicId ? 'Задание по плану' : 'Адаптивная практика'}
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '0.5rem', lineHeight: '1.5' }}>
            {topicId
              ? 'Вопросы подобраны по теме из вашего учебного плана. После завершения результат автоматически зачтётся в план.'
              : 'Вопросы подбираются под ваш уровень с помощью IRT-алгоритма. Объяснения после каждого ответа, подсказки и без ограничения по времени.'
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
            ▶ Начать практику
          </button>
        </div>
      </div>
    );
  }

  if (testCompleted) {
    return (
      <div className="test-container">
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <span style={{ fontSize: '3rem', display: 'block', marginBottom: '1rem' }}>🎉</span>
          <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '1rem' }}>
            {planEntryId ? 'Задание выполнено!' : 'Практика завершена!'}
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1rem' }}>
            {planEntryId
              ? 'Результат автоматически зачтён в ваш учебный план.'
              : 'Вы ответили на все доступные вопросы. Проверьте аналитику, чтобы увидеть прогресс.'
            }
          </p>
          <p style={{ marginBottom: '2rem', fontSize: '1.1rem' }}>
            Отвечено вопросов: <strong>{questionsAnswered}</strong>
          </p>
          <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
            <button onClick={handleFinishTest} className="btn btn-primary">
              {planEntryId ? '📅 Вернуться к плану' : '📊 Аналитика'}
            </button>
            {!planEntryId && (
              <button onClick={handleResetTest} className="btn btn-secondary" disabled={isLoading}>
                {isLoading ? 'Сброс...' : '🔄 Начать заново'}
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
    return (
      <div className="test-container">
        <div className="card" style={{ textAlign: 'center' }}>
          <p className="error-message">{error}</p>
          <button onClick={() => navigate('/')} className="btn btn-primary" style={{ marginTop: '1rem' }}>
            Back to Exam Selection
          </button>
        </div>
      </div>
    );
  }

  if (!currentQuestion) {
    return (
      <div className="test-container">
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '1rem' }}>
            No Questions Available
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem' }}>
            There are no more questions available for the selected exams.
          </p>
          <button onClick={() => navigate('/')} className="btn btn-primary">
            Choose Different Exams
          </button>
        </div>
      </div>
    );
  }

  const getDifficultyClass = (difficulty: string) => {
    switch (difficulty.toLowerCase()) {
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
      {/* Daily usage banner for free users */}
      {!isPro && dailyUsage && dailyUsage.questionsRemaining >= 0 && dailyUsage.questionsRemaining <= 5 && (
        <UpgradeBanner
          questionsRemaining={dailyUsage.questionsRemaining}
          questionsLimit={dailyUsage.questionsLimit}
        />
      )}

      <DailyLimitModal isOpen={showLimitModal} onClose={() => setShowLimitModal(false)} />

      <div className="question-card card">
        {/* Practice Mode Badge + Quit */}
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
            📚 Practice
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
            ✕ Выйти
          </button>
        </div>

        {/* Quit Confirmation */}
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
                ? `Завершить задание? Ваш прогресс (${questionsAnswered} вопросов) будет зачтён в учебный план.`
                : `Завершить практику? Ваш прогресс (${questionsAnswered} вопросов) сохранится.`
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
                Да, выйти
              </button>
              <button
                onClick={() => setShowQuitConfirm(false)}
                className="btn btn-secondary"
                style={{ padding: '0.375rem 1rem', fontSize: '0.8rem' }}
              >
                Продолжить
              </button>
            </div>
          </div>
        )}

        {/* Progress Bar */}
        <div className="progress-container" style={{ marginBottom: '1.5rem' }}>
          <div style={{ 
            display: 'flex', 
            justifyContent: 'space-between', 
            alignItems: 'center',
            marginBottom: '0.5rem'
          }}>
            <span style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
              Question {questionsAnswered + 1} of {totalQuestions}
            </span>
            <span style={{ fontSize: '0.875rem', fontWeight: '600', color: 'var(--primary-color)' }}>
              {totalQuestions > 0 ? Math.round(((questionsAnswered) / totalQuestions) * 100) : 0}%
            </span>
          </div>
          <div style={{
            width: '100%',
            height: '8px',
            backgroundColor: 'var(--border-color)',
            borderRadius: '4px',
            overflow: 'hidden'
          }}>
            <div style={{
              width: `${totalQuestions > 0 ? ((questionsAnswered) / totalQuestions) * 100 : 0}%`,
              height: '100%',
              backgroundColor: 'var(--primary-color)',
              borderRadius: '4px',
              transition: 'width 0.3s ease'
            }} />
          </div>
        </div>

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
                {hintLoading ? '...' : showHint ? '💡 Hide Hint' : '💡 Hint'}
              </button>
            )}
            <span className={`difficulty-badge ${getDifficultyClass(currentQuestion.difficulty)}`}>
              {currentQuestion.difficulty}
            </span>
          </div>
        </div>

        {/* Hint Display */}
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
            <span style={{ fontWeight: '600', color: 'var(--warning-color)' }}>💡 Hint: </span>
            {hintText}
          </div>
        )}

        <p className="question-text">{currentQuestion.text}</p>

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
                {answerResult.isCorrect ? '✅' : '❌'}
              </span>
              <p
                style={{
                  fontWeight: '700',
                  fontSize: '1.125rem',
                  color: answerResult.isCorrect ? 'var(--success-color)' : 'var(--error-color)',
                  margin: 0,
                }}
              >
                {answerResult.isCorrect ? 'Correct!' : 'Incorrect'}
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
                  Correct answer:
                </p>
                <p style={{ margin: '0.25rem 0 0 0', fontWeight: '600', color: 'var(--success-color)' }}>
                  {answerResult.correctOptionText}
                </p>
              </div>
            )}

            {/* Show explanation only in practice mode */}
            {answerResult.explanation && (
              <div style={{
                marginBottom: '0.75rem',
                padding: '0.75rem',
                backgroundColor: 'rgba(99, 102, 241, 0.1)',
                borderRadius: '0.5rem',
                border: '1px solid rgba(99, 102, 241, 0.2)'
              }}>
                <p style={{ margin: 0, fontSize: '0.75rem', fontWeight: '600', color: 'var(--primary-color)', textTransform: 'uppercase' }}>
                  💡 Explanation
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
                Skill: {answerResult.skillChange > 0 ? '+' : ''}{answerResult.skillChange}
              </span>
              <span>•</span>
              <span>Level: {answerResult.newSkillLevel}</span>
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
              {isLoading ? 'Submitting...' : 'Submit Answer'}
            </button>
          ) : (
            <button onClick={handleNextQuestion} className="btn btn-primary">
              Next Question
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export default TestPage;
