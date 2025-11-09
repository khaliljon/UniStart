import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Users, TrendingUp, BookOpen, Award, ArrowLeft } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import { useAuth } from '../context/AuthContext';

interface Student {
  userId: string;
  email: string;
  userName: string;
  totalAttempts: number;
  averageScore: number;
  averagePercentage: number;
  bestScore: number;
  lastAttemptDate: string;
  quizzesTaken: number;
}

interface StudentStats {
  totalStudents: number;
  activeToday: number;
  averageProgress: number;
}

const TeacherStudentsPage = () => {
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  const [students, setStudents] = useState<Student[]>([]);
  const [stats, setStats] = useState<StudentStats>({
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
      const response = await api.get('/teacher/students');
      console.log('✅ Ответ от API:', response.data);
      
      // API возвращает объект с полем Students
      const studentsData = response.data.students || response.data.Students || [];
      console.log('✅ Студенты:', studentsData);
      
      setStudents(studentsData);
      
      // Подсчет статистики
      setStats({
        totalStudents: studentsData.length,
        activeToday: studentsData.filter((s: Student) => s.quizzesTaken > 0).length,
        averageProgress: studentsData.reduce((acc: number, s: Student) => acc + s.averagePercentage, 0) / studentsData.length || 0,
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
    navigate(`/teacher/students/${studentId}/stats`);
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
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-purple-50 py-8 px-4">
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
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Студент
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Email
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Средний балл
                    </th>
                    <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Пройдено тестов
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
                          <BookOpen className="w-4 h-4 text-green-500" />
                          <span className="text-sm font-medium text-gray-900">
                            {student.averagePercentage.toFixed(1)}%
                          </span>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-center">
                        <div className="flex items-center justify-center gap-1">
                          <Award className="w-4 h-4 text-blue-500" />
                          <span className="text-sm font-medium text-gray-900">
                            {student.quizzesTaken}
                          </span>
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
