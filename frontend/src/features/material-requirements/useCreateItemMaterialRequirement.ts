import { useMutation } from '@tanstack/react-query'
import { createItemMaterialRequirement } from '../../api/materialRequirements'
import type { CreateItemMaterialRequirementInput } from '../../types/materialRequirements'

export function useCreateItemMaterialRequirement() {
  return useMutation({
    mutationFn: (input: { itemMasterId: string; payload: CreateItemMaterialRequirementInput }) =>
      createItemMaterialRequirement(input.itemMasterId, input.payload),
  })
}
