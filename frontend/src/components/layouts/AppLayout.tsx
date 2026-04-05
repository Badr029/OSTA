import { NavLink, Outlet } from 'react-router-dom'

export function AppLayout() {
  return (
    <div className="app-shell">
      <header className="app-header">
        <strong className="app-brand-title">OSTA</strong>

        <nav className="console-switch" aria-label="Console switch">
          <NavLink
            to="/supervisor"
            className={({ isActive }) => `console-switch-link ${isActive ? 'console-switch-link--active' : ''}`}
          >
            Supervisor
          </NavLink>
          <NavLink
            to="/planning"
            className={({ isActive }) => `console-switch-link ${isActive ? 'console-switch-link--active' : ''}`}
          >
            Planning
          </NavLink>
        </nav>
      </header>

      <Outlet />
    </div>
  )
}
