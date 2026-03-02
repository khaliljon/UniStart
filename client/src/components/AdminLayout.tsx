import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useState, useRef, useEffect } from 'react';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTheme } from '../hooks/useTheme';
import { logout } from '../store/slices/authSlice';
import { chatService } from '../services/chatService';

function AdminLayout() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { theme, toggleTheme } = useTheme();
  const [moreOpen, setMoreOpen] = useState(false);
  const moreRef = useRef<HTMLLIElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (moreRef.current && !moreRef.current.contains(e.target as Node)) {
        setMoreOpen(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const handleLogout = () => {
    chatService.stop();
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
                Обзор
              </NavLink>
            </li>
            <li>
              <NavLink to="/questions">
                Вопросы
              </NavLink>
            </li>
            <li>
              <NavLink to="/users">
                Пользователи
              </NavLink>
            </li>
            <li ref={moreRef} style={{ position: 'relative' }}>
              <button
                onClick={() => setMoreOpen(!moreOpen)}
                style={{
                  background: 'none', border: 'none', cursor: 'pointer',
                  color: 'var(--text-primary)', fontSize: 'inherit', fontFamily: 'inherit',
                  padding: '0.5rem 0.75rem', display: 'flex', alignItems: 'center', gap: '0.25rem',
                }}
              >
                Ещё <span style={{ fontSize: '0.55rem', opacity: 0.6 }}>▼</span>
              </button>
              {moreOpen && (
                <div style={{
                  position: 'absolute', top: '100%', right: 0, minWidth: '170px',
                  background: 'var(--bg-primary)', border: '1px solid var(--border-color)',
                  borderRadius: '8px', boxShadow: '0 4px 16px rgba(0,0,0,0.12)',
                  zIndex: 100, padding: '0.35rem 0', marginTop: '0.25rem',
                }}>
                  {[
                    { label: 'Тьюторы', path: '/tutors' },
                    { label: 'Импорт', path: '/import' },
                    { label: 'Аудит', path: '/audit' },
                    { label: 'Здоровье', path: '/health' },
                    { label: 'Активность', path: '/activity' },
                  ].map(item => (
                    <NavLink
                      key={item.path}
                      to={item.path}
                      onClick={() => setMoreOpen(false)}
                      style={({ isActive }) => ({
                        display: 'block', padding: '0.5rem 1rem', fontSize: '0.88rem',
                        color: isActive ? 'var(--primary-color)' : 'var(--text-primary)',
                        fontWeight: isActive ? 600 : 400,
                        textDecoration: 'none',
                      })}
                    >
                      {item.label}
                    </NavLink>
                  ))}
                </div>
              )}
            </li>
          </ul>

          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <button
              onClick={toggleTheme}
              className="theme-toggle"
              title={theme === 'light' ? 'Тёмная тема' : 'Светлая тема'}
            >
              {theme === 'light' ? '◑' : '○'}
            </button>
            <span style={{ color: 'var(--text-secondary)' }}>
              {user?.name}
            </span>
            <button onClick={handleLogout} className="btn btn-outline">
              Выйти
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
