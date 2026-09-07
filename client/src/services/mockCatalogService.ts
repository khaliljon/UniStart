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
  runsByLanguage?: Record<string, number> | null;
  tiers: MockTier[];
  titleKz?: string | null;
  titleEn?: string | null;
  description?: string | null;
  descriptionKz?: string | null;
  descriptionEn?: string | null;
}

export interface MockPackage {
  id: number;
  key: string;
  name: string;
  nameKz?: string | null;
  nameEn?: string | null;
  pickCount: number;
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

export interface CheckoutLine {
  kind: 'mock' | 'package' | 'book';
  mockExamId?: number;
  runs?: number;
  packageKey?: string;
  selectedMockIds?: number[];
  bookMaterialId?: number;
  language?: string;
}

export interface CheckoutQuote {
  total: number;
  currency: string;
}


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
  nameKz?: string | null;
  nameEn?: string | null;
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
