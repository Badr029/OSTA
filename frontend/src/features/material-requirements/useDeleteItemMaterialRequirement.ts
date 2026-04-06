import { useMutation } from '@tanstack/react-query'
import { deleteItemMaterialRequirement } from '../../api/materialRequirements'

export function useDeleteItemMaterialRequirement() {
  return useMutation({
    mutationFn: (input: { itemMasterId: string; requirementId: string }) =>
      deleteItemMaterialRequirement(input.itemMasterId, input.requirementId),
  })
}
