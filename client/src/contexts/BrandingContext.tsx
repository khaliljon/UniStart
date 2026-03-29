import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import { tutorService } from '../services/tutorService';
import type { SchoolBranding } from '../types';

interface BrandingContextValue {
  branding: SchoolBranding | null;
  isWhiteLabel: boolean;
}

const BrandingContext = createContext<BrandingContextValue>({
  branding: null,
  isWhiteLabel: false,
});

export function useBranding() {
  return useContext(BrandingContext);
}

function getSubdomain(): string | null {
  const host = window.location.hostname;
  // match *.unistart.kz but not unistart.kz or www.unistart.kz
  const match = host.match(/^([a-z0-9-]+)\.unistart\.kz$/i);
  if (match && match[1] !== 'www') return match[1];
  return null;
}

function setMeta(name: string, content: string, isProperty = false) {
  const attr = isProperty ? 'property' : 'name';
  let el = document.querySelector<HTMLMetaElement>(`meta[${attr}="${name}"]`);
  if (!el) {
    el = document.createElement('meta');
    el.setAttribute(attr, name);
    document.head.appendChild(el);
  }
  el.content = content;
}

export function BrandingProvider({ children }: { children: ReactNode }) {
  const [branding, setBranding] = useState<SchoolBranding | null>(null);
  const subdomain = getSubdomain();

  useEffect(() => {
    if (!subdomain) return;
    tutorService.getSchoolBranding(subdomain).then((data) => {
      if (!data) return;
      setBranding(data);

      // Apply CSS custom properties
      const root = document.documentElement;
      if (data.primaryColor) root.style.setProperty('--primary-color', data.primaryColor);
      if (data.primaryHoverColor) root.style.setProperty('--primary-hover', data.primaryHoverColor);
      if (data.accentColor) root.style.setProperty('--success-color', data.accentColor);

      // SEO: dynamic title, meta, OG tags
      const title = `${data.navbarTitle || data.name} — Подготовка к экзаменам на UniStart`;
      const description = data.description || `${data.navbarTitle || data.name} — подготовка к экзаменам с адаптивными тестами на платформе UniStart.`;
      const origin = window.location.origin;

      document.title = title;
      setMeta('description', description);
      setMeta('og:title', title, true);
      setMeta('og:description', description, true);
      setMeta('og:url', origin, true);
      setMeta('og:site_name', data.navbarTitle || data.name, true);
      setMeta('twitter:title', title, true);
      setMeta('twitter:description', description, true);

      if (data.logoUrl) {
        const logoAbsolute = data.logoUrl.startsWith('http') ? data.logoUrl : `${origin}${data.logoUrl}`;
        setMeta('og:image', logoAbsolute, true);
        setMeta('twitter:image', logoAbsolute, true);
        // Dynamic favicon — draw logo as circle with transparent background
        document.querySelectorAll<HTMLLinkElement>('link[rel="icon"]:not([data-brand])').forEach(l => l.remove());
        const img = new Image();
        img.crossOrigin = 'anonymous';
        img.onload = () => {
          const size = 64;
          const canvas = document.createElement('canvas');
          canvas.width = size;
          canvas.height = size;
          const ctx = canvas.getContext('2d')!;
          ctx.beginPath();
          ctx.arc(size / 2, size / 2, size / 2, 0, Math.PI * 2);
          ctx.closePath();
          ctx.clip();
          ctx.drawImage(img, 0, 0, size, size);
          let link = document.querySelector<HTMLLinkElement>('link[rel="icon"][data-brand]');
          if (!link) {
            link = document.createElement('link');
            link.rel = 'icon';
            link.setAttribute('data-brand', 'true');
            document.head.appendChild(link);
          }
          link.href = canvas.toDataURL('image/png');
        };
        img.src = logoAbsolute;
      }

      if (data.primaryColor) {
        setMeta('theme-color', data.primaryColor);
      }

      // Canonical
      let canonical = document.querySelector<HTMLLinkElement>('link[rel="canonical"]');
      if (canonical) canonical.href = origin;
    });
  }, [subdomain]);

  return (
    <BrandingContext.Provider value={{ branding, isWhiteLabel: !!subdomain }}>
      {children}
    </BrandingContext.Provider>
  );
}
