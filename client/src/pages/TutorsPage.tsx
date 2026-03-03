import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { tutorService } from '../services/tutorService';
import type { TutorCard, TutorListResult } from '../types';

function TutorsPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const [result, setResult] = useState<TutorListResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState(searchParams.get('q') || '');
  const [examFilter, setExamFilter] = useState(searchParams.get('exam') || '');
  const [sortBy, setSortBy] = useState(searchParams.get('sort') || 'rating');
  const [onlyAvailable, setOnlyAvailable] = useState(searchParams.get('available') === '1');
  const [page, setPage] = useState(Number(searchParams.get('page')) || 1);

  const fetchTutors = useCallback(async () => {
    setLoading(true);
    try {
      const data = await tutorService.getTutors({
        search: search || undefined,
        examType: examFilter || undefined,
        sortBy,
        onlyAvailable: onlyAvailable || undefined,
        page,
        pageSize: 12,
      });
      setResult(data);
    } catch (err) {
      console.error('Failed to load tutors:', err);
    } finally {
      setLoading(false);
    }
  }, [search, examFilter, sortBy, onlyAvailable, page]);

  useEffect(() => {
    fetchTutors();
  }, [fetchTutors]);

  useEffect(() => {
    const params: Record<string, string> = {};
    if (search) params.q = search;
    if (examFilter) params.exam = examFilter;
    if (sortBy !== 'rating') params.sort = sortBy;
    if (onlyAvailable) params.available = '1';
    if (page > 1) params.page = String(page);
    setSearchParams(params, { replace: true });
  }, [search, examFilter, sortBy, onlyAvailable, page, setSearchParams]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchTutors();
  };

  const renderStars = (rating: number) => {
    const full = Math.floor(rating);
    const half = rating - full >= 0.5;
    const stars: string[] = [];
    for (let i = 0; i < full; i++) stars.push('★');
    if (half) stars.push('½');
    while (stars.length < 5) stars.push('☆');
    return stars.join('');
  };

  const renderCard = (tutor: TutorCard) => (
    <div
      key={tutor.userId}
      className="card"
      style={{
        cursor: 'pointer',
        transition: 'transform 0.2s, box-shadow 0.2s',
        display: 'flex',
        flexDirection: 'column',
        gap: '0.75rem',
      }}
      onClick={() => navigate(`/tutors/${tutor.userId}`)}
      onMouseEnter={(e) => {
        (e.currentTarget as HTMLElement).style.transform = 'translateY(-4px)';
        (e.currentTarget as HTMLElement).style.boxShadow = '0 8px 24px rgba(0,0,0,0.15)';
      }}
      onMouseLeave={(e) => {
        (e.currentTarget as HTMLElement).style.transform = '';
        (e.currentTarget as HTMLElement).style.boxShadow = '';
      }}
    >
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
        <div style={{
          width: '56px', height: '56px', borderRadius: '50%',
          background: 'linear-gradient(135deg, var(--primary-color), var(--primary-hover))',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          color: '#fff', fontWeight: 700, fontSize: '1.2rem', flexShrink: 0,
        }}>
          {tutor.name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2)}
        </div>
        <div style={{ flex: 1, minWidth: 0 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem' }}>
            <span style={{ fontWeight: 700, fontSize: '1.05rem' }}>{tutor.name}</span>
            {tutor.isVerified && (
              <span title="Верифицирован" style={{ color: '#3b82f6', fontSize: '1rem' }}>✓</span>
            )}
          </div>
          <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
            {tutor.headline || 'Тьютор'}
          </div>
        </div>
      </div>

      {/* Specializations */}
      {tutor.specializations.length > 0 && (
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.35rem' }}>
          {tutor.specializations.map(s => (
            <span key={s} style={{
              fontSize: '0.72rem', padding: '0.15rem 0.5rem', borderRadius: '999px',
              background: 'var(--bg-secondary)', color: 'var(--text-secondary)', fontWeight: 500,
            }}>{s}</span>
          ))}
        </div>
      )}

      {/* Bio snippet */}
      {tutor.bio && (
        <p style={{
          fontSize: '0.85rem', color: 'var(--text-secondary)', margin: 0,
          display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden',
        }}>
          {tutor.bio}
        </p>
      )}

      {/* Stats row */}
      <div style={{
        display: 'flex', alignItems: 'center', gap: '0.75rem', marginTop: 'auto',
        fontSize: '0.82rem', color: 'var(--text-secondary)',
      }}>
        <span style={{ color: '#f59e0b' }}>
          {renderStars(tutor.averageRating)}{' '}
          <strong style={{ color: 'var(--text-primary)' }}>{tutor.averageRating.toFixed(1)}</strong>
        </span>
        <span>{tutor.totalReviews}</span>
        <span>{tutor.totalStudents}</span>
        {tutor.hourlyRate != null && (
          <span style={{ marginLeft: 'auto', fontWeight: 600, color: 'var(--primary-color)' }}>
            {tutor.hourlyRate}₽/ч
          </span>
        )}
      </div>

      {/* Availability badge */}
      <div style={{
        fontSize: '0.75rem', fontWeight: 600,
        color: tutor.isAvailable ? '#10b981' : '#ef4444',
      }}>
        {tutor.isAvailable ? '● Доступен' : '○ Не доступен'}
      </div>
    </div>
  );

  return (
    <div className="animate-fade-in">
      <div style={{ marginBottom: '1.5rem' }}>
        <h1 style={{ margin: '0 0 0.25rem' }}>Тьюторы</h1>
        <p style={{ margin: 0, color: 'var(--text-secondary)' }}>
          Найдите опытного тьютора для подготовки к экзаменам
        </p>
      </div>

      {/* Filters bar */}
      <div className="card" style={{ display: 'flex', flexWrap: 'wrap', gap: '0.75rem', alignItems: 'center', marginBottom: '1.5rem' }}>
        <form onSubmit={handleSearch} style={{ flex: '1 1 200px', display: 'flex', gap: '0.5rem' }}>
          <input
            type="text"
            placeholder="Поиск по имени..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="form-input"
            style={{ flex: 1, padding: '0.5rem 0.75rem', fontSize: '0.9rem' }}
          />
          <button type="submit" className="btn btn-primary" style={{ padding: '0.5rem 1rem', fontSize: '0.85rem' }}>
            Поиск
          </button>
        </form>

        <select
          value={examFilter}
          onChange={(e) => { setExamFilter(e.target.value); setPage(1); }}
          className="form-input"
          style={{ padding: '0.5rem 0.75rem', fontSize: '0.85rem', minWidth: '150px' }}
        >
          <option value="">All exams</option>
          <option value="SAT">SAT</option>
          <option value="TOEFL">TOEFL</option>
          <option value="IELTS">IELTS</option>
          <option value="NUET">NUET</option>
          <option value="CSCA">CSCA</option>
        </select>

        <select
          value={sortBy}
          onChange={(e) => { setSortBy(e.target.value); setPage(1); }}
          className="form-input"
          style={{ padding: '0.5rem 0.75rem', fontSize: '0.85rem', minWidth: '150px' }}
        >
          <option value="rating">По рейтингу</option>
          <option value="reviews">По отзывам</option>
          <option value="students">По ученикам</option>
          <option value="price_asc">Цена ↑</option>
          <option value="price_desc">Цена ↓</option>
          <option value="newest">Новые</option>
        </select>

        <label style={{ display: 'flex', alignItems: 'center', gap: '0.35rem', fontSize: '0.85rem', cursor: 'pointer', whiteSpace: 'nowrap' }}>
          <input
            type="checkbox"
            checked={onlyAvailable}
            onChange={(e) => { setOnlyAvailable(e.target.checked); setPage(1); }}
          />
          Только доступные
        </label>
      </div>

      {/* Results */}
      {loading ? (
        <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
          Загрузка тьюторов...
        </div>
      ) : !result || result.items.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
          <div style={{ fontSize: '2.5rem', marginBottom: '0.75rem' }}></div>
          <h3>Тьюторы не найдены</h3>
          <p style={{ color: 'var(--text-secondary)' }}>Попробуйте изменить параметры поиска</p>
        </div>
      ) : (
        <>
          <div style={{ marginBottom: '0.75rem', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
            Найдено: {result.totalCount} тьютор(ов)
          </div>

          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))',
            gap: '1rem',
            marginBottom: '1.5rem',
          }}>
            {result.items.map(renderCard)}
          </div>

          {/* Pagination */}
          {result.totalPages > 1 && (
            <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem' }}>
              <button
                className="btn"
                disabled={page <= 1}
                onClick={() => setPage(p => p - 1)}
                style={{ padding: '0.5rem 1rem', fontSize: '0.85rem' }}
              >
                ← Назад
              </button>
              <span style={{ display: 'flex', alignItems: 'center', fontSize: '0.9rem' }}>
                {page} / {result.totalPages}
              </span>
              <button
                className="btn"
                disabled={page >= result.totalPages}
                onClick={() => setPage(p => p + 1)}
                style={{ padding: '0.5rem 1rem', fontSize: '0.85rem' }}
              >
                Вперёд →
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}

export default TutorsPage;
