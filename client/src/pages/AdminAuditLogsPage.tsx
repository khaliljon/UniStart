import { useState, useEffect, useCallback } from 'react';
import adminService from '../services/adminService';

interface AuditLog {
  id: number;
  userId: number;
  userEmail: string;
  action: string;
  entityType: string;
  entityId: string | null;
  oldValues: string | null;
  newValues: string | null;
  ipAddress: string | null;
  timestamp: string;
}

const ACTION_COLORS: Record<string, string> = {
  Create: '#22c55e',
  Update: '#3b82f6',
  Delete: '#ef4444',
  Restore: '#a855f7',
  Block: '#f97316',
  Unblock: '#06b6d4',
  BulkImport: '#8b5cf6',
};

function AdminAuditLogsPage() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  // Filters
  const [action, setAction] = useState('');
  const [entityType, setEntityType] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  // Expanded row
  const [expandedId, setExpandedId] = useState<number | null>(null);

  const pageSize = 30;

  const loadLogs = useCallback(async () => {
    setLoading(true);
    try {
      const result = await adminService.getAuditLogs({
        action: action || undefined,
        entityType: entityType || undefined,
        from: fromDate || undefined,
        to: toDate || undefined,
        page,
        pageSize,
      });
      setLogs(result.items);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);
    } catch {
      setLogs([]);
    } finally {
      setLoading(false);
    }
  }, [action, entityType, fromDate, toDate, page]);

  useEffect(() => {
    loadLogs();
  }, [loadLogs]);

  const handleFilterChange = () => {
    setPage(1);
  };

  const formatDate = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleDateString('ru-RU', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit', second: '2-digit',
    });
  };

  const formatJson = (json: string | null): string => {
    if (!json) return '—';
    try {
      return JSON.stringify(JSON.parse(json), null, 2);
    } catch {
      return json;
    }
  };

  return (
    <div>
      <h1 style={{ marginBottom: '1.5rem' }}>Журнал аудита</h1>

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.5rem', flexWrap: 'wrap', alignItems: 'end' }}>
        <div>
          <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>Действие</label>
          <select
            value={action}
            onChange={e => { setAction(e.target.value); handleFilterChange(); }}
            className="form-select"
            style={{ minWidth: '140px' }}
          >
            <option value="">Все</option>
            <option value="Create">Create</option>
            <option value="Update">Update</option>
            <option value="Delete">Delete</option>
            <option value="Restore">Restore</option>
            <option value="Block">Block</option>
            <option value="Unblock">Unblock</option>
            <option value="BulkImport">BulkImport</option>
          </select>
        </div>

        <div>
          <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>Сущность</label>
          <select
            value={entityType}
            onChange={e => { setEntityType(e.target.value); handleFilterChange(); }}
            className="form-select"
            style={{ minWidth: '140px' }}
          >
            <option value="">Все</option>
            <option value="Question">Question</option>
            <option value="User">User</option>
            <option value="Topic">Topic</option>
          </select>
        </div>

        <div>
          <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>С даты</label>
          <input
            type="date"
            value={fromDate}
            onChange={e => { setFromDate(e.target.value); handleFilterChange(); }}
            className="form-input"
          />
        </div>

        <div>
          <label style={{ display: 'block', fontSize: '0.75rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>По дату</label>
          <input
            type="date"
            value={toDate}
            onChange={e => { setToDate(e.target.value); handleFilterChange(); }}
            className="form-input"
          />
        </div>

        <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', alignSelf: 'center' }}>
          {totalCount} записей
        </div>
      </div>

      {/* Table */}
      {loading ? (
        <div className="loading-skeleton" style={{ height: '400px', borderRadius: '12px' }} />
      ) : logs.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
          Нет записей аудита
        </div>
      ) : (
        <div style={{ overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ borderBottom: '2px solid var(--border-color)' }}>
                <th style={thStyle}>Время</th>
                <th style={thStyle}>Пользователь</th>
                <th style={thStyle}>Действие</th>
                <th style={thStyle}>Сущность</th>
                <th style={thStyle}>ID</th>
                <th style={thStyle}>IP</th>
                <th style={thStyle}></th>
              </tr>
            </thead>
            <tbody>
              {logs.map(log => (
                <>
                  <tr
                    key={log.id}
                    style={{
                      borderBottom: '1px solid var(--border-color)',
                      cursor: (log.oldValues || log.newValues) ? 'pointer' : 'default',
                    }}
                    onClick={() => {
                      if (log.oldValues || log.newValues) {
                        setExpandedId(expandedId === log.id ? null : log.id);
                      }
                    }}
                  >
                    <td style={tdStyle}>{formatDate(log.timestamp)}</td>
                    <td style={tdStyle}>
                      <span style={{ fontSize: '0.85rem' }}>{log.userEmail}</span>
                    </td>
                    <td style={tdStyle}>
                      <span style={{
                        padding: '0.15rem 0.5rem',
                        borderRadius: '999px',
                        fontSize: '0.75rem',
                        fontWeight: 600,
                        color: '#fff',
                        background: ACTION_COLORS[log.action] || '#6b7280',
                      }}>
                        {log.action}
                      </span>
                    </td>
                    <td style={tdStyle}>{log.entityType}</td>
                    <td style={{ ...tdStyle, fontFamily: 'monospace', fontSize: '0.85rem' }}>{log.entityId || '—'}</td>
                    <td style={{ ...tdStyle, fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{log.ipAddress || '—'}</td>
                    <td style={tdStyle}>
                      {(log.oldValues || log.newValues) && (
                        <span style={{ fontSize: '0.8rem', color: 'var(--primary-color)' }}>
                          {expandedId === log.id ? '▲' : '▼'}
                        </span>
                      )}
                    </td>
                  </tr>
                  {expandedId === log.id && (
                    <tr key={`${log.id}-detail`}>
                      <td colSpan={7} style={{ padding: '0.75rem 1rem', background: 'var(--bg-secondary)' }}>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                          {log.oldValues && (
                            <div>
                              <div style={{ fontWeight: 600, marginBottom: '0.25rem', color: '#ef4444', fontSize: '0.8rem' }}>До изменения</div>
                              <pre style={preStyle}>{formatJson(log.oldValues)}</pre>
                            </div>
                          )}
                          {log.newValues && (
                            <div>
                              <div style={{ fontWeight: 600, marginBottom: '0.25rem', color: '#22c55e', fontSize: '0.8rem' }}>После изменения</div>
                              <pre style={preStyle}>{formatJson(log.newValues)}</pre>
                            </div>
                          )}
                        </div>
                      </td>
                    </tr>
                  )}
                </>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginTop: '1.5rem', alignItems: 'center' }}>
          <button
            className="btn btn-outline"
            disabled={page <= 1}
            onClick={() => setPage(1)}
            style={{ padding: '0.3rem 0.6rem', fontSize: '0.85rem' }}
          >«</button>
          <button
            className="btn btn-outline"
            disabled={page <= 1}
            onClick={() => setPage(p => p - 1)}
            style={{ padding: '0.3rem 0.6rem', fontSize: '0.85rem' }}
          >‹</button>
          <span style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
            {page} / {totalPages}
          </span>
          <button
            className="btn btn-outline"
            disabled={page >= totalPages}
            onClick={() => setPage(p => p + 1)}
            style={{ padding: '0.3rem 0.6rem', fontSize: '0.85rem' }}
          >›</button>
          <button
            className="btn btn-outline"
            disabled={page >= totalPages}
            onClick={() => setPage(totalPages)}
            style={{ padding: '0.3rem 0.6rem', fontSize: '0.85rem' }}
          >»</button>
        </div>
      )}
    </div>
  );
}

const thStyle: React.CSSProperties = {
  textAlign: 'left',
  padding: '0.75rem 0.5rem',
  fontSize: '0.8rem',
  fontWeight: 600,
  color: 'var(--text-secondary)',
  textTransform: 'uppercase',
  letterSpacing: '0.05em',
};

const tdStyle: React.CSSProperties = {
  padding: '0.6rem 0.5rem',
  fontSize: '0.9rem',
};

const preStyle: React.CSSProperties = {
  background: 'var(--bg-primary)',
  padding: '0.5rem',
  borderRadius: '6px',
  fontSize: '0.75rem',
  overflow: 'auto',
  maxHeight: '200px',
  margin: 0,
  whiteSpace: 'pre-wrap',
  wordBreak: 'break-word',
};

export default AdminAuditLogsPage;
