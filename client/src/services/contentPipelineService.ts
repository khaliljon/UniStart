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

export interface AdminSkillSummary {
  id: number;
  code: string;
  name: string;
  topicCount: number;
  questionCount: number;
  hasStudentActivity: boolean;
}

// ─── Dry-run preview plan (no LLM, no DB writes) ──────────────────────────────

export interface DrivePlanFile {
  driveFileId: string;
  name: string;
  folderPath: string | null;
  mappedSkillName: string | null;
  fileOrder: number;
  ingestible: boolean;
  changed: boolean;
  matchedRule: string | null;
}

export interface DrivePlanTsaPair {
  questionsName: string;
  answersName: string | null;
  changed: boolean;
}

export interface DriveSyncPlan {
  totalFiles: number;
  ingestibleCount: number;
  changedCount: number;
  unitNames: string[];
  normal: DrivePlanFile[];
  tsaPairs: DrivePlanTsaPair[];
  warnings: string[];
}

// ─── Content mapping rules (folder→skill config) ──────────────────────────────

export interface ContentMappingRule {
  id: number;
  examSectionName: string | null;
  matchType: string; // 'FolderSegment' | 'FileName'
  pattern: string;
  skillName: string;
  glossary: string | null;
  sortOrder: number;
  isActive: boolean;
}

export type ContentMappingRuleInput = Omit<ContentMappingRule, 'id'>;

export interface TsaClassification {
  id: number;
  skillName: string;
  topicName: string | null;
  examSectionName: string | null;
  questionPreview: string | null;
  createdAt: string;
  lastSeenAt: string;
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

  // List all skills with content counts (for cleanup of garbage skills).
  async getSkills(): Promise<AdminSkillSummary[]> {
    const resp = await api.get<AdminSkillSummary[]>('/admin/content/skills');
    return resp.data;
  },

  // Delete a skill and all its content. Pass force=true to delete despite student activity.
  async deleteSkill(id: number, force = false): Promise<{ deleted: boolean; skillId: number; skillName: string }> {
    const resp = await api.delete<{ deleted: boolean; skillId: number; skillName: string }>(
      `/admin/content/skills/${id}`, { params: { force } });
    return resp.data;
  },

  // Dry-run: preview how a Drive folder WOULD be mapped (no LLM, no DB writes).
  async previewDriveSync(rootFolderId: string, examTypeCode: string, examSectionName: string): Promise<DriveSyncPlan> {
    const resp = await api.post<DriveSyncPlan>('/admin/content/drive/preview', {
      rootFolderId,
      examTypeCode,
      examSectionName,
    });
    return resp.data;
  },

  // List folder→skill mapping rules.
  async getMappings(): Promise<ContentMappingRule[]> {
    const resp = await api.get<ContentMappingRule[]>('/admin/content/mappings');
    return resp.data;
  },

  async createMapping(input: ContentMappingRuleInput): Promise<ContentMappingRule> {
    const resp = await api.post<ContentMappingRule>('/admin/content/mappings', input);
    return resp.data;
  },

  async updateMapping(id: number, input: ContentMappingRuleInput): Promise<ContentMappingRule> {
    const resp = await api.put<ContentMappingRule>(`/admin/content/mappings/${id}`, input);
    return resp.data;
  },

  async deleteMapping(id: number): Promise<{ deleted: boolean; id: number }> {
    const resp = await api.delete<{ deleted: boolean; id: number }>(`/admin/content/mappings/${id}`);
    return resp.data;
  },

  // Read-only audit of cached per-question TSA classifications.
  async getTsaClassifications(skillName?: string): Promise<TsaClassification[]> {
    const resp = await api.get<TsaClassification[]>('/admin/content/tsa-classifications', {
      params: skillName ? { skillName } : undefined,
    });
    return resp.data;
  },
};

export default contentPipelineService;
