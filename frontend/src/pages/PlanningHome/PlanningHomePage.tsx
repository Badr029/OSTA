import { Link } from 'react-router-dom'

export function PlanningHomePage() {
  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="brand-block">
          <span className="eyebrow">Planning Console</span>
          <h1 className="page-title">Planning Home</h1>
          <p className="page-subtitle">
            Prepare clean engineering and planning data before execution starts: import BOMs, review results, define materials, and set routes.
          </p>
        </div>
      </header>

      <section className="home-grid">
        <article className="panel panel-pad home-card">
          <div>
            <span className="eyebrow">Import</span>
            <h2 className="section-title">BOM Import</h2>
            <p className="page-subtitle">
              Choose a template, upload a CSV, preview mapped rows, and commit a batch into the system.
            </p>
          </div>
          <Link className="text-link text-link--button" to="/planning/imports">
            Open BOM Import
          </Link>
        </article>

        <article className="panel panel-pad home-card">
          <div>
            <span className="eyebrow">Review</span>
            <h2 className="section-title">Import History</h2>
            <p className="page-subtitle">
              Reopen past batch results, inspect row outcomes, and confirm what landed after earlier imports.
            </p>
          </div>
          <Link className="text-link text-link--button" to="/planning/imports/history">
            Open Import History
          </Link>
        </article>

        <article className="panel panel-pad home-card">
          <div>
            <span className="eyebrow">Readiness</span>
            <h2 className="section-title">Material Requirements</h2>
            <p className="page-subtitle">
              Define the requirement layer behind each execution assembly so material readiness can turn green.
            </p>
          </div>
          <Link className="text-link text-link--button" to="/planning/material-requirements">
            Open Material Requirements
          </Link>
        </article>

        <article className="panel panel-pad home-card">
          <div>
            <span className="eyebrow">Routing</span>
            <h2 className="section-title">Routing Setup</h2>
            <p className="page-subtitle">
              Create the production route and operation sequence that will later drive work orders and queue flow.
            </p>
          </div>
          <Link className="text-link text-link--button" to="/planning/routing-setup">
            Open Routing Setup
          </Link>
        </article>

        <article className="panel panel-pad home-card">
          <div>
            <span className="eyebrow">Execution Prep</span>
            <h2 className="section-title">Work Order Prep</h2>
            <p className="page-subtitle">
              Check linked item readiness, confirm routing and materials, generate the work order, and release it when ready.
            </p>
          </div>
          <Link className="text-link text-link--button" to="/planning/work-order-prep">
            Open Work Order Prep
          </Link>
        </article>
      </section>
    </main>
  )
}
