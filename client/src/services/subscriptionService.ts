import api from './api';
import type { SubscriptionStatus, DailyUsage, TierLimits, UpgradeResponse } from '../types';

export const subscriptionService = {
  getStatus: async (): Promise<SubscriptionStatus> => {
    const response = await api.get('/subscription/status');
    return response.data;
  },

  getDailyUsage: async (): Promise<DailyUsage> => {
    const response = await api.get('/subscription/daily-usage');
    return response.data;
  },

  checkAccess: async (feature: string): Promise<{ feature: string; hasAccess: boolean }> => {
    const response = await api.get(`/subscription/access/${feature}`);
    return response.data;
  },

  upgrade: async (plan: string): Promise<UpgradeResponse> => {
    const response = await api.post('/subscription/upgrade', { plan });
    return response.data;
  },

  getPlans: async (): Promise<{ free: TierLimits; pro: TierLimits }> => {
    const response = await api.get('/subscription/plans');
    return response.data;
  },
};
