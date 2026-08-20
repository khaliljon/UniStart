import api from './api';


export interface QuestionImportJob {
  id: number;
  fileName: string;
  fileType: string;
  examTypeCode: string;
  sectionId: number | null;
  topicId: number | null;
  status: string;
  createdAt: string;
  completedAt: string | null;
  totalExtracted: number;
  totalApproved: number;
  totalRejected: number;
  errorMessage: string | null;
  instructions: string | null;
  files: ImportJobFile[] | null;
  contentType: string;
  resultSummary: string | null;
}

export interface ImportJobFile {
  id: number;
  fileName: string;
  fileType: string;
  role: string;
}

export type FileRole = 'Questions' | 'Answers' | 'Mixed';

export interface BatchFileEntry {
  file: File;
  role: FileRole;
}

export interface DraftOption {
  text: string;
  isCorrect: boolean;
}

export interface ImportedQuestionDraft {
  id: number;
  importJobId: number;
  questionText: string;
  options: DraftOption[];
  explanation: string | null;
  hint: string | null;
  topicId: number | null;
  topicName: string | null;
  difficulty: string;
  irtA: number;
  irtB: number;
  irtC: number;
  status: string;
  source: string;
  createdAt: string;
  reviewedAt: string | null;
  imageUrl: string | null;
}

export interface UpdateDraftPayload {
  questionText?: string;
  options?: DraftOption[];
  explanation?: string;
  hint?: string;
  topicId?: number;
  difficulty?: string;
  irtA?: number;
  irtB?: number;
  irtC?: number;
  imageUrl?: string;
}


export const questionImportService = {
  async upload(file: File, examTypeCode: string, sectionId?: number, topicId?: number, instructions?: string, contentType?: 'questions' | 'theory', mode?: 'ai' | 'strict'): Promise<QuestionImportJob> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('examTypeCode', examTypeCode);
    if (sectionId != null) formData.append('sectionId', String(sectionId));
    if (topicId != null) formData.append('topicId', String(topicId));
    if (instructions) formData.append('instructions', instructions);
    if (contentType) formData.append('contentType', contentType);
    if (mode) formData.append('mode', mode);
    const resp = await api.post<QuestionImportJob>('/admin/question-import/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      timeout: 600000,
    });
    return resp.data;
  },

  async uploadBatch(
    entries: BatchFileEntry[],
    examTypeCode: string,
    sectionId?: number,
    topicId?: number,
    instructions?: string,
  ): Promise<QuestionImportJob> {
    const formData = new FormData();
    for (const entry of entries) {
      formData.append('files', entry.file);
    }
    formData.append('examTypeCode', examTypeCode);
    formData.append('fileRoles', entries.map(e => e.role).join(','));
    if (sectionId != null) formData.append('sectionId', String(sectionId));
    if (topicId != null) formData.append('topicId', String(topicId));
    if (instructions) formData.append('instructions', instructions);
    const resp = await api.post<QuestionImportJob>('/admin/question-import/upload-batch', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      timeout: 600000,
    });
    return resp.data;
  },

  async getJobs(): Promise<QuestionImportJob[]> {
    const resp = await api.get<QuestionImportJob[]>('/admin/question-import/jobs');
    return resp.data;
  },

  async getJob(jobId: number): Promise<QuestionImportJob> {
    const resp = await api.get<QuestionImportJob>(`/admin/question-import/jobs/${jobId}`);
    return resp.data;
  },

  async getDrafts(jobId: number, status?: string): Promise<ImportedQuestionDraft[]> {
    const resp = await api.get<ImportedQuestionDraft[]>(`/admin/question-import/jobs/${jobId}/drafts`, {
      params: status ? { status } : undefined,
    });
    return resp.data;
  },

  async getDraft(draftId: number): Promise<ImportedQuestionDraft> {
    const resp = await api.get<ImportedQuestionDraft>(`/admin/question-import/drafts/${draftId}`);
    return resp.data;
  },

  async updateDraft(draftId: number, payload: UpdateDraftPayload): Promise<ImportedQuestionDraft> {
    const resp = await api.put<ImportedQuestionDraft>(`/admin/question-import/drafts/${draftId}`, payload);
    return resp.data;
  },

  async approveDraft(draftId: number): Promise<void> {
    await api.put(`/admin/question-import/drafts/${draftId}/approve`);
  },

  async rejectDraft(draftId: number): Promise<void> {
    await api.put(`/admin/question-import/drafts/${draftId}/reject`);
  },

  async approveAll(jobId: number): Promise<{ approved: number }> {
    const resp = await api.post<{ approved: number }>(`/admin/question-import/jobs/${jobId}/approve-all`);
    return resp.data;
  },

  async deleteJob(jobId: number): Promise<void> {
    await api.delete(`/admin/question-import/jobs/${jobId}`);
  },

  async deleteAllJobs(): Promise<{ deleted: number }> {
    const resp = await api.delete<{ deleted: number }>('/admin/question-import/jobs');
    return resp.data;
  },
};
