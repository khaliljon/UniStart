// Country dial codes for the phone input. `dial` is the E.164 country calling
// code without the leading "+". `min`/`max` are the expected national number
// length (digits after the code) used for light validation.
export interface Country {
  code: string;   // ISO-3166 alpha-2
  name: string;
  dial: string;
  min: number;
  max: number;
}

export const COUNTRIES: Country[] = [
  { code: 'KZ', name: 'Қазақстан / Казахстан', dial: '7', min: 10, max: 10 },
  { code: 'RU', name: 'Россия', dial: '7', min: 10, max: 10 },
  { code: 'KG', name: 'Кыргызстан', dial: '996', min: 9, max: 9 },
  { code: 'UZ', name: 'Ózbekiston', dial: '998', min: 9, max: 9 },
  { code: 'TJ', name: 'Тоҷикистон', dial: '992', min: 9, max: 9 },
  { code: 'TM', name: 'Türkmenistan', dial: '993', min: 8, max: 8 },
  { code: 'AZ', name: 'Azərbaycan', dial: '994', min: 9, max: 9 },
  { code: 'AM', name: 'Հայաստան', dial: '374', min: 8, max: 8 },
  { code: 'GE', name: 'საქართველო', dial: '995', min: 9, max: 9 },
  { code: 'BY', name: 'Беларусь', dial: '375', min: 9, max: 9 },
  { code: 'UA', name: 'Україна', dial: '380', min: 9, max: 9 },
  { code: 'MD', name: 'Moldova', dial: '373', min: 8, max: 8 },
  { code: 'TR', name: 'Türkiye', dial: '90', min: 10, max: 10 },
  { code: 'CN', name: '中国', dial: '86', min: 11, max: 11 },
  { code: 'IN', name: 'India', dial: '91', min: 10, max: 10 },
  { code: 'AE', name: 'الإمارات', dial: '971', min: 9, max: 9 },
  { code: 'US', name: 'United States', dial: '1', min: 10, max: 10 },
  { code: 'GB', name: 'United Kingdom', dial: '44', min: 10, max: 10 },
  { code: 'DE', name: 'Deutschland', dial: '49', min: 10, max: 11 },
  { code: 'FR', name: 'France', dial: '33', min: 9, max: 9 },
  { code: 'PL', name: 'Polska', dial: '48', min: 9, max: 9 },
  { code: 'KR', name: '한국', dial: '82', min: 9, max: 10 },
  { code: 'JP', name: '日本', dial: '81', min: 10, max: 10 },
];

/** All dial codes, longest first — so prefix matching picks the most specific. */
const DIALS_BY_LENGTH = [...COUNTRIES].sort((a, b) => b.dial.length - a.dial.length);

/** Best-guess the user's country from the browser locale region. */
export function detectCountry(): Country {
  try {
    const region =
      new Intl.Locale(navigator.language).region ||
      navigator.language.split('-')[1]?.toUpperCase();
    if (region) {
      const found = COUNTRIES.find((c) => c.code === region);
      if (found) return found;
    }
  } catch {
    /* ignore */
  }
  return COUNTRIES[0]; // Kazakhstan
}

/** Splits a full "+<dial><national>" string into a country + national digits. */
export function splitPhone(full: string | null | undefined): { country: Country; national: string } {
  const digits = (full ?? '').replace(/\D/g, '');
  if (digits) {
    const match = DIALS_BY_LENGTH.find((c) => digits.startsWith(c.dial));
    if (match) return { country: match, national: digits.slice(match.dial.length) };
  }
  return { country: detectCountry(), national: '' };
}

/** True when the national part length fits the country's expected range. */
export function isValidPhone(country: Country, national: string): boolean {
  const len = national.replace(/\D/g, '').length;
  return len >= country.min && len <= country.max;
}
