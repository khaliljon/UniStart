import { lazy, Suspense } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { useAppSelector } from './hooks/useAppSelector'
import Layout from './components/Layout'
import AdminLayout from './components/AdminLayout'
import CookieBanner from './components/CookieBanner'
import Analytics from './components/Analytics'
import { ToastProvider } from './components/Toast'

const LoginPage = lazy(() => import('./pages/LoginPage'))
const RegisterPage = lazy(() => import('./pages/RegisterPage'))
const CompleteProfilePage = lazy(() => import('./pages/CompleteProfilePage'))
const DashboardPage = lazy(() => import('./pages/DashboardPage'))
const LearnPage = lazy(() => import('./pages/LearnPage'))
const MockExamPage = lazy(() => import('./pages/MockExamPage'))
const ProgressPage = lazy(() => import('./pages/ProgressPage'))
const StudyPlanPage = lazy(() => import('./pages/StudyPlanPage'))
const ProfilePage = lazy(() => import('./pages/ProfilePage'))
const DiagnosticTestPage = lazy(() => import('./pages/DiagnosticTestPage'))
const AdminStatsPage = lazy(() => import('./pages/AdminStatsPage'))
const AdminQuestionsPage = lazy(() => import('./pages/AdminQuestionsPage'))
const AdminUsersPage = lazy(() => import('./pages/AdminUsersPage'))
const AdminAuditLogsPage = lazy(() => import('./pages/AdminAuditLogsPage'))
const AdminSystemHealthPage = lazy(() => import('./pages/AdminSystemHealthPage'))
const AdminBackupsPage = lazy(() => import('./pages/AdminBackupsPage'))
const AdminLegalPage = lazy(() => import('./pages/AdminLegalPage'))
const AdminUserActivityPage = lazy(() => import('./pages/AdminUserActivityPage'))
const AdminQuestionImportPage = lazy(() => import('./pages/AdminQuestionImportPage'))
const AdminContentPage = lazy(() => import('./pages/AdminContentPage'))
const AdminTrashPage = lazy(() => import('./pages/AdminTrashPage'))
const AdminProfilePage = lazy(() => import('./pages/AdminProfilePage'))
const AdminMocksPage = lazy(() => import('./pages/AdminMocksPage'))
const AdminSalesPage = lazy(() => import('./pages/AdminSalesPage'))
const AdminPricingPage = lazy(() => import('./pages/AdminPricingPage'))
const AdminExamDatesPage = lazy(() => import('./pages/AdminExamDatesPage'))
const AdminSpecialtyTracksPage = lazy(() => import('./pages/AdminSpecialtyTracksPage'))
const LandingPage = lazy(() => import('./pages/CscaLandingPage'))
const PrivacyPage = lazy(() => import('./pages/PrivacyPage'))
const TermsPage = lazy(() => import('./pages/TermsPage'))
const ForgotPasswordPage = lazy(() => import('./pages/ForgotPasswordPage'))
const ReferralTermsPage = lazy(() => import('./pages/ReferralTermsPage'))
const AboutCscaPage = lazy(() => import('./pages/AboutCscaPage'))
const CscaMaterialsPage = lazy(() => import('./pages/CscaMaterialsPage'))
const CscaMocksPage = lazy(() => import('./pages/CscaMocksPage'))
const CscaCoursesPage = lazy(() => import('./pages/CscaCoursesPage'))
const AboutUsPage = lazy(() => import('./pages/AboutUsPage'))
const ContactsPage = lazy(() => import('./pages/ContactsPage'))
const CscaNewsPage = lazy(() => import('./pages/CscaNewsPage'))
const CscaNewsArticlePage = lazy(() => import('./pages/CscaNewsArticlePage'))
const PurchasesPage = lazy(() => import('./pages/PurchasesPage'))
const CartPage = lazy(() => import('./pages/CartPage'))
const MaterialsPage = lazy(() => import('./pages/MaterialsPage'))
const AdminNewsPage = lazy(() => import('./pages/AdminNewsPage'))
const AdminSupportPage = lazy(() => import('./pages/AdminSupportPage'))

const PageLoader = () => (
  <div className="loading"><div className="spinner" /></div>
)

function StudentRoutes() {
  return (
    <Route path="/" element={<Layout />}>
      <Route index element={<DashboardPage />} />
      <Route path="learn" element={<LearnPage />} />
      <Route path="progress" element={<ProgressPage />} />
      <Route path="plan" element={<StudyPlanPage />} />
      <Route path="profile" element={<ProfilePage />} />
      <Route path="purchases" element={<PurchasesPage />} />
      <Route path="cart" element={<CartPage />} />
      <Route path="materials" element={<MaterialsPage />} />
      <Route path="exams/result/:attemptId" element={<MockExamPage />} />
      <Route path="diagnostic" element={<DiagnosticTestPage />} />
      <Route path="tutors" element={<Navigate to="/" replace />} />
      <Route path="tutors/*" element={<Navigate to="/" replace />} />
      <Route path="messages" element={<Navigate to="/" replace />} />
      <Route path="assignments" element={<Navigate to="/" replace />} />
      <Route path="test" element={<Navigate to="/learn" replace />} />
      <Route path="mock-exam" element={<Navigate to="/learn?tab=mock" replace />} />
      <Route path="topics" element={<Navigate to="/learn?tab=topics" replace />} />
      <Route path="review" element={<Navigate to="/learn?tab=review" replace />} />
      <Route path="analytics" element={<Navigate to="/progress" replace />} />
      <Route path="prediction" element={<Navigate to="/progress?tab=prediction" replace />} />
      <Route path="history" element={<Navigate to="/progress?tab=history" replace />} />
      <Route path="study-plan" element={<Navigate to="/plan" replace />} />
      <Route path="recommendations" element={<Navigate to="/" replace />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Route>
  )
}

function AdminRoutes() {
  return (
    <Route path="/" element={<AdminLayout />}>
      <Route index element={<AdminStatsPage />} />
      <Route path="questions" element={<AdminQuestionsPage />} />
      <Route path="mocks" element={<AdminMocksPage />} />
      <Route path="sales" element={<AdminSalesPage />} />
      <Route path="pricing" element={<AdminPricingPage />} />
      <Route path="exam-dates" element={<AdminExamDatesPage />} />
      <Route path="specialty-tracks" element={<AdminSpecialtyTracksPage />} />
      <Route path="content" element={<AdminContentPage />} />
      <Route path="news" element={<AdminNewsPage />} />
      <Route path="support" element={<AdminSupportPage />} />
      <Route path="users" element={<AdminUsersPage />} />
      <Route path="audit" element={<AdminAuditLogsPage />} />
      <Route path="health" element={<AdminSystemHealthPage />} />
      <Route path="backups" element={<AdminBackupsPage />} />
      <Route path="legal" element={<AdminLegalPage />} />
      <Route path="activity" element={<AdminUserActivityPage />} />
      <Route path="import" element={<AdminQuestionImportPage />} />
      <Route path="question-import" element={<Navigate to="/admin/import" replace />} />
      <Route path="trash" element={<AdminTrashPage />} />
      <Route path="profile" element={<AdminProfilePage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Route>
  )
}

function App() {
  const { isAuthenticated, user } = useAppSelector((state) => state.auth)
  const isAdmin = user?.role === 'Admin'

  return (
    <ToastProvider>
    <Analytics />
    <Suspense fallback={<PageLoader />}>
    <Routes>
      <Route path="/landing" element={!isAuthenticated ? <LandingPage /> : <Navigate to="/" replace />} />
      <Route path="/login" element={!isAuthenticated ? <LoginPage /> : <Navigate to="/" />} />
      <Route path="/register" element={!isAuthenticated ? <RegisterPage /> : <Navigate to="/" />} />
      <Route path="/forgot-password" element={!isAuthenticated ? <ForgotPasswordPage /> : <Navigate to="/" replace />} />
      <Route path="/privacy" element={<PrivacyPage />} />
      <Route path="/terms" element={<TermsPage />} />
      <Route path="/referral-terms" element={<ReferralTermsPage />} />
      <Route path="/csca/about" element={<AboutCscaPage />} />
      <Route path="/csca/news" element={<CscaNewsPage />} />
      <Route path="/csca/news/:slug" element={<CscaNewsArticlePage />} />
      <Route path="/csca/courses" element={<CscaCoursesPage />} />
      <Route path="/csca/materials" element={<CscaMaterialsPage />} />
      <Route path="/csca/mocks" element={<CscaMocksPage />} />
      <Route path="/csca/about-us" element={<AboutUsPage />} />
      <Route path="/csca/contacts" element={<ContactsPage />} />
      <Route path="/complete-profile" element={isAuthenticated ? <CompleteProfilePage /> : <Navigate to="/" replace />} />

      {isAuthenticated ? (
        isAdmin ? AdminRoutes() : StudentRoutes()
      ) : (
        <Route path="*" element={<Navigate to="/landing" />} />
      )}
    </Routes>
    </Suspense>
    <CookieBanner />
    </ToastProvider>
  )
}

export default App
