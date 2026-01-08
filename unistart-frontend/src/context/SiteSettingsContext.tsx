import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import api from '../services/api';

interface SiteSettings {
  siteName: string;
  siteDescription: string;
}

interface SiteSettingsContextType {
  settings: SiteSettings;
  loading: boolean;
  refreshSettings: () => Promise<void>;
}

const SiteSettingsContext = createContext<SiteSettingsContextType | undefined>(undefined);

export const useSiteSettings = () => {
  const context = useContext(SiteSettingsContext);
  if (!context) {
    throw new Error('useSiteSettings must be used within SiteSettingsProvider');
  }
  return context;
};

export const SiteSettingsProvider = ({ children }: { children: ReactNode }) => {
  const [settings, setSettings] = useState<SiteSettings>({
    siteName: 'UniStart',
    siteDescription: 'Образовательная платформа для изучения с помощью карточек, квизов и экзаменов',
  });
  const [loading, setLoading] = useState(true);

  const loadSettings = async () => {
    try {
      // Проверяем наличие токена перед запросом
      const token = localStorage.getItem('token');
      if (!token) {
        // Если нет токена, используем дефолтные настройки
        setLoading(false);
        return;
      }

      const response = await api.get('/admin/settings');
      const data = response.data;
      const loadedSettings: SiteSettings = {
        siteName: data?.SiteName || data?.siteName || 'UniStart',
        siteDescription: data?.SiteDescription || data?.siteDescription || 'Образовательная платформа для изучения с помощью карточек, квизов и экзаменов',
      };
      setSettings(loadedSettings);
      
      // Обновляем document.title
      document.title = `${loadedSettings.siteName} - Твой путь в университет мечты`;
      
      // Обновляем meta description
      let metaDescription = document.querySelector('meta[name="description"]');
      if (!metaDescription) {
        metaDescription = document.createElement('meta');
        metaDescription.setAttribute('name', 'description');
        document.head.appendChild(metaDescription);
      }
      metaDescription.setAttribute('content', loadedSettings.siteDescription);
    } catch (error) {
      // Игнорируем ошибку при загрузке настроек (например, если пользователь не администратор)
      // Используем дефолтные значения из state
      console.debug('Не удалось загрузить настройки сайта, используются значения по умолчанию');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSettings();
  }, []);

  const refreshSettings = async () => {
    await loadSettings();
  };

  return (
    <SiteSettingsContext.Provider value={{ settings, loading, refreshSettings }}>
      {children}
    </SiteSettingsContext.Provider>
  );
};

