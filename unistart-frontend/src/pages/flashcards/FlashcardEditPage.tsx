import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Save, Plus, Trash2, FileX } from 'lucide-react';
import { motion } from 'framer-motion';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import { flashcardService } from '../../services/flashcardService';
import { subjectService, Subject } from '../../services/subjectService';
import { useAuth } from '../../context/AuthContext';
import type { FlashcardType } from '../../types';
import api from '../../services/api';

interface FlashcardForm {
  id?: number;
  type: FlashcardType;
  question: string;
  answer: string;
  optionsJson?: string;
  matchingPairsJson?: string;
  sequenceJson?: string;
  explanation: string;
}

interface SetForm {
  title: string;
  description: string;
  subjectIds: number[];
  isPublic: boolean;
  isPublished: boolean;
}

const FlashcardEditPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  const [fetching, setFetching] = useState(true);
  const [saving, setSaving] = useState(false);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [formData, setFormData] = useState<SetForm>({
    title: '',
    description: '',
    subjectIds: [],
    isPublic: false,
    isPublished: false,
  });
  const [flashcards, setFlashcards] = useState<FlashcardForm[]>([]);

  useEffect(() => {
    loadData();
  }, [id]);

  const loadData = async () => {
    try {
      const [subjectsData, flashcardData] = await Promise.all([
        subjectService.getSubjects(),
        flashcardService.getSet(Number(id))
      ]);
      
      setSubjects(subjectsData);
      
      // Преобразуем subjects в subjectIds
      const subjectIds = flashcardData.subjects?.map((s: any) => s.id) || [];
      
      setFormData({
        title: flashcardData.title,
        description: flashcardData.description,
        subjectIds: subjectIds,
        isPublic: isAdmin ? true : (flashcardData.isPublic || false),
        isPublished: flashcardData.isPublished || false,
      });
      
      // Загружаем карточки
      if (flashcardData.flashcards) {
        setFlashcards(flashcardData.flashcards.map(card => ({
          id: card.id,
          type: card.type,
          question: card.question,
          answer: card.answer,
          optionsJson: card.optionsJson || '',
          matchingPairsJson: card.matchingPairsJson || '',
          sequenceJson: card.sequenceJson || '',
          explanation: card.explanation || '',
        })));
      }
    } catch (error) {
      console.error('Ошибка загрузки данных:', error);
      alert('Не удалось загрузить набор карточек');
      navigate('/flashcards');
    } finally {
      setFetching(false);
    }
  };

  const addFlashcard = () => {
    setFlashcards([
      ...flashcards,
      {
        type: 0, // SingleChoice
        question: '',
        answer: '',
        optionsJson: JSON.stringify(['', '', '', '']),
        matchingPairsJson: JSON.stringify([]),
        sequenceJson: JSON.stringify([]),
        explanation: '',
      },
    ]);
  };

  const removeFlashcard = (index: number) => {
    setFlashcards(flashcards.filter((_, i) => i !== index));
  };

  const updateFlashcard = (index: number, field: keyof FlashcardForm, value: any) => {
    const newFlashcards = [...flashcards];
    newFlashcards[index] = { ...newFlashcards[index], [field]: value };
    setFlashcards(newFlashcards);
  };

  const updateOption = (cardIndex: number, optionIndex: number, value: string) => {
    const card = flashcards[cardIndex];
    const options = JSON.parse(card.optionsJson || '[]');
    options[optionIndex] = value;
    updateFlashcard(cardIndex, 'optionsJson', JSON.stringify(options));
  };

  // Matching pairs helpers
  const addMatchingPair = (cardIndex: number) => {
    const card = flashcards[cardIndex];
    const pairs = card.matchingPairsJson ? JSON.parse(card.matchingPairsJson) : [];
    pairs.push({ left: '', right: '' });
    updateFlashcard(cardIndex, 'matchingPairsJson', JSON.stringify(pairs));
  };

  const removeMatchingPair = (cardIndex: number, pairIndex: number) => {
    const card = flashcards[cardIndex];
    const pairs = JSON.parse(card.matchingPairsJson || '[]');
    pairs.splice(pairIndex, 1);
    updateFlashcard(cardIndex, 'matchingPairsJson', JSON.stringify(pairs));
  };

  const updateMatchingPair = (cardIndex: number, pairIndex: number, side: 'left' | 'right', value: string) => {
    const card = flashcards[cardIndex];
    const pairs = JSON.parse(card.matchingPairsJson || '[]');
    pairs[pairIndex][side] = value;
    updateFlashcard(cardIndex, 'matchingPairsJson', JSON.stringify(pairs));
  };

  // Sequence helpers
  const addSequenceItem = (cardIndex: number) => {
    const card = flashcards[cardIndex];
    const items = card.sequenceJson ? JSON.parse(card.sequenceJson) : [];
    items.push('');
    updateFlashcard(cardIndex, 'sequenceJson', JSON.stringify(items));
  };

  const removeSequenceItem = (cardIndex: number, itemIndex: number) => {
    const card = flashcards[cardIndex];
    const items = JSON.parse(card.sequenceJson || '[]');
    items.splice(itemIndex, 1);
    updateFlashcard(cardIndex, 'sequenceJson', JSON.stringify(items));
  };

  const updateSequenceItem = (cardIndex: number, itemIndex: number, value: string) => {
    const card = flashcards[cardIndex];
    const items = JSON.parse(card.sequenceJson || '[]');
    items[itemIndex] = value;
    updateFlashcard(cardIndex, 'sequenceJson', JSON.stringify(items));
  };

  const handleSubmit = async (e: React.FormEvent, publish: boolean = false) => {
    e.preventDefault();

    if (!formData.title || !formData.title.trim()) {
      alert('Заполните название');
      return;
    }

    if (!formData.subjectIds || formData.subjectIds.length === 0) {
      alert('Выберите хотя бы один предмет');
      return;
    }

    if (flashcards.length === 0) {
      alert('Добавьте хотя бы одну карточку');
      return;
    }

    // Проверка карточек
    for (let i = 0; i < flashcards.length; i++) {
      const card = flashcards[i];
      if (!card.question) {
        alert(`Карточка ${i + 1}: введите вопрос`);
        return;
      }
      
      if (card.type === 0) { // SingleChoice
        if (!card.answer) {
          alert(`Карточка ${i + 1}: введите правильный ответ`);
          return;
        }
        const options = JSON.parse(card.optionsJson || '[]');
        if (options.length < 2) {
          alert(`Карточка ${i + 1}: добавьте минимум 2 варианта ответа`);
          return;
        }
      } else if (card.type === 1) { // FillInTheBlank
        if (!card.answer) {
          alert(`Карточка ${i + 1}: введите правильный ответ`);
          return;
        }
      } else if (card.type === 2) { // Matching
        const pairs = JSON.parse(card.matchingPairsJson || '[]');
        if (pairs.length < 2) {
          alert(`Карточка ${i + 1}: добавьте минимум 2 пары для сопоставления`);
          return;
        }
        for (let j = 0; j < pairs.length; j++) {
          if (!pairs[j].left || !pairs[j].right) {
            alert(`Карточка ${i + 1}, пара ${j + 1}: заполните оба поля`);
            return;
          }
        }
      } else if (card.type === 3) { // Sequencing
        const items = JSON.parse(card.sequenceJson || '[]');
        if (items.length < 2) {
          alert(`Карточка ${i + 1}: добавьте минимум 2 элемента для последовательности`);
          return;
        }
        for (let j = 0; j < items.length; j++) {
          if (!items[j]) {
            alert(`Карточка ${i + 1}, элемент ${j + 1}: введите текст`);
            return;
          }
        }
      }
    }

    try {
      setSaving(true);

      const setData = {
        title: formData.title,
        description: formData.description,
        subjectIds: formData.subjectIds,
        isPublic: formData.isPublic,
      };

      await flashcardService.updateSet(Number(id), setData);

      // Обновляем карточки
      const response = await api.get(`/flashcards/sets/${id}`);
      const existingCardIds = response.data.flashcards?.map((c: any) => c.id) || [];
      
      // Удаляем карточки, которых больше нет
      for (const cardId of existingCardIds) {
        if (!flashcards.find(c => c.id === cardId)) {
          await api.delete(`/flashcards/cards/${cardId}`);
        }
      }

      // Обновляем/создаем карточки
      for (let i = 0; i < flashcards.length; i++) {
        const card = flashcards[i];
        const cardData = {
          type: card.type,
          question: card.question,
          answer: card.answer,
          optionsJson: card.optionsJson || null,
          matchingPairsJson: card.matchingPairsJson || null,
          sequenceJson: card.sequenceJson || null,
          explanation: card.explanation || '',
          flashcardSetId: Number(id),
          orderIndex: i,
        };

        if (card.id) {
          await api.put(`/flashcards/cards/${card.id}`, cardData);
        } else {
          await api.post('/flashcards/cards', cardData);
        }
      }

      // Публикуем если нужно
      if (publish && !formData.isPublished) {
        await api.patch(`/flashcards/sets/${id}/publish`);
      }

      alert('Набор карточек успешно обновлен!');
      navigate('/flashcards');
    } catch (error: any) {
      console.error('Ошибка обновления:', error);
      console.error('Response data:', error.response?.data);
      const errorMessage = error.response?.data?.message || 
                          error.response?.data?.title ||
                          JSON.stringify(error.response?.data) || 
                          'Не удалось обновить набор';
      alert(errorMessage);
    } finally {
      setSaving(false);
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
              onClick={() => navigate('/flashcards')}
              variant="secondary"
              className="flex items-center gap-2"
            >
              <ArrowLeft className="w-4 h-4" />
              Назад
            </Button>
            <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
              Редактировать набор карточек
            </h1>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Основная информация */}
          <Card>
            <h2 className="text-xl font-semibold mb-4 text-gray-900 dark:text-white">
              Основная информация
            </h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Название набора *
                </label>
                <input
                  type="text"
                  value={formData.title}
                  onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                  required
                  maxLength={200}
                  className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100"
                  placeholder="Например: Математический анализ"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Описание *
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  required
                  maxLength={1000}
                  rows={4}
                  className="w-full px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100"
                  placeholder="Опишите содержание набора карточек"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Предмет *
                </label>
                <select
                  value={formData.subjectIds[0] || ''}
                  onChange={(e) => setFormData({ ...formData, subjectIds: e.target.value ? [parseInt(e.target.value)] : [] })}
                  required
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
                <div className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    id="isPublic"
                    checked={formData.isPublic}
                    onChange={(e) => setFormData({ ...formData, isPublic: e.target.checked })}
                    className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                  />
                  <label htmlFor="isPublic" className="text-sm font-medium text-gray-700 dark:text-gray-300">
                    Сделать набор публичным (доступен всем студентам)
                  </label>
                </div>
              )}
            </div>
          </Card>

          {/* Карточки */}
          <Card>
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
                Карточки ({flashcards.length})
              </h2>
              <Button
                type="button"
                onClick={addFlashcard}
                variant="primary"
                className="flex items-center gap-2"
              >
                <Plus className="w-4 h-4" />
                Добавить карточку
              </Button>
            </div>

            {flashcards.length === 0 ? (
              <div className="text-center py-12 text-gray-500 dark:text-gray-400">
                <p>Нет карточек. Нажмите "Добавить карточку" чтобы начать.</p>
              </div>
            ) : (
              <div className="space-y-6">
                {flashcards.map((card, index) => (
                  <motion.div
                    key={index}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="border border-gray-200 dark:border-gray-700 rounded-lg p-4 bg-gray-50 dark:bg-gray-800"
                  >
                    <div className="flex items-start justify-between mb-4">
                      <h3 className="font-semibold text-gray-900 dark:text-white">
                        Карточка {index + 1}
                      </h3>
                      <button
                        type="button"
                        onClick={() => removeFlashcard(index)}
                        className="text-red-600 hover:text-red-700 dark:text-red-400"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>

                    <div className="space-y-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          Тип карточки
                        </label>
                        <select
                          value={card.type}
                          onChange={(e) => updateFlashcard(index, 'type', Number(e.target.value))}
                          className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100"
                        >
                          <option value={0}>Выбор ответа</option>
                          <option value={1}>Заполнение пропусков</option>
                          <option value={2}>Соответствие</option>
                          <option value={3}>Последовательность</option>
                        </select>
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          Вопрос *
                        </label>
                        <textarea
                          value={card.question}
                          onChange={(e) => updateFlashcard(index, 'question', e.target.value)}
                          className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 resize-none"
                          rows={2}
                          placeholder="Введите вопрос..."
                          required
                        />
                      </div>

                      {/* Multiple Choice */}
                      {card.type === 0 && (
                        <div>
                          <label className="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2 block">
                            Варианты ответов *
                          </label>

                          <div className="space-y-2">
                            {JSON.parse(card.optionsJson || '[]').map((option: string, optIndex: number) => (
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
                                    updateOption(index, optIndex, newValue);
                                    // Обновляем answer только если этот вариант был выбран как правильный
                                    if (card.answer === oldValue && oldValue.trim()) {
                                      updateFlashcard(index, 'answer', newValue);
                                    }
                                  }}
                                  className={`flex-1 px-3 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 ${
                                    card.answer === option && option.trim() && card.answer.trim()
                                      ? 'border-green-500 dark:border-green-600 bg-green-50 dark:bg-green-900/20'
                                      : 'border-gray-300 dark:border-gray-600'
                                  }`}
                                  placeholder={`Вариант ${optIndex + 1}`}
                                />
                              </div>
                            ))}
                          </div>
                          <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                            Отметьте один правильный ответ
                          </p>
                        </div>
                      )}

                      {/* Fill in the Blank */}
                      {card.type === 1 && (
                        <div>
                          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                            Правильный ответ *
                          </label>
                          <input
                            type="text"
                            value={card.answer}
                            onChange={(e) => updateFlashcard(index, 'answer', e.target.value)}
                            className="w-full px-3 py-2 border border-green-500 dark:border-green-600 bg-green-50 dark:bg-green-900/20 rounded-lg focus:ring-2 focus:ring-green-500 text-gray-900 dark:text-gray-100"
                            placeholder="Слово или фраза для заполнения пропуска"
                            required
                          />
                          <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                            Используйте подчеркивание (_____) в вопросе для обозначения пропуска
                          </p>
                        </div>
                      )}

                      {/* Matching */}
                      {card.type === 2 && (
                        <div>
                          <div className="flex items-center justify-between mb-2">
                            <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                              Пары для сопоставления *
                            </label>
                            <button
                              type="button"
                              onClick={() => addMatchingPair(index)}
                              className="text-sm text-primary-600 dark:text-primary-400 hover:text-primary-700 flex items-center gap-1"
                            >
                              <Plus className="w-4 h-4" />
                              Добавить пару
                            </button>
                          </div>

                          <div className="space-y-3">
                            {JSON.parse(card.matchingPairsJson || '[]').map((pair: any, pairIndex: number) => (
                              <div key={pairIndex} className="flex items-center gap-2">
                                <div className="flex-1 grid grid-cols-2 gap-2">
                                  <input
                                    type="text"
                                    value={pair.left}
                                    onChange={(e) => updateMatchingPair(index, pairIndex, 'left', e.target.value)}
                                    className="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100"
                                    placeholder="Левая сторона"
                                  />
                                  <input
                                    type="text"
                                    value={pair.right}
                                    onChange={(e) => updateMatchingPair(index, pairIndex, 'right', e.target.value)}
                                    className="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100"
                                    placeholder="Правая сторона"
                                  />
                                </div>
                                <button
                                  type="button"
                                  onClick={() => removeMatchingPair(index, pairIndex)}
                                  className="p-1 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded"
                                  title="Удалить пару"
                                >
                                  <Trash2 className="w-4 h-4" />
                                </button>
                              </div>
                            ))}
                          </div>
                          <p className="text-xs text-gray-500 dark:text-gray-400 mt-2">
                            Студенту нужно будет сопоставить элементы из левой и правой колонок
                          </p>
                        </div>
                      )}

                      {/* Sequencing */}
                      {card.type === 3 && (
                        <div>
                          <div className="flex items-center justify-between mb-2">
                            <label className="text-sm font-medium text-gray-700 dark:text-gray-300">
                              Элементы последовательности (в правильном порядке) *
                            </label>
                            <button
                              type="button"
                              onClick={() => addSequenceItem(index)}
                              className="text-sm text-primary-600 dark:text-primary-400 hover:text-primary-700 flex items-center gap-1"
                            >
                              <Plus className="w-4 h-4" />
                              Добавить элемент
                            </button>
                          </div>

                          <div className="space-y-2">
                            {JSON.parse(card.sequenceJson || '[]').map((item: string, itemIndex: number) => (
                              <div key={itemIndex} className="flex items-center gap-2">
                                <div className="flex-shrink-0 w-8 h-8 bg-primary-100 dark:bg-primary-900/30 rounded-full flex items-center justify-center text-primary-600 dark:text-primary-400 font-semibold text-sm">
                                  {itemIndex + 1}
                                </div>
                                <input
                                  type="text"
                                  value={item}
                                  onChange={(e) => updateSequenceItem(index, itemIndex, e.target.value)}
                                  className="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100"
                                  placeholder={`Шаг ${itemIndex + 1}`}
                                />
                                <button
                                  type="button"
                                  onClick={() => removeSequenceItem(index, itemIndex)}
                                  className="p-1 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded"
                                  title="Удалить элемент"
                                >
                                  <Trash2 className="w-4 h-4" />
                                </button>
                              </div>
                            ))}
                          </div>
                          <p className="text-xs text-gray-500 dark:text-gray-400 mt-2">
                            Элементы будут перемешаны, студенту нужно расположить их в правильном порядке
                          </p>
                        </div>
                      )}

                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          Объяснение (опционально)
                        </label>
                        <textarea
                          value={card.explanation}
                          onChange={(e) => updateFlashcard(index, 'explanation', e.target.value)}
                          className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 resize-none"
                          rows={2}
                          placeholder="Дополнительное объяснение..."
                        />
                      </div>
                    </div>
                  </motion.div>
                ))}

                <Button
                  type="button"
                  onClick={addFlashcard}
                  variant="primary"
                  className="w-full flex items-center justify-center gap-2 py-3"
                >
                  <Plus className="w-5 h-5" />
                  Добавить карточку
                </Button>
              </div>
            )}
          </Card>

          {/* Кнопки действий */}
          <div className="flex justify-end gap-3">
            <Button
              type="button"
              onClick={() => navigate('/flashcards')}
              variant="secondary"
              disabled={saving}
            >
              Отмена
            </Button>
            <Button
              type="button"
              onClick={(e: any) => handleSubmit(e, false)}
              variant="secondary"
              disabled={saving}
              className="flex items-center gap-2"
            >
              <Save className="w-4 h-4" />
              {saving ? 'Сохранение...' : 'Сохранить'}
            </Button>
            {formData.isPublished ? (
              <Button
                type="button"
                onClick={async () => {
                  try {
                    await api.patch(`/flashcards/sets/${id}/unpublish`);
                    setFormData({ ...formData, isPublished: false });
                    alert('Набор снят с публикации');
                  } catch (error) {
                    console.error('Ошибка:', error);
                    alert('Не удалось снять с публикации');
                  }
                }}
                variant="secondary"
                disabled={saving}
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
                disabled={saving}
                className="flex items-center gap-2"
              >
                <Save className="w-4 h-4" />
                {saving ? 'Публикация...' : 'Опубликовать'}
              </Button>
            )}
          </div>
        </form>
      </div>
    </div>
  );
};

export default FlashcardEditPage;
