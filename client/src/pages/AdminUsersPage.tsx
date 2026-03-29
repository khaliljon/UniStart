import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import adminService from '../services/adminService';
import { tutorService } from '../services/tutorService';
import type { AdminUser, AdminUserStats, TutorSchoolCard } from '../types';
import { useTranslation } from '../hooks/useTranslation';
import { getDateLocale } from '../i18n';

const ROLE_COLORS: Record<string, string> = {
  Admin: 'var(--error-color)',
  SchoolAdmin: '#e67e22',
  SchoolTutor: '#0ea5e9',
  Tutor: 'var(--warning-color)',
  Student: 'var(--primary-color)',
};

const TIER_COLORS: Record<string, string> = {
  Pro: '#f59e0b',
  Free: 'var(--text-muted)',
};

function AdminUsersPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [stats, setStats] = useState<AdminUserStats | null>(null);
  const [selected, setSelected] = useState<AdminUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Tutor profile cache
  type TutorInfo = { averageRating: number; totalReviews: number; totalStudents: number; isVerified: boolean; specializations: string; hourlyRate: number | null };
  const [tutorCache, setTutorCache] = useState<Map<number, TutorInfo>>(new Map());
  const [tutorLoading, setTutorLoading] = useState(false);

  // Filters
  const [filterRole, setFilterRole] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [showDeleted, setShowDeleted] = useState(false);

  // Edit form state
  const [editMode, setEditMode] = useState(false);
  const [editData, setEditData] = useState({
    name: '',
    email: '',
    role: '',
    subscriptionTier: '',
    schoolId: null as number | null,
  });

  // Schools list for role assignment
  const [schools, setSchools] = useState<TutorSchoolCard[]>([]);

  // Pagination (OP-13)
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const loadUsers = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const result = await adminService.getUsers(
        filterRole || undefined,
        searchQuery || undefined,
        page,
        50,
        showDeleted
      );
      setUsers(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch {
      setError(t.admin.common.loadError);
    } finally {
      setIsLoading(false);
    }
  }, [filterRole, searchQuery, page, showDeleted]);

  const loadStats = async () => {
    try {
      const s = await adminService.getUserStats();
      setStats(s);
    } catch {
      // ignore
    }
  };

  useEffect(() => { loadUsers(); }, [loadUsers]);
  useEffect(() => { loadStats(); }, []);
  useEffect(() => {
    tutorService.getSchools().then(setSchools).catch(() => {});
  }, []);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setSearchQuery(searchInput);
    setPage(1);
  };

  const openUser = (user: AdminUser) => {
    setSelected(user);
    setEditMode(false);
    setSuccess(null);
    // Load tutor data if user is a Tutor and not cached
    if (user.role === 'Tutor' && !tutorCache.has(user.id)) {
      setTutorLoading(true);
      adminService.getTutors().then(tutors => {
        const map = new Map(tutorCache);
        for (const u of tutors) {
          map.set(u.userId, {
            averageRating: u.averageRating,
            totalReviews: u.totalReviews,
            totalStudents: u.totalStudents,
            isVerified: u.isVerified,
            specializations: u.specializations,
            hourlyRate: u.hourlyRate,
          });
        }
        setTutorCache(map);
      }).catch(() => { /* ignore */ }).finally(() => setTutorLoading(false));
    }
  };

  const startEdit = () => {
    if (!selected) return;
    setEditData({
      name: selected.name,
      email: selected.email,
      role: selected.role,
      subscriptionTier: selected.subscriptionTier,
      schoolId: selected.schoolId ?? null,
    });
    setEditMode(true);
    setSuccess(null);
  };

  const cancelEdit = () => {
    setEditMode(false);
  };

  const saveUser = async () => {
    if (!selected) return;
    try {
      setError(null);
      const payload: Parameters<typeof adminService.updateUser>[1] = {};
      if (editData.name !== selected.name) payload.name = editData.name;
      if (editData.email !== selected.email) payload.email = editData.email;
      if (editData.role !== selected.role) payload.role = editData.role;
      if (editData.subscriptionTier !== selected.subscriptionTier) payload.subscriptionTier = editData.subscriptionTier;
      // School assignment: always send when role is Tutor/SchoolAdmin
      if (editData.role === 'SchoolTutor' || editData.role === 'SchoolAdmin') {
        if (editData.schoolId) {
          payload.schoolId = editData.schoolId;
        } else if (selected.schoolId) {
          payload.clearSchool = true;
        }
      }
      const updated = await adminService.updateUser(selected.id, payload);
      setSelected(updated);
      setEditMode(false);
      setSuccess(t.admin.users.userUpdated);
      loadUsers();
      loadStats();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.admin.users.userUpdateError;
      setError(msg);
    }
  };

  const deleteUser = async (id: number) => {
    if (!confirm(t.admin.users.userDeleteConfirm)) return;
    try {
      setError(null);
      await adminService.deleteUser(id);
      setSelected(null);
      setSuccess(t.admin.users.userDeleted);
      loadUsers();
      loadStats();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.admin.users.userDeleteError;
      setError(msg);
    }
  };

  // Block / Unblock (OP-14)
  const blockUser = async (id: number) => {
    const reason = prompt(t.admin.users.blockReasonPrompt);
    try {
      setError(null);
      const updated = await adminService.blockUser(id, reason || undefined);
      setSelected(updated);
      setSuccess(t.admin.users.userBlocked);
      loadUsers();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.admin.users.blockError;
      setError(msg);
    }
  };

  const unblockUser = async (id: number) => {
    try {
      setError(null);
      const updated = await adminService.unblockUser(id);
      setSelected(updated);
      setSuccess(t.admin.users.userUnblocked);
      loadUsers();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.admin.users.unblockError;
      setError(msg);
    }
  };

  const restoreUser = async (id: number) => {
    if (!confirm(t.admin.users.restoreConfirm)) return;
    try {
      setError(null);
      await adminService.restoreUser(id);
      setSelected(null);
      setSuccess(t.admin.users.userRestored);
      loadUsers();
      loadStats();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.admin.users.restoreError;
      setError(msg);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <h1>{t.admin.users.title}</h1>
      <button className="btn btn-outline" onClick={() => adminService.exportUsersCsv(filterRole || undefined)} style={{ fontSize: '0.85rem', alignSelf: 'flex-start', marginTop: '-0.5rem' }}>
        {t.admin.users.exportCsv}
      </button>

      {/* Stats Cards */}
      {stats && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))', gap: '1rem' }}>
          <StatCard label={t.admin.common.total} value={stats.totalUsers} color="var(--primary-color)" />
          <StatCard label={t.admin.users.students} value={stats.students} color="var(--primary-color)" />
          <StatCard label={t.admin.users.tutorsLabel} value={stats.tutors} color="var(--warning-color)" />
          <StatCard label={t.admin.users.admins} value={stats.admins} color="var(--error-color)" />
          <StatCard label="Pro" value={stats.proUsers} color="#f59e0b" />
          <StatCard label={t.admin.users.activeDays} value={stats.activeLast7Days} color="var(--success-color)" />
        </div>
      )}

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success" style={{ background: 'var(--success-bg)', color: 'var(--success-color)', padding: '0.75rem 1rem', borderRadius: '8px' }}>{success}</div>}

      {/* Filters */}
      <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', alignItems: 'center' }}>
        <select
          value={filterRole}
          onChange={(e) => { setFilterRole(e.target.value); setPage(1); }}
          className="select"
          style={{ padding: '0.5rem', borderRadius: '8px', minWidth: '140px' }}
        >
          <option value="">{t.admin.users.allRoles}</option>
          <option value="Student">Student</option>
          <option value="Tutor">Tutor</option>
          <option value="SchoolAdmin">SchoolAdmin</option>
          <option value="Admin">Admin</option>
        </select>

        <form onSubmit={handleSearch} style={{ display: 'flex', gap: '0.5rem', flex: 1, minWidth: '200px' }}>
          <input
            type="text"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder={t.admin.users.searchPlaceholder}
            className="form-input"
            style={{ flex: 1 }}
          />
          <button type="submit" className="btn btn-primary" style={{ padding: '0.5rem 1rem' }}>{t.admin.users.searchBtn}</button>
        </form>

        <label style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', fontSize: '0.85rem', cursor: 'pointer', whiteSpace: 'nowrap' }}>
          <input
            type="checkbox"
            checked={showDeleted}
            onChange={(e) => { setShowDeleted(e.target.checked); setPage(1); }}
          />
          {t.admin.users.showDeleted}
        </label>
      </div>

      {/* Main content */}
      <div style={{ display: 'grid', gridTemplateColumns: selected ? '1fr 1fr' : '1fr', gap: '1.5rem' }}>
        {/* Users list */}
        <div className="card" style={{ padding: '1rem', overflow: 'auto', maxHeight: '70vh' }}>
          {isLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>{t.admin.common.loading}</div>
          ) : users.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>{t.admin.users.notFound}</div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                  <th style={{ padding: '0.5rem' }}>ID</th>
                  <th style={{ padding: '0.5rem' }}>{t.admin.users.nameCol}</th>
                  <th style={{ padding: '0.5rem' }}>Email</th>
                  <th style={{ padding: '0.5rem' }}>{t.admin.users.roleCol}</th>
                  <th style={{ padding: '0.5rem' }}>{t.admin.users.planCol}</th>
                  <th style={{ padding: '0.5rem' }}>{t.admin.users.answersCol}</th>
                </tr>
              </thead>
              <tbody>
                {users.map((u) => (
                  <tr
                    key={u.id}
                    onClick={() => openUser(u)}
                    style={{
                      borderBottom: '1px solid var(--border-color)',
                      cursor: 'pointer',
                      background: selected?.id === u.id ? 'var(--bg-hover)' : undefined,
                      transition: 'background 0.15s',
                    }}
                    onMouseEnter={(e) => (e.currentTarget.style.background = 'var(--bg-hover)')}
                    onMouseLeave={(e) => (e.currentTarget.style.background = selected?.id === u.id ? 'var(--bg-hover)' : '')}
                  >
                    <td style={{ padding: '0.5rem' }}>{u.id}</td>
                    <td style={{ padding: '0.5rem', fontWeight: 500 }}>
                      {u.name}
                      {u.isBlocked && <span style={{ color: 'var(--error-color)', fontSize: '0.75rem', marginLeft: '0.35rem' }} title={u.blockReason || t.admin.users.blocked}>[{t.admin.users.blocked}]</span>}
                      {u.isDeleted && <span style={{ color: 'var(--text-muted)', fontSize: '0.75rem', marginLeft: '0.35rem' }} title={u.deletedAt ? `${t.admin.users.deleted} ${new Date(u.deletedAt).toLocaleDateString(getDateLocale())}` : t.admin.users.deleted}>[{t.admin.users.deleted}]</span>}
                    </td>
                    <td style={{ padding: '0.5rem', color: 'var(--text-secondary)' }}>{u.email}</td>
                    <td style={{ padding: '0.5rem' }}>
                      <span style={{
                        padding: '0.15rem 0.5rem',
                        borderRadius: '999px',
                        background: ROLE_COLORS[u.role] || 'var(--text-muted)',
                        color: '#fff',
                        fontSize: '0.75rem',
                        fontWeight: 600,
                      }}>
                        {u.role}
                      </span>
                    </td>
                    <td style={{ padding: '0.5rem' }}>
                      <span style={{
                        padding: '0.15rem 0.5rem',
                        borderRadius: '999px',
                        background: TIER_COLORS[u.subscriptionTier] || 'var(--text-muted)',
                        color: '#fff',
                        fontSize: '0.75rem',
                        fontWeight: 600,
                      }}>
                        {u.subscriptionTier}
                      </span>
                    </td>
                    <td style={{ padding: '0.5rem', textAlign: 'center' }}>
                      {u.totalAnswers > 0
                        ? `${u.correctAnswers}/${u.totalAnswers}`
                        : '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          <div style={{ marginTop: '0.75rem', textAlign: 'right', color: 'var(--text-muted)', fontSize: '0.8rem' }}>
            {t.admin.users.found} {totalCount}
          </div>
          {/* Pagination (OP-13) */}
          {totalPages > 1 && (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '0.5rem', padding: '0.75rem 0 0' }}>
              <button className="btn btn-outline" disabled={page <= 1} onClick={() => setPage(1)} style={{ fontSize: '0.85rem' }}>«</button>
              <button className="btn btn-outline" disabled={page <= 1} onClick={() => setPage(p => p - 1)} style={{ fontSize: '0.85rem' }}>‹</button>
              <span style={{ padding: '0 0.75rem', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
                {page} / {totalPages}
              </span>
              <button className="btn btn-outline" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)} style={{ fontSize: '0.85rem' }}>›</button>
              <button className="btn btn-outline" disabled={page >= totalPages} onClick={() => setPage(totalPages)} style={{ fontSize: '0.85rem' }}>»</button>
            </div>
          )}
        </div>

        {/* Detail panel */}
        {selected && (
          <div className="card" style={{ padding: '1.5rem', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <h2 style={{ margin: 0, fontSize: '1.2rem' }}>
                {editMode ? t.admin.users.editTitle : t.admin.users.profileTitle}
              </h2>
              <button onClick={() => setSelected(null)} style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '1.2rem', color: 'var(--text-muted)' }}>✕</button>
            </div>

            {editMode ? (
              /* Edit form */
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  {t.admin.users.nameLabel}
                  <input
                    type="text"
                    value={editData.name}
                    onChange={(e) => setEditData({ ...editData, name: e.target.value })}
                    className="form-input"
                    style={{ width: '100%', marginTop: '0.25rem' }}
                  />
                </label>
                <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  {t.admin.users.emailLabel}
                  <input
                    type="email"
                    value={editData.email}
                    onChange={(e) => setEditData({ ...editData, email: e.target.value })}
                    className="form-input"
                    style={{ width: '100%', marginTop: '0.25rem' }}
                  />
                </label>
                <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  {t.admin.users.roleLabel}
                  <select
                    value={editData.role}
                    onChange={(e) => setEditData({ ...editData, role: e.target.value })}
                    style={{ width: '100%', marginTop: '0.25rem' }}
                  >
                    <option value="Student">Student</option>
                    <option value="Tutor">Tutor</option>
                    <option value="SchoolTutor">SchoolTutor</option>
                    <option value="SchoolAdmin">SchoolAdmin</option>
                    <option value="Admin">Admin</option>
                  </select>
                </label>
                {(editData.role === 'SchoolTutor' || editData.role === 'SchoolAdmin') && schools.length > 0 && (
                  <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                    {t.admin.users.schoolLabel}
                    <select
                      value={editData.schoolId ?? ''}
                      onChange={(e) => setEditData({ ...editData, schoolId: e.target.value ? Number(e.target.value) : null })}
                      style={{ width: '100%', marginTop: '0.25rem' }}
                    >
                      {editData.role !== 'SchoolTutor' && <option value="">{t.admin.users.noSchool}</option>}
                      {schools.map(s => (
                        <option key={s.id} value={s.id}>{s.name} ({s.slug})</option>
                      ))}
                    </select>
                  </label>
                )}
                <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  {t.admin.users.subscriptionLabel}
                  <select
                    value={editData.subscriptionTier}
                    onChange={(e) => setEditData({ ...editData, subscriptionTier: e.target.value })}
                    style={{ width: '100%', marginTop: '0.25rem' }}
                  >
                    <option value="Free">Free</option>
                    <option value="Pro">Pro</option>
                  </select>
                </label>

                <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem' }}>
                  <button className="btn btn-primary" onClick={saveUser} style={{ flex: 1 }}>{t.admin.common.save}</button>
                  <button className="btn btn-outline" onClick={cancelEdit} style={{ flex: 1 }}>{t.admin.common.cancel}</button>
                </div>
              </div>
            ) : (
              /* View mode */
              <>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                  <InfoField label="ID" value={String(selected.id)} />
                  <InfoField label={t.admin.users.nameLabel} value={selected.name} />
                  <InfoField label={t.admin.users.emailLabel} value={selected.email} />
                  <InfoField label={t.admin.users.roleLabel} value={selected.role} color={ROLE_COLORS[selected.role]} />
                  <InfoField label={t.admin.users.subscriptionLabel} value={selected.subscriptionTier} color={TIER_COLORS[selected.subscriptionTier]} />
                  <InfoField label={t.admin.users.onboarding} value={selected.hasCompletedOnboarding ? t.admin.users.onboardingDone : t.admin.users.onboardingNotDone} />
                  <InfoField label={t.admin.users.registeredAt} value={new Date(selected.createdAt).toLocaleDateString(getDateLocale())} />
                  <InfoField label={t.admin.users.updatedAt} value={selected.updatedAt ? new Date(selected.updatedAt).toLocaleDateString(getDateLocale()) : '—'} />
                  {selected.schoolName && (
                    <InfoField label={t.admin.users.schoolLabel} value={selected.schoolName} color="#e67e22" />
                  )}
                </div>

                {/* Block status (OP-14) */}
                {selected.isBlocked && (
                  <div style={{ padding: '0.75rem', background: 'rgba(239,68,68,0.08)', borderRadius: '8px', border: '1px solid var(--error-color)' }}>
                    <div style={{ fontWeight: 600, color: 'var(--error-color)', fontSize: '0.9rem' }}>{t.admin.users.blockedLabel}</div>
                    {selected.blockReason && <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>{t.admin.users.blockReason} {selected.blockReason}</div>}
                    {selected.blockedAt && <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '0.15rem' }}>{t.admin.users.since} {new Date(selected.blockedAt).toLocaleDateString(getDateLocale())}</div>}
                  </div>
                )}

                {/* Deleted status */}
                {selected.isDeleted && (
                  <div style={{ padding: '0.75rem', background: 'rgba(107,114,128,0.08)', borderRadius: '8px', border: '1px solid var(--text-muted)' }}>
                    <div style={{ fontWeight: 600, color: 'var(--text-muted)', fontSize: '0.9rem' }}>{t.admin.users.deletedLabel}</div>
                    {selected.deletedAt && <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '0.15rem' }}>{t.admin.users.since} {new Date(selected.deletedAt).toLocaleDateString(getDateLocale())}</div>}
                  </div>
                )}

                <div style={{ borderTop: '1px solid var(--border-color)', paddingTop: '0.75rem' }}>
                  <h3 style={{ fontSize: '1rem', margin: '0 0 0.5rem' }}>{t.admin.users.statistics}</h3>
                  {selected.role === 'Tutor' ? (
                    /* Tutor-specific stats */
                    tutorLoading ? (
                      <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{t.admin.common.loading}</div>
                    ) : (() => {
                      const tutor = tutorCache.get(selected.id);
                      if (!tutor) return <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{t.admin.users.noTutorProfile}</div>;
                      return (
                        <>
                          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '0.75rem' }}>
                            <StatCard label={t.admin.users.studentsCount} value={tutor.totalStudents} color="var(--primary-color)" small />
                            <StatCard label={t.admin.users.reviewsCount} value={tutor.totalReviews} color="var(--warning-color)" small />
                            <StatCard label={t.admin.users.ratingLabel} value={tutor.averageRating} color="var(--success-color)" small />
                          </div>
                          <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem', flexWrap: 'wrap' }}>
                            <span style={{
                              padding: '0.2rem 0.6rem', borderRadius: '999px', fontSize: '0.75rem', fontWeight: 600,
                              background: tutor.isVerified ? 'var(--success-color)' : 'var(--text-muted)',
                              color: '#fff',
                            }}>
                              {tutor.isVerified ? t.admin.users.verifiedLabel : t.admin.users.notVerified}
                            </span>
                            {tutor.hourlyRate != null && (
                              <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                                {tutor.hourlyRate} $/час
                              </span>
                            )}
                          </div>
                          {tutor.specializations && (
                            <div style={{ marginTop: '0.5rem', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                              {t.admin.users.specializations} {tutor.specializations}
                            </div>
                          )}
                        </>
                      );
                    })()
                  ) : (
                    /* Student / Admin stats */
                    <>
                      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '0.75rem' }}>
                        <StatCard label={t.admin.users.answersCount} value={selected.totalAnswers} color="var(--primary-color)" small />
                        <StatCard label={t.admin.users.correctCount} value={selected.correctAnswers} color="var(--success-color)" small />
                        <StatCard label={t.admin.users.sessionsCount} value={selected.testSessions} color="var(--info-color, #3b82f6)" small />
                      </div>
                      {selected.totalAnswers > 0 && (
                        <div style={{ marginTop: '0.5rem', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                          {t.admin.users.accuracyLabel} {Math.round((selected.correctAnswers / selected.totalAnswers) * 100)}%
                        </div>
                      )}
                    </>
                  )}
                </div>

                <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem', flexWrap: 'wrap' }}>
                  <button className="btn btn-primary" onClick={startEdit} style={{ flex: 1 }}>{t.admin.users.editBtn}</button>
                  <button
                    className="btn btn-outline"
                    onClick={() => navigate(`/activity?id=${selected.id}`)}
                    style={{ flex: 1 }}
                  >
                    {t.admin.users.activityBtn}
                  </button>
                  {selected.role !== 'Admin' && (
                    selected.isBlocked ? (
                      <button
                        className="btn"
                        onClick={() => unblockUser(selected.id)}
                        style={{ flex: 1, background: 'var(--success-color)', color: '#fff', border: 'none', cursor: 'pointer', borderRadius: '8px', padding: '0.5rem' }}
                      >
                        {t.admin.users.unblockBtn}
                      </button>
                    ) : (
                      <button
                        className="btn"
                        onClick={() => blockUser(selected.id)}
                        style={{ flex: 1, background: 'var(--warning-color)', color: '#fff', border: 'none', cursor: 'pointer', borderRadius: '8px', padding: '0.5rem' }}
                      >
                        {t.admin.users.blockBtn}
                      </button>
                    )
                  )}
                  {selected.isDeleted ? (
                    <button
                      className="btn"
                      onClick={() => restoreUser(selected.id)}
                      style={{ flex: 1, background: 'var(--success-color)', color: '#fff', border: 'none', cursor: 'pointer', borderRadius: '8px', padding: '0.5rem' }}
                    >
                      {t.admin.users.restoreBtn}
                    </button>
                  ) : (
                    <button
                      className="btn"
                      onClick={() => deleteUser(selected.id)}
                      style={{ flex: 1, background: 'var(--error-color)', color: '#fff', border: 'none', cursor: 'pointer', borderRadius: '8px', padding: '0.5rem' }}
                    >
                      {t.admin.users.deleteBtn}
                    </button>
                  )}
                </div>
              </>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

function StatCard({ label, value, color, small }: { label: string; value: number; color: string; small?: boolean }) {
  return (
    <div className="card" style={{
      padding: small ? '0.5rem 0.75rem' : '1rem',
      textAlign: 'center',
      borderLeft: `3px solid ${color}`,
    }}>
      <div style={{ fontSize: small ? '1.2rem' : '1.8rem', fontWeight: 700, color }}>
        {value}
      </div>
      <div style={{ fontSize: small ? '0.7rem' : '0.8rem', color: 'var(--text-muted)', marginTop: '0.25rem' }}>
        {label}
      </div>
    </div>
  );
}

function InfoField({ label, value, color }: { label: string; value: string; color?: string }) {
  return (
    <div>
      <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', marginBottom: '0.15rem' }}>{label}</div>
      <div style={{ fontSize: '0.9rem', fontWeight: 500, color: color || 'var(--text-primary)' }}>{value}</div>
    </div>
  );
}

export default AdminUsersPage;
