import api from './api';

export interface PresenceInfo {
  isOnline: boolean;
  lastSeenAt: string | null;
}

export interface BatchPresence {
  userId: number;
  isOnline: boolean;
}

export const userService = {
  async getPresence(userId: number): Promise<PresenceInfo> {
    const response = await api.get<PresenceInfo>(`/users/${userId}/presence`);
    return response.data;
  },

  async getPresenceBatch(userIds: number[]): Promise<BatchPresence[]> {
    const response = await api.post<BatchPresence[]>('/users/presence/batch', userIds);
    return response.data;
  },
};
