<template>
  <nav class="bg-white shadow-sm border-b">
    <div class="container mx-auto px-6">
      <div class="flex justify-between items-center h-16">
        <div class="flex items-center space-x-8">
          <NuxtLink to="/" class="flex items-center space-x-2">
            <div class="h-10 w-10 bg-primary-600 rounded-full flex items-center justify-center">
              <i class="pi pi-graduation-cap text-xl text-white"></i>
            </div>
            <span class="text-xl font-bold text-primary-600">
              Sistema REA
            </span>
          </NuxtLink>

          <div v-if="authStore.isAuthenticated" class="hidden md:flex space-x-1">
            <NuxtLink
                to="/students"
                class="px-4 py-2 rounded-md text-sm font-medium transition-colors"
                :class="isActive('/students') ? 'bg-primary-100 text-primary-700' : 'text-gray-700 hover:bg-gray-100'"
            >
              <i class="pi pi-users mr-2"></i>
              Estudiantes
            </NuxtLink>

            <NuxtLink
                to="/teachers"
                class="px-4 py-2 rounded-md text-sm font-medium transition-colors"
                :class="isActive('/teachers') ? 'bg-primary-100 text-primary-700' : 'text-gray-700 hover:bg-gray-100'"
            >
              <i class="pi pi-user mr-2"></i>
              Profesores
            </NuxtLink>

            <NuxtLink
                to="/academic-records"
                class="px-4 py-2 rounded-md text-sm font-medium transition-colors"
                :class="isActive('/academic-records') ? 'bg-primary-100 text-primary-700' : 'text-gray-700 hover:bg-gray-100'"
            >
              <i class="pi pi-book mr-2"></i>
              Registros
            </NuxtLink>

            <NuxtLink
                v-if="authStore.isAdmin"
                to="/ocr"
                class="px-4 py-2 rounded-md text-sm font-medium transition-colors"
                :class="isActive('/ocr') ? 'bg-primary-100 text-primary-700' : 'text-gray-700 hover:bg-gray-100'"
            >
              <i class="pi pi-camera mr-2"></i>
              OCR
            </NuxtLink>
          </div>
        </div>

        <div v-if="authStore.isAuthenticated" class="flex items-center space-x-4">
          <div class="hidden sm:block text-right">
            <p class="text-sm font-medium text-gray-700">
              {{ authStore.user?.username }}
            </p>
            <p class="text-xs text-gray-500">
              {{ authStore.user?.role }}
            </p>
          </div>

          <Button
              icon="pi pi-sign-out"
              label="Salir"
              severity="secondary"
              size="small"
              @click="handleLogout"
          />
        </div>
      </div>
    </div>
  </nav>
</template>

<script setup lang="ts">
const authStore = useAuthStore()
const route = useRoute()

const isActive = (path: string) => {
  return route.path.startsWith(path)
}

const handleLogout = async () => {
  if (confirm('¿Está seguro que desea cerrar sesión?')) {
    await authStore.logout()
  }
}
</script>