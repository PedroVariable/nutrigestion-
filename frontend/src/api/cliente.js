import axios from 'axios'

// En desarrollo usa el proxy de Vite (/api → localhost:5000 — ver vite.config.js).
// En producción, define VITE_API_URL en el build (ej: https://tu-api.onrender.com/api).
const baseURL = import.meta.env.VITE_API_URL || '/api'

const cliente = axios.create({
  baseURL,
  headers: { 'Content-Type': 'application/json' }
})

// Adjunta el token JWT en cada petición automáticamente
cliente.interceptors.request.use((config) => {
  const token = localStorage.getItem('ng_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Si el servidor responde 401 (token expirado), redirige al login
cliente.interceptors.response.use(
  (respuesta) => respuesta,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('ng_token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default cliente