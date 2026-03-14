import { useState, type ReactNode } from 'react';
import { PricingModal } from './PricingModal';
import { useTranslation } from '../hooks/useTranslation';

interface ProGateProps {
  children: ReactNode;
  hasAccess: boolean;
  featureName?: string;
}

/**
 * Wraps Pro-only content with a blur overlay + CTA if user doesn't have access.
 * Usage: <ProGate hasAccess={user.isPro}><ExpensiveComponent /></ProGate>
 */
export function ProGate({ children, hasAccess, featureName }: ProGateProps) {
  const { t } = useTranslation();
  const [showPricing, setShowPricing] = useState(false);

  if (hasAccess) {
    return <>{children}</>;
  }

  const displayFeature = featureName || t.common.loading;

  return (
    <>
      <div style={{ position: 'relative' }}>
        {/* Blurred content teaser */}
        <div style={{
          filter: 'blur(6px)',
          pointerEvents: 'none',
          userSelect: 'none',
          opacity: 0.6,
        }}>
          {children}
        </div>

        {/* Overlay */}
        <div style={{
          position: 'absolute',
          inset: 0,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          background: 'rgba(255,255,255,0.1)',
          borderRadius: '1rem',
        }}>
          <div style={{
            background: 'var(--card-bg, #fff)',
            borderRadius: '1rem',
            padding: '2rem',
            textAlign: 'center',
            boxShadow: '0 8px 32px rgba(0,0,0,0.12)',
            maxWidth: '320px',
          }}>
            <h3 style={{ margin: '0 0 0.5rem', fontSize: '1.1rem', color: 'var(--text-primary)' }}>
              {t.limits.proFeature}
            </h3>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', lineHeight: 1.5, marginBottom: '1rem' }}>
              {t.limits.proFeatureDesc.replace('{feature}', displayFeature)}
            </p>
            <button
              onClick={() => setShowPricing(true)}
              className="btn btn-primary"
              style={{
                padding: '0.6rem 1.5rem',
                borderRadius: '0.75rem',
                fontSize: '0.9rem',
              }}
            >
              {t.limits.learnAboutPro}
            </button>
          </div>
        </div>
      </div>

      <PricingModal isOpen={showPricing} onClose={() => setShowPricing(false)} />
    </>
  );
}
