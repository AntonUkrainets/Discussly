import PropTypes from 'prop-types';
import '../Styles/Popup.css';

export default function Popup({ isError, message, onClose }) {
    const dialogClassName = isError
        ? 'message-dialog message-dialog-error'
        : 'message-dialog message-dialog-success';

    return (
        <div className="message-dialog-backdrop" onClick={onClose}>
            <div className={dialogClassName} onClick={(e) => e.stopPropagation()}>
                <button onClick={onClose} className="message-dialog-close-button">
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="15"
                        height="15"
                        fill="none"
                        viewBox="0 0 15 15"
                    >
                        <path
                            fill="white"
                            fillOpacity="1"
                            fillRule="nonzero"
                            d="M14.68 13.16c.1.1.18.22.23.35.06.13.08.27.08.41 0 .14-.02.28-.08.41-.05.13-.13.25-.23.35-.1.1-.22.18-.35.23a.978.978 0 01-.82 0c-.13-.05-.25-.13-.35-.23L7.49 9.01l-5.66 5.67c-.1.1-.22.18-.35.23-.13.06-.27.08-.41.08-.14 0-.28-.02-.41-.08-.13-.05-.25-.13-.35-.23-.1-.1-.18-.22-.23-.35a.969.969 0 01-.08-.41c0-.14.02-.28.08-.41.05-.13.13-.25.23-.35L5.98 7.5.31 1.83c-.1-.1-.18-.22-.23-.35A.968.968 0 010 1.07C0 .93.02.79.08.66.13.53.21.41.31.31.41.21.53.13.66.08.79.02.93 0 1.07 0c.14 0 .28.02.41.08.13.05.25.13.35.23l5.66 5.67L13.16.31c.1-.1.22-.18.35-.23.13-.06.27-.08.41-.08.14 0 .28.02.41.08.13.05.25.13.35.23.1.1.18.22.23.35a.978.978 0 010 .82c-.05.13-.13.25-.23.35L9.01 7.5l5.67 5.66z"
                        ></path>
                    </svg>
                </button>
                <h3>{message}</h3>
            </div>
        </div>
    );
}

Popup.propTypes = {
    isError: PropTypes.bool.isRequired,
    message: PropTypes.string.isRequired,
    onClose: PropTypes.func.isRequired,
};