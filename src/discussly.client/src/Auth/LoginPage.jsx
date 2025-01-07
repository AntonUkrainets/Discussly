import axios from 'axios';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Popup from '../Dialogs/Popup';

export default function LoginPage() {
    const [formData, setFormData] = useState({
        email: '',
        password: ''
    });

    const [popupMessage, setPopupMessage] = useState('');
    const [isError, setIsError] = useState(false);

    const navigate = useNavigate();

    useEffect(() => {
        if (!popupMessage)
            return;

        const timer = setTimeout(() => {
            handleClosePopup();
        }, 4000);

        return () => clearTimeout(timer);
    }, [popupMessage]);

    const validateEmail = (email) => {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email) && email.length >= 5 && email.length <= 254;
    };

    const validatePassword = (password) => {
        return password.length >= 8 && password.length <= 64;
    };

    const validateForm = () => {
        let newError = '';

        if (!validateEmail(formData.email)) {
            newError = 'Invalid email format or length (5-254 characters)';
        }
        if (!validatePassword(formData.password)) {
            newError = 'Password must be between 8 and 64 characters';
        }

        setPopupMessage(newError);
        return newError;
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData((prevState) => ({
            ...prevState,
            [name]: value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setIsError(false);

        if (validateForm() !== '') {
            setIsError(true);
            return;
        }

        try {
            const response = await axios.post(
                `${import.meta.env.VITE_REACT_APP_BASE_URL}/account/login`,
                {
                    email: formData.email,
                    password: formData.password
                }
            );

            if (response.status === 200 && response.data.token) {
                setPopupMessage('Success');
                setIsError(false);

                localStorage.setItem('authToken', response.data.token);
                navigate('/comments');
            }
        } catch (error) {
            if (error.response && error.response.status === 401) {
                setPopupMessage('Invalid email or password');
            } else if (error.response && error.response.status === 404) {
                setPopupMessage('User not found or invalid email');
            } else if (error.response && error.response.status === 400) {
                setPopupMessage(`${error.response.data}`);
            } else {
                setPopupMessage('An unexpected error occurred. Please try again later.');
            }
            setIsError(true);
        }
    };

    const handleNavigateToSignUp = () => {
        navigate('/sign-up');
    };

    const handleClosePopup = () => {
        setPopupMessage('');
        setIsError(false);
    };

    return (
        <div className="auth-container">
            <div className="auth-form-container">
                <h2>Log In</h2>
                <p style={{ textAlign: "center", textWrapStyle: "balance" }}>
                    Welcome back! Please login to your account.
                </p>
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

                    <p>Don&apos;t have an account?</p>
                    <button
                        type="button"
                        className="auth-form__auth-button"
                        onClick={handleNavigateToSignUp}
                    >
                        Sign up
                    </button>

                    <button type="submit" className="submit-button">
                        Log In
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