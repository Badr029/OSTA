import { useMemo, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { AxiosError } from 'axios'
import { Breadcrumbs } from '../../components/ui/Breadcrumbs'
import { StatusBadge } from '../../components/ui/StatusBadge'
import { useDeleteRoutingOperation } from '../../features/routing/useDeleteRoutingOperation'
import { useCreateRoutingOperation } from '../../features/routing/useCreateRoutingOperation'
import { useCreateRoutingTemplate } from '../../features/routing/useCreateRoutingTemplate'
import { useRoutingOperations } from '../../features/routing/useRoutingOperations'
import { useRoutingTemplates } from '../../features/routing/useRoutingTemplates'
import { useUpdateRoutingOperation } from '../../features/routing/useUpdateRoutingOperation'
import { useFinishedGoodAssemblies } from '../../features/material-requirements/useFinishedGoodAssemblies'
import { useItemMasters } from '../../features/material-requirements/useItemMasters'
import { useProjectFinishedGoods } from '../../features/material-requirements/useProjectFinishedGoods'
import { useProjects } from '../../features/material-requirements/useProjects'
import { useWorkCenters } from '../../features/work-centers/useWorkCenters'
import type { CreateRoutingOperationInput } from '../../types/routing'

const initialOperationForm = {
  operationNumber: '0010',
  operationCode: 'CUT',
  operationName: 'Cutting',
  workCenterId: '',
  setupTimeMinutes: '0',
  runTimeMinutes: '0',
  sequence: '10',
  isQcGate: false,
}

function formatQuantity(value: number | null | undefined) {
  if (value == null) {
    return '-'
  }

  return new Intl.NumberFormat(undefined, {
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
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

function parseRequiredNumber(value: string) {
  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : NaN
}

function buildDefaultRouteCode(itemCode: string) {
  return `${itemCode}-ROUTE-A`.slice(0, 100)
}

function buildDefaultRouteName(itemName: string) {
  return `${itemName} Route`.slice(0, 200)
}

export function RoutingSetupPage() {
  const queryClient = useQueryClient()
  const projectsQuery = useProjects()
  const itemMastersQuery = useItemMasters()
  const workCentersQuery = useWorkCenters()
  const routingTemplatesQuery = useRoutingTemplates()
    const createRoutingTemplateMutation = useCreateRoutingTemplate()
    const createRoutingOperationMutation = useCreateRoutingOperation()
    const updateRoutingOperationMutation = useUpdateRoutingOperation()
    const deleteRoutingOperationMutation = useDeleteRoutingOperation()

  const [selectedProjectId, setSelectedProjectId] = useState('')
  const [selectedFinishedGoodId, setSelectedFinishedGoodId] = useState('')
  const [selectedAssemblyId, setSelectedAssemblyId] = useState('')
  const [operationForm, setOperationForm] = useState(initialOperationForm)
  const [editingOperationId, setEditingOperationId] = useState<string | null>(null)
  const [inactiveWorkCenterWarning, setInactiveWorkCenterWarning] = useState<string | null>(null)
  const [pageError, setPageError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const projects = useMemo(() => projectsQuery.data ?? [], [projectsQuery.data])
  const effectiveProjectId = useMemo(() => {
    if (selectedProjectId) {
      return selectedProjectId
    }

    return projects.find((project) => project.code === 'QCC-25-30744')?.id ?? projects[0]?.id ?? ''
  }, [projects, selectedProjectId])

  const finishedGoodsQuery = useProjectFinishedGoods(effectiveProjectId || undefined)
  const finishedGoods = useMemo(() => finishedGoodsQuery.data ?? [], [finishedGoodsQuery.data])
  const effectiveFinishedGoodId = useMemo(() => {
    if (selectedFinishedGoodId) {
      return selectedFinishedGoodId
    }

    return finishedGoods[0]?.id ?? ''
  }, [finishedGoods, selectedFinishedGoodId])

  const assembliesQuery = useFinishedGoodAssemblies(effectiveFinishedGoodId || undefined)
  const assemblies = useMemo(() => assembliesQuery.data ?? [], [assembliesQuery.data])
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

  const routingTemplates = useMemo(() => routingTemplatesQuery.data ?? [], [routingTemplatesQuery.data])
  const activeRoutingTemplate = useMemo(() => {
    if (!linkedItemMasterId) {
      return null
    }

    return (
      routingTemplates.find((template) => template.itemMasterId === linkedItemMasterId && template.isActive) ??
      routingTemplates.find((template) => template.itemMasterId === linkedItemMasterId) ??
      null
    )
  }, [linkedItemMasterId, routingTemplates])

  const routingOperationsQuery = useRoutingOperations(activeRoutingTemplate?.id)
  const routingOperations = useMemo(() => routingOperationsQuery.data ?? [], [routingOperationsQuery.data])
  const workCenters = useMemo(() => workCentersQuery.data ?? [], [workCentersQuery.data])
  const activeWorkCenters = useMemo(() => workCenters.filter((workCenter) => workCenter.isActive), [workCenters])
  const defaultActiveWorkCenterId = useMemo(
    () => activeWorkCenters.find((workCenter) => workCenter.code === 'LASER')?.id ?? activeWorkCenters[0]?.id ?? '',
    [activeWorkCenters],
  )
  const selectedWorkCenterId = editingOperationId
    ? operationForm.workCenterId
    : operationForm.workCenterId || defaultActiveWorkCenterId

  const handleOperationFieldChange = (field: keyof typeof initialOperationForm, value: string | boolean) => {
    if (field === 'workCenterId') {
      setInactiveWorkCenterWarning(null)
    }

    setOperationForm((current) => ({ ...current, [field]: value }))
  }

  const resetOperationForm = () => {
    const nextSequence = (routingOperations.at(-1)?.sequence ?? 0) + 10
    setOperationForm({
      operationNumber: String(nextSequence).padStart(4, '0'),
      operationCode: '',
      operationName: '',
      workCenterId: defaultActiveWorkCenterId,
      setupTimeMinutes: '0',
      runTimeMinutes: '0',
      sequence: String(nextSequence),
      isQcGate: false,
    })
    setEditingOperationId(null)
    setInactiveWorkCenterWarning(null)
  }

  const handleEditOperation = (operationId: string) => {
    const operation = routingOperations.find((item) => item.id === operationId)
    if (!operation) {
      return
    }

    setPageError(null)
    setSuccessMessage(null)
    setEditingOperationId(operation.id)
    const operationWorkCenterIsActive = activeWorkCenters.some((workCenter) => workCenter.id === operation.workCenterId)
    setOperationForm({
      operationNumber: operation.operationNumber,
      operationCode: operation.operationCode,
      operationName: operation.operationName,
      workCenterId: operationWorkCenterIsActive ? operation.workCenterId : '',
      setupTimeMinutes: String(operation.setupTimeMinutes),
      runTimeMinutes: String(operation.runTimeMinutes),
      sequence: String(operation.sequence),
      isQcGate: operation.isQcGate,
    })
    setInactiveWorkCenterWarning(
      operationWorkCenterIsActive
        ? null
        : `This route step currently points to inactive work center '${operation.workCenterCode}'. Choose an active work center before saving.`,
    )
  }

  const handleCreateRoutingTemplate = async () => {
    if (!linkedItemMaster) {
      setPageError('Select an assembly with a linked source item master before creating a routing template.')
      return
    }

    setPageError(null)
    setSuccessMessage(null)

    try {
      await createRoutingTemplateMutation.mutateAsync({
        itemMasterId: linkedItemMaster.id,
        code: buildDefaultRouteCode(linkedItemMaster.code),
        name: buildDefaultRouteName(linkedItemMaster.name),
        revision: linkedItemMaster.revision || 'A',
        status: 'Active',
        isActive: true,
      })

      await queryClient.invalidateQueries({ queryKey: ['routing-templates'] })
      setSuccessMessage('Routing template created successfully.')
    } catch (error) {
      setPageError(getErrorMessage(error))
    }
  }

  const handleSaveOperation = async () => {
    if (!activeRoutingTemplate) {
      setPageError('Create a routing template before adding operations.')
      return
    }

    const sequence = parseRequiredNumber(operationForm.sequence)
    const setupTimeMinutes = parseRequiredNumber(operationForm.setupTimeMinutes)
    const runTimeMinutes = parseRequiredNumber(operationForm.runTimeMinutes)

    if (!operationForm.operationNumber.trim()) {
      setPageError('Operation number is required.')
      return
    }

    if (!operationForm.operationCode.trim() || !operationForm.operationName.trim()) {
      setPageError('Operation code and operation name are required.')
      return
    }

    if (!selectedWorkCenterId) {
      setPageError('Choose a work center for the operation.')
      return
    }

    if (!Number.isInteger(sequence) || sequence <= 0) {
      setPageError('Sequence must be a whole number greater than zero.')
      return
    }

    if ([setupTimeMinutes, runTimeMinutes].some((value) => Number.isNaN(value) || value < 0)) {
      setPageError('Setup time and run time must be valid numbers greater than or equal to zero.')
      return
    }

    const payload: CreateRoutingOperationInput = {
      operationNumber: operationForm.operationNumber.trim(),
      operationCode: operationForm.operationCode.trim(),
      operationName: operationForm.operationName.trim(),
      workCenterId: selectedWorkCenterId,
      setupTimeMinutes,
      runTimeMinutes,
      sequence,
      isQcGate: operationForm.isQcGate,
    }

    setPageError(null)
    setSuccessMessage(null)

    try {
      if (editingOperationId) {
        await updateRoutingOperationMutation.mutateAsync({
          routingTemplateId: activeRoutingTemplate.id,
          operationId: editingOperationId,
          payload,
        })
      } else {
        await createRoutingOperationMutation.mutateAsync({
          routingTemplateId: activeRoutingTemplate.id,
          payload,
        })
      }

      await queryClient.invalidateQueries({ queryKey: ['routing-operations', activeRoutingTemplate.id] })
      resetOperationForm()
      setSuccessMessage(editingOperationId ? 'Routing operation updated successfully.' : 'Routing operation added successfully.')
    } catch (error) {
      setPageError(getErrorMessage(error))
    }
  }

  const handleDeleteOperation = async (operationId: string, operationCode: string) => {
    if (!activeRoutingTemplate) {
      return
    }

    const confirmed = window.confirm(`Delete routing operation '${operationCode}' from this route?`)
    if (!confirmed) {
      return
    }

    setPageError(null)
    setSuccessMessage(null)

    try {
      await deleteRoutingOperationMutation.mutateAsync({
        routingTemplateId: activeRoutingTemplate.id,
        operationId,
      })

      await queryClient.invalidateQueries({ queryKey: ['routing-operations', activeRoutingTemplate.id] })

      if (editingOperationId === operationId) {
        resetOperationForm()
      }

      setSuccessMessage('Routing operation deleted successfully.')
    } catch (error) {
      setPageError(getErrorMessage(error))
    }
  }

  const projectsError = projectsQuery.isError ? getErrorMessage(projectsQuery.error) : null
  const finishedGoodsError = finishedGoodsQuery.isError ? getErrorMessage(finishedGoodsQuery.error) : null
  const assembliesError = assembliesQuery.isError ? getErrorMessage(assembliesQuery.error) : null
  const linkedItemError = itemMastersQuery.isError ? getErrorMessage(itemMastersQuery.error) : null
  const routingTemplatesError = routingTemplatesQuery.isError ? getErrorMessage(routingTemplatesQuery.error) : null
  const workCentersError = workCentersQuery.isError ? getErrorMessage(workCentersQuery.error) : null

  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="page-head-stack">
          <Breadcrumbs
            items={[
              { label: 'Planning', to: '/planning' },
              { label: 'Routing Setup' },
            ]}
          />
          <div className="brand-block">
            <span className="eyebrow">Planning Console</span>
            <h1 className="page-title">Routing Setup</h1>
            <p className="page-subtitle">
              Define the production route that will drive work order generation, operation flow, queue presence, and execution lifecycle.
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
                Stay in project context first, then resolve the linked source item that the routing template belongs to.
              </p>
            </div>
          </div>

          <div className="import-form-grid">
            <div className="field">
              <label htmlFor="routing-project-selector">Project</label>
              <select
                id="routing-project-selector"
                value={effectiveProjectId}
                onChange={(event) => {
                  setSelectedProjectId(event.target.value)
                  setSelectedFinishedGoodId('')
                  setSelectedAssemblyId('')
                  setPageError(null)
                  setSuccessMessage(null)
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
              <label htmlFor="routing-fg-selector">Finished Good</label>
              <select
                id="routing-fg-selector"
                value={effectiveFinishedGoodId}
                onChange={(event) => {
                  setSelectedFinishedGoodId(event.target.value)
                  setSelectedAssemblyId('')
                  setPageError(null)
                  setSuccessMessage(null)
                }}
                disabled={!effectiveProjectId || finishedGoodsQuery.isLoading || finishedGoods.length === 0}
              >
                {finishedGoods.map((finishedGood) => (
                  <option key={finishedGood.id} value={finishedGood.id}>
                    {finishedGood.code} - {finishedGood.name}
                  </option>
                ))}
              </select>
              {finishedGoodsQuery.isLoading ? <span className="muted">Loading finished goods...</span> : null}
              {finishedGoodsError ? <span className="muted">{finishedGoodsError}</span> : null}
            </div>

            <div className="field field--full">
              <label htmlFor="routing-assembly-selector">Assembly</label>
              <select
                id="routing-assembly-selector"
                value={effectiveAssemblyId}
                onChange={(event) => {
                  setSelectedAssemblyId(event.target.value)
                  setPageError(null)
                  setSuccessMessage(null)
                }}
                disabled={!effectiveFinishedGoodId || assembliesQuery.isLoading || assemblies.length === 0}
              >
                {assemblies.map((assembly) => (
                  <option key={assembly.id} value={assembly.id}>
                    {assembly.code} - {assembly.name}
                  </option>
                ))}
              </select>
              {assembliesQuery.isLoading ? <span className="muted">Loading assemblies...</span> : null}
              {assembliesError ? <span className="muted">{assembliesError}</span> : null}
            </div>
          </div>

          {(projectsQuery.isLoading || finishedGoodsQuery.isLoading || assembliesQuery.isLoading) ? (
            <div className="loading-box">
              Refreshing project context so you can define routing on the right assembly.
            </div>
          ) : null}
        </section>

        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Linked Source Item</span>
              <h2 className="section-title">Product-definition target for routing</h2>
              <p className="page-subtitle">
                Routing belongs to the linked source item master behind the selected execution assembly.
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
                  This execution assembly does not currently point to a source component item master, so routing cannot be defined from this screen yet.
                </p>
              </div>
            )
          ) : null}
        </section>

        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Routing Template Status</span>
              <h2 className="section-title">Current route for this item</h2>
              <p className="page-subtitle">
                Create the route first, then add ordered operations beneath it.
              </p>
            </div>
          </div>

          {routingTemplatesQuery.isLoading ? <div className="center-message">Loading routing templates...</div> : null}
          {routingTemplatesError ? <div className="error-box">{routingTemplatesError}</div> : null}

          {!routingTemplatesQuery.isLoading && !routingTemplatesError ? (
            !linkedItemMaster ? (
              <div className="center-message">Resolve a linked source item first to inspect routing.</div>
            ) : activeRoutingTemplate ? (
              <div className="detail-grid">
                <div className="detail-stat">
                  <span>Route Code</span>
                  <strong>{activeRoutingTemplate.code}</strong>
                </div>
                <div className="detail-stat">
                  <span>Route Name</span>
                  <strong>{activeRoutingTemplate.name}</strong>
                </div>
                <div className="detail-stat">
                  <span>Status</span>
                  <strong>
                    <StatusBadge status={activeRoutingTemplate.status} />
                  </strong>
                </div>
                <div className="detail-stat">
                  <span>Revision</span>
                  <strong>{activeRoutingTemplate.revision}</strong>
                </div>
                <div className="detail-stat">
                  <span>Active</span>
                  <strong>{activeRoutingTemplate.isActive ? 'Yes' : 'No'}</strong>
                </div>
                <div className="detail-stat">
                  <span>Operation Count</span>
                  <strong>{routingOperations.length}</strong>
                </div>
              </div>
            ) : (
              <div className="warning-box">
                <strong>No routing template defined for this item.</strong>
                <p className="page-subtitle">
                  Create a route now so work order generation and execution can follow a real operation sequence.
                </p>
                <div className="button-row">
                  <button
                    type="button"
                    className="action-button"
                    disabled={!linkedItemMaster || createRoutingTemplateMutation.isPending}
                    onClick={handleCreateRoutingTemplate}
                  >
                    {createRoutingTemplateMutation.isPending ? 'Creating...' : 'Create Routing Template'}
                  </button>
                </div>
              </div>
            )
          ) : null}
        </section>

        <section className="panel">
          <div className="panel-pad import-section-head">
            <div>
              <span className="eyebrow">Routing Operations</span>
              <h2 className="section-title">Current route steps</h2>
              <p className="page-subtitle">
                Review the ordered operation flow that will be copied into work order operations later.
              </p>
            </div>
          </div>

          {!activeRoutingTemplate ? (
            <div className="center-message">Create a routing template before adding or reviewing operations.</div>
          ) : null}

          {activeRoutingTemplate && routingOperationsQuery.isLoading ? (
            <div className="loading-box">Loading routing operations...</div>
          ) : null}

          {activeRoutingTemplate && routingOperationsQuery.isError ? (
            <div className="panel-pad">
              <div className="error-box">
                Unable to load routing operations for this template.
                <div className="muted">{getErrorMessage(routingOperationsQuery.error)}</div>
              </div>
            </div>
          ) : null}

          {activeRoutingTemplate &&
          !routingOperationsQuery.isLoading &&
          !routingOperationsQuery.isError &&
          routingOperations.length === 0 ? (
            <div className="center-message">No routing operations defined yet for this template.</div>
          ) : null}

          {activeRoutingTemplate &&
          !routingOperationsQuery.isLoading &&
          !routingOperationsQuery.isError &&
          routingOperations.length > 0 ? (
            <div className="table-wrap">
              <table className="board-table detail-table">
                <thead>
                  <tr>
                    <th>Operation #</th>
                    <th>Code</th>
                    <th>Name</th>
                    <th>Work Center</th>
                    <th>Setup</th>
                    <th>Run</th>
                    <th>Sequence</th>
                    <th>QC Gate</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {routingOperations.map((operation) => (
                    <tr key={operation.id}>
                      <td>{operation.operationNumber}</td>
                      <td>{operation.operationCode}</td>
                      <td>{operation.operationName}</td>
                      <td>{operation.workCenterCode}</td>
                      <td>{formatQuantity(operation.setupTimeMinutes)}</td>
                      <td>{formatQuantity(operation.runTimeMinutes)}</td>
                      <td>{operation.sequence}</td>
                      <td>{operation.isQcGate ? 'Yes' : 'No'}</td>
                      <td>
                        <div className="table-action-stack">
                          <button
                            type="button"
                            className="table-action table-action--edit"
                            onClick={() => handleEditOperation(operation.id)}
                          >
                            Edit
                          </button>
                          <button
                            type="button"
                            className="table-action table-action--delete"
                            disabled={deleteRoutingOperationMutation.isPending}
                            onClick={() => handleDeleteOperation(operation.id, operation.operationCode)}
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

        {!editingOperationId ? (
        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Add Operation</span>
              <h2 className="section-title">Create a new route step</h2>
              <p className="page-subtitle">
                Add CUT, FITUP, WELD, FINAL_QC, or any other step once the route exists.
              </p>
            </div>
          </div>

          <div className="import-form-grid">
            <div className="field">
              <label htmlFor="routing-operation-number">Operation Number</label>
              <input
                id="routing-operation-number"
                type="text"
                value={operationForm.operationNumber}
                onChange={(event) => handleOperationFieldChange('operationNumber', event.target.value)}
                disabled={!activeRoutingTemplate}
              />
            </div>

            <div className="field">
              <label htmlFor="routing-operation-code">Operation Code</label>
              <input
                id="routing-operation-code"
                type="text"
                value={operationForm.operationCode}
                onChange={(event) => handleOperationFieldChange('operationCode', event.target.value)}
                placeholder="CUT"
                disabled={!activeRoutingTemplate}
              />
            </div>

            <div className="field field--full">
              <label htmlFor="routing-operation-name">Operation Name</label>
              <input
                id="routing-operation-name"
                type="text"
                value={operationForm.operationName}
                onChange={(event) => handleOperationFieldChange('operationName', event.target.value)}
                placeholder="Cutting"
                disabled={!activeRoutingTemplate}
              />
            </div>

            <div className="field">
              <label htmlFor="routing-work-center">Work Center</label>
              <select
                  id="routing-work-center"
                value={selectedWorkCenterId}
                onChange={(event) => handleOperationFieldChange('workCenterId', event.target.value)}
                disabled={!activeRoutingTemplate || workCentersQuery.isLoading || activeWorkCenters.length === 0}
              >
                {activeWorkCenters.map((workCenter) => (
                  <option key={workCenter.id} value={workCenter.id}>
                    {workCenter.code} - {workCenter.name}
                  </option>
                ))}
              </select>
              {workCentersQuery.isLoading ? <span className="muted">Loading work centers...</span> : null}
              {workCentersError ? <span className="muted">{workCentersError}</span> : null}
              {!workCentersQuery.isLoading && activeWorkCenters.length === 0 ? (
                <span className="muted">No active work centers are available for new route steps.</span>
              ) : null}
            </div>

            <div className="field">
              <label htmlFor="routing-sequence">Sequence</label>
              <input
                id="routing-sequence"
                type="number"
                min="1"
                step="1"
                value={operationForm.sequence}
                onChange={(event) => handleOperationFieldChange('sequence', event.target.value)}
                disabled={!activeRoutingTemplate}
              />
            </div>

            <div className="field">
              <label htmlFor="routing-setup-time">Setup Time Minutes</label>
              <input
                id="routing-setup-time"
                type="number"
                min="0"
                step="0.01"
                value={operationForm.setupTimeMinutes}
                onChange={(event) => handleOperationFieldChange('setupTimeMinutes', event.target.value)}
                disabled={!activeRoutingTemplate}
              />
            </div>

            <div className="field">
              <label htmlFor="routing-run-time">Run Time Minutes</label>
              <input
                id="routing-run-time"
                type="number"
                min="0"
                step="0.01"
                value={operationForm.runTimeMinutes}
                onChange={(event) => handleOperationFieldChange('runTimeMinutes', event.target.value)}
                disabled={!activeRoutingTemplate}
              />
            </div>

            <div className="field field--full checkbox-field">
              <label htmlFor="routing-is-qc-gate" className="checkbox-label">
                <input
                  id="routing-is-qc-gate"
                  type="checkbox"
                  checked={operationForm.isQcGate}
                  onChange={(event) => handleOperationFieldChange('isQcGate', event.target.checked)}
                  disabled={!activeRoutingTemplate}
                />
                Mark this step as a QC gate
              </label>
            </div>
          </div>

          <div className="button-row">
            <button
              type="button"
              className="action-button"
              disabled={!activeRoutingTemplate || createRoutingOperationMutation.isPending}
              onClick={handleSaveOperation}
            >
              {createRoutingOperationMutation.isPending ? 'Adding...' : 'Add Operation'}
            </button>
          </div>
        </section>
        ) : null}

        {editingOperationId ? (
          <section className="panel panel-pad">
            <div className="import-section-head">
              <div>
                <span className="eyebrow">Edit Operation</span>
                <h2 className="section-title">Update the selected route step</h2>
                <p className="page-subtitle">
                  You are editing an existing step. Save to apply the correction, or cancel to leave the route unchanged.
                </p>
              </div>
            </div>

            <div className="import-form-grid">
              <div className="field">
                <label htmlFor="routing-edit-operation-number">Operation Number</label>
                <input
                  id="routing-edit-operation-number"
                  type="text"
                  value={operationForm.operationNumber}
                  onChange={(event) => handleOperationFieldChange('operationNumber', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="routing-edit-operation-code">Operation Code</label>
                <input
                  id="routing-edit-operation-code"
                  type="text"
                  value={operationForm.operationCode}
                  onChange={(event) => handleOperationFieldChange('operationCode', event.target.value)}
                />
              </div>

              <div className="field field--full">
                <label htmlFor="routing-edit-operation-name">Operation Name</label>
                <input
                  id="routing-edit-operation-name"
                  type="text"
                  value={operationForm.operationName}
                  onChange={(event) => handleOperationFieldChange('operationName', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="routing-edit-work-center">Work Center</label>
                <select
                  id="routing-edit-work-center"
                  value={selectedWorkCenterId}
                  onChange={(event) => handleOperationFieldChange('workCenterId', event.target.value)}
                  disabled={workCentersQuery.isLoading || activeWorkCenters.length === 0}
                >
                  <option value="">Choose an active work center</option>
                  {activeWorkCenters.map((workCenter) => (
                    <option key={workCenter.id} value={workCenter.id}>
                      {workCenter.code} - {workCenter.name}
                    </option>
                  ))}
                </select>
                {inactiveWorkCenterWarning ? <span className="muted">{inactiveWorkCenterWarning}</span> : null}
                {!workCentersQuery.isLoading && !inactiveWorkCenterWarning && activeWorkCenters.length === 0 ? (
                  <span className="muted">No active work centers are available. Re-enable one before saving this step.</span>
                ) : null}
              </div>

              <div className="field">
                <label htmlFor="routing-edit-sequence">Sequence</label>
                <input
                  id="routing-edit-sequence"
                  type="number"
                  min="1"
                  step="1"
                  value={operationForm.sequence}
                  onChange={(event) => handleOperationFieldChange('sequence', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="routing-edit-setup-time">Setup Time Minutes</label>
                <input
                  id="routing-edit-setup-time"
                  type="number"
                  min="0"
                  step="0.01"
                  value={operationForm.setupTimeMinutes}
                  onChange={(event) => handleOperationFieldChange('setupTimeMinutes', event.target.value)}
                />
              </div>

              <div className="field">
                <label htmlFor="routing-edit-run-time">Run Time Minutes</label>
                <input
                  id="routing-edit-run-time"
                  type="number"
                  min="0"
                  step="0.01"
                  value={operationForm.runTimeMinutes}
                  onChange={(event) => handleOperationFieldChange('runTimeMinutes', event.target.value)}
                />
              </div>

              <div className="field field--full checkbox-field">
                <label htmlFor="routing-edit-is-qc-gate" className="checkbox-label">
                  <input
                    id="routing-edit-is-qc-gate"
                    type="checkbox"
                    checked={operationForm.isQcGate}
                    onChange={(event) => handleOperationFieldChange('isQcGate', event.target.checked)}
                  />
                  Mark this step as a QC gate
                </label>
              </div>
            </div>

            <div className="button-row">
              <button
                type="button"
                className="action-button"
                disabled={updateRoutingOperationMutation.isPending}
                onClick={handleSaveOperation}
              >
                {updateRoutingOperationMutation.isPending ? 'Saving...' : 'Save Operation'}
              </button>
              <button
                type="button"
                className="action-button action-button--secondary"
                onClick={resetOperationForm}
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
