/** Russian pluralization for "мок": 1 мок, 3 мока, 5 моков. */
export function moks(n: number): string {
  const mod10 = n % 10;
  const mod100 = n % 100;
  let word: string;
  if (mod10 === 1 && mod100 !== 11) word = 'мок';
  else if (mod10 >= 2 && mod10 <= 4 && !(mod100 >= 12 && mod100 <= 14)) word = 'мока';
  else word = 'моков';
  return `${n} ${word}`;
}
