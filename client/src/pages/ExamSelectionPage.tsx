import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { fetchExams, toggleExamSelection } from '../store/slices/examSlice';
import { resetTest } from '../store/slices/testSlice';

function ExamSelectionPage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { exams, selectedExams, isLoading, error } = useAppSelector((state) => state.exam);

  useEffect(() => {
    dispatch(fetchExams());
    dispatch(resetTest());
  }, [dispatch]);

  const handleExamClick = (examCode: string) => {
    dispatch(toggleExamSelection(examCode));
  };

  const handleStartTest = () => {
    if (selectedExams.length > 0) {
      navigate('/test');
    }
  };

  const examDescriptions: Record<string, string> = {
    SAT: 'Comprehensive test covering Reading, Writing, and Math sections for college admissions in the United States.',
    NUET: 'Nazarbayev University Entrance Test - Critical thinking, quantitative reasoning, and English proficiency.',
  };

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

  return (
    <div>
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ fontSize: '1.5rem', fontWeight: '700', marginBottom: '0.5rem' }}>
          Select Your Exam
        </h1>
        <p style={{ color: 'var(--text-secondary)' }}>
          Choose one or more exams to start your adaptive practice session
        </p>
      </div>

      <div className="exam-grid">
        {exams.map((exam) => (
          <div
            key={exam.code}
            className={`card exam-card ${selectedExams.includes(exam.code) ? 'selected' : ''}`}
            onClick={() => handleExamClick(exam.code)}
          >
            <h3 className="exam-card-title">{exam.code}</h3>
            <p className="exam-card-description">
              {examDescriptions[exam.code] || exam.name}
            </p>
            {selectedExams.includes(exam.code) && (
              <div
                style={{
                  marginTop: '1rem',
                  color: 'var(--primary-color)',
                  fontWeight: '500',
                }}
              >
                ✓ Selected
              </div>
            )}
          </div>
        ))}
      </div>

      {selectedExams.length > 0 && (
        <div style={{ marginTop: '2rem', textAlign: 'center' }}>
          <button onClick={handleStartTest} className="btn btn-primary">
            Start Adaptive Test ({selectedExams.length} exam{selectedExams.length > 1 ? 's' : ''})
          </button>
        </div>
      )}
    </div>
  );
}

export default ExamSelectionPage;
