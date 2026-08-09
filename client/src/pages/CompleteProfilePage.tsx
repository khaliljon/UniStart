import { useState, FormEvent } from 'react';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTranslation } from '../hooks/useTranslation';
import { completeProfile, logout, clearError } from '../store/slices/authSlice';
import PhoneField from '../components/PhoneField';
import { splitPhone, isValidPhone } from '../utils/countries';

function CompleteProfilePage() {
  const dispatch = useAppDispatch();
  const { isLoading, error } = useAppSelector((state) => state.auth);
  const { t } = useTranslation();

  const [phoneNumber, setPhoneNumber] = useState('');
  const [validationError, setValidationError] = useState('');

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setValidationError('');
    const { country, national } = splitPhone(phoneNumber);
    if (!isValidPhone(country, national)) {
      setValidationError(t.auth.phoneInvalid);
      return;
    }
    dispatch(completeProfile(phoneNumber));
  };

  return (
    <div className="auth-container">
      <div className="auth-card card">
        <h1 className="auth-title">{t.auth.completeProfile.title}</h1>
        <p className="auth-subtitle">{t.auth.completeProfile.subtitle}</p>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="phoneNumber" className="form-label">
              {t.auth.phone}
            </label>
            <PhoneField
              id="phoneNumber"
              value={phoneNumber}
              onChange={(v) => { setPhoneNumber(v); if (error) dispatch(clearError()); setValidationError(''); }}
            />
            <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.4rem' }}>
              {t.auth.completeProfile.hint}
            </p>
          </div>

          {(validationError || error) && (
            <p className="error-message">{validationError || error}</p>
          )}

          <button
            type="submit"
            className="btn btn-primary"
            style={{ width: '100%', marginTop: '1rem' }}
            disabled={isLoading}
          >
            {isLoading ? t.common.loading : t.auth.completeProfile.submit}
          </button>
        </form>

        <p style={{ textAlign: 'center', marginTop: '1rem', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
          <button
            onClick={() => dispatch(logout())}
            style={{
              background: 'none', border: 'none', color: 'var(--primary-color)',
              cursor: 'pointer', fontWeight: 600, fontSize: '0.85rem', padding: 0,
            }}
          >
            {t.auth.completeProfile.logout}
          </button>
        </p>
      </div>
    </div>
  );
}

export default CompleteProfilePage;
