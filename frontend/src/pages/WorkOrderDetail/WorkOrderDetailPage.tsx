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
      <header className="topbar topbar--page-action">
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
              Inspect the current execution state, decide whether release still applies, and move the route forward one operation at a time.
            </p>
          </div>
        </div>
        <div className="page-header-actions">
          {summary && releaseReadiness && summary.status === 'Planned' ? (
            <button
              type="button"
              className="action-button"
              disabled={!releaseReadiness.isReleaseReady || releaseMutation.isPending}
              onClick={() => releaseMutation.mutate()}
            >
              {releaseMutation.isPending ? 'Releasing...' : 'Release Work Order'}
            </button>
          ) : (
            <Link className="text-link text-link--button" to="/supervisor/work-orders">
              Back to Work Orders
            </Link>
          )}
        </div>
      </header>

      {isLoading ? (
        <section className="panel panel-pad panel--section">
          <div className="loading-box loading-box--compact">Loading work order state...</div>
        </section>
      ) : null}

      {isError ? (
        <section className="panel panel-pad panel--section">
          <div className="error-box">
            Unable to load this work order right now.
            <div className="muted">{getErrorMessage(pageError)}</div>
          </div>
        </section>
      ) : null}

      {!isLoading && !isError && summary && releaseReadiness ? (
        <section className="detail-shell detail-shell--tight">
          <section className="panel panel-pad panel--section">
            <div className="section-header-compact">
              <span className="eyebrow">Context</span>
              <h2 className="section-title">Work order and product context</h2>
            </div>

            <div className="detail-grid detail-grid--dense">
              <div className="detail-stat">
                <span>Work Order</span>
                <strong>{summary.workOrderNumber}</strong>
              </div>
              <div className="detail-stat">
                <span>Project</span>
                <strong>{summary.projectCode}</strong>
              </div>
              <div className="detail-stat">
                <span>Finished Good</span>
                <strong>{summary.finishedGoodCode}</strong>
              </div>
              <div className="detail-stat">
                <span>Assembly</span>
                <strong>{summary.assemblyCode}</strong>
              </div>
              <div className="detail-stat">
                <span>Planned Qty</span>
                <strong>{formatQuantity(summary.plannedQuantity)}</strong>
              </div>
              <div className="detail-stat">
                <span>Completed Qty</span>
                <strong>{formatQuantity(summary.completedQuantity)}</strong>
              </div>
            </div>

            <div className="status-strip">
              <div className="status-strip__item">
                <span>Status</span>
                <strong className="detail-badge">
                  <StatusBadge status={summary.status} />
                </strong>
              </div>
              <div className="status-strip__item">
                <span>Material</span>
                <strong className="detail-badge">
                  <StatusBadge
                    label={summary.isMaterialReady ? 'Ready' : 'Missing'}
                    tone={summary.isMaterialReady ? 'ready' : 'missing'}
                  />
                </strong>
              </div>
              <div className="status-strip__item">
                <span>Release</span>
                <strong className="detail-badge">
                  <StatusBadge label={releaseState!.label} tone={releaseState!.tone} />
                </strong>
              </div>
              <div className="status-strip__item">
                <span>Current Operation</span>
                <strong>
                  {summary.currentOperation?.operationCode ??
                    (summary.status === 'Completed' || summary.status === 'Closed'
                      ? 'Completed route'
                      : 'Waiting')}
                </strong>
              </div>
              <div className="status-strip__item">
                <span>Next Operation</span>
                <strong>{summary.nextOperation?.operationCode ?? 'None'}</strong>
              </div>
              <div className="status-strip__item">
                <span>Released At</span>
                <strong>{formatDateTime(summary.releasedAtUtc)}</strong>
              </div>
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

            {summary.status !== 'Planned' ? (
              <div className="info-box info-box--full">
                Release is only relevant while the work order is still planned. This work order is already operating in its current lifecycle state.
              </div>
            ) : null}

            {actionSuccess ? <div className="success-box">{actionSuccess}</div> : null}
            {actionError ? <div className="error-box">{actionError}</div> : null}
          </section>

          <section className="panel panel--section">
            <div className="panel-pad section-head-row">
              <div className="section-header-compact">
                <span className="eyebrow">Operations</span>
                <h2 className="section-title">Execution route</h2>
              </div>
              <div className="badge-row badge-row--compact">
                <StatusBadge label={`${summary.totalOperations} Total`} tone="neutral" />
                <StatusBadge label={`${summary.completedOperationsCount} Completed`} tone="completed" />
                <StatusBadge label={`${summary.blockedOperationsCount} Blocked`} tone="blocked" />
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
