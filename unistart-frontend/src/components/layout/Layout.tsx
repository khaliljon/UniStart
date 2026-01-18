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
  Check,
  User as UserIcon
} from 'lucide-react';
import { useState, useRef, useEffect } from 'react';
import Button from '../common/Button';

const Layout = () => {
  const { user, isAdmin, isTeacher, logout } = useAuth();
  const { theme, setTheme } = useTheme();
  const { settings: siteSettings } = useSiteSettings();
  const navigate = useNavigate();
  const [isProfileMenuOpen, setIsProfileMenuOpen] = useState(false);
  const profileMenuRef = useRef<HTMLDivElement>(null);

  const themes = [
    { id: 'light' as const, name: 'Светлая', icon: Sun, emoji: '☀️' },
    { id: 'dark' as const, name: 'Тёмная', icon: Moon, emoji: '🌙' },
    { id: 'ocean' as const, name: 'Океан', icon: Palette, emoji: '🌊' },
    { id: 'synthwave' as const, name: 'Synthwave', icon: Palette, emoji: '🎮' },
    { id: 'high-contrast' as const, name: 'Контраст', icon: Palette, emoji: '♿' },
  ];

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (profileMenuRef.current && !profileMenuRef.current.contains(event.target as Node)) {
        setIsProfileMenuOpen(false);
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
      <header className="sticky top-0 z-50 bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700">
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
              {user ? (
                <div className="relative" ref={profileMenuRef}>
                  {/* Profile Button */}
                  <button
                    type="button"
                    onClick={() => setIsProfileMenuOpen(!isProfileMenuOpen)}
                    className="flex items-center gap-2 p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                    aria-label="Меню профиля"
                  >
                    <div className="w-8 h-8 rounded-full bg-gradient-to-br from-primary-500 to-primary-600 flex items-center justify-center text-white font-medium text-sm">
                      {user.firstName?.charAt(0).toUpperCase()}{user.lastName?.charAt(0).toUpperCase()}
                    </div>
                  </button>

                  {/* Dropdown Menu */}
                  {isProfileMenuOpen && (
                    <div className="absolute right-0 mt-2 w-72 rounded-lg shadow-xl bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 z-50 overflow-hidden">
                      {/* User Info Header */}
                      <div className="p-4 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-full bg-gradient-to-br from-primary-500 to-primary-600 flex items-center justify-center text-white font-medium">
                            {user.firstName?.charAt(0).toUpperCase()}{user.lastName?.charAt(0).toUpperCase()}
                          </div>
                          <div className="flex-1 min-w-0">
                            <p className="text-sm font-semibold text-gray-900 dark:text-white truncate">
                              {user.firstName} {user.lastName}
                            </p>
                            <p className="text-xs text-gray-500 dark:text-gray-400 truncate">
                              {user.email}
                            </p>
                          </div>
                        </div>
                      </div>

                      {/* Menu Items */}
                      <div className="p-2">
                        <Link
                          to="/profile"
                          onClick={() => setIsProfileMenuOpen(false)}
                          className="flex items-center gap-3 px-3 py-2 rounded-md hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300 transition-colors"
                        >
                          <UserIcon className="w-5 h-5" />
                          <span className="text-sm font-medium">Мой аккаунт</span>
                        </Link>

                        {/* Theme Submenu */}
                        <div className="px-3 py-2">
                          <div className="flex items-center gap-3 text-gray-700 dark:text-gray-300 mb-2">
                            <Palette className="w-5 h-5" />
                            <span className="text-sm font-medium">Оформление</span>
                          </div>
                          <div className="ml-8 space-y-1">
                            {themes.map((t) => (
                              <button
                                key={t.id}
                                onClick={() => {
                                  setTheme(t.id);
                                }}
                                className={`w-full flex items-center gap-2 px-2 py-1.5 rounded text-xs transition-colors ${
                                  theme === t.id
                                    ? 'bg-primary-50 dark:bg-primary-900/20 text-primary-600 dark:text-primary-400'
                                    : 'hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-600 dark:text-gray-400'
                                }`}
                              >
                                <span>{t.emoji}</span>
                                <span className="flex-1 text-left">{t.name}</span>
                                {theme === t.id && (
                                  <Check className="w-3 h-3" />
                                )}
                              </button>
                            ))}
                          </div>
                        </div>

                        {/* Divider */}
                        <div className="my-2 border-t border-gray-200 dark:border-gray-700"></div>

                        {/* Logout */}
                        <button
                          onClick={() => {
                            setIsProfileMenuOpen(false);
                            handleLogout();
                          }}
                          className="w-full flex items-center gap-3 px-3 py-2 rounded-md hover:bg-red-50 dark:hover:bg-red-900/20 text-red-600 dark:text-red-400 transition-colors"
                        >
                          <LogOut className="w-5 h-5" />
                          <span className="text-sm font-medium">Выход</span>
                        </button>
                      </div>
                    </div>
                  )}
                </div>
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
