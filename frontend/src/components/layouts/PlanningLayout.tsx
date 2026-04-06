import { NavLink, Outlet } from 'react-router-dom'

export function PlanningLayout() {
  return (
    <div className="console-shell">
      <section className="console-bar">
        <nav className="console-nav" aria-label="Planning navigation">
          <NavLink
            to="/planning"
            end
            className={({ isActive }) => `console-nav-link ${isActive ? 'console-nav-link--active' : ''}`}
          >
            Home
          </NavLink>
          <NavLink
            to="/planning/imports"
            className={({ isActive }) => `console-nav-link ${isActive ? 'console-nav-link--active' : ''}`}
          >
            BOM Import
          </NavLink>
          <NavLink
            to="/planning/imports/history"
            className={({ isActive }) => `console-nav-link ${isActive ? 'console-nav-link--active' : ''}`}
          >
            Import History
          </NavLink>
          <NavLink
            to="/planning/material-requirements"
            className={({ isActive }) => `console-nav-link ${isActive ? 'console-nav-link--active' : ''}`}
          >
            Material Requirements
          </NavLink>
          <NavLink
            to="/planning/work-centers"
            className={({ isActive }) => `console-nav-link ${isActive ? 'console-nav-link--active' : ''}`}
          >
            Work Centers
          </NavLink>
          <NavLink
            to="/planning/routing-setup"
            className={({ isActive }) => `console-nav-link ${isActive ? 'console-nav-link--active' : ''}`}
          >
            Routing Setup
          </NavLink>
          <NavLink
            to="/planning/work-order-prep"
            className={({ isActive }) => `console-nav-link ${isActive ? 'console-nav-link--active' : ''}`}
          >
            Work Order Prep
          </NavLink>
        </nav>
      </section>

      <Outlet />
    </div>
  )
}
