import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import {
  fetchNextQuestion,
  submitAnswer,
  selectAnswer,
  clearAnswerResult,
  resetTestProgress,
} from '../store/slices/testSlice';

function TestPage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { selectedExams } = useAppSelector((state) => state.exam);
  const {
    currentQuestion,
    selectedAnswer,
    answerResult,
    questionsAnswered,
    totalQuestions,
    testCompleted,
    isLoading,
    error,
  } = useAppSelector((state) => state.test);

  const [showFeedback, setShowFeedback] = useState(false);

  useEffect(() => {
    if (selectedExams.length === 0) {
      navigate('/');
      return;
    }

    dispatch(fetchNextQuestion({ examTypeCodes: selectedExams }));
  }, [dispatch, selectedExams, navigate]);

  const handleOptionClick = (optionId: number) => {
    if (!answerResult && !isLoading) {
      dispatch(selectAnswer(optionId));
    }
  };

  const handleSubmit = async () => {
    if (selectedAnswer && currentQuestion) {
      await dispatch(
        submitAnswer({
          questionId: currentQuestion.id,
          answerOptionId: selectedAnswer,
        })
      );
      setShowFeedback(true);
    }
  };

  const handleNextQuestion = () => {
    setShowFeedback(false);
    dispatch(clearAnswerResult());
    dispatch(fetchNextQuestion({ examTypeCodes: selectedExams }));
  };

  const handleFinishTest = () => {
    navigate('/analytics');
  };

  const handleResetTest = async () => {
    await dispatch(resetTestProgress());
    dispatch(fetchNextQuestion({ examTypeCodes: selectedExams }));
  };

  if (selectedExams.length === 0) {
    return null;
  }

  if (testCompleted) {
    return (
      <div className="test-container">
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <h2 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '1rem' }}>
            Test Completed!
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem' }}>
            You've answered all available questions. Check your analytics to see your progress.
          </p>
          <p style={{ marginBottom: '2rem' }}>
            Questions answered: <strong>{questionsAnswered}</strong>
          </p>
          <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
            <button onClick={handleFinishTest} className="btn btn-primary">
              View Analytics
            </button>
            <button onClick={handleResetTest} className="btn btn-secondary" disabled={isLoading}>
              {isLoading ? 'Resetting...' : 'Restart Test'}
            </button>
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
      <div className="question-card card">
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
          <span className={`difficulty-badge ${getDifficultyClass(currentQuestion.difficulty)}`}>
            {currentQuestion.difficulty}
          </span>
        </div>

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
              color: 'var(--text-secondary)'
            }}>
              <span>
                Skill: {answerResult.skillChange > 0 ? '+' : ''}{answerResult.skillChange}
              </span>
              <span>•</span>
              <span>Level: {answerResult.newSkillLevel}</span>
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
