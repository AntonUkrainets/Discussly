import axios from 'axios';
import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Popup from '../Dialogs/Popup';
import '../Styles/Auth.css';

export default function SignUpPage() {
    const [formData, setFormData] = useState({
        email: '',
        username: '',
        password: '',
        homepage: '',
        captchaCode: ''
    });

    const [popupMessage, setPopupMessage] = useState('');
    const [isError, setIsError] = useState(false);

    const [captchaImage, setCaptchaImage] = useState(null);
    const [isCaptchaVisible, setIsCaptchaVisible] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        if (!popupMessage)
            return;

        const timer = setTimeout(() => {
            handleClosePopup();
        }, 4000);

        return () => clearTimeout(timer);
    }, [popupMessage]);

    const validateEmail = (email) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email) && email.length >= 5 && email.length <= 254;
    const validateUsername = (username) => username.length >= 3 && username.length <= 30;
    const validatePassword = (password) => password.length >= 8 && password.length <= 64;
    const validateHomepage = (homepage) => {
        try {
            new URL(homepage);
            return true;
        } catch {
            return false;
        }
    };

    const validateForm = () => {
        let newError = '';

        if (!validateEmail(formData.email)) {
            newError = 'Invalid email format or length (5-254 characters)';
        }

        if (!validateUsername(formData.username)) {
            newError = 'Username must be between 3 and 30 characters';
        }

        if (!validatePassword(formData.password)) {
            newError = 'Password must be between 8 and 64 characters';
        }

        if (formData.homepage && !validateHomepage(formData.homepage)) {
            newError = 'Invalid URL format';
        }

        if (isCaptchaVisible && !formData.captchaCode) {
            newError = 'Please enter the captcha answer';
        }

        setPopupMessage(newError);
        return newError;
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prevState => ({
            ...prevState,
            [name]: value
        }));
    };

    const fetchCaptcha = async () => {
        if (!validateEmail(formData.email)) {
            setPopupMessage('Please enter a valid email to generate captcha');
            setIsError(true);
            return;
        }

        try {
            const response = await axios.get(`${import.meta.env.VITE_REACT_APP_BASE_URL}/captcha`, {
                params: { email: formData.email },
                responseType: 'blob'
            });

            const blob = response.data;
            const imageUrl = URL.createObjectURL(blob);
            setCaptchaImage(imageUrl);
            setIsCaptchaVisible(true);
            setFormData(prevState => ({ ...prevState, captchaCode: '' }));
        } catch (error) {
            setPopupMessage(error.message);
            setIsError(true);
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setIsError(false);

        if (validateForm() !== '') {
            setIsError(true);
            return;
        }

        if (!isCaptchaVisible) {
            await fetchCaptcha();
            return;
        }

        setIsSubmitting(true);

        try {
            await axios.post(
                `${import.meta.env.VITE_REACT_APP_BASE_URL}/account/sign-up`,
                {
                    email: formData.email,
                    username: formData.username,
                    password: formData.password,
                    homepage: formData.homepage || null,
                    captchaCode: formData.captchaCode
                },
                {
                    withCredentials: true
                }
            );

            navigate('/login');
        } catch (error) {
            if (error.response) {
                const { status } = error.response;
                if (status === 400) {
                    setPopupMessage('The CAPTCHA you entered is incorrect. Please try again.');
                } else if (status === 409) {
                    setPopupMessage('A user with this email or username already exists. Please choose another one.');
                } else {
                    setPopupMessage('An unexpected error occurred. Please try again later.');
                }
            } else {
                setPopupMessage('A network error occurred. Please check your connection or try again later.');
            }
            setIsError(true);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleNavigateToLogin = () => {
        navigate('/login');
    };

    const handleClosePopup = () => {
        setPopupMessage('');
        setIsError(false);
    };

    return (
        <div className="auth-container">
            <div className="auth-form-container">
                <h2>Sign Up</h2>
                <p>Create your account to get started</p>
                <form onSubmit={handleSubmit} className="auth-form">
                    <div className="form-group">
                        <label htmlFor="email">Email</label>
                        <input
                            type="email"
                            id="email"
                            name="email"
                            value={formData.email}
                            onChange={handleChange}
                            required
                            placeholder="you@example.com"
                        />
                    </div>
                    <div className="form-group">
                        <label htmlFor="username">Username</label>
                        <input
                            type="text"
                            id="username"
                            name="username"
                            value={formData.username}
                            onChange={handleChange}
                            required
                            placeholder="johndoe"
                        />
                    </div>
                    <div className="form-group">
                        <label htmlFor="password">Password</label>
                        <input
                            type="password"
                            id="password"
                            name="password"
                            value={formData.password}
                            onChange={handleChange}
                            required
                            placeholder="••••••••"
                        />
                    </div>
                    <div className="form-group">
                        <label htmlFor="homepage">Homepage</label>
                        <input
                            type="url"
                            id="homepage"
                            name="homepage"
                            value={formData.homepage}
                            onChange={handleChange}
                            placeholder="https://yourhomepage.com"
                        />
                    </div>

                    {isCaptchaVisible && (
                        <div className="form-group captcha-group">
                            <div className="form-group__captcha">
                                <img src={captchaImage} alt="Captcha" className="captcha-image" />
                                <button
                                    type="button"
                                    className="refresh-captcha-button"
                                    onClick={fetchCaptcha}
                                >
                                    Refresh
                                </button>
                            </div>
                            <input
                                type="text"
                                id="captcha"
                                name="captchaCode"
                                value={formData.captchaCode}
                                onChange={handleChange}
                                required
                                placeholder="Enter captcha"
                            />
                        </div>
                    )}

                    <button type="submit" className="submit-button" disabled={isSubmitting}>
                        {isCaptchaVisible ? 'Sign Up' : 'Continue'}
                    </button>
                    <button
                        type="button"
                        className="auth-form__auth-button"
                        onClick={handleNavigateToLogin}>
                        Already registered?
                    </button>
                </form>
                {popupMessage && (
                    <Popup
                        isError={isError}
                        message={popupMessage}
                        onClose={handleClosePopup}
                    />
                )}
            </div>
        </div>
    );
}