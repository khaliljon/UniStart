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

export interface CheckoutRequest {
  itemType: string;
  itemCode: string;
  title: string;
  subjects?: string | null;
  amount: number;
  currency?: string;
}

export const purchaseService = {
  async list(): Promise<Purchase[]> {
    const res = await api.get<Purchase[]>('/purchases');
    return res.data;
  },

  async checkout(dto: CheckoutRequest): Promise<Purchase> {
    const res = await api.post<Purchase>('/purchases/checkout', dto);
    return res.data;
  },
};
