import { useState, useEffect } from 'react';
import { notificationService } from '../services/notificationService';
import type { NotificationPreferences } from '../types';

const NOTIFICATION_ITEMS = [
  {
    key: 'welcomeEmail' as const,
    label: 'Welcome-письмо',
    description: 'Приветственное письмо при регистрации',
    icon: '',
  },
  {
    key: 'streakReminder' as const,
    label: 'Напоминание о серии',
    description: 'Уведомление, если вы не занимались 2+ дня',
    icon: '',
  },
  {
    key: 'weeklyDigest' as const,
    label: 'Еженедельный дайджест',
    description: 'Отчёт о прогрессе за неделю каждый понедельник',
    icon: '',
  },
  {
    key: 'studyPlanReminder' as const,
    label: 'Напоминание о плане',
    description: 'Ежедневное напоминание о плане обучения',
    icon: '',
  },
  {
    key: 'achievementNotification' as const,
    label: 'Достижения',
    description: 'Уведомления о новых достижениях и вехах',
    icon: '',
  },
];

function NotificationSettingsPage() {
  const [preferences, setPreferences] = useState<NotificationPreferences | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadPreferences();
  }, []);

  const loadPreferences = async () => {
    try {
      setLoading(true);
      const prefs = await notificationService.getPreferences();
      setPreferences(prefs);
    } catch {
      setError('Не удалось загрузить настройки');
    } finally {
      setLoading(false);
    }
  };

  const handleToggle = async (key: keyof NotificationPreferences) => {
    if (!preferences) return;

    const newValue = !preferences[key];
    setSaving(key);

    try {
      const updated = await notificationService.updatePreferences({ [key]: newValue });
      setPreferences(updated);
    } catch {
      setError('Не удалось сохранить настройку');
    } finally {
      setSaving(null);
    }
  };

  const handleToggleAll = async (enable: boolean) => {
    setSaving('all');
    try {
      const updated = await notificationService.updatePreferences({
        welcomeEmail: enable,
        streakReminder: enable,
        weeklyDigest: enable,
        studyPlanReminder: enable,
        achievementNotification: enable,
      });
      setPreferences(updated);
    } catch {
      setError('Не удалось сохранить настройки');
    } finally {
      setSaving(null);
    }
  };

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '4rem 0' }}>
        <div className="loading-spinner" />
        <p style={{ color: 'var(--text-secondary)', marginTop: '1rem' }}>
          Загрузка настроек...
        </p>
      </div>
    );
  }

  if (error && !preferences) {
    return (
      <div style={{ textAlign: 'center', padding: '4rem 0' }}>
        <p style={{ color: 'var(--danger)' }}>{error}</p>
        <button className="btn btn-primary" onClick={loadPreferences}>
          Попробовать снова
        </button>
      </div>
    );
  }

  const allEnabled = preferences
    ? NOTIFICATION_ITEMS.every((item) => preferences[item.key])
    : false;
  const allDisabled = preferences
    ? NOTIFICATION_ITEMS.every((item) => !preferences[item.key])
    : false;

  return (
    <div style={{ maxWidth: 640, margin: '0 auto' }}>
      <h1 style={{ marginBottom: '0.5rem' }}>Уведомления</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem' }}>
        Управляйте email-уведомлениями. Мы отправляем только то, что вам действительно нужно.
      </p>

      {error && (
        <div
          className="card"
          style={{
            background: 'rgba(239,68,68,0.1)',
            border: '1px solid var(--danger)',
            padding: '0.75rem 1rem',
            marginBottom: '1rem',
            borderRadius: 8,
            color: 'var(--danger)',
          }}
        >
          {error}
        </div>
      )}

      <div
        style={{
          display: 'flex',
          gap: '0.5rem',
          marginBottom: '1.5rem',
        }}
      >
        <button
          className="btn btn-outline"
          disabled={saving !== null || allEnabled}
          onClick={() => handleToggleAll(true)}
          style={{ fontSize: '0.875rem' }}
        >
          Включить все
        </button>
        <button
          className="btn btn-outline"
          disabled={saving !== null || allDisabled}
          onClick={() => handleToggleAll(false)}
          style={{ fontSize: '0.875rem' }}
        >
          Выключить все
        </button>
      </div>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
        {NOTIFICATION_ITEMS.map((item) => {
          const enabled = preferences ? preferences[item.key] : false;
          const isSaving = saving === item.key || saving === 'all';

          return (
            <div
              key={item.key}
              className="card"
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: '1rem',
                padding: '1.25rem',
                opacity: isSaving ? 0.7 : 1,
                transition: 'opacity 0.2s',
              }}
            >
              <div style={{ fontSize: '1.5rem', flexShrink: 0 }}>{item.icon}</div>
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 600, fontSize: '0.95rem' }}>{item.label}</div>
                <div
                  style={{
                    color: 'var(--text-secondary)',
                    fontSize: '0.85rem',
                    marginTop: '0.25rem',
                  }}
                >
                  {item.description}
                </div>
              </div>
              <button
                onClick={() => handleToggle(item.key)}
                disabled={isSaving}
                style={{
                  position: 'relative',
                  width: 52,
                  height: 28,
                  borderRadius: 14,
                  border: 'none',
                  background: enabled
                    ? 'linear-gradient(135deg, #6c5ce7, #a855f7)'
                    : 'var(--border)',
                  cursor: isSaving ? 'wait' : 'pointer',
                  transition: 'background 0.3s',
                  flexShrink: 0,
                  padding: 0,
                }}
                title={enabled ? 'Выключить' : 'Включить'}
              >
                <div
                  style={{
                    position: 'absolute',
                    top: 3,
                    left: enabled ? 27 : 3,
                    width: 22,
                    height: 22,
                    borderRadius: '50%',
                    background: '#fff',
                    boxShadow: '0 1px 3px rgba(0,0,0,0.2)',
                    transition: 'left 0.3s',
                  }}
                />
              </button>
            </div>
          );
        })}
      </div>

      <div
        className="card"
        style={{
          marginTop: '2rem',
          padding: '1.25rem',
          background: 'rgba(108,92,231,0.05)',
          border: '1px solid rgba(108,92,231,0.15)',
          borderRadius: 12,
        }}
      >
        <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'flex-start' }}>
          <span style={{ fontSize: '1.25rem' }}></span>
          <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)', lineHeight: 1.6 }}>
            <strong style={{ color: 'var(--text-primary)' }}>Как это работает:</strong>
            <br />
            • <strong>Streak-напоминание</strong> отправляется, если вы не занимались 2 дня подряд
            <br />
            • <strong>Еженедельный дайджест</strong> приходит каждый понедельник с итогами недели
            <br />
            • Все письма содержат ссылку для отключения уведомлений
          </div>
        </div>
      </div>
    </div>
  );
}

export default NotificationSettingsPage;
