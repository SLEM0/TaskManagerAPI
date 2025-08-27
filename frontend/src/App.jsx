import React from 'react';
import { HashRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { useAuth } from './hooks/useAuth';
import RegisterForm from './components/RegisterForm';
import LoginForm from './components/LoginForm';

const AuthPage = () => {
    const [isLogin, setIsLogin] = React.useState(false);

    return (
        <div className="app">
            <div className="animated-background">
                <div className="floating-shape shape-1"></div>
                <div className="floating-shape shape-2"></div>
                <div className="floating-shape shape-3"></div>
                <div className="floating-shape shape-4"></div>
            </div>

            {!isLogin ? (
                <RegisterForm onSwitchToLogin={() => setIsLogin(true)} />
            ) : (
                <LoginForm onSwitchToRegister={() => setIsLogin(false)} />
            )}
        </div>
    );
};

const ProtectedRoute = ({ children }) => {
    const { isAuthenticated, loading } = useAuth();

    if (loading) {
        return (
            <div className="app">
                <div className="loading-spinner"></div>
            </div>
        );
    }

    return isAuthenticated ? children : <Navigate to="/auth" />;
};

const Dashboard = () => {
    const { user, logout } = useAuth();

    return (
        <div className="app">
            <div className="animated-background">
                <div className="floating-shape shape-1"></div>
                <div className="floating-shape shape-2"></div>
                <div className="floating-shape shape-3"></div>
                <div className="floating-shape shape-4"></div>
            </div>

            <div className="auth-wrapper">
                <div className="card glass-card">
                    <div className="card-header">
                        <div className="logo-wrapper">
                            <span className="logo-text">TaskFlow</span>
                        </div>
                        <h2 className="card-title">
                            Welcome to Task Manager!
                        </h2>
                        <p className="card-subtitle">
                            Hello, {user?.email}! Your dashboard is coming soon.
                        </p>
                    </div>

                    <div className="dashboard-content">
                        <div style={{
                            background: 'rgba(30, 41, 59, 0.3)',
                            padding: '24px',
                            borderRadius: '12px',
                            marginBottom: '24px'
                        }}>
                            <h3 style={{ color: 'white', marginBottom: '16px' }}>What's Next?</h3>
                            <ul style={{ color: '#94a3b8', paddingLeft: '20px' }}>
                                <li>Create and manage tasks</li>
                                <li>Set deadlines and priorities</li>
                                <li>Organize by categories</li>
                                <li>Track your progress</li>
                            </ul>
                        </div>

                        <button
                            onClick={logout}
                            className="btn-primary neon-button"
                            style={{ background: 'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)' }}
                        >
                            Sign Out
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

const AppContent = () => {
    const { isAuthenticated, loading } = useAuth();

    if (loading) {
        return (
            <div className="app">
                <div className="loading-spinner"></div>
            </div>
        );
    }

    return (
        <Routes>
            <Route
                path="/auth"
                element={!isAuthenticated ? <AuthPage /> : <Navigate to="/" replace />}
            />
            <Route
                path="/"
                element={
                    <ProtectedRoute>
                        <Dashboard />
                    </ProtectedRoute>
                }
            />
            {/* Добавляем catch-all route для SPA */}
            <Route
                path="*"
                element={<Navigate to={isAuthenticated ? "/" : "/auth"} replace />}
            />
        </Routes>
    );
};

function App() {
    return (
        <AuthProvider>
            <Router>
                <AppContent />
            </Router>
        </AuthProvider>
    );
}

export default App;