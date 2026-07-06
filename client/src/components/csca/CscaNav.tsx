import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from '../../i18n';
import { cscaStrings } from '../../i18n/csca';
import LanguageSwitcher from '../LanguageSwitcher';

/** Public navigation bar for the CSCA marketing pages. Links to real routes. */
export default function CscaNav() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];
  const [menuOpen, setMenuOpen] = useState(false);

  const links: { to: string; label: string }[] = [
    { to: '/csca/about', label: s.navAbout },
    { to: '/landing#news', label: s.navNews },
    { to: '/csca/mocks', label: s.navMocks },
    { to: '/csca/materials', label: s.navMaterials },
    { to: '/csca/about-us', label: s.navAboutUs },
    { to: '/csca/contacts', label: s.navContacts },
  ];

  return (
    <nav className="csca-nav">
      <div className="csca-nav-inner">
        <Link className="csca-brand" to="/landing" onClick={() => setMenuOpen(false)}>
          <img src="/unistart-logo.png" alt="UniStart" className="csca-brand-logo" />
          <span className="csca-brand-sub csca-hanzi csca-brand-tagline">专注 CSCA 备考</span>
        </Link>

        <div className={`csca-nav-links ${menuOpen ? 'open' : ''}`}>
          {links.map((n) => (
            <Link key={n.to} className="csca-nav-link" to={n.to} onClick={() => setMenuOpen(false)}>
              {n.label}
            </Link>
          ))}
        </div>

        <div className="csca-nav-actions">
          <LanguageSwitcher />
          <button className="csca-btn csca-btn-ghost csca-btn-sm" onClick={() => navigate('/login')}>{s.login}</button>
          <button className="csca-btn csca-btn-primary csca-btn-sm" onClick={() => navigate('/register')}>{s.cabinet}</button>
          <button className="csca-burger" onClick={() => setMenuOpen((o) => !o)} aria-label="Menu">
            <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
              {menuOpen ? <path d="M6 6l12 12M18 6L6 18" /> : <path d="M4 7h16M4 12h16M4 17h16" />}
            </svg>
          </button>
        </div>
      </div>
    </nav>
  );
}
