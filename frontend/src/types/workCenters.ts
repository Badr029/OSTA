export interface WorkCenter {
  id: string
  code: string
  name: string
  department: string
  hourlyRate: number
  isActive: boolean
}

export interface WorkCenterInput {
  code: string
  name: string
  department: string
  hourlyRate: number
  isActive: boolean
}

export interface WorkCenterQueueItem {
  workOrderId: string
  workOrderNumber: string
  workOrderStatus: string
  projectCode: string
  finishedGoodCode: string
  assemblyCode: string
  operationId: string
  operationNumber: string
  operationCode: string
  operationName: string
  operationStatus: string
  plannedQuantity: number
  completedQuantity: number
  releasedAtUtc: string | null
  startedAtUtc: string | null
  isQcGate: boolean
  sequence: number
}
