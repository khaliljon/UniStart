import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { tutorService } from '../services/tutorService';
import { useTranslation } from '../i18n';
import { useBranding } from '../contexts/BrandingContext';
import type { TutorSchoolDetail, TutorCard } from '../types';

export default function SchoolDetailPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { isWhiteLabel } = useBranding();
  const [school, setSchool] = useState<TutorSchoolDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!slug) return;
    (async () => {
      try {
        setLoading(true);
        const data = await tutorService.getSchool(slug);
        setSchool(data);
      } catch (err) {
        console.error('Failed to load school:', err);
      } finally {
        setLoading(false);
      }
    })();
  }, [slug]);

  const renderStars = (rating: number) => {
    const full = Math.floor(rating);
    const half = rating - full >= 0.5;
    const stars: string[] = [];
    for (let i = 0; i < full; i++) stars.push('★');
    if (half) stars.push('½');
    while (stars.length < 5) stars.push('☆');
    return stars.join('');
  };

  const renderTutorCard = (tutor: TutorCard) => (
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
              <span title={t.tutor.verified} style={{ color: '#3b82f6', fontSize: '1rem' }}>✓</span>
            )}
          </div>
          <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
            {tutor.headline || t.tutor.defaultHeadline}
          </div>
        </div>
      </div>

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

      {tutor.bio && (
        <p style={{
          fontSize: '0.85rem', color: 'var(--text-secondary)', margin: 0,
          display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden',
        }}>
          {tutor.bio}
        </p>
      )}

      <div style={{
        display: 'flex', alignItems: 'center', gap: '0.75rem', marginTop: 'auto',
        fontSize: '0.82rem', color: 'var(--text-secondary)',
      }}>
        <span style={{ color: '#f59e0b' }}>
          {renderStars(tutor.averageRating)}{' '}
          <strong style={{ color: 'var(--text-primary)' }}>{tutor.averageRating.toFixed(1)}</strong>
        </span>
        <span>{tutor.totalReviews} {t.tutor.reviewsCount}</span>
        <span>{tutor.totalStudents} {t.tutor.studentsCount}</span>
        {tutor.hourlyRate != null && (
          <span style={{ marginLeft: 'auto', fontWeight: 600, color: 'var(--primary-color)' }}>
            {tutor.hourlyRate}{t.tutor.perHourShort}
          </span>
        )}
      </div>

      <div style={{
        fontSize: '0.75rem', fontWeight: 600,
        color: tutor.isAvailable ? '#10b981' : '#ef4444',
      }}>
        {tutor.isAvailable ? t.tutor.available : t.tutor.unavailable}
      </div>
    </div>
  );

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
        {t.tutor.loading}
      </div>
    );
  }

  if (!school) {
    return (
      <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
        <h3>{t.tutor.schoolNotFound}</h3>
        <button onClick={() => navigate('/tutors')} className="btn btn-outline" style={{ marginTop: '1rem' }}>
          &larr; {t.tutor.backToTutors}
        </button>
      </div>
    );
  }

  return (
    <div className="animate-fade-in">
      {/* Back button */}
      <button onClick={() => navigate('/tutors')} className="btn btn-outline" style={{ marginBottom: '1.5rem' }}>
        &larr; {t.tutor.backToTutors}
      </button>

      {/* School header card */}
      <div className="card animate-fade-in-up" style={{ marginBottom: '2rem', padding: '2rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1rem' }}>
          <div style={{
            width: '72px', height: '72px', borderRadius: '1rem',
            background: 'linear-gradient(135deg, #ec4899, #f43f5e)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            color: '#fff', fontWeight: 700, fontSize: '2rem', flexShrink: 0,
          }}>
            {school.logoUrl ? (
              <img src={school.logoUrl} alt={school.name} style={{ width: '100%', height: '100%', borderRadius: '1rem', objectFit: 'cover' }} />
            ) : (
              ''
            )}
          </div>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <h1 style={{ margin: 0, fontSize: '1.5rem', fontWeight: 700 }}>{school.name}</h1>
              {!isWhiteLabel && school.isPartner && (
                <span style={{
                  fontSize: '0.7rem',
                  fontWeight: 600,
                  padding: '0.15rem 0.5rem',
                  borderRadius: '999px',
                  backgroundColor: 'rgba(79, 70, 229, 0.1)',
                  color: 'var(--primary-color)',
                }}>{t.tutor.partnerBadge}</span>
              )}
            </div>
            <p style={{ margin: '0.25rem 0 0', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
              {school.tutors.length} {school.tutors.length === 1 ? t.tutor.tutorCount : t.tutor.tutorsCount}
            </p>
          </div>
        </div>

        {school.description && (
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1rem', lineHeight: 1.6 }}>
            {school.description}
          </p>
        )}

        {school.specializations.length > 0 && (
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem', marginBottom: '1rem' }}>
            {school.specializations.map(s => (
              <span key={s} style={{
                fontSize: '0.8rem', padding: '0.2rem 0.6rem', borderRadius: '999px',
                background: 'var(--bg-secondary)', color: 'var(--text-secondary)', fontWeight: 500,
              }}>{s}</span>
            ))}
          </div>
        )}

        {/* Social links */}
        <div style={{ display: 'flex', gap: '0.75rem', fontSize: '0.9rem' }}>
          {school.instagramUrl && (
            <a href={school.instagramUrl} target="_blank" rel="noopener noreferrer"
              style={{ color: 'var(--primary-color)', textDecoration: 'none', fontWeight: 500 }}>
              Instagram
            </a>
          )}
          {school.telegramUrl && (
            <a href={school.telegramUrl} target="_blank" rel="noopener noreferrer"
              style={{ color: 'var(--primary-color)', textDecoration: 'none', fontWeight: 500 }}>
              Telegram
            </a>
          )}
          {school.websiteUrl && (
            <a href={school.websiteUrl} target="_blank" rel="noopener noreferrer"
              style={{ color: 'var(--primary-color)', textDecoration: 'none', fontWeight: 500 }}>
              {t.tutor.websiteLink}
            </a>
          )}
        </div>
      </div>

      {/* School's Tutors */}
      <h2 style={{ fontSize: '1.15rem', fontWeight: 600, marginBottom: '1rem' }}>
        {t.tutor.schoolTutors}
      </h2>

      {school.tutors.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '2rem' }}>
          <p style={{ color: 'var(--text-secondary)' }}>{t.tutor.noSchoolTutors}</p>
        </div>
      ) : (
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))',
          gap: '1rem',
        }}>
          {school.tutors.map(renderTutorCard)}
        </div>
      )}
    </div>
  );
}
