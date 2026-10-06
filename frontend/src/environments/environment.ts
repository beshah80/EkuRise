export const environment = {
  production: false,
  apiUrl: typeof window !== 'undefined' && window.location.hostname === 'localhost'
    ? 'http://localhost:5000/api'
    : 'https://ekubapi-production.up.railway.app/api'
};
