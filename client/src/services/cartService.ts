// Client-side shopping cart (localStorage). Items live here BEFORE payment.
// After checkout they become server-side Purchase records ("Мои покупки").

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
  },

  remove(itemType: string, itemCode: string): void {
    write(read().filter((i) => !(i.itemType === itemType && i.itemCode === itemCode)));
  },

  clear(): void {
    write([]);
  },
};
