import React, { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { Save, AlertCircle, CheckCircle, Settings } from 'lucide-react';
import preferencesService, { UserPreferencesDto, UserPreferencesResponseDto } from '../../services/preferencesService';
import { subjectService, Subject } from '../../services/subjectService';
import { referenceService, Country, ExamType, City } from '../../services/referenceService';

const PreferencesPage: React.FC = () => {
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
    const [subjects, setSubjects] = useState<Subject[]>([]);
    const [countries, setCountries] = useState<Country[]>([]);
    const [examTypes, setExamTypes] = useState<ExamType[]>([]);
    const [cities, setCities] = useState<City[]>([]);
    const [preferences, setPreferences] = useState<UserPreferencesResponseDto | null>(null);

    const [formData, setFormData] = useState<UserPreferencesDto>({
        learningGoal: 'SelfStudy',
        targetExamType: '',
        targetUniversityId: undefined,
        careerGoal: '',
        preferredCountry: '',
        preferredCity: '',
        willingToRelocate: false,
        maxBudgetPerYear: undefined,
        interestedInScholarships: true,
        preferredLanguages: ['Russian'],
        englishLevel: '',
        interestedSubjectIds: [],
        strongSubjectIds: [],
        weakSubjectIds: [],
        prefersFlashcards: true,
        prefersQuizzes: true,
        prefersExams: false,
        preferredDifficulty: 2,
        dailyStudyTimeMinutes: 30,
        preferredStudyTime: '',
        studyDays: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
        motivationLevel: 3,
        needsReminders: true,
    });

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            setLoading(true);
            const [prefs, subjs, countriesData, examTypesData] = await Promise.all([
                preferencesService.getMyPreferences(),
                subjectService.getSubjects(),
                referenceService.getCountries(),
                referenceService.getExamTypes()
            ]);

            setSubjects(subjs);
            setCountries(countriesData);
            setExamTypes(examTypesData);

            setPreferences(prefs);

            // Загружаем города для выбранной страны
            if (prefs.preferredCountry && countriesData.length > 0) {
                const country = countriesData.find(c => c.name === prefs.preferredCountry);
                if (country) {
                    const citiesData = await referenceService.getCities(country.id);
                    setCities(citiesData);
                }
            }

            // Заполняем форму
            setFormData({
                learningGoal: prefs.learningGoal,
                targetExamType: prefs.targetExamType || '',
                targetUniversityId: prefs.targetUniversityId,
                careerGoal: prefs.careerGoal || '',
                preferredCountry: prefs.preferredCountry || '',
                preferredCity: prefs.preferredCity || '',
                willingToRelocate: prefs.willingToRelocate,
                maxBudgetPerYear: prefs.maxBudgetPerYear,
                interestedInScholarships: prefs.interestedInScholarships,
                preferredLanguages: prefs.preferredLanguages,
                englishLevel: prefs.englishLevel || '',
                interestedSubjectIds: prefs.interestedSubjects.map(s => s.id),
                strongSubjectIds: prefs.strongSubjects.map(s => s.id),
                weakSubjectIds: prefs.weakSubjects.map(s => s.id),
                prefersFlashcards: prefs.prefersFlashcards,
                prefersQuizzes: prefs.prefersQuizzes,
                prefersExams: prefs.prefersExams,
                preferredDifficulty: prefs.preferredDifficulty,
                dailyStudyTimeMinutes: prefs.dailyStudyTimeMinutes,
                preferredStudyTime: prefs.preferredStudyTime || '',
                studyDays: prefs.studyDays,
                motivationLevel: prefs.motivationLevel,
                needsReminders: prefs.needsReminders,
            });
        } catch (error) {
            console.error('Failed to load preferences:', error);
            setMessage({ type: 'error', text: 'Не удалось загрузить настройки' });
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        try {
            setSaving(true);
            setMessage(null);

            if (formData.interestedSubjectIds.length === 0) {
                setMessage({ type: 'error', text: 'Выберите хотя бы один интересующий предмет' });
                return;
            }

            await preferencesService.createOrUpdatePreferences(formData);
            setMessage({ type: 'success', text: 'Настройки успешно сохранены!' });

            // Перезагружаем данные
            await loadData();
        } catch (error) {
            console.error('Failed to save preferences:', error);
            setMessage({ type: 'error', text: 'Ошибка при сохранении настроек' });
        } finally {
            setSaving(false);
        }
    };

    const updateFormData = (field: keyof UserPreferencesDto, value: any) => {
        setFormData(prev => ({ ...prev, [field]: value }));
    };

    const toggleArrayValue = (field: keyof UserPreferencesDto, value: any) => {
        const currentArray = formData[field] as any[];
        if (currentArray.includes(value)) {
            updateFormData(field, currentArray.filter(v => v !== value));
        } else {
            updateFormData(field, [...currentArray, value]);
        }
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gray-50 flex items-center justify-center">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto mb-4"></div>
                    <p className="text-gray-600">Загрузка настроек...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-50 py-8 px-4">
            <div className="max-w-5xl mx-auto">
                {/* Header */}
                <div className="mb-8">
                    <div className="flex items-center gap-3 mb-2">
                        <Settings className="w-8 h-8 text-indigo-600" />
                        <h1 className="text-3xl font-bold text-gray-900">Настройки предпочтений</h1>
                    </div>
                    <p className="text-gray-600">
                        Управляйте своими предпочтениями для персонализированных рекомендаций
                    </p>
                </div>

                {/* Message */}
                {message && (
                    <motion.div
                        initial={{ opacity: 0, y: -10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className={`mb-6 p-4 rounded-lg flex items-center gap-3 ${message.type === 'success' ? 'bg-green-50 text-green-800' : 'bg-red-50 text-red-800'
                            }`}
                    >
                        {message.type === 'success' ? <CheckCircle size={20} /> : <AlertCircle size={20} />}
                        {message.text}
                    </motion.div>
                )}

                <div className="space-y-6">
                    {/* Цели обучения */}
                    <div className="bg-white rounded-xl shadow-sm p-6">
                        <h2 className="text-xl font-semibold text-gray-900 mb-4">🎯 Цели обучения</h2>

                        <div className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    Основная цель
                                </label>
                                <select
                                    value={formData.learningGoal}
                                    onChange={(e) => updateFormData('learningGoal', e.target.value)}
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                                >
                                    <option value="ENT">Подготовка к ЕНТ</option>
                                    <option value="University">Поступление в вуз</option>
                                    <option value="SelfStudy">Самообразование</option>
                                    <option value="Professional">Профессиональное развитие</option>
                                </select>
                            </div>

                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Целевой экзамен
                                    </label>
                                    <select
                                        value={formData.targetExamType || ''}
                                        onChange={(e) => updateFormData('targetExamType', e.target.value)}
                                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                                    >
                                        <option value="">Не выбран</option>
                                        {examTypes.map(examType => (
                                            <option key={examType.id} value={examType.name}>
                                                {examType.name} ({examType.code})
                                            </option>
                                        ))}
                                    </select>
                                </div>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-2">
                                    Карьерная цель
                                </label>
                                <input
                                    type="text"
                                    value={formData.careerGoal || ''}
                                    onChange={(e) => updateFormData('careerGoal', e.target.value)}
                                    placeholder="IT, Медицина, Инженерия..."
                                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                                />
                            </div>
                        </div>
                    </div>

                    {/* География */}
                    <div className="bg-white rounded-xl shadow-sm p-6">
                        <h2 className="text-xl font-semibold text-gray-900 mb-4">🌍 География</h2>

                        <div className="space-y-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Предпочитаемая страна
                                    </label>
                                    <select
                                        value={formData.preferredCountry || ''}
                                        onChange={async (e) => {
                                            updateFormData('preferredCountry', e.target.value);
                                            updateFormData('preferredCity', ''); // сбрасываем город
                                            const country = countries.find(c => c.name === e.target.value);
                                            if (country) {
                                                const citiesData = await referenceService.getCities(country.id);
                                                setCities(citiesData);
                                            } else {
                                                setCities([]);
                                            }
                                        }}
                                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                                    >
                                        <option value="">Не указана</option>
                                        {countries.map(country => (
                                            <option key={country.id} value={country.name}>
                                                {country.flagEmoji} {country.name}
                                            </option>
                                        ))}
                                    </select>
                                </div>

                                {formData.preferredCountry && cities.length > 0 && (
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-2">
                                            Предпочитаемый город
                                        </label>
                                        <select
                                            value={formData.preferredCity || ''}
                                            onChange={(e) => updateFormData('preferredCity', e.target.value)}
                                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                                        >
                                            <option value="">Не указан</option>
                                            {cities.map(city => (
                                                <option key={city.id} value={city.name}>
                                                    {city.name}
                                                </option>
                                            ))}
                                        </select>
                                    </div>
                                )}

                                <label className="flex items-center">
                                    <input
                                        type="checkbox"
                                        checked={formData.willingToRelocate}
                                        onChange={(e) => updateFormData('willingToRelocate', e.target.checked)}
                                        className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                                    />
                                    <span className="ml-2 text-sm text-gray-700">Готов(а) к переезду</span>
                                </label>
                            </div>
                        </div>

                        {/* Финансы */}
                        <div className="bg-white rounded-xl shadow-sm p-6">
                            <h2 className="text-xl font-semibold text-gray-900 mb-4">💰 Финансы</h2>

                            <div className="space-y-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Максимальный бюджет на обучение (в год, ₸)
                                    </label>
                                    <input
                                        type="number"
                                        value={formData.maxBudgetPerYear || ''}
                                        onChange={(e) => updateFormData('maxBudgetPerYear', e.target.value ? parseFloat(e.target.value) : undefined)}
                                        placeholder="500000"
                                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                                    />
                                </div>

                                <label className="flex items-center">
                                    <input
                                        type="checkbox"
                                        checked={formData.interestedInScholarships}
                                        onChange={(e) => updateFormData('interestedInScholarships', e.target.checked)}
                                        className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                                    />
                                    <span className="ml-2 text-sm text-gray-700">Интересуют гранты и стипендии</span>
                                </label>
                            </div>
                        </div>

                        {/* Языки */}
                        <div className="bg-white rounded-xl shadow-sm p-6">
                            <h2 className="text-xl font-semibold text-gray-900 mb-4">🗣️ Языки</h2>

                            <div className="space-y-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Предпочитаемые языки обучения
                                    </label>
                                    <div className="space-y-2">
                                        {['Russian', 'English', 'Kazakh'].map(lang => (
                                            <label key={lang} className="flex items-center">
                                                <input
                                                    type="checkbox"
                                                    checked={formData.preferredLanguages.includes(lang)}
                                                    onChange={() => toggleArrayValue('preferredLanguages', lang)}
                                                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                                                />
                                                <span className="ml-2 text-sm text-gray-700">
                                                    {lang === 'Russian' ? 'Русский' : lang === 'English' ? 'Английский' : 'Казахский'}
                                                </span>
                                            </label>
                                        ))}
                                    </div>
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Уровень английского
                                    </label>
                                    <select
                                        value={formData.englishLevel || ''}
                                        onChange={(e) => updateFormData('englishLevel', e.target.value)}
                                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                                    >
                                        <option value="">Не указан</option>
                                        <option value="A1">A1 (Начальный)</option>
                                        <option value="A2">A2 (Элементарный)</option>
                                        <option value="B1">B1 (Средний)</option>
                                        <option value="B2">B2 (Выше среднего)</option>
                                        <option value="C1">C1 (Продвинутый)</option>
                                        <option value="C2">C2 (Владение в совершенстве)</option>
                                    </select>
                                </div>
                            </div>
                        </div>

                        {/* Предметы */}
                        <div className="bg-white rounded-xl shadow-sm p-6">
                            <h2 className="text-xl font-semibold text-gray-900 mb-4">📚 Предметы</h2>

                            <div className="space-y-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Интересующие предметы *
                                    </label>
                                    <div className="grid grid-cols-2 md:grid-cols-3 gap-2 max-h-60 overflow-y-auto border border-gray-200 rounded-lg p-4">
                                        {subjects.map(subject => (
                                            <label key={subject.id} className="flex items-center">
                                                <input
                                                    type="checkbox"
                                                    checked={formData.interestedSubjectIds.includes(subject.id)}
                                                    onChange={() => toggleArrayValue('interestedSubjectIds', subject.id)}
                                                    className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                                                />
                                                <span className="ml-2 text-sm text-gray-700">{subject.name}</span>
                                            </label>
                                        ))}
                                    </div>
                                </div>

                                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-2">
                                            Сильные предметы
                                        </label>
                                        <div className="grid grid-cols-1 gap-2 max-h-40 overflow-y-auto border border-gray-200 rounded-lg p-4">
                                            {subjects.map(subject => (
                                                <label key={subject.id} className="flex items-center">
                                                    <input
                                                        type="checkbox"
                                                        checked={formData.strongSubjectIds.includes(subject.id)}
                                                        onChange={() => toggleArrayValue('strongSubjectIds', subject.id)}
                                                        className="h-4 w-4 text-green-600 focus:ring-green-500 border-gray-300 rounded"
                                                    />
                                                    <span className="ml-2 text-sm text-gray-700">{subject.name}</span>
                                                </label>
                                            ))}
                                        </div>
                                    </div>

                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-2">
                                            Слабые предметы
                                        </label>
                                        <div className="grid grid-cols-1 gap-2 max-h-40 overflow-y-auto border border-gray-200 rounded-lg p-4">
                                            {subjects.map(subject => (
                                                <label key={subject.id} className="flex items-center">
                                                    <input
                                                        type="checkbox"
                                                        checked={formData.weakSubjectIds.includes(subject.id)}
                                                        onChange={() => toggleArrayValue('weakSubjectIds', subject.id)}
                                                        className="h-4 w-4 text-orange-600 focus:ring-orange-500 border-gray-300 rounded"
                                                    />
                                                    <span className="ml-2 text-sm text-gray-700">{subject.name}</span>
                                                </label>
                                            ))}
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Формат обучения */}
                        <div className="bg-white rounded-xl shadow-sm p-6">
                            <h2 className="text-xl font-semibold text-gray-900 mb-4">⏰ Формат обучения</h2>

                            <div className="space-y-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Предпочитаемые форматы
                                    </label>
                                    <div className="space-y-2">
                                        <label className="flex items-center">
                                            <input
                                                type="checkbox"
                                                checked={formData.prefersFlashcards}
                                                onChange={(e) => updateFormData('prefersFlashcards', e.target.checked)}
                                                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                                            />
                                            <span className="ml-2 text-sm text-gray-700">Флэш-карточки</span>
                                        </label>
                                        <label className="flex items-center">
                                            <input
                                                type="checkbox"
                                                checked={formData.prefersQuizzes}
                                                onChange={(e) => updateFormData('prefersQuizzes', e.target.checked)}
                                                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                                            />
                                            <span className="ml-2 text-sm text-gray-700">Тесты и квизы</span>
                                        </label>
                                        <label className="flex items-center">
                                            <input
                                                type="checkbox"
                                                checked={formData.prefersExams}
                                                onChange={(e) => updateFormData('prefersExams', e.target.checked)}
                                                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                                            />
                                            <span className="ml-2 text-sm text-gray-700">Экзамены</span>
                                        </label>
                                    </div>
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Предпочитаемая сложность: {formData.preferredDifficulty === 1 ? 'Легко' : formData.preferredDifficulty === 2 ? 'Средне' : 'Сложно'}
                                    </label>
                                    <input
                                        type="range"
                                        min="1"
                                        max="3"
                                        value={formData.preferredDifficulty}
                                        onChange={(e) => updateFormData('preferredDifficulty', parseInt(e.target.value))}
                                        className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Планируемое время обучения в день: {formData.dailyStudyTimeMinutes} минут
                                    </label>
                                    <input
                                        type="range"
                                        min="5"
                                        max="240"
                                        step="5"
                                        value={formData.dailyStudyTimeMinutes}
                                        onChange={(e) => updateFormData('dailyStudyTimeMinutes', parseInt(e.target.value))}
                                        className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Предпочитаемое время
                                    </label>
                                    <select
                                        value={formData.preferredStudyTime || ''}
                                        onChange={(e) => updateFormData('preferredStudyTime', e.target.value)}
                                        className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                                    >
                                        <option value="">Не указано</option>
                                        <option value="Morning">Утро (6:00 - 12:00)</option>
                                        <option value="Afternoon">День (12:00 - 18:00)</option>
                                        <option value="Evening">Вечер (18:00 - 22:00)</option>
                                        <option value="Night">Ночь (22:00 - 6:00)</option>
                                    </select>
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Дни для обучения
                                    </label>
                                    <div className="flex flex-wrap gap-2">
                                        {[
                                            { value: 'Mon', label: 'Пн' },
                                            { value: 'Tue', label: 'Вт' },
                                            { value: 'Wed', label: 'Ср' },
                                            { value: 'Thu', label: 'Чт' },
                                            { value: 'Fri', label: 'Пт' },
                                            { value: 'Sat', label: 'Сб' },
                                            { value: 'Sun', label: 'Вс' },
                                        ].map(day => (
                                            <button
                                                key={day.value}
                                                onClick={() => toggleArrayValue('studyDays', day.value)}
                                                className={`px-4 py-2 rounded-lg border-2 transition-all ${formData.studyDays.includes(day.value)
                                                        ? 'border-indigo-600 bg-indigo-50 text-indigo-700'
                                                        : 'border-gray-200 text-gray-600 hover:border-indigo-300'
                                                    }`}
                                            >
                                                {day.label}
                                            </button>
                                        ))}
                                    </div>
                                </div>
                            </div>
                        </div>

                        {/* Мотивация */}
                        <div className="bg-white rounded-xl shadow-sm p-6">
                            <h2 className="text-xl font-semibold text-gray-900 mb-4">⚡ Мотивация</h2>

                            <div className="space-y-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-2">
                                        Уровень мотивации: {formData.motivationLevel}/5
                                    </label>
                                    <input
                                        type="range"
                                        min="1"
                                        max="5"
                                        value={formData.motivationLevel}
                                        onChange={(e) => updateFormData('motivationLevel', parseInt(e.target.value))}
                                        className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
                                    />
                                </div>

                                <label className="flex items-center">
                                    <input
                                        type="checkbox"
                                        checked={formData.needsReminders}
                                        onChange={(e) => updateFormData('needsReminders', e.target.checked)}
                                        className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                                    />
                                    <span className="ml-2 text-sm text-gray-700">Отправлять напоминания об обучении</span>
                                </label>
                            </div>
                        </div>

                        {/* Save Button */}
                        <div className="flex justify-end">
                            <button
                                onClick={handleSave}
                                disabled={saving || formData.interestedSubjectIds.length === 0}
                                className="flex items-center gap-2 px-8 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors disabled:bg-gray-300 disabled:cursor-not-allowed"
                            >
                                <Save size={20} />
                                {saving ? 'Сохранение...' : 'Сохранить изменения'}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default PreferencesPage;
