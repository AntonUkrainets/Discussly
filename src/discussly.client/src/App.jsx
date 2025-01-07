import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './App.css';

export default function App() {
    const navigate = useNavigate();

    useEffect(() => {
        const token = localStorage.getItem('authToken');
        if (!token) {
            navigate('/login');
        }
    }, [navigate]);

    return (
        <div>
            <h1>Welcome to the App</h1>
        </div>
    );
}