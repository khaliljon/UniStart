import api from './api';
import type {
  DiagnosticSession,
  DiagnosticQuestion,
  DiagnosticAnswerResult,
  DiagnosticResult,
} from '../types';

export const diagnosticService = {
  async start(examTypeCode: string): Promise<DiagnosticSession> {
    const response = await api.post<DiagnosticSession>('/diagnostic/start', { examTypeCode });
    return response.data;
  },

  async getCurrentQuestion(sessionId: number): Promise<DiagnosticQuestion | null> {
    try {
      const response = await api.get<DiagnosticQuestion>(`/diagnostic/${sessionId}/current`);
      return response.data;
    } catch (err: any) {
      if (err.response?.status === 404) return null;
      throw err;
    }
  },

  async submitAnswer(
    sessionId: number,
    questionId: number,
    answerOptionId: number,
    timeSpentSeconds?: number
  ): Promise<DiagnosticAnswerResult> {
    const response = await api.post<DiagnosticAnswerResult>('/diagnostic/answer', {
      sessionId,
      questionId,
      answerOptionId,
      timeSpentSeconds,
    });
    return response.data;
  },

  async getResults(sessionId: number): Promise<DiagnosticResult> {
    const response = await api.get<DiagnosticResult>(`/diagnostic/${sessionId}/results`);
    return response.data;
  },
};
