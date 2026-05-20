import { useMemo } from 'react';
import { Link, useParams } from 'react-router-dom';
import { loadAdvisorConfig } from '../advisorConfig';

const DIFF_COLORS: Record<string, string> = {
  'Несложно': '#22c55e',
  'Средне': '#f59e0b',
  'Сложно': '#ef4444',
  'Очень сложно': '#7c3aed',
};

function Row({ label, value }: { label: string; value?: string | number | null }) {
  if (!value && value !== 0) return null;
  return (
    <div style={{ display: 'flex', gap: '0.75rem', padding: '0.6rem 0', borderBottom: '1px solid var(--border-color)' }}>
      <span style={{ minWidth: 180, color: 'var(--text-secondary)', fontSize: '0.88rem' }}>{label}</span>
      <span style={{ fontSize: '0.9rem' }}>{value}</span>
    </div>
  );
}

export default function UniversityDetailPage() {
  const { id } = useParams<{ id: string }>();
  const config = useMemo(() => loadAdvisorConfig(), []);
  const uni = config.universities.find((u) => u.id === id);

  if (!uni) {
    return (
      <div style={{ maxWidth: 800, margin: '2rem auto', padding: '0 1rem' }}>
        <Link to="/advisor" style={{ color: 'var(--accent-color)', fontSize: '0.9rem' }}>
          ← Назад к советнику
        </Link>
        <div className="card" style={{ marginTop: '1.5rem', padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
          Университет не найден.
        </div>
      </div>
    );
  }

  return (
    <div style={{ maxWidth: 860, margin: '0 auto' }}>
      {/* Back */}
      <Link
        to="/advisor"
        style={{ display: 'inline-flex', alignItems: 'center', gap: '0.3rem', color: 'var(--text-secondary)', fontSize: '0.88rem', textDecoration: 'none', marginBottom: '1.25rem' }}
      >
        ← Назад к советнику
      </Link>

      {/* Header */}
      <div className="card" style={{ marginBottom: '1.25rem', padding: '1.75rem' }}>
        <div style={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: '1rem', flexWrap: 'wrap' }}>
          <div>
            <h1 style={{ margin: '0 0 0.25rem' }}>{uni.name}</h1>
            <div style={{ color: 'var(--text-secondary)', fontSize: '0.95rem' }}>
              {uni.city && `${uni.city}, `}{uni.country}
            </div>
          </div>
          {uni.photoUrl && (
            <img
              src={uni.photoUrl}
              alt={uni.name}
              style={{ width: 100, height: 100, objectFit: 'cover', borderRadius: 10, border: '1px solid var(--border-color)' }}
            />
          )}
        </div>

        {/* Tags */}
        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginTop: '1rem' }}>
          <span style={{ padding: '0.25rem 0.75rem', borderRadius: '999px', background: 'var(--accent-color)', color: '#fff', fontSize: '0.8rem', fontWeight: 600 }}>
            {uni.exam}
          </span>
          {uni.minScore != null && (
            <span style={{ padding: '0.25rem 0.75rem', borderRadius: '999px', border: '1px solid var(--border-color)', fontSize: '0.8rem' }}>
              Мин. балл: {uni.minScore}
            </span>
          )}
          {uni.level && (
            <span style={{ padding: '0.25rem 0.75rem', borderRadius: '999px', border: `1px solid ${DIFF_COLORS[uni.level] ?? 'var(--border-color)'}`, color: DIFF_COLORS[uni.level] ?? 'inherit', fontSize: '0.8rem' }}>
              {uni.level}
            </span>
          )}
          {uni.language && (
            <span style={{ padding: '0.25rem 0.75rem', borderRadius: '999px', border: '1px solid var(--border-color)', fontSize: '0.8rem' }}>
              {uni.language}
            </span>
          )}
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.25rem', marginBottom: '1.25rem' }}>
        {/* Main details */}
        <div className="card" style={{ padding: '1.25rem' }}>
          <h2 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Основная информация</h2>
          <Row label="Страна" value={uni.country} />
          <Row label="Город" value={uni.city} />
          <Row label="Экзамен" value={uni.exam} />
          <Row label="Минимальный балл" value={uni.minScore ?? 'Не указан'} />
          <Row label="Сложность поступления" value={uni.level || 'Не указана'} />
          <Row label="Язык обучения" value={uni.language} />
          <Row label="Рейтинг" value={uni.ranking} />
          <Row label="Гранты" value={uni.grants} />
        </div>

        {/* Admission details */}
        <div className="card" style={{ padding: '1.25rem' }}>
          <h2 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Поступление</h2>
          <Row label="Процедура подачи" value={uni.procedure} />
          <Row label="Сроки подачи" value={uni.deadlines} />
          <Row label="Процент принятия" value={uni.acceptance} />
          {uni.plus && (
            <div style={{ padding: '0.6rem 0', borderBottom: '1px solid var(--border-color)' }}>
              <span style={{ display: 'block', color: 'var(--text-secondary)', fontSize: '0.88rem', marginBottom: '0.2rem' }}>Преимущества</span>
              <span style={{ color: 'var(--success-color)', fontSize: '0.9rem' }}>+ {uni.plus}</span>
            </div>
          )}
          {uni.minus && (
            <div style={{ padding: '0.6rem 0', borderBottom: '1px solid var(--border-color)' }}>
              <span style={{ display: 'block', color: 'var(--text-secondary)', fontSize: '0.88rem', marginBottom: '0.2rem' }}>Недостатки</span>
              <span style={{ color: 'var(--error-color)', fontSize: '0.9rem' }}>− {uni.minus}</span>
            </div>
          )}
        </div>
      </div>

      {/* Specialties */}
      {uni.specialties.length > 0 && (
        <div className="card" style={{ padding: '1.25rem', marginBottom: '1.25rem' }}>
          <h2 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Специальности ({uni.specialties.length})</h2>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem' }}>
            {uni.specialties.map((sp) => (
              <span
                key={sp}
                style={{ padding: '0.25rem 0.65rem', borderRadius: '999px', border: '1px solid var(--border-color)', fontSize: '0.82rem', color: 'var(--text-secondary)' }}
              >
                {sp}
              </span>
            ))}
          </div>
        </div>
      )}

      {/* Description */}
      {uni.description && (
        <div className="card" style={{ padding: '1.25rem', marginBottom: '1.25rem' }}>
          <h2 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Описание</h2>
          <p style={{ margin: 0, lineHeight: 1.7, color: 'var(--text-secondary)', fontSize: '0.92rem' }}>{uni.description}</p>
        </div>
      )}

      {/* Video */}
      {uni.videoUrl && (
        <div className="card" style={{ padding: '1.25rem', marginBottom: '1.25rem' }}>
          <h2 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Видео</h2>
          {uni.videoUrl.includes('youtube') || uni.videoUrl.includes('youtu.be') ? (
            <iframe
              src={uni.videoUrl.replace('watch?v=', 'embed/').replace('youtu.be/', 'www.youtube.com/embed/')}
              style={{ width: '100%', height: 340, border: 'none', borderRadius: 8 }}
              allowFullScreen
              title={`${uni.name} video`}
            />
          ) : (
            <a href={uni.videoUrl} target="_blank" rel="noopener noreferrer" style={{ color: 'var(--accent-color)' }}>
              Смотреть видео →
            </a>
          )}
        </div>
      )}

      {/* Map */}
      {uni.lat != null && uni.lng != null && (
        <div className="card" style={{ padding: '1.25rem', marginBottom: '1.25rem' }}>
          <h2 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>На карте</h2>
          <iframe
            title="university-map"
            src={`https://www.openstreetmap.org/export/embed.html?bbox=${uni.lng - 0.3}%2C${uni.lat - 0.15}%2C${uni.lng + 0.3}%2C${uni.lat + 0.15}&layer=mapnik&marker=${uni.lat}%2C${uni.lng}`}
            style={{ width: '100%', height: 300, border: 'none', borderRadius: 8 }}
            loading="lazy"
          />
          <a
            href={`https://www.openstreetmap.org/?mlat=${uni.lat}&mlon=${uni.lng}#map=14/${uni.lat}/${uni.lng}`}
            target="_blank"
            rel="noopener noreferrer"
            style={{ display: 'inline-block', marginTop: '0.5rem', fontSize: '0.82rem', color: 'var(--text-secondary)' }}
          >
            Открыть в OpenStreetMap →
          </a>
        </div>
      )}
    </div>
  );
}
