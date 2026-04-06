import { useMutation } from '@tanstack/react-query'
import { deleteRoutingOperation } from '../../api/routing'

export function useDeleteRoutingOperation() {
  return useMutation({
    mutationFn: (input: { routingTemplateId: string; operationId: string }) =>
      deleteRoutingOperation(input.routingTemplateId, input.operationId),
  })
}
