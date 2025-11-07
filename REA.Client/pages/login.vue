<template>
  <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <!-- Logo and Title -->
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

      <!-- Login Card -->
      <Card class="shadow-2xl">
        <template #content>
          <form @submit.prevent="handleLogin" class="space-y-6">
            <!-- Error Message -->
            <Message v-if="error" severity="error" :closable="false">
              {{ error }}
            </Message>

            <!-- Success Message -->
            <Message v-if="successMessage" severity="success" :closable="false">
              {{ successMessage }}
            </Message>

            <!-- Username/Email Field -->
            <div>
              <label for="username" class="block text-sm font-medium text-gray-700 mb-2">
                Usuario o Correo Electrónico
              </label>
              <InputText
                  id="username"
                  v-model="credentials.usernameOrEmail"
                  type="text"
                  placeholder="admin / admin@reasystem.com"
                  class="w-full"
                  :class="{ 'p-invalid': errors.usernameOrEmail }"
                  :disabled="isLoading"
                  autocomplete="username"
              />
              <small v-if="errors.usernameOrEmail" class="p-error">
                {{ errors.usernameOrEmail }}
              </small>
            </div>

            <!-- Password Field -->
            <div>
              <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
                Contraseña
              </label>
              <div class="relative">
                <InputText
                    id="password"
                    v-model="credentials.password"
                    :type="showPassword ? 'text' : 'password'"
                    placeholder="••••••••"
                    class="w-full pr-10"
                    :class="{ 'p-invalid': errors.password }"
                    :disabled="isLoading"
                    autocomplete="current-password"
                />
                <button
                    type="button"
                    @click="showPassword = !showPassword"
                    class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700"
                    tabindex="-1"
                >
                  <i :class="showPassword ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
                </button>
              </div>
              <small v-if="errors.password" class="p-error">
                {{ errors.password }}
              </small>
            </div>

            <!-- Remember Me -->
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <input
                    id="remember-me"
                    v-model="rememberMe"
                    type="checkbox"
                    class="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                >
                <label for="remember-me" class="ml-2 block text-sm text-gray-700">
                  Recordarme
                </label>
              </div>
            </div>

            <!-- Submit Button -->
            <Button
                type="submit"
                label="Iniciar Sesión"
                icon="pi pi-sign-in"
                class="w-full"
                :loading="isLoading"
                :disabled="isLoading"
            />

            <!-- Demo Credentials -->
            <div class="mt-4 p-4 bg-blue-50 rounded-lg">
              <p class="text-xs text-blue-800 font-semibold mb-2">🔐 Credenciales de Prueba:</p>
              <div class="space-y-1 text-xs text-blue-700">
                <p><strong>Director:</strong> admin / Admin123!</p>
                <p><strong>Subdirector:</strong> viceprincipal / Admin123!</p>
                <p><strong>Profesor:</strong> teacher / Admin123!</p>
              </div>
            </div>
          </form>
        </template>
      </Card>

      <!-- Footer -->
      <div class="text-center text-sm text-gray-600">
        <p>© 2024 Sistema REA. Todos los derechos reservados.</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: false,
  middleware: ['guest']
})

const authStore = useAuthStore()
const router = useRouter()

const credentials = reactive({
  usernameOrEmail: '',
  password: ''
})

const errors = reactive({
  usernameOrEmail: '',
  password: ''
})

const error = ref<string | null>(null)
const successMessage = ref<string | null>(null)
const isLoading = ref(false)
const showPassword = ref(false)
const rememberMe = ref(false)

const validateForm = (): boolean => {
  let isValid = true

  // Reset errors
  errors.usernameOrEmail = ''
  errors.password = ''
  error.value = null

  if (!credentials.usernameOrEmail.trim()) {
    errors.usernameOrEmail = 'Usuario o correo es requerido'
    isValid = false
  }

  if (!credentials.password) {
    errors.password = 'Contraseña es requerida'
    isValid = false
  }

  return isValid
}

const handleLogin = async () => {
  if (!validateForm()) return

  isLoading.value = true
  error.value = null

  try {
    const success = await authStore.login(
        credentials.usernameOrEmail,
        credentials.password,
        rememberMe.value
    )

    if (success) {
      successMessage.value = '¡Login exitoso! Redirigiendo...'

      setTimeout(() => {
        router.push('/')
      }, 1000)
    }
  } catch (err: any) {
    error.value = err.message || 'Error al iniciar sesión. Por favor, intente nuevamente.'
  } finally {
    isLoading.value = false
  }
}

// Auto-fill demo credentials (development only)
onMounted(() => {
  if (process.dev) {
    credentials.usernameOrEmail = 'admin'
    credentials.password = 'Admin123!'
  }
})
</script>

<style scoped>
/* Additional custom styles if needed */
.p-card {
  background: white;
  border-radius: 0.5rem;
}
</style>