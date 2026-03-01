import api from './api';
import type { Conversation, MessagesPage } from '../types';

export const messageService = {
  async getConversations(): Promise<Conversation[]> {
    const response = await api.get<Conversation[]>('/messages/conversations');
    return response.data;
  },

  async getMessages(conversationId: number, page = 1, pageSize = 50): Promise<MessagesPage> {
    const response = await api.get<MessagesPage>(
      `/messages/conversations/${conversationId}`,
      { params: { page, pageSize } }
    );
    return response.data;
  },

  async startConversation(tutorId: number, message?: string): Promise<Conversation> {
    const response = await api.post<Conversation>('/messages/conversations', { tutorId, message });
    return response.data;
  },

  async sendMessage(conversationId: number, text: string): Promise<void> {
    await api.post(`/messages/conversations/${conversationId}/messages`, { text });
  },

  async markAsRead(conversationId: number): Promise<void> {
    await api.post(`/messages/conversations/${conversationId}/read`);
  },

  async getUnreadCount(): Promise<number> {
    const response = await api.get<number>('/messages/unread-count');
    return response.data;
  },

  async archiveConversation(conversationId: number): Promise<void> {
    await api.post(`/messages/conversations/${conversationId}/archive`);
  },
};
