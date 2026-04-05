import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { StatusBadge } from '../../components/ui/StatusBadge'
import { formatStatusLabel } from '../../components/ui/statusBadgeUtils'
import { useWorkOrdersSummary } from '../../features/work-orders/useWorkOrdersSummary'
import type { WorkOrderStatus } from '../../types/workOrders'

const statusOptions: Array<{ label: string; value: '' | WorkOrderStatus }> = [
  { label: 'All statuses', value: '' },
  { label: 'Planned', value: 'Planned' },
  { label: 'Released', value: 'Released' },
  { label: 'In Progress', value: 'InProgress' },
  { label: 'Completed', value: 'Completed' },
  { label: 'Closed', value: 'Closed' },
  { label: 'On Hold', value: 'OnHold' },
]

function getReleaseState(status: WorkOrderStatus, isReleaseReady: boolean) {
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

function getCurrentOperationLabel(status: WorkOrderStatus, currentOperationCode: string | null) {
  if (currentOperationCode) {
    return currentOperationCode
  }

  if (status === 'Completed' || status === 'Closed') {
    return 'Completed route'
  }

  return 'Waiting'
}

export function WorkOrdersBoardPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState<'' | WorkOrderStatus>('')
  const [projectCode, setProjectCode] = useState('')

  const filters = useMemo(
    () => ({
      status: status || undefined,
      projectCode: projectCode.trim() || undefined,
    }),
    [projectCode, status],
  )

  const { data, isLoading, isError, error } = useWorkOrdersSummary(filters)

  const totals = useMemo(() => {
    const rows = data ?? []

    return {
      total: rows.length,
      active: rows.filter((row) => row.status === 'InProgress').length,
      releasable: rows.filter((row) => row.isReleaseReady).length,
      missingMaterial: rows.filter((row) => !row.isMaterialReady).length,
    }
  }, [data])

  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="brand-block">
          <span className="eyebrow">Supervisor Console</span>
          <h1 className="page-title">Work Orders Board</h1>
          <p className="page-subtitle">
            See what is running, what is blocked, and which work orders are ready for the next decision.
          </p>
        </div>
      </header>

      <section className="board-shell">
        <div className="panel panel-pad">
          <div className="filters">
            <div className="field">
              <label htmlFor="status-filter">Status</label>
              <select
                id="status-filter"
                value={status}
                onChange={(event) => setStatus(event.target.value as '' | WorkOrderStatus)}
              >
                {statusOptions.map((option) => (
                  <option key={option.label} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>

            <div className="field">
              <label htmlFor="project-filter">Project Code</label>
              <input
                id="project-filter"
                type="text"
                value={projectCode}
                onChange={(event) => setProjectCode(event.target.value)}
                placeholder="Filter one project quickly"
              />
            </div>
          </div>
        </div>

        <div className="summary-strip">
          <div className="stat-chip">
            <strong>{totals.total}</strong>
            <span>Total work orders</span>
          </div>
          <div className="stat-chip">
            <strong>{totals.active}</strong>
            <span>In progress now</span>
          </div>
          <div className="stat-chip">
            <strong>{totals.releasable}</strong>
            <span>Ready to release</span>
          </div>
          <div className="stat-chip">
            <strong>{totals.missingMaterial}</strong>
            <span>Missing material definition</span>
          </div>
        </div>

        <section className="panel">
          <div className="table-wrap">
            {isLoading ? (
              <div className="center-message">Loading live work orders...</div>
            ) : null}

            {isError ? (
              <div className="panel-pad">
                <div className="error-box">
                  Unable to load the work orders board right now.
                  <div className="muted">{error instanceof Error ? error.message : 'Unknown error'}</div>
                </div>
              </div>
            ) : null}

            {!isLoading && !isError && (data?.length ?? 0) === 0 ? (
              <div className="center-message">
                No work orders match these filters yet.
              </div>
            ) : null}

            {!isLoading && !isError && (data?.length ?? 0) > 0 ? (
              <table className="board-table">
                <thead>
                  <tr>
                    <th>WO Number</th>
                    <th>Project</th>
                    <th>Assembly</th>
                    <th>Status</th>
                    <th>Material Ready</th>
                    <th>Release Ready</th>
                    <th>Current Operation</th>
                    <th>Next Operation</th>
                  </tr>
                </thead>
                <tbody>
                  {data?.map((workOrder) => {
                    const releaseState = getReleaseState(workOrder.status, workOrder.isReleaseReady)

                    return (
                      <tr
                        key={workOrder.workOrderId}
                        className="row-clickable"
                        onClick={() => navigate(`/supervisor/work-orders/${workOrder.workOrderId}`)}
                      >
                        <td>
                          <div className="primary-cell">
                            <strong>{workOrder.workOrderNumber}</strong>
                            <span>{workOrder.finishedGoodCode}</span>
                          </div>
                        </td>
                        <td>
                          <div className="primary-cell">
                            <strong>{workOrder.projectCode}</strong>
                            <span>
                              {workOrder.completedQuantity} / {workOrder.plannedQuantity}
                            </span>
                          </div>
                        </td>
                        <td>
                          <div className="primary-cell">
                            <strong>{workOrder.assemblyCode}</strong>
                            <span>{workOrder.releasedAtUtc ? 'Released' : 'Not released yet'}</span>
                          </div>
                        </td>
                        <td>
                          <StatusBadge status={workOrder.status} />
                        </td>
                        <td>
                          <StatusBadge
                            label={workOrder.isMaterialReady ? 'Ready' : 'Missing'}
                            tone={workOrder.isMaterialReady ? 'ready' : 'missing'}
                          />
                        </td>
                        <td>
                          <StatusBadge label={releaseState.label} tone={releaseState.tone} />
                        </td>
                        <td>
                          <div className="operation-stack">
                            <strong>{getCurrentOperationLabel(workOrder.status, workOrder.currentOperationCode)}</strong>
                            <span>{formatStatusLabel(workOrder.currentOperationStatus)}</span>
                          </div>
                        </td>
                        <td>
                          <div className="operation-stack">
                            <strong>{workOrder.nextOperationCode ?? 'No next step'}</strong>
                            <span>{workOrder.nextOperationCode ? 'Queued next' : 'End of route'}</span>
                          </div>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            ) : null}
          </div>
        </section>
      </section>
    </main>
  )
}
