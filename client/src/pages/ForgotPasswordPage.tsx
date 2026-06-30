import { useState, FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from '../hooks/useTranslation';
import { authService } from '../services/authService';

type Step = 'email' | 'code' | 'newPassword';

function ForgotPasswordPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const fp = t.auth.fp;

  const [step, setStep] = useState<Step>('email');
  const [email, setEmail] = useState('');
  const [code, setCode] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleSendCode = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);
    try {
      await authService.forgotPassword(email);
      setStep('code');
      setSuccess(fp.codeSent);
    } catch (err: any) {
      setError(err?.response?.data?.error ?? fp.sendError);
    } finally {
      setIsLoading(false);
    }
  };

  const handleVerifyCode = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    if (code.length !== 6) {
      setError(fp.invalidCode);
      return;
    }
    setStep('newPassword');
    setSuccess('');
  };

  const handleResetPassword = async (e: FormEvent) => {
    e.preventDefault();
    setError('');

    if (newPassword.length < 8) {
      setError(fp.passwordTooShort);
      return;
    }
    if (newPassword !== confirmPassword) {
      setError(fp.passwordMismatch);
      return;
    }

    setIsLoading(true);
    try {
      await authService.resetPassword({ email, code, newPassword });
      setSuccess(fp.resetSuccess);
      setTimeout(() => navigate('/login'), 2000);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error;
      setError(msg || fp.resetError);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card card">
        <h1 className="auth-title">{fp.title}</h1>
        <p className="auth-subtitle">
          {step === 'email' && fp.enterEmail}
          {step === 'code' && fp.enterCode}
          {step === 'newPassword' && fp.enterNewPassword}
        </p>

        {error && <p className="error-message">{error}</p>}
        {success && <p style={{ color: 'var(--success-color)', textAlign: 'center', marginBottom: '1rem', fontSize: '0.9rem' }}>{success}</p>}

        {step === 'email' && (
          <form onSubmit={handleSendCode}>
            <div className="form-group">
              <label htmlFor="email" className="form-label">{t.auth.email}</label>
              <input
                type="email"
                id="email"
                className="form-input"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="Enter your email"
                required
              />
            </div>
            <button
              type="submit"
              className="btn btn-primary"
              style={{ width: '100%', marginTop: '1rem' }}
              disabled={isLoading}
            >
              {isLoading ? t.common.loading : fp.sendCode}
            </button>
          </form>
        )}

        {step === 'code' && (
          <form onSubmit={handleVerifyCode}>
            <div className="form-group">
              <label htmlFor="code" className="form-label">{t.auth.verificationCode}</label>
              <input
                type="text"
                id="code"
                className="form-input"
                value={code}
                onChange={(e) => setCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                placeholder="000000"
                maxLength={6}
                style={{ textAlign: 'center', fontSize: '1.5rem', letterSpacing: '0.5rem' }}
                required
              />
            </div>
            <button
              type="submit"
              className="btn btn-primary"
              style={{ width: '100%', marginTop: '1rem' }}
              disabled={code.length !== 6}
            >
              {fp.verifyCode}
            </button>
            <button
              type="button"
              onClick={handleSendCode}
              style={{
                width: '100%', marginTop: '0.75rem', background: 'none', border: 'none',
                color: 'var(--primary-color)', cursor: 'pointer', fontSize: '0.9rem',
              }}
              disabled={isLoading}
            >
              {t.auth.resendCode}
            </button>
          </form>
        )}

        {step === 'newPassword' && (
          <form onSubmit={handleResetPassword}>
            <div className="form-group">
              <label htmlFor="newPassword" className="form-label">{fp.newPassword}</label>
              <div style={{ position: 'relative' }}>
                <input
                  type={showPassword ? 'text' : 'password'}
                  id="newPassword"
                  className="form-input"
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  placeholder="Min 8 characters"
                  required
                  minLength={8}
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
              <label htmlFor="confirmPassword" className="form-label">{t.auth.confirmPassword}</label>
              <input
                type={showPassword ? 'text' : 'password'}
                id="confirmPassword"
                className="form-input"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
                minLength={8}
              />
            </div>
            <button
              type="submit"
              className="btn btn-primary"
              style={{ width: '100%', marginTop: '1rem' }}
              disabled={isLoading}
            >
              {isLoading ? t.common.loading : fp.resetButton}
            </button>
          </form>
        )}

        <p style={{ textAlign: 'center', marginTop: '1.5rem', color: 'var(--text-secondary)' }}>
          <Link to="/login" style={{ color: 'var(--primary-color)' }}>
            ← {t.auth.login}
          </Link>
        </p>
      </div>
    </div>
  );
}

export default ForgotPasswordPage;
