import { Link } from 'react-router-dom'

export function SupervisorHomePage() {
  return (
    <main className="page-shell">
      <header className="topbar">
        <div className="brand-block">
          <span className="eyebrow">Supervisor Console</span>
          <h1 className="page-title">Supervisor Home</h1>
          <p className="page-subtitle">
            Step into live execution work: monitor work orders, inspect the next action, and watch station queues.
          </p>
        </div>
      </header>

      <section className="home-grid">
        <article className="panel panel-pad home-card">
          <div>
            <span className="eyebrow">Execution View</span>
            <h2 className="section-title">Work Orders Board</h2>
            <p className="page-subtitle">
              Review what is planned, released, in progress, or blocked, then open any work order detail directly.
            </p>
          </div>
          <Link className="text-link text-link--button" to="/supervisor/work-orders">
            Open Work Orders
          </Link>
        </article>

        <article className="panel panel-pad home-card">
          <div>
            <span className="eyebrow">Station View</span>
            <h2 className="section-title">Work Center Queue</h2>
            <p className="page-subtitle">
              See what each station is running now and what is waiting next at LASER, FITUP, WELD, and QC.
            </p>
          </div>
          <Link className="text-link text-link--button" to="/supervisor/work-centers">
            Open Work Centers
          </Link>
        </article>
      </section>
    </main>
  )
}
