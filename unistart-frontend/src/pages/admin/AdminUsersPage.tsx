import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Users, Trash2, Lock, Unlock, ArrowLeft, UserPlus } from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import api from '../../services/api';

interface AdminUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  lockoutEnd: string | null;
  
  // Обновленные поля статистики
  completedFlashcardSets?: number;
  reviewedCards?: number;
  masteredCards?: number;
  totalQuizzesTaken?: number;
  totalQuizAttempts?: number;
  averageScore?: number;
  totalExamsTaken?: number;
  lastActivityDate?: string;
}

const AdminUsersPage = () => {
  const navigate = useNavigate();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterRole, setFilterRole] = useState<string>('');
  const [showRoleModal, setShowRoleModal] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const [selectedRole, setSelectedRole] = useState<string>('Student');

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      const response = await api.get('/admin/users');
      console.log('Users response:', response.data);
      
      // API может вернуть массив или объект с массивом users
      const usersArray = Array.isArray(response.data) 
        ? response.data 
        : (response.data.users || response.data.Users || []);
      
      setUsers(usersArray);
    } catch (error) {
      console.error('Ошибка загрузки пользователей:', error);
      alert('Ошибка загрузки пользователей');
    } finally {
      setLoading(false);
    }
  };

  const toggleUserLockout = async (userId: string, isLocked: boolean) => {
    if (!confirm(`Вы уверены, что хотите ${isLocked ? 'разблокировать' : 'заблокировать'} пользователя?`)) {
      return;
    }

    try {
      // Отправляем isLocked = !isLocked (инвертируем текущее состояние)
      await api.post(`/admin/users/${userId}/lockout`, { 
        isLocked: !isLocked 
      });
      
      loadUsers();
      alert(`Пользователь успешно ${isLocked ? 'разблокирован' : 'заблокирован'}`);
    } catch (error: any) {
      console.error('Ошибка блокировки пользователя:', error);
      const errorMessage = error.response?.data?.message || error.response?.data?.Message || 'Ошибка выполнения операции';
      alert(errorMessage);
    }
  };

  const deleteUser = async (userId: string) => {
    if (!confirm('Вы уверены, что хотите удалить этого пользователя? Это действие необратимо!')) {
      return;
    }

    try {
      await api.delete(`/admin/users/${userId}`);
      loadUsers();
      alert('Пользователь успешно удален');
    } catch (error) {
      console.error('Ошибка удаления пользователя:', error);
      alert('Ошибка удаления пользователя');
    }
  };

  const changeUserRole = async (userId: string, role: string, action: 'add' | 'remove') => {
    try {
      if (action === 'add') {
        await api.post(`/admin/users/${userId}/role`, { roleName: role });
        alert(`Роль "${role}" успешно добавлена`);
      } else {
        await api.delete(`/admin/users/${userId}/role`, { data: { roleName: role } });
        alert(`Роль "${role}" успешно удалена`);
      }
      loadUsers();
    } catch (error: any) {
      console.error('Ошибка изменения роли:', error);
      const errorMessage = error.response?.data?.message || error.response?.data?.Message || 'Ошибка изменения роли';
      alert(errorMessage);
    }
  };

  const filteredUsers = Array.isArray(users) ? users.filter((user) => {
    const matchesSearch =
      user.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      `${user.firstName} ${user.lastName}`.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesRole = !filterRole || user.roles?.includes(filterRole);

    return matchesSearch && matchesRole;
  }) : [];

  const getRoleBadgeColor = (role: string) => {
    switch (role) {
      case 'Admin':
        return 'bg-red-100 text-red-800';
      case 'Teacher':
        return 'bg-blue-100 text-blue-800';
      case 'Student':
        return 'bg-green-100 text-green-800';
      default:
        return 'bg-gray-100 text-gray-800';
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
                👥 Управление пользователями
              </h1>
              <p className="text-gray-600">
                Всего пользователей: {users.length}
              </p>
            </div>
          </div>
        </motion.div>

        {/* Фильтры */}
        <Card className="p-6 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Поиск
              </label>
              <input
                type="text"
                placeholder="Поиск по имени или email..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Фильтр по роли
              </label>
              <select
                value={filterRole}
                onChange={(e) => setFilterRole(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Все роли</option>
                <option value="Admin">Администраторы</option>
                <option value="Teacher">Преподаватели</option>
                <option value="Student">Студенты</option>
              </select>
            </div>
          </div>
        </Card>

        {/* Список пользователей */}
        <Card className="p-6">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50 dark:bg-gray-700">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Пользователь
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Роли
                  </th>
                  <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Активность
                  </th>
                  <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Статус
                  </th>
                  <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Действия
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {filteredUsers.map((user) => {
                  const isLocked = user.lockoutEnd && new Date(user.lockoutEnd) > new Date();
                  
                  return (
                    <motion.tr
                      key={user.id}
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      className="hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                    >
                      <td className="px-6 py-4">
                        <div className="flex items-center">
                          <div className="flex-shrink-0 h-10 w-10 bg-primary-500 rounded-full flex items-center justify-center text-white font-semibold">
                            {user.firstName?.charAt(0) || user.email.charAt(0).toUpperCase()}
                          </div>
                          <div className="ml-4">
                            <div className="text-sm font-medium text-gray-900 dark:text-white">
                              {user.firstName} {user.lastName}
                            </div>
                            <div className="text-sm text-gray-500 dark:text-gray-400">{user.email}</div>
                          </div>
                        </div>
                      </td>

                      <td className="px-6 py-4">
                        <div className="flex flex-wrap gap-2">
                          {user.roles?.map((role) => (
                            <button
                              key={role}
                              onClick={() => {
                                if (confirm(`Удалить роль "${role}" у пользователя?`)) {
                                  changeUserRole(user.id, role, 'remove');
                                }
                              }}
                              className={`px-2 py-1 text-xs font-medium rounded-full ${getRoleBadgeColor(role)} hover:opacity-70 transition-opacity cursor-pointer`}
                              title="Нажмите, чтобы удалить роль"
                            >
                              {role} ×
                            </button>
                          ))}
                        </div>
                      </td>

                      <td className="px-6 py-4 text-center">
                        <div className="text-sm text-gray-900 dark:text-gray-100">
                          Квизы: {user.totalQuizzesTaken || 0}
                        </div>
                        <div className="text-sm text-gray-900 dark:text-gray-100">
                          Экзамены: {user.totalExamsTaken || 0}
                        </div>
                        <div className="text-sm text-gray-500 dark:text-gray-400" title="Освоено / Просмотрено карточек">
                          Карточки: {user.masteredCards || 0} / {user.reviewedCards || 0}
                        </div>
                      </td>

                      <td className="px-6 py-4 text-center">
                        {isLocked ? (
                          <span className="px-2 py-1 text-xs font-medium rounded-full bg-red-100 text-red-800">
                            Заблокирован
                          </span>
                        ) : (
                          <span className="px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800">
                            Активен
                          </span>
                        )}
                      </td>

                      <td className="px-6 py-4">
                        <div className="flex items-center justify-center gap-2">
                          <Button
                            variant="secondary"
                            size="sm"
                            onClick={() => toggleUserLockout(user.id, !!isLocked)}
                            title={isLocked ? 'Разблокировать' : 'Заблокировать'}
                          >
                            {isLocked ? (
                              <Unlock className="w-4 h-4" />
                            ) : (
                              <Lock className="w-4 h-4" />
                            )}
                          </Button>

                          <Button
                            variant="secondary"
                            size="sm"
                            onClick={() => {
                              setSelectedUserId(user.id);
                              setShowRoleModal(true);
                            }}
                            title="Добавить роль"
                          >
                            <UserPlus className="w-4 h-4" />
                          </Button>

                          <Button
                            variant="danger"
                            size="sm"
                            onClick={() => deleteUser(user.id)}
                            title="Удалить пользователя"
                          >
                            <Trash2 className="w-4 h-4" />
                          </Button>
                        </div>
                      </td>
                    </motion.tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {filteredUsers.length === 0 && (
            <div className="text-center py-12">
              <Users className="w-16 h-16 text-gray-400 mx-auto mb-4" />
              <p className="text-gray-600">
                {searchTerm || filterRole
                  ? 'Пользователи не найдены'
                  : 'Нет зарегистрированных пользователей'}
              </p>
            </div>
          )}
        </Card>
      </div>

      {/* Модальное окно для выбора роли */}
      {showRoleModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            animate={{ opacity: 1, scale: 1 }}
            className="bg-white rounded-lg shadow-xl p-6 max-w-md w-full mx-4"
          >
            <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-4">
              Добавить роль пользователю
            </h3>

            <div className="mb-6">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Выберите роль:
              </label>
              <select
                value={selectedRole}
                onChange={(e) => setSelectedRole(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="Student">Student (Студент)</option>
                <option value="Teacher">Teacher (Преподаватель)</option>
                <option value="Admin">Admin (Администратор)</option>
              </select>
            </div>

            <div className="flex gap-3 justify-end">
              <Button
                variant="secondary"
                onClick={() => {
                  setShowRoleModal(false);
                  setSelectedUserId(null);
                }}
              >
                Отмена
              </Button>
              <Button
                variant="primary"
                onClick={() => {
                  if (selectedUserId) {
                    changeUserRole(selectedUserId, selectedRole, 'add');
                    setShowRoleModal(false);
                    setSelectedUserId(null);
                  }
                }}
              >
                Добавить
              </Button>
            </div>
          </motion.div>
        </div>
      )}
    </div>
  );
};

export default AdminUsersPage;
