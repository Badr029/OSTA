import { formatStatusLabel, getStatusTone } from './statusBadgeUtils'

interface StatusBadgeProps {
  status?: string | null
  label?: string | null
  tone?: string | null
  className?: string
}

function sanitizeTone(value: string) {
  return value.replace(/\s+/g, '').toLowerCase()
}

export function StatusBadge({ status, label, tone, className }: StatusBadgeProps) {
  const resolvedLabel = label ?? formatStatusLabel(status)
  const resolvedTone = sanitizeTone(tone ?? getStatusTone(label ?? status))

  return (
    <span className={['badge', `badge--${resolvedTone}`, className].filter(Boolean).join(' ')}>
      {resolvedLabel}
    </span>
  )
}
