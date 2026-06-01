import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { pacientesApi } from '../../api/servicios'
import { Search, UserPlus, ChevronRight } from 'lucide-react'
import dayjs from 'dayjs'
import ModalPaciente from './ModalPaciente'

export default function PaginaPacientes() {
  const [pacientes,   setPacientes]   = useState([])
  const [busqueda,    setBusqueda]    = useState('')
  const [cargando,    setCargando]    = useState(true)
  const [mostrarModal, setMostrarModal] = useState(false)

  const cargarPacientes = async (termino = '') => {
    setCargando(true)
    try {
      const { data } = await pacientesApi.obtenerTodos(termino)
      setPacientes(data)
    } finally {
      setCargando(false)
    }
  }

  // Búsqueda con debounce de 300ms para no hacer petición en cada tecla
  useEffect(() => {
    const temporizador = setTimeout(() => cargarPacientes(busqueda), 300)
    return () => clearTimeout(temporizador)
  }, [busqueda])

  return (
    <div>
      {/* Encabezado */}
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Pacientes</h1>
        <button
          className="btn-primario flex items-center gap-2"
          onClick={() => setMostrarModal(true)}
        >
          <UserPlus className="w-4 h-4" />
          Nuevo paciente
        </button>
      </div>

      {/* Buscador */}
      <div className="relative mb-4">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
        <input
          type="text"
          className="input pl-9"
          placeholder="Buscar por nombre, apellido o teléfono..."
          value={busqueda}
          onChange={(e) => setBusqueda(e.target.value)}
        />
      </div>

      {/* Tabla */}
      <div className="tarjeta p-0 overflow-hidden">
        {cargando ? (
          <div className="p-10 text-center text-sm text-gray-400">Cargando pacientes...</div>
        ) : pacientes.length === 0 ? (
          <div className="p-10 text-center text-sm text-gray-500">
            {busqueda ? 'No se encontraron pacientes con esa búsqueda.' : 'Aún no hay pacientes registrados.'}
          </div>
        ) : (
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b border-gray-100">
              <tr>
                <th className="text-left px-5 py-3 font-medium text-gray-600">Paciente</th>
                <th className="text-left px-5 py-3 font-medium text-gray-600 hidden sm:table-cell">Teléfono</th>
                <th className="text-left px-5 py-3 font-medium text-gray-600 hidden md:table-cell">Último peso</th>
                <th className="text-left px-5 py-3 font-medium text-gray-600 hidden md:table-cell">Registrado</th>
                <th className="px-5 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {pacientes.map((p) => (
                <tr key={p.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-5 py-3.5">
                    <div className="flex items-center gap-3">
                      <div className="w-9 h-9 bg-primario-100 rounded-full flex items-center justify-center shrink-0">
                        <span className="text-primario-700 text-xs font-bold">
                          {p.nombre?.[0]}{p.apellido?.[0]}
                        </span>
                      </div>
                      <span className="font-medium text-gray-900">
                        {p.nombre} {p.apellido}
                      </span>
                    </div>
                  </td>
                  <td className="px-5 py-3.5 text-gray-600 hidden sm:table-cell">
                    {p.telefono || '—'}
                  </td>
                  <td className="px-5 py-3.5 text-gray-600 hidden md:table-cell">
                    {p.ultimoPeso ? `${p.ultimoPeso} kg` : '—'}
                  </td>
                  <td className="px-5 py-3.5 text-gray-500 hidden md:table-cell">
                    {dayjs(p.creadoEn).format('DD/MM/YYYY')}
                  </td>
                  <td className="px-5 py-3.5 text-right">
                    <Link
                      to={`/pacientes/${p.id}`}
                      className="text-primario-600 hover:text-primario-700 inline-flex items-center gap-1 font-medium"
                    >
                      Ver <ChevronRight className="w-3.5 h-3.5" />
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Modal de nuevo paciente */}
      {mostrarModal && (
        <ModalPaciente
          onCerrar={() => setMostrarModal(false)}
          onGuardado={(nuevo) => {
            setPacientes((prev) => [nuevo, ...prev])
            setMostrarModal(false)
          }}
        />
      )}
    </div>
  )
}