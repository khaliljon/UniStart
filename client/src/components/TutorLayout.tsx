import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useState, useEffect, useRef } from 'react';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTheme } from '../hooks/useTheme';
import { logout } from '../store/slices/authSlice';
import { tutorService } from '../services/tutorService';
import { messageService } from '../services/messageService';
import { chatService } from '../services/chatService';
import { useBranding } from '../contexts/BrandingContext';

function TutorLayout() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { theme, toggleTheme } = useTheme();

  const [pendingCount, setPendingCount] = useState(0);
  const [unreadCount, setUnreadCount] = useState(0);
  const [hasSchool, setHasSchool] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const { isWhiteLabel, branding } = useBranding();
  const [wlBlocked, setWlBlocked] = useState(false);

  useEffect(() => {
    const loadCounts = async () => {
      try {
        const [pending, unread] = await Promise.all([
          tutorService.getPendingRequests(),
          messageService.getUnreadCount(),
        ]);
        setPendingCount(pending.length);
        setUnreadCount(unread);
      } catch {
        // ignore
      }
    };
    loadCounts();
    const interval = setInterval(loadCounts, 30000);

    // Check school ownership
    tutorService.getMySchool().then(s => setHasSchool(s !== null)).catch(() => {});

    // WL access guard: check if tutor is verified for this school
    if (isWhiteLabel && user) {
      tutorService.getTutorProfile(user.id).then(profile => {
        if (!profile.isVerified) setWlBlocked(true);
      }).catch(() => setWlBlocked(true));
    }

    // Live unread via SignalR
    chatService.start();
    const unsub = chatService.onUnreadCount((count) => {
      setUnreadCount(count);
    });

    return () => {
      clearInterval(interval);
      unsub();
    };
  }, []);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false);
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

  const goTo = (path: string) => {
    setMenuOpen(false);
    navigate(path);
  };

  const Badge = ({ count }: { count: number }) =>
    count > 0 ? (
      <span style={{
        background: '#ef4444', color: '#fff', borderRadius: '999px',
        padding: '0.1rem 0.4rem', fontSize: '0.65rem', fontWeight: 700,
        marginLeft: '0.3rem', verticalAlign: 'super', lineHeight: 1,
      }}>
        {count > 99 ? '99+' : count}
      </span>
    ) : null;

  const initials = user?.name
    ? user.name.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2)
    : '?';

  // WL: block unverified tutors
  if (wlBlocked) {
    return (
      <div className="layout" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh' }}>
        <div className="card" style={{ padding: '2.5rem', textAlign: 'center', maxWidth: '480px' }}>
          <h2 style={{ marginBottom: '1rem' }}>
            {branding?.navbarTitle || branding?.name || 'School'}
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem', lineHeight: 1.5 }}>
            Your tutor application for this school is pending verification. The school administrator will review your application shortly.
          </p>
          <button className="btn btn-primary" onClick={handleLogout}>
            Log out
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="layout">
      <nav className="navbar">
        <div className="container navbar-content">
          <NavLink to="/" className="navbar-brand">
            UniStart <span style={{
              fontSize: '0.65rem',
              padding: '0.15rem 0.5rem',
              borderRadius: '999px',
              background: '#8b5cf6',
              color: '#fff',
              marginLeft: '0.5rem',
              verticalAlign: 'middle',
              fontWeight: 700,
              letterSpacing: '0.05em',
            }}>ТЬЮТОР</span>
          </NavLink>

          <ul className="navbar-nav">
            <li>
              <NavLink to="/" end>
                Главная
                <Badge count={pendingCount} />
              </NavLink>
            </li>
            <li>
              <NavLink to="/students">
                Ученики
              </NavLink>
            </li>
            <li>
              <NavLink to="/questions">
                Вопросы
              </NavLink>
            </li>
            <li>
              <NavLink to="/assignments">
                Задания
              </NavLink>
            </li>
            <li>
              <NavLink to="/messages">
                Сообщения
                <Badge count={unreadCount} />
              </NavLink>
            </li>
            <li>
              <NavLink to="/schedule">
                Расписание
              </NavLink>
            </li>
            {!isWhiteLabel && (
              <li>
                <NavLink to="/school">
                  Школа
                </NavLink>
              </li>
            )}
            {!isWhiteLabel && hasSchool && (
              <li>
                <NavLink to="/school-admin">
                  Панель школы
                </NavLink>
              </li>
            )}
            <li>
              <NavLink to="/content">
                Контент
              </NavLink>
            </li>
          </ul>

          <div ref={menuRef} style={{ position: 'relative', display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <button
              onClick={toggleTheme}
              className="theme-toggle"
              title={theme === 'light' ? 'Тёмная тема' : 'Светлая тема'}
            >
              {theme === 'light' ? '◑' : '○'}
            </button>
            <button
              onClick={() => setMenuOpen(!menuOpen)}
              className="profile-trigger"
              title="Меню"
            >
              <span className="profile-avatar" style={{ width: '32px', height: '32px', fontSize: '0.75rem' }}>
                {initials}
              </span>
              <span style={{ fontSize: '0.6rem', opacity: 0.6 }}>▼</span>
            </button>
            {menuOpen && (
              <div className="profile-dropdown" style={{ minWidth: '160px' }}>
                <div style={{ padding: '0.5rem 1rem', fontSize: '0.8rem', color: 'var(--text-secondary)', fontWeight: 500 }}>
                  {user?.name}
                </div>
                <div className="profile-dropdown-divider" />
                <button className="profile-dropdown-item" onClick={() => goTo('/reviews')}>Отзывы</button>
                <button className="profile-dropdown-item" onClick={() => goTo('/my-profile')}>Профиль тьютора</button>
                <button className="profile-dropdown-item" onClick={() => goTo('/profile')}>Настройки</button>
                <div className="profile-dropdown-divider" />
                <button className="profile-dropdown-item profile-dropdown-danger" onClick={handleLogout}>Выйти</button>
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

export default TutorLayout;
