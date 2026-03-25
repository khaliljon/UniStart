import { useState, useEffect, useCallback } from 'react';
import api from '../services/api';

interface SchoolApplication {
  id: number;
  contactName: string;
  email: string;
  phone: string | null;
  schoolName: string;
  message: string | null;
  status: string;
  createdAt: string;
  reviewedAt: string | null;
  reviewedByUserId: number | null;
}

function AdminApplicationsPage() {
  const [apps, setApps] = useState<SchoolApplication[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('');
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [updating, setUpdating] = useState<number | null>(null);

  const fetchApps = useCallback(async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: '20' });
      if (filter) params.set('status', filter);
      const { data } = await api.get(`/admin/school-applications?${params}`);
      setApps(data.items);
      setTotal(data.total);
    } catch { /* ignore */ } finally {
      setLoading(false);
    }
  }, [page, filter]);

  useEffect(() => { fetchApps(); }, [fetchApps]);

  const updateStatus = async (id: number, status: string) => {
    setUpdating(id);
    try {
      await api.put(`/admin/school-applications/${id}/status`, { status });
      await fetchApps();
    } catch { /* ignore */ } finally {
      setUpdating(null);
    }
  };

  const statusBadge = (status: string) => {
    const colors: Record<string, { bg: string; color: string }> = {
      Pending: { bg: 'rgba(245,158,11,0.15)', color: '#f59e0b' },
      Approved: { bg: 'rgba(16,185,129,0.15)', color: '#10b981' },
      Rejected: { bg: 'rgba(239,68,68,0.15)', color: '#ef4444' },
    };
    const c = colors[status] || colors.Pending;
    return (
      <span style={{
        padding: '0.2rem 0.6rem', borderRadius: '1rem', fontSize: '0.75rem',
        fontWeight: 600, background: c.bg, color: c.color,
      }}>
        {status}
      </span>
    );
  };

  return (
    <div style={{ padding: '1.5rem' }}>
      <h1 style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: '1rem' }}>
        Заявки от школ
      </h1>

      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
        {['', 'Pending', 'Approved', 'Rejected'].map(f => (
          <button
            key={f}
            onClick={() => { setFilter(f); setPage(1); }}
            className={filter === f ? 'btn btn-primary' : 'btn btn-outline'}
            style={{ padding: '0.35rem 0.75rem', fontSize: '0.85rem' }}
          >
            {f || 'Все'}
          </button>
        ))}
        <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', alignSelf: 'center', marginLeft: 'auto' }}>
          Всего: {total}
        </span>
      </div>

      {loading ? (
        <p style={{ color: 'var(--text-secondary)' }}>Загрузка...</p>
      ) : apps.length === 0 ? (
        <p style={{ color: 'var(--text-secondary)' }}>Нет заявок</p>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {apps.map(app => (
            <div key={app.id} style={{
              background: 'var(--card-bg)', border: '1px solid var(--border-color)',
              borderRadius: '0.75rem', padding: '1.25rem',
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '0.5rem' }}>
                <div>
                  <div style={{ fontWeight: 600, fontSize: '1.05rem', color: 'var(--text-primary)' }}>
                    {app.schoolName}
                  </div>
                  <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                    {app.contactName} · {app.email}
                    {app.phone && ` · ${app.phone}`}
                  </div>
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                  {statusBadge(app.status)}
                  <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                    {new Date(app.createdAt).toLocaleDateString('ru-RU')}
                  </span>
                </div>
              </div>

              {app.message && (
                <p style={{
                  marginTop: '0.75rem', fontSize: '0.9rem', color: 'var(--text-primary)',
                  background: 'var(--bg-secondary)', padding: '0.75rem', borderRadius: '0.5rem',
                  lineHeight: 1.5,
                }}>
                  {app.message}
                </p>
              )}

              {app.status === 'Pending' && (
                <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.75rem' }}>
                  <button
                    className="btn btn-primary"
                    style={{ padding: '0.35rem 1rem', fontSize: '0.85rem' }}
                    disabled={updating === app.id}
                    onClick={() => updateStatus(app.id, 'Approved')}
                  >
                    {updating === app.id ? '...' : 'Одобрить'}
                  </button>
                  <button
                    className="btn btn-outline"
                    style={{ padding: '0.35rem 1rem', fontSize: '0.85rem', color: '#ef4444', borderColor: '#ef4444' }}
                    disabled={updating === app.id}
                    onClick={() => updateStatus(app.id, 'Rejected')}
                  >
                    Отклонить
                  </button>
                </div>
              )}

              {app.reviewedAt && (
                <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.5rem' }}>
                  Рассмотрено: {new Date(app.reviewedAt).toLocaleDateString('ru-RU')}
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {total > 20 && (
        <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'center', marginTop: '1.5rem' }}>
          <button
            className="btn btn-outline"
            disabled={page <= 1}
            onClick={() => setPage(p => p - 1)}
          >
            ←
          </button>
          <span style={{ alignSelf: 'center', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
            {page} / {Math.ceil(total / 20)}
          </span>
          <button
            className="btn btn-outline"
            disabled={page >= Math.ceil(total / 20)}
            onClick={() => setPage(p => p + 1)}
          >
            →
          </button>
        </div>
      )}
    </div>
  );
}

export default AdminApplicationsPage;
