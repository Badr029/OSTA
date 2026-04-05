import { apiClient } from './client'
import type {
  GenerateWorkOrderInput,
  WorkOrderOperation,
  WorkOrderReleaseReadiness,
  WorkOrdersSummaryFilters,
  WorkOrderSummary,
  WorkOrderSummaryListItem,
} from '../types/workOrders'

export async function getWorkOrdersSummary(
  filters: WorkOrdersSummaryFilters = {},
): Promise<WorkOrderSummaryListItem[]> {
  const response = await apiClient.get<WorkOrderSummaryListItem[]>('/work-orders/summary', {
    params: filters,
  })

  return response.data
}

export async function getWorkOrderSummary(id: string): Promise<WorkOrderSummary> {
  const response = await apiClient.get<WorkOrderSummary>(`/work-orders/${id}/summary`)
  return response.data
}

export async function getWorkOrderOperations(id: string): Promise<WorkOrderOperation[]> {
  const response = await apiClient.get<WorkOrderOperation[]>(`/work-orders/${id}/operations`)
  return response.data
}

export async function getReleaseReadiness(id: string): Promise<WorkOrderReleaseReadiness> {
  const response = await apiClient.get<WorkOrderReleaseReadiness>(`/work-orders/${id}/release-readiness`)
  return response.data
}

export async function releaseWorkOrder(id: string) {
  const response = await apiClient.post(`/work-orders/${id}/release`)
  return response.data
}

export async function generateWorkOrder(input: GenerateWorkOrderInput) {
  const response = await apiClient.post('/work-orders/generate', input)
  return response.data
}

export async function startOperation(id: string) {
  const response = await apiClient.post(`/work-order-operations/${id}/start`)
  return response.data
}

export async function completeOperation(id: string) {
  const response = await apiClient.post(`/work-order-operations/${id}/complete`)
  return response.data
}
