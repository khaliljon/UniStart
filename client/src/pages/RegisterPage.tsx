import { useState, FormEvent } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTranslation } from '../hooks/useTranslation';
import { useGoogleSignIn } from '../hooks/useGoogleSignIn';
import { register, verifyEmail, clearError } from '../store/slices/authSlice';
import { authService } from '../services/authService';

function RegisterPage() {
  const dispatch = useAppDispatch();
  const { isLoading, error, pendingVerificationEmail } = useAppSelector((state) => state.auth);
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();

  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('+7');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const role = 'Student' as const;
  const [acceptedTerms, setAcceptedTerms] = useState(false);
  const [validationError, setValidationError] = useState('');
  const [code, setCode] = useState('');
  const [resending, setResending] = useState(false);
  const [resendMsg, setResendMsg] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  useGoogleSignIn('google-register-btn', 'signup_with');

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setValidationError('');

    if (!acceptedTerms) {
      setValidationError(t.legal.consentRequired);
      return;
    }

    if (password !== confirmPassword) {
      setValidationError('Passwords do not match');
      return;
    }

    if (firstName.trim().length < 2) {
      setValidationError('First name must be at least 2 characters');
      return;
    }
    if (/\d/.test(firstName)) {
      setValidationError('First name must not contain digits');
      return;
    }
    if (lastName.trim().length < 2) {
      setValidationError('Last name must be at least 2 characters');
      return;
    }
    if (/\d/.test(lastName)) {
      setValidationError('Last name must not contain digits');
      return;
    }

    // Basic email domain check (server does full DNS validation)
    const emailDomain = email.split('@')[1];
    if (!emailDomain || !emailDomain.includes('.') || emailDomain.endsWith('.')) {
      setValidationError('Please enter a valid email address with an existing domain');
      return;
    }

    if (!/^\+77\d{9}$/.test(phoneNumber)) {
      setValidationError(t.auth.phoneInvalid);
      return;
    }

    if (password.length < 10) {
      setValidationError('Password must be at least 10 characters');
      return;
    }
    if (!/[A-Z]/.test(password)) {
      setValidationError('Password must contain an uppercase letter');
      return;
    }
    if (!/[a-z]/.test(password)) {
      setValidationError('Password must contain a lowercase letter');
      return;
    }
    if (!/[0-9]/.test(password)) {
      setValidationError('Password must contain a digit');
      return;
    }
    if (!/[^A-Za-z0-9]/.test(password)) {
      setValidationError('Password must contain a special character');
      return;
    }

    dispatch(register({ firstName: firstName.trim(), lastName: lastName.trim(), email, password, phoneNumber, role, referralCode: searchParams.get('ref') || undefined }));
  };

  const handleVerify = async (e: FormEvent) => {
    e.preventDefault();
    if (!pendingVerificationEmail) return;
    dispatch(verifyEmail({ email: pendingVerificationEmail, code }));
  };

  const handleResend = async () => {
    if (!pendingVerificationEmail || resending) return;
    setResending(true);
    setResendMsg('');
    try {
      await authService.resendCode({ email: pendingVerificationEmail });
      setResendMsg(t.auth.codeSent);
    } catch {
      setResendMsg(t.auth.codeResendError);
    } finally {
      setResending(false);
    }
  };

  const handleInputChange = () => {
    if (error) {
      dispatch(clearError());
    }
    setValidationError('');
  };

  // Verification code screen
  if (pendingVerificationEmail) {
    return (
      <div className="auth-container">
        <div className="auth-card card">
          <h1 className="auth-title">{t.auth.verifyTitle}</h1>
          <p className="auth-subtitle">
            {t.auth.verifySubtitle} <strong>{pendingVerificationEmail}</strong>
          </p>

          <form onSubmit={handleVerify}>
            <div className="form-group">
              <label htmlFor="code" className="form-label">
                {t.auth.verificationCode}
              </label>
              <input
                type="text"
                id="code"
                className="form-input"
                value={code}
                onChange={(e) => {
                  const val = e.target.value.replace(/\D/g, '').slice(0, 6);
                  setCode(val);
                  if (error) dispatch(clearError());
                }}
                placeholder="000000"
                maxLength={6}
                style={{ textAlign: 'center', fontSize: '1.5rem', letterSpacing: '0.5rem' }}
                required
                autoFocus
              />
            </div>

            {error && <p className="error-message">{error}</p>}
            {resendMsg && (
              <p style={{ color: 'var(--success-color)', fontSize: '0.85rem', textAlign: 'center' }}>
                {resendMsg}
              </p>
            )}

            <button
              type="submit"
              className="btn btn-primary"
              style={{ width: '100%', marginTop: '1rem' }}
              disabled={isLoading || code.length !== 6}
            >
              {isLoading ? t.common.loading : t.auth.verify}
            </button>
          </form>

          <p style={{ textAlign: 'center', marginTop: '1rem', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
            {t.auth.noCodeReceived}{' '}
            <button
              onClick={handleResend}
              disabled={resending}
              style={{
                background: 'none', border: 'none', color: 'var(--primary-color)',
                cursor: 'pointer', fontWeight: 600, fontSize: '0.85rem', padding: 0,
              }}
            >
              {resending ? t.common.loading : t.auth.resendCode}
            </button>
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-container">
      <div className="auth-card card">
        <h1 className="auth-title">{t.auth.registerTitle}</h1>
        <p className="auth-subtitle">{t.auth.registerSubtitle}</p>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="firstName" className="form-label">
              {t.auth.firstName}
            </label>
            <input
              type="text"
              id="firstName"
              className="form-input"
              value={firstName}
              onChange={(e) => {
                setFirstName(e.target.value);
                handleInputChange();
              }}
              placeholder={t.auth.firstName}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="lastName" className="form-label">
              {t.auth.lastName}
            </label>
            <input
              type="text"
              id="lastName"
              className="form-input"
              value={lastName}
              onChange={(e) => {
                setLastName(e.target.value);
                handleInputChange();
              }}
              placeholder={t.auth.lastName}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="email" className="form-label">
              {t.auth.email}
            </label>
            <input
              type="email"
              id="email"
              className="form-input"
              value={email}
              onChange={(e) => {
                setEmail(e.target.value);
                handleInputChange();
              }}
              placeholder="Enter your email"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="phoneNumber" className="form-label">
              {t.auth.phone}
            </label>
            <input
              type="tel"
              id="phoneNumber"
              className="form-input"
              value={phoneNumber}
              onChange={(e) => {
                let digits = e.target.value.replace(/\D/g, '');
                if (digits.startsWith('8')) digits = '7' + digits.slice(1);
                if (!digits.startsWith('7')) digits = '7' + digits;
                setPhoneNumber('+' + digits.slice(0, 11));
                handleInputChange();
              }}
              placeholder="+7 700 123 45 67"
              maxLength={12}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="password" className="form-label">
              {t.auth.password}
            </label>
            <div style={{ position: 'relative' }}>
              <input
                type={showPassword ? 'text' : 'password'}
                id="password"
                className="form-input"
                value={password}
                onChange={(e) => {
                  setPassword(e.target.value);
                  handleInputChange();
                }}
                placeholder="Create a password"
                required
                style={{ paddingRight: '2.5rem' }}
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                style={{
                  position: 'absolute', right: '0.75rem', top: '50%', transform: 'translateY(-50%)',
                  background: 'none', border: 'none', cursor: 'pointer', padding: 0,
                  color: 'var(--text-secondary)', fontSize: '1.1rem', lineHeight: 1,
                }}
                tabIndex={-1}
              >
                {showPassword ? '\u25C9' : '\u25CE'}
              </button>
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword" className="form-label">
              {t.auth.confirmPassword}
            </label>
            <div style={{ position: 'relative' }}>
              <input
                type={showConfirmPassword ? 'text' : 'password'}
                id="confirmPassword"
                className="form-input"
                value={confirmPassword}
                onChange={(e) => {
                  setConfirmPassword(e.target.value);
                  handleInputChange();
                }}
                placeholder="Confirm your password"
                required
                style={{ paddingRight: '2.5rem' }}
              />
              <button
                type="button"
                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                style={{
                  position: 'absolute', right: '0.75rem', top: '50%', transform: 'translateY(-50%)',
                  background: 'none', border: 'none', cursor: 'pointer', padding: 0,
                  color: 'var(--text-secondary)', fontSize: '1.1rem', lineHeight: 1,
                }}
                tabIndex={-1}
              >
                {showConfirmPassword ? '\u25C9' : '\u25CE'}
              </button>
            </div>
          </div>

          <div style={{
            display: 'flex',
            alignItems: 'flex-start',
            gap: '0.5rem',
            marginTop: '0.5rem',
          }}>
            <input
              type="checkbox"
              id="acceptTerms"
              checked={acceptedTerms}
              onChange={(e) => {
                setAcceptedTerms(e.target.checked);
                handleInputChange();
              }}
              style={{ marginTop: '0.25rem', flexShrink: 0 }}
            />
            <label htmlFor="acceptTerms" style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', lineHeight: 1.4 }}>
              {t.legal.consentText}{' '}
              (<Link to="/terms" style={{ color: 'var(--primary-color)' }} target="_blank">{t.legal.termsTitle}</Link>
              {', '}
              <Link to="/privacy" style={{ color: 'var(--primary-color)' }} target="_blank">{t.legal.privacyTitle}</Link>)
            </label>
          </div>

          {(error || validationError) && (
            <p className="error-message">{validationError || error}</p>
          )}

          <button
            type="submit"
            className="btn btn-primary"
            style={{ width: '100%', marginTop: '1rem' }}
            disabled={isLoading}
          >
            {isLoading ? t.common.loading : t.auth.register}
          </button>
        </form>

        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', margin: '1.5rem 0' }}>
          <div style={{ flex: 1, height: '1px', background: 'var(--border-color)' }} />
          <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{t.auth.orContinueWith}</span>
          <div style={{ flex: 1, height: '1px', background: 'var(--border-color)' }} />
        </div>

        <div id="google-register-btn" style={{ display: 'flex', justifyContent: 'center' }}>
          <button
            type="button"
            onClick={() => {
              if (window.google) return;
              alert('Google Sign-In is not configured yet');
            }}
            style={{
              display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '0.75rem',
              width: '100%', maxWidth: '360px', padding: '0.7rem 1rem',
              border: '1px solid var(--border-color)', borderRadius: '0.5rem',
              background: 'var(--card-bg, #fff)', cursor: 'pointer',
              fontSize: '0.9rem', color: 'var(--text-primary)', fontWeight: 500,
            }}
          >
            <svg width="18" height="18" viewBox="0 0 48 48"><path fill="#EA4335" d="M24 9.5c3.54 0 6.71 1.22 9.21 3.6l6.85-6.85C35.9 2.38 30.47 0 24 0 14.62 0 6.51 5.38 2.56 13.22l7.98 6.19C12.43 13.72 17.74 9.5 24 9.5z"/><path fill="#4285F4" d="M46.98 24.55c0-1.57-.15-3.09-.38-4.55H24v9.02h12.94c-.58 2.96-2.26 5.48-4.78 7.18l7.73 6c4.51-4.18 7.09-10.36 7.09-17.65z"/><path fill="#FBBC05" d="M10.53 28.59a14.5 14.5 0 0 1 0-9.18l-7.98-6.19a24.1 24.1 0 0 0 0 21.56l7.98-6.19z"/><path fill="#34A853" d="M24 48c6.48 0 11.93-2.13 15.89-5.81l-7.73-6c-2.15 1.45-4.92 2.3-8.16 2.3-6.26 0-11.57-4.22-13.47-9.91l-7.98 6.19C6.51 42.62 14.62 48 24 48z"/></svg>
            Google
          </button>
        </div>

        <p style={{ textAlign: 'center', marginTop: '1.5rem', color: 'var(--text-secondary)' }}>
          {t.auth.hasAccount}{' '}
          <Link to="/login" style={{ color: 'var(--primary-color)' }}>
            {t.auth.login}
          </Link>
        </p>
      </div>
    </div>
  );
}

export default RegisterPage;
