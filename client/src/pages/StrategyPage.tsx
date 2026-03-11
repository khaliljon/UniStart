import { useState, useEffect, useCallback } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { strategyService } from '../services/strategyService';
import type { StrategyGuideSummary, StrategyGuide } from '../types';
import { ContentRenderer } from '../components/MathRenderer';

function StrategyPage() {
  const { selectedExams } = useAppSelector((state) => state.exam);
  const [guides, setGuides] = useState<StrategyGuideSummary[]>([]);
  const [activeGuide, setActiveGuide] = useState<StrategyGuide | null>(null);
  const [loading, setLoading] = useState(true);
  const [filterCategory, setFilterCategory] = useState<string>('all');

  const loadGuides = useCallback(async () => {
    if (selectedExams.length === 0) return;
    setLoading(true);
    try {
      const data = await strategyService.getGuides(selectedExams);
      setGuides(data);
    } catch (err) {
      console.error('Failed to load guides:', err);
    } finally {
      setLoading(false);
    }
  }, [selectedExams]);

  useEffect(() => { loadGuides(); }, [loadGuides]);

  const openGuide = async (guideId: number) => {
    try {
      const guide = await strategyService.getGuide(guideId);
      setActiveGuide(guide);
      if (!guide.isRead) {
        await strategyService.markRead(guideId);
        setGuides(prev => prev.map(g => g.id === guideId ? { ...g, isRead: true } : g));
      }
    } catch (err) {
      console.error('Failed to load guide:', err);
    }
  };

  if (loading) {
    return <div className="loading-container"><div className="loading-spinner" /></div>;
  }

  // Guide detail view
  if (activeGuide) {
    return (
      <div className="animate-fade-in">
        <button className="btn btn-secondary" onClick={() => setActiveGuide(null)} style={{ marginBottom: '1rem' }}>
          Back to Guides
        </button>
        <div className="card" style={{ padding: '2rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1rem' }}>
            <h2 style={{ margin: 0 }}>{activeGuide.title}</h2>
            <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', whiteSpace: 'nowrap' }}>
              ~{activeGuide.estimatedReadMinutes} min read
            </span>
          </div>
          <span style={{
            display: 'inline-block', fontSize: '0.75rem', padding: '2px 8px',
            borderRadius: '4px', background: 'var(--bg-secondary)', color: 'var(--text-secondary)',
            marginBottom: '1rem'
          }}>
            {activeGuide.category}
          </span>
          <p style={{ color: 'var(--text-secondary)', fontStyle: 'italic', marginBottom: '1.5rem' }}>
            {activeGuide.summary}
          </p>
          <ContentRenderer content={activeGuide.content} style={{ lineHeight: 1.8 }} />
        </div>
      </div>
    );
  }

  const categories = ['all', ...new Set(guides.map(g => g.category))];
  const filtered = filterCategory === 'all' ? guides : guides.filter(g => g.category === filterCategory);
  const readCount = guides.filter(g => g.isRead).length;

  // Group by exam → list
  const groupedByExam = filtered.reduce<Record<string, StrategyGuideSummary[]>>((acc, g) => {
    (acc[g.examTypeCode] ??= []).push(g);
    return acc;
  }, {});

  const examKeys = Object.keys(groupedByExam).sort();
  const hasMultipleExams = examKeys.length > 1;

  // Guide list view
  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h3 style={{ margin: 0 }}>Strategy Guides</h3>
        <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
          {readCount} / {guides.length} read
        </span>
      </div>

      {/* Category filter */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        {categories.map(cat => (
          <button
            key={cat}
            className={`btn ${filterCategory === cat ? 'btn-primary' : 'btn-secondary'}`}
            style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem' }}
            onClick={() => setFilterCategory(cat)}
          >
            {cat === 'all' ? 'All' : cat}
          </button>
        ))}
      </div>

      {filtered.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <p style={{ color: 'var(--text-secondary)' }}>No strategy guides available for this exam</p>
        </div>
      ) : (
        examKeys.map(examCode => (
          <div key={examCode} style={{ marginBottom: hasMultipleExams ? '2rem' : 0 }}>
            {hasMultipleExams && (
              <h3 style={{ marginBottom: '0.75rem', fontSize: '1rem', borderBottom: '2px solid var(--primary-color)', paddingBottom: '0.5rem' }}>
                {examCode}
              </h3>
            )}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              {groupedByExam[examCode].map(g => (
                <div
                  key={g.id}
                  className="card"
                  style={{ padding: '1rem 1.25rem', cursor: 'pointer', opacity: g.isRead ? 0.7 : 1 }}
                  onClick={() => openGuide(g.id)}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    <div>
                      <h4 style={{ margin: '0 0 0.25rem' }}>
                        {g.isRead && <span style={{ color: 'var(--success)', marginRight: '0.5rem' }}>[done]</span>}
                        {g.title}
                      </h4>
                      <p style={{ margin: 0, fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{g.summary}</p>
                    </div>
                    <div style={{ textAlign: 'right', whiteSpace: 'nowrap', marginLeft: '1rem' }}>
                      <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{g.category}</span>
                      <br />
                      <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>~{g.estimatedReadMinutes} min</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))
      )}
    </div>
  );
}

export default StrategyPage;
