import { Routes, Route, Navigate } from 'react-router-dom'
import { useAppSelector } from './hooks/useAppSelector'
import Layout from './components/Layout'
import AdminLayout from './components/AdminLayout'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import OnboardingPage from './pages/OnboardingPage'
import ExamSelectionPage from './pages/ExamSelectionPage'
import TestPage from './pages/TestPage'
import AnalyticsPage from './pages/AnalyticsPage'
import ReviewPage from './pages/ReviewPage'
import TopicsPage from './pages/TopicsPage'
import HistoryPage from './pages/HistoryPage'
import StudyPlanPage from './pages/StudyPlanPage'
import PredictionPage from './pages/PredictionPage'
import RecommendationsPage from './pages/RecommendationsPage'
import AdminStatsPage from './pages/AdminStatsPage'
import AdminQuestionsPage from './pages/AdminQuestionsPage'
import AdminImportPage from './pages/AdminImportPage'
import AdminUsersPage from './pages/AdminUsersPage'
import MockExamPage from './pages/MockExamPage'
import DiagnosticTestPage from './pages/DiagnosticTestPage'
import LandingPage from './pages/LandingPage'
import NotificationSettingsPage from './pages/NotificationSettingsPage'

function StudentRoutes() {
  return (
    <Route path="/" element={<Layout />}>
      <Route index element={<ExamSelectionPage />} />
      <Route path="test" element={<TestPage />} />
      <Route path="analytics" element={<AnalyticsPage />} />
      <Route path="history" element={<HistoryPage />} />
      <Route path="review" element={<ReviewPage />} />
      <Route path="topics" element={<TopicsPage />} />
      <Route path="study-plan" element={<StudyPlanPage />} />
      <Route path="prediction" element={<PredictionPage />} />
      <Route path="recommendations" element={<RecommendationsPage />} />
      <Route path="mock-exam" element={<MockExamPage />} />
      <Route path="diagnostic" element={<DiagnosticTestPage />} />
      <Route path="notifications" element={<NotificationSettingsPage />} />
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
      <Route path="import" element={<AdminImportPage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Route>
  )
}

function App() {
  const { isAuthenticated, user } = useAppSelector((state) => state.auth)
  const isAdmin = user?.role === 'Admin'
  const needsOnboarding = isAuthenticated && !isAdmin && !user?.hasCompletedOnboarding

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
          isAdmin ? AdminRoutes() : StudentRoutes()
        )
      ) : (
        <Route path="*" element={<Navigate to="/landing" />} />
      )}
    </Routes>
  )
}

export default App
