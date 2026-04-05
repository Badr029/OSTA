import { useQuery } from '@tanstack/react-query'
import { getWorkCenters } from '../../api/workCenters'

export function useWorkCenters() {
  return useQuery({
    queryKey: ['work-centers'],
    queryFn: getWorkCenters,
    staleTime: 60_000,
  })
}
