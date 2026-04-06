import { useMemo, useState } from 'react'
import { AxiosError } from 'axios'
import { useQueryClient } from '@tanstack/react-query'
import { Breadcrumbs } from '../../components/ui/Breadcrumbs'
import { StatusBadge } from '../../components/ui/StatusBadge'
import { useCreateWorkCenter } from '../../features/work-centers/useCreateWorkCenter'
import { useUpdateWorkCenter } from '../../features/work-centers/useUpdateWorkCenter'
import { useWorkCenters } from '../../features/work-centers/useWorkCenters'
import type { WorkCenterInput } from '../../types/workCenters'

const initialFormState = {
  code: '',
  name: '',
  department: '',
  hourlyRate: '0',
  isActive: true,
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

function parseHourlyRate(value: string) {
  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : NaN
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat(undefined, {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(value)
}

export function WorkCentersPage() {
  const queryClient = useQueryClient()
  const workCentersQuery = useWorkCenters()
  const createWorkCenterMutation = useCreateWorkCenter()
  const updateWorkCenterMutation = useUpdateWorkCenter()

  const [formState, setFormState] = useState(initialFormState)
  const [editingWorkCenterId, setEditingWorkCenterId] = useState<string | null>(null)
  const [pageError, setPageError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const workCenters = useMemo(() => workCentersQuery.data ?? [], [workCentersQuery.data])
  const sortedWorkCenters = useMemo(
    () =>
      [...workCenters].sort((left, right) => {
        if (left.isActive !== right.isActive) {
          return left.isActive ? -1 : 1
        }

        return left.code.localeCompare(right.code)
      }),
    [workCenters],
  )

  const resetForm = () => {
    setFormState(initialFormState)
    setEditingWorkCenterId(null)
  }

  const handleFieldChange = (field: keyof typeof initialFormState, value: string | boolean) => {
    setFormState((current) => ({ ...current, [field]: value }))
  }

  const handleEditWorkCenter = (workCenterId: string) => {
    const workCenter = workCenters.find((item) => item.id === workCenterId)
    if (!workCenter) {
      return
    }

    setPageError(null)
    setSuccessMessage(null)
    setEditingWorkCenterId(workCenter.id)
    setFormState({
      code: workCenter.code,
      name: workCenter.name,
      department: workCenter.department,
      hourlyRate: String(workCenter.hourlyRate),
      isActive: workCenter.isActive,
    })
  }

  const handleSave = async () => {
    const hourlyRate = parseHourlyRate(formState.hourlyRate)

    if (!formState.code.trim() || !formState.name.trim() || !formState.department.trim()) {
      setPageError('Code, name, and department are required.')
      return
    }

    if (Number.isNaN(hourlyRate) || hourlyRate < 0) {
      setPageError('Hourly rate must be a valid number greater than or equal to zero.')
      return
    }

    const payload: WorkCenterInput = {
      code: formState.code.trim(),
      name: formState.name.trim(),
      department: formState.department.trim(),
      hourlyRate,
      isActive: formState.isActive,
    }

    setPageError(null)
    setSuccessMessage(null)

    try {
      if (editingWorkCenterId) {
        await updateWorkCenterMutation.mutateAsync({
          id: editingWorkCenterId,
          payload,
        })
      } else {
        await createWorkCenterMutation.mutateAsync(payload)
      }

      await queryClient.invalidateQueries({ queryKey: ['work-centers'] })
      resetForm()
      setSuccessMessage(editingWorkCenterId ? 'Work center updated successfully.' : 'Work center created successfully.')
    } catch (error) {
      setPageError(getErrorMessage(error))
    }
  }

  const handleToggleActive = async (workCenterId: string) => {
    const workCenter = workCenters.find((item) => item.id === workCenterId)
    if (!workCenter) {
      return
    }

    if (workCenter.isActive) {
      const confirmed = window.confirm(`Disable work center '${workCenter.code}'?`)
      if (!confirmed) {
        return
      }
    }

    setPageError(null)
    setSuccessMessage(null)

    try {
      await updateWorkCenterMutation.mutateAsync({
        id: workCenter.id,
        payload: {
          code: workCenter.code,
          name: workCenter.name,
          department: workCenter.department,
          hourlyRate: workCenter.hourlyRate,
          isActive: !workCenter.isActive,
        },
      })

      await queryClient.invalidateQueries({ queryKey: ['work-centers'] })

      if (editingWorkCenterId === workCenter.id) {
        setFormState((current) => ({ ...current, isActive: !workCenter.isActive }))
      }

      setSuccessMessage(workCenter.isActive ? 'Work center disabled successfully.' : 'Work center enabled successfully.')
    } catch (error) {
      setPageError(getErrorMessage(error))
    }
  }

  const workCentersError = workCentersQuery.isError ? getErrorMessage(workCentersQuery.error) : null

  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="page-head-stack">
          <Breadcrumbs
            items={[
              { label: 'Planning', to: '/planning' },
              { label: 'Work Centers' },
            ]}
          />
          <div className="brand-block">
            <span className="eyebrow">Planning Console</span>
            <h1 className="page-title">Work Centers</h1>
            <p className="page-subtitle">
              Maintain the active stations planners can assign to routing steps. Disable work centers safely instead of deleting historical references.
            </p>
          </div>
        </div>
      </header>

      <section className="detail-shell">
        <section className="panel">
          <div className="panel-pad import-section-head">
            <div>
              <span className="eyebrow">Current Work Centers</span>
              <h2 className="section-title">Routing master data</h2>
              <p className="page-subtitle">
                Active work centers remain available for routing setup. Inactive ones stay visible here for history and control, but drop out of new routing selections.
              </p>
            </div>
          </div>

          {workCentersQuery.isLoading ? <div className="loading-box">Loading work center master data...</div> : null}
          {workCentersError ? <div className="error-box">{workCentersError}</div> : null}

          {!workCentersQuery.isLoading && !workCentersError && sortedWorkCenters.length === 0 ? (
            <div className="center-message">No work centers exist yet. Add the first one below.</div>
          ) : null}

          {!workCentersQuery.isLoading && !workCentersError && sortedWorkCenters.length > 0 ? (
            <div className="table-wrap">
              <table className="board-table detail-table">
                <thead>
                  <tr>
                    <th>Code</th>
                    <th>Name</th>
                    <th>Department</th>
                    <th>Hourly Rate</th>
                    <th>Active</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {sortedWorkCenters.map((workCenter) => (
                    <tr key={workCenter.id}>
                      <td>{workCenter.code}</td>
                      <td>{workCenter.name}</td>
                      <td>{workCenter.department}</td>
                      <td>{formatCurrency(workCenter.hourlyRate)}</td>
                      <td>
                        <StatusBadge
                          label={workCenter.isActive ? 'Active' : 'Inactive'}
                          tone={workCenter.isActive ? 'true' : 'false'}
                        />
                      </td>
                      <td>
                        <div className="table-action-stack">
                          <button
                            type="button"
                            className="table-action table-action--edit"
                            onClick={() => handleEditWorkCenter(workCenter.id)}
                          >
                            Edit
                          </button>
                          <button
                            type="button"
                            className={`table-action ${workCenter.isActive ? 'table-action--delete' : 'table-action--start'}`}
                            disabled={updateWorkCenterMutation.isPending}
                            onClick={() => handleToggleActive(workCenter.id)}
                          >
                            {workCenter.isActive ? 'Disable' : 'Enable'}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : null}
        </section>

        {!editingWorkCenterId ? (
          <section className="panel panel-pad">
            <div className="import-section-head">
              <div>
                <span className="eyebrow">Add Work Center</span>
                <h2 className="section-title">Create a new routing station</h2>
                <p className="page-subtitle">
                  Add the minimal master data needed to support routing cleanly. New work centers start active by default.
                </p>
              </div>
            </div>

            <div className="import-form-grid">
              <div className="field">
                <label htmlFor="work-center-code">Code</label>
                <input
                  id="work-center-code"
                  type="text"
                  value={formState.code}
                  onChange={(event) => handleFieldChange('code', event.target.value)}
                  placeholder="LASER"
                />
              </div>

              <div className="field">
                <label htmlFor="work-center-name">Name</label>
                <input
                  id="work-center-name"
                  type="text"
                  value={formState.name}
                  onChange={(event) => handleFieldChange('name', event.target.value)}
                  placeholder="Laser Cutting"
                />
              </div>

              <div className="field">
                <label htmlFor="work-center-department">Department</label>
                <input
                  id="work-center-department"
                  type="text"
                  value={formState.department}
                  onChange={(event) => handleFieldChange('department', event.target.value)}
                  placeholder="Fabrication"
                />
              </div>

              <div className="field">
                <label htmlFor="work-center-hourly-rate">Hourly Rate</label>
                <input
                  id="work-center-hourly-rate"
                  type="number"
                  min="0"
                  step="0.01"
                  value={formState.hourlyRate}
                  onChange={(event) => handleFieldChange('hourlyRate', event.target.value)}
                />
              </div>

              <div className="field field--full checkbox-field">
                <label htmlFor="work-center-active" className="checkbox-label">
                  <input
                    id="work-center-active"
                    type="checkbox"
                    checked={formState.isActive}
                    onChange={(event) => handleFieldChange('isActive', event.target.checked)}
                  />
                  Keep this work center active for routing use
                </label>
              </div>
            </div>

            <div className="button-row">
              <button
                type="button"
                className="action-button"
                disabled={createWorkCenterMutation.isPending}
                onClick={handleSave}
              >
                {createWorkCenterMutation.isPending ? 'Creating...' : 'Create Work Center'}
              </button>
            </div>
          </section>
        ) : null}

        {editingWorkCenterId ? (
          <section className="panel panel-pad">
            <div className="import-section-head">
              <div>
                <span className="eyebrow">Edit Work Center</span>
                <h2 className="section-title">Update the selected station</h2>
                <p className="page-subtitle">
                  Save the correction here, or cancel to leave this work center unchanged.
                </p>
              </div>
            </div>

            <div className="import-form-grid">
              <div className="field">
                <label htmlFor="work-center-edit-code">Code</label>
                <input
                  id="work-center-edit-code"
                  type="text"
                  value={formState.code}
                  onChange={(event) => handleFieldChange('code', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="work-center-edit-name">Name</label>
                <input
                  id="work-center-edit-name"
                  type="text"
                  value={formState.name}
                  onChange={(event) => handleFieldChange('name', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="work-center-edit-department">Department</label>
                <input
                  id="work-center-edit-department"
                  type="text"
                  value={formState.department}
                  onChange={(event) => handleFieldChange('department', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="work-center-edit-hourly-rate">Hourly Rate</label>
                <input
                  id="work-center-edit-hourly-rate"
                  type="number"
                  min="0"
                  step="0.01"
                  value={formState.hourlyRate}
                  onChange={(event) => handleFieldChange('hourlyRate', event.target.value)}
                />
              </div>

              <div className="field field--full checkbox-field">
                <label htmlFor="work-center-edit-active" className="checkbox-label">
                  <input
                    id="work-center-edit-active"
                    type="checkbox"
                    checked={formState.isActive}
                    onChange={(event) => handleFieldChange('isActive', event.target.checked)}
                  />
                  Keep this work center active for routing use
                </label>
              </div>
            </div>

            <div className="button-row">
              <button
                type="button"
                className="action-button"
                disabled={updateWorkCenterMutation.isPending}
                onClick={handleSave}
              >
                {updateWorkCenterMutation.isPending ? 'Saving...' : 'Save Work Center'}
              </button>
              <button
                type="button"
                className="action-button action-button--secondary"
                onClick={resetForm}
              >
                Cancel Edit
              </button>
            </div>
          </section>
        ) : null}

        {successMessage ? <div className="success-box">{successMessage}</div> : null}
        {pageError ? <div className="error-box">{pageError}</div> : null}
      </section>
    </main>
  )
}
