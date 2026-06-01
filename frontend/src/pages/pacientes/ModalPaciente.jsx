import { useState } from 'react'
import { pacientesApi } from '../../api/servicios'
import { X } from 'lucide-react'

const formularioVacio = {
  nombre: '', apellido: '', correo: '', telefono: '',
  fechaNacimiento: '', genero: '', direccion: '', notas: ''
}

export default function ModalPaciente({ paciente, onCerrar, onGuardado }) {
  const [formulario, setFormulario] = useState(paciente ?? formularioVacio)
  const [error,      setError]      = useState('')
  const [cargando,   setCargando]   = useState(false)
  const esEdicion = !!paciente?.id

  const manejarCambio = (campo) => (e) =>
    setFormulario({ ...formulario, [campo]: e.target.value })

  const manejarEnvio = async (e) => {
    e.preventDefault()
    setError('')
    setCargando(true)

    try {
      const { data } = esEdicion
        ? await pacientesApi.actualizar(paciente.id, formulario)
        : await pacientesApi.crear(formulario)

      onGuardado(data ?? { ...formulario, id: paciente?.id })
    } catch (err) {
      setError(err.response?.data?.mensaje || 'Error al guardar el paciente.')
    } finally {
      setCargando(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-xl w-full max-w-lg shadow-xl">

        {/* Encabezado */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100">
          <h2 className="font-semibold text-gray-900">
            {esEdicion ? 'Editar paciente' : 'Nuevo paciente'}
          </h2>
          <button onClick={onCerrar} className="text-gray-400 hover:text-gray-600 transition-colors">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Formulario */}
        <form onSubmit={manejarEnvio} className="p-5 space-y-4">

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="etiqueta">Nombre *</label>
              <input className="input" value={formulario.nombre}
                onChange={manejarCambio('nombre')} required />
            </div>
            <div>
              <label className="etiqueta">Apellido *</label>
              <input className="input" value={formulario.apellido}
                onChange={manejarCambio('apellido')} required />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="etiqueta">Teléfono</label>
              <input className="input" type="tel" value={formulario.telefono}
                onChange={manejarCambio('telefono')} />
            </div>
            <div>
              <label className="etiqueta">Género</label>
              <select className="input" value={formulario.genero}
                onChange={manejarCambio('genero')}>
                <option value="">Sin especificar</option>
                <option value="F">Femenino</option>
                <option value="M">Masculino</option>
                <option value="Otro">Otro</option>
              </select>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="etiqueta">Correo electrónico</label>
              <input className="input" type="email" value={formulario.correo}
                onChange={manejarCambio('correo')} />
            </div>
            <div>
              <label className="etiqueta">Fecha de nacimiento</label>
              <input className="input" type="date"
                value={formulario.fechaNacimiento?.slice(0, 10)}
                onChange={manejarCambio('fechaNacimiento')} />
            </div>
          </div>

          <div>
            <label className="etiqueta">Dirección</label>
            <input className="input" value={formulario.direccion}
              onChange={manejarCambio('direccion')} />
          </div>

          <div>
            <label className="etiqueta">Notas</label>
            <textarea className="input resize-none" rows={3}
              value={formulario.notas} onChange={manejarCambio('notas')} />
          </div>

          {error && (
            <p className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">
              {error}
            </p>
          )}

          <div className="flex justify-end gap-2 pt-1">
            <button type="button" className="btn-secundario" onClick={onCerrar}>
              Cancelar
            </button>
            <button type="submit" className="btn-primario" disabled={cargando}>
              {cargando ? 'Guardando...' : esEdicion ? 'Guardar cambios' : 'Crear paciente'}
            </button>
          </div>

        </form>
      </div>
    </div>
  )
}