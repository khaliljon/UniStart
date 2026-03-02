import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAppSelector } from '../hooks/useAppSelector';
import TestPage from './TestPage';
import MockExamPage from './MockExamPage';
import TopicsPage from './TopicsPage';
import ReviewPage from './ReviewPage';
import FormulaPage from './FormulaPage';
import FlashcardPage from './FlashcardPage';
import TimedDrillPage from './TimedDrillPage';
import StrategyPage from './StrategyPage';
import MistakeJournalPage from './MistakeJournalPage';

type LearnTab = 'practice' | 'mock' | 'topics' | 'review' | 'formulas' | 'flashcards' | 'drills' | 'strategies' | 'mistakes';

interface TabGroup {
  label: string;
  tabs: { id: LearnTab; label: string; desc: string }[];
}

const TAB_GROUPS: TabGroup[] = [
  {
    label: 'Learn',
    tabs: [
      { id: 'topics', label: 'Lessons', desc: 'Structured lessons by topic' },
      { id: 'formulas', label: 'Formulas', desc: 'Formula reference cards' },
      { id: 'flashcards', label: 'Flashcards', desc: 'Spaced repetition cards' },
      { id: 'strategies', label: 'Strategies', desc: 'Exam strategy guides' },
    ],
  },
  {
    label: 'Train',
    tabs: [
      { id: 'practice', label: 'Practice', desc: 'Adaptive practice' },
      { id: 'mock', label: 'Mock Exam', desc: 'Full exam format' },
      { id: 'drills', label: 'Drills', desc: 'Timed drill challenges' },
      { id: 'review', label: 'Review', desc: 'Review wrong answers' },
      { id: 'mistakes', label: 'Journal', desc: 'Mistake analysis' },
    ],
  },
];

const ALL_TAB_IDS = TAB_GROUPS.flatMap(g => g.tabs.map(t => t.id));
const isValidTab = (v: string | null): v is LearnTab => ALL_TAB_IDS.includes(v as LearnTab);

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
        <h2>Select an Exam</h2>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
          Choose at least one exam to start learning
        </p>
        <button className="btn btn-primary" onClick={() => navigate('/profile')}>
          Select Exam
        </button>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ padding: '1.5rem 0' }}>
      {/* Tab groups */}
      {TAB_GROUPS.map(group => (
        <div key={group.label} style={{ marginBottom: '0.5rem' }}>
          <div style={{ fontSize: '0.7rem', color: 'var(--text-secondary)', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: '0.25rem', paddingLeft: '0.25rem' }}>
            {group.label}
          </div>
          <div className="learn-tabs" style={{ marginBottom: '0.25rem' }}>
            {group.tabs.map(tab => (
              <button
                key={tab.id}
                className={`learn-tab ${activeTab === tab.id ? 'learn-tab-active' : ''}`}
                onClick={() => switchTab(tab.id)}
                title={tab.desc}
              >
                <span className="learn-tab-label">{tab.label}</span>
              </button>
            ))}
          </div>
        </div>
      ))}

      {/* Tab content */}
      <div style={{ marginTop: '0.5rem' }}>
        {activeTab === 'practice' && <TestPage />}
        {activeTab === 'mock' && <MockExamPage />}
        {activeTab === 'topics' && <TopicsPage />}
        {activeTab === 'review' && <ReviewPage />}
        {activeTab === 'formulas' && <FormulaPage />}
        {activeTab === 'flashcards' && <FlashcardPage />}
        {activeTab === 'drills' && <TimedDrillPage />}
        {activeTab === 'strategies' && <StrategyPage />}
        {activeTab === 'mistakes' && <MistakeJournalPage />}
      </div>
    </div>
  );
}

export default LearnPage;
