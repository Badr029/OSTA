import { useQuery } from '@tanstack/react-query'
import { getProjectFinishedGoods } from '../../api/materialRequirements'

export function useProjectFinishedGoods(projectId: string | undefined) {
  return useQuery({
    queryKey: ['project-finishedgoods', projectId],
    queryFn: () => getProjectFinishedGoods(projectId!),
    enabled: Boolean(projectId),
  })
}
