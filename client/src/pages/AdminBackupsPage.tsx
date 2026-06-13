import { useEffect, useState, useCallback } from 'react';
import adminService from '../services/adminService';

interface BackupFile {
  fileName: string;
  sizeBytes: number;
  createdAtUtc: string;
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function AdminBackupsPage() {
  const [backups, setBackups] = useState<BackupFile[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState('');
  const [info, setInfo] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    try {
      setBackups(await adminService.listBackups());
    } catch {
      setError('Не удалось загрузить список резервных копий.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const handleCreate = async () => {
    setCreating(true);
    setError('');
    setInfo('');
    try {
      const created = await adminService.createBackup();
      setInfo(`Создана резервная копия: ${created.fileName} (${formatSize(created.sizeBytes)})`);
      await load();
    } catch (e: unknown) {
      const resp = (e as { response?: { data?: { error?: string } } })?.response?.data;
      setError(resp?.error || 'Не удалось создать резервную копию.');
    } finally {
      setCreating(false);
    }
  };

  const handleDownload = async (fileName: string) => {
    try {
      const blob = await adminService.downloadBackup(fileName);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(url);
    } catch {
      setError('Не удалось скачать файл.');
    }
  };

  const handleDelete = async (fileName: string) => {
    if (!confirm(`Удалить резервную копию ${fileName}?`)) return;
    try {
      await adminService.deleteBackup(fileName);
      await load();
    } catch {
      setError('Не удалось удалить файл.');
    }
  };

  return (
    <div className="animate-fade-in">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem', flexWrap: 'wrap', gap: '0.5rem' }}>
        <h1 style={{ margin: 0 }}>Резервные копии БД</h1>
        <button className="btn btn-primary" onClick={handleCreate} disabled={creating}>
          {creating ? 'Создание...' : 'Создать копию сейчас'}
        </button>
      </div>

      <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem', marginBottom: '1rem' }}>
        Автоматическое резервное копирование выполняется ежедневно в 01:00 UTC. Хранятся последние 14 копий.
        Копии можно скачать и использовать для восстановления через <code>gunzip</code> + <code>psql</code>.
      </p>

      {error && (
        <div style={{ background: '#fef2f2', color: '#dc2626', padding: '0.75rem 1rem', borderRadius: '0.5rem', marginBottom: '1rem', fontSize: '0.85rem' }}>{error}</div>
      )}
      {info && (
        <div style={{ background: '#f0fdf4', color: '#16a34a', padding: '0.75rem 1rem', borderRadius: '0.5rem', marginBottom: '1rem', fontSize: '0.85rem' }}>{info}</div>
      )}

      <div className="card">
        {loading ? (
          <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Загрузка...</div>
        ) : backups.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>Резервных копий пока нет.</div>
        ) : (
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.88rem' }}>
            <thead>
              <tr style={{ textAlign: 'left', borderBottom: '1px solid var(--border-color)' }}>
                <th style={{ padding: '0.5rem' }}>Файл</th>
                <th style={{ padding: '0.5rem' }}>Дата (UTC)</th>
                <th style={{ padding: '0.5rem' }}>Размер</th>
                <th style={{ padding: '0.5rem', textAlign: 'right' }}>Действия</th>
              </tr>
            </thead>
            <tbody>
              {backups.map(b => (
                <tr key={b.fileName} style={{ borderBottom: '1px solid var(--border-color)' }}>
                  <td style={{ padding: '0.5rem', fontFamily: 'monospace' }}>{b.fileName}</td>
                  <td style={{ padding: '0.5rem' }}>{new Date(b.createdAtUtc).toLocaleString('ru-RU')}</td>
                  <td style={{ padding: '0.5rem' }}>{formatSize(b.sizeBytes)}</td>
                  <td style={{ padding: '0.5rem', textAlign: 'right', whiteSpace: 'nowrap' }}>
                    <button className="btn btn-outline" style={{ fontSize: '0.8rem', padding: '0.25rem 0.6rem', marginRight: '0.4rem' }} onClick={() => handleDownload(b.fileName)}>Скачать</button>
                    <button className="btn btn-outline" style={{ fontSize: '0.8rem', padding: '0.25rem 0.6rem', color: 'var(--error-color)' }} onClick={() => handleDelete(b.fileName)}>Удалить</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

export default AdminBackupsPage;
