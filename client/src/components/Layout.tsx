import { Outlet, NavLink } from 'react-router-dom';
import { useState } from 'react';
import ProfileDropdown from './ProfileDropdown';
import GuidedTour from './GuidedTour';
import LanguageSwitcher from './LanguageSwitcher';
import { useTranslation } from '../hooks/useTranslation';

function Layout() {
  const { t } = useTranslation();
  const [menuOpen, setMenuOpen] = useState(false);

  return (
    <div className="layout">
      <GuidedTour />
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
