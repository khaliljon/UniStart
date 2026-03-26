import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useState, useEffect, useRef } from 'react';
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
  const menuRef = useRef<HTMLDivElement>(null);
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

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) setMenuOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const handleLogout = () => {
    chatService.stop();
    dispatch(logout());
    navigate('/login');
  };

  const goTo = (path: string) => { setMenuOpen(false); navigate(path); };

  const Badge = ({ count }: { count: number }) =>
    count > 0 ? (
      <span style={{
        background: '#ef4444', color: '#fff', borderRadius: '999px',
        padding: '0.1rem 0.4rem', fontSize: '0.65rem', fontWeight: 700,
        marginLeft: '0.3rem', verticalAlign: 'super', lineHeight: 1,
      }}>{count > 99 ? '99+' : count}</span>
    ) : null;

  const initials = user?.name
    ? user.name.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2)
    : '?';

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

          <ul className="navbar-nav">
            <li>
              <NavLink to="/" end>
                {t.schoolAdmin.dashboard}
              </NavLink>
            </li>
            <li>
              <NavLink to="/applications">
                {t.schoolAdmin.applications}
                <Badge count={pendingApps} />
              </NavLink>
            </li>
            <li>
              <NavLink to="/messages">
                {t.schoolAdmin.messages}
                <Badge count={unreadCount} />
              </NavLink>
            </li>
            <li>
              <NavLink to="/school">
                {t.schoolAdmin.school}
              </NavLink>
            </li>
          </ul>

          <div ref={menuRef} style={{ position: 'relative', display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <button onClick={toggleTheme} className="theme-toggle" title={theme === 'light' ? 'Dark' : 'Light'}>
              {theme === 'light' ? '◑' : '○'}
            </button>
            <button onClick={() => setMenuOpen(!menuOpen)} className="profile-trigger" title="Menu">
              <span className="profile-avatar" style={{ width: '32px', height: '32px', fontSize: '0.75rem' }}>{initials}</span>
              <span style={{ fontSize: '0.6rem', opacity: 0.6 }}>▼</span>
            </button>
            {menuOpen && (
              <div className="profile-dropdown" style={{ minWidth: '160px' }}>
                <div style={{ padding: '0.5rem 1rem', fontSize: '0.8rem', color: 'var(--text-secondary)', fontWeight: 500 }}>
                  {user?.name}
                </div>
                <div className="profile-dropdown-divider" />
                <button className="profile-dropdown-item" onClick={() => goTo('/profile')}>{t.schoolAdmin.settings}</button>
                <div className="profile-dropdown-divider" />
                <button className="profile-dropdown-item profile-dropdown-danger" onClick={handleLogout}>{t.schoolAdmin.logout}</button>
              </div>
            )}
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
