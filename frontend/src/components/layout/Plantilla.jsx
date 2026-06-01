import { Outlet, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/ContextoAutenticacion'
import { LayoutDashboard, Users, CalendarDays, LogOut, Leaf } from 'lucide-react'

const navegacion = [
  { ruta: '/',          etiqueta: 'Dashboard', Icono: LayoutDashboard, exacta: true },
  { ruta: '/pacientes', etiqueta: 'Pacientes', Icono: Users },
  { ruta: '/citas',     etiqueta: 'Citas',     Icono: CalendarDays },
]

export default function Plantilla() {
  const { usuario, cerrarSesion } = useAuth()
  const navegar = useNavigate()

  const handleCerrarSesion = () => {
    cerrarSesion()
    navegar('/login')
  }

  return (
    <div className="flex h-screen overflow-hidden">

      {/* ── Sidebar ─────────────────────────────────────────────────────────── */}
      <aside className="w-60 bg-white border-r border-gray-200 flex flex-col shrink-0">

        {/* Logo */}
        <div className="flex items-center gap-2.5 px-5 py-5 border-b border-gray-100">
          <div className="w-8 h-8 bg-primario-600 rounded-lg flex items-center justify-center">
            <Leaf className="w-4 h-4 text-white" />
          </div>
          <span className="font-semibold text-gray-900">NutriGestión</span>
        </div>

        {/* Menú de navegación */}
        <nav className="flex-1 px-3 py-4 space-y-1">
          {navegacion.map(({ ruta, etiqueta, Icono, exacta }) => (
            <NavLink
              key={ruta}
              to={ruta}
              end={exacta}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-primario-50 text-primario-700'
                    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                }`
              }
            >
              <Icono className="w-4 h-4 shrink-0" />
              {etiqueta}
            </NavLink>
          ))}
        </nav>

        {/* Info del usuario + cerrar sesión */}
        <div className="px-3 py-4 border-t border-gray-100">
          <div className="flex items-center gap-3 px-3 py-2 mb-1">
            <div className="w-8 h-8 bg-primario-100 rounded-full flex items-center justify-center shrink-0">
              <span className="text-primario-700 text-xs font-bold">
                {usuario?.nombre?.charAt(0).toUpperCase()}
              </span>
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-gray-900 truncate">{usuario?.nombre}</p>
              <p className="text-xs text-gray-500 truncate">{usuario?.correo}</p>
            </div>
          </div>

          <button
            onClick={handleCerrarSesion}
            className="flex items-center gap-3 w-full px-3 py-2 text-sm text-gray-600
                       hover:bg-gray-100 hover:text-gray-900 rounded-lg transition-colors"
          >
            <LogOut className="w-4 h-4" />
            Cerrar sesión
          </button>
        </div>
      </aside>

      {/* ── Contenido principal ──────────────────────────────────────────────── */}
      <main className="flex-1 overflow-y-auto bg-gray-50">
        <div className="max-w-6xl mx-auto px-6 py-6">
          <Outlet />
        </div>
      </main>

    </div>
  )
}