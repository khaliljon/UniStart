import axios from 'axios';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5220/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Интерцептор для добавления JWT токена к каждому запросу
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Интерцептор для обработки ошибок
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Токен истек или невалиден - очищаем
      localStorage.removeItem('token');
      
      // Редиректим на login только если мы не уже на странице логина/регистрации
      const currentPath = window.location.pathname;
      if (currentPath !== '/login' && currentPath !== '/register' && currentPath !== '/') {
        // Используем replace вместо href чтобы избежать полной перезагрузки страницы
        window.location.replace('/login');
      }
    }
    return Promise.reject(error);
  }
);

export default api;
