# 🎨 UniStart Frontend - Руководство по разработке

## 🚀 SETUP ПРОЕКТА

### Создание React + TypeScript проекта

```bash
# С использованием Vite (рекомендуется - быстрее)
npm create vite@latest unistart-frontend -- --template react-ts
cd unistart-frontend
npm install

# Установка зависимостей
npm install react-router-dom axios
npm install -D tailwindcss postcss autoprefixer
npm install react-hook-form zod @hookform/resolvers
npm install recharts react-icons
npm install @tanstack/react-query

# Настройка Tailwind CSS
npx tailwindcss init -p
```

---

## 📁 СТРУКТУРА ПРОЕКТА

```
frontend/
├── public/
├── src/
│   ├── components/
│   │   ├── common/           # Переиспользуемые компоненты
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Card.tsx
│   │   │   ├── Modal.tsx
│   │   │   └── Loader.tsx
│   │   ├── layout/           # Layout компоненты
│   │   │   ├── Header.tsx
│   │   │   ├── Footer.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── Navigation.tsx
│   │   ├── flashcards/       # Flashcards компоненты
│   │   │   ├── FlashcardList.tsx
│   │   │   ├── FlashcardItem.tsx
│   │   │   ├── FlashcardStudy.tsx
│   │   │   └── FlashcardForm.tsx
│   │   └── quiz/             # Quiz компоненты
│   │       ├── QuizList.tsx
│   │       ├── QuizCard.tsx
│   │       ├── QuestionItem.tsx
│   │       └── QuizResult.tsx
│   ├── pages/                # Страницы (routes)
│   │   ├── Home.tsx
│   │   ├── Login.tsx
│   │   ├── Register.tsx
│   │   ├── Dashboard.tsx
│   │   ├── FlashcardsPage.tsx
│   │   ├── FlashcardStudyPage.tsx
│   │   ├── QuizzesPage.tsx
│   │   ├── QuizTakingPage.tsx
│   │   └── ProfilePage.tsx
│   ├── services/             # API сервисы
│   │   ├── api.ts            # Axios instance
│   │   ├── authService.ts
│   │   ├── flashcardService.ts
│   │   └── quizService.ts
│   ├── hooks/                # Custom hooks
│   │   ├── useAuth.ts
│   │   ├── useFlashcards.ts
│   │   └── useQuizzes.ts
│   ├── context/              # React Context
│   │   └── AuthContext.tsx
│   ├── types/                # TypeScript типы
│   │   └── index.ts
│   ├── utils/                # Утилиты
│   │   ├── dateFormatter.ts
│   │   └── validators.ts
│   ├── App.tsx
│   ├── main.tsx
│   └── index.css
├── .env
├── package.json
├── tailwind.config.js
├── tsconfig.json
└── vite.config.ts
```

---

## 🔧 НАСТРОЙКА

### 1. tailwind.config.js

```javascript
/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#f0f9ff',
          100: '#e0f2fe',
          500: '#0ea5e9',
          600: '#0284c7',
          700: '#0369a1',
        },
        success: '#10b981',
        warning: '#f59e0b',
        error: '#ef4444',
      },
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
      },
    },
  },
  plugins: [],
  darkMode: 'class', // Поддержка темной темы
}
```

### 2. .env

```env
VITE_API_BASE_URL=https://localhost:7xxx/api
```

### 3. src/services/api.ts

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Интерцептор для добавления токена
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
      // Токен истек - перенаправить на login
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
```

---

## 📝 TYPESCRIPT ТИПЫ

### src/types/index.ts

```typescript
// Синхронизировано с Backend DTOs!

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  totalCardsStudied: number;
  totalQuizzesTaken: number;
  createdAt: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  expiresAt: string;
}

// Flashcards
export interface FlashcardSet {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  updatedAt: string;
  totalCards: number;
  cardsToReview: number;
}

export interface Flashcard {
  id: number;
  question: string;
  answer: string;
  explanation: string;
  orderIndex: number;
  nextReviewDate?: string;
  lastReviewedAt?: string;
  isDueForReview: boolean;
}

export interface CreateFlashcardDto {
  question: string;
  answer: string;
  explanation: string;
  flashcardSetId: number;
}

export interface ReviewFlashcardDto {
  flashcardId: number;
  quality: number; // 0-5
}

// Quizzes
export interface Quiz {
  id: number;
  title: string;
  description: string;
  timeLimit: number;
  subject: string;
  difficulty: string;
  questionCount: number;
  totalPoints: number;
}

export interface QuizDetail {
  id: number;
  title: string;
  description: string;
  timeLimit: number;
  subject: string;
  difficulty: string;
  questions: Question[];
}

export interface Question {
  id: number;
  text: string;
  questionType: string;
  points: number;
  imageUrl?: string;
  answers: Answer[];
}

export interface Answer {
  id: number;
  text: string;
  isCorrect?: boolean; // null при отображении пользователю
}

export interface SubmitQuizDto {
  quizId: number;
  timeSpentSeconds: number;
  userAnswers: Record<number, number[]>; // questionId -> answerIds
}

export interface QuizResult {
  score: number;
  maxScore: number;
  percentage: number;
  timeSpentSeconds: number;
  questionResults: QuestionResult[];
}

export interface QuestionResult {
  questionId: number;
  questionText: string;
  isCorrect: boolean;
  pointsEarned: number;
  maxPoints: number;
  correctAnswerIds: number[];
  userAnswerIds: number[];
  explanation?: string;
}
```

---

## 🔐 AUTHENTICATION

### src/context/AuthContext.tsx

```typescript
import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { authService } from '../services/authService';
import { User, LoginDto, RegisterDto } from '../types';

interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (credentials: LoginDto) => Promise<void>;
  register: (data: RegisterDto) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  loading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Проверяем токен при загрузке
    const storedToken = localStorage.getItem('token');
    if (storedToken) {
      setToken(storedToken);
      loadUser();
    } else {
      setLoading(false);
    }
  }, []);

  const loadUser = async () => {
    try {
      const userData = await authService.getProfile();
      setUser(userData);
    } catch (error) {
      localStorage.removeItem('token');
      setToken(null);
    } finally {
      setLoading(false);
    }
  };

  const login = async (credentials: LoginDto) => {
    const response = await authService.login(credentials);
    localStorage.setItem('token', response.token);
    setToken(response.token);
    await loadUser();
  };

  const register = async (data: RegisterDto) => {
    const response = await authService.register(data);
    localStorage.setItem('token', response.token);
    setToken(response.token);
    await loadUser();
  };

  const logout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setUser(null);
  };

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
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
```

### src/services/authService.ts

```typescript
import api from './api';
import { LoginDto, RegisterDto, AuthResponse, User } from '../types';

export const authService = {
  async login(credentials: LoginDto): Promise<AuthResponse> {
    const { data } = await api.post<AuthResponse>('/auth/login', credentials);
    return data;
  },

  async register(userData: RegisterDto): Promise<AuthResponse> {
    const { data } = await api.post<AuthResponse>('/auth/register', userData);
    return data;
  },

  async getProfile(): Promise<User> {
    const { data } = await api.get<User>('/auth/profile');
    return data;
  },

  async updateProfile(updates: Partial<User>): Promise<void> {
    await api.put('/auth/profile', updates);
  },
};
```

---

## 🎴 FLASHCARDS КОМПОНЕНТЫ

### src/services/flashcardService.ts

```typescript
import api from './api';
import { FlashcardSet, Flashcard, CreateFlashcardDto, ReviewFlashcardDto } from '../types';

export const flashcardService = {
  async getSets(): Promise<FlashcardSet[]> {
    const { data } = await api.get<FlashcardSet[]>('/flashcards/sets');
    return data;
  },

  async getSet(id: number): Promise<FlashcardSet> {
    const { data } = await api.get<FlashcardSet>(`/flashcards/sets/${id}`);
    return data;
  },

  async getCardsForReview(setId: number): Promise<Flashcard[]> {
    const { data } = await api.get<Flashcard[]>(`/flashcards/sets/${setId}/review`);
    return data;
  },

  async createCard(dto: CreateFlashcardDto): Promise<Flashcard> {
    const { data } = await api.post<Flashcard>('/flashcards/cards', dto);
    return data;
  },

  async reviewCard(dto: ReviewFlashcardDto) {
    const { data } = await api.post('/flashcards/cards/review', dto);
    return data;
  },
};
```

### src/components/flashcards/FlashcardStudy.tsx

```typescript
import { useState } from 'react';
import { Flashcard } from '../../types';
import { flashcardService } from '../../services/flashcardService';

interface Props {
  flashcard: Flashcard;
  onNext: () => void;
}

export const FlashcardStudy = ({ flashcard, onNext }: Props) => {
  const [showAnswer, setShowAnswer] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleRating = async (quality: number) => {
    setLoading(true);
    try {
      await flashcardService.reviewCard({
        flashcardId: flashcard.id,
        quality,
      });
      onNext();
    } catch (error) {
      console.error('Failed to review flashcard', error);
    } finally {
      setLoading(false);
      setShowAnswer(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto p-6">
      <div className="bg-white rounded-lg shadow-lg p-8">
        {/* Вопрос */}
        <div className="text-center mb-8">
          <h2 className="text-2xl font-bold mb-4">
            {showAnswer ? 'Ответ' : 'Вопрос'}
          </h2>
          <p className="text-xl">
            {showAnswer ? flashcard.answer : flashcard.question}
          </p>
        </div>

        {/* Объяснение */}
        {showAnswer && flashcard.explanation && (
          <div className="bg-blue-50 p-4 rounded-lg mb-6">
            <p className="text-sm text-gray-700">{flashcard.explanation}</p>
          </div>
        )}

        {/* Кнопки */}
        {!showAnswer ? (
          <button
            onClick={() => setShowAnswer(true)}
            className="w-full bg-primary-500 text-white py-3 rounded-lg hover:bg-primary-600"
          >
            Показать ответ
          </button>
        ) : (
          <div className="space-y-2">
            <p className="text-center text-sm text-gray-600 mb-4">
              Насколько хорошо вы помните?
            </p>
            <div className="grid grid-cols-3 gap-2">
              <button
                onClick={() => handleRating(5)}
                className="bg-green-500 text-white py-2 rounded hover:bg-green-600"
                disabled={loading}
              >
                Отлично
              </button>
              <button
                onClick={() => handleRating(3)}
                className="bg-yellow-500 text-white py-2 rounded hover:bg-yellow-600"
                disabled={loading}
              >
                Хорошо
              </button>
              <button
                onClick={() => handleRating(0)}
                className="bg-red-500 text-white py-2 rounded hover:bg-red-600"
                disabled={loading}
              >
                Плохо
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
```

---

## 📝 QUIZ КОМПОНЕНТЫ

### src/services/quizService.ts

```typescript
import api from './api';
import { Quiz, QuizDetail, SubmitQuizDto, QuizResult } from '../types';

export const quizService = {
  async getQuizzes(filters?: { subject?: string; difficulty?: string }): Promise<Quiz[]> {
    const { data } = await api.get<Quiz[]>('/quizzes', { params: filters });
    return data;
  },

  async getQuiz(id: number): Promise<QuizDetail> {
    const { data } = await api.get<QuizDetail>(`/quizzes/${id}`);
    return data;
  },

  async submitQuiz(dto: SubmitQuizDto): Promise<QuizResult> {
    const { data } = await api.post<QuizResult>('/quizzes/submit', dto);
    return data;
  },
};
```

### src/components/quiz/QuestionItem.tsx

```typescript
import { Question } from '../../types';

interface Props {
  question: Question;
  selectedAnswers: number[];
  onAnswerSelect: (answerId: number) => void;
}

export const QuestionItem = ({ question, selectedAnswers, onAnswerSelect }: Props) => {
  const isMultipleChoice = question.questionType === 'MultipleChoice';

  const handleSelect = (answerId: number) => {
    if (isMultipleChoice) {
      // Toggle для множественного выбора
      if (selectedAnswers.includes(answerId)) {
        onAnswerSelect(answerId); // Remove
      } else {
        onAnswerSelect(answerId); // Add
      }
    } else {
      // Замена для одиночного выбора
      onAnswerSelect(answerId);
    }
  };

  return (
    <div className="bg-white p-6 rounded-lg shadow mb-4">
      <h3 className="text-lg font-semibold mb-4">
        {question.text}
        <span className="text-sm text-gray-500 ml-2">
          ({question.points} {question.points === 1 ? 'балл' : 'балла'})
        </span>
      </h3>

      <div className="space-y-2">
        {question.answers.map((answer) => (
          <label
            key={answer.id}
            className={`block p-3 border rounded-lg cursor-pointer hover:bg-gray-50 ${
              selectedAnswers.includes(answer.id)
                ? 'border-primary-500 bg-primary-50'
                : 'border-gray-300'
            }`}
          >
            <input
              type={isMultipleChoice ? 'checkbox' : 'radio'}
              name={`question-${question.id}`}
              checked={selectedAnswers.includes(answer.id)}
              onChange={() => handleSelect(answer.id)}
              className="mr-3"
            />
            {answer.text}
          </label>
        ))}
      </div>
    </div>
  );
};
```

---

## 🎨 UI КОМПОНЕНТЫ

### src/components/common/Button.tsx

```typescript
import { ButtonHTMLAttributes } from 'react';

interface Props extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger';
  size?: 'sm' | 'md' | 'lg';
}

export const Button = ({
  children,
  variant = 'primary',
  size = 'md',
  className = '',
  ...props
}: Props) => {
  const baseClasses = 'font-medium rounded-lg transition-colors';
  
  const variantClasses = {
    primary: 'bg-primary-500 text-white hover:bg-primary-600',
    secondary: 'bg-gray-200 text-gray-800 hover:bg-gray-300',
    danger: 'bg-red-500 text-white hover:bg-red-600',
  };

  const sizeClasses = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2',
    lg: 'px-6 py-3 text-lg',
  };

  return (
    <button
      className={`${baseClasses} ${variantClasses[variant]} ${sizeClasses[size]} ${className}`}
      {...props}
    >
      {children}
    </button>
  );
};
```

---

## 🔄 REACT ROUTER

### src/App.tsx

```typescript
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { Home } from './pages/Home';
import { Login } from './pages/Login';
import { Register } from './pages/Register';
import { Dashboard } from './pages/Dashboard';
import { FlashcardsPage } from './pages/FlashcardsPage';
import { QuizzesPage } from './pages/QuizzesPage';

const PrivateRoute = ({ children }: { children: JSX.Element }) => {
  const { isAuthenticated, loading } = useAuth();
  
  if (loading) return <div>Loading...</div>;
  return isAuthenticated ? children : <Navigate to="/login" />;
};

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          
          <Route path="/dashboard" element={
            <PrivateRoute><Dashboard /></PrivateRoute>
          } />
          <Route path="/flashcards" element={
            <PrivateRoute><FlashcardsPage /></PrivateRoute>
          } />
          <Route path="/quizzes" element={
            <PrivateRoute><QuizzesPage /></PrivateRoute>
          } />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
```

---

## ✅ BEST PRACTICES

### 1. Используйте React Query для кеширования

```bash
npm install @tanstack/react-query
```

```typescript
import { useQuery } from '@tanstack/react-query';
import { flashcardService } from '../services/flashcardService';

export const useFlashcardSets = () => {
  return useQuery({
    queryKey: ['flashcardSets'],
    queryFn: () => flashcardService.getSets(),
    staleTime: 5 * 60 * 1000, // 5 минут
  });
};
```

### 2. Обработка ошибок

```typescript
try {
  await authService.login(credentials);
} catch (error) {
  if (axios.isAxiosError(error)) {
    const message = error.response?.data?.message || 'Ошибка сервера';
    setError(message);
  }
}
```

### 3. Темная тема

```typescript
// Переключение темы
const toggleTheme = () => {
  if (document.documentElement.classList.contains('dark')) {
    document.documentElement.classList.remove('dark');
  } else {
    document.documentElement.classList.add('dark');
  }
};
```

---

**Следующий шаг:** Создайте React проект и начните с аутентификации! 🚀
