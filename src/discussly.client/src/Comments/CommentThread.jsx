import { useLazyQuery } from '@apollo/client';
import { Search } from 'lucide-react';
import React, { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { GET_COMMENT_BY_ID, GET_LATEST_COMMENTS } from '../Config/Constants';
import Header from '../Header/Header';
import '../Styles/CommentThread.css';
import UseWebSocketComments from '../Hooks/UseWebSocketComments';
import Comment from './Comment';

export default function CommentThread() {
    const navigate = useNavigate();

    const [comments, setComments] = useState([]);
    const [totalComments, setTotalComments] = useState(0);

    const [pageIndex, setPageIndex] = useState(1);
    const [totalPages, setTotalPages] = useState(1);

    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [isPreviewOpen, setIsPreviewOpen] = useState(false);
    const [selectedFile, setSelectedFile] = useState(null);
    const [parentCommentId, setParentCommentId] = useState(null);
    const [rootCommentId, setRootCommentId] = useState(null);
    const [commentText, setCommentText] = useState('');
    const textareaRef = useRef(null);

    const [sortBy, setSortBy] = useState('UserName');
    const [sortDirection, setSortDirection] = useState('DESC');

    const [searchQuery, setSearchQuery] = useState('');

    const [fetchComments, { data: commentsData, error }] = useLazyQuery(GET_LATEST_COMMENTS, {
        fetchPolicy: 'network-only',
    });

    const [fetchCommentById] = useLazyQuery(GET_COMMENT_BY_ID);

    useEffect(() => {
        const authToken = localStorage.getItem('authToken');

        if (!authToken) {
            navigate('/login');
        }
    }, [navigate]);

    UseWebSocketComments(async (newComment) => {
        try {
            if (!newComment?.id)
                return;

            const commentExists = comments.some((comment) => String(comment.id) === String(newComment.id));
            if (commentExists)
                return;

            const { data } = await fetchCommentById({
                variables: {
                    request: { id: newComment.id }
                }
            });

            if (data?.commentById) {
                setComments((prevComments) => {
                    const updatedComments = insertCommentIntoTree(
                        [...prevComments],
                        data.commentById,
                        data.commentById.parentCommentId
                    );

                    if (updatedComments.length > 25)
                        updatedComments.pop();

                    return updatedComments;
                });


                setTotalComments((prev) => {
                    const updatedTotal = prev + 1;
                    setTotalPages(Math.ceil(updatedTotal / 25));
                    return updatedTotal;
                });
            }
        } catch (error) {
            console.error('❌ Error fetching comment by ID:', error);
        }
    });

    const insertCommentIntoTree = (comments, newComment, parentId) => {
        if (!parentId) {
            return [...comments, newComment];
        }

        return comments.map((comment) => {
            let mutableComment = { ...comment };

            if (String(comment.id) === String(parentId)) {
                mutableComment.childComments = [
                    ...(comment.childComments || []),
                    newComment
                ];
                return mutableComment;
            }

            if (comment.childComments?.length > 0) {
                mutableComment.childComments = insertCommentIntoTree(
                    [...comment.childComments],
                    newComment,
                    parentId
                );
            }

            return mutableComment;
        });
    };

    useEffect(() => {
        fetchComments({
            variables: {
                request: {
                    pageIndex,
                    pageSize: 25,
                    sortBy,
                    sortDirection,
                    searchText: searchQuery,
                },
            },
        });
    }, [pageIndex, searchQuery, sortBy, sortDirection, fetchComments]);

    useEffect(() => {
        if (commentsData?.latestComments) {
            setComments(commentsData.latestComments.comments);
            setTotalComments(commentsData.latestComments.totalComments);

            const calculatedPages = commentsData.latestComments.totalComments > 0
                ? Math.ceil(commentsData.latestComments.totalComments / 25)
                : 1;

            setTotalPages(calculatedPages);
        }
    }, [commentsData]);

    const handleSearchChange = (e) => {
        setSearchQuery(e.target.value);
        setPageIndex(1);
    };

    const handleNextPage = () => {
        if (pageIndex < totalPages) {
            setPageIndex((prev) => prev + 1);
        }
    };

    const handlePreviousPage = () => {
        if (pageIndex > 1) {
            setPageIndex((prev) => prev - 1);
        }
    };

    const toggleDialog = () => {
        setIsDialogOpen((prev) => !prev);
        setParentCommentId(null);
        setRootCommentId(null);
    };

    const togglePreview = () => {
        setIsPreviewOpen((prev) => !prev);
    };

    const handleCommentTextChange = (e) => {
        if (e.target.value.length <= 1000) {
            setCommentText(e.target.value);
        }
    };

    const handleReply = (parentId, rootId) => {
        setParentCommentId(parentId);
        setRootCommentId(rootId || parentId);
        setIsDialogOpen(true);
    };

    if (error) return <p>Error: {error.message}</p>;

    const handleFileUpload = (e) => {
        const file = e.target.files[0];
        if (file) {
            const allowedFileTypes = [
                'image/jpeg',
                'image/png',
                'image/gif',
                'text/plain'
            ];

            if (!allowedFileTypes.includes(file.type)) {
                alert('Only JPG, GIF, PNG, and TXT files are allowed.');
                e.target.value = '';
                return;
            }

            if (file.type === 'text/plain' && file.size > 100 * 1024) {
                alert('TXT files must not exceed 100KB.');
                e.target.value = '';
                return;
            }

            setSelectedFile(file);
            e.target.value = '';
        }
    };

    const handleRemoveFile = () => {
        setSelectedFile(null);
        const fileInput = document.getElementById('file-input');
        if (fileInput) {
            fileInput.value = '';
        }
    };

    const handleSubmitComment = async () => {
        const trimmedText = commentText.trim();

        if (!trimmedText) {
            alert('Comment text cannot be empty.');
            return;
        }

        if (!isValidXHTML(trimmedText)) {
            alert('Invalid XHTML structure! Please check your tags.');
            return;
        }

        try {
            const formData = new FormData();

            formData.append('Text', trimmedText.replace(/\n/g, '\\n'))

            if (parentCommentId) {
                formData.append('ParentCommentId', parentCommentId);
            }

            if (rootCommentId) {
                formData.append('RootCommentId', rootCommentId);
            }

            if (selectedFile) {
                formData.append('FileStream', selectedFile);
            }

            const authToken = localStorage.getItem('authToken');

            if (!authToken) {
                navigate('/login');
                return;
            }

            const response = await fetch(`${import.meta.env.VITE_REACT_APP_BASE_URL}/comments`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${authToken}`
                },
                body: formData,
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Failed to add comment');
            }

            setIsDialogOpen(false);
            setSelectedFile(null);
            setParentCommentId(null);
            setRootCommentId(null);
            setCommentText('');
        } catch (error) {
            alert(`Error: ${error.message}`);
        }
    };

    const isValidXHTML = (text) => {
        const parser = new DOMParser();
        const parsed = parser.parseFromString(`<div>${text}</div>`, 'application/xhtml+xml');
        return !parsed.querySelector('parsererror');
    };

    const insertAnchorTag = () => {
        const textarea = textareaRef.current;
        if (!textarea) return;

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const value = textarea.value;

        const newText =
            value.substring(0, start) +
            '[a href="" title=""][/a]' +
            value.substring(end);

        if (isValidXHTML(newText)) {
            setCommentText(newText);

            setTimeout(() => {
                textarea.selectionStart = textarea.selectionEnd = start + 9;
                textarea.focus();
            }, 0);
        } else {
            alert('Invalid XHTML structure! Please check your tags.');
        }
    };

    const insertStrongTag = () => {
        const textarea = textareaRef.current;
        if (!textarea) return;

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const value = textarea.value;

        const newText = value.substring(0, start) + '[strong][/strong]' + value.substring(end);

        if (isValidXHTML(newText)) {
            setCommentText(newText);

            setTimeout(() => {
                textarea.selectionStart = textarea.selectionEnd = start + 8;
                textarea.focus();
            }, 0);
        } else {
            alert('Invalid XHTML structure! Please check your tags.');
        }
    };


    const insertCodeTag = () => {
        const textarea = textareaRef.current;
        if (!textarea) return;

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const value = textarea.value;

        const newText = value.substring(0, start) + '[code][/code]' + value.substring(end);

        if (isValidXHTML(newText)) {
            setCommentText(newText);

            setTimeout(() => {
                textarea.selectionStart = textarea.selectionEnd = start + 6;
                textarea.focus();
            }, 0);
        } else {
            alert('Invalid XHTML structure! Please check your tags.');
        }
    };


    const insertItalicTag = () => {
        const textarea = textareaRef.current;
        if (!textarea) return;

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const value = textarea.value;

        const newText = value.substring(0, start) + '[i][/i]' + value.substring(end);

        if (isValidXHTML(newText)) {
            setCommentText(newText);

            setTimeout(() => {
                textarea.selectionStart = textarea.selectionEnd = start + 3;
                textarea.focus();
            }, 0);
        } else {
            alert('Invalid XHTML structure! Please check your tags.');
        }
    };

    const formatCommentText = (text) => {
        return text
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/\n/g, "<br />")

            .replace(
                /\[a\s+href="([^"]+)"(?:\s+title="([^"]*)")?\]/g,
                (_, href, title) => {
                    const titleAttr = title ? ` title="${title}"` : '';
                    return `<a href="${href}"${titleAttr}>`;
                }
            )
            .replace(/\[\/a\]/g, "</a>")

            .replace(/\[i\]/g, "<i>")
            .replace(/\[\/i\]/g, "</i>")
            .replace(/\[code\]/g, "<code>")
            .replace(/\[\/code\]/g, "</code>")
            .replace(/\[strong\]/g, "<strong>")
            .replace(/\[\/strong\]/g, "</strong>");
    };

    return (
        <>

            <Header />

            <div className="pagination first">
                <button onClick={handlePreviousPage} disabled={pageIndex === 1 || totalPages === 1}>
                    Previous Page
                </button>
                <span>
                    Page {totalPages > 0 ? pageIndex : 1} of {totalPages}
                </span>
                <button onClick={handleNextPage} disabled={pageIndex >= totalPages || totalPages === 1}>
                    Next Page
                </button>
            </div>

            <div className="comment-thread__search-block">
                <div className="search-bar">
                    <Search size={22.5} />
                    <input
                        type="text"
                        value={searchQuery}
                        onChange={handleSearchChange}
                        placeholder="Search comments..."
                        className="search-input"
                    />
                </div>

                <div className="sort-controls">
                    <label>Sort By:</label>
                    <select value={sortBy} onChange={(e) => setSortBy(e.target.value)}>
                        <option value="UserName">User Name</option>
                        <option value="Email">Email</option>
                        <option value="CreatedDate">Date</option>
                    </select>

                    <label>Direction:</label>
                    <select value={sortDirection} onChange={(e) => setSortDirection(e.target.value)}>
                        <option value="ASC">Ascending</option>
                        <option value="DESC">Descending</option>
                    </select>
                </div>
            </div>

            <div className="comment-thread">
                {comments.map((comment) => (
                    <div className={`comment-branch ${comment.childComments?.length > 0 ? 'branch' : ''}`}
                        key={String(comment.id)}>
                        <Comment comment={comment} onReply={handleReply} />
                    </div>
                ))}
            </div>

            <button
                className={`add-comment ${isDialogOpen ? 'dialog-open' : ''}`}
                onClick={toggleDialog}
            >
                <svg xmlns="http://www.w3.org/2000/svg" width="45" height="45" viewBox="0 0 20 20">
                    <path
                        fill="blue"
                        d="M10 0c5.342 0 10 4.41 10 9.5c0 5.004-4.553 8.942-10 8.942a11 11 0 0 1-3.43-.546c-.464.45-.623.603-1.608 1.553c-.71.536-1.378.718-1.975.38c-.602-.34-.783-1.002-.66-1.874l.4-2.319C.99 14.002 0 11.842 0 9.5C0 4.41 4.657 0 10 0m0 1.4c-4.586 0-8.6 3.8-8.6 8.1c0 2.045.912 3.928 2.52 5.33l.02.017l.297.258l-.067.39l-.138.804l-.037.214l-.285 1.658a3 3 0 0 0-.03.337v.095q0 .007-.002.008c.007-.01.143-.053.376-.223l2.17-2.106l.414.156a9.6 9.6 0 0 0 3.362.605c4.716 0 8.6-3.36 8.6-7.543c0-4.299-4.014-8.1-8.6-8.1M5.227 7.813a1.5 1.5 0 1 1 0 2.998a1.5 1.5 0 0 1 0-2.998m4.998 0a1.5 1.5 0 1 1 0 2.998a1.5 1.5 0 0 1 0-2.998m4.997 0a1.5 1.5 0 1 1 0 2.998a1.5 1.5 0 0 1 0-2.998"
                    />
                </svg>
            </button>

            <div className="pagination second">
                <button onClick={handlePreviousPage} disabled={pageIndex === 1}>
                    Previous Page
                </button>
                <span>
                    Page {pageIndex} of {totalPages}
                </span>
                <button onClick={handleNextPage} disabled={pageIndex === totalPages}>
                    Next Page
                </button>
            </div>

            {isDialogOpen && (
                <div className="dialog-overlay" onClick={toggleDialog}>
                    <div
                        className="dialog-overlay__container"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <h2 className="dialog-overlay__title">
                            {parentCommentId ? 'Reply to Comment' : 'Add a Comment'}
                        </h2>
                        <div className="comment-actions">
                            <button className="comment-actions__action" onClick={insertAnchorTag}>
                                <svg xmlns="http://www.w3.org/2000/svg" width="22.5" height="22.5" viewBox="0 0 24 24"><path fill="blue" d="M11 17H7q-2.075 0-3.537-1.463T2 12t1.463-3.537T7 7h4v2H7q-1.25 0-2.125.875T4 12t.875 2.125T7 15h4zm-3-4v-2h8v2zm5 4v-2h4q1.25 0 2.125-.875T20 12t-.875-2.125T17 9h-4V7h4q2.075 0 3.538 1.463T22 12t-1.463 3.538T17 17z" /></svg>
                            </button>
                            <button className="comment-actions__action" onClick={insertCodeTag}>
                                <svg xmlns="http://www.w3.org/2000/svg" width="22.5" height="22.5" viewBox="0 0 24 24"><g fill="none" stroke="blue" strokeLinecap="round" strokeWidth="2"><path strokeLinejoin="round" d="m7 8l-4 4l4 4" /><path d="m10.5 18l3-12" /><path strokeLinejoin="round" d="m17 8l4 4l-4 4" /></g></svg>
                            </button>
                            <button className="comment-actions__action" onClick={insertItalicTag}>
                                <svg xmlns="http://www.w3.org/2000/svg" width="22.5" height="22.5" viewBox="0 0 24 24"><path fill="none" stroke="blue" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M11 5h6M7 19h6m1-14l-4 14" /></svg>
                            </button>
                            <button className="comment-actions__action" onClick={insertStrongTag}>
                                <svg xmlns="http://www.w3.org/2000/svg" width="22.5" height="22.5" viewBox="0 0 1024 1024"><path fill="blue" d="M697.8 481.4c33.6-35 54.2-82.3 54.2-134.3v-10.2C752 229.3 663.9 142 555.3 142H259.4c-15.1 0-27.4 12.3-27.4 27.4v679.1c0 16.3 13.2 29.5 29.5 29.5h318.7c117 0 211.8-94.2 211.8-210.5v-11c0-73-37.4-137.3-94.2-175.1M328 238h224.7c57.1 0 103.3 44.4 103.3 99.3v9.5c0 54.8-46.3 99.3-103.3 99.3H328zm366.6 429.4c0 62.9-51.7 113.9-115.5 113.9H328V542.7h251.1c63.8 0 115.5 51 115.5 113.9z" /></svg>
                            </button>
                            <button
                                className="comment-actions__action"
                                onClick={() => document.getElementById('file-input').click()}
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" width="22.5" height="22.5" viewBox="0 0 24 24">
                                    <path
                                        fill="none"
                                        stroke="blue"
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        strokeWidth="2"
                                        d="M6 7.91V16a6 6 0 0 0 6 6v0a6 6 0 0 0 6-6V6a4 4 0 0 0-4-4v0a4 4 0 0 0-4 4v9.182a2 2 0 0 0 2 2v0a2 2 0 0 0 2-2V8"
                                    />
                                </svg>
                            </button>
                            <input
                                type="file"
                                id="file-input"
                                accept=".jpg, .jpeg, .png, .gif, .txt"
                                style={{ display: 'none' }}
                                onChange={handleFileUpload}
                            />

                            {selectedFile && (
                                <div className="selected-file-preview">
                                    <p>{selectedFile.name}</p>
                                    <button onClick={handleRemoveFile} className="selected-file-preview__close-button">
                                        <svg
                                            xmlns="http://www.w3.org/2000/svg"
                                            width="22.5"
                                            height="22.5"
                                            fill="none"
                                            viewBox="0 0 22.5 22.5"
                                        >
                                            <defs>
                                                <clipPath id="clip2_1">
                                                    <path id="svg" fill="#fff" fillOpacity="0" d="M0 0h22.5v22.5H0z"></path>
                                                </clipPath>
                                            </defs>
                                            <path id="Frame 1" fill="#FFF" fillOpacity="0" d="M0 0h22.5v22.5H0z"></path>
                                            <rect
                                                id="Rectangle 1"
                                                width="19"
                                                height="18"
                                                x="2"
                                                y="2"
                                                fill="#FF6B6B"
                                                fillOpacity="1"
                                                rx="9"
                                            ></rect>
                                            <g clipPath="url(#clip2_1)">
                                                <path
                                                    id="path"
                                                    fill="red"
                                                    fillOpacity="1"
                                                    fillRule="nonzero"
                                                    d="M11.25 1.4c-5.49 0-9.85 4.36-9.85 9.85 0 5.48 4.36 9.84 9.85 9.84 5.48 0 9.84-4.36 9.84-9.84 0-5.49-4.36-9.85-9.84-9.85m3.79 14.77-3.79-3.8-3.8 3.8-1.13-1.13 3.8-3.79-3.8-3.8 1.13-1.13 3.8 3.8 3.79-3.8 1.13 1.13-3.8 3.8 3.8 3.79z"
                                                ></path>
                                            </g>
                                        </svg>
                                    </button>
                                </div>
                            )}
                        </div>
                        <textarea
                            ref={textareaRef}
                            className="dialog-overlay__textarea"
                            placeholder="Write your comment here..."
                            value={commentText}
                            onChange={handleCommentTextChange}
                        />
                        <p className="dialog-overlay__character-count">{commentText.length}/1000</p>
                        <div className="dialog-overlay__buttons">
                            <button
                                className="dialog-overlay__button submit"
                                onClick={handleSubmitComment}
                            >
                                Submit
                            </button>
                            <button className={`dialog-overlay__button preview ${isPreviewOpen ? 'preview-open' : ''}`} onClick={togglePreview}>
                                Preview
                            </button>
                            <button className="dialog-overlay__button close" onClick={toggleDialog}>
                                Close
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {isPreviewOpen && (
                <div className="preview-overlay" onClick={togglePreview}>
                    <div
                        className="preview-overlay__container"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <h2 className="preview-overlay__title">Preview</h2>
                        <div
                            className="preview-content"
                            dangerouslySetInnerHTML={{
                                __html: formatCommentText(commentText)
                            }}
                        />
                        <button
                            className="preview-overlay__button close"
                            onClick={(e) => {
                                e.stopPropagation();
                                togglePreview();
                            }}>
                            Close
                        </button>
                    </div>
                </div>
            )}
        </>
    );
}