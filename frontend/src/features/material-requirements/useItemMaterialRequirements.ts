import { useQuery } from '@tanstack/react-query'
import { getItemMaterialRequirements } from '../../api/materialRequirements'

export function useItemMaterialRequirements(itemMasterId: string | undefined) {
  return useQuery({
    queryKey: ['item-material-requirements', itemMasterId],
    queryFn: () => getItemMaterialRequirements(itemMasterId!),
    enabled: Boolean(itemMasterId),
  })
}
