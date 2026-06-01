import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/ContextoAutenticacion'
import { autenticacionApi } from '../../api/servicios'
import { Leaf } from 'lucide-react'

export default function PaginaLogin() {
  const [formulario, setFormulario] = useState({ correo: '', contrasena: '' })
  const [error,      setError]      = useState('')
  const [cargando,   setCargando]   = useState(false)

  const { iniciarSesion } = useAuth()
  const navegar = useNavigate()

  const manejarCambio = (campo) => (e) =>
    setFormulario({ ...formulario, [campo]: e.target.value })

  const manejarEnvio = async (e) => {
    e.preventDefault()
    setError('')
    setCargando(true)

    try {
      const { data } = await autenticacionApi.login(formulario)
      iniciarSesion(data)
      navegar('/')
    } catch (err) {
      setError(err.response?.data?.mensaje || 'Error al iniciar sesión. Intenta de nuevo.')
    } finally {
      setCargando(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
      <div className="w-full max-w-sm">

        {/* Logo */}
        <div className="flex flex-col items-center mb-8">
          <div className="w-14 h-14 bg-primario-600 rounded-2xl flex items-center justify-center mb-3 shadow-lg">
            <Leaf className="w-7 h-7 text-white" />
          </div>
          <h1 className="text-2xl font-bold text-gray-900">NutriGestión</h1>
          <p className="text-sm text-gray-500 mt-1">Sistema de gestión nutricional</p>
        </div>

        {/* Formulario */}
        <div className="tarjeta">
          <h2 className="text-base font-semibold text-gray-900 mb-5">Iniciar sesión</h2>

          <form onSubmit={manejarEnvio} className="space-y-4">
            <div>
              <label className="etiqueta">Correo electrónico</label>
              <input
                type="email"
                className="input"
                placeholder="nutriologo@correo.com"
                value={formulario.correo}
                onChange={manejarCambio('correo')}
                required
              />
            </div>

            <div>
              <label className="etiqueta">Contraseña</label>
              <input
                type="password"
                className="input"
                placeholder="••••••••"
                value={formulario.contrasena}
                onChange={manejarCambio('contrasena')}
                required
              />
            </div>

            {error && (
              <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
                {error}
              </div>
            )}

            <button type="submit" className="btn-primario w-full" disabled={cargando}>
              {cargando ? 'Ingresando...' : 'Iniciar sesión'}
            </button>
          </form>
        </div>

      </div>
    </div>
  )
}