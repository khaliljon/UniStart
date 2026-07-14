import api from './api';
import type { CheckoutLine } from './mockCatalogService';

export const paymentsService = {
  /** Create a Polar checkout and return the hosted checkout URL. */
  createCheckout: (lines: CheckoutLine[]): Promise<{ url: string }> =>
    api.post<{ url: string }>('/payments/checkout', { lines }).then((r) => r.data),
};
