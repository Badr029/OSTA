import { useQuery } from '@tanstack/react-query'
import { getWorkOrderOperations } from '../../api/workOrders'

export function useWorkOrderOperations(id: string | undefined) {
  return useQuery({
    queryKey: ['work-order-operations', id],
    queryFn: () => getWorkOrderOperations(id!),
    enabled: Boolean(id),
  })
}
