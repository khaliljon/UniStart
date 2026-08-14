import api from './api';

export interface CartItem {
  itemType: string;
  itemCode: string;
  title: string;
  subjects?: string | null;
  amount: number;
  currency: string;
  runs?: number;
  selectedMockIds?: number[];
}

const KEY = 'cart';
const EVENT = 'cart-changed';

function read(): CartItem[] {
  try {
    const raw = localStorage.getItem(KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? (parsed as CartItem[]) : [];
  } catch {
    return [];
  }
}

function write(items: CartItem[]): void {
  localStorage.setItem(KEY, JSON.stringify(items));
  window.dispatchEvent(new Event(EVENT));
}

export const cartService = {
  eventName: EVENT,

  list(): CartItem[] {
    return read();
  },

  count(): number {
    return read().length;
  },

  has(itemType: string, itemCode: string): boolean {
    return read().some((i) => i.itemType === itemType && i.itemCode === itemCode);
  },

  add(item: CartItem): void {
    const items = read().filter((i) => !(i.itemType === item.itemType && i.itemCode === item.itemCode));
    items.push(item);
    write(items);
    api.post('/cart', item).catch(() => {});
  },

  remove(itemType: string, itemCode: string): void {
    write(read().filter((i) => !(i.itemType === itemType && i.itemCode === itemCode)));
    api.delete('/cart/item', { params: { itemType, itemCode } }).catch(() => {});
  },

  clear(): void {
    write([]);
    api.delete('/cart').catch(() => {});
  },

  async sync(): Promise<void> {
    try {
      const res = await api.get<CartItem[]>('/cart');
      if (Array.isArray(res.data)) write(res.data);
    } catch {
    }
  },
};
