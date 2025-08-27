import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Eye, EyeOff, Mail, Lock, Sparkles, X, Check } from 'lucide-react';
import { useAuth } from '../hooks/useAuth';

const LoginForm = ({ onSwitchToRegister }) => {
    const [showPassword, setShowPassword] = useState(false);
    const [serverError, setServerError] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isAutoCompleteDisabled, setIsAutoCompleteDisabled] = useState(true);
    const [touchedFields, setTouchedFields] = useState({});
    const { login, loading } = useAuth();

    const {
        register,
        handleSubmit,
        formState: { errors },
        watch,
        trigger
    } = useForm();

    useEffect(() => {
        const timer = setTimeout(() => {
            setIsAutoCompleteDisabled(false);
        }, 100);

        return () => clearTimeout(timer);
    }, []);

    const handleFocus = (e) => {
        if (isAutoCompleteDisabled) {
            e.target.removeAttribute('readonly');
        }
    };

    const handleBlur = async (fieldName) => {
        setTouchedFields(prev => ({ ...prev, [fieldName]: true }));
        await trigger(fieldName);
    };

    const onSubmit = async (data) => {
        setServerError('');
        setIsSubmitting(true);

        const result = await login(data);

        if (!result.success) {
            setServerError(result.error);
        }
        setIsSubmitting(false);
    };

    // Функции для проверки, показывать ли сообщения
    const shouldShowError = (fieldName) => touchedFields[fieldName] && errors[fieldName];
    const shouldShowSuccess = (fieldName) => touchedFields[fieldName] && !errors[fieldName] && watch(fieldName);

    return (
        <div className="auth-container">
            {/* Анимированный фон */}
            <div className="animated-background">
                <div className="floating-shape shape-1"></div>
                <div className="floating-shape shape-2"></div>
                <div className="floating-shape shape-3"></div>
                <div className="floating-shape shape-4"></div>
            </div>

            <div className="auth-wrapper">
                <div className="card glass-card animate-slide-in">
                    <div className="card-header">
                        <div className="logo-wrapper">
                            <Sparkles className="logo-icon" />
                            <span className="logo-text">TaskFlow</span>
                        </div>
                        <h2 className="card-title">
                            Welcome Back
                        </h2>
                        <p className="card-subtitle">
                            Sign in to continue managing your tasks
                        </p>
                    </div>

                    <form onSubmit={handleSubmit(onSubmit)} className="form-container" autoComplete="off">
                        {serverError && (
                            <div className="server-error">
                                <X size={18} />
                                <span>{serverError}</span>
                            </div>
                        )}

                        <div className="input-group">
                            <div className="input-wrapper">
                                <Mail className="input-icon" />
                                <input
                                    type="email"
                                    {...register('email', {
                                        required: 'Email is required',
                                        pattern: {
                                            value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                                            message: 'Invalid email address'
                                        }
                                    })}
                                    placeholder=" "
                                    className="input-field"
                                    onBlur={() => handleBlur('email')}
                                    readOnly={isAutoCompleteDisabled}
                                    onFocus={handleFocus}
                                />
                                <label className="input-label">Email</label>
                            </div>
                            {shouldShowError('email') && (
                                <span className="error-message">
                                    <X size={14} />
                                    {errors.email.message}
                                </span>
                            )}
                            {shouldShowSuccess('email') && (
                                <span className="success-message">
                                    <Check size={14} />
                                    Email looks good
                                </span>
                            )}
                        </div>

                        <div className="input-group">
                            <div className="input-wrapper">
                                <Lock className="input-icon" />
                                <input
                                    type={showPassword ? 'text' : 'password'}
                                    {...register('password', {
                                        required: 'Password is required',
                                        minLength: {
                                            value: 6,
                                            message: 'Password must be at least 6 characters'
                                        }
                                    })}
                                    placeholder=" "
                                    className="input-field"
                                    onBlur={() => handleBlur('password')}
                                    readOnly={isAutoCompleteDisabled}
                                    onFocus={handleFocus}
                                />
                                <label className="input-label">Password</label>
                                <button
                                    type="button"
                                    onClick={() => setShowPassword(!showPassword)}
                                    className="password-toggle"
                                >
                                    {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                                </button>
                            </div>
                            {shouldShowError('password') && (
                                <span className="error-message">
                                    <X size={14} />
                                    {errors.password.message}
                                </span>
                            )}
                            {shouldShowSuccess('password') && (
                                <span className="success-message">
                                    <Check size={14} />
                                    Password looks good
                                </span>
                            )}
                        </div>

                        <button
                            type="submit"
                            disabled={loading || isSubmitting}
                            className="btn-primary neon-button"
                        >
                            {loading || isSubmitting ? (
                                <>
                                    <div className="loading-spinner"></div>
                                    Signing In...
                                </>
                            ) : (
                                'Sign In'
                            )}
                        </button>
                    </form>

                    <div className="card-footer">
                        <p>
                            Don't have an account?{' '}
                            <button
                                onClick={onSwitchToRegister}
                                className="text-link"
                            >
                                Create Account
                            </button>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default LoginForm;