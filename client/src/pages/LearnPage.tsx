import MockExamPage from './MockExamPage';

// Temporarily reduced to the mock-exam view only (per product decision).
// Other tabs — practice, drills, review, lessons, formulas, flashcards,
// strategies, journal — are hidden for now. Their page components still exist
// and can be re-enabled later.
function LearnPage() {
  return (
    <div className="animate-fade-in" style={{ padding: '1.5rem 0' }}>
      <MockExamPage />
    </div>
  );
}

export default LearnPage;
