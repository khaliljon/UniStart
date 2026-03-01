import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAppSelector } from '../hooks/useAppSelector';
import { tutorService } from '../services/tutorService';
import { messageService } from '../services/messageService';
import type { TutorProfileDetail, CreateReviewRequest } from '../types';

function TutorProfilePage() {
  const { userId } = useParams<{ userId: string }>();
  const navigate = useNavigate();
  const currentUser = useAppSelector((state) => state.auth.user);

  const [profile, setProfile] = useState<TutorProfileDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Review form
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState('');
  const [submittingReview, setSubmittingReview] = useState(false);
  const [reviewError, setReviewError] = useState('');
  const [reviewSuccess, setReviewSuccess] = useState(false);

  const [startingChat, setStartingChat] = useState(false);
  const [showRequestModal, setShowRequestModal] = useState(false);
  const [requestMessage, setRequestMessage] = useState('');
  const [requestSent, setRequestSent] = useState(false);

  const fetchProfile = useCallback(async () => {
    if (!userId) return;
    setLoading(true);
    try {
      const data = await tutorService.getTutorProfile(Number(userId));
      setProfile(data);
    } catch {
      setError('Не удалось загрузить профиль тьютора');
    } finally {
      setLoading(false);
    }
  }, [userId]);

  useEffect(() => {
    fetchProfile();
  }, [fetchProfile]);

  const handleStartChat = async () => {
    if (!profile) return;
    setStartingChat(true);
    try {
      const conversation = await messageService.startConversation(profile.userId, requestMessage || undefined);
      if (conversation.status === 'Pending') {
        setRequestSent(true);
        setShowRequestModal(false);
      } else {
        navigate(`/messages?c=${conversation.id}`);
      }
    } catch {
      alert('Не удалось отправить заявку');
    } finally {
      setStartingChat(false);
    }
  };

  const handleReview = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!profile) return;
    setSubmittingReview(true);
    setReviewError('');
    try {
      const data: CreateReviewRequest = { rating, comment: comment || undefined };
      await tutorService.leaveReview(profile.userId, data);
      setReviewSuccess(true);
      setComment('');
      fetchProfile(); // refresh reviews
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: string } })?.response?.data;
      setReviewError(typeof msg === 'string' ? msg : 'Не удалось оставить отзыв');
    } finally {
      setSubmittingReview(false);
    }
  };

  const renderStars = (r: number) => {
    return Array.from({ length: 5 }, (_, i) => (
      <span key={i} style={{ color: i < r ? '#f59e0b' : '#d1d5db', fontSize: '1.1rem' }}>★</span>
    ));
  };

  if (loading) {
    return <div className="animate-fade-in" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>Загрузка...</div>;
  }

  if (error || !profile) {
    return (
      <div className="animate-fade-in card" style={{ textAlign: 'center', padding: '3rem' }}>
        <div style={{ fontSize: '2.5rem', marginBottom: '0.75rem' }}>—</div>
        <h3>{error || 'Тьютор не найден'}</h3>
        <button className="btn btn-primary" onClick={() => navigate('/tutors')} style={{ marginTop: '1rem' }}>
          ← К списку тьюторов
        </button>
      </div>
    );
  }

  const isOwnProfile = currentUser?.id === profile.userId;

  return (
    <div className="animate-fade-in">
      <button
        onClick={() => navigate('/tutors')}
        className="btn"
        style={{ marginBottom: '1rem', fontSize: '0.85rem' }}
      >
        ← Все тьюторы
      </button>

      {/* Profile header */}
      <div className="card" style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap', marginBottom: '1rem' }}>
        <div style={{
          width: '80px', height: '80px', borderRadius: '50%',
          background: 'linear-gradient(135deg, var(--primary-color), var(--primary-hover))',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          color: '#fff', fontWeight: 700, fontSize: '1.8rem', flexShrink: 0,
        }}>
          {profile.name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2)}
        </div>

        <div style={{ flex: 1, minWidth: '200px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.25rem' }}>
            <h1 style={{ margin: 0, fontSize: '1.5rem' }}>{profile.name}</h1>
            {profile.isVerified && <span title="Верифицированный тьютор" style={{ color: '#3b82f6', fontSize: '1.3rem' }}>✓</span>}
            <span style={{
              fontSize: '0.75rem', fontWeight: 600, marginLeft: '0.25rem',
              color: profile.isAvailable ? '#10b981' : '#ef4444',
            }}>
              {profile.isAvailable ? '● Доступен' : '○ Не доступен'}
            </span>
          </div>
          <p style={{ margin: '0 0 0.5rem', color: 'var(--text-secondary)', fontSize: '1rem' }}>{profile.headline}</p>

          {/* Stats */}
          <div style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap', fontSize: '0.9rem' }}>
            <div>
              <span style={{ color: '#f59e0b' }}>★</span>{' '}
              <strong>{profile.averageRating.toFixed(1)}</strong>{' '}
              <span style={{ color: 'var(--text-secondary)' }}>({profile.totalReviews} отзывов)</span>
            </div>
            <div><strong>{profile.totalStudents}</strong> учеников</div>
            {profile.hourlyRate != null && (
              <div style={{ color: 'var(--primary-color)', fontWeight: 600 }}>{profile.hourlyRate}₽/час</div>
            )}
            <div style={{ color: 'var(--text-secondary)' }}>
              {profile.contactPreference === 'Chat' ? 'Чат' : profile.contactPreference === 'Email' ? 'Email' : 'Чат / Email'}
            </div>
          </div>

          {/* Specializations */}
          {profile.specializations.length > 0 && (
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.35rem', marginTop: '0.75rem' }}>
              {profile.specializations.map(s => (
                <span key={s} style={{
                  fontSize: '0.78rem', padding: '0.2rem 0.6rem', borderRadius: '999px',
                  background: 'var(--bg-secondary)', color: 'var(--text-secondary)', fontWeight: 500,
                }}>{s}</span>
              ))}
            </div>
          )}
        </div>

        {/* Action buttons */}
        {!isOwnProfile && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', alignSelf: 'flex-start' }}>
            {requestSent ? (
              <div style={{
                padding: '0.65rem 1.5rem', fontSize: '0.9rem', borderRadius: '8px',
                background: 'var(--bg-secondary)', color: 'var(--text-secondary)',
                textAlign: 'center', fontWeight: 500,
              }}>
                Заявка отправлена
              </div>
            ) : (
              <button
                className="btn btn-primary"
                onClick={() => setShowRequestModal(true)}
                disabled={startingChat}
                style={{ padding: '0.65rem 1.5rem', fontSize: '0.95rem' }}
              >
                Написать тьютору
              </button>
            )}
          </div>
        )}
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(350px, 1fr))', gap: '1rem' }}>
        {/* Left column */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {/* Bio */}
          {profile.bio && (
            <div className="card">
              <h3 style={{ margin: '0 0 0.75rem' }}>О себе</h3>
              <p style={{ margin: 0, whiteSpace: 'pre-line', lineHeight: 1.6, fontSize: '0.93rem' }}>{profile.bio}</p>
            </div>
          )}

          {/* Experience */}
          {profile.experience && (
            <div className="card">
              <h3 style={{ margin: '0 0 0.75rem' }}>Опыт</h3>
              <p style={{ margin: 0, whiteSpace: 'pre-line', lineHeight: 1.6, fontSize: '0.93rem' }}>{profile.experience}</p>
            </div>
          )}

          {/* Schedule */}
          {profile.schedule.length > 0 && (
            <div className="card">
              <h3 style={{ margin: '0 0 0.75rem' }}>Расписание</h3>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
                {profile.schedule.map(slot => (
                  <div key={slot.id} style={{
                    display: 'flex', justifyContent: 'space-between',
                    padding: '0.4rem 0.75rem', borderRadius: '8px',
                    background: 'var(--bg-secondary)', fontSize: '0.88rem',
                  }}>
                    <span style={{ fontWeight: 600 }}>{slot.dayName}</span>
                    <span style={{ color: 'var(--text-secondary)' }}>{slot.startTime} – {slot.endTime}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Right column — Reviews */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <div className="card">
            <h3 style={{ margin: '0 0 0.75rem' }}>
              Отзывы ({profile.totalReviews})
            </h3>

            {profile.recentReviews.length === 0 ? (
              <p style={{ color: 'var(--text-secondary)', margin: 0 }}>Пока нет отзывов</p>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                {profile.recentReviews.map(review => (
                  <div key={review.id} style={{
                    padding: '0.75rem', borderRadius: '8px',
                    background: 'var(--bg-secondary)',
                  }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.35rem' }}>
                      <span style={{ fontWeight: 600, fontSize: '0.88rem' }}>{review.studentName}</span>
                      <span style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                        {new Date(review.createdAt).toLocaleDateString('ru-RU')}
                      </span>
                    </div>
                    <div style={{ marginBottom: '0.25rem' }}>{renderStars(review.rating)}</div>
                    {review.comment && (
                      <p style={{ margin: 0, fontSize: '0.88rem', lineHeight: 1.5 }}>{review.comment}</p>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Leave a review */}
          {!isOwnProfile && !reviewSuccess && (
            <div className="card">
              <h3 style={{ margin: '0 0 0.75rem' }}>Оставить отзыв</h3>
              <form onSubmit={handleReview} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                <div>
                  <label style={{ fontSize: '0.85rem', marginBottom: '0.25rem', display: 'block' }}>Оценка</label>
                  <div style={{ display: 'flex', gap: '0.25rem' }}>
                    {[1, 2, 3, 4, 5].map(n => (
                      <button
                        key={n}
                        type="button"
                        onClick={() => setRating(n)}
                        style={{
                          background: 'none', border: 'none', cursor: 'pointer', padding: '0.15rem',
                          fontSize: '1.5rem', color: n <= rating ? '#f59e0b' : '#d1d5db',
                        }}
                      >★</button>
                    ))}
                  </div>
                </div>
                <div>
                  <label style={{ fontSize: '0.85rem', marginBottom: '0.25rem', display: 'block' }}>Комментарий (необязательно)</label>
                  <textarea
                    value={comment}
                    onChange={(e) => setComment(e.target.value)}
                    className="form-input"
                    rows={3}
                    maxLength={1000}
                    placeholder="Расскажите о вашем опыте..."
                    style={{ width: '100%', resize: 'vertical', fontSize: '0.9rem' }}
                  />
                </div>
                {reviewError && <div style={{ color: '#ef4444', fontSize: '0.85rem' }}>{reviewError}</div>}
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={submittingReview}
                  style={{ alignSelf: 'flex-start', padding: '0.5rem 1.5rem' }}
                >
                  {submittingReview ? 'Отправка...' : 'Отправить отзыв'}
                </button>
              </form>
            </div>
          )}

          {reviewSuccess && (
            <div className="card" style={{ background: '#10b981', color: '#fff', textAlign: 'center' }}>
              Спасибо за ваш отзыв!
            </div>
          )}
        </div>
      </div>

      {/* Request Modal */}
      {showRequestModal && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center',
          zIndex: 1000,
        }} onClick={() => setShowRequestModal(false)}>
          <div className="card" style={{ width: '100%', maxWidth: '480px', margin: '1rem' }} onClick={e => e.stopPropagation()}>
            <h3 style={{ margin: '0 0 0.5rem' }}>Заявка на обучение</h3>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', margin: '0 0 1rem' }}>
              Тьютор {profile.name} получит вашу заявку и решит, принять или отклонить.
              После принятия откроется чат.
            </p>
            <textarea
              className="form-input"
              rows={4}
              value={requestMessage}
              onChange={e => setRequestMessage(e.target.value)}
              placeholder="Расскажите о себе и ваших целях (необязательно)..."
              style={{ width: '100%', resize: 'vertical', fontSize: '0.9rem', marginBottom: '1rem' }}
            />
            <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
              <button className="btn" onClick={() => setShowRequestModal(false)}>Отмена</button>
              <button
                className="btn btn-primary"
                onClick={handleStartChat}
                disabled={startingChat}
              >
                {startingChat ? 'Отправка...' : 'Отправить заявку'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default TutorProfilePage;
