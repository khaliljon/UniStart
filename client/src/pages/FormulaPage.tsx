import { useState, useEffect, useCallback } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { formulaService } from '../services/formulaService';
import type { FormulaCard } from '../types';
import MathText, { renderMarkdown } from '../components/MathRenderer';

function FormulaPage() {
  const { selectedExams } = useAppSelector((state) => state.exam);
  const [formulas, setFormulas] = useState<FormulaCard[]>([]);
  const [loading, setLoading] = useState(true);
  const [showBookmarksOnly, setShowBookmarksOnly] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  const loadFormulas = useCallback(async () => {
    if (selectedExams.length === 0) return;
    setLoading(true);
    try {
      const data = showBookmarksOnly
        ? await formulaService.getBookmarks()
        : await formulaService.getFormulas(selectedExams);
      setFormulas(data);
    } catch (err) {
      console.error('Failed to load formulas:', err);
    } finally {
      setLoading(false);
    }
  }, [selectedExams, showBookmarksOnly]);

  useEffect(() => { loadFormulas(); }, [loadFormulas]);

  const handleToggleBookmark = async (formulaId: number) => {
    try {
      const result = await formulaService.toggleBookmark(formulaId);
      setFormulas(prev =>
        prev.map(f => f.id === formulaId ? { ...f, isBookmarked: result.isBookmarked } : f)
          .filter(f => !showBookmarksOnly || f.isBookmarked)
      );
    } catch (err) {
      console.error('Failed to toggle bookmark:', err);
    }
  };

  const filtered = formulas.filter(f =>
    f.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
    f.topicName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    f.formula.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Group by exam → topic (two-level hierarchy)
  const groupedByExam = filtered.reduce<Record<string, Record<string, FormulaCard[]>>>((acc, f) => {
    const exam = f.examTypeCode || 'Other';
    const topic = f.topicName;
    (acc[exam] ??= {})[topic] ??= [];
    acc[exam][topic].push(f);
    return acc;
  }, {});

  const examKeys = Object.keys(groupedByExam).sort();
  const hasMultipleExams = examKeys.length > 1;

  if (loading) {
    return <div className="loading-container"><div className="loading-spinner" /></div>;
  }

  return (
    <div className="animate-fade-in">
      {/* Controls */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        <input
          type="text"
          placeholder="Search formulas..."
          value={searchTerm}
          onChange={e => setSearchTerm(e.target.value)}
          className="form-input"
          style={{ flex: 1, minWidth: '200px' }}
        />
        <button
          className={`btn ${showBookmarksOnly ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setShowBookmarksOnly(!showBookmarksOnly)}
        >
          {showBookmarksOnly ? 'Bookmarks' : 'All'}
        </button>
      </div>

      {filtered.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <p style={{ color: 'var(--text-secondary)' }}>
            {showBookmarksOnly ? 'No bookmarked formulas yet' : 'No formulas available for this exam'}
          </p>
        </div>
      ) : (
        examKeys.map(examCode => (
          <div key={examCode} style={{ marginBottom: hasMultipleExams ? '2rem' : 0 }}>
            {hasMultipleExams && (
              <h2 style={{ marginBottom: '1rem', fontSize: '1.1rem', borderBottom: '2px solid var(--primary-color)', paddingBottom: '0.5rem' }}>
                {examCode}
              </h2>
            )}
            {Object.entries(groupedByExam[examCode]).map(([topicName, cards]) => (
              <div key={topicName} style={{ marginBottom: '1.5rem' }}>
                <h3 style={{ marginBottom: '0.75rem', color: 'var(--text-secondary)', fontSize: '0.85rem', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                  {topicName}
                </h3>
                <div style={{ display: 'grid', gap: '0.75rem', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))' }}>
                  {cards.map(f => (
                    <div key={f.id} className="card" style={{ padding: '1.25rem' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                        <h4 style={{ margin: 0, fontSize: '0.95rem' }}>{f.title}</h4>
                        <button
                          className="btn-icon"
                          onClick={() => handleToggleBookmark(f.id)}
                          title={f.isBookmarked ? 'Remove bookmark' : 'Add bookmark'}
                          style={{ opacity: f.isBookmarked ? 1 : 0.5, fontSize: '1.2rem', background: 'none', border: 'none', cursor: 'pointer', color: f.isBookmarked ? 'var(--warning-color)' : 'var(--text-secondary)' }}
                        >
                          {f.isBookmarked ? '\u2605' : '\u2606'}
                        </button>
                      </div>
                      <div
                        style={{
                          margin: '0.75rem 0', padding: '0.75rem',
                          background: 'var(--bg-secondary)', borderRadius: '8px',
                          fontSize: '1.1rem', textAlign: 'center',
                          overflowX: 'auto'
                        }}
                      >
                        <MathText text={`$${f.formula}$`} />
                      </div>
                      {f.description && (
                        <p style={{ margin: 0, fontSize: '0.85rem', color: 'var(--text-secondary)' }}
                          dangerouslySetInnerHTML={{ __html: renderMarkdown(f.description) }} />
                      )}
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        ))
      )}
    </div>
  );
}

export default FormulaPage;
