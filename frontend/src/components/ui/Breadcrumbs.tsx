import { Link } from 'react-router-dom'

interface BreadcrumbItem {
  label: string
  to?: string
}

interface BreadcrumbsProps {
  items: BreadcrumbItem[]
}

export function Breadcrumbs({ items }: BreadcrumbsProps) {
  return (
    <nav className="breadcrumbs" aria-label="Breadcrumb">
      {items.map((item, index) => {
        const isLast = index === items.length - 1

        return (
          <span key={`${item.label}-${index}`} className="breadcrumbs__item">
            {item.to && !isLast ? (
              <Link className="breadcrumbs__link" to={item.to}>
                {item.label}
              </Link>
            ) : (
              <span className="breadcrumbs__current">{item.label}</span>
            )}
            {!isLast ? <span className="breadcrumbs__separator">/</span> : null}
          </span>
        )
      })}
    </nav>
  )
}
