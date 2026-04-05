import { useQuery } from '@tanstack/react-query'
import { getWorkOrderSummary } from '../../api/workOrders'

export function useWorkOrderSummary(id: string | undefined) {
  return useQuery({
    queryKey: ['work-order-summary', id],
    queryFn: () => getWorkOrderSummary(id!),
    enabled: Boolean(id),
  })
}
