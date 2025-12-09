import { useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Plus, Trash2, Save, ArrowLeft } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';
import { FlashcardType } from '../types';

interface Flashcard {
  question: string;
  answer: string;
  explanation: string;
  type: FlashcardType;
  options?: string[];
  matchingPairs?: Array<{ term: string; definition: string }>;
  sequence?: string[];
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
          type: FlashcardType.MultipleChoice,
          options: ['', '', '', ''],
        },
      ],
    });
  };

  const removeFlashcard = (index: number) => {
    const newFlashcards = flashcardSet.flashcards.filter((_, i) => i !== index);
    setFlashcardSet({ ...flashcardSet, flashcards: newFlashcards });
  };

  const updateFlashcard = (index: number, field: keyof Flashcard, value: any) => {
    const newFlashcards = [...flashcardSet.flashcards];
    newFlashcards[index] = { ...newFlashcards[index], [field]: value };
    setFlashcardSet({ ...flashcardSet, flashcards: newFlashcards });
  };

  const updateFlashcardOption = (cardIndex: number, optionIndex: number, value: string) => {
    const newFlashcards = [...flashcardSet.flashcards];
    const options = [...(newFlashcards[cardIndex].options || [])];
    options[optionIndex] = value;
    newFlashcards[cardIndex] = { ...newFlashcards[cardIndex], options };
    setFlashcardSet({ ...flashcardSet, flashcards: newFlashcards });
  };

  const updateMatchingPair = (cardIndex: number, pairIndex: number, field: 'term' | 'definition', value: string) => {
    const newFlashcards = [...flashcardSet.flashcards];
    const pairs = [...(newFlashcards[cardIndex].matchingPairs || [])];
    pairs[pairIndex] = { ...pairs[pairIndex], [field]: value };
    newFlashcards[cardIndex] = { ...newFlashcards[cardIndex], matchingPairs: pairs };
    setFlashcardSet({ ...flashcardSet, flashcards: newFlashcards });
  };

  const updateSequenceItem = (cardIndex: number, itemIndex: number, value: string) => {
    const newFlashcards = [...flashcardSet.flashcards];
    const sequence = [...(newFlashcards[cardIndex].sequence || [])];
    sequence[itemIndex] = value;
    newFlashcards[cardIndex] = { ...newFlashcards[cardIndex], sequence };
    setFlashcardSet({ ...flashcardSet, flashcards: newFlashcards });
  };

  const addOption = (cardIndex: number) => {
    const newFlashcards = [...flashcardSet.flashcards];
    const options = [...(newFlashcards[cardIndex].options || []), ''];
    newFlashcards[cardIndex] = { ...newFlashcards[cardIndex], options };
    setFlashcardSet({ ...flashcardSet, flashcards: newFlashcards });
  };

  const addMatchingPair = (cardIndex: number) => {
    const newFlashcards = [...flashcardSet.flashcards];
    const pairs = [...(newFlashcards[cardIndex].matchingPairs || []), { term: '', definition: '' }];
    newFlashcards[cardIndex] = { ...newFlashcards[cardIndex], matchingPairs: pairs };
    setFlashcardSet({ ...flashcardSet, flashcards: newFlashcards });
  };

  const addSequenceItem = (cardIndex: number) => {
    const newFlashcards = [...flashcardSet.flashcards];
    const sequence = [...(newFlashcards[cardIndex].sequence || []), ''];
    newFlashcards[cardIndex] = { ...newFlashcards[cardIndex], sequence };
    setFlashcardSet({ ...flashcardSet, flashcards: newFlashcards });
  };

  const handleTypeChange = (cardIndex: number, newType: FlashcardType) => {
    const newFlashcards = [...flashcardSet.flashcards];
    const card = { ...newFlashcards[cardIndex], type: newType };
    
    // Initialize type-specific fields
    if (newType === FlashcardType.MultipleChoice) {
      card.options = ['', '', '', ''];
      card.matchingPairs = undefined;
      card.sequence = undefined;
    } else if (newType === FlashcardType.Matching) {
      card.matchingPairs = [{ term: '', definition: '' }, { term: '', definition: '' }];
      card.options = undefined;
      card.sequence = undefined;
    } else if (newType === FlashcardType.Sequencing) {
      card.sequence = ['', '', ''];
      card.options = undefined;
      card.matchingPairs = undefined;
    } else {
      card.options = undefined;
      card.matchingPairs = undefined;
      card.sequence = undefined;
    }
    
    newFlashcards[cardIndex] = card;
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
        const cardData: any = {
          flashcardSetId: setId,
          question: card.question,
          answer: card.answer,
          explanation: card.explanation || '',
          type: card.type,
        };

        // Add type-specific data
        if (card.type === FlashcardType.MultipleChoice && card.options) {
          cardData.optionsJson = JSON.stringify(card.options.filter(o => o.trim()));
        } else if (card.type === FlashcardType.Matching && card.matchingPairs) {
          cardData.matchingPairsJson = JSON.stringify(card.matchingPairs.filter(p => p.term.trim() && p.definition.trim()));
        } else if (card.type === FlashcardType.Sequencing && card.sequence) {
          cardData.sequenceJson = JSON.stringify(card.sequence.filter(s => s.trim()));
        }

        await api.post('/flashcards/cards', cardData);
      }

      alert('Набор карточек успешно создан!');
      navigate('/dashboard');
    } catch (error: any) {
      console.error('Ошибка создания набора карточек:', error);
      console.error('Response:', error.response);
      console.error('Response data:', error.response?.data);
      console.error('Response status:', error.response?.status);
      
      let errorMessage = 'Ошибка создания набора карточек';
      
      if (error.response?.status === 401) {
        errorMessage = 'Ошибка авторизации. Пожалуйста, войдите в систему.';
      } else if (error.response?.status === 403) {
        errorMessage = 'У вас нет прав для создания карточек.';
      } else if (error.response?.data?.message) {
        errorMessage = error.response.data.message;
      } else if (typeof error.response?.data === 'string') {
        errorMessage = error.response.data;
      } else if (error.response?.data?.errors) {
        errorMessage = Object.values(error.response.data.errors).flat().join(', ');
      }
      
      alert(errorMessage);
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
                          Тип карточки *
                        </label>
                        <select
                          value={card.type}
                          onChange={(e) => handleTypeChange(index, parseInt(e.target.value))}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                        >
                          <option value={FlashcardType.MultipleChoice}>Выбор ответа</option>
                          <option value={FlashcardType.FillInTheBlank}>Заполнить пропуск</option>
                          <option value={FlashcardType.Matching}>Сопоставление</option>
                          <option value={FlashcardType.Sequencing}>Упорядочивание</option>
                        </select>
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Вопрос *
                        </label>
                        <textarea
                          required
                          value={card.question}
                          onChange={(e) =>
                            updateFlashcard(index, 'question', e.target.value)
                          }
                          rows={2}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                          placeholder="Введите вопрос"
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Правильный ответ *
                        </label>
                        <textarea
                          required
                          value={card.answer}
                          onChange={(e) =>
                            updateFlashcard(index, 'answer', e.target.value)
                          }
                          rows={2}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                          placeholder={card.type === FlashcardType.MultipleChoice ? "Введите правильный ответ (должен совпадать с одним из вариантов)" : "Введите правильный ответ"}
                        />
                      </div>

                      {/* Multiple Choice Options */}
                      {card.type === FlashcardType.MultipleChoice && (
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">
                            Варианты ответов *
                          </label>
                          <div className="space-y-2">
                            {card.options?.map((option, optIndex) => (
                              <input
                                key={optIndex}
                                type="text"
                                value={option}
                                onChange={(e) => updateFlashcardOption(index, optIndex, e.target.value)}
                                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                                placeholder={`Вариант ${optIndex + 1}`}
                                required
                              />
                            ))}
                            <Button
                              type="button"
                              variant="secondary"
                              size="sm"
                              onClick={() => addOption(index)}
                              className="flex items-center gap-1"
                            >
                              <Plus className="w-4 h-4" />
                              Добавить вариант
                            </Button>
                          </div>
                        </div>
                      )}

                      {/* Matching Pairs */}
                      {card.type === FlashcardType.Matching && (
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">
                            Пары для сопоставления *
                          </label>
                          <div className="space-y-3">
                            {card.matchingPairs?.map((pair, pairIndex) => (
                              <div key={pairIndex} className="grid grid-cols-2 gap-2">
                                <input
                                  type="text"
                                  value={pair.term}
                                  onChange={(e) => updateMatchingPair(index, pairIndex, 'term', e.target.value)}
                                  className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                                  placeholder="Термин"
                                  required
                                />
                                <input
                                  type="text"
                                  value={pair.definition}
                                  onChange={(e) => updateMatchingPair(index, pairIndex, 'definition', e.target.value)}
                                  className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                                  placeholder="Определение"
                                  required
                                />
                              </div>
                            ))}
                            <Button
                              type="button"
                              variant="secondary"
                              size="sm"
                              onClick={() => addMatchingPair(index)}
                              className="flex items-center gap-1"
                            >
                              <Plus className="w-4 h-4" />
                              Добавить пару
                            </Button>
                          </div>
                        </div>
                      )}

                      {/* Sequence Items */}
                      {card.type === FlashcardType.Sequencing && (
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">
                            Элементы для упорядочивания * (в правильном порядке)
                          </label>
                          <div className="space-y-2">
                            {card.sequence?.map((item, itemIndex) => (
                              <div key={itemIndex} className="flex items-center gap-2">
                                <span className="text-sm text-gray-600 w-6">{itemIndex + 1}.</span>
                                <input
                                  type="text"
                                  value={item}
                                  onChange={(e) => updateSequenceItem(index, itemIndex, e.target.value)}
                                  className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                                  placeholder={`Шаг ${itemIndex + 1}`}
                                  required
                                />
                              </div>
                            ))}
                            <Button
                              type="button"
                              variant="secondary"
                              size="sm"
                              onClick={() => addSequenceItem(index)}
                              className="flex items-center gap-1"
                            >
                              <Plus className="w-4 h-4" />
                              Добавить элемент
                            </Button>
                          </div>
                        </div>
                      )}

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
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
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
