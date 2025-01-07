import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import '../Styles/Header.css';

export default function Header () {
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const navigate = useNavigate();

    const toggleMenu = () => {
        setIsMenuOpen((prev) => !prev);
    };

    const handleComments = () => {
        navigate('/comments');
    };

    const handleSettings = () => {
        navigate('/settings');
    };

    const handleLogout = () => {
        localStorage.removeItem('authToken');
        navigate('/login');
    };

    return (
        <div className="header">
            <div className="header-container">
                <button className="header-container__button" onClick={handleComments}>Comments</button>
                <div className="header-container__profile-menu">
                    <button
                        className={`header-container__profile-button ${isMenuOpen ? 'menu-open' : ''}`}
                        onClick={toggleMenu}
                    >
                        Profile
                    </button>
                    {isMenuOpen && (
                        <div className="profile-menu">
                            <ul className="profile-menu__list">
                                <li className="profile-menu__item" onClick={handleSettings}>Settings</li>
                                <li className="profile-menu__item" onClick={handleLogout}>
                                    Log Out
                                </li>
                            </ul>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}