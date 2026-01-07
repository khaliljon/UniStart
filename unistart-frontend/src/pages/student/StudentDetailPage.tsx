import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { ArrowLeft, TrendingUp, BookOpen, Award, Calendar, CheckCircle, Target } from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import { useAuth } from '../../context/AuthContext';
import { StudentDetailedStats, FlashcardProgress } from '../../types';
import api from '../../services/api';

const StudentDetailPage = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { isAdmin } = useAuth();
  const { studentId } = useParams<{ studentId: string }>();
  const [student, setStudent] = useState<StudentDetailedStats | null>(null);
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
      const data = response.data;
      
      console.log('📊 Детальные данные студента:', data);

      setStudent({
        userId: data.StudentId || data.studentId,
        email: data.Email || data.email,
        userName: data.UserName || data.userName,
        firstName: data.FirstName || data.firstName,
        lastName: data.LastName || data.lastName,
        
        // Карточки (ОБНОВЛЕНО)
        completedFlashcardSets: data.CompletedFlashcardSets || data.completedFlashcardSets || 0,
        reviewedCards: data.ReviewedCards || data.reviewedCards || 0,
        masteredCards: data.MasteredCards || data.masteredCards || 0,
        
        // Квизы
        totalQuizAttempts: data.TotalQuizAttempts || data.totalQuizAttempts || 0,
        quizzesTaken: data.QuizzesTaken || data.quizzesTaken || 0,
        averageQuizScore: data.AverageQuizScore || data.averageQuizScore || 0,
        bestQuizScore: data.BestQuizScore || data.bestQuizScore || 0,
        totalAttempts: data.TotalQuizAttempts || data.totalQuizAttempts || 0,
        averageScore: data.AverageScore || data.averageScore || 0,
        
        // Экзамены
        totalExamAttempts: data.TotalExamAttempts || data.totalExamAttempts || 0,
        examsTaken: data.ExamsTaken || data.examsTaken || 0,
        averageExamScore: data.AverageExamScore || data.averageExamScore || 0,
        bestExamScore: data.BestExamScore || data.bestExamScore || 0,
        
        // FlashcardProgress (НОВОЕ!)
        flashcardProgress: data.FlashcardProgress || data.flashcardProgress,
        
        // Попытки
        attempts: data.Attempts || data.attempts || [],
        examAttempts: data.ExamAttempts || data.examAttempts || [],
      } as StudentDetailedStats);
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
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Карточек освоено</p>
                <p className="text-3xl font-bold text-gray-900 dark:text-white">
                  {student.masteredCards || 0}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-500 mt-1">
                  Просмотрено: {student.reviewedCards || 0}
                </p>
              </div>
              <div className="bg-green-500 p-4 rounded-lg">
                <CheckCircle className="w-8 h-8 text-white" />
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
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <Card className="p-6">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Статистика по квизам</h3>
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Попыток:</span>
                <span className="font-medium text-gray-900 dark:text-white">{student.totalQuizAttempts || 0}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Уникальных квизов:</span>
                <span className="font-medium text-gray-900 dark:text-white">{student.quizzesTaken || 0}</span>
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
                <span className="text-gray-600 dark:text-gray-400">Уникальных экзаменов:</span>
                <span className="font-medium text-gray-900 dark:text-white">{student.examsTaken || 0}</span>
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

          <Card className="p-6">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Статистика по карточкам</h3>
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Завершено наборов:</span>
                <span className="font-medium text-gray-900 dark:text-white">{student.completedFlashcardSets || 0}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Просмотрено карточек:</span>
                <span className="font-medium text-gray-900 dark:text-white">{student.reviewedCards || 0}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Освоено карточек:</span>
                <span className="font-medium text-green-600 dark:text-green-400">{student.masteredCards || 0}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600 dark:text-gray-400">Процент освоения:</span>
                <span className="font-medium text-gray-900 dark:text-white">
                  {student.reviewedCards > 0 
                    ? ((student.masteredCards / student.reviewedCards) * 100).toFixed(1)
                    : 0}%
                </span>
              </div>
            </div>
          </Card>
        </div>

        {/* Прогресс по наборам карточек */}
        {student.flashcardProgress && (
          <Card className="p-6 mb-6">
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6 flex items-center gap-2">
              <BookOpen className="w-6 h-6 text-green-500" />
              Прогресс по наборам карточек
            </h2>

            {(!student.flashcardProgress.setDetails || student.flashcardProgress.setDetails.length === 0) ? (
              <div className="text-center py-12">
                <p className="text-gray-600 dark:text-gray-400">
                  Студент еще не начал изучение карточек
                </p>
              </div>
            ) : (
              <>
                {/* Общая статистика по карточкам */}
                <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
                  <div className="text-center">
                    <p className="text-sm text-gray-600 dark:text-gray-400">Наборов открыто</p>
                    <p className="text-2xl font-bold text-gray-900 dark:text-white">
                      {student.flashcardProgress.setsAccessed}
                    </p>
                  </div>
                  <div className="text-center">
                    <p className="text-sm text-gray-600 dark:text-gray-400">Наборов завершено</p>
                    <p className="text-2xl font-bold text-green-600">
                      {student.flashcardProgress.setsCompleted}
                    </p>
                  </div>
                  <div className="text-center">
                    <p className="text-sm text-gray-600 dark:text-gray-400">Карточек просмотрено</p>
                    <p className="text-2xl font-bold text-blue-600">
                      {student.flashcardProgress.totalCardsReviewed}
                    </p>
                  </div>
                  <div className="text-center">
                    <p className="text-sm text-gray-600 dark:text-gray-400">Карточек освоено</p>
                    <p className="text-2xl font-bold text-purple-600">
                      {student.flashcardProgress.masteredCards}
                    </p>
                  </div>
                </div>

                {/* Детализация по каждому набору */}
                <div className="space-y-4">
                  {student.flashcardProgress.setDetails.map((setDetail, index) => (
                    <motion.div
                      key={setDetail.setId}
                      initial={{ opacity: 0, x: -20 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: index * 0.05 }}
                      className="border border-gray-200 dark:border-gray-700 rounded-lg p-4 hover:shadow-md transition-shadow"
                    >
                      <div className="flex items-start justify-between mb-3">
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-1">
                            <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                              {setDetail.setTitle}
                            </h3>
                            {setDetail.isCompleted && (
                              <span className="px-2 py-1 bg-green-100 text-green-800 text-xs font-medium rounded-full flex items-center gap-1">
                                <CheckCircle className="w-3 h-3" />
                                Завершен
                              </span>
                            )}
                          </div>
                          <p className="text-sm text-gray-500 dark:text-gray-400">
                            Последний доступ: {new Date(setDetail.lastAccessed).toLocaleDateString('ru-RU')}
                          </p>
                        </div>
                        <div className="text-right">
                          <p className="text-2xl font-bold text-purple-600">
                            {setDetail.masteredCards}/{setDetail.totalCards}
                          </p>
                          <p className="text-xs text-gray-500 dark:text-gray-400">освоено</p>
                        </div>
                      </div>

                      <div className="grid grid-cols-3 gap-4 mb-3">
                        <div>
                          <p className="text-xs text-gray-600 dark:text-gray-400 mb-1">Всего карточек</p>
                          <p className="text-lg font-medium text-gray-900 dark:text-white">{setDetail.totalCards}</p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-600 dark:text-gray-400 mb-1">Просмотрено</p>
                          <p className="text-lg font-medium text-blue-600">{setDetail.reviewedCards}</p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-600 dark:text-gray-400 mb-1">Освоено</p>
                          <p className="text-lg font-medium text-green-600">{setDetail.masteredCards}</p>
                        </div>
                      </div>

                      {/* Прогресс-бар */}
                      <div className="space-y-2">
                        <div className="flex justify-between text-xs text-gray-600 dark:text-gray-400">
                          <span>Прогресс просмотра</span>
                          <span>{setDetail.totalCards > 0 ? ((setDetail.reviewedCards / setDetail.totalCards) * 100).toFixed(0) : 0}%</span>
                        </div>
                        <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                          <div
                            className="bg-blue-500 h-2 rounded-full transition-all duration-500"
                            style={{ 
                              width: `${setDetail.totalCards > 0 ? (setDetail.reviewedCards / setDetail.totalCards) * 100 : 0}%` 
                            }}
                          />
                        </div>

                        <div className="flex justify-between text-xs text-gray-600 dark:text-gray-400 mt-2">
                          <span>Прогресс освоения</span>
                          <span>{setDetail.totalCards > 0 ? ((setDetail.masteredCards / setDetail.totalCards) * 100).toFixed(0) : 0}%</span>
                        </div>
                        <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                          <div
                            className="bg-green-500 h-2 rounded-full transition-all duration-500"
                            style={{ 
                              width: `${setDetail.totalCards > 0 ? (setDetail.masteredCards / setDetail.totalCards) * 100 : 0}%` 
                            }}
                          />
                        </div>
                      </div>
                    </motion.div>
                  ))}
                </div>
              </>
            )}
          </Card>
        )}
      </div>
    </div>
  );
};

export default StudentDetailPage;
