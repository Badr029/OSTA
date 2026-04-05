import { useQuery } from '@tanstack/react-query'
import { getWorkCenterQueue } from '../../api/workCenters'

export function useWorkCenterQueue(workCenterId: string | undefined) {
  return useQuery({
    queryKey: ['work-center-queue', workCenterId],
    queryFn: () => getWorkCenterQueue(workCenterId!),
    enabled: Boolean(workCenterId),
  })
}
