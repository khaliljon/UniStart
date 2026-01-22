import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import preferencesService from '../../services/preferencesService';
import AdminDashboard from './AdminDashboard';
import TeacherDashboard from './TeacherDashboard';
import StudentDashboard from './StudentDashboard';

const Dashboard = () => {
  const { isAdmin, isTeacher, loading } = useAuth();
  const navigate = useNavigate();
  const [checkingOnboarding, setCheckingOnboarding] = useState(true);

  useEffect(() => {
    const checkOnboarding = async () => {
      // Проверяем onboarding только для студентов (не для админов и учителей)
      if (!isAdmin && !isTeacher && !loading) {
        try {
          const status = await preferencesService.checkOnboardingStatus();
          if (!status.onboardingCompleted) {
            navigate('/onboarding');
          }
        } catch (error) {
          console.error('Failed to check onboarding status:', error);
          // Если ошибка (например, предпочтений вообще нет), отправляем на onboarding
          navigate('/onboarding');
        }
      }
      setCheckingOnboarding(false);
    };

    if (!loading) {
      checkOnboarding();
    }
  }, [isAdmin, isTeacher, loading, navigate]);

  if (loading || checkingOnboarding) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-xl text-gray-600 dark:text-gray-400">Загрузка...</div>
      </div>
    );
  }

  // Админ имеет доступ ко всем дашбордам, показываем админский
  if (isAdmin) {
    return <AdminDashboard />;
  }

  // Учитель видит дашборд преподавателя
  if (isTeacher) {
    return <TeacherDashboard />;
  }

  // Студент (или пользователь без роли) видит студенческий дашборд
  return <StudentDashboard />;
};

export default Dashboard;
