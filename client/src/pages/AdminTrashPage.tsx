import { useState, useEffect, useCallback } from 'react';
import adminService from '../services/adminService';
import type { TrashItem } from '../types';
import { useTranslation } from '../hooks/useTranslation';
import { getDateLocale } from '../i18n';

type TabFilter = 'all' | 'User' | 'Question';

function AdminTrashPage() {
  const { t } = useTranslation();
  const [items, setItems] = useState<TrashItem[]>([]);
  const [totalUsers, setTotalUsers] = useState(0);
  const [totalQuestions, setTotalQuestions] = useState(0);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState<TabFilter>('all');
  const [actionLoading, setActionLoading] = useState<number | null>(null);

  const loadTrash = useCallback(async () => {
    setLoading(true);
    try {
      const data = await adminService.getTrash();
      setItems(data.items);
      setTotalUsers(data.totalUsers);
      setTotalQuestions(data.totalQuestions);
    } catch {
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadTrash();
  }, [loadTrash]);

  const filtered = tab === 'all' ? items : items.filter(i => i.entityType === tab);

  const handleRestore = async (item: TrashItem) => {
    if (!confirm(`${t.admin.trash.restoreConfirm} "${item.displayName}"?`)) return;
    setActionLoading(item.id);
    try {
      if (item.entityType === 'Question') {
        await adminService.restoreQuestion(item.id);
      } else {
        await adminService.restoreUser(item.id);
      }
      await loadTrash();
    } catch {
      alert(t.admin.trash.restoreError);
    } finally {
      setActionLoading(null);
    }
  };

  const handleHardDelete = async (item: TrashItem) => {
    if (!confirm(`${t.admin.trash.permanentDeleteConfirm} "${item.displayName}"?`)) return;
    setActionLoading(item.id);
    try {
      if (item.entityType === 'Question') {
        await adminService.hardDeleteQuestion(item.id);
      } else {
        await adminService.hardDeleteUser(item.id);
      }
      await loadTrash();
    } catch {
      alert(t.admin.trash.deleteError);
    } finally {
      setActionLoading(null);
    }
  };

  const handleEmptyTrash = async () => {
    if (!confirm(`${t.admin.trash.emptyConfirm} (${items.length})?`)) return;
    setActionLoading(-1);
    try {
      await adminService.emptyTrash();
      await loadTrash();
    } catch {
      alert(t.admin.trash.emptyError);
    } finally {
      setActionLoading(null);
    }
  };

  const formatDate = (iso: string | null) => {
    if (!iso) return '—';
    return new Date(iso).toLocaleDateString(getDateLocale(), {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit',
    });
  };

  const getDaysColor = (days: number) => {
    if (days <= 3) return '#ef4444';
    if (days <= 7) return '#f97316';
    if (days <= 14) return '#eab308';
    return 'var(--text-secondary)';
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h1 style={{ marginBottom: '0.25rem' }}>{t.admin.trash.title}</h1>
          <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', margin: 0 }}>
            {t.admin.trash.subtitle}
          </p>
        </div>
        {items.length > 0 && (
          <button
            onClick={handleEmptyTrash}
            disabled={actionLoading !== null}
            className="btn"
            style={{
              background: '#ef4444', color: '#fff', border: 'none',
              opacity: actionLoading !== null ? 0.6 : 1,
            }}
          >
            {actionLoading === -1 ? t.admin.trash.emptying : t.admin.trash.emptyBtn}
          </button>
        )}
      </div>

      {/* Summary cards */}
      <div style={{ display: 'flex', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        {[
          { label: t.admin.trash.totalLabel, count: items.length, color: 'var(--text-primary)' },
          { label: t.admin.trash.usersLabel, count: totalUsers, color: '#3b82f6' },
          { label: t.admin.trash.questionsLabel, count: totalQuestions, color: '#a855f7' },
        ].map(c => (
          <div key={c.label} className="card" style={{ padding: '1rem 1.5rem', minWidth: '140px', textAlign: 'center' }}>
            <div style={{ fontSize: '1.75rem', fontWeight: 700, color: c.color }}>{c.count}</div>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{c.label}</div>
          </div>
        ))}
      </div>

      {/* Tab filters */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1rem' }}>
        {([
          ['all', t.admin.trash.filterAll],
          ['User', `${t.admin.trash.filterUsers} (${totalUsers})`],
          ['Question', `${t.admin.trash.filterQuestions} (${totalQuestions})`],
        ] as [TabFilter, string][]).map(([key, label]) => (
          <button
            key={key}
            onClick={() => setTab(key)}
            className="btn"
            style={{
              background: tab === key ? 'var(--primary-color)' : 'var(--bg-secondary)',
              color: tab === key ? '#fff' : 'var(--text-primary)',
              border: 'none',
              fontSize: '0.85rem',
              padding: '0.4rem 1rem',
              borderRadius: '8px',
            }}
          >
            {label}
          </button>
        ))}
      </div>

      {/* Table */}
      {loading ? (
        <div className="loading-skeleton" style={{ height: '400px', borderRadius: '12px' }} />
      ) : filtered.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>
          {t.admin.trash.empty}
        </div>
      ) : (
        <div className="card" style={{ overflow: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.88rem' }}>
            <thead>
              <tr style={{ borderBottom: '2px solid var(--border-color)' }}>
                <th style={th}>{t.admin.trash.typeCol}</th>
                <th style={th}>{t.admin.trash.nameCol}</th>
                <th style={th}>{t.admin.trash.detailsCol}</th>
                <th style={th}>{t.admin.trash.deletedCol}</th>
                <th style={th}>{t.admin.trash.daysLeftCol}</th>
                <th style={{ ...th, textAlign: 'right' }}>{t.admin.trash.actionsCol}</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map(item => (
                <tr key={`${item.entityType}-${item.id}`} style={{ borderBottom: '1px solid var(--border-color)' }}>
                  <td style={td}>
                    <span style={{
                      display: 'inline-block', padding: '0.15rem 0.5rem', borderRadius: '4px',
                      fontSize: '0.75rem', fontWeight: 600,
                      background: item.entityType === 'User' ? '#3b82f620' : '#a855f720',
                      color: item.entityType === 'User' ? '#3b82f6' : '#a855f7',
                    }}>
                      {item.entityType === 'User' ? t.admin.trash.typeUser : t.admin.trash.typeQuestion}
                    </span>
                  </td>
                  <td style={{ ...td, maxWidth: '300px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    {item.displayName}
                  </td>
                  <td style={{ ...td, color: 'var(--text-secondary)', fontSize: '0.82rem' }}>
                    {item.detail || '—'}
                  </td>
                  <td style={{ ...td, fontSize: '0.82rem' }}>
                    {formatDate(item.deletedAt)}
                  </td>
                  <td style={td}>
                    <span style={{ fontWeight: 600, color: getDaysColor(item.daysUntilPurge) }}>
                      {item.daysUntilPurge} {t.admin.trash.daysShort}
                    </span>
                  </td>
                  <td style={{ ...td, textAlign: 'right', whiteSpace: 'nowrap' }}>
                    <button
                      onClick={() => handleRestore(item)}
                      disabled={actionLoading !== null}
                      className="btn"
                      style={{
                        fontSize: '0.78rem', padding: '0.25rem 0.6rem',
                        background: '#22c55e', color: '#fff', border: 'none',
                        marginRight: '0.4rem', borderRadius: '6px',
                        opacity: actionLoading === item.id ? 0.6 : 1,
                      }}
                    >
                      {t.admin.trash.restoreBtn}
                    </button>
                    <button
                      onClick={() => handleHardDelete(item)}
                      disabled={actionLoading !== null}
                      className="btn"
                      style={{
                        fontSize: '0.78rem', padding: '0.25rem 0.6rem',
                        background: '#ef4444', color: '#fff', border: 'none',
                        borderRadius: '6px',
                        opacity: actionLoading === item.id ? 0.6 : 1,
                      }}
                    >
                      {t.admin.trash.permanentDeleteBtn}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

const th: React.CSSProperties = {
  textAlign: 'left', padding: '0.75rem 0.5rem', fontSize: '0.78rem',
  color: 'var(--text-secondary)', fontWeight: 600, textTransform: 'uppercase',
  letterSpacing: '0.05em',
};

const td: React.CSSProperties = {
  padding: '0.65rem 0.5rem', verticalAlign: 'middle',
};

export default AdminTrashPage;
