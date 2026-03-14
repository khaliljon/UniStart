import { useState, useEffect, useCallback } from 'react';
import { tutorService } from '../services/tutorService';
import type { TutorProfileDetail } from '../types';
import { getDateLocale } from '../i18n';

function TutorReviewsPage() {
  const [profile, setProfile] = useState<TutorProfileDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [filterRating, setFilterRating] = useState<number | null>(null);

  const loadProfile = useCallback(async () => {
    try {
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        const data = await tutorService.getTutorProfile(user.id);
        setProfile(data);
      }
    } catch {
      console.error('Failed to load profile');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadProfile();
  }, [loadProfile]);

  const formatDate = (dateStr: string) => {
    const d = new Date(dateStr);
    return d.toLocaleDateString(getDateLocale(), { day: 'numeric', month: 'long', year: 'numeric' });
  };

  const renderStars = (r: number) => (
    <span>
      {Array.from({ length: 5 }, (_, i) => (
        <span key={i} style={{ color: i < r ? '#f59e0b' : '#d1d5db', fontSize: '1rem' }}>★</span>
      ))}
    </span>
  );

  if (loading) {
    return <div className="animate-fade-in" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>Загрузка...</div>;
  }

  if (!profile) {
    return <div className="animate-fade-in card" style={{ textAlign: 'center', padding: '2rem' }}>Профиль не найден</div>;
  }

  const reviews = profile.recentReviews;
  const filtered = filterRating ? reviews.filter(r => r.rating === filterRating) : reviews;

  // Rating distribution
  const distribution = [5, 4, 3, 2, 1].map(star => ({
    star,
    count: reviews.filter(r => r.rating === star).length,
    percent: reviews.length > 0 ? (reviews.filter(r => r.rating === star).length / reviews.length) * 100 : 0,
  }));

  return (
    <div className="animate-fade-in">
      <h1 style={{ marginBottom: '1.5rem' }}>Отзывы</h1>

      {/* Summary */}
      <div className="card" style={{ marginBottom: '1rem' }}>
        <div style={{ display: 'flex', gap: '2rem', flexWrap: 'wrap', alignItems: 'center' }}>
          <div style={{ textAlign: 'center' }}>
            <div style={{ fontSize: '2.5rem', fontWeight: 700, color: '#f59e0b' }}>
              {profile.averageRating.toFixed(1)}
            </div>
            <div>{renderStars(Math.round(profile.averageRating))}</div>
            <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
              {profile.totalReviews} отзывов
            </div>
          </div>

          <div style={{ flex: 1, minWidth: '200px' }}>
            {distribution.map(d => (
              <div key={d.star} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.3rem' }}>
                <span style={{ width: '40px', fontSize: '0.82rem', textAlign: 'right' }}>{d.star} ★</span>
                <div style={{
                  flex: 1, height: '8px', borderRadius: '4px',
                  background: 'var(--bg-secondary)', overflow: 'hidden',
                }}>
                  <div style={{
                    width: `${d.percent}%`, height: '100%', borderRadius: '4px',
                    background: d.star >= 4 ? '#10b981' : d.star === 3 ? '#f59e0b' : '#ef4444',
                    transition: 'width 0.3s',
                  }} />
                </div>
                <span style={{ width: '30px', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                  {d.count}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Filter */}
      <div style={{ display: 'flex', gap: '0.4rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
        <button
          className={`btn ${filterRating === null ? 'btn-primary' : 'btn-outline'}`}
          onClick={() => setFilterRating(null)}
          style={{ padding: '0.35rem 0.75rem', fontSize: '0.82rem' }}
        >
          Все ({reviews.length})
        </button>
        {[5, 4, 3, 2, 1].map(star => {
          const count = reviews.filter(r => r.rating === star).length;
          return (
            <button
              key={star}
              className={`btn ${filterRating === star ? 'btn-primary' : 'btn-outline'}`}
              onClick={() => setFilterRating(star)}
              style={{ padding: '0.35rem 0.75rem', fontSize: '0.82rem' }}
            >
              {star}★ ({count})
            </button>
          );
        })}
      </div>

      {/* Reviews list */}
      {filtered.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
          <div style={{ fontSize: '2.5rem', marginBottom: '0.5rem' }}>★</div>
          {filterRating ? 'Нет отзывов с таким рейтингом' : 'Пока нет отзывов'}
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {filtered.map(review => (
            <div key={review.id} className="card" style={{ padding: '1rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.5rem' }}>
                <div>
                  <div style={{ fontWeight: 600 }}>{review.studentName}</div>
                  <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                    {formatDate(review.createdAt)}
                  </div>
                </div>
                {renderStars(review.rating)}
              </div>
              {review.comment && (
                <p style={{ margin: 0, fontSize: '0.9rem', color: 'var(--text-primary)' }}>
                  {review.comment}
                </p>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

export default TutorReviewsPage;
