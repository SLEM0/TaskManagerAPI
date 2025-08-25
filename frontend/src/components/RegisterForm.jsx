import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Eye, EyeOff, Mail, User, Lock, Sparkles, Check, X } from 'lucide-react';
import { useAuth } from '../hooks/useAuth';

const RegisterForm = ({ onSwitchToLogin }) => {
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);
    const [serverError, setServerError] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { register: registerUser, loading } = useAuth();

    const {
        register,
        handleSubmit,
        formState: { errors },
        watch,
        trigger
    } = useForm();

    const password = watch('password');
    const username = watch('username');
    const email = watch('email');

    const onSubmit = async (data) => {
        setServerError('');
        setIsSubmitting(true);
        const result = await registerUser(data);

        if (!result.success) {
            setServerError(result.error);
        }
        setIsSubmitting(false);
    };

    // Функция для проверки сложности пароля
    const getPasswordStrength = (pass) => {
        if (!pass) return 0;
        let strength = 0;
        if (pass.length >= 6) strength += 20;
        if (pass.match(/[a-z]+/)) strength += 20;
        if (pass.match(/[A-Z]+/)) strength += 20;
        if (pass.match(/[0-9]+/)) strength += 20;
        if (pass.match(/[!@#$%^&*(),.?":{}|<>]+/)) strength += 20;
        return strength;
    };

    const passwordStrength = getPasswordStrength(password);

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
                            Create Account
                        </h2>
                        <p className="card-subtitle">
                            Join us to start managing your tasks
                        </p>
                    </div>

                    <form onSubmit={handleSubmit(onSubmit)} className="form-container">
                        {serverError && (
                            <div className="server-error">
                                <X size={18} />
                                <span>{serverError}</span>
                            </div>
                        )}

                        <div className="input-group">
                            <div className="input-wrapper">
                                <User className="input-icon" />
                                <input
                                    {...register('username', {
                                        required: 'Username is required',
                                        minLength: {
                                            value: 3,
                                            message: 'Username must be at least 3 characters'
                                        }
                                    })}
                                    placeholder=" "
                                    className="input-field"
                                    onBlur={() => trigger('username')}
                                />
                                <label className="input-label">Username</label>
                            </div>
                            {errors.username && (
                                <span className="error-message">
                                    <X size={14} />
                                    {errors.username.message}
                                </span>
                            )}
                            {username && !errors.username && (
                                <span className="success-message">
                                    <Check size={14} />
                                    Username is available
                                </span>
                            )}
                        </div>

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
                                    onBlur={() => trigger('email')}
                                />
                                <label className="input-label">Email</label>
                            </div>
                            {errors.email && (
                                <span className="error-message">
                                    <X size={14} />
                                    {errors.email.message}
                                </span>
                            )}
                            {email && !errors.email && (
                                <span className="success-message">
                                    <Check size={14} />
                                    Email is valid
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
                                    onBlur={() => trigger('password')}
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

                            {password && (
                                <div className="password-strength">
                                    <div
                                        className="strength-bar"
                                        style={{ width: `${passwordStrength}%` }}
                                        data-strength={passwordStrength}
                                    ></div>
                                </div>
                            )}

                            {errors.password && (
                                <span className="error-message">
                                    <X size={14} />
                                    {errors.password.message}
                                </span>
                            )}
                            {password && !errors.password && (
                                <span className="success-message">
                                    <Check size={14} />
                                    Password strength: {passwordStrength >= 60 ? 'Strong' : passwordStrength >= 40 ? 'Medium' : 'Weak'}
                                </span>
                            )}
                        </div>

                        <div className="input-group">
                            <div className="input-wrapper">
                                <Lock className="input-icon" />
                                <input
                                    type={showConfirmPassword ? 'text' : 'password'}
                                    {...register('confirmPassword', {
                                        required: 'Please confirm your password',
                                        validate: value =>
                                            value === password || 'Passwords do not match'
                                    })}
                                    placeholder=" "
                                    className="input-field"
                                    onBlur={() => trigger('confirmPassword')}
                                />
                                <label className="input-label">Confirm Password</label>
                                <button
                                    type="button"
                                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                                    className="password-toggle"
                                >
                                    {showConfirmPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                                </button>
                            </div>
                            {errors.confirmPassword && (
                                <span className="error-message">
                                    <X size={14} />
                                    {errors.confirmPassword.message}
                                </span>
                            )}
                            {watch('confirmPassword') && !errors.confirmPassword && (
                                <span className="success-message">
                                    <Check size={14} />
                                    Passwords match
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
                                    Creating Account...
                                </>
                            ) : (
                                'Create Account'
                            )}
                        </button>
                    </form>

                    <div className="card-footer">
                        <p>
                            Already have an account?{' '}
                            <button
                                onClick={onSwitchToLogin}
                                className="text-link"
                            >
                                Sign In
                            </button>
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default RegisterForm;