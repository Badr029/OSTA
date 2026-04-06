import { useMemo, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { AxiosError } from 'axios'
import { Breadcrumbs } from '../../components/ui/Breadcrumbs'
import { useCreateItemMaterialRequirement } from '../../features/material-requirements/useCreateItemMaterialRequirement'
import { useDeleteItemMaterialRequirement } from '../../features/material-requirements/useDeleteItemMaterialRequirement'
import { useFinishedGoodAssemblies } from '../../features/material-requirements/useFinishedGoodAssemblies'
import { useItemMaterialRequirements } from '../../features/material-requirements/useItemMaterialRequirements'
import { useItemMasters } from '../../features/material-requirements/useItemMasters'
import { useProjectFinishedGoods } from '../../features/material-requirements/useProjectFinishedGoods'
import { useProjects } from '../../features/material-requirements/useProjects'
import { useUpdateItemMaterialRequirement } from '../../features/material-requirements/useUpdateItemMaterialRequirement'
import type { CreateItemMaterialRequirementInput, ItemMaterialRequirement } from '../../types/materialRequirements'

function formatQuantity(value: number | null | undefined) {
  if (value == null) {
    return '-'
  }

  return new Intl.NumberFormat(undefined, {
    minimumFractionDigits: 0,
    maximumFractionDigits: 3,
  }).format(value)
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

function parseOptionalNumber(value: string) {
  const trimmed = value.trim()
  if (!trimmed) {
    return null
  }

  const parsed = Number(trimmed)
  return Number.isFinite(parsed) ? parsed : NaN
}

const initialFormState = {
  materialCode: '',
  requiredQuantity: '1',
  uom: 'PL',
  thicknessMm: '',
  lengthMm: '',
  widthMm: '',
  weightKg: '',
  notes: '',
}

export function MaterialRequirementsPage() {
  const queryClient = useQueryClient()
  const projectsQuery = useProjects()
  const itemMastersQuery = useItemMasters()
  const createRequirementMutation = useCreateItemMaterialRequirement()
  const updateRequirementMutation = useUpdateItemMaterialRequirement()
  const deleteRequirementMutation = useDeleteItemMaterialRequirement()

  const [selectedProjectId, setSelectedProjectId] = useState('')
  const [selectedFinishedGoodId, setSelectedFinishedGoodId] = useState('')
  const [selectedAssemblyId, setSelectedAssemblyId] = useState('')
  const [form, setForm] = useState(initialFormState)
  const [editingRequirementId, setEditingRequirementId] = useState<string | null>(null)
  const [formError, setFormError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const projects = useMemo(() => projectsQuery.data ?? [], [projectsQuery.data])

  const effectiveProjectId = useMemo(() => {
    if (selectedProjectId) {
      return selectedProjectId
    }

    return projects.find((project) => project.code === 'QCC-25-30744')?.id ?? projects[0]?.id ?? ''
  }, [projects, selectedProjectId])

  const actualFinishedGoodsQuery = useProjectFinishedGoods(effectiveProjectId || undefined)
  const finishedGoods = useMemo(() => actualFinishedGoodsQuery.data ?? [], [actualFinishedGoodsQuery.data])

  const effectiveFinishedGoodId = useMemo(() => {
    if (selectedFinishedGoodId) {
      return selectedFinishedGoodId
    }

    return finishedGoods[0]?.id ?? ''
  }, [finishedGoods, selectedFinishedGoodId])

  const actualAssembliesQuery = useFinishedGoodAssemblies(effectiveFinishedGoodId || undefined)
  const assemblies = useMemo(() => actualAssembliesQuery.data ?? [], [actualAssembliesQuery.data])

  const effectiveAssemblyId = useMemo(() => {
    if (selectedAssemblyId) {
      return selectedAssemblyId
    }

    return assemblies[0]?.id ?? ''
  }, [assemblies, selectedAssemblyId])

  const selectedProject = useMemo(
    () => projects.find((project) => project.id === effectiveProjectId) ?? null,
    [effectiveProjectId, projects],
  )

  const selectedAssembly = useMemo(
    () => assemblies.find((assembly) => assembly.id === effectiveAssemblyId) ?? null,
    [assemblies, effectiveAssemblyId],
  )

  const itemMasters = useMemo(() => itemMastersQuery.data ?? [], [itemMastersQuery.data])
  const linkedItemMasterId = selectedAssembly?.sourceComponentItemMasterId ?? ''
  const linkedItemMaster = useMemo(
    () => itemMasters.find((item) => item.id === linkedItemMasterId) ?? null,
    [itemMasters, linkedItemMasterId],
  )

  const requirementsQuery = useItemMaterialRequirements(linkedItemMasterId || undefined)

  const handleChange = (field: keyof typeof initialFormState, value: string) => {
    setForm((current) => ({ ...current, [field]: value }))
  }

  const resetForm = () => {
    setForm(initialFormState)
    setEditingRequirementId(null)
  }

  const startEditRequirement = (requirement: ItemMaterialRequirement) => {
    setFormError(null)
    setSuccessMessage(null)
    setEditingRequirementId(requirement.id)
    setForm({
      materialCode: requirement.materialCode,
      requiredQuantity: String(requirement.requiredQuantity),
      uom: requirement.uom,
      thicknessMm: requirement.thicknessMm == null ? '' : String(requirement.thicknessMm),
      lengthMm: requirement.lengthMm == null ? '' : String(requirement.lengthMm),
      widthMm: requirement.widthMm == null ? '' : String(requirement.widthMm),
      weightKg: requirement.weightKg == null ? '' : String(requirement.weightKg),
      notes: requirement.notes ?? '',
    })
  }

  const handleSubmit = async () => {
    if (!linkedItemMasterId) {
      setFormError('Select an assembly that has a linked source item master before managing requirements.')
      return
    }

    const requiredQuantity = Number(form.requiredQuantity)
    const thicknessMm = parseOptionalNumber(form.thicknessMm)
    const lengthMm = parseOptionalNumber(form.lengthMm)
    const widthMm = parseOptionalNumber(form.widthMm)
    const weightKg = parseOptionalNumber(form.weightKg)

    if (!form.materialCode.trim()) {
      setFormError('Material code is required.')
      return
    }

    if (!Number.isFinite(requiredQuantity) || requiredQuantity <= 0) {
      setFormError('Required quantity must be a number greater than zero.')
      return
    }

    if ([thicknessMm, lengthMm, widthMm, weightKg].some((value) => Number.isNaN(value))) {
      setFormError('Thickness, length, width, and weight must be valid numbers when provided.')
      return
    }

    const payload: CreateItemMaterialRequirementInput = {
      materialCode: form.materialCode.trim(),
      requiredQuantity,
      uom: form.uom.trim() || 'PL',
      thicknessMm,
      lengthMm,
      widthMm,
      weightKg,
      notes: form.notes.trim() || null,
    }

    setFormError(null)
    setSuccessMessage(null)

    try {
      if (editingRequirementId) {
        await updateRequirementMutation.mutateAsync({
          itemMasterId: linkedItemMasterId,
          requirementId: editingRequirementId,
          payload,
        })
      } else {
        await createRequirementMutation.mutateAsync({
          itemMasterId: linkedItemMasterId,
          payload,
        })
      }

      await queryClient.invalidateQueries({
        queryKey: ['item-material-requirements', linkedItemMasterId],
      })

      resetForm()
      setSuccessMessage(editingRequirementId ? 'Material requirement updated successfully.' : 'Material requirement added successfully.')
    } catch (error) {
      setFormError(getErrorMessage(error))
    }
  }

  const handleDeleteRequirement = async (requirement: ItemMaterialRequirement) => {
    if (!linkedItemMasterId) {
      return
    }

    const confirmed = window.confirm(`Delete material requirement '${requirement.materialCode}'?`)
    if (!confirmed) {
      return
    }

    setFormError(null)
    setSuccessMessage(null)

    try {
      await deleteRequirementMutation.mutateAsync({
        itemMasterId: linkedItemMasterId,
        requirementId: requirement.id,
      })

      await queryClient.invalidateQueries({
        queryKey: ['item-material-requirements', linkedItemMasterId],
      })

      if (editingRequirementId === requirement.id) {
        resetForm()
      }

      setSuccessMessage('Material requirement deleted successfully.')
    } catch (error) {
      setFormError(getErrorMessage(error))
    }
  }

  const projectsError = projectsQuery.isError ? getErrorMessage(projectsQuery.error) : null
  const finishedGoodsError = actualFinishedGoodsQuery.isError ? getErrorMessage(actualFinishedGoodsQuery.error) : null
  const assembliesError = actualAssembliesQuery.isError ? getErrorMessage(actualAssembliesQuery.error) : null
  const linkedItemError = itemMastersQuery.isError ? getErrorMessage(itemMastersQuery.error) : null

  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="page-head-stack">
          <Breadcrumbs
            items={[
              { label: 'Planning', to: '/planning' },
              { label: 'Material Requirements' },
            ]}
          />
          <div className="brand-block">
            <span className="eyebrow">Planning Console</span>
            <h1 className="page-title">Material Requirements</h1>
            <p className="page-subtitle">
              Start from the project and assembly the planner actually cares about, then define the linked material requirement setup.
            </p>
          </div>
        </div>
      </header>

      <section className="detail-shell">
        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Project Context</span>
              <h2 className="section-title">Choose project and assembly</h2>
              <p className="page-subtitle">
                Work the same way planners think: pick the project first, then the execution assembly, then define the linked source item requirement.
              </p>
            </div>
          </div>

          <div className="import-form-grid">
            <div className="field">
              <label htmlFor="project-selector">Project</label>
              <select
                id="project-selector"
                value={effectiveProjectId}
                onChange={(event) => {
                  setSelectedProjectId(event.target.value)
                  setSelectedFinishedGoodId('')
                  setSelectedAssemblyId('')
                  setSuccessMessage(null)
                  setFormError(null)
                }}
              >
                {projects.map((project) => (
                  <option key={project.id} value={project.id}>
                    {project.code} - {project.name}
                  </option>
                ))}
              </select>
              {projectsQuery.isLoading ? <span className="muted">Loading projects...</span> : null}
              {projectsError ? <span className="muted">{projectsError}</span> : null}
            </div>

            <div className="field">
              <label htmlFor="finished-good-selector">Finished Good</label>
              <select
                id="finished-good-selector"
                value={effectiveFinishedGoodId}
                onChange={(event) => {
                  setSelectedFinishedGoodId(event.target.value)
                  setSelectedAssemblyId('')
                  setSuccessMessage(null)
                  setFormError(null)
                }}
                disabled={!effectiveProjectId || actualFinishedGoodsQuery.isLoading || finishedGoods.length === 0}
              >
                {finishedGoods.map((finishedGood) => (
                  <option key={finishedGood.id} value={finishedGood.id}>
                    {finishedGood.code} - {finishedGood.name}
                  </option>
                ))}
              </select>
              {actualFinishedGoodsQuery.isLoading ? <span className="muted">Loading finished goods...</span> : null}
              {finishedGoodsError ? <span className="muted">{finishedGoodsError}</span> : null}
            </div>

            <div className="field field--full">
              <label htmlFor="assembly-selector">Assembly</label>
              <select
                id="assembly-selector"
                value={effectiveAssemblyId}
                onChange={(event) => {
                  setSelectedAssemblyId(event.target.value)
                  setSuccessMessage(null)
                  setFormError(null)
                }}
                disabled={!effectiveFinishedGoodId || actualAssembliesQuery.isLoading || assemblies.length === 0}
              >
                {assemblies.map((assembly) => (
                  <option key={assembly.id} value={assembly.id}>
                    {assembly.code} - {assembly.name}
                  </option>
                ))}
              </select>
              {actualAssembliesQuery.isLoading ? <span className="muted">Loading assemblies...</span> : null}
              {assembliesError ? <span className="muted">{assembliesError}</span> : null}
            </div>
          </div>

          {(projectsQuery.isLoading || actualFinishedGoodsQuery.isLoading || actualAssembliesQuery.isLoading) ? (
            <div className="loading-box">
              Refreshing project context so you can pick the right assembly.
            </div>
          ) : null}
        </section>

        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Linked Source Item</span>
              <h2 className="section-title">Product-definition item behind this assembly</h2>
              <p className="page-subtitle">
                Material requirements are still stored on the linked item master, but the planner can stay in assembly context.
              </p>
            </div>
          </div>

          {!selectedAssembly ? (
            <div className="center-message">Select an assembly to resolve its linked source item master.</div>
          ) : null}

          {itemMastersQuery.isLoading && selectedAssembly ? (
            <div className="loading-box">Resolving the linked source item master for this assembly.</div>
          ) : null}

          {linkedItemError && selectedAssembly ? (
            <div className="error-box">
              Unable to resolve linked item master information.
              <div className="muted">{linkedItemError}</div>
            </div>
          ) : null}

          {selectedAssembly && !itemMastersQuery.isLoading && !linkedItemError ? (
            linkedItemMaster ? (
              <div className="detail-grid">
                <div className="detail-stat">
                  <span>Project</span>
                  <strong>{selectedProject?.code ?? '-'}</strong>
                </div>
                <div className="detail-stat">
                  <span>Assembly</span>
                  <strong>{selectedAssembly.code}</strong>
                </div>
                <div className="detail-stat">
                  <span>Linked Item Master</span>
                  <strong>{linkedItemMaster.code}</strong>
                </div>
                <div className="detail-stat">
                  <span>Linked Item Name</span>
                  <strong>{linkedItemMaster.name}</strong>
                </div>
                <div className="detail-stat">
                  <span>Item Type</span>
                  <strong>{linkedItemMaster.itemType}</strong>
                </div>
                <div className="detail-stat">
                  <span>Revision</span>
                  <strong>{linkedItemMaster.revision}</strong>
                </div>
              </div>
            ) : (
              <div className="warning-box">
                <strong>No linked source item master</strong>
                <p className="page-subtitle">
                  This execution assembly does not currently point to a source component item master, so material requirements cannot be defined from this screen yet.
                </p>
              </div>
            )
          ) : null}
        </section>

        <section className="panel">
          <div className="panel-pad import-section-head">
            <div>
              <span className="eyebrow">Existing Requirements</span>
              <h2 className="section-title">Current requirement definitions</h2>
              <p className="page-subtitle">
                Confirm whether the linked source item already has the material definition needed for readiness checks.
              </p>
            </div>
            {selectedAssembly ? (
              <div className="badge-row">
                <span className="badge badge--ready">{selectedAssembly.code}</span>
              </div>
            ) : null}
          </div>

          {!linkedItemMasterId ? (
            <div className="center-message">Select an assembly with a linked source item master to load requirements.</div>
          ) : null}

          {linkedItemMasterId && requirementsQuery.isLoading ? (
            <div className="center-message">Loading material requirements...</div>
          ) : null}

          {linkedItemMasterId && requirementsQuery.isError ? (
            <div className="panel-pad">
              <div className="error-box">
                Unable to load material requirements for this linked item.
                <div className="muted">{getErrorMessage(requirementsQuery.error)}</div>
              </div>
            </div>
          ) : null}

          {linkedItemMasterId &&
          !requirementsQuery.isLoading &&
          !requirementsQuery.isError &&
          (requirementsQuery.data?.length ?? 0) === 0 ? (
            <div className="center-message">No material requirements defined for this linked item.</div>
          ) : null}

          {linkedItemMasterId &&
          !requirementsQuery.isLoading &&
          !requirementsQuery.isError &&
          (requirementsQuery.data?.length ?? 0) > 0 ? (
            <div className="table-wrap">
              <table className="board-table detail-table">
                <thead>
                  <tr>
                    <th>Material Code</th>
                    <th>Required Qty</th>
                    <th>UOM</th>
                    <th>Thickness</th>
                    <th>Length</th>
                    <th>Width</th>
                    <th>Weight</th>
                    <th>Notes</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {requirementsQuery.data?.map((requirement) => (
                    <tr key={requirement.id}>
                      <td>{requirement.materialCode}</td>
                      <td>{formatQuantity(requirement.requiredQuantity)}</td>
                      <td>{requirement.uom}</td>
                      <td>{formatQuantity(requirement.thicknessMm)}</td>
                      <td>{formatQuantity(requirement.lengthMm)}</td>
                      <td>{formatQuantity(requirement.widthMm)}</td>
                      <td>{formatQuantity(requirement.weightKg)}</td>
                      <td>{requirement.notes ?? '-'}</td>
                      <td>
                        <div className="table-action-stack">
                          <button
                            type="button"
                            className="table-action table-action--edit"
                            onClick={() => startEditRequirement(requirement)}
                          >
                            Edit
                          </button>
                          <button
                            type="button"
                            className="table-action table-action--delete"
                            disabled={deleteRequirementMutation.isPending}
                            onClick={() => handleDeleteRequirement(requirement)}
                          >
                            Delete
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

        {!editingRequirementId ? (
        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Add Requirement</span>
              <h2 className="section-title">Create a new material requirement</h2>
              <p className="page-subtitle">
                Add one requirement at a time while staying in the context of the execution assembly that needs it.
              </p>
            </div>
          </div>

          <div className="import-form-grid">
            <div className="field">
              <label htmlFor="material-code">Material Code</label>
              <input
                id="material-code"
                type="text"
                value={form.materialCode}
                onChange={(event) => handleChange('materialCode', event.target.value)}
                placeholder="STL-S355"
                disabled={!linkedItemMasterId}
              />
            </div>

            <div className="field">
              <label htmlFor="required-quantity">Required Quantity</label>
              <input
                id="required-quantity"
                type="number"
                min="0.0001"
                step="0.001"
                value={form.requiredQuantity}
                onChange={(event) => handleChange('requiredQuantity', event.target.value)}
                disabled={!linkedItemMasterId}
              />
            </div>

            <div className="field">
              <label htmlFor="requirement-uom">UOM</label>
              <input
                id="requirement-uom"
                type="text"
                value={form.uom}
                onChange={(event) => handleChange('uom', event.target.value)}
                disabled={!linkedItemMasterId}
              />
            </div>

            <div className="field">
              <label htmlFor="thickness-mm">Thickness mm</label>
              <input
                id="thickness-mm"
                type="number"
                min="0"
                step="0.001"
                value={form.thicknessMm}
                onChange={(event) => handleChange('thicknessMm', event.target.value)}
                disabled={!linkedItemMasterId}
              />
            </div>

            <div className="field">
              <label htmlFor="length-mm">Length mm</label>
              <input
                id="length-mm"
                type="number"
                min="0"
                step="0.001"
                value={form.lengthMm}
                onChange={(event) => handleChange('lengthMm', event.target.value)}
                disabled={!linkedItemMasterId}
              />
            </div>

            <div className="field">
              <label htmlFor="width-mm">Width mm</label>
              <input
                id="width-mm"
                type="number"
                min="0"
                step="0.001"
                value={form.widthMm}
                onChange={(event) => handleChange('widthMm', event.target.value)}
                disabled={!linkedItemMasterId}
              />
            </div>

            <div className="field">
              <label htmlFor="weight-kg">Weight kg</label>
              <input
                id="weight-kg"
                type="number"
                min="0"
                step="0.001"
                value={form.weightKg}
                onChange={(event) => handleChange('weightKg', event.target.value)}
                disabled={!linkedItemMasterId}
              />
            </div>

            <div className="field field--full">
              <label htmlFor="requirement-notes">Notes</label>
              <textarea
                id="requirement-notes"
                className="textarea-field"
                rows={4}
                value={form.notes}
                onChange={(event) => handleChange('notes', event.target.value)}
                placeholder="Frame side plate requirement"
                disabled={!linkedItemMasterId}
              />
            </div>
          </div>

          <div className="button-row">
            <button
              type="button"
              className="action-button"
              disabled={!linkedItemMasterId || createRequirementMutation.isPending}
              onClick={handleSubmit}
            >
              {createRequirementMutation.isPending ? 'Adding...' : 'Add Requirement'}
            </button>
          </div>
        </section>
        ) : null}

        {editingRequirementId ? (
          <section className="panel panel-pad">
            <div className="import-section-head">
              <div>
                <span className="eyebrow">Edit Requirement</span>
                <h2 className="section-title">Update the selected material requirement</h2>
                <p className="page-subtitle">
                  You are editing an existing requirement. Save to apply the correction, or cancel to leave it unchanged.
                </p>
              </div>
            </div>

            <div className="import-form-grid">
              <div className="field">
                <label htmlFor="edit-material-code">Material Code</label>
                <input
                  id="edit-material-code"
                  type="text"
                  value={form.materialCode}
                  onChange={(event) => handleChange('materialCode', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="edit-required-quantity">Required Quantity</label>
                <input
                  id="edit-required-quantity"
                  type="number"
                  min="0.0001"
                  step="0.001"
                  value={form.requiredQuantity}
                  onChange={(event) => handleChange('requiredQuantity', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="edit-requirement-uom">UOM</label>
                <input
                  id="edit-requirement-uom"
                  type="text"
                  value={form.uom}
                  onChange={(event) => handleChange('uom', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="edit-thickness-mm">Thickness mm</label>
                <input
                  id="edit-thickness-mm"
                  type="number"
                  min="0"
                  step="0.001"
                  value={form.thicknessMm}
                  onChange={(event) => handleChange('thicknessMm', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="edit-length-mm">Length mm</label>
                <input
                  id="edit-length-mm"
                  type="number"
                  min="0"
                  step="0.001"
                  value={form.lengthMm}
                  onChange={(event) => handleChange('lengthMm', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="edit-width-mm">Width mm</label>
                <input
                  id="edit-width-mm"
                  type="number"
                  min="0"
                  step="0.001"
                  value={form.widthMm}
                  onChange={(event) => handleChange('widthMm', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="edit-weight-kg">Weight kg</label>
                <input
                  id="edit-weight-kg"
                  type="number"
                  min="0"
                  step="0.001"
                  value={form.weightKg}
                  onChange={(event) => handleChange('weightKg', event.target.value)}
                />
              </div>

              <div className="field field--full">
                <label htmlFor="edit-requirement-notes">Notes</label>
                <textarea
                  id="edit-requirement-notes"
                  className="textarea-field"
                  rows={4}
                  value={form.notes}
                  onChange={(event) => handleChange('notes', event.target.value)}
                />
              </div>
            </div>

            <div className="button-row">
              <button
                type="button"
                className="action-button"
                disabled={updateRequirementMutation.isPending}
                onClick={handleSubmit}
              >
                {updateRequirementMutation.isPending ? 'Saving...' : 'Save Requirement'}
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
        {formError ? <div className="error-box">{formError}</div> : null}
      </section>
    </main>
  )
}
