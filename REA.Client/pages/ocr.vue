<template>
  <div class="container mx-auto px-4 py-8">
    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900 mb-2">
        <i class="pi pi-camera mr-3"></i>
        Procesamiento OCR de Documentos
      </h1>
      <p class="text-gray-600">
        Sube una imagen de un documento académico para extraer y procesar la información automáticamente
      </p>
    </div>

    <!-- Upload Section -->
    <Card class="mb-6">
      <template #content>
        <div class="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center hover:border-primary-500 transition-colors">
          <input
              ref="fileInput"
              type="file"
              accept="image/*,.pdf"
              @change="handleFileSelect"
              class="hidden"
          />

          <div v-if="!previewUrl" class="space-y-4">
            <div class="flex justify-center">
              <div class="h-20 w-20 bg-primary-50 rounded-full flex items-center justify-center">
                <i class="pi pi-cloud-upload text-4xl text-primary-600"></i>
              </div>
            </div>
            <div>
              <p class="text-lg font-medium text-gray-900 mb-1">
                Selecciona un documento para procesar
              </p>
              <p class="text-sm text-gray-500">
                Formatos soportados: JPG, PNG, PDF (máx. 5MB)
              </p>
            </div>
            <Button
                label="Seleccionar Archivo"
                icon="pi pi-image"
                @click="fileInput?.click()"
                class="mt-4"
            />
          </div>

          <div v-else class="space-y-4">
            <!-- Image Preview -->
            <div class="mb-4">
              <img
                  :src="previewUrl"
                  alt="Preview"
                  class="max-h-96 mx-auto rounded-lg shadow-md border-2 border-gray-200"
              />
            </div>

            <!-- File Info -->
            <div class="bg-gray-50 rounded-lg p-4 mb-4">
              <div class="flex items-center justify-between text-sm">
                <div class="flex items-center space-x-2">
                  <i class="pi pi-file text-primary-600"></i>
                  <span class="font-medium text-gray-900">{{ fileName }}</span>
                </div>
                <span class="text-gray-500">{{ fileSize }}</span>
              </div>
            </div>

            <!-- Action Buttons -->
            <div class="flex justify-center space-x-3">
              <Button
                  label="Procesar Documento"
                  icon="pi pi-cog"
                  :loading="isLoading"
                  @click="processImage"
                  :disabled="isLoading"
              />
              <Button
                  label="Cambiar Imagen"
                  icon="pi pi-refresh"
                  severity="secondary"
                  @click="clearImage"
                  :disabled="isLoading"
              />
            </div>
          </div>

          <!-- Progress Bar -->
          <div v-if="isLoading" class="mt-6">
            <ProgressBar mode="indeterminate" class="h-2" />
            <p class="text-sm text-gray-600 mt-2">
              Procesando documento con Google Vision API...
            </p>
          </div>
        </div>
      </template>
    </Card>

    <!-- Results Section -->
    <div v-if="result || error" class="space-y-6">
      <!-- Error Message -->
      <Message v-if="error" severity="error" :closable="true" @close="error = null">
        <div class="flex items-start">
          <i class="pi pi-times-circle text-xl mr-3"></i>
          <div>
            <p class="font-semibold">Error al procesar el documento</p>
            <p class="text-sm mt-1">{{ error }}</p>
          </div>
        </div>
      </Message>

      <!-- Success Results -->
      <div v-if="result && result.success">
        <!-- Extracted Text -->
        <Card>
          <template #header>
            <div class="p-4 border-b bg-green-50">
              <div class="flex items-center justify-between">
                <div class="flex items-center space-x-2">
                  <i class="pi pi-check-circle text-2xl text-green-600"></i>
                  <h3 class="text-xl font-semibold text-gray-900">
                    Texto Extraído
                  </h3>
                </div>
                <Button
                    icon="pi pi-copy"
                    label="Copiar"
                    size="small"
                    severity="secondary"
                    @click="copyToClipboard(result.extractedText || '')"
                />
              </div>
            </div>
          </template>
          <template #content>
            <div class="bg-gray-50 rounded-lg p-4 font-mono text-sm whitespace-pre-wrap border border-gray-200">
              {{ result.extractedText || 'No se extrajo texto' }}
            </div>
          </template>
        </Card>

        <!-- Parsed Data -->
        <Card v-if="result.data">
          <template #header>
            <div class="p-4 border-b bg-blue-50">
              <div class="flex items-center space-x-2">
                <i class="pi pi-database text-2xl text-blue-600"></i>
                <h3 class="text-xl font-semibold text-gray-900">
                  Datos Procesados
                </h3>
              </div>
            </div>
          </template>
          <template #content>
            <div class="space-y-4">
              <!-- Student Info -->
              <div v-if="result.data.student" class="bg-white p-4 rounded-lg border">
                <h4 class="font-semibold text-gray-900 mb-3 flex items-center">
                  <i class="pi pi-user mr-2 text-blue-600"></i>
                  Información del Estudiante
                </h4>
                <div class="grid grid-cols-2 gap-3 text-sm">
                  <div v-if="result.data.student.fullName">
                    <span class="text-gray-500">Nombre:</span>
                    <p class="font-medium">{{ result.data.student.fullName }}</p>
                  </div>
                  <div v-if="result.data.student.studentId">
                    <span class="text-gray-500">ID:</span>
                    <p class="font-medium">{{ result.data.student.studentId }}</p>
                  </div>
                  <div v-if="result.data.student.grade">
                    <span class="text-gray-500">Grado:</span>
                    <p class="font-medium">{{ result.data.student.grade }}</p>
                  </div>
                  <div v-if="result.data.student.section">
                    <span class="text-gray-500">Sección:</span>
                    <p class="font-medium">{{ result.data.student.section }}</p>
                  </div>
                </div>
              </div>

              <!-- Academic Info -->
              <div v-if="result.data.academic" class="bg-white p-4 rounded-lg border">
                <h4 class="font-semibold text-gray-900 mb-3 flex items-center">
                  <i class="pi pi-book mr-2 text-green-600"></i>
                  Información Académica
                </h4>
                <div class="grid grid-cols-2 gap-3 text-sm">
                  <div v-if="result.data.academic.subject">
                    <span class="text-gray-500">Materia:</span>
                    <p class="font-medium">{{ result.data.academic.subject }}</p>
                  </div>
                  <div v-if="result.data.academic.score">
                    <span class="text-gray-500">Calificación:</span>
                    <p class="font-medium">{{ result.data.academic.score }}</p>
                  </div>
                  <div v-if="result.data.academic.period">
                    <span class="text-gray-500">Período:</span>
                    <p class="font-medium">{{ result.data.academic.period }}</p>
                  </div>
                  <div v-if="result.data.academic.schoolYear">
                    <span class="text-gray-500">Año Escolar:</span>
                    <p class="font-medium">{{ result.data.academic.schoolYear }}</p>
                  </div>
                </div>
              </div>

              <!-- Additional Fields -->
              <div v-if="result.data.additionalFields && Object.keys(result.data.additionalFields).length > 0"
                   class="bg-white p-4 rounded-lg border">
                <h4 class="font-semibold text-gray-900 mb-3 flex items-center">
                  <i class="pi pi-info-circle mr-2 text-orange-600"></i>
                  Campos Adicionales
                </h4>
                <div class="space-y-2 text-sm">
                  <div v-for="(value, key) in result.data.additionalFields" :key="key">
                    <span class="text-gray-500">{{ key }}:</span>
                    <span class="font-medium ml-2">{{ value }}</span>
                  </div>
                </div>
              </div>
            </div>
          </template>
        </Card>

        <!-- Actions -->
        <div class="flex justify-end space-x-3">
          <Button
              label="Procesar Otro Documento"
              icon="pi pi-refresh"
              severity="secondary"
              @click="resetAll"
          />
          <Button
              label="Guardar en Sistema"
              icon="pi pi-save"
              @click="saveToSystem"
              v-if="result.data"
          />
        </div>
      </div>
    </div>

    <!-- Help Section -->
    <Card class="mt-8 bg-blue-50 border-blue-200">
      <template #content>
        <div class="flex items-start space-x-4">
          <i class="pi pi-info-circle text-3xl text-blue-600 mt-1"></i>
          <div>
            <h3 class="font-semibold text-gray-900 mb-2">
              Consejos para mejores resultados
            </h3>
            <ul class="text-sm text-gray-700 space-y-1">
              <li>• Asegúrate de que el documento esté bien iluminado</li>
              <li>• Usa imágenes con alta resolución</li>
              <li>• Evita sombras y reflejos</li>
              <li>• El texto debe estar claramente visible</li>
              <li>• Formatos recomendados: JPG, PNG</li>
            </ul>
          </div>
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import {useAuthStore} from "~/stores/auth";

definePageMeta({
  middleware: defineNuxtRouteMiddleware((to, from) => {
    const authStore = useAuthStore()

    if (!authStore.isAuthenticated) {
      return navigateTo('/login')
    }
  })
})
const fileInput = ref<HTMLInputElement>()
const previewUrl = ref<string>('')
const selectedFile = ref<File | null>(null)
const fileName = ref<string>('')
const fileSize = ref<string>('')
const isLoading = ref(false)
const result = ref<any>(null)
const error = ref<string | null>(null)

const { $api } = useNuxtApp()

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]

  if (file) {
    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      error.value = 'El archivo es demasiado grande. Máximo 5MB.'
      return
    }

    // Validate file type
    const validTypes = ['image/jpeg', 'image/png', 'image/jpg', 'application/pdf']
    if (!validTypes.includes(file.type)) {
      error.value = 'Formato no soportado. Use JPG, PNG o PDF.'
      return
    }

    selectedFile.value = file
    fileName.value = file.name
    fileSize.value = formatFileSize(file.size)
    previewUrl.value = URL.createObjectURL(file)
    error.value = null
  }
}

const processImage = async () => {
  if (!selectedFile.value) return

  isLoading.value = true
  error.value = null
  result.value = null

  try {
    const base64 = await fileToBase64(selectedFile.value)

    const response = await $api('/ocr/process', {
      method: 'POST',
      body: {
        imageBase64: base64,
        documentType: 'AcademicRecord'
      }
    })

    result.value = response

    if (!response.success) {
      error.value = response.error || 'Error al procesar el documento'
    }
  } catch (err: any) {
    console.error('Error processing image:', err)
    error.value = err.data?.message || err.message || 'Error al procesar el documento'
  } finally {
    isLoading.value = false
  }
}

const clearImage = () => {
  previewUrl.value = ''
  selectedFile.value = null
  fileName.value = ''
  fileSize.value = ''
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

const resetAll = () => {
  clearImage()
  result.value = null
  error.value = null
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

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
}

const copyToClipboard = async (text: string) => {
  try {
    await navigator.clipboard.writeText(text)
    // Show success message (you can add a toast notification here)
    alert('Texto copiado al portapapeles')
  } catch (err) {
    console.error('Failed to copy:', err)
  }
}

const saveToSystem = async () => {
  // TODO: Implement save functionality
  alert('Funcionalidad de guardado en desarrollo')
}
</script>

<style scoped>
.p-card {
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
}
</style>