import { useState, useEffect, useCallback } from 'react';
import { schoolAdminService } from '../services/schoolAdminService';
import { useTranslation } from '../i18n';
import api from '../services/api';
import type { SchoolTutor } from '../services/schoolAdminService';

function SchoolAdminTutorsPage() {
  const { t } = useTranslation();
  const [tutors, setTutors] = useState<SchoolTutor[]>([]);
  const [loading, setLoading] = useState(true);

  const loadTutors = useCallback(async () => {
    setLoading(true);
    try {
      setTutors(await schoolAdminService.getTutors());
    } catch { /* */ } finally { setLoading(false); }
  }, []);

  useEffect(() => { loadTutors(); }, [loadTutors]);

  return (
    <div>
      <h1 style={{ marginBottom: '1.5rem' }}>{t.schoolAdmin.tutors}</h1>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1rem' }}>
        {tutors.map(tr => (
          <div key={tr.userId} className="card" style={{ padding: '1.25rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
              <div>
                <strong>{tr.name}</strong>
                {tr.isVerified && <span style={{ color: 'var(--success-color)', marginLeft: '0.5rem' }}>&#10003;</span>}
              </div>
              <span style={{ fontSize: '0.8rem', color: tr.isAvailable ? 'var(--success-color)' : 'var(--text-secondary)' }}>
                {tr.isAvailable ? t.schoolAdmin.available : t.schoolAdmin.unavailable}
              </span>
            </div>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', margin: '0.5rem 0' }}>{tr.headline}</div>
            <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{tr.email}</div>
            <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.75rem', fontSize: '0.8rem' }}>
              <span>&#9733; {tr.averageRating.toFixed(1)}</span>
              <span>{tr.totalStudents} {t.schoolAdmin.students.toLowerCase()}</span>
              <span>{tr.specializations.join(', ')}</span>
            </div>
            <div style={{ marginTop: '0.75rem' }}>
              {tr.isVerified ? (
                <button className="btn btn-outline" style={{ fontSize: '0.75rem', padding: '0.25rem 0.7rem', color: 'var(--error-color)', borderColor: 'var(--error-color)' }}
                  onClick={async () => {
                    try {
                      await api.post(`/tutor-school-applications/unverify/${tr.userId}`);
                      loadTutors();
                    } catch { /* */ }
                  }}>{t.schoolAdmin.unverify}</button>
              ) : (
                <button className="btn btn-primary" style={{ fontSize: '0.75rem', padding: '0.25rem 0.7rem' }}
                  onClick={async () => {
                    try {
                      await api.post(`/tutor-school-applications/verify/${tr.userId}`);
                      loadTutors();
                    } catch { /* */ }
                  }}>{t.schoolAdmin.verify}</button>
              )}
            </div>
          </div>
        ))}
        {tutors.length === 0 && !loading && (
          <div className="card" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
            {t.schoolAdmin.noTutors}
          </div>
        )}
      </div>
    </div>
  );
}

export default SchoolAdminTutorsPage;
