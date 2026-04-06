import { useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import { Breadcrumbs } from '../../components/ui/Breadcrumbs'
import { StatusBadge } from '../../components/ui/StatusBadge'
import { useBomImportBatches } from '../../features/imports/useBomImportBatches'

function formatDateTime(value: string | null | undefined) {
  if (!value) {
    return 'Not available'
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function ImportHistoryPage() {
  const navigate = useNavigate()
  const batchesQuery = useBomImportBatches()

  const batches = useMemo(() => batchesQuery.data ?? [], [batchesQuery.data])

  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="page-head-stack">
          <Breadcrumbs
            items={[
              { label: 'Planning', to: '/planning' },
              { label: 'Imports', to: '/planning/imports' },
              { label: 'History' },
            ]}
          />
          <div className="brand-block">
            <span className="eyebrow">Planning Console</span>
            <h1 className="page-title">Import History</h1>
            <p className="page-subtitle">
              Review past BOM imports and inspect results whenever you need to reopen a batch.
            </p>
          </div>
        </div>
      </header>

      <section className="board-shell">
        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Batch List</span>
              <h2 className="section-title">Past imports</h2>
              <p className="page-subtitle">
                Open any batch to review row outcomes, diagnose failures, or confirm what was imported.
              </p>
            </div>
          </div>

          {batchesQuery.isLoading ? (
            <div className="loading-box">Loading import history...</div>
          ) : null}

          {batchesQuery.isError ? (
            <div className="error-box">
              Unable to load import history right now.
              <div className="muted">
                {batchesQuery.error instanceof Error ? batchesQuery.error.message : 'Unknown error'}
              </div>
            </div>
          ) : null}

          {!batchesQuery.isLoading && !batchesQuery.isError && batches.length === 0 ? (
            <div className="center-message">No imports found. Start by importing a BOM.</div>
          ) : null}

          {!batchesQuery.isLoading && !batchesQuery.isError && batches.length > 0 ? (
            <div className="table-wrap">
              <table className="board-table">
                <thead>
                  <tr>
                    <th>Batch ID</th>
                    <th>Source File</th>
                    <th>Imported At</th>
                    <th>Status</th>
                    <th>Total Rows</th>
                    <th>Success</th>
                    <th>Failed</th>
                  </tr>
                </thead>
                <tbody>
                  {batches.map((batch) => (
                    <tr
                      key={batch.id}
                      className="row-clickable"
                      onClick={() => navigate(`/planning/imports/${batch.id}`)}
                    >
                      <td>
                        <div className="primary-cell">
                          <strong>{batch.id}</strong>
                          <span>Open batch detail</span>
                        </div>
                      </td>
                      <td>{batch.sourceFileName}</td>
                      <td>{formatDateTime(batch.importedAtUtc)}</td>
                      <td>
                        <StatusBadge status={batch.status} />
                      </td>
                      <td>{batch.totalRows}</td>
                      <td>{batch.successfulRows}</td>
                      <td>{batch.failedRows}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : null}
        </section>
      </section>
    </main>
  )
}
