import api from './api';

export interface KaspiPendingOrder {
  orderCode: string;
  userId: number;
  userName: string;
  email: string;
  amount: number;
  currency: string;
  status: string;
  externalPaymentId?: string | null;
  createdAt: string;
  paidAt?: string | null;
}

export interface KaspiConfirmPayload {
  kaspiPaymentId: string;
  paidAmount: number;
  note?: string;
}

export const adminPaymentsService = {
  async listKaspiPending(): Promise<KaspiPendingOrder[]> {
    const res = await api.get<KaspiPendingOrder[]>('/admin/payments/kaspi', { params: { status: 'Pending' } });
    return res.data;
  },

  async confirmKaspi(orderCode: string, payload: KaspiConfirmPayload): Promise<void> {
    await api.post(`/admin/payments/kaspi/${encodeURIComponent(orderCode)}/confirm`, payload);
  },
};
