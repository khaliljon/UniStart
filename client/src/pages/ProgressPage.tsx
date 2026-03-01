import { useSearchParams } from 'react-router-dom';
import AnalyticsPage from './AnalyticsPage';
import PredictionPage from './PredictionPage';
import HistoryPage from './HistoryPage';

type ProgressTab = 'overview' | 'prediction' | 'history';

const TABS: { id: ProgressTab; label: string; icon: string }[] = [
  { id: 'overview', label: 'Обзор', icon: '' },
  { id: 'prediction', label: 'Прогноз', icon: '' },
  { id: 'history', label: 'История', icon: '' },
];

const isValidTab = (v: string | null): v is ProgressTab =>
  v === 'overview' || v === 'prediction' || v === 'history';

function ProgressPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const tabParam = searchParams.get('tab');
  const activeTab: ProgressTab = isValidTab(tabParam) ? tabParam : 'overview';

  const switchTab = (tab: ProgressTab) => {
    setSearchParams(tab === 'overview' ? {} : { tab }, { replace: true });
  };

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
        {activeTab === 'overview' && <AnalyticsPage />}
        {activeTab === 'prediction' && <PredictionPage />}
        {activeTab === 'history' && <HistoryPage />}
      </div>
    </div>
  );
}

export default ProgressPage;
