import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { ArrowLeft, TrendingUp, BookOpen, Award, Calendar } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';

interface StudentDetail {
  studentId: string;
  email: string;
  userName?: string;
  firstName?: string;
  lastName?: string;
  totalCardsStudied: number;
  totalQuizAttempts?: number;
  totalExamAttempts?: number;
  quizzesTaken: number;
  examsTaken?: number;
  averageQuizScore?: number;
  averageExamScore?: number;
  averageScore?: number;
  bestQuizScore?: number;
  bestExamScore?: number;
}

interface StudentProgress {
  subject: string;
  quizzesTaken: number;
  averageScore: number;
  cardsStudied: number;
}

const StudentDetailPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { isAdmin } = useAuth();
  const { studentId } = useParams<{ studentId: string }>();
  const [student, setStudent] = useState<StudentDetail | null>(null);
  const [progress, setProgress] = useState<StudentProgress[]>([]);
  const [loading, setLoading] = useState(true);
  
  // Определяем путь назад на основе текущего URL
  const backPath = location.pathname.includes('/admin/') ? '/admin/students' : '/teacher/students';

  useEffect(() => {
    loadStudentData();
  }, [studentId]);

  const loadStudentData = async () => {
    try {
      // Используем разные эндпоинты для админа и учителя
      const endpoint = isAdmin 
        ? `/admin/students/${studentId}/stats`
        : `/teacher/students/${studentId}/stats`;
      
      const response = await api.get(endpoint);
      const studentData = response.data;

      setStudent({
        studentId: studentData.StudentId || studentData.studentId,
        email: studentData.Email || studentData.email,
        userName: studentData.UserName || studentData.userName,
        firstName: studentData.FirstName || studentData.firstName,
        lastName: studentData.LastName || studentData.lastName,
        totalCardsStudied: studentData.TotalCardsStudied || studentData.totalCardsStudied || 0,
        totalQuizAttempts: studentData.TotalQuizAttempts || studentData.totalQuizAttempts || 0,
        totalExamAttempts: studentData.TotalExamAttempts || studentData.totalExamAttempts || 0,
        quizzesTaken: studentData.QuizzesTaken || studentData.quizzesTaken || 0,
        examsTaken: studentData.ExamsTaken || studentData.examsTaken || 0,
        averageQuizScore: studentData.AverageQuizScore || studentData.averageQuizScore || 0,
        averageExamScore: studentData.AverageExamScore || studentData.averageExamScore || 0,
        averageScore: studentData.AverageScore || studentData.averageScore || 0,
        bestQuizScore: studentData.BestQuizScore || studentData.bestQuizScore || 0,
        bestExamScore: studentData.BestExamScore || studentData.bestExamScore || 0,
      });
      
      // Прогресс по предметам можно будет добавить позже, пока оставляем пустым
      setProgress([]);
    } catch (error) {
      console.error('Ошибка загрузки данных студента:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600">Загрузка...</div>
      </div>
    );
  }

  if (!student) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <p className="text-xl text-gray-600 mb-4">Студент не найден</p>
          <Button onClick={() => navigate(backPath)}>
            Вернуться к списку
          </Button>
        </div>
      </div>
    );
  }

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
            onClick={() => navigate(backPath)}
            className="mb-4 flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Назад к списку студентов
          </Button>

          <div className="flex items-center gap-4">
            <div className="flex-shrink-0 h-16 w-16 bg-primary-500 rounded-full flex items-center justify-center text-white text-2xl font-semibold">
              {(student.firstName || student.userName || student.email)?.charAt(0).toUpperCase() || 'S'}
            </div>
            <div>
              <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-1">
                {student.firstName && student.lastName 
                  ? `${student.firstName} ${student.lastName}`
                  : student.userName || student.email}
              </h1>
              <p className="text-gray-600 dark:text-gray-400">{student.email}</p>
            </div>
          </div>
        </motion.div>

        {/* Общая статистика */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <Card className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Изучено карточек</p>
                <p className="text-3xl font-bold text-gray-900 dark:text-white">
                  {student.totalCardsStudied || 0}
                </p>
              </div>
              <div className="bg-green-500 p-4 rounded-lg">
                <BookOpen className="w-8 h-8 text-white" />
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Пройдено квизов</p>
                <p className="text-3xl font-bold text-gray-900 dark:text-white">
                  {student.quizzesTaken || 0}
                </p>
              </div>
              <div className="bg-blue-500 p-4 rounded-lg">
                <Award className="w-8 h-8 text-white" />
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Пройдено экзаменов</p>
                <p className="text-3xl font-bold text-gray-900 dark:text-white">
                  {student.examsTaken || 0}
                </p>
              </div>
              <div className="bg-purple-500 p-4 rounded-lg">
                <Award className="w-8 h-8 text-white" />
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Средний балл</p>
                <p className="text-3xl font-bold text-gray-900 dark:text-white">
                  {(student.averageScore || 0).toFixed(1)}%
                </p>
              </div>
              <div className="bg-orange-500 p-4 rounded-lg">
                <TrendingUp className="w-8 h-8 text-white" />
              </div>
            </div>
          </Card>
        </div>

        {/* Детальная статистика */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <Card className="p-6">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Статистика по квизам</h3>
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Попыток:</span>
                <span className="font-medium text-gray-900 dark:text-white">{student.totalQuizAttempts || 0}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Средний балл:</span>
                <span className="font-medium text-gray-900 dark:text-white">{(student.averageQuizScore || 0).toFixed(1)}%</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Лучший результат:</span>
                <span className="font-medium text-gray-900 dark:text-white">{(student.bestQuizScore || 0).toFixed(1)}%</span>
              </div>
            </div>
          </Card>

          <Card className="p-6">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Статистика по экзаменам</h3>
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Попыток:</span>
                <span className="font-medium text-gray-900 dark:text-white">{student.totalExamAttempts || 0}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Средний балл:</span>
                <span className="font-medium text-gray-900 dark:text-white">{(student.averageExamScore || 0).toFixed(1)}%</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Лучший результат:</span>
                <span className="font-medium text-gray-900 dark:text-white">{(student.bestExamScore || 0).toFixed(1)}%</span>
              </div>
            </div>
          </Card>
        </div>

        {/* Прогресс по предметам */}
        <Card className="p-6 mb-6">
          <h2 className="text-2xl font-bold text-gray-900 mb-6 flex items-center gap-2">
            <Calendar className="w-6 h-6 text-primary-500" />
            Прогресс по предметам
          </h2>

          {progress.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-gray-600">
                Студент еще не начал изучение материалов
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              {progress.map((item, index) => (
                <motion.div
                  key={index}
                  initial={{ opacity: 0, x: -20 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: index * 0.1 }}
                  className="border border-gray-200 rounded-lg p-4"
                >
                  <div className="flex items-center justify-between mb-3">
                    <h3 className="text-lg font-semibold text-gray-900">
                      {item.subject}
                    </h3>
                    <span className="text-2xl font-bold text-primary-500">
                      {Math.round(item.averageScore)}%
                    </span>
                  </div>

                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div className="flex items-center gap-2">
                      <Award className="w-4 h-4 text-blue-500" />
                      <span className="text-gray-600 dark:text-gray-400">
                        Квизов: <span className="font-medium text-gray-900 dark:text-white">{item.quizzesTaken}</span>
                      </span>
                    </div>
                    <div className="flex items-center gap-2">
                      <BookOpen className="w-4 h-4 text-green-500" />
                      <span className="text-gray-600">
                        Карточек: <span className="font-medium text-gray-900">{item.cardsStudied}</span>
                      </span>
                    </div>
                  </div>

                  <div className="mt-3">
                    <div className="w-full bg-gray-200 rounded-full h-2">
                      <div
                        className="bg-primary-500 h-2 rounded-full transition-all duration-500"
                        style={{ width: `${item.averageScore}%` }}
                      />
                    </div>
                  </div>
                </motion.div>
              ))}
            </div>
          )}
        </Card>
      </div>
    </div>
  );
};

export default StudentDetailPage;
