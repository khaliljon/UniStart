import { useState, useEffect, useCallback } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { mistakeService } from '../services/mistakeService';
import type { MistakeEntry, MistakeAnalysis } from '../types';

const ERROR_TYPES = ['Careless', 'KnowledgeGap', 'TimePressure', 'TrickQuestion'];

function MistakeJournalPage() {
  const { selectedExams } = useAppSelector((state) => state.exam);
  const [mistakes, setMistakes] = useState<MistakeEntry[]>([]);
  const [analysis, setAnalysis] = useState<MistakeAnalysis | null>(null);
  const [loading, setLoading] = useState(true);
  const [showAnalysis, setShowAnalysis] = useState(false);
  const [filterType, setFilterType] = useState<string>('all');
  const [page, setPage] = useState(1);
  const [editingNote, setEditingNote] = useState<number | null>(null);
  const [noteText, setNoteText] = useState('');
  const examCode = selectedExams[0] || '';

  const loadMistakes = useCallback(async () => {
    setLoading(true);
    try {
      const errorType = filterType === 'all' ? undefined : filterType === 'unclassified' ? 'unclassified' : filterType;
      const data = await mistakeService.getMistakes({
        examTypeCode: examCode || undefined,
        errorType,
        page,
        pageSize: 20,
      });
      setMistakes(data);
    } catch (err) {
      console.error('Failed to load mistakes:', err);
    } finally {
      setLoading(false);
    }
  }, [examCode, filterType, page]);

  useEffect(() => { loadMistakes(); }, [loadMistakes]);

  const loadAnalysis = async () => {
    try {
      const data = await mistakeService.getAnalysis(examCode || undefined);
      setAnalysis(data);
      setShowAnalysis(true);
    } catch (err) {
      console.error('Failed to load analysis:', err);
    }
  };

  const handleSetErrorType = async (userAnswerId: number, errorType: string | null) => {
    try {
      await mistakeService.setErrorType(userAnswerId, errorType);
      setMistakes(prev => prev.map(m =>
        m.userAnswerId === userAnswerId ? { ...m, errorType } : m
      ));
    } catch (err) {
      console.error('Failed to set error type:', err);
    }
  };

  const handleSaveNote = async (userAnswerId: number) => {
    try {
      await mistakeService.setNote(userAnswerId, noteText);
      setMistakes(prev => prev.map(m =>
        m.userAnswerId === userAnswerId ? { ...m, noteText } : m
      ));
      setEditingNote(null);
      setNoteText('');
    } catch (err) {
      console.error('Failed to save note:', err);
    }
  };

  if (loading) {
    return <div className="loading-container"><div className="loading-spinner" /></div>;
  }

  if (showAnalysis && analysis) {
    return (
      <div className="animate-fade-in">
        <button className="btn btn-secondary" onClick={() => setShowAnalysis(false)} style={{ marginBottom: '1rem' }}>
          Back to Journal
        </button>
        <h3>Mistake Analysis</h3>
        <div className="card" style={{ padding: '1.5rem', marginBottom: '1rem' }}>
          <h4 style={{ margin: '0 0 1rem' }}>Total mistakes: {analysis.totalMistakes}</h4>

          {analysis.errorPatterns.length > 0 && (
            <>
              <h4 style={{ margin: '1rem 0 0.5rem' }}>Error Patterns</h4>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                {analysis.errorPatterns.map(p => (
                  <div key={p.errorType} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <div style={{ width: '120px', fontSize: '0.85rem' }}>{p.errorType}</div>
                    <div style={{ flex: 1, background: 'var(--bg-secondary)', borderRadius: '4px', height: '20px', overflow: 'hidden' }}>
                      <div style={{ width: `${p.percentage}%`, height: '100%', background: 'var(--accent)', borderRadius: '4px' }} />
                    </div>
                    <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', width: '60px', textAlign: 'right' }}>
                      {p.count} ({p.percentage}%)
                    </span>
                  </div>
                ))}
              </div>
            </>
          )}
        </div>

        {analysis.topicBreakdown.length > 0 && (
          <div className="card" style={{ padding: '1.5rem' }}>
            <h4 style={{ margin: '0 0 0.5rem' }}>Topics with Most Mistakes</h4>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
              {analysis.topicBreakdown.slice(0, 10).map(t => (
                <div key={t.topicId} style={{ display: 'flex', justifyContent: 'space-between', padding: '0.25rem 0', fontSize: '0.9rem' }}>
                  <span>{t.topicName}</span>
                  <span style={{ color: 'var(--text-secondary)' }}>{t.mistakeCount}</span>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    );
  }

  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h3 style={{ margin: 0 }}>Mistake Journal</h3>
        <button className="btn btn-secondary" onClick={loadAnalysis}>Analysis</button>
      </div>

      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        {['all', 'unclassified', ...ERROR_TYPES].map(type => (
          <button
            key={type}
            className={`btn ${filterType === type ? 'btn-primary' : 'btn-secondary'}`}
            style={{ fontSize: '0.8rem', padding: '0.25rem 0.75rem' }}
            onClick={() => { setFilterType(type); setPage(1); }}
          >
            {type === 'all' ? 'All' : type}
          </button>
        ))}
      </div>

      {mistakes.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <p style={{ color: 'var(--text-secondary)' }}>
            {filterType === 'all' ? 'No mistakes recorded yet' : `No ${filterType} mistakes found`}
          </p>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {mistakes.map(m => (
            <div key={m.userAnswerId} className="card" style={{ padding: '1.25rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                <div>
                  <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{m.topicName} / {m.difficulty}</span>
                </div>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                  {new Date(m.answeredAt).toLocaleDateString()}
                </span>
              </div>

              <p style={{ margin: '0 0 0.5rem', lineHeight: 1.5 }}>{m.questionText}</p>

              <div style={{ display: 'flex', gap: '1rem', fontSize: '0.85rem', marginBottom: '0.5rem' }}>
                <span style={{ color: '#dc3545' }}>Your answer: {m.userAnswerText || '(none)'}</span>
                <span style={{ color: '#28a745' }}>Correct: {m.correctAnswerText || '(unknown)'}</span>
              </div>

              {m.explanation && (
                <p style={{ margin: '0 0 0.5rem', fontSize: '0.85rem', color: 'var(--text-secondary)', fontStyle: 'italic' }}>
                  {m.explanation}
                </p>
              )}

              <div style={{ display: 'flex', gap: '0.25rem', marginBottom: '0.5rem', flexWrap: 'wrap' }}>
                {ERROR_TYPES.map(type => (
                  <button
                    key={type}
                    className={`btn ${m.errorType === type ? 'btn-primary' : 'btn-secondary'}`}
                    style={{ fontSize: '0.7rem', padding: '2px 8px' }}
                    onClick={() => handleSetErrorType(m.userAnswerId, m.errorType === type ? null : type)}
                  >
                    {type}
                  </button>
                ))}
              </div>

              {editingNote === m.userAnswerId ? (
                <div style={{ display: 'flex', gap: '0.5rem' }}>
                  <input
                    className="form-input"
                    value={noteText}
                    onChange={e => setNoteText(e.target.value)}
                    placeholder="Write a note about this mistake..."
                    style={{ flex: 1 }}
                  />
                  <button className="btn btn-primary" onClick={() => handleSaveNote(m.userAnswerId)} disabled={!noteText.trim()}>Save</button>
                  <button className="btn btn-secondary" onClick={() => { setEditingNote(null); setNoteText(''); }}>Cancel</button>
                </div>
              ) : (
                <div
                  style={{ fontSize: '0.85rem', color: m.noteText ? 'var(--text-primary)' : 'var(--text-secondary)', cursor: 'pointer' }}
                  onClick={() => { setEditingNote(m.userAnswerId); setNoteText(m.noteText || ''); }}
                >
                  {m.noteText || '+ Add note'}
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {mistakes.length === 20 && (
        <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginTop: '1rem' }}>
          <button className="btn btn-secondary" onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1}>
            Prev
          </button>
          <span style={{ padding: '0.5rem', color: 'var(--text-secondary)' }}>Page {page}</span>
          <button className="btn btn-secondary" onClick={() => setPage(p => p + 1)}>
            Next
          </button>
        </div>
      )}
    </div>
  );
}

export default MistakeJournalPage;
