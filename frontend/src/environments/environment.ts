export const environment = {
  production: false,
  // Local dev: http://localhost:5000/api
  // Production (Railway): set RAILWAY_API_URL below after deployment
  apiUrl: typeof window !== 'undefined' && window.location.hostname === 'localhost'
    ? 'http://localhost:5000/api'
    : 'https://YOUR-RAILWAY-APP.railway.app/api'
};
