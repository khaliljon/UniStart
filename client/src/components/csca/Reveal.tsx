import { useEffect, useRef, useState, type ReactNode, type CSSProperties } from 'react';

interface RevealProps {
  children: ReactNode;
  /** Direction/style of the entrance. */
  variant?: 'up' | 'left' | 'right' | 'zoom';
  /** Delay before the transition starts (ms). */
  delay?: number;
  /** Stagger direct children (for grids of cards). */
  stagger?: boolean;
  className?: string;
  style?: CSSProperties;
}

/**
 * Reveals its children with a smooth entrance the first time they scroll into
 * view (IntersectionObserver). Pure CSS drives the motion; this only toggles a
 * class. Honors prefers-reduced-motion via the CSS.
 */
export default function Reveal({
  children, variant = 'up', delay = 0, stagger = false, className = '', style,
}: RevealProps) {
  const ref = useRef<HTMLDivElement>(null);
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const io = new IntersectionObserver(
      (entries) => {
        for (const e of entries) {
          if (e.isIntersecting) {
            setVisible(true);
            io.unobserve(e.target);
          }
        }
      },
      { threshold: 0.15, rootMargin: '0px 0px -8% 0px' },
    );
    io.observe(el);
    return () => io.disconnect();
  }, []);

  const variantClass =
    variant === 'left' ? 'from-left'
    : variant === 'right' ? 'from-right'
    : variant === 'zoom' ? 'zoom'
    : '';

  return (
    <div
      ref={ref}
      className={`reveal ${variantClass} ${stagger ? 'reveal-stagger' : ''} ${visible ? 'is-visible' : ''} ${className}`}
      style={{ transitionDelay: `${delay}ms`, ...style }}
    >
      {children}
    </div>
  );
}
