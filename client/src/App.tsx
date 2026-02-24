import { Routes, Route, Navigate } from 'react-router-dom'
import { useAppSelector } from './hooks/useAppSelector'
import Layout from './components/Layout'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import ExamSelectionPage from './pages/ExamSelectionPage'
import TestPage from './pages/TestPage'
import AnalyticsPage from './pages/AnalyticsPage'
import ReviewPage from './pages/ReviewPage'
import TopicsPage from './pages/TopicsPage'
import HistoryPage from './pages/HistoryPage'
import StudyPlanPage from './pages/StudyPlanPage'
import PredictionPage from './pages/PredictionPage'
import RecommendationsPage from './pages/RecommendationsPage'

function App() {
  const { isAuthenticated } = useAppSelector((state) => state.auth)

  return (
    <Routes>
      <Route path="/login" element={!isAuthenticated ? <LoginPage /> : <Navigate to="/" />} />
      <Route path="/register" element={!isAuthenticated ? <RegisterPage /> : <Navigate to="/" />} />
      
      <Route path="/" element={isAuthenticated ? <Layout /> : <Navigate to="/login" />}>
        <Route index element={<ExamSelectionPage />} />
        <Route path="test" element={<TestPage />} />
        <Route path="analytics" element={<AnalyticsPage />} />
        <Route path="history" element={<HistoryPage />} />
        <Route path="review" element={<ReviewPage />} />
        <Route path="topics" element={<TopicsPage />} />
        <Route path="study-plan" element={<StudyPlanPage />} />
        <Route path="prediction" element={<PredictionPage />} />
        <Route path="recommendations" element={<RecommendationsPage />} />
      </Route>
    </Routes>
  )
}

export default App
