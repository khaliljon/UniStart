import { useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import {
  Download,
  FileText,
  Users,
  BookOpen,
  Award,
  ArrowLeft,
  CheckCircle,
  Loader,
  Database,
} from 'lucide-react';
import Card from '../../components/common/Card';
import Button from '../../components/common/Button';
import api from '../../services/api';

interface ExportOption {
  id: string;
  title: string;
  description: string;
  icon: any;
  endpoint: string;
  filename: string;
  color: string;
}

const AdminExportPage = () => {
  const navigate = useNavigate();
  const [exportingId, setExportingId] = useState<string | null>(null);
  const [exportSuccess, setExportSuccess] = useState<string | null>(null);

  const exportOptions: ExportOption[] = [
    {
      id: 'users',
      title: 'Пользователи',
      description: 'Экспорт всех пользователей с ролями и статистикой',
      icon: Users,
      endpoint: '/admin/export/users',
      filename: 'UniStart_Users',
      color: 'bg-blue-500',
    },
    {
      id: 'quizzes',
      title: 'Квизы',
      description: 'Экспорт всех квизов с вопросами и ответами',
      icon: FileText,
      endpoint: '/admin/export/quizzes',
      filename: 'UniStart_Quizzes',
      color: 'bg-green-500',
    },
    {
      id: 'flashcards',
      title: 'Карточки',
      description: 'Экспорт всех наборов карточек с содержимым',
      icon: BookOpen,
      endpoint: '/admin/export/flashcards',
      filename: 'UniStart_Flashcards',
      color: 'bg-purple-500',
    },
    {
      id: 'attempts',
      title: 'Попытки квизов',
      description: 'Экспорт всех результатов прохождения квизов',
      icon: Award,
      endpoint: '/admin/export/attempts',
      filename: 'UniStart_Quiz_Attempts',
      color: 'bg-yellow-500',
    },
    {
      id: 'full',
      title: 'Полный экспорт',
      description: 'Экспорт всех данных платформы (архив)',
      icon: Database,
      endpoint: '/admin/export/full',
      filename: 'UniStart_Full_Export',
      color: 'bg-red-500',
    },
  ];

  const handleExport = async (option: ExportOption) => {
    setExportingId(option.id);
    setExportSuccess(null);

    try {
      const response = await api.get(option.endpoint, {
        responseType: 'blob',
      });

      // Создаём ссылку для скачивания
      const blob = new Blob([response.data], { 
        type: option.id === 'full' ? 'application/zip' : 'text/csv;charset=utf-8;' 
      });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);
      
      const extension = option.id === 'full' ? 'zip' : 'csv';
      const fileName = `${option.filename}_${new Date().toISOString().split('T')[0]}.${extension}`;
      
      link.setAttribute('href', url);
      link.setAttribute('download', fileName);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);

      setExportSuccess(option.id);
      setTimeout(() => setExportSuccess(null), 3000);
    } catch (error: any) {
      console.error('Ошибка экспорта:', error);
      if (error.response?.status === 404) {
        alert('Эндпоинт экспорта не найден. Функционал находится в разработке.');
      } else {
        alert('Не удалось экспортировать данные. Попробуйте позже.');
      }
    } finally {
      setExportingId(null);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 py-8 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
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
                📥 Экспорт данных платформы
              </h1>
              <p className="text-gray-600">
                Скачайте данные UniStart для анализа и резервного копирования
              </p>
            </div>
          </div>
        </motion.div>

        {/* Info Card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="mb-8"
        >
          <Card className="p-6 bg-blue-50 border-blue-200">
            <div className="flex items-start gap-4">
              <Download className="w-8 h-8 text-blue-500 flex-shrink-0" />
              <div>
                <h3 className="text-lg font-semibold text-gray-900 mb-2">
                  Форматы экспорта
                </h3>
                <ul className="text-sm text-gray-600 space-y-1">
                  <li>• CSV файлы для простых данных (пользователи, попытки)</li>
                  <li>• ZIP архив для полного экспорта всей платформы</li>
                  <li>• Файлы содержат дату экспорта в имени для удобной организации</li>
                  <li>• Все текстовые данные в кодировке UTF-8</li>
                </ul>
              </div>
            </div>
          </Card>
        </motion.div>

        {/* Export Options */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
          className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8"
        >
          {exportOptions.map((option, index) => (
            <motion.div
              key={option.id}
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.2 + index * 0.1 }}
            >
              <Card className="p-6 hover:shadow-lg transition-shadow">
                <div className="flex items-start gap-4">
                  <div className={`${option.color} p-4 rounded-lg flex-shrink-0`}>
                    <option.icon className="w-8 h-8 text-white" />
                  </div>
                  <div className="flex-1">
                    <h3 className="text-xl font-bold text-gray-900 mb-2">
                      {option.title}
                    </h3>
                    <p className="text-gray-600 text-sm mb-4">
                      {option.description}
                    </p>
                    <div className="flex items-center gap-3">
                      {exportSuccess === option.id && (
                        <motion.div
                          initial={{ scale: 0 }}
                          animate={{ scale: 1 }}
                          className="flex items-center gap-2 text-green-600"
                        >
                          <CheckCircle className="w-5 h-5" />
                          <span className="text-sm font-medium">Скачано!</span>
                        </motion.div>
                      )}
                      <Button
                        onClick={() => handleExport(option)}
                        disabled={exportingId === option.id}
                        className="flex items-center gap-2"
                      >
                        {exportingId === option.id ? (
                          <>
                            <Loader className="w-4 h-4 animate-spin" />
                            <span>Экспорт...</span>
                          </>
                        ) : (
                          <>
                            <Download className="w-4 h-4" />
                            <span>Скачать</span>
                          </>
                        )}
                      </Button>
                    </div>
                  </div>
                </div>
              </Card>
            </motion.div>
          ))}
        </motion.div>

        {/* Additional Info */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.7 }}
        >
          <Card className="p-6 bg-gray-50">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">
              💡 Рекомендации по использованию
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <h4 className="font-medium text-gray-900 mb-2">
                  Для анализа данных:
                </h4>
                <ul className="space-y-1 text-sm text-gray-600">
                  <li>• Используйте CSV файлы в Excel или Google Sheets</li>
                  <li>• Экспортируйте попытки квизов для анализа успеваемости</li>
                  <li>• Данные пользователей помогут понять аудиторию</li>
                </ul>
              </div>
              <div>
                <h4 className="font-medium text-gray-900 mb-2">
                  Для резервного копирования:
                </h4>
                <ul className="space-y-1 text-sm text-gray-600">
                  <li>• Делайте полный экспорт регулярно (раз в неделю)</li>
                  <li>• Храните архивы в безопасном месте</li>
                  <li>• Проверяйте целостность данных после экспорта</li>
                </ul>
              </div>
            </div>
          </Card>
        </motion.div>
      </div>
    </div>
  );
};

export default AdminExportPage;
