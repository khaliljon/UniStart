import { useState, useEffect, useCallback, useRef } from 'react';
import adminService from '../services/adminService';

type SystemHealth = Awaited<ReturnType<typeof adminService.getSystemHealth>>;

const STATUS_COLORS: Record<string, string> = {
  Healthy: '#22c55e',
  Degraded: '#f59e0b',
  Unhealthy: '#ef4444',
  Succeeded: '#22c55e',
  Processing: '#3b82f6',
  Failed: '#ef4444',
  Scheduled: '#a855f7',
  Enqueued: '#f59e0b',
};

function formatUptime(minutes: number): string {
  if (minutes < 60) return `${Math.round(minutes)} мин`;
  if (minutes < 1440) return `${Math.floor(minutes / 60)} ч ${Math.round(minutes % 60)} мин`;
  const days = Math.floor(minutes / 1440);
  const hours = Math.floor((minutes % 1440) / 60);
  return `${days} д ${hours} ч`;
}

function formatDate(iso: string | null): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('ru-RU', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit', second: '2-digit'
  });
}

export default function AdminSystemHealthPage() {
  const [data, setData] = useState<SystemHealth | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [triggering, setTriggering] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setError('');
      const result = await adminService.getSystemHealth();
      setData(result);
    } catch {
      setError('Не удалось загрузить данные о системе');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  // Auto-refresh every 30 seconds
  useEffect(() => {
    const timer = setInterval(load, 30000);
    return () => clearInterval(timer);
  }, [load]);

  const triggerTimerRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  useEffect(() => {
    return () => { clearTimeout(triggerTimerRef.current); };
  }, []);

  const handleTrigger = async (jobId: string) => {
    setTriggering(jobId);
    try {
      await adminService.triggerJob(jobId);
      clearTimeout(triggerTimerRef.current);
      triggerTimerRef.current = setTimeout(load, 2000);
    } catch { /* ignore */ }
    setTriggering(null);
  };

  if (loading) return <div style={{ padding: 32, textAlign: 'center' }}>Загрузка...</div>;
  if (error) return <div style={{ padding: 32, color: '#ef4444' }}>{error}</div>;
  if (!data) return null;

  const overallColor = STATUS_COLORS[data.status] || '#6b7280';

  return (
    <div style={{ maxWidth: 1100, margin: '0 auto' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <h2 style={{ margin: 0 }}>Здоровье системы</h2>
        <button onClick={load} style={{
          padding: '8px 16px', borderRadius: 8, border: '1px solid var(--border)',
          background: 'var(--bg-secondary)', cursor: 'pointer', color: 'var(--text-primary)'
        }}>Обновить</button>
      </div>

      {/* Overall Status Banner */}
      <div style={{
        padding: '16px 24px', borderRadius: 12, marginBottom: 24,
        background: overallColor + '18', border: `2px solid ${overallColor}`,
        display: 'flex', alignItems: 'center', gap: 12
      }}>
        <span style={{ fontSize: 28 }}>{data.status === 'Healthy' ? '' : data.status === 'Degraded' ? '' : ''}</span>
        <div>
          <div style={{ fontWeight: 700, fontSize: 18, color: overallColor }}>
            {data.status === 'Healthy' ? 'Система работает нормально' :
             data.status === 'Degraded' ? 'Система работает с ограничениями' :
             'Система недоступна'}
          </div>
          <div style={{ fontSize: 13, color: 'var(--text-secondary)', marginTop: 2 }}>
            Обновлено: {formatDate(data.system.serverTime)}
          </div>
        </div>
      </div>

      {/* Cards Grid */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 16, marginBottom: 24 }}>
        <StatCard label="Окружение" value={data.system.environment} icon="" />
        <StatCard label="Uptime" value={formatUptime(data.system.uptime)} icon="" />
        <StatCard label="Память" value={`${data.system.memoryMB.toFixed(0)} MB`} icon="" />
        <StatCard label=".NET" value={data.system.dotnetVersion} icon="" />
        <StatCard label="Потоки" value={String(data.system.threadCount)} icon="" />
        <StatCard label="Машина" value={data.system.machineName} icon="" />
      </div>

      {/* Two-column layout */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24, marginBottom: 24 }}>
        {/* Health Checks */}
        <div style={{ background: 'var(--bg-secondary)', borderRadius: 12, padding: 20 }}>
          <h3 style={{ margin: '0 0 16px' }}>Health Checks</h3>
          {data.healthChecks.length === 0 ? (
            <div style={{ color: 'var(--text-secondary)' }}>Нет проверок</div>
          ) : data.healthChecks.map(hc => (
            <div key={hc.name} style={{
              display: 'flex', justifyContent: 'space-between', alignItems: 'center',
              padding: '10px 0', borderBottom: '1px solid var(--border)'
            }}>
              <div>
                <div style={{ fontWeight: 600 }}>{hc.name}</div>
                {hc.error && <div style={{ fontSize: 12, color: '#ef4444', marginTop: 2 }}>{hc.error}</div>}
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <span style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{hc.duration.toFixed(0)}ms</span>
                <span style={{
                  padding: '2px 10px', borderRadius: 12, fontSize: 12, fontWeight: 600,
                  color: '#fff', background: STATUS_COLORS[hc.status] || '#6b7280'
                }}>{hc.status}</span>
              </div>
            </div>
          ))}
        </div>

        {/* Database Stats */}
        <div style={{ background: 'var(--bg-secondary)', borderRadius: 12, padding: 20 }}>
          <h3 style={{ margin: '0 0 16px' }}>База данных</h3>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <DbStat label="Пользователей" value={data.database.totalUsers} />
            <DbStat label="Вопросов" value={data.database.totalQuestions} />
            <DbStat label="Ответов" value={data.database.totalAnswers} />
            <DbStat label="Сессий" value={data.database.totalSessions} />
            <DbStat label="Активных сегодня" value={data.database.activeUsersToday} highlight />
            <DbStat label="Ответов сегодня" value={data.database.answersToday} highlight />
          </div>
        </div>
      </div>

      {/* Recurring Jobs */}
      <div style={{ background: 'var(--bg-secondary)', borderRadius: 12, padding: 20 }}>
        <h3 style={{ margin: '0 0 16px' }}>Фоновые задачи (Hangfire)</h3>
        {!data.recurringJobs || data.recurringJobs.length === 0 ? (
          <div style={{ color: 'var(--text-secondary)' }}>Нет зарегистрированных задач</div>
        ) : (
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ borderBottom: '2px solid var(--border)' }}>
                {['Задача', 'Расписание', 'Последний запуск', 'Следующий запуск', 'Статус', ''].map(h => (
                  <th key={h} style={{ padding: '8px 12px', textAlign: 'left', fontSize: 13, color: 'var(--text-secondary)' }}>{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {data.recurringJobs.map(job => (
                <tr key={job.id} style={{ borderBottom: '1px solid var(--border)' }}>
                  <td style={{ padding: '10px 12px', fontWeight: 600 }}>{job.id}</td>
                  <td style={{ padding: '10px 12px', fontFamily: 'monospace', fontSize: 13 }}>{job.cron}</td>
                  <td style={{ padding: '10px 12px', fontSize: 13 }}>{formatDate(job.lastExecution)}</td>
                  <td style={{ padding: '10px 12px', fontSize: 13 }}>{formatDate(job.nextExecution)}</td>
                  <td style={{ padding: '10px 12px' }}>
                    {job.lastJobState ? (
                      <span style={{
                        padding: '2px 10px', borderRadius: 12, fontSize: 12, fontWeight: 600,
                        color: '#fff', background: STATUS_COLORS[job.lastJobState] || '#6b7280'
                      }}>{job.lastJobState}</span>
                    ) : <span style={{ color: 'var(--text-secondary)', fontSize: 13 }}>—</span>}
                  </td>
                  <td style={{ padding: '10px 12px' }}>
                    <button
                      onClick={() => handleTrigger(job.id)}
                      disabled={triggering === job.id}
                      style={{
                        padding: '4px 12px', borderRadius: 6, border: '1px solid var(--border)',
                        background: 'var(--bg-primary)', cursor: 'pointer', fontSize: 12,
                        color: 'var(--text-primary)', opacity: triggering === job.id ? 0.5 : 1
                      }}
                    >
                      {triggering === job.id ? '...' : '▶ Запустить'}
                    </button>
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

function StatCard({ label, value, icon }: { label: string; value: string; icon: string }) {
  return (
    <div style={{
      background: 'var(--bg-secondary)', borderRadius: 12, padding: '16px 20px',
      display: 'flex', alignItems: 'center', gap: 12
    }}>
      <span style={{ fontSize: 24 }}>{icon}</span>
      <div>
        <div style={{ fontSize: 12, color: 'var(--text-secondary)' }}>{label}</div>
        <div style={{ fontWeight: 700, fontSize: 16 }}>{value}</div>
      </div>
    </div>
  );
}

function DbStat({ label, value, highlight }: { label: string; value: number; highlight?: boolean }) {
  return (
    <div style={{
      padding: 12, borderRadius: 8,
      background: highlight ? 'var(--accent-color)11' : 'var(--bg-primary)'
    }}>
      <div style={{ fontSize: 12, color: 'var(--text-secondary)', marginBottom: 4 }}>{label}</div>
      <div style={{ fontWeight: 700, fontSize: 20, color: highlight ? 'var(--accent-color)' : 'var(--text-primary)' }}>
        {value.toLocaleString('ru-RU')}
      </div>
    </div>
  );
}
