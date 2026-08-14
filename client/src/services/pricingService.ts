import api from './api';

export interface Pricing {
  mockPrice: number;
  materialPrice: number;
  currency: string;
}

export interface UpdatePricing {
  mockPrice: number;
  materialPrice: number;
  currency: string;
}

export const pricingService = {
  get: (): Promise<Pricing> => api.get<Pricing>('/pricing').then((r) => r.data),

  update: (dto: UpdatePricing): Promise<Pricing> =>
    api.put<Pricing>('/admin/pricing', dto).then((r) => r.data),
};
