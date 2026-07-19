import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useState, useRef, useEffect } from 'react';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTheme } from '../hooks/useTheme';
import { useTranslation } from '../hooks/useTranslation';
import { logout } from '../store/slices/authSlice';
import LanguageSwitcher from './LanguageSwitcher';

function AdminLayout() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { theme, toggleTheme } = useTheme();
  const { t, locale } = useTranslation();
  const [moreOpen, setMoreOpen] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);
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
                <NavLink to="/mocks" onClick={() => setMenuOpen(false)}>
                  {locale === 'en' ? 'Mock Exams' : locale === 'kz' ? 'Сынақтар' : 'Пробники'}
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
                </NavLink>
                {moreOpen && (
                  <div style={{
                    position: 'absolute', top: '100%', right: 0, minWidth: '170px',
                    background: 'var(--card-background)', border: '1px solid var(--border-color)',
                    borderRadius: '8px', boxShadow: '0 4px 16px rgba(0,0,0,0.18)',
                    zIndex: 100, padding: '0.35rem 0', marginTop: '0.25rem',
                  }}>
                    {[
                      { label: t.admin.nav.news, path: '/news' },
                      { label: t.admin.nav.support, path: '/support' },
                      { label: t.admin.nav.mocks, path: '/mocks' },
                      { label: t.admin.nav.pricing, path: '/pricing' },
                      { label: 'Даты экзаменов', path: '/exam-dates' },
                      { label: t.admin.nav.sales, path: '/sales' },
                      { label: t.admin.nav.import, path: '/import' },
                      { label: t.admin.nav.audit, path: '/audit' },
                      { label: t.admin.nav.health, path: '/health' },
                      { label: t.admin.nav.backups, path: '/backups' },
                      { label: t.admin.nav.legal, path: '/legal' },
                      { label: t.admin.nav.trash, path: '/trash' },
                    ].map(item => (
                      <NavLink
                        key={item.path}
                        to={item.path}
                        onClick={() => { setMoreOpen(false); setMenuOpen(false); }}
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
