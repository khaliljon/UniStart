import { Link, useNavigate, Outlet } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { 
  BookOpen, 
  FileText, 
  Users, 
  Settings, 
  LogOut,
  LayoutDashboard
} from 'lucide-react';
import Button from '../common/Button';

const Layout = () => {
  const { user, isAdmin, isTeacher, logout } = useAuth();
  const navigate = useNavigate();

  console.log('🏗️ Layout рендерится. User:', user);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            {/* Logo */}
            <Link to="/" className="flex items-center gap-2">
              <div className="w-10 h-10 bg-gradient-to-br from-primary-500 to-primary-600 rounded-lg flex items-center justify-center">
                <span className="text-white font-bold text-xl">U</span>
              </div>
              <span className="text-2xl font-bold text-gray-900">UniStart</span>
            </Link>

            {/* Navigation */}
            {user && (
              <nav className="hidden md:flex items-center gap-6">
                <Link
                  to="/dashboard"
                  className="flex items-center gap-2 text-gray-700 hover:text-primary-600 transition-colors"
                >
                  <LayoutDashboard className="w-5 h-5" />
                  <span>Панель</span>
                </Link>

                <Link
                  to="/flashcards"
                  className="flex items-center gap-2 text-gray-700 hover:text-primary-600 transition-colors"
                >
                  <BookOpen className="w-5 h-5" />
                  <span>Карточки</span>
                </Link>

                <Link
                  to="/quizzes"
                  className="flex items-center gap-2 text-gray-700 hover:text-primary-600 transition-colors"
                >
                  <FileText className="w-5 h-5" />
                  <span>Тесты</span>
                </Link>

                {(isTeacher || isAdmin) && (
                  <Link
                    to="/teacher/students"
                    className="flex items-center gap-2 text-gray-700 hover:text-primary-600 transition-colors"
                  >
                    <Users className="w-5 h-5" />
                    <span>Студенты</span>
                  </Link>
                )}

                {isAdmin && (
                  <Link
                    to="/admin/users"
                    className="flex items-center gap-2 text-gray-700 hover:text-primary-600 transition-colors"
                  >
                    <Settings className="w-5 h-5" />
                    <span>Админ</span>
                  </Link>
                )}
              </nav>
            )}

            {/* User Menu */}
            <div className="flex items-center gap-4">
              {user ? (
                <>
                  <div className="hidden sm:flex flex-col items-end">
                    <span className="text-sm font-medium text-gray-900">
                      {user.firstName} {user.lastName}
                    </span>
                    <span className="text-xs text-gray-500">{user.email}</span>
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
      <footer className="bg-white border-t border-gray-200 mt-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-4">
                UniStart
              </h3>
              <p className="text-gray-600 text-sm">
                Образовательная платформа для подготовки к поступлению в
                университеты Казахстана
              </p>
            </div>

            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-4">
                Полезные ссылки
              </h3>
              <ul className="space-y-2">
                <li>
                  <a href="#" className="text-gray-600 hover:text-primary-600 text-sm">
                    О платформе
                  </a>
                </li>
                <li>
                  <a href="#" className="text-gray-600 hover:text-primary-600 text-sm">
                    Гайды по поступлению
                  </a>
                </li>
                <li>
                  <a href="#" className="text-gray-600 hover:text-primary-600 text-sm">
                    FAQ
                  </a>
                </li>
                <li>
                  <a href="#" className="text-gray-600 hover:text-primary-600 text-sm">
                    Контакты
                  </a>
                </li>
              </ul>
            </div>

            <div>
              <h3 className="text-lg font-semibold text-gray-900 mb-4">
                Контакты
              </h3>
              <ul className="space-y-2 text-sm text-gray-600">
                <li>Email: support@unistart.kz</li>
                <li>Телефон: +7 (777) 123-45-67</li>
                <li>Астана, Казахстан</li>
              </ul>
            </div>
          </div>

          <div className="mt-8 pt-8 border-t border-gray-200 text-center text-sm text-gray-500">
            © 2025 UniStart. Все права защищены.
          </div>
        </div>
      </footer>
    </div>
  );
};

export default Layout;
