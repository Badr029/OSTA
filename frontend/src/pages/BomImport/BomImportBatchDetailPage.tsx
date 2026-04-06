import { AxiosError } from 'axios'
import { Link, useParams } from 'react-router-dom'
import { Breadcrumbs } from '../../components/ui/Breadcrumbs'
import { StatusBadge } from '../../components/ui/StatusBadge'
import { useBomImportBatch } from '../../features/imports/useBomImportBatch'

function formatQuantity(value: number | null | undefined) {
  if (value == null) {
    return '-'
  }

  return new Intl.NumberFormat(undefined, {
    minimumFractionDigits: 0,
    maximumFractionDigits: 3,
  }).format(value)
}

function formatDateTime(value: string | null | undefined) {
  if (!value) {
    return 'Not available'
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

function getErrorMessage(error: unknown) {
  if (error instanceof AxiosError) {
    return error.response?.data?.detail ?? error.response?.data?.title ?? error.message
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Unknown error'
}

export function BomImportBatchDetailPage() {
  const { id } = useParams()
  const batchQuery = useBomImportBatch(id)

  if (!id) {
    return (
      <main className="page-shell">
        <section className="panel panel-pad">
          <div className="error-box">No import batch id was provided in the route.</div>
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
              { label: 'Planning', to: '/planning' },
              { label: 'Import History', to: '/planning/imports/history' },
              { label: batchQuery.data?.id ? `Batch ${batchQuery.data.id}` : 'Batch Detail' },
            ]}
          />
          <div className="brand-block">
            <span className="eyebrow">Planning Console</span>
            <h1 className="page-title">Import Batch Detail</h1>
            <p className="page-subtitle">
              Inspect the committed batch, review row outcomes, and confirm how the import landed.
            </p>
          </div>
        </div>
        <Link className="text-link" to="/planning/imports/history">
          Back to import history
        </Link>
      </header>

      {batchQuery.isLoading ? (
        <section className="panel">
          <div className="loading-box">Loading batch detail...</div>
        </section>
      ) : null}

      {batchQuery.isError ? (
        <section className="panel panel-pad">
          <div className="error-box">
            Unable to load this import batch right now.
            <div className="muted">{getErrorMessage(batchQuery.error)}</div>
          </div>
        </section>
      ) : null}

      {!batchQuery.isLoading && !batchQuery.isError && batchQuery.data ? (
        <section className="detail-shell">
          <section className="panel panel-pad">
            <div className="detail-header">
              <div>
                <span className="eyebrow">Committed Batch</span>
                <h2 className="detail-title">{batchQuery.data.id}</h2>
                <p className="page-subtitle">{batchQuery.data.sourceFileName}</p>
              </div>
              <div className="badge-row">
                <StatusBadge status={batchQuery.data.status} />
              </div>
            </div>

            <div className="detail-grid">
              <div className="detail-stat">
                <span>Imported At</span>
                <strong>{formatDateTime(batchQuery.data.importedAtUtc)}</strong>
              </div>
              <div className="detail-stat">
                <span>Total Rows</span>
                <strong>{batchQuery.data.totalRows}</strong>
              </div>
              <div className="detail-stat">
                <span>Successful Rows</span>
                <strong>{batchQuery.data.successfulRows}</strong>
              </div>
              <div className="detail-stat">
                <span>Failed Rows</span>
                <strong>{batchQuery.data.failedRows}</strong>
              </div>
            </div>
          </section>

          <section className="panel">
            <div className="panel-pad operations-head">
              <div>
                <span className="eyebrow">Batch Lines</span>
                <h3 className="section-title">Row outcomes</h3>
              </div>
            </div>

            <div className="table-wrap">
              <table className="board-table detail-table">
                <thead>
                  <tr>
                    <th>Row</th>
                    <th>Project</th>
                    <th>Assembly</th>
                    <th>Part</th>
                    <th>Description</th>
                    <th>Qty</th>
                    <th>Status</th>
                    <th>Error</th>
                  </tr>
                </thead>
                <tbody>
                  {batchQuery.data.lines.map((line) => (
                    <tr key={line.id}>
                      <td>{line.rowNumber}</td>
                      <td>
                        <div className="primary-cell">
                          <strong>{line.projectCode}</strong>
                          <span>{line.finishedGoodCode}</span>
                        </div>
                      </td>
                      <td>
                        <div className="primary-cell">
                          <strong>{line.assemblyCode}</strong>
                          <span>{line.assemblyName}</span>
                        </div>
                      </td>
                      <td>{line.partNumber}</td>
                      <td>{line.description}</td>
                      <td>{formatQuantity(line.quantity)}</td>
                      <td>
                        <StatusBadge status={line.status} />
                      </td>
                      <td>{line.errorMessage ?? '-'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        </section>
      ) : null}
    </main>
  )
}
