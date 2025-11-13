<template>
  <div class="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center">
    <input
        ref="fileInput"
        type="file"
        accept="image/*"
        @change="handleFileSelect"
        class="hidden"
    />

    <div v-if="!previewUrl" class="space-y-4">
      <i class="pi pi-cloud-upload text-4xl text-gray-400"></i>
      <p class="text-gray-600">Upload a document image</p>
      <Button
          label="Choose File"
          icon="pi pi-image"
          @click="fileInput?.click()"
      />
    </div>

    <div v-else class="space-y-4">
      <img :src="previewUrl" alt="Preview" class="max-h-64 mx-auto rounded" />
      <div class="flex justify-center space-x-2">
        <Button
            label="Process"
            icon="pi pi-cog"
            :loading="isLoading"
            @click="processImage"
        />
        <Button
            label="Change"
            severity="secondary"
            @click="clearImage"
        />
      </div>
    </div>

    <ProgressBar v-if="isLoading" mode="indeterminate" class="mt-4" />
  </div>
</template>

<script setup lang="ts">
import type {OCRProcessRequest} from "~/types/api";

const fileInput = ref<HTMLInputElement>()
const previewUrl = ref<string>('')
const selectedFile = ref<File | null>(null)

const ocrStore = useOCRStore()
const { isLoading } = storeToRefs(ocrStore)

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]

  if (file && file.type.startsWith('image/')) {
    selectedFile.value = file
    previewUrl.value = URL.createObjectURL(file)
  }
}

const processImage = async () => {
  if (!selectedFile.value) return

  try {
    const base64 = await fileToBase64(selectedFile.value)
    const request: OCRProcessRequest = {
      imageBase64: base64,
      documentType: 'GradeRecord' // You can make this dynamic
    }

    await ocrStore.processDocument(request)
  } catch (error) {
    console.error('Error processing image:', error)
  }
}

const clearImage = () => {
  previewUrl.value = ''
  selectedFile.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
  ocrStore.resetResult()
}

const fileToBase64 = (file: File): Promise<string> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.readAsDataURL(file)
    reader.onload = () => {
      const base64 = reader.result as string
      // Remove data:image/...;base64, prefix
      resolve(base64.split(',')[1])
    }
    reader.onerror = error => reject(error)
  })
}
</script>