import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { Building2, Plus, Edit2, Trash2, ArrowLeft, Search, Filter } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import api from '../services/api';

interface University {
  id: number;
  name: string;
  nameEn?: string;
  city?: string;
  description?: string;
  website?: string;
  type: number;
  isActive: boolean;
  countryId: number;
  countryName: string;
  countryCode: string;
  examTypeIds: number[];
  examsCount: number;
}

interface Country {
  id: number;
  name: string;
  code: string;
}

const universityTypes = [
  { value: 0, label: 'Государственный' },
  { value: 1, label: 'Частный' },
  { value: 2, label: 'Международный' }
];

const AdminUniversitiesPage = () => {
  const navigate = useNavigate();
  const [universities, setUniversities] = useState<University[]>([]);
  const [filteredUniversities, setFilteredUniversities] = useState<University[]>([]);
  const [countries, setCountries] = useState<Country[]>([]);
  const [examTypes, setExamTypes] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [filterCountryId, setFilterCountryId] = useState<number | null>(null);
  const [showModal, setShowModal] = useState(false);
  const [editingUniversity, setEditingUniversity] = useState<University | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    nameEn: '',
    city: '',
    description: '',
    website: '',
    type: 0,
    countryId: 0,
    examTypeIds: [] as number[],
    isActive: true
  });

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    applyFilters();
  }, [searchQuery, filterCountryId, universities]);

  const loadData = async () => {
    try {
      setLoading(true);
      const [universitiesRes, countriesRes, examTypesRes] = await Promise.all([
        api.get('/universities'),
        api.get('/countries'),
        api.get('/examtypes')
      ]);
      setUniversities(universitiesRes.data);
      setFilteredUniversities(universitiesRes.data);
      setCountries(countriesRes.data);
      setExamTypes(examTypesRes.data);
    } catch (error) {
      console.error('Ошибка загрузки данных:', error);
      alert('Не удалось загрузить данные');
    } finally {
      setLoading(false);
    }
  };

  const applyFilters = () => {
    let filtered = [...universities];

    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(uni => 
        uni.name.toLowerCase().includes(query) ||
        uni.nameEn?.toLowerCase().includes(query) ||
        uni.city?.toLowerCase().includes(query)
      );
    }

    if (filterCountryId) {
      filtered = filtered.filter(uni => uni.countryId === filterCountryId);
    }

    setFilteredUniversities(filtered);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.countryId) {
      alert('Выберите страну');
      return;
    }

    if (!formData.examTypeIds || formData.examTypeIds.length === 0) {
      alert('Выберите хотя бы один тип экзамена');
      return;
    }

    // Убедимся что все числовые поля имеют правильный тип
    const payload = {
      name: formData.name,
      nameEn: formData.nameEn || null,
      city: formData.city || null,
      description: formData.description || null,
      website: formData.website || null,
      type: Number(formData.type),
      countryId: Number(formData.countryId),
      examTypeIds: formData.examTypeIds.map(id => Number(id)),
      isActive: Boolean(formData.isActive)
    };

    console.log('=== Отправка данных ===');
    console.log('payload:', JSON.stringify(payload, null, 2));

    try {
      if (editingUniversity) {
        console.log('PUT запрос к:', `/universities/${editingUniversity.id}`);
        const response = await api.put(`/universities/${editingUniversity.id}`, payload);
        console.log('Ответ:', response);
        alert('Университет успешно обновлен');
      } else {
        console.log('POST запрос к: /universities');
        const response = await api.post('/universities', payload);
        console.log('Ответ:', response);
        alert('Университет успешно добавлен');
      }
      
      setShowModal(false);
      resetForm();
      loadData();
    } catch (error: any) {
      console.error('=== ОШИБКА ===');
      console.error('Полная ошибка:', error);
      console.error('response:', error.response);
      console.error('response.data:', error.response?.data);
      console.error('response.status:', error.response?.status);
      console.error('response.data.errors:', error.response?.data?.errors);
      alert(
        'Ошибка валидации:\n' + 
        JSON.stringify(error.response?.data?.errors || error.response?.data, null, 2)
      );
    }
  };

  const handleEdit = (university: University) => {
    setEditingUniversity(university);
    setFormData({
      name: university.name,
      nameEn: university.nameEn || '',
      city: university.city || '',
      description: university.description || '',
      website: university.website || '',
      type: university.type,
      countryId: university.countryId,
      examTypeIds: university.examTypeIds || [],
      isActive: university.isActive
    });
    setShowModal(true);
  };

  const handleDelete = async (university: University) => {
    if (university.examsCount > 0) {
      alert(`Нельзя удалить университет: есть ${university.examsCount} связанных экзаменов`);
      return;
    }

    if (!window.confirm(`Вы уверены, что хотите удалить "${university.name}"?`)) {
      return;
    }

    try {
      await api.delete(`/universities/${university.id}`);
      alert('Университет успешно удален');
      loadData();
    } catch (error: any) {
      console.error('Ошибка удаления университета:', error);
      alert(error.response?.data?.message || 'Не удалось удалить университет');
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      nameEn: '',
      city: '',
      description: '',
      website: '',
      type: 0,
      countryId: countries.length > 0 ? countries[0].id : 0,
      examTypeIds: [],
      isActive: true
    });
    setEditingUniversity(null);
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
                <Building2 className="w-8 h-8 text-primary-500" />
                Управление университетами
              </h1>
              <p className="mt-2 text-gray-600 dark:text-gray-400">
                Университеты и вузы для международной системы
              </p>
            </div>
            <Button variant="primary" onClick={handleAddNew} className="flex items-center gap-2">
              <Plus className="w-4 h-4" />
              Добавить университет
            </Button>
          </div>
        </div>

        {/* Filters */}
        <Card className="p-4 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <input
                type="text"
                placeholder="Поиск по названию или городу..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:ring-2 focus:ring-primary-500"
              />
            </div>
            <div className="relative">
              <Filter className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
              <select
                value={filterCountryId || ''}
                onChange={(e) => setFilterCountryId(e.target.value ? Number(e.target.value) : null)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:ring-2 focus:ring-primary-500"
              >
                <option value="">Все страны</option>
                {countries.map(country => (
                  <option key={country.id} value={country.id}>
                    {country.name} ({country.code})
                  </option>
                ))}
              </select>
            </div>
          </div>
        </Card>

        {/* Universities List */}
        {loading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-500 mx-auto"></div>
            <p className="mt-4 text-gray-600 dark:text-gray-400">Загрузка университетов...</p>
          </div>
        ) : (
          <div className="grid gap-4">
            {filteredUniversities.map((university) => (
              <motion.div
                key={university.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
              >
                <Card className="p-6 hover:shadow-lg transition-shadow">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-2">
                        <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                          {university.name}
                        </h3>
                        <span className="text-xs px-2 py-1 bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200 rounded">
                          {universityTypes.find(t => t.value === university.type)?.label}
                        </span>
                      </div>
                      {university.nameEn && (
                        <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">
                          {university.nameEn}
                        </p>
                      )}
                      <div className="flex flex-wrap gap-3 text-sm text-gray-600 dark:text-gray-400 mb-2">
                        <span className="flex items-center gap-1">
                          🌍 {university.countryName}
                        </span>
                        {university.city && (
                          <span>📍 {university.city}</span>
                        )}
                        {university.website && (
                          <a 
                            href={university.website} 
                            target="_blank" 
                            rel="noopener noreferrer"
                            className="text-primary-600 hover:underline"
                          >
                            🔗 Сайт
                          </a>
                        )}
                      </div>
                      {university.description && (
                        <p className="text-sm text-gray-600 dark:text-gray-400 mt-2">
                          {university.description}
                        </p>
                      )}
                      <p className="text-xs text-gray-500 dark:text-gray-500 mt-2">
                        Экзаменов: {university.examsCount}
                      </p>
                    </div>
                    <div className="flex items-center gap-2 ml-4">
                      <span className={`px-3 py-1 rounded-full text-xs font-medium ${
                        university.isActive 
                          ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                          : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                      }`}>
                        {university.isActive ? 'Активен' : 'Неактивен'}
                      </span>
                      <Button
                        variant="secondary"
                        size="sm"
                        onClick={() => handleEdit(university)}
                      >
                        <Edit2 className="w-4 h-4" />
                      </Button>
                      <Button
                        variant="danger"
                        size="sm"
                        onClick={() => handleDelete(university)}
                      >
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </div>
                  </div>
                </Card>
              </motion.div>
            ))}

            {filteredUniversities.length === 0 && (
              <Card className="p-12 text-center">
                <Building2 className="w-16 h-16 text-gray-400 mx-auto mb-4" />
                <p className="text-gray-600 dark:text-gray-400">
                  {searchQuery || filterCountryId ? 'Университеты не найдены' : 'Нет добавленных университетов'}
                </p>
              </Card>
            )}
          </div>
        )}

        {/* Modal */}
        {showModal && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4 overflow-y-auto">
            <motion.div
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-2xl w-full my-8"
            >
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
                {editingUniversity ? 'Редактировать университет' : 'Добавить университет'}
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
                      placeholder="МГУ им. Ломоносова"
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
                      placeholder="Lomonosov Moscow State University"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Страна *
                    </label>
                    <select
                      required
                      value={formData.countryId}
                      onChange={(e) => setFormData({ ...formData, countryId: Number(e.target.value) })}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                    >
                      <option value={0}>Выберите страну</option>
                      {countries.map(country => (
                        <option key={country.id} value={country.id}>
                          {country.name} ({country.code})
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Город
                    </label>
                    <input
                      type="text"
                      value={formData.city}
                      onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                      placeholder="Москва"
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Тип *
                    </label>
                    <select
                      value={formData.type}
                      onChange={(e) => setFormData({ ...formData, type: Number(e.target.value) })}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                    >
                      {universityTypes.map(type => (
                        <option key={type.value} value={type.value}>
                          {type.label}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                      Веб-сайт
                    </label>
                    <input
                      type="url"
                      value={formData.website}
                      onChange={(e) => setFormData({ ...formData, website: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                      placeholder="https://msu.ru"
                    />
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
                    placeholder="Краткое описание университета..."
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Типы экзаменов
                  </label>
                  <div className="grid grid-cols-2 gap-2 max-h-48 overflow-y-auto p-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-gray-50 dark:bg-gray-800">
                    {examTypes.map((examType) => (
                      <label key={examType.id} className="flex items-center gap-2 cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700 p-2 rounded">
                        <input
                          type="checkbox"
                          checked={formData.examTypeIds.includes(examType.id)}
                          onChange={(e) => {
                            if (e.target.checked) {
                              setFormData({ ...formData, examTypeIds: [...formData.examTypeIds, examType.id] });
                            } else {
                              setFormData({ ...formData, examTypeIds: formData.examTypeIds.filter(id => id !== examType.id) });
                            }
                          }}
                          className="rounded border-gray-300 dark:border-gray-600 text-primary-600"
                        />
                        <span className="text-sm text-gray-700 dark:text-gray-300">{examType.name}</span>
                      </label>
                    ))}
                  </div>
                  <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                    Выберите типы экзаменов, которые принимает университет
                  </p>
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
                    {editingUniversity ? 'Сохранить' : 'Добавить'}
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

export default AdminUniversitiesPage;
