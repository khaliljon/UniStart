import { useState, useEffect, useCallback } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { flashcardService } from '../services/flashcardService';
import type { FlashcardDeck, FlashcardReview } from '../types';
import MathText from '../components/MathRenderer';

type View = 'decks' | 'review' | 'create';

function FlashcardPage() {
  const { selectedExams } = useAppSelector((state) => state.exam);
  const [view, setView] = useState<View>('decks');
  const [decks, setDecks] = useState<FlashcardDeck[]>([]);
  const [, setActiveDeck] = useState<FlashcardDeck | null>(null);
  const [cards, setCards] = useState<FlashcardReview[]>([]);
  const [cardIndex, setCardIndex] = useState(0);
  const [flipped, setFlipped] = useState(false);
  const [loading, setLoading] = useState(true);

  // Create deck form
  const [newTitle, setNewTitle] = useState('');
  const [newDesc, setNewDesc] = useState('');

  const loadDecks = useCallback(async () => {
    setLoading(true);
    try {
      const data = await flashcardService.getDecks(selectedExams[0]);
      setDecks(data);
    } catch (err) {
      console.error('Failed to load decks:', err);
    } finally {
      setLoading(false);
    }
  }, [selectedExams]);

  useEffect(() => { loadDecks(); }, [loadDecks]);

  const startReview = async (deck: FlashcardDeck) => {
    try {
      const due = await flashcardService.getDueCards(deck.id);
      if (due.length === 0) {
        alert('No cards due for review in this deck.');
        return;
      }
      setActiveDeck(deck);
      setCards(due);
      setCardIndex(0);
      setFlipped(false);
      setView('review');
    } catch (err) {
      console.error('Failed to start review:', err);
    }
  };

  const handleRate = async (quality: number) => {
    const card = cards[cardIndex];
    try {
      await flashcardService.reviewCard({ flashcardId: card.id, quality });
      if (cardIndex < cards.length - 1) {
        setCardIndex(cardIndex + 1);
        setFlipped(false);
      } else {
        setView('decks');
        loadDecks();
      }
    } catch (err) {
      console.error('Failed to submit review:', err);
    }
  };

  const handleCreateDeck = async () => {
    if (!newTitle.trim()) return;
    try {
      await flashcardService.createDeck({
        title: newTitle.trim(),
        description: newDesc.trim() || undefined,
        examTypeCode: selectedExams[0],
      });
      setNewTitle('');
      setNewDesc('');
      setView('decks');
      loadDecks();
    } catch (err) {
      console.error('Failed to create deck:', err);
    }
  };

  const handleDeleteDeck = async (deckId: number) => {
    if (!confirm('Delete this deck and all its cards?')) return;
    try {
      await flashcardService.deleteDeck(deckId);
      loadDecks();
    } catch (err) {
      console.error('Failed to delete deck:', err);
    }
  };

  if (loading) {
    return <div className="loading-container"><div className="loading-spinner" /></div>;
  }

  // Review mode
  if (view === 'review' && cards.length > 0) {
    const card = cards[cardIndex];
    return (
      <div className="animate-fade-in">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <button className="btn btn-secondary" onClick={() => { setView('decks'); loadDecks(); }}>
            Back to Decks
          </button>
          <span style={{ color: 'var(--text-secondary)' }}>
            {cardIndex + 1} / {cards.length}
          </span>
        </div>

        <div
          className="card"
          style={{
            padding: '3rem 2rem', textAlign: 'center', cursor: 'pointer',
            minHeight: '200px', display: 'flex', flexDirection: 'column',
            justifyContent: 'center', alignItems: 'center'
          }}
          onClick={() => setFlipped(!flipped)}
        >
          <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginBottom: '1rem' }}>
            {flipped ? 'Answer' : 'Question'} -- click to flip
          </p>
          <div style={{ fontSize: '1.2rem', lineHeight: 1.6 }}>
            <MathText text={flipped ? card.back : card.front} as="div" />
          </div>
        </div>

        {flipped && (
          <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'center', marginTop: '1.5rem', flexWrap: 'wrap' }}>
            <button className="btn" style={{ background: '#dc3545', color: '#fff' }} onClick={() => handleRate(1)}>
              Forgot
            </button>
            <button className="btn" style={{ background: '#fd7e14', color: '#fff' }} onClick={() => handleRate(2)}>
              Hard
            </button>
            <button className="btn" style={{ background: '#ffc107', color: '#000' }} onClick={() => handleRate(3)}>
              OK
            </button>
            <button className="btn" style={{ background: '#28a745', color: '#fff' }} onClick={() => handleRate(4)}>
              Good
            </button>
            <button className="btn" style={{ background: '#007bff', color: '#fff' }} onClick={() => handleRate(5)}>
              Easy
            </button>
          </div>
        )}
      </div>
    );
  }

  // Create deck form
  if (view === 'create') {
    return (
      <div className="animate-fade-in">
        <button className="btn btn-secondary" onClick={() => setView('decks')} style={{ marginBottom: '1rem' }}>
          Back to Decks
        </button>
        <div className="card" style={{ padding: '1.5rem' }}>
          <h3 style={{ marginBottom: '1rem' }}>Create Deck</h3>
          <div style={{ marginBottom: '1rem' }}>
            <label className="form-label">Title</label>
            <input className="form-input" value={newTitle} onChange={e => setNewTitle(e.target.value)} placeholder="Deck title" />
          </div>
          <div style={{ marginBottom: '1rem' }}>
            <label className="form-label">Description (optional)</label>
            <textarea className="form-input" value={newDesc} onChange={e => setNewDesc(e.target.value)} placeholder="Description" rows={3} />
          </div>
          <button className="btn btn-primary" onClick={handleCreateDeck} disabled={!newTitle.trim()}>
            Create
          </button>
        </div>
      </div>
    );
  }

  // Decks list
  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h3 style={{ margin: 0 }}>Flashcard Decks</h3>
        <button className="btn btn-primary" onClick={() => setView('create')}>
          + New Deck
        </button>
      </div>

      {decks.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <p style={{ color: 'var(--text-secondary)' }}>No flashcard decks available</p>
        </div>
      ) : (
        <div style={{ display: 'grid', gap: '0.75rem', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))' }}>
          {decks.map(deck => (
            <div key={deck.id} className="card" style={{ padding: '1.25rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div>
                  <h4 style={{ margin: '0 0 0.25rem 0' }}>{deck.title}</h4>
                  {deck.description && <p style={{ margin: 0, fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{deck.description}</p>}
                </div>
                {deck.isSystem && <span style={{ fontSize: '0.7rem', padding: '2px 6px', background: 'var(--accent)', color: '#fff', borderRadius: '4px' }}>System</span>}
              </div>
              <div style={{ display: 'flex', gap: '1rem', margin: '0.75rem 0', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                <span>{deck.totalCards} cards</span>
                <span>{deck.dueCards} due</span>
                <span>{deck.masteredCards} mastered</span>
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button
                  className="btn btn-primary"
                  onClick={() => startReview(deck)}
                  disabled={deck.dueCards === 0}
                  style={{ flex: 1 }}
                >
                  Review ({deck.dueCards})
                </button>
                {!deck.isSystem && (
                  <button className="btn btn-secondary" onClick={() => handleDeleteDeck(deck.id)}>
                    Delete
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default FlashcardPage;
