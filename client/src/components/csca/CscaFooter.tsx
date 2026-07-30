import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from '../../i18n';
import { cscaStrings } from '../../i18n/csca';
import { SOCIAL_LINKS } from '../../socialLinks';

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
            <div className="csca-brand" style={{ marginBottom: '0.9rem', display: 'flex', alignItems: 'center', gap: '0.6rem' }}>
              <img src="/favicon-256.png" alt="UniStart" style={{ height: 40, width: 40 }} />
              <span className="csca-brand-text" style={{ color: '#fff' }}>UNISTART</span>
            </div>
            <p style={{ fontSize: '0.9rem', lineHeight: 1.6, maxWidth: 320 }}>{s.footerDesc}</p>
          </div>
          <div>
            <h4>{s.footerPlatform}</h4>
            <Link to="/csca/mocks">{s.navMocks}</Link>
            <Link to="/csca/materials">{s.navMaterials}</Link>
            <Link to="/csca/news">{s.navNews}</Link>
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
            <a href={`mailto:${SOCIAL_LINKS.email}`}>{SOCIAL_LINKS.email}</a>
            <a href={SOCIAL_LINKS.telegramChannel} target="_blank" rel="noopener noreferrer">{s.socialTelegram}</a>
            <a href={SOCIAL_LINKS.supportBot} target="_blank" rel="noopener noreferrer">{s.socialSupportBot}</a>
            <a href={SOCIAL_LINKS.instagram} target="_blank" rel="noopener noreferrer">Instagram</a>
            <a href={SOCIAL_LINKS.tiktok} target="_blank" rel="noopener noreferrer">TikTok</a>
          </div>
        </div>
        <div className="csca-footer-bottom">
          <span>© {new Date().getFullYear()} UniStart. {s.footerRights}</span>
          <span style={{ display: 'flex', gap: '1.25rem', flexWrap: 'wrap' }}>
            <Link to="/terms">{s.footerTerms}</Link>
            <Link to="/privacy">{s.footerPrivacy}</Link>
          </span>
          <span className="csca-hanzi">学无止境</span>
        </div>
      </div>
    </footer>
  );
}
