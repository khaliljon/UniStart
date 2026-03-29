import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import adminService from '../services/adminService';
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
    } catch { alert('Ошибка одобрения'); }
    finally { setActionLoading(null); }
  };

  const handleReject = async (school: AdminSchool) => {
    if (!confirm(`Отклонить школу "${school.name}"?`)) return;
    setActionLoading(school.id);
    try {
      await adminService.rejectSchool(school.id);
      setSchools(prev => prev.filter(s => s.id !== school.id));
      if (selected?.id === school.id) { setSelected(null); setDetail(null); }
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
                <div style={{ display: 'flex', gap: '0.35rem', flexShrink: 0, flexDirection: 'column' }} onClick={e => e.stopPropagation()}>
                  {!school.isApproved && (
                    <button
                      onClick={() => handleApprove(school)}
                      disabled={actionLoading === school.id}
                      className="btn"
                      style={{ padding: '0.3rem 0.6rem', fontSize: '0.75rem', background: '#dcfce7', color: '#16a34a', border: 'none' }}
                    >
                      {actionLoading === school.id ? '...' : '✓ Одобрить'}
                    </button>
                  )}
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
                    {actionLoading === school.id ? '...' : isPaid(school) ? 'Снять подписку' : 'Активировать'}
                  </button>
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
                {!selected.isApproved && (
                  <>
                    <button onClick={() => handleApprove(selected)} className="btn" style={{ padding: '0.25rem 0.6rem', fontSize: '0.75rem', background: '#dcfce7', color: '#16a34a', border: 'none' }}>
                      Одобрить
                    </button>
                    <button onClick={() => handleReject(selected)} className="btn" style={{ padding: '0.25rem 0.6rem', fontSize: '0.75rem', background: '#fef2f2', color: '#dc2626', border: 'none' }}>
                      Отклонить
                    </button>
                  </>
                )}
                <button
                  onClick={() => handleDeleteSchool(selected)}
                  className="btn btn-outline"
                  style={{ padding: '0.25rem 0.6rem', fontSize: '0.75rem', color: '#dc2626', borderColor: '#dc2626' }}
                >
                  {t.admin.common.delete}
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
                {selected.isApproved ? '✓ Верифицирована' : '⏳ Не верифицирована'}
              </span>
              <span style={{
                padding: '0.2rem 0.6rem', borderRadius: '999px', fontSize: '0.75rem', fontWeight: 600,
                background: isPaid(selected) ? '#eff6ff' : '#fef2f2',
                color: isPaid(selected) ? '#2563eb' : '#dc2626',
              }}>
                {isPaid(selected) ? `Оплачена до ${formatDate(selected.subscriptionExpiresAt!)}` : 'Не оплачена'}
              </span>
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
    </div>
  );
}

export default AdminSchoolsPage;
