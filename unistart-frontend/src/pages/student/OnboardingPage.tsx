import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  ChevronRight, 
  ChevronLeft, 
  Check, 
  X,
  Target,
  Globe,
  Wallet,
  Languages,
  BookOpen,
  Clock,
  Zap
} from 'lucide-react';
import preferencesService, { UserPreferencesDto } from '../../services/preferencesService';
import { subjectService, Subject } from '../../services/subjectService';
import { referenceService, Country, ExamType, City } from '../../services/referenceService';

const OnboardingPage: React.FC = () => {
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [countries, setCountries] = useState<Country[]>([]);
  const [examTypes, setExamTypes] = useState<ExamType[]>([]);
  const [cities, setCities] = useState<City[]>([]);
  
  // Состояние формы
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
    loadSubjects();
  }, []);

  const loadSubjects = async () => {
    try {
      const [subjectsData, countriesData, examTypesData] = await Promise.all([
        subjectService.getSubjects(),
        referenceService.getCountries(),
        referenceService.getExamTypes()
      ]);
      setSubjects(subjectsData);
      setCountries(countriesData);
      setExamTypes(examTypesData);
    } catch (error) {
      console.error('Failed to load reference data:', error);
    }
  };

  const loadCitiesForCountry = async (countryName: string) => {
    try {
      const country = countries.find(c => c.name === countryName);
      if (country) {
        const citiesData = await referenceService.getCities(country.id);
        setCities(citiesData);
      }
    } catch (error) {
      console.error('Failed to load cities:', error);
    }
  };

  const steps = [
    { title: 'Цели обучения', icon: Target },
    { title: 'География', icon: Globe },
    { title: 'Финансы', icon: Wallet },
    { title: 'Языки', icon: Languages },
    { title: 'Предметы', icon: BookOpen },
    { title: 'Формат обучения', icon: Clock },
    { title: 'Мотивация', icon: Zap },
  ];

  const handleSkip = async () => {
    try {
      setLoading(true);
      await preferencesService.skipOnboarding();
      navigate('/dashboard');
    } catch (error) {
      console.error('Failed to skip onboarding:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleNext = () => {
    if (currentStep < steps.length - 1) {
      setCurrentStep(currentStep + 1);
    } else {
      handleSubmit();
    }
  };

  const handleBack = () => {
    if (currentStep > 0) {
      setCurrentStep(currentStep - 1);
    }
  };

  const handleSubmit = async () => {
    try {
      setLoading(true);
      await preferencesService.createOrUpdatePreferences(formData);
      await preferencesService.completeOnboarding();
      navigate('/dashboard');
    } catch (error) {
      console.error('Failed to save preferences:', error);
      alert('Ошибка при сохранении предпочтений');
    } finally {
      setLoading(false);
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

  const renderStepContent = () => {
    switch (currentStep) {
      case 0: // Цели обучения
        return (
          <div className="space-y-6">
            <h2 className="text-2xl font-bold text-gray-900">Какая у вас основная цель обучения?</h2>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {[
                { value: 'ENT', label: 'Подготовка к ЕНТ', description: 'Сдача единого национального тестирования' },
                { value: 'University', label: 'Поступление в вуз', description: 'Подготовка к поступлению в университет' },
                { value: 'SelfStudy', label: 'Самообразование', description: 'Изучение интересующих тем' },
                { value: 'Professional', label: 'Профессиональное развитие', description: 'Повышение квалификации' },
              ].map(goal => (
                <button
                  key={goal.value}
                  onClick={() => updateFormData('learningGoal', goal.value)}
                  className={`p-4 text-left rounded-xl border-2 transition-all ${
                    formData.learningGoal === goal.value
                      ? 'border-indigo-600 bg-indigo-50'
                      : 'border-gray-200 hover:border-indigo-300'
                  }`}
                >
                  <div className="font-semibold text-gray-900">{goal.label}</div>
                  <div className="text-sm text-gray-600 mt-1">{goal.description}</div>
                </button>
              ))}
            </div>

            {(formData.learningGoal === 'ENT' || formData.learningGoal === 'University') && (
              <div className="mt-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Целевой экзамен (опционально)
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
            )}

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Карьерная цель (опционально)
              </label>
              <input
                type="text"
                value={formData.careerGoal || ''}
                onChange={(e) => updateFormData('careerGoal', e.target.value)}
                placeholder="Например: IT, Медицина, Инженерия"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
              />
            </div>
          </div>
        );

      case 1: // География
        return (
          <div className="space-y-6">
            <h2 className="text-2xl font-bold text-gray-900">Где вы планируете учиться?</h2>
            
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Предпочитаемая страна
              </label>
              <select
                value={formData.preferredCountry || ''}
                onChange={(e) => {
                  updateFormData('preferredCountry', e.target.value);
                  updateFormData('preferredCity', ''); // сбрасываем город при смене страны
                  loadCitiesForCountry(e.target.value);
                }}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
              >
                <option value="">Выберите страну</option>
                {countries.map(country => (
                  <option key={country.id} value={country.name}>
                    {country.flagEmoji} {country.name}
                  </option>
                ))}
              </select>

              {formData.preferredCountry && cities.length > 0 && (
                <div className="mt-4">
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Предпочитаемый город (опционально)
                  </label>
                  <select
                    value={formData.preferredCity || ''}
                    onChange={(e) => updateFormData('preferredCity', e.target.value)}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                  >
                    <option value="">Выберите город</option>
                    {cities.map(city => (
                      <option key={city.id} value={city.name}>
                        {city.name}
                      </option>
                    ))}
                  </select>
                </div>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Предпочитаемый город
              </label>
              <input
                type="text"
                value={formData.preferredCity || ''}
                onChange={(e) => updateFormData('preferredCity', e.target.value)}
                placeholder="Например: Алматы, Астана"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            <div className="flex items-center">
              <input
                type="checkbox"
                checked={formData.willingToRelocate}
                onChange={(e) => updateFormData('willingToRelocate', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
              />
              <label className="ml-2 text-sm text-gray-700">
                Готов(а) к переезду в другой город/страну
              </label>
            </div>
          </div>
        );

      case 2: // Финансы
        return (
          <div className="space-y-6">
            <h2 className="text-2xl font-bold text-gray-900">Финансовые возможности</h2>
            
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Максимальный бюджет на обучение (в год, ₸)
              </label>
              <input
                type="number"
                value={formData.maxBudgetPerYear || ''}
                onChange={(e) => updateFormData('maxBudgetPerYear', e.target.value ? parseFloat(e.target.value) : undefined)}
                placeholder="Например: 500000"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
              />
            </div>

            <div className="flex items-center">
              <input
                type="checkbox"
                checked={formData.interestedInScholarships}
                onChange={(e) => updateFormData('interestedInScholarships', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
              />
              <label className="ml-2 text-sm text-gray-700">
                Интересуют гранты и стипендии
              </label>
            </div>
          </div>
        );

      case 3: // Языки
        return (
          <div className="space-y-6">
            <h2 className="text-2xl font-bold text-gray-900">Языковые предпочтения</h2>
            
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
                Уровень английского языка
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
        );

      case 4: // Предметы
        return (
          <div className="space-y-6">
            <h2 className="text-2xl font-bold text-gray-900">Выберите предметы</h2>
            
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
              {formData.interestedSubjectIds.length === 0 && (
                <p className="text-sm text-red-600 mt-1">Выберите хотя бы один предмет</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Сильные предметы (опционально)
              </label>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-2 max-h-40 overflow-y-auto border border-gray-200 rounded-lg p-4">
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
                Слабые предметы (нужно подтянуть)
              </label>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-2 max-h-40 overflow-y-auto border border-gray-200 rounded-lg p-4">
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
        );

      case 5: // Формат обучения
        return (
          <div className="space-y-6">
            <h2 className="text-2xl font-bold text-gray-900">Формат обучения</h2>
            
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
              <div className="flex justify-between text-xs text-gray-500 mt-1">
                <span>Легко</span>
                <span>Средне</span>
                <span>Сложно</span>
              </div>
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
              <div className="flex justify-between text-xs text-gray-500 mt-1">
                <span>5 мин</span>
                <span>120 мин</span>
                <span>4 часа</span>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Предпочитаемое время для обучения
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
                    className={`px-4 py-2 rounded-lg border-2 transition-all ${
                      formData.studyDays.includes(day.value)
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
        );

      case 6: // Мотивация
        return (
          <div className="space-y-6">
            <h2 className="text-2xl font-bold text-gray-900">Последний шаг!</h2>
            
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
              <div className="flex justify-between text-xs text-gray-500 mt-1">
                <span>😴</span>
                <span>😐</span>
                <span>🙂</span>
                <span>😊</span>
                <span>🔥</span>
              </div>
            </div>

            <div className="flex items-center">
              <input
                type="checkbox"
                checked={formData.needsReminders}
                onChange={(e) => updateFormData('needsReminders', e.target.checked)}
                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
              />
              <label className="ml-2 text-sm text-gray-700">
                Отправлять напоминания об обучении
              </label>
            </div>

            <div className="bg-indigo-50 border border-indigo-200 rounded-lg p-4">
              <p className="text-sm text-indigo-900">
                <strong>Готово!</strong> На основе ваших ответов мы подберем персонализированные рекомендации
                для вашего обучения. Вы всегда сможете изменить настройки в профиле.
              </p>
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  const canProceed = () => {
    if (currentStep === 4) {
      return formData.interestedSubjectIds.length > 0;
    }
    return true;
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-white to-purple-50 py-12 px-4">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Добро пожаловать в UniStart! 🎓</h1>
          <p className="text-gray-600">Давайте настроим ваше обучение под вас</p>
        </div>

        {/* Progress Bar */}
        <div className="mb-8">
          <div className="flex items-center justify-between mb-2">
            {steps.map((step, index) => {
              const Icon = step.icon;
              return (
                <div key={index} className="flex flex-col items-center flex-1">
                  <div className={`w-10 h-10 rounded-full flex items-center justify-center transition-all ${
                    index < currentStep 
                      ? 'bg-green-500 text-white' 
                      : index === currentStep 
                      ? 'bg-indigo-600 text-white' 
                      : 'bg-gray-200 text-gray-400'
                  }`}>
                    {index < currentStep ? <Check size={20} /> : <Icon size={20} />}
                  </div>
                  <span className="text-xs mt-2 text-center hidden md:block">{step.title}</span>
                </div>
              );
            })}
          </div>
          <div className="w-full bg-gray-200 rounded-full h-2">
            <div 
              className="bg-indigo-600 h-2 rounded-full transition-all duration-300"
              style={{ width: `${((currentStep + 1) / steps.length) * 100}%` }}
            />
          </div>
        </div>

        {/* Content */}
        <div className="bg-white rounded-2xl shadow-xl p-8 mb-6">
          <AnimatePresence mode="wait">
            <motion.div
              key={currentStep}
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
              transition={{ duration: 0.3 }}
            >
              {renderStepContent()}
            </motion.div>
          </AnimatePresence>
        </div>

        {/* Navigation */}
        <div className="flex items-center justify-between">
          <div>
            {currentStep > 0 && (
              <button
                onClick={handleBack}
                className="flex items-center gap-2 px-6 py-3 text-gray-600 hover:text-gray-900 transition-colors"
              >
                <ChevronLeft size={20} />
                Назад
              </button>
            )}
          </div>

          <div className="flex items-center gap-4">
            <button
              onClick={handleSkip}
              disabled={loading}
              className="flex items-center gap-2 px-6 py-3 text-gray-600 hover:text-gray-900 transition-colors"
            >
              <X size={20} />
              Пропустить
            </button>

            <button
              onClick={handleNext}
              disabled={loading || !canProceed()}
              className="flex items-center gap-2 px-8 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors disabled:bg-gray-300 disabled:cursor-not-allowed"
            >
              {currentStep === steps.length - 1 ? (
                <>
                  <Check size={20} />
                  Завершить
                </>
              ) : (
                <>
                  Далее
                  <ChevronRight size={20} />
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default OnboardingPage;
