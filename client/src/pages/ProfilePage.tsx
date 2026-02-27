import { useEffect, useState } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { fetchExams, toggleExamSelection } from '../store/slices/examSlice';
import { subscriptionService } from '../services/subscriptionService';
import type { SubscriptionStatus } from '../types';

function ProfilePage() {
  const { user } = useAppSelector((state) => state.auth);
  const { exams, selectedExams } = useAppSelector((state) => state.exam);
  const dispatch = useAppDispatch();
  const [sub, setSub] = useState<SubscriptionStatus | null>(null);

  useEffect(() => {
    dispatch(fetchExams());
    subscriptionService.getStatus().then(setSub).catch(() => {});
  }, [dispatch]);

  const isPro = user?.subscriptionTier === 'Pro';

  return (
    <div className="animate-fade-in" style={{ maxWidth: '640px', margin: '0 auto', padding: '2rem 0' }}>
      <h1 style={{ marginBottom: '1.5rem' }}>👤 Профиль</h1>

      {/* ─── User Info Card ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1rem' }}>
          <div style={{
            width: '56px', height: '56px', borderRadius: '50%',
            background: 'var(--primary-color)', color: '#fff',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: '1.25rem', fontWeight: 700, flexShrink: 0,
          }}>
            {user?.name?.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2) || '?'}
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.2rem' }}>{user?.name}</h2>
            <p style={{ margin: '0.15rem 0 0', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{user?.email}</p>
          </div>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem', fontSize: '0.85rem' }}>
          <InfoRow label="Роль" value={user?.role === 'Student' ? 'Студент' : user?.role === 'Tutor' ? 'Репетитор' : user?.role || '—'} />
          <InfoRow label="Регистрация" value={user?.createdAt ? new Date(user.createdAt).toLocaleDateString('ru') : '—'} />
        </div>
      </div>

      {/* ─── Subscription ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>💳 Подписка</h3>
        <div style={{
          display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.75rem',
        }}>
          <span style={{
            padding: '0.3rem 0.75rem', borderRadius: '999px', fontWeight: 700, fontSize: '0.85rem',
            background: isPro ? 'linear-gradient(135deg, #f59e0b, #ef4444)' : 'var(--bg-secondary)',
            color: isPro ? '#fff' : 'var(--text-secondary)',
          }}>
            {isPro ? '⭐ PRO' : 'FREE'}
          </span>
          {isPro && user?.subscriptionExpiresAt && (
            <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
              до {new Date(user.subscriptionExpiresAt).toLocaleDateString('ru')}
            </span>
          )}
        </div>

        {sub && (
          <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
            <p style={{ margin: '0.25rem 0' }}>Вопросов сегодня: {sub.dailyUsage.questionsAnswered} / {sub.limits.questionsPerDay === -1 ? '∞' : sub.limits.questionsPerDay}</p>
            <p style={{ margin: '0.25rem 0' }}>Уроков сегодня: {sub.dailyUsage.lessonsViewed} / {sub.limits.lessonsPerDay === -1 ? '∞' : sub.limits.lessonsPerDay}</p>
          </div>
        )}

        {!isPro && (
          <button className="btn btn-primary" style={{ marginTop: '0.75rem', fontSize: '0.85rem' }}
            onClick={() => { /* TODO: upgrade flow */ }}>
            ⬆️ Перейти на PRO
          </button>
        )}
      </div>

      {/* ─── Selected Exams ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>📝 Мои экзамены</h3>
        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
          {exams.map(exam => {
            const isSelected = selectedExams.includes(exam.code);
            return (
              <button
                key={exam.code}
                onClick={() => dispatch(toggleExamSelection(exam.code))}
                style={{
                  padding: '0.5rem 1rem', borderRadius: '999px', border: '2px solid',
                  borderColor: isSelected ? 'var(--primary-color)' : 'var(--border-color)',
                  background: isSelected ? 'rgba(79,70,229,0.1)' : 'transparent',
                  color: isSelected ? 'var(--primary-color)' : 'var(--text-secondary)',
                  fontWeight: 600, cursor: 'pointer', transition: 'all 0.2s', fontSize: '0.9rem',
                }}
              >
                {isSelected ? '✓ ' : ''}{exam.code}
              </button>
            );
          })}
        </div>
        {selectedExams.length === 0 && (
          <p style={{ color: 'var(--warning-color)', fontSize: '0.85rem', marginTop: '0.5rem' }}>
            Выберите хотя бы один экзамен
          </p>
        )}
      </div>
    </div>
  );
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <div style={{ color: 'var(--text-secondary)', fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.04em' }}>{label}</div>
      <div style={{ fontWeight: 500 }}>{value}</div>
    </div>
  );
}

export default ProfilePage;
