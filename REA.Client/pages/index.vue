<template>
  <div class="container mx-auto px-4 py-8">
    <!-- Loading State -->
    <div v-if="isChecking" class="flex items-center justify-center min-h-screen">
      <div class="text-center">
        <ProgressSpinner />
        <p class="mt-4 text-gray-600">Cargando...</p>
      </div>
    </div>

    <!-- Main Content -->
    <div v-else class="bg-white rounded-lg shadow-md p-8">
      <h1 class="text-4xl font-bold text-gray-900 mb-4">
        ¡Bienvenido al Sistema REA!
      </h1>

      <div  class="space-y-6">
        <!-- User Info Card -->
        <div class="p-6 bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-600">Usuario activo</p>
              <p class="text-2xl font-bold text-gray-900">
                {{ currentUser?.username || 'Usuario' }}
              </p>
              <p class="text-sm text-gray-600 mt-1">
                Rol: <span class="font-semibold">{{ currentUser?.role || 'N/A' }}</span>
              </p>
              <p class="text-xs text-green-600 mt-1" v-if="isDevMode">
                ✅ Modo Desarrollo Activo
              </p>
            </div>
            <Button
                label="Cerrar Sesión"
                icon="pi pi-sign-out"
                severity="danger"
                @click="handleLogout"
            />
          </div>
        </div>

        <!-- Navigation Cards -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mt-8">
          <NuxtLink
              to="/students"
              class="block p-6 bg-blue-50 rounded-lg hover:bg-blue-100 transition-colors shadow-sm hover:shadow-md"
          >
            <div class="flex items-center justify-center mb-4">
              <i class="pi pi-users text-5xl text-blue-600"></i>
            </div>
            <h3 class="text-xl font-semibold text-center text-gray-900">
              Estudiantes
            </h3>
            <p class="text-sm text-gray-600 text-center mt-2">
              Gestionar información de estudiantes
            </p>
          </NuxtLink>

          <NuxtLink
              to="/teachers"
              class="block p-6 bg-green-50 rounded-lg hover:bg-green-100 transition-colors shadow-sm hover:shadow-md"
          >
            <div class="flex items-center justify-center mb-4">
              <i class="pi pi-user text-5xl text-green-600"></i>
            </div>
            <h3 class="text-xl font-semibold text-center text-gray-900">
              Profesores
            </h3>
            <p class="text-sm text-gray-600 text-center mt-2">
              Gestionar profesores
            </p>
          </NuxtLink>

          <NuxtLink
              to="/academic-records"
              class="block p-6 bg-purple-50 rounded-lg hover:bg-purple-100 transition-colors shadow-sm hover:shadow-md"
          >
            <div class="flex items-center justify-center mb-4">
              <i class="pi pi-book text-5xl text-purple-600"></i>
            </div>
            <h3 class="text-xl font-semibold text-center text-gray-900">
              Registros Académicos
            </h3>
            <p class="text-sm text-gray-600 text-center mt-2">
              Gestionar calificaciones
            </p>
          </NuxtLink>

          <NuxtLink
              to="/ocr"
              class="block p-6 bg-orange-50 rounded-lg hover:bg-orange-100 transition-colors shadow-sm hover:shadow-md"
          >
            <div class="flex items-center justify-center mb-4">
              <i class="pi pi-camera text-5xl text-orange-600"></i>
            </div>
            <h3 class="text-xl font-semibold text-center text-gray-900">
              OCR
            </h3>
            <p class="text-sm text-gray-600 text-center mt-2">
              Escanear documentos
            </p>
          </NuxtLink>
        </div>

        <!-- Quick Actions -->
        <div class="mt-8 grid grid-cols-1 md:grid-cols-3 gap-4">
          <Card class="bg-gradient-to-r from-blue-500 to-blue-600 text-white">
            <template #content>
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm opacity-90">Total Estudiantes</p>
                  <p class="text-3xl font-bold">156</p>
                </div>
                <i class="pi pi-users text-4xl opacity-50"></i>
              </div>
            </template>
          </Card>

          <Card class="bg-gradient-to-r from-green-500 to-green-600 text-white">
            <template #content>
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm opacity-90">Profesores Activos</p>
                  <p class="text-3xl font-bold">24</p>
                </div>
                <i class="pi pi-user text-4xl opacity-50"></i>
              </div>
            </template>
          </Card>

          <Card class="bg-gradient-to-r from-purple-500 to-purple-600 text-white">
            <template #content>
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm opacity-90">Registros Este Mes</p>
                  <p class="text-3xl font-bold">89</p>
                </div>
                <i class="pi pi-book text-4xl opacity-50"></i>
              </div>
            </template>
          </Card>
        </div>
      </div>

      <!-- Not authenticated fallback -->
<!--      <div v-else class="text-center py-12">-->
<!--        <i class="pi pi-lock text-6xl text-gray-400 mb-4"></i>-->
<!--        <p class="text-xl text-gray-600 mb-4">-->
<!--          Por favor inicia sesión para continuar-->
<!--        </p>-->
<!--        <Button-->
<!--            label="Ir al Login"-->
<!--            icon="pi pi-sign-in"-->
<!--            size="large"-->
<!--            @click="router.push('/login')"-->
<!--        />-->
<!--      </div>-->
    </div>
  </div>
</template>

<script setup lang="ts">
const router = useRouter()

// State
const isChecking = ref(true)
const isLoggedIn = ref(false)
const currentUser = ref<any>(null)
const isDevMode = ref(false)

// Check auth status directly from localStorage
const checkAuth = () => {
  console.log('🔍 Checking authentication...')

  if (!process.client) {
    console.log('⏳ Server-side, skipping auth check')
    return
  }

  const token = localStorage.getItem('auth_token')
  const userStr = localStorage.getItem('auth_user')
  const devModeFlag = localStorage.getItem('dev_mode') === 'true'

  console.log('📦 Auth data from localStorage:', {
    hasToken: !!token,
    hasUser: !!userStr,
    devMode: devModeFlag,
    token: token?.substring(0, 20) + '...'
  })

  isDevMode.value = devModeFlag

  if (token && userStr) {
    try {
      currentUser.value = JSON.parse(userStr)
      isLoggedIn.value = true

      console.log('✅ User authenticated:', {
        username: currentUser.value?.username,
        role: currentUser.value?.role,
        devMode: devModeFlag
      })
    } catch (e) {
      console.error('❌ Error parsing user data:', e)
      isLoggedIn.value = false
    }
  } else {
    console.log('❌ No auth data found in localStorage')
    isLoggedIn.value = false
  }
}

// Initialize on client side only
onMounted(() => {
  console.log('🏠 Index page mounted')

  // Wait a bit for client-side hydration to complete
  setTimeout(() => {
    checkAuth()
    isChecking.value = false
  }, 200)
})

const handleLogout = () => {
  if (confirm('¿Está seguro que desea cerrar sesión?')) {
    console.log('🚪 Logging out...')

    // Clear all auth data
    localStorage.removeItem('auth_token')
    localStorage.removeItem('auth_user')
    localStorage.removeItem('auth_refresh_token')
    localStorage.removeItem('dev_mode')
    sessionStorage.clear()

    // Update local state
    isLoggedIn.value = false
    currentUser.value = null

    // Redirect to login
    router.push('/login')
  }
}

definePageMeta({
  middleware: [] // No middleware, we handle auth manually
})
</script>