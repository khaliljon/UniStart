// Chinese-language subjects (Technical / Humanities Chinese) are taught only in
// Chinese, so the en/zh language choice does not apply to them.
const CHINESE_RE = /chinese|китай|қытай|кытай|汉语|中文|语文/i;

export function isChineseOnlySubject(...texts: Array<string | null | undefined>): boolean {
  return texts.some((t) => !!t && CHINESE_RE.test(t));
}
