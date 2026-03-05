import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { authService } from '../../services/authService';
import type { User, LoginRequest, RegisterRequest, AuthResponse } from '../../types';

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

const storedUser = localStorage.getItem('user');
const storedToken = localStorage.getItem('token');
const storedExpiresAt = localStorage.getItem('tokenExpiresAt');

// Check if the stored token is expired
const isTokenValid = (() => {
  if (!storedToken || !storedExpiresAt) return false;
  try {
    const expiresAt = new Date(storedExpiresAt).getTime();
    return expiresAt > Date.now();
  } catch {
    return false;
  }
})();

// Clear stale auth data if token expired
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
};

export const login = createAsyncThunk(
  'auth/login',
  async (data: LoginRequest, { rejectWithValue }) => {
    try {
      const response = await authService.login(data);
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

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout: (state) => {
      state.user = null;
      state.token = null;
      state.isAuthenticated = false;
      authService.removeToken();
      localStorage.removeItem('tokenExpiresAt');
    },
    clearError: (state) => {
      state.error = null;
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
    builder
      // Login
      .addCase(login.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action: PayloadAction<AuthResponse>) => {
        state.isLoading = false;
        state.user = {
          id: action.payload.userId,
          email: action.payload.email,
          name: action.payload.name,
          role: action.payload.role,
          hasCompletedOnboarding: action.payload.hasCompletedOnboarding,
          subscriptionTier: action.payload.subscriptionTier || 'Free',
          subscriptionExpiresAt: action.payload.subscriptionExpiresAt || null,
          createdAt: new Date().toISOString(),
        };
        state.token = action.payload.token;
        state.isAuthenticated = true;
        authService.saveToken(action.payload.token);
        localStorage.setItem('user', JSON.stringify(state.user));
        localStorage.setItem('tokenExpiresAt', action.payload.expiresAt);
      })
      .addCase(login.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Register
      .addCase(register.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(register.fulfilled, (state, action: PayloadAction<AuthResponse>) => {
        state.isLoading = false;
        state.user = {
          id: action.payload.userId,
          email: action.payload.email,
          name: action.payload.name,
          role: action.payload.role,
          hasCompletedOnboarding: action.payload.hasCompletedOnboarding,
          subscriptionTier: action.payload.subscriptionTier || 'Free',
          subscriptionExpiresAt: action.payload.subscriptionExpiresAt || null,
          createdAt: new Date().toISOString(),
        };
        state.token = action.payload.token;
        state.isAuthenticated = true;
        authService.saveToken(action.payload.token);
        localStorage.setItem('user', JSON.stringify(state.user));
        localStorage.setItem('tokenExpiresAt', action.payload.expiresAt);
      })
      .addCase(register.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { logout, clearError, setOnboardingComplete, setSubscription } = authSlice.actions;
export default authSlice.reducer;
