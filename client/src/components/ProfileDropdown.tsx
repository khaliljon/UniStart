import { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useAppSelector } from '../hooks/useAppSelector';
import { useTheme } from '../hooks/useTheme';
import { useTranslation } from '../i18n';
import { logout } from '../store/slices/authSlice';
import { messageService } from '../services/messageService';
import { chatService } from '../services/chatService';

function ProfileDropdown() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { theme, toggleTheme } = useTheme();
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [unreadCount, setUnreadCount] = useState(0);
  const ref = useRef<HTMLDivElement>(null);

  // Close on outside click
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  // Unread messages tracking
  useEffect(() => {
    messageService.getUnreadCount().then(setUnreadCount).catch(() => {});
    chatService.start();
    const unsub = chatService.onUnreadCount((count) => setUnreadCount(count));
    const interval = setInterval(() => {
      messageService.getUnreadCount().then(setUnreadCount).catch(() => {});
    }, 60000);
    return () => { unsub(); clearInterval(interval); };
  }, []);

  const handleLogout = () => {
    setOpen(false);
    chatService.stop();
    dispatch(logout());
    navigate('/login');
  };

  const goTo = (path: string) => {
    setOpen(false);
    navigate(path);
  };

  const initials = user?.name
    ? user.name.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2)
    : '?';

  return (
    <div ref={ref} style={{ position: 'relative' }}>
      {/* Trigger button */}
      <button
        onClick={() => setOpen(!open)}
        className="profile-trigger"
        title={t.nav.profile}
      >
        <span className="profile-avatar" style={{ position: 'relative' }}>
          {initials}
          {unreadCount > 0 && (
            <span style={{
              position: 'absolute', top: '-2px', right: '-4px',
              width: '8px', height: '8px', borderRadius: '50%',
              background: '#ef4444', border: '2px solid var(--bg-primary)',
            }} />
          )}
        </span>
        <span className="profile-name">{user?.name}</span>
        <span style={{ fontSize: '0.6rem', marginLeft: '0.2rem', opacity: 0.6 }}>▼</span>
      </button>

      {/* Dropdown */}
      {open && (
        <div className="profile-dropdown">
          {/* User info header */}
          <div className="profile-dropdown-header">
            <span className="profile-avatar" style={{ width: '36px', height: '36px', fontSize: '0.85rem' }}>
              {initials}
            </span>
            <div>
              <div style={{ fontWeight: 600, fontSize: '0.9rem' }}>{user?.name}</div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{user?.email}</div>
              {user?.subscriptionTier === 'Pro' && (
                <span style={{
                  fontSize: '0.65rem', fontWeight: 700, color: '#fff',
                  background: 'linear-gradient(135deg, #f59e0b, #ef4444)',
                  padding: '0.1rem 0.4rem', borderRadius: '999px', marginTop: '0.15rem',
                  display: 'inline-block',
                }}>PRO</span>
              )}
            </div>
          </div>

          <div className="profile-dropdown-divider" />

          {/* Menu items */}
          <button className="profile-dropdown-item" onClick={() => goTo('/profile')}>
            {t.nav.myInfo}
          </button>
          <button className="profile-dropdown-item" onClick={() => goTo('/purchases')}>
            {t.nav.myPurchases}
          </button>
          <button className="profile-dropdown-item" onClick={() => goTo('/progress')}>
            {t.nav.myProgress}
          </button>
          <button className="profile-dropdown-item" onClick={() => goTo('/messages')}>
            <span style={{ display: 'inline-flex', alignItems: 'center', gap: '0.3rem' }}>
              {t.nav.messages}
              {unreadCount > 0 && (
                <span style={{
                  background: '#ef4444', color: '#fff', borderRadius: '999px',
                  padding: '0.05rem 0.35rem', fontSize: '0.65rem', fontWeight: 700, lineHeight: 1.2,
                }}>{unreadCount > 99 ? '99+' : unreadCount}</span>
              )}
            </span>
          </button>
          <button className="profile-dropdown-item" onClick={() => goTo('/profile/notifications')}>
            {t.nav.notifications}
          </button>

          <div className="profile-dropdown-divider" />

          <button className="profile-dropdown-item" onClick={() => { toggleTheme(); }}>
            {theme === 'light' ? t.nav.darkMode : t.nav.lightMode}
          </button>

          <div className="profile-dropdown-divider" />

          <button className="profile-dropdown-item profile-dropdown-danger" onClick={handleLogout}>
            {t.nav.logout}
          </button>
        </div>
      )}
    </div>
  );
}

export default ProfileDropdown;
