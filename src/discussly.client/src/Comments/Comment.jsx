import "../Styles/Comment.css";
import { DEFAULT_AVATAR_URL } from '../Config/Constants';
import PropTypes from 'prop-types';

export default function Comment({ comment, level = 0, onReply }) {
    return (
        <div
            className="comment-container"
            style={{
                marginLeft: `${level * 20}px`,
            }}
        >
            <div className={`comment ${comment.childComments?.length > 0 ? 'parent-comment' : ''}`}>
                <div className="comment-content">
                    <div className="comment-header">
                        <div className="comment-header__info-section">
                            <div className="comment-author">
                                <img
                                    src={comment.userAvatar || DEFAULT_AVATAR_URL}
                                    alt="avatar"
                                    className="comment__avatar"
                                />
                                <span className="author-name">{comment.userName}</span>
                                <span className="timestamp">
                                    {new Date(comment.createdAt).toLocaleString()}
                                </span>
                            </div>
                        </div>
                        <button className="comment-container__reply-button" onClick={() => onReply(comment.id, comment.rootCommentId || comment.id)}>
                            <svg xmlns="http://www.w3.org/2000/svg" width="22.5" height="22.5" viewBox="0 0 24 24"><path fill="blue" d="M11 20L1 12l10-8v5c5.523 0 10 4.477 10 10q0 .41-.032.81A9 9 0 0 0 13 15h-2z" /></svg>
                        </button>
                    </div>
                    <p
                        className="comment-text"
                        dangerouslySetInnerHTML={{
                            __html: comment.text.replace(/\n/g, '<br />')
                        }}
                    ></p>
                    {comment.attachmentUrl && (
                        <div className="comment-attachment">
                            <a
                                href={comment.attachmentUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="attachment-link"
                            >
                                {comment.attachmentFileName || 'Attached File'}
                            </a>
                        </div>
                    )}
                </div>
            </div>

            {comment.childComments && comment.childComments.length > 0 && (
                <div className="child-comments">
                    {comment.childComments.map((child) => (
                        <Comment
                            key={child.id}
                            comment={child}
                            level={level + 1}
                            onReply={onReply}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}

Comment.propTypes = {
    comment: PropTypes.shape({
        userName: PropTypes.string.isRequired,
        userAvatar: PropTypes.string.isRequired,
        createdAt: PropTypes.string.isRequired,
        text: PropTypes.string.isRequired,
        attachmentUrl: PropTypes.string,
        attachmentFileName: PropTypes.string,
        childComments: PropTypes.arrayOf(PropTypes.object),
        id: PropTypes.string.isRequired,
        rootCommentId: PropTypes.string,
    }).isRequired,
    level: PropTypes.number,
    onReply: PropTypes.func.isRequired,
};