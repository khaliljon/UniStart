import { createContext, useContext, useState, useCallback, useMemo, type ReactNode } from 'react';
import type { Locale, Translations } from './types';
import { ru } from './ru';
import { kz } from './kz';
import { en } from './en';

const translations: Record<Locale, Translations> = { ru, kz, en };

const STORAGE_KEY = 'unistart_locale';

function getInitialLocale(): Locale {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === 'ru' || stored === 'kz' || stored === 'en') return stored;
  } catch { /* SSR / private browsing */ }
  return 'ru';
}

export interface I18nContextValue {
  locale: Locale;
  setLocale: (l: Locale) => void;
  t: Translations;
  dateLocale: string;
}

export const I18nContext = createContext<I18nContextValue>({
  locale: 'ru',
  setLocale: () => {},
  t: ru,
  dateLocale: 'ru-RU',
});

export function I18nProvider({ children }: { children: ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(getInitialLocale);

  const setLocale = useCallback((l: Locale) => {
    setLocaleState(l);
    try { localStorage.setItem(STORAGE_KEY, l); } catch { /* ignore */ }
  }, []);

  const value = useMemo<I18nContextValue>(
    () => ({ locale, setLocale, t: translations[locale], dateLocale: DATE_LOCALES[locale] }),
    [locale, setLocale],
  );

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useTranslation() {
  return useContext(I18nContext);
}

export const DATE_LOCALES: Record<Locale, string> = { ru: 'ru-RU', kz: 'kk-KZ', en: 'en-US' };

export function getDateLocale(): string {
  const stored = localStorage.getItem(STORAGE_KEY) as Locale | null;
  const locale: Locale = stored === 'ru' || stored === 'kz' || stored === 'en' ? stored : 'ru';
  return DATE_LOCALES[locale];
}
