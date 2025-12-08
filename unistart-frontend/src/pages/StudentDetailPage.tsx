import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, TrendingUp, BookOpen, Award, Calendar } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface StudentDetail {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  totalCardsStudied: number;
  totalQuizzesTaken: number;
}

interface StudentProgress {
  subject: string;
  quizzesTaken: number;
  averageScore: number;
  cardsStudied: number;
}

const StudentDetailPage = () => {
  const navigate = useNavigate();
  const { studentId } = useParams<{ studentId: string }>();
  const [student, setStudent] = useState<StudentDetail | null>(null);
  const [progress, setProgress] = useState<StudentProgress[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadStudentData();
  }, [studentId]);

  const loadStudentData = async () => {
    try {
      const [studentData, progressData] = await Promise.all([
        api.get(`/teacher/students/${studentId}/stats`),
        api.get(`/student/progress-by-subject`), // Это нужно адаптировать для учителя
      ]);

      setStudent(studentData.data);
      setProgress(progressData.data || []);
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
          <Button onClick={() => navigate('/teacher/students')}>
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
            onClick={() => navigate('/teacher/students')}
            className="mb-4 flex items-center gap-2"
          >
            <ArrowLeft className="w-4 h-4" />
            Назад к списку студентов
          </Button>

          <div className="flex items-center gap-4">
            <div className="flex-shrink-0 h-16 w-16 bg-primary-500 rounded-full flex items-center justify-center text-white text-2xl font-semibold">
              {student.firstName?.charAt(0) || 'S'}
            </div>
            <div>
              <h1 className="text-4xl font-bold text-gray-900 mb-1">
                {student.firstName} {student.lastName}
              </h1>
              <p className="text-gray-600">{student.email}</p>
            </div>
          </div>
        </motion.div>

        {/* Общая статистика */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <Card className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Изучено карточек</p>
                <p className="text-3xl font-bold text-gray-900">
                  {student.totalCardsStudied}
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
                <p className="text-sm text-gray-600 mb-1">Пройдено тестов</p>
                <p className="text-3xl font-bold text-gray-900">
                  {student.totalQuizzesTaken}
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
                <p className="text-sm text-gray-600 mb-1">Средний балл</p>
                <p className="text-3xl font-bold text-gray-900">
                  {progress.length > 0
                    ? Math.round(
                        progress.reduce((acc, p) => acc + p.averageScore, 0) /
                          progress.length
                      )
                    : 0}
                  %
                </p>
              </div>
              <div className="bg-orange-500 p-4 rounded-lg">
                <TrendingUp className="w-8 h-8 text-white" />
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
                      <span className="text-gray-600">
                        Тестов: <span className="font-medium text-gray-900">{item.quizzesTaken}</span>
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
