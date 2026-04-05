import { useMutation } from '@tanstack/react-query'
import { createRoutingOperation } from '../../api/routing'
import type { CreateRoutingOperationInput } from '../../types/routing'

export function useCreateRoutingOperation() {
  return useMutation({
    mutationFn: (input: { routingTemplateId: string; payload: CreateRoutingOperationInput }) =>
      createRoutingOperation(input.routingTemplateId, input.payload),
  })
}
