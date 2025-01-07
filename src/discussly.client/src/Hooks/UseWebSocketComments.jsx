import { useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';

const useWebSocketComments = (onNewComment) => {
    const onNewCommentRef = useRef(onNewComment);
    const navigate = useNavigate();

    useEffect(() => {
        onNewCommentRef.current = onNewComment;
    }, [onNewComment]);

    useEffect(() => {
        const socket = new WebSocket(import.meta.env.VITE_REACT_APP_WEBSOCKET_URL);

        socket.onopen = () => {
            //console.log('✅ WebSocket connection established');
        };

        socket.onmessage = (event) => {
            try {
                const data = JSON.parse(event.data);

                if (data.status === 403) {
                    localStorage.removeItem('authToken');
                    navigate('/login');
                    return;
                }

                if (data.type === 'NEW_COMMENT' && data.comment_id) {
                    onNewCommentRef.current({ id: data.comment_id });
                }

            } catch (error) {
                //console.error('❌ Error parsing WebSocket message:', error);
            }
        };

        socket.onerror = (error) => {
            //console.error('❌ WebSocket error:', error);
        };

        socket.onclose = (event) => {
            //console.warn('⚠️ WebSocket connection closed:', event.reason || 'No reason');
        };

        return () => {
            if (socket.readyState === WebSocket.OPEN) {
                socket.close(1000, 'Component unmounted');
            }
        };
    }, []);
};

export default useWebSocketComments;