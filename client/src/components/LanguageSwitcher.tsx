import { useTranslation } from '../hooks/useTranslation';
import type { Locale } from '../i18n/types';

const localeLabels: Record<Locale, string> = { ru: 'RU', kz: 'KZ', en: 'EN' };
const locales: Locale[] = ['ru', 'kz', 'en'];

export default function LanguageSwitcher() {
  const { locale, setLocale } = useTranslation();

  return (
    <div className="language-switcher">
      {locales.map(l => (
        <button
          key={l}
          className={`lang-btn${l === locale ? ' active' : ''}`}
          onClick={() => setLocale(l)}
        >
          {localeLabels[l]}
        </button>
      ))}
    </div>
  );
}
