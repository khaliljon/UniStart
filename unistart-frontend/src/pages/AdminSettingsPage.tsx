import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  Settings,
  ArrowLeft,
  Save,
  AlertCircle,
  CheckCircle,
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface SystemSettings {
  siteName: string;
  siteDescription: string;
  allowRegistration: boolean;
  requireEmailVerification: boolean;
  maxQuizAttempts: number;
  sessionTimeout: number;
  enableNotifications: boolean;
}

const AdminSettingsPage = () => {
  const navigate = useNavigate();
  const [settings, setSettings] = useState<SystemSettings>({
    siteName: 'UniStart',
    siteDescription: 'Образовательная платформа для изучения с помощью карточек и тестов',
    allowRegistration: true,
    requireEmailVerification: false,
    maxQuizAttempts: 3,
    sessionTimeout: 30,
    enableNotifications: true,
  });
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error', text: string } | null>(null);

  useEffect(() => {
    loadSettings();
  }, []);

  const loadSettings = async () => {
    try {
      setLoading(true);
      const response = await api.get('/admin/settings');
      setSettings(response.data);
    } catch (error) {
      console.error('Ошибка загрузки настроек:', error);
      // Используем дефолтные настройки
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    try {
      setSaving(true);
      await api.put('/admin/settings', settings);
      setMessage({ type: 'success', text: 'Настройки успешно сохранены' });
      setTimeout(() => setMessage(null), 3000);
    } catch (error) {
      console.error('Ошибка сохранения настроек:', error);
      setMessage({ type: 'error', text: 'Не удалось сохранить настройки' });
      setTimeout(() => setMessage(null), 3000);
    } finally {
      setSaving(false);
    }
  };

  const handleChange = (field: keyof SystemSettings, value: any) => {
    setSettings(prev => ({ ...prev, [field]: value }));
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка настроек...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-purple-50 py-8 px-4">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <Button
            variant="secondary"
            onClick={() => navigate('/admin/analytics')}
            className="mb-4 flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Назад к аналитике
          </Button>

          <div className="flex items-center gap-3 mb-2">
            <Settings className="w-10 h-10 text-purple-500" />
            <h1 className="text-4xl font-bold text-gray-900">
              Настройки системы
            </h1>
          </div>
          <p className="text-gray-600">
            Конфигурация и параметры платформы
          </p>
        </motion.div>

        {/* Message */}
        {message && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            className={`mb-6 p-4 rounded-lg flex items-center gap-2 ${
              message.type === 'success'
                ? 'bg-green-50 text-green-800 border border-green-200'
                : 'bg-red-50 text-red-800 border border-red-200'
            }`}
          >
            {message.type === 'success' ? (
              <CheckCircle className="w-5 h-5" />
            ) : (
              <AlertCircle className="w-5 h-5" />
            )}
            {message.text}
          </motion.div>
        )}

        {/* Settings Form */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
        >
          <Card className="p-6 mb-6">
            <h2 className="text-2xl font-bold text-gray-900 mb-6">Основные настройки</h2>

            <div className="space-y-6">
              {/* Site Name */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Название сайта
                </label>
                <input
                  type="text"
                  value={settings.siteName}
                  onChange={(e) => handleChange('siteName', e.target.value)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>

              {/* Site Description */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Описание сайта
                </label>
                <textarea
                  value={settings.siteDescription}
                  onChange={(e) => handleChange('siteDescription', e.target.value)}
                  rows={3}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>

              {/* Max Quiz Attempts */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Максимальное количество попыток для тестов (по умолчанию)
                </label>
                <input
                  type="number"
                  value={settings.maxQuizAttempts}
                  onChange={(e) => handleChange('maxQuizAttempts', parseInt(e.target.value))}
                  min={1}
                  max={10}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>

              {/* Session Timeout */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Таймаут сессии (минуты)
                </label>
                <input
                  type="number"
                  value={settings.sessionTimeout}
                  onChange={(e) => handleChange('sessionTimeout', parseInt(e.target.value))}
                  min={5}
                  max={120}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                />
              </div>
            </div>
          </Card>

          <Card className="p-6 mb-6">
            <h2 className="text-2xl font-bold text-gray-900 mb-6">Регистрация и доступ</h2>

            <div className="space-y-4">
              {/* Allow Registration */}
              <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div>
                  <h3 className="font-medium text-gray-900">Разрешить регистрацию</h3>
                  <p className="text-sm text-gray-600">Пользователи могут создавать новые аккаунты</p>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    checked={settings.allowRegistration}
                    onChange={(e) => handleChange('allowRegistration', e.target.checked)}
                    className="sr-only peer"
                  />
                  <div className="w-11 h-6 bg-gray-300 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                </label>
              </div>

              {/* Require Email Verification */}
              <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div>
                  <h3 className="font-medium text-gray-900">Требовать подтверждение email</h3>
                  <p className="text-sm text-gray-600">Новые пользователи должны подтвердить email</p>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    checked={settings.requireEmailVerification}
                    onChange={(e) => handleChange('requireEmailVerification', e.target.checked)}
                    className="sr-only peer"
                  />
                  <div className="w-11 h-6 bg-gray-300 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                </label>
              </div>

              {/* Enable Notifications */}
              <div className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div>
                  <h3 className="font-medium text-gray-900">Включить уведомления</h3>
                  <p className="text-sm text-gray-600">Отправлять уведомления пользователям</p>
                </div>
                <label className="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    checked={settings.enableNotifications}
                    onChange={(e) => handleChange('enableNotifications', e.target.checked)}
                    className="sr-only peer"
                  />
                  <div className="w-11 h-6 bg-gray-300 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600"></div>
                </label>
              </div>
            </div>
          </Card>

          {/* Save Button */}
          <div className="flex justify-end">
            <Button
              variant="primary"
              onClick={handleSave}
              disabled={saving}
              className="flex items-center gap-2 px-8"
            >
              {saving ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                  Сохранение...
                </>
              ) : (
                <>
                  <Save className="w-5 h-5" />
                  Сохранить настройки
                </>
              )}
            </Button>
          </div>
        </motion.div>
      </div>
    </div>
  );
};

export default AdminSettingsPage;
