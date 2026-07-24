// Locale-safe date helpers.
//
// Some runtimes (and reduced-ICU builds) don't ship Kazakh (kk) month names,
// so `toLocaleDateString('kk-KZ', { month: 'long' })` falls back to the root
// locale and renders months as "M01".."M12". To guarantee correct Kazakh
// output we map month names explicitly for KZ and defer to Intl for ru/en.

const KZ_MONTHS = [
  'қаңтар', 'ақпан', 'наурыз', 'сәуір', 'мамыр', 'маусым',
  'шілде', 'тамыз', 'қыркүйек', 'қазан', 'қараша', 'желтоқсан',
];

type Loc = string | undefined;

const tagFor = (locale: Loc) => (locale === 'en' ? 'en-US' : locale === 'kz' ? 'kk-KZ' : 'ru-RU');

/** Capitalized month name for the given date in the active locale. */
export function monthNameLocalized(iso: string, locale: Loc): string {
  const d = new Date(iso);
  if (locale === 'kz') {
    const m = KZ_MONTHS[d.getMonth()] ?? '';
    return m.charAt(0).toUpperCase() + m.slice(1);
  }
  const m = d.toLocaleDateString(tagFor(locale), { month: 'long' });
  return m.charAt(0).toUpperCase() + m.slice(1);
}

/** Full "day month year" date in the active locale (Kazakh-safe). */
export function fullDateLocalized(iso: string, locale: Loc): string {
  const d = new Date(iso);
  if (locale === 'kz') return `${d.getFullYear()} ж. ${d.getDate()} ${KZ_MONTHS[d.getMonth()] ?? ''}`;
  return d.toLocaleDateString(tagFor(locale), { day: 'numeric', month: 'long', year: 'numeric' });
}

/** Short numeric date (dd.mm.yyyy style) in the active locale (Kazakh-safe). */
export function shortDateLocalized(iso: string, locale: Loc): string {
  const d = new Date(iso);
  if (locale === 'kz') {
    const dd = String(d.getDate()).padStart(2, '0');
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    return `${dd}.${mm}.${d.getFullYear()}`;
  }
  return d.toLocaleDateString(tagFor(locale));
}
