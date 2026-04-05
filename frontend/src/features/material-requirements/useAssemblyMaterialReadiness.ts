import { useQuery } from '@tanstack/react-query'
import { getAssemblyMaterialReadiness } from '../../api/materialRequirements'

export function useAssemblyMaterialReadiness(assemblyId: string | undefined) {
  return useQuery({
    queryKey: ['assembly-material-readiness', assemblyId],
    queryFn: () => getAssemblyMaterialReadiness(assemblyId!),
    enabled: Boolean(assemblyId),
  })
}
