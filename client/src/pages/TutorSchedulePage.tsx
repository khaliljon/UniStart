import { useState, useEffect, useCallback } from 'react';
import { tutorService } from '../services/tutorService';
import type { TutorProfileDetail, ScheduleSlotInput } from '../types';

const DAY_NAMES = ['Воскресенье', 'Понедельник', 'Вторник', 'Среда', 'Четверг', 'Пятница', 'Суббота'];
const SHORT_DAYS = ['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'];

function TutorSchedulePage() {
  const [profile, setProfile] = useState<TutorProfileDetail | null>(null);
  const [slots, setSlots] = useState<ScheduleSlotInput[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);

  const loadProfile = useCallback(async () => {
    try {
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        const data = await tutorService.getTutorProfile(user.id);
        setProfile(data);
        setSlots(data.schedule.map(s => ({
          dayOfWeek: s.dayOfWeek,
          startTime: s.startTime,
          endTime: s.endTime,
        })));
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

  const addSlot = () => {
    setSlots(prev => [...prev, { dayOfWeek: 1, startTime: '09:00', endTime: '17:00' }]);
    setSaved(false);
  };

  const removeSlot = (index: number) => {
    setSlots(prev => prev.filter((_, i) => i !== index));
    setSaved(false);
  };

  const updateSlot = (index: number, field: keyof ScheduleSlotInput, value: string | number) => {
    setSlots(prev => prev.map((s, i) => i === index ? { ...s, [field]: value } : s));
    setSaved(false);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await tutorService.setSchedule(slots);
      setSaved(true);
    } catch {
      alert('Не удалось сохранить расписание');
    } finally {
      setSaving(false);
    }
  };

  // Build a grid view: 7 days × time slots
  const buildGrid = () => {
    const grid: Record<number, { start: string; end: string }[]> = {};
    for (let i = 0; i < 7; i++) grid[i] = [];
    slots.forEach(s => {
      if (!grid[s.dayOfWeek]) grid[s.dayOfWeek] = [];
      grid[s.dayOfWeek].push({ start: s.startTime, end: s.endTime });
    });
    return grid;
  };

  if (loading) {
    return <div className="animate-fade-in" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>Загрузка...</div>;
  }

  const grid = buildGrid();

  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '0.5rem' }}>
        <h1 style={{ margin: 0 }}>Расписание</h1>
        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
          {saved && <span style={{ color: '#10b981', fontSize: '0.85rem' }}>Сохранено</span>}
          <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
            {saving ? 'Сохранение...' : 'Сохранить'}
          </button>
        </div>
      </div>

      {/* Weekly overview */}
      <div className="card" style={{ marginBottom: '1rem', overflowX: 'auto' }}>
        <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>Обзор недели</h3>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: '0.5rem', minWidth: '500px' }}>
          {[1, 2, 3, 4, 5, 6, 0].map(day => (
            <div key={day} style={{
              textAlign: 'center', padding: '0.5rem', borderRadius: '8px',
              background: grid[day]?.length > 0 ? 'rgba(16, 185, 129, 0.1)' : 'var(--bg-secondary)',
              border: grid[day]?.length > 0 ? '1px solid rgba(16, 185, 129, 0.3)' : '1px solid var(--border-color)',
            }}>
              <div style={{ fontWeight: 600, fontSize: '0.85rem', marginBottom: '0.3rem' }}>{SHORT_DAYS[day]}</div>
              {grid[day]?.length > 0 ? (
                grid[day].map((slot, i) => (
                  <div key={i} style={{ fontSize: '0.72rem', color: '#10b981' }}>
                    {slot.start}–{slot.end}
                  </div>
                ))
              ) : (
                <div style={{ fontSize: '0.72rem', color: 'var(--text-secondary)' }}>Выходной</div>
              )}
            </div>
          ))}
        </div>
      </div>

      {/* Slot editor */}
      <div className="card">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
          <h3 style={{ margin: 0, fontSize: '1rem' }}>Слоты доступности</h3>
          <button className="btn btn-outline" onClick={addSlot} style={{ fontSize: '0.85rem' }}>
            + Добавить слот
          </button>
        </div>

        {slots.length === 0 ? (
          <p style={{ color: 'var(--text-secondary)', margin: 0 }}>
            Нет слотов. Нажмите "Добавить слот" для настройки расписания.
          </p>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {slots.map((slot, index) => (
              <div
                key={index}
                style={{
                  display: 'flex', gap: '0.5rem', alignItems: 'center', flexWrap: 'wrap',
                  padding: '0.5rem 0.75rem', borderRadius: '8px', background: 'var(--bg-secondary)',
                }}
              >
                <select
                  className="form-input"
                  value={slot.dayOfWeek}
                  onChange={e => updateSlot(index, 'dayOfWeek', Number(e.target.value))}
                  style={{ width: '140px', padding: '0.4rem' }}
                >
                  {DAY_NAMES.map((name, i) => (
                    <option key={i} value={i}>{name}</option>
                  ))}
                </select>

                <input
                  type="time"
                  className="form-input"
                  value={slot.startTime}
                  onChange={e => updateSlot(index, 'startTime', e.target.value)}
                  style={{ width: '110px', padding: '0.4rem' }}
                />
                <span style={{ color: 'var(--text-secondary)' }}>—</span>
                <input
                  type="time"
                  className="form-input"
                  value={slot.endTime}
                  onChange={e => updateSlot(index, 'endTime', e.target.value)}
                  style={{ width: '110px', padding: '0.4rem' }}
                />

                <button
                  className="btn"
                  onClick={() => removeSlot(index)}
                  style={{ padding: '0.3rem 0.6rem', fontSize: '0.85rem', color: '#ef4444' }}
                  title="Удалить слот"
                >
                  ✕
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Availability toggle info */}
      {profile && (
        <div className="card" style={{ marginTop: '1rem', display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <span style={{ fontSize: '0.9rem' }}>
            Статус: {profile.isAvailable ? '● Принимаю новых учеников' : '○ Не принимаю новых учеников'}
          </span>
          <span style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
            (изменить можно на странице профиля)
          </span>
        </div>
      )}
    </div>
  );
}

export default TutorSchedulePage;
