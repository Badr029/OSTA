import axios from 'axios'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5053/api/v1',
  headers: {
    Accept: 'application/json',
  },
})
