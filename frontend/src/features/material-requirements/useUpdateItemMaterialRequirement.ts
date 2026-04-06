import { useMutation } from '@tanstack/react-query'
import { updateItemMaterialRequirement } from '../../api/materialRequirements'
import type { UpdateItemMaterialRequirementInput } from '../../types/materialRequirements'

export function useUpdateItemMaterialRequirement() {
  return useMutation({
    mutationFn: (input: {
      itemMasterId: string
      requirementId: string
      payload: UpdateItemMaterialRequirementInput
    }) => updateItemMaterialRequirement(input.itemMasterId, input.requirementId, input.payload),
  })
}
