import { lazy, Suspense } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { useAppSelector } from './hooks/useAppSelector'
import Layout from './components/Layout'
import AdminLayout from './components/AdminLayout'
import TutorLayout from './components/TutorLayout'
import SchoolAdminLayout from './components/SchoolAdminLayout'
import CookieBanner from './components/CookieBanner'
import { ToastProvider } from './components/Toast'

// ── Lazy-loaded pages (code splitting) ──────────────────
const LoginPage = lazy(() => import('./pages/LoginPage'))
const RegisterPage = lazy(() => import('./pages/RegisterPage'))
const CompleteProfilePage = lazy(() => import('./pages/CompleteProfilePage'))
const DashboardPage = lazy(() => import('./pages/DashboardPage'))
const LearnPage = lazy(() => import('./pages/LearnPage'))
const ProgressPage = lazy(() => import('./pages/ProgressPage'))
const StudyPlanPage = lazy(() => import('./pages/StudyPlanPage'))
const ProfilePage = lazy(() => import('./pages/ProfilePage'))
const NotificationSettingsPage = lazy(() => import('./pages/NotificationSettingsPage'))
const DiagnosticTestPage = lazy(() => import('./pages/DiagnosticTestPage'))
const AdminStatsPage = lazy(() => import('./pages/AdminStatsPage'))
const AdminQuestionsPage = lazy(() => import('./pages/AdminQuestionsPage'))
const AdminUsersPage = lazy(() => import('./pages/AdminUsersPage'))
const AdminAuditLogsPage = lazy(() => import('./pages/AdminAuditLogsPage'))
const AdminSystemHealthPage = lazy(() => import('./pages/AdminSystemHealthPage'))
const AdminBackupsPage = lazy(() => import('./pages/AdminBackupsPage'))
const AdminLegalPage = lazy(() => import('./pages/AdminLegalPage'))
const AdminUserActivityPage = lazy(() => import('./pages/AdminUserActivityPage'))
const AdminTutorsPage = lazy(() => import('./pages/AdminTutorsPage'))
const AdminQuestionImportPage = lazy(() => import('./pages/AdminQuestionImportPage'))
const AdminContentPage = lazy(() => import('./pages/AdminContentPage'))
const AdminTrashPage = lazy(() => import('./pages/AdminTrashPage'))
const AdminProfilePage = lazy(() => import('./pages/AdminProfilePage'))
const AdminSchoolsPage = lazy(() => import('./pages/AdminSchoolsPage'))
const LandingPage = lazy(() => import('./pages/CscaLandingPage'))
const TutorsPage = lazy(() => import('./pages/TutorsPage'))
const TutorProfilePage = lazy(() => import('./pages/TutorProfilePage'))
const SchoolDetailPage = lazy(() => import('./pages/SchoolDetailPage'))
const PrivacyPage = lazy(() => import('./pages/PrivacyPage'))
const TermsPage = lazy(() => import('./pages/TermsPage'))
const MessagesPage = lazy(() => import('./pages/MessagesPage'))
const TutorHomePage = lazy(() => import('./pages/TutorHomePage'))
const TutorStudentsPage = lazy(() => import('./pages/TutorStudentsPage'))
const TutorSchedulePage = lazy(() => import('./pages/TutorSchedulePage'))
const TutorProfileEditPage = lazy(() => import('./pages/TutorProfileEditPage'))
const TutorReviewsPage = lazy(() => import('./pages/TutorReviewsPage'))
const TutorQuestionsPage = lazy(() => import('./pages/TutorQuestionsPage'))
const TutorAssignmentsPage = lazy(() => import('./pages/TutorAssignmentsPage'))
const TutorSchoolManagePage = lazy(() => import('./pages/TutorSchoolManagePage'))
const SchoolAdminDashboardPage = lazy(() => import('./pages/SchoolAdminDashboardPage'))
const SchoolAdminStudentsPage = lazy(() => import('./pages/SchoolAdminStudentsPage'))
const SchoolAdminTutorsPage = lazy(() => import('./pages/SchoolAdminTutorsPage'))
const SchoolAdminApplicationsPage = lazy(() => import('./pages/SchoolAdminApplicationsPage'))
const SchoolAdminMocksPage = lazy(() => import('./pages/SchoolAdminMocksPage'))
const SchoolAdminBrandingPage = lazy(() => import('./pages/SchoolAdminBrandingPage'))
const TutorContentPage = lazy(() => import('./pages/TutorContentPage'))
const StudentAssignmentsPage = lazy(() => import('./pages/StudentAssignmentsPage'))
const ForgotPasswordPage = lazy(() => import('./pages/ForgotPasswordPage'))
const ReferralTermsPage = lazy(() => import('./pages/ReferralTermsPage'))
const AboutCscaPage = lazy(() => import('./pages/AboutCscaPage'))
const CscaMaterialsPage = lazy(() => import('./pages/CscaMaterialsPage'))
const CscaMocksPage = lazy(() => import('./pages/CscaMocksPage'))
const CscaCoursesPage = lazy(() => import('./pages/CscaCoursesPage'))
const AboutUsPage = lazy(() => import('./pages/AboutUsPage'))
const ContactsPage = lazy(() => import('./pages/ContactsPage'))
const CscaNewsPage = lazy(() => import('./pages/CscaNewsPage'))
const CheckoutPage = lazy(() => import('./pages/CheckoutPage'))
const PurchasesPage = lazy(() => import('./pages/PurchasesPage'))
const AdminNewsPage = lazy(() => import('./pages/AdminNewsPage'))
const AdminSupportPage = lazy(() => import('./pages/AdminSupportPage'))

// ── Suspense fallback ───────────────────────────────────
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
      <Route path="profile/notifications" element={<NotificationSettingsPage />} />
      <Route path="purchases" element={<PurchasesPage />} />
      <Route path="checkout" element={<CheckoutPage />} />
      <Route path="diagnostic" element={<DiagnosticTestPage />} />
      <Route path="tutors" element={<TutorsPage />} />
      <Route path="tutors/schools/:slug" element={<SchoolDetailPage />} />
      <Route path="tutors/:userId" element={<TutorProfilePage />} />
      <Route path="assignments" element={<StudentAssignmentsPage />} />
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
      <Route path="content" element={<AdminContentPage />} />
      <Route path="news" element={<AdminNewsPage />} />
      <Route path="support" element={<AdminSupportPage />} />
      <Route path="users" element={<AdminUsersPage />} />
      <Route path="tutors" element={<AdminTutorsPage />} />
      <Route path="audit" element={<AdminAuditLogsPage />} />
      <Route path="health" element={<AdminSystemHealthPage />} />
      <Route path="backups" element={<AdminBackupsPage />} />
      <Route path="legal" element={<AdminLegalPage />} />
      <Route path="activity" element={<AdminUserActivityPage />} />
      <Route path="import" element={<AdminQuestionImportPage />} />
      <Route path="question-import" element={<Navigate to="/admin/import" replace />} />
      <Route path="trash" element={<AdminTrashPage />} />
      <Route path="schools" element={<AdminSchoolsPage />} />
      <Route path="profile" element={<AdminProfilePage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Route>
  )
}

function TutorRoutes() {
  return (
    <Route path="/" element={<TutorLayout />}>
      <Route index element={<TutorHomePage />} />
      <Route path="students" element={<TutorStudentsPage />} />
      <Route path="questions" element={<TutorQuestionsPage />} />
      <Route path="assignments" element={<TutorAssignmentsPage />} />
      <Route path="messages" element={<MessagesPage />} />
      <Route path="schedule" element={<TutorSchedulePage />} />
      <Route path="school" element={<TutorSchoolManagePage />} />
      <Route path="content" element={<TutorContentPage />} />
      <Route path="reviews" element={<TutorReviewsPage />} />
      <Route path="my-profile" element={<TutorProfileEditPage />} />
      <Route path="profile" element={<ProfilePage />} />
      <Route path="tutors/:userId" element={<TutorProfilePage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Route>
  )
}

function SchoolAdminRoutes() {
  return (
    <Route path="/" element={<SchoolAdminLayout />}>
      <Route index element={<SchoolAdminDashboardPage />} />
      <Route path="mocks" element={<SchoolAdminMocksPage />} />
      <Route path="students" element={<SchoolAdminStudentsPage />} />
      <Route path="tutors" element={<SchoolAdminTutorsPage />} />
      <Route path="applications" element={<SchoolAdminApplicationsPage />} />
      <Route path="school" element={<TutorSchoolManagePage />} />
      <Route path="branding" element={<SchoolAdminBrandingPage />} />
      <Route path="messages" element={<MessagesPage />} />
      <Route path="profile" element={<ProfilePage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Route>
  )
}

function App() {
  const { isAuthenticated, user } = useAppSelector((state) => state.auth)
  const isAdmin = user?.role === 'Admin'
  const isTutor = user?.role === 'Tutor' || user?.role === 'SchoolTutor'
  const isSchoolAdmin = user?.role === 'SchoolAdmin'
  const needsPhone = isAuthenticated && !user?.phoneNumber

  return (
    <ToastProvider>
    <Suspense fallback={<PageLoader />}>
    <Routes>
      <Route path="/landing" element={!isAuthenticated ? <LandingPage /> : <Navigate to="/" replace />} />
      <Route path="/login" element={!isAuthenticated ? <LoginPage /> : <Navigate to={needsPhone ? '/complete-profile' : '/'} />} />
      <Route path="/register" element={!isAuthenticated ? <RegisterPage /> : <Navigate to={needsPhone ? '/complete-profile' : '/'} />} />
      <Route path="/forgot-password" element={!isAuthenticated ? <ForgotPasswordPage /> : <Navigate to="/" replace />} />
      <Route path="/privacy" element={<PrivacyPage />} />
      <Route path="/terms" element={<TermsPage />} />
      <Route path="/referral-terms" element={<ReferralTermsPage />} />
      <Route path="/csca/about" element={<AboutCscaPage />} />
      <Route path="/csca/news" element={<CscaNewsPage />} />
      <Route path="/csca/courses" element={<CscaCoursesPage />} />
      <Route path="/csca/materials" element={<CscaMaterialsPage />} />
      <Route path="/csca/mocks" element={<CscaMocksPage />} />
      <Route path="/csca/about-us" element={<AboutUsPage />} />
      <Route path="/csca/contacts" element={<ContactsPage />} />
      <Route path="/complete-profile" element={needsPhone ? <CompleteProfilePage /> : <Navigate to="/" replace />} />

      {isAuthenticated ? (
        needsPhone ? (
          <Route path="*" element={<Navigate to="/complete-profile" />} />
        ) : (
          isAdmin ? AdminRoutes() : isSchoolAdmin ? SchoolAdminRoutes() : isTutor ? TutorRoutes() : StudentRoutes()
        )
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
