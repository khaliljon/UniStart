import { useAuth } from '../../context/AuthContext';
import AdminDashboard from './AdminDashboard';
import TeacherDashboard from './TeacherDashboard';
import StudentDashboard from './StudentDashboard';

const Dashboard = () => {
  const { isAdmin, isTeacher, loading } = useAuth();

  if (loading) {
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
