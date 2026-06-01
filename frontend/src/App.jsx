import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { ProveedorAutenticacion, useAuth } from './context/ContextoAutenticacion'
import Plantilla            from './components/layout/Plantilla'
import PaginaLogin          from './pages/auth/PaginaLogin'
import PaginaDashboard      from './pages/dashboard/PaginaDashboard'
import PaginaPacientes      from './pages/pacientes/PaginaPacientes'
import PaginaDetallePaciente from './pages/pacientes/PaginaDetallePaciente'
import PaginaCitas          from './pages/citas/PaginaCitas'

// Protege rutas — si no hay sesión, redirige al login
function RutaPrivada({ children }) {
  const { usuario, cargando } = useAuth()

  if (cargando) {
    return (
      <div className="flex items-center justify-center h-screen text-gray-400">
        Cargando...
      </div>
    )
  }

  return usuario ? children : <Navigate to="/login" replace />
}

function Rutas() {
  const { usuario } = useAuth()

  return (
    <Routes>
      {/* Login — si ya hay sesión redirige al inicio */}
      <Route
        path="/login"
        element={usuario ? <Navigate to="/" replace /> : <PaginaLogin />}
      />

      {/* Rutas privadas dentro del layout con sidebar */}
      <Route
        path="/"
        element={
          <RutaPrivada>
            <Plantilla />
          </RutaPrivada>
        }
      >
        <Route index                    element={<PaginaDashboard />} />
        <Route path="pacientes"         element={<PaginaPacientes />} />
        <Route path="pacientes/:id"     element={<PaginaDetallePaciente />} />
        <Route path="citas"             element={<PaginaCitas />} />
      </Route>

      {/* Cualquier ruta desconocida redirige al inicio */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default function App() {
  return (
    <BrowserRouter>
      <ProveedorAutenticacion>
        <Rutas />
      </ProveedorAutenticacion>
    </BrowserRouter>
  )
}