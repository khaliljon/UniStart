import { useState } from 'react';
import { PricingModal } from './PricingModal';

/**
 * Modal shown when daily question limit is reached.
 */
interface DailyLimitModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function DailyLimitModal({ isOpen, onClose }: DailyLimitModalProps) {
  const [showPricing, setShowPricing] = useState(false);

  if (!isOpen) return null;

  return (
    <>
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
          padding: '2.5rem',
          maxWidth: '420px',
          width: '90%',
          textAlign: 'center',
          boxShadow: '0 20px 60px rgba(0,0,0,0.2)',
        }}>
          <div style={{ fontSize: '3.5rem', marginBottom: '1rem' }}>⏰</div>
          <h2 style={{ fontSize: '1.5rem', marginBottom: '0.75rem', color: 'var(--text-primary)' }}>
            Лимит на сегодня исчерпан
          </h2>
          <p style={{
            color: 'var(--text-secondary)',
            fontSize: '0.95rem',
            lineHeight: 1.6,
            marginBottom: '2rem',
          }}>
            Вы использовали все 15 бесплатных вопросов на сегодня.
            Возвращайтесь завтра или перейдите на Pro для безлимитной подготовки.
          </p>

          <button
            onClick={() => setShowPricing(true)}
            className="btn btn-primary"
            style={{
              width: '100%',
              padding: '0.85rem',
              borderRadius: '0.75rem',
              fontSize: '1rem',
              marginBottom: '0.75rem',
            }}
          >
            🚀 Перейти на Pro
          </button>

          <button
            onClick={onClose}
            style={{
              width: '100%',
              padding: '0.7rem',
              borderRadius: '0.75rem',
              border: '1px solid var(--border-color, #e5e7eb)',
              background: 'none',
              color: 'var(--text-secondary)',
              cursor: 'pointer',
              fontSize: '0.9rem',
            }}
          >
            Вернусь завтра
          </button>
        </div>
      </div>

      <PricingModal isOpen={showPricing} onClose={() => setShowPricing(false)} />
    </>
  );
}
