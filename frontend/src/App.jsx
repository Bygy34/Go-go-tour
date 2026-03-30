import { Link, Navigate, Route, Routes } from 'react-router-dom'
import ToursPage from './pages/ToursPage'
import TourDetailsPage from './pages/TourDetailsPage'
import AdminPage from './pages/AdminPage'

export default function App() {
  return (
    <div className="container">
      <header className="header">
        <h1>GoGo Tour</h1>
        <nav>
          <Link to="/">Туры</Link>
          <Link to="/admin">Админка</Link>
        </nav>
      </header>
      <Routes>
        <Route path="/" element={<ToursPage />} />
        <Route path="/tour/:id" element={<TourDetailsPage />} />
        <Route path="/admin" element={<AdminPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </div>
  )
}
