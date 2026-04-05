import { useQuery } from '@tanstack/react-query'
import { getRoutingOperations } from '../../api/routing'

export function useRoutingOperations(routingTemplateId: string | undefined) {
  return useQuery({
    queryKey: ['routing-operations', routingTemplateId],
    queryFn: () => getRoutingOperations(routingTemplateId!),
    enabled: Boolean(routingTemplateId),
  })
}
