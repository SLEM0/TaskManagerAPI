import React, { createContext, useState, useEffect } from 'react';
import { authApi } from '../services/api';

// Создаем Context
const AuthContext = createContext();

// Провайдер, который будет оборачивать наше приложение
export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const token = localStorage.getItem('authToken');
        if (token) {
            setUser({});
        }
        setLoading(false);
    }, []);

    const register = async (userData) => {
        try {
            const response = await authApi.register(userData);
            const { token } = response.data;

            localStorage.setItem('authToken', token);
            setUser({ email: userData.email });

            return { success: true };
        } catch (error) {
            return {
                success: false,
                error: error.response?.data?.message || 'Registration failed'
            };
        }
    };

    const login = async (credentials) => {
        try {
            const response = await authApi.login(credentials);
            const { token } = response.data;

            localStorage.setItem('authToken', token);
            setUser({ email: credentials.email });

            return { success: true };
        } catch (error) {
            return {
                success: false,
                error: error.response?.data?.message || 'Login failed'
            };
        }
    };

    const logout = () => {
        localStorage.removeItem('authToken');
        setUser(null);
        authApi.logout().catch(console.error);
    };

    const value = {
        user,
        loading,
        register,
        login,
        logout,
        isAuthenticated: !!user,
    };

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
};

// Экспортируем контекст для использования в хуке
export { AuthContext };