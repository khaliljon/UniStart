import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { Brain, Sparkles, TrendingUp, ArrowRight } from 'lucide-react';
import Card from '../common/Card';
import Button from '../common/Button';
import aiService, { RecommendedFlashcard } from '../../services/aiService';
import { useNavigate } from 'react-router-dom';

const AIRecommendedFlashcards = () => {
  const navigate = useNavigate();
  const [recommendations, setRecommendations] = useState<RecommendedFlashcard[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadRecommendations();
  }, []);

  const loadRecommendations = async () => {
    try {
      const data = await aiService.getRecommendedFlashcards(3);
      // Показываем только если есть реальные рекомендации
      setRecommendations(Array.isArray(data) && data.length > 0 ? data : []);
    } catch (error: any) {
      // Игнорируем 404 - backend может быть не запущен
      if (error?.response?.status !== 404) {
        console.error('Ошибка загрузки AI рекомендаций:', error);
      }
      setRecommendations([]);
    } finally {
      setLoading(false);
    }
  };

  if (loading || recommendations.length === 0) {
    return null;
  }

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      className="mb-8"
    >
      <Card className="bg-gradient-to-br from-gray-700 to-gray-800 dark:from-gray-800 dark:to-gray-900 text-white">
        <div className="mb-4">
          <div className="flex items-center gap-2 mb-2">
            <Brain className="w-6 h-6" />
            <h3 className="text-xl font-bold">AI Рекомендации</h3>
          </div>
          <p className="text-sm opacity-90">
            Карточки, подобранные специально для вас на основе ваших слабых мест
          </p>
        </div>

        <div className="space-y-3">
          {recommendations.map((rec) => (
            <div
              key={rec.id}
              className="p-4 bg-white/20 backdrop-blur-sm rounded-lg hover:bg-white/30 transition-colors cursor-pointer border border-white/30"
              onClick={() => navigate(`/flashcards/${rec.id}/study`)}
            >
              <div className="flex items-start justify-between mb-2">
                <div className="flex-1">
                  <p className="font-semibold text-lg text-white">{rec.title}</p>
                  <p className="text-sm text-white/95">{rec.subject}</p>
                </div>
                <ArrowRight className="w-5 h-5 flex-shrink-0 text-white" />
              </div>
              <div className="flex items-start gap-2 mb-2">
                <Sparkles className="w-4 h-4 flex-shrink-0 mt-0.5 text-white" />
                <p className="text-sm text-white/95">{rec.recommendationReason}</p>
              </div>
              <div className="flex items-center gap-4 text-xs text-white/90">
                <span>{rec.cardsCount} карточек</span>
                <span>~{rec.estimatedStudyTime} мин</span>
              </div>
            </div>
          ))}
        </div>

        <div className="mt-4 pt-4 border-t border-white/20">
          <Button
            variant="ghost"
            className="w-full text-white hover:bg-white/10"
            onClick={() => navigate('/ai/dashboard')}
          >
            <TrendingUp className="w-4 h-4 mr-2" />
            Все AI рекомендации
          </Button>
        </div>
      </Card>
    </motion.div>
  );
};

export default AIRecommendedFlashcards;
