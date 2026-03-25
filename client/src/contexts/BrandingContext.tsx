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
    });
  }, [subdomain]);

  return (
    <BrandingContext.Provider value={{ branding, isWhiteLabel: !!subdomain }}>
      {children}
    </BrandingContext.Provider>
  );
}
