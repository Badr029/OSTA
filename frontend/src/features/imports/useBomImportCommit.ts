import { useMutation } from '@tanstack/react-query'
import { uploadBomCommit } from '../../api/imports'

export function useBomImportCommit() {
  return useMutation({
    mutationFn: uploadBomCommit,
  })
}
