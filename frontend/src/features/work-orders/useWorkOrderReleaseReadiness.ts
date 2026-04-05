import { useQuery } from '@tanstack/react-query'
import { getReleaseReadiness } from '../../api/workOrders'

export function useWorkOrderReleaseReadiness(id: string | undefined) {
  return useQuery({
    queryKey: ['work-order-release-readiness', id],
    queryFn: () => getReleaseReadiness(id!),
    enabled: Boolean(id),
  })
}
