import { useState } from 'react';
import adminService from '../services/adminService';
import { useTranslation } from '../hooks/useTranslation';

function AdminImportPage() {
  const { t } = useTranslation();
  const [importJson, setImportJson] = useState('');
  const [importResult, setImportResult] = useState<{ imported: number; failed: number; errors: string[] } | null>(null);
  const [importLoading, setImportLoading] = useState(false);

  const handleImport = async () => {
    try {
      setImportLoading(true);
      setImportResult(null);
      const parsed = JSON.parse(importJson);
      const arr = Array.isArray(parsed) ? parsed : parsed.questions;
      const result = await adminService.bulkImport(arr);
      setImportResult(result);
    } catch (e) {
      setImportResult({ imported: 0, failed: 0, errors: [`JSON parse error: ${e}`] });
    } finally {
      setImportLoading(false);
    }
  };

  return (
    <div className="animate-fade-in" style={{ padding: '2rem 0' }}>
      <h1 style={{ marginBottom: '0.5rem' }}>{t.admin.import.title}</h1>
      <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem' }}>
        {t.admin.import.subtitle}
      </p>

      <div className="card" style={{ padding: '1.5rem' }}>
        <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '1rem' }}>
          {t.admin.import.instructions} <code>topicId</code>, <code>text</code>, <code>difficulty</code>, <code>answerOptions[]</code>.
        </p>
        <textarea
          value={importJson}
          onChange={e => setImportJson(e.target.value)}
          rows={14}
          placeholder={`[\n  {\n    "topicId": 1,\n    "text": "What is 2+2?",\n    "difficulty": "Easy",\n    "explanation": "Basic addition",\n    "answerOptions": [\n      { "text": "3", "isCorrect": false },\n      { "text": "4", "isCorrect": true },\n      { "text": "5", "isCorrect": false },\n      { "text": "6", "isCorrect": false }\n    ]\n  }\n]`}
          style={{
            width: '100%', fontFamily: 'monospace', fontSize: '0.85rem', padding: '0.75rem',
            borderRadius: '0.5rem', border: '1px solid var(--border-color)',
            background: 'var(--background-color)', color: 'var(--text-primary)',
            resize: 'vertical'
          }}
        />
        <button
          className="btn btn-primary"
          style={{ marginTop: '1rem' }}
          disabled={importLoading || !importJson.trim()}
          onClick={handleImport}
        >
          {importLoading ? t.admin.import.importing : t.admin.import.importBtn}
        </button>

        {importResult && (
          <div style={{
            marginTop: '1rem', padding: '1rem', borderRadius: '0.5rem',
            background: importResult.failed > 0 ? 'rgba(239,68,68,0.08)' : 'rgba(16,185,129,0.08)',
            border: `1px solid ${importResult.failed > 0 ? 'var(--error-color)' : 'var(--success-color)'}`
          }}>
            <div style={{ fontWeight: 600, marginBottom: '0.5rem' }}>
              {t.admin.import.imported} {importResult.imported} / {importResult.imported + importResult.failed}
            </div>
            {importResult.errors.length > 0 && (
              <div style={{ fontSize: '0.85rem', color: 'var(--error-color)' }}>
                {importResult.errors.map((e, i) => <div key={i}>• {e}</div>)}
              </div>
            )}
          </div>
        )}
      </div>

      {/* Reference */}
      <div className="card" style={{ padding: '1.5rem', marginTop: '1rem' }}>
        <h3 style={{ marginBottom: '0.75rem' }}>{t.admin.import.formatTitle}</h3>
        <pre style={{
          background: 'var(--background-color)', padding: '1rem', borderRadius: '0.5rem',
          fontSize: '0.8rem', overflow: 'auto', color: 'var(--text-primary)'
        }}>
{`{
  "topicId": number,       ${t.admin.import.commentTopicId}
  "text": string,          ${t.admin.import.commentQuestionText}
  "difficulty": string,    ${t.admin.import.commentDifficulty}
  "explanation": string?,  ${t.admin.import.commentExplanation}
  "answerOptions": [       ${t.admin.import.commentOptions}
    { "text": string, "isCorrect": boolean }
  ]
}`}
        </pre>
        <p style={{ color: 'var(--text-secondary)', fontSize: '0.8rem', marginTop: '0.75rem' }}>
          IRT-параметры (b, a, c) рассчитываются автоматически на основе difficulty.
        </p>
      </div>
    </div>
  );
}

export default AdminImportPage;
