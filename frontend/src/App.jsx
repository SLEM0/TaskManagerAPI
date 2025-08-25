import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { useAuth } from './hooks/useAuth';
import RegisterForm from './components/RegisterForm';

const AuthPage = () => {
    const [isLogin, setIsLogin] = React.useState(false);

    return (
        <div className="app">
            <div className="floating-shape shape-1"></div>
            <div className="floating-shape shape-2"></div>

            {!isLogin ? (
                <RegisterForm onSwitchToLogin={() => setIsLogin(true)} />
            ) : (
                <div className=" animate-fade-in">
                    <div className="text-center">
                        <h2 style={{ fontSize: '28px', fontWeight: 'bold', color: 'white', marginBottom: '32px' }}>
                            Login Form Coming Soon
                        </h2>
                        <button
                            onClick={() => setIsLogin(false)}
                            className="text-link"
                            style={{ fontSize: '16px' }}
                        >
                            Go back to Register
                        </button>
                    </div>
                </div>
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

const AppContent = () => {
    const { isAuthenticated } = useAuth();

    return (
        <Routes>
            <Route
                path="/auth"
                element={!isAuthenticated ? <AuthPage /> : <Navigate to="/" />}
            />
            <Route
                path="/"
                element={
                    <ProtectedRoute>
                        <div className="app">
                            <div className="card">
                                <h2 style={{ fontSize: '24px', fontWeight: 'bold', color: 'white', marginBottom: '16px' }}>
                                    Welcome to Task Manager!
                                </h2>
                                <p style={{ color: '#94a3b8' }}>Your dashboard will be here soon.</p>
                            </div>
                        </div>
                    </ProtectedRoute>
                }
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