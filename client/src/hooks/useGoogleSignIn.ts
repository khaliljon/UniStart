import { useEffect, useRef } from 'react';
import { useAppDispatch } from './useAppDispatch';
import { googleLogin } from '../store/slices/authSlice';
import api from '../services/api';

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: {
            client_id: string;
            callback: (response: { credential: string }) => void;
            itp_support?: boolean;
          }) => void;
          renderButton: (element: HTMLElement, config: {
            theme: string; size: string; width: number; text: string;
          }) => void;
          disableAutoSelect: () => void;
          cancel: () => void;
        };
      };
    };
    __gsi_loaded?: boolean;
  }
}

export function useGoogleSignIn(buttonId: string, text: 'continue_with' | 'signup_with', enabled = true) {
  const dispatch = useAppDispatch();
  const clientIdRef = useRef<string | null>(null);

  useEffect(() => {
    if (!enabled) return;
    let cancelled = false;

    const initGsi = () => {
      if (cancelled || !window.google || !clientIdRef.current) return;
      window.google.accounts.id.initialize({
        client_id: clientIdRef.current,
        callback: (response) => {
          dispatch(googleLogin({ idToken: response.credential }));
        },
        itp_support: true,
      });
      const btnEl = document.getElementById(buttonId);
      if (btnEl) {
        btnEl.innerHTML = '';
        window.google.accounts.id.renderButton(btnEl, {
          theme: 'outline',
          size: 'large',
          width: 360,
          text,
        });
      }
    };

    (async () => {
      try {
        const res = await api.get<{ clientId: string }>('/auth/google-client-id');
        if (cancelled || !res.data.clientId) return;
        clientIdRef.current = res.data.clientId;
      } catch {
        return;
      }

      if (window.google) {
        initGsi();
        return;
      }

      if (!window.__gsi_loaded) {
        window.__gsi_loaded = true;
        const script = document.createElement('script');
        script.src = 'https://accounts.google.com/gsi/client';
        script.async = true;
        script.defer = true;
        script.onload = () => initGsi();
        document.head.appendChild(script);
      } else {
        const check = setInterval(() => {
          if (window.google) {
            clearInterval(check);
            initGsi();
          }
        }, 100);
        setTimeout(() => clearInterval(check), 5000);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [buttonId, text, dispatch, enabled]);
}
