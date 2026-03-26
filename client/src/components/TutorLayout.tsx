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
import { useTranslation } from '../i18n';
import api from '../services/api';
import type { TutorSchoolCard } from '../types';

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
  const { t } = useTranslation();
  const [tutorBlocked, setTutorBlocked] = useState(false);
  const [gateSchools, setGateSchools] = useState<TutorSchoolCard[]>([]);
  const [gateApps, setGateApps] = useState<{ id: number; schoolId: number; schoolName: string; status: string }[]>([]);
  const [applyMsg, setApplyMsg] = useState('');
  const [applyingTo, setApplyingTo] = useState<number | null>(null);
  const [applyError, setApplyError] = useState<string | null>(null);

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

    // Tutor verification gate: block unverified tutors everywhere
    if (user && user.role === 'Tutor') {
      tutorService.getTutorProfile(user.id).then(profile => {
        if (!profile.isVerified) {
          setTutorBlocked(true);
          // Load schools list and my applications for the gate UI
          tutorService.getSchools().then(s => setGateSchools(s)).catch(() => {});
          api.get('/tutor-school-applications/my').then(r => setGateApps(r.data)).catch(() => {});
        }
      }).catch(() => setTutorBlocked(true));
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

  // Tutor verification gate
  if (tutorBlocked) {
    const hasPending = gateApps.some(a => a.status === 'Pending');
    const appliedSchoolIds = new Set(gateApps.map(a => a.schoolId));

    const handleApply = async (schoolId: number) => {
      setApplyError(null);
      try {
        await api.post('/tutor-school-applications', { schoolId, message: applyMsg || undefined });
        setApplyingTo(null);
        setApplyMsg('');
        const r = await api.get('/tutor-school-applications/my');
        setGateApps(r.data);
      } catch (e: any) {
        setApplyError(e.response?.data?.error || 'Error');
      }
    };

    return (
      <div className="layout" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '100vh', padding: '1rem' }}>
        <div className="card" style={{ padding: '2.5rem', maxWidth: '600px', width: '100%' }}>
          <h2 style={{ marginBottom: '0.5rem', textAlign: 'center' }}>
            {isWhiteLabel ? (branding?.navbarTitle || branding?.name || 'School') : 'UniStart'}
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem', lineHeight: 1.5, textAlign: 'center' }}>
            {hasPending ? t.tutorGate.pendingDesc : t.tutorGate.description}
          </p>

          {/* My applications */}
          {gateApps.length > 0 && (
            <div style={{ marginBottom: '1.5rem' }}>
              <h4 style={{ marginBottom: '0.5rem' }}>{t.tutorGate.myApps}</h4>
              {gateApps.map(a => (
                <div key={a.id} style={{
                  display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                  padding: '0.5rem 0.75rem', borderRadius: '8px', background: 'var(--bg-secondary)', marginBottom: '0.4rem',
                }}>
                  <span style={{ fontWeight: 500 }}>{a.schoolName}</span>
                  <span style={{
                    fontSize: '0.75rem', fontWeight: 600, padding: '0.15rem 0.5rem', borderRadius: '999px',
                    background: a.status === 'Pending' ? '#f59e0b22' : a.status === 'Approved' ? '#10b98122' : '#ef444422',
                    color: a.status === 'Pending' ? '#f59e0b' : a.status === 'Approved' ? '#10b981' : '#ef4444',
                  }}>
                    {a.status === 'Pending' ? t.tutorGate.statusPending : a.status === 'Approved' ? t.tutorGate.statusApproved : t.tutorGate.statusRejected}
                  </span>
                </div>
              ))}
            </div>
          )}

          {/* Available schools to apply */}
          {gateSchools.filter(s => !appliedSchoolIds.has(s.id)).length > 0 && (
            <div style={{ marginBottom: '1.5rem' }}>
              <h4 style={{ marginBottom: '0.5rem' }}>{t.tutorGate.availableSchools}</h4>
              {gateSchools.filter(s => !appliedSchoolIds.has(s.id)).map(s => (
                <div key={s.id} style={{
                  display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                  padding: '0.5rem 0.75rem', borderRadius: '8px', background: 'var(--bg-secondary)', marginBottom: '0.4rem',
                }}>
                  <div>
                    <span style={{ fontWeight: 500 }}>{s.name}</span>
                    {s.description && <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '0.15rem' }}>{s.description.slice(0, 80)}{s.description.length > 80 ? '...' : ''}</div>}
                  </div>
                  {applyingTo === s.id ? (
                    <div style={{ display: 'flex', gap: '0.3rem', alignItems: 'center' }}>
                      <input
                        value={applyMsg}
                        onChange={e => setApplyMsg(e.target.value)}
                        placeholder={t.tutorGate.messagePlaceholder}
                        style={{ fontSize: '0.8rem', padding: '0.3rem 0.5rem', borderRadius: '6px', border: '1px solid var(--border)', width: '140px' }}
                      />
                      <button className="btn btn-primary" style={{ fontSize: '0.75rem', padding: '0.3rem 0.6rem' }} onClick={() => handleApply(s.id)}>
                        {t.tutorGate.send}
                      </button>
                      <button className="btn" style={{ fontSize: '0.75rem', padding: '0.3rem 0.6rem' }} onClick={() => { setApplyingTo(null); setApplyMsg(''); }}>
                        &times;
                      </button>
                    </div>
                  ) : (
                    <button className="btn btn-primary" style={{ fontSize: '0.75rem', padding: '0.3rem 0.8rem' }} onClick={() => setApplyingTo(s.id)}>
                      {t.tutorGate.applyBtn}
                    </button>
                  )}
                </div>
              ))}
              {applyError && <p style={{ color: '#ef4444', fontSize: '0.8rem', marginTop: '0.5rem' }}>{applyError}</p>}
            </div>
          )}

          <div style={{ textAlign: 'center' }}>
            <button className="btn btn-primary" onClick={handleLogout}>
              {t.tutorGate.logoutBtn}
            </button>
          </div>
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
            {!isWhiteLabel && user?.role === 'SchoolAdmin' && (
              <li>
                <NavLink to="/school">
                  Школа
                </NavLink>
              </li>
            )}
            {!isWhiteLabel && user?.role === 'SchoolAdmin' && hasSchool && (
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
