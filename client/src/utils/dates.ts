const KZ_MONTHS = [
  'қаңтар', 'ақпан', 'наурыз', 'сәуір', 'мамыр', 'маусым',
  'шілде', 'тамыз', 'қыркүйек', 'қазан', 'қараша', 'желтоқсан',
];

type Loc = string | undefined;

const tagFor = (locale: Loc) => (locale === 'en' ? 'en-US' : locale === 'kz' ? 'kk-KZ' : 'ru-RU');

export function monthNameLocalized(iso: string, locale: Loc): string {
  const d = new Date(iso);
  if (locale === 'kz') {
    const m = KZ_MONTHS[d.getMonth()] ?? '';
    return m.charAt(0).toUpperCase() + m.slice(1);
  }
  const m = d.toLocaleDateString(tagFor(locale), { month: 'long' });
  return m.charAt(0).toUpperCase() + m.slice(1);
}

export function fullDateLocalized(iso: string, locale: Loc): string {
  const d = new Date(iso);
  if (locale === 'kz') return `${d.getFullYear()} ж. ${d.getDate()} ${KZ_MONTHS[d.getMonth()] ?? ''}`;
  return d.toLocaleDateString(tagFor(locale), { day: 'numeric', month: 'long', year: 'numeric' });
}

export function shortDateLocalized(iso: string, locale: Loc): string {
  const d = new Date(iso);
  if (locale === 'kz') {
    const dd = String(d.getDate()).padStart(2, '0');
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    return `${dd}.${mm}.${d.getFullYear()}`;
  }
  return d.toLocaleDateString(tagFor(locale));
}

/** "14–15 ноября 2026" for two-day sittings; falls back to a single date. */
export function dateRangeLocalized(startIso: string, endIso: string | null | undefined, locale: Loc): string {
  if (!endIso) return fullDateLocalized(startIso, locale);
  const a = new Date(startIso);
  const b = new Date(endIso);
  if (a.getTime() === b.getTime()) return fullDateLocalized(startIso, locale);

  // Same month → collapse to "14–15 <month> <year>".
  if (a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth()) {
    const month = monthNameLocalized(startIso, locale).toLowerCase();
    return locale === 'kz'
      ? `${a.getFullYear()} ж. ${a.getDate()}–${b.getDate()} ${month}`
      : locale === 'en'
        ? `${monthNameLocalized(startIso, locale)} ${a.getDate()}–${b.getDate()}, ${a.getFullYear()}`
        : `${a.getDate()}–${b.getDate()} ${month} ${a.getFullYear()}`;
  }
  return `${fullDateLocalized(startIso, locale)} – ${fullDateLocalized(endIso, locale)}`;
}
