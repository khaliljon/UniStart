import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, useParams } from 'react-router-dom';
import { Plus, Trash2, Save, ArrowLeft } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface Answer {
  text: string;
  isCorrect: boolean;
}

interface Question {
  text: string;
  questionType: string;
  points: number;
  explanation: string;
  answers: Answer[];
}

interface QuizForm {
  title: string;
  subject: string;
  difficulty: string;
  timeLimit: number;
  isPublic: boolean;
  questions: Question[];
}

const EditQuizPage = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  const [quiz, setQuiz] = useState<QuizForm>({
    title: '',
    subject: '',
    difficulty: 'Medium',
    timeLimit: 30,
    isPublic: true,
    questions: [],
  });

  useEffect(() => {
    loadQuiz();
  }, [id]);

  const loadQuiz = async () => {
    try {
      setFetching(true);
      const response = await api.get(`/quizzes/${id}`);
      const quizData = response.data;
      
      setQuiz({
        title: quizData.title,
        subject: quizData.subject,
        difficulty: quizData.difficulty,
        timeLimit: quizData.timeLimit,
        isPublic: quizData.isPublic,
        questions: quizData.questions.map((q: any) => ({
          text: q.text,
          questionType: q.questionType,
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
          questionType: 'SingleChoice',
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
    const newQuestions = [...quiz.questions];
    const question = newQuestions[questionIndex];

    if (question.questionType === 'SingleChoice') {
      // Для одиночного выбора - сбросить все и установить только выбранный
      question.answers.forEach((a, i) => {
        a.isCorrect = i === answerIndex;
      });
    } else {
      // Для множественного выбора - переключить
      question.answers[answerIndex].isCorrect = !question.answers[answerIndex].isCorrect;
    }

    setQuiz({ ...quiz, questions: newQuestions });
  };

  const handleSubmit = async (e: React.FormEvent) => {
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
      if (!q.text) {
        alert(`Вопрос ${i + 1}: введите текст вопроса`);
        return;
      }
      if (q.answers.length < 2) {
        alert(`Вопрос ${i + 1}: добавьте минимум 2 варианта ответа`);
        return;
      }
      if (!q.answers.some((a) => a.isCorrect)) {
        alert(`Вопрос ${i + 1}: отметьте правильный ответ`);
        return;
      }
      for (let j = 0; j < q.answers.length; j++) {
        if (!q.answers[j].text) {
          alert(`Вопрос ${i + 1}, ответ ${j + 1}: введите текст ответа`);
          return;
        }
      }
    }

    try {
      setLoading(true);

      const quizData = {
        ...quiz,
        questions: quiz.questions.map((q, index) => ({
          ...q,
          order: index,
          answers: q.answers.map((a, aIndex) => ({
            ...a,
            order: aIndex,
          })),
        })),
      };

      await api.put(`/quizzes/${id}`, quizData);
      alert('Квиз успешно обновлен!');
      navigate('/quizzes');
    } catch (error: any) {
      console.error('Ошибка обновления квиза:', error);
      alert(error.response?.data?.message || 'Не удалось обновить квиз');
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
    <div className="min-h-screen bg-gray-50 py-8">
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
                  <input
                    type="text"
                    value={quiz.subject}
                    onChange={(e) => setQuiz({ ...quiz, subject: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    placeholder="Математика"
                    required
                  />
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

                <div className="flex items-end">
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={quiz.isPublic}
                      onChange={(e) => setQuiz({ ...quiz, isPublic: e.target.checked })}
                      className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                    />
                    <span className="text-sm font-medium text-gray-700">
                      Публичный квиз (доступен всем)
                    </span>
                  </label>
                </div>
              </div>
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
                      <button
                        type="button"
                        onClick={() => removeQuestion(qIndex)}
                        className="text-red-600 hover:text-red-700"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
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

                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-1">
                            Тип вопроса
                          </label>
                          <select
                            value={question.questionType}
                            onChange={(e) => updateQuestion(qIndex, 'questionType', e.target.value)}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          >
                            <option value="SingleChoice">Одиночный выбор</option>
                            <option value="MultipleChoice">Множественный выбор</option>
                          </select>
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-1">
                            Баллы
                          </label>
                          <input
                            type="number"
                            value={question.points}
                            onChange={(e) => updateQuestion(qIndex, 'points', Number(e.target.value))}
                            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                            min="1"
                          />
                        </div>
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
                          <button
                            type="button"
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
                                checked={answer.isCorrect}
                                onChange={() => toggleCorrectAnswer(qIndex, aIndex)}
                                className="w-4 h-4 text-primary-600 border-gray-300 focus:ring-primary-500"
                              />
                              <input
                                type="text"
                                value={answer.text}
                                onChange={(e) =>
                                  updateAnswer(qIndex, aIndex, 'text', e.target.value)
                                }
                                className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                                placeholder={`Вариант ${aIndex + 1}`}
                                required
                              />
                              {question.answers.length > 2 && (
                                <button
                                  type="button"
                                  onClick={() => removeAnswer(qIndex, aIndex)}
                                  className="text-red-600 hover:text-red-700"
                                >
                                  <Trash2 className="w-4 h-4" />
                                </button>
                              )}
                            </div>
                          ))}
                        </div>
                        <p className="text-xs text-gray-500 mt-1">
                          {question.questionType === 'SingleChoice'
                            ? 'Отметьте один правильный ответ'
                            : 'Отметьте все правильные ответы'}
                        </p>
                      </div>
                    </div>
                  </motion.div>
                ))}
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
              type="submit"
              variant="primary"
              disabled={loading}
              className="flex items-center gap-2"
            >
              <Save className="w-4 h-4" />
              {loading ? 'Сохранение...' : 'Сохранить изменения'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditQuizPage;
