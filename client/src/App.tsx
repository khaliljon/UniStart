import { Routes, Route, Navigate } from 'react-router-dom'
import { useAppSelector } from './hooks/useAppSelector'
import Layout from './components/Layout'
import AdminLayout from './components/AdminLayout'
import TutorLayout from './components/TutorLayout'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import OnboardingPage from './pages/OnboardingPage'
import DashboardPage from './pages/DashboardPage'
import LearnPage from './pages/LearnPage'
import ProgressPage from './pages/ProgressPage'
import StudyPlanPage from './pages/StudyPlanPage'
import ProfilePage from './pages/ProfilePage'
import NotificationSettingsPage from './pages/NotificationSettingsPage'
import DiagnosticTestPage from './pages/DiagnosticTestPage'
import AdminStatsPage from './pages/AdminStatsPage'
import AdminQuestionsPage from './pages/AdminQuestionsPage'
import AdminImportPage from './pages/AdminImportPage'
import AdminUsersPage from './pages/AdminUsersPage'
import AdminAuditLogsPage from './pages/AdminAuditLogsPage'
import AdminSystemHealthPage from './pages/AdminSystemHealthPage'
import AdminUserActivityPage from './pages/AdminUserActivityPage'
import AdminTutorsPage from './pages/AdminTutorsPage'
import AdminQuestionImportPage from './pages/AdminQuestionImportPage'
import LandingPage from './pages/LandingPage'
import TutorsPage from './pages/TutorsPage'
import TutorProfilePage from './pages/TutorProfilePage'
import MessagesPage from './pages/MessagesPage'
import TutorHomePage from './pages/TutorHomePage'
import TutorStudentsPage from './pages/TutorStudentsPage'
import TutorSchedulePage from './pages/TutorSchedulePage'
import TutorProfileEditPage from './pages/TutorProfileEditPage'
import TutorReviewsPage from './pages/TutorReviewsPage'

function StudentRoutes() {
  return (
    <Route path="/" element={<Layout />}>
      <Route index element={<DashboardPage />} />
      <Route path="learn" element={<LearnPage />} />
      <Route path="progress" element={<ProgressPage />} />
      <Route path="plan" element={<StudyPlanPage />} />
      <Route path="profile" element={<ProfilePage />} />
      <Route path="profile/notifications" element={<NotificationSettingsPage />} />
      <Route path="diagnostic" element={<DiagnosticTestPage />} />
      <Route path="tutors" element={<TutorsPage />} />
      <Route path="tutors/:userId" element={<TutorProfilePage />} />
      <Route path="messages" element={<MessagesPage />} />
      {/* Legacy redirects */}
      <Route path="test" element={<Navigate to="/learn" replace />} />
      <Route path="mock-exam" element={<Navigate to="/learn?tab=mock" replace />} />
      <Route path="topics" element={<Navigate to="/learn?tab=topics" replace />} />
      <Route path="review" element={<Navigate to="/learn?tab=review" replace />} />
      <Route path="analytics" element={<Navigate to="/progress" replace />} />
      <Route path="prediction" element={<Navigate to="/progress?tab=prediction" replace />} />
      <Route path="history" element={<Navigate to="/progress?tab=history" replace />} />
      <Route path="study-plan" element={<Navigate to="/plan" replace />} />
      <Route path="recommendations" element={<Navigate to="/" replace />} />
      <Route path="notifications" element={<Navigate to="/profile/notifications" replace />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Route>
  )
}

function AdminRoutes() {
  return (
    <Route path="/" element={<AdminLayout />}>
      <Route index element={<AdminStatsPage />} />
      <Route path="questions" element={<AdminQuestionsPage />} />
      <Route path="users" element={<AdminUsersPage />} />
      <Route path="tutors" element={<AdminTutorsPage />} />
      <Route path="audit" element={<AdminAuditLogsPage />} />
      <Route path="health" element={<AdminSystemHealthPage />} />
      <Route path="activity" element={<AdminUserActivityPage />} />
      <Route path="import" element={<AdminImportPage />} />
      <Route path="question-import" element={<AdminQuestionImportPage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Route>
  )
}

function TutorRoutes() {
  return (
    <Route path="/" element={<TutorLayout />}>
      <Route index element={<TutorHomePage />} />
      <Route path="students" element={<TutorStudentsPage />} />
      <Route path="messages" element={<MessagesPage />} />
      <Route path="schedule" element={<TutorSchedulePage />} />
      <Route path="reviews" element={<TutorReviewsPage />} />
      <Route path="my-profile" element={<TutorProfileEditPage />} />
      <Route path="profile" element={<ProfilePage />} />
      <Route path="tutors/:userId" element={<TutorProfilePage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Route>
  )
}

function App() {
  const { isAuthenticated, user } = useAppSelector((state) => state.auth)
  const isAdmin = user?.role === 'Admin'
  const isTutor = user?.role === 'Tutor'
  const needsOnboarding = isAuthenticated && !isAdmin && !isTutor && !user?.hasCompletedOnboarding

  return (
    <Routes>
      <Route path="/landing" element={!isAuthenticated ? <LandingPage /> : <Navigate to="/" replace />} />
      <Route path="/login" element={!isAuthenticated ? <LoginPage /> : <Navigate to={needsOnboarding ? '/onboarding' : '/'} />} />
      <Route path="/register" element={!isAuthenticated ? <RegisterPage /> : <Navigate to={needsOnboarding ? '/onboarding' : '/'} />} />
      <Route path="/onboarding" element={needsOnboarding ? <OnboardingPage /> : <Navigate to="/" replace />} />

      {isAuthenticated ? (
        needsOnboarding ? (
          <Route path="*" element={<Navigate to="/onboarding" />} />
        ) : (
          isAdmin ? AdminRoutes() : isTutor ? TutorRoutes() : StudentRoutes()
        )
      ) : (
        <Route path="*" element={<Navigate to="/landing" />} />
      )}
    </Routes>
  )
}

export default App
