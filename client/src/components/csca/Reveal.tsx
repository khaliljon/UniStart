import { useEffect, useRef, useState, type ReactNode, type CSSProperties } from 'react';

interface RevealProps {
  children: ReactNode;
  variant?: 'up' | 'left' | 'right' | 'zoom';
  delay?: number;
  stagger?: boolean;
  className?: string;
  style?: CSSProperties;
}

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
