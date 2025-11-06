import { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { authService } from '../services/authService'
import { User, LoginDto, RegisterDto } from '../types'

interface AuthContextType {
  user: User | null
  token: string | null
  login: (credentials: LoginDto) => Promise<void>
  register: (data: RegisterDto) => Promise<void>
  logout: () => void
  isAuthenticated: boolean
  loading: boolean
  hasRole: (role: string) => boolean
  isAdmin: boolean
  isTeacher: boolean
  isStudent: boolean
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined)

// Функция для парсинга JWT токена и извлечения ролей
const parseJwt = (token: string) => {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
};

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<User | null>(null)
  const [token, setToken] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    console.log('🔐 AuthContext: Проверяем токен при загрузке...');
    // Проверяем токен при загрузке приложения
    const storedToken = localStorage.getItem('token')
    if (storedToken) {
      console.log('✅ Токен найден в localStorage');
      setToken(storedToken)
      loadUser(storedToken)
    } else {
      console.log('❌ Токен не найден');
      setLoading(false)
    }
  }, [])

  const loadUser = async (authToken: string = token || '') => {
    console.log('👤 Загружаем профиль пользователя...');
    try {
      // Загружаем профиль пользователя
      const userData = await authService.getProfile()
      console.log('✅ Профиль загружен:', userData);
      
      // Получаем роли из API
      const rolesData = await authService.getRoles()
      console.log('🎭 Роли получены:', rolesData);
      
      // Парсим JWT чтобы получить роли из токена (fallback)
      const jwtPayload = parseJwt(authToken);
      let roles: string[] = rolesData.roles || [];
      
      // Если в API нет ролей, пытаемся извлечь из JWT
      if (roles.length === 0 && jwtPayload) {
        const roleClaimKey = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
        const roleClaim = jwtPayload[roleClaimKey];
        if (roleClaim) {
          roles = Array.isArray(roleClaim) ? roleClaim : [roleClaim];
        }
        console.log('🎭 Роли из JWT:', roles);
      }
      
      console.log('✅ Пользователь установлен:', { ...userData, roles });
      setUser({ ...userData, roles })
    } catch (error) {
      console.error('❌ Failed to load user:', error)
      localStorage.removeItem('token')
      setToken(null)
    } finally {
      setLoading(false)
      console.log('✅ AuthContext загрузка завершена');
    }
  }

  const login = async (credentials: LoginDto) => {
    const response = await authService.login(credentials)
    localStorage.setItem('token', response.token)
    setToken(response.token)
    await loadUser(response.token)
  }

  const register = async (data: RegisterDto) => {
    const response = await authService.register(data)
    localStorage.setItem('token', response.token)
    setToken(response.token)
    await loadUser(response.token)
  }

  const logout = () => {
    localStorage.removeItem('token')
    setToken(null)
    setUser(null)
  }

  const hasRole = (role: string): boolean => {
    return user?.roles?.includes(role) || false
  }

  const isAdmin = hasRole('Admin')
  const isTeacher = hasRole('Teacher')
  const isStudent = hasRole('Student')

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        login,
        register,
        logout,
        isAuthenticated: !!token,
        loading,
        hasRole,
        isAdmin,
        isTeacher,
        isStudent,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return context
}
