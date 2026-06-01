import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { pacientesApi, consultasApi, archivosApi } from '../../api/servicios'
import { ArrowLeft, Pencil, Trash2, Plus, Upload, Download, FileText, ImageIcon } from 'lucide-react'
import dayjs from 'dayjs'
import ModalPaciente from './ModalPaciente'

const PESTANAS = ['Consultas', 'Archivos']

export default function PaginaDetallePaciente() {
  const { id } = useParams()
  const navegar = useNavigate()

  const [paciente,    setPaciente]    = useState(null)
  const [consultas,   setConsultas]   = useState([])
  const [archivos,    setArchivos]    = useState([])
  const [pestana,     setPestana]     = useState(0)
  const [editModal,   setEditModal]   = useState(false)
  const [subiendo,    setSubiendo]    = useState(false)

  useEffect(() => {
    pacientesApi.obtenerPorId(id).then(({ data }) => setPaciente(data))
    consultasApi.obtenerPorPaciente(id).then(({ data }) => setConsultas(data))
    archivosApi.obtenerPorPaciente(id).then(({ data }) => setArchivos(data))
  }, [id])

  const manejarEliminar = async () => {
    if (!confirm('¿Estás seguro de eliminar este paciente? El registro se desactivará.')) return
    await pacientesApi.eliminar(id)
    navegar('/pacientes')
  }

  const manejarSubirArchivo = async (e) => {
    const archivo = e.target.files[0]
    if (!archivo) return
    setSubiendo(true)

    const formData = new FormData()
    formData.append('archivo', archivo)
    formData.append('tipoArchivo', archivo.type.startsWith('image/') ? 'imagen' : 'documento')

    try {
      const { data } = await archivosApi.subir(id, formData)
      setArchivos((prev) => [data, ...prev])
    } finally {
      setSubiendo(false)
      e.target.value = ''
    }
  }

  const manejarDescargar = async (archivo) => {
    const { data } = await archivosApi.descargar(id, archivo.id)
    const url = URL.createObjectURL(new Blob([data], { type: archivo.tipoContenido }))
    const enlace = document.createElement('a')
    enlace.href = url
    enlace.download = archivo.nombreOriginal
    enlace.click()
    URL.revokeObjectURL(url)
  }

  if (!paciente) {
    return <div className="text-sm text-gray-400 p-6">Cargando paciente...</div>
  }

  const edad = paciente.fechaNacimiento
    ? dayjs().diff(dayjs(paciente.fechaNacimiento), 'year')
    : null

  return (
    <div>
      {/* Encabezado */}
      <div className="flex items-center gap-3 mb-6">
        <button
          onClick={() => navegar('/pacientes')}
          className="text-gray-500 hover:text-gray-700 transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
        </button>
        <h1 className="text-xl font-bold text-gray-900 flex-1">
          {paciente.nombre} {paciente.apellido}
        </h1>
        <button
          onClick={() => setEditModal(true)}
          className="btn-secundario flex items-center gap-2 text-sm"
        >
          <Pencil className="w-3.5 h-3.5" /> Editar
        </button>
        <button onClick={manejarEliminar} className="btn-peligro">
          <Trash2 className="w-3.5 h-3.5" /> Eliminar
        </button>
      </div>

      {/* Tarjeta de información */}
      <div className="tarjeta mb-6">
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-sm">
          {[
            ['Teléfono',    paciente.telefono || '—'],
            ['Correo',      paciente.correo   || '—'],
            ['Edad',        edad !== null ? `${edad} años` : '—'],
            ['Último peso', paciente.ultimoPeso ? `${paciente.ultimoPeso} kg` : '—'],
          ].map(([etiq, val]) => (
            <div key={etiq}>
              <p className="text-xs text-gray-500 mb-0.5">{etiq}</p>
              <p className="font-medium text-gray-900">{val}</p>
            </div>
          ))}
        </div>
        {paciente.notas && (
          <p className="mt-4 text-sm text-gray-600 border-t border-gray-100 pt-3">
            {paciente.notas}
          </p>
        )}
      </div>

      {/* Pestañas */}
      <div className="flex gap-6 border-b border-gray-200 mb-5">
        {PESTANAS.map((p, i) => (
          <button
            key={p}
            onClick={() => setPestana(i)}
            className={`pb-2.5 text-sm font-medium border-b-2 transition-colors ${
              pestana === i
                ? 'text-primario-600 border-primario-600'
                : 'text-gray-500 border-transparent hover:text-gray-700'
            }`}
          >
            {p}
          </button>
        ))}
      </div>

      {/* ── Pestaña: Consultas ─────────────────────────────────────────────── */}
      {pestana === 0 && (
        <div>
          <div className="flex justify-end mb-3">
            <button className="btn-primario flex items-center gap-2 text-sm">
              <Plus className="w-3.5 h-3.5" /> Nueva consulta
            </button>
          </div>

          {consultas.length === 0 ? (
            <p className="text-sm text-gray-500 text-center py-10">
              Este paciente no tiene consultas registradas.
            </p>
          ) : (
            <div className="space-y-3">
              {consultas.map((c) => (
                <div key={c.id} className="tarjeta">
                  <div className="flex items-start justify-between mb-2">
                    <p className="font-medium text-gray-900">
                      {dayjs(c.fecha).format('DD [de] MMMM [de] YYYY')}
                    </p>
                    <div className="flex gap-4 text-xs text-gray-500">
                      {c.peso  && <span>Peso: <strong>{c.peso} kg</strong></span>}
                      {c.talla && <span>Talla: <strong>{c.talla} cm</strong></span>}
                      {c.imc   && <span>IMC: <strong>{c.imc}</strong></span>}
                    </div>
                  </div>
                  {c.observaciones && (
                    <p className="text-sm text-gray-600">{c.observaciones}</p>
                  )}
                  {c.recomendaciones && (
                    <p className="text-sm text-gray-500 mt-1 italic">
                      Recomendaciones: {c.recomendaciones}
                    </p>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* ── Pestaña: Archivos ──────────────────────────────────────────────── */}
      {pestana === 1 && (
        <div>
          <div className="flex justify-end mb-3">
            <label className="btn-primario flex items-center gap-2 text-sm cursor-pointer">
              <Upload className="w-3.5 h-3.5" />
              {subiendo ? 'Subiendo...' : 'Subir archivo'}
              <input
                type="file"
                className="hidden"
                accept=".pdf,image/*"
                onChange={manejarSubirArchivo}
              />
            </label>
          </div>

          {archivos.length === 0 ? (
            <p className="text-sm text-gray-500 text-center py-10">
              No hay archivos cargados para este paciente.
            </p>
          ) : (
            <div className="space-y-2">
              {archivos.map((a) => (
                <div key={a.id} className="tarjeta flex items-center justify-between py-3">
                  <div className="flex items-center gap-3">
                    {a.tipoContenido?.startsWith('image/')
                      ? <ImageIcon className="w-5 h-5 text-blue-500 shrink-0" />
                      : <FileText  className="w-5 h-5 text-red-500 shrink-0" />
                    }
                    <div>
                      <p className="text-sm font-medium text-gray-900">{a.nombreOriginal}</p>
                      <p className="text-xs text-gray-500">
                        {dayjs(a.subidoEn).format('DD/MM/YYYY')} · {(a.tamanoBytes / 1024).toFixed(0)} KB
                      </p>
                    </div>
                  </div>
                  <button
                    onClick={() => manejarDescargar(a)}
                    className="text-primario-600 hover:text-primario-700 transition-colors"
                    title="Descargar archivo"
                  >
                    <Download className="w-4 h-4" />
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Modal de edición */}
      {editModal && (
        <ModalPaciente
          paciente={paciente}
          onCerrar={() => setEditModal(false)}
          onGuardado={(actualizado) => {
            setPaciente(actualizado)
            setEditModal(false)
          }}
        />
      )}
    </div>
  )
}