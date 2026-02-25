import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import type { RootState } from '../store';
import type { TopicProgress, Question, AnswerResult, TopicLesson } from '../types';
import { testService } from '../services/testService';
import { lessonService } from '../services/lessonService';
import { TopicsSkeleton, QuestionSkeleton } from '../components/Skeleton';

type ViewMode = 'topics' | 'practice' | 'lesson';

export default function TopicsPage() {
  const navigate = useNavigate();
  const { selectedExams } = useSelector((state: RootState) => state.exam);
  
  const [topics, setTopics] = useState<TopicProgress[]>([]);
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState<ViewMode>('topics');
  
  // Practice mode state
  const [currentTopic, setCurrentTopic] = useState<TopicProgress | null>(null);
  const [questions, setQuestions] = useState<Question[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [selectedOption, setSelectedOption] = useState<number | null>(null);
  const [answerResult, setAnswerResult] = useState<AnswerResult | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [hintText, setHintText] = useState<string | null>(null);
  const [hintLoading, setHintLoading] = useState(false);
  const [showHint, setShowHint] = useState(false);

  // Lesson mode state
  const [lessons, setLessons] = useState<TopicLesson[]>([]);
  const [currentLessonIndex, setCurrentLessonIndex] = useState(0);

  useEffect(() => {
    if (selectedExams.length === 0) {
      navigate('/');
      return;
    }
    loadTopics();
  }, [selectedExams, navigate]);

  const loadTopics = async () => {
    try {
      setLoading(true);
      const data = await testService.getTopicsWithProgress(selectedExams);
      setTopics(data);
    } catch (error) {
      console.error('Failed to load topics:', error);
    } finally {
      setLoading(false);
    }
  };

  const startTopicLesson = async (topic: TopicProgress) => {
    try {
      setLoading(true);
      const data = await lessonService.getLessonsByTopic(topic.topicId);
      if (data.length === 0) {
        // No lessons — go straight to practice
        startTopicPractice(topic);
        return;
      }
      setLessons(data);
      setCurrentTopic(topic);
      setCurrentLessonIndex(0);
      setViewMode('lesson');
    } catch (error) {
      console.error('Failed to load lessons:', error);
    } finally {
      setLoading(false);
    }
  };

  const startTopicPractice = async (topic: TopicProgress) => {
    try {
      setLoading(true);
      const data = await testService.getQuestionsByTopic(topic.topicId);
      setQuestions(data);
      setCurrentTopic(topic);
      setCurrentIndex(0);
      setSelectedOption(null);
      setAnswerResult(null);
      setHintText(null);
      setShowHint(false);
      setViewMode('practice');
    } catch (error) {
      console.error('Failed to load topic questions:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async () => {
    if (selectedOption === null) return;
    
    const currentQuestion = questions[currentIndex];
    setSubmitting(true);
    
    try {
      const result = await testService.submitAnswer({
        questionId: currentQuestion.id,
        answerOptionId: selectedOption,
      });
      setAnswerResult(result);
    } catch (error) {
      console.error('Failed to submit answer:', error);
    } finally {
      setSubmitting(false);
    }
  };

  const handleRequestHint = async () => {
    const currentQuestion = questions[currentIndex];
    if (hintText) {
      setShowHint(!showHint);
      return;
    }
    setHintLoading(true);
    try {
      const hint = await lessonService.getQuestionHint(currentQuestion.id);
      setHintText(hint);
      setShowHint(true);
    } catch {
      setHintText(null);
    } finally {
      setHintLoading(false);
    }
  };

  const handleNext = () => {
    if (currentIndex < questions.length - 1) {
      setCurrentIndex(currentIndex + 1);
      setSelectedOption(null);
      setAnswerResult(null);
      setHintText(null);
      setShowHint(false);
    } else {
      setViewMode('topics');
      loadTopics();
    }
  };

  const getMasteryColor = (mastery: number) => {
    if (mastery >= 80) return 'var(--success-color)';
    if (mastery >= 50) return 'var(--warning-color)';
    return 'var(--error-color)';
  };

  const getMasteryBadgeStyle = (mastery: number): React.CSSProperties => {
    if (mastery >= 80) return { backgroundColor: 'rgba(16, 185, 129, 0.1)', color: 'var(--success-color)' };
    if (mastery >= 50) return { backgroundColor: 'rgba(245, 158, 11, 0.1)', color: 'var(--warning-color)' };
    return { backgroundColor: 'rgba(239, 68, 68, 0.1)', color: 'var(--error-color)' };
  };

  const getDifficultyStyle = (difficulty: string) => {
    switch(difficulty) {
      case 'Easy': return { backgroundColor: 'rgba(16, 185, 129, 0.1)', color: 'var(--success-color)' };
      case 'Medium': return { backgroundColor: 'rgba(245, 158, 11, 0.1)', color: 'var(--warning-color)' };
      default: return { backgroundColor: 'rgba(239, 68, 68, 0.1)', color: 'var(--error-color)' };
    }
  };

  /** Simple markdown-like renderer for lesson content */
  const renderContent = (content: string) => {
    const lines = content.split('\n');
    const elements: React.ReactNode[] = [];
    let inTable = false;
    let tableRows: string[][] = [];

    const flushTable = () => {
      if (tableRows.length > 0) {
        elements.push(
          <div key={`table-${elements.length}`} style={{ overflowX: 'auto', margin: '1rem 0' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
              <thead>
                <tr>
                  {tableRows[0].map((cell, i) => (
                    <th key={i} style={{ padding: '0.5rem 0.75rem', textAlign: 'left', borderBottom: '2px solid var(--border-color)', fontWeight: '600', color: 'var(--text-primary)' }}>
                      {cell}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {tableRows.slice(1).map((row, ri) => (
                  <tr key={ri}>
                    {row.map((cell, ci) => (
                      <td key={ci} style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--border-color)', color: 'var(--text-secondary)' }}>
                        {cell}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        );
        tableRows = [];
      }
      inTable = false;
    };

    for (let i = 0; i < lines.length; i++) {
      const line = lines[i];

      // Table row
      if (line.trim().startsWith('|') && line.trim().endsWith('|')) {
        // Skip separator rows
        if (line.match(/^\|[\s-|]+\|$/)) continue;
        const cells = line.split('|').filter(c => c.trim() !== '').map(c => c.trim());
        tableRows.push(cells);
        inTable = true;
        continue;
      }

      if (inTable) flushTable();

      // Headers
      if (line.startsWith('## ')) {
        elements.push(<h3 key={i} style={{ fontSize: '1.125rem', fontWeight: '700', color: 'var(--text-primary)', marginTop: '1.5rem', marginBottom: '0.5rem' }}>{line.slice(3)}</h3>);
      } else if (line.startsWith('### ')) {
        elements.push(<h4 key={i} style={{ fontSize: '1rem', fontWeight: '600', color: 'var(--text-primary)', marginTop: '1rem', marginBottom: '0.375rem' }}>{line.slice(4)}</h4>);
      }
      // Math blocks
      else if (line.startsWith('$$')) {
        elements.push(<pre key={i} style={{ margin: '0.75rem 0', padding: '0.75rem', backgroundColor: 'rgba(99, 102, 241, 0.05)', borderRadius: '0.5rem', fontFamily: 'monospace', fontSize: '0.875rem', overflowX: 'auto', color: 'var(--text-primary)' }}>{line.replace(/\$\$/g, '')}</pre>);
      }
      // Blockquote
      else if (line.startsWith('> ')) {
        elements.push(<blockquote key={i} style={{ margin: '0.75rem 0', padding: '0.75rem 1rem', borderLeft: '3px solid var(--primary-color)', backgroundColor: 'rgba(99, 102, 241, 0.05)', borderRadius: '0 0.5rem 0.5rem 0', fontSize: '0.875rem', color: 'var(--text-secondary)', fontStyle: 'italic' }}>{line.slice(2)}</blockquote>);
      }
      // List items
      else if (line.match(/^(\d+)\. /)) {
        elements.push(<p key={i} style={{ margin: '0.25rem 0', paddingLeft: '1.5rem', fontSize: '0.875rem', color: 'var(--text-secondary)', lineHeight: '1.6' }}>{line}</p>);
      }
      else if (line.startsWith('- ')) {
        elements.push(<p key={i} style={{ margin: '0.25rem 0', paddingLeft: '1.5rem', fontSize: '0.875rem', color: 'var(--text-secondary)', lineHeight: '1.6' }}>• {line.slice(2)}</p>);
      }
      // Emoji/symbol lines (like ❌ ✅)
      else if (line.trim().length > 0) {
        // Inline formatting: **bold**, *italic*, `code`, $math$
        const formatted = line
          .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
          .replace(/\*(.+?)\*/g, '<em>$1</em>')
          .replace(/`(.+?)`/g, '<code style="background:rgba(99,102,241,0.1);padding:2px 4px;border-radius:3px;font-size:0.8rem">$1</code>')
          .replace(/\$(.+?)\$/g, '<code style="font-family:monospace;color:var(--primary-color)">$1</code>');
        elements.push(<p key={i} style={{ margin: '0.375rem 0', fontSize: '0.875rem', color: 'var(--text-secondary)', lineHeight: '1.6' }} dangerouslySetInnerHTML={{ __html: formatted }} />);
      } else {
        elements.push(<div key={i} style={{ height: '0.5rem' }} />);
      }
    }
    if (inTable) flushTable();
    return elements;
  };

  if (loading && viewMode === 'topics') {
    return (
      <div style={{ maxWidth: '800px', margin: '0 auto' }}>
        <div className="skeleton" style={{ height: '2rem', width: '150px', marginBottom: '1.5rem' }} />
        <TopicsSkeleton />
      </div>
    );
  }

  if (loading && (viewMode === 'practice' || viewMode === 'lesson')) {
    return (
      <div style={{ maxWidth: '700px', margin: '0 auto' }}>
        <QuestionSkeleton />
      </div>
    );
  }

  // ───────────── LESSON VIEW ─────────────
  if (viewMode === 'lesson' && lessons.length > 0) {
    const lesson = lessons[currentLessonIndex];

    return (
      <div className="animate-fade-in" style={{ maxWidth: '750px', margin: '0 auto' }}>
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
          <span style={{ 
            padding: '0.375rem 0.75rem', 
            backgroundColor: 'rgba(245, 158, 11, 0.1)', 
            color: 'var(--warning-color)',
            borderRadius: '9999px',
            fontSize: '0.875rem',
            fontWeight: '500'
          }}>
            📖 Урок • {currentTopic?.topicName}
          </span>
          <button onClick={() => { setViewMode('topics'); loadTopics(); }} className="btn btn-outline">
            ← К темам
          </button>
        </div>

        {/* Lesson navigation */}
        {lessons.length > 1 && (
          <div className="card" style={{ padding: '0.75rem', marginBottom: '1rem' }}>
            <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
              {lessons.map((l, idx) => (
                <button
                  key={l.id}
                  onClick={() => setCurrentLessonIndex(idx)}
                  style={{
                    padding: '0.375rem 0.75rem',
                    borderRadius: '0.5rem',
                    border: 'none',
                    fontSize: '0.8rem',
                    fontWeight: idx === currentLessonIndex ? '600' : '400',
                    backgroundColor: idx === currentLessonIndex ? 'var(--primary-color)' : 'var(--border-color)',
                    color: idx === currentLessonIndex ? 'white' : 'var(--text-secondary)',
                    cursor: 'pointer',
                    transition: 'all 0.2s ease'
                  }}
                >
                  {idx + 1}. {l.title}
                </button>
              ))}
            </div>
          </div>
        )}

        {/* Lesson Content */}
        <div className="card animate-fade-in-up" style={{ padding: '2rem' }}>
          <h2 style={{ fontSize: '1.25rem', fontWeight: '700', color: 'var(--text-primary)', marginBottom: '1rem' }}>
            {lesson.title}
          </h2>

          <div>{renderContent(lesson.content)}</div>

          {/* Video Link */}
          {lesson.videoUrl && (
            <div style={{
              marginTop: '1.5rem',
              padding: '1rem',
              borderRadius: '0.75rem',
              backgroundColor: 'rgba(239, 68, 68, 0.05)',
              border: '1px solid rgba(239, 68, 68, 0.2)',
              display: 'flex',
              alignItems: 'center',
              gap: '0.75rem'
            }}>
              <span style={{ fontSize: '1.5rem' }}>🎬</span>
              <div style={{ flex: 1 }}>
                <p style={{ fontSize: '0.875rem', fontWeight: '600', color: 'var(--text-primary)', margin: 0 }}>Видео-урок</p>
                <p style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', margin: '0.25rem 0 0 0' }}>Посмотрите обучающее видео для лучшего понимания</p>
              </div>
              <a
                href={lesson.videoUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="btn btn-outline"
                style={{ whiteSpace: 'nowrap', fontSize: '0.8rem', textDecoration: 'none' }}
              >
                ▶ Смотреть
              </a>
            </div>
          )}
        </div>

        {/* Bottom Actions */}
        <div style={{ marginTop: '1.5rem', display: 'flex', gap: '0.75rem' }}>
          {currentLessonIndex < lessons.length - 1 ? (
            <button
              onClick={() => setCurrentLessonIndex(currentLessonIndex + 1)}
              className="btn btn-primary"
              style={{ flex: 1 }}
            >
              Следующий урок →
            </button>
          ) : (
            <button
              onClick={() => currentTopic && startTopicPractice(currentTopic)}
              className="btn btn-primary"
              style={{ flex: 1 }}
            >
              🎯 Начать практику
            </button>
          )}
        </div>
      </div>
    );
  }

  // ───────────── TOPICS LIST VIEW ─────────────
  if (viewMode === 'topics') {
    return (
      <div className="animate-fade-in" style={{ maxWidth: '800px', margin: '0 auto' }}>
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <h1 style={{ fontSize: '1.5rem', fontWeight: '700', color: 'var(--text-primary)' }}>📚 Темы</h1>
          <button onClick={() => navigate('/test')} className="btn btn-outline">
            ← Назад
          </button>
        </div>

        {/* Selected Exams */}
        {selectedExams.length > 0 && (
          <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginBottom: '1.5rem' }}>
            {selectedExams.map(exam => (
              <span key={exam} style={{
                padding: '0.25rem 0.75rem',
                backgroundColor: 'var(--primary-color)',
                color: 'white',
                borderRadius: '9999px',
                fontSize: '0.75rem',
                fontWeight: '500'
              }}>
                {exam}
              </span>
            ))}
          </div>
        )}

        {/* Stats Summary */}
        <div className="card animate-fade-in-up" style={{ marginBottom: '1.5rem', padding: '1.5rem' }}>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '1rem', textAlign: 'center' }}>
            <div>
              <div style={{ fontSize: '1.75rem', fontWeight: '700', color: 'var(--primary-color)' }}>{topics.length}</div>
              <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Всего тем</div>
            </div>
            <div>
              <div style={{ fontSize: '1.75rem', fontWeight: '700', color: 'var(--success-color)' }}>
                {topics.filter(t => t.masteryPercentage >= 80).length}
              </div>
              <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Освоено</div>
            </div>
            <div>
              <div style={{ fontSize: '1.75rem', fontWeight: '700', color: 'var(--warning-color)' }}>
                {topics.filter(t => t.masteryPercentage < 50).length}
              </div>
              <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>Требуют внимания</div>
            </div>
          </div>
        </div>

        {/* Topics Grid */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {topics.map((topic, index) => (
            <div
              key={topic.topicId}
              className="card animate-fade-in-up"
              style={{ 
                padding: '1.25rem',
                borderLeft: `4px solid ${getMasteryColor(topic.masteryPercentage)}`,
                animationDelay: `${index * 0.05}s`
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                <div>
                  <h3 style={{ fontWeight: '600', color: 'var(--text-primary)', marginBottom: '0.25rem' }}>{topic.topicName}</h3>
                  <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                    {topic.totalQuestions} {topic.totalQuestions === 1 ? 'вопрос' : 'вопросов'} в теме
                    {(topic.lessonCount ?? 0) > 0 && (
                      <span style={{ marginLeft: '0.5rem' }}>• 📖 {topic.lessonCount} {topic.lessonCount === 1 ? 'урок' : 'уроков'}</span>
                    )}
                  </p>
                </div>
                <span style={{ 
                  padding: '0.25rem 0.75rem', 
                  borderRadius: '9999px', 
                  fontSize: '0.875rem', 
                  fontWeight: '500',
                  ...getMasteryBadgeStyle(topic.masteryPercentage)
                }}>
                  {topic.masteryPercentage}%
                </span>
              </div>

              {/* Progress Bar */}
              <div style={{ width: '100%', backgroundColor: 'var(--border-color)', borderRadius: '9999px', height: '0.5rem' }}>
                <div style={{ 
                  width: `${Math.min(topic.masteryPercentage, 100)}%`,
                  backgroundColor: getMasteryColor(topic.masteryPercentage),
                  height: '0.5rem',
                  borderRadius: '9999px',
                  transition: 'width 0.3s ease'
                }} />
              </div>

              <div style={{ marginTop: '0.5rem', display: 'flex', justifyContent: 'space-between', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                <span>✅ {topic.correctAnswers} верных ответов</span>
                <span>❌ {topic.incorrectAnswers} ошибок</span>
              </div>

              {/* Action Buttons */}
              <div style={{ marginTop: '0.75rem', display: 'flex', gap: '0.5rem' }}>
                {(topic.lessonCount ?? 0) > 0 && (
                  <button
                    onClick={() => startTopicLesson(topic)}
                    className="btn btn-outline"
                    style={{ flex: 1, fontSize: '0.8rem', padding: '0.5rem' }}
                  >
                    📖 Урок
                  </button>
                )}
                <button
                  onClick={() => startTopicPractice(topic)}
                  className="btn btn-primary"
                  style={{ flex: 1, fontSize: '0.8rem', padding: '0.5rem' }}
                >
                  🎯 Практика
                </button>
              </div>
            </div>
          ))}
        </div>

        {topics.length === 0 && (
          <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
            <div style={{ fontSize: '4rem', marginBottom: '1rem' }}>📭</div>
            <p style={{ color: 'var(--text-secondary)' }}>Темы не найдены для выбранных экзаменов</p>
          </div>
        )}
      </div>
    );
  }

  // ───────────── PRACTICE VIEW ─────────────
  const currentQuestion = questions[currentIndex];

  return (
    <div className="animate-fade-in" style={{ maxWidth: '700px', margin: '0 auto' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <span style={{ 
          padding: '0.375rem 0.75rem', 
          backgroundColor: 'rgba(79, 70, 229, 0.1)', 
          color: 'var(--primary-color)',
          borderRadius: '9999px',
          fontSize: '0.875rem',
          fontWeight: '500'
        }}>
          📚 {currentTopic?.topicName}
        </span>
        <button onClick={() => setViewMode('topics')} className="btn btn-outline">
          ← К темам
        </button>
      </div>

      {/* Progress */}
      <div className="card" style={{ padding: '1rem', marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.875rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>
          <span>Вопрос {currentIndex + 1} из {questions.length}</span>
        </div>
        <div style={{ width: '100%', backgroundColor: 'var(--border-color)', borderRadius: '9999px', height: '0.5rem' }}>
          <div style={{ 
            width: `${((currentIndex + 1) / questions.length) * 100}%`,
            backgroundColor: 'var(--primary-color)',
            height: '0.5rem',
            borderRadius: '9999px',
            transition: 'width 0.3s ease'
          }} />
        </div>
      </div>

      {/* Question Card */}
      <div className="card card-static animate-fade-in-up" key={currentQuestion.id}>
        <div style={{ marginBottom: '1rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <span style={{ 
            padding: '0.25rem 0.75rem', 
            borderRadius: '9999px', 
            fontSize: '0.75rem', 
            fontWeight: '500',
            ...getDifficultyStyle(currentQuestion.difficulty)
          }}>
            {currentQuestion.difficulty === 'Easy' ? 'Легкий' :
             currentQuestion.difficulty === 'Medium' ? 'Средний' : 'Сложный'}
          </span>

          {/* Hint Button */}
          {currentQuestion.hasHint && !answerResult && (
            <button
              onClick={handleRequestHint}
              disabled={hintLoading}
              style={{
                padding: '0.25rem 0.75rem',
                borderRadius: '9999px',
                fontSize: '0.75rem',
                fontWeight: '500',
                border: '1px solid rgba(245, 158, 11, 0.3)',
                backgroundColor: showHint ? 'rgba(245, 158, 11, 0.15)' : 'rgba(245, 158, 11, 0.05)',
                color: 'var(--warning-color)',
                cursor: 'pointer',
                transition: 'all 0.2s ease'
              }}
            >
              {hintLoading ? '...' : showHint ? '💡 Скрыть подсказку' : '💡 Подсказка'}
            </button>
          )}
        </div>

        {/* Hint Display */}
        {showHint && hintText && (
          <div className="animate-fade-in-up" style={{
            marginBottom: '1rem',
            padding: '0.75rem 1rem',
            borderRadius: '0.75rem',
            backgroundColor: 'rgba(245, 158, 11, 0.08)',
            border: '1px solid rgba(245, 158, 11, 0.2)',
            fontSize: '0.85rem',
            color: 'var(--text-secondary)',
            lineHeight: '1.5'
          }}>
            <span style={{ fontWeight: '600', color: 'var(--warning-color)' }}>💡 Подсказка: </span>
            {hintText}
          </div>
        )}

        <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1.5rem', color: 'var(--text-primary)' }}>
          {currentQuestion.text}
        </h2>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {currentQuestion.options.map((option) => {
            let style: React.CSSProperties = { 
              border: '2px solid var(--border-color)',
              backgroundColor: 'var(--card-background)'
            };
            
            if (answerResult) {
              if (option.id === answerResult.correctOptionId) {
                style = { border: '2px solid var(--success-color)', backgroundColor: 'rgba(16, 185, 129, 0.1)' };
              } else if (option.id === selectedOption && !answerResult.isCorrect) {
                style = { border: '2px solid var(--error-color)', backgroundColor: 'rgba(239, 68, 68, 0.1)' };
              }
            } else if (selectedOption === option.id) {
              style = { border: '2px solid var(--primary-color)', backgroundColor: 'rgba(79, 70, 229, 0.1)' };
            }

            return (
              <button
                key={option.id}
                onClick={() => !answerResult && setSelectedOption(option.id)}
                disabled={answerResult !== null}
                style={{
                  ...style,
                  width: '100%',
                  padding: '1rem',
                  borderRadius: '0.75rem',
                  textAlign: 'left',
                  cursor: answerResult ? 'default' : 'pointer',
                  transition: 'all 0.2s ease',
                  color: 'var(--text-primary)',
                  fontSize: '0.875rem'
                }}
              >
                {option.text}
              </button>
            );
          })}
        </div>

        {/* Result Feedback */}
        {answerResult && (
          <div className="animate-fade-in-up" style={{
            marginTop: '1.5rem',
            padding: '1rem',
            borderRadius: '0.75rem',
            backgroundColor: answerResult.isCorrect ? 'rgba(16, 185, 129, 0.1)' : 'rgba(239, 68, 68, 0.1)',
            border: `1px solid ${answerResult.isCorrect ? 'var(--success-color)' : 'var(--error-color)'}`
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
              <span style={{ fontSize: '1.5rem' }}>{answerResult.isCorrect ? '✅' : '❌'}</span>
              <span style={{ fontWeight: '600', color: answerResult.isCorrect ? 'var(--success-color)' : 'var(--error-color)' }}>
                {answerResult.isCorrect ? 'Правильно!' : 'Неправильно'}
              </span>
            </div>
            
            {!answerResult.isCorrect && (
              <p style={{ color: 'var(--text-primary)', marginBottom: '0.5rem' }}>
                <span style={{ fontWeight: '500' }}>Правильный ответ: </span>
                {answerResult.correctOptionText}
              </p>
            )}
            
            {answerResult.explanation && (
              <div style={{ marginTop: '0.75rem', paddingTop: '0.75rem', borderTop: '1px solid var(--border-color)' }}>
                <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                  <span style={{ fontWeight: '500' }}>Объяснение: </span>
                  {answerResult.explanation}
                </p>
              </div>
            )}
          </div>
        )}

        {/* Actions */}
        <div style={{ marginTop: '1.5rem' }}>
          {!answerResult ? (
            <button
              onClick={handleSubmit}
              disabled={selectedOption === null || submitting}
              className="btn btn-primary"
              style={{ width: '100%' }}
            >
              {submitting ? 'Проверка...' : 'Ответить'}
            </button>
          ) : (
            <button
              onClick={handleNext}
              className="btn btn-primary"
              style={{ width: '100%' }}
            >
              {currentIndex < questions.length - 1 ? 'Следующий вопрос' : 'Завершить тему'}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
