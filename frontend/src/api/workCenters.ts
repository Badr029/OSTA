import { apiClient } from './client'
import type { WorkCenter, WorkCenterQueueItem } from '../types/workCenters'

export async function getWorkCenters(): Promise<WorkCenter[]> {
  const response = await apiClient.get<WorkCenter[]>('/work-centers')
  return response.data
}

export async function getWorkCenterQueue(id: string): Promise<WorkCenterQueueItem[]> {
  const response = await apiClient.get<WorkCenterQueueItem[]>(`/work-centers/${id}/queue`)
  return response.data
}
