import { useMutation } from '@tanstack/react-query'
import { updateWorkCenter } from '../../api/workCenters'
import type { WorkCenterInput } from '../../types/workCenters'

export function useUpdateWorkCenter() {
  return useMutation({
    mutationFn: (input: { id: string; payload: WorkCenterInput }) => updateWorkCenter(input.id, input.payload),
  })
}
