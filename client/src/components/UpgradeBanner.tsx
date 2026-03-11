import { useState } from 'react';
import { PricingModal } from './PricingModal';
import { useTranslation } from '../hooks/useTranslation';

interface UpgradeBannerProps {
  questionsRemaining?: number;
  questionsLimit?: number;
}

export function UpgradeBanner({ questionsRemaining, questionsLimit }: UpgradeBannerProps) {
  const { t } = useTranslation();
  const [showPricing, setShowPricing] = useState(false);

  const isLow = questionsRemaining !== undefined && questionsRemaining <= 5;
  const isExhausted = questionsRemaining !== undefined && questionsRemaining <= 0;

  return (
    <>
      <div style={{
        background: isExhausted
          ? 'linear-gradient(135deg, #fef2f2, #fee2e2)'
          : isLow
            ? 'linear-gradient(135deg, #fffbeb, #fef3c7)'
            : 'linear-gradient(135deg, #eef2ff, #e0e7ff)',
        borderRadius: '0.75rem',
        padding: '0.75rem 1rem',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: '1rem',
        marginBottom: '1rem',
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
          <span style={{ fontSize: '1.2rem' }}>
            {isExhausted ? '✕' : isLow ? '!' : '★'}
          </span>
          <div>
            <div style={{
              fontWeight: 600,
              fontSize: '0.85rem',
              color: isExhausted ? '#991b1b' : isLow ? '#92400e' : '#3730a3',
            }}>
              {isExhausted
                ? t.limits.exhausted
                : questionsRemaining !== undefined
                  ? t.limits.remaining.replace('{n}', String(questionsRemaining)).replace('{total}', String(questionsLimit))
                  : t.limits.unlockAll}
            </div>
            <div style={{
              fontSize: '0.75rem',
              color: isExhausted ? '#b91c1c' : isLow ? '#b45309' : '#4338ca',
              opacity: 0.8,
            }}>
              {isExhausted
                ? t.limits.exhaustedDesc
                : isLow
                  ? t.limits.lowDesc.replace('{n}', String(questionsRemaining))
                  : t.limits.unlockDesc}
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
            background: isExhausted ? '#ef4444' : isLow ? '#f59e0b' : '#6366f1',
            color: '#fff',
            fontWeight: 600,
            fontSize: '0.8rem',
            cursor: 'pointer',
            whiteSpace: 'nowrap',
          }}
        >
          {isExhausted ? t.limits.upgrade : 'Pro'}
        </button>
      </div>

      <PricingModal isOpen={showPricing} onClose={() => setShowPricing(false)} />
    </>
  );
}
