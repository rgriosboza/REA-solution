<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div class="text-center">
        <div class="mx-auto h-16 w-16 bg-primary-600 rounded-full flex items-center justify-center mb-4">
          <i class="pi pi-graduation-cap text-3xl text-white"></i>
        </div>
        <h2 class="text-3xl font-extrabold text-gray-900">
          Sistema REA
        </h2>
        <p class="mt-2 text-sm text-gray-600">
          Monseñor Nicolás Antonio Madrigal
        </p>
      </div>

      <Card class="shadow-2xl">
        <template #content>
          <form @submit.prevent="handleLogin" class="space-y-6">
            <Message severity="warn" :closable="false">
              <div class="text-sm">
                <strong>🔧 Modo Desarrollo:</strong> Cualquier credencial funciona
              </div>
            </Message>

            <Message v-if="error" severity="error" :closable="false">
              {{ error }}
            </Message>

            <div>
              <label for="username" class="block text-sm font-medium text-gray-700 mb-2">
                Usuario o Correo Electrónico
              </label>
              <InputText
                  id="username"
                  v-model="credentials.usernameOrEmail"
                  type="text"
                  placeholder="Cualquier usuario"
                  class="w-full"
                  :disabled="isLoading"
                  autocomplete="username"
              />
            </div>

            <div>
              <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
                Contraseña
              </label>
              <InputText
                  id="password"
                  v-model="credentials.password"
                  type="password"
                  placeholder="Cualquier contraseña"
                  class="w-full"
                  :disabled="isLoading"
                  autocomplete="current-password"
              />
            </div>

            <Button
                type="submit"
                label="Iniciar Sesión"
                icon="pi pi-sign-in"
                class="w-full"
                :loading="isLoading"
                :disabled="isLoading"
            />

            <div class="mt-4 space-y-2">
              <p class="text-xs text-gray-600 font-semibold mb-2">⚡ Acceso Rápido:</p>
              <div class="grid grid-cols-3 gap-2">
                <Button
                    label="Director"
                    severity="info"
                    size="small"
                    @click="quickLogin('Director')"
                    :disabled="isLoading"
                />
                <Button
                    label="Subdirector"
                    severity="info"
                    size="small"
                    @click="quickLogin('VicePrincipal')"
                    :disabled="isLoading"
                />
                <Button
                    label="Profesor"
                    severity="info"
                    size="small"
                    @click="quickLogin('Teacher')"
                    :disabled="isLoading"
                />
              </div>
            </div>
          </form>
        </template>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: false,
  middleware: [] // Remove guest middleware for now
})

const router = useRouter()
const credentials = reactive({
  usernameOrEmail: 'admin',
  password: 'dev123'
})

const error = ref<string | null>(null)
const isLoading = ref(false)

const handleLogin = async () => {
  if (!credentials.usernameOrEmail.trim() || !credentials.password.trim()) {
    error.value = 'Por favor ingrese usuario y contraseña'
    return
  }

  await quickLogin('Director')
}

const quickLogin = async (role: string) => {
  isLoading.value = true
  error.value = null

  try {
    const mockUser = {
      id: 1,
      username: role === 'Director' ? 'admin' : role === 'VicePrincipal' ? 'viceprincipal' : 'teacher',
      email: `${role.toLowerCase()}@reasystem.com`,
      role: role,
      createdAt: new Date().toISOString(),
      lastLogin: new Date().toISOString(),
      isActive: true
    }

    const mockToken = `dev-token-${Date.now()}`

    console.log('🔐 Setting auth data for role:', role)

    // Clear any existing data first
    localStorage.clear()
    sessionStorage.clear()

    // Set dev mode and auth data
    localStorage.setItem('dev_mode', 'true')
    localStorage.setItem('auth_token', mockToken)
    localStorage.setItem('auth_user', JSON.stringify(mockUser))

    // Verify it was set
    const verification = {
      devMode: localStorage.getItem('dev_mode'),
      token: localStorage.getItem('auth_token'),
      user: localStorage.getItem('auth_user')
    }

    console.log('✅ Auth data saved:', {
      devMode: verification.devMode,
      hasToken: !!verification.token,
      hasUser: !!verification.user,
      user: mockUser
    })

    // Wait a moment to ensure localStorage is written
    await new Promise(resolve => setTimeout(resolve, 100))

    console.log('🔄 Redirecting to home...')

    // Use window.location for a full page reload
    window.location.href = '/'

  } catch (err: any) {
    console.error('❌ Login error:', err)
    error.value = 'Error al iniciar sesión'
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.p-card {
  background: white;
  border-radius: 0.5rem;
}
</style>