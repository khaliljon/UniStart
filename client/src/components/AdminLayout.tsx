import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useState, useRef, useEffect } from 'react';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTheme } from '../hooks/useTheme';
import { useTranslation } from '../hooks/useTranslation';
import { logout } from '../store/slices/authSlice';
import { chatService } from '../services/chatService';
import LanguageSwitcher from './LanguageSwitcher';
import adminService from '../services/adminService';

function AdminLayout() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { theme, toggleTheme } = useTheme();
  const { t } = useTranslation();
  const [moreOpen, setMoreOpen] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
  const moreRef = useRef<HTMLLIElement>(null);
  const [pendingCounts, setPendingCounts] = useState<{ pendingSchools: number; pendingVerifications: number; total: number }>({ pendingSchools: 0, pendingVerifications: 0, total: 0 });
  const [showSchoolBadge, setShowSchoolBadge] = useState(false);
  const [showTutorBadge, setShowTutorBadge] = useState(false);

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
    const refresh = () => adminService.getPendingCounts().then(counts => {
      setPendingCounts(counts);
      const seenSchools = Number(localStorage.getItem('admin_seen_schools') || '0');
      const seenVerifications = Number(localStorage.getItem('admin_seen_verifications') || '0');
      setShowSchoolBadge(counts.pendingSchools > 0 && counts.pendingSchools > seenSchools);
      setShowTutorBadge(counts.pendingVerifications > 0 && counts.pendingVerifications > seenVerifications);
    }).catch(() => {});
    refresh();
    const interval = setInterval(refresh, 60000);
    window.addEventListener('admin-badge-refresh', refresh);
    return () => { clearInterval(interval); window.removeEventListener('admin-badge-refresh', refresh); };
  }, []);

  const dismissSchoolBadge = () => {
    localStorage.setItem('admin_seen_schools', String(pendingCounts.pendingSchools));
    setShowSchoolBadge(false);
  };

  const dismissTutorBadge = () => {
    localStorage.setItem('admin_seen_verifications', String(pendingCounts.pendingVerifications));
    setShowTutorBadge(false);
  };

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

          <button
            className="burger-btn"
            onClick={() => setMenuOpen(!menuOpen)}
            aria-label="Toggle menu"
          >
            <span className={`burger-icon ${menuOpen ? 'open' : ''}`} />
          </button>

          <div className={`navbar-collapse ${menuOpen ? 'show' : ''}`}>
            <ul className="navbar-nav" style={{ marginLeft: '1.5rem' }}>
              <li>
                <NavLink to="/" end onClick={() => setMenuOpen(false)}>
                  {t.admin.nav.dashboard}
                </NavLink>
              </li>
              <li>
                <NavLink to="/questions" onClick={() => setMenuOpen(false)}>
                  {t.admin.nav.questions}
                </NavLink>
              </li>
              <li>
                <NavLink to="/content" onClick={() => setMenuOpen(false)}>
                  {t.admin.nav.content}
                </NavLink>
              </li>
              <li>
                <NavLink to="/users" onClick={() => setMenuOpen(false)}>
                  {t.admin.nav.users}
                </NavLink>
              </li>
              <li>
                <NavLink to="/tutors" onClick={() => { setMenuOpen(false); dismissTutorBadge(); }} style={{ position: 'relative' }}>
                  {t.admin.nav.tutors}
                  {showTutorBadge && (
                    <span style={{
                      position: 'absolute', top: '-4px', right: '-10px',
                      background: '#ef4444', color: '#fff', fontSize: '0.6rem', fontWeight: 700,
                      borderRadius: '999px', minWidth: '16px', height: '16px',
                      display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
                      padding: '0 4px',
                    }}>{pendingCounts.pendingVerifications}</span>
                  )}
                </NavLink>
              </li>
              <li ref={moreRef} style={{ position: 'relative' }}>
                <NavLink
                  to="#"
                  onClick={(e) => { e.preventDefault(); setMoreOpen(!moreOpen); }}
                  className={({ isActive: _unused }) => ''}
                  style={{
                    gap: '0.25rem',
                    position: 'relative',
                  }}
                >
                  {t.admin.nav.more} <span style={{ fontSize: '0.55rem', opacity: 0.6 }}>▼</span>
                  {showSchoolBadge && (
                    <span style={{
                      position: 'absolute', top: '-4px', right: '-10px',
                      background: '#ef4444', color: '#fff', fontSize: '0.6rem', fontWeight: 700,
                      borderRadius: '999px', minWidth: '16px', height: '16px',
                      display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
                      padding: '0 4px',
                    }}>{pendingCounts.pendingSchools}</span>
                  )}
                </NavLink>
                {moreOpen && (
                  <div style={{
                    position: 'absolute', top: '100%', right: 0, minWidth: '170px',
                    background: 'var(--card-background)', border: '1px solid var(--border-color)',
                    borderRadius: '8px', boxShadow: '0 4px 16px rgba(0,0,0,0.18)',
                    zIndex: 100, padding: '0.35rem 0', marginTop: '0.25rem',
                  }}>
                    {[
                      { label: t.admin.nav.import, path: '/import' },
                      { label: t.admin.nav.questionImport, path: '/question-import' },
                      { label: t.admin.nav.schools, path: '/schools' },
                      { label: t.admin.nav.advisorConfig, path: '/advisor-config' },
                      { label: t.admin.nav.audit, path: '/audit' },
                      { label: t.admin.nav.health, path: '/health' },
                      { label: t.admin.nav.trash, path: '/trash' },
                    ].map(item => (
                      <NavLink
                        key={item.path}
                        to={item.path}
                        onClick={() => { setMoreOpen(false); setMenuOpen(false); if (item.path === '/schools') dismissSchoolBadge(); }}
                        style={({ isActive }) => ({
                          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                          padding: '0.5rem 1rem', fontSize: '0.88rem',
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
              <LanguageSwitcher />
              <button
                onClick={toggleTheme}
                className="theme-toggle"
                title={theme === 'light' ? t.nav.darkMode : t.nav.lightMode}
              >
                {theme === 'light' ? '◑' : '○'}
              </button>
              <NavLink to="/profile" style={{ color: 'var(--text-secondary)', textDecoration: 'none' }} onClick={() => setMenuOpen(false)}>
                {user?.name}
              </NavLink>
              <button onClick={handleLogout} className="btn btn-outline">
                {t.nav.logout}
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

export default AdminLayout;
