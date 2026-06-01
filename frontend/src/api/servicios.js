import cliente from './cliente'

// ── Autenticación ──────────────────────────────────────────────────────────────
export const autenticacionApi = {
  login:    (datos) => cliente.post('/autenticacion/login',    datos),
  registro: (datos) => cliente.post('/autenticacion/registro', datos),
}

// ── Pacientes ──────────────────────────────────────────────────────────────────
export const pacientesApi = {
  obtenerTodos:  (busqueda) => cliente.get('/pacientes', { params: busqueda ? { busqueda } : {} }),
  obtenerPorId:  (id)       => cliente.get(`/pacientes/${id}`),
  crear:         (datos)    => cliente.post('/pacientes', datos),
  actualizar:    (id, datos) => cliente.put(`/pacientes/${id}`, datos),
  eliminar:      (id)       => cliente.delete(`/pacientes/${id}`),
}

// ── Consultas ──────────────────────────────────────────────────────────────────
export const consultasApi = {
  obtenerPorPaciente: (pacienteId)        => cliente.get(`/pacientes/${pacienteId}/consultas`),
  obtenerPorId:       (pacienteId, id)    => cliente.get(`/pacientes/${pacienteId}/consultas/${id}`),
  crear:              (pacienteId, datos) => cliente.post(`/pacientes/${pacienteId}/consultas`, datos),
  actualizar:         (pacienteId, id, datos) => cliente.put(`/pacientes/${pacienteId}/consultas/${id}`, datos),
  eliminar:           (pacienteId, id)    => cliente.delete(`/pacientes/${pacienteId}/consultas/${id}`),
}

// ── Citas ──────────────────────────────────────────────────────────────────────
export const citasApi = {
  obtenerPorRango:    (desde, hasta) => cliente.get('/citas', { params: { desde, hasta } }),
  obtenerHoy:         ()             => cliente.get('/citas/hoy'),
  obtenerPorPaciente: (pacienteId)   => cliente.get(`/citas/paciente/${pacienteId}`),
  crear:              (datos)        => cliente.post('/citas', datos),
  actualizar:         (id, datos)    => cliente.put(`/citas/${id}`, datos),
  eliminar:           (id)           => cliente.delete(`/citas/${id}`),
}

// ── Archivos ───────────────────────────────────────────────────────────────────
export const archivosApi = {
  obtenerPorPaciente: (pacienteId) =>
    cliente.get(`/pacientes/${pacienteId}/archivos`),

  subir: (pacienteId, formData) =>
    cliente.post(`/pacientes/${pacienteId}/archivos`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    }),

  descargar: (pacienteId, archivoId) =>
    cliente.get(`/pacientes/${pacienteId}/archivos/${archivoId}/descargar`, {
      responseType: 'blob'
    }),

  eliminar: (pacienteId, archivoId) =>
    cliente.delete(`/pacientes/${pacienteId}/archivos/${archivoId}`),
}

// ── Dashboard ──────────────────────────────────────────────────────────────────
export const dashboardApi = {
  obtenerResumen: () => cliente.get('/dashboard/resumen'),
}