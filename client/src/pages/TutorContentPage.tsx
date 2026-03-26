import { useState, useEffect } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { tutorService } from '../services/tutorService';
import { useTranslation } from '../i18n';
import AdminContentPage from './AdminContentPage';

function TutorContentPage() {
  const { user } = useAppSelector(state => state.auth);
  const { t } = useTranslation();
  const [loading, setLoading] = useState(true);
  const [allowed, setAllowed] = useState(false);

  useEffect(() => {
    if (!user) return;
    tutorService.getTutorProfile(user.id).then(profile => {
      // School tutors: content allowed (school pays)
      // Free tutors: need paid subscription
      setAllowed(!!profile.schoolId || profile.hasPaidSubscription);
    }).catch(() => {}).finally(() => setLoading(false));
  }, [user]);

  if (loading) return <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-secondary)' }}>...</div>;

  if (!allowed) {
    return (
      <div className="animate-fade-in" style={{ display: 'flex', justifyContent: 'center', padding: '3rem 1rem' }}>
        <div className="card" style={{ padding: '2rem', maxWidth: '500px', textAlign: 'center' }}>
          <div style={{ fontSize: '2.5rem', marginBottom: '0.75rem' }}>&#128274;</div>
          <h2 style={{ margin: '0 0 0.5rem' }}>{t.tutorSubscription.title}</h2>
          <p style={{ color: 'var(--text-secondary)', lineHeight: 1.5, marginBottom: '1rem' }}>
            {t.tutorSubscription.description}
          </p>
          <div className="card" style={{ padding: '1rem', background: 'var(--bg-secondary)', textAlign: 'left' }}>
            <div style={{ fontWeight: 600, marginBottom: '0.5rem' }}>{t.tutorSubscription.howTo}</div>
            <ol style={{ margin: 0, paddingLeft: '1.25rem', color: 'var(--text-secondary)', fontSize: '0.88rem', lineHeight: 1.7 }}>
              <li>{t.tutorSubscription.step1}</li>
              <li>{t.tutorSubscription.step2}</li>
              <li>{t.tutorSubscription.step3}</li>
            </ol>
          </div>
        </div>
      </div>
    );
  }

  return <AdminContentPage />;
}

export default TutorContentPage;
