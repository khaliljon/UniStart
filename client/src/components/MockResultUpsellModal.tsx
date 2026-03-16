import { useState } from 'react';
import { PricingModal } from './PricingModal';
import { useTranslation } from '../hooks/useTranslation';

interface MockResultUpsellModalProps {
  isOpen: boolean;
  onClose: () => void;
  score: number;
  totalCorrect: number;
  totalQuestions: number;
}

export function MockResultUpsellModal({ isOpen, onClose, score, totalCorrect, totalQuestions }: MockResultUpsellModalProps) {
  const { t } = useTranslation();
  const [step, setStep] = useState(1);
  const [showPricing, setShowPricing] = useState(false);

  if (!isOpen) return null;

  const overlay: React.CSSProperties = {
    position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex',
    alignItems: 'center', justifyContent: 'center', zIndex: 1000, padding: '1rem',
  };
  const modal: React.CSSProperties = {
    background: 'var(--bg-primary)', borderRadius: 16, maxWidth: 480, width: '100%',
    padding: '2rem', position: 'relative', maxHeight: '90vh', overflowY: 'auto',
  };
  const stepIndicator: React.CSSProperties = {
    display: 'flex', justifyContent: 'center', gap: '0.5rem', marginBottom: '1.5rem',
  };

  return (
    <>
      <div style={overlay} onClick={onClose}>
        <div style={modal} onClick={e => e.stopPropagation()}>
          {/* Close */}
          <button onClick={onClose} style={{
            position: 'absolute', top: 12, right: 16, background: 'none', border: 'none',
            fontSize: '1.5rem', cursor: 'pointer', color: 'var(--text-secondary)',
          }}>×</button>

          {/* Step indicator dots */}
          <div style={stepIndicator}>
            {[1, 2, 3, 4].map(s => (
              <div key={s} style={{
                width: 8, height: 8, borderRadius: '50%',
                background: s === step ? 'var(--primary-color)' : 'var(--border-color)',
                transition: 'background 0.2s',
              }} />
            ))}
          </div>

          {/* Step 1: Score + CTA analytics */}
          {step === 1 && (
            <div style={{ textAlign: 'center' }}>
              <div style={{ fontSize: '2.5rem', marginBottom: '0.25rem' }}>🎯</div>
              <h2 style={{ margin: '0 0 0.5rem', fontSize: '1.3rem' }}>{t.mockUpsell.title}</h2>
              <div style={{
                fontSize: '2.5rem', fontWeight: 800, margin: '0.5rem 0',
                color: score >= 80 ? '#27ae60' : score >= 60 ? '#f39c12' : '#e74c3c',
              }}>
                {score}%
              </div>
              <p style={{ color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>
                {t.mockUpsell.scoreText}: {totalCorrect} / {totalQuestions}
              </p>
              <p style={{ color: 'var(--text-primary)', fontWeight: 500, margin: '1rem 0' }}>
                {t.mockUpsell.wantAnalytics}
              </p>
              <button className="btn btn-primary" style={{ width: '100%', padding: '0.75rem' }}
                onClick={() => setStep(2)}>
                {t.mockUpsell.showAnalytics} →
              </button>
            </div>
          )}

          {/* Step 2: Analytics preview (blurred) */}
          {step === 2 && (
            <div style={{ textAlign: 'center' }}>
              <div style={{ fontSize: '2.5rem', marginBottom: '0.25rem' }}>📊</div>
              <h2 style={{ margin: '0 0 1rem', fontSize: '1.2rem' }}>{t.mockUpsell.analyticsPreview}</h2>

              {/* Faux blurred radar chart */}
              <div style={{
                height: 160, borderRadius: 12, margin: '0 auto 1rem',
                background: 'linear-gradient(135deg, var(--bg-secondary), var(--border-color))',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                filter: 'blur(3px)', position: 'relative', overflow: 'hidden',
              }}>
                <div style={{ fontWeight: 700, fontSize: '1.2rem', color: 'var(--text-secondary)' }}>
                  Radar Chart Preview
                </div>
              </div>

              <div style={{
                background: 'var(--bg-secondary)', borderRadius: 10, padding: '0.75rem 1rem',
                marginBottom: '1rem', fontSize: '0.9rem', color: 'var(--text-secondary)', lineHeight: 1.5,
              }}>
                {t.mockUpsell.analyticsPreview}
              </div>

              <button className="btn btn-primary" style={{ width: '100%', padding: '0.75rem' }}
                onClick={() => setStep(3)}>
                {t.mockUpsell.whatElse} →
              </button>
            </div>
          )}

          {/* Step 3: Mistakes preview (blurred partial list) */}
          {step === 3 && (
            <div style={{ textAlign: 'center' }}>
              <div style={{ fontSize: '2.5rem', marginBottom: '0.25rem' }}>🔍</div>
              <h2 style={{ margin: '0 0 1rem', fontSize: '1.2rem' }}>{t.mockUpsell.mistakesPreview}</h2>

              {/* Faux mistake items — first visible, rest blurred */}
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', marginBottom: '1rem' }}>
                <div style={{
                  background: 'var(--bg-secondary)', borderRadius: 8, padding: '0.6rem 0.8rem',
                  borderLeft: '4px solid #e74c3c', textAlign: 'left', fontSize: '0.85rem',
                }}>
                  <div style={{ fontWeight: 600, marginBottom: '0.25rem' }}>Q12. Sample question...</div>
                  <div style={{ color: '#27ae60', fontSize: '0.8rem' }}>✓ Correct: Option A</div>
                </div>
                {[1, 2, 3].map(i => (
                  <div key={i} style={{
                    background: 'var(--bg-secondary)', borderRadius: 8, padding: '0.6rem 0.8rem',
                    borderLeft: '4px solid #e74c3c', filter: 'blur(4px)', textAlign: 'left', fontSize: '0.85rem',
                  }}>
                    <div style={{ fontWeight: 600 }}>Q{12 + i}. Blurred question...</div>
                    <div style={{ color: '#27ae60', fontSize: '0.8rem' }}>✓ Correct: Option X</div>
                  </div>
                ))}
              </div>

              <button className="btn btn-primary" style={{ width: '100%', padding: '0.75rem' }}
                onClick={() => setStep(4)}>
                {t.mockUpsell.tryPro} →
              </button>
            </div>
          )}

          {/* Step 4: CTA with pricing */}
          {step === 4 && (
            <div style={{ textAlign: 'center' }}>
              <div style={{ fontSize: '2.5rem', marginBottom: '0.25rem' }}>🚀</div>
              <h2 style={{ margin: '0 0 0.75rem', fontSize: '1.3rem' }}>{t.mockUpsell.tryPro}</h2>

              <div style={{
                background: 'linear-gradient(135deg, #667eea20, #764ba220)',
                borderRadius: 12, padding: '1.25rem', marginBottom: '1rem',
              }}>
                <div style={{ fontSize: '1.5rem', fontWeight: 800, marginBottom: '0.25rem' }}>
                  {t.mockUpsell.specialOffer}
                </div>
              </div>

              <button className="btn btn-primary" style={{ width: '100%', padding: '0.75rem', fontSize: '1rem' }}
                onClick={() => setShowPricing(true)}>
                {t.mockUpsell.tryPro}
              </button>
              <button className="btn btn-outline" style={{ width: '100%', marginTop: '0.5rem', padding: '0.6rem' }}
                onClick={onClose}>
                {t.mockUpsell.close}
              </button>
            </div>
          )}
        </div>
      </div>

      <PricingModal isOpen={showPricing} onClose={() => { setShowPricing(false); onClose(); }} />
    </>
  );
}
