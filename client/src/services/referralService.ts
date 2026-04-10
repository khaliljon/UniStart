import api from './api';

export interface ReferralStats {
  code: string | null;
  isActive: boolean;
  totalReferred: number;
  totalPaid: number;
  totalEarned: number;
  availableBalance: number;
  bonusDays: number;
  rewardType: 'money' | 'days';
}

export interface ReferralActivateResponse {
  code: string;
  message: string;
}

export interface ReferralUsageItem {
  id: number;
  userName: string;
  registeredAt: string;
  paidAt: string | null;
  rewardGranted: boolean;
}

export const referralService = {
  activate: () => api.post<ReferralActivateResponse>('/referral/activate').then(r => r.data),
  getStats: () => api.get<ReferralStats>('/referral/stats').then(r => r.data),
  getReferrals: () => api.get<ReferralUsageItem[]>('/referral/referrals').then(r => r.data),
};
