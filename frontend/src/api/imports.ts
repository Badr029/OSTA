import { apiClient } from './client'
import type {
  BomImportBatchSummary,
  BomImportBatchPreview,
  BomImportBatchResult,
  BomImportMode,
  BomImportTemplateSummary,
} from '../types/imports'

export async function getBomImportTemplates(): Promise<BomImportTemplateSummary[]> {
  const response = await apiClient.get<BomImportTemplateSummary[]>('/bom-import-templates')
  return response.data
}

export async function getBomImportBatches(): Promise<BomImportBatchSummary[]> {
  const response = await apiClient.get<BomImportBatchSummary[]>('/bom-import-batches')
  return response.data
}

export async function uploadBomPreview(input: {
  templateCode: string
  defaultValuesJson?: string
  file: File
}): Promise<BomImportBatchPreview> {
  const formData = new FormData()
  formData.append('templateCode', input.templateCode)

  if (input.defaultValuesJson?.trim()) {
    formData.append('defaultValuesJson', input.defaultValuesJson.trim())
  }

  formData.append('file', input.file)

  const response = await apiClient.post<BomImportBatchPreview>('/bom-import-batches/upload/preview', formData)
  return response.data
}

export async function uploadBomCommit(input: {
  templateCode: string
  importMode: BomImportMode
  defaultValuesJson?: string
  file: File
}): Promise<BomImportBatchResult> {
  const formData = new FormData()
  formData.append('templateCode', input.templateCode)
  formData.append('importMode', input.importMode)

  if (input.defaultValuesJson?.trim()) {
    formData.append('defaultValuesJson', input.defaultValuesJson.trim())
  }

  formData.append('file', input.file)

  const response = await apiClient.post<BomImportBatchResult>('/bom-import-batches/upload', formData)
  return response.data
}

export async function getBomImportBatch(id: string): Promise<BomImportBatchResult> {
  const response = await apiClient.get<BomImportBatchResult>(`/bom-import-batches/${id}`)
  return response.data
}
