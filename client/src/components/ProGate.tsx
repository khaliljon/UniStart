import { useState, type ReactNode } from 'react';
import { PricingModal } from './PricingModal';

interface ProGateProps {
  children: ReactNode;
  hasAccess: boolean;
  featureName?: string;
}

/**
 * Wraps Pro-only content with a blur overlay + CTA if user doesn't have access.
 * Usage: <ProGate hasAccess={user.isPro}><ExpensiveComponent /></ProGate>
 */
export function ProGate({ children, hasAccess, featureName = 'эту функцию' }: ProGateProps) {
  const [showPricing, setShowPricing] = useState(false);

  if (hasAccess) {
    return <>{children}</>;
  }

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
            <div style={{ fontSize: '2rem', marginBottom: '0.75rem' }}>🔒</div>
            <h3 style={{ margin: '0 0 0.5rem', fontSize: '1.1rem', color: 'var(--text-primary)' }}>
              Pro-функция
            </h3>
            <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', lineHeight: 1.5, marginBottom: '1rem' }}>
              Разблокируйте {featureName} с тарифом Pro для максимальной подготовки к экзамену.
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
              Узнать о Pro
            </button>
          </div>
        </div>
      </div>

      <PricingModal isOpen={showPricing} onClose={() => setShowPricing(false)} />
    </>
  );
}
