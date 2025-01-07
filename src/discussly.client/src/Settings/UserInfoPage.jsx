import axios from 'axios';
import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { DEFAULT_AVATAR_URL } from '../Config/Constants';
import Header from '../Header/Header';
import '../Styles/UserInfoPage.css';

export default function UserInfoPage() {
    const [avatar, setAvatar] = useState("");
    const [avatarFile, setAvatarFile] = useState(null);
    const [homepage, setHomepage] = useState("");
    const [email, setEmail] = useState("");
    const [username, setUsername] = useState("");
    const [isEdited, setIsEdited] = useState(false);

    const [originalData, setOriginalData] = useState({
        avatar: "",
        homepage: "",
        email: "",
        username: ""
    });

    const navigate = useNavigate();

    useEffect(() => {
        const authToken = localStorage.getItem('authToken');
        if (!authToken) {
            navigate('/login');
            return;
        }

        const fetchUserInfo = async () => {
            try {
                const response = await axios.get(
                    `${import.meta.env.VITE_REACT_APP_BASE_URL}/userInfo`,
                    {
                        headers: {
                            'Authorization': `Bearer ${authToken}`,
                        },
                    }
                );

                const data = response.data;
                const avatarUrl = data.avatarUrl || DEFAULT_AVATAR_URL;
                const homepageValue = data.homePage || "";

                setAvatar(avatarUrl);
                setHomepage(homepageValue);
                setEmail(data.email);
                setUsername(data.username);

                setOriginalData({
                    avatar: avatarUrl,
                    homepage: homepageValue,
                    email: data.email,
                    username: data.username
                });
            } catch (error) {
                if (error.response && error.response.status === 403) {
                    localStorage.removeItem('authToken');
                }
                navigate('/login');
            }
        };

        fetchUserInfo();
    }, [navigate]);

    const handleAvatarChange = (e) => {
        const file = e.target.files?.[0];
        if (file) {
            setAvatarFile(file);
            const reader = new FileReader();
            reader.onloadend = () => {
                setAvatar(reader.result);
                setIsEdited(true);
            };
            reader.readAsDataURL(file);
        }
    };

    const handleRemoveAvatar = () => {
        setAvatar(DEFAULT_AVATAR_URL);
        setAvatarFile(null);
        setIsEdited(true);
    };

    const handleHomepageChange = (e) => {
        setHomepage(e.target.value);
        setIsEdited(true);
    };

    const handleSave = async () => {
        try {
            const authToken = localStorage.getItem('authToken');
            const formData = new FormData();

            let avatarAction = 'None';

            if (avatarFile) {
                avatarAction = 'Update';
                formData.append('FileStream', avatarFile);
            } else if (avatar === DEFAULT_AVATAR_URL && originalData.avatar !== DEFAULT_AVATAR_URL) {
                avatarAction = 'Delete';
                formData.append('FileStream', '');
            } else {
                formData.append('FileStream', '');
            }

            formData.append('AvatarAction', avatarAction);
            formData.append('HomePage', homepage || '');
            console.log(formData);
            await axios.put(
                `${import.meta.env.VITE_REACT_APP_BASE_URL}/userInfo`,
                formData,
                {
                    headers: {
                        'Authorization': `Bearer ${authToken}`,
                        'Content-Type': 'multipart/form-data',
                    },
                }
            );

            setIsEdited(false);
            setOriginalData({
                avatar,
                homepage,
                email,
                username
            });
        } catch (error) {
            console.error('Error saving changes:', error);
        }
    };

    const handleDiscard = () => {
        setAvatar(originalData.avatar);
        setHomepage(originalData.homepage);
        setEmail(originalData.email);
        setUsername(originalData.username);
        setAvatarFile(null);
        setIsEdited(false);
    };

    return (
        <>
            <Header />
            <div className="settings-container">
                <h2 className="settings-container__title">Settings</h2>
                <div className="settings-container__form">
                    <div className="settings-container__avatar">
                        <div className="settings-container__avatar-wrapper">
                            {avatar !== DEFAULT_AVATAR_URL && (
                                <button className="settings-container__avatar-delete" onClick={handleRemoveAvatar}>
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 20 20"><path fill="red" d="M10 20a10 10 0 1 1 0-20a10 10 0 0 1 0 20m5-11H5v2h10z" /></svg>
                                </button>
                            )}
                            <img
                                src={avatar || DEFAULT_AVATAR_URL}
                                alt="User avatar"
                                className="avatar"
                            />
                            <label htmlFor="avatar-upload" className="settings-container__avatar-upload">+</label>
                            <input
                                id="avatar-upload"
                                type="file"
                                accept="image/*"
                                className="hidden-input"
                                onChange={handleAvatarChange}
                            />
                        </div>
                        {avatar !== DEFAULT_AVATAR_URL && (
                            <button className="remove-button" onClick={handleRemoveAvatar}>
                                Remove Photo
                            </button>
                        )}
                    </div>

                    <div className="form-group">
                        <label htmlFor="email">Email</label>
                        <input
                            id="email"
                            type="email"
                            value={email}
                            disabled
                            className="input"
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="username">Username</label>
                        <input
                            id="username"
                            value={username}
                            disabled
                            className="input"
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="homepage">Homepage</label>
                        <input
                            id="homepage"
                            value={homepage}
                            onChange={handleHomepageChange}
                            placeholder="https://yourhomepage.com"
                            className="input"
                        />
                    </div>

                    <div className="button-group">
                        <button
                            onClick={handleSave}
                            disabled={!isEdited}
                            className="save-button"
                        >
                            Save Changes
                        </button>
                        <button
                            onClick={handleDiscard}
                            disabled={!isEdited}
                            className="discard-button"
                        >
                            Discard
                        </button>
                    </div>
                </div>
            </div>
        </>
    );
}