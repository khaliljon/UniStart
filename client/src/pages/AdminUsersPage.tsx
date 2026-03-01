import { useEffect, useState, useCallback } from 'react';
import adminService from '../services/adminService';
import type { AdminUser, AdminUserStats } from '../types';

const ROLE_COLORS: Record<string, string> = {
  Admin: 'var(--error-color)',
  Tutor: 'var(--warning-color)',
  Student: 'var(--primary-color)',
};

const TIER_COLORS: Record<string, string> = {
  Pro: '#f59e0b',
  Free: 'var(--text-muted)',
};

function AdminUsersPage() {
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [stats, setStats] = useState<AdminUserStats | null>(null);
  const [selected, setSelected] = useState<AdminUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Filters
  const [filterRole, setFilterRole] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [searchInput, setSearchInput] = useState('');

  // Edit form state
  const [editMode, setEditMode] = useState(false);
  const [editData, setEditData] = useState({
    name: '',
    email: '',
    role: '',
    subscriptionTier: '',
  });

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
        50
      );
      setUsers(result.items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch {
      setError('Ошибка загрузки пользователей');
    } finally {
      setIsLoading(false);
    }
  }, [filterRole, searchQuery, page]);

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

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setSearchQuery(searchInput);
    setPage(1);
  };

  const openUser = (user: AdminUser) => {
    setSelected(user);
    setEditMode(false);
    setSuccess(null);
  };

  const startEdit = () => {
    if (!selected) return;
    setEditData({
      name: selected.name,
      email: selected.email,
      role: selected.role,
      subscriptionTier: selected.subscriptionTier,
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
      const updated = await adminService.updateUser(selected.id, {
        name: editData.name !== selected.name ? editData.name : undefined,
        email: editData.email !== selected.email ? editData.email : undefined,
        role: editData.role !== selected.role ? editData.role : undefined,
        subscriptionTier: editData.subscriptionTier !== selected.subscriptionTier ? editData.subscriptionTier : undefined,
      });
      setSelected(updated);
      setEditMode(false);
      setSuccess('Пользователь обновлён');
      loadUsers();
      loadStats();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Ошибка обновления';
      setError(msg);
    }
  };

  const deleteUser = async (id: number) => {
    if (!confirm('Удалить пользователя и все его данные? Это действие необратимо.')) return;
    try {
      setError(null);
      await adminService.deleteUser(id);
      setSelected(null);
      setSuccess('Пользователь удалён');
      loadUsers();
      loadStats();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Ошибка удаления';
      setError(msg);
    }
  };

  // Block / Unblock (OP-14)
  const blockUser = async (id: number) => {
    const reason = prompt('Причина блокировки (необязательно):');
    try {
      setError(null);
      const updated = await adminService.blockUser(id, reason || undefined);
      setSelected(updated);
      setSuccess('Пользователь заблокирован');
      loadUsers();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Ошибка блокировки';
      setError(msg);
    }
  };

  const unblockUser = async (id: number) => {
    try {
      setError(null);
      const updated = await adminService.unblockUser(id);
      setSelected(updated);
      setSuccess('Пользователь разблокирован');
      loadUsers();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Ошибка разблокировки';
      setError(msg);
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <h1>Управление пользователями</h1>
      <button className="btn btn-outline" onClick={() => adminService.exportUsersCsv(filterRole || undefined)} style={{ fontSize: '0.85rem', alignSelf: 'flex-start', marginTop: '-0.5rem' }}>
        Экспорт CSV
      </button>

      {/* Stats Cards */}
      {stats && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))', gap: '1rem' }}>
          <StatCard label="Всего" value={stats.totalUsers} color="var(--primary-color)" />
          <StatCard label="Студенты" value={stats.students} color="var(--primary-color)" />
          <StatCard label="Тьюторы" value={stats.tutors} color="var(--warning-color)" />
          <StatCard label="Админы" value={stats.admins} color="var(--error-color)" />
          <StatCard label="Pro" value={stats.proUsers} color="#f59e0b" />
          <StatCard label="Активны (7д)" value={stats.activeLast7Days} color="var(--success-color)" />
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
          <option value="">Все роли</option>
          <option value="Student">Student</option>
          <option value="Tutor">Tutor</option>
          <option value="Admin">Admin</option>
        </select>

        <form onSubmit={handleSearch} style={{ display: 'flex', gap: '0.5rem', flex: 1, minWidth: '200px' }}>
          <input
            type="text"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            placeholder="Поиск по имени или email..."
            className="form-input"
            style={{ flex: 1 }}
          />
          <button type="submit" className="btn btn-primary" style={{ padding: '0.5rem 1rem' }}>Поиск</button>
        </form>
      </div>

      {/* Main content */}
      <div style={{ display: 'grid', gridTemplateColumns: selected ? '1fr 1fr' : '1fr', gap: '1.5rem' }}>
        {/* Users list */}
        <div className="card" style={{ padding: '1rem', overflow: 'auto', maxHeight: '70vh' }}>
          {isLoading ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>Загрузка...</div>
          ) : users.length === 0 ? (
            <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-muted)' }}>Пользователи не найдены</div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left' }}>
                  <th style={{ padding: '0.5rem' }}>ID</th>
                  <th style={{ padding: '0.5rem' }}>Имя</th>
                  <th style={{ padding: '0.5rem' }}>Email</th>
                  <th style={{ padding: '0.5rem' }}>Роль</th>
                  <th style={{ padding: '0.5rem' }}>Тариф</th>
                  <th style={{ padding: '0.5rem' }}>Ответы</th>
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
                      {u.isBlocked && <span style={{ color: 'var(--error-color)', fontSize: '0.75rem', marginLeft: '0.35rem' }} title={u.blockReason || 'Заблокирован'}>[Блок]</span>}
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
            Найдено: {totalCount}
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
                {editMode ? 'Редактирование' : 'Профиль пользователя'}
              </h2>
              <button onClick={() => setSelected(null)} style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '1.2rem', color: 'var(--text-muted)' }}>✕</button>
            </div>

            {editMode ? (
              /* Edit form */
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  Имя
                  <input
                    type="text"
                    value={editData.name}
                    onChange={(e) => setEditData({ ...editData, name: e.target.value })}
                    className="form-input"
                    style={{ width: '100%', marginTop: '0.25rem' }}
                  />
                </label>
                <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  Email
                  <input
                    type="email"
                    value={editData.email}
                    onChange={(e) => setEditData({ ...editData, email: e.target.value })}
                    className="form-input"
                    style={{ width: '100%', marginTop: '0.25rem' }}
                  />
                </label>
                <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  Роль
                  <select
                    value={editData.role}
                    onChange={(e) => setEditData({ ...editData, role: e.target.value })}
                    style={{ width: '100%', marginTop: '0.25rem' }}
                  >
                    <option value="Student">Student</option>
                    <option value="Tutor">Tutor</option>
                    <option value="Admin">Admin</option>
                  </select>
                </label>
                <label style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  Подписка
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
                  <button className="btn btn-primary" onClick={saveUser} style={{ flex: 1 }}>Сохранить</button>
                  <button className="btn btn-outline" onClick={cancelEdit} style={{ flex: 1 }}>Отмена</button>
                </div>
              </div>
            ) : (
              /* View mode */
              <>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
                  <InfoField label="ID" value={String(selected.id)} />
                  <InfoField label="Имя" value={selected.name} />
                  <InfoField label="Email" value={selected.email} />
                  <InfoField label="Роль" value={selected.role} color={ROLE_COLORS[selected.role]} />
                  <InfoField label="Подписка" value={selected.subscriptionTier} color={TIER_COLORS[selected.subscriptionTier]} />
                  <InfoField label="Онбординг" value={selected.hasCompletedOnboarding ? 'Пройден' : 'Не завершён'} />
                  <InfoField label="Дата регистрации" value={new Date(selected.createdAt).toLocaleDateString('ru-RU')} />
                  <InfoField label="Обновлён" value={selected.updatedAt ? new Date(selected.updatedAt).toLocaleDateString('ru-RU') : '—'} />
                </div>

                {/* Block status (OP-14) */}
                {selected.isBlocked && (
                  <div style={{ padding: '0.75rem', background: 'rgba(239,68,68,0.08)', borderRadius: '8px', border: '1px solid var(--error-color)' }}>
                    <div style={{ fontWeight: 600, color: 'var(--error-color)', fontSize: '0.9rem' }}>Заблокирован</div>
                    {selected.blockReason && <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>Причина: {selected.blockReason}</div>}
                    {selected.blockedAt && <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '0.15rem' }}>С {new Date(selected.blockedAt).toLocaleDateString('ru-RU')}</div>}
                  </div>
                )}

                <div style={{ borderTop: '1px solid var(--border-color)', paddingTop: '0.75rem' }}>
                  <h3 style={{ fontSize: '1rem', margin: '0 0 0.5rem' }}>Статистика</h3>
                  <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '0.75rem' }}>
                    <StatCard label="Ответов" value={selected.totalAnswers} color="var(--primary-color)" small />
                    <StatCard label="Верных" value={selected.correctAnswers} color="var(--success-color)" small />
                    <StatCard label="Сессий" value={selected.testSessions} color="var(--info-color, #3b82f6)" small />
                  </div>
                  {selected.totalAnswers > 0 && (
                    <div style={{ marginTop: '0.5rem', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                      Точность: {Math.round((selected.correctAnswers / selected.totalAnswers) * 100)}%
                    </div>
                  )}
                </div>

                <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem' }}>
                  <button className="btn btn-primary" onClick={startEdit} style={{ flex: 1 }}>Редактировать</button>
                  {selected.role !== 'Admin' && (
                    selected.isBlocked ? (
                      <button
                        className="btn"
                        onClick={() => unblockUser(selected.id)}
                        style={{ flex: 1, background: 'var(--success-color)', color: '#fff', border: 'none', cursor: 'pointer', borderRadius: '8px', padding: '0.5rem' }}
                      >
                        Разблокировать
                      </button>
                    ) : (
                      <button
                        className="btn"
                        onClick={() => blockUser(selected.id)}
                        style={{ flex: 1, background: 'var(--warning-color)', color: '#fff', border: 'none', cursor: 'pointer', borderRadius: '8px', padding: '0.5rem' }}
                      >
                        Заблокировать
                      </button>
                    )
                  )}
                  <button
                    className="btn"
                    onClick={() => deleteUser(selected.id)}
                    style={{ flex: 1, background: 'var(--error-color)', color: '#fff', border: 'none', cursor: 'pointer', borderRadius: '8px', padding: '0.5rem' }}
                  >
                    Удалить
                  </button>
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
