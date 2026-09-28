import { Outlet, NavLink } from 'react-router-dom';
import { useState, useEffect } from 'react';
import ProfileDropdown from './ProfileDropdown';
import LanguageSwitcher from './LanguageSwitcher';
import { useTranslation } from '../hooks/useTranslation';
import { cartService } from '../services/cartService';

function Layout() {
  const { t } = useTranslation();
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => { cartService.sync(); }, []);

  return (
    <div className="layout">
      <nav className="navbar">
        <div className="container navbar-content">
          <NavLink to="/" className="navbar-brand" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <img src="/favicon-256.png" alt="UniStart" style={{ height: 34, width: 34 }} />
            <span style={{ fontFamily: 'var(--font-display, inherit)', fontWeight: 800, fontSize: '1.15rem', color: 'var(--text-primary)' }}>UniStart</span>
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
                  {t.nav.home}
                </NavLink>
              </li>
              <li>
                <NavLink to="/news" onClick={() => setMenuOpen(false)}>
                  {t.nav.news}
                </NavLink>
              </li>
              <li>
                <NavLink to="/learn?tab=mock" onClick={() => setMenuOpen(false)}>
                  {t.nav.exams}
                </NavLink>
              </li>
              <li>
                <NavLink to="/materials" onClick={() => setMenuOpen(false)}>
                  {t.nav.materials}
                </NavLink>
              </li>
            </ul>

            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <LanguageSwitcher />
              <ProfileDropdown />
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

export default Layout;
