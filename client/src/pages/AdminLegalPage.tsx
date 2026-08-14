import { useEffect, useState } from 'react';
import { useTranslation } from '../hooks/useTranslation';
import legalService, { type LegalDocument } from '../services/legalService';

type Lang = 'ru' | 'kz' | 'en';

const LANGS: { key: Lang; label: string }[] = [
  { key: 'ru', label: 'Русский' },
  { key: 'kz', label: 'Қазақша' },
  { key: 'en', label: 'English' },
];

function AdminLegalPage() {
  const { t } = useTranslation();
  const [docs, setDocs] = useState<LegalDocument[]>([]);
  const [activeSlug, setActiveSlug] = useState<string>('');
  const [lang, setLang] = useState<Lang>('ru');
  const [title, setTitle] = useState({ ru: '', kz: '', en: '' });
  const [lastUpdatedLabel, setLastUpdatedLabel] = useState({ ru: '', kz: '', en: '' });
  const [content, setContent] = useState({ ru: '', kz: '', en: '' });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [savedAt, setSavedAt] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    legalService
      .getAll()
      .then((list) => {
        const filtered = list.filter((d) => d.slug !== 'referral');
        setDocs(filtered);
        if (filtered.length > 0) selectDoc(filtered[0]);
      })
      .catch(() => setError(t.common.error))
      .finally(() => setLoading(false));
  }, []);

  const selectDoc = (doc: LegalDocument) => {
    setActiveSlug(doc.slug);
    setLang('ru');
    setTitle({ ru: doc.title, kz: doc.titleKz ?? '', en: doc.titleEn ?? '' });
    setLastUpdatedLabel({
      ru: doc.lastUpdatedLabel,
      kz: doc.lastUpdatedLabelKz ?? '',
      en: doc.lastUpdatedLabelEn ?? '',
    });
    setContent({ ru: doc.content, kz: doc.contentKz ?? '', en: doc.contentEn ?? '' });
    setSavedAt(null);
    setError(null);
  };

  const handleSelect = (slug: string) => {
    const doc = docs.find((d) => d.slug === slug);
    if (doc) selectDoc(doc);
  };

  const handleSave = async () => {
    if (!activeSlug) return;
    setSaving(true);
    setError(null);
    setSavedAt(null);
    try {
      const updated = await legalService.update(activeSlug, {
        title: title.ru,
        lastUpdatedLabel: lastUpdatedLabel.ru,
        content: content.ru,
        titleKz: title.kz || null,
        titleEn: title.en || null,
        lastUpdatedLabelKz: lastUpdatedLabel.kz || null,
        lastUpdatedLabelEn: lastUpdatedLabel.en || null,
        contentKz: content.kz || null,
        contentEn: content.en || null,
      });
      setDocs((prev) => prev.map((d) => (d.slug === updated.slug ? updated : d)));
      setSavedAt(new Date().toLocaleTimeString());
    } catch {
      setError(t.common.error);
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return <div className="container" style={{ padding: '2rem' }}>{t.common.loading}</div>;
  }

  return (
    <div className="container" style={{ padding: '1.5rem 0 4rem' }}>
      <h1 style={{ fontSize: '1.75rem', fontWeight: 700, marginBottom: '1.5rem' }}>
        {t.admin.legal.title}
      </h1>

      <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginBottom: '1.5rem' }}>
        {docs.map((d) => (
          <button
            key={d.slug}
            onClick={() => handleSelect(d.slug)}
            className={activeSlug === d.slug ? 'btn btn-primary' : 'btn btn-outline'}
          >
            {d.title}
          </button>
        ))}
      </div>

      {activeSlug && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', maxWidth: '900px' }}>
          <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
            {LANGS.map((l) => (
              <button
                key={l.key}
                onClick={() => setLang(l.key)}
                className={lang === l.key ? 'btn btn-primary' : 'btn btn-outline'}
                style={{ padding: '0.4rem 0.9rem', fontSize: '0.85rem' }}
              >
                {l.label}
              </button>
            ))}
          </div>

          <label style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
            <span style={{ fontWeight: 600, fontSize: '0.9rem' }}>{t.admin.legal.docTitle}</span>
            <input
              type="text"
              value={title[lang]}
              onChange={(e) => setTitle((p) => ({ ...p, [lang]: e.target.value }))}
              style={inputStyle}
            />
          </label>

          <label style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
            <span style={{ fontWeight: 600, fontSize: '0.9rem' }}>{t.admin.legal.lastUpdated}</span>
            <input
              type="text"
              value={lastUpdatedLabel[lang]}
              onChange={(e) => setLastUpdatedLabel((p) => ({ ...p, [lang]: e.target.value }))}
              style={inputStyle}
            />
          </label>

          <label style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
            <span style={{ fontWeight: 600, fontSize: '0.9rem' }}>{t.admin.legal.content}</span>
            <textarea
              value={content[lang]}
              onChange={(e) => setContent((p) => ({ ...p, [lang]: e.target.value }))}
              rows={26}
              style={{ ...inputStyle, resize: 'vertical', fontFamily: 'inherit', lineHeight: 1.6, whiteSpace: 'pre-wrap' }}
            />
          </label>

          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
            <button onClick={handleSave} disabled={saving} className="btn btn-primary">
              {saving ? t.common.loading : t.common.save}
            </button>
            {savedAt && (
              <span style={{ color: 'var(--success-color)', fontSize: '0.85rem' }}>
                {t.admin.legal.saved} ({savedAt})
              </span>
            )}
            {error && (
              <span style={{ color: 'var(--error-color)', fontSize: '0.85rem' }}>{error}</span>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

const inputStyle: React.CSSProperties = {
  padding: '0.6rem 0.75rem',
  borderRadius: '0.5rem',
  border: '1px solid var(--border-color)',
  background: 'var(--input-background, var(--card-background))',
  color: 'var(--text-primary)',
  fontSize: '0.95rem',
  width: '100%',
};

export default AdminLegalPage;
