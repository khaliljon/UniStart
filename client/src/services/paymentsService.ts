import api from './api';
import type { CheckoutLine } from './mockCatalogService';

export const paymentsService = {
  createCheckout: (lines: CheckoutLine[]): Promise<{ url: string }> =>
    api.post<{ url: string }>('/payments/checkout', { lines }).then((r) => r.data),
};
