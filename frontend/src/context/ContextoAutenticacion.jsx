import { createContext, useContext, useState, useEffect } from 'react'

const ContextoAuth = createContext(null)

export function ProveedorAutenticacion({ children }) {
  const [usuario,  setUsuario]  = useState(null)
  const [token,    setToken]    = useState(() => localStorage.getItem('ng_token'))
  const [cargando, setCargando] = useState(true)

  useEffect(() => {
    if (token) {
      try {
        // Decodificar el payload del JWT para leer nombre y correo
        const payload    = JSON.parse(atob(token.split('.')[1]))
        const expirado   = payload.exp * 1000 < Date.now()

        if (expirado) {
          cerrarSesion()
        } else {
          setUsuario({
            nombre: payload.unique_name || payload.name,
            correo: payload.email
          })
        }
      } catch {
        cerrarSesion()
      }
    }
    setCargando(false)
  }, [token])

  const iniciarSesion = (datos) => {
    localStorage.setItem('ng_token', datos.token)
    setToken(datos.token)
    setUsuario({ nombre: datos.nombre, correo: datos.correo })
  }

  const cerrarSesion = () => {
    localStorage.removeItem('ng_token')
    setToken(null)
    setUsuario(null)
  }

  return (
    <ContextoAuth.Provider value={{ usuario, token, iniciarSesion, cerrarSesion, cargando }}>
      {children}
    </ContextoAuth.Provider>
  )
}

// Hook para usar el contexto en cualquier componente
export const useAuth = () => useContext(ContextoAuth)