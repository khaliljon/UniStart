import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { materialsService, type StudyMaterial } from '../services/materialsService';
import { purchaseService } from '../services/purchaseService';

function MaterialsPage() {
  const navigate = useNavigate();
  const [materials, setMaterials] = useState<StudyMaterial[] | null>(null);
  const [ownedSubjects, setOwnedSubjects] = useState<Set<string>>(new Set());
  const [downloading, setDownloading] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    materialsService.list().then(setMaterials).catch(() => setMaterials([]));
    purchaseService
      .list()
      .then((ps) => setOwnedSubjects(new Set(ps.filter((p) => p.itemType === 'book').map((p) => p.itemCode))))
      .catch(() => {});
  }, []);

  const download = async (subjectKey: string) => {
    setDownloading(subjectKey);
    setError(null);
    try {
      const { pdfUrl } = await materialsService.download(subjectKey);
      window.open(pdfUrl, '_blank', 'noopener');
    } catch {
      setError('Не удалось получить файл. Возможно, материал ещё не куплен.');
    } finally {
      setDownloading(null);
    }
  };

  return (
    <div style={{ maxWidth: 820, margin: '1.5rem auto', padding: '0 1rem' }}>
      <h1 className="csca-h2" style={{ fontSize: '1.6rem', marginBottom: '1.25rem' }}>Материалы</h1>

      {error && (
        <div style={{ padding: '0.6rem 1rem', background: 'var(--error-bg)', color: 'var(--error-color)', borderRadius: 8, marginBottom: '1rem', fontSize: '0.9rem' }}>
          {error}
        </div>
      )}

      {materials === null ? (
        <div className="loading"><div className="spinner" /></div>
      ) : materials.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '2rem' }}>
          <p style={{ color: 'var(--text-secondary)' }}>Учебные материалы скоро появятся.</p>
          <button className="btn btn-primary" onClick={() => navigate('/')}>На Главную</button>
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '0.75rem' }}>
          {materials.map((m) => {
            const owned = ownedSubjects.has(m.subjectKey);
            return (
              <div key={m.id} className="card" style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                <div style={{ fontWeight: 700 }}>{m.title}</div>
                {m.description && (
                  <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)' }}>{m.description}</div>
                )}
                <div style={{ marginTop: 'auto' }}>
                  {owned ? (
                    <button className="btn btn-primary" style={{ width: '100%' }}
                            disabled={downloading === m.subjectKey}
                            onClick={() => download(m.subjectKey)}>
                      {downloading === m.subjectKey ? 'Открываю…' : '⤓ Скачать PDF'}
                    </button>
                  ) : (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.35rem' }}>
                      <div style={{ fontWeight: 800, color: 'var(--primary-color)' }}>
                        {m.price.toLocaleString('ru-RU')} ₸
                      </div>
                      <button className="btn btn-outline" style={{ width: '100%' }} onClick={() => navigate('/')}>
                        Купить на Главной
                      </button>
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

export default MaterialsPage;
