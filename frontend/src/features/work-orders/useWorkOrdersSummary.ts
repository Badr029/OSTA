import { useQuery } from '@tanstack/react-query'
import { getWorkOrdersSummary } from '../../api/workOrders'
import type { WorkOrdersSummaryFilters } from '../../types/workOrders'

export function useWorkOrdersSummary(filters: WorkOrdersSummaryFilters) {
  return useQuery({
    queryKey: ['work-orders-summary', filters],
    queryFn: () => getWorkOrdersSummary(filters),
  })
}
