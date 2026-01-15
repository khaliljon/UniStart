import { useEffect, useState } from 'react';
import { motion } from 'framer-motion';
import { Brain, Sparkles, TrendingUp, ArrowRight, Clock } from 'lucide-react';
import Card from '../common/Card';
import Button from '../common/Button';
import aiService, { RecommendedExam } from '../../services/aiService';
import { useNavigate } from 'react-router-dom';

const AIRecommendedExams = () => {
  const navigate = useNavigate();
  const [recommendations, setRecommendations] = useState<RecommendedExam[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadRecommendations();
  }, []);

  const loadRecommendations = async () => {
    try {
      const data = await aiService.getRecommendedExams(3);
      setRecommendations(Array.isArray(data) ? data : []);
    } catch (error: any) {
      // Игнорируем 404 - backend может быть не запущен
      if (error?.response?.status !== 404) {
        console.error('Ошибка загрузки AI рекомендаций:', error);
      }
    } finally {
      setLoading(false);
    }
  };

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty.toLowerCase()) {
      case 'hard':
        return 'bg-red-100 text-red-700';
      case 'medium':
        return 'bg-yellow-100 text-yellow-700';
      case 'easy':
        return 'bg-green-100 text-green-700';
      default:
        return 'bg-gray-100 text-gray-700';
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
      <Card className="bg-gradient-to-br from-orange-500 to-red-600 text-white">
        <div className="mb-4">
          <div className="flex items-center gap-2 mb-2">
            <Brain className="w-6 h-6" />
            <h3 className="text-xl font-bold">AI Рекомендации</h3>
          </div>
          <p className="text-sm opacity-90">
            Экзамены для проверки ваших знаний
          </p>
        </div>

        <div className="space-y-3">
          {recommendations.map((rec) => (
            <div
              key={rec.id}
              className="p-4 bg-white/20 backdrop-blur-sm rounded-lg hover:bg-white/30 transition-colors cursor-pointer border border-white/30"
              onClick={() => navigate(`/exams/${rec.id}/take`)}
            >
              <div className="flex items-start justify-between mb-2">
                <div className="flex-1">
                  <p className="font-semibold text-lg text-white">{rec.title}</p>
                  <p className="text-sm text-white/95">{rec.subject}</p>
                </div>
                <div className="flex items-center gap-2">
                  <span className={`px-2 py-1 text-xs font-semibold rounded ${getDifficultyColor(rec.difficulty)}`}>
                    {rec.difficulty}
                  </span>
                  <ArrowRight className="w-5 h-5 flex-shrink-0 text-white" />
                </div>
              </div>
              <div className="flex items-start gap-2 mb-2">
                <Sparkles className="w-4 h-4 flex-shrink-0 mt-0.5 text-white" />
                <p className="text-sm text-white/95">{rec.recommendationReason}</p>
              </div>
              <div className="flex items-center gap-4 text-xs text-white/90">
                <span>{rec.questionsCount} вопросов</span>
                <div className="flex items-center gap-1">
                  <Clock className="w-3 h-3" />
                  <span>{rec.duration} мин</span>
                </div>
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

export default AIRecommendedExams;
