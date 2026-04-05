import { useQuery } from '@tanstack/react-query'
import { getFinishedGoodAssemblies } from '../../api/materialRequirements'

export function useFinishedGoodAssemblies(finishedGoodId: string | undefined) {
  return useQuery({
    queryKey: ['finishedgood-assemblies', finishedGoodId],
    queryFn: () => getFinishedGoodAssemblies(finishedGoodId!),
    enabled: Boolean(finishedGoodId),
  })
}
