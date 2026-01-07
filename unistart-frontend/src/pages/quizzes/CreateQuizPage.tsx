import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Plus, Trash2, Save, ArrowLeft } from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import api from '../../services/api';
import { useAuth } from '../../context/AuthContext';

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

const CreateQuizPage = () => {
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  const [loading, setLoading] = useState(false);
  const [subjects, setSubjects] = useState<any[]>([]);
  const [courses, setCourses] = useState<any[]>([]);
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
    isPublic: isAdmin, // Админ всегда создает публичные квизы
    isPublished: false,
    isLearningMode: false,
    questions: [],
  });

  useEffect(() => {
    loadSubjects();
    loadCourses();
  }, []);

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

  const loadCourses = async () => {
    try {
      const response = await api.get('/courses');
      setCourses(response.data || []);
    } catch (error) {
      console.error('Ошибка загрузки курсов:', error);
    }
  };

  const loadSubjectHierarchy = async (subjectId: number) => {
    try {
      const response = await api.get(`/subjects/${subjectId}/hierarchy`);
      setSubjectHierarchy(response.data);
    } catch (error) {
      console.error('Ошибка загрузки иерархии предмета:', error);
      setSubjectHierarchy(null);
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

  const handleSubmit = async (e: React.FormEvent, publish: boolean = false) => {
    e.preventDefault();
    
    if (quiz.questions.length === 0) {
      alert('Добавьте хотя бы один вопрос!');
      return;
    }

    // Валидация вопросов
    for (let i = 0; i < quiz.questions.length; i++) {
      const question = quiz.questions[i];
      
      if (!question.text.trim()) {
        alert(`Вопрос ${i + 1}: введите текст вопроса!`);
        return;
      }
      
      const validAnswers = question.answers.filter(a => a.text.trim());
      if (validAnswers.length < 2) {
        alert(`Вопрос ${i + 1}: добавьте минимум 2 варианта ответа!`);
        return;
      }
      
      // Проверка дубликатов вариантов ответов
      const answerTexts = validAnswers.map(a => a.text.trim().toLowerCase());
      const uniqueAnswers = new Set(answerTexts);
      if (uniqueAnswers.size !== answerTexts.length) {
        alert(`Вопрос ${i + 1}: варианты ответов не должны повторяться!`);
        return;
      }
      
      // Проверка правильного ответа
      if (!question.answers.some(a => a.isCorrect && a.text.trim())) {
        alert(`Вопрос ${i + 1}: отметьте хотя бы один правильный ответ!`);
        return;
      }
    }

    // Валидация для связанных квизов
    if (quiz.quizType !== 'Standalone') {
      if (quiz.quizType === 'PracticeQuiz' && !selectedTopicId) {
        alert('Для практического квиза выберите тему!');
        setLoading(false);
        return;
      }
      if ((quiz.quizType === 'ModuleFinalQuiz' || quiz.quizType === 'CaseStudyQuiz') && !selectedModuleId) {
        alert('Для финального или кейс-квиза модуля выберите модуль!');
        setLoading(false);
        return;
      }
      if (quiz.quizType === 'CourseFinalQuiz' && (!selectedSubjectId || !selectedModuleId)) {
        alert('Для финального квиза курса выберите предмет и модуль!');
        setLoading(false);
        return;
      }
    }

    setLoading(true);
    try {
      // Шаг 1: Создаем квиз
      const quizData: any = {
        title: quiz.title,
        subject: quiz.subject,
        difficulty: quiz.difficulty,
        timeLimit: quiz.timeLimit,
        description: quiz.description || quiz.title || 'Описание',
        isPublic: quiz.isPublic,
        isPublished: false, // Всегда создаем как черновик, потом публикуем отдельно
        isLearningMode: quiz.isLearningMode,
        type: quiz.quizType,
      };

      // Добавляем связи с иерархией
      if (selectedTopicId) quizData.topicId = selectedTopicId;
      if (selectedModuleId) quizData.moduleId = selectedModuleId;
      if (selectedCompetencyId) quizData.competencyId = selectedCompetencyId;

      const quizResponse = await api.post('/quizzes', quizData);

      const quizId = quizResponse.data.id;
      console.log('Quiz created with ID:', quizId);

      // Шаг 2: Добавляем вопросы к квизу
      for (const question of quiz.questions) {
        const questionResponse = await api.post('/quizzes/questions', {
          quizId: quizId,
          text: question.text,
          points: question.points,
          explanation: question.explanation || '',
        });

        const questionId = questionResponse.data.id;
        console.log('Question created with ID:', questionId);

        // Шаг 3: Добавляем ответы к вопросу
        for (const answer of question.answers) {
          await api.post('/quizzes/answers', {
            questionId: questionId,
            text: answer.text,
            isCorrect: answer.isCorrect,
          });
        }
      }

      // Шаг 4: Публикуем квиз если нужно
      if (publish) {
        await api.patch(`/quizzes/${quizId}/publish`);
      }

      alert(`Квиз успешно ${publish ? 'создан и опубликован' : 'сохранен как черновик'}!`);
      navigate('/quizzes');
    } catch (error: any) {
      console.error('Ошибка создания квиза:', error);
      console.error('Response:', error.response);
      console.error('Response data:', error.response?.data);
      console.error('Response status:', error.response?.status);
      
      let errorMessage = 'Ошибка создания квиза';
      
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

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-5xl mx-auto">
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <Button
            variant="secondary"
            onClick={() => navigate('/quizzes')}
            className="mb-4 flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Назад
          </Button>

          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-2">
            📝 Создание квиза
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            {isAdmin ? 'Создайте новый квиз' : 'Создайте новый квиз для своих студентов'}
          </p>
        </motion.div>

        <form onSubmit={handleSubmit}>
          {/* Основная информация */}
          <Card className="p-6 mb-6">
            <h2 className="text-2xl font-bold text-gray-900 mb-6">
              Основная информация
            </h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Название квиза *
                </label>
                <input
                  type="text"
                  required
                  value={quiz.title}
                  onChange={(e) => setQuiz({ ...quiz, title: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  placeholder="Введите название"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Предмет *
                </label>
                <select
                  required
                  value={quiz.subject}
                  onChange={(e) => {
                    setQuiz({ ...quiz, subject: e.target.value });
                    const subject = subjects.find(s => s.name === e.target.value);
                    setSelectedSubjectId(subject?.id || null);
                  }}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
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
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Тип квиза *
                </label>
                <select
                  required
                  value={quiz.quizType}
                  onChange={(e) => {
                    setQuiz({ ...quiz, quizType: e.target.value });
                    // Сбрасываем выбор иерархии при смене типа
                    setSelectedModuleId(null);
                    setSelectedCompetencyId(null);
                    setSelectedTopicId(null);
                  }}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                >
                  <option value="Standalone">Обычный квиз (независимый)</option>
                  <option value="PracticeQuiz">Практический квиз по теме (с объяснениями)</option>
                  <option value="ModuleFinalQuiz">Итоговый квиз модуля (без объяснений)</option>
                  <option value="CaseStudyQuiz">Кейс-квиз модуля (анализ данных)</option>
                  <option value="CourseFinalQuiz">Пробный тест ЕНТ</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
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

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Ограничение времени (минуты)
                </label>
                <input
                  type="number"
                  min="1"
                  value={quiz.timeLimit}
                  onChange={(e) => setQuiz({ ...quiz, timeLimit: parseInt(e.target.value) })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                />
              </div>
            </div>

            {/* Описание */}
            <div className="mt-6">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Описание
              </label>
              <textarea
                value={quiz.description || ''}
                onChange={(e) => setQuiz({ ...quiz, description: e.target.value })}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                rows={3}
                placeholder="Введите описание квиза (необязательно)"
              />
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

            {!isAdmin && (
              <div className="mt-6 space-y-3">
                <label className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    checked={quiz.isPublic}
                    onChange={(e) => setQuiz({ ...quiz, isPublic: e.target.checked })}
                    className="w-4 h-4 text-primary-600 rounded focus:ring-primary-500"
                  />
                  <span className="text-sm text-gray-700 dark:text-gray-300">
                    Публичный доступ (доступен всем студентам, а не только вашим)
                  </span>
                </label>
              </div>
            )}

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
          </Card>

          {/* Вопросы */}
          <Card className="p-6 mb-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-gray-900">
                Вопросы ({quiz.questions.length})
              </h2>
              <Button
                type="button"
                variant="primary"
                onClick={addQuestion}
                className="flex items-center gap-2"
              >
                <Plus className="w-5 h-5" />
                Добавить вопрос
              </Button>
            </div>

            {quiz.questions.length === 0 ? (
              <div className="text-center py-12 border-2 border-dashed border-gray-300 rounded-lg">
                <p className="text-gray-600 mb-4">Вопросы еще не добавлены</p>
                <Button
                  type="button"
                  variant="primary"
                  onClick={addQuestion}
                  className="flex items-center gap-2 mx-auto"
                >
                  <Plus className="w-5 h-5" />
                  Добавить первый вопрос
                </Button>
              </div>
            ) : (
              <div className="space-y-6">
                {quiz.questions.map((question, qIndex) => (
                  <div
                    key={qIndex}
                    className="border border-gray-300 rounded-lg p-6 bg-white"
                  >
                    <div className="flex items-start justify-between mb-4">
                      <h3 className="text-lg font-semibold text-gray-900">
                        Вопрос {qIndex + 1}
                      </h3>
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
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Текст вопроса *
                        </label>
                        <textarea
                          required
                          value={question.text}
                          onChange={(e) => updateQuestion(qIndex, 'text', e.target.value)}
                          rows={3}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          placeholder="Введите текст вопроса"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Объяснение (опционально)
                        </label>
                        <textarea
                          value={question.explanation}
                          onChange={(e) =>
                            updateQuestion(qIndex, 'explanation', e.target.value)
                          }
                          rows={2}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          placeholder="Объяснение правильного ответа"
                        />
                      </div>

                      {/* Ответы */}
                      <div>
                        <div className="flex items-center justify-between mb-3">
                          <label className="block text-sm font-medium text-gray-700">
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

                        <div className="space-y-3">
                          {question.answers.map((answer, aIndex) => (
                            <div key={aIndex} className="flex items-start gap-3">
                              <input
                                type="radio"
                                name={`question-${qIndex}-correct`}
                                checked={answer.isCorrect}
                                onChange={() => {
                                  // Сбросить все остальные ответы
                                  const newQuestions = [...quiz.questions];
                                  newQuestions[qIndex].answers.forEach((a, i) => {
                                    a.isCorrect = i === aIndex;
                                  });
                                  setQuiz({ ...quiz, questions: newQuestions });
                                }}
                                className="mt-2 w-4 h-4 text-primary-600 rounded focus:ring-primary-500"
                                />
                                <input
                                  type="text"
                                  required
                                  value={answer.text}
                                  onChange={(e) =>
                                    updateAnswer(qIndex, aIndex, 'text', e.target.value)
                                  }
                                  className={`flex-1 px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent ${
                                    answer.isCorrect && answer.text.trim()
                                      ? 'border-green-500 bg-green-50'
                                      : 'border-gray-300'
                                  }`}
                                  placeholder={`Вариант ${aIndex + 1}`}
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
                      </div>
                    </div>
                  </div>
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
          <div className="flex items-center justify-end gap-4">
            <Button
              type="button"
              variant="secondary"
              onClick={() => navigate('/quizzes')}
            >
              Отмена
            </Button>
            <Button
              type="button"
              variant="secondary"
              onClick={(e: any) => handleSubmit(e, false)}
              disabled={loading || quiz.questions.length === 0}
              className="flex items-center gap-2"
            >
              <Save className="w-5 h-5" />
              {loading ? 'Сохранение...' : 'Сохранить как черновик'}
            </Button>
            <Button
              type="button"
              variant="primary"
              onClick={(e: any) => handleSubmit(e, true)}
              disabled={loading || quiz.questions.length === 0}
              className="flex items-center gap-2"
            >
              <Save className="w-5 h-5" />
              {loading ? 'Публикация...' : 'Опубликовать'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CreateQuizPage;
