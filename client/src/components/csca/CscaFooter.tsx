import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from '../../i18n';
import { cscaStrings } from '../../i18n/csca';
import { PagodaMark } from './ChineseMotifs';

/** Public footer for the CSCA marketing pages. */
export default function CscaFooter() {
  const navigate = useNavigate();
  const { locale } = useTranslation();
  const s = cscaStrings[locale];

  return (
    <footer className="csca-footer">
      <div className="csca-wrap">
        <div className="csca-footer-grid">
          <div>
            <div className="csca-brand" style={{ marginBottom: '0.9rem' }}>
              <PagodaMark style={{ height: 36, width: 36, color: '#fff' }} />
              <span className="csca-brand-text" style={{ color: '#fff' }}>UNISTART</span>
            </div>
            <p style={{ fontSize: '0.9rem', lineHeight: 1.6, maxWidth: 320 }}>{s.footerDesc}</p>
          </div>
          <div>
            <h4>{s.footerPlatform}</h4>
            <Link to="/csca/mocks">{s.navMocks}</Link>
            <Link to="/csca/materials">{s.navMaterials}</Link>
            <Link to="/csca/courses">{s.navCourses}</Link>
          </div>
          <div>
            <h4>{s.footerAbout}</h4>
            <Link to="/csca/about">{s.navAbout}</Link>
            <Link to="/csca/about-us">{s.navAboutUs}</Link>
            <a onClick={() => navigate('/register')} style={{ cursor: 'pointer' }}>{s.cabinet}</a>
          </div>
          <div>
            <h4>{s.footerContacts}</h4>
            <Link to="/csca/contacts">{s.navContacts}</Link>
            <a href="mailto:unistart.kz@gmail.com">unistart.kz@gmail.com</a>
            <a href="https://t.me/unistart" target="_blank" rel="noopener noreferrer">Telegram</a>
          </div>
        </div>
        <div className="csca-footer-bottom">
          <span>© {new Date().getFullYear()} UniStart. {s.footerRights}</span>
          <span className="csca-hanzi">学无止境</span>
        </div>
      </div>
    </footer>
  );
}
