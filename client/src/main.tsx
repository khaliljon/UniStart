import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from 'react-redux'
import { BrowserRouter } from 'react-router-dom'
import { store } from './store'
import { I18nProvider } from './i18n'
import { BrandingProvider } from './contexts/BrandingContext'
import App from './App'
import ErrorBoundary from './components/ErrorBoundary'
import './index.css'

// Handle auth transfer from main domain to school subdomain
const _atParam = new URLSearchParams(window.location.search).get('authTransfer');
if (_atParam) {
  try {
    const data = JSON.parse(_atParam);
    if (data.token && data.expiresAt && data.user) {
      // Admin must NEVER stay on a subdomain — redirect to main domain immediately
      const _sub = window.location.hostname.match(/^([a-z0-9-]+)\.unistart\.kz$/i);
      if (_sub && data.user.role === 'Admin') {
        window.location.href = `https://unistart.kz?authTransfer=${encodeURIComponent(_atParam)}`;
      } else {
        localStorage.setItem('token', data.token);
        localStorage.setItem('tokenExpiresAt', data.expiresAt);
        localStorage.setItem('user', JSON.stringify(data.user));
      }
    }
  } catch { /* ignore malformed */ }
  window.history.replaceState({}, '', window.location.pathname);
}

// Guard: if Admin is already authenticated on a subdomain, redirect to main
(() => {
  const _sub = window.location.hostname.match(/^([a-z0-9-]+)\.unistart\.kz$/i);
  if (!_sub) return;
  try {
    const u = JSON.parse(localStorage.getItem('user') || 'null');
    const tk = localStorage.getItem('token');
    const exp = localStorage.getItem('tokenExpiresAt');
    if (u?.role === 'Admin' && tk && exp && new Date(exp).getTime() > Date.now()) {
      const transfer = encodeURIComponent(JSON.stringify({ token: tk, expiresAt: exp, user: u }));
      window.location.href = `https://unistart.kz?authTransfer=${transfer}`;
    }
  } catch { /* ignore */ }
})();

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ErrorBoundary>
      <I18nProvider>
        <Provider store={store}>
          <BrowserRouter>
            <BrandingProvider>
              <App />
            </BrandingProvider>
          </BrowserRouter>
        </Provider>
      </I18nProvider>
    </ErrorBoundary>
  </StrictMode>,
)
