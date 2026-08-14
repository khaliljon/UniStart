export type PluralLocale = 'ru' | 'kz' | 'en';

export function moks(n: number, locale: PluralLocale = 'ru'): string {
  if (locale === 'en') return `${n} ${n === 1 ? 'mock' : 'mocks'}`;
  if (locale === 'kz') return `${n} мок`;
  const mod10 = n % 10;
  const mod100 = n % 100;
  let word: string;
  if (mod10 === 1 && mod100 !== 11) word = 'мок';
  else if (mod10 >= 2 && mod10 <= 4 && !(mod100 >= 12 && mod100 <= 14)) word = 'мока';
  else word = 'моков';
  return `${n} ${word}`;
}
