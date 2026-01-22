import { Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import PrivateRoute from './components/layout/PrivateRoute'
import RoleRoute from './components/layout/RoleRoute'
import Layout from './components/layout/Layout'

// Pages
import Home from './pages/Home'
import Login from './pages/auth/Login'
import Register from './pages/auth/Register'
import ProfilePage from './pages/profile/ProfilePage'
import DashboardRouter from './pages/dashboard/DashboardRouter'
import FlashcardsPage from './pages/flashcards/FlashcardsPage'
import FlashcardStudyPage from './pages/flashcards/FlashcardStudyPage'
import FlashcardEditPage from './pages/flashcards/FlashcardEditPage'
import FlashcardStatsPage from './pages/flashcards/FlashcardStatsPage'
import QuizzesPage from './pages/quizzes/QuizzesPage'
import QuizTakePage from './pages/quizzes/QuizTakePage'
import QuizResultPage from './pages/quizzes/QuizResultPage'
import QuizResultsPage from './pages/quizzes/QuizResultsPage'
import QuizStatsPage from './pages/quizzes/QuizStatsPage'
import TheoryPage from './pages/theory/TheoryPage'
import ExamsPage from './pages/exams/ExamsPage'
import ExamTakePage from './pages/exams/ExamTakePage'
import ExamResultsPage from './pages/exams/ExamResultsPage'
import CreateExamPage from './pages/exams/CreateExamPage'
import EditExamPage from './pages/exams/EditExamPage'
import ExamStatsPage from './pages/exams/ExamStatsPage'

// Teacher Pages
import CreateQuizPage from './pages/quizzes/CreateQuizPage'
import EditQuizPage from './pages/quizzes/EditQuizPage'
import CreateFlashcardSetPage from './pages/flashcards/CreateFlashcardSetPage'
import TeacherStudentsPage from './pages/teacher/TeacherStudentsPage'
import StudentDetailPage from './pages/student/StudentDetailPage'
import TeacherAnalyticsPage from './pages/teacher/TeacherAnalyticsPage'
import TeacherExportPage from './pages/teacher/TeacherExportPage'

// Admin Pages
import AdminUsersPage from './pages/admin/AdminUsersPage'
import AdminSubjectsPage from './pages/admin/AdminSubjectsPage'
import AdminAnalyticsPage from './pages/admin/AdminAnalyticsPage'
import AdminExportPage from './pages/admin/AdminExportPage'
import AdminAchievementsPage from './pages/admin/AdminAchievementsPage'
import AdminQuizzesPage from './pages/admin/AdminQuizzesPage'
import AdminExamsPage from './pages/admin/AdminExamsPage'
import AdminSettingsPage from './pages/admin/AdminSettingsPage'
import AdminCountriesPage from './pages/admin/AdminCountriesPage'
import AdminUniversitiesPage from './pages/admin/AdminUniversitiesPage'
import AdminExamTypesPage from './pages/admin/AdminExamTypesPage'
import AdminMLTrainingPage from './pages/admin/AdminMLTrainingPage'
import AdminAIFlashcardsPage from './pages/admin/AdminAIFlashcardsPage'

// Student Pages
import StudentProgressPage from './pages/student/StudentProgressPage'
import StudentAchievementsPage from './pages/student/StudentAchievementsPage'
import StudentLeaderboardPage from './pages/student/StudentLeaderboardPage'
import OnboardingPage from './pages/student/OnboardingPage'

// Profile Pages
import PreferencesPage from './pages/profile/PreferencesPage'

// AI Pages
import AIDashboardPage from './pages/ai/AIDashboardPage'
import UniversityRecommendationsPage from './pages/ai/UniversityRecommendationsPage'

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
          {/* Onboarding (без Layout для полного экрана) */}
          <Route
            path="/onboarding"
            element={
              <PrivateRoute>
                <OnboardingPage />
              </PrivateRoute>
            }
          />
          
          {/* Защищенные страницы */}
          <Route
            path="/profile"
            element={
              <PrivateRoute>
                <ProfilePage />
              </PrivateRoute>
            }
          />
          <Route
            path="/profile/preferences"
            element={
              <PrivateRoute>
                <PreferencesPage />
              </PrivateRoute>
            }
          />
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
            path="/flashcards/:id/edit"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                  <FlashcardEditPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/flashcards/:id/stats"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                  <FlashcardStatsPage />
                </RoleRoute>
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
            path="/quizzes/:id/take"
            element={
              <PrivateRoute>
                <QuizTakePage />
              </PrivateRoute>
            }
          />
          <Route
            path="/learning/theory/:id"
            element={
              <PrivateRoute>
                <TheoryPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/quizzes/:id/results"
            element={
              <PrivateRoute>
                <QuizResultsPage />
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
          <Route
            path="/quizzes/:id/stats"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                  <QuizStatsPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/quizzes/:id/edit"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                  <EditQuizPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />

          {/* Экзамены */}
          <Route
            path="/exams"
            element={
              <PrivateRoute>
                <ExamsPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/exams/create"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                  <CreateExamPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/exams/:id/take"
            element={
              <PrivateRoute>
                <ExamTakePage />
              </PrivateRoute>
            }
          />
          <Route
            path="/exams/:id/results"
            element={
              <PrivateRoute>
                <ExamResultsPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/exams/:id/stats"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                  <ExamStatsPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/exams/:id/edit"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher', 'Admin']}>
                  <EditExamPage />
                </RoleRoute>
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
                <RoleRoute allowedRoles={['Teacher']}>
                  <TeacherStudentsPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/students"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <TeacherStudentsPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/teacher/students/:studentId/stats"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Teacher']}>
                  <StudentDetailPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/students/:studentId/stats"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
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
        />

          {/* AI Страницы */}
          <Route
            path="/ai/dashboard"
            element={
              <PrivateRoute>
                <AIDashboardPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/ai/universities"
            element={
              <PrivateRoute>
                <UniversityRecommendationsPage />
              </PrivateRoute>
            }
          />
          
          {/* Страницы для администраторов */}
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
          <Route
            path="/admin/subjects"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminSubjectsPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/quizzes"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminQuizzesPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/exams"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminExamsPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/analytics"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminAnalyticsPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/export"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminExportPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/achievements"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminAchievementsPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/settings"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminSettingsPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/countries"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminCountriesPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/universities"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminUniversitiesPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/exam-types"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminExamTypesPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/ml-training"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin']}>
                  <AdminMLTrainingPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />
          <Route
            path="/admin/ai-flashcards"
            element={
              <PrivateRoute>
                <RoleRoute allowedRoles={['Admin', 'Teacher']}>
                  <AdminAIFlashcardsPage />
                </RoleRoute>
              </PrivateRoute>
            }
          />

          {/* Страницы для студентов */}
          <Route
            path="/student/progress"
            element={
              <PrivateRoute>
                <StudentProgressPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/student/achievements"
            element={
              <PrivateRoute>
                <StudentAchievementsPage />
              </PrivateRoute>
            }
          />
          <Route
            path="/student/leaderboard"
            element={
              <PrivateRoute>
                <StudentLeaderboardPage />
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