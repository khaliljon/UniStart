// Cart with a localStorage cache for instant reads, synced to a server-side cart
// so it follows the user across devices. Server calls are best-effort (a guest
// gets a 401 we ignore and fall back to local only).
import api from './api';

export interface CartItem {
  /** "mock" | "book" | "package". */
  itemType: string;
  /** Stable code: mock id, subject key, or package key. */
  itemCode: string;
  title: string;
  subjects?: string | null;
  amount: number;
  currency: string;
  /** Run-based fields (mock/package purchases). */
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

  /** Add or replace an item (same type + code is replaced, e.g. a different run tier). */
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

  /** Pull the server cart (cross-device) and replace the local cache. */
  async sync(): Promise<void> {
    try {
      const res = await api.get<CartItem[]>('/cart');
      if (Array.isArray(res.data)) write(res.data);
    } catch {
      /* guest / offline — keep local cache */
    }
  },
};
