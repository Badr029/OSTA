export interface ItemMasterSummary {
  id: string
  code: string
  name: string
  description: string
  itemType: string
  procurementType: string
  baseUom: string
  revision: string
  isActive: boolean
}

export interface ProjectSummary {
  id: string
  code: string
  name: string
}

export interface FinishedGoodSummary {
  id: string
  code: string
  name: string
  projectId: string
  sourceItemMasterId: string | null
  sourceBomHeaderId: string | null
}

export interface AssemblySummary {
  id: string
  code: string
  name: string
  finishedGoodId: string
  sourceBomItemId: string | null
  sourceComponentItemMasterId: string | null
}

export interface ItemMaterialRequirement {
  id: string
  itemMasterId: string
  materialCode: string
  requiredQuantity: number
  uom: string
  thicknessMm: number | null
  lengthMm: number | null
  widthMm: number | null
  weightKg: number | null
  notes: string | null
}

export interface AssemblyMaterialReadinessRequirement {
  id: string
  materialCode: string
  requiredQuantity: number
  uom: string
  thicknessMm: number | null
  lengthMm: number | null
  widthMm: number | null
  weightKg: number | null
  notes: string | null
}

export interface AssemblyMaterialReadiness {
  assemblyId: string
  assemblyCode: string
  sourceComponentItemMasterId: string | null
  sourceComponentItemCode: string | null
  isMaterialReady: boolean
  materialRequirementCount: number
  missingReasons: string[]
  requirements: AssemblyMaterialReadinessRequirement[]
}

export interface CreateItemMaterialRequirementInput {
  materialCode: string
  requiredQuantity: number
  uom: string
  thicknessMm?: number | null
  lengthMm?: number | null
  widthMm?: number | null
  weightKg?: number | null
  notes?: string | null
}

export type UpdateItemMaterialRequirementInput = CreateItemMaterialRequirementInput
