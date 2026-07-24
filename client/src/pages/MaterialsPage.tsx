import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { materialsService, type StudyMaterial } from '../services/materialsService';
import { purchaseService } from '../services/purchaseService';

function MaterialsPage() {
  const navigate = useNavigate();
  const [materials, setMaterials] = useState<StudyMaterial[] | null>(null);
  const [ownedIds, setOwnedIds] = useState<Set<string>>(new Set());
  const [downloading, setDownloading] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    materialsService.list().then(setMaterials).catch(() => setMaterials([]));
    purchaseService
      .list()
      .then((ps) => setOwnedIds(new Set(ps.filter((p) => p.itemType === 'book').map((p) => p.itemCode))))
      .catch(() => {});
  }, []);

  const download = async (id: number) => {
    setDownloading(id);
    setError(null);
    try {
      const { pdfUrl } = await materialsService.download(id);
      window.open(pdfUrl, '_blank', 'noopener');
    } catch (e) {
      const serverMsg = (e as { response?: { data?: { error?: string } } })?.response?.data?.error;
      setError(serverMsg || 'Не удалось получить файл. Возможно, материал ещё не куплен.');
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
      ) : (() => {
        const owned = materials.filter((m) => ownedIds.has(String(m.id)));
        if (owned.length === 0) {
          return (
            <div className="card" style={{ textAlign: 'center', padding: '2rem' }}>
              <p style={{ color: 'var(--text-secondary)', marginBottom: '1rem' }}>
                У вас пока нет материалов. Приобретите учебники на Главной.
              </p>
              <button className="btn btn-primary" onClick={() => navigate('/')}>Приобрести на Главной</button>
            </div>
          );
        }
        return (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '0.75rem' }}>
          {owned.map((m) => (
            <div key={m.id} className="card" style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
              <div style={{ fontWeight: 700 }}>{m.title}</div>
              {m.description && (
                <div style={{ fontSize: '0.82rem', color: 'var(--text-secondary)' }}>{m.description}</div>
              )}
              <button className="btn btn-primary" style={{ width: '100%', marginTop: 'auto' }}
                      disabled={downloading === m.id}
                      onClick={() => download(m.id)}>
                {downloading === m.id ? 'Открываю…' : '⤓ Скачать PDF'}
              </button>
            </div>
          ))}
        </div>
        );
      })()}
    </div>
  );
}

export default MaterialsPage;
