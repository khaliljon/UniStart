import api from './api';

export interface MockSectionInput {
  id?: number;
  examSectionId: number | null;
  name: string;
  timeLimitMinutes: number;
  questionCount: number;
  sortOrder: number;
  instructions: string | null;
}

export interface MockExamListItem {
  id: number;
  examTypeCode: string;
  title: string;
  description: string;
  titleKz?: string | null;
  titleEn?: string | null;
  descriptionKz?: string | null;
  descriptionEn?: string | null;
  totalTimeMinutes: number;
  isActive: boolean;
  sectionCount: number;
  questionCount: number;
  attemptCount: number;
}

export interface MockExamDetail {
  id: number;
  examTypeCode: string;
  title: string;
  description: string;
  titleKz?: string | null;
  titleEn?: string | null;
  descriptionKz?: string | null;
  descriptionEn?: string | null;
  totalTimeMinutes: number;
  isActive: boolean;
  sections: Array<{
    id: number;
    examSectionId: number | null;
    name: string;
    timeLimitMinutes: number;
    questionCount: number;
    sortOrder: number;
    instructions: string | null;
  }>;
}

export interface SaveMockExam {
  examTypeCode: string;
  title: string;
  description: string;
  titleKz?: string | null;
  titleEn?: string | null;
  descriptionKz?: string | null;
  descriptionEn?: string | null;
  totalTimeMinutes: number;
  isActive: boolean;
  sections: MockSectionInput[];
}

export interface ExamTypeOption {
  code: string;
  name: string;
}

export interface ExamSectionOption {
  id: number;
  name: string;
  availableQuestions?: number;
}

const mockAdminService = {
  getExamTypes: () =>
    api.get<ExamTypeOption[]>('/school-admin/mocks/exam-types').then((r) => r.data),

  getExamSections: (code: string) =>
    api.get<ExamSectionOption[]>(`/school-admin/mocks/exam-types/${encodeURIComponent(code)}/sections`).then((r) => r.data),

  list: () =>
    api.get<MockExamListItem[]>('/school-admin/mocks').then((r) => r.data),

  get: (id: number) =>
    api.get<MockExamDetail>(`/school-admin/mocks/${id}`).then((r) => r.data),

  create: (data: SaveMockExam) =>
    api.post<MockExamDetail>('/school-admin/mocks', data).then((r) => r.data),

  update: (id: number, data: SaveMockExam) =>
    api.put<MockExamDetail>(`/school-admin/mocks/${id}`, data).then((r) => r.data),

  toggleActive: (id: number, isActive: boolean) =>
    api.put<{ id: number; isActive: boolean }>(`/school-admin/mocks/${id}/active`, { isActive }).then((r) => r.data),

  remove: (id: number) =>
    api.delete(`/school-admin/mocks/${id}`).then((r) => r.data),
};

export default mockAdminService;
