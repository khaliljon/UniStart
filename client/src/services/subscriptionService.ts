import api from './api';
import type { SubscriptionStatus, DailyUsage, TierLimits, UpgradeResponse } from '../types';

const MAX_CLOCK_SKEW_MS = 2 * 60 * 60 * 1000; // 2 hours

function hasClockSkew(serverTimeUtc: string): boolean {
  const serverMs = new Date(serverTimeUtc).getTime();
  if (isNaN(serverMs)) return false;
  return Math.abs(Date.now() - serverMs) > MAX_CLOCK_SKEW_MS;
}

export const subscriptionService = {
  getStatus: async (): Promise<SubscriptionStatus> => {
    const response = await api.get('/subscription/status');
    return response.data;
  },

  getDailyUsage: async (): Promise<DailyUsage> => {
    const response = await api.get('/subscription/daily-usage');
    const data: DailyUsage = response.data;
    // If the user's local clock is off by >2 hours, treat the limit as reached
    // to prevent gaming by changing the device clock
    if (hasClockSkew(data.serverTimeUtc)) {
      return { ...data, isLimitReached: true, questionsRemaining: 0 };
    }
    return data;
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
