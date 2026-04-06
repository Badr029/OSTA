import { NavLink, Outlet, useLocation } from 'react-router-dom'

export function AppLayout() {
  const location = useLocation()
  const isPlanning = location.pathname.startsWith('/planning')

  const consoleLinks = isPlanning
    ? [
        { to: '/planning', label: 'Home', end: true },
        { to: '/planning/imports', label: 'BOM Import' },
        { to: '/planning/imports/history', label: 'Import History' },
        { to: '/planning/material-requirements', label: 'Material Requirements' },
        { to: '/planning/work-centers', label: 'Work Centers' },
        { to: '/planning/routing-setup', label: 'Routing Setup' },
        { to: '/planning/work-order-prep', label: 'Work Order Prep' },
      ]
    : [
        { to: '/supervisor', label: 'Home', end: true },
        { to: '/supervisor/work-orders', label: 'Work Orders' },
        { to: '/supervisor/work-centers', label: 'Work Centers' },
      ]

  return (
    <div className="app-shell">
      <aside className="app-sidebar">
        <div className="app-sidebar__brand">
          <strong className="app-brand-title">OSTA</strong>
          <span className="app-brand-subtitle">Operations Structure &amp; Tracking Assistant</span>
        </div>

        <div className="sidebar-section">
          <span className="sidebar-label">Console</span>
          <nav className="console-switch console-switch--sidebar" aria-label="Console switch">
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
        </div>

        <div className="sidebar-section">
          <span className="sidebar-label">{isPlanning ? 'Planning Navigation' : 'Supervisor Navigation'}</span>
          <nav className="console-nav console-nav--sidebar" aria-label={isPlanning ? 'Planning navigation' : 'Supervisor navigation'}>
            {consoleLinks.map((link) => (
              <NavLink
                key={link.to}
                to={link.to}
                end={link.end}
                className={({ isActive }) => `console-nav-link ${isActive ? 'console-nav-link--active' : ''}`}
              >
                {link.label}
              </NavLink>
            ))}
          </nav>
        </div>
      </aside>

      <div className="app-content">
        <Outlet />
      </div>
    </div>
  )
}
