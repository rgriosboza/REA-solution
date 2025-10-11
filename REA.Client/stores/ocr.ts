import { defineStore } from 'pinia'

export const useOCRStore = defineStore('ocr', () => {
    const isLoading = ref(false)
    const result = ref<OCRProcessResponse | null>(null)
    const error = ref<string | null>(null)

    const { $api } = useNuxtApp()

    const processDocument = async (request: OCRProcessRequest) => {
        isLoading.value = true
        error.value = null
        result.value = null

        try {
            const response = await $api.post<OCRProcessResponse>('/ocr/process', request)
            result.value = response.data
            return response.data
        } catch (err: any) {
            error.value = err.response?.data?.message || 'Failed to process document'
            console.error('Error processing document:', err)
            throw err
        } finally {
            isLoading.value = false
        }
    }

    const resetResult = () => {
        result.value = null
        error.value = null
    }

    return {
        isLoading,
        result,
        error,
        processDocument,
        resetResult
    }
})