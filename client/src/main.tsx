import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from 'react-redux'
import { BrowserRouter } from 'react-router-dom'
import { store } from './store'
import { I18nProvider } from './i18n'
import App from './App'
import ErrorBoundary from './components/ErrorBoundary'
import './index.css'

const _atParam = new URLSearchParams(window.location.search).get('authTransfer');
if (_atParam) {
  try {
    const data = JSON.parse(_atParam);
    if (data.token && data.expiresAt && data.user) {
      const _sub = window.location.hostname.match(/^([a-z0-9-]+)\.unistart\.kz$/i);
      if (_sub && data.user.role === 'Admin') {
        window.location.href = `https://unistart.kz?authTransfer=${encodeURIComponent(_atParam)}`;
      } else {
        localStorage.setItem('token', data.token);
        localStorage.setItem('tokenExpiresAt', data.expiresAt);
        localStorage.setItem('user', JSON.stringify(data.user));
      }
    }
  } catch {}
  window.history.replaceState({}, '', window.location.pathname);
}

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
  } catch {}
})();

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ErrorBoundary>
      <I18nProvider>
        <Provider store={store}>
          <BrowserRouter>
            <App />
          </BrowserRouter>
        </Provider>
      </I18nProvider>
    </ErrorBoundary>
  </StrictMode>,
)
