import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Plus, Trash2, Save, ArrowLeft } from 'lucide-react';
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
  subject: string;
  difficulty: string;
  timeLimit: number;
  isPublic: boolean;
  isPublished: boolean;
  questions: Question[];
}

const CreateQuizPage = () => {
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  const [loading, setLoading] = useState(false);
  const [subjects, setSubjects] = useState<any[]>([]);
  const [quiz, setQuiz] = useState<QuizForm>({
    title: '',
    subject: '',
    difficulty: 'Medium',
    timeLimit: 30,
    isPublic: isAdmin, // Админ всегда создает публичные квизы
    isPublished: false,
    questions: [],
  });

  useEffect(() => {
    loadSubjects();
  }, []);

  const loadSubjects = async () => {
    try {
      const response = await api.get('/subjects');
      setSubjects(response.data);
    } catch (error) {
      console.error('Ошибка загрузки предметов:', error);
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

    for (const question of quiz.questions) {
      if (!question.text.trim()) {
        alert('Все вопросы должны иметь текст!');
        return;
      }
      if (question.answers.length < 2) {
        alert('У каждого вопроса должно быть минимум 2 ответа!');
        return;
      }
      if (!question.answers.some(a => a.isCorrect)) {
        alert('У каждого вопроса должен быть хотя бы один правильный ответ!');
        return;
      }
    }

    setLoading(true);
    try {
      // Шаг 1: Создаем квиз
      const quizResponse = await api.post('/quizzes', {
        title: quiz.title,
        subject: quiz.subject,
        difficulty: quiz.difficulty,
        timeLimit: quiz.timeLimit,
        description: quiz.title,
        isPublic: quiz.isPublic,
        isPublished: false, // Всегда создаем как черновик, потом публикуем отдельно
      });

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
      console.error('Response data:', error.response?.data);
      alert(error.response?.data?.message || error.response?.data || 'Ошибка создания квиза');
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
            onClick={() => navigate('/dashboard')}
            className="mb-4 flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Назад к панели
          </Button>

          <h1 className="text-4xl font-bold text-gray-900 mb-2">
            📝 Создание квиза
          </h1>
          <p className="text-gray-600">
            Создайте новый тест для своих студентов
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
                  onChange={(e) => setQuiz({ ...quiz, subject: e.target.value })}
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

            {!isAdmin && (
              <div className="mt-6 space-y-3">
                <label className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    checked={quiz.isPublic}
                    onChange={(e) => setQuiz({ ...quiz, isPublic: e.target.checked })}
                    className="w-4 h-4 text-primary-600 rounded focus:ring-primary-500"
                  />
                  <span className="text-sm text-gray-700">
                    Публичный доступ (доступен всем студентам, а не только вашим)
                  </span>
                </label>
              </div>
            )}
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
                      <Button
                        type="button"
                        variant="danger"
                        size="sm"
                        onClick={() => removeQuestion(qIndex)}
                      >
                        <Trash2 className="w-4 h-4" />
                      </Button>
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

                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">
                            Баллы
                          </label>
                          <input
                            type="number"
                            min="1"
                            value={question.points}
                            onChange={(e) =>
                              updateQuestion(qIndex, 'points', parseInt(e.target.value))
                            }
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          />
                        </div>
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
                          <Button
                            type="button"
                            variant="secondary"
                            size="sm"
                            onClick={() => addAnswer(qIndex)}
                          >
                            <Plus className="w-4 h-4" />
                          </Button>
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
                                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                                placeholder={`Вариант ${aIndex + 1}`}
                              />
                              {question.answers.length > 2 && (
                                <Button
                                  type="button"
                                  variant="danger"
                                  size="sm"
                                  onClick={() => removeAnswer(qIndex, aIndex)}
                                >
                                  <Trash2 className="w-4 h-4" />
                                </Button>
                              )}
                            </div>
                          ))}
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
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
