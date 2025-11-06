import { useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Plus, Trash2, Save, ArrowLeft } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface Flashcard {
  question: string;
  answer: string;
  explanation: string;
}

interface FlashcardSetForm {
  title: string;
  description: string;
  subject: string;
  isPublic: boolean;
  flashcards: Flashcard[];
}

const CreateFlashcardSetPage = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [flashcardSet, setFlashcardSet] = useState<FlashcardSetForm>({
    title: '',
    description: '',
    subject: '',
    isPublic: true,
    flashcards: [],
  });

  const addFlashcard = () => {
    setFlashcardSet({
      ...flashcardSet,
      flashcards: [
        ...flashcardSet.flashcards,
        {
          question: '',
          answer: '',
          explanation: '',
        },
      ],
    });
  };

  const removeFlashcard = (index: number) => {
    const newFlashcards = flashcardSet.flashcards.filter((_, i) => i !== index);
    setFlashcardSet({ ...flashcardSet, flashcards: newFlashcards });
  };

  const updateFlashcard = (index: number, field: keyof Flashcard, value: string) => {
    const newFlashcards = [...flashcardSet.flashcards];
    newFlashcards[index] = { ...newFlashcards[index], [field]: value };
    setFlashcardSet({ ...flashcardSet, flashcards: newFlashcards });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (flashcardSet.flashcards.length === 0) {
      alert('Добавьте хотя бы одну карточку!');
      return;
    }

    for (const card of flashcardSet.flashcards) {
      if (!card.question.trim() || !card.answer.trim()) {
        alert('Все карточки должны иметь вопрос и ответ!');
        return;
      }
    }

    setLoading(true);
    try {
      // Шаг 1: Создаем набор карточек
      const setResponse = await api.post('/flashcards/sets', {
        title: flashcardSet.title,
        description: flashcardSet.description,
        subject: flashcardSet.subject,
        isPublic: flashcardSet.isPublic,
      });

      const setId = setResponse.data.id;
      console.log('FlashcardSet created with ID:', setId);

      // Шаг 2: Добавляем карточки к набору
      for (const card of flashcardSet.flashcards) {
        await api.post('/flashcards/cards', {
          flashcardSetId: setId,
          question: card.question,
          answer: card.answer,
          explanation: card.explanation || '',
        });
      }

      alert('Набор карточек успешно создан!');
      navigate('/dashboard');
    } catch (error: any) {
      console.error('Ошибка создания набора карточек:', error);
      console.error('Response data:', error.response?.data);
      alert(error.response?.data?.message || error.response?.data || 'Ошибка создания набора карточек');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-green-50 py-8 px-4">
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
            📚 Создание набора карточек
          </h1>
          <p className="text-gray-600">
            Создайте новый набор карточек для изучения
          </p>
        </motion.div>

        <form onSubmit={handleSubmit}>
          {/* Основная информация */}
          <Card className="p-6 mb-6">
            <h2 className="text-2xl font-bold text-gray-900 mb-6">
              Основная информация
            </h2>

            <div className="space-y-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Название набора *
                </label>
                <input
                  type="text"
                  required
                  value={flashcardSet.title}
                  onChange={(e) =>
                    setFlashcardSet({ ...flashcardSet, title: e.target.value })
                  }
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  placeholder="Введите название набора"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Описание
                </label>
                <textarea
                  value={flashcardSet.description}
                  onChange={(e) =>
                    setFlashcardSet({ ...flashcardSet, description: e.target.value })
                  }
                  rows={3}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                  placeholder="Краткое описание набора карточек"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Предмет
                </label>
                <select
                  value={flashcardSet.subject}
                  onChange={(e) =>
                    setFlashcardSet({ ...flashcardSet, subject: e.target.value })
                  }
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                >
                  <option value="">Выберите предмет</option>
                  <option value="Mathematics">Математика</option>
                  <option value="Physics">Физика</option>
                  <option value="Chemistry">Химия</option>
                  <option value="Biology">Биология</option>
                  <option value="History">История Казахстана</option>
                  <option value="English">Английский язык</option>
                  <option value="Kazakh">Казахский язык</option>
                  <option value="Russian">Русский язык</option>
                  <option value="Geography">География</option>
                  <option value="Computer Science">Информатика</option>
                </select>
              </div>

              <div>
                <label className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    checked={flashcardSet.isPublic}
                    onChange={(e) =>
                      setFlashcardSet({ ...flashcardSet, isPublic: e.target.checked })
                    }
                    className="w-4 h-4 text-primary-600 rounded focus:ring-primary-500"
                  />
                  <span className="text-sm text-gray-700">
                    Сделать набор публичным (доступен всем студентам)
                  </span>
                </label>
              </div>
            </div>
          </Card>

          {/* Карточки */}
          <Card className="p-6 mb-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-gray-900">
                Карточки ({flashcardSet.flashcards.length})
              </h2>
              <Button
                type="button"
                variant="primary"
                onClick={addFlashcard}
                className="flex items-center gap-2"
              >
                <Plus className="w-5 h-5" />
                Добавить карточку
              </Button>
            </div>

            {flashcardSet.flashcards.length === 0 ? (
              <div className="text-center py-12 border-2 border-dashed border-gray-300 rounded-lg">
                <p className="text-gray-600 mb-4">Карточки еще не добавлены</p>
                <Button
                  type="button"
                  variant="primary"
                  onClick={addFlashcard}
                  className="flex items-center gap-2 mx-auto"
                >
                  <Plus className="w-5 h-5" />
                  Добавить первую карточку
                </Button>
              </div>
            ) : (
              <div className="space-y-6">
                {flashcardSet.flashcards.map((card, index) => (
                  <motion.div
                    key={index}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: index * 0.05 }}
                    className="border border-gray-300 rounded-lg p-6 bg-white"
                  >
                    <div className="flex items-start justify-between mb-4">
                      <h3 className="text-lg font-semibold text-gray-900">
                        Карточка {index + 1}
                      </h3>
                      <Button
                        type="button"
                        variant="danger"
                        size="sm"
                        onClick={() => removeFlashcard(index)}
                      >
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </div>

                    <div className="space-y-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Вопрос (передняя сторона) *
                        </label>
                        <textarea
                          required
                          value={card.question}
                          onChange={(e) =>
                            updateFlashcard(index, 'question', e.target.value)
                          }
                          rows={2}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          placeholder="Введите вопрос или термин"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Ответ (задняя сторона) *
                        </label>
                        <textarea
                          required
                          value={card.answer}
                          onChange={(e) =>
                            updateFlashcard(index, 'answer', e.target.value)
                          }
                          rows={3}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          placeholder="Введите ответ или определение"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Дополнительное объяснение (опционально)
                        </label>
                        <textarea
                          value={card.explanation}
                          onChange={(e) =>
                            updateFlashcard(index, 'explanation', e.target.value)
                          }
                          rows={2}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                          placeholder="Дополнительные пояснения или примеры"
                        />
                      </div>
                    </div>
                  </motion.div>
                ))}
              </div>
            )}
          </Card>

          {/* Кнопки действий */}
          <div className="flex items-center justify-end gap-4">
            <Button
              type="button"
              variant="secondary"
              onClick={() => navigate('/dashboard')}
            >
              Отмена
            </Button>
            <Button
              type="submit"
              variant="primary"
              disabled={loading || flashcardSet.flashcards.length === 0}
              className="flex items-center gap-2"
            >
              <Save className="w-5 h-5" />
              {loading ? 'Сохранение...' : 'Создать набор'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CreateFlashcardSetPage;
