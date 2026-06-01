import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'


export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Todas las llamadas a /api se redirigen al backend .NET
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      }
    }
  }
})