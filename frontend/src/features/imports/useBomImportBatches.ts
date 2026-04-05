import { useQuery } from '@tanstack/react-query'
import { getBomImportBatches } from '../../api/imports'

export function useBomImportBatches() {
  return useQuery({
    queryKey: ['bom-import-batches'],
    queryFn: getBomImportBatches,
  })
}
