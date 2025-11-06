import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { GraduationCap } from 'lucide-react'
import Button from '../components/common/Button'
import Input from '../components/common/Input'

const Register = () => {
  const navigate = useNavigate()
  const { register } = useAuth()
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
  })
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    })
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)

    try {
      await register(formData)
      navigate('/dashboard')
    } catch (err: any) {
      setError(err.response?.data?.message || 'Ошибка при регистрации')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-primary-50 to-purple-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl shadow-xl p-8 w-full max-w-md">
        {/* Logo */}
        <div className="flex justify-center mb-8">
          <div className="flex items-center space-x-2">
            <GraduationCap className="w-10 h-10 text-primary-500" />
            <span className="text-3xl font-bold text-gray-900">UniStart</span>
          </div>
        </div>

        {/* Title */}
        <h2 className="text-2xl font-bold text-center text-gray-900 mb-8">
          Создать аккаунт
        </h2>

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 text-red-600 p-3 rounded-lg mb-4 text-sm">
            {error}
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Input
              type="text"
              name="firstName"
              label="Имя"
              placeholder="Айдар"
              value={formData.firstName}
              onChange={handleChange}
              required
            />

            <Input
              type="text"
              name="lastName"
              label="Фамилия"
              placeholder="Айдаров"
              value={formData.lastName}
              onChange={handleChange}
              required
            />
          </div>

          <Input
            type="email"
            name="email"
            label="Email"
            placeholder="your@email.com"
            value={formData.email}
            onChange={handleChange}
            required
          />

          <Input
            type="password"
            name="password"
            label="Пароль"
            placeholder="••••••••"
            value={formData.password}
            onChange={handleChange}
            required
            minLength={6}
          />

          <Button
            type="submit"
            className="w-full"
            isLoading={isLoading}
          >
            Зарегистрироваться
          </Button>
        </form>

        {/* Login Link */}
        <p className="text-center mt-6 text-gray-600">
          Уже есть аккаунт?{' '}
          <Link to="/login" className="text-primary-500 font-medium hover:text-primary-600">
            Войти
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

export default Register
