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

export interface KaspiNotification {
  id: number;
  status: string;
  orderCode?: string | null;
  kaspiPaymentId?: string | null;
  amount?: number | null;
  currency: string;
  paidAt?: string | null;
  receivedAt: string;
  errorMessage?: string | null;
  userId?: number | null;
  userName?: string | null;
  userEmail?: string | null;
  expectedAmount?: number | null;
  orderFound: boolean;
  amountMatches: boolean;
  paymentIdUnique: boolean;
}

export const adminPaymentsService = {
  async listKaspiPending(): Promise<KaspiPendingOrder[]> {
    const res = await api.get<KaspiPendingOrder[]>('/admin/payments/kaspi', { params: { status: 'Pending' } });
    return res.data;
  },

  async confirmKaspi(orderCode: string, payload: KaspiConfirmPayload): Promise<void> {
    await api.post(`/admin/payments/kaspi/${encodeURIComponent(orderCode)}/confirm`, payload);
  },

  async listKaspiNotifications(status?: string): Promise<KaspiNotification[]> {
    const res = await api.get<KaspiNotification[]>('/admin/payments/kaspi/notifications', { params: status ? { status } : undefined });
    return res.data;
  },

  async confirmKaspiNotification(id: number): Promise<void> {
    await api.post(`/admin/payments/kaspi/notifications/${id}/confirm`);
  },

  async rejectKaspiNotification(id: number): Promise<void> {
    await api.post(`/admin/payments/kaspi/notifications/${id}/reject`);
  },

  async notifyUserOfMismatch(id: number): Promise<void> {
    await api.post(`/admin/payments/kaspi/notifications/${id}/notify-user`);
  },

  async markRefunded(id: number): Promise<void> {
    await api.post(`/admin/payments/kaspi/notifications/${id}/mark-refunded`);
  },
};
