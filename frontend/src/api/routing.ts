import { apiClient } from './client'
import type {
  CreateRoutingOperationInput,
  CreateRoutingTemplateInput,
  RoutingOperation,
  RoutingTemplateSummary,
} from '../types/routing'

export async function getRoutingTemplates(): Promise<RoutingTemplateSummary[]> {
  const response = await apiClient.get<RoutingTemplateSummary[]>('/routing-templates')
  return response.data
}

export async function createRoutingTemplate(
  input: CreateRoutingTemplateInput,
): Promise<RoutingTemplateSummary> {
  const response = await apiClient.post<RoutingTemplateSummary>('/routing-templates', input)
  return response.data
}

export async function getRoutingOperations(routingTemplateId: string): Promise<RoutingOperation[]> {
  const response = await apiClient.get<RoutingOperation[]>(
    `/routing-templates/${routingTemplateId}/operations`,
  )
  return response.data
}

export async function createRoutingOperation(
  routingTemplateId: string,
  input: CreateRoutingOperationInput,
): Promise<RoutingOperation> {
  const response = await apiClient.post<RoutingOperation>(
    `/routing-templates/${routingTemplateId}/operations`,
    input,
  )
  return response.data
}
