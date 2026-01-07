import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Users, TrendingUp, BookOpen, Award, ArrowLeft, CheckCircle } from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import { useAuth } from '../../context/AuthContext';
import { StudentStats } from '../../types';
import api from '../../services/api';

interface DashboardStats {
  totalStudents: number;
  activeToday: number;
  averageProgress: number;
}

const TeacherStudentsPage = () => {
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  const [students, setStudents] = useState<StudentStats[]>([]);
  const [stats, setStats] = useState<DashboardStats>({
    totalStudents: 0,
    activeToday: 0,
    averageProgress: 0,
  });
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    loadStudents();
  }, []);

  const loadStudents = async () => {
    console.log('🔍 Начинаем загрузку студентов...');
    try {
      // Для админа используем другой эндпоинт
      const endpoint = isAdmin ? '/admin/users?role=Student&pageSize=1000' : '/teacher/students';
      console.log('📡 Используем эндпоинт:', endpoint);
      
      const response = await api.get(endpoint);
      console.log('✅ Ответ от API:', response.data);
      
      let studentsData;
      
      if (isAdmin) {
        // Для админа API возвращает объект с полем Users
        const usersArray = response.data.Users || response.data.users || [];
        console.log('👥 Массив пользователей:', usersArray);
        
        // Преобразуем формат данных админа в формат студентов
        studentsData = usersArray.map((user: any): StudentStats => {
          // Обрабатываем данные с учетом camelCase и PascalCase
          const totalQuizAttempts = user.TotalQuizAttempts || user.totalQuizAttempts || 0;
          const totalQuizzesTaken = user.TotalQuizzesTaken || user.totalQuizzesTaken || 0;
          const averageScore = user.AverageScore || user.averageScore || 0;
          const totalExamsTaken = user.TotalExamsTaken || user.totalExamsTaken || 0;
          const lastActivityDate = user.LastActivityDate || user.lastActivityDate || user.LastLoginAt || user.lastLoginAt || user.CreatedAt || user.createdAt || '';
          
          // НОВОЕ: используем обновленные поля для карточек
          const completedFlashcardSets = user.CompletedFlashcardSets || user.completedFlashcardSets || 0;
          const reviewedCards = user.ReviewedCards || user.reviewedCards || 0;
          const masteredCards = user.MasteredCards || user.masteredCards || 0;
          
          console.log('📊 Обработка пользователя:', {
            email: user.Email || user.email,
            totalQuizAttempts,
            totalQuizzesTaken,
            averageScore,
            totalExamsTaken,
            completedFlashcardSets,
            reviewedCards,
            masteredCards,
            lastActivityDate,
            rawUser: user
          });
          
          return {
            userId: user.Id || user.id,
            email: user.Email || user.email,
            userName: user.UserName || user.userName || `${user.FirstName || user.firstName || ''} ${user.LastName || user.lastName || ''}`.trim() || user.Email || user.email,
            firstName: user.FirstName || user.firstName,
            lastName: user.LastName || user.lastName,
            totalAttempts: totalQuizAttempts,
            averageScore: averageScore,
            quizzesTaken: totalQuizzesTaken,
            examsTaken: totalExamsTaken,
            lastAttemptDate: lastActivityDate,
            lastActivityDate: lastActivityDate,
            
            // Новые поля для карточек
            completedFlashcardSets,
            reviewedCards,
            masteredCards,
          };
        });
      } else {
        // Для учителя API возвращает объект с полем Students
        const studentsArray = response.data.Students || response.data.students || [];
        studentsData = studentsArray.map((s: any): StudentStats => ({
          userId: s.UserId || s.userId,
          email: s.Email || s.email,
          userName: s.UserName || s.userName || s.email,
          firstName: s.FirstName || s.firstName,
          lastName: s.LastName || s.lastName,
          totalAttempts: s.TotalAttempts || s.totalAttempts || 0,
          averageScore: s.AverageScore || s.averageScore || 0,
          bestScore: s.BestScore || s.bestScore || 0,
          lastAttemptDate: s.LastAttemptDate || s.lastAttemptDate || '',
          lastActivityDate: s.LastActivityDate || s.lastActivityDate || s.LastAttemptDate || s.lastAttemptDate || '',
          quizzesTaken: s.QuizzesTaken || s.quizzesTaken || 0,
          examsTaken: s.ExamsTaken || s.examsTaken || 0,
          
          // Новые поля для карточек
          completedFlashcardSets: s.CompletedFlashcardSets || s.completedFlashcardSets || 0,
          reviewedCards: s.ReviewedCards || s.reviewedCards || s.CardsStudied || s.cardsStudied || 0,
          masteredCards: s.MasteredCards || s.masteredCards || 0,
        }));
      }
      
      console.log('✅ Обработанные студенты:', studentsData);
      
      setStudents(studentsData);
      
      // Подсчет статистики
      const today = new Date();
      today.setUTCHours(0, 0, 0, 0); // Используем UTC для сравнения
      
      const activeTodayCount = studentsData.filter((s: StudentStats) => {
        const dateToCheck = s.lastActivityDate || s.lastAttemptDate;
        if (!dateToCheck) return false;
        try {
          const lastDate = new Date(dateToCheck);
          const lastDateUTC = new Date(Date.UTC(
            lastDate.getUTCFullYear(),
            lastDate.getUTCMonth(),
            lastDate.getUTCDate()
          ));
          const todayUTC = new Date(Date.UTC(
            today.getUTCFullYear(),
            today.getUTCMonth(),
            today.getUTCDate()
          ));
          return lastDateUTC >= todayUTC;
        } catch (e) {
          console.error('Ошибка парсинга даты:', dateToCheck, e);
          return false;
        }
      }).length;
      
      // Считаем средний прогресс только из студентов с активностью
      const studentsWithActivity = studentsData.filter((s: StudentStats) => 
        (s.averageScore || 0) > 0 || 
        (s.quizzesTaken || 0) > 0 || 
        (s.examsTaken || 0) > 0 || 
        (s.reviewedCards || 0) > 0
      );
      
      const avgProgress = studentsWithActivity.length > 0
        ? studentsWithActivity.reduce((acc: number, s: StudentStats) => acc + (s.averageScore || 0), 0) / studentsWithActivity.length
        : 0;
      
      setStats({
        totalStudents: studentsData.length,
        activeToday: activeTodayCount,
        averageProgress: Math.round(avgProgress * 100) / 100,
      });
    } catch (error) {
      console.error('❌ Ошибка загрузки студентов:', error);
    } finally {
      setLoading(false);
    }
  };

  const filteredStudents = students.filter(
    (student) =>
      student.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      student.userName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const viewStudentDetails = (studentId: string) => {
    const basePath = isAdmin ? '/admin/students' : '/teacher/students';
    navigate(`${basePath}/${studentId}/stats`);
  };

  if (loading) {
    console.log('⏳ Состояние загрузки...');
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка...</div>
      </div>
    );
  }

  console.log('🎨 Рендеринг страницы. Студентов:', students.length);

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-7xl mx-auto">
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
            👥 {isAdmin ? 'Студенты' : 'Мои студенты'}
          </h1>
          <p className="text-gray-600">
            {isAdmin
              ? 'Все студенты платформы и их прогресс'
              : 'Отслеживайте прогресс и достижения ваших студентов'}
          </p>
        </motion.div>

        {/* Статистика */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <Card className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Всего студентов</p>
                <p className="text-3xl font-bold text-gray-900">
                  {stats.totalStudents}
                </p>
              </div>
              <div className="bg-blue-500 p-4 rounded-lg">
                <Users className="w-8 h-8 text-white" />
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Активных сегодня</p>
                <p className="text-3xl font-bold text-gray-900">
                  {stats.activeToday}
                </p>
              </div>
              <div className="bg-green-500 p-4 rounded-lg">
                <TrendingUp className="w-8 h-8 text-white" />
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Средний прогресс</p>
                <p className="text-3xl font-bold text-gray-900">
                  {Math.round(stats.averageProgress)}
                </p>
              </div>
              <div className="bg-purple-500 p-4 rounded-lg">
                <BookOpen className="w-8 h-8 text-white" />
              </div>
            </div>
          </Card>
        </div>

        {/* Поиск */}
        <Card className="p-6 mb-6">
          <input
            type="text"
            placeholder="Поиск по имени или email..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
          />
        </Card>

        {/* Список студентов */}
        <Card className="p-6">
          <h2 className="text-2xl font-bold text-gray-900 mb-6">
            Список студентов ({filteredStudents.length})
          </h2>

          {filteredStudents.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-gray-600">
                {searchTerm ? 'Студенты не найдены' : 'У вас пока нет студентов'}
              </p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gray-50 dark:bg-gray-700">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Студент
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Email
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Средний балл
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Квизы
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Экзамены
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Карточки
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Действия
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {filteredStudents.map((student) => (
                    <motion.tr
                      key={student.userId}
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      className="hover:bg-gray-50 transition-colors"
                    >
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center">
                          <div className="flex-shrink-0 h-10 w-10 bg-primary-500 rounded-full flex items-center justify-center text-white font-semibold">
                            {student.userName?.charAt(0).toUpperCase() || student.email?.charAt(0).toUpperCase() || 'S'}
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-gray-900">
                              {student.userName || student.email}
                            </div>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-500">{student.email}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-center">
                        <div className="flex items-center justify-center gap-1">
                          <TrendingUp className="w-4 h-4 text-green-500" />
                          <span className="text-sm font-medium text-gray-900">
                            {(student.averageScore || 0).toFixed(1)}%
                          </span>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-center">
                        <div className="flex items-center justify-center gap-1">
                          <Award className="w-4 h-4 text-blue-500" />
                          <span className="text-sm font-medium text-gray-900">
                            {student.quizzesTaken || 0}
                          </span>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-center">
                        <div className="flex items-center justify-center gap-1">
                          <Award className="w-4 h-4 text-purple-500" />
                          <span className="text-sm font-medium text-gray-900">
                            {student.examsTaken || 0}
                          </span>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-center">
                        <div className="flex flex-col items-center gap-0.5">
                          <div className="flex items-center gap-1">
                            <CheckCircle className="w-4 h-4 text-green-600" />
                            <span className="text-sm font-medium text-gray-900" title="Освоено карточек">
                              {student.masteredCards || 0}
                            </span>
                          </div>
                          <div className="flex items-center gap-1">
                            <BookOpen className="w-3 h-3 text-gray-400" />
                            <span className="text-xs text-gray-500" title="Просмотрено карточек">
                              {student.reviewedCards || 0}
                          </span>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-center">
                        <Button
                          variant="secondary"
                          size="sm"
                          onClick={() => viewStudentDetails(student.userId)}
                        >
                          Подробнее
                        </Button>
                      </td>
                    </motion.tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>
      </div>
    </div>
  );
};

export default TeacherStudentsPage;
