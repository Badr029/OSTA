import { useMemo, useState } from 'react'
import { useQueries } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { StatusBadge } from '../../components/ui/StatusBadge'
import { formatStatusLabel } from '../../components/ui/statusBadgeUtils'
import { getWorkCenterQueue } from '../../api/workCenters'
import { useWorkCenterQueue } from '../../features/work-centers/useWorkCenterQueue'
import { useWorkCenters } from '../../features/work-centers/useWorkCenters'
import type { WorkCenterQueueItem } from '../../types/workCenters'

function formatQuantity(value: number) {
  return new Intl.NumberFormat(undefined, {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(value)
}

function formatStartedAt(value: string | null) {
  if (!value) {
    return 'Not started'
  }

  return new Intl.DateTimeFormat(undefined, {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  }).format(new Date(value))
}

function getQueueStatusTone(status: string) {
  if (status === 'InProgress') {
    return 'inprogress'
  }

  if (status === 'Ready') {
    return 'ready'
  }

  return status
}

function renderQueueRowSecondaryText(item: WorkCenterQueueItem) {
  if (item.operationStatus === 'InProgress') {
    return item.startedAtUtc ? `Started ${formatStartedAt(item.startedAtUtc)}` : 'Running now'
  }

  return item.workOrderStatus === 'Released' ? 'Ready to start' : item.workOrderStatus
}

export function WorkCenterQueuePage() {
  const navigate = useNavigate()
  const [userSelectedWorkCenterId, setUserSelectedWorkCenterId] = useState<string>()

  const workCentersQuery = useWorkCenters()
  const workCenters = useMemo(() => workCentersQuery.data ?? [], [workCentersQuery.data])

  const selectedWorkCenterId = useMemo(() => {
    if (
      userSelectedWorkCenterId &&
      workCenters.some((workCenter) => workCenter.id === userSelectedWorkCenterId)
    ) {
      return userSelectedWorkCenterId
    }

    const laser = workCenters.find((workCenter) => workCenter.code === 'LASER')
    return laser?.id ?? workCenters[0]?.id
  }, [userSelectedWorkCenterId, workCenters])

  const selectedWorkCenter = useMemo(
    () => workCenters.find((workCenter) => workCenter.id === selectedWorkCenterId),
    [selectedWorkCenterId, workCenters],
  )

  const queueQuery = useWorkCenterQueue(selectedWorkCenterId)
  const queue = queueQuery.data ?? []
  const queueCountQueries = useQueries({
    queries: workCenters.map((workCenter) => ({
      queryKey: ['work-center-queue', workCenter.id],
      queryFn: () => getWorkCenterQueue(workCenter.id),
      staleTime: 15_000,
    })),
  })

  const queueCountsByWorkCenterId = useMemo(() => {
    return workCenters.reduce<Record<string, number>>((accumulator, workCenter, index) => {
      const queryResult = queueCountQueries[index]
      accumulator[workCenter.id] = Array.isArray(queryResult?.data) ? queryResult.data.length : 0
      return accumulator
    }, {})
  }, [queueCountQueries, workCenters])

  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="brand-block">
          <span className="eyebrow">Supervisor Console</span>
          <h1 className="page-title">Work Center Queue</h1>
          <p className="page-subtitle">
            Monitor what is running and what is ready at each station, then jump straight into the work order that needs attention.
          </p>
        </div>
      </header>

      <section className="board-shell">
        <section className="panel panel-pad">
          {workCentersQuery.isLoading ? (
            <div className="center-message">Loading work centers...</div>
          ) : null}

          {workCentersQuery.isError ? (
            <div className="error-box">
              Unable to load work centers right now.
              <div className="muted">
                {workCentersQuery.error instanceof Error ? workCentersQuery.error.message : 'Unknown error'}
              </div>
            </div>
          ) : null}

          {!workCentersQuery.isLoading && !workCentersQuery.isError && workCenters.length > 0 ? (
            <div className="tab-row" role="tablist" aria-label="Work center selector">
              {workCenters.map((workCenter) => (
                <button
                  key={workCenter.id}
                  type="button"
                  className={`tab-chip ${workCenter.id === selectedWorkCenterId ? 'tab-chip--active' : ''}`}
                  onClick={() => setUserSelectedWorkCenterId(workCenter.id)}
                >
                  <strong>
                    {workCenter.code} ({queueCountsByWorkCenterId[workCenter.id] ?? 0})
                  </strong>
                  <span>{workCenter.name}</span>
                </button>
              ))}
            </div>
          ) : null}
        </section>

        <section className="panel">
          <div className="panel-pad queue-head">
            <div>
              <span className="eyebrow">Selected Work Center</span>
              <h2 className="section-title">{selectedWorkCenter?.code ?? 'Choose a station'}</h2>
              <p className="page-subtitle">
                {selectedWorkCenter
                  ? `${selectedWorkCenter.name} / ${selectedWorkCenter.department}`
                  : 'Select a work center to inspect the live queue.'}
              </p>
            </div>
          </div>

          <div className="table-wrap">
            {selectedWorkCenterId && queueQuery.isLoading ? (
              <div className="center-message">Loading live queue...</div>
            ) : null}

            {selectedWorkCenterId && queueQuery.isError ? (
              <div className="panel-pad">
                <div className="error-box">
                  Unable to load this work center queue right now.
                  <div className="muted">
                    {queueQuery.error instanceof Error ? queueQuery.error.message : 'Unknown error'}
                  </div>
                </div>
              </div>
            ) : null}

            {!selectedWorkCenterId && !workCentersQuery.isLoading ? (
              <div className="center-message">No work center is selected yet.</div>
            ) : null}

            {selectedWorkCenterId && !queueQuery.isLoading && !queueQuery.isError && queue.length === 0 ? (
              <div className="center-message">No active queue for this work center.</div>
            ) : null}

            {selectedWorkCenterId && !queueQuery.isLoading && !queueQuery.isError && queue.length > 0 ? (
              <table className="board-table queue-table">
                <thead>
                  <tr>
                    <th>WO Number</th>
                    <th>Project</th>
                    <th>Assembly</th>
                    <th>Operation</th>
                    <th>Status</th>
                    <th>Planned Qty</th>
                    <th>Completed Qty</th>
                    <th>Started At</th>
                  </tr>
                </thead>
                <tbody>
                  {queue.map((item) => (
                    <tr
                      key={item.operationId}
                      className="row-clickable row-clickable--queue"
                      onClick={() => navigate(`/supervisor/work-orders/${item.workOrderId}`)}
                    >
                      <td>
                        <div className="primary-cell">
                          <strong>{item.workOrderNumber}</strong>
                          <span>{item.finishedGoodCode}</span>
                        </div>
                      </td>
                      <td>
                        <div className="primary-cell">
                          <strong>{item.projectCode}</strong>
                          <span>{formatStatusLabel(item.workOrderStatus)}</span>
                        </div>
                      </td>
                      <td>
                        <div className="primary-cell">
                          <strong>{item.assemblyCode}</strong>
                          <span>{item.operationNumber}</span>
                        </div>
                      </td>
                      <td>
                        <div className="operation-stack">
                          <strong>{item.operationCode}</strong>
                          <span>{item.operationName}</span>
                        </div>
                      </td>
                      <td>
                        <StatusBadge
                          status={item.operationStatus}
                          tone={getQueueStatusTone(item.operationStatus)}
                        />
                      </td>
                      <td>{formatQuantity(item.plannedQuantity)}</td>
                      <td>{formatQuantity(item.completedQuantity)}</td>
                      <td>
                        <div className="operation-stack">
                          <strong>{formatStartedAt(item.startedAtUtc)}</strong>
                          <span>{renderQueueRowSecondaryText(item)}</span>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : null}
          </div>
        </section>
      </section>
    </main>
  )
}
