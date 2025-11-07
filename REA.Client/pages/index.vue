<template>
  <div class="container mx-auto px-4 py-8">
    <div class="bg-white rounded-lg shadow-md p-8">
      <h1 class="text-4xl font-bold text-gray-900 mb-4">
        ¡Bienvenido al Sistema REA!
      </h1>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mt-8">
        <!-- Students Card -->
        <NuxtLink
            to="/students"
            class="block p-6 bg-blue-50 rounded-lg hover:bg-blue-100 transition-colors"
        >
          <div class="flex items-center justify-center mb-4">
            <i class="pi pi-users text-4xl text-blue-600"></i>
          </div>
          <h3 class="text-xl font-semibold text-center text-gray-900">
            Estudiantes
          </h3>
          <p class="text-sm text-gray-600 text-center mt-2">
            Gestionar información de estudiantes
          </p>
        </NuxtLink>

        <!-- Teachers Card -->
        <NuxtLink
            to="/teachers"
            class="block p-6 bg-green-50 rounded-lg hover:bg-green-100 transition-colors"
        >
          <div class="flex items-center justify-center mb-4">
            <i class="pi pi-user text-4xl text-green-600"></i>
          </div>
          <h3 class="text-xl font-semibold text-center text-gray-900">
            Profesores
          </h3>
          <p class="text-sm text-gray-600 text-center mt-2">
            Gestionar profesores
          </p>
        </NuxtLink>

        <!-- Academic Records Card -->
        <NuxtLink
            to="/academic-records"
            class="block p-6 bg-purple-50 rounded-lg hover:bg-purple-100 transition-colors"
        >
          <div class="flex items-center justify-center mb-4">
            <i class="pi pi-book text-4xl text-purple-600"></i>
          </div>
          <h3 class="text-xl font-semibold text-center text-gray-900">
            Registros Académicos
          </h3>
          <p class="text-sm text-gray-600 text-center mt-2">
            Gestionar calificaciones
          </p>
        </NuxtLink>

        <!-- OCR Card -->
        <NuxtLink
            v-if="authStore.isAdmin"
            to="/ocr"
            class="block p-6 bg-orange-50 rounded-lg hover:bg-orange-100 transition-colors"
        >
          <div class="flex items-center justify-center mb-4">
            <i class="pi pi-camera text-4xl text-orange-600"></i>
          </div>
          <h3 class="text-xl font-semibold text-center text-gray-900">
            OCR
          </h3>
          <p class="text-sm text-gray-600 text-center mt-2">
            Escanear documentos
          </p>
        </NuxtLink>
      </div>

      <!-- User Info -->
      <div class="mt-12 p-6 bg-gray-50 rounded-lg">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-600">Usuario activo</p>
            <p class="text-lg font-semibold text-gray-900">
              {{ authStore.user?.username }}
            </p>
            <p class="text-sm text-gray-600">
              {{ authStore.user?.role }}
            </p>
          </div>
          <Button
              label="Cerrar Sesión"
              icon="pi pi-sign-out"
              severity="danger"
              @click="authStore.logout()"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  middleware: ['auth']
})

const authStore = useAuthStore()
</script>