export type WorkOrderStatus =
  | 'Planned'
  | 'Released'
  | 'InProgress'
  | 'Completed'
  | 'Closed'
  | 'OnHold'

export type OperationStatus =
  | 'Planned'
  | 'Ready'
  | 'InProgress'
  | 'Completed'
  | 'Blocked'
  | 'QcHold'

export interface WorkOrderSummaryListItem {
  workOrderId: string
  workOrderNumber: string
  status: WorkOrderStatus
  projectCode: string
  finishedGoodCode: string
  assemblyCode: string
  plannedQuantity: number
  completedQuantity: number
  isReleaseReady: boolean
  isMaterialReady: boolean
  currentOperationCode: string | null
  currentOperationStatus: string | null
  nextOperationCode: string | null
  releasedAtUtc: string | null
}

export interface WorkOrderOperationSummary {
  id: string
  operationNumber: string
  operationCode: string
  operationName: string
  workCenterCode: string
  status: OperationStatus
  sequence: number
  isQcGate: boolean
}

export interface WorkOrderSummary {
  workOrderId: string
  workOrderNumber: string
  status: WorkOrderStatus
  projectCode: string
  finishedGoodCode: string
  assemblyCode: string
  plannedQuantity: number
  completedQuantity: number
  isReleaseReady: boolean
  isMaterialReady: boolean
  totalOperations: number
  completedOperationsCount: number
  blockedOperationsCount: number
  inProgressOperationsCount: number
  currentOperation: WorkOrderOperationSummary | null
  nextOperation: WorkOrderOperationSummary | null
  releasedAtUtc: string | null
  closedAtUtc: string | null
}

export interface WorkOrderOperation {
  id: string
  workOrderId: string
  routingOperationId: string | null
  operationNumber: string
  operationCode: string
  operationName: string
  workCenterId: string
  workCenterCode: string
  status: OperationStatus
  plannedQuantity: number
  completedQuantity: number
  startedAtUtc: string | null
  completedAtUtc: string | null
  sequence: number
  isQcGate: boolean
}

export interface WorkOrderReleaseReadiness {
  workOrderId: string
  workOrderNumber: string
  workOrderStatus: WorkOrderStatus
  assemblyId: string
  assemblyCode: string
  hasOperations: boolean
  operationCount: number
  isMaterialReady: boolean
  isReleaseReady: boolean
  blockingReasons: string[]
}

export interface GenerateWorkOrderInput {
  projectId: string
  finishedGoodId: string
  assemblyId: string
  plannedQuantity?: number
}

export interface WorkOrdersSummaryFilters {
  status?: string
  projectCode?: string
  assemblyCode?: string
  isMaterialReady?: boolean
  isReleaseReady?: boolean
}
