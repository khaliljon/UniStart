import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { ArrowLeft, BookOpen, Clock, Video, FileText } from 'lucide-react';
import Button from '../components/common/Button';
import api from '../services/api';

interface TheoryData {
  id: number;
  title: string;
  content: string;
  coverImageUrl?: string;
  estimatedReadTimeMinutes: number;
  additionalResources?: string;
  topic: {
    id: number;
    title: string;
    competency: {
      id: number;
      title: string;
      module: {
        id: number;
        title: string;
        subject: {
          id: number;
          name: string;
        };
      };
    };
  };
}

const TheoryPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [theory, setTheory] = useState<TheoryData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      loadTheory(parseInt(id));
    }
  }, [id]);

  const loadTheory = async (theoryId: number) => {
    try {
      setLoading(true);
      setError(null);
      const response = await api.get(`/learning/theory/${theoryId}`);
      setTheory(response.data);
    } catch (err: any) {
      console.error('Ошибка загрузки теории:', err);
      setError(err.response?.data?.message || 'Не удалось загрузить теорию');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 flex items-center justify-center">
        <div className="text-white text-xl">Загрузка теории...</div>
      </div>
    );
  }

  if (error || !theory) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 flex items-center justify-center">
        <div className="text-center">
          <div className="text-white text-xl mb-4">{error || 'Теория не найдена'}</div>
          <Button variant="primary" onClick={() => navigate('/quizzes')}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Вернуться к обучению
          </Button>
        </div>
      </div>
    );
  }

  // Парсим дополнительные ресурсы (JSON массив)
  let additionalResources: any[] = [];
  try {
    if (theory.additionalResources) {
      additionalResources = JSON.parse(theory.additionalResources);
    }
  } catch (e) {
    console.error('Ошибка парсинга дополнительных ресурсов:', e);
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 py-8 px-4">
      <div className="max-w-4xl mx-auto">
        {/* Breadcrumb */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-6"
        >
          <Button
            variant="secondary"
            onClick={() => navigate('/quizzes')}
            className="mb-4 bg-white/10 hover:bg-white/20 border-white/20 text-white"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Назад к обучению
          </Button>
          
          <div className="text-sm text-white/60 mb-4 flex items-center gap-2 flex-wrap">
            <span>{theory.topic.competency.module.subject.name}</span>
            <span>›</span>
            <span>{theory.topic.competency.module.title}</span>
            <span>›</span>
            <span>{theory.topic.competency.title}</span>
            <span>›</span>
            <span className="text-white">{theory.topic.title}</span>
          </div>
        </motion.div>

        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="mb-8"
        >
          <h1 className="text-4xl font-bold text-white mb-4">{theory.title}</h1>
          
          <div className="flex items-center gap-4 text-white/60">
            <div className="flex items-center gap-2">
              <Clock className="w-4 h-4" />
              <span>{theory.estimatedReadTimeMinutes} мин чтения</span>
            </div>
            <div className="flex items-center gap-2">
              <BookOpen className="w-4 h-4" />
              <span>Теория</span>
            </div>
          </div>
        </motion.div>

        {/* Cover Image */}
        {theory.coverImageUrl && (
          <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 0.2 }}
            className="mb-8 rounded-xl overflow-hidden"
          >
            <img
              src={theory.coverImageUrl}
              alt={theory.title}
              className="w-full h-64 object-cover"
            />
          </motion.div>
        )}

        {/* Content */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="bg-white/10 backdrop-blur-lg rounded-xl p-8 border border-white/20 mb-8"
        >
          <div
            className="prose prose-invert prose-lg max-w-none text-white/90"
            dangerouslySetInnerHTML={{ __html: theory.content }}
          />
        </motion.div>

        {/* Additional Resources */}
        {additionalResources.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.4 }}
            className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20"
          >
            <h2 className="text-2xl font-bold text-white mb-4 flex items-center gap-2">
              <FileText className="w-6 h-6 text-purple-400" />
              Дополнительные материалы
            </h2>
            <div className="space-y-3">
              {additionalResources.map((resource: any, index: number) => (
                <a
                  key={index}
                  href={resource.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="block p-4 bg-white/5 rounded-lg border border-white/10 hover:bg-white/10 transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <Video className="w-5 h-5 text-blue-400" />
                    <div>
                      <div className="text-white font-medium">{resource.title || resource.url}</div>
                      {resource.description && (
                        <div className="text-white/60 text-sm">{resource.description}</div>
                      )}
                    </div>
                  </div>
                </a>
              ))}
            </div>
          </motion.div>
        )}
      </div>
    </div>
  );
};

export default TheoryPage;

