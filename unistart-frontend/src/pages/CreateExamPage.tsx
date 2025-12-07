import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
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
} from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';

interface ExamQuestion {
  text: string;
  explanation: string;
  points: number;
  answers: ExamAnswer[];
}

interface ExamAnswer {
  text: string;
  isCorrect: boolean;
}

const CreateExamPage = () => {
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  const [loading, setLoading] = useState(false);

  // Основная информация
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [subject, setSubject] = useState('');
  const [difficulty, setDifficulty] = useState('Medium');
  
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
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [isPublic, setIsPublic] = useState(isAdmin); // Админ всегда создает публичные экзамены

  // Вопросы
  const [questions, setQuestions] = useState<ExamQuestion[]>([
    {
      text: '',
      explanation: '',
      points: 1,
      answers: [
        { text: '', isCorrect: false },
        { text: '', isCorrect: false },
      ],
    },
  ]);

  const addQuestion = () => {
    setQuestions([
      ...questions,
      {
        text: '',
        explanation: '',
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
    // Сбросить все ответы как неправильные
    updatedQuestions[questionIndex].answers.forEach((a) => (a.isCorrect = false));
    // Установить выбранный ответ как правильный
    updatedQuestions[questionIndex].answers[answerIndex].isCorrect = true;
    setQuestions(updatedQuestions);
  };

  const validateExam = (): string | null => {
    if (!title.trim()) return 'Введите название экзамена';
    if (!subject.trim()) return 'Введите предмет';
    if (questions.length === 0) return 'Добавьте хотя бы один вопрос';

    for (let i = 0; i < questions.length; i++) {
      const q = questions[i];
      if (!q.text.trim()) return `Вопрос ${i + 1}: введите текст вопроса`;
      if (q.points <= 0) return `Вопрос ${i + 1}: баллы должны быть больше 0`;
      if (q.answers.length < 2) return `Вопрос ${i + 1}: добавьте минимум 2 варианта ответа`;

      const hasCorrect = q.answers.some((a) => a.isCorrect);
      if (!hasCorrect) return `Вопрос ${i + 1}: отметьте правильный ответ`;

      for (let j = 0; j < q.answers.length; j++) {
        if (!q.answers[j].text.trim())
          return `Вопрос ${i + 1}, ответ ${j + 1}: введите текст ответа`;
      }
    }

    return null;
  };

  const handleSubmit = async (publish: boolean) => {
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
        subject,
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
        startDate: startDate || null,
        endDate: endDate || null,
        isPublished: publish,
        isPublic: isPublic,
        tagIds: [],
        questions: questions.map((q, index) => ({
          text: q.text,
          explanation: q.explanation,
          points: q.points,
          order: index,
          answers: q.answers.map((a, aIndex) => ({
            text: a.text,
            isCorrect: a.isCorrect,
            order: aIndex,
          })),
        })),
      };

      await api.post('/exams', examData);
      alert(`Экзамен ${publish ? 'опубликован' : 'сохранен как черновик'} успешно!`);
      navigate('/exams');
    } catch (error: any) {
      console.error('Ошибка создания экзамена:', error);
      alert(error.response?.data?.message || 'Не удалось создать экзамен');
    } finally {
      setLoading(false);
    }
  };

  const totalPoints = questions.reduce((sum, q) => sum + q.points, 0);

  return (
    <div className="min-h-screen bg-gray-50 py-8">
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
              Создание экзамена
            </h1>
          </div>
          <div className="flex gap-2">
            <Button
              onClick={() => handleSubmit(false)}
              variant="secondary"
              disabled={loading}
              className="flex items-center gap-2"
            >
              <Save className="w-4 h-4" />
              Сохранить черновик
            </Button>
            <Button
              onClick={() => handleSubmit(true)}
              variant="primary"
              disabled={loading}
              className="flex items-center gap-2"
            >
              <Save className="w-4 h-4" />
              Опубликовать
            </Button>
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
                    Предмет *
                  </label>
                  <input
                    type="text"
                    value={subject}
                    onChange={(e) => setSubject(e.target.value)}
                    placeholder="Математика"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    maxLength={100}
                  />
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

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Дата начала
                  </label>
                  <input
                    type="datetime-local"
                    value={startDate}
                    onChange={(e) => setStartDate(e.target.value)}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Дата окончания
                  </label>
                  <input
                    type="datetime-local"
                    value={endDate}
                    onChange={(e) => setEndDate(e.target.value)}
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
                  <span className="text-sm text-gray-700">Перемешивать варианты ответов</span>
                </label>

                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={showCorrectAnswers}
                    onChange={(e) => setShowCorrectAnswers(e.target.checked)}
                    className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                  />
                  <span className="text-sm text-gray-700">Показывать правильные ответы после сдачи</span>
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
        <Card className="mb-6 bg-blue-50 border-blue-200">
          <div className="flex items-start gap-3">
            <AlertCircle className="w-5 h-5 text-blue-600 mt-0.5" />
            <div className="text-sm text-blue-900">
              <p className="font-medium mb-1">Сводка экзамена:</p>
              <ul className="list-disc list-inside space-y-1 text-blue-800">
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
            Сохранить черновик
          </Button>
          <Button
            onClick={() => handleSubmit(true)}
            variant="primary"
            disabled={loading}
            className="flex items-center gap-2"
          >
            <Save className="w-4 h-4" />
            Опубликовать экзамен
          </Button>
        </div>
      </div>
    </div>
  );
};

export default CreateExamPage;
