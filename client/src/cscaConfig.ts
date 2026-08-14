// Central, easy-to-edit configuration for the CSCA landing.
// Update exam dates, pricing and stats here — the UI reads from this file.

export interface CscaExamSitting {
  /** ISO date (YYYY-MM-DD) of the exam sitting. */
  date: string;
  /** Human label per locale is derived in the UI; this is the month key. */
  monthKey: string;
}

/** 2026 CSCA exam sittings (5 per year). Update as official dates are confirmed. */
export const CSCA_EXAM_SITTINGS: CscaExamSitting[] = [
  { date: '2026-01-17', monthKey: 'january' },
  { date: '2026-03-15', monthKey: 'march' },
  { date: '2026-06-27', monthKey: 'june' },
  { date: '2026-09-19', monthKey: 'september' },
  { date: '2026-11-21', monthKey: 'november' },
];

export const CSCA_REGISTRATION_OPENS = '2026-05-01';

export function getNextSitting(now: Date = new Date()): CscaExamSitting {
  const upcoming = CSCA_EXAM_SITTINGS.find((s) => new Date(s.date).getTime() >= now.getTime());
  return upcoming ?? CSCA_EXAM_SITTINGS[CSCA_EXAM_SITTINGS.length - 1];
}

export function daysUntil(isoDate: string, now: Date = new Date()): number {
  const ms = new Date(isoDate).getTime() - now.getTime();
  return Math.max(0, Math.ceil(ms / 86_400_000));
}

export interface CscaSubject {
  key: 'chineseTech' | 'chineseHum' | 'math' | 'physics' | 'chemistry';
  hanzi: string;
  cover: string;
  required?: boolean;
}

export const CSCA_SUBJECTS: CscaSubject[] = [
  { key: 'math', hanzi: '数学', cover: 'linear-gradient(160deg,#8A0B1F,#C8102E)', required: true },
  { key: 'physics', hanzi: '物理', cover: 'linear-gradient(160deg,#123a63,#1f5c9c)' },
  { key: 'chemistry', hanzi: '化学', cover: 'linear-gradient(160deg,#14532d,#1f7a44)' },
  { key: 'chineseTech', hanzi: '科技汉语', cover: 'linear-gradient(160deg,#5b3a1a,#a9762f)' },
  { key: 'chineseHum', hanzi: '文科汉语', cover: 'linear-gradient(160deg,#4a2a5b,#8a4fa3)' },
];

export interface CscaPackage {
  key: 'start' | 'standard' | 'advanced' | 'full';
  price: number;
  subjects: number | 'all';
  featured?: boolean;
}

export const CSCA_PACKAGES: CscaPackage[] = [
  { key: 'start', price: 2490, subjects: 1 },
  { key: 'standard', price: 4490, subjects: 2, featured: true },
  { key: 'advanced', price: 5990, subjects: 3 },
  { key: 'full', price: 7990, subjects: 'all' },
];

export const CSCA_BOOK_PRICE = 6990;

export const CSCA_STATS: { value: string; key: 'students' | 'questions' | 'answered' | 'success' }[] = [
  { value: '10 000+', key: 'students' },
  { value: '50+', key: 'questions' },
  { value: '5 000+', key: 'answered' },
  { value: '98%', key: 'success' },
];
