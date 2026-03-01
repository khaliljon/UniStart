import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAppSelector } from '../hooks/useAppSelector';
import TestPage from './TestPage';
import MockExamPage from './MockExamPage';
import TopicsPage from './TopicsPage';
import ReviewPage from './ReviewPage';

type LearnTab = 'practice' | 'mock' | 'topics' | 'review';

const TABS: { id: LearnTab; label: string; icon: string; desc: string }[] = [
  { id: 'practice', label: 'Практика', icon: '▶', desc: 'Адаптивный тест' },
  { id: 'mock', label: 'Mock Exam', icon: '', desc: 'Полный формат экзамена' },
  { id: 'topics', label: 'Темы', icon: '', desc: 'Уроки и практика по темам' },
  { id: 'review', label: 'Ошибки', icon: '', desc: 'Повторение ошибок' },
];

const isValidTab = (v: string | null): v is LearnTab =>
  v === 'practice' || v === 'mock' || v === 'topics' || v === 'review';

function LearnPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { selectedExams } = useAppSelector((state) => state.exam);

  const tabParam = searchParams.get('tab');
  const activeTab: LearnTab = isValidTab(tabParam) ? tabParam : 'practice';

  const switchTab = (tab: LearnTab) => {
    setSearchParams(tab === 'practice' ? {} : { tab }, { replace: true });
  };

  // If no exams selected, prompt user
  if (selectedExams.length === 0) {
    return (
      <div className="animate-fade-in" style={{ textAlign: 'center', padding: '4rem 1rem' }}>
        <h2>Выберите экзамен</h2>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
          Для начала обучения нужно выбрать хотя бы один экзамен
        </p>
        <button className="btn btn-primary" onClick={() => navigate('/profile')}>
          Выбрать экзамен
        </button>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ padding: '1.5rem 0' }}>
      {/* Tab bar */}
      <div className="learn-tabs">
        {TABS.map(tab => (
          <button
            key={tab.id}
            className={`learn-tab ${activeTab === tab.id ? 'learn-tab-active' : ''}`}
            onClick={() => switchTab(tab.id)}
          >
            <span className="learn-tab-icon">{tab.icon}</span>
            <span className="learn-tab-label">{tab.label}</span>
          </button>
        ))}
      </div>

      {/* Tab content */}
      <div style={{ marginTop: '0.5rem' }}>
        {activeTab === 'practice' && <TestPage />}
        {activeTab === 'mock' && <MockExamPage />}
        {activeTab === 'topics' && <TopicsPage />}
        {activeTab === 'review' && <ReviewPage />}
      </div>
    </div>
  );
}

export default LearnPage;
