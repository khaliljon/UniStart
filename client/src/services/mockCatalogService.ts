import api from './api';

export interface MockTier {
  id: number;
  runs: number;
  price: number;
  currency: string;
}

export interface MockTemplate {
  mockExamId: number;
  title: string;
  examTypeCode: string;
  totalQuestions: number;
  totalTimeMinutes: number;
  runsRemaining: number;
  tiers: MockTier[];
}

export interface MockPackage {
  id: number;
  key: string;
  name: string;
  pickCount: number; // 0 = all subjects
  runsEach: number;
  price: number;
  currency: string;
  sortOrder: number;
}

export interface MockCatalog {
  freeRunAvailable: boolean;
  templates: MockTemplate[];
  packages: MockPackage[];
}

/** A single checkout line (run purchase or package). */
export interface CheckoutLine {
  kind: 'mock' | 'package';
  mockExamId?: number;
  runs?: number;
  packageKey?: string;
  selectedMockIds?: number[];
}

export interface CheckoutQuote {
  total: number;
  currency: string;
}

// ── Admin catalog shapes ─────────────────────────────────

export interface AdminTier {
  id: number;
  mockExamId: number;
  runs: number;
  price: number;
  currency: string;
  isActive: boolean;
}

export interface SaveTier {
  mockExamId: number;
  runs: number;
  price: number;
  currency: string;
  isActive: boolean;
}

export interface AdminPackage {
  id: number;
  key: string;
  name: string;
  pickCount: number;
  runsEach: number;
  price: number;
  currency: string;
  sortOrder: number;
  isActive: boolean;
}

export type SavePackage = Omit<AdminPackage, 'id'>;

export const mockCatalogService = {
  getCatalog: (): Promise<MockCatalog> => api.get<MockCatalog>('/mock-catalog').then((r) => r.data),

  quote: (lines: CheckoutLine[]): Promise<CheckoutQuote> =>
    api.post<CheckoutQuote>('/mock-catalog/quote', { lines }).then((r) => r.data),

  checkout: (lines: CheckoutLine[]): Promise<CheckoutQuote> =>
    api.post<CheckoutQuote>('/mock-catalog/checkout', { lines }).then((r) => r.data),

  // ── Admin ──────────────────────────────────────────────
  adminTiers: (): Promise<AdminTier[]> =>
    api.get<AdminTier[]>('/mock-catalog/admin/tiers').then((r) => r.data),
  createTier: (dto: SaveTier): Promise<{ id: number }> =>
    api.post('/mock-catalog/admin/tiers', dto).then((r) => r.data),
  updateTier: (id: number, dto: SaveTier): Promise<{ id: number }> =>
    api.put(`/mock-catalog/admin/tiers/${id}`, dto).then((r) => r.data),
  deleteTier: (id: number): Promise<void> =>
    api.delete(`/mock-catalog/admin/tiers/${id}`).then(() => undefined),

  adminPackages: (): Promise<AdminPackage[]> =>
    api.get<AdminPackage[]>('/mock-catalog/admin/packages').then((r) => r.data),
  createPackage: (dto: SavePackage): Promise<{ id: number }> =>
    api.post('/mock-catalog/admin/packages', dto).then((r) => r.data),
  updatePackage: (id: number, dto: SavePackage): Promise<{ id: number }> =>
    api.put(`/mock-catalog/admin/packages/${id}`, dto).then((r) => r.data),
  deletePackage: (id: number): Promise<void> =>
    api.delete(`/mock-catalog/admin/packages/${id}`).then(() => undefined),
};
