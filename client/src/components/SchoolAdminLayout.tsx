import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTheme } from '../hooks/useTheme';
import { useTranslation } from '../i18n';
import { logout } from '../store/slices/authSlice';
import { chatService } from '../services/chatService';
import { messageService } from '../services/messageService';
import api from '../services/api';

function SchoolAdminLayout() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { theme, toggleTheme } = useTheme();
  const { t } = useTranslation();
  const [menuOpen, setMenuOpen] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);
  const [pendingApps, setPendingApps] = useState(0);

  useEffect(() => {
    messageService.getUnreadCount().then(setUnreadCount).catch(() => {});
    api.get<{ id: number; status: string }[]>('/tutor-school-applications/school?status=Pending')
      .then(({ data }) => setPendingApps(data.length))
      .catch(() => {});

    chatService.start();
    const unsub = chatService.onUnreadCount(setUnreadCount);

    const interval = setInterval(() => {
      messageService.getUnreadCount().then(setUnreadCount).catch(() => {});
      api.get<{ id: number; status: string }[]>('/tutor-school-applications/school?status=Pending')
        .then(({ data }) => setPendingApps(data.length))
        .catch(() => {});
    }, 30000);

    return () => { clearInterval(interval); unsub(); };
  }, []);

  const handleLogout = () => {
    chatService.stop();
    dispatch(logout());
    navigate('/login');
  };

  const Badge = ({ count }: { count: number }) =>
    count > 0 ? (
      <span style={{
        background: '#ef4444', color: '#fff', borderRadius: '999px',
        padding: '0.1rem 0.4rem', fontSize: '0.65rem', fontWeight: 700,
        marginLeft: '0.3rem', verticalAlign: 'super', lineHeight: 1,
      }}>{count > 99 ? '99+' : count}</span>
    ) : null;

  return (
    <div className="layout">
      <nav className="navbar">
        <div className="container navbar-content">
          <NavLink to="/" className="navbar-brand">
            UniStart <span style={{
              fontSize: '0.65rem',
              padding: '0.15rem 0.5rem',
              borderRadius: '999px',
              background: '#f59e0b',
              color: '#fff',
              marginLeft: '0.5rem',
              verticalAlign: 'middle',
              fontWeight: 700,
              letterSpacing: '0.05em',
            }}>{t.schoolAdmin.badge}</span>
          </NavLink>

          <button
            className="burger-btn"
            onClick={() => setMenuOpen(!menuOpen)}
            aria-label="Toggle menu"
          >
            <span className={`burger-icon ${menuOpen ? 'open' : ''}`} />
          </button>

          <div className={`navbar-collapse ${menuOpen ? 'show' : ''}`}>
            <ul className="navbar-nav">
              <li>
                <NavLink to="/" end onClick={() => setMenuOpen(false)}>
                  {t.schoolAdmin.dashboard}
                </NavLink>
              </li>
              <li>
                <NavLink to="/students" onClick={() => setMenuOpen(false)}>
                  {t.schoolAdmin.students}
                </NavLink>
              </li>
              <li>
                <NavLink to="/tutors" onClick={() => setMenuOpen(false)}>
                  {t.schoolAdmin.tutors}
                </NavLink>
              </li>
              <li>
                <NavLink to="/applications" onClick={() => setMenuOpen(false)}>
                  {t.schoolAdmin.applications}
                  <Badge count={pendingApps} />
                </NavLink>
              </li>
              <li>
                <NavLink to="/messages" onClick={() => setMenuOpen(false)}>
                  {t.schoolAdmin.messages}
                  <Badge count={unreadCount} />
                </NavLink>
              </li>
            </ul>

            <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
              <button onClick={toggleTheme} className="theme-toggle" title={theme === 'light' ? 'Dark' : 'Light'}>
                {theme === 'light' ? '◑' : '○'}
              </button>
              <NavLink to="/school" style={{ color: 'var(--text-secondary)', textDecoration: 'none', fontSize: '0.85rem' }} onClick={() => setMenuOpen(false)}>
                {t.schoolAdmin.school}
              </NavLink>
              <NavLink to="/profile" style={{ color: 'var(--text-secondary)', textDecoration: 'none', fontSize: '0.85rem' }} onClick={() => setMenuOpen(false)}>
                {user?.name}
              </NavLink>
              <button onClick={handleLogout} className="btn btn-outline">
                {t.schoolAdmin.logout}
              </button>
            </div>
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

export default SchoolAdminLayout;
