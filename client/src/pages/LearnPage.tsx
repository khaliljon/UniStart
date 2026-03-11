import { useNavigate, useSearchParams } from 'react-router-dom';
import { useMemo } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTranslation } from '../hooks/useTranslation';
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

const ALL_TAB_IDS: LearnTab[] = ['topics', 'formulas', 'flashcards', 'strategies', 'practice', 'mock', 'drills', 'review', 'mistakes'];
const isValidTab = (v: string | null): v is LearnTab => ALL_TAB_IDS.includes(v as LearnTab);

function LearnPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { selectedExams } = useAppSelector((state) => state.exam);
  const { t } = useTranslation();

  const TAB_GROUPS: TabGroup[] = useMemo(() => [
    {
      label: t.learn.groupLearn,
      tabs: [
        { id: 'topics', label: t.learn.lessons, desc: t.learn.lessonsDesc },
        { id: 'formulas', label: t.learn.formulas, desc: t.learn.formulasDesc },
        { id: 'flashcards', label: t.learn.flashcards, desc: t.learn.flashcardsDesc },
        { id: 'strategies', label: t.learn.strategies, desc: t.learn.strategiesDesc },
      ],
    },
    {
      label: t.learn.groupTrain,
      tabs: [
        { id: 'practice', label: t.learn.practice, desc: t.learn.practiceDesc },
        { id: 'mock', label: t.learn.mockExam, desc: t.learn.mockExamDesc },
        { id: 'drills', label: t.learn.drills, desc: t.learn.drillsDesc },
        { id: 'review', label: t.learn.review, desc: t.learn.reviewDesc },
        { id: 'mistakes', label: t.learn.journal, desc: t.learn.journalDesc },
      ],
    },
  ], [t]);

  const tabParam = searchParams.get('tab');
  const activeTab: LearnTab = isValidTab(tabParam) ? tabParam : 'practice';

  const switchTab = (tab: LearnTab) => {
    setSearchParams(tab === 'practice' ? {} : { tab }, { replace: true });
  };

  // If no exams selected, prompt user
  if (selectedExams.length === 0) {
    return (
      <div className="animate-fade-in" style={{ textAlign: 'center', padding: '4rem 1rem' }}>
        <h2>{t.learn.selectExam}</h2>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
          {t.learn.selectExamDesc}
        </p>
        <button className="btn btn-primary" onClick={() => navigate('/profile')}>
          {t.learn.selectExam}
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
