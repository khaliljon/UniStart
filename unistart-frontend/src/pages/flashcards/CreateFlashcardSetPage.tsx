import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Plus, Trash2, Save, ArrowLeft } from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import api from '../../services/api';
import { FlashcardType } from '../../types';
import { subjectService, Subject } from '../../services/subjectService';
import { useAuth } from '../../context/AuthContext';

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
  subjectIds: number[];
  isPublic: boolean;
  flashcards: Flashcard[];
}

const CreateFlashcardSetPage = () => {
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  const [loading, setLoading] = useState(false);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [flashcardSet, setFlashcardSet] = useState<FlashcardSetForm>({
    title: '',
    description: '',
    subjectIds: [],
    isPublic: isAdmin ? true : false,
    flashcards: [],
  });

  useEffect(() => {
    loadSubjects();
  }, []);

  const loadSubjects = async () => {
    try {
      const data = await subjectService.getSubjects();
      setSubjects(data);
    } catch (error) {
      console.error('Ошибка загрузки предметов:', error);
    }
  };

  const addFlashcard = () => {
    setFlashcardSet({
      ...flashcardSet,
      flashcards: [
        ...flashcardSet.flashcards,
        {
          question: '',
          answer: '',
          explanation: '',
          type: FlashcardType.SingleChoice,
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
    if (newType === FlashcardType.SingleChoice) {
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

  const handleSubmit = async (e: React.FormEvent, publish: boolean = false) => {
    e.preventDefault();

    if (!flashcardSet.title.trim()) {
      alert('Введите название набора!');
      return;
    }

    if (!flashcardSet.subjectIds || flashcardSet.subjectIds.length === 0) {
      alert('Выберите хотя бы один предмет!');
      return;
    }

    if (flashcardSet.flashcards.length === 0) {
      alert('Добавьте хотя бы одну карточку!');
      return;
    }

    // Валидация карточек
    for (let i = 0; i < flashcardSet.flashcards.length; i++) {
      const card = flashcardSet.flashcards[i];
      
      // Проверка вопроса
      if (!card.question.trim()) {
        alert(`Карточка ${i + 1}: заполните вопрос!`);
        return;
      }
      
      // Валидация в зависимости от типа
      if (card.type === FlashcardType.SingleChoice) {
        // Проверка правильного ответа
        if (!card.answer || !card.answer.trim()) {
          alert(`Карточка ${i + 1}: отметьте правильный ответ!`);
          return;
        }
        
        // Проверка вариантов ответов
        const validOptions = card.options?.filter(o => o.trim()) || [];
        if (validOptions.length < 2) {
          alert(`Карточка ${i + 1}: добавьте минимум 2 варианта ответа!`);
          return;
        }
        
        // Проверка дубликатов вариантов
        const uniqueOptions = new Set(validOptions.map(o => o.trim().toLowerCase()));
        if (uniqueOptions.size !== validOptions.length) {
          alert(`Карточка ${i + 1}: варианты ответов не должны повторяться!`);
          return;
        }
        
        // Проверка, что правильный ответ есть среди вариантов
        if (!validOptions.includes(card.answer)) {
          alert(`Карточка ${i + 1}: правильный ответ должен быть одним из вариантов!`);
          return;
        }
      } else if (card.type === FlashcardType.FillInTheBlank) {
        if (!card.answer || !card.answer.trim()) {
          alert(`Карточка ${i + 1}: введите правильный ответ!`);
          return;
        }
      } else if (card.type === FlashcardType.Matching) {
        const validPairs = card.matchingPairs?.filter(p => p.term.trim() && p.definition.trim()) || [];
        if (validPairs.length < 2) {
          alert(`Карточка ${i + 1}: добавьте минимум 2 пары для сопоставления!`);
          return;
        }
        
        // Проверка дубликатов терминов и определений
        const terms = validPairs.map(p => p.term.trim().toLowerCase());
        const definitions = validPairs.map(p => p.definition.trim().toLowerCase());
        if (new Set(terms).size !== terms.length) {
          alert(`Карточка ${i + 1}: термины не должны повторяться!`);
          return;
        }
        if (new Set(definitions).size !== definitions.length) {
          alert(`Карточка ${i + 1}: определения не должны повторяться!`);
          return;
        }
      } else if (card.type === FlashcardType.Sequencing) {
        const validSequence = card.sequence?.filter(s => s.trim()) || [];
        if (validSequence.length < 2) {
          alert(`Карточка ${i + 1}: добавьте минимум 2 элемента в последовательность!`);
          return;
        }
        
        // Проверка дубликатов в последовательности
        const uniqueItems = new Set(validSequence.map(s => s.trim().toLowerCase()));
        if (uniqueItems.size !== validSequence.length) {
          alert(`Карточка ${i + 1}: элементы последовательности не должны повторяться!`);
          return;
        }
      }
    }

    setLoading(true);
    try {
      // Шаг 1: Создаем набор карточек
      const setResponse = await api.post('/flashcards/sets', {
        title: flashcardSet.title,
        description: flashcardSet.description,
        subjectIds: flashcardSet.subjectIds,
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
        if (card.type === FlashcardType.SingleChoice && card.options) {
          cardData.optionsJson = JSON.stringify(card.options.filter(o => o.trim()));
        } else if (card.type === FlashcardType.Matching && card.matchingPairs) {
          cardData.matchingPairsJson = JSON.stringify(card.matchingPairs.filter(p => p.term.trim() && p.definition.trim()));
        } else if (card.type === FlashcardType.Sequencing && card.sequence) {
          cardData.sequenceJson = JSON.stringify(card.sequence.filter(s => s.trim()));
        }

        await api.post('/flashcards/cards', cardData);
      }

      // Шаг 3: Публикуем если нужно
      if (publish) {
        await api.patch(`/flashcards/sets/${setId}/publish`);
      }

      alert(`Набор карточек успешно ${publish ? 'создан и опубликован' : 'сохранен как черновик'}!`);
      navigate('/flashcards');
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
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-green-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
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

          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-2">
            📚 Создание набора карточек
          </h1>
          <p className="text-gray-600 dark:text-gray-400">
            Создайте новый набор карточек для изучения
          </p>
        </motion.div>

        <form onSubmit={handleSubmit}>
          {/* Основная информация */}
          <Card className="p-6 mb-6">
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">
              Основная информация
            </h2>

            <div className="space-y-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Название набора *
                </label>
                <input
                  type="text"
                  required
                  value={flashcardSet.title}
                  onChange={(e) =>
                    setFlashcardSet({ ...flashcardSet, title: e.target.value })
                  }
                  className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100"
                  placeholder="Введите название набора"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Описание
                </label>
                <textarea
                  value={flashcardSet.description}
                  onChange={(e) =>
                    setFlashcardSet({ ...flashcardSet, description: e.target.value })
                  }
                  rows={3}
                  className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100"
                  placeholder="Краткое описание набора карточек"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Предмет
                </label>
                <select
                  value={flashcardSet.subjectIds[0] || ''}
                  onChange={(e) =>
                    setFlashcardSet({ ...flashcardSet, subjectIds: e.target.value ? [parseInt(e.target.value)] : [] })
                  }
                  className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100"
                >
                  <option value="">Выберите предмет</option>
                  {subjects.map((subject) => (
                    <option key={subject.id} value={subject.id}>
                      {subject.name}
                    </option>
                  ))}
                </select>
              </div>

              {!isAdmin && (
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
                    <span className="text-sm text-gray-700 dark:text-gray-300">
                      Сделать набор публичным (доступен всем студентам)
                    </span>
                  </label>
                </div>
              )}
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
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                          Тип карточки *
                        </label>
                        <select
                          value={card.type}
                          onChange={(e) => handleTypeChange(index, parseInt(e.target.value))}
                          className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:border-gray-600 dark:text-white"
                        >
                          <option value={FlashcardType.SingleChoice}>Выбор ответа</option>
                          <option value={FlashcardType.FillInTheBlank}>Заполнить пропуск</option>
                          <option value={FlashcardType.Matching}>Сопоставление</option>
                          <option value={FlashcardType.Sequencing}>Упорядочивание</option>
                        </select>
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                          Вопрос *
                        </label>
                        <textarea
                          required
                          value={card.question}
                          onChange={(e) =>
                            updateFlashcard(index, 'question', e.target.value)
                          }
                          rows={2}
                          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
                          placeholder="Введите вопрос"
                        />
                      </div>

                      {/* Правильный ответ только для Fill in the Blank */}
                      {card.type === FlashcardType.FillInTheBlank && (
                        <div>
                          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                            Правильный ответ *
                          </label>
                          <textarea
                            required
                            value={card.answer}
                            onChange={(e) =>
                              updateFlashcard(index, 'answer', e.target.value)
                            }
                            rows={2}
                            className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                            placeholder="Введите правильный ответ"
                          />
                        </div>
                      )}

                      {/* Multiple Choice Options */}
                      {card.type === FlashcardType.SingleChoice && (
                        <div>
                          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                            Варианты ответов *
                          </label>
                          <div className="space-y-2">
                            {card.options?.map((option, optIndex) => (
                              <div key={optIndex} className="flex items-center gap-2">
                                <input
                                  type="radio"
                                  name={`correct-answer-${index}`}
                                  checked={card.answer === option}
                                  onChange={() => updateFlashcard(index, 'answer', option)}
                                  className="w-4 h-4 text-green-600 border-gray-300 focus:ring-green-500"
                                  title="Отметить как правильный ответ"
                                />
                                <input
                                  type="text"
                                  value={option}
                                  onChange={(e) => {
                                    const newValue = e.target.value;
                                    const oldValue = option;
                                    updateFlashcardOption(index, optIndex, newValue);
                                    // Обновляем answer только если этот вариант был выбран как правильный
                                    if (card.answer === oldValue && oldValue.trim()) {
                                      updateFlashcard(index, 'answer', newValue);
                                    }
                                  }}
                                  className={`flex-1 px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent ${
                                    card.answer === option && option.trim() && card.answer.trim()
                                      ? 'border-green-500 bg-green-50 dark:bg-green-900/20'
                                      : 'border-gray-300 dark:border-gray-600'
                                  } bg-white dark:bg-gray-700 text-gray-900 dark:text-white`}
                                  placeholder={`Вариант ${optIndex + 1}`}
                                  required
                                />
                              </div>
                            ))}
                          </div>
                          <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                            Отметьте один правильный ответ
                          </p>
                        </div>
                      )}

                      {/* Matching Pairs */}
                      {card.type === FlashcardType.Matching && (
                        <div>
                          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                            Пары для сопоставления *
                          </label>
                          <div className="space-y-3">
                            {card.matchingPairs?.map((pair, pairIndex) => (
                              <div key={pairIndex} className="grid grid-cols-2 gap-2">
                                <input
                                  type="text"
                                  value={pair.term}
                                  onChange={(e) => updateMatchingPair(index, pairIndex, 'term', e.target.value)}
                                  className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                                  placeholder="Термин"
                                  required
                                />
                                <input
                                  type="text"
                                  value={pair.definition}
                                  onChange={(e) => updateMatchingPair(index, pairIndex, 'definition', e.target.value)}
                                  className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
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
                          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                            Элементы для упорядочивания * (в правильном порядке)
                          </label>
                          <div className="space-y-2">
                            {card.sequence?.map((item, itemIndex) => (
                              <div key={itemIndex} className="flex items-center gap-2">
                                <span className="text-sm text-gray-600 dark:text-gray-400 w-6">{itemIndex + 1}.</span>
                                <input
                                  type="text"
                                  value={item}
                                  onChange={(e) => updateSequenceItem(index, itemIndex, e.target.value)}
                                  className="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
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
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                          Объяснение (опционально)
                        </label>
                        <textarea
                          value={card.explanation}
                          onChange={(e) =>
                            updateFlashcard(index, 'explanation', e.target.value)
                          }
                          rows={2}
                          className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                          placeholder="Дополнительные пояснения или примеры"
                        />
                      </div>
                    </div>
                  </motion.div>
                ))}

                <Button
                  type="button"
                  variant="primary"
                  onClick={addFlashcard}
                  className="w-full flex items-center justify-center gap-2 py-3"
                >
                  <Plus className="w-5 h-5" />
                  Добавить карточку
                </Button>
              </div>
            )}
          </Card>

          {/* Кнопки действий */}
          <div className="flex items-center justify-end gap-4">
            <Button
              type="button"
              variant="secondary"
              onClick={() => navigate('/flashcards')}
            >
              Отмена
            </Button>
            <Button
              type="button"
              variant="secondary"
              onClick={(e: any) => handleSubmit(e, false)}
              disabled={loading || flashcardSet.flashcards.length === 0}
              className="flex items-center gap-2"
            >
              <Save className="w-5 h-5" />
              {loading ? 'Сохранение...' : 'Сохранить как черновик'}
            </Button>
            <Button
              type="button"
              variant="primary"
              onClick={(e: any) => handleSubmit(e, true)}
              disabled={loading || flashcardSet.flashcards.length === 0}
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

export default CreateFlashcardSetPage;
