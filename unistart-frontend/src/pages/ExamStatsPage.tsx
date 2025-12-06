import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { motion } from 'framer-motion'
import { 
  ArrowLeft, 
  Users, 
  TrendingUp, 
  Clock, 
  Award,
  BarChart3,
  CheckCircle,
  XCircle,
  Edit
} from 'lucide-react'
import Button from '../components/common/Button'
import api from '../services/api'

interface ExamStats {
  examId: number
  examTitle: string
  totalAttempts: number
  averageScore: number
  averageTimeSpent: number
  passRate: number
  questionStats: QuestionStat[]
  recentAttempts: RecentAttempt[]
}

interface QuestionStat {
  questionId: number
  questionText: string
  correctAnswers: number
  totalAnswers: number
  successRate: number
}

interface RecentAttempt {
  id: number
  studentName: string
  score: number
  maxScore: number
  percentage: number
  timeSpent: number
  completedAt: string
}

const ExamStatsPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [loading, setLoading] = useState(true)
  const [stats, setStats] = useState<ExamStats | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    loadStats()
  }, [id])

  const loadStats = async () => {
    try {
      setLoading(true)
      setError(null)
      const response = await api.get(`/exams/${id}/stats`)
      setStats(response.data)
    } catch (err: any) {
      console.error('Ошибка загрузки статистики:', err)
      setError(err.response?.data?.message || 'Не удалось загрузить статистику экзамена')
    } finally {
      setLoading(false)
    }
  }

  const formatTime = (seconds: number) => {
    const minutes = Math.floor(seconds / 60)
    const secs = seconds % 60
    return `${minutes}м ${secs}с`
  }

  const formatDate = (dateString: string) => {
    const date = new Date(dateString)
    return date.toLocaleDateString('ru-RU', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 flex items-center justify-center">
        <div className="text-white text-xl">Загрузка статистики...</div>
      </div>
    )
  }

  if (error || !stats) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 flex items-center justify-center">
        <div className="text-center">
          <XCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <h2 className="text-white text-2xl mb-4">{error || 'Статистика не найдена'}</h2>
          <button
            onClick={() => navigate(-1)}
            className="px-6 py-2 bg-white/10 hover:bg-white/20 text-white rounded-lg transition-colors"
          >
            Вернуться назад
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 py-8 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="mb-8 flex items-center justify-between"
        >
          <div>
            <button
              onClick={() => navigate(-1)}
              className="flex items-center gap-2 text-white/70 hover:text-white mb-4 transition-colors"
            >
              <ArrowLeft className="w-5 h-5" />
              Назад
            </button>
            <h1 className="text-4xl font-bold text-white mb-2">{stats.examTitle}</h1>
            <p className="text-white/60">Статистика и аналитика экзамена</p>
          </div>
          <Button
            onClick={() => navigate(`/exams/${id}/edit`)}
            variant="secondary"
            className="flex items-center gap-2"
          >
            <Edit className="w-4 h-4" />
            Редактировать
          </Button>
        </motion.div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.1 }}
            className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20"
          >
            <div className="flex items-center gap-3 mb-2">
              <Users className="w-8 h-8 text-blue-400" />
              <div>
                <p className="text-white/60 text-sm">Всего попыток</p>
                <p className="text-3xl font-bold text-white">{stats.totalAttempts}</p>
              </div>
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2 }}
            className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20"
          >
            <div className="flex items-center gap-3 mb-2">
              <TrendingUp className="w-8 h-8 text-green-400" />
              <div>
                <p className="text-white/60 text-sm">Средний балл</p>
                <p className="text-3xl font-bold text-white">{stats.averageScore.toFixed(1)}%</p>
              </div>
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
            className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20"
          >
            <div className="flex items-center gap-3 mb-2">
              <Clock className="w-8 h-8 text-yellow-400" />
              <div>
                <p className="text-white/60 text-sm">Среднее время</p>
                <p className="text-3xl font-bold text-white">{formatTime(stats.averageTimeSpent)}</p>
              </div>
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.4 }}
            className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20"
          >
            <div className="flex items-center gap-3 mb-2">
              <Award className="w-8 h-8 text-purple-400" />
              <div>
                <p className="text-white/60 text-sm">% сдавших</p>
                <p className="text-3xl font-bold text-white">{stats.passRate.toFixed(1)}%</p>
              </div>
            </div>
          </motion.div>
        </div>

        {/* Questions Stats */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}
          className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20 mb-8"
        >
          <div className="flex items-center gap-3 mb-6">
            <BarChart3 className="w-6 h-6 text-white" />
            <h2 className="text-2xl font-bold text-white">Статистика по вопросам</h2>
          </div>
          <div className="space-y-4">
            {stats.questionStats.map((question, index) => (
              <div key={question.questionId} className="bg-white/5 rounded-lg p-4">
                <div className="flex items-start justify-between mb-3">
                  <div className="flex-1">
                    <p className="text-white font-medium mb-1">
                      Вопрос {index + 1}
                    </p>
                    <p className="text-white/70 text-sm">{question.questionText}</p>
                  </div>
                  <div className="ml-4">
                    <div className={`px-3 py-1 rounded-full text-sm font-medium ${
                      question.successRate >= 70
                        ? 'bg-green-500/20 text-green-400'
                        : question.successRate >= 40
                        ? 'bg-yellow-500/20 text-yellow-400'
                        : 'bg-red-500/20 text-red-400'
                    }`}>
                      {question.successRate.toFixed(1)}%
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-4 text-sm">
                  <div className="flex items-center gap-2 text-green-400">
                    <CheckCircle className="w-4 h-4" />
                    <span>{question.correctAnswers} правильных</span>
                  </div>
                  <div className="flex items-center gap-2 text-red-400">
                    <XCircle className="w-4 h-4" />
                    <span>{question.totalAnswers - question.correctAnswers} неправильных</span>
                  </div>
                </div>
                {/* Progress bar */}
                <div className="mt-3 h-2 bg-white/10 rounded-full overflow-hidden">
                  <div
                    className={`h-full transition-all ${
                      question.successRate >= 70
                        ? 'bg-green-500'
                        : question.successRate >= 40
                        ? 'bg-yellow-500'
                        : 'bg-red-500'
                    }`}
                    style={{ width: `${question.successRate}%` }}
                  />
                </div>
              </div>
            ))}
          </div>
        </motion.div>

        {/* Recent Attempts */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.6 }}
          className="bg-white/10 backdrop-blur-lg rounded-xl p-6 border border-white/20"
        >
          <div className="flex items-center gap-3 mb-6">
            <Users className="w-6 h-6 text-white" />
            <h2 className="text-2xl font-bold text-white">Последние попытки</h2>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b border-white/20">
                  <th className="text-left text-white/60 font-medium py-3 px-4">Студент</th>
                  <th className="text-left text-white/60 font-medium py-3 px-4">Балл</th>
                  <th className="text-left text-white/60 font-medium py-3 px-4">Процент</th>
                  <th className="text-left text-white/60 font-medium py-3 px-4">Время</th>
                  <th className="text-left text-white/60 font-medium py-3 px-4">Дата</th>
                </tr>
              </thead>
              <tbody>
                {stats.recentAttempts.map((attempt) => (
                  <tr key={attempt.id} className="border-b border-white/10 hover:bg-white/5 transition-colors">
                    <td className="py-3 px-4 text-white">{attempt.studentName}</td>
                    <td className="py-3 px-4 text-white">{attempt.score}/{attempt.maxScore}</td>
                    <td className="py-3 px-4">
                      <span className={`px-2 py-1 rounded-full text-sm font-medium ${
                        attempt.percentage >= 70
                          ? 'bg-green-500/20 text-green-400'
                          : attempt.percentage >= 40
                          ? 'bg-yellow-500/20 text-yellow-400'
                          : 'bg-red-500/20 text-red-400'
                      }`}>
                        {attempt.percentage.toFixed(1)}%
                      </span>
                    </td>
                    <td className="py-3 px-4 text-white/70">{formatTime(attempt.timeSpent)}</td>
                    <td className="py-3 px-4 text-white/70">{formatDate(attempt.completedAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            {stats.recentAttempts.length === 0 && (
              <div className="text-center py-8 text-white/60">
                Пока нет попыток прохождения
              </div>
            )}
          </div>
        </motion.div>
      </div>
    </div>
  )
}

export default ExamStatsPage
