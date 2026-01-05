import { useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import './Dashboard.css'

const Dashboard = () => {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  return (
    <div className="dashboard-container">
      <div className="dashboard-card">
        <div className="dashboard-header">
          <h1>Welcome to Your Dashboard</h1>
          <button onClick={handleLogout} className="logout-button">
            Logout
          </button>
        </div>

        <div className="user-info">
          <h2>User Information</h2>
          <div className="info-item">
            <strong>Email:</strong> {user?.email}
          </div>
          {user?.firstName && (
            <div className="info-item">
              <strong>First Name:</strong> {user.firstName}
            </div>
          )}
          {user?.lastName && (
            <div className="info-item">
              <strong>Last Name:</strong> {user.lastName}
            </div>
          )}
          <div className="info-item">
            <strong>Email Verified:</strong> {user?.emailVerified ? 'Yes' : 'No'}
          </div>
        </div>

        <div className="dashboard-content">
          <p>You are successfully authenticated! This is your protected dashboard.</p>
          <p>Your authentication token is stored securely and will be used for API requests.</p>
        </div>
      </div>
    </div>
  )
}

export default Dashboard

