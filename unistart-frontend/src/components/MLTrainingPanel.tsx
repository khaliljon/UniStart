import React, { useState, useEffect } from 'react';
import api from '../services/api';
import './MLTrainingPanel.css';

interface TrainingStats {
  totalRecords: number;
  recordsLast24Hours: number;
  recordsLast7Days: number;
  recordsLast30Days: number;
  canTrain: boolean;
  isModelTrained: boolean;
  lastTrainingDate: string | null;
  uniqueUsers: number;
  uniqueFlashcards: number;
  averageEaseFactor: number;
  averageInterval: number;
  averageRetentionRate: number;
}

export const MLTrainingPanel: React.FC = () => {
  const [stats, setStats] = useState<TrainingStats | null>(null);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error' | 'info', text: string } | null>(null);
  const [syntheticCount, setSyntheticCount] = useState(100);

  useEffect(() => {
    loadStats();
  }, []);

  const loadStats = async () => {
    try {
      const response = await api.get<TrainingStats>('/mltraining/training-stats');
      setStats(response.data);
    } catch (error) {
      console.error('Failed to load stats:', error);
      setMessage({ type: 'error', text: 'Не удалось загрузить статистику' });
    }
  };

  const generateSyntheticData = async () => {
    setLoading(true);
    setMessage(null);
    try {
      const { data } = await api.post(`/mltraining/generate-synthetic-data?count=${syntheticCount}`);
      setMessage({ 
        type: 'success', 
        text: `Сгенерировано ${data.recordsGenerated} записей. Всего: ${data.totalRecords}` 
      });
      await loadStats();
    } catch (error: any) {
      setMessage({ 
        type: 'error', 
        text: error.response?.data?.message || 'Ошибка генерации данных' 
      });
    } finally {
      setLoading(false);
    }
  };

  const retrainModel = async () => {
    setLoading(true);
    setMessage(null);
    try {
      await api.post('/mltraining/retrain');
      setMessage({ 
        type: 'success', 
        text: 'Модель успешно переобучена!' 
      });
      await loadStats();
    } catch (error: any) {
      setMessage({ 
        type: 'error', 
        text: error.response?.data?.message || 'Ошибка при обучении модели' 
      });
    } finally {
      setLoading(false);
    }
  };

  const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setLoading(true);
    setMessage(null);
    const formData = new FormData();
    formData.append('file', file);

    try {
      const response = await api.post('/mltraining/import-csv', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      setMessage({ 
        type: 'success', 
        text: `Импортировано ${response.data.recordsAdded} записей` 
      });
      if (response.data.errors.length > 0) {
        console.warn('Import errors:', response.data.errors);
      }
      await loadStats();
    } catch (error: any) {
      setMessage({ 
        type: 'error', 
        text: error.response?.data?.message || 'Ошибка импорта' 
      });
    } finally {
      setLoading(false);
      event.target.value = ''; // Reset file input
    }
  };

  if (!stats) {
    return <div className="loading">Загрузка...</div>;
  }

  return (
    <div className="ml-training-panel">
      <h2>🤖 ML Model Training Control</h2>

      {message && (
        <div className={`alert alert-${message.type}`}>
          {message.text}
        </div>
      )}

      <div className="stats-grid">
        <div className="stat-card">
          <h3>📊 Статистика данных</h3>
          <div className="stat-item">
            <span className="label">Всего записей:</span>
            <span className="value">{stats.totalRecords}</span>
          </div>
          <div className="stat-item">
            <span className="label">За последние 24 часа:</span>
            <span className="value">{stats.recordsLast24Hours}</span>
          </div>
          <div className="stat-item">
            <span className="label">За последние 7 дней:</span>
            <span className="value">{stats.recordsLast7Days}</span>
          </div>
          <div className="stat-item">
            <span className="label">Уникальных пользователей:</span>
            <span className="value">{stats.uniqueUsers}</span>
          </div>
          <div className="stat-item">
            <span className="label">Уникальных карточек:</span>
            <span className="value">{stats.uniqueFlashcards}</span>
          </div>
        </div>

        <div className="stat-card">
          <h3>🎯 Статус модели</h3>
          <div className="stat-item">
            <span className="label">Модель обучена:</span>
            <span className={`badge ${stats.isModelTrained ? 'success' : 'warning'}`}>
              {stats.isModelTrained ? '✅ Да' : '⚠️ Нет'}
            </span>
          </div>
          <div className="stat-item">
            <span className="label">Можно обучить:</span>
            <span className={`badge ${stats.canTrain ? 'success' : 'danger'}`}>
              {stats.canTrain ? '✅ Да' : '❌ Нужно >= 100 записей'}
            </span>
          </div>
          {stats.lastTrainingDate && (
            <div className="stat-item">
              <span className="label">Последнее обучение:</span>
              <span className="value">
                {new Date(stats.lastTrainingDate).toLocaleString('ru-RU')}
              </span>
            </div>
          )}
          <div className="stat-item">
            <span className="label">Средний retention:</span>
            <span className="value">{stats.averageRetentionRate.toFixed(1)}%</span>
          </div>
        </div>
      </div>

      <div className="actions-section">
        <h3>🔧 Действия</h3>
        
        <div className="action-group">
          <h4>1. Генерация тестовых данных</h4>
          <p>Создать синтетические данные для быстрого запуска</p>
          <div className="input-group">
            <input
              type="number"
              min="10"
              max="10000"
              value={syntheticCount}
              onChange={(e) => setSyntheticCount(parseInt(e.target.value))}
              disabled={loading}
            />
            <button
              onClick={generateSyntheticData}
              disabled={loading}
              className="btn btn-primary"
            >
              {loading ? 'Генерация...' : 'Сгенерировать данные'}
            </button>
          </div>
        </div>

        <div className="action-group">
          <h4>2. Импорт из CSV</h4>
          <p>
            Загрузите CSV файл с тренировочными данными.{' '}
            <a href="/templates/training_data_template.csv" download>
              Скачать шаблон
            </a>
          </p>
          <input
            type="file"
            accept=".csv"
            onChange={handleFileUpload}
            disabled={loading}
            className="file-input"
          />
        </div>

        <div className="action-group">
          <h4>3. Переобучить модель</h4>
          <p>
            {stats.canTrain 
              ? 'Модель будет переобучена на всех доступных данных' 
              : `Необходимо минимум 100 записей (сейчас ${stats.totalRecords})`
            }
          </p>
          <button
            onClick={retrainModel}
            disabled={loading || !stats.canTrain}
            className="btn btn-success"
          >
            {loading ? 'Обучение...' : 'Переобучить модель'}
          </button>
        </div>
      </div>

      <div className="info-section">
        <h3>ℹ️ Как это работает</h3>
        <ol>
          <li>
            <strong>Автоматический сбор данных:</strong> При каждом повторении флешкарты 
            система сохраняет данные о том, насколько хорошо пользователь её запомнил
          </li>
          <li>
            <strong>Обучение модели:</strong> ML.NET анализирует паттерны обучения 
            всех пользователей и учится предсказывать оптимальное время повторения
          </li>
          <li>
            <strong>Персонализация:</strong> Модель учитывает индивидуальные особенности 
            каждого пользователя (скорость забывания, retention rate)
          </li>
          <li>
            <strong>Continuous Learning:</strong> Чем больше данных собирается, 
            тем точнее становятся предсказания. Рекомендуется переобучать модель 
            раз в неделю.
          </li>
        </ol>
      </div>
    </div>
  );
};
