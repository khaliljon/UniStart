// Chinese-language subjects (Technical / Humanities Chinese) are taught only in
// Chinese, so the en/zh language choice does not apply to them.
const CHINESE_RE = /chinese|китай|қытай|кытай|汉语|中文|语文/i;

export function isChineseOnlySubject(...texts: Array<string | null | undefined>): boolean {
  return texts.some((t) => !!t && CHINESE_RE.test(t));
}

// Stable, locale-independent machine key for a subject, derived from any of its
// (possibly localized) names. Used as the analytics `subject` parameter.
export function subjectSlug(...names: Array<string | null | undefined>): string {
  const text = names.filter(Boolean).join(' ').toLowerCase();
  const chinese = CHINESE_RE.test(text);
  if (chinese && /(tech|техн|科技)/i.test(text)) return 'chineseTech';
  if (chinese && /(hum|гуман|文科)/i.test(text)) return 'chineseHum';
  if (chinese) return 'chinese';
  if (/math|матем|数学/i.test(text)) return 'math';
  if (/phys|физи|物理/i.test(text)) return 'physics';
  if (/chem|хими|化学/i.test(text)) return 'chemistry';
  return 'unknown';
}
