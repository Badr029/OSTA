import { useMutation } from '@tanstack/react-query'
import { uploadBomPreview } from '../../api/imports'

export function useBomImportPreview() {
  return useMutation({
    mutationFn: uploadBomPreview,
  })
}
