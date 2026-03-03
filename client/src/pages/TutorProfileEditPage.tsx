import { useState, useEffect, useCallback } from 'react';
import { tutorService } from '../services/tutorService';
import type { TutorProfileDetail, UpdateTutorProfile } from '../types';

function TutorProfileEditPage() {
  const [profile, setProfile] = useState<TutorProfileDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);

  // Form state
  const [headline, setHeadline] = useState('');
  const [bio, setBio] = useState('');
  const [experience, setExperience] = useState('');
  const [specializations, setSpecializations] = useState<string[]>([]);
  const [hourlyRate, setHourlyRate] = useState<string>('');
  const [isAvailable, setIsAvailable] = useState(true);
  const [contactPreference, setContactPreference] = useState('Chat');

  const EXAM_OPTIONS = ['SAT', 'TOEFL', 'IELTS', 'NUET', 'CSCA'];

  const loadProfile = useCallback(async () => {
    try {
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        const data = await tutorService.getTutorProfile(user.id);
        setProfile(data);
        setHeadline(data.headline);
        setBio(data.bio);
        setExperience(data.experience);
        setSpecializations(data.specializations);
        setHourlyRate(data.hourlyRate?.toString() || '');
        setIsAvailable(data.isAvailable);
        setContactPreference(data.contactPreference);
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

  const handleSave = async () => {
    setSaving(true);
    try {
      const data: UpdateTutorProfile = {
        headline,
        bio,
        experience,
        specializations,
        hourlyRate: hourlyRate ? Number(hourlyRate) : undefined,
        isAvailable,
        contactPreference,
      };
      await tutorService.updateProfile(data);
      setSaved(true);
      setTimeout(() => setSaved(false), 3000);
    } catch {
      alert('Не удалось сохранить профиль');
    } finally {
      setSaving(false);
    }
  };

  const toggleSpec = (spec: string) => {
    setSpecializations(prev =>
      prev.includes(spec) ? prev.filter(s => s !== spec) : [...prev, spec]
    );
    setSaved(false);
  };

  if (loading) {
    return <div className="animate-fade-in" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>Загрузка...</div>;
  }

  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '0.5rem' }}>
        <h1 style={{ margin: 0 }}>Мой профиль</h1>
        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
          {saved && <span style={{ color: '#10b981', fontSize: '0.85rem' }}>Сохранено</span>}
          <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
            {saving ? 'Сохранение...' : 'Сохранить'}
          </button>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(350px, 1fr))', gap: '1rem' }}>
        {/* Left: edit form */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <div className="card">
            <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Основная информация</h3>

            <div className="form-group" style={{ marginBottom: '0.75rem' }}>
              <label className="form-label">Заголовок</label>
              <input
                className="form-input"
                value={headline}
                onChange={e => { setHeadline(e.target.value); setSaved(false); }}
                placeholder="Сертифицированный TOEFL-тьютор"
                maxLength={200}
              />
            </div>

            <div className="form-group" style={{ marginBottom: '0.75rem' }}>
              <label className="form-label">О себе</label>
              <textarea
                className="form-input"
                rows={4}
                value={bio}
                onChange={e => { setBio(e.target.value); setSaved(false); }}
                placeholder="Расскажите о себе, вашем подходе к обучению..."
                maxLength={2000}
              />
              <div style={{ fontSize: '0.72rem', color: 'var(--text-secondary)', textAlign: 'right' }}>
                {bio.length}/2000
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: '0.75rem' }}>
              <label className="form-label">Опыт</label>
              <textarea
                className="form-input"
                rows={3}
                value={experience}
                onChange={e => { setExperience(e.target.value); setSaved(false); }}
                placeholder="5 лет опыта, средний прирост учеников +150 баллов SAT"
                maxLength={1000}
              />
            </div>

            <div className="form-group" style={{ marginBottom: '0.75rem' }}>
              <label className="form-label">Стоимость (₸/час)</label>
              <input
                type="number"
                className="form-input"
                value={hourlyRate}
                onChange={e => { setHourlyRate(e.target.value); setSaved(false); }}
                placeholder="5000"
                min={0}
              />
            </div>
          </div>

          <div className="card">
            <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Специализации</h3>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.4rem' }}>
              {EXAM_OPTIONS.map(spec => (
                <button
                  key={spec}
                  className={`btn ${specializations.includes(spec) ? 'btn-primary' : 'btn-outline'}`}
                  onClick={() => toggleSpec(spec)}
                  style={{ padding: '0.35rem 0.75rem', fontSize: '0.82rem' }}
                >
                  {spec}
                </button>
              ))}
            </div>
          </div>

          <div className="card">
            <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Настройки</h3>

            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.75rem' }}>
              <label style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                <input
                  type="checkbox"
                  checked={isAvailable}
                  onChange={e => { setIsAvailable(e.target.checked); setSaved(false); }}
                />
                Принимаю новых учеников
              </label>
            </div>

            <div className="form-group">
              <label className="form-label">Способ связи</label>
              <select
                className="form-input"
                value={contactPreference}
                onChange={e => { setContactPreference(e.target.value); setSaved(false); }}
              >
                <option value="Chat">Чат</option>
                <option value="Email">Email</option>
                <option value="Both">Чат и Email</option>
              </select>
            </div>
          </div>
        </div>

        {/* Right: live preview */}
        <div>
          <div className="card" style={{ position: 'sticky', top: '1rem' }}>
            <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Предпросмотр карточки</h3>

            <div style={{
              padding: '1rem', borderRadius: '10px', border: '1px solid var(--border-color)',
              background: 'var(--bg-secondary)',
            }}>
              <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'flex-start' }}>
                <div style={{
                  width: '56px', height: '56px', borderRadius: '50%',
                  background: 'linear-gradient(135deg, var(--primary-color), var(--primary-hover))',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  color: '#fff', fontWeight: 700, fontSize: '1.2rem', flexShrink: 0,
                }}>
                  {(profile?.name || '??').split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2)}
                </div>
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: 700, fontSize: '1.1rem' }}>{profile?.name || 'Имя'}</div>
                  <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
                    {headline || 'Заголовок...'}
                  </div>
                  <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem', fontSize: '0.82rem' }}>
                    <span>★ {profile?.averageRating.toFixed(1) || '0.0'}</span>
                    <span>{profile?.totalStudents || 0}</span>
                    {hourlyRate && <span style={{ color: 'var(--primary-color)', fontWeight: 600 }}>{hourlyRate}₸/ч</span>}
                  </div>
                </div>
                <span style={{
                  fontSize: '0.72rem',
                  color: isAvailable ? '#10b981' : '#ef4444',
                  fontWeight: 600,
                }}>
                  {isAvailable ? '● Доступен' : '○ Недоступен'}
                </span>
              </div>

              {specializations.length > 0 && (
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.3rem', marginTop: '0.75rem' }}>
                  {specializations.map(s => (
                    <span key={s} style={{
                      fontSize: '0.72rem', padding: '0.15rem 0.5rem', borderRadius: '999px',
                      background: 'var(--bg-primary)', color: 'var(--text-secondary)', fontWeight: 500,
                    }}>{s}</span>
                  ))}
                </div>
              )}

              {bio && (
                <div style={{ marginTop: '0.75rem', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  {bio.slice(0, 150)}{bio.length > 150 ? '...' : ''}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default TutorProfileEditPage;
