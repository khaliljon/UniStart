import type { ReactNode } from 'react';
import { useEffect } from 'react';
import CscaNav from './CscaNav';
import CscaFooter from './CscaFooter';

export default function CscaPageShell({ children }: { children: ReactNode }) {
  useEffect(() => { window.scrollTo({ top: 0 }); }, []);

  return (
    <div className="csca-landing">
      <CscaNav />
      {children}
      <CscaFooter />
    </div>
  );
}

export function CscaPageHero({ eyebrow, title, lead }: { eyebrow?: string; title: string; lead?: string }) {
  return (
    <header className="csca-wrap" style={{ padding: '3rem 1.5rem 1rem', textAlign: 'center' }}>
      {eyebrow && <span className="csca-eyebrow">{eyebrow}</span>}
      <h1 className="csca-hero-title" style={{ fontSize: 'clamp(2rem, 5vw, 3.2rem)', margin: '1rem auto 0.8rem' }}>{title}</h1>
      {lead && <p className="csca-hero-sub" style={{ margin: '0 auto' }}>{lead}</p>}
    </header>
  );
}
