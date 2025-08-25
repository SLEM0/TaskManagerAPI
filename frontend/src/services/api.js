import axios from 'axios';

const API_BASE_URL = 'https://localhost:8081/api';

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

api.interceptors.request.use((config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export const authApi = {
    register: (userData) => api.post('/Auth/register', userData),
    login: (credentials) => api.post('/Auth/login', credentials),
    refresh: (refreshToken) => api.post('/Auth/refresh', { refreshToken }),
    logout: () => api.post('/Auth/revoke'),
};

export default api;