import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { authService } from '../../services/authService';
import { trackEvent } from '../../utils/analytics';
import type { User, LoginRequest, RegisterRequest, AuthResponse, VerifyEmailRequest, GoogleLoginRequest } from '../../types';

function getSubdomain(): string | null {
  const match = window.location.hostname.match(/^([a-z0-9-]+)\.unistart\.kz$/i);
  return match ? match[1] : null;
}

function redirectAfterAuth(response: AuthResponse): boolean {
  const hostname = window.location.hostname;
  const currentSubdomain = getSubdomain();
  const isMainDomain = hostname === 'unistart.kz' || hostname === 'www.unistart.kz';

  const buildTransfer = () => {
    const user = {
      id: response.userId, email: response.email,
      firstName: response.firstName, lastName: response.lastName, name: response.name,
      role: response.role, hasCompletedOnboarding: response.hasCompletedOnboarding,
      subscriptionTier: response.subscriptionTier || 'Free',
      subscriptionExpiresAt: response.subscriptionExpiresAt || null,
      emailVerified: response.emailVerified, phoneNumber: response.phoneNumber ?? null,
      hasFullAccess: response.hasFullAccess ?? false,
      createdAt: new Date().toISOString(),
    };
    return encodeURIComponent(JSON.stringify({ token: response.token, expiresAt: response.expiresAt, user }));
  };

  if (isMainDomain && response.schoolSubdomain) {
    window.location.href = `https://${response.schoolSubdomain}.unistart.kz?authTransfer=${buildTransfer()}`;
    return true;
  }

  if (currentSubdomain && response.role === 'Admin') {
    window.location.href = `https://unistart.kz?authTransfer=${buildTransfer()}`;
    return true;
  }

  if (currentSubdomain && !response.schoolSubdomain) {
    window.location.href = `https://unistart.kz?authTransfer=${buildTransfer()}`;
    return true;
  }

  if (currentSubdomain && response.schoolSubdomain && response.schoolSubdomain !== currentSubdomain) {
    window.location.href = `https://${response.schoolSubdomain}.unistart.kz?authTransfer=${buildTransfer()}`;
    return true;
  }

  return false;
}

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  pendingVerificationEmail: string | null;
}

const storedUser = localStorage.getItem('user');
const storedToken = localStorage.getItem('token');
const storedExpiresAt = localStorage.getItem('tokenExpiresAt');

const isTokenValid = (() => {
  if (!storedToken || !storedExpiresAt) return false;
  try {
    const expiresAt = new Date(storedExpiresAt).getTime();
    return expiresAt > Date.now();
  } catch {
    return false;
  }
})();

if (storedToken && !isTokenValid) {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  localStorage.removeItem('tokenExpiresAt');
}

const initialState: AuthState = {
  user: isTokenValid && storedUser ? JSON.parse(storedUser) : null,
  token: isTokenValid ? storedToken : null,
  isAuthenticated: isTokenValid,
  isLoading: false,
  error: null,
  pendingVerificationEmail: null,
};

export const login = createAsyncThunk(
  'auth/login',
  async (data: LoginRequest, { rejectWithValue }) => {
    try {
      const response = await authService.login(data);
      trackEvent('login', { method: 'email' });
      if (redirectAfterAuth(response)) {
        await new Promise<never>(() => {});
      }
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { error?: string } } };
      return rejectWithValue(err.response?.data?.error || 'Login failed');
    }
  }
);

export const register = createAsyncThunk(
  'auth/register',
  async (data: RegisterRequest, { rejectWithValue }) => {
    try {
      const response = await authService.register(data);
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { error?: string } } };
      return rejectWithValue(err.response?.data?.error || 'Registration failed');
    }
  }
);

export const verifyEmail = createAsyncThunk(
  'auth/verifyEmail',
  async (data: VerifyEmailRequest, { rejectWithValue }) => {
    try {
      const response = await authService.verifyEmail(data);
      trackEvent('sign_up', { method: 'email' });
      if (redirectAfterAuth(response)) {
        await new Promise<never>(() => {});
      }
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { error?: string } } };
      return rejectWithValue(err.response?.data?.error || 'Verification failed');
    }
  }
);

export const googleLogin = createAsyncThunk(
  'auth/googleLogin',
  async (data: GoogleLoginRequest, { rejectWithValue }) => {
    try {
      const response = await authService.googleLogin(data);
      trackEvent(response.isNewUser ? 'sign_up' : 'login', { method: 'google' });
      if (redirectAfterAuth(response)) {
        await new Promise<never>(() => {});
      }
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { error?: string } } };
      return rejectWithValue(err.response?.data?.error || 'Google login failed');
    }
  }
);

export const completeProfile = createAsyncThunk(
  'auth/completeProfile',
  async (phoneNumber: string, { rejectWithValue }) => {
    try {
      const response = await authService.completeProfile(phoneNumber);
      return response;
    } catch (error: unknown) {
      const err = error as { response?: { data?: { error?: string } } };
      return rejectWithValue(err.response?.data?.error || 'Failed to save phone number');
    }
  }
);

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout: (state) => {
      state.user = null;
      state.token = null;
      state.isAuthenticated = false;
      state.pendingVerificationEmail = null;
      authService.removeToken();
      localStorage.removeItem('tokenExpiresAt');
      localStorage.removeItem('user');
      try {
        window.google?.accounts.id.disableAutoSelect();
      } catch {}
    },
    clearError: (state) => {
      state.error = null;
    },
    clearPendingVerification: (state) => {
      state.pendingVerificationEmail = null;
    },
    setOnboardingComplete: (state) => {
      if (state.user) {
        state.user.hasCompletedOnboarding = true;
        localStorage.setItem('user', JSON.stringify(state.user));
      }
    },
    setSubscription: (state, action: PayloadAction<{ tier: string; expiresAt: string | null }>) => {
      if (state.user) {
        state.user.subscriptionTier = action.payload.tier;
        state.user.subscriptionExpiresAt = action.payload.expiresAt;
        localStorage.setItem('user', JSON.stringify(state.user));
      }
    },
  },
  extraReducers: (builder) => {
    const handleAuthFulfilled = (state: AuthState, action: PayloadAction<AuthResponse>) => {
      state.isLoading = false;
      state.user = {
        id: action.payload.userId,
        email: action.payload.email,
        firstName: action.payload.firstName,
        lastName: action.payload.lastName,
        name: action.payload.name,
        role: action.payload.role,
        hasCompletedOnboarding: action.payload.hasCompletedOnboarding,
        subscriptionTier: action.payload.subscriptionTier || 'Free',
        subscriptionExpiresAt: action.payload.subscriptionExpiresAt || null,
        emailVerified: action.payload.emailVerified,
        phoneNumber: action.payload.phoneNumber ?? null,
        hasFullAccess: action.payload.hasFullAccess ?? false,
        createdAt: new Date().toISOString(),
      };
      state.token = action.payload.token;
      state.isAuthenticated = true;
      state.pendingVerificationEmail = null;
      authService.saveToken(action.payload.token);
      localStorage.setItem('user', JSON.stringify(state.user));
      localStorage.setItem('tokenExpiresAt', action.payload.expiresAt);
    };

    builder
      .addCase(login.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action) => {
        handleAuthFulfilled(state, action);
      })
      .addCase(login.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(register.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(register.fulfilled, (state, action: PayloadAction<AuthResponse>) => {
        if (action.payload.emailVerified) {
          handleAuthFulfilled(state, action);
        } else {
          state.isLoading = false;
          state.pendingVerificationEmail = action.payload.email;
        }
      })
      .addCase(register.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(verifyEmail.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(verifyEmail.fulfilled, (state, action) => {
        handleAuthFulfilled(state, action);
      })
      .addCase(verifyEmail.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(googleLogin.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(googleLogin.fulfilled, (state, action) => {
        handleAuthFulfilled(state, action);
      })
      .addCase(googleLogin.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      .addCase(completeProfile.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(completeProfile.fulfilled, (state, action) => {
        handleAuthFulfilled(state, action);
      })
      .addCase(completeProfile.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { logout, clearError, clearPendingVerification, setOnboardingComplete, setSubscription } = authSlice.actions;
export default authSlice.reducer;
