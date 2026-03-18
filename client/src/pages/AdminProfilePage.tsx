import { useState } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTranslation } from '../hooks/useTranslation';
import { authService } from '../services/authService';

function AdminProfilePage() {
  const { user } = useAppSelector((state) => state.auth);
  const { t } = useTranslation();

  // Password change
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [pwdError, setPwdError] = useState<string | null>(null);
  const [pwdSuccess, setPwdSuccess] = useState<string | null>(null);
  const [pwdLoading, setPwdLoading] = useState(false);

  // Email change
  const [newEmail, setNewEmail] = useState('');
  const [emailPassword, setEmailPassword] = useState('');
  const [emailError, setEmailError] = useState<string | null>(null);
  const [emailSuccess, setEmailSuccess] = useState<string | null>(null);
  const [emailLoading, setEmailLoading] = useState(false);

  const handleChangePassword = async () => {
    setPwdError(null);
    setPwdSuccess(null);

    if (newPassword !== confirmPassword) {
      setPwdError(t.profilePage.passwordsDoNotMatch);
      return;
    }
    if (newPassword.length < 6) {
      setPwdError('Minimum 6 characters');
      return;
    }

    try {
      setPwdLoading(true);
      await authService.changePassword({ currentPassword, newPassword });
      setPwdSuccess(t.profilePage.passwordChanged);
      setCurrentPassword('');
      setNewPassword('');
      setConfirmPassword('');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.profilePage.passwordChangeError;
      setPwdError(msg);
    } finally {
      setPwdLoading(false);
    }
  };

  const handleChangeEmail = async () => {
    setEmailError(null);
    setEmailSuccess(null);

    if (!newEmail.trim() || !newEmail.includes('@')) {
      setEmailError('Invalid email');
      return;
    }

    try {
      setEmailLoading(true);
      await authService.changeEmail({ newEmail, password: emailPassword });
      setEmailSuccess(t.profilePage.emailChanged);
      setNewEmail('');
      setEmailPassword('');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.profilePage.emailChangeError;
      setEmailError(msg);
    } finally {
      setEmailLoading(false);
    }
  };

  return (
    <div className="animate-fade-in" style={{ maxWidth: '640px', margin: '0 auto', padding: '2rem 0' }}>
      <h1 style={{ marginBottom: '1.5rem' }}>{t.profilePage.title}</h1>

      {/* ─── User Info Card ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1rem' }}>
          <div style={{
            width: '56px', height: '56px', borderRadius: '50%',
            background: 'var(--primary-color)', color: '#fff',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: '1.25rem', fontWeight: 700, flexShrink: 0,
          }}>
            {user?.name?.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2) || '?'}
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.2rem' }}>{user?.name}</h2>
            <p style={{ margin: '0.15rem 0 0', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{user?.email}</p>
          </div>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem', fontSize: '0.85rem' }}>
          <div>
            <div style={{ color: 'var(--text-secondary)', fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.04em' }}>
              {t.profilePage.personalInfo}
            </div>
            <div style={{ fontWeight: 500 }}>Admin</div>
          </div>
          <div>
            <div style={{ color: 'var(--text-secondary)', fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.04em' }}>
              {t.profilePage.registeredAt}
            </div>
            <div style={{ fontWeight: 500 }}>
              {user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : '—'}
            </div>
          </div>
        </div>
      </div>

      {/* ─── Change Password ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 1rem', fontSize: '1rem' }}>{t.profilePage.changePassword}</h3>

        {pwdError && (
          <div style={{ color: 'var(--error-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>
            {pwdError}
          </div>
        )}
        {pwdSuccess && (
          <div style={{ color: 'var(--success-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'rgba(16,185,129,0.08)', borderRadius: '6px', fontSize: '0.85rem' }}>
            ✓ {pwdSuccess}
          </div>
        )}

        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.3rem', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-secondary)' }}>
              {t.profilePage.currentPassword}
            </label>
            <input
              type="password"
              className="form-input"
              value={currentPassword}
              onChange={e => setCurrentPassword(e.target.value)}
              style={{ width: '100%' }}
            />
          </div>
          <div>
            <label style={{ display: 'block', marginBottom: '0.3rem', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-secondary)' }}>
              {t.profilePage.newPassword}
            </label>
            <input
              type="password"
              className="form-input"
              value={newPassword}
              onChange={e => setNewPassword(e.target.value)}
              style={{ width: '100%' }}
            />
          </div>
          <div>
            <label style={{ display: 'block', marginBottom: '0.3rem', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-secondary)' }}>
              {t.profilePage.confirmNewPassword}
            </label>
            <input
              type="password"
              className="form-input"
              value={confirmPassword}
              onChange={e => setConfirmPassword(e.target.value)}
              style={{ width: '100%' }}
            />
          </div>
          <button
            className="btn btn-primary"
            onClick={handleChangePassword}
            disabled={pwdLoading || !currentPassword || !newPassword || !confirmPassword}
            style={{ alignSelf: 'flex-start', fontSize: '0.9rem' }}
          >
            {pwdLoading ? '...' : t.profilePage.changePassword}
          </button>
        </div>
      </div>

      {/* ─── Change Email ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 1rem', fontSize: '1rem' }}>{t.profilePage.newEmail}</h3>

        {emailError && (
          <div style={{ color: 'var(--error-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>
            {emailError}
          </div>
        )}
        {emailSuccess && (
          <div style={{ color: 'var(--success-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'rgba(16,185,129,0.08)', borderRadius: '6px', fontSize: '0.85rem' }}>
            ✓ {emailSuccess}
          </div>
        )}

        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.3rem', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-secondary)' }}>
              {t.profilePage.newEmail}
            </label>
            <input
              type="email"
              className="form-input"
              value={newEmail}
              onChange={e => setNewEmail(e.target.value)}
              placeholder="new@email.com"
              style={{ width: '100%' }}
            />
          </div>
          <div>
            <label style={{ display: 'block', marginBottom: '0.3rem', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-secondary)' }}>
              {t.profilePage.enterPassword}
            </label>
            <input
              type="password"
              className="form-input"
              value={emailPassword}
              onChange={e => setEmailPassword(e.target.value)}
              style={{ width: '100%' }}
            />
          </div>
          <button
            className="btn btn-primary"
            onClick={handleChangeEmail}
            disabled={emailLoading || !newEmail || !emailPassword}
            style={{ alignSelf: 'flex-start', fontSize: '0.9rem' }}
          >
            {emailLoading ? '...' : t.admin.common.save}
          </button>
        </div>
      </div>
    </div>
  );
}

export default AdminProfilePage;
