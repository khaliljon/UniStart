import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { GraduationCap, BookOpen, Brain, TrendingUp } from 'lucide-react'
import Button from '../components/common/Button'

const Home = () => {
  return (
    <div className="min-h-screen bg-gradient-to-br from-primary-50 via-white to-purple-50">
      {/* Header */}
      <nav className="container mx-auto px-4 py-6 flex justify-between items-center">
        <div className="flex items-center space-x-2">
          <GraduationCap className="w-8 h-8 text-primary-500" />
          <span className="text-2xl font-bold text-gray-900">
            <span className="inline-flex items-center justify-center w-8 h-8 bg-gradient-to-br from-primary-500 to-primary-600 rounded text-white mr-0.5">U</span>
            <span>niStart</span>
          </span>
        </div>
        <div className="flex items-center space-x-4">
          <Link to="/login">
            <Button variant="secondary" size="sm">Войти</Button>
          </Link>
          <Link to="/register">
            <Button variant="primary" size="sm">Регистрация</Button>
          </Link>
        </div>
      </nav>

      {/* Hero Section */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.6 }}
        className="container mx-auto px-4 py-20 text-center"
      >
        <h1 className="text-6xl font-bold text-gray-900 mb-6">
          Твой путь в <span className="text-primary-500">университет мечты</span>
        </h1>
        <p className="text-xl text-gray-600 mb-12 max-w-2xl mx-auto">
          Современная платформа для подготовки к поступлению в Назарбаевский Университет 
          и другие ведущие вузы Казахстана
        </p>
        <div className="flex justify-center space-x-4">
          <Link to="/register">
            <Button size="lg">Начать бесплатно</Button>
          </Link>
          <Button variant="secondary" size="lg">Узнать больше</Button>
        </div>
      </motion.div>

      {/* Features */}
      <div className="container mx-auto px-4 py-16">
        <div className="grid md:grid-cols-3 gap-8">
          {/* Feature 1 */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2, duration: 0.5 }}
            className="bg-white p-8 rounded-xl shadow-lg hover:shadow-xl transition-shadow"
          >
            <div className="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center mb-4">
              <BookOpen className="w-6 h-6 text-primary-500" />
            </div>
            <h3 className="text-xl font-bold mb-3">Интерактивные карточки</h3>
            <p className="text-gray-600">
              Изучай материал с помощью умного алгоритма интервального повторения. 
              Эффективность обучения +40%!
            </p>
          </motion.div>

          {/* Feature 2 */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3, duration: 0.5 }}
            className="bg-white p-8 rounded-xl shadow-lg hover:shadow-xl transition-shadow"
          >
            <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center mb-4">
              <Brain className="w-6 h-6 text-purple-500" />
            </div>
            <h3 className="text-xl font-bold mb-3">Тесты и симуляции</h3>
            <p className="text-gray-600">
              Проходи тесты в формате ЕНТ. Получай детальную аналитику и 
              рекомендации по улучшению результатов.
            </p>
          </motion.div>

          {/* Feature 3 */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.4, duration: 0.5 }}
            className="bg-white p-8 rounded-xl shadow-lg hover:shadow-xl transition-shadow"
          >
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center mb-4">
              <TrendingUp className="w-6 h-6 text-green-500" />
            </div>
            <h3 className="text-xl font-bold mb-3">Отслеживай прогресс</h3>
            <p className="text-gray-600">
              Визуализируй свои достижения с помощью графиков и статистики. 
              Мотивируйся каждый день!
            </p>
          </motion.div>
        </div>
      </div>

      {/* Stats Section */}
      <div className="bg-primary-500 text-white py-16 mt-16">
        <div className="container mx-auto px-4">
          <div className="grid md:grid-cols-3 gap-8 text-center">
            <div>
              <div className="text-4xl font-bold mb-2">1000+</div>
              <div className="text-primary-100">Студентов</div>
            </div>
            <div>
              <div className="text-4xl font-bold mb-2">500+</div>
              <div className="text-primary-100">Карточек</div>
            </div>
            <div>
              <div className="text-4xl font-bold mb-2">50+</div>
              <div className="text-primary-100">Тестов</div>
            </div>
          </div>
        </div>
      </div>

      {/* Footer */}
      <footer className="bg-gray-900 text-white py-8 mt-16">
        <div className="container mx-auto px-4 text-center">
          <p className="text-gray-400">
            © 2025 UniStart. Сделано с ❤️ для студентов Казахстана
          </p>
        </div>
      </footer>
    </div>
  )
}

export default Home
