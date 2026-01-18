import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  FileText,
  Clock,
  Award,
  Play,
  Plus,
  Search,
  TrendingUp,
  BookOpen,
  AlertCircle,
  CheckCircle,
  Lock,
  Trash2,
  Upload,
  FileX,
  Edit,
} from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import api from '../../services/api';
import { useAuth } from '../../context/AuthContext';
import AIRecommendedExams from '../../components/ai/AIRecommendedExams';

interface Exam {
  id: number;
  title: string;
  description: string;
  subject: string;
  difficulty: string;
  timeLimit: number;
  questionCount: number;
  totalPoints: number;
  maxAttempts: number;
  remainingAttempts: number;
  passingScore: number;
  isProctored: boolean;
  isPublished?: boolean;
  startDate?: string;
  endDate?: string;
  createdAt: string;
  universityId: number;
}

interface Subject {
  id: number;
  name: string;
}

const ExamsPage = () => {
  const navigate = useNavigate();
  const { isTeacher, isAdmin } = useAuth();
  const [exams, setExams] = useState<Exam[]>([]);
  const [subjectsList, setSubjectsList] = useState<Subject[]>([]);
  const [countriesList, setCountriesList] = useState<{ id: number; name: string }[]>([]);
  const [universitiesList, setUniversitiesList] = useState<{ id: number; name: string; countryId: number }[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [subjectFilter, setSubjectFilter] = useState<string>('');
  const [difficultyFilter, setDifficultyFilter] = useState<string>('');
  const [countryFilter, setCountryFilter] = useState<string>('');
  const [universityFilter, setUniversityFilter] = useState<string>('');

  useEffect(() => {
    loadExams();
    loadSubjects();
    loadCountries();
    loadUniversities();
  }, [subjectFilter, difficultyFilter, countryFilter, universityFilter]);
  const loadCountries = async () => {
    try {
      const response = await api.get('/countries');
      setCountriesList(response.data);
    } catch (error) {
      console.error('Ошибка загрузки стран:', error);
    }
  };

  const loadUniversities = async () => {
    try {
      const response = await api.get('/universities');
      setUniversitiesList(response.data);
    } catch (error) {
      console.error('Ошибка загрузки вузов:', error);
    }
  };

  const loadSubjects = async () => {
    try {
      const response = await api.get('/subjects');
      setSubjectsList(response.data);
    } catch (error) {
      console.error('Ошибка загрузки предметов:', error);
    }
  };

  const loadExams = async () => {
    try {
      setLoading(true);
      
      // Админы видят все экзамены, учителя - свои, студенты - опубликованные
      let endpoint = '/student/available-exams'; // По умолчанию для студентов
      
      if (isAdmin) {
        endpoint = '/admin/exams'; // Админ видит все экзамены
      } else if (isTeacher) {
        endpoint = '/exams/my'; // Учитель видит свои экзамены
      }
      
      const params = new URLSearchParams();
      if (subjectFilter) params.append('subject', subjectFilter);
      if (difficultyFilter) params.append('difficulty', difficultyFilter);
      if (countryFilter) params.append('country', countryFilter);
      if (universityFilter) params.append('university', universityFilter);
      const response = await api.get(`${endpoint}?${params.toString()}`);
      setExams(Array.isArray(response.data) ? response.data : []);
    } catch (error) {
      console.error('Ошибка загрузки экзаменов:', error);
    } finally {
      setLoading(false);
    }
  };

  // Фильтрация вузов по стране (по id)
  const filteredUniversities = countryFilter
    ? universitiesList.filter(u => u.countryId === Number(countryFilter))
    : universitiesList;

  // Фильтрация экзаменов по вузу (по id)
  const filteredExams = exams.filter(exam => {
    const matchesSearch = exam.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      exam.description.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesUniversity = universityFilter ? exam.universityId === Number(universityFilter) : true;
    return matchesSearch && matchesUniversity;
  });

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'Easy':
        return 'text-green-600 bg-green-100';
      case 'Medium':
        return 'text-yellow-600 bg-yellow-100';
      case 'Hard':
        return 'text-red-600 bg-red-100';
      default:
        return 'text-gray-600 bg-gray-100';
    }
  };

  const getDifficultyLabel = (difficulty: string) => {
    switch (difficulty) {
      case 'Easy':
        return 'Легкий';
      case 'Medium':
        return 'Средний';
      case 'Hard':
        return 'Сложный';
      default:
        return difficulty;
    }
  };

  const isExamAvailable = (exam: Exam) => {
    const now = new Date();
    if (exam.startDate && new Date(exam.startDate) > now) return false;
    if (exam.endDate && new Date(exam.endDate) < now) return false;
    return true;
  };

  const handlePublish = async (examId: number) => {
    if (!confirm('Вы уверены, что хотите опубликовать этот экзамен?')) {
      return;
    }
    try {
      await api.patch(`/exams/${examId}/publish`);
      loadExams(); // Перезагружаем список
    } catch (error) {
      console.error('Ошибка публикации экзамена:', error);
      alert('Не удалось опубликовать экзамен');
    }
  };

  const handleUnpublish = async (examId: number) => {
    if (!confirm('Вы уверены, что хотите снять экзамен с публикации?')) {
      return;
    }
    try {
      await api.patch(`/exams/${examId}/unpublish`);
      loadExams(); // Перезагружаем список
    } catch (error) {
      console.error('Ошибка отмены публикации экзамена:', error);
      alert('Не удалось снять экзамен с публикации');
    }
  };

  const handleDelete = async (examId: number) => {
    if (!confirm('Вы уверены, что хотите удалить этот экзамен? Это действие нельзя отменить.')) {
      return;
    }
    try {
      await api.delete(`/exams/${examId}`);
      loadExams(); // Перезагружаем список
    } catch (error) {
      console.error('Ошибка удаления экзамена:', error);
      alert('Не удалось удалить экзамен');
    }
  };

  if (loading) {
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
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-7xl mx-auto">
      {/* Заголовок и кнопка создания */}
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white flex items-center gap-3">
            <FileText className="w-8 h-8 text-primary-500" />
            {isAdmin ? 'Экзамены' : (isTeacher ? 'Мои Экзамены' : 'Доступные Экзамены')}
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            {isAdmin
              ? 'Все экзамены в системе'
              : (isTeacher
                ? 'Управляйте своими экзаменами и отслеживайте результаты студентов'
                : 'Проверьте свои знания с помощью экзаменов')}
          </p>
        </div>
      </div>

      {/* AI Рекомендации */}
      <div className="mb-6">
        <AIRecommendedExams />
      </div>

      {/* Кнопка создания */}
      <div className="mb-6">
        {(isTeacher || isAdmin) && (
          <Button
            onClick={() => navigate('/exams/create')}
            variant="primary"
            className="flex items-center gap-2"
          >
            <Plus className="w-5 h-5" />
            Создать экзамен
          </Button>
        )}
      </div>

      {/* Поиск и фильтры */}
      <Card className="mb-6">
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-[2] relative min-w-[300px]">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="Поиск экзаменов..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 placeholder:text-gray-400 dark:placeholder:text-gray-500 text-base"
              style={{ minWidth: '300px', fontSize: '1.1rem' }}
            />
          </div>
          <select
            value={countryFilter}
            onChange={(e) => setCountryFilter(e.target.value)}
            className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100"
            style={{ minWidth: '180px' }}
          >
            <option value="">Все страны</option>
            {countriesList.map((country) => (
              <option key={country.id} value={country.id}>
                {country.name}
              </option>
            ))}
          </select>
          <select
            value={universityFilter}
            onChange={(e) => setUniversityFilter(e.target.value)}
            className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100"
            style={{ minWidth: '140px', maxWidth: '220px' }}
          >
            <option value="">Все вузы</option>
            {filteredUniversities.map((university) => (
              <option key={university.id} value={university.id}>
                {university.name}
              </option>
            ))}
          </select>
          <select
            value={subjectFilter}
            onChange={(e) => setSubjectFilter(e.target.value)}
            className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100"
          >
            <option value="">Все предметы</option>
            {subjectsList.map((subject) => (
              <option key={subject.id} value={subject.name}>
                {subject.name}
              </option>
            ))}
          </select>
          <select
            value={difficultyFilter}
            onChange={(e) => setDifficultyFilter(e.target.value)}
            className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100"
          >
            <option value="">Все уровни</option>
            <option value="Easy">Легкий</option>
            <option value="Medium">Средний</option>
            <option value="Hard">Сложный</option>
          </select>
        </div>
      </Card>

      {/* Список экзаменов */}
      {filteredExams.length === 0 ? (
        <Card className="text-center py-12">
          <FileText className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-700 mb-2">
            {searchQuery || subjectFilter || difficultyFilter
              ? 'Экзамены не найдены'
              : 'Нет доступных экзаменов'}
          </h3>
          <p className="text-gray-600 mb-6">
            {(isTeacher || isAdmin)
              ? 'Создайте свой первый экзамен для студентов'
              : 'Пока нет доступных экзаменов для прохождения'}
          </p>
          {(isTeacher || isAdmin) && (
            <Button
              onClick={() => navigate('/exams/create')}
              variant="primary"
              className="inline-flex items-center gap-2"
            >
              <Plus className="w-5 h-5" />
              Создать экзамен
            </Button>
          )}
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredExams.map((exam, index) => {
            const available = isExamAvailable(exam);
            
            return (
              <motion.div
                key={exam.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.1 }}
              >
                <Card className="h-full flex flex-col hover:shadow-lg transition-shadow">
                  {/* Заголовок экзамена */}
                  <div className="flex justify-between items-start mb-3">
                    <h3 className="text-lg font-semibold text-gray-900 dark:text-white flex-1">
                      {exam.title}
                    </h3>
                    <span
                      className={`px-2 py-1 rounded-full text-xs font-medium ${getDifficultyColor(
                        exam.difficulty
                      )}`}
                    >
                      {getDifficultyLabel(exam.difficulty)}
                    </span>
                  </div>

                  {/* Описание */}
                  <p className="text-gray-600 dark:text-gray-400 text-sm mb-4 flex-1">
                    {exam.description}
                  </p>

                  {/* Предмет */}
                  <div className="flex items-center gap-2 mb-3">
                    <BookOpen className="w-4 h-4 text-gray-400" />
                    <span className="text-sm text-gray-700">{exam.subject}</span>
                  </div>

                  {/* Статистика */}
                  <div className="grid grid-cols-2 gap-3 mb-4">
                    <div className="flex items-center gap-2">
                      <FileText className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">
                        {exam.questionCount} вопросов
                      </span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Clock className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">{exam.timeLimit} мин</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Award className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">
                        {exam.maxScore} баллов
                      </span>
                    </div>
                    <div className="flex items-center gap-2">
                      <TrendingUp className="w-4 h-4 text-gray-400" />
                      <span className="text-sm text-gray-600">
                        {exam.passingScore}% проходной
                      </span>
                    </div>
                  </div>

                  {/* Особенности экзамена */}
                  <div className="flex flex-wrap gap-2 mb-4">
                    {!(isTeacher || isAdmin) && (
                      <span className={`px-2 py-1 rounded text-xs flex items-center gap-1 ${
                        exam.remainingAttempts <= 0 
                          ? 'bg-red-100 text-red-700' 
                          : 'bg-blue-100 text-blue-700'
                      }`}>
                        <AlertCircle className="w-3 h-3" />
                        {exam.remainingAttempts} из {exam.maxAttempts} {exam.maxAttempts === 1 ? 'попытки' : 'попыток'} осталось
                      </span>
                    )}
                    {exam.isProctored && (
                      <span className="px-2 py-1 bg-purple-100 text-purple-700 rounded text-xs flex items-center gap-1">
                        <Lock className="w-3 h-3" />
                        С контролем
                      </span>
                    )}
                    {!available && (
                      <span className="px-2 py-1 bg-gray-100 text-gray-700 rounded text-xs flex items-center gap-1">
                        <Clock className="w-3 h-3" />
                        Недоступен
                      </span>
                    )}
                  </div>

                  {/* Кнопки действий */}
                  {(isTeacher || isAdmin) ? (
                    <div className="flex flex-col gap-2">
                      <div className="flex gap-2">
                        <Button
                          onClick={() => navigate(`/exams/${exam.id}/edit`)}
                          variant="secondary"
                          className="flex-1 flex items-center justify-center gap-2"
                        >
                          <Edit className="w-4 h-4" />
                          Редактировать
                        </Button>
                        <Button
                          onClick={() => navigate(`/exams/${exam.id}/stats`)}
                          variant="primary"
                          className="flex-1 flex items-center justify-center gap-2"
                        >
                          <TrendingUp className="w-4 h-4" />
                          Статистика
                        </Button>
                      </div>
                      <div className="flex gap-2">
                        {exam.isPublished ? (
                          <Button
                            variant="secondary"
                            size="sm"
                            className="flex-1 flex items-center justify-center gap-2"
                            onClick={() => handleUnpublish(exam.id)}
                          >
                            <FileX className="w-4 h-4" />
                            Снять с публикации
                          </Button>
                        ) : (
                          <Button
                            variant="primary"
                            size="sm"
                            className="flex-1 flex items-center justify-center gap-2 bg-green-600 hover:bg-green-700"
                            onClick={() => handlePublish(exam.id)}
                          >
                            <Upload className="w-4 h-4" />
                            Опубликовать
                          </Button>
                        )}
                        <Button
                          variant="danger"
                          size="sm"
                          className="px-4 flex items-center justify-center gap-2"
                          onClick={() => handleDelete(exam.id)}
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      </div>
                    </div>
                  ) : (
                    <Button
                      onClick={() => navigate(`/exams/${exam.id}/take`)}
                      variant="primary"
                      className="w-full flex items-center justify-center gap-2"
                      disabled={!available || exam.remainingAttempts <= 0}
                    >
                      {exam.remainingAttempts <= 0 ? (
                        <>
                          <FileX className="w-5 h-5" />
                          Попытки исчерпаны
                        </>
                      ) : available ? (
                        <>
                          <Play className="w-4 h-4" />
                          Начать экзамен
                        </>
                      ) : (
                        <>
                          <Lock className="w-4 h-4" />
                          Недоступен
                        </>
                      )}
                    </Button>
                  )}

                  {/* Статус публикации для преподавателей */}
                  {(isTeacher || isAdmin) && (
                    <div className="mt-3 pt-3 border-t border-gray-200 flex items-center justify-between">
                      <span className="text-xs text-gray-500">
                        {exam.isPublished ? (
                          <span className="flex items-center gap-1 text-green-600">
                            <CheckCircle className="w-3 h-3" />
                            Опубликован
                          </span>
                        ) : (
                          <span className="flex items-center gap-1 text-gray-600">
                            <AlertCircle className="w-3 h-3" />
                            Черновик
                          </span>
                        )}
                      </span>
                      <span className="text-xs text-gray-400">
                        {new Date(exam.createdAt).toLocaleDateString('ru-RU')}
                      </span>
                    </div>
                  )}
                </Card>
              </motion.div>
            );
          })}
        </div>
      )}
      </div>
    </div>
  );
};

export default ExamsPage;
