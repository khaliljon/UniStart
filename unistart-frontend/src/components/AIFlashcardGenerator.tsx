import React, { useState, useEffect } from 'react';
import api from '../services/api';
import './AIFlashcardGenerator.css';

interface GenerateRequest {
  sourceText: string;
  count: number;
  difficulty: 'easy' | 'medium' | 'hard';
  language: string;
  flashcardSetId?: number;
  newSetTitle?: string;
  subject?: string;
}

interface GeneratedFlashcard {
  question: string;
  answer: string;
  explanation?: string;
  difficultyLevel: number;
  tags: string[];
}

interface GenerateResponse {
  flashcards: GeneratedFlashcard[];
  flashcardSetId?: number;
  modelUsed: string;
  tokensUsed: number;
  success: boolean;
  errorMessage?: string;
}

interface StatusResponse {
  isConfigured: boolean;
  availableModels: string[];
  message: string;
}

const AIFlashcardGenerator: React.FC = () => {
  const [sourceText, setSourceText] = useState('');
  const [count, setCount] = useState(10);
  const [difficulty, setDifficulty] = useState<'easy' | 'medium' | 'hard'>('medium');
  const [language, setLanguage] = useState('ru');
  const [newSetTitle, setNewSetTitle] = useState('');
  const [subject, setSubject] = useState('');
  
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState<StatusResponse | null>(null);
  const [result, setResult] = useState<GenerateResponse | null>(null);
  const [message, setMessage] = useState<{ type: 'success' | 'error' | 'info', text: string } | null>(null);

  useEffect(() => {
    checkStatus();
  }, []);

  const checkStatus = async () => {
    try {
      const response = await api.get<StatusResponse>('/ai/flashcards/status');
      setStatus(response.data);
    } catch (error) {
      console.error('Failed to check AI service status:', error);
      setMessage({ 
        type: 'error', 
        text: 'Не удалось проверить статус AI сервиса' 
      });
    }
  };

  const handleGenerate = async () => {
    if (!sourceText.trim()) {
      setMessage({ type: 'error', text: 'Введите текст для генерации' });
      return;
    }

    if (!newSetTitle.trim()) {
      setMessage({ type: 'error', text: 'Введите название набора' });
      return;
    }

    setLoading(true);
    setMessage(null);
    setResult(null);

    try {
      const request: GenerateRequest = {
        sourceText: sourceText.trim(),
        count,
        difficulty,
        language,
        newSetTitle: newSetTitle.trim(),
        subject: subject.trim() || undefined
      };

      const response = await api.post<GenerateResponse>('/ai/flashcards/generate', request);
      
      if (response.data.success) {
        setResult(response.data);
        setMessage({ 
          type: 'success', 
          text: `✅ Сгенерировано ${response.data.flashcards.length} карточек! Использовано ${response.data.tokensUsed} токенов` 
        });
        
        // Очистить форму после успеха
        setSourceText('');
        setNewSetTitle('');
        setSubject('');
      } else {
        setMessage({ 
          type: 'error', 
          text: response.data.errorMessage || 'Ошибка при генерации' 
        });
      }
    } catch (error: any) {
      setMessage({ 
        type: 'error', 
        text: error.response?.data?.message || 'Ошибка при генерации flashcards' 
      });
    } finally {
      setLoading(false);
    }
  };

  const getWordCount = (text: string) => {
    return text.trim().split(/\s+/).length;
  };

  const getEstimatedCost = () => {
    const wordCount = getWordCount(sourceText);
    const inputTokens = Math.ceil(wordCount * 1.3); // Примерно 1.3 токена на слово
    const outputTokens = count * 50; // Примерно 50 токенов на карточку
    
    // Claude Sonnet 3.5: $3/$15 за 1M токенов
    const inputCost = (inputTokens / 1_000_000) * 3;
    const outputCost = (outputTokens / 1_000_000) * 15;
    const totalCost = inputCost + outputCost;
    
    return {
      inputTokens,
      outputTokens,
      totalTokens: inputTokens + outputTokens,
      cost: totalCost
    };
  };

  if (!status) {
    return <div className="ai-flashcard-generator loading">Загрузка...</div>;
  }

  if (!status.isConfigured) {
    return (
      <div className="ai-flashcard-generator">
        <div className="alert alert-warning">
          <h3>⚠️ AI сервис не настроен</h3>
          <p>{status.message}</p>
          <p>Добавьте Anthropic API ключ в appsettings.json:</p>
          <pre>
{`"AI": {
  "Flashcards": {
    "AnthropicApiKey": "sk-ant-api03-YOUR_KEY"
  }
}`}
          </pre>
        </div>
      </div>
    );
  }

  const estimate = getEstimatedCost();

  return (
    <div className="ai-flashcard-generator">
      <div className="header">
        <h2>🤖 AI Генератор Flashcards</h2>
        <div className="status-badge success">
          ✅ {status.availableModels.join(', ')}
        </div>
      </div>

      {message && (
        <div className={`alert alert-${message.type}`}>
          {message.text}
        </div>
      )}

      <div className="generator-form">
        <div className="form-group">
          <label htmlFor="sourceText">
            Исходный текст *
            <span className="word-count">
              {getWordCount(sourceText)} слов / {sourceText.length} символов (макс: 50000)
            </span>
          </label>
          <textarea
            id="sourceText"
            value={sourceText}
            onChange={(e) => setSourceText(e.target.value)}
            placeholder="Вставьте текст лекции, статьи или учебника..."
            rows={12}
            disabled={loading}
            maxLength={50000}
          />
          <small>Минимум 50 слов, максимум 50000 символов (~8000 слов). Для больших текстов разбейте на части.</small>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="newSetTitle">Название набора *</label>
            <input
              type="text"
              id="newSetTitle"
              value={newSetTitle}
              onChange={(e) => setNewSetTitle(e.target.value)}
              placeholder="Например: Квантовая физика - основы"
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="subject">Предмет (опционально)</label>
            <input
              type="text"
              id="subject"
              value={subject}
              onChange={(e) => setSubject(e.target.value)}
              placeholder="Физика, Математика, История..."
              disabled={loading}
            />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="count">Количество карточек</label>
            <input
              type="number"
              id="count"
              value={count}
              onChange={(e) => setCount(Math.min(50, Math.max(1, parseInt(e.target.value) || 10)))}
              min="1"
              max="50"
              disabled={loading}
            />
            <small>От 1 до 50</small>
          </div>

          <div className="form-group">
            <label htmlFor="difficulty">Сложность</label>
            <select
              id="difficulty"
              value={difficulty}
              onChange={(e) => setDifficulty(e.target.value as 'easy' | 'medium' | 'hard')}
              disabled={loading}
            >
              <option value="easy">Легкая</option>
              <option value="medium">Средняя</option>
              <option value="hard">Сложная</option>
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="language">Язык</label>
            <select
              id="language"
              value={language}
              onChange={(e) => setLanguage(e.target.value)}
              disabled={loading}
            >
              <option value="ru">Русский</option>
              <option value="en">English</option>
            </select>
          </div>
        </div>

        <div className="cost-estimate">
          <h4>📊 Оценка стоимости</h4>
          <div className="estimate-details">
            <span>Токенов: ~{estimate.totalTokens.toLocaleString()}</span>
            <span>Стоимость: ~${estimate.cost.toFixed(4)}</span>
          </div>
        </div>

        <button
          onClick={handleGenerate}
          disabled={loading || !sourceText.trim() || !newSetTitle.trim()}
          className="btn btn-primary btn-generate"
        >
          {loading ? (
            <>
              <span className="spinner"></span>
              Генерация...
            </>
          ) : (
            <>
              🚀 Сгенерировать {count} карточек
            </>
          )}
        </button>
      </div>

      {result && result.success && (
        <div className="result-section">
          <h3>✅ Результат генерации</h3>
          <div className="result-stats">
            <div className="stat">
              <span className="label">Создано карточек:</span>
              <span className="value">{result.flashcards.length}</span>
            </div>
            <div className="stat">
              <span className="label">Модель:</span>
              <span className="value">{result.modelUsed}</span>
            </div>
            <div className="stat">
              <span className="label">Использовано токенов:</span>
              <span className="value">{result.tokensUsed.toLocaleString()}</span>
            </div>
            {result.flashcardSetId && (
              <div className="stat">
                <span className="label">ID набора:</span>
                <span className="value">#{result.flashcardSetId}</span>
              </div>
            )}
          </div>

          <div className="flashcards-preview">
            <h4>Предпросмотр карточек</h4>
            {result.flashcards.map((card, index) => (
              <div key={index} className="flashcard-item">
                <div className="flashcard-header">
                  <span className="card-number">#{index + 1}</span>
                  <span className="difficulty-badge">
                    {'⭐'.repeat(card.difficultyLevel)}
                  </span>
                </div>
                <div className="flashcard-content">
                  <div className="question">
                    <strong>Q:</strong> {card.question}
                  </div>
                  <div className="answer">
                    <strong>A:</strong> {card.answer}
                  </div>
                  {card.explanation && (
                    <div className="explanation">
                      <strong>💡 Объяснение:</strong> {card.explanation}
                    </div>
                  )}
                  {card.tags.length > 0 && (
                    <div className="tags">
                      {card.tags.map((tag, i) => (
                        <span key={i} className="tag">{tag}</span>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="info-section">
        <h4>ℹ️ Как это работает</h4>
        <ol>
          <li>Вставьте текст (лекция, статья, учебник) - минимум 50 слов</li>
          <li>Укажите количество карточек и уровень сложности</li>
          <li>AI проанализирует текст и создаст вопрос-ответ пары</li>
          <li>Карточки автоматически сохранятся в новый набор</li>
          <li>Можете сразу опубликовать набор для студентов</li>
        </ol>
      </div>
    </div>
  );
};

export default AIFlashcardGenerator;
