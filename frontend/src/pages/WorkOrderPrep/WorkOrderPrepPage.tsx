import { useMemo, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { AxiosError } from 'axios'
import { Link } from 'react-router-dom'
import { Breadcrumbs } from '../../components/ui/Breadcrumbs'
import { StatusBadge } from '../../components/ui/StatusBadge'
import { generateWorkOrder, releaseWorkOrder } from '../../api/workOrders'
import { useAssemblyMaterialReadiness } from '../../features/material-requirements/useAssemblyMaterialReadiness'
import { useFinishedGoodAssemblies } from '../../features/material-requirements/useFinishedGoodAssemblies'
import { useItemMasters } from '../../features/material-requirements/useItemMasters'
import { useProjectFinishedGoods } from '../../features/material-requirements/useProjectFinishedGoods'
import { useProjects } from '../../features/material-requirements/useProjects'
import { useRoutingOperations } from '../../features/routing/useRoutingOperations'
import { useRoutingTemplates } from '../../features/routing/useRoutingTemplates'
import { useWorkOrderReleaseReadiness } from '../../features/work-orders/useWorkOrderReleaseReadiness'
import { useWorkOrdersSummary } from '../../features/work-orders/useWorkOrdersSummary'

function getErrorMessage(error: unknown) {
  if (error instanceof AxiosError) {
    return error.response?.data?.detail ?? error.response?.data?.title ?? error.message
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Unknown error'
}

export function WorkOrderPrepPage() {
  const queryClient = useQueryClient()
  const projectsQuery = useProjects()
  const itemMastersQuery = useItemMasters()
  const routingTemplatesQuery = useRoutingTemplates()

  const [selectedProjectId, setSelectedProjectId] = useState('')
  const [selectedFinishedGoodId, setSelectedFinishedGoodId] = useState('')
  const [selectedAssemblyId, setSelectedAssemblyId] = useState('')
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
  const selectedFinishedGood = useMemo(
    () => finishedGoods.find((finishedGood) => finishedGood.id === effectiveFinishedGoodId) ?? null,
    [effectiveFinishedGoodId, finishedGoods],
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

  const materialReadinessQuery = useAssemblyMaterialReadiness(effectiveAssemblyId || undefined)
  const routingTemplates = useMemo(() => routingTemplatesQuery.data ?? [], [routingTemplatesQuery.data])
  const linkedRoutingTemplate = useMemo(() => {
    if (!linkedItemMasterId) {
      return null
    }

    return (
      routingTemplates.find((template) => template.itemMasterId === linkedItemMasterId && template.isActive) ??
      routingTemplates.find((template) => template.itemMasterId === linkedItemMasterId) ??
      null
    )
  }, [linkedItemMasterId, routingTemplates])
  const routingOperationsQuery = useRoutingOperations(linkedRoutingTemplate?.id)

  const workOrdersQuery = useWorkOrdersSummary({
    projectCode: selectedProject?.code ?? undefined,
    assemblyCode: selectedAssembly?.code ?? undefined,
  })

  const existingWorkOrder = useMemo(() => {
    return workOrdersQuery.data?.find((row) => row.projectCode === selectedProject?.code && row.assemblyCode === selectedAssembly?.code) ?? null
  }, [selectedAssembly?.code, selectedProject?.code, workOrdersQuery.data])

  const releaseReadinessQuery = useWorkOrderReleaseReadiness(existingWorkOrder?.workOrderId)

  const generateMutation = useMutation({
    mutationFn: () =>
      generateWorkOrder({
        projectId: effectiveProjectId,
        finishedGoodId: effectiveFinishedGoodId,
        assemblyId: effectiveAssemblyId,
      }),
    onMutate: () => {
      setPageError(null)
      setSuccessMessage(null)
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['work-orders-summary'] }),
        queryClient.invalidateQueries({ queryKey: ['work-order-release-readiness'] }),
      ])
      setSuccessMessage('Work order generated successfully.')
    },
    onError: (error) => setPageError(getErrorMessage(error)),
  })

  const releaseMutation = useMutation({
    mutationFn: () => releaseWorkOrder(existingWorkOrder!.workOrderId),
    onMutate: () => {
      setPageError(null)
      setSuccessMessage(null)
    },
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['work-orders-summary'] }),
        queryClient.invalidateQueries({ queryKey: ['work-order-release-readiness', existingWorkOrder?.workOrderId] }),
        queryClient.invalidateQueries({ queryKey: ['work-order-summary', existingWorkOrder?.workOrderId] }),
      ])
      setSuccessMessage('Work order released successfully.')
    },
    onError: (error) => setPageError(getErrorMessage(error)),
  })

  const projectsError = projectsQuery.isError ? getErrorMessage(projectsQuery.error) : null
  const finishedGoodsError = finishedGoodsQuery.isError ? getErrorMessage(finishedGoodsQuery.error) : null
  const assembliesError = assembliesQuery.isError ? getErrorMessage(assembliesQuery.error) : null
  const linkedItemError = itemMastersQuery.isError ? getErrorMessage(itemMastersQuery.error) : null
  const materialReadinessError = materialReadinessQuery.isError ? getErrorMessage(materialReadinessQuery.error) : null
  const routingTemplatesError = routingTemplatesQuery.isError ? getErrorMessage(routingTemplatesQuery.error) : null
  const workOrdersError = workOrdersQuery.isError ? getErrorMessage(workOrdersQuery.error) : null
  const releaseReadinessError = releaseReadinessQuery.isError ? getErrorMessage(releaseReadinessQuery.error) : null

  const routingExists = Boolean(linkedRoutingTemplate)
  const routeOperationCount = routingExists ? routingOperationsQuery.data?.length ?? 0 : 0

  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="page-head-stack">
          <Breadcrumbs
            items={[
              { label: 'Planning', to: '/planning' },
              { label: 'Work Order Prep' },
            ]}
          />
          <div className="brand-block">
            <span className="eyebrow">Planning Console</span>
            <h1 className="page-title">Work Order Prep</h1>
            <p className="page-subtitle">
              Move cleanly from planning into execution by checking readiness, generating the work order, and releasing it when the assembly is ready.
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
                Start in planning context, then prepare the exact assembly that should move into execution.
              </p>
            </div>
          </div>

          <div className="import-form-grid">
            <div className="field">
              <label htmlFor="prep-project-selector">Project</label>
              <select
                id="prep-project-selector"
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
              <label htmlFor="prep-fg-selector">Finished Good</label>
              <select
                id="prep-fg-selector"
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
              <label htmlFor="prep-assembly-selector">Assembly</label>
              <select
                id="prep-assembly-selector"
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
              Refreshing project context so you can prepare the right assembly for execution.
            </div>
          ) : null}
        </section>

        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Linked Source Item</span>
              <h2 className="section-title">Product-definition source for this assembly</h2>
              <p className="page-subtitle">
                This is the linked source item that material readiness and routing are evaluated against.
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
                  <span>Finished Good</span>
                  <strong>{selectedFinishedGood?.code ?? '-'}</strong>
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
                  <span>Item Name</span>
                  <strong>{linkedItemMaster.name}</strong>
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
                  This execution assembly is not linked to a source component item master yet, so it cannot be prepared for execution here.
                </p>
              </div>
            )
          ) : null}
        </section>

        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Preparation Status</span>
              <h2 className="section-title">Readiness before generation</h2>
              <p className="page-subtitle">
                Check the two core prerequisites first: material definition and routing availability.
              </p>
            </div>
          </div>

          <div className="detail-grid">
            <div className="detail-stat">
              <span>Material Readiness</span>
              <strong className="detail-badge">
                {materialReadinessQuery.isLoading
                  ? <StatusBadge label="Checking" tone="neutral" />
                  : materialReadinessQuery.data?.isMaterialReady
                    ? <StatusBadge label="Ready" tone="ready" />
                    : <StatusBadge label="Missing" tone="missing" />}
              </strong>
            </div>
            <div className="detail-stat">
              <span>Material Requirements</span>
              <strong>{materialReadinessQuery.data?.materialRequirementCount ?? 0}</strong>
            </div>
            <div className="detail-stat">
              <span>Routing Exists</span>
              <strong className="detail-badge">
                <StatusBadge label={routingExists ? 'Ready' : 'Missing'} tone={routingExists ? 'ready' : 'missing'} />
              </strong>
            </div>
            <div className="detail-stat">
              <span>Route Operations</span>
              <strong>{routeOperationCount}</strong>
            </div>
            <div className="detail-stat">
              <span>Existing Work Order</span>
              <strong>{existingWorkOrder ? existingWorkOrder.workOrderNumber : 'None'}</strong>
            </div>
            <div className="detail-stat">
              <span>Current WO Status</span>
              <strong className="detail-badge">
                {existingWorkOrder ? <StatusBadge status={existingWorkOrder.status} /> : <StatusBadge label="Missing" tone="missing" />}
              </strong>
            </div>
          </div>

          {materialReadinessError ? <div className="warning-box">{materialReadinessError}</div> : null}
          {routingTemplatesError ? <div className="warning-box">{routingTemplatesError}</div> : null}
          {workOrdersError ? <div className="warning-box">{workOrdersError}</div> : null}
        </section>

        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Work Order Action</span>
              <h2 className="section-title">Generate the work order</h2>
              <p className="page-subtitle">
                Create the execution record for this assembly once the planner is satisfied with the setup state.
              </p>
            </div>
          </div>

          {!existingWorkOrder ? (
            <>
              <div className="center-message">No work order exists yet for this assembly.</div>
              <div className="button-row">
                <button
                  type="button"
                  className="action-button"
                  disabled={!selectedAssembly || generateMutation.isPending}
                  onClick={() => generateMutation.mutate()}
                >
                  {generateMutation.isPending ? 'Generating...' : 'Generate Work Order'}
                </button>
              </div>
            </>
          ) : (
            <div className="detail-grid">
              <div className="detail-stat">
                <span>Work Order</span>
                <strong>{existingWorkOrder.workOrderNumber}</strong>
              </div>
                <div className="detail-stat">
                  <span>Status</span>
                  <strong className="detail-badge">
                    <StatusBadge status={existingWorkOrder.status} />
                  </strong>
                </div>
              <div className="detail-stat">
                <span>Current Operation</span>
                <strong>{existingWorkOrder.currentOperationCode ?? 'Waiting'}</strong>
              </div>
            </div>
          )}
        </section>

        <section className="panel panel-pad">
          <div className="import-section-head">
            <div>
              <span className="eyebrow">Release Readiness</span>
              <h2 className="section-title">Release into execution</h2>
              <p className="page-subtitle">
                Once the work order exists, confirm release readiness and release it directly from planning if appropriate.
              </p>
            </div>
            {existingWorkOrder ? (
              <Link className="text-link text-link--button" to={`/supervisor/work-orders/${existingWorkOrder.workOrderId}`}>
                Open WO Detail
              </Link>
            ) : null}
          </div>

          {!existingWorkOrder ? (
            <div className="center-message">Generate a work order first to check release readiness.</div>
          ) : null}

          {existingWorkOrder && releaseReadinessQuery.isLoading ? (
            <div className="center-message">Checking release readiness...</div>
          ) : null}

          {existingWorkOrder && releaseReadinessError ? (
            <div className="error-box">{releaseReadinessError}</div>
          ) : null}

          {existingWorkOrder && !releaseReadinessQuery.isLoading && !releaseReadinessError && releaseReadinessQuery.data ? (
            <>
              <div className="detail-grid">
                <div className="detail-stat">
                  <span>Release Ready</span>
                  <strong className="detail-badge">
                    <StatusBadge
                      label={releaseReadinessQuery.data.isReleaseReady ? 'Ready' : 'Blocked'}
                      tone={releaseReadinessQuery.data.isReleaseReady ? 'ready' : 'blocked'}
                    />
                  </strong>
                </div>
                <div className="detail-stat">
                  <span>Has Operations</span>
                  <strong>{releaseReadinessQuery.data.hasOperations ? 'Yes' : 'No'}</strong>
                </div>
                <div className="detail-stat">
                  <span>Operation Count</span>
                  <strong>{releaseReadinessQuery.data.operationCount}</strong>
                </div>
                <div className="detail-stat">
                  <span>Material Ready</span>
                  <strong className="detail-badge">
                    <StatusBadge
                      label={releaseReadinessQuery.data.isMaterialReady ? 'Ready' : 'Missing'}
                      tone={releaseReadinessQuery.data.isMaterialReady ? 'ready' : 'missing'}
                    />
                  </strong>
                </div>
                <div className="detail-stat">
                  <span>WO Status</span>
                  <strong className="detail-badge">
                    <StatusBadge status={releaseReadinessQuery.data.workOrderStatus} />
                  </strong>
                </div>
              </div>

              {releaseReadinessQuery.data.blockingReasons.length > 0 ? (
                <div className="warning-box">
                  <strong>Release is blocked</strong>
                  <ul className="reason-list">
                    {releaseReadinessQuery.data.blockingReasons.map((reason) => (
                      <li key={reason}>{reason}</li>
                    ))}
                  </ul>
                </div>
              ) : null}

              <div className="button-row">
                <button
                  type="button"
                  className="action-button"
                  disabled={!releaseReadinessQuery.data.isReleaseReady || releaseMutation.isPending}
                  onClick={() => releaseMutation.mutate()}
                >
                  {releaseMutation.isPending ? 'Releasing...' : 'Release Work Order'}
                </button>
              </div>
            </>
          ) : null}

          {successMessage ? <div className="success-box">{successMessage}</div> : null}
          {pageError ? <div className="error-box">{pageError}</div> : null}
        </section>
      </section>
    </main>
  )
}
