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

export interface AdminSales {
  count: number;
  totalRevenue: number;
  currency: string;
  items: AdminPurchase[];
  totalNet: number;
}

export const purchaseService = {
  async list(): Promise<Purchase[]> {
    const res = await api.get<Purchase[]>('/purchases');
    return res.data;
  },

  async adminList(params?: { status?: string; itemType?: string; from?: string; to?: string }): Promise<AdminSales> {
    const res = await api.get<AdminSales>('/purchases/admin/all', { params });
    return res.data;
  },

  async adminExportCsv(params?: { status?: string; itemType?: string; from?: string; to?: string }): Promise<Blob> {
    const res = await api.get('/purchases/admin/export.csv', { params, responseType: 'blob' });
    return res.data as Blob;
  },
};
