import { useQuery } from '@tanstack/react-query'
import { getItemMasters } from '../../api/materialRequirements'

export function useItemMasters() {
  return useQuery({
    queryKey: ['item-masters'],
    queryFn: getItemMasters,
  })
}
