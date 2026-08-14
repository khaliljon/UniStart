import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from '../hooks/useTranslation';
import type { Translations } from '../i18n/types';

interface TourStep {
  target: string;
  titleKey: keyof Translations['tour'];
  descKey: keyof Translations['tour'];
  placement: 'top' | 'bottom' | 'left' | 'right';
}

const TOUR_STEPS: TourStep[] = [
  { target: '.navbar-brand', titleKey: 'welcomeTitle', descKey: 'welcomeDesc', placement: 'bottom' },
  { target: '.navbar-nav li:nth-child(1) a', titleKey: 'dashboardTitle', descKey: 'dashboardDesc', placement: 'bottom' },
  { target: '.navbar-nav li:nth-child(2) a', titleKey: 'learnTitle', descKey: 'learnDesc', placement: 'bottom' },
  { target: '.navbar-nav li:nth-child(3) a', titleKey: 'progressTitle', descKey: 'progressDesc', placement: 'bottom' },
  { target: '.navbar-nav li:nth-child(4) a', titleKey: 'studyPlanTitle', descKey: 'studyPlanDesc', placement: 'bottom' },
  { target: '.profile-dropdown', titleKey: 'profileTitle', descKey: 'profileDesc', placement: 'bottom' },
];

const STORAGE_KEY = 'unistart_tour_completed';

export default function GuidedTour() {
  const { t } = useTranslation();
  const [currentStep, setCurrentStep] = useState(0);
  const [visible, setVisible] = useState(false);
  const [tooltipPos, setTooltipPos] = useState({ top: 0, left: 0 });
  const [highlightRect, setHighlightRect] = useState({ top: 0, left: 0, width: 0, height: 0 });

  useEffect(() => {
    const completed = localStorage.getItem(STORAGE_KEY);
    if (!completed) {
      const timer = setTimeout(() => setVisible(true), 800);
      return () => clearTimeout(timer);
    }
  }, []);

  const positionTooltip = useCallback((step: TourStep) => {
    const el = document.querySelector(step.target);
    if (!el) return;
    const rect = el.getBoundingClientRect();
    const pad = 8;

    setHighlightRect({
      top: rect.top - pad,
      left: rect.left - pad,
      width: rect.width + pad * 2,
      height: rect.height + pad * 2,
    });

    const tooltipW = 340;
    const tooltipH = 180;

    let top = 0;
    let left = 0;

    switch (step.placement) {
      case 'bottom':
        top = rect.bottom + 16;
        left = rect.left + rect.width / 2 - tooltipW / 2;
        break;
      case 'top':
        top = rect.top - tooltipH - 16;
        left = rect.left + rect.width / 2 - tooltipW / 2;
        break;
      case 'right':
        top = rect.top + rect.height / 2 - tooltipH / 2;
        left = rect.right + 16;
        break;
      case 'left':
        top = rect.top + rect.height / 2 - tooltipH / 2;
        left = rect.left - tooltipW - 16;
        break;
    }
    left = Math.max(12, Math.min(left, window.innerWidth - tooltipW - 12));
    top = Math.max(12, top);

    setTooltipPos({ top, left });
  }, []);

  useEffect(() => {
    if (!visible) return;
    const step = TOUR_STEPS[currentStep];
    positionTooltip(step);

    const onResize = () => positionTooltip(step);
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize);
  }, [visible, currentStep, positionTooltip]);

  const handleNext = () => {
    if (currentStep < TOUR_STEPS.length - 1) {
      setCurrentStep((s) => s + 1);
    } else {
      handleClose();
    }
  };

  const handlePrev = () => {
    if (currentStep > 0) {
      setCurrentStep((s) => s - 1);
    }
  };

  const handleClose = () => {
    setVisible(false);
    localStorage.setItem(STORAGE_KEY, 'true');
  };

  if (!visible) return null;

  const step = TOUR_STEPS[currentStep];
  const isLast = currentStep === TOUR_STEPS.length - 1;

  return (
    <>
      <div
        style={{
          position: 'fixed',
          inset: 0,
          zIndex: 9998,
          pointerEvents: 'none',
        }}
      >
        <svg width="100%" height="100%" style={{ position: 'absolute', inset: 0 }}>
          <defs>
            <mask id="tour-mask">
              <rect width="100%" height="100%" fill="white" />
              <rect
                x={highlightRect.left}
                y={highlightRect.top}
                width={highlightRect.width}
                height={highlightRect.height}
                rx={8}
                fill="black"
              />
            </mask>
          </defs>
          <rect
            width="100%"
            height="100%"
            fill="rgba(0,0,0,0.55)"
            mask="url(#tour-mask)"
            style={{ pointerEvents: 'auto' }}
            onClick={handleClose}
          />
        </svg>
      </div>

      <div
        style={{
          position: 'fixed',
          top: highlightRect.top,
          left: highlightRect.left,
          width: highlightRect.width,
          height: highlightRect.height,
          borderRadius: 8,
          border: '2px solid #6366f1',
          boxShadow: '0 0 0 4px rgba(99,102,241,0.3)',
          zIndex: 9999,
          pointerEvents: 'none',
          transition: 'all 0.3s ease',
        }}
      />

      <div
        style={{
          position: 'fixed',
          top: tooltipPos.top,
          left: tooltipPos.left,
          width: 340,
          background: '#fff',
          borderRadius: 12,
          boxShadow: '0 8px 32px rgba(0,0,0,0.18)',
          zIndex: 10000,
          padding: '20px',
          transition: 'all 0.3s ease',
        }}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 8 }}>
          <h4 style={{ margin: 0, fontSize: 16, fontWeight: 700, color: '#1e293b' }}>
            {t.tour[step.titleKey]}
          </h4>
          <button
            onClick={handleClose}
            style={{
              background: 'none',
              border: 'none',
              cursor: 'pointer',
              fontSize: 18,
              color: '#94a3b8',
              padding: '0 4px',
              lineHeight: 1,
            }}
            aria-label="Close tour"
          >
            ×
          </button>
        </div>
        <p style={{ margin: '0 0 16px', fontSize: 14, lineHeight: 1.6, color: '#475569' }}>{t.tour[step.descKey]}</p>

        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <span style={{ fontSize: 12, color: '#94a3b8' }}>
            {currentStep + 1} / {TOUR_STEPS.length}
          </span>
          <div style={{ display: 'flex', gap: 8 }}>
            {currentStep > 0 && (
              <button
                onClick={handlePrev}
                style={{
                  padding: '6px 14px',
                  borderRadius: 6,
                  border: '1px solid #e2e8f0',
                  background: '#fff',
                  color: '#475569',
                  cursor: 'pointer',
                  fontSize: 13,
                  fontWeight: 500,
                }}
              >
                {t.tour.back}
              </button>
            )}
            <button
              onClick={handleNext}
              style={{
                padding: '6px 18px',
                borderRadius: 6,
                border: 'none',
                background: '#6366f1',
                color: '#fff',
                cursor: 'pointer',
                fontSize: 13,
                fontWeight: 600,
              }}
            >
              {isLast ? t.tour.getStarted : t.tour.next}
            </button>
          </div>
        </div>

        <div style={{ display: 'flex', justifyContent: 'center', gap: 6, marginTop: 12 }}>
          {TOUR_STEPS.map((_, i) => (
            <div
              key={i}
              style={{
                width: 7,
                height: 7,
                borderRadius: '50%',
                background: i === currentStep ? '#6366f1' : i < currentStep ? '#a5b4fc' : '#e2e8f0',
                transition: 'background 0.2s',
              }}
            />
          ))}
        </div>
      </div>
    </>
  );
}
