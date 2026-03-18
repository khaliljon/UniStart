import api from './api';
import type { AuthResponse, LoginRequest, RegisterRequest, VerifyEmailRequest, ResendCodeRequest, GoogleLoginRequest } from '../types';

export const authService = {
  async login(data: LoginRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/login', data);
    return response.data;
  },

  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/register', data);
    return response.data;
  },

  async verifyEmail(data: VerifyEmailRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/verify-email', data);
    return response.data;
  },

  async resendCode(data: ResendCodeRequest): Promise<void> {
    await api.post('/auth/resend-code', data);
  },

  async googleLogin(data: GoogleLoginRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/google', data);
    return response.data;
  },

  saveToken(token: string): void {
    localStorage.setItem('token', token);
  },

  getToken(): string | null {
    return localStorage.getItem('token');
  },

  removeToken(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  isAuthenticated(): boolean {
    return !!this.getToken();
  },

  async changePassword(data: { currentPassword: string; newPassword: string }): Promise<void> {
    await api.post('/auth/change-password', data);
  },

  async changeEmail(data: { newEmail: string; password: string }): Promise<void> {
    await api.post('/auth/change-email', data);
  },
};
