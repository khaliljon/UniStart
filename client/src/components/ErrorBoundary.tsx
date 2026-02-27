import { Component, type ErrorInfo, type ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('[ErrorBoundary] Uncaught error:', error, info.componentStack);
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError) {
      return (
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '60vh',
          padding: '2rem',
          textAlign: 'center',
        }}>
          <h1 style={{ fontSize: '1.5rem', marginBottom: '1rem', color: 'var(--color-text, #1a1a1a)' }}>
            Что-то пошло не так
          </h1>
          <p style={{ marginBottom: '1.5rem', color: 'var(--color-text-secondary, #666)', maxWidth: '400px' }}>
            Произошла непредвиденная ошибка. Попробуйте обновить страницу или вернуться на главную.
          </p>
          <div style={{ display: 'flex', gap: '0.75rem' }}>
            <button
              onClick={() => window.location.reload()}
              style={{
                padding: '0.5rem 1.25rem',
                borderRadius: '8px',
                border: 'none',
                background: 'var(--color-primary, #6366f1)',
                color: '#fff',
                cursor: 'pointer',
                fontSize: '0.95rem',
              }}
            >
              Обновить страницу
            </button>
            <button
              onClick={() => { this.handleReset(); window.location.href = '/'; }}
              style={{
                padding: '0.5rem 1.25rem',
                borderRadius: '8px',
                border: '1px solid var(--color-border, #e2e8f0)',
                background: 'transparent',
                color: 'var(--color-text, #1a1a1a)',
                cursor: 'pointer',
                fontSize: '0.95rem',
              }}
            >
              На главную
            </button>
          </div>
          {process.env.NODE_ENV === 'development' && this.state.error && (
            <pre style={{
              marginTop: '2rem',
              padding: '1rem',
              background: 'var(--color-surface, #f8fafc)',
              borderRadius: '8px',
              fontSize: '0.75rem',
              maxWidth: '600px',
              overflow: 'auto',
              textAlign: 'left',
              color: '#e53e3e',
            }}>
              {this.state.error.message}
              {'\n'}
              {this.state.error.stack}
            </pre>
          )}
        </div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
