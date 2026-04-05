import { useQuery } from '@tanstack/react-query'
import { getRoutingTemplates } from '../../api/routing'

export function useRoutingTemplates() {
  return useQuery({
    queryKey: ['routing-templates'],
    queryFn: getRoutingTemplates,
  })
}
