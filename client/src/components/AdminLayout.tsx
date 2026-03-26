import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useState, useRef, useEffect } from 'react';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTheme } from '../hooks/useTheme';
import { useTranslation } from '../hooks/useTranslation';
import { logout } from '../store/slices/authSlice';
import { chatService } from '../services/chatService';
import api from '../services/api';
import LanguageSwitcher from './LanguageSwitcher';

function AdminLayout() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { theme, toggleTheme } = useTheme();
  const { t } = useTranslation();
  const [moreOpen, setMoreOpen] = useState(false);
  const moreRef = useRef<HTMLLIElement>(null);
  const [pendingApps, setPendingApps] = useState(0);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (moreRef.current && !moreRef.current.contains(e.target as Node)) {
        setMoreOpen(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  useEffect(() => {
    const loadPending = async () => {
      try {
        const { data } = await api.get('/admin/school-applications?status=Pending&pageSize=1');
        setPendingApps(data.total);
      } catch { /* ignore */ }
    };
    loadPending();
    const interval = setInterval(loadPending, 60000);
    return () => clearInterval(interval);
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
                {t.admin.nav.dashboard}
              </NavLink>
            </li>
            <li>
              <NavLink to="/questions">
                {t.admin.nav.questions}
              </NavLink>
            </li>
            <li>
              <NavLink to="/content">
                {t.admin.nav.content}
              </NavLink>
            </li>
            <li>
              <NavLink to="/users">
                {t.admin.nav.users}
              </NavLink>
            </li>
            <li>
              <NavLink to="/tutors">
                {t.admin.nav.tutors}
              </NavLink>
            </li>
            <li ref={moreRef} style={{ position: 'relative' }}>
              <NavLink
                to="#"
                onClick={(e) => { e.preventDefault(); setMoreOpen(!moreOpen); }}
                className={({ isActive: _unused }) => ''}
                style={{
                  gap: '0.25rem',
                }}
              >
                {t.admin.nav.more} {pendingApps > 0 && (
                  <span style={{
                    background: '#ef4444', width: '0.5rem', height: '0.5rem',
                    borderRadius: '50%', display: 'inline-block', marginLeft: '0.25rem',
                  }} />
                )}<span style={{ fontSize: '0.55rem', opacity: 0.6 }}>▼</span>
              </NavLink>
              {moreOpen && (
                <div style={{
                  position: 'absolute', top: '100%', right: 0, minWidth: '170px',
                  background: 'var(--card-background)', border: '1px solid var(--border-color)',
                  borderRadius: '8px', boxShadow: '0 4px 16px rgba(0,0,0,0.18)',
                  zIndex: 100, padding: '0.35rem 0', marginTop: '0.25rem',
                }}>
                  {[
                    { label: t.admin.nav.applications, path: '/applications', badge: pendingApps },
                    { label: t.admin.nav.import, path: '/import' },
                    { label: t.admin.nav.questionImport, path: '/question-import' },
                    { label: t.admin.nav.schools, path: '/schools' },
                    { label: t.admin.nav.audit, path: '/audit' },
                    { label: t.admin.nav.health, path: '/health' },
                    { label: t.admin.nav.trash, path: '/trash' },
                  ].map(item => (
                    <NavLink
                      key={item.path}
                      to={item.path}
                      onClick={() => setMoreOpen(false)}
                      style={({ isActive }) => ({
                        display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                        padding: '0.5rem 1rem', fontSize: '0.88rem',
                        color: isActive ? 'var(--primary-color)' : 'var(--text-primary)',
                        fontWeight: isActive ? 600 : 400,
                        textDecoration: 'none',
                      })}
                    >
                      {item.label}
                      {'badge' in item && (item as { badge?: number }).badge! > 0 && (
                        <span style={{
                          background: '#ef4444', color: '#fff', borderRadius: '999px',
                          padding: '0.1rem 0.45rem', fontSize: '0.65rem', fontWeight: 700,
                          lineHeight: 1, marginLeft: '0.5rem', minWidth: '1.1rem', textAlign: 'center',
                        }}>
                          {(item as { badge?: number }).badge! > 99 ? '99+' : (item as { badge?: number }).badge}
                        </span>
                      )}
                    </NavLink>
                  ))}
                </div>
              )}
            </li>
          </ul>

          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <LanguageSwitcher />
            <button
              onClick={toggleTheme}
              className="theme-toggle"
              title={theme === 'light' ? t.nav.darkMode : t.nav.lightMode}
            >
              {theme === 'light' ? '◑' : '○'}
            </button>
            <NavLink to="/profile" style={{ color: 'var(--text-secondary)', textDecoration: 'none' }}>
              {user?.name}
            </NavLink>
            <button onClick={handleLogout} className="btn btn-outline">
              {t.nav.logout}
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
