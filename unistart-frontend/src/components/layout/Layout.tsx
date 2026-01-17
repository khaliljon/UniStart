import { Link, useNavigate, Outlet } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useTheme } from '../../context/ThemeContext';
import { useSiteSettings } from '../../context/SiteSettingsContext';
import { 
  BookOpen, 
  FileText, 
  Users, 
  Settings, 
  LogOut,
  LayoutDashboard,
  ClipboardCheck,
  Sun,
  Moon,
  Palette,
  Check
} from 'lucide-react';
import { useState, useRef, useEffect } from 'react';
import Button from '../common/Button';

const Layout = () => {
  const { user, isAdmin, isTeacher, logout } = useAuth();
  const { theme, setTheme } = useTheme();
  const { settings: siteSettings } = useSiteSettings();
  const navigate = useNavigate();
  const [isThemeMenuOpen, setIsThemeMenuOpen] = useState(false);
  const themeMenuRef = useRef<HTMLDivElement>(null);

  const themes = [
    { id: 'light' as const, name: 'Светлая', icon: Sun, emoji: '☀️' },
    { id: 'dark' as const, name: 'Тёмная', icon: Moon, emoji: '🌙' },
    { id: 'ocean' as const, name: 'Океан', icon: Palette, emoji: '🌊' },
    { id: 'synthwave' as const, name: 'Synthwave', icon: Palette, emoji: '🎮' },
    { id: 'high-contrast' as const, name: 'Контраст', icon: Palette, emoji: '♿' },
  ];

  const currentTheme = themes.find(t => t.id === theme) || themes[0];

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (themeMenuRef.current && !themeMenuRef.current.contains(event.target as Node)) {
        setIsThemeMenuOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  console.log('🏗️ Layout рендерится. User:', user);
  console.log('🎨 Текущая тема:', theme);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <header className="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            {/* Logo */}
            <Link to="/" className="flex items-center" title={siteSettings.siteDescription}>
              <span className="text-2xl font-bold text-gray-900 dark:text-white">
                <span className="inline-flex items-center justify-center w-8 h-8 bg-gradient-to-br from-primary-500 to-primary-600 rounded text-white mr-0.5">
                  {siteSettings.siteName.charAt(0).toUpperCase()}
                </span>
                <span>{siteSettings.siteName.substring(1)}</span>
              </span>
            </Link>

            {/* Navigation */}
            {user && (
              <nav className="hidden md:flex items-center gap-6">
                <Link
                  to="/dashboard"
                  className="flex items-center gap-2 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
                >
                  <LayoutDashboard className="w-5 h-5" />
                  <span>Панель</span>
                </Link>

                <Link
                  to="/flashcards"
                  className="flex items-center gap-2 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
                >
                  <BookOpen className="w-5 h-5" />
                  <span>Карточки</span>
                </Link>

                <Link
                  to="/quizzes"
                  className="flex items-center gap-2 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
                >
                  <FileText className="w-5 h-5" />
                  <span>Квизы</span>
                </Link>

                <Link
                  to="/exams"
                  className="flex items-center gap-2 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
                >
                  <ClipboardCheck className="w-5 h-5" />
                  <span>Экзамены</span>
                </Link>

                {isTeacher && (
                  <Link
                    to="/teacher/students"
                    className="flex items-center gap-2 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
                  >
                    <Users className="w-5 h-5" />
                    <span>Студенты</span>
                  </Link>
                )}
                {isAdmin && (
                  <Link
                    to="/admin/students"
                    className="flex items-center gap-2 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
                  >
                    <Users className="w-5 h-5" />
                    <span>Студенты</span>
                  </Link>
                )}

                {isAdmin && (
                  <>
                    <Link
                      to="/admin/ml-training"
                      className="flex items-center gap-2 text-purple-600 dark:text-purple-400 hover:text-purple-700 dark:hover:text-purple-300 transition-colors font-medium"
                      title="ML Model Training - Управление обучением модели"
                    >
                      🤖
                      <span>ML Training</span>
                    </Link>
                    <Link
                      to="/admin/ai-flashcards"
                      className="flex items-center gap-2 text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 transition-colors font-medium"
                      title="AI Flashcard Generator - Генерация карточек с помощью ИИ"
                    >
                      ✨
                      <span>AI Генератор</span>
                    </Link>
                    <Link
                      to="/admin/users"
                      className="flex items-center gap-2 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
                    >
                      <Settings className="w-5 h-5" />
                      <span>Админ</span>
                    </Link>
                  </>
                )}
              </nav>
            )}

            {/* User Menu */}
            <div className="flex items-center gap-4">
              {/* Theme Selector Dropdown */}
              <div className="relative" ref={themeMenuRef}>
                <button
                  type="button"
                  onClick={() => setIsThemeMenuOpen(!isThemeMenuOpen)}
                  className="p-2 rounded-lg text-gray-500 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800 transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-primary-500 hover:scale-105 active:scale-95"
                  title="Выбрать тему"
                  aria-label="Выбрать тему"
                >
                  <currentTheme.icon className="w-5 h-5 transition-transform duration-200" />
                </button>

                {/* Dropdown Menu */}
                {isThemeMenuOpen && (
                  <div className="absolute right-0 mt-2 w-56 rounded-lg shadow-lg bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 z-50 overflow-hidden">
                    <div className="p-2">
                      <div className="px-3 py-2 text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase">
                        Выбор темы
                      </div>
                      {themes.map((t) => {
                        const Icon = t.icon;
                        return (
                          <button
                            key={t.id}
                            onClick={() => {
                              setTheme(t.id);
                              setIsThemeMenuOpen(false);
                            }}
                            className={`w-full flex items-center gap-3 px-3 py-2 rounded-md transition-colors ${
                              theme === t.id
                                ? 'bg-primary-50 dark:bg-primary-900/20 text-primary-600 dark:text-primary-400'
                                : 'hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300'
                            }`}
                          >
                            <span className="text-lg">{t.emoji}</span>
                            <span className="flex-1 text-left text-sm font-medium">{t.name}</span>
                            {theme === t.id && (
                              <Check className="w-4 h-4" />
                            )}
                          </button>
                        );
                      })}
                    </div>
                  </div>
                )}
              </div>

              {user ? (
                <>
                  <div className="hidden sm:flex flex-col items-end">
                    <span className="text-sm font-medium text-gray-900 dark:text-white">
                      {user.firstName} {user.lastName}
                    </span>
                    <span className="text-xs text-gray-500 dark:text-gray-400">{user.email}</span>
                  </div>
                  <Button
                    variant="secondary"
                    onClick={handleLogout}
                    className="flex items-center gap-2"
                  >
                    <LogOut className="w-4 h-4" />
                    <span className="hidden sm:inline">Выход</span>
                  </Button>
                </>
              ) : (
                <div className="flex items-center gap-3">
                  <Link to="/login">
                    <Button variant="secondary">Вход</Button>
                  </Link>
                  <Link to="/register">
                    <Button variant="primary">Регистрация</Button>
                  </Link>
                </div>
              )}
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main><Outlet /></main>

      {/* Footer */}
      <footer className="bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700 mt-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div>
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                UniStart
              </h3>
              <p className="text-gray-600 dark:text-gray-400 text-sm">
                Образовательная платформа для подготовки к поступлению в
                университеты Казахстана
              </p>
            </div>

            <div>
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Полезные ссылки
              </h3>
              <ul className="space-y-2">
                <li>
                  <a href="#" className="text-gray-600 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 text-sm">
                    О платформе
                  </a>
                </li>
                <li>
                  <a href="#" className="text-gray-600 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 text-sm">
                    Гайды по поступлению
                  </a>
                </li>
                <li>
                  <a href="#" className="text-gray-600 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 text-sm">
                    FAQ
                  </a>
                </li>
                <li>
                  <a href="#" className="text-gray-600 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 text-sm">
                    Контакты
                  </a>
                </li>
              </ul>
            </div>

            <div>
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Контакты
              </h3>
              <ul className="space-y-2 text-sm text-gray-600 dark:text-gray-400">
                <li>Email: support@unistart.kz</li>
                <li>Телефон: +7 (777) 123-45-67</li>
                <li>Астана, Казахстан</li>
              </ul>
            </div>
          </div>

          <div className="mt-8 pt-8 border-t border-gray-200 dark:border-gray-700 text-center text-sm text-gray-500 dark:text-gray-400">
            © 2025 UniStart. Все права защищены.
          </div>
        </div>
      </footer>
    </div>
  );
};

export default Layout;
