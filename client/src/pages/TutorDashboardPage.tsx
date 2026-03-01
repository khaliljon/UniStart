import { useState, useEffect, useCallback } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { tutorService } from '../services/tutorService';
import type { TutorProfileDetail, UpdateTutorProfile, ScheduleSlotInput } from '../types';

const DAY_NAMES = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс'];

function TutorDashboardPage() {
  const currentUser = useAppSelector((state) => state.auth.user);

  const [profile, setProfile] = useState<TutorProfileDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [successMsg, setSuccessMsg] = useState('');
  const [tab, setTab] = useState<'profile' | 'schedule' | 'students'>('profile');

  // Profile form
  const [headline, setHeadline] = useState('');
  const [bio, setBio] = useState('');
  const [experience, setExperience] = useState('');
  const [specializations, setSpecializations] = useState('');
  const [hourlyRate, setHourlyRate] = useState('');
  const [isAvailable, setIsAvailable] = useState(true);
  const [contactPreference, setContactPreference] = useState('Chat');

  // Schedule form
  const [slots, setSlots] = useState<ScheduleSlotInput[]>([]);

  // Students
  const [students, setStudents] = useState<{ userId: number; name: string }[]>([]);

  const loadProfile = useCallback(async () => {
    if (!currentUser?.id) return;
    setLoading(true);
    try {
      const data = await tutorService.getTutorProfile(currentUser.id);
      setProfile(data);
      setHeadline(data.headline || '');
      setBio(data.bio || '');
      setExperience(data.experience || '');
      setSpecializations(data.specializations.join(', '));
      setHourlyRate(data.hourlyRate?.toString() || '');
      setIsAvailable(data.isAvailable);
      setContactPreference(data.contactPreference || 'Chat');
      setSlots(data.schedule.map(s => ({
        dayOfWeek: s.dayOfWeek,
        startTime: s.startTime,
        endTime: s.endTime,
      })));
    } catch {
      // Profile might not exist yet — that's ok
    } finally {
      setLoading(false);
    }
  }, [currentUser?.id]);

  useEffect(() => {
    loadProfile();
  }, [loadProfile]);

  const showSuccess = (msg: string) => {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(''), 3000);
  };

  const handleSaveProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      const data: UpdateTutorProfile = {
        headline: headline || undefined,
        bio: bio || undefined,
        experience: experience || undefined,
        specializations: specializations ? specializations.split(',').map(s => s.trim()).filter(Boolean) : undefined,
        hourlyRate: hourlyRate ? Number(hourlyRate) : undefined,
        isAvailable,
        contactPreference,
      };
      await tutorService.updateProfile(data);
      showSuccess('Профиль сохранён ✓');
      loadProfile();
    } catch {
      alert('Не удалось сохранить профиль');
    } finally {
      setSaving(false);
    }
  };

  const handleSaveSchedule = async () => {
    setSaving(true);
    try {
      await tutorService.setSchedule(slots);
      showSuccess('Расписание сохранено ✓');
      loadProfile();
    } catch {
      alert('Не удалось сохранить расписание');
    } finally {
      setSaving(false);
    }
  };

  const addSlot = () => {
    setSlots(prev => [...prev, { dayOfWeek: 1, startTime: '09:00', endTime: '17:00' }]);
  };

  const removeSlot = (index: number) => {
    setSlots(prev => prev.filter((_, i) => i !== index));
  };

  const updateSlot = (index: number, field: keyof ScheduleSlotInput, value: string | number) => {
    setSlots(prev => prev.map((s, i) => i === index ? { ...s, [field]: value } : s));
  };

  const loadStudents = useCallback(async () => {
    try {
      const data = await tutorService.getMyStudents();
      setStudents(data.map(s => ({ userId: s.userId, name: s.name })));
    } catch {
      console.error('Failed to load students');
    }
  }, []);

  useEffect(() => {
    if (tab === 'students') loadStudents();
  }, [tab, loadStudents]);

  if (loading) {
    return <div className="animate-fade-in" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>Загрузка...</div>;
  }

  const tabs = [
    { id: 'profile' as const, label: 'Профиль' },
    { id: 'schedule' as const, label: 'Расписание' },
    { id: 'students' as const, label: 'Ученики' },
  ];

  return (
    <div className="animate-fade-in">
      <div style={{ marginBottom: '1.5rem' }}>
        <h1 style={{ margin: '0 0 0.25rem' }}>Панель тьютора</h1>
        <p style={{ margin: 0, color: 'var(--text-secondary)' }}>
          Управляйте своим профилем и расписанием
        </p>
      </div>

      {successMsg && (
        <div style={{
          padding: '0.75rem 1rem', marginBottom: '1rem', borderRadius: '8px',
          background: '#10b981', color: '#fff', fontWeight: 600,
        }}>
          {successMsg}
        </div>
      )}

      {/* Stats row */}
      {profile && (
        <div style={{
          display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))',
          gap: '0.75rem', marginBottom: '1.5rem',
        }}>
          {[
            { label: 'Рейтинг', value: `★ ${profile.averageRating.toFixed(1)}`, color: '#f59e0b' },
            { label: 'Отзывы', value: String(profile.totalReviews), color: '#3b82f6' },
            { label: 'Ученики', value: String(profile.totalStudents), color: '#10b981' },
            { label: 'Статус', value: profile.isAvailable ? 'Доступен' : 'Не доступен', color: profile.isAvailable ? '#10b981' : '#ef4444' },
          ].map(stat => (
            <div key={stat.label} className="card" style={{ textAlign: 'center', padding: '1rem' }}>
              <div style={{ fontSize: '1.5rem', fontWeight: 700, color: stat.color }}>{stat.value}</div>
              <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>{stat.label}</div>
            </div>
          ))}
        </div>
      )}

      {/* Tabs */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
        {tabs.map(t => (
          <button
            key={t.id}
            className={`btn ${tab === t.id ? 'btn-primary' : ''}`}
            onClick={() => setTab(t.id)}
            style={{ padding: '0.5rem 1rem', fontSize: '0.88rem' }}
          >
            {t.label}
          </button>
        ))}
      </div>

      {/* Profile tab */}
      {tab === 'profile' && (
        <div className="card">
          <h3 style={{ margin: '0 0 1rem' }}>Редактирование профиля</h3>
          <form onSubmit={handleSaveProfile} style={{ display: 'flex', flexDirection: 'column', gap: '1rem', maxWidth: '600px' }}>
            <div>
              <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: '0.25rem', fontWeight: 600 }}>Заголовок</label>
              <input
                type="text"
                value={headline}
                onChange={(e) => setHeadline(e.target.value)}
                className="form-input"
                placeholder="напр. Репетитор ЕГЭ по математике"
                maxLength={200}
                style={{ width: '100%', padding: '0.5rem 0.75rem' }}
              />
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: '0.25rem', fontWeight: 600 }}>О себе</label>
              <textarea
                value={bio}
                onChange={(e) => setBio(e.target.value)}
                className="form-input"
                rows={4}
                maxLength={2000}
                placeholder="Расскажите о себе и своём подходе к обучению..."
                style={{ width: '100%', resize: 'vertical' }}
              />
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: '0.25rem', fontWeight: 600 }}>Опыт</label>
              <textarea
                value={experience}
                onChange={(e) => setExperience(e.target.value)}
                className="form-input"
                rows={3}
                maxLength={1000}
                placeholder="Образование, стаж, достижения..."
                style={{ width: '100%', resize: 'vertical' }}
              />
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: '0.25rem', fontWeight: 600 }}>Специализации (через запятую)</label>
              <input
                type="text"
                value={specializations}
                onChange={(e) => setSpecializations(e.target.value)}
                className="form-input"
                placeholder="EGE, OGE, SAT"
                style={{ width: '100%', padding: '0.5rem 0.75rem' }}
              />
            </div>

            <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
              <div style={{ flex: '1 1 120px' }}>
                <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: '0.25rem', fontWeight: 600 }}>Стоимость (₽/час)</label>
                <input
                  type="number"
                  value={hourlyRate}
                  onChange={(e) => setHourlyRate(e.target.value)}
                  className="form-input"
                  placeholder="1500"
                  min={0}
                  style={{ width: '100%', padding: '0.5rem 0.75rem' }}
                />
              </div>

              <div style={{ flex: '1 1 150px' }}>
                <label style={{ display: 'block', fontSize: '0.85rem', marginBottom: '0.25rem', fontWeight: 600 }}>Способ связи</label>
                <select
                  value={contactPreference}
                  onChange={(e) => setContactPreference(e.target.value)}
                  className="form-input"
                  style={{ width: '100%', padding: '0.5rem 0.75rem' }}
                >
                  <option value="Chat">Чат</option>
                  <option value="Email">Email</option>
                  <option value="Both">Чат и Email</option>
                </select>
              </div>
            </div>

            <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer' }}>
              <input
                type="checkbox"
                checked={isAvailable}
                onChange={(e) => setIsAvailable(e.target.checked)}
              />
              <span style={{ fontSize: '0.9rem' }}>Доступен для новых учеников</span>
            </label>

            <button
              type="submit"
              className="btn btn-primary"
              disabled={saving}
              style={{ alignSelf: 'flex-start', padding: '0.6rem 2rem' }}
            >
              {saving ? 'Сохранение...' : 'Сохранить профиль'}
            </button>
          </form>
        </div>
      )}

      {/* Schedule tab */}
      {tab === 'schedule' && (
        <div className="card">
          <h3 style={{ margin: '0 0 1rem' }}>Расписание занятий</h3>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', margin: '0 0 1rem' }}>
            Укажите дни и часы, когда вы доступны для занятий
          </p>

          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', marginBottom: '1rem' }}>
            {slots.map((slot, index) => (
              <div key={index} style={{
                display: 'flex', alignItems: 'center', gap: '0.75rem', flexWrap: 'wrap',
                padding: '0.5rem 0.75rem', borderRadius: '8px', background: 'var(--bg-secondary)',
              }}>
                <select
                  value={slot.dayOfWeek}
                  onChange={(e) => updateSlot(index, 'dayOfWeek', Number(e.target.value))}
                  className="form-input"
                  style={{ padding: '0.4rem 0.5rem', minWidth: '80px' }}
                >
                  {DAY_NAMES.map((name, i) => (
                    <option key={i} value={i + 1}>{name}</option>
                  ))}
                </select>
                <input
                  type="time"
                  value={slot.startTime}
                  onChange={(e) => updateSlot(index, 'startTime', e.target.value)}
                  className="form-input"
                  style={{ padding: '0.4rem 0.5rem' }}
                />
                <span style={{ color: 'var(--text-secondary)' }}>–</span>
                <input
                  type="time"
                  value={slot.endTime}
                  onChange={(e) => updateSlot(index, 'endTime', e.target.value)}
                  className="form-input"
                  style={{ padding: '0.4rem 0.5rem' }}
                />
                <button
                  onClick={() => removeSlot(index)}
                  className="btn"
                  style={{ padding: '0.3rem 0.6rem', fontSize: '0.85rem', color: '#ef4444' }}
                >
                  ✕
                </button>
              </div>
            ))}

            {slots.length === 0 && (
              <p style={{ color: 'var(--text-secondary)', margin: 0 }}>Расписание пока пустое</p>
            )}
          </div>

          <div style={{ display: 'flex', gap: '0.75rem' }}>
            <button onClick={addSlot} className="btn" style={{ padding: '0.5rem 1rem', fontSize: '0.88rem' }}>
              + Добавить слот
            </button>
            <button
              onClick={handleSaveSchedule}
              className="btn btn-primary"
              disabled={saving}
              style={{ padding: '0.5rem 1.5rem', fontSize: '0.88rem' }}
            >
              {saving ? 'Сохранение...' : 'Сохранить расписание'}
            </button>
          </div>
        </div>
      )}

      {/* Students tab */}
      {tab === 'students' && (
        <div className="card">
          <h3 style={{ margin: '0 0 1rem' }}>Мои ученики</h3>
          {students.length === 0 ? (
            <p style={{ color: 'var(--text-secondary)', margin: 0 }}>
              У вас пока нет учеников. Они появятся, когда студенты начнут с вами общение.
            </p>
          ) : (
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', gap: '0.75rem' }}>
              {students.map(s => (
                <div key={s.userId} style={{
                  display: 'flex', alignItems: 'center', gap: '0.75rem',
                  padding: '0.75rem', borderRadius: '8px', background: 'var(--bg-secondary)',
                }}>
                  <div style={{
                    width: '40px', height: '40px', borderRadius: '50%',
                    background: 'linear-gradient(135deg, #10b981, #059669)',
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                    color: '#fff', fontWeight: 700, fontSize: '0.85rem', flexShrink: 0,
                  }}>
                    {s.name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2)}
                  </div>
                  <span style={{ fontWeight: 600, fontSize: '0.93rem' }}>{s.name}</span>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default TutorDashboardPage;
