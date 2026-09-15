import api from './api';

export interface Purchase {
  id: number;
  itemType: string;
  itemCode: string;
  title: string;
  subjects?: string | null;
  amount: number;
  currency: string;
  status: string;
  purchasedAt: string;
  polarOrderId?: string | null;
  checkoutRef?: string | null;
  paymentProvider?: string | null;
  externalPaymentId?: string | null;
  orderCode?: string | null;
}

export interface AdminPurchase extends Purchase {
  userId: number;
  userName: string;
  userEmail: string;
  grossAmount: number;
  taxAmount: number;
  platformFeeAmount: number;
  platformFeeCurrency?: string | null;
  netAmount: number;
  totalAmount: number;
}

export interface SalesFee {
  provider: string;
  currency: string;
  amount: number;
}

export interface AdminSales {
  totalRevenue: number;
  currency: string;
  paidOrders: number;
  fees: SalesFee[];
  items: AdminPurchase[];
}

export type SalesFilters = { status?: string; itemType?: string; provider?: string; from?: string; to?: string };

export const purchaseService = {
  async list(): Promise<Purchase[]> {
    const res = await api.get<Purchase[]>('/purchases');
    return res.data;
  },

  async adminList(params?: SalesFilters): Promise<AdminSales> {
    const res = await api.get<AdminSales>('/purchases/admin/all', { params });
    return res.data;
  },

  async adminExportCsv(params?: SalesFilters): Promise<Blob> {
    const res = await api.get('/purchases/admin/export.csv', { params, responseType: 'blob' });
    return res.data as Blob;
  },
};
