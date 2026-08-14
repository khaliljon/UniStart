import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import type { RootState } from '../store';
import type { TopicProgress, Question, AnswerResult, TopicLesson } from '../types';
import { testService } from '../services/testService';
import { lessonService } from '../services/lessonService';
import { subscriptionService } from '../services/subscriptionService';
import { ProGate } from '../components/ProGate';
import { TopicsSkeleton, QuestionSkeleton } from '../components/Skeleton';
import { ContentRenderer } from '../components/MathRenderer';
import { useTranslation } from '../hooks/useTranslation';

type ViewMode = 'topics' | 'practice' | 'lesson';

export default function TopicsPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { selectedExams, selectedSectionIds } = useSelector((state: RootState) => state.exam);
  
  const [topics, setTopics] = useState<TopicProgress[]>([]);
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState<ViewMode>('topics');
  const [selectedSection, setSelectedSection] = useState<string | null>(null);
  
  const [currentTopic, setCurrentTopic] = useState<TopicProgress | null>(null);
  const [questions, setQuestions] = useState<Question[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [selectedOption, setSelectedOption] = useState<number | null>(null);
  const [answerResult, setAnswerResult] = useState<AnswerResult | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [hintText, setHintText] = useState<string | null>(null);
  const [hintLoading, setHintLoading] = useState(false);
  const [showHint, setShowHint] = useState(false);

  const [lessons, setLessons] = useState<TopicLesson[]>([]);
  const [currentLessonIndex, setCurrentLessonIndex] = useState(0);
  const [hasAccess, setHasAccess] = useState(true);

  useEffect(() => {
    subscriptionService.getStatus().then((s) => {
      setHasAccess(s.isPro);
    }).catch(() => {});
  }, []);

  useEffect(() => {
    if (selectedExams.length === 0) {
      navigate('/');
      return;
    }
    loadTopics();
  }, [selectedExams, selectedSectionIds, navigate]);

  const loadTopics = async () => {
    try {
      setLoading(true);
      const data = await testService.getTopicsWithProgress(selectedExams, selectedSectionIds);
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

  if (viewMode === 'lesson' && lessons.length > 0) {
    const lesson = lessons[currentLessonIndex];

    return (
      <div className="animate-fade-in" style={{ maxWidth: '750px', margin: '0 auto' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
          <span style={{ 
            padding: '0.375rem 0.75rem', 
            backgroundColor: 'rgba(245, 158, 11, 0.1)', 
            color: 'var(--warning-color)',
            borderRadius: '9999px',
            fontSize: '0.875rem',
            fontWeight: '500'
          }}>
            {t.topics.lessonLabel} • {currentTopic?.topicName}
          </span>
          <button onClick={() => { setViewMode('topics'); loadTopics(); }} className="btn btn-outline">
            {t.topics.backToTopics}
          </button>
        </div>

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

        <div className="card animate-fade-in-up" style={{ padding: '2rem' }}>
          <h2 style={{ fontSize: '1.25rem', fontWeight: '700', color: 'var(--text-primary)', marginBottom: '1rem' }}>
            {lesson.title}
          </h2>

          <ContentRenderer content={lesson.content} />
        </div>

        <div style={{ marginTop: '1.5rem', display: 'flex', gap: '0.75rem' }}>
          {currentLessonIndex < lessons.length - 1 ? (
            <button
              onClick={() => setCurrentLessonIndex(currentLessonIndex + 1)}
              className="btn btn-primary"
              style={{ flex: 1 }}
            >
              {t.topics.nextLesson}
            </button>
          ) : (
            <button
              onClick={() => currentTopic && startTopicPractice(currentTopic)}
              className="btn btn-primary"
              style={{ flex: 1 }}
            >
              {t.topics.startPractice}
            </button>
          )}
        </div>
      </div>
    );
  }

  if (viewMode === 'topics') {
    const sectionGroups: { sectionName: string; sectionId: number | null; topics: TopicProgress[] }[] = [];
    const sectionMap = new Map<string, TopicProgress[]>();
    const sectionIdMap = new Map<string, number | null>();
    
    topics.forEach(tp => {
      const key = tp.sectionName || t.topics.other;
      if (!sectionMap.has(key)) {
        sectionMap.set(key, []);
        sectionIdMap.set(key, tp.sectionId ?? null);
      }
      sectionMap.get(key)!.push(tp);
    });
    
    sectionMap.forEach((sectionTopics, name) => {
      sectionGroups.push({ sectionName: name, sectionId: sectionIdMap.get(name) ?? null, topics: sectionTopics });
    });

    const hasSections = sectionGroups.length > 1 || (sectionGroups.length === 1 && sectionGroups[0].sectionName !== t.topics.other);

    const activeSection = selectedSection ? sectionGroups.find(s => s.sectionName === selectedSection) : null;

    return (
      <div className="animate-fade-in" style={{ maxWidth: '800px', margin: '0 auto' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <h1 style={{ fontSize: '1.5rem', fontWeight: '700', color: 'var(--text-primary)' }}>{t.topics.learning}</h1>
          <button onClick={() => navigate('/test')} className="btn btn-outline">
            ← {t.topics.back}
          </button>
        </div>

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

        <div className="card animate-fade-in-up" style={{ marginBottom: '1.5rem', padding: '1.5rem' }}>
          <div style={{ display: 'grid', gridTemplateColumns: hasSections ? 'repeat(4, 1fr)' : 'repeat(3, 1fr)', gap: '1rem', textAlign: 'center' }}>
            {hasSections && (
              <div>
                <div style={{ fontSize: '1.75rem', fontWeight: '700', color: 'var(--text-primary)' }}>{sectionGroups.length}</div>
                <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>{t.topics.sections}</div>
              </div>
            )}
            <div>
              <div style={{ fontSize: '1.75rem', fontWeight: '700', color: 'var(--primary-color)' }}>{topics.length}</div>
              <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>{t.topics.totalTopics}</div>
            </div>
            <div>
              <div style={{ fontSize: '1.75rem', fontWeight: '700', color: 'var(--success-color)' }}>
                {topics.filter(tp => tp.masteryPercentage >= 80).length}
              </div>
              <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>{t.topics.mastered}</div>
            </div>
            <div>
              <div style={{ fontSize: '1.75rem', fontWeight: '700', color: 'var(--warning-color)' }}>
                {topics.filter(tp => tp.masteryPercentage < 50).length}
              </div>
              <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>{t.topics.needsWork}</div>
            </div>
          </div>
        </div>

        {hasSections && !selectedSection && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            {sectionGroups.map((section, sIdx) => {
              const sectionMastery = section.topics.length > 0
                ? Math.round(section.topics.reduce((sum, t) => sum + t.masteryPercentage, 0) / section.topics.length)
                : 0;
              const masteredCount = section.topics.filter(tp => tp.masteryPercentage >= 80).length;

              return (
                <div
                  key={section.sectionName}
                  className="card animate-fade-in-up"
                  onClick={() => setSelectedSection(section.sectionName)}
                  style={{
                    padding: '1.25rem',
                    cursor: 'pointer',
                    transition: 'all 0.2s ease',
                    borderLeft: `4px solid ${getMasteryColor(sectionMastery)}`,
                    animationDelay: `${sIdx * 0.06}s`
                  }}
                  onMouseEnter={e => { e.currentTarget.style.transform = 'translateX(4px)'; e.currentTarget.style.boxShadow = '0 4px 12px rgba(0,0,0,0.1)'; }}
                  onMouseLeave={e => { e.currentTarget.style.transform = ''; e.currentTarget.style.boxShadow = ''; }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                      <span style={{
                        width: '2.5rem', height: '2.5rem', borderRadius: '0.625rem',
                        display: 'flex', alignItems: 'center', justifyContent: 'center',
                        fontSize: '1.1rem', fontWeight: '700',
                        backgroundColor: 'rgba(79, 70, 229, 0.1)', color: 'var(--primary-color)'
                      }}>
                        {sIdx + 1}
                      </span>
                      <div>
                        <h2 style={{ fontSize: '1.05rem', fontWeight: '600', color: 'var(--text-primary)', margin: 0 }}>
                          {section.sectionName}
                        </h2>
                        <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                          {section.topics.length} {t.topics.topics}
                          {masteredCount > 0 && <span> • {masteredCount} {t.topics.topicsMastered}</span>}
                        </span>
                      </div>
                    </div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                      <div style={{ width: '100px', backgroundColor: 'var(--border-color)', borderRadius: '9999px', height: '0.5rem' }}>
                        <div style={{
                          width: `${Math.min(sectionMastery, 100)}%`,
                          backgroundColor: getMasteryColor(sectionMastery),
                          height: '0.5rem', borderRadius: '9999px', transition: 'width 0.3s ease'
                        }} />
                      </div>
                      <span style={{
                        padding: '0.25rem 0.625rem', borderRadius: '9999px', fontSize: '0.8rem', fontWeight: '600',
                        ...getMasteryBadgeStyle(sectionMastery)
                      }}>
                        {sectionMastery}%
                      </span>
                      <span style={{ color: 'var(--text-secondary)', fontSize: '1.1rem' }}>›</span>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}

        {(selectedSection && activeSection) || !hasSections ? (
          <div className="animate-fade-in">
            {hasSections && selectedSection && (
              <button
                onClick={() => setSelectedSection(null)}
                style={{
                  display: 'flex', alignItems: 'center', gap: '0.4rem',
                  background: 'none', border: 'none', cursor: 'pointer',
                  color: 'var(--primary-color)', fontSize: '0.9rem', fontWeight: '500',
                  marginBottom: '1rem', padding: '0.25rem 0'
                }}
              >
                ← {t.topics.allSections}
              </button>
            )}

            {hasSections && activeSection && (() => {
              const sectionMastery = activeSection.topics.length > 0
                ? Math.round(activeSection.topics.reduce((sum, t) => sum + t.masteryPercentage, 0) / activeSection.topics.length)
                : 0;
              return (
                <div style={{
                  display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                  marginBottom: '1rem', padding: '0.75rem 1rem', borderRadius: '0.75rem',
                  backgroundColor: 'var(--card-background)', border: '1px solid var(--border-color)'
                }}>
                  <h2 style={{ fontSize: '1.1rem', fontWeight: '600', color: 'var(--text-primary)', margin: 0 }}>
                    {activeSection.sectionName}
                  </h2>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                      {activeSection.topics.length} {t.topics.topics}
                    </span>
                    <span style={{ ...getMasteryBadgeStyle(sectionMastery), padding: '0.2rem 0.5rem', borderRadius: '9999px', fontSize: '0.75rem', fontWeight: '600' }}>
                      {sectionMastery}%
                    </span>
                  </div>
                </div>
              );
            })()}

            <ProGate hasAccess={hasAccess} featureName={t.topics.learning}>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              {(activeSection ? activeSection.topics : sectionGroups.flatMap(s => s.topics)).map((topic, index) => (
                <div
                  key={topic.topicId}
                  className="card animate-fade-in-up"
                  style={{
                    padding: '1.25rem',
                    borderLeft: `4px solid ${getMasteryColor(topic.masteryPercentage)}`,
                    animationDelay: `${index * 0.03}s`
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
                    <div>
                      <h3 style={{ fontWeight: '600', color: 'var(--text-primary)', marginBottom: '0.25rem' }}>{topic.topicName}</h3>
                      <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                        {topic.totalQuestions} {t.topics.questionsInTopic}
                        {(topic.lessonCount ?? 0) > 0 && (
                          <span style={{ marginLeft: '0.5rem' }}>• {topic.lessonCount} {t.topics.lessonsCount}</span>
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

                  <div style={{ width: '100%', backgroundColor: 'var(--border-color)', borderRadius: '9999px', height: '0.5rem' }}>
                    <div style={{
                      width: `${Math.min(topic.masteryPercentage, 100)}%`,
                      backgroundColor: getMasteryColor(topic.masteryPercentage),
                      height: '0.5rem', borderRadius: '9999px', transition: 'width 0.3s ease'
                    }} />
                  </div>

                  <div style={{ marginTop: '0.5rem', display: 'flex', justifyContent: 'space-between', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                    <span>✓ {topic.correctAnswers} {t.topics.correctAnswers}</span>
                    <span>✕ {topic.incorrectAnswers} {t.topics.errors}</span>
                  </div>

                  <div style={{ marginTop: '0.75rem', display: 'flex', gap: '0.5rem' }}>
                    {(topic.lessonCount ?? 0) > 0 && (
                      <button
                        onClick={() => startTopicLesson(topic)}
                        className="btn btn-outline"
                        style={{ flex: 1, fontSize: '0.8rem', padding: '0.5rem' }}
                      >
                        {t.topics.lessonLabel}
                      </button>
                    )}
                    <button
                      onClick={() => startTopicPractice(topic)}
                      className="btn btn-primary"
                      style={{ flex: 1, fontSize: '0.8rem', padding: '0.5rem' }}
                    >
                      {t.topics.practiceLabel}
                    </button>
                  </div>
                </div>
              ))}
            </div>
            </ProGate>
          </div>
        ) : null}

        {topics.length === 0 && (
          <div className="card" style={{ textAlign: 'center', padding: '3rem' }}>
            <p style={{ color: 'var(--text-secondary)' }}>{t.topics.noTopics}</p>
          </div>
        )}
      </div>
    );
  }

  const currentQuestion = questions[currentIndex];

  if (!currentQuestion) {
    return (
      <div className="animate-fade-in" style={{ maxWidth: '700px', margin: '0 auto' }}>
        <div className="card" style={{ textAlign: 'center', padding: '3rem 2rem' }}>
          <div style={{
            width: '64px',
            height: '64px',
            borderRadius: '50%',
            backgroundColor: 'rgba(79, 70, 229, 0.1)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: '2rem',
            margin: '0 auto 1.25rem'
          }}>
            📝
          </div>
          <h2 style={{ fontSize: '1.35rem', fontWeight: '700', marginBottom: '0.75rem' }}>
            {t.practice.noQuestionsYet}
          </h2>
          <p style={{ color: 'var(--text-secondary)', marginBottom: '1.5rem', lineHeight: 1.5 }}>
            {t.practice.noQuestionsYetDesc}
          </p>
          <button onClick={() => setViewMode('topics')} className="btn btn-primary">
            {t.topics.backToTopics}
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ maxWidth: '700px', margin: '0 auto' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <span style={{ 
          padding: '0.375rem 0.75rem', 
          backgroundColor: 'rgba(79, 70, 229, 0.1)', 
          color: 'var(--primary-color)',
          borderRadius: '9999px',
          fontSize: '0.875rem',
          fontWeight: '500'
        }}>
          {currentTopic?.topicName}
        </span>
        <button onClick={() => setViewMode('topics')} className="btn btn-outline">
          {t.topics.backToTopics}
        </button>
      </div>

      <div className="card" style={{ padding: '1rem', marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.875rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>
          <span>{t.topics.questionOf.replace('{n}', String(currentIndex + 1)).replace('{total}', String(questions.length))}</span>
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

      <div className="card card-static animate-fade-in-up" key={currentQuestion.id}>
        <div style={{ marginBottom: '1rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <span style={{ 
            padding: '0.25rem 0.75rem', 
            borderRadius: '9999px', 
            fontSize: '0.75rem', 
            fontWeight: '500',
            ...getDifficultyStyle(currentQuestion.difficulty)
          }}>
            {currentQuestion.difficulty === 'Easy' ? t.practice.easy :
             currentQuestion.difficulty === 'Medium' ? t.practice.medium : t.practice.hard}
          </span>

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
              {hintLoading ? '...' : showHint ? t.practice.hideHint : t.practice.hint}
            </button>
          )}
        </div>

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
            <span style={{ fontWeight: '600', color: 'var(--warning-color)' }}>{t.practice.hint}: </span>
            {hintText}
          </div>
        )}

        <h2 style={{ fontSize: '1.125rem', fontWeight: '600', marginBottom: '1.5rem', color: 'var(--text-primary)' }}>
          {currentQuestion.text}
        </h2>

        {currentQuestion.imageUrl && (
          <img
            src={currentQuestion.imageUrl}
            alt="question"
            style={{ maxWidth: '100%', maxHeight: '360px', objectFit: 'contain', borderRadius: 8, marginBottom: '1.5rem', display: 'block' }}
          />
        )}

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

        {answerResult && (
          <div className="animate-fade-in-up" style={{
            marginTop: '1.5rem',
            padding: '1rem',
            borderRadius: '0.75rem',
            backgroundColor: answerResult.isCorrect ? 'rgba(16, 185, 129, 0.1)' : 'rgba(239, 68, 68, 0.1)',
            border: `1px solid ${answerResult.isCorrect ? 'var(--success-color)' : 'var(--error-color)'}`
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
              <span style={{ fontSize: '1.5rem' }}>{answerResult.isCorrect ? '✓' : '✕'}</span>
              <span style={{ fontWeight: '600', color: answerResult.isCorrect ? 'var(--success-color)' : 'var(--error-color)' }}>
                {answerResult.isCorrect ? t.practice.correct : t.practice.incorrect}
              </span>
            </div>
            
            {!answerResult.isCorrect && (
              <p style={{ color: 'var(--text-primary)', marginBottom: '0.5rem' }}>
                <span style={{ fontWeight: '500' }}>{t.practice.correctAnswer}: </span>
                {answerResult.correctOptionText}
              </p>
            )}
            
            {answerResult.explanation && (
              <div style={{ marginTop: '0.75rem', paddingTop: '0.75rem', borderTop: '1px solid var(--border-color)' }}>
                <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                  <span style={{ fontWeight: '500' }}>{t.practice.explanation}: </span>
                  {answerResult.explanation}
                </p>
              </div>
            )}
          </div>
        )}

        <div style={{ marginTop: '1.5rem' }}>
          {!answerResult ? (
            <button
              onClick={handleSubmit}
              disabled={selectedOption === null || submitting}
              className="btn btn-primary"
              style={{ width: '100%' }}
            >
              {submitting ? t.topics.checking : t.topics.answer}
            </button>
          ) : (
            <button
              onClick={handleNext}
              className="btn btn-primary"
              style={{ width: '100%' }}
            >
              {currentIndex < questions.length - 1 ? t.topics.nextQ : t.topics.finish}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
