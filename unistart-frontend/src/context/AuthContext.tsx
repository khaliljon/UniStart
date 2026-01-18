import { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { authService } from '../services/authService'
import { User, LoginDto, RegisterDto } from '../types'

interface AuthContextType {
  user: User | null
  token: string | null
  login: (credentials: LoginDto) => Promise<void>
  register: (data: RegisterDto) => Promise<void>
  logout: () => void
  refreshUser: () => Promise<void>
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

  console.log('🚀 AuthProvider: Компонент рендерится, loading:', loading, 'user:', user?.email);

  // Функция для извлечения ролей из разных источников
  const getUserRoles = (profileData: any, jwtToken: string): string[] => {
    console.log('🔍 getUserRoles вызвана с profileData:', profileData);
    
    // 1. Пытаемся получить из ответа профиля
    let roles: string[] = [];
    
    if (Array.isArray(profileData.roles)) {
      roles = profileData.roles;
      console.log('✅ Роли найдены в profileData.roles:', roles);
    } else if (Array.isArray(profileData.Roles)) {
      roles = profileData.Roles;
      console.log('✅ Роли найдены в profileData.Roles:', roles);
    }
    
    if (roles.length > 0) {
      console.log('🎭 Роли найдены в профиле:', roles);
      return roles;
    }

    // 2. Извлекаем из JWT токена
    console.log('🔍 Роли не найдены в профиле, пытаемся извлечь из JWT...');
    const jwtPayload = parseJwt(jwtToken);
    console.log('🔍 JWT payload:', jwtPayload);
    
    if (jwtPayload) {
      // Проверяем разные варианты ключей для ролей
      const possibleRoleKeys = [
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
        'role',
        'roles',
        'Role',
        'Roles'
      ];

      for (const key of possibleRoleKeys) {
        const roleClaim = jwtPayload[key];
        if (roleClaim) {
          roles = Array.isArray(roleClaim) ? roleClaim : [roleClaim];
          if (roles.length > 0) {
            console.log(`🎭 Роли найдены в JWT (ключ: ${key}):`, roles);
            return roles;
          }
        }
      }
    }

    // 3. Дефолтная роль Student
    console.log('⚠️ Роли не найдены, используем дефолтную: Student');
    return ['Student'];
  };

  useEffect(() => {
    console.log('⚡ useEffect запущен!');
    
    const initializeAuth = async () => {
      console.log('🔐 AuthContext: Инициализация...');
      
      try {
        const storedToken = localStorage.getItem('token');
        console.log('🔍 Проверка localStorage.token:', storedToken ? 'НАЙДЕН' : 'НЕ НАЙДЕН');
        
        if (storedToken) {
          console.log('✅ Токен найден в localStorage, длина:', storedToken.length);
          console.log('📝 Первые 30 символов токена:', storedToken.substring(0, 30) + '...');
          
          setToken(storedToken);
          console.log('📝 setToken вызван');
          
          console.log('📞 Вызываем loadUser...');
          await loadUser(storedToken);
          console.log('✅ loadUser завершен');
        } else {
          console.log('❌ Токен не найден в localStorage');
          setLoading(false);
          console.log('📝 setLoading(false) вызван');
        }
      } catch (error) {
        console.error('💥 Ошибка в initializeAuth:', error);
        setLoading(false);
      }
    };

    initializeAuth();
    
    return () => {
      console.log('🧹 useEffect cleanup');
    };
  }, [])

  const loadUser = async (authToken: string) => {
    console.log('👤 === loadUser НАЧАЛО ===');
    console.log('👤 Параметр authToken:', authToken ? `ЕСТЬ (длина: ${authToken.length})` : 'ОТСУТСТВУЕТ');
    
    if (!authToken) {
      console.error('❌ loadUser: токен не передан!');
      setLoading(false);
      return;
    }

    console.log('👤 Загружаем профиль пользователя...');
    console.log('🔑 Токен для загрузки:', authToken.substring(0, 20) + '...');
    
    try {
      console.log('📡 Отправляем запрос authService.getProfile()...');
      
      // Загружаем профиль пользователя
      const userData = await authService.getProfile()
      
      console.log('✅ Профиль загружен успешно!');
      console.log('📦 userData:', userData);
      console.log('📦 userData.Roles:', (userData as any).Roles);
      console.log('📦 userData.roles:', (userData as any).roles);
      
      // Извлекаем роли из профиля и JWT
      console.log('🔍 Вызываем getUserRoles...');
      const roles = getUserRoles(userData, authToken);
      console.log('✅ getUserRoles вернул:', roles);
      
      const userWithRoles = { ...userData, roles };
      console.log('✅ Пользователь с ролями:', userWithRoles);
      console.log('📝 Вызываем setUser...');
      setUser(userWithRoles);
      console.log('✅ setUser вызван');
      
    } catch (error: any) {
      console.error('❌ === loadUser ОШИБКА ===');
      console.error('💥 Ошибка:', error);
      console.error('💥 Статус:', error.response?.status);
      console.error('💥 Данные:', error.response?.data);
      console.error('💥 Сообщение:', error.message);
      
      // Очищаем токен ТОЛЬКО при ошибке 401 (Unauthorized)
      if (error.response?.status === 401) {
        console.log('🧹 Ошибка 401: Очищаем токен из localStorage...');
        localStorage.removeItem('token')
        setToken(null)
        setUser(null)
      } else {
        console.log('⚠️ Ошибка не 401, токен не удаляем');
      }
    } finally {
      console.log('📝 setLoading(false)...');
      setLoading(false)
      console.log('✅ === loadUser КОНЕЦ ===');
    }
  }

  const login = async (credentials: LoginDto) => {
    try {
      const response = await authService.login(credentials)
      localStorage.setItem('token', response.token)
      setToken(response.token)
      await loadUser(response.token)
    } catch (error) {
      // Пробрасываем ошибку дальше, чтобы Login.tsx мог её обработать
      throw error
    }
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

  const refreshUser = async () => {
    const currentToken = token || localStorage.getItem('token')
    if (currentToken) {
      await loadUser(currentToken)
    }
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
        refreshUser,
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
