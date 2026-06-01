import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { dashboardApi } from '../../api/servicios'
import { Users, CalendarDays, CalendarCheck, ChevronRight } from 'lucide-react'
import dayjs from 'dayjs'
import 'dayjs/locale/es'
dayjs.locale('es')

function TarjetaEstadistica({ Icono, etiqueta, valor, color }) {
  return (
    <div className="tarjeta flex items-center gap-4">
      <div className={`w-12 h-12 rounded-xl flex items-center justify-center shrink-0 ${color}`}>
        <Icono className="w-6 h-6 text-white" />
      </div>
      <div>
        <p className="text-3xl font-bold text-gray-900">{valor ?? '—'}</p>
        <p className="text-sm text-gray-500">{etiqueta}</p>
      </div>
    </div>
  )
}

const ETIQUETA_ESTADO = {
  programada: { texto: 'Programada', clase: 'badge-programada' },
  completada: { texto: 'Completada', clase: 'badge-completada' },
  cancelada:  { texto: 'Cancelada',  clase: 'badge-cancelada'  },
}

export default function PaginaDashboard() {
  const [resumen,  setResumen]  = useState(null)
  const [cargando, setCargando] = useState(true)

  useEffect(() => {
    dashboardApi.obtenerResumen()
      .then(({ data }) => setResumen(data))
      .finally(() => setCargando(false))
  }, [])

  const fechaHoy = dayjs().format('dddd, D [de] MMMM [de] YYYY')

  return (
    <div>
      {/* Encabezado */}
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-sm text-gray-500 capitalize mt-0.5">{fechaHoy}</p>
      </div>

      {/* Tarjetas de estadísticas */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
        <TarjetaEstadistica
          Icono={Users}
          etiqueta="Pacientes activos"
          valor={cargando ? '...' : resumen?.totalPacientes}
          color="bg-primario-600"
        />
        <TarjetaEstadistica
          Icono={CalendarDays}
          etiqueta="Citas próximas"
          valor={cargando ? '...' : resumen?.citasProximas}
          color="bg-blue-500"
        />
        <TarjetaEstadistica
          Icono={CalendarCheck}
          etiqueta="Citas hoy"
          valor={cargando ? '...' : resumen?.citasHoy?.length}
          color="bg-amber-500"
        />
      </div>

      {/* Lista de citas de hoy */}
      <div className="tarjeta">
        <div className="flex items-center justify-between mb-4">
          <h2 className="font-semibold text-gray-900">Citas de hoy</h2>
          <Link
            to="/citas"
            className="text-sm text-primario-600 hover:text-primario-700 flex items-center gap-1"
          >
            Ver agenda completa <ChevronRight className="w-3.5 h-3.5" />
          </Link>
        </div>

        {cargando ? (
          <p className="text-sm text-gray-400">Cargando...</p>
        ) : resumen?.citasHoy?.length === 0 ? (
          <p className="text-sm text-gray-500 py-4 text-center">
            No hay citas programadas para hoy.
          </p>
        ) : (
          <ul className="divide-y divide-gray-100">
            {resumen?.citasHoy?.map((cita) => {
              const estado = ETIQUETA_ESTADO[cita.estado] ?? ETIQUETA_ESTADO.programada
              return (
                <li key={cita.id} className="flex items-center justify-between py-3">
                  <div>
                    <p className="text-sm font-medium text-gray-900">{cita.nombrePaciente}</p>
                    <p className="text-xs text-gray-500">
                      {dayjs(cita.fechaHora).format('HH:mm')} · {cita.duracionMinutos} min
                    </p>
                  </div>
                  <span className={estado.clase}>{estado.texto}</span>
                </li>
              )
            })}
          </ul>
        )}
      </div>
    </div>
  )
}