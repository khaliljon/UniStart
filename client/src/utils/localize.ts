/** Picks a localized string, falling back to the base (RU) value when a translation is empty. */
export function pickLocalized(base: string, kz?: string | null, en?: string | null, locale?: string): string {
  if (locale === 'kz') return kz || base;
  if (locale === 'en') return en || base;
  return base;
}
