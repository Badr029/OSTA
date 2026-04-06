import { useMutation } from '@tanstack/react-query'
import { createWorkCenter } from '../../api/workCenters'
import type { WorkCenterInput } from '../../types/workCenters'

export function useCreateWorkCenter() {
  return useMutation({
    mutationFn: (payload: WorkCenterInput) => createWorkCenter(payload),
  })
}
