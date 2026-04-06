import { apiClient } from './client'
import type { WorkCenter, WorkCenterInput, WorkCenterQueueItem } from '../types/workCenters'

export async function getWorkCenters(): Promise<WorkCenter[]> {
  const response = await apiClient.get<WorkCenter[]>('/work-centers')
  return response.data
}

export async function getWorkCenterQueue(id: string): Promise<WorkCenterQueueItem[]> {
  const response = await apiClient.get<WorkCenterQueueItem[]>(`/work-centers/${id}/queue`)
  return response.data
}

export async function createWorkCenter(payload: WorkCenterInput): Promise<WorkCenter> {
  const response = await apiClient.post<WorkCenter>('/work-centers', payload)
  return response.data
}

export async function updateWorkCenter(id: string, payload: WorkCenterInput): Promise<WorkCenter> {
  const response = await apiClient.put<WorkCenter>(`/work-centers/${id}`, payload)
  return response.data
}
