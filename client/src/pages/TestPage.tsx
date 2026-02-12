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
        <div className="question-header">
          <span className="question-number">
            Question {questionsAnswered + 1} • {currentQuestion.topicName}
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
            style={{
              marginTop: '1.5rem',
              padding: '1rem',
              borderRadius: '0.5rem',
              backgroundColor: answerResult.isCorrect
                ? 'rgba(16, 185, 129, 0.1)'
                : 'rgba(239, 68, 68, 0.1)',
            }}
          >
            <p
              style={{
                fontWeight: '600',
                color: answerResult.isCorrect ? 'var(--success-color)' : 'var(--error-color)',
              }}
            >
              {answerResult.isCorrect ? '✓ Correct!' : '✗ Incorrect'}
            </p>
            <p style={{ fontSize: '0.875rem', marginTop: '0.5rem' }}>
              Skill change: {answerResult.skillChange > 0 ? '+' : ''}
              {answerResult.skillChange} • New level: {answerResult.newSkillLevel}
            </p>
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
