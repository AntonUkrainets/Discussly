import { ApolloClient, InMemoryCache, createHttpLink, ApolloLink } from '@apollo/client';

const httpLink = createHttpLink({
    uri: `${import.meta.env.VITE_REACT_APP_BASE_URL}/graphql`,
});

const authLink = new ApolloLink((operation, forward) => {
    const token = localStorage.getItem('authToken');

    operation.setContext(({ headers = {} }) => ({
        headers: {
            ...headers,
            Authorization: token ? `Bearer ${token}` : '',
        },
    }));

    return forward(operation);
});

const client = new ApolloClient({
    link: authLink.concat(httpLink),
    cache: new InMemoryCache(),
});

export default client;