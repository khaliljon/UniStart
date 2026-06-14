import { useState, useEffect, useRef, useCallback } from 'react';
import { schoolAdminService, type SchoolOwnBranding } from '../services/schoolAdminService';
import { useToast } from '../components/Toast';
import { useTranslation } from '../i18n';

const fieldLabel: React.CSSProperties = {
  display: 'flex', flexDirection: 'column', gap: '0.35rem',
  fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-secondary)',
};
const fieldInput: React.CSSProperties = {
  padding: '0.55rem 0.7rem', borderRadius: '0.5rem',
  border: '1px solid var(--border-color)', background: 'var(--bg-color)',
  color: 'var(--text-color)', fontSize: '0.9rem', fontWeight: 400,
};
const colorInput: React.CSSProperties = {
  width: '100%', height: 38, padding: 0,
  border: '1px solid var(--border-color)', borderRadius: '0.4rem', cursor: 'pointer',
};

function SchoolAdminBrandingPage() {
  const { t } = useTranslation();
  const { showToast } = useToast();
  const b = t.schoolAdmin.branding;

  const [form, setForm] = useState<SchoolOwnBranding | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await schoolAdminService.getBranding();
      setForm(data);
    } catch (err) {
      const status = (err as { response?: { status?: number } })?.response?.status;
      setError(status === 404 ? b.noSchool : b.loadError);
    } finally {
      setLoading(false);
    }
  }, [b.loadError, b.noSchool]);

  useEffect(() => { load(); }, [load]);

  const handleUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = '';
    if (!file || !form) return;
    setUploading(true);
    try {
      const url = await schoolAdminService.uploadImage(file);
      setForm({ ...form, logoUrl: url });
    } catch (err) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
      showToast(msg || b.uploadError, 'error');
    } finally {
      setUploading(false);
    }
  };

  const handleSave = async () => {
    if (!form) return;
    setSaving(true);
    try {
      await schoolAdminService.updateBranding({
        navbarTitle: form.navbarTitle,
        logoUrl: form.logoUrl,
        websiteUrl: form.websiteUrl,
        instagramUrl: form.instagramUrl,
        telegramUrl: form.telegramUrl,
        primaryColor: form.primaryColor,
        primaryHoverColor: form.primaryHoverColor,
        accentColor: form.accentColor,
      });
      showToast(b.saved, 'success');
    } catch (err) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
      showToast(msg || b.saveError, 'error');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return <div className="loading"><div className="spinner" /></div>;
  }

  if (!form) {
    return (
      <div className="card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
        {error || b.noSchool}
      </div>
    );
  }

  const brandName = form.navbarTitle || form.name;

  return (
    <div style={{ maxWidth: 640, margin: '0 auto' }}>
      <div style={{ marginBottom: '1.25rem' }}>
        <h1 style={{ margin: 0, fontSize: '1.4rem', fontWeight: 700 }}>{b.title}</h1>
        <p style={{ margin: '0.4rem 0 0', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{b.subtitle}</p>
      </div>

      <div className="card" style={{ padding: '1.5rem', display: 'flex', flexDirection: 'column', gap: '1.1rem' }}>
        {/* Logo */}
        <div>
          <div style={{ ...fieldLabel, marginBottom: '0.5rem' }}>{b.logo}</div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
            {form.logoUrl ? (
              <img
                src={form.logoUrl}
                alt={brandName}
                style={{ width: 64, height: 64, borderRadius: '50%', objectFit: 'cover', border: '1px solid var(--border-color)' }}
              />
            ) : (
              <div style={{
                width: 64, height: 64, borderRadius: '50%',
                background: `linear-gradient(135deg, ${form.primaryColor || 'var(--primary-color)'}, ${form.primaryHoverColor || 'var(--primary-hover)'})`,
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                color: '#fff', fontWeight: 700, fontSize: '1.5rem',
              }}>
                {brandName.charAt(0).toUpperCase()}
              </div>
            )}
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button
                type="button"
                className="btn btn-outline"
                style={{ padding: '0.45rem 0.9rem' }}
                disabled={uploading}
                onClick={() => fileRef.current?.click()}
              >
                {uploading ? b.saving : b.uploadLogo}
              </button>
              {form.logoUrl && (
                <button
                  type="button"
                  className="btn btn-outline"
                  style={{ padding: '0.45rem 0.9rem' }}
                  disabled={uploading}
                  onClick={() => setForm({ ...form, logoUrl: null })}
                >
                  {b.removeLogo}
                </button>
              )}
            </div>
            <input
              ref={fileRef}
              type="file"
              accept="image/png,image/jpeg,image/webp,image/gif"
              style={{ display: 'none' }}
              onChange={handleUpload}
            />
          </div>
          <p style={{ margin: '0.5rem 0 0', color: 'var(--text-secondary)', fontSize: '0.78rem' }}>{b.logoHint}</p>
        </div>

        {/* Navbar title */}
        <label style={fieldLabel}>{b.navbarTitle}
          <input
            value={form.navbarTitle ?? ''}
            onChange={e => setForm({ ...form, navbarTitle: e.target.value || null })}
            placeholder={form.name}
            style={fieldInput}
          />
          <span style={{ fontWeight: 400, fontSize: '0.78rem' }}>{b.navbarTitleHint}</span>
        </label>

        {/* Colors */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.75rem' }}>
          <label style={fieldLabel}>{b.primaryColor}
            <input type="color" value={form.primaryColor || '#2563eb'} onChange={e => setForm({ ...form, primaryColor: e.target.value })} style={colorInput} />
          </label>
          <label style={fieldLabel}>{b.primaryHoverColor}
            <input type="color" value={form.primaryHoverColor || '#1d4ed8'} onChange={e => setForm({ ...form, primaryHoverColor: e.target.value })} style={colorInput} />
          </label>
          <label style={fieldLabel}>{b.accentColor}
            <input type="color" value={form.accentColor || '#f59e0b'} onChange={e => setForm({ ...form, accentColor: e.target.value })} style={colorInput} />
          </label>
        </div>

        {/* Links */}
        <label style={fieldLabel}>{b.website}
          <input value={form.websiteUrl ?? ''} onChange={e => setForm({ ...form, websiteUrl: e.target.value || null })} placeholder="https://..." style={fieldInput} />
        </label>
        <label style={fieldLabel}>{b.instagram}
          <input value={form.instagramUrl ?? ''} onChange={e => setForm({ ...form, instagramUrl: e.target.value || null })} placeholder="https://www.instagram.com/..." style={fieldInput} />
        </label>
        <label style={fieldLabel}>{b.telegram}
          <input value={form.telegramUrl ?? ''} onChange={e => setForm({ ...form, telegramUrl: e.target.value || null })} placeholder="https://t.me/..." style={fieldInput} />
        </label>

        {form.subdomain && (
          <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
            {b.subdomainHint}: <strong>{form.subdomain}.unistart.kz</strong>
          </div>
        )}

        <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: '0.25rem' }}>
          <button
            onClick={handleSave}
            disabled={saving || uploading}
            className="btn"
            style={{ padding: '0.55rem 1.3rem', background: 'var(--primary-color)', color: '#fff', border: 'none' }}
          >
            {saving ? b.saving : b.save}
          </button>
        </div>
      </div>
    </div>
  );
}

export default SchoolAdminBrandingPage;
