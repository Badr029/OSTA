import { useMutation } from '@tanstack/react-query'
import { updateRoutingOperation } from '../../api/routing'
import type { UpdateRoutingOperationInput } from '../../types/routing'

export function useUpdateRoutingOperation() {
  return useMutation({
    mutationFn: (input: {
      routingTemplateId: string
      operationId: string
      payload: UpdateRoutingOperationInput
    }) => updateRoutingOperation(input.routingTemplateId, input.operationId, input.payload),
  })
}
