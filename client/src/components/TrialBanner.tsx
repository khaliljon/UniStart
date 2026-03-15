import { useState } from 'react';
import { PricingModal } from './PricingModal';
import { useTranslation } from '../hooks/useTranslation';

interface TrialBannerProps {
  daysRemaining: number;
}

export function TrialBanner({ daysRemaining }: TrialBannerProps) {
  const { t } = useTranslation();
  const [showPricing, setShowPricing] = useState(false);
  const [dismissed, setDismissed] = useState(false);

  if (dismissed) return null;

  const isLastDay = daysRemaining <= 1;

  return (
    <>
      <div style={{
        background: isLastDay
          ? 'linear-gradient(135deg, #fef3c7, #fde68a)'
          : 'linear-gradient(135deg, #ecfdf5, #d1fae5)',
        borderRadius: '0.75rem',
        padding: '0.75rem 1rem',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: '1rem',
        marginBottom: '1rem',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', flex: 1 }}>
          <span style={{ fontSize: '1.2rem' }}>{isLastDay ? '⏰' : '🎉'}</span>
          <div>
            <div style={{
              fontWeight: 600,
              fontSize: '0.85rem',
              color: isLastDay ? '#92400e' : '#065f46',
            }}>
              {t.limits.trialActive.replace('{n}', String(daysRemaining))}
            </div>
            <div style={{
              fontSize: '0.75rem',
              color: isLastDay ? '#b45309' : '#047857',
              opacity: 0.8,
            }}>
              {t.limits.unlockDesc}
            </div>
          </div>
        </div>
        <button
          onClick={() => setShowPricing(true)}
          style={{
            flexShrink: 0,
            padding: '0.4rem 1rem',
            borderRadius: '0.5rem',
            border: 'none',
            background: isLastDay ? '#f59e0b' : '#10b981',
            color: '#fff',
            fontWeight: 600,
            fontSize: '0.8rem',
            cursor: 'pointer',
            whiteSpace: 'nowrap',
          }}
        >
          Pro
        </button>
        <button
          onClick={() => setDismissed(true)}
          style={{
            flexShrink: 0,
            background: 'none',
            border: 'none',
            cursor: 'pointer',
            fontSize: '1.1rem',
            lineHeight: 1,
            padding: '0.25rem',
            color: isLastDay ? '#92400e' : '#065f46',
            opacity: 0.6,
          }}
          aria-label="Close"
        >
          &times;
        </button>
      </div>

      <PricingModal isOpen={showPricing} onClose={() => setShowPricing(false)} />
    </>
  );
}
