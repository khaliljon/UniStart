import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { Award, ArrowLeft, Plus, Trash2, Edit2, Save, X } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface Achievement {
  id: string;
  name: string;
  description: string;
  iconName: string;
  category: string;
  requiredCount: number;
  createdAt: string;
}

interface AchievementForm {
  name: string;
  description: string;
  iconName: string;
  category: string;
  requiredCount: number;
}

const AdminAchievementsPage = () => {
  const navigate = useNavigate();
  const [achievements, setAchievements] = useState<Achievement[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState<AchievementForm>({
    name: '',
    description: '',
    iconName: '🏆',
    category: 'Quiz',
    requiredCount: 1,
  });

  const categories = [
    'Quiz',
    'Flashcard',
    'Learning',
    'Social',
    'Streak',
    'Milestone',
  ];

  const icons = ['🏆', '⭐', '🎯', '🔥', '💎', '👑', '🎓', '📚', '✨', '🌟', '🏅', '🎖️'];

  useEffect(() => {
    loadAchievements();
  }, []);

  const loadAchievements = async () => {
    try {
      const response = await api.get('/admin/achievements');
      console.log('Achievements response:', response.data);
      
      const achievementsArray = Array.isArray(response.data)
        ? response.data
        : (response.data.achievements || response.data.Achievements || []);
      
      setAchievements(achievementsArray);
    } catch (error) {
      console.error('Ошибка загрузки достижений:', error);
      // Моковые данные для демонстрации
      setAchievements([
        {
          id: '1',
          name: 'Первый шаг',
          description: 'Пройдите первый тест',
          iconName: '🎯',
          category: 'Quiz',
          requiredCount: 1,
          createdAt: new Date().toISOString(),
        },
        {
          id: '2',
          name: 'Книжный червь',
          description: 'Изучите 100 карточек',
          iconName: '📚',
          category: 'Flashcard',
          requiredCount: 100,
          createdAt: new Date().toISOString(),
        },
        {
          id: '3',
          name: 'Огненная серия',
          description: 'Поддерживайте серию 7 дней',
          iconName: '🔥',
          category: 'Streak',
          requiredCount: 7,
          createdAt: new Date().toISOString(),
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingId) {
        await api.put(`/admin/achievements/${editingId}`, formData);
        alert('Достижение успешно обновлено');
      } else {
        await api.post('/admin/achievements', formData);
        alert('Достижение успешно создано');
      }
      
      loadAchievements();
      resetForm();
    } catch (error) {
      console.error('Ошибка сохранения достижения:', error);
      alert('Ошибка при сохранении достижения');
    }
  };

  const handleEdit = (achievement: Achievement) => {
    setFormData({
      name: achievement.name,
      description: achievement.description,
      iconName: achievement.iconName,
      category: achievement.category,
      requiredCount: achievement.requiredCount,
    });
    setEditingId(achievement.id);
    setShowCreateForm(true);
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Вы уверены, что хотите удалить это достижение?')) {
      return;
    }

    try {
      await api.delete(`/admin/achievements/${id}`);
      loadAchievements();
      alert('Достижение успешно удалено');
    } catch (error) {
      console.error('Ошибка удаления достижения:', error);
      alert('Ошибка при удалении достижения');
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      iconName: '🏆',
      category: 'Quiz',
      requiredCount: 1,
    });
    setEditingId(null);
    setShowCreateForm(false);
  };

  const getCategoryColor = (category: string) => {
    switch (category) {
      case 'Quiz':
        return 'bg-blue-100 text-blue-800';
      case 'Flashcard':
        return 'bg-purple-100 text-purple-800';
      case 'Learning':
        return 'bg-green-100 text-green-800';
      case 'Social':
        return 'bg-pink-100 text-pink-800';
      case 'Streak':
        return 'bg-orange-100 text-orange-800';
      case 'Milestone':
        return 'bg-yellow-100 text-yellow-800';
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
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-purple-50 py-8 px-4">
      <div className="max-w-6xl mx-auto">
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
              <h1 className="text-4xl font-bold text-gray-900 mb-2">
                🏆 Управление достижениями
              </h1>
              <p className="text-gray-600">
                Всего достижений: {achievements.length}
              </p>
            </div>

            {!showCreateForm && (
              <Button
                variant="primary"
                onClick={() => setShowCreateForm(true)}
                className="flex items-center gap-2"
              >
                <Plus className="w-4 h-4" />
                Создать достижение
              </Button>
            )}
          </div>
        </motion.div>

        {/* Форма создания/редактирования */}
        {showCreateForm && (
          <motion.div
            initial={{ opacity: 0, y: -20 }}
            animate={{ opacity: 1, y: 0 }}
            className="mb-8"
          >
            <Card className="p-6">
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-2xl font-bold text-gray-900">
                  {editingId ? 'Редактировать достижение' : 'Создать достижение'}
                </h2>
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={resetForm}
                >
                  <X className="w-4 h-4" />
                </Button>
              </div>

              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Название *
                    </label>
                    <input
                      type="text"
                      required
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                      placeholder="Первый шаг"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Категория *
                    </label>
                    <select
                      required
                      value={formData.category}
                      onChange={(e) => setFormData({ ...formData, category: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    >
                      {categories.map((cat) => (
                        <option key={cat} value={cat}>
                          {cat}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Описание *
                  </label>
                  <textarea
                    required
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    rows={3}
                    placeholder="Пройдите первый тест"
                  />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Иконка *
                    </label>
                    <div className="flex flex-wrap gap-2">
                      {icons.map((icon) => (
                        <button
                          key={icon}
                          type="button"
                          onClick={() => setFormData({ ...formData, iconName: icon })}
                          className={`text-2xl p-2 rounded-lg border-2 transition-all ${
                            formData.iconName === icon
                              ? 'border-primary-500 bg-primary-50'
                              : 'border-gray-200 hover:border-primary-300'
                          }`}
                        >
                          {icon}
                        </button>
                      ))}
                    </div>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Требуемое количество *
                    </label>
                    <input
                      type="number"
                      required
                      min="1"
                      value={formData.requiredCount}
                      onChange={(e) =>
                        setFormData({ ...formData, requiredCount: parseInt(e.target.value) || 1 })
                      }
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                      placeholder="1"
                    />
                    <p className="text-xs text-gray-500 mt-1">
                      Сколько раз нужно выполнить действие для получения достижения
                    </p>
                  </div>
                </div>

                <div className="flex gap-3 justify-end pt-4">
                  <Button type="button" variant="secondary" onClick={resetForm}>
                    Отмена
                  </Button>
                  <Button type="submit" variant="primary" className="flex items-center gap-2">
                    <Save className="w-4 h-4" />
                    {editingId ? 'Сохранить изменения' : 'Создать достижение'}
                  </Button>
                </div>
              </form>
            </Card>
          </motion.div>
        )}

        {/* Список достижений */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {achievements.map((achievement, index) => (
            <motion.div
              key={achievement.id}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: index * 0.1 }}
            >
              <Card className="p-6 hover:shadow-lg transition-shadow">
                <div className="flex items-start justify-between mb-4">
                  <div className="text-5xl">{achievement.iconName}</div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleEdit(achievement)}
                      className="p-2 text-gray-600 hover:text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                      title="Редактировать"
                    >
                      <Edit2 className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => handleDelete(achievement.id)}
                      className="p-2 text-gray-600 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                      title="Удалить"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                </div>

                <h3 className="text-xl font-bold text-gray-900 mb-2">
                  {achievement.name}
                </h3>

                <p className="text-gray-600 text-sm mb-4">
                  {achievement.description}
                </p>

                <div className="flex items-center justify-between">
                  <span
                    className={`px-3 py-1 text-xs font-medium rounded-full ${getCategoryColor(
                      achievement.category
                    )}`}
                  >
                    {achievement.category}
                  </span>
                  <span className="text-sm text-gray-500">
                    Требуется: {achievement.requiredCount}
                  </span>
                </div>
              </Card>
            </motion.div>
          ))}
        </div>

        {achievements.length === 0 && !showCreateForm && (
          <Card className="p-12">
            <div className="text-center">
              <Award className="w-16 h-16 text-gray-400 mx-auto mb-4" />
              <p className="text-gray-600 mb-4">Достижения еще не созданы</p>
              <Button
                variant="primary"
                onClick={() => setShowCreateForm(true)}
                className="flex items-center gap-2 mx-auto"
              >
                <Plus className="w-4 h-4" />
                Создать первое достижение
              </Button>
            </div>
          </Card>
        )}
      </div>
    </div>
  );
};

export default AdminAchievementsPage;
