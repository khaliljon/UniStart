import { useState, useEffect, useCallback } from 'react';
import type { CSSProperties } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import adminService from '../services/adminService';
import type { AdminSchoolBranding, AdminCreateSchool } from '../services/adminService';
import api from '../services/api';
import { getDateLocale } from '../i18n';

interface AdminSchool {
  id: number; name: string; slug: string; logoUrl: string | null;
  isPartner: boolean; isApproved: boolean; isActive: boolean;
  specializations: string; websiteUrl: string | null;
  subscriptionExpiresAt: string | null; subscriptionPaidAt: string | null;
  createdAt: string;
  ownerUserId: number | null; ownerName: string | null; ownerEmail: string | null;
  tutorCount: number; studentCount: number;
}

interface SchoolDetail {
  students: { total: number; items: Array<{ userId: number; name: string; email: string; subscriptionTier: string; createdAt: string; lastSeenAt: string | null }> };
  tutors: Array<{ userId: number; name: string; email: string; headline: string; isVerified: boolean; isAvailable: boolean; totalStudents: number; averageRating: number; role?: string }>;
}

const fieldLabel: CSSProperties = {
  display: 'flex', flexDirection: 'column', gap: '0.3rem',
  fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-secondary)',
};
const fieldInput: CSSProperties = {
  padding: '0.5rem 0.7rem', borderRadius: '0.5rem', border: '1px solid var(--border-color)',
  background: 'var(--bg-primary)', color: 'var(--text-primary)', fontSize: '0.9rem', fontWeight: 400,
};

function AdminSchoolsPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [schools, setSchools] = useState<AdminSchool[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<'all' | 'approved' | 'unapproved' | 'paid' | 'unpaid'>('all');
  const [selected, setSelected] = useState<AdminSchool | null>(null);
  const [detail, setDetail] = useState<SchoolDetail | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState<number | null>(null);

  // Branding editor state
  const [brandingForm, setBrandingForm] = useState<AdminSchoolBranding | null>(null);
  const [brandingLoading, setBrandingLoading] = useState(false);
  const [brandingSaving, setBrandingSaving] = useState(false);
  const [brandingError, setBrandingError] = useState<string | null>(null);

  // Create-school state
  const [createOpen, setCreateOpen] = useState(false);
  const [createForm, setCreateForm] = useState<AdminCreateSchool>({ name: '' });
  const [createSaving, setCreateSaving] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  const loadSchools = useCallback(async () => {
    try {
      const data = await adminService.getAllSchools();
      setSchools(data);
    } catch (err) {
      console.error('Failed to load schools:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadSchools(); }, [loadSchools]);

  const loadSchoolDetail = useCallback(async (school: AdminSchool) => {
    setSelected(school);
    setDetail(null);
    setDetailLoading(true);
    try {
      const [studentsRes, tutorsRes] = await Promise.all([
        api.get(`/admin/schools/${school.id}/students`, { params: { pageSize: 50 } }),
        api.get(`/admin/schools/${school.id}/tutors`),
      ]);
      setDetail({ students: studentsRes.data, tutors: tutorsRes.data });
    } catch { /* ignore */ } finally {
      setDetailLoading(false);
    }
  }, []);

  const isPaid = (s: AdminSchool) => s.subscriptionExpiresAt != null && new Date(s.subscriptionExpiresAt) > new Date();
  const formatDate = (d: string) => new Date(d).toLocaleDateString(getDateLocale(), { day: 'numeric', month: 'short', year: 'numeric' });

  const handleApprove = async (school: AdminSchool) => {
    setActionLoading(school.id);
    try {
      await adminService.approveSchool(school.id);
      setSchools(prev => prev.map(s => s.id === school.id ? { ...s, isApproved: true } : s));
      if (selected?.id === school.id) setSelected(prev => prev ? { ...prev, isApproved: true } : prev);
      window.dispatchEvent(new Event('admin-badge-refresh'));
    } catch { alert('Ошибка'); }
    finally { setActionLoading(null); }
  };

  const handleUnapprove = async (school: AdminSchool) => {
    setActionLoading(school.id);
    try {
      await adminService.unapproveSchool(school.id);
      setSchools(prev => prev.map(s => s.id === school.id ? { ...s, isApproved: false } : s));
      if (selected?.id === school.id) setSelected(prev => prev ? { ...prev, isApproved: false } : prev);
      window.dispatchEvent(new Event('admin-badge-refresh'));
    } catch { alert('Ошибка'); }
    finally { setActionLoading(null); }
  };

  const handleRestore = async (school: AdminSchool) => {
    setActionLoading(school.id);
    try {
      await adminService.restoreSchool(school.id);
      setSchools(prev => prev.map(s => s.id === school.id ? { ...s, isActive: true } : s));
      if (selected?.id === school.id) setSelected(prev => prev ? { ...prev, isActive: true } : prev);
      window.dispatchEvent(new Event('admin-badge-refresh'));
    } catch { alert('Ошибка'); }
    finally { setActionLoading(null); }
  };

  const handleToggleSubscription = async (school: AdminSchool) => {
    setActionLoading(school.id);
    try {
      if (isPaid(school)) {
        await adminService.deactivateSchoolSubscription(school.id);
        setSchools(prev => prev.map(s => s.id === school.id ? { ...s, subscriptionExpiresAt: null } : s));
        if (selected?.id === school.id) setSelected(prev => prev ? { ...prev, subscriptionExpiresAt: null } : prev);
      } else {
        const res = await adminService.activateSchoolSubscription(school.id);
        setSchools(prev => prev.map(s => s.id === school.id ? { ...s, subscriptionExpiresAt: res.subscriptionExpiresAt, subscriptionPaidAt: new Date().toISOString() } : s));
        if (selected?.id === school.id) setSelected(prev => prev ? { ...prev, subscriptionExpiresAt: res.subscriptionExpiresAt } : prev);
      }
    } catch { alert('Ошибка подписки'); }
    finally { setActionLoading(null); }
  };

  const handleDeleteSchool = useCallback(async (school: AdminSchool) => {
    if (!confirm(`${t.admin.common.deleteConfirm}: ${school.name}?`)) return;
    try {
      await adminService.deleteSchool(school.id);
      setSchools(prev => prev.filter(s => s.id !== school.id));
      setSelected(null); setDetail(null);
    } catch { alert(t.admin.common.deleteError); }
  }, [t]);

  // ─── Branding editor ──────────────────────────────────
  const openBranding = useCallback(async (schoolId: number) => {
    setBrandingForm(null);
    setBrandingError(null);
    setBrandingLoading(true);
    try {
      const data = await adminService.getSchoolBranding(schoolId);
      setBrandingForm(data);
    } catch {
      setBrandingError('Не удалось загрузить брендинг');
    } finally {
      setBrandingLoading(false);
    }
  }, []);

  const handleSaveBranding = async () => {
    if (!brandingForm) return;
    setBrandingSaving(true);
    setBrandingError(null);
    try {
      await adminService.updateSchoolBranding(brandingForm.id, {
        name: brandingForm.name,
        subdomain: brandingForm.subdomain,
        navbarTitle: brandingForm.navbarTitle,
        description: brandingForm.description,
        descriptionEn: brandingForm.descriptionEn,
        descriptionKz: brandingForm.descriptionKz,
        logoUrl: brandingForm.logoUrl,
        websiteUrl: brandingForm.websiteUrl,
        instagramUrl: brandingForm.instagramUrl,
        telegramUrl: brandingForm.telegramUrl,
        specializations: brandingForm.specializations,
        primaryColor: brandingForm.primaryColor,
        primaryHoverColor: brandingForm.primaryHoverColor,
        accentColor: brandingForm.accentColor,
      });
      setBrandingForm(null);
      await loadSchools();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setBrandingError(msg || 'Ошибка сохранения');
    } finally {
      setBrandingSaving(false);
    }
  };

  const handleCreateSchool = async () => {
    if (!createForm.name.trim()) { setCreateError('Укажите название'); return; }
    setCreateSaving(true);
    setCreateError(null);
    try {
      const res = await adminService.createSchool(createForm);
      setCreateOpen(false);
      setCreateForm({ name: '' });
      await loadSchools();
      // Open branding editor for the newly created school
      await openBranding(res.id);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message;
      setCreateError(msg || 'Ошибка создания');
    } finally {
      setCreateSaving(false);
    }
  };

  const sanitizeSubdomain = (v: string) => v.toLowerCase().replace(/[^a-z0-9-]/g, '');

  const filteredSchools = schools.filter(s => {
    if (filter === 'approved') return s.isApproved;
    if (filter === 'unapproved') return !s.isApproved;
    if (filter === 'paid') return isPaid(s);
    if (filter === 'unpaid') return !isPaid(s);
    return true;
  });

  if (loading) {
    return <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>{t.admin.common.loading}</div>;
  }

  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
        <h1 style={{ fontSize: '1.5rem', fontWeight: 700 }}>
          {t.admin.nav.schools}
          <span style={{ fontWeight: 400, fontSize: '1rem', color: 'var(--text-secondary)', marginLeft: '0.75rem' }}>{schools.length}</span>
        </h1>
        <span style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
          Верифицированы: {schools.filter(s => s.isApproved).length} | Оплачены: {schools.filter(isPaid).length}
        </span>
      </div>

      <div style={{ marginBottom: '1rem' }}>
        <button
          onClick={() => { setCreateForm({ name: '' }); setCreateError(null); setCreateOpen(true); }}
          className="btn"
          style={{ padding: '0.45rem 0.9rem', fontSize: '0.85rem', background: 'var(--primary-color)', color: '#fff', border: 'none' }}
        >
          + Создать школу (white-label)
        </button>
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
        {(['all', 'approved', 'unapproved', 'paid', 'unpaid'] as const).map(f => (
          <button
            key={f}
            onClick={() => setFilter(f)}
            className="btn"
            style={{
              padding: '0.4rem 0.8rem', fontSize: '0.85rem',
              background: filter === f ? 'var(--primary-color)' : undefined,
              color: filter === f ? '#fff' : undefined,
            }}
          >
            {f === 'all' ? 'Все' : f === 'approved' ? 'Верифицированы' : f === 'unapproved' ? 'Не верифицированы' : f === 'paid' ? 'Оплачены' : 'Не оплачены'}
            {' '}({schools.filter(s => {
              if (f === 'approved') return s.isApproved;
              if (f === 'unapproved') return !s.isApproved;
              if (f === 'paid') return isPaid(s);
              if (f === 'unpaid') return !isPaid(s);
              return true;
            }).length})
          </button>
        ))}
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: selected ? '1fr 1.2fr' : '1fr', gap: '1.5rem' }}>
        {/* Schools list */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {filteredSchools.length === 0 ? (
            <div className="card" style={{ textAlign: 'center', padding: '2rem' }}>
              <p style={{ color: 'var(--text-secondary)' }}>{t.admin.common.noData}</p>
            </div>
          ) : filteredSchools.map(school => (
            <div
              key={school.id}
              className="card"
              onClick={() => loadSchoolDetail(school)}
              style={{
                cursor: 'pointer',
                padding: '1rem 1.25rem',
                border: selected?.id === school.id ? '2px solid var(--primary-color)' : '2px solid transparent',
                borderLeft: !school.isApproved
                  ? '4px solid #f59e0b'
                  : isPaid(school)
                    ? '4px solid #22c55e'
                    : '4px solid #ef4444',
                transition: 'border-color 0.2s',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                {school.logoUrl ? (
                  <img src={school.logoUrl} alt="" style={{ width: 40, height: 40, borderRadius: '50%', objectFit: 'cover' }} />
                ) : (
                  <div style={{
                    width: 40, height: 40, borderRadius: '50%',
                    background: 'linear-gradient(135deg, var(--primary-color), var(--primary-hover))',
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                    color: '#fff', fontWeight: 700, fontSize: '0.9rem',
                  }}>
                    {school.name.charAt(0)}
                  </div>
                )}
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', flexWrap: 'wrap' }}>
                    <strong style={{ fontSize: '1rem' }}>{school.name}</strong>
                    {school.isApproved ? (
                      <span style={{
                        background: '#dcfce7', color: '#16a34a', padding: '0.1rem 0.5rem',
                        borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700,
                      }}>Верифицирована</span>
                    ) : (
                      <span style={{
                        background: '#fef9c3', color: '#ca8a04', padding: '0.1rem 0.5rem',
                        borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700,
                      }}>Не верифицирована</span>
                    )}
                    {isPaid(school) ? (
                      <span style={{
                        background: '#eff6ff', color: '#2563eb', padding: '0.1rem 0.5rem',
                        borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700,
                      }}>Оплачена</span>
                    ) : (
                      <span style={{
                        background: '#fef2f2', color: '#dc2626', padding: '0.1rem 0.5rem',
                        borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700,
                      }}>Не оплачена</span>
                    )}
                    {!school.isActive && (
                      <span style={{
                        background: '#fef2f2', color: '#dc2626', padding: '0.1rem 0.5rem',
                        borderRadius: '999px', fontSize: '0.7rem', fontWeight: 700,
                      }}>Отклонена</span>
                    )}
                  </div>
                  <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)', marginTop: '0.15rem' }}>
                    {school.slug} · {school.tutorCount} тьюторов · {school.studentCount} учеников
                  </div>
                  {school.ownerName && (
                    <div style={{ fontSize: '0.78rem', color: 'var(--text-secondary)' }}>
                      Владелец: {school.ownerName} ({school.ownerEmail})
                    </div>
                  )}
                  {isPaid(school) && school.subscriptionExpiresAt && (
                    <div style={{ fontSize: '0.78rem', color: new Date(school.subscriptionExpiresAt) > new Date() ? '#22c55e' : '#ef4444' }}>
                      Подписка до: {formatDate(school.subscriptionExpiresAt)}
                    </div>
                  )}
                </div>

                {/* Quick Actions */}
                <div style={{ display: 'flex', gap: '0.35rem', flexShrink: 0 }} onClick={e => e.stopPropagation()}>
                  {!school.isActive ? (
                    <button
                      onClick={() => handleRestore(school)}
                      disabled={actionLoading === school.id}
                      className="btn"
                      style={{ padding: '0.3rem 0.6rem', fontSize: '0.75rem', border: 'none', background: '#dcfce7', color: '#16a34a' }}
                    >
                      {actionLoading === school.id ? '...' : 'Восстановить'}
                    </button>
                  ) : (<>
                  <button
                    onClick={() => school.isApproved ? handleUnapprove(school) : handleApprove(school)}
                    disabled={actionLoading === school.id}
                    className="btn"
                    style={{
                      padding: '0.3rem 0.6rem', fontSize: '0.75rem', border: 'none',
                      background: school.isApproved ? '#fef2f2' : '#dcfce7',
                      color: school.isApproved ? '#dc2626' : '#16a34a',
                    }}
                  >
                    {actionLoading === school.id ? '...' : school.isApproved ? 'Снять ✓' : 'Верифицировать'}
                  </button>
                  {school.isApproved && (
                    <button
                      onClick={() => handleToggleSubscription(school)}
                      disabled={actionLoading === school.id}
                      className="btn"
                      style={{
                        padding: '0.3rem 0.6rem', fontSize: '0.75rem', border: 'none',
                        background: isPaid(school) ? '#fef2f2' : '#eff6ff',
                        color: isPaid(school) ? '#dc2626' : '#2563eb',
                      }}
                    >
                      {actionLoading === school.id ? '...' : isPaid(school) ? 'Подписка −' : 'Подписка +'}
                    </button>
                  )}
                  </>)}
                </div>
              </div>

              {school.specializations && (
                <div style={{ display: 'flex', gap: '0.3rem', marginTop: '0.5rem', flexWrap: 'wrap' }}>
                  {school.specializations.split(',').filter(Boolean).map(s => (
                    <span key={s} style={{
                      fontSize: '0.7rem', padding: '0.1rem 0.45rem', borderRadius: '999px',
                      background: 'var(--bg-secondary)', color: 'var(--text-secondary)',
                    }}>{s.trim()}</span>
                  ))}
                </div>
              )}
            </div>
          ))}
        </div>

        {/* School detail panel */}
        {selected && (
          <div className="card" style={{ padding: '1.5rem', alignSelf: 'start' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
              <h2 style={{ margin: 0, fontSize: '1.2rem', fontWeight: 700 }}>{selected.name}</h2>
              <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                {!selected.isActive ? (
                  <button
                    onClick={() => handleRestore(selected)}
                    disabled={actionLoading === selected.id}
                    className="btn"
                    style={{ padding: '0.25rem 0.6rem', fontSize: '0.75rem', border: 'none', background: '#dcfce7', color: '#16a34a' }}
                  >
                    {actionLoading === selected.id ? '...' : 'Восстановить'}
                  </button>
                ) : (<>
                <button
                  onClick={() => selected.isApproved ? handleUnapprove(selected) : handleApprove(selected)}
                  className="btn"
                  style={{
                    padding: '0.25rem 0.6rem', fontSize: '0.75rem', border: 'none',
                    background: selected.isApproved ? '#fef2f2' : '#dcfce7',
                    color: selected.isApproved ? '#dc2626' : '#16a34a',
                  }}
                >
                  {selected.isApproved ? 'Снять ✓' : 'Верифицировать'}
                </button>

                <button
                  onClick={() => handleDeleteSchool(selected)}
                  className="btn btn-outline"
                  style={{ padding: '0.25rem 0.6rem', fontSize: '0.75rem', color: '#dc2626', borderColor: '#dc2626' }}
                >
                  {t.admin.common.delete}
                </button>
                </>)}
                <button
                  onClick={() => openBranding(selected.id)}
                  className="btn"
                  style={{ padding: '0.25rem 0.6rem', fontSize: '0.75rem', border: 'none', background: 'var(--primary-color)', color: '#fff' }}
                >
                  Брендинг
                </button>
                <button onClick={() => setSelected(null)} style={{
                  background: 'none', border: 'none', fontSize: '1.2rem', cursor: 'pointer', color: 'var(--text-secondary)',
                }}>&times;</button>
              </div>
            </div>

            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '1rem' }}>
              <span>ID: {selected.id}</span> · <span>Slug: {selected.slug}</span>
              {selected.websiteUrl && (
                <> · <a href={selected.websiteUrl} target="_blank" rel="noopener noreferrer" style={{ color: 'var(--primary-color)' }}>Website</a></>
              )}
            </div>

            {/* Status badges */}
            <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
              <span style={{
                padding: '0.2rem 0.6rem', borderRadius: '999px', fontSize: '0.75rem', fontWeight: 600,
                background: selected.isApproved ? '#dcfce7' : '#fef9c3',
                color: selected.isApproved ? '#16a34a' : '#ca8a04',
              }}>
                {selected.isApproved ? '✓ Верифицирована' : 'Не верифицирована'}
              </span>
              <span style={{
                padding: '0.2rem 0.6rem', borderRadius: '999px', fontSize: '0.75rem', fontWeight: 600,
                background: isPaid(selected) ? '#eff6ff' : '#fef2f2',
                color: isPaid(selected) ? '#2563eb' : '#dc2626',
              }}>
                {isPaid(selected) ? `Оплачена до ${formatDate(selected.subscriptionExpiresAt!)}` : 'Не оплачена'}
              </span>
              {!selected.isActive && (
                <span style={{
                  padding: '0.2rem 0.6rem', borderRadius: '999px', fontSize: '0.75rem', fontWeight: 600,
                  background: '#fef2f2', color: '#dc2626',
                }}>
                  Отклонена
                </span>
              )}
            </div>

            {detailLoading ? (
              <p style={{ color: 'var(--text-secondary)' }}>{t.admin.common.loading}</p>
            ) : detail ? (
              <>
                {/* Stats */}
                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.75rem', marginBottom: '1.5rem' }}>
                  <div style={{ textAlign: 'center', padding: '0.75rem', background: 'var(--bg-secondary)', borderRadius: '0.5rem' }}>
                    <div style={{ fontSize: '1.4rem', fontWeight: 700, color: 'var(--primary-color)' }}>{detail.students.total}</div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Учеников</div>
                  </div>
                  <div style={{ textAlign: 'center', padding: '0.75rem', background: 'var(--bg-secondary)', borderRadius: '0.5rem' }}>
                    <div style={{ fontSize: '1.4rem', fontWeight: 700, color: '#8b5cf6' }}>{detail.tutors.length}</div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Тьюторов</div>
                  </div>
                  <div style={{ textAlign: 'center', padding: '0.75rem', background: 'var(--bg-secondary)', borderRadius: '0.5rem' }}>
                    <div style={{ fontSize: '1.4rem', fontWeight: 700, color: '#10b981' }}>{selected.specializations ? selected.specializations.split(',').filter(Boolean).length : 0}</div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>Специализаций</div>
                  </div>
                </div>

                {/* Tutors */}
                <h3 style={{ fontSize: '1rem', fontWeight: 600, marginBottom: '0.75rem' }}>Тьюторы</h3>
                {detail.tutors.length === 0 ? (
                  <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Нет тьюторов</p>
                ) : (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', marginBottom: '1.5rem' }}>
                    {detail.tutors.map(tutor => (
                      <div key={tutor.userId} style={{
                        display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                        padding: '0.6rem 0.75rem', background: 'var(--bg-secondary)', borderRadius: '0.5rem',
                      }}>
                        <div>
                          <span style={{ fontWeight: 600 }}>{tutor.name}</span>
                          {tutor.role === 'SchoolAdmin' && (
                            <span style={{
                              fontSize: '0.65rem', padding: '0.1rem 0.4rem', borderRadius: '999px', marginLeft: '0.4rem',
                              background: 'rgba(245, 158, 11, 0.15)', color: '#d97706', fontWeight: 600,
                            }}>SchoolAdmin</span>
                          )}
                          {tutor.isVerified && <span style={{ color: '#3b82f6', marginLeft: '0.3rem' }}>✓</span>}
                          {tutor.email && <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginLeft: '0.5rem' }}>{tutor.email}</span>}
                        </div>
                        <button
                          onClick={() => navigate(`/users?search=${encodeURIComponent(tutor.name)}`)}
                          className="btn btn-outline"
                          style={{ padding: '0.2rem 0.6rem', fontSize: '0.75rem' }}
                        >
                          {t.admin.common.edit}
                        </button>
                      </div>
                    ))}
                  </div>
                )}

                {/* Students preview */}
                {detail.students.total > 0 && (
                  <>
                    <h3 style={{ fontSize: '1rem', fontWeight: 600, marginBottom: '0.75rem' }}>
                      Ученики ({detail.students.total})
                    </h3>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
                      {detail.students.items.slice(0, 10).map(s => (
                        <div key={s.userId} style={{
                          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                          padding: '0.4rem 0.75rem', background: 'var(--bg-secondary)', borderRadius: '0.5rem', fontSize: '0.85rem',
                        }}>
                          <span>{s.name} <span style={{ color: 'var(--text-secondary)' }}>{s.email}</span></span>
                          <span style={{ color: 'var(--text-secondary)', fontSize: '0.75rem' }}>{s.subscriptionTier}</span>
                        </div>
                      ))}
                      {detail.students.total > 10 && (
                        <p style={{ textAlign: 'center', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                          +{detail.students.total - 10} ...
                        </p>
                      )}
                    </div>
                  </>
                )}
              </>
            ) : null}
          </div>
        )}
      </div>

      {/* ─── Branding editor modal ─── */}
      {(brandingLoading || brandingForm) && (
        <div
          onClick={() => { if (!brandingSaving) setBrandingForm(null); }}
          style={{
            position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', zIndex: 1000,
            display: 'flex', alignItems: 'flex-start', justifyContent: 'center', padding: '2rem 1rem', overflowY: 'auto',
          }}
        >
          <div
            onClick={e => e.stopPropagation()}
            className="card"
            style={{ width: '100%', maxWidth: 640, padding: '1.75rem', alignSelf: 'flex-start' }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
              <h2 style={{ margin: 0, fontSize: '1.2rem', fontWeight: 700 }}>Брендинг школы</h2>
              <button onClick={() => { if (!brandingSaving) setBrandingForm(null); }} style={{
                background: 'none', border: 'none', fontSize: '1.4rem', cursor: 'pointer', color: 'var(--text-secondary)',
              }}>&times;</button>
            </div>

            {brandingLoading ? (
              <p style={{ color: 'var(--text-secondary)' }}>{t.admin.common.loading}</p>
            ) : brandingForm ? (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                {brandingError && (
                  <div style={{ padding: '0.6rem 0.85rem', borderRadius: '0.5rem', background: '#fef2f2', color: '#dc2626', fontSize: '0.85rem' }}>{brandingError}</div>
                )}

                <label style={fieldLabel}>Название
                  <input value={brandingForm.name} onChange={e => setBrandingForm({ ...brandingForm, name: e.target.value })} style={fieldInput} />
                </label>

                <label style={fieldLabel}>Поддомен
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem' }}>
                    <input
                      value={brandingForm.subdomain ?? ''}
                      onChange={e => setBrandingForm({ ...brandingForm, subdomain: sanitizeSubdomain(e.target.value) || null })}
                      placeholder="linhao"
                      style={{ ...fieldInput, flex: 1 }}
                    />
                    <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', whiteSpace: 'nowrap' }}>.unistart.kz</span>
                  </div>
                </label>

                <label style={fieldLabel}>Заголовок в навбаре
                  <input value={brandingForm.navbarTitle ?? ''} onChange={e => setBrandingForm({ ...brandingForm, navbarTitle: e.target.value || null })} style={fieldInput} />
                </label>

                <label style={fieldLabel}>Описание (RU)
                  <textarea value={brandingForm.description} onChange={e => setBrandingForm({ ...brandingForm, description: e.target.value })} rows={2} style={{ ...fieldInput, resize: 'vertical' }} />
                </label>
                <label style={fieldLabel}>Описание (EN)
                  <textarea value={brandingForm.descriptionEn ?? ''} onChange={e => setBrandingForm({ ...brandingForm, descriptionEn: e.target.value || null })} rows={2} style={{ ...fieldInput, resize: 'vertical' }} />
                </label>
                <label style={fieldLabel}>Описание (KZ)
                  <textarea value={brandingForm.descriptionKz ?? ''} onChange={e => setBrandingForm({ ...brandingForm, descriptionKz: e.target.value || null })} rows={2} style={{ ...fieldInput, resize: 'vertical' }} />
                </label>

                <label style={fieldLabel}>Логотип (URL)
                  <input value={brandingForm.logoUrl ?? ''} onChange={e => setBrandingForm({ ...brandingForm, logoUrl: e.target.value || null })} style={fieldInput} />
                </label>
                <label style={fieldLabel}>Сайт (URL)
                  <input value={brandingForm.websiteUrl ?? ''} onChange={e => setBrandingForm({ ...brandingForm, websiteUrl: e.target.value || null })} style={fieldInput} />
                </label>
                <label style={fieldLabel}>Instagram (URL)
                  <input value={brandingForm.instagramUrl ?? ''} onChange={e => setBrandingForm({ ...brandingForm, instagramUrl: e.target.value || null })} placeholder="https://www.instagram.com/..." style={fieldInput} />
                </label>
                <label style={fieldLabel}>Telegram (URL)
                  <input value={brandingForm.telegramUrl ?? ''} onChange={e => setBrandingForm({ ...brandingForm, telegramUrl: e.target.value || null })} placeholder="https://t.me/..." style={fieldInput} />
                </label>

                <label style={fieldLabel}>Специализации (через запятую)
                  <input value={brandingForm.specializations} onChange={e => setBrandingForm({ ...brandingForm, specializations: e.target.value })} style={fieldInput} />
                </label>

                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.75rem' }}>
                  <label style={fieldLabel}>Основной цвет
                    <input type="color" value={brandingForm.primaryColor || '#2563eb'} onChange={e => setBrandingForm({ ...brandingForm, primaryColor: e.target.value })} style={{ width: '100%', height: 38, padding: 0, border: '1px solid var(--border-color)', borderRadius: '0.4rem', cursor: 'pointer' }} />
                  </label>
                  <label style={fieldLabel}>Hover-цвет
                    <input type="color" value={brandingForm.primaryHoverColor || '#1d4ed8'} onChange={e => setBrandingForm({ ...brandingForm, primaryHoverColor: e.target.value })} style={{ width: '100%', height: 38, padding: 0, border: '1px solid var(--border-color)', borderRadius: '0.4rem', cursor: 'pointer' }} />
                  </label>
                  <label style={fieldLabel}>Акцентный цвет
                    <input type="color" value={brandingForm.accentColor || '#f59e0b'} onChange={e => setBrandingForm({ ...brandingForm, accentColor: e.target.value })} style={{ width: '100%', height: 38, padding: 0, border: '1px solid var(--border-color)', borderRadius: '0.4rem', cursor: 'pointer' }} />
                  </label>
                </div>

                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.6rem', marginTop: '0.5rem' }}>
                  <button onClick={() => setBrandingForm(null)} disabled={brandingSaving} className="btn btn-outline" style={{ padding: '0.5rem 1rem' }}>{t.admin.common.cancel}</button>
                  <button onClick={handleSaveBranding} disabled={brandingSaving} className="btn" style={{ padding: '0.5rem 1.2rem', background: 'var(--primary-color)', color: '#fff', border: 'none' }}>
                    {brandingSaving ? '...' : t.admin.common.save}
                  </button>
                </div>
              </div>
            ) : null}
          </div>
        </div>
      )}

      {/* ─── Create school modal ─── */}
      {createOpen && (
        <div
          onClick={() => { if (!createSaving) setCreateOpen(false); }}
          style={{
            position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', zIndex: 1000,
            display: 'flex', alignItems: 'flex-start', justifyContent: 'center', padding: '2rem 1rem', overflowY: 'auto',
          }}
        >
          <div onClick={e => e.stopPropagation()} className="card" style={{ width: '100%', maxWidth: 520, padding: '1.75rem', alignSelf: 'flex-start' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
              <h2 style={{ margin: 0, fontSize: '1.2rem', fontWeight: 700 }}>Создать школу</h2>
              <button onClick={() => { if (!createSaving) setCreateOpen(false); }} style={{
                background: 'none', border: 'none', fontSize: '1.4rem', cursor: 'pointer', color: 'var(--text-secondary)',
              }}>&times;</button>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              {createError && (
                <div style={{ padding: '0.6rem 0.85rem', borderRadius: '0.5rem', background: '#fef2f2', color: '#dc2626', fontSize: '0.85rem' }}>{createError}</div>
              )}
              <label style={fieldLabel}>Название *
                <input value={createForm.name} onChange={e => setCreateForm({ ...createForm, name: e.target.value })} style={fieldInput} />
              </label>
              <label style={fieldLabel}>Поддомен
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem' }}>
                  <input
                    value={createForm.subdomain ?? ''}
                    onChange={e => setCreateForm({ ...createForm, subdomain: sanitizeSubdomain(e.target.value) || undefined })}
                    placeholder="linhao"
                    style={{ ...fieldInput, flex: 1 }}
                  />
                  <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', whiteSpace: 'nowrap' }}>.unistart.kz</span>
                </div>
              </label>
              <label style={fieldLabel}>Описание
                <textarea value={createForm.description ?? ''} onChange={e => setCreateForm({ ...createForm, description: e.target.value || undefined })} rows={2} style={{ ...fieldInput, resize: 'vertical' }} />
              </label>
              <label style={fieldLabel}>Специализации (через запятую)
                <input value={createForm.specializations ?? ''} onChange={e => setCreateForm({ ...createForm, specializations: e.target.value || undefined })} style={fieldInput} />
              </label>
              <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', margin: 0 }}>
                Школа создаётся активной и верифицированной, без владельца. Остальной брендинг можно настроить после создания.
              </p>
              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '0.6rem' }}>
                <button onClick={() => setCreateOpen(false)} disabled={createSaving} className="btn btn-outline" style={{ padding: '0.5rem 1rem' }}>{t.admin.common.cancel}</button>
                <button onClick={handleCreateSchool} disabled={createSaving} className="btn" style={{ padding: '0.5rem 1.2rem', background: 'var(--primary-color)', color: '#fff', border: 'none' }}>
                  {createSaving ? '...' : 'Создать'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default AdminSchoolsPage;
