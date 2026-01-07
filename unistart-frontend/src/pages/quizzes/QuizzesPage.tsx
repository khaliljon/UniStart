import { useEffect, useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  BookOpen,
  Play,
  Plus,
  Search,
  ChevronRight,
  ChevronDown,
  Target,
  Clock,
  Award,
  Edit,
  Trash2,
  Upload,
  FileX,
  // CheckCircle, // Unused
  // AlertCircle, // Unused
  TrendingUp,
  // GraduationCap, // Unused
  Layers,
  BookMarked,
  FileText,
  Puzzle,
  Trophy,
  // Video, // Unused
  // Sparkles, // Unused
} from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import api from '../../services/api';
import { useAuth } from '../../context/AuthContext';

interface Course {
  id: number;
  title: string;
  description?: string;
  icon?: string;
  coverImageUrl?: string;
  year?: number;
  direction?: string;
  subjectsCount: number;
  subjects: Subject[];
}

interface Subject {
  id: number;
  name: string;
  description?: string;
  modulesCount?: number;
  hasHierarchy?: boolean;
  modules?: Module[];
}

interface Module {
  id: number;
  title: string;
  description?: string;
  icon?: string;
  orderIndex: number;
  entQuestionCount?: number;
  competencies?: Competency[];
  hasCaseStudy?: boolean;
  caseStudyQuizId?: number;
  caseStudyQuizTitle?: string;
  hasModuleFinalQuiz?: boolean;
  moduleFinalQuizId?: number;
  moduleFinalQuizTitle?: string;
}

interface Competency {
  id: number;
  title: string;
  description?: string;
  icon?: string;
  orderIndex: number;
  topics?: Topic[];
}

interface Topic {
  id: number;
  title: string;
  description?: string;
  icon?: string;
  orderIndex: number;
  hasTheory: boolean;
  theoryId?: number;
  hasPracticeQuiz: boolean;
  practiceQuizId?: number;
  practiceQuizTitle?: string;
}

interface SimpleQuiz {
  id: number;
  title: string;
  description: string;
  subject: string;
  difficulty: string;
  timeLimit: number;
  questionCount: number;
  totalPoints: number;
  isPublished?: boolean;
  createdAt: string;
}

const QuizzesPage = () => {
  const navigate = useNavigate();
  const { isTeacher, isAdmin } = useAuth();
  const [viewMode, setViewMode] = useState<'hierarchy' | 'list'>('hierarchy'); // 'hierarchy' или 'list'
  
  // Данные для иерархии
  const [courses, setCourses] = useState<Course[]>([]);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [expandedItems, setExpandedItems] = useState<Set<string>>(new Set());
  
  // Данные для списка квизов (старый режим)
  const [quizzes, setQuizzes] = useState<SimpleQuiz[]>([]);
  const [subjectsList, setSubjectsList] = useState<Subject[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [subjectFilter, setSubjectFilter] = useState<string>('');
  const [difficultyFilter, setDifficultyFilter] = useState<string>('');

  useEffect(() => {
    loadData();
  }, [viewMode, subjectFilter, difficultyFilter]);

  const loadData = async () => {
    try {
      setLoading(true);
      if (viewMode === 'hierarchy') {
        await loadHierarchy();
      } else {
        await loadQuizzes();
        await loadSubjects();
      }
    } catch (error) {
      console.error('Ошибка загрузки данных:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadHierarchy = async () => {
    try {
      // Загружаем курсы
      const coursesResponse = await api.get('/learning/courses');
      setCourses(coursesResponse.data);
      
      // Загружаем предметы без курсов (standalone)
      const subjectsResponse = await api.get('/learning/subjects');
      setSubjects(subjectsResponse.data.filter((s: Subject) => s.hasHierarchy));
    } catch (error) {
      console.error('Ошибка загрузки иерархии:', error);
    }
  };

  const loadSubjectHierarchy = async (subjectId: number) => {
    try {
      const response = await api.get(`/learning/subjects/${subjectId}`);
      const subjectData = response.data;
      
      // Обновляем предмет в списке
      setSubjects(prev => prev.map(s => s.id === subjectId ? { ...s, modules: subjectData.modules } : s));
      
      // Обновляем предметы в курсах
      setCourses(prev => prev.map(course => ({
        ...course,
        subjects: course.subjects.map(s => s.id === subjectId ? { ...s, modules: subjectData.modules } : s)
      })));
    } catch (error) {
      console.error('Ошибка загрузки иерархии предмета:', error);
    }
  };

  const loadCourseHierarchy = async (courseId: number) => {
    try {
      const response = await api.get(`/learning/courses/${courseId}`);
      const courseData = response.data;
      
      setCourses(prev => prev.map(c => c.id === courseId ? courseData : c));
    } catch (error) {
      console.error('Ошибка загрузки иерархии курса:', error);
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

  const loadQuizzes = async () => {
    try {
      let endpoint = '/student/available-quizzes';
      if (isAdmin) {
        endpoint = '/admin/quizzes';
      } else if (isTeacher) {
        endpoint = '/quizzes/my';
      }
      
      const params = new URLSearchParams();
      if (subjectFilter) params.append('subject', subjectFilter);
      if (difficultyFilter) params.append('difficulty', difficultyFilter);
      
      const response = await api.get(`${endpoint}?${params.toString()}`);
      setQuizzes(Array.isArray(response.data) ? response.data : []);
    } catch (error) {
      console.error('Ошибка загрузки квизов:', error);
      setQuizzes([]);
    }
  };

  const toggleExpand = (key: string) => {
    setExpandedItems(prev => {
      const newSet = new Set(prev);
      if (newSet.has(key)) {
        newSet.delete(key);
      } else {
        newSet.add(key);
        // Автозагрузка данных при раскрытии
        if (key.startsWith('course-')) {
          const courseId = parseInt(key.split('-')[1]);
          loadCourseHierarchy(courseId);
        } else if (key.startsWith('subject-')) {
          const subjectId = parseInt(key.split('-')[1]);
          loadSubjectHierarchy(subjectId);
        }
      }
      return newSet;
    });
  };

  const navigateToTheory = (theoryId: number) => {
    navigate(`/learning/theory/${theoryId}`);
  };

  const navigateToQuiz = (quizId: number) => {
    navigate(`/quizzes/${quizId}/take`);
  };

  const handlePublish = async (quizId: number) => {
    if (!confirm('Опубликовать квиз?')) return;
    try {
      await api.patch(`/quizzes/${quizId}/publish`);
      await loadQuizzes();
    } catch (error) {
      console.error('Ошибка публикации квиза:', error);
      alert('Не удалось опубликовать квиз');
    }
  };

  const handleUnpublish = async (quizId: number) => {
    if (!confirm('Снять квиз с публикации?')) return;
    try {
      await api.patch(`/quizzes/${quizId}/unpublish`);
      await loadQuizzes();
    } catch (error) {
      console.error('Ошибка снятия с публикации:', error);
      alert('Не удалось снять квиз с публикации');
    }
  };

  const handleDelete = async (quizId: number) => {
    if (!confirm('Вы уверены, что хотите удалить этот квиз?')) return;
    try {
      await api.delete(`/quizzes/${quizId}`);
      // Перезагружаем список независимо от статуса ответа
      await loadQuizzes();
    } catch (error: any) {
      console.error('Ошибка удаления квиза:', error);
      const errorMessage = error.response?.data?.message || error.response?.statusText || 'Не удалось удалить квиз';
      alert(errorMessage);
      // Все равно перезагружаем список на случай, если удаление прошло, но был ошибка ответа
      await loadQuizzes();
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка...</div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8"
        >
          <div className="flex items-center justify-between mb-6">
            <div>
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white mb-3 flex items-center gap-3">
                <FileText className="w-8 h-8 text-primary-500" />
                {isAdmin || isTeacher ? 'Квизы' : 'Квизы'}
              </h1>
              <p className="text-gray-600 dark:text-gray-300 text-lg">
                {isAdmin || isTeacher
                  ? 'Создавайте курсы, модули и квизы. Управляйте иерархией обучения'
                  : 'Изучайте по модулям, темам и компетенциям'}
              </p>
            </div>
            <div className="flex items-center gap-3">
              <div className="flex bg-white dark:bg-gray-800 rounded-lg p-1 border border-gray-200 dark:border-gray-700 shadow-sm">
                <button
                  onClick={() => setViewMode('hierarchy')}
                  className={`px-4 py-2 rounded-md transition-all ${
                    viewMode === 'hierarchy'
                      ? 'bg-primary-500 text-white'
                      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                  }`}
                >
                  Иерархия
                </button>
                <button
                  onClick={() => setViewMode('list')}
                  className={`px-4 py-2 rounded-md transition-all ${
                    viewMode === 'list'
                      ? 'bg-primary-500 text-white'
                      : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                  }`}
                >
                  Список
                </button>
              </div>
              {(isTeacher || isAdmin) && (
                <Button
                  variant="primary"
                  onClick={() => navigate('/quizzes/create')}
                  className="flex items-center gap-2"
                >
                  <Plus className="w-5 h-5" />
                  Создать квиз
                </Button>
              )}
            </div>
          </div>
        </motion.div>

        {viewMode === 'hierarchy' ? (
          <HierarchyView
            courses={courses}
            subjects={subjects}
            expandedItems={expandedItems}
            onToggleExpand={toggleExpand}
            onTheoryClick={navigateToTheory}
            onQuizClick={navigateToQuiz}
          />
        ) : (
          <ListView
            quizzes={quizzes.filter((quiz) => {
              const matchesSearch = searchQuery 
                ? (quiz.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
                   quiz.description.toLowerCase().includes(searchQuery.toLowerCase()))
                : true;
              const matchesSubject = subjectFilter ? quiz.subject === subjectFilter : true;
              const matchesDifficulty = difficultyFilter ? quiz.difficulty === difficultyFilter : true;
              return matchesSearch && matchesSubject && matchesDifficulty;
            })}
            subjectsList={subjectsList}
            searchQuery={searchQuery}
            setSearchQuery={setSearchQuery}
            subjectFilter={subjectFilter}
            setSubjectFilter={setSubjectFilter}
            difficultyFilter={difficultyFilter}
            setDifficultyFilter={setDifficultyFilter}
            isTeacher={isTeacher || false}
            isAdmin={isAdmin || false}
            navigate={navigate}
            onPublish={handlePublish}
            onUnpublish={handleUnpublish}
            onDelete={handleDelete}
          />
        )}
      </div>
    </div>
  );
};

// Компонент для отображения иерархии
const HierarchyView = ({
  courses,
  subjects,
  expandedItems,
  onToggleExpand,
  onTheoryClick,
  onQuizClick,
}: {
  courses: Course[];
  subjects: Subject[];
  expandedItems: Set<string>;
  onToggleExpand: (key: string) => void;
  onTheoryClick: (id: number) => void;
  onQuizClick: (id: number) => void;
}) => {
  if (courses.length === 0 && subjects.length === 0) {
    return (
      <Card className="p-12 text-center">
        <BookOpen className="w-16 h-16 text-gray-400 mx-auto mb-4" />
        <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
          Иерархия обучения пока не настроена
        </h3>
        <p className="text-gray-600 dark:text-gray-400">
          Курсы и модули будут доступны после их создания
        </p>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      {/* Курсы */}
      {courses.map((course, courseIndex) => (
        <motion.div
          key={course.id}
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: courseIndex * 0.1 }}
        >
          <CourseCard
            course={course}
            expandedItems={expandedItems}
            onToggleExpand={onToggleExpand}
            onTheoryClick={onTheoryClick}
            onQuizClick={onQuizClick}
          />
        </motion.div>
      ))}

      {/* Standalone предметы (без курса) */}
      {subjects.length > 0 && (
        <div className="mt-8">
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
            <Layers className="w-6 h-6 text-primary-500" />
            Предметы
          </h2>
          <div className="space-y-4">
            {subjects.map((subject, subjectIndex) => (
              <motion.div
                key={subject.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: (courses.length + subjectIndex) * 0.1 }}
              >
                <SubjectCard
                  subject={subject}
                  expandedItems={expandedItems}
                  onToggleExpand={onToggleExpand}
                  onTheoryClick={onTheoryClick}
                  onQuizClick={onQuizClick}
                />
              </motion.div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

// Карточка курса
const CourseCard = ({
  course,
  expandedItems,
  onToggleExpand,
  onTheoryClick,
  onQuizClick,
}: {
  course: Course;
  expandedItems: Set<string>;
  onToggleExpand: (key: string) => void;
  onTheoryClick: (id: number) => void;
  onQuizClick: (id: number) => void;
}) => {
  const isExpanded = expandedItems.has(`course-${course.id}`);

  return (
    <Card className="bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-gray-800 dark:to-gray-700 border border-gray-200 dark:border-gray-600">
      <div
        className="cursor-pointer"
        onClick={() => onToggleExpand(`course-${course.id}`)}
      >
        <div className="flex items-center justify-between p-6">
          <div className="flex items-center gap-4">
            <div className="text-5xl">{course.icon || '🎓'}</div>
            <div>
              <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-1">{course.title}</h3>
              {course.description && (
                <p className="text-gray-600 dark:text-gray-300">{course.description}</p>
              )}
              <div className="flex items-center gap-4 mt-2 text-sm text-gray-500 dark:text-gray-400">
                {course.year && <span>📅 {course.year}</span>}
                {course.direction && <span>📍 {course.direction}</span>}
                <span>📚 {course.subjectsCount} предметов</span>
              </div>
            </div>
          </div>
          <div className="text-gray-500 dark:text-gray-400">
            {isExpanded ? (
              <ChevronDown className="w-6 h-6" />
            ) : (
              <ChevronRight className="w-6 h-6" />
            )}
          </div>
        </div>
      </div>

      <AnimatePresence>
        {isExpanded && course.subjects && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            className="overflow-hidden"
          >
            <div className="px-6 pb-6 space-y-4 border-t border-white/10 pt-4">
              {course.subjects.map((subject) => (
                <SubjectCard
                  key={subject.id}
                  subject={subject}
                  expandedItems={expandedItems}
                  onToggleExpand={onToggleExpand}
                  onTheoryClick={onTheoryClick}
                  onQuizClick={onQuizClick}
                  nested
                />
              ))}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </Card>
  );
};

// Карточка предмета
const SubjectCard = ({
  subject,
  expandedItems,
  onToggleExpand,
  onTheoryClick,
  onQuizClick,
  nested = false,
}: {
  subject: Subject;
  expandedItems: Set<string>;
  onToggleExpand: (key: string) => void;
  onTheoryClick: (id: number) => void;
  onQuizClick: (id: number) => void;
  nested?: boolean;
}) => {
  const isExpanded = expandedItems.has(`subject-${subject.id}`);

  return (
    <Card className={`${nested ? 'bg-white/5' : 'bg-white/10'} backdrop-blur-lg border-white/20`}>
      <div
        className="cursor-pointer"
        onClick={() => onToggleExpand(`subject-${subject.id}`)}
      >
        <div className="flex items-center justify-between p-4">
          <div className="flex items-center gap-3">
            <BookOpen className="w-5 h-5 text-blue-400" />
            <div>
              <h4 className="text-lg font-semibold text-gray-900 dark:text-white">{subject.name}</h4>
              {subject.description && (
                <p className="text-gray-600 dark:text-gray-400 text-sm">{subject.description}</p>
              )}
              {subject.modulesCount !== undefined && (
                <span className="text-gray-500 dark:text-gray-400 text-xs mt-1">
                  {subject.modulesCount} модулей
                </span>
              )}
            </div>
          </div>
          {subject.modules && subject.modules.length > 0 && (
            <div className="text-gray-500 dark:text-gray-400">
              {isExpanded ? (
                <ChevronDown className="w-5 h-5" />
              ) : (
                <ChevronRight className="w-5 h-5" />
              )}
            </div>
          )}
        </div>
      </div>

      <AnimatePresence>
        {isExpanded && subject.modules && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            className="overflow-hidden"
          >
            <div className="px-4 pb-4 space-y-3 border-t border-gray-200 dark:border-gray-700 pt-4">
              {subject.modules.map((module) => (
                <ModuleCard
                  key={module.id}
                  module={module}
                  expandedItems={expandedItems}
                  onToggleExpand={onToggleExpand}
                  onTheoryClick={onTheoryClick}
                  onQuizClick={onQuizClick}
                />
              ))}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </Card>
  );
};

// Карточка модуля
const ModuleCard = ({
  module,
  expandedItems,
  onToggleExpand,
  onTheoryClick,
  onQuizClick,
}: {
  module: Module;
  expandedItems: Set<string>;
  onToggleExpand: (key: string) => void;
  onTheoryClick: (id: number) => void;
  onQuizClick: (id: number) => void;
}) => {
  const isExpanded = expandedItems.has(`module-${module.id}`);

  return (
    <div className="bg-white/5 rounded-lg border border-white/10">
      <div
        className="cursor-pointer"
        onClick={() => onToggleExpand(`module-${module.id}`)}
      >
        <div className="flex items-center justify-between p-3">
          <div className="flex items-center gap-3">
            <div className="text-2xl">{module.icon || '📦'}</div>
            <div>
              <h5 className="text-base font-semibold text-gray-900 dark:text-white">{module.title}</h5>
              {module.description && (
                <p className="text-gray-600 dark:text-gray-400 text-xs">{module.description}</p>
              )}
              <div className="flex items-center gap-3 mt-1">
                {module.entQuestionCount && (
                  <span className="text-xs text-yellow-600 dark:text-yellow-400 flex items-center gap-1">
                    <Target className="w-3 h-3" />
                    ЕНТ: {module.entQuestionCount} вопросов
                  </span>
                )}
                {module.competencies && (
                  <span className="text-xs text-gray-500 dark:text-gray-400">
                    {module.competencies.length} компетенций
                  </span>
                )}
              </div>
            </div>
          </div>
          {module.competencies && module.competencies.length > 0 && (
            <div className="text-gray-500 dark:text-gray-400">
              {isExpanded ? (
                <ChevronDown className="w-4 h-4" />
              ) : (
                <ChevronRight className="w-4 h-4" />
              )}
            </div>
          )}
        </div>
      </div>

      <AnimatePresence>
        {isExpanded && module.competencies && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            className="overflow-hidden"
          >
            <div className="px-3 pb-3 space-y-2 border-t border-gray-200 dark:border-gray-700 pt-3">
              {module.competencies.map((competency) => (
                <CompetencyCard
                  key={competency.id}
                  competency={competency}
                  expandedItems={expandedItems}
                  onToggleExpand={onToggleExpand}
                  onTheoryClick={onTheoryClick}
                  onQuizClick={onQuizClick}
                />
              ))}
              
              {/* Итоговый кейс модуля */}
              {module.hasCaseStudy && module.caseStudyQuizId && (
                <div className="mt-3 p-3 bg-purple-50 dark:bg-purple-900/20 rounded-lg border border-purple-200 dark:border-purple-800">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <Puzzle className="w-4 h-4 text-purple-600 dark:text-purple-400" />
                      <span className="text-sm font-semibold text-gray-900 dark:text-white">
                        Итоговый кейс: {module.caseStudyQuizTitle}
                      </span>
                    </div>
                    <Button
                      variant="primary"
                      size="sm"
                      onClick={() => onQuizClick(module.caseStudyQuizId!)}
                    >
                      <Play className="w-3 h-3" />
                    </Button>
                  </div>
                </div>
              )}
              
              {/* Финальный квиз модуля */}
              {module.hasModuleFinalQuiz && module.moduleFinalQuizId && (
                <div className="mt-2 p-3 bg-red-50 dark:bg-red-900/20 rounded-lg border border-red-200 dark:border-red-800">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <Trophy className="w-4 h-4 text-red-600 dark:text-red-400" />
                      <span className="text-sm font-semibold text-gray-900 dark:text-white">
                        Финальный тест модуля: {module.moduleFinalQuizTitle}
                      </span>
                    </div>
                    <Button
                      variant="primary"
                      size="sm"
                      onClick={() => onQuizClick(module.moduleFinalQuizId!)}
                      className="bg-red-600 hover:bg-red-700"
                    >
                      <Play className="w-3 h-3" />
                    </Button>
                  </div>
                </div>
              )}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

// Карточка компетенции
const CompetencyCard = ({
  competency,
  expandedItems,
  onToggleExpand,
  onTheoryClick,
  onQuizClick,
}: {
  competency: Competency;
  expandedItems: Set<string>;
  onToggleExpand: (key: string) => void;
  onTheoryClick: (id: number) => void;
  onQuizClick: (id: number) => void;
}) => {
  const isExpanded = expandedItems.has(`competency-${competency.id}`);

  return (
    <div className="bg-gray-50 dark:bg-gray-800/50 rounded-lg border border-gray-200 dark:border-gray-700">
      <div
        className="cursor-pointer"
        onClick={() => onToggleExpand(`competency-${competency.id}`)}
      >
        <div className="flex items-center justify-between p-2">
          <div className="flex items-center gap-2">
            <Target className="w-4 h-4 text-green-600 dark:text-green-400" />
            <span className="text-sm font-medium text-gray-900 dark:text-white">{competency.title}</span>
            {competency.topics && (
              <span className="text-xs text-gray-500 dark:text-gray-400">
                ({competency.topics.length} тем)
              </span>
            )}
          </div>
          {competency.topics && competency.topics.length > 0 && (
            <div className="text-gray-500 dark:text-gray-400">
              {isExpanded ? (
                <ChevronDown className="w-3 h-3" />
              ) : (
                <ChevronRight className="w-3 h-3" />
              )}
            </div>
          )}
        </div>
      </div>

      <AnimatePresence>
        {isExpanded && competency.topics && (
          <motion.div
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: 'auto', opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            className="overflow-hidden"
          >
            <div className="px-2 pb-2 space-y-2 border-t border-gray-200 dark:border-gray-700 pt-2">
              {competency.topics.map((topic) => (
                <TopicCard
                  key={topic.id}
                  topic={topic}
                  onTheoryClick={onTheoryClick}
                  onQuizClick={onQuizClick}
                />
              ))}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

// Карточка темы
const TopicCard = ({
  topic,
  onTheoryClick,
  onQuizClick,
}: {
  topic: Topic;
  onTheoryClick: (id: number) => void;
  onQuizClick: (id: number) => void;
}) => {
  return (
    <div className="bg-gray-50 dark:bg-gray-800/50 rounded-lg border border-gray-200 dark:border-gray-700 p-2">
      <div className="flex items-center justify-between mb-2">
        <div className="flex items-center gap-2">
          <BookMarked className="w-4 h-4 text-blue-600 dark:text-blue-400" />
          <span className="text-sm text-gray-900 dark:text-white font-medium">{topic.title}</span>
        </div>
      </div>
      
      <div className="flex items-center gap-2 flex-wrap">
        {topic.hasTheory && topic.theoryId && (
          <Button
            variant="secondary"
            size="sm"
            onClick={() => onTheoryClick(topic.theoryId!)}
            className="text-xs"
          >
            <FileText className="w-3 h-3" />
            Теория
          </Button>
        )}
        {topic.hasPracticeQuiz && topic.practiceQuizId && (
          <Button
            variant="secondary"
            size="sm"
            onClick={() => onQuizClick(topic.practiceQuizId!)}
            className="text-xs"
          >
            <Play className="w-3 h-3" />
            Практика
          </Button>
        )}
      </div>
    </div>
  );
};

// Компонент для отображения списка (старый режим)
const ListView = ({
  quizzes,
  subjectsList,
  searchQuery,
  setSearchQuery,
  subjectFilter,
  setSubjectFilter,
  difficultyFilter,
  setDifficultyFilter,
  isTeacher,
  isAdmin,
  navigate,
  onPublish,
  onUnpublish,
  onDelete,
}: {
  quizzes: SimpleQuiz[];
  subjectsList: Subject[];
  searchQuery: string;
  setSearchQuery: (value: string) => void;
  subjectFilter: string;
  setSubjectFilter: (value: string) => void;
  difficultyFilter: string;
  setDifficultyFilter: (value: string) => void;
  isTeacher: boolean;
  isAdmin: boolean;
  navigate: (path: string) => void;
  onPublish: (quizId: number) => Promise<void>;
  onUnpublish: (quizId: number) => Promise<void>;
  onDelete: (quizId: number) => Promise<void>;
}) => {
  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty.toLowerCase()) {
      case 'easy':
        return 'bg-green-100 text-green-800';
      case 'medium':
        return 'bg-yellow-100 text-yellow-800';
      case 'hard':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getDifficultyLabel = (difficulty: string) => {
    switch (difficulty.toLowerCase()) {
      case 'easy':
        return 'Легко';
      case 'medium':
        return 'Средне';
      case 'hard':
        return 'Сложно';
      default:
        return difficulty;
    }
  };

  return (
    <>
      {/* Поиск и фильтры */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="mb-6"
      >
        <div className="flex flex-col md:flex-row gap-4">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="Поиск по названию или описанию..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-gray-900 dark:text-white placeholder:text-gray-500 dark:placeholder:text-gray-400"
            />
          </div>

          <select
            value={subjectFilter}
            onChange={(e) => setSubjectFilter(e.target.value)}
            className="px-4 py-2 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-gray-900 dark:text-white"
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
            className="px-4 py-2 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent text-gray-900 dark:text-white"
          >
            <option value="">Любая сложность</option>
            <option value="Easy">Легко</option>
            <option value="Medium">Средне</option>
            <option value="Hard">Сложно</option>
          </select>
        </div>
      </motion.div>

      {/* Список квизов */}
      {quizzes.length === 0 ? (
        <Card className="p-12 text-center">
          <BookOpen className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
            Квизы не найдены
          </h3>
          <p className="text-gray-600 dark:text-gray-400">
            {isTeacher || isAdmin
              ? 'Создайте свой первый квиз, чтобы начать'
              : 'Пока нет доступных квизов'}
          </p>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {quizzes.map((quiz, index) => (
            <motion.div
              key={quiz.id}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.1 }}
            >
              <Card className="h-full flex flex-col hover:shadow-lg transition-shadow">
                <div className="flex justify-between items-start mb-3">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white flex-1">
                    {quiz.title}
                  </h3>
                  <span
                    className={`px-2 py-1 rounded-full text-xs font-medium ${getDifficultyColor(
                      quiz.difficulty
                    )}`}
                  >
                    {getDifficultyLabel(quiz.difficulty)}
                  </span>
                </div>

                <p className="text-gray-600 dark:text-gray-400 text-sm mb-4 flex-1">
                  {quiz.description}
                </p>

                <div className="flex items-center gap-2 mb-3">
                  <BookOpen className="w-4 h-4 text-gray-400" />
                  <span className="text-sm text-gray-700 dark:text-gray-300">{quiz.subject}</span>
                </div>

                <div className="grid grid-cols-2 gap-3 mb-4">
                  <div className="flex items-center gap-2">
                    <Target className="w-4 h-4 text-gray-400" />
                    <span className="text-sm text-gray-600 dark:text-gray-400">
                      {quiz.questionCount} вопросов
                    </span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Clock className="w-4 h-4 text-gray-400" />
                    <span className="text-sm text-gray-600 dark:text-gray-400">{quiz.timeLimit} мин</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Award className="w-4 h-4 text-gray-400" />
                    <span className="text-sm text-gray-600 dark:text-gray-400">
                      {quiz.totalPoints} баллов
                    </span>
                  </div>
                </div>

                {/* Кнопки действий */}
                {(isTeacher || isAdmin) ? (
                  <div className="flex flex-col gap-2">
                    <div className="flex gap-2">
                      <Button
                        onClick={() => navigate(`/quizzes/${quiz.id}/edit`)}
                        variant="secondary"
                        className="flex-1 flex items-center justify-center gap-2"
                      >
                        <Edit className="w-4 h-4" />
                        Редактировать
                      </Button>
                      <Button
                        onClick={() => navigate(`/quizzes/${quiz.id}/stats`)}
                        variant="primary"
                        className="flex-1 flex items-center justify-center gap-2"
                      >
                        <TrendingUp className="w-4 h-4" />
                        Статистика
                      </Button>
                    </div>
                    <div className="flex gap-2">
                      {quiz.isPublished ? (
                        <Button
                          variant="secondary"
                          size="sm"
                          className="flex-1 flex items-center justify-center gap-2"
                          onClick={() => onUnpublish(quiz.id)}
                        >
                          <FileX className="w-4 h-4" />
                          Снять с публикации
                        </Button>
                      ) : (
                        <Button
                          variant="primary"
                          size="sm"
                          className="flex-1 flex items-center justify-center gap-2 bg-green-600 hover:bg-green-700"
                          onClick={() => onPublish(quiz.id)}
                        >
                          <Upload className="w-4 h-4" />
                          Опубликовать
                        </Button>
                      )}
                      <Button
                        variant="danger"
                        size="sm"
                        className="px-4 flex items-center justify-center gap-2"
                        onClick={() => onDelete(quiz.id)}
                      >
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </div>
                  </div>
                ) : (
                  <Button
                    onClick={() => navigate(`/quizzes/${quiz.id}/take`)}
                    variant="primary"
                    className="w-full flex items-center justify-center gap-2"
                  >
                    <Play className="w-4 h-4" />
                    Начать квиз
                  </Button>
                )}
              </Card>
            </motion.div>
          ))}
        </div>
      )}
    </>
  );
};

export default QuizzesPage;
