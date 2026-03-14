import axios from 'axios';

const API_BASE_URL = '/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  // Serialize arrays as 'key=value1&key=value2' for ASP.NET Core
  paramsSerializer: {
    serialize: (params) => {
      const parts: string[] = [];
      for (const key in params) {
        const value = params[key];
        if (Array.isArray(value)) {
          value.forEach((v) => parts.push(`${encodeURIComponent(key)}=${encodeURIComponent(v)}`));
        } else if (value !== undefined && value !== null) {
          parts.push(`${encodeURIComponent(key)}=${encodeURIComponent(value)}`);
        }
      }
      return parts.join('&');
    },
  },
});

// Track refresh state to avoid concurrent refreshes
let isRefreshing = false;
let refreshPromise: Promise<string | null> | null = null;

async function refreshToken(): Promise<string | null> {
  try {
    const response = await axios.post<{
      token: string;
      expiresAt: string;
    }>(`${API_BASE_URL}/auth/refresh`, null, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('token')}`,
      },
    });
    const { token, expiresAt } = response.data;
    localStorage.setItem('token', token);
    localStorage.setItem('tokenExpiresAt', expiresAt);
    // Also update user data if returned
    const fullResp = response.data as Record<string, unknown>;
    if (fullResp.userId) {
      const user = {
        id: fullResp.userId,
        email: fullResp.email,
        name: fullResp.name,
        role: fullResp.role,
        hasCompletedOnboarding: fullResp.hasCompletedOnboarding,
        subscriptionTier: fullResp.subscriptionTier || 'Free',
        subscriptionExpiresAt: fullResp.subscriptionExpiresAt || null,
        createdAt: localStorage.getItem('user')
          ? JSON.parse(localStorage.getItem('user')!).createdAt
          : new Date().toISOString(),
      };
      localStorage.setItem('user', JSON.stringify(user));
    }
    return token;
  } catch {
    return null;
  }
}

// Add auth token to requests + proactive refresh if about to expire
api.interceptors.request.use(async (config) => {
  // Set Accept-Language from saved locale
  const locale = localStorage.getItem('unistart_locale') || 'ru';
  config.headers['Accept-Language'] = locale;

  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;

    // Proactively refresh if token expires within 30 minutes
    // (skip for the refresh endpoint itself to avoid loops)
    const expiresAt = localStorage.getItem('tokenExpiresAt');
    if (expiresAt && !config.url?.includes('/auth/refresh')) {
      const expiresTime = new Date(expiresAt).getTime();
      const thirtyMinutes = 30 * 60 * 1000;
      if (expiresTime - Date.now() < thirtyMinutes && expiresTime > Date.now()) {
        if (!isRefreshing) {
          isRefreshing = true;
          refreshPromise = refreshToken().finally(() => {
            isRefreshing = false;
            refreshPromise = null;
          });
        }
        if (refreshPromise) {
          const newToken = await refreshPromise;
          if (newToken) {
            config.headers.Authorization = `Bearer ${newToken}`;
          }
        }
      }
    }
  }
  return config;
});

// Handle auth errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      localStorage.removeItem('tokenExpiresAt');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
