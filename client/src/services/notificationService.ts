import api from './api';
import { NotificationPreferences, UpdateNotificationPreferences } from '../types';

export const notificationService = {
  getPreferences: () =>
    api.get<NotificationPreferences>('/notification/preferences').then(r => r.data),

  updatePreferences: (data: UpdateNotificationPreferences) =>
    api.put<NotificationPreferences>('/notification/preferences', data).then(r => r.data),
};
