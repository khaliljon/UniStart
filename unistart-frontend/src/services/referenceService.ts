import api from './api';

export interface Country {
  id: number;
  name: string;
  nameEn?: string;
  code: string;
  flagEmoji?: string;
  isActive: boolean;
  universitiesCount: number;
  examsCount: number;
}

export interface ExamType {
  id: number;
  name: string;
  nameEn?: string;
  code: string;
  description?: string;
  defaultCountryId?: number;
  defaultCountryName?: string;
  isActive: boolean;
  examsCount: number;
}
export interface City {
  id: number;
  name: string;
  nameEn?: string;
  countryId: number;
  countryName: string;
  countryCode?: string;
  region?: string;
  population?: number;
  isActive: boolean;
  createdAt: string;
}
class ReferenceService {
  // Countries
  async getCountries(): Promise<Country[]> {
    const response = await api.get<Country[]>('/countries');
    return response.data;
  }

  async getCountry(id: number): Promise<Country> {
    const response = await api.get<Country>(`/countries/${id}`);
    return response.data;
  }

  // Exam Types
  async getExamTypes(): Promise<ExamType[]> {
    const response = await api.get<ExamType[]>('/examtypes');
    return response.data;
  }

  async getExamType(id: number): Promise<ExamType> {
    const response = await api.get<ExamType>(`/examtypes/${id}`);
    return response.data;
  }

  // Cities
  async getCities(countryId?: number): Promise<City[]> {
    const params = countryId ? { countryId } : {};
    const response = await api.get<City[]>('/cities', { params });
    return response.data;
  }

  async getCity(id: number): Promise<City> {
    const response = await api.get<City>(`/cities/${id}`);
    return response.data;
  }
}

export const referenceService = new ReferenceService();
