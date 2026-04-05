import { NavLink, Outlet } from 'react-router-dom'

export function SupervisorLayout() {
  return (
    <div className="console-shell">
      <section className="console-bar">
        <nav className="console-nav" aria-label="Supervisor navigation">
          <NavLink
            to="/supervisor"
            end
            className={({ isActive }) => `console-nav-link ${isActive ? 'console-nav-link--active' : ''}`}
          >
            Home
          </NavLink>
          <NavLink
            to="/supervisor/work-orders"
            className={({ isActive }) => `console-nav-link ${isActive ? 'console-nav-link--active' : ''}`}
          >
            Work Orders
          </NavLink>
          <NavLink
            to="/supervisor/work-centers"
            className={({ isActive }) => `console-nav-link ${isActive ? 'console-nav-link--active' : ''}`}
          >
            Work Centers
          </NavLink>
        </nav>
      </section>

      <Outlet />
    </div>
  )
}
