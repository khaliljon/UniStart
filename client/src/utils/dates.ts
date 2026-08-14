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
