import { useState, FormEvent, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTranslation } from '../hooks/useTranslation';
import { login, googleLogin, clearError } from '../store/slices/authSlice';
import api from '../services/api';

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: { client_id: string; callback: (response: { credential: string }) => void }) => void;
          renderButton: (element: HTMLElement, config: { theme: string; size: string; width: number; text: string }) => void;
        };
      };
    };
  }
}

function LoginPage() {
  const dispatch = useAppDispatch();
  const { isLoading, error } = useAppSelector((state) => state.auth);
  const { t } = useTranslation();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  useEffect(() => {
    let script: HTMLScriptElement | null = null;
    (async () => {
      // Fetch Google Client ID from backend
      try {
        const res = await api.get<{ clientId: string }>('/auth/google-client-id');
        const clientId = res.data.clientId;
        if (!clientId) return;

        script = document.createElement('script');
        script.src = 'https://accounts.google.com/gsi/client';
        script.async = true;
        script.defer = true;
        script.onload = () => {
          if (window.google) {
            window.google.accounts.id.initialize({
              client_id: clientId,
              callback: (response) => {
                dispatch(googleLogin({ idToken: response.credential }));
              },
            });
            const btnEl = document.getElementById('google-login-btn');
            if (btnEl) {
              window.google.accounts.id.renderButton(btnEl, {
                theme: 'outline',
                size: 'large',
                width: 360,
                text: 'continue_with',
              });
            }
          }
        };
        document.head.appendChild(script);
      } catch { /* Google sign-in not configured */ }
    })();
    return () => { if (script) document.head.removeChild(script); };
  }, [dispatch]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    dispatch(login({ email, password }));
  };

  const handleInputChange = () => {
    if (error) {
      dispatch(clearError());
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card card">
        <h1 className="auth-title">{t.auth.loginTitle}</h1>
        <p className="auth-subtitle">{t.auth.loginSubtitle}</p>

        <form onSubmit={handleSubmit}>
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
                placeholder="Enter your password"
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

          {error && <p className="error-message">{error}</p>}

          <button
            type="submit"
            className="btn btn-primary"
            style={{ width: '100%', marginTop: '1rem' }}
            disabled={isLoading}
          >
            {isLoading ? t.common.loading : t.auth.login}
          </button>
        </form>

        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', margin: '1.5rem 0' }}>
          <div style={{ flex: 1, height: '1px', background: 'var(--border-color)' }} />
          <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{t.auth.orContinueWith}</span>
          <div style={{ flex: 1, height: '1px', background: 'var(--border-color)' }} />
        </div>

        <div id="google-login-btn" style={{ display: 'flex', justifyContent: 'center' }}>
          <button
            type="button"
            onClick={() => {
              if (window.google) return; // Google SDK will handle it
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
          {t.auth.noAccount}{' '}
          <Link to="/register" style={{ color: 'var(--primary-color)' }}>
            {t.auth.register}
          </Link>
        </p>
      </div>
    </div>
  );
}

export default LoginPage;
