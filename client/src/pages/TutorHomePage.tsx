import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { tutorService } from '../services/tutorService';
import { messageService } from '../services/messageService';
import { useBranding } from '../contexts/BrandingContext';
import api from '../services/api';
import type { TutorProfileDetail, PendingRequest, TutorSchoolCard } from '../types';
import { getDateLocale, useTranslation } from '../i18n';

function TutorHomePage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { isWhiteLabel } = useBranding();
  const [profile, setProfile] = useState<TutorProfileDetail | null>(null);
  const [pending, setPending] = useState<PendingRequest[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<number | null>(null);
  const [schools, setSchools] = useState<TutorSchoolCard[]>([]);
  const [myApps, setMyApps] = useState<{ id: number; schoolId: number; schoolName: string; status: string }[]>([]);
  const [applyingTo, setApplyingTo] = useState<number | null>(null);
  const [applyMsg, setApplyMsg] = useState('');
  const [applyError, setApplyError] = useState<string | null>(null);
  const [requestingVerification, setRequestingVerification] = useState(false);

  const loadData = useCallback(async () => {
    try {
      const [pendingReqs, unread] = await Promise.all([
        tutorService.getPendingRequests(),
        messageService.getUnreadCount(),
      ]);
      setPending(pendingReqs);
      setUnreadCount(unread);

      // Load own profile — get userId from first pending or from profile endpoint
      try {
        // We'll try to load own tutor profile via the current user's ID
        // Since we don't have it directly, let's just show fallback on failure
      } catch { /* ignore */ }
    } catch (err) {
      console.error('Failed to load dashboard data:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  // Load tutor profile separately — need current user ID
  useEffect(() => {
    const loadProfile = async () => {
      try {
        const userStr = localStorage.getItem('user');
        if (userStr) {
          const user = JSON.parse(userStr);
          const data = await tutorService.getTutorProfile(user.id);
          setProfile(data);

          // Load partner schools for free tutors (no school, not on WL)
          if (!data.schoolId && !isWhiteLabel) {
            const [schoolsList, appsResp] = await Promise.all([
              tutorService.getSchools(),
              api.get<{ id: number; schoolId: number; schoolName: string; status: string }[]>('/tutor-school-applications/my').catch(() => ({ data: [] })),
            ]);
            setSchools(schoolsList);
            setMyApps(appsResp.data);
          }
        }
      } catch { /* ignore — profile might not exist yet */ }
    };
    loadProfile();
    loadData();
  }, [loadData, isWhiteLabel]);

  const handleApplyToSchool = async (schoolId: number) => {
    setApplyError(null);
    try {
      await api.post('/tutor-school-applications', { schoolId, message: applyMsg || undefined });
      setApplyingTo(null);
      setApplyMsg('');
      const r = await api.get('/tutor-school-applications/my');
      setMyApps(r.data);
    } catch (e: any) {
      setApplyError(e.response?.data?.error || 'Error');
    }
  };

  const handleRequestVerification = async () => {
    setRequestingVerification(true);
    try {
      await api.post('/tutors/request-verification');
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        const data = await tutorService.getTutorProfile(user.id);
        setProfile(data);
      }
    } catch { /* ignore */ } finally {
      setRequestingVerification(false);
    }
  };

  const handleAccept = async (conversationId: number) => {
    setActionLoading(conversationId);
    try {
      await tutorService.acceptStudent(conversationId);
      setPending(prev => prev.filter(p => p.conversationId !== conversationId));
    } catch {
      alert(t.tutor.acceptError);
    } finally {
      setActionLoading(null);
    }
  };

  const handleDecline = async (conversationId: number) => {
    const reason = prompt(t.tutor.declineReasonPrompt);
    setActionLoading(conversationId);
    try {
      await tutorService.declineStudent(conversationId, reason || undefined);
      setPending(prev => prev.filter(p => p.conversationId !== conversationId));
    } catch {
      alert(t.tutor.declineError);
    } finally {
      setActionLoading(null);
    }
  };

  const formatDate = (dateStr: string) => {
    const d = new Date(dateStr);
    return d.toLocaleDateString(getDateLocale(), { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });
  };

  if (loading) {
    return <div className="animate-fade-in" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>{t.schoolAdmin.loading}</div>;
  }

  return (
    <div className="animate-fade-in">
      <h1 style={{ marginBottom: '1.5rem' }}>{t.tutor.home}</h1>

      {/* Stat cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))', gap: '1rem', marginBottom: '1.5rem' }}>
        {[
          { label: t.tutor.newRequests, value: pending.length, icon: '', color: '#f59e0b', onClick: () => {} },
          { label: t.tutor.unreadMessages, value: unreadCount, icon: '', color: '#3b82f6', onClick: () => navigate('/messages') },
          { label: t.tutor.rating, value: profile ? profile.averageRating.toFixed(1) : '—', icon: '', color: '#10b981', onClick: () => navigate('/reviews') },
          { label: t.tutor.studentsCount, value: profile?.totalStudents ?? 0, icon: '', color: '#8b5cf6', onClick: () => navigate('/students') },
        ].map((stat, i) => (
          <div
            key={i}
            className="card"
            onClick={stat.onClick}
            style={{ cursor: 'pointer', textAlign: 'center', padding: '1.25rem' }}
          >
            <div style={{ fontSize: '2rem', marginBottom: '0.3rem' }}>{stat.icon}</div>
            <div style={{ fontSize: '1.5rem', fontWeight: 700, color: stat.color }}>{stat.value}</div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{stat.label}</div>
          </div>
        ))}
      </div>

      {/* Pending requests */}
      <div className="card" style={{ marginBottom: '1rem' }}>
        <h2 style={{ margin: '0 0 1rem', fontSize: '1.15rem' }}>
          {t.tutor.newRequests} {pending.length > 0 && <span style={{
            background: '#f59e0b', color: '#fff', borderRadius: '999px',
            padding: '0.15rem 0.5rem', fontSize: '0.75rem', fontWeight: 700, marginLeft: '0.5rem',
          }}>{pending.length}</span>}
        </h2>

        {pending.length === 0 ? (
          <p style={{ color: 'var(--text-secondary)', margin: 0 }}>{t.tutor.noNewRequests}</p>
        ) : (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {pending.map(req => (
              <div
                key={req.conversationId}
                style={{
                  padding: '1rem', borderRadius: '10px',
                  background: 'var(--bg-secondary)', border: '1px solid var(--border-color)',
                  display: 'flex', flexWrap: 'wrap', gap: '0.75rem', alignItems: 'center',
                }}
              >
                <div style={{
                  width: '42px', height: '42px', borderRadius: '50%',
                  background: 'linear-gradient(135deg, #f59e0b, #d97706)',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  color: '#fff', fontWeight: 700, fontSize: '0.85rem', flexShrink: 0,
                }}>
                  {req.studentName.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2)}
                </div>

                <div style={{ flex: 1, minWidth: '150px' }}>
                  <div style={{ fontWeight: 600 }}>{req.studentName}</div>
                  <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                    {req.studentEmail} · {formatDate(req.requestedAt)}
                  </div>
                  {req.requestMessage && (
                    <div style={{
                      marginTop: '0.3rem', fontSize: '0.85rem',
                      color: 'var(--text-primary)', fontStyle: 'italic',
                      background: 'var(--bg-primary)', padding: '0.4rem 0.6rem',
                      borderRadius: '6px', borderLeft: '3px solid var(--primary-color)',
                    }}>
                      "{req.requestMessage}"
                    </div>
                  )}
                </div>

                <div style={{ display: 'flex', gap: '0.5rem', flexShrink: 0 }}>
                  <button
                    className="btn btn-primary"
                    onClick={() => handleAccept(req.conversationId)}
                    disabled={actionLoading === req.conversationId}
                    style={{ padding: '0.4rem 1rem', fontSize: '0.85rem' }}
                  >
                    {t.tutor.accept}
                  </button>
                  <button
                    className="btn btn-outline"
                    onClick={() => handleDecline(req.conversationId)}
                    disabled={actionLoading === req.conversationId}
                    style={{ padding: '0.4rem 1rem', fontSize: '0.85rem' }}
                  >
                    {t.tutor.decline}
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Quick info */}
      {/* Verification banner */}
      {profile && !profile.isVerified && (
        <div className="card" style={{
          padding: '1rem 1.25rem', marginBottom: '1rem',
          border: '1px solid',
          borderColor: profile.verificationRequestedAt ? '#f59e0b44' : '#3b82f644',
          background: profile.verificationRequestedAt ? '#f59e0b11' : '#3b82f611',
        }}>
          {profile.verificationRequestedAt ? (
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <span style={{ fontSize: '1.1rem' }}>&#9203;</span>
              <div>
                <div style={{ fontWeight: 600 }}>{t.tutorVerification.pendingTitle}</div>
                <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', marginTop: '0.15rem' }}>
                  {t.tutorVerification.pendingDesc}
                </div>
              </div>
            </div>
          ) : (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: '1rem', flexWrap: 'wrap' }}>
              <div>
                <div style={{ fontWeight: 600 }}>{t.tutorVerification.notVerifiedTitle}</div>
                <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', marginTop: '0.15rem' }}>
                  {t.tutorVerification.notVerifiedDesc}
                </div>
              </div>
              <button
                className="btn btn-primary"
                style={{ whiteSpace: 'nowrap', fontSize: '0.85rem' }}
                onClick={handleRequestVerification}
                disabled={requestingVerification}
              >{t.tutorVerification.requestBtn}</button>
            </div>
          )}
        </div>
      )}

      {profile && (
        <div className="card">
          <h2 style={{ margin: '0 0 0.75rem', fontSize: '1.15rem' }}>{t.tutor.myProfile}</h2>
          <div style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap', fontSize: '0.9rem' }}>
            <div><strong>{t.tutor.headline}:</strong> {profile.headline}</div>
            <div><strong>{t.tutor.statusLabel}:</strong> {profile.isAvailable ? t.tutor.available : t.tutor.unavailable}</div>
            {profile.isVerified && <div style={{ color: 'var(--success-color)', fontWeight: 600 }}>{t.tutor.verified}</div>}
          </div>
          <button
            className="btn btn-outline"
            onClick={() => navigate('/my-profile')}
            style={{ marginTop: '0.75rem', fontSize: '0.85rem' }}
          >
            {t.tutor.editProfile}
          </button>
        </div>
      )}

      {/* Partner Schools — for free tutors not bound to any school */}
      {!isWhiteLabel && profile && !profile.schoolId && schools.length > 0 && (
        <div className="card" style={{ marginTop: '1rem' }}>
          <h2 style={{ margin: '0 0 0.5rem', fontSize: '1.15rem' }}>{t.tutor.partnerSchools}</h2>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', margin: '0 0 1rem' }}>
            {t.tutor.partnerSchoolsDesc}
          </p>

          {/* My applications */}
          {myApps.length > 0 && (
            <div style={{ marginBottom: '1rem' }}>
              <h4 style={{ margin: '0 0 0.5rem', fontSize: '0.95rem' }}>{t.tutor.myApplications}</h4>
              {myApps.map(a => (
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
                    {a.status === 'Pending' ? t.tutor.appPending : a.status === 'Approved' ? t.tutor.appApproved : t.tutor.appRejected}
                  </span>
                </div>
              ))}
            </div>
          )}

          {/* Available schools to apply to */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '0.75rem' }}>
            {schools.filter(s => !myApps.some(a => a.schoolId === s.id)).map(s => (
              <div key={s.id} style={{
                padding: '1rem', borderRadius: '10px', background: 'var(--bg-secondary)',
                border: '1px solid var(--border-color)',
              }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', marginBottom: '0.5rem' }}>
                  <strong>{s.name}</strong>
                  {s.isPartner && <span style={{ fontSize: '0.7rem', background: 'var(--primary-color)', color: '#fff', padding: '0.1rem 0.4rem', borderRadius: '999px' }}>{t.tutor.partner}</span>}
                </div>
                {s.description && <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>{s.description.slice(0, 100)}{s.description.length > 100 ? '...' : ''}</div>}
                <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)', marginBottom: '0.75rem' }}>
                  {s.tutorCount} {t.tutor.tutorsCount}
                  {s.specializations && s.specializations.length > 0 && ` · ${s.specializations.join(', ')}`}
                </div>
                {applyingTo === s.id ? (
                  <div style={{ display: 'flex', gap: '0.35rem', alignItems: 'center', flexWrap: 'wrap' }}>
                    <input
                      value={applyMsg}
                      onChange={e => setApplyMsg(e.target.value)}
                      placeholder={t.tutor.messagePlaceholder}
                      style={{ flex: 1, minWidth: '120px', fontSize: '0.82rem', padding: '0.35rem 0.5rem', borderRadius: '6px', border: '1px solid var(--border-color)', background: 'var(--bg-primary)', color: 'var(--text-primary)' }}
                    />
                    <button className="btn btn-primary" style={{ fontSize: '0.78rem', padding: '0.35rem 0.7rem' }} onClick={() => handleApplyToSchool(s.id)}>
                      {t.tutor.send}
                    </button>
                    <button className="btn btn-outline" style={{ fontSize: '0.78rem', padding: '0.35rem 0.5rem' }} onClick={() => { setApplyingTo(null); setApplyMsg(''); setApplyError(null); }}>
                      {t.tutor.cancel}
                    </button>
                  </div>
                ) : (
                  <button className="btn btn-primary" style={{ fontSize: '0.82rem', padding: '0.35rem 0.9rem' }} onClick={() => setApplyingTo(s.id)}>
                    {t.tutor.applyToJoin}
                  </button>
                )}
              </div>
            ))}
          </div>
          {applyError && <p style={{ color: '#ef4444', fontSize: '0.8rem', marginTop: '0.5rem' }}>{applyError}</p>}
        </div>
      )}
    </div>
  );
}

export default TutorHomePage;
