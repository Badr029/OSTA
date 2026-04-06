import { apiClient } from './client'
import type {
  AssemblyMaterialReadiness,
  AssemblySummary,
  CreateItemMaterialRequirementInput,
  FinishedGoodSummary,
  ItemMasterSummary,
  ItemMaterialRequirement,
  ProjectSummary,
  UpdateItemMaterialRequirementInput,
} from '../types/materialRequirements'

export async function getProjects(): Promise<ProjectSummary[]> {
  const response = await apiClient.get<ProjectSummary[]>('/projects')
  return response.data
}

export async function getProjectFinishedGoods(projectId: string): Promise<FinishedGoodSummary[]> {
  const response = await apiClient.get<FinishedGoodSummary[]>(`/projects/${projectId}/finishedgoods`)
  return response.data
}

export async function getFinishedGoodAssemblies(finishedGoodId: string): Promise<AssemblySummary[]> {
  const response = await apiClient.get<AssemblySummary[]>(`/finishedgoods/${finishedGoodId}/assemblies`)
  return response.data
}

export async function getAssemblyMaterialReadiness(assemblyId: string): Promise<AssemblyMaterialReadiness> {
  const response = await apiClient.get<AssemblyMaterialReadiness>(`/assemblies/${assemblyId}/material-readiness`)
  return response.data
}

export async function getItemMasters(): Promise<ItemMasterSummary[]> {
  const response = await apiClient.get<ItemMasterSummary[]>('/item-masters')
  return response.data
}

export async function getItemMaterialRequirements(itemMasterId: string): Promise<ItemMaterialRequirement[]> {
  const response = await apiClient.get<ItemMaterialRequirement[]>(
    `/item-masters/${itemMasterId}/material-requirements`,
  )
  return response.data
}

export async function createItemMaterialRequirement(
  itemMasterId: string,
  input: CreateItemMaterialRequirementInput,
): Promise<ItemMaterialRequirement> {
  const response = await apiClient.post<ItemMaterialRequirement>(
    `/item-masters/${itemMasterId}/material-requirements`,
    input,
  )
  return response.data
}

export async function updateItemMaterialRequirement(
  itemMasterId: string,
  requirementId: string,
  input: UpdateItemMaterialRequirementInput,
): Promise<ItemMaterialRequirement> {
  const response = await apiClient.put<ItemMaterialRequirement>(
    `/item-masters/${itemMasterId}/material-requirements/${requirementId}`,
    input,
  )
  return response.data
}

export async function deleteItemMaterialRequirement(
  itemMasterId: string,
  requirementId: string,
): Promise<void> {
  await apiClient.delete(`/item-masters/${itemMasterId}/material-requirements/${requirementId}`)
}
