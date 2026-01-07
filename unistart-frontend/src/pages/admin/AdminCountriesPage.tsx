import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { Globe, Plus, Edit2, Trash2, ArrowLeft, Search } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import api from '../../services/api';

interface Country {
  id: number;
  name: string;
  nameEn?: string;
  code: string;
  flagEmoji?: string;
  isActive: boolean;
  universitiesCount: number;
  examsCount: number;
}

const AdminCountriesPage = () => {
  const navigate = useNavigate();
  const [countries, setCountries] = useState<Country[]>([]);
  const [filteredCountries, setFilteredCountries] = useState<Country[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingCountry, setEditingCountry] = useState<Country | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    nameEn: '',
    code: '',
    flagEmoji: '',
    isActive: true
  });

  useEffect(() => {
    loadCountries();
  }, []);

  useEffect(() => {
    const query = searchQuery.toLowerCase();
    const filtered = countries.filter(country => 
      country.name.toLowerCase().includes(query) ||
      country.nameEn?.toLowerCase().includes(query) ||
      country.code.toLowerCase().includes(query)
    );
    setFilteredCountries(filtered);
  }, [searchQuery, countries]);

  const loadCountries = async () => {
    try {
      setLoading(true);
      const response = await api.get('/countries');
      setCountries(response.data);
      setFilteredCountries(response.data);
    } catch (error) {
      console.error('Ошибка загрузки стран:', error);
      alert('Не удалось загрузить список стран');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      if (editingCountry) {
        await api.put(`/countries/${editingCountry.id}`, formData);
        alert('Страна успешно обновлена');
      } else {
        await api.post('/countries', formData);
        alert('Страна успешно добавлена');
      }
      
      setShowModal(false);
      resetForm();
      loadCountries();
    } catch (error: any) {
      console.error('Ошибка сохранения страны:', error);
      alert(error.response?.data?.message || 'Не удалось сохранить страну');
    }
  };

  const handleEdit = (country: Country) => {
    setEditingCountry(country);
    setFormData({
      name: country.name,
      nameEn: country.nameEn || '',
      code: country.code,
      flagEmoji: country.flagEmoji || '',
      isActive: country.isActive
    });
    setShowModal(true);
  };

  const handleDelete = async (country: Country) => {
    if (country.universitiesCount > 0 || country.examsCount > 0) {
      alert(`Нельзя удалить страну: есть ${country.universitiesCount} вузов и ${country.examsCount} экзаменов`);
      return;
    }

    if (!window.confirm(`Вы уверены, что хотите удалить страну "${country.name}"?`)) {
      return;
    }

    try {
      await api.delete(`/countries/${country.id}`);
      alert('Страна успешно удалена');
      loadCountries();
    } catch (error: any) {
      console.error('Ошибка удаления страны:', error);
      alert(error.response?.data?.message || 'Не удалось удалить страну');
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      nameEn: '',
      code: '',
      flagEmoji: '',
      isActive: true
    });
    setEditingCountry(null);
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
                <Globe className="w-8 h-8 text-primary-500" />
                Управление странами
              </h1>
              <p className="mt-2 text-gray-600 dark:text-gray-400">
                Страны для международной системы образования
              </p>
            </div>
            <Button variant="primary" onClick={handleAddNew} className="flex items-center gap-2">
              <Plus className="w-4 h-4" />
              Добавить страну
            </Button>
          </div>
        </div>

        {/* Search */}
        <Card className="p-4 mb-6">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
            <input
              type="text"
              placeholder="Поиск по названию или коду страны..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:ring-2 focus:ring-primary-500"
            />
          </div>
        </Card>

        {/* Countries List */}
        {loading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-500 mx-auto"></div>
            <p className="mt-4 text-gray-600 dark:text-gray-400">Загрузка стран...</p>
          </div>
        ) : (
          <div className="grid gap-4">
            {filteredCountries.map((country) => (
              <motion.div
                key={country.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
              >
                <Card className="p-6 hover:shadow-lg transition-shadow">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-4 flex-1">
                      <div className="text-4xl">{country.flagEmoji || '🌍'}</div>
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                          {country.name}
                          {country.nameEn && (
                            <span className="text-sm font-normal text-gray-500 dark:text-gray-400 ml-2">
                              ({country.nameEn})
                            </span>
                          )}
                        </h3>
                        <p className="text-sm text-gray-600 dark:text-gray-400">
                          Код: {country.code}
                        </p>
                        <div className="flex gap-4 mt-1 text-sm text-gray-500 dark:text-gray-400">
                          <span>Вузов: {country.universitiesCount}</span>
                          <span>Экзаменов: {country.examsCount}</span>
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <span className={`px-3 py-1 rounded-full text-xs font-medium ${
                        country.isActive 
                          ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                          : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
                      }`}>
                        {country.isActive ? 'Активна' : 'Неактивна'}
                      </span>
                      <Button
                        variant="secondary"
                        size="sm"
                        onClick={() => handleEdit(country)}
                      >
                        <Edit2 className="w-4 h-4" />
                      </Button>
                      <Button
                        variant="danger"
                        size="sm"
                        onClick={() => handleDelete(country)}
                      >
                        <Trash2 className="w-4 h-4" />
                      </Button>
                    </div>
                  </div>
                </Card>
              </motion.div>
            ))}

            {filteredCountries.length === 0 && (
              <Card className="p-12 text-center">
                <Globe className="w-16 h-16 text-gray-400 mx-auto mb-4" />
                <p className="text-gray-600 dark:text-gray-400">
                  {searchQuery ? 'Страны не найдены' : 'Нет добавленных стран'}
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
              className="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-md w-full"
            >
              <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
                {editingCountry ? 'Редактировать страну' : 'Добавить страну'}
              </h2>
              
              <form onSubmit={handleSubmit} className="space-y-4">
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
                    placeholder="Казахстан"
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
                    placeholder="Kazakhstan"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Код страны (2-3 символа) *
                  </label>
                  <input
                    type="text"
                    required
                    maxLength={3}
                    value={formData.code}
                    onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                    placeholder="KZ"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Флаг (эмодзи)
                  </label>
                  <input
                    type="text"
                    value={formData.flagEmoji}
                    onChange={(e) => setFormData({ ...formData, flagEmoji: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
                    placeholder="🇰🇿"
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
                    Активна
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
                    {editingCountry ? 'Сохранить' : 'Добавить'}
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

export default AdminCountriesPage;
