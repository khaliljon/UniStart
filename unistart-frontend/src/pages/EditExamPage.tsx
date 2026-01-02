import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { motion } from 'framer-motion';
import {
  Plus,
  Trash2,
  Save,
  ArrowLeft,
  FileText,
  Clock,
  Award,
  AlertCircle,
  Settings,
  FileX,
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';

interface ExamQuestion {
  text: string;
  explanation: string;
  questionType: string;
  points: number;
  answers: ExamAnswer[];
}

interface ExamAnswer {
  text: string;
  isCorrect: boolean;
}

const EditExamPage = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { isAdmin } = useAuth();
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  interface SubjectOption { id: number; name: string }

  const [subjects, setSubjects] = useState<SubjectOption[]>([]);
  const [countries, setCountries] = useState<any[]>([]);
  const [universities, setUniversities] = useState<any[]>([]);
  const [examTypes, setExamTypes] = useState<any[]>([]);
  const [allExamTypes, setAllExamTypes] = useState<any[]>([]);

  // Основная информация
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [subject, setSubject] = useState(''); // Для обратной совместимости
  const [selectedSubjectIds, setSelectedSubjectIds] = useState<number[]>([]);
  const [difficulty, setDifficulty] = useState('Medium');
  
  // Международная система
  const [countryId, setCountryId] = useState<number | null>(null);
  const [universityId, setUniversityId] = useState<number | null>(null);
  const [examTypeId, setExamTypeId] = useState<number | null>(null);
  
  // Настройки экзамена
  const [timeLimit, setTimeLimit] = useState(60);
  const [passingScore, setPassingScore] = useState(70);
  const [maxAttempts, setMaxAttempts] = useState(3);
  const [isProctored, setIsProctored] = useState(false);
  const [shuffleQuestions, setShuffleQuestions] = useState(true);
  const [shuffleAnswers, setShuffleAnswers] = useState(true);
  const [showResultsAfter, setShowResultsAfter] = useState('Immediate');
  const [showCorrectAnswers, setShowCorrectAnswers] = useState(true);
  const [showDetailedFeedback, setShowDetailedFeedback] = useState(true);
  const [isPublished, setIsPublished] = useState(false);
  const [isPublic, setIsPublic] = useState(false);
  const [strictTiming, setStrictTiming] = useState(false);

  // Вопросы
  const [questions, setQuestions] = useState<ExamQuestion[]>([
    {
      text: '',
      explanation: '',
      questionType: 'SingleChoice',
      points: 1,
      answers: [
        { text: '', isCorrect: false },
        { text: '', isCorrect: false },
      ],
    },
  ]);

  useEffect(() => {
    const loadData = async () => {
      await loadSubjects();
      await loadInternationalData();
      await loadExam();
    };
    loadData();
  }, [id]);

  useEffect(() => {
    if (countryId) {
      loadUniversities(countryId);
    } else {
      setUniversities([]);
      setUniversityId(null);
    }
  }, [countryId]);

  useEffect(() => {
    if (universityId) {
      // Фильтруем типы экзаменов по выбранному университету
      const university = universities.find(u => u.id === universityId);
      if (university && university.examTypeIds && university.examTypeIds.length > 0) {
        // Фильтруем из всех типов только те, что поддерживает университет
        const filteredTypes = allExamTypes.filter(et => university.examTypeIds.includes(et.id));
        setExamTypes(filteredTypes);
        // Сбрасываем выбранный тип, если он не поддерживается университетом
        if (examTypeId && !university.examTypeIds.includes(examTypeId)) {
          setExamTypeId(null);
        }
      } else {
        // Если типов нет, показываем все
        setExamTypes(allExamTypes);
      }
    } else {
      // Если университет не выбран, показываем все типы
      setExamTypes(allExamTypes);
    }
  }, [universityId, universities, allExamTypes]);

  const loadSubjects = async (): Promise<SubjectOption[]> => {
    try {
      const response = await api.get('/subjects');
      setSubjects(response.data);
      return response.data;
    } catch (error) {
      console.error('Ошибка загрузки предметов:', error);
      return [];
    }
  };

  const loadInternationalData = async () => {
    try {
      const [countriesRes, examTypesRes] = await Promise.all([
        api.get('/countries'),
        api.get('/examtypes')
      ]);
      setCountries(countriesRes.data);
      setAllExamTypes(examTypesRes.data);
      setExamTypes(examTypesRes.data);
    } catch (error) {
      console.error('Ошибка загрузки международных данных:', error);
    }
  };

  const loadUniversities = async (cId: number) => {
    try {
      const response = await api.get(`/universities?countryId=${cId}`);
      setUniversities(response.data);
    } catch (error) {
      console.error('Ошибка загрузки университетов:', error);
    }
  };

  const loadExam = async () => {
    try {
      setFetching(true);
      const response = await api.get(`/exams/${id}`);
      const exam = response.data;
      const loadedSubjects = await loadSubjects(); // Убеждаемся, что предметы загружены

      setTitle(exam.title);
      setDescription(exam.description || '');
      setSubject(exam.subject || '');
      // Используем subjectIds если есть, иначе пытаемся найти по названиям
      if (exam.subjectIds && exam.subjectIds.length > 0) {
        setSelectedSubjectIds(exam.subjectIds);
      } else if (exam.subjects && exam.subjects.length > 0 && loadedSubjects.length > 0) {
        const foundIds = loadedSubjects
          .filter((s: SubjectOption) => exam.subjects.includes(s.name))
          .map((s: SubjectOption) => s.id);
        setSelectedSubjectIds(foundIds);
      } else {
        setSelectedSubjectIds([]);
      }
      setDifficulty(exam.difficulty);
      setCountryId(exam.countryId || null);
      setUniversityId(exam.universityId || null);
      setExamTypeId(exam.examTypeId || null);
      setTimeLimit(exam.timeLimit);
      setPassingScore(exam.passingScore);
      setMaxAttempts(exam.maxAttempts);
      setIsProctored(exam.isProctored);
      setShuffleQuestions(exam.shuffleQuestions);
      setShuffleAnswers(exam.shuffleAnswers);
      setShowResultsAfter(exam.showResultsAfter);
      setShowCorrectAnswers(exam.showCorrectAnswers);
      setShowDetailedFeedback(exam.showDetailedFeedback);
      setIsPublished(exam.isPublished);
      setStrictTiming(exam.strictTiming || false);

      if (exam.questions && exam.questions.length > 0) {
        setQuestions(
          exam.questions.map((q: any) => ({
            text: q.text,
            explanation: q.explanation || '',
            questionType: q.questionType || 'SingleChoice',
            points: q.points,
            answers: q.answers.map((a: any) => ({
              text: a.text,
              isCorrect: a.isCorrect,
            })),
          }))
        );
      }
    } catch (error) {
      console.error('Ошибка загрузки экзамена:', error);
      alert('Не удалось загрузить экзамен');
      navigate('/exams');
    } finally {
      setFetching(false);
    }
  };

  const addQuestion = () => {
    setQuestions([
      ...questions,
      {
        text: '',
        explanation: '',
        questionType: 'SingleChoice',
        points: 1,
        answers: [
          { text: '', isCorrect: false },
          { text: '', isCorrect: false },
        ],
      },
    ]);
  };

  const removeQuestion = (questionIndex: number) => {
    if (questions.length > 1) {
      setQuestions(questions.filter((_, i) => i !== questionIndex));
    }
  };

  const updateQuestion = (questionIndex: number, field: keyof ExamQuestion, value: any) => {
    const updatedQuestions = [...questions];
    updatedQuestions[questionIndex] = {
      ...updatedQuestions[questionIndex],
      [field]: value,
    };
    setQuestions(updatedQuestions);
  };

  const addAnswer = (questionIndex: number) => {
    const updatedQuestions = [...questions];
    updatedQuestions[questionIndex].answers.push({ text: '', isCorrect: false });
    setQuestions(updatedQuestions);
  };

  const removeAnswer = (questionIndex: number, answerIndex: number) => {
    const updatedQuestions = [...questions];
    if (updatedQuestions[questionIndex].answers.length > 2) {
      updatedQuestions[questionIndex].answers = updatedQuestions[questionIndex].answers.filter(
        (_, i) => i !== answerIndex
      );
      setQuestions(updatedQuestions);
    }
  };

  const updateAnswer = (
    questionIndex: number,
    answerIndex: number,
    field: keyof ExamAnswer,
    value: any
  ) => {
    const updatedQuestions = [...questions];
    updatedQuestions[questionIndex].answers[answerIndex] = {
      ...updatedQuestions[questionIndex].answers[answerIndex],
      [field]: value,
    };
    setQuestions(updatedQuestions);
  };

  const toggleCorrectAnswer = (questionIndex: number, answerIndex: number) => {
    const updatedQuestions = [...questions];
    const question = updatedQuestions[questionIndex];
    
    if (question.questionType === 'SingleChoice') {
      // Для одиночного выбора - сбросить все и выбрать один
      question.answers.forEach((a) => (a.isCorrect = false));
      question.answers[answerIndex].isCorrect = true;
    } else {
      // Для множественного выбора - переключить состояние
      question.answers[answerIndex].isCorrect = !question.answers[answerIndex].isCorrect;
    }
    
    setQuestions(updatedQuestions);
  };

  const validateExam = (): string | null => {
    if (!title.trim()) return 'Введите название экзамена';
    if (selectedSubjectIds.length === 0) return 'Выберите хотя бы один предмет';
    if (questions.length === 0) return 'Добавьте хотя бы один вопрос';

    for (let i = 0; i < questions.length; i++) {
      const q = questions[i];
      if (!q.text.trim()) return `Вопрос ${i + 1}: введите текст вопроса`;
      if (q.points <= 0) return `Вопрос ${i + 1}: баллы должны быть больше 0`;
      
      const validAnswers = q.answers.filter(a => a.text.trim());
      if (validAnswers.length < 2) return `Вопрос ${i + 1}: добавьте минимум 2 варианта ответа`;

      // Проверка дубликатов вариантов ответов
      const answerTexts = validAnswers.map(a => a.text.trim().toLowerCase());
      const uniqueAnswers = new Set(answerTexts);
      if (uniqueAnswers.size !== answerTexts.length) {
        return `Вопрос ${i + 1}: варианты ответов не должны повторяться`;
      }

      const hasCorrect = validAnswers.some((a) => a.isCorrect);
      if (!hasCorrect) return `Вопрос ${i + 1}: отметьте правильный ответ`;
    }

    return null;
  };

  const handleSubmit = async (publish?: boolean) => {
    const error = validateExam();
    if (error) {
      alert(error);
      return;
    }

    try {
      setLoading(true);

      const examData = {
        title,
        description,
        subject: selectedSubjectIds.length > 0 ? subjects.find(s => s.id === selectedSubjectIds[0])?.name || '' : subject, // Для обратной совместимости
        subjectIds: selectedSubjectIds.length > 0 ? selectedSubjectIds : [], // Новый способ
        difficulty,
        timeLimit,
        passingScore,
        maxAttempts,
        isProctored,
        shuffleQuestions,
        shuffleAnswers,
        showResultsAfter,
        showCorrectAnswers,
        showDetailedFeedback,
        isPublished: isPublished,
        isPublic: isPublic,
        strictTiming: strictTiming,
        countryId: countryId,
        universityId: universityId,
        examTypeId: examTypeId,
        tagIds: [],
        questions: questions.map((q, index) => ({
          text: q.text,
          explanation: q.explanation,
          questionType: q.questionType,
          points: q.points,
          order: index,
          answers: q.answers.map((a, aIndex) => ({
            text: a.text,
            isCorrect: a.isCorrect,
            order: aIndex,
          })),
        })),
      };

      await api.put(`/exams/${id}`, examData);
      
      // Публикуем если нужно
      if (publish && !isPublished) {
        await api.patch(`/exams/${id}/publish`);
        setIsPublished(true);
      }
      
      alert('Экзамен успешно обновлен!');
      navigate('/exams');
    } catch (error: any) {
      console.error('Ошибка обновления экзамена:', error);
      alert(error.response?.data?.message || 'Не удалось обновить экзамен');
    } finally {
      setLoading(false);
    }
  };

  const totalPoints = questions.reduce((sum, q) => sum + q.points, 0);

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
      <div className="max-w-5xl mx-auto px-4">
        {/* Заголовок */}
        <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Button
              onClick={() => navigate('/exams')}
              variant="secondary"
              className="flex items-center gap-2"
            >
              <ArrowLeft className="w-4 h-4" />
              Назад
            </Button>
            <h1 className="text-3xl font-bold text-gray-900 flex items-center gap-3">
              <FileText className="w-8 h-8 text-primary-500" />
              Редактирование экзамена
            </h1>
          </div>
        </div>

        {/* Основная информация */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
        >
          <Card className="mb-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <FileText className="w-5 h-5" />
              Основная информация
            </h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Название экзамена *
                </label>
                <input
                  type="text"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder="Например: Итоговый экзамен по математике"
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  maxLength={200}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Описание
                </label>
                <textarea
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder="Краткое описание экзамена..."
                  rows={3}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent resize-none"
                  maxLength={1000}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Предметы *
                  </label>
                  <select
                    multiple
                    value={selectedSubjectIds.map(id => id.toString())}
                    onChange={(e) => {
                      const selected = Array.from(e.target.selectedOptions, option => parseInt(option.value));
                      setSelectedSubjectIds(selected);
                    }}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent min-h-[100px]"
                    size={5}
                  >
                    {subjects.map((subject) => (
                      <option key={subject.id} value={subject.id}>
                        {subject.name}
                      </option>
                    ))}
                  </select>
                  <p className="text-xs text-gray-500 mt-1">
                    Удерживайте Ctrl (Cmd на Mac) для выбора нескольких предметов
                  </p>
                  {selectedSubjectIds.length > 0 && (
                    <div className="mt-2 flex flex-wrap gap-2">
                      {selectedSubjectIds.map((id) => {
                        const subj = subjects.find(s => s.id === id);
                        return subj ? (
                          <span
                            key={id}
                            className="inline-flex items-center gap-1 px-2 py-1 bg-blue-100 text-blue-800 rounded text-sm"
                          >
                            {subj.name}
                            <button
                              type="button"
                              onClick={() => setSelectedSubjectIds(selectedSubjectIds.filter(sid => sid !== id))}
                              className="text-blue-600 hover:text-blue-800"
                            >
                              ×
                            </button>
                          </span>
                        ) : null;
                      })}
                    </div>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Сложность
                  </label>
                  <select
                    value={difficulty}
                    onChange={(e) => setDifficulty(e.target.value)}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  >
                    <option value="Easy">Легкий</option>
                    <option value="Medium">Средний</option>
                    <option value="Hard">Сложный</option>
                  </select>
                </div>
              </div>

              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Страна
                  </label>
                  <select
                    value={countryId || ''}
                    onChange={(e) => setCountryId(e.target.value ? Number(e.target.value) : null)}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  >
                    <option value="">Не указано</option>
                    {countries.map((country) => (
                      <option key={country.id} value={country.id}>
                        {country.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Университет
                  </label>
                  <select
                    value={universityId || ''}
                    onChange={(e) => setUniversityId(e.target.value ? Number(e.target.value) : null)}
                    disabled={!countryId}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
                  >
                    <option value="">Не указано</option>
                    {universities.map((university) => (
                      <option key={university.id} value={university.id}>
                        {university.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Тип экзамена
                  </label>
                  <select
                    value={examTypeId || ''}
                    onChange={(e) => setExamTypeId(e.target.value ? Number(e.target.value) : null)}
                    disabled={!universityId}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed"
                  >
                    <option value="">{universityId ? 'Не указано' : 'Сначала выберите университет'}</option>
                    {examTypes.map((examType) => (
                      <option key={examType.id} value={examType.id}>
                        {examType.name}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
            </div>
          </Card>
        </motion.div>

        {/* Настройки экзамена */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
        >
          <Card className="mb-6">
            <h2 className="text-xl font-semibold text-gray-900 mb-4 flex items-center gap-2">
              <Settings className="w-5 h-5" />
              Настройки экзамена
            </h2>
            <div className="space-y-4">
              <div className="grid grid-cols-3 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-1">
                    <Clock className="w-4 h-4" />
                    Время (минуты)
                  </label>
                  <input
                    type="number"
                    value={timeLimit}
                    onChange={(e) => setTimeLimit(Number(e.target.value))}
                    min="1"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-1">
                    <Award className="w-4 h-4" />
                    Проходной балл (%)
                  </label>
                  <input
                    type="number"
                    value={passingScore}
                    onChange={(e) => setPassingScore(Number(e.target.value))}
                    min="0"
                    max="100"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-1">
                    <AlertCircle className="w-4 h-4" />
                    Макс. попытки
                  </label>
                  <input
                    type="number"
                    value={maxAttempts}
                    onChange={(e) => setMaxAttempts(Number(e.target.value))}
                    min="1"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  />
                </div>
              </div>

              <div className="space-y-3 pt-4 border-t border-gray-200">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={isProctored}
                    onChange={(e) => setIsProctored(e.target.checked)}
                    className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                  />
                  <span className="text-sm text-gray-700">Контролируемый экзамен</span>
                </label>

                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={shuffleQuestions}
                    onChange={(e) => setShuffleQuestions(e.target.checked)}
                    className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                  />
                  <span className="text-sm text-gray-700">Перемешивать вопросы</span>
                </label>

                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={shuffleAnswers}
                    onChange={(e) => setShuffleAnswers(e.target.checked)}
                    className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                  />
                  <span className="text-sm text-gray-700 dark:text-gray-300">Перемешивать варианты ответов</span>
                </label>

                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={strictTiming}
                    onChange={(e) => setStrictTiming(e.target.checked)}
                    className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                  />
                  <span className="text-sm text-gray-700 dark:text-gray-300">Строгий контроль времени</span>
                </label>
                <p className="text-xs text-gray-500 dark:text-gray-400 ml-6">
                  Экзамен будет автоматически завершен при истечении времени
                </p>

                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={showCorrectAnswers}
                    onChange={(e) => setShowCorrectAnswers(e.target.checked)}
                    className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                  />
                  <span className="text-sm text-gray-700 dark:text-gray-300">Показывать правильные ответы после сдачи</span>
                </label>

                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={showDetailedFeedback}
                    onChange={(e) => setShowDetailedFeedback(e.target.checked)}
                    className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                  />
                  <span className="text-sm text-gray-700">Показывать подробную обратную связь</span>
                </label>

                {!isAdmin && (
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={isPublic}
                      onChange={(e) => setIsPublic(e.target.checked)}
                      className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                    />
                    <span className="text-sm text-gray-700">Публичный доступ (доступен всем студентам, а не только вашим)</span>
                  </label>
                )}
              </div>
            </div>
          </Card>
        </motion.div>

        {/* Вопросы */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
        >
          <Card className="mb-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-900 flex items-center gap-2">
                <FileText className="w-5 h-5" />
                Вопросы ({questions.length})
              </h2>
              <div className="text-sm text-gray-600 flex items-center gap-1">
                <Award className="w-4 h-4" />
                Всего баллов: {totalPoints}
              </div>
            </div>

            <div className="space-y-6">
              {questions.map((question, qIndex) => (
                <div key={qIndex} className="border border-gray-200 rounded-lg p-4 bg-gray-50">
                  <div className="flex items-start justify-between mb-3">
                    <h3 className="font-semibold text-gray-900">Вопрос {qIndex + 1}</h3>
                    <div className="flex items-center gap-2">
                      <input
                        type="number"
                        value={question.points}
                        onChange={(e) => updateQuestion(qIndex, 'points', Number(e.target.value))}
                        min="1"
                        className="w-20 px-2 py-1 text-sm border border-gray-300 rounded focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                        placeholder="Баллы"
                      />
                      {questions.length > 1 && (
                        <button
                          onClick={() => removeQuestion(qIndex)}
                          className="p-1 text-red-600 hover:bg-red-50 rounded"
                          title="Удалить вопрос"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      )}
                    </div>
                  </div>

                  <div className="space-y-3">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Текст вопроса *
                      </label>
                      <textarea
                        value={question.text}
                        onChange={(e) => updateQuestion(qIndex, 'text', e.target.value)}
                        placeholder="Введите вопрос..."
                        rows={2}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent resize-none"
                        maxLength={1000}
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Объяснение (опционально)
                      </label>
                      <textarea
                        value={question.explanation}
                        onChange={(e) => updateQuestion(qIndex, 'explanation', e.target.value)}
                        placeholder="Объяснение правильного ответа..."
                        rows={2}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent resize-none"
                        maxLength={2000}
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Тип вопроса *
                      </label>
                      <select
                        value={question.questionType}
                        onChange={(e) => updateQuestion(qIndex, 'questionType', e.target.value)}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                      >
                        <option value="SingleChoice">Одиночный выбор</option>
                        <option value="MultipleChoice">Множественный выбор</option>
                      </select>
                      <p className="mt-1 text-xs text-gray-500">
                        {question.questionType === 'SingleChoice' 
                          ? 'Только один правильный ответ' 
                          : 'Можно выбрать несколько правильных ответов'}
                      </p>
                    </div>

                    <div>
                      <div className="flex items-center justify-between mb-2">
                        <label className="text-sm font-medium text-gray-700">
                          Варианты ответов *
                        </label>
                        <button
                          onClick={() => addAnswer(qIndex)}
                          className="text-sm text-primary-600 hover:text-primary-700 flex items-center gap-1"
                        >
                          <Plus className="w-4 h-4" />
                          Добавить вариант
                        </button>
                      </div>

                      <div className="space-y-2">
                        {question.answers.map((answer, aIndex) => (
                          <div key={aIndex} className="flex items-center gap-2">
                            <input
                              type={question.questionType === 'SingleChoice' ? 'radio' : 'checkbox'}
                              name={question.questionType === 'SingleChoice' ? `correct-${qIndex}` : undefined}
                              checked={answer.isCorrect}
                              onChange={() => toggleCorrectAnswer(qIndex, aIndex)}
                              className="w-4 h-4 text-green-600 border-gray-300 focus:ring-green-500"
                              title="Отметить как правильный ответ"
                            />
                            <input
                              type="text"
                              value={answer.text}
                              onChange={(e) => updateAnswer(qIndex, aIndex, 'text', e.target.value)}
                              placeholder={`Вариант ${aIndex + 1}`}
                              className={`flex-1 px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent ${
                                answer.isCorrect
                                  ? 'border-green-500 bg-green-50'
                                  : 'border-gray-300'
                              }`}
                              maxLength={500}
                            />
                            {question.answers.length > 2 && (
                              <button
                                onClick={() => removeAnswer(qIndex, aIndex)}
                                className="p-1 text-red-600 hover:bg-red-50 rounded"
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
                onClick={addQuestion}
                variant="secondary"
                className="w-full flex items-center justify-center gap-2 py-3"
              >
                <Plus className="w-5 h-5" />
                Добавить вопрос
              </Button>
            </div>
          </Card>
        </motion.div>

        {/* Итоговая информация */}
        <Card className="mb-6 bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800">
          <div className="flex items-start gap-3">
            <AlertCircle className="w-5 h-5 text-blue-600 dark:text-blue-400 mt-0.5" />
            <div className="text-sm text-blue-900 dark:text-blue-100">
              <p className="font-medium mb-1">Сводка экзамена:</p>
              <ul className="list-disc list-inside space-y-1 text-blue-800 dark:text-blue-200">
                <li>Вопросов: {questions.length}</li>
                <li>Всего баллов: {totalPoints}</li>
                <li>Проходной балл: {Math.ceil((totalPoints * passingScore) / 100)} из {totalPoints} ({passingScore}%)</li>
                <li>Время на выполнение: {timeLimit} минут</li>
                <li>Количество попыток: {maxAttempts}</li>
              </ul>
            </div>
          </div>
        </Card>

        {/* Кнопки действий внизу */}
        <div className="flex justify-end gap-3 pb-8">
          <Button
            onClick={() => navigate('/exams')}
            variant="secondary"
            disabled={loading}
          >
            Отмена
          </Button>
          <Button
            onClick={() => handleSubmit(false)}
            variant="secondary"
            disabled={loading}
            className="flex items-center gap-2"
          >
            <Save className="w-4 h-4" />
            {loading ? 'Сохранение...' : 'Сохранить'}
          </Button>
          {isPublished ? (
            <Button
              onClick={async () => {
                try {
                  await api.patch(`/exams/${id}/unpublish`);
                  setIsPublished(false);
                  alert('Экзамен снят с публикации');
                } catch (error) {
                  console.error('Ошибка снятия с публикации:', error);
                  alert('Не удалось снять экзамен с публикации');
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
              onClick={() => handleSubmit(true)}
              variant="primary"
              disabled={loading}
              className="flex items-center gap-2"
            >
              <Save className="w-4 h-4" />
              {loading ? 'Публикация...' : 'Опубликовать'}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
};

export default EditExamPage;
