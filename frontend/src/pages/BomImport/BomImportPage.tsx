import { useMemo, useState } from 'react'
import { AxiosError } from 'axios'
import { Link } from 'react-router-dom'
import { StatusBadge } from '../../components/ui/StatusBadge'
import { useBomImportCommit } from '../../features/imports/useBomImportCommit'
import { useBomImportPreview } from '../../features/imports/useBomImportPreview'
import { useBomImportTemplates } from '../../features/imports/useBomImportTemplates'
import type {
  BomImportBatchPreview,
  BomImportBatchResult,
  BomImportMode,
} from '../../types/imports'

const importModeOptions: Array<{ label: string; value: BomImportMode }> = [
  { label: 'Execution + Product Definition', value: 'ExecutionAndProductDefinition' },
  { label: 'Execution Only', value: 'ExecutionOnly' },
]

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

function buildFileKey(file: File | null) {
  return JSON.stringify({
    fileName: file?.name ?? null,
    fileSize: file?.size ?? null,
    fileModifiedAt: file?.lastModified ?? null,
  })
}

function buildPreviewKey(input: {
  templateCode: string
  defaultValuesJson: string
  file: File | null
}) {
  return JSON.stringify({
    templateCode: input.templateCode,
    defaultValuesJson: input.defaultValuesJson.trim(),
    fileName: input.file?.name ?? null,
    fileSize: input.file?.size ?? null,
    fileModifiedAt: input.file?.lastModified ?? null,
  })
}

function PreviewTable({
  preview,
  selectedImportMode,
  isHidden,
  onToggleHidden,
}: {
  preview: BomImportBatchPreview
  selectedImportMode: BomImportMode
  isHidden: boolean
  onToggleHidden: () => void
}) {
  const previewRows = preview.lines.slice(0, 12)

  return (
    <section className="panel">
      <div className="panel-pad">
        <div className="import-section-head">
          <div>
            <span className="eyebrow">Preview Result</span>
            <h2 className="section-title">Mapped rows before import</h2>
            <p className="page-subtitle">
              Review the mapped output before anything is committed to the system.
            </p>
          </div>
          <div className="import-section-actions">
            <button type="button" className="toggle-button" onClick={onToggleHidden}>
              {isHidden ? 'Show Preview' : 'Hide Preview'}
            </button>
            <div className="summary-strip">
              <div className="stat-chip">
                <strong>{preview.totalRows}</strong>
                <span>Mapped rows</span>
              </div>
              <div className="stat-chip">
                <strong>{preview.templateCode}</strong>
                <span>Template used</span>
              </div>
              <div className="stat-chip">
                <strong>{selectedImportMode === 'ExecutionAndProductDefinition' ? 'Dual' : 'Execution'}</strong>
                <span>Commit mode selected</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {isHidden ? (
        <div className="panel-pad">
          <p className="muted">Preview is hidden. Show it again any time before committing.</p>
        </div>
      ) : (
        <>
          <div className="table-wrap">
            <table className="board-table">
              <thead>
                <tr>
                  <th>Row</th>
                  <th>Project</th>
                  <th>FG</th>
                  <th>Assembly</th>
                  <th>Part</th>
                  <th>Revision</th>
                  <th>Description</th>
                  <th>Qty</th>
                  <th>Material</th>
                  <th>Thickness</th>
                  <th>Weight</th>
                </tr>
              </thead>
              <tbody>
                {previewRows.map((line) => (
                  <tr key={`${line.rowNumber}-${line.partNumber}`}>
                    <td>{line.rowNumber}</td>
                    <td>
                      <div className="primary-cell">
                        <strong>{line.projectCode}</strong>
                        <span>{line.projectName}</span>
                      </div>
                    </td>
                    <td>
                      <div className="primary-cell">
                        <strong>{line.finishedGoodCode}</strong>
                        <span>{line.finishedGoodName}</span>
                      </div>
                    </td>
                    <td>
                      <div className="primary-cell">
                        <strong>{line.assemblyCode}</strong>
                        <span>{line.assemblyName}</span>
                      </div>
                    </td>
                    <td>{line.partNumber}</td>
                    <td>{line.revision}</td>
                    <td>{line.description}</td>
                    <td>{formatQuantity(line.quantity)}</td>
                    <td>{line.materialCode ?? '-'}</td>
                    <td>{formatQuantity(line.thicknessMm)}</td>
                    <td>{formatQuantity(line.weightKg)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {preview.totalRows > previewRows.length ? (
            <div className="panel-pad">
              <p className="muted">
                Showing the first {previewRows.length} rows of {preview.totalRows}. The import will process the full file.
              </p>
            </div>
          ) : null}
        </>
      )}
    </section>
  )
}

function ResultCard({ result }: { result: BomImportBatchResult }) {
  return (
    <section className="panel panel-pad">
      <div className="success-box">Import committed successfully.</div>

      <div className="import-section-head">
        <div>
          <span className="eyebrow">Import Result</span>
          <h2 className="section-title">Batch committed</h2>
          <p className="page-subtitle">
            The batch has been created and the backend result is shown below.
          </p>
        </div>
        <div className="import-section-actions">
          <Link className="text-link text-link--button" to={`/planning/imports/${result.id}`}>
            View Batch Detail
          </Link>
          <StatusBadge status={result.status} />
        </div>
      </div>

      <div className="result-grid">
        <div className="detail-stat">
          <span>Batch Id</span>
          <strong>{result.id}</strong>
        </div>
        <div className="detail-stat">
          <span>Imported At</span>
          <strong>{formatDateTime(result.importedAtUtc)}</strong>
        </div>
        <div className="detail-stat">
          <span>Source File</span>
          <strong>{result.sourceFileName}</strong>
        </div>
        <div className="detail-stat">
          <span>Total Rows</span>
          <strong>{result.totalRows}</strong>
        </div>
        <div className="detail-stat">
          <span>Successful Rows</span>
          <strong>{result.successfulRows}</strong>
        </div>
        <div className="detail-stat">
          <span>Failed Rows</span>
          <strong>{result.failedRows}</strong>
        </div>
      </div>

      {result.failedRows > 0 ? (
        <div className="warning-box">
          <strong>Some rows failed</strong>
          <ul className="reason-list">
            {result.lines
              .filter((line) => line.errorMessage)
              .slice(0, 5)
              .map((line) => (
                <li key={line.id}>
                  Row {line.rowNumber}: {line.errorMessage}
                </li>
              ))}
          </ul>
        </div>
      ) : null}
    </section>
  )
}

export function BomImportPage() {
  const templatesQuery = useBomImportTemplates()
  const previewMutation = useBomImportPreview()
  const commitMutation = useBomImportCommit()

  const [templateCode, setTemplateCode] = useState('')
  const [importMode, setImportMode] = useState<BomImportMode>('ExecutionAndProductDefinition')
  const [defaultValuesJson, setDefaultValuesJson] = useState('')
  const [file, setFile] = useState<File | null>(null)
  const [previewResult, setPreviewResult] = useState<BomImportBatchPreview | null>(null)
  const [committedResult, setCommittedResult] = useState<BomImportBatchResult | null>(null)
  const [lastPreviewKey, setLastPreviewKey] = useState<string | null>(null)
  const [lastImportedFileKey, setLastImportedFileKey] = useState<string | null>(null)
  const [isPreviewHidden, setIsPreviewHidden] = useState(false)

  const templates = useMemo(() => templatesQuery.data ?? [], [templatesQuery.data])

  const selectedTemplateCode = useMemo(() => {
    if (templateCode) {
      return templateCode
    }

    return (
      templates.find((template) => template.code === 'QCC_MTO_CSV_V1')?.code ??
      templates.find((template) => template.isActive)?.code ??
      templates[0]?.code ??
      ''
    )
  }, [templateCode, templates])

  const currentPreviewKey = buildPreviewKey({
    templateCode: selectedTemplateCode,
    defaultValuesJson,
    file,
  })
  const currentFileKey = buildFileKey(file)
  const isAlreadyImportedForCurrentFile = Boolean(file) && currentFileKey === lastImportedFileKey

  const canPreview = Boolean(selectedTemplateCode && file)
  const canCommit =
    Boolean(previewResult) &&
    currentPreviewKey === lastPreviewKey &&
    !isAlreadyImportedForCurrentFile &&
    !previewMutation.isPending &&
    !commitMutation.isPending

  const previewError = previewMutation.isError ? getErrorMessage(previewMutation.error) : null
  const commitError = commitMutation.isError ? getErrorMessage(commitMutation.error) : null

  const handlePreview = () => {
    if (!selectedTemplateCode || !file) {
      return
    }

    setCommittedResult(null)

    previewMutation.mutate(
      {
        templateCode: selectedTemplateCode,
        defaultValuesJson,
        file,
      },
      {
        onSuccess: (result) => {
          setPreviewResult(result)
          setLastPreviewKey(currentPreviewKey)
          setIsPreviewHidden(false)
        },
      },
    )
  }

  const handleCommit = () => {
    if (!selectedTemplateCode || !file || !canCommit) {
      return
    }

    commitMutation.mutate(
      {
        templateCode: selectedTemplateCode,
        importMode,
        defaultValuesJson,
        file,
      },
      {
        onSuccess: (result) => {
          setCommittedResult(result)
          setLastImportedFileKey(currentFileKey)
        },
      },
    )
  }

  const handleFileChange = (nextFile: File | null) => {
    setFile(nextFile)
    setPreviewResult(null)
    setCommittedResult(null)
    setLastPreviewKey(null)
    setLastImportedFileKey(null)
    setIsPreviewHidden(false)
  }

  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="brand-block">
          <span className="eyebrow">Planning Console</span>
          <h1 className="page-title">BOM Import</h1>
          <p className="page-subtitle">
            Choose a template, preview mapped rows, and commit the import only when the output looks right.
          </p>
        </div>
      </header>

      <section className="detail-shell">
        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Import Form</span>
              <h2 className="section-title">Prepare a CSV import</h2>
              <p className="page-subtitle">
                Keep preview and import separate so planners can trust what is about to be created.
              </p>
            </div>
          </div>

          {templatesQuery.isLoading ? <div className="center-message">Loading BOM templates...</div> : null}

          {templatesQuery.isError ? (
            <div className="error-box">
              Unable to load BOM import templates right now.
              <div className="muted">{getErrorMessage(templatesQuery.error)}</div>
            </div>
          ) : null}

          {!templatesQuery.isLoading && !templatesQuery.isError ? (
            <div className="import-form-grid">
              <div className="field">
                <label htmlFor="template-code">Template</label>
                <select
                  id="template-code"
                  value={selectedTemplateCode}
                  onChange={(event) => setTemplateCode(event.target.value)}
                >
                  {templates.map((template) => (
                    <option key={template.id} value={template.code}>
                      {template.code} - {template.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field">
                <label htmlFor="import-mode">Import Mode</label>
                <select
                  id="import-mode"
                  value={importMode}
                  onChange={(event) => setImportMode(event.target.value as BomImportMode)}
                >
                  {importModeOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>

              <div className="field field--full">
                <label htmlFor="csv-file">CSV File</label>
                <input
                  id="csv-file"
                  type="file"
                  accept=".csv,text/csv"
                  onChange={(event) => handleFileChange(event.target.files?.[0] ?? null)}
                />
                <span className="muted">
                  CSV only for now. Keep preview separate from commit so you can verify the mapping first.
                </span>
              </div>

              <div className="field field--full">
                <label htmlFor="default-values-json">Default Values JSON</label>
                <textarea
                  id="default-values-json"
                  className="textarea-field"
                  value={defaultValuesJson}
                  onChange={(event) => setDefaultValuesJson(event.target.value)}
                  placeholder='{"ProjectCode":"QCC-25-30744","ProjectName":"QCC-25-30744"}'
                  rows={6}
                />
                <span className="muted">
                  Example: {"{"}"ProjectCode":"QCC-25-30744","ProjectName":"QCC-25-30744"{"}"}
                </span>
              </div>
            </div>
          ) : null}

          <div className="button-row">
            <button
              type="button"
              className="action-button"
              disabled={!canPreview || previewMutation.isPending || templatesQuery.isLoading || templates.length === 0}
              onClick={handlePreview}
            >
              {previewMutation.isPending ? 'Previewing...' : 'Preview Import'}
            </button>

            <button
              type="button"
              className="action-button action-button--secondary"
              disabled={!canCommit}
              onClick={handleCommit}
            >
              {commitMutation.isPending ? 'Importing...' : isAlreadyImportedForCurrentFile ? 'Imported' : 'Import BOM'}
            </button>
          </div>

          {previewError ? <div className="error-box">{previewError}</div> : null}
          {commitError ? <div className="error-box">{commitError}</div> : null}
        </section>

        {previewResult ? (
          <PreviewTable
            preview={previewResult}
            selectedImportMode={importMode}
            isHidden={isPreviewHidden}
            onToggleHidden={() => setIsPreviewHidden((value) => !value)}
          />
        ) : (
          <section className="panel">
            <div className="center-message">
              Preview mapped rows here before committing any BOM import.
            </div>
          </section>
        )}

        {committedResult ? (
          <ResultCard result={committedResult} />
        ) : (
          <section className="panel">
            <div className="center-message">
              Import result summary will appear here after a successful commit.
            </div>
          </section>
        )}
      </section>
    </main>
  )
}
