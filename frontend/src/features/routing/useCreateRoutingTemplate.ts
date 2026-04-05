import { useMutation } from '@tanstack/react-query'
import { createRoutingTemplate } from '../../api/routing'
import type { CreateRoutingTemplateInput } from '../../types/routing'

export function useCreateRoutingTemplate() {
  return useMutation({
    mutationFn: (input: CreateRoutingTemplateInput) => createRoutingTemplate(input),
  })
}
