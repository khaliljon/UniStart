import api from './api';
import type { CheckoutLine } from './mockCatalogService';

export interface PolarCheckoutResponse {
  provider: 'polar';
  orderCode: string;
  url: string;
}

export interface KaspiCheckoutResponse {
  provider: 'kaspi';
  orderCode: string;
  amount: number;
  currency: string;
  paymentUrl: string;
}

export type CheckoutProvider = 'polar' | 'kaspi';
export type CheckoutResponse = PolarCheckoutResponse | KaspiCheckoutResponse;

export const paymentsService = {
  createCheckout: (lines: CheckoutLine[], provider: CheckoutProvider = 'polar'): Promise<CheckoutResponse> =>
    api.post<CheckoutResponse>('/payments/checkout', { lines, provider }).then((r) => r.data),
};
