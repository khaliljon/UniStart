import api from './api';

export interface SupportTicketSummary {
  id: number;
  telegramUserId: number;
  username?: string | null;
  firstName?: string | null;
  status: string;
  lastMessageAt: string;
  createdAt: string;
  messageCount: number;
}

export interface SupportMessage {
  id: number;
  direction: string; // "In" | "Out"
  text: string;
  createdAt: string;
}

export interface SupportTicketDetail {
  id: number;
  telegramUserId: number;
  username?: string | null;
  firstName?: string | null;
  status: string;
  lastMessageAt: string;
  messages: SupportMessage[];
}

export const supportService = {
  async listTickets(): Promise<SupportTicketSummary[]> {
    const res = await api.get<SupportTicketSummary[]>('/admin/support/tickets');
    return res.data;
  },
  async getTicket(id: number): Promise<SupportTicketDetail> {
    const res = await api.get<SupportTicketDetail>(`/admin/support/tickets/${id}`);
    return res.data;
  },
  async reply(id: number, text: string): Promise<void> {
    await api.post(`/admin/support/tickets/${id}/reply`, { text });
  },
};
