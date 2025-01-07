import { ApolloProvider } from '@apollo/client';
import React from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import LoginPage from './Auth/LoginPage';
import SignUpPage from './Auth/SignUpPage';
import AuthProvider from './AuthContext';
import client from './Comments/ApolloClient';
import CommentThread from './Comments/CommentThread';
import UserInfoPage from './Settings/UserInfoPage';
import './index.css';

const root = createRoot(document.getElementById('root'));

root.render(
    <ApolloProvider client={client}>
        <AuthProvider>
            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<Navigate to="/login" replace />} />
                    <Route path="/sign-up" element={<SignUpPage />} />
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/comments" element={<CommentThread />} />
                    <Route path="/settings" element={<UserInfoPage />} />
                    <Route path="*" element={<Navigate to="/login" replace />} />
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    </ApolloProvider>
);