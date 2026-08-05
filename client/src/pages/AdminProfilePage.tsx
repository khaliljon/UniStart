import { useState } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useTranslation } from '../hooks/useTranslation';
import { authService } from '../services/authService';
import api from '../services/api';
import { completeProfile } from '../store/slices/authSlice';

function AdminProfilePage() {
  const { user } = useAppSelector((state) => state.auth);
  const dispatch = useAppDispatch();
  const { t } = useTranslation();

  // Password change
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [pwdError, setPwdError] = useState<string | null>(null);
  const [pwdSuccess, setPwdSuccess] = useState<string | null>(null);
  const [pwdLoading, setPwdLoading] = useState(false);

  // Phone change
  const [editPhone, setEditPhone] = useState(user?.phoneNumber || '+7');
  const [phoneError, setPhoneError] = useState<string | null>(null);
  const [phoneSuccess, setPhoneSuccess] = useState<string | null>(null);
  const [phoneLoading, setPhoneLoading] = useState(false);

  // Name change
  const [editFirstName, setEditFirstName] = useState(user?.firstName || '');
  const [editLastName, setEditLastName] = useState(user?.lastName || '');
  const [nameError, setNameError] = useState<string | null>(null);
  const [nameSuccess, setNameSuccess] = useState<string | null>(null);
  const [nameLoading, setNameLoading] = useState(false);

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

  const handleChangePhone = async () => {
    setPhoneError(null);
    setPhoneSuccess(null);
    if (!/^\+77\d{9}$/.test(editPhone)) { setPhoneError(t.auth.phoneInvalid); return; }
    try {
      setPhoneLoading(true);
      await dispatch(completeProfile(editPhone)).unwrap();
      setPhoneSuccess(t.common.save + ' ✓');
    } catch (err: unknown) {
      setPhoneError(typeof err === 'string' ? err : t.auth.phoneInvalid);
    } finally {
      setPhoneLoading(false);
    }
  };

  const handleChangeName = async () => {
    setNameError(null);
    setNameSuccess(null);
    if (editFirstName.trim().length < 2 || /\d/.test(editFirstName)) { setNameError(t.auth.firstName); return; }
    if (editLastName.trim().length < 2 || /\d/.test(editLastName)) { setNameError(t.auth.lastName); return; }
    try {
      setNameLoading(true);
      await api.put(`/users/${user?.id}`, { firstName: editFirstName.trim(), lastName: editLastName.trim() });
      setNameSuccess(t.common.save + ' ✓');
      if (user) {
        const updated = { ...user, firstName: editFirstName.trim(), lastName: editLastName.trim(), name: `${editFirstName.trim()} ${editLastName.trim()}`.trim() };
        localStorage.setItem('user', JSON.stringify(updated));
      }
    } catch (err: unknown) {
      setNameError((err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.profilePage.passwordChangeError);
    } finally {
      setNameLoading(false);
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

      {/* ─── Edit Name ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 1rem', fontSize: '1rem' }}>{t.auth.firstName} / {t.auth.lastName}</h3>
        {nameError && (
          <div style={{ color: 'var(--error-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>{nameError}</div>
        )}
        {nameSuccess && (
          <div style={{ color: 'var(--success-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'rgba(16,185,129,0.08)', borderRadius: '6px', fontSize: '0.85rem' }}>✓ {nameSuccess}</div>
        )}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.3rem', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-secondary)' }}>{t.auth.firstName}</label>
            <input type="text" className="form-input" value={editFirstName} onChange={e => setEditFirstName(e.target.value)} style={{ width: '100%' }} />
          </div>
          <div>
            <label style={{ display: 'block', marginBottom: '0.3rem', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-secondary)' }}>{t.auth.lastName}</label>
            <input type="text" className="form-input" value={editLastName} onChange={e => setEditLastName(e.target.value)} style={{ width: '100%' }} />
          </div>
          <button
            className="btn btn-primary"
            onClick={handleChangeName}
            disabled={nameLoading || !editFirstName.trim() || !editLastName.trim()}
            style={{ alignSelf: 'flex-start', fontSize: '0.9rem' }}
          >
            {nameLoading ? '...' : t.common.save}
          </button>
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

      {/* ─── Edit Phone ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 1rem', fontSize: '1rem' }}>{t.auth.phone}</h3>

        {phoneError && (
          <div style={{ color: 'var(--error-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>
            {phoneError}
          </div>
        )}
        {phoneSuccess && (
          <div style={{ color: 'var(--success-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'rgba(16,185,129,0.08)', borderRadius: '6px', fontSize: '0.85rem' }}>
            ✓ {phoneSuccess}
          </div>
        )}

        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <div>
            <label style={{ display: 'block', marginBottom: '0.3rem', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-secondary)' }}>
              {t.auth.phone}
            </label>
            <input
              type="tel"
              className="form-input"
              value={editPhone}
              onChange={e => {
                let digits = e.target.value.replace(/\D/g, '');
                if (digits.startsWith('8')) digits = '7' + digits.slice(1);
                if (!digits.startsWith('7')) digits = '7' + digits;
                setEditPhone('+' + digits.slice(0, 11));
              }}
              placeholder="+7 700 123 45 67"
              maxLength={12}
              style={{ width: '100%' }}
            />
          </div>
          <button
            className="btn btn-primary"
            onClick={handleChangePhone}
            disabled={phoneLoading || !editPhone}
            style={{ alignSelf: 'flex-start', fontSize: '0.9rem' }}
          >
            {phoneLoading ? '...' : t.common.save}
          </button>
        </div>
      </div>
    </div>
  );
}

export default AdminProfilePage;
