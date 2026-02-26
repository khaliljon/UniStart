import { useState } from 'react';
import { subscriptionService } from '../services/subscriptionService';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { setSubscription } from '../store/slices/authSlice';

interface PricingModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const FREE_FEATURES = [
  '15 вопросов в день',
  '1 урок в день',
  'Базовая аналитика',
  'Недельный прогноз',
  'Диагностический тест',
  'Персональный план (базовый)',
];

const PRO_FEATURES = [
  'Безлимитные вопросы',
  'Все уроки',
  'Mock Exams (пробные экзамены)',
  'Полная аналитика (radar, heatmap)',
  'Полный план подготовки',
  'Real-time прогноз балла',
  'Приоритетная поддержка',
];

export function PricingModal({ isOpen, onClose }: PricingModalProps) {
  const dispatch = useAppDispatch();
  const [isUpgrading, setIsUpgrading] = useState(false);
  const [result, setResult] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleUpgrade = async () => {
    setIsUpgrading(true);
    try {
      const res = await subscriptionService.upgrade('Pro');
      if (res.success) {
        dispatch(setSubscription({ tier: res.tier, expiresAt: res.expiresAt }));
        setResult(res.message);
        setTimeout(() => {
          onClose();
          setResult(null);
        }, 2000);
      }
    } catch {
      setResult('Ошибка при обновлении тарифа');
    } finally {
      setIsUpgrading(false);
    }
  };

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        zIndex: 1000,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'rgba(0,0,0,0.5)',
        backdropFilter: 'blur(4px)',
      }}
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div style={{
        background: 'var(--card-bg, #fff)',
        borderRadius: '1.5rem',
        padding: '2rem',
        maxWidth: '720px',
        width: '90%',
        maxHeight: '90vh',
        overflow: 'auto',
        boxShadow: '0 20px 60px rgba(0,0,0,0.2)',
      }}>
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <h2 style={{ fontSize: '1.5rem', marginBottom: '0.5rem', color: 'var(--text-primary)' }}>
            Выберите тариф
          </h2>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.95rem' }}>
            Разблокируйте все возможности для максимальной подготовки
          </p>
        </div>

        {result && (
          <div style={{
            background: 'rgba(16, 185, 129, 0.08)',
            color: 'var(--success-color)',
            padding: '0.75rem',
            borderRadius: '0.5rem',
            textAlign: 'center',
            marginBottom: '1rem',
            fontWeight: 600,
          }}>
            {result}
          </div>
        )}

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem' }}>
          {/* Free */}
          <div style={{
            border: '2px solid var(--border-color, #e5e7eb)',
            borderRadius: '1rem',
            padding: '1.5rem',
          }}>
            <div style={{ fontWeight: 700, fontSize: '1.2rem', marginBottom: '0.25rem', color: 'var(--text-primary)' }}>
              Free
            </div>
            <div style={{ fontSize: '2rem', fontWeight: 700, color: 'var(--text-primary)', marginBottom: '0.25rem' }}>
              $0
            </div>
            <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '1.25rem' }}>
              навсегда
            </div>
            <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
              {FREE_FEATURES.map((f) => (
                <li key={f} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  <span style={{ color: 'var(--success-color)' }}>✓</span> {f}
                </li>
              ))}
            </ul>
          </div>

          {/* Pro */}
          <div style={{
            border: '2px solid var(--primary-color)',
            borderRadius: '1rem',
            padding: '1.5rem',
            background: 'rgba(99, 102, 241, 0.03)',
            position: 'relative',
          }}>
            <div style={{
              position: 'absolute',
              top: '-10px',
              right: '1rem',
              background: 'var(--primary-color)',
              color: '#fff',
              padding: '0.15rem 0.75rem',
              borderRadius: '1rem',
              fontSize: '0.7rem',
              fontWeight: 600,
            }}>
              Рекомендуем
            </div>
            <div style={{ fontWeight: 700, fontSize: '1.2rem', marginBottom: '0.25rem', color: 'var(--primary-color)' }}>
              Pro
            </div>
            <div style={{ fontSize: '2rem', fontWeight: 700, color: 'var(--text-primary)', marginBottom: '0.25rem' }}>
              $9.99
            </div>
            <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '1.25rem' }}>
              / месяц
            </div>
            <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
              {PRO_FEATURES.map((f) => (
                <li key={f} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.85rem', color: 'var(--text-primary)' }}>
                  <span style={{ color: 'var(--primary-color)' }}>✓</span> {f}
                </li>
              ))}
            </ul>
            <button
              onClick={handleUpgrade}
              disabled={isUpgrading}
              className="btn btn-primary"
              style={{
                width: '100%',
                marginTop: '1.25rem',
                padding: '0.75rem',
                borderRadius: '0.75rem',
              }}
            >
              {isUpgrading ? 'Обновление...' : 'Перейти на Pro'}
            </button>
          </div>
        </div>

        <button
          onClick={onClose}
          style={{
            display: 'block',
            margin: '1.5rem auto 0',
            background: 'none',
            border: 'none',
            color: 'var(--text-secondary)',
            cursor: 'pointer',
            fontSize: '0.9rem',
          }}
        >
          Закрыть
        </button>
      </div>
    </div>
  );
}
