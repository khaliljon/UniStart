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
      localStorage.setItem('token', data.token);
      localStorage.setItem('tokenExpiresAt', data.expiresAt);
      localStorage.setItem('user', JSON.stringify(data.user));
    }
  } catch { /* ignore malformed */ }
  window.history.replaceState({}, '', window.location.pathname);
}

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
