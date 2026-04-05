export function formatStatusLabel(value: string | null | undefined, fallback = 'Waiting') {
  if (!value) {
    return fallback
  }

  return value
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace(/^Qc\b/i, 'QC')
}

export function getStatusTone(value: string | null | undefined) {
  switch (value) {
    case 'Planned':
      return 'planned'
    case 'Released':
      return 'released'
    case 'Ready':
      return 'ready'
    case 'InProgress':
      return 'inprogress'
    case 'Completed':
      return 'completed'
    case 'Closed':
    case 'Done':
      return 'done'
    case 'Blocked':
    case 'Failed':
    case 'Error':
      return 'blocked'
    case 'QcHold':
      return 'qchold'
    case 'Missing':
      return 'missing'
    case 'Not Applicable':
      return 'notapplicable'
    case 'Active':
      return 'ready'
    case 'true':
      return 'ready'
    case 'false':
      return 'missing'
    default:
      return 'neutral'
  }
}
