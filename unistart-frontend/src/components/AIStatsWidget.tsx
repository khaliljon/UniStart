import { useEffect, useState } from 'react';
import { Brain, TrendingUp, Target, Lightbulb } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import Card from './common/Card';
import Button from './common/Button';
import aiService from '../services/aiService';

const AIStatsWidget = () => {
  const navigate = useNavigate();
  const [nextTopic, setNextTopic] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadAIStats();
  }, []);

  const loadAIStats = async () => {
    try {
      const topic = await aiService.getNextTopic();
      setNextTopic(topic?.topic || null);
    } catch (error: any) {
      // Игнорируем 404 - backend может быть не запущен
      if (error?.response?.status !== 404) {
        console.error('Ошибка загрузки AI статистики:', error);
      }
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <Card>
        <div className="animate-pulse">
          <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
          <div className="h-4 bg-gray-200 rounded w-1/2"></div>
        </div>
      </Card>
    );
  }

  return (
    <Card className="bg-gradient-to-br from-purple-500 to-blue-600 text-white">
      <div className="flex items-start gap-4">
        <div className="p-3 bg-white/20 rounded-lg">
          <Brain className="w-8 h-8" />
        </div>
        <div className="flex-1">
          <h3 className="text-xl font-bold mb-2">AI Ассистент</h3>
          {nextTopic ? (
            <>
              <div className="flex items-center gap-2 mb-2">
                <Target className="w-5 h-5" />
                <p className="text-sm opacity-90">Следующая тема:</p>
              </div>
              <p className="font-semibold text-lg mb-3">{nextTopic}</p>
              <Button
                variant="ghost"
                className="bg-white/20 hover:bg-white/30 text-white border-0"
                onClick={() => navigate('/ai/dashboard')}
              >
                <TrendingUp className="w-4 h-4 mr-2" />
                Открыть AI панель
              </Button>
            </>
          ) : (
            <>
              <div className="flex items-center gap-2 mb-2">
                <Lightbulb className="w-5 h-5" />
                <p className="text-sm opacity-90">
                  Персонализированные рекомендации готовы
                </p>
              </div>
              <Button
                variant="ghost"
                className="bg-white/20 hover:bg-white/30 text-white border-0 mt-2"
                onClick={() => navigate('/ai/dashboard')}
              >
                <Brain className="w-4 h-4 mr-2" />
                Посмотреть рекомендации
              </Button>
            </>
          )}
        </div>
      </div>
    </Card>
  );
};

export default AIStatsWidget;
