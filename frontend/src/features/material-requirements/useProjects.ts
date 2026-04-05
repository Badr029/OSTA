import { useQuery } from '@tanstack/react-query'
import { getProjects } from '../../api/materialRequirements'

export function useProjects() {
  return useQuery({
    queryKey: ['projects'],
    queryFn: getProjects,
  })
}
