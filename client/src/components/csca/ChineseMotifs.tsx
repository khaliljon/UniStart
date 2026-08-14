interface MotifProps {
  className?: string;
  style?: React.CSSProperties;
}

export function BrushDivider({ className, style }: MotifProps) {
  return (
    <svg className={className} style={style} viewBox="0 0 1200 26" fill="none" preserveAspectRatio="none" aria-hidden="true">
      <path
        d="M8 15C120 4 240 20 360 13C480 6 560 18 680 12C800 6 900 21 1010 12C1080 7 1140 12 1192 10"
        stroke="currentColor" strokeWidth="6" strokeLinecap="round"
        style={{ filter: 'drop-shadow(0 1px 0 rgba(0,0,0,0.05))' }}
      />
      <path d="M40 20C300 16 900 16 1160 19" stroke="currentColor" strokeWidth="2" strokeLinecap="round" opacity="0.4" />
    </svg>
  );
}

export function CloudPattern({ className, style }: MotifProps) {
  return (
    <svg className={className} style={style} viewBox="0 0 120 60" fill="none" aria-hidden="true">
      <path
        d="M10 40c0-8 6-13 13-12 1-9 9-15 18-13 4-6 13-7 18-2 8-2 15 4 14 12 7 1 11 7 9 14H10z"
        stroke="currentColor" strokeWidth="2" opacity="0.5"
      />
    </svg>
  );
}

export function PagodaMark({ className, style }: MotifProps) {
  return (
    <svg className={className} style={style} viewBox="0 0 64 64" fill="none" aria-hidden="true">
      <circle cx="32" cy="30" r="26" stroke="currentColor" strokeWidth="2.5" opacity="0.9" />
      <path d="M20 22h24l-4 4H24z" fill="currentColor" />
      <path d="M23 28h18l-3 4H26z" fill="currentColor" />
      <path d="M26 34h12v8H26z" fill="currentColor" />
      <path d="M31 14l6 6H25z" fill="currentColor" />
      <path d="M14 46c6-3 12-3 18 0 6-3 12-3 18 0v3c-6-3-12-3-18 0-6-3-12-3-18 0z" fill="currentColor" />
    </svg>
  );
}

export function SealStamp({ text = '备考', size = 56, className, style }: MotifProps & { text?: string; size?: number }) {
  return (
    <div
      className={className}
      style={{
        width: size, height: size, borderRadius: 10,
        background: 'var(--csca-red, #C8102E)', color: '#fff',
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        boxShadow: '0 4px 12px rgba(200,16,46,0.35)',
        fontFamily: "'Noto Serif SC', serif", fontWeight: 700,
        lineHeight: 1.05, letterSpacing: '0.02em', textAlign: 'center',
        fontSize: text.length > 2 ? size * 0.26 : size * 0.4,
        writingMode: text.length > 2 ? 'vertical-rl' : 'horizontal-tb',
        ...style,
      }}
      aria-hidden="true"
    >
      {text}
    </div>
  );
}

export function MistMountains({ className, style }: MotifProps) {
  return (
    <svg className={className} style={style} viewBox="0 0 1200 220" fill="none" preserveAspectRatio="none" aria-hidden="true">
      <path d="M0 180L120 120L220 165L340 90L460 160L600 70L740 155L880 100L1010 165L1120 120L1200 168V220H0z" fill="currentColor" opacity="0.10" />
      <path d="M0 200L150 150L300 185L470 130L640 190L820 140L980 190L1140 155L1200 185V220H0z" fill="currentColor" opacity="0.16" />
    </svg>
  );
}

export function FeatureIcon({ name, size = 24 }: { name: 'adaptive' | 'analytics' | 'mock' | 'materials' | 'ai' | 'plan'; size?: number }) {
  const common = { width: size, height: size, viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', strokeWidth: 2, strokeLinecap: 'round' as const, strokeLinejoin: 'round' as const, 'aria-hidden': true };
  switch (name) {
    case 'adaptive':
      return (<svg {...common}><path d="M12 3v18" /><path d="M5 8l7-5 7 5" /><circle cx="12" cy="14" r="3" /></svg>);
    case 'analytics':
      return (<svg {...common}><path d="M3 3v18h18" /><rect x="7" y="11" width="3" height="6" /><rect x="12" y="7" width="3" height="10" /><rect x="17" y="13" width="3" height="4" /></svg>);
    case 'mock':
      return (<svg {...common}><rect x="4" y="3" width="16" height="18" rx="2" /><path d="M8 8h8M8 12h8M8 16h5" /></svg>);
    case 'materials':
      return (<svg {...common}><path d="M4 5a2 2 0 0 1 2-2h9v18H6a2 2 0 0 1-2-2z" /><path d="M15 3h3a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-3" /></svg>);
    case 'ai':
      return (<svg {...common}><circle cx="12" cy="12" r="4" /><path d="M12 2v3M12 19v3M2 12h3M19 12h3M5 5l2 2M17 17l2 2M19 5l-2 2M7 17l-2 2" /></svg>);
    case 'plan':
      return (<svg {...common}><rect x="3" y="4" width="18" height="18" rx="2" /><path d="M3 10h18M8 2v4M16 2v4" /><path d="M8 14l2 2 4-4" /></svg>);
  }
}
