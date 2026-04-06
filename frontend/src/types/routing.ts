export interface RoutingTemplateSummary {
  id: string
  itemMasterId: string
  code: string
  name: string
  revision: string
  status: string
  isActive: boolean
}

export interface RoutingOperation {
  id: string
  routingTemplateId: string
  operationNumber: string
  operationCode: string
  operationName: string
  workCenterId: string
  workCenterCode: string
  workCenterName: string
  setupTimeMinutes: number
  runTimeMinutes: number
  sequence: number
  isQcGate: boolean
}

export interface CreateRoutingTemplateInput {
  itemMasterId: string
  code: string
  name: string
  revision: string
  status: 'Draft' | 'Active' | 'Obsolete'
  isActive: boolean
}

export interface CreateRoutingOperationInput {
  operationNumber: string
  operationCode: string
  operationName: string
  workCenterId: string
  setupTimeMinutes: number
  runTimeMinutes: number
  sequence: number
  isQcGate: boolean
}

export type UpdateRoutingOperationInput = CreateRoutingOperationInput
