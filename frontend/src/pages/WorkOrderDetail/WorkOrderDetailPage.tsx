import { useMemo, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { AxiosError } from 'axios'
import { Link, useParams } from 'react-router-dom'
import { Breadcrumbs } from '../../components/ui/Breadcrumbs'
import { StatusBadge } from '../../components/ui/StatusBadge'
import {
  completeOperation,
  releaseWorkOrder,
  startOperation,
} from '../../api/workOrders'
import { useWorkOrderOperations } from '../../features/work-orders/useWorkOrderOperations'
import { useWorkOrderReleaseReadiness } from '../../features/work-orders/useWorkOrderReleaseReadiness'
import { useWorkOrderSummary } from '../../features/work-orders/useWorkOrderSummary'
import type { WorkOrderOperation } from '../../types/workOrders'

function getReleaseState(status: string, isReleaseReady: boolean) {
  if (status === 'Planned') {
    return isReleaseReady
      ? { label: 'Ready', tone: 'ready' }
      : { label: 'Blocked', tone: 'blocked' }
  }

  if (status === 'Completed' || status === 'Closed') {
    return { label: 'Done', tone: 'completed' }
  }

  return { label: 'Not Applicable', tone: 'notapplicable' }
}

function formatQuantity(value: number) {
  return new Intl.NumberFormat(undefined, {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(value)
}

function formatDateTime(value: string | null) {
  if (!value) return 'Not yet'

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

function getErrorMessage(error: unknown) {
  if (error instanceof AxiosError) {
    return error.response?.data?.detail ?? error.message
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Unknown error'
}

function isCurrentOperation(operation: WorkOrderOperation, currentOperationId: string | undefined) {
  return operation.id === currentOperationId
}

function isNextOperation(operation: WorkOrderOperation, nextOperationId: string | undefined) {
  return operation.id === nextOperationId
}

export function WorkOrderDetailPage() {
  const { id } = useParams()
  const queryClient = useQueryClient()
  const [actionError, setActionError] = useState<string | null>(null)
  const [actionSuccess, setActionSuccess] = useState<string | null>(null)

  const summaryQuery = useWorkOrderSummary(id)
  const operationsQuery = useWorkOrderOperations(id)
  const releaseReadinessQuery = useWorkOrderReleaseReadiness(id)

  const refreshWorkOrder = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['work-order-summary', id] }),
      queryClient.invalidateQueries({ queryKey: ['work-order-operations', id] }),
      queryClient.invalidateQueries({ queryKey: ['work-order-release-readiness', id] }),
      queryClient.invalidateQueries({ queryKey: ['work-orders-summary'] }),
      queryClient.invalidateQueries({ queryKey: ['work-center-queue'] }),
    ])
  }

  const releaseMutation = useMutation({
    mutationFn: () => releaseWorkOrder(id!),
    onMutate: () => {
      setActionError(null)
      setActionSuccess(null)
    },
    onSuccess: async () => {
      await refreshWorkOrder()
      setActionSuccess('Work order released successfully.')
    },
    onError: (error) => setActionError(getErrorMessage(error)),
  })

  const startMutation = useMutation({
    mutationFn: (operationId: string) => startOperation(operationId),
    onMutate: () => {
      setActionError(null)
      setActionSuccess(null)
    },
    onSuccess: async () => {
      await refreshWorkOrder()
      setActionSuccess('Operation started successfully.')
    },
    onError: (error) => setActionError(getErrorMessage(error)),
  })

  const completeMutation = useMutation({
    mutationFn: (operationId: string) => completeOperation(operationId),
    onMutate: () => {
      setActionError(null)
      setActionSuccess(null)
    },
    onSuccess: async () => {
      await refreshWorkOrder()
      setActionSuccess('Operation completed successfully.')
    },
    onError: (error) => setActionError(getErrorMessage(error)),
  })

  const isLoading = summaryQuery.isLoading || operationsQuery.isLoading || releaseReadinessQuery.isLoading
  const isError = summaryQuery.isError || operationsQuery.isError || releaseReadinessQuery.isError

  const pageError = useMemo(() => {
    return summaryQuery.error ?? operationsQuery.error ?? releaseReadinessQuery.error
  }, [operationsQuery.error, releaseReadinessQuery.error, summaryQuery.error])

  const summary = summaryQuery.data
  const operations = operationsQuery.data ?? []
  const releaseReadiness = releaseReadinessQuery.data

  const currentOperationId = summary?.currentOperation?.id
  const nextOperationId = summary?.nextOperation?.id
  const releaseState = summary && releaseReadiness
    ? getReleaseState(summary.status, releaseReadiness.isReleaseReady)
    : null

  if (!id) {
    return (
      <main className="page-shell">
        <section className="panel panel-pad">
          <div className="error-box">No work order id was provided in the route.</div>
        </section>
      </main>
    )
  }

  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="page-head-stack">
          <Breadcrumbs
            items={[
              { label: 'Supervisor', to: '/supervisor' },
              { label: 'Work Orders', to: '/supervisor/work-orders' },
              { label: summary?.workOrderNumber ?? 'Detail' },
            ]}
          />
          <div className="brand-block">
            <span className="eyebrow">Supervisor Console</span>
            <h1 className="page-title">Work Order Detail</h1>
            <p className="page-subtitle">
              Inspect one work order, release it when ready, and move each operation through the route with confidence.
            </p>
          </div>
        </div>
        <Link className="text-link" to="/supervisor/work-orders">
          Back to board
        </Link>
      </header>

      {isLoading ? (
        <section className="panel">
          <div className="center-message">Loading work order state...</div>
        </section>
      ) : null}

      {isError ? (
        <section className="panel panel-pad">
          <div className="error-box">
            Unable to load this work order right now.
            <div className="muted">{getErrorMessage(pageError)}</div>
          </div>
        </section>
      ) : null}

      {!isLoading && !isError && summary && releaseReadiness ? (
        <section className="detail-shell">
          <section className="panel panel-pad">
            <div className="detail-header">
              <div>
                <span className="eyebrow">Work Order</span>
                <h2 className="detail-title">{summary.workOrderNumber}</h2>
                <p className="page-subtitle">
                  {summary.projectCode} / {summary.finishedGoodCode} / {summary.assemblyCode}
                </p>
              </div>

              <div className="badge-row">
                <StatusBadge status={summary.status} />
                <StatusBadge
                  label={`Material ${summary.isMaterialReady ? 'Ready' : 'Missing'}`}
                  tone={summary.isMaterialReady ? 'ready' : 'missing'}
                />
                <StatusBadge
                  label={`Release ${releaseState!.label}`}
                  tone={releaseState!.tone}
                />
              </div>
            </div>

            <div className="detail-grid">
              <div className="detail-stat">
                <span>Planned Qty</span>
                <strong>{formatQuantity(summary.plannedQuantity)}</strong>
              </div>
              <div className="detail-stat">
                <span>Completed Qty</span>
                <strong>{formatQuantity(summary.completedQuantity)}</strong>
              </div>
              <div className="detail-stat">
                <span>Current Operation</span>
                <strong>
                  {summary.currentOperation?.operationCode ??
                    (summary.status === 'Completed' || summary.status === 'Closed'
                      ? 'Completed route'
                      : 'Waiting')}
                </strong>
              </div>
              <div className="detail-stat">
                <span>Next Operation</span>
                <strong>{summary.nextOperation?.operationCode ?? 'None'}</strong>
              </div>
              <div className="detail-stat">
                <span>Released At</span>
                <strong>{formatDateTime(summary.releasedAtUtc)}</strong>
              </div>
              <div className="detail-stat">
                <span>Closed At</span>
                <strong>{formatDateTime(summary.closedAtUtc)}</strong>
              </div>
            </div>
          </section>

          <section className="panel panel-pad">
            <div className="action-card">
              <div>
                <span className="eyebrow">Release Control</span>
                <h3 className="section-title">Move this work order into production</h3>
                <p className="page-subtitle">
                  Release only when routing exists, material definition is ready, and the work order is still planned.
                </p>
              </div>

              {summary.status === 'Planned' ? (
                <button
                  type="button"
                  className="action-button"
                  disabled={!releaseReadiness.isReleaseReady || releaseMutation.isPending}
                  onClick={() => releaseMutation.mutate()}
                >
                  {releaseMutation.isPending ? 'Releasing...' : 'Release Work Order'}
                </button>
              ) : (
                <div className="info-box">
                  Release no longer applies once a work order has moved beyond the planned state.
                </div>
              )}
            </div>

            {summary.status === 'Planned' && releaseReadiness.blockingReasons.length > 0 ? (
              <div className="warning-box">
                <strong>Release is blocked</strong>
                <ul className="reason-list">
                  {releaseReadiness.blockingReasons.map((reason) => (
                    <li key={reason}>{reason}</li>
                  ))}
                </ul>
              </div>
            ) : null}

            {actionSuccess ? <div className="success-box">{actionSuccess}</div> : null}
            {actionError ? <div className="error-box">{actionError}</div> : null}
          </section>

          <section className="panel">
            <div className="panel-pad operations-head">
              <div>
                <span className="eyebrow">Operations</span>
                <h3 className="section-title">Run the route step by step</h3>
              </div>
              <div className="badge-row">
                <span className="badge badge--ready">{summary.totalOperations} Total</span>
                <span className="badge badge--completed">{summary.completedOperationsCount} Completed</span>
                <span className="badge badge--blocked">{summary.blockedOperationsCount} Blocked</span>
              </div>
            </div>

            <div className="table-wrap">
              <table className="board-table detail-table">
                <thead>
                  <tr>
                    <th>Operation #</th>
                    <th>Code</th>
                    <th>Name</th>
                    <th>Work Center</th>
                    <th>Status</th>
                    <th>Planned Qty</th>
                    <th>Completed Qty</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  {operations.map((operation) => {
                    const canStart = operation.status === 'Ready'
                    const canComplete = operation.status === 'InProgress'
                    const busy =
                      (startMutation.isPending && startMutation.variables === operation.id) ||
                      (completeMutation.isPending && completeMutation.variables === operation.id)

                    return (
                      <tr
                        key={operation.id}
                        className={[
                          isCurrentOperation(operation, currentOperationId) ? 'row-current' : '',
                          isNextOperation(operation, nextOperationId) ? 'row-next' : '',
                        ]
                          .filter(Boolean)
                          .join(' ')}
                      >
                        <td>{operation.operationNumber}</td>
                        <td>
                          <div className="primary-cell">
                            <strong>{operation.operationCode}</strong>
                            <span>{operation.isQcGate ? 'QC Gate' : 'Production Step'}</span>
                          </div>
                        </td>
                        <td>{operation.operationName}</td>
                        <td>{operation.workCenterCode}</td>
                        <td>
                          <StatusBadge status={operation.status} />
                        </td>
                        <td>{formatQuantity(operation.plannedQuantity)}</td>
                        <td>{formatQuantity(operation.completedQuantity)}</td>
                        <td>
                          {canStart ? (
                            <button
                              type="button"
                              className="table-action table-action--start"
                              disabled={busy}
                              onClick={() => startMutation.mutate(operation.id)}
                            >
                              {busy ? 'Working...' : 'Start'}
                            </button>
                          ) : null}

                          {canComplete ? (
                            <button
                              type="button"
                              className="table-action table-action--complete"
                              disabled={busy}
                              onClick={() => completeMutation.mutate(operation.id)}
                            >
                              {busy ? 'Working...' : 'Complete'}
                            </button>
                          ) : null}

                          {!canStart && !canComplete ? <span className="muted">No action</span> : null}
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          </section>
        </section>
      ) : null}
    </main>
  )
}
