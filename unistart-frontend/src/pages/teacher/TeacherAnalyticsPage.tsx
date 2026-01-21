import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  Users,
  FileText,
  Award,
  BookOpen,
  ArrowLeft,
  Target,
  BarChart3,
  TrendingUp,
  Download,
} from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import api from '../../services/api';

interface StudentStats {
  studentId: string;
  studentEmail: string;
  studentUserName: string;
  totalQuizzesTaken: number;
  totalExamsTaken: number;
  averageScore: number;
  averagePercentage: number;
  lastActivityDate: string;
}

interface AnalyticsData {
  students: StudentStats[];
  totalCount: number;
  page: number;
  pageSize: number;
}

const TeacherAnalyticsPage = () => {
  const navigate = useNavigate();
  const [students, setStudents] = useState<StudentStats[]>([]);
  const [totalStudents, setTotalStudents] = useState(0);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const pageSize = 20;

  useEffect(() => {
    loadAnalytics();
  }, [page]);

  const loadAnalytics = async () => {
    try {
      const response = await api.get(`/teacher/analytics/students?page=${page}&pageSize=${pageSize}`);
      const data: AnalyticsData = response.data;
      
      setStudents(data.students || []);
      setTotalStudents(data.totalCount || 0);
    } catch (error) {
      console.error('Ошибка загрузки аналитики:', error);
      setStudents([]);
      setTotalStudents(0);
    } finally {
      setLoading(false);
    }
  };

  const exportData = async () => {
    try {
      const response = await api.get('/teacher/export/analytics', {
        responseType: 'blob',
      });
      
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `analytics-${new Date().toISOString().split('T')[0]}.csv`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (error) {
      console.error('Ошибка экспорта данных:', error);
      alert('Не удалось экспортировать данные');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка аналитики...</div>
      </div>
    );
  }

  const getScoreColor = (percentage: number) => {
    if (percentage >= 80) return 'bg-green-100 text-green-800';
    if (percentage >= 60) return 'bg-yellow-100 text-yellow-800';
    return 'bg-red-100 text-red-800';
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
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
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-2">
                📊 Аналитика и статистика
              </h1>
              <p className="text-gray-600 dark:text-gray-300">
                Детальная информация о ваших квизах и успеваемости студентов
              </p>
            </div>
            <Button
              variant="primary"
              onClick={exportData}
              className="flex items-center gap-2"
            >
              <Download className="w-4 h-4" />
              Экспорт данных
            </Button>
          </div>
        </motion.div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
          >
            <Card className="p-6 bg-gradient-to-br from-blue-500 to-blue-600 text-white">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-blue-100 text-sm mb-1">Студентов</p>
                  <p className="text-3xl font-bold">{totalStudents}</p>
                </div>
                <Users className="w-12 h-12 text-blue-200" />
              </div>
            </Card>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
          >
            <Card className="p-6 bg-gradient-to-br from-green-500 to-green-600 text-white">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-green-100 text-sm mb-1">Средний балл</p>
                  <p className="text-3xl font-bold">
                    {students.length > 0 
                      ? (students.reduce((sum, s) => sum + s.averagePercentage, 0) / students.length).toFixed(1)
                      : 0}%
                  </p>
                </div>
                <Award className="w-12 h-12 text-green-200" />
              </div>
            </Card>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
          >
            <Card className="p-6 bg-gradient-to-br from-purple-500 to-purple-600 text-white">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-purple-100 text-sm mb-1">Активность</p>
                  <p className="text-3xl font-bold">
                    {students.reduce((sum, s) => sum + s.totalQuizzesTaken + s.totalExamsTaken, 0)}
                  </p>
                  <p className="text-purple-100 text-xs mt-1">Всего попыток</p>
                </div>
                <TrendingUp className="w-12 h-12 text-purple-200" />
              </div>
            </Card>
          </motion.div>
        </div>

        {/* Students Table */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
        >
          <Card className="p-6">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
                <BarChart3 className="w-6 h-6 text-primary-500" />
                Студенты
              </h2>
            </div>

            {students.length === 0 ? (
              <div className="text-center py-12">
                <Users className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                <p className="text-gray-500 text-lg mb-2">
                  У вас пока нет студентов
                </p>
                <p className="text-gray-400 mb-6">
                  Создайте контент и поделитесь им со студентами
                </p>
                <div className="flex gap-4 justify-center">
                  <Button onClick={() => navigate('/quizzes/create')}>
                    Создать квиз
                  </Button>
                  <Button variant="secondary" onClick={() => navigate('/exams/create')}>
                    Создать экзамен
                  </Button>
                </div>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gray-50 dark:bg-gray-700">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Студент
                      </th>
                      <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Квизов пройдено
                      </th>
                      <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Экзаменов пройдено
                      </th>
                      <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Средний балл
                      </th>
                      <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Последняя активность
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                    {students.map((student) => (
                      <motion.tr
                        key={student.studentId}
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        className="hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors cursor-pointer"
                        onClick={() => navigate(`/teacher/students/${student.studentId}`)}
                      >
                        <td className="px-6 py-4">
                          <div>
                            <div className="text-sm font-medium text-gray-900 dark:text-white">
                              {student.studentUserName || student.studentEmail}
                            </div>
                            <div className="text-xs text-gray-500 dark:text-gray-400">
                              {student.studentEmail}
                            </div>
                          </div>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <span className="text-sm font-medium text-gray-900 dark:text-white">
                            {student.totalQuizzesTaken}
                          </span>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <span className="text-sm font-medium text-gray-900 dark:text-white">
                            {student.totalExamsTaken}
                          </span>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <span
                            className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-medium ${getScoreColor(
                              student.averagePercentage
                            )}`}
                          >
                            {student.averagePercentage.toFixed(1)}%
                          </span>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <div className="text-sm text-gray-600 dark:text-gray-400">
                            {student.lastActivityDate
                              ? new Date(student.lastActivityDate).toLocaleDateString('ru-RU')
                              : 'Нет данных'}
                          </div>
                        </td>
                      </motion.tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </Card>
        </motion.div>

        {/* Quick Actions */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}
          className="mt-8 grid grid-cols-1 md:grid-cols-3 gap-6"
        >
          <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer" onClick={() => navigate('/teacher/students')}>
            <div className="text-center">
              <Users className="w-12 h-12 text-primary-500 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                Просмотреть всех студентов
              </h3>
              <p className="text-gray-600 dark:text-gray-400 text-sm">
                Просмотр списка студентов и их прогресса
              </p>
            </div>
          </Card>

          <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer" onClick={exportData}>
            <div className="text-center">
              <Download className="w-12 h-12 text-green-500 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                Экспорт данных
              </h3>
              <p className="text-gray-600 dark:text-gray-400 text-sm">
                Скачать результаты в CSV формате
              </p>
            </div>
          </Card>

          <Card className="p-6 hover:shadow-lg transition-shadow cursor-pointer" onClick={() => navigate('/quizzes/create')}>
            <div className="text-center">
              <Target className="w-12 h-12 text-blue-500 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                Создать квиз
              </h3>
              <p className="text-gray-600 dark:text-gray-400 text-sm">
                Добавить новый квиз для студентов
              </p>
            </div>
          </Card>
        </motion.div>
      </div>
    </div>
  );
};

export default TeacherAnalyticsPage;
