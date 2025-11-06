import { Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import PrivateRoute from './components/layout/PrivateRoute'
import RoleRoute from './components/layout/RoleRoute'
import Layout from './components/layout/Layout'

// Pages
import Home from './pages/Home'
import Login from './pages/Login'
import Register from './pages/Register'
import DashboardRouter from './pages/DashboardRouter'
import FlashcardsPage from './pages/FlashcardsPage'
import FlashcardStudyPage from './pages/FlashcardStudyPage'
import QuizzesPage from './pages/QuizzesPage'
import QuizTakingPage from './pages/QuizTakingPage'
import QuizResultPage from './pages/QuizResultPage'

// Teacher Pages
import CreateQuizPage from './pages/CreateQuizPage'
import CreateFlashcardSetPage from './pages/CreateFlashcardSetPage'
import TeacherStudentsPage from './pages/TeacherStudentsPage'
import StudentDetailPage from './pages/StudentDetailPage'
import TeacherAnalyticsPage from './pages/TeacherAnalyticsPage'
import TeacherExportPage from './pages/TeacherExportPage'

// Admin Pages
import AdminUsersPage from './pages/AdminUsersPage'

function App() {
  return (
    <AuthProvider>
      <Routes>
        {/* Публичные страницы без Layout */}
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        {/* Защищенные страницы с Layout */}
        <Route element={<Layout />}>
          {/* Защищенные страницы */}
          <Route
            path="/dashboard"
            element={
              <PrivateRoute>
                <DashboardRouter />
              </PrivateRoute>
            }
          />
          <Route
            path="/flashcards"
            element={
              <PrivateRoute>
                <FlashcardsPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/flashcards/:setId/study"
            element={
              <PrivateRoute>
                <FlashcardStudyPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/quizzes"
            element={
              <PrivateRoute>
                <QuizzesPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/quizzes/:quizId/take"
            element={
              <PrivateRoute>
                <QuizTakingPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/quizzes/:quizId/result"
            element={
              <PrivateRoute>
                <QuizResultPage />
              </PrivateRoute>
            }
          />

          {/* Страницы для преподавателей */}
          <Route
            path="/quizzes/create"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                  <CreateQuizPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/flashcards/create"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                  <CreateFlashcardSetPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/teacher/students"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                  <TeacherStudentsPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/teacher/students/:studentId/stats"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                  <StudentDetailPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
        <Route
          path="/teacher/analytics"
          element={
            <PrivateRoute>
              <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                <TeacherAnalyticsPage />
              </RoleRoute>
            </PrivateRoute>
          }
        />
        <Route
          path="/teacher/export"
          element={
            <PrivateRoute>
              <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                <TeacherExportPage />
              </RoleRoute>
            </PrivateRoute>
          }
        />          {/* Страницы для администраторов */}
          <Route
            path="/admin/users"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminUsersPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />

          {/* Redirect для несуществующих маршрутов */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </AuthProvider>
  )
}

export default App