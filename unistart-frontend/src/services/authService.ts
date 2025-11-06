import api from './api';
import { LoginDto, RegisterDto, AuthResponse, User } from '../types';

export const authService = {
  async login(credentials: LoginDto): Promise<AuthResponse> {
    const { data } = await api.post<AuthResponse>('/auth/login', credentials);
    return data;
  },

  async register(userData: RegisterDto): Promise<AuthResponse> {
    const { data } = await api.post<AuthResponse>('/auth/register', userData);
    return data;
  },

  async getProfile(): Promise<User> {
    const { data } = await api.get<User>('/auth/profile');
    return data;
  },

  async getRoles(): Promise<{ id: string; email: string; roles: string[] }> {
    const { data } = await api.get('/profile/roles');
    return data;
  },

  async updateProfile(updates: Partial<User>): Promise<void> {
    await api.put('/auth/profile', updates);
  },
};
