import { gql } from '@apollo/client';

export const DEFAULT_AVATAR_URL = "https://discussly.blob.core.windows.net/attachments/anonym.png";

export const GET_LATEST_COMMENTS = gql`
   query GetLatestComments($request: GetCommentsRequestInput!) {
    latestComments(request: $request) {
      comments {
        id
        text
        userName
        userAvatar
        createdAt
        parentCommentId
        rootCommentId
        attachmentUrl
        attachmentFileName
        ...ChildCommentsFragment @defer
      }
      totalComments
      currentPage
    }
  }

  fragment ChildCommentsFragment on Comment {
    childComments {
      id
      text
      userName
      userAvatar
      createdAt
      parentCommentId
      rootCommentId
      attachmentUrl
      attachmentFileName
      childComments {
        id
        text
        userName
        userAvatar
        createdAt
        parentCommentId
        rootCommentId
        attachmentUrl
        attachmentFileName
        childComments {
          id
          text
          userName
          userAvatar
          createdAt
          parentCommentId
          rootCommentId
          attachmentUrl
          attachmentFileName
        }
      }
    }
  }
`;

export const GET_COMMENT_BY_ID = gql`
  query GetCommentById($request: GetCommentByIdRequestInput!) {
    commentById(request: $request) {
      id
      text
      userName
      userAvatar
      createdAt
      parentCommentId
      rootCommentId
      attachmentUrl
      attachmentFileName
    }
  }
`;