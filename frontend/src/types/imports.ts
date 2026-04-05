export type BomImportMode = 'ExecutionOnly' | 'ExecutionAndProductDefinition'

export interface BomImportBatchSummary {
  id: string
  sourceFileName: string
  importedAtUtc: string
  status: string
  totalRows: number
  successfulRows: number
  failedRows: number
}

export interface BomImportTemplateSummary {
  id: string
  code: string
  name: string
  formatType: string
  structureType: string
  headerRowIndex: number
  dataStartRowIndex: number
  isActive: boolean
}

export interface BomImportLinePreview {
  rowNumber: number
  projectCode: string
  projectName: string
  finishedGoodCode: string
  finishedGoodName: string
  assemblyCode: string
  assemblyName: string
  partNumber: string
  revision: string
  description: string
  quantity: number
  materialCode: string | null
  thicknessMm: number | null
  weightKg: number | null
  drawingNumber: string | null
  finishCode: string | null
  specification: string | null
  notes: string | null
  processRouteCode: string | null
  scrapPercent: number | null
  cutOnly: boolean | null
}

export interface BomImportBatchPreview {
  templateCode: string
  sourceFileName: string
  importMode: string
  totalRows: number
  lines: BomImportLinePreview[]
}

export interface BomImportBatchLineResult {
  id: string
  rowNumber: number
  projectCode: string
  projectName: string
  finishedGoodCode: string
  finishedGoodName: string
  assemblyCode: string
  assemblyName: string
  partNumber: string
  revision: string
  description: string
  quantity: number
  materialCode: string | null
  thicknessMm: number | null
  weightKg: number | null
  drawingNumber: string | null
  finishCode: string | null
  specification: string | null
  notes: string | null
  processRouteCode: string | null
  scrapPercent: number | null
  cutOnly: boolean | null
  status: string
  errorMessage: string | null
}

export interface BomImportBatchResult {
  id: string
  sourceFileName: string
  importedAtUtc: string
  status: string
  totalRows: number
  successfulRows: number
  failedRows: number
  lines: BomImportBatchLineResult[]
}
