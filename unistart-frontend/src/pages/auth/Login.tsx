import { useState, useEffect } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { GraduationCap } from 'lucide-react'
import Button from '../../components/common/Button'
import Input from '../../components/common/Input'

const Login = () => {
  const navigate = useNavigate()
  const { user, login, loading } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)

  // Редирект когда user установлен (только если загрузка завершена)
  useEffect(() => {
    if (!loading && user) {
      navigate('/dashboard', { replace: true })
    }
  }, [user, loading, navigate])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)

    try {
      await login({ email, password })
      // navigate УБРАН - редирект через useEffect
    } catch (err: any) {
      console.error('Login error:', err);
      
      // Проверяем различные типы ошибок
      const errorData = err.response?.data;
      const status = err.response?.status;
      
      let errorMessage = 'Неверный email или пароль';
      
      if (status === 401) {
        // Проверяем, заблокирован ли пользователь
        if (errorData?.message?.toLowerCase().includes('заблокирован') || 
            errorData?.message?.toLowerCase().includes('locked') ||
            errorData?.message?.toLowerCase().includes('lockout')) {
          errorMessage = '🔒 Ваш аккаунт заблокирован. Обратитесь к администратору для разблокировки.';
        } else {
          errorMessage = errorData?.message || 'Неверный email или пароль';
        }
      } else if (errorData?.message) {
        errorMessage = errorData.message;
      }
      
      setError(errorMessage);
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-blue-50 dark:from-gray-900 dark:to-gray-800 flex items-center justify-center p-4">
      <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-8 w-full max-w-md">
        {/* Logo */}
        <div className="flex justify-center mb-8">
          <div className="flex items-center space-x-2">
            <GraduationCap className="w-10 h-10 text-primary-500" />
            <span className="text-3xl font-bold text-gray-900 dark:text-white">
              <span className="inline-flex items-center justify-center w-10 h-10 bg-gradient-to-br from-primary-500 to-primary-600 rounded text-white mr-1">U</span>
              <span>niStart</span>
            </span>
          </div>
        </div>

        {/* Title */}
        <h2 className="text-2xl font-bold text-center text-gray-900 dark:text-white mb-8">
          Войти в аккаунт
        </h2>

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 text-red-600 p-3 rounded-lg mb-4 text-sm">
            {error}
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            type="email"
            label="Email"
            placeholder="your@email.com"
            value={email}
            onChange={(e) => {
              setEmail(e.target.value)
              setError('') // Очищаем ошибку при вводе
            }}
            required
          />

          <Input
            type="password"
            label="Пароль"
            placeholder="••••••••"
            value={password}
            onChange={(e) => {
              setPassword(e.target.value)
              setError('') // Очищаем ошибку при вводе
            }}
            required
          />

          <Button
            type="submit"
            className="w-full"
            isLoading={isLoading}
          >
            Войти
          </Button>
        </form>

        {/* Register Link */}
        <p className="text-center mt-6 text-gray-600 dark:text-gray-400">
          Нет аккаунта?{' '}
          <Link to="/register" className="text-primary-500 font-medium hover:text-primary-600">
            Зарегистрироваться
          </Link>
        </p>

        {/* Back to Home */}
        <Link to="/">
          <Button variant="secondary" className="w-full mt-4">
            На главную
          </Button>
        </Link>
      </div>
    </div>
  )
}

export default Login
