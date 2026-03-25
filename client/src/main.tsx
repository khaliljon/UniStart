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
