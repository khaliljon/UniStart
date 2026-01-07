import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, useParams } from 'react-router-dom';
import { Plus, Trash2, Save, ArrowLeft, FileX } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';

interface Answer {
  text: string;
  isCorrect: boolean;
}

interface Question {
  text: string;
  points: number;
  explanation: string;
  answers: Answer[];
}

interface QuizForm {
  title: string;
  description?: string;
  subject: string;
  difficulty: string;
  timeLimit: number;
  quizType: string; // Standalone, PracticeQuiz, ModuleFinalQuiz, CourseFinalQuiz, CaseStudyQuiz
  isPublic: boolean;
  isPublished: boolean;
  isLearningMode: boolean;
  questions: Question[];
  // Связи с иерархией (опционально)
  topicId?: number;
  moduleId?: number;
  competencyId?: number;
}

const EditQuizPage = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { isAdmin } = useAuth();
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  const [subjects, setSubjects] = useState<any[]>([]);
  // const [courses, setCourses] = useState<any[]>([]); // Unused - commented out
  const [selectedSubjectId, setSelectedSubjectId] = useState<number | null>(null);
  const [selectedModuleId, setSelectedModuleId] = useState<number | null>(null);
  const [selectedCompetencyId, setSelectedCompetencyId] = useState<number | null>(null);
  const [selectedTopicId, setSelectedTopicId] = useState<number | null>(null);
  const [subjectHierarchy, setSubjectHierarchy] = useState<any>(null);
  
  const [quiz, setQuiz] = useState<QuizForm>({
    title: '',
    description: '',
    subject: '',
    difficulty: 'Medium',
    timeLimit: 30,
    quizType: 'Standalone',
    isPublic: false,
    isPublished: false,
    isLearningMode: false,
    questions: [],
  });

  useEffect(() => {
    loadSubjects();
    // loadCourses(); // Commented out - courses not used
    loadQuiz();
  }, [id]);

  useEffect(() => {
    if (selectedSubjectId) {
      loadSubjectHierarchy(selectedSubjectId);
    }
  }, [selectedSubjectId]);

  const loadSubjects = async () => {
    try {
      const response = await api.get('/subjects');
      setSubjects(response.data);
    } catch (error) {
      console.error('Ошибка загрузки предметов:', error);
    }
  };

  // Unused function - courses variable commented out
  // const loadCourses = async () => {
  //   try {
  //     const response = await api.get('/courses');
  //     setCourses(response.data || []);
  //   } catch (error) {
  //     console.error('Ошибка загрузки курсов:', error);
  //   }
  // };

  const loadSubjectHierarchy = async (subjectId: number) => {
    try {
      const response = await api.get(`/subjects/${subjectId}/hierarchy`);
      setSubjectHierarchy(response.data);
    } catch (error) {
      console.error('Ошибка загрузки иерархии предмета:', error);
      setSubjectHierarchy(null);
    }
  };

  const loadQuiz = async () => {
    try {
      setFetching(true);
      const response = await api.get(`/quizzes/${id}`);
      const quizData = response.data;
      
      const subject = subjects.find((s: any) => s.name === quizData.subject);
      if (subject) {
        setSelectedSubjectId(subject.id);
      }

      // Загружаем связи с иерархией
      if (quizData.topicId) setSelectedTopicId(quizData.topicId);
      if (quizData.moduleId) setSelectedModuleId(quizData.moduleId);
      if (quizData.competencyId) setSelectedCompetencyId(quizData.competencyId);

      setQuiz({
        title: quizData.title,
        description: quizData.description || '',
        subject: quizData.subject,
        difficulty: quizData.difficulty,
        timeLimit: quizData.timeLimit,
        quizType: quizData.type || 'Standalone',
        isPublic: isAdmin ? true : (quizData.isPublic || false),
        isPublished: quizData.isPublished || false,
        isLearningMode: quizData.isLearningMode || false,
        topicId: quizData.topicId,
        moduleId: quizData.moduleId,
        competencyId: quizData.competencyId,
        questions: quizData.questions.map((q: any) => ({
          text: q.text,
          points: q.points,
          explanation: q.explanation || '',
          answers: q.answers.map((a: any) => ({
            text: a.text,
            isCorrect: a.isCorrect,
          })),
        })),
      });
    } catch (error) {
      console.error('Ошибка загрузки квиза:', error);
      alert('Не удалось загрузить квиз');
      navigate('/quizzes');
    } finally {
      setFetching(false);
    }
  };

  const addQuestion = () => {
    setQuiz({
      ...quiz,
      questions: [
        ...quiz.questions,
        {
          text: '',
          points: 1,
          explanation: '',
          answers: [
            { text: '', isCorrect: false },
            { text: '', isCorrect: false },
          ],
        },
      ],
    });
  };

  const removeQuestion = (index: number) => {
    const newQuestions = quiz.questions.filter((_, i) => i !== index);
    setQuiz({ ...quiz, questions: newQuestions });
  };

  const updateQuestion = (index: number, field: keyof Question, value: any) => {
    const newQuestions = [...quiz.questions];
    newQuestions[index] = { ...newQuestions[index], [field]: value };
    setQuiz({ ...quiz, questions: newQuestions });
  };

  const addAnswer = (questionIndex: number) => {
    const newQuestions = [...quiz.questions];
    newQuestions[questionIndex].answers.push({ text: '', isCorrect: false });
    setQuiz({ ...quiz, questions: newQuestions });
  };

  const removeAnswer = (questionIndex: number, answerIndex: number) => {
    const newQuestions = [...quiz.questions];
    newQuestions[questionIndex].answers = newQuestions[questionIndex].answers.filter(
      (_, i) => i !== answerIndex
    );
    setQuiz({ ...quiz, questions: newQuestions });
  };

  const updateAnswer = (
    questionIndex: number,
    answerIndex: number,
    field: keyof Answer,
    value: any
  ) => {
    const newQuestions = [...quiz.questions];
    newQuestions[questionIndex].answers[answerIndex] = {
      ...newQuestions[questionIndex].answers[answerIndex],
      [field]: value,
    };
    setQuiz({ ...quiz, questions: newQuestions });
  };

  const toggleCorrectAnswer = (questionIndex: number, answerIndex: number) => {
    setQuiz(prev => {
      const newQuestions = [...prev.questions];
      const question = { ...newQuestions[questionIndex] };
      
      // Для одиночного выбора - только один правильный ответ
      question.answers = question.answers.map((answer, i) => ({
        ...answer,
        isCorrect: i === answerIndex
      }));
      
      newQuestions[questionIndex] = question;
      return { ...prev, questions: newQuestions };
    });
  };

  const handleSubmit = async (e: React.FormEvent, publish: boolean = false) => {
    e.preventDefault();

    if (!quiz.title || !quiz.subject) {
      alert('Заполните название и предмет квиза');
      return;
    }

    if (quiz.questions.length === 0) {
      alert('Добавьте хотя бы один вопрос');
      return;
    }

    // Проверка вопросов
    for (let i = 0; i < quiz.questions.length; i++) {
      const q = quiz.questions[i];
      if (!q.text.trim()) {
        alert(`Вопрос ${i + 1}: введите текст вопроса`);
        return;
      }
      
      const validAnswers = q.answers.filter(a => a.text.trim());
      if (validAnswers.length < 2) {
        alert(`Вопрос ${i + 1}: добавьте минимум 2 варианта ответа`);
        return;
      }
      
      // Проверка дубликатов вариантов ответов
      const answerTexts = validAnswers.map(a => a.text.trim().toLowerCase());
      const uniqueAnswers = new Set(answerTexts);
      if (uniqueAnswers.size !== answerTexts.length) {
        alert(`Вопрос ${i + 1}: варианты ответов не должны повторяться`);
        return;
      }
      
      if (!validAnswers.some((a) => a.isCorrect)) {
        alert(`Вопрос ${i + 1}: отметьте правильный ответ`);
        return;
      }
    }

    // Валидация для связанных квизов
    if (quiz.quizType !== 'Standalone') {
      if (quiz.quizType === 'PracticeQuiz' && !selectedTopicId) {
        alert('Для практического квиза выберите тему!');
        return;
      }
      if ((quiz.quizType === 'ModuleFinalQuiz' || quiz.quizType === 'CaseStudyQuiz') && !selectedModuleId) {
        alert('Для финального или кейс-квиза модуля выберите модуль!');
        return;
      }
    }

    try {
      setLoading(true);

      const quizData: any = {
        title: quiz.title,
        description: quiz.description || '',
        subject: quiz.subject,
        difficulty: quiz.difficulty,
        timeLimit: quiz.timeLimit,
        quizType: quiz.quizType,
        isPublic: quiz.isPublic,
        isPublished: publish ? true : quiz.isPublished,
        isLearningMode: quiz.isLearningMode,
        questions: quiz.questions.map((q, index) => ({
          text: q.text,
          points: q.points,
          explanation: q.explanation || '',
          order: index,
          answers: q.answers.map((a, aIndex) => ({
            text: a.text,
            isCorrect: a.isCorrect,
            order: aIndex,
          })),
        })),
      };

      await api.put(`/quizzes/${id}`, quizData);
      
      // Публикуем если нужно
      if (publish && !quiz.isPublished) {
        await api.patch(`/quizzes/${id}/publish`);
      }
      
      alert('Квиз успешно обновлен!');
      navigate('/quizzes');
    } catch (error: any) {
      console.error('Ошибка обновления квиза:', error);
      console.error('Response:', error.response);
      console.error('Response data:', error.response?.data);
      console.error('Response status:', error.response?.status);
      
      let errorMessage = 'Не удалось обновить квиз';
      
      if (error.response?.data) {
        const data = error.response.data;
        
        // Обработка ошибок валидации ModelState
        if (data.errors) {
          const errorMessages = Object.entries(data.errors)
            .map(([field, messages]: [string, any]) => {
              const msgs = Array.isArray(messages) ? messages : [messages];
              return `${field}: ${msgs.join(', ')}`;
            })
            .join('\n');
          errorMessage = `Ошибки валидации:\n${errorMessages}`;
        } else if (data.message) {
          errorMessage = data.message;
        } else if (data.title) {
          errorMessage = data.title;
        } else if (typeof data === 'string') {
          errorMessage = data;
        }
      }
      
      alert(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  if (fetching) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <motion.div
          animate={{ rotate: 360 }}
          transition={{ duration: 1, repeat: Infinity, ease: 'linear' }}
          className="w-16 h-16 border-4 border-primary-500 border-t-transparent rounded-full"
        />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8">
      <div className="max-w-4xl mx-auto px-4">
        <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Button
              onClick={() => navigate('/quizzes')}
              variant="secondary"
              className="flex items-center gap-2"
            >
              <ArrowLeft className="w-4 h-4" />
              Назад
            </Button>
            <h1 className="text-3xl font-bold text-gray-900">Редактирование квиза</h1>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Основная информация */}
          <Card>
            <h2 className="text-xl font-semibold mb-4">Основная информация</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Название квиза *
                </label>
                <input
                  type="text"
                  value={quiz.title}
                  onChange={(e) => setQuiz({ ...quiz, title: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  placeholder="Например: Математика - Алгебра 9 класс"
                  required
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Предмет *
                  </label>
                  <select
                    value={quiz.subject}
                    onChange={(e) => {
                      setQuiz({ ...quiz, subject: e.target.value });
                      const subject = subjects.find((s: any) => s.name === e.target.value);
                      setSelectedSubjectId(subject?.id || null);
                    }}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    required
                  >
                    <option value="">Выберите предмет</option>
                    {subjects.map((subject) => (
                      <option key={subject.id} value={subject.name}>
                        {subject.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Сложность
                  </label>
                  <select
                    value={quiz.difficulty}
                    onChange={(e) => setQuiz({ ...quiz, difficulty: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  >
                    <option value="Easy">Легкий</option>
                    <option value="Medium">Средний</option>
                    <option value="Hard">Сложный</option>
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Тип квиза *
                </label>
                <select
                  value={quiz.quizType}
                  onChange={(e) => {
                    setQuiz({ ...quiz, quizType: e.target.value });
                    setSelectedModuleId(null);
                    setSelectedCompetencyId(null);
                    setSelectedTopicId(null);
                  }}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  required
                >
                  <option value="Standalone">Обычный квиз (независимый)</option>
                  <option value="PracticeQuiz">Практический квиз по теме (с объяснениями)</option>
                  <option value="ModuleFinalQuiz">Итоговый квиз модуля (без объяснений)</option>
                  <option value="CaseStudyQuiz">Кейс-квиз модуля (анализ данных)</option>
                  <option value="CourseFinalQuiz">Пробный тест ЕНТ</option>
                </select>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Время на прохождение (минуты)
                  </label>
                  <input
                    type="number"
                    value={quiz.timeLimit}
                    onChange={(e) => setQuiz({ ...quiz, timeLimit: Number(e.target.value) })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    min="1"
                  />
                </div>

                {!isAdmin && (
                  <div className="flex items-end">
                    <label className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={quiz.isPublic}
                        onChange={(e) => setQuiz({ ...quiz, isPublic: e.target.checked })}
                        className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                      />
                      <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                        Публичный квиз (доступен всем)
                      </span>
                    </label>
                  </div>
                )}
              </div>

              <div className="mt-6 space-y-3">
                <label className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    checked={quiz.isLearningMode}
                    onChange={(e) => setQuiz({ ...quiz, isLearningMode: e.target.checked })}
                    className="w-4 h-4 text-primary-600 rounded focus:ring-primary-500"
                  />
                  <span className="text-sm text-gray-700 dark:text-gray-300">
                    Режим обучения (показывать объяснения сразу после каждого ответа)
                  </span>
                </label>
                <p className="text-xs text-gray-500 dark:text-gray-400 ml-6">
                  В режиме обучения студент увидит правильный ответ и объяснение сразу после выбора. 
                  В обычном режиме результаты показываются только в конце.
                </p>
              </div>

              {/* Связи с иерархией в зависимости от типа квиза */}
              {quiz.quizType !== 'Standalone' && selectedSubjectId && subjectHierarchy && (
                <div className="mt-6 p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg border border-blue-200 dark:border-blue-800">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
                    Связь с иерархией обучения
                  </h3>
                  
                  {/* Выбор модуля (для всех типов кроме PracticeQuiz) */}
                  {(quiz.quizType === 'ModuleFinalQuiz' || quiz.quizType === 'CaseStudyQuiz' || quiz.quizType === 'CourseFinalQuiz') && (
                    <div className="mb-4">
                      <label className="block text-sm font-medium text-gray-700 mb-2">
                        Модуль *
                      </label>
                      <select
                        required
                        value={selectedModuleId || ''}
                        onChange={(e) => {
                          setSelectedModuleId(parseInt(e.target.value) || null);
                          setSelectedCompetencyId(null);
                          setSelectedTopicId(null);
                        }}
                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                      >
                        <option value="">Выберите модуль</option>
                        {subjectHierarchy.modules?.map((module: any) => (
                          <option key={module.id} value={module.id}>
                            {module.icon} {module.title}
                          </option>
                        ))}
                      </select>
                    </div>
                  )}

                  {/* Выбор компетенции и темы (для PracticeQuiz) */}
                  {quiz.quizType === 'PracticeQuiz' && (
                    <>
                      <div className="mb-4">
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Модуль
                        </label>
                        <select
                          value={selectedModuleId || ''}
                          onChange={(e) => {
                            setSelectedModuleId(parseInt(e.target.value) || null);
                            setSelectedCompetencyId(null);
                            setSelectedTopicId(null);
                          }}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                        >
                          <option value="">Выберите модуль (опционально)</option>
                          {subjectHierarchy.modules?.map((module: any) => (
                            <option key={module.id} value={module.id}>
                              {module.icon} {module.title}
                            </option>
                          ))}
                        </select>
                      </div>

                      {selectedModuleId && (
                        <>
                          <div className="mb-4">
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                              Компетенция
                            </label>
                            <select
                              value={selectedCompetencyId || ''}
                              onChange={(e) => {
                                setSelectedCompetencyId(parseInt(e.target.value) || null);
                                setSelectedTopicId(null);
                              }}
                              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                            >
                              <option value="">Выберите компетенцию</option>
                              {subjectHierarchy.modules
                                ?.find((m: any) => m.id === selectedModuleId)
                                ?.competencies?.map((comp: any) => (
                                  <option key={comp.id} value={comp.id}>
                                    {comp.icon} {comp.title}
                                  </option>
                                ))}
                            </select>
                          </div>

                          {selectedCompetencyId && (
                            <div className="mb-4">
                              <label className="block text-sm font-medium text-gray-700 mb-2">
                                Тема *
                              </label>
                              <select
                                required
                                value={selectedTopicId || ''}
                                onChange={(e) => setSelectedTopicId(parseInt(e.target.value) || null)}
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                              >
                                <option value="">Выберите тему</option>
                                {subjectHierarchy.modules
                                  ?.find((m: any) => m.id === selectedModuleId)
                                  ?.competencies?.find((c: any) => c.id === selectedCompetencyId)
                                  ?.topics?.map((topic: any) => (
                                    <option key={topic.id} value={topic.id}>
                                      {topic.icon} {topic.title}
                                    </option>
                                  ))}
                              </select>
                            </div>
                          )}
                        </>
                      )}
                    </>
                  )}
                </div>
              )}
            </div>
          </Card>

          {/* Вопросы */}
          <Card>
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold">
                Вопросы ({quiz.questions.length})
              </h2>
              <Button
                type="button"
                onClick={addQuestion}
                variant="primary"
                className="flex items-center gap-2"
              >
                <Plus className="w-4 h-4" />
                Добавить вопрос
              </Button>
            </div>

            {quiz.questions.length === 0 ? (
              <div className="text-center py-12 text-gray-500">
                <p>Нет вопросов. Нажмите "Добавить вопрос" чтобы начать.</p>
              </div>
            ) : (
              <div className="space-y-6">
                {quiz.questions.map((question, qIndex) => (
                  <motion.div
                    key={qIndex}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="border border-gray-200 rounded-lg p-4 bg-gray-50"
                  >
                    <div className="flex items-start justify-between mb-4">
                      <h3 className="font-semibold text-gray-900">Вопрос {qIndex + 1}</h3>
                      <div className="flex items-center gap-2">
                        <input
                          type="number"
                          min="1"
                          value={question.points}
                          onChange={(e) =>
                            updateQuestion(qIndex, 'points', parseInt(e.target.value))
                          }
                          className="w-20 px-2 py-1 text-sm border border-gray-300 rounded focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          placeholder="Баллы"
                        />
                        {quiz.questions.length > 1 && (
                          <button
                            type="button"
                            onClick={() => removeQuestion(qIndex)}
                            className="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors"
                            title="Удалить вопрос"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                        )}
                      </div>
                    </div>

                    <div className="space-y-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Текст вопроса *
                        </label>
                        <textarea
                          value={question.text}
                          onChange={(e) => updateQuestion(qIndex, 'text', e.target.value)}
                          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent resize-none"
                          rows={2}
                          placeholder="Введите вопрос..."
                          required
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                          Объяснение (опционально)
                        </label>
                        <textarea
                          value={question.explanation}
                          onChange={(e) => updateQuestion(qIndex, 'explanation', e.target.value)}
                          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent resize-none"
                          rows={2}
                          placeholder="Объяснение правильного ответа..."
                        />
                      </div>

                      {/* Варианты ответов */}
                      <div>
                        <div className="flex items-center justify-between mb-2">
                          <label className="text-sm font-medium text-gray-700">
                            Варианты ответов *
                          </label>
                          {question.answers.length < 5 && (
                            <button
                              type="button"
                              onClick={() => addAnswer(qIndex)}
                              className="text-sm text-primary-600 hover:text-primary-700 flex items-center gap-1"
                            >
                              <Plus className="w-4 h-4" />
                              Добавить вариант
                            </button>
                          )}
                        </div>

                        <div className="space-y-2">
                          {question.answers.map((answer, aIndex) => (
                            <div key={aIndex} className="flex items-center gap-2">
                              <input
                                type="radio"
                                name={`correct-${qIndex}`}
                                checked={answer.isCorrect}
                                onChange={() => toggleCorrectAnswer(qIndex, aIndex)}
                                className="w-4 h-4 text-green-600 border-gray-300 focus:ring-green-500"
                                title="Отметить как правильный ответ"
                              />
                              <input
                                type="text"
                                value={answer.text}
                                onChange={(e) =>
                                  updateAnswer(qIndex, aIndex, 'text', e.target.value)
                                }
                                className={`flex-1 px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent ${
                                  answer.isCorrect && answer.text.trim()
                                    ? 'border-green-500 bg-green-50'
                                    : 'border-gray-300'
                                }`}
                                placeholder={`Вариант ${aIndex + 1}`}
                                required
                              />
                              {question.answers.length > 2 && (
                                <button
                                  type="button"
                                  onClick={() => removeAnswer(qIndex, aIndex)}
                                  className="p-1.5 text-red-600 hover:bg-red-50 rounded transition-colors"
                                  title="Удалить вариант"
                                >
                                  <Trash2 className="w-4 h-4" />
                                </button>
                              )}
                            </div>
                          ))}
                        </div>
                        <p className="text-xs text-gray-500 mt-1">
                          Отметьте один правильный ответ
                        </p>
                      </div>
                    </div>
                  </motion.div>
                ))}

                <Button
                  type="button"
                  variant="primary"
                  onClick={addQuestion}
                  className="w-full flex items-center justify-center gap-2 py-3"
                >
                  <Plus className="w-5 h-5" />
                  Добавить вопрос
                </Button>
              </div>
            )}
          </Card>

          {/* Кнопки действий */}
          <div className="flex justify-end gap-3">
            <Button
              type="button"
              onClick={() => navigate('/quizzes')}
              variant="secondary"
              disabled={loading}
            >
              Отмена
            </Button>
            <Button
              type="button"
              onClick={(e: any) => handleSubmit(e, false)}
              variant="secondary"
              disabled={loading}
              className="flex items-center gap-2"
            >
              <Save className="w-4 h-4" />
              {loading ? 'Сохранение...' : 'Сохранить'}
            </Button>
            {quiz.isPublished ? (
              <Button
                type="button"
                onClick={async () => {
                  try {
                    await api.patch(`/quizzes/${id}/unpublish`);
                    setQuiz({ ...quiz, isPublished: false });
                    alert('Квиз снят с публикации');
                  } catch (error) {
                    console.error('Ошибка снятия с публикации:', error);
                    alert('Не удалось снять квиз с публикации');
                  }
                }}
                variant="secondary"
                disabled={loading}
                className="flex items-center gap-2"
              >
                <FileX className="w-4 h-4" />
                Снять с публикации
              </Button>
            ) : (
              <Button
                type="button"
                onClick={(e: any) => handleSubmit(e, true)}
                variant="primary"
                disabled={loading}
                className="flex items-center gap-2"
              >
                <Save className="w-4 h-4" />
                {loading ? 'Публикация...' : 'Опубликовать'}
              </Button>
            )}
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditQuizPage;
