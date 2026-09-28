// Central, easy-to-edit configuration for the CSCA landing.
// Update exam dates, pricing and stats here — the UI reads from this file.

export interface CscaExamSitting {
  /** ISO date (YYYY-MM-DD) of day 1 of the exam sitting. */
  date: string;
  /** ISO date of day 2; sittings span two days. */
  endDate?: string | null;
  /** Human label per locale is derived in the UI; this is the month key. */
  monthKey: string;
}

/** Official CSCA schedule Nov 2026 – Jun 2027. Fallback only — the DB is the source of truth. */
export const CSCA_EXAM_SITTINGS: CscaExamSitting[] = [
  { date: '2026-11-14', endDate: '2026-11-15', monthKey: 'november' },
  { date: '2026-12-19', endDate: '2026-12-20', monthKey: 'december' },
  { date: '2027-01-23', endDate: '2027-01-24', monthKey: 'january' },
  { date: '2027-03-13', endDate: '2027-03-14', monthKey: 'march' },
  { date: '2027-04-24', endDate: '2027-04-25', monthKey: 'april' },
  { date: '2027-06-26', endDate: '2027-06-27', monthKey: 'june' },
];

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
  /** Exam duration in minutes. */
  minutes: number;
  /** Number of questions. */
  questions: number;
}

export const CSCA_SUBJECTS: CscaSubject[] = [
  { key: 'math', hanzi: '数学', cover: 'linear-gradient(160deg,#8A0B1F,#C8102E)', required: true, minutes: 60, questions: 48 },
  { key: 'physics', hanzi: '物理', cover: 'linear-gradient(160deg,#123a63,#1f5c9c)', minutes: 60, questions: 48 },
  { key: 'chemistry', hanzi: '化学', cover: 'linear-gradient(160deg,#14532d,#1f7a44)', minutes: 60, questions: 48 },
  { key: 'chineseTech', hanzi: '科技汉语', cover: 'linear-gradient(160deg,#5b3a1a,#a9762f)', minutes: 90, questions: 80 },
  { key: 'chineseHum', hanzi: '文科汉语', cover: 'linear-gradient(160deg,#4a2a5b,#8a4fa3)', minutes: 90, questions: 80 },
];

export type CscaSubjectKey = CscaSubject['key'];

/** Official CSCA exam fee in CNY. */
export const CSCA_FEE = { single: 450, multiple: 700 };

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
