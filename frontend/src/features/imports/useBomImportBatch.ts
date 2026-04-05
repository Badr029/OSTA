import { useQuery } from '@tanstack/react-query'
import { getBomImportBatch } from '../../api/imports'

export function useBomImportBatch(id: string | undefined) {
  return useQuery({
    queryKey: ['bom-import-batch', id],
    queryFn: () => getBomImportBatch(id!),
    enabled: Boolean(id),
  })
}
