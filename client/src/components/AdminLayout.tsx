import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTheme } from '../hooks/useTheme';
import { logout } from '../store/slices/authSlice';

function AdminLayout() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { theme, toggleTheme } = useTheme();

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  return (
    <div className="layout">
      <nav className="navbar">
        <div className="container navbar-content">
          <NavLink to="/" className="navbar-brand">
            UniStart <span style={{
              fontSize: '0.65rem',
              padding: '0.15rem 0.5rem',
              borderRadius: '999px',
              background: 'var(--error-color)',
              color: '#fff',
              marginLeft: '0.5rem',
              verticalAlign: 'middle',
              fontWeight: 700,
              letterSpacing: '0.05em',
            }}>ADMIN</span>
          </NavLink>

          <ul className="navbar-nav">
            <li>
              <NavLink to="/" end>
                📊 Статистика
              </NavLink>
            </li>
            <li>
              <NavLink to="/questions">
                📋 Вопросы
              </NavLink>
            </li>
            <li>
              <NavLink to="/users">
                👥 Пользователи
              </NavLink>
            </li>
            <li>
              <NavLink to="/import">
                📥 Импорт
              </NavLink>
            </li>
            <li>
              <NavLink to="/audit">
                📜 Аудит
              </NavLink>
            </li>
            <li>
              <NavLink to="/health">
                🏥 Здоровье
              </NavLink>
            </li>
            <li>
              <NavLink to="/activity">
                👤 Активность
              </NavLink>
            </li>
          </ul>

          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <button
              onClick={toggleTheme}
              className="theme-toggle"
              title={theme === 'light' ? 'Switch to dark mode' : 'Switch to light mode'}
            >
              {theme === 'light' ? '🌙' : '☀️'}
            </button>
            <span style={{ color: 'var(--text-secondary)' }}>
              {user?.name}
            </span>
            <button onClick={handleLogout} className="btn btn-outline">
              Logout
            </button>
          </div>
        </div>
      </nav>

      <main className="main-content">
        <div className="container">
          <Outlet />
        </div>
      </main>
    </div>
  );
}

export default AdminLayout;
