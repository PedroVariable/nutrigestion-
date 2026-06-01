import { useEffect, useState } from 'react'
import { citasApi, pacientesApi } from '../../api/servicios'
import { Plus, ChevronLeft, ChevronRight, X, Clock, Calendar as CalendarIcon } from 'lucide-react'
import dayjs from 'dayjs'
import 'dayjs/locale/es'

const formularioVacio = {
  pacienteId: '',
  fecha: dayjs().format('YYYY-MM-DD'), // Fecha por defecto: hoy
  hora: '09:00',                        // Hora por defecto
  duracionMinutos: 45,
  notas: ''
}

export default function PaginaCitas() {
  const [citas, setCitas] = useState([])
  const [semana, setSemana] = useState(0)
  const [mostrarModal, setMostrarModal] = useState(false)
  const [pacientes, setPacientes] = useState([])
  const [formulario, setFormulario] = useState(formularioVacio)
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  const inicioSemana = dayjs().startOf('week').add(semana, 'week')
  const finSemana = inicioSemana.endOf('week')
  const dias = Array.from({ length: 7 }, (_, i) => inicioSemana.add(i, 'day'))

  useEffect(() => {
    citasApi.obtenerPorRango(inicioSemana.toISOString(), finSemana.toISOString())
      .then(({ data }) => setCitas(data))
  }, [semana])

  const abrirModal = () => {
    pacientesApi.obtenerTodos().then(({ data }) => setPacientes(data))
    setError('')
    setFormulario(formularioVacio)
    setMostrarModal(true)
  }

  const manejarCrear = async (e) => {
    e.preventDefault()
    setError('')

    // Combinar fecha y hora para el backend
    const fechaHoraISO = dayjs(`${formulario.fecha}T${formulario.hora}`).toISOString()

    if (dayjs(fechaHoraISO).isBefore(dayjs())) {
      setError('No puedes programar una cita en el pasado.')
      return
    }

    setGuardando(true)
    const paciente = pacientes.find((p) => p.id === formulario.pacienteId)

    try {
      const { data } = await citasApi.crear({
        ...formulario,
        nombrePaciente: paciente ? `${paciente.nombre} ${paciente.apellido}` : 'Paciente desconocido',
        fechaHora: fechaHoraISO,
        estado: 'programada',
      })
      setCitas((prev) => [...prev, data])
      setMostrarModal(false)
    } catch (err) {
      setError(err.response?.data?.mensaje || 'Error al conectar con el servidor.')
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className="fade-in">
      {/* Encabezado */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 m-0">Agenda</h1>
          <p className="text-gray-500">Gestiona las consultas de la semana</p>
        </div>
        <button className="btn-primario flex items-center gap-2" onClick={abrirModal}>
          <Plus className="w-5 h-5" /> Nueva Cita
        </button>
      </div>

      {/* Navegador de Semana */}
      <div className="tarjeta mb-6 flex items-center justify-between bg-white/50 backdrop-blur-sm">
        <button onClick={() => setSemana(s => s - 1)} className="btn-secundario p-2">
          <ChevronLeft className="w-5 h-5" />
        </button>
        <div className="text-center">
          <span className="text-lg font-bold text-gray-800 capitalize">
            {inicioSemana.format('MMMM YYYY')}
          </span>
          <p className="text-xs text-gray-500 font-medium tracking-widest uppercase">
            {inicioSemana.format('DD [al] ')} {finSemana.format('DD')}
          </p>
        </div>
        <button onClick={() => setSemana(s => s + 1)} className="btn-secundario p-2">
          <ChevronRight className="w-5 h-5" />
        </button>
      </div>

      {/* Grid del Calendario */}
      <div className="grid grid-cols-1 md:grid-cols-7 gap-4">
        {dias.map((dia) => {
          const citasDia = citas.filter(c => dayjs(c.fechaHora).isSame(dia, 'day'))
          const esHoy = dia.isSame(dayjs(), 'day')

          return (
            <div key={dia.toString()} className={`tarjeta p-0 overflow-hidden border-t-4 ${esHoy ? 'border-purple-500' : 'border-transparent'}`}>
              <div className={`p-2 text-center border-b ${esHoy ? 'bg-purple-50' : 'bg-gray-50'}`}>
                <p className="text-[10px] uppercase tracking-tighter font-bold text-gray-400">{dia.format('ddd')}</p>
                <p className={`text-xl font-black ${esHoy ? 'text-purple-600' : 'text-gray-700'}`}>{dia.format('D')}</p>
              </div>
              
              <div className="p-2 space-y-2 min-h-[200px]">
                {citasDia.length === 0 ? (
                  <p className="text-[10px] text-gray-300 text-center mt-4 italic">Sin citas</p>
                ) : (
                  citasDia.map((cita) => (
                    <div key={cita.id} className="bg-white border-l-4 border-purple-400 shadow-sm rounded-r-lg p-2 transition-transform hover:scale-105 cursor-pointer">
                      <p className="text-[11px] font-bold text-gray-800 truncate">{cita.nombrePaciente}</p>
                      <div className="flex items-center gap-1 text-[10px] text-purple-600 font-medium">
                        <Clock className="w-3 h-3" /> {dayjs(cita.fechaHora).format('HH:mm')}
                      </div>
                    </div>
                  ))
                )}
              </div>
            </div>
          )
        })}
      </div>

      {/* Modal - Mejorado para selección de Hora */}
      {mostrarModal && (
        <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl w-full max-w-md shadow-2xl fade-in">
            <div className="flex items-center justify-between p-6 border-b">
              <h2 className="text-xl font-bold text-gray-800">Agendar Paciente</h2>
              <button onClick={() => setMostrarModal(false)} className="text-gray-400 hover:text-gray-600">
                <X className="w-6 h-6" />
              </button>
            </div>

            <form onSubmit={manejarCrear} className="p-6 space-y-5">
              <div>
                <label className="etiqueta">Seleccionar Paciente</label>
                <select 
                  className="input" 
                  value={formulario.pacienteId}
                  onChange={e => setFormulario({...formulario, pacienteId: e.target.value})}
                  required
                >
                  <option value="">Buscar en la lista...</option>
                  {pacientes.map(p => <option key={p.id} value={p.id}>{p.nombre} {p.apellido}</option>)}
                </select>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="etiqueta flex items-center gap-1"><CalendarIcon className="w-3 h-3"/> Fecha</label>
                  <input 
                    type="date" 
                    className="input" 
                    value={formulario.fecha}
                    onChange={e => setFormulario({...formulario, fecha: e.target.value})}
                    required 
                  />
                </div>
                <div>
                  <label className="etiqueta flex items-center gap-1"><Clock className="w-3 h-3"/> Hora</label>
                  <input 
                    type="time" 
                    className="input" 
                    value={formulario.hora}
                    onChange={e => setFormulario({...formulario, hora: e.target.value})}
                    required 
                  />
                </div>
              </div>

              <div>
                <label className="etiqueta">Duración estimada</label>
                <div className="grid grid-cols-3 gap-2">
                  {[30, 45, 60].map(min => (
                    <button
                      key={min}
                      type="button"
                      onClick={() => setFormulario({...formulario, duracionMinutos: min})}
                      className={`py-2 text-xs rounded-lg font-bold transition-all ${
                        formulario.duracionMinutos === min 
                        ? 'bg-purple-600 text-white' 
                        : 'bg-gray-100 text-gray-500 hover:bg-gray-200'
                      }`}
                    >
                      {min} min
                    </button>
                  ))}
                </div>
              </div>

              {error && <div className="p-3 bg-red-50 text-red-600 text-xs rounded-xl border border-red-100">{error}</div>}

              <div className="flex gap-3 pt-2">
                <button type="button" onClick={() => setMostrarModal(false)} className="btn-secundario flex-1">Cancelar</button>
                <button type="submit" disabled={guardando} className="btn-primario flex-1">
                  {guardando ? 'Procesando...' : 'Confirmar Cita'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}