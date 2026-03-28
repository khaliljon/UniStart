import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from '../i18n';
import { tutorService } from '../services/tutorService';
import adminService from '../services/adminService';
import api from '../services/api';
import type { TutorSchoolCard } from '../types';

interface SchoolDetail {
  students: { total: number; items: Array<{ userId: number; name: string; email: string; subscriptionTier: string; createdAt: string; lastSeenAt: string | null }> };
  tutors: Array<{ userId: number; name: string; email: string; headline: string; isVerified: boolean; isAvailable: boolean; totalStudents: number; averageRating: number; role?: string }>;
}

function AdminSchoolsPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [schools, setSchools] = useState<TutorSchoolCard[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<TutorSchoolCard | null>(null);
  const [detail, setDetail] = useState<SchoolDetail | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [pendingSchools, setPendingSchools] = useState<Array<{ id: number; name: string; slug: string; ownerName: string; ownerEmail: string; createdAt: string }>>([]);

  useEffect(() => {
    tutorService.getSchools().then(setSchools).catch(console.error).finally(() => setLoading(false));
    adminService.getPendingSchools().then(setPendingSchools).catch(() => {});
  }, []);

  const loadSchoolDetail = useCallback(async (school: TutorSchoolCard) => {
    setSelected(school);
    setDetail(null);
    setDetailLoading(true);
    try {
      // Use school-admin-like endpoints via admin proxy
      const [studentsRes, tutorsRes] = await Promise.all([
        api.get(`/admin/schools/${school.id}/students`, { params: { pageSize: 50 } }),
        api.get(`/admin/schools/${school.id}/tutors`),
      ]);
      setDetail({ students: studentsRes.data, tutors: tutorsRes.data });
    } catch {
      // Fallback: load school detail page data
      try {
        const schoolData = await tutorService.getSchool(school.slug);
        if (schoolData) {
          setDetail({
            students: { total: 0, items: [] },
            tutors: schoolData.tutors.map(t => ({
              userId: t.userId, name: t.name, email: '', headline: t.headline || '',
              isVerified: t.isVerified, isAvailable: t.isAvailable,
              totalStudents: t.totalStudents, averageRating: t.averageRating,
            })),
          });
        }
      } catch { /* ignore */ }
    } finally {
      setDetailLoading(false);
    }
  }, []);

  const handleDeleteSchool = useCallback(async (school: TutorSchoolCard) => {
    if (!confirm(`${t.admin.common.deleteConfirm}: ${school.name}?`)) return;
    try {
      await adminService.deleteSchool(school.id);
      setSchools(prev => prev.filter(s => s.id !== school.id));
      setSelected(null);
      setDetail(null);
    } catch {
      alert(t.admin.common.deleteError);
    }
  }, [t]);

  const handleApproveSchool = useCallback(async (id: number) => {
    try {
      await adminService.approveSchool(id);
      setPendingSchools(prev => prev.filter(s => s.id !== id));
      tutorService.getSchools().then(setSchools).catch(() => {});
    } catch { alert('Error approving school'); }
  }, []);

  const handleRejectSchool = useCallback(async (id: number) => {
    if (!confirm(t.admin.common.deleteConfirm + '?')) return;
    try {
      await adminService.rejectSchool(id);
      setPendingSchools(prev => prev.filter(s => s.id !== id));
    } catch { alert('Error rejecting school'); }
  }, [t]);

  if (loading) {
    return <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>{t.admin.common.loading}</div>;
  }

  return (
    <div className="animate-fade-in">
      <h1 style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: '1.5rem' }}>
        {t.admin.nav.schools}
        <span style={{ fontWeight: 400, fontSize: '1rem', color: 'var(--text-secondary)', marginLeft: '0.75rem' }}>
          {schools.length}
        </span>
      </h1>

      {/* Pending school applications */}
      {pendingSchools.length > 0 && (
        <div className="card" style={{ padding: '1.25rem', marginBottom: '1.5rem', border: '1px solid #f59e0b44', background: '#f59e0b11' }}>
          <h2 style={{ margin: '0 0 0.75rem', fontSize: '1.1rem', fontWeight: 700 }}>
            {t.admin.schools.pendingTitle}
            <span style={{ background: '#f59e0b', color: '#fff', borderRadius: '999px', padding: '0.1rem 0.5rem', fontSize: '0.75rem', fontWeight: 700, marginLeft: '0.5rem' }}>
              {pendingSchools.length}
            </span>
          </h2>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {pendingSchools.map(s => (
              <div key={s.id} style={{
                display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: '0.5rem',
                padding: '0.75rem', borderRadius: '8px', background: 'var(--bg-secondary)',
              }}>
                <div>
                  <strong>{s.name}</strong>
                  <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginLeft: '0.5rem' }}>{s.slug}</span>
                  <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                    {s.ownerName} ({s.ownerEmail}) · {new Date(s.createdAt).toLocaleDateString()}
                  </div>
                </div>
                <div style={{ display: 'flex', gap: '0.5rem' }}>
                  <button className="btn btn-primary" style={{ padding: '0.35rem 0.8rem', fontSize: '0.8rem' }} onClick={() => handleApproveSchool(s.id)}>
                    {t.schoolAdmin.approve}
                  </button>
                  <button className="btn btn-outline" style={{ padding: '0.35rem 0.8rem', fontSize: '0.8rem', color: '#ef4444', borderColor: '#ef4444' }} onClick={() => handleRejectSchool(s.id)}>
                    {t.schoolAdmin.reject}
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: selected ? '1fr 1.2fr' : '1fr', gap: '1.5rem' }}>
        {/* Schools list */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {schools.length === 0 ? (
            <div className="card" style={{ textAlign: 'center', padding: '2rem' }}>
              <p style={{ color: 'var(--text-secondary)' }}>{t.admin.common.noData}</p>
            </div>
          ) : schools.map(school => (
            <div
              key={school.id}
              className="card"
              onClick={() => loadSchoolDetail(school)}
              style={{
                cursor: 'pointer',
                padding: '1rem 1.25rem',
                border: selected?.id === school.id ? '2px solid var(--primary-color)' : '2px solid transparent',
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
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <strong style={{ fontSize: '1rem' }}>{school.name}</strong>
                    {school.isPartner && (
                      <span style={{
                        fontSize: '0.6rem', padding: '0.1rem 0.4rem', borderRadius: '999px',
                        background: 'rgba(99, 102, 241, 0.15)', color: 'var(--primary-color)', fontWeight: 600,
                      }}>Partner</span>
                    )}
                  </div>
                  <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
                    {school.slug} &middot; {school.tutorCount} {t.tutor.tutorsCount}
                  </div>
                </div>
              </div>
              {school.specializations.length > 0 && (
                <div style={{ display: 'flex', gap: '0.3rem', marginTop: '0.5rem', flexWrap: 'wrap' }}>
                  {school.specializations.map(s => (
                    <span key={s} style={{
                      fontSize: '0.7rem', padding: '0.1rem 0.45rem', borderRadius: '999px',
                      background: 'var(--bg-secondary)', color: 'var(--text-secondary)',
                    }}>{s}</span>
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
              <span>ID: {selected.id}</span> &middot; <span>Slug: {selected.slug}</span>
              {selected.websiteUrl && (
                <> &middot; <a href={selected.websiteUrl} target="_blank" rel="noopener noreferrer" style={{ color: 'var(--primary-color)' }}>Website</a></>
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
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{t.admin.stats.totalUsers}</div>
                  </div>
                  <div style={{ textAlign: 'center', padding: '0.75rem', background: 'var(--bg-secondary)', borderRadius: '0.5rem' }}>
                    <div style={{ fontSize: '1.4rem', fontWeight: 700, color: '#8b5cf6' }}>{detail.tutors.length}</div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{t.tutor.schoolTutors}</div>
                  </div>
                  <div style={{ textAlign: 'center', padding: '0.75rem', background: 'var(--bg-secondary)', borderRadius: '0.5rem' }}>
                    <div style={{ fontSize: '1.4rem', fontWeight: 700, color: '#10b981' }}>{selected.specializations.length}</div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{t.tutor.specialization}</div>
                  </div>
                </div>

                {/* Tutors */}
                <h3 style={{ fontSize: '1rem', fontWeight: 600, marginBottom: '0.75rem' }}>{t.tutor.schoolTutors}</h3>
                {detail.tutors.length === 0 ? (
                  <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{t.tutor.noSchoolTutors}</p>
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
                      {t.admin.stats.totalUsers} ({detail.students.total})
                    </h3>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
                      {detail.students.items.slice(0, 10).map(s => (
                        <div key={s.userId} style={{
                          display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                          padding: '0.4rem 0.75rem', background: 'var(--bg-secondary)', borderRadius: '0.5rem',
                          fontSize: '0.85rem',
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
