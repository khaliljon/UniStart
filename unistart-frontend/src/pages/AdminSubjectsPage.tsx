import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import { BookOpen, Trash2, Edit, Plus, ArrowLeft, Check, X } from 'lucide-react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface Subject {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  createdAt: string;
}

const AdminSubjectsPage = () => {
  const navigate = useNavigate();
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editingSubject, setEditingSubject] = useState<Subject | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    isActive: true
  });

  useEffect(() => {
    loadSubjects();
  }, []);

  const loadSubjects = async () => {
    try {
      // Используем endpoint /all чтобы видеть и неактивные предметы
      const response = await api.get('/subjects/all');
      setSubjects(response.data);
    } catch (error) {
      console.error('Ошибка загрузки предметов:', error);
      alert('Ошибка загрузки предметов');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      if (editingSubject) {
        await api.put(`/subjects/${editingSubject.id}`, {
          id: editingSubject.id,
          ...formData
        });
        alert('Предмет успешно обновлен');
      } else {
        await api.post('/subjects', formData);
        alert('Предмет успешно создан');
      }
      
      setShowModal(false);
      setEditingSubject(null);
      setFormData({ name: '', description: '', isActive: true });
      loadSubjects();
    } catch (error) {
      console.error('Ошибка сохранения предмета:', error);
      alert('Ошибка сохранения предмета');
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Вы уверены, что хотите удалить этот предмет?')) {
      return;
    }

    try {
      await api.delete(`/subjects/${id}`);
      loadSubjects();
      alert('Предмет успешно удален (деактивирован)');
    } catch (error) {
      console.error('Ошибка удаления предмета:', error);
      alert('Ошибка удаления предмета');
    }
  };

  const openEditModal = (subject: Subject) => {
    setEditingSubject(subject);
    setFormData({
      name: subject.name,
      description: subject.description || '',
      isActive: subject.isActive
    });
    setShowModal(true);
  };

  const openCreateModal = () => {
    setEditingSubject(null);
    setFormData({ name: '', description: '', isActive: true });
    setShowModal(true);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        <div className="mb-8">
          <Button 
            variant="secondary" 
            className="mb-4 flex items-center gap-2"
            onClick={() => navigate('/dashboard')}
          >
            <ArrowLeft className="w-4 h-4" />
            Назад к панели
          </Button>
          
          <div className="flex justify-between items-center">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">Управление предметами</h1>
              <p className="mt-2 text-gray-600">
                Создание и редактирование учебных предметов
              </p>
            </div>
            <Button onClick={openCreateModal} className="flex items-center gap-2">
              <Plus className="w-4 h-4" />
              Добавить предмет
            </Button>
          </div>
        </div>

        {loading ? (
          <div className="flex justify-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
          </div>
        ) : (
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {subjects.map((subject) => (
              <motion.div
                key={subject.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
              >
                <Card className={`p-6 ${!subject.isActive ? 'opacity-60 bg-gray-100' : ''}`}>
                  <div className="flex justify-between items-start mb-4">
                    <div className="p-3 bg-primary-100 rounded-lg">
                      <BookOpen className="w-6 h-6 text-primary-600" />
                    </div>
                    <div className="flex gap-2">
                      <button
                        onClick={() => openEditModal(subject)}
                        className="p-2 text-gray-400 hover:text-primary-600 transition-colors"
                        title="Редактировать"
                      >
                        <Edit className="w-4 h-4" />
                      </button>
                      {subject.isActive && (
                        <button
                          onClick={() => handleDelete(subject.id)}
                          className="p-2 text-gray-400 hover:text-red-600 transition-colors"
                          title="Удалить"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      )}
                    </div>
                  </div>

                  <h3 className="text-xl font-bold text-gray-900 mb-2">
                    {subject.name}
                  </h3>
                  
                  {subject.description && (
                    <p className="text-gray-600 mb-4 line-clamp-2">
                      {subject.description}
                    </p>
                  )}

                  <div className="flex items-center gap-2 mt-4 pt-4 border-t border-gray-100">
                    <span className={`px-2 py-1 text-xs rounded-full ${
                      subject.isActive 
                        ? 'bg-green-100 text-green-800' 
                        : 'bg-red-100 text-red-800'
                    }`}>
                      {subject.isActive ? 'Активен' : 'Неактивен'}
                    </span>
                    <span className="text-xs text-gray-500 ml-auto">
                      ID: {subject.id}
                    </span>
                  </div>
                </Card>
              </motion.div>
            ))}
          </div>
        )}

        {/* Modal */}
        {showModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              className="bg-white rounded-xl shadow-xl max-w-md w-full p-6"
            >
              <div className="flex justify-between items-center mb-6">
                <h2 className="text-xl font-bold text-gray-900">
                  {editingSubject ? 'Редактирование предмета' : 'Новый предмет'}
                </h2>
                <button
                  onClick={() => setShowModal(false)}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <X className="w-6 h-6" />
                </button>
              </div>

              <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Название
                  </label>
                  <input
                    type="text"
                    required
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    placeholder="Например: Математика"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Описание
                  </label>
                  <textarea
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    rows={3}
                    placeholder="Краткое описание предмета..."
                  />
                </div>

                <div className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    id="isActive"
                    checked={formData.isActive}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                    className="rounded text-primary-600 focus:ring-primary-500"
                  />
                  <label htmlFor="isActive" className="text-sm text-gray-700">
                    Активен
                  </label>
                </div>

                <div className="flex gap-3 mt-6">
                  <Button
                    type="button"
                    variant="secondary"
                    className="flex-1"
                    onClick={() => setShowModal(false)}
                  >
                    Отмена
                  </Button>
                  <Button
                    type="submit"
                    className="flex-1"
                  >
                    {editingSubject ? 'Сохранить' : 'Создать'}
                  </Button>
                </div>
              </form>
            </motion.div>
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminSubjectsPage;
