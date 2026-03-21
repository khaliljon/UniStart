import { createContext, useContext, useState, useCallback } from 'react';

type ToastType = 'success' | 'error' | 'info';

interface ToastItem {
  id: number;
  message: string;
  type: ToastType;
}

interface ToastContextValue {
  showToast: (message: string, type?: ToastType) => void;
}

const ToastContext = createContext<ToastContextValue>({ showToast: () => {} });

export const useToast = () => useContext(ToastContext);

let nextId = 0;

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const showToast = useCallback((message: string, type: ToastType = 'success') => {
    const id = ++nextId;
    setToasts((prev) => [...prev, { id, message, type }]);
    setTimeout(() => setToasts((prev) => prev.filter((t) => t.id !== id)), 3000);
  }, []);

  const COLORS: Record<ToastType, { bg: string; border: string }> = {
    success: { bg: '#10b981', border: '#059669' },
    error: { bg: '#ef4444', border: '#dc2626' },
    info: { bg: '#6366f1', border: '#4f46e5' },
  };

  return (
    <ToastContext.Provider value={{ showToast }}>
      {children}
      {toasts.length > 0 && (
        <div style={{
          position: 'fixed', top: '1rem', left: '50%', transform: 'translateX(-50%)',
          zIndex: 9999, display: 'flex', flexDirection: 'column', gap: '0.5rem',
          pointerEvents: 'none', maxWidth: '90vw',
        }}>
          {toasts.map((t) => (
            <div
              key={t.id}
              style={{
                padding: '0.75rem 1.25rem', borderRadius: '10px',
                background: COLORS[t.type].bg, border: `1px solid ${COLORS[t.type].border}`,
                color: '#fff', fontSize: '0.9rem', fontWeight: 500,
                boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
                animation: 'toast-slide-in 0.3s ease-out',
                pointerEvents: 'auto', textAlign: 'center',
              }}
            >
              {t.message}
            </div>
          ))}
        </div>
      )}
    </ToastContext.Provider>
  );
}
