import { useQuery } from '@tanstack/react-query'
import { getBomImportTemplates } from '../../api/imports'

export function useBomImportTemplates() {
  return useQuery({
    queryKey: ['bom-import-templates'],
    queryFn: getBomImportTemplates,
    staleTime: 60_000,
  })
}
