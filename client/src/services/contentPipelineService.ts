import api from './api';

// ─── Normalized ingestion payload (mirrors backend ContentIngestionDtos) ──────

export interface IngestOption {
  text: string;
  isCorrect: boolean;
}

export interface IngestQuestion {
  text: string;
  options: IngestOption[];
  explanation?: string | null;
  hint?: string | null;
  difficulty?: string;
  sortOrder?: number;
}

export interface IngestFormula {
  title: string;
  formula: string;
  description?: string | null;
  sortOrder?: number;
}

export interface IngestTopic {
  name: string;
  sortOrder: number;
  lessonContent?: string | null;
  formulas?: IngestFormula[] | null;
  questions?: IngestQuestion[] | null;
}

export interface IngestContent {
  examTypeCode: string;
  examSectionName: string;
  skillName: string;
  topics: IngestTopic[];
}

export interface IngestResult {
  skillId: number;
  skillName: string;
  skillCreated: boolean;
  topicsCreated: number;
  topicsUpdated: number;
  lessonsCreated: number;
  lessonsUpdated: number;
  formulasCreated: number;
  questionsCreated: number;
  questionsSkippedDuplicate: number;
  warnings: string[];
}

export interface DriveSyncItem {
  id: number;
  driveFileId: string;
  name: string;
  mimeType: string;
  folderPath: string | null;
  status: string;
  mappedSkillName: string | null;
  mappedSkillId: number | null;
  errorMessage: string | null;
  driveModifiedAt: string | null;
  lastSyncedAt: string | null;
}

export interface ParseInput {
  examTypeCode: string;
  examSectionName: string;
  text?: string;
  file?: File;
}

function buildForm(input: ParseInput): FormData {
  const form = new FormData();
  form.append('ExamTypeCode', input.examTypeCode);
  form.append('ExamSectionName', input.examSectionName);
  if (input.text) form.append('Text', input.text);
  if (input.file) form.append('File', input.file);
  return form;
}

export const contentPipelineService = {
  // Step 1 — LLM-parse a study pack into a normalized payload (no DB write).
  async parse(input: ParseInput): Promise<IngestContent> {
    const resp = await api.post<IngestContent>('/admin/content/parse', buildForm(input), {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return resp.data;
  },

  // Step 2 — commit a (possibly reviewed) payload into the DB.
  async ingest(payload: IngestContent): Promise<IngestResult> {
    const resp = await api.post<IngestResult>('/admin/content/ingest', payload);
    return resp.data;
  },

  // One-shot — parse + ingest in a single call.
  async parseAndIngest(input: ParseInput): Promise<IngestResult> {
    const resp = await api.post<IngestResult>('/admin/content/parse-and-ingest', buildForm(input), {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return resp.data;
  },

  // Enqueue a background Google Drive folder sync (Hangfire).
  async startDriveSync(rootFolderId: string, examTypeCode: string, examSectionName: string): Promise<{ jobId: string; message: string }> {
    const resp = await api.post<{ jobId: string; message: string }>('/admin/content/drive/sync', {
      rootFolderId,
      examTypeCode,
      examSectionName,
    });
    return resp.data;
  },

  // Reconciliation report of every tracked Drive file.
  async getDriveItems(): Promise<DriveSyncItem[]> {
    const resp = await api.get<DriveSyncItem[]>('/admin/content/drive/items');
    return resp.data;
  },
};

export default contentPipelineService;
