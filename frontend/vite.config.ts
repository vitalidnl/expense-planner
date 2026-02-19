import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/transactions': 'http://localhost:5147',
      '/recurrence-rules': 'http://localhost:5147',
      '/recurring-transactions': 'http://localhost:5147',
      '/forecast': 'http://localhost:5147',
      '/reset': 'http://localhost:5147',
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: './src/test/setupTests.ts',
  },
})
