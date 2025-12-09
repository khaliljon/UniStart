import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { ClipboardList, Plus, Edit2, Trash2, ArrowLeft, Search } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface ExamType {
  id: number;
  name: string;
  nameEn?: string;
  code: string;
  description?: string;
  defaultCountryId?: number;
  defaultCountryName?: string;
  isActive: boolean;
  examsCount: number;
}

interface Country {
  id: number;
  name: string;
  code: string;
}

const AdminExamTypesPage = () => {
  const navigate = useNavigate();
  const [examTypes, setExamTypes] = useState<ExamType[]>([]);
  const [filteredExamTypes, setFilteredExamTypes] = useState<ExamType[]>([]);
  const [countries, setCountries] = useState<Country[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingExamType, setEditingExamType] = useState<ExamType | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    nameEn: '',
    code: '',
    description: '',
    defaultCountryId: null as number | null,
    isActive: true
  });

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    const query = searchQuery.toLowerCase();
    const filtered = examTypes.filter(et => 
      et.name.toLowerCase().includes(query) ||
      et.nameEn?.toLowerCase().includes(query) ||
      et.code.toLowerCase().includes(query)
    );
    setFilteredExamTypes(filtered);
  }, [searchQuery, examTypes]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [examTypesRes, countriesRes] = await Promise.all([
        api.get('/examtypes'),
        api.get('/countries')
      ]);
      setExamTypes(examTypesRes.data);
      setFilteredExamTypes(examTypesRes.data);
      setCountries(countriesRes.data);
    } catch (error) {
      console.error('Ошибка загрузки данных:', error);
      alert('Не удалось загрузить данные');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      if (editingExamType) {
        await api.put(`/examtypes/${editingExamType.id}`, formData);
        alert('Тип экзамена успешно обновлен');
      } else {
        await api.post('/examtypes', formData);
        alert('Тип экзамена успешно добавлен');
      }
      
      setShowModal(false);
      resetForm();
      loadData();
    } catch (error: any) {
      console.error('Ошибка сохранения типа экзамена:', error);
      alert(error.response?.data?.message || 'Не удалось сохранить тип экзамена');
    }
  };

  const handleEdit = (examType: ExamType) => {
    setEditingExamType(examType);
    setFormData({
      name: examType.name,
      nameEn: examType.nameEn || '',
      code: examType.code,
      description: examType.description || '',
      defaultCountryId: examType.defaultCountryId || null,
      isActive: examType.isActive
    });
    setShowModal(true);
  };

  const handleDelete = async (examType: ExamType) => {
    if (examType.examsCount > 0) {
      alert(`Нельзя удалить тип экзамена: есть ${examType.examsCount} связанных экзаменов`);
      return;
    }

    if (!window.confirm(`Вы уверены, что хотите удалить "${examType.name}"?`)) {
      return;
    }

    try {
      await api.delete(`/examtypes/${examType.id}`);
      alert('Тип экзамена успешно удален');
      loadData();
    } catch (error: any) {
      console.error('Ошибка удаления типа экзамена:', error);
      alert(error.response?.data?.message || 'Не удалось удалить тип экзамена');
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      nameEn: '',
      code: '',
      description: '',
      defaultCountryId: null,
      isActive: true
    });
    setEditingExamType(null);
  };

  const handleAddNew = () => {
    resetForm();
    setShowModal(true);
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
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
              <h1 className="text-3xl font-bold text-gray-900 dark:text-white flex items-center gap-3">
                <ClipboardList className="w-8 h-8 text-primary-500" />
                Типы экзаменов
              </h1>
              <p className="mt-2 text-gray-600 dark:text-gray-400">
                Стандартизированные типы экзаменов (ЕНТ, ЕГЭ, SAT, IELTS и т.д.)
              </p>
            </div>
            <Button variant="primary" onClick={handleAddNew} className="flex items-center gap-2">
              <Plus className="w-4 h-4" />
              Добавить тип экзамена
            </Button>
          </div>
        </div>

        {/* Search */}
        <Card className="p-4 mb-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="Поиск по названию или коду..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </Card>

        {/* Exam Types List */}
        {loading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-500 mx-auto"></div>
            <p className="mt-4 text-gray-600 dark:text-gray-400">Загрузка типов экзаменов...</p>
          </div>
        ) : (
          <div className="grid gap-4">
            {filteredExamTypes.map((examType) => (
              <motion.div
                key={examType.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
              >
                <Card className="p-6 hover:shadow-lg transition-shadow">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-2">
                        <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                          {examType.name}
                        </h3>
                        <span className="text-xs px-2 py-1 bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200 rounded font-mono">
                          {examType.code}
                        </span>
                      </div>
                      {examType.nameEn && (
                        <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">
                          {examType.nameEn}
                        </p>
                      )}
                      {examType.description && (
                        <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">
                          {examType.description}
                        </p>
                      )}
                      <div className="flex gap-4 text-sm text-gray-500 dark:text-gray-400">
                        {examType.defaultCountryName && (
                          <span>🌍 Основная страна: {examType.defaultCountryName}</span>
                        )}
                        <span>Экзаменов: {examType.examsCount}</span>
                      </div>
                    </div>
                    <div className="flex items-center gap-2 ml-4">
                      <span className={`px-3 py-1 rounded-full text-xs font-medium ${
                        examType.isActive 
                          ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                          : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                      }`}>
                        {examType.isActive ? 'Активен' : 'Неактивен'}
                      </span>
                      <Button
                        variant="secondary"
                        size="sm"
                        onClick={() => handleEdit(examType)}
                      >
                        <Edit2 className="w-4 h-4" />
                      </Button>
                      <Button
                        variant="danger"
                        size="sm"
                        onClick={() => handleDelete(examType)}
                      >
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </div>
                  </div>
                </Card>
              </motion.div>
            ))}

            {filteredExamTypes.length === 0 && (
              <Card className="p-12 text-center">
                <ClipboardList className="w-16 h-16 text-gray-400 mx-auto mb-4" />
                <p className="text-gray-600 dark:text-gray-400">
                  {searchQuery ? 'Типы экзаменов не найдены' : 'Нет добавленных типов экзаменов'}
                </p>
              </Card>
            )}
          </div>
        )}

        {/* Modal */}
        {showModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
            <motion.div
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-2xl w-full"
            >
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
                {editingExamType ? 'Редактировать тип экзамена' : 'Добавить тип экзамена'}
              </h2>
              
              <form onSubmit={handleSubmit} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Название (RU) *
                    </label>
                    <input
                      type="text"
                      required
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                      placeholder="Единое Национальное Тестирование"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Название (EN)
                    </label>
                    <input
                      type="text"
                      value={formData.nameEn}
                      onChange={(e) => setFormData({ ...formData, nameEn: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                      placeholder="Unified National Testing"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Код *
                    </label>
                    <input
                      type="text"
                      required
                      value={formData.code}
                      onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white font-mono"
                      placeholder="ENT"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Основная страна (опционально)
                    </label>
                    <select
                      value={formData.defaultCountryId || ''}
                      onChange={(e) => setFormData({ ...formData, defaultCountryId: e.target.value ? Number(e.target.value) : null })}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                    >
                      <option value="">Не выбрана</option>
                      {countries.map(country => (
                        <option key={country.id} value={country.id}>
                          {country.name} ({country.code})
                        </option>
                      ))}
                    </select>
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Описание
                  </label>
                  <textarea
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    rows={3}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                    placeholder="Краткое описание экзамена..."
                  />
                </div>

                <div className="flex items-center gap-2">
                  <input
                    type="checkbox"
                    id="isActive"
                    checked={formData.isActive}
                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                    className="rounded border-gray-300 dark:border-gray-600"
                  />
                  <label htmlFor="isActive" className="text-sm text-gray-700 dark:text-gray-300">
                    Активен
                  </label>
                </div>

                <div className="flex gap-3 pt-4">
                  <Button
                    type="button"
                    variant="secondary"
                    onClick={() => {
                      setShowModal(false);
                      resetForm();
                    }}
                    className="flex-1"
                  >
                    Отмена
                  </Button>
                  <Button type="submit" variant="primary" className="flex-1">
                    {editingExamType ? 'Сохранить' : 'Добавить'}
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

export default AdminExamTypesPage;
