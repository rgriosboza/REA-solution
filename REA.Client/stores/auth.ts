// stores/auth.ts
import { defineStore } from 'pinia'

export interface AuthUser {
    id: number
    username: string
    email: string
    role: 'Director' | 'VicePrincipal' | 'Teacher'
    createdAt: string
    lastLogin?: string
    isActive: boolean
}

export const useAuthStore = defineStore('auth', () => {
    const router = useRouter()

    // State
    const user = ref<AuthUser | null>(null)
    const token = ref<string | null>(null)
    const refreshToken = ref<string | null>(null)
    const isLoading = ref(false)
    const error = ref<string | null>(null)

    // Getters
    const isAuthenticated = computed(() => !!token.value && !!user.value)
    const userRole = computed(() => user.value?.role || null)
    const isDirector = computed(() => user.value?.role === 'Director')
    const isVicePrincipal = computed(() => user.value?.role === 'VicePrincipal')
    const isTeacher = computed(() => user.value?.role === 'Teacher')
    const isAdmin = computed(() => isDirector.value || isVicePrincipal.value)

    // Actions
    const login = async (usernameOrEmail: string, password: string, rememberMe = false) => {
        isLoading.value = true
        error.value = null

        try {
            const { $api } = useNuxtApp()
            const response = await $api<any>('/auth/login', {
                method: 'POST',
                body: {
                    usernameOrEmail,
                    password
                }
            })

            if (response.success && response.token && response.user) {
                setAuth(response.token, response.refreshToken, response.user, rememberMe)
                return true
            } else {
                throw new Error(response.message || 'Login failed')
            }
        } catch (err: any) {
            error.value = err.data?.message || err.message || 'Error al iniciar sesión'
            return false
        } finally {
            isLoading.value = false
        }
    }

    const logout = async () => {
        try {
            const { $api } = useNuxtApp()
            await $api('/auth/logout', { method: 'POST' })
        } catch (err) {
            console.error('Logout API call failed:', err)
        } finally {
            clearAuth()
            if (process.client) {
                window.location.href = '/login'
            } else {
                router.push('/login')
            }
        }
    }

    const setAuth = (
        authToken: string,
        authRefreshToken: string | null,
        authUser: AuthUser,
        rememberMe = false
    ) => {
        token.value = authToken
        refreshToken.value = authRefreshToken
        user.value = authUser

        if (process.client) {
            const storage = rememberMe ? localStorage : sessionStorage
            storage.setItem('auth_token', authToken)
            if (authRefreshToken) {
                storage.setItem('auth_refresh_token', authRefreshToken)
            }
            storage.setItem('auth_user', JSON.stringify(authUser))

            console.log('✅ Auth data saved:', {
                hasToken: !!authToken,
                hasUser: !!authUser,
                username: authUser.username
            })
        }
    }

    const clearAuth = () => {
        token.value = null
        refreshToken.value = null
        user.value = null

        if (process.client) {
            localStorage.removeItem('auth_token')
            localStorage.removeItem('auth_refresh_token')
            localStorage.removeItem('auth_user')
            localStorage.removeItem('dev_mode')
            sessionStorage.removeItem('auth_token')
            sessionStorage.removeItem('auth_refresh_token')
            sessionStorage.removeItem('auth_user')
        }
    }

    const initAuth = () => {
        if (!process.client) return

        console.log('🔄 Initializing auth...')

        // Try localStorage first (remember me), then sessionStorage
        const storedToken = localStorage.getItem('auth_token') || sessionStorage.getItem('auth_token')
        const storedRefreshToken = localStorage.getItem('auth_refresh_token') || sessionStorage.getItem('auth_refresh_token')
        const storedUser = localStorage.getItem('auth_user') || sessionStorage.getItem('auth_user')

        console.log('📦 Found auth data:', {
            hasToken: !!storedToken,
            hasRefreshToken: !!storedRefreshToken,
            hasUser: !!storedUser
        })

        if (storedToken && storedUser) {
            try {
                const parsedUser = JSON.parse(storedUser)

                // In dev mode, skip token expiry check
                const isDevMode = localStorage.getItem('dev_mode') === 'true'

                if (isDevMode) {
                    console.log('🔧 Dev mode: Accepting stored auth without validation')
                    token.value = storedToken
                    refreshToken.value = storedRefreshToken
                    user.value = parsedUser
                    console.log('✅ Auth initialized successfully')
                    return
                }

                // Production mode: verify token hasn't expired
                const tokenData = parseJwt(storedToken)
                if (tokenData && tokenData.exp * 1000 > Date.now()) {
                    token.value = storedToken
                    refreshToken.value = storedRefreshToken
                    user.value = parsedUser
                    console.log('✅ Auth initialized successfully')
                } else {
                    console.log('⏰ Token expired')
                    clearAuth()
                }
            } catch (error) {
                console.error('❌ Error parsing auth data:', error)
                clearAuth()
            }
        } else {
            console.log('❌ No auth data found')
        }
    }

    const parseJwt = (token: string) => {
        try {
            const base64Url = token.split('.')[1]
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
            const jsonPayload = decodeURIComponent(
                atob(base64)
                    .split('')
                    .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                    .join('')
            )
            return JSON.parse(jsonPayload)
        } catch {
            return null
        }
    }

    const hasRole = (requiredRole: string | string[]): boolean => {
        if (!user.value) return false
        const roles = Array.isArray(requiredRole) ? requiredRole : [requiredRole]
        return roles.includes(user.value.role)
    }

    const hasPermission = (permission: string): boolean => {
        if (!user.value) return false

        const rolePermissions: Record<string, string[]> = {
            Director: ['all'],
            VicePrincipal: ['students.manage', 'teachers.view', 'records.manage', 'payments.manage'],
            Teacher: ['students.view', 'records.create', 'records.view']
        }

        const userPermissions = rolePermissions[user.value.role] || []
        return userPermissions.includes('all') || userPermissions.includes(permission)
    }

    // Initialize auth on store creation (only on client)
    if (process.client) {
        initAuth()
    }

    return {
        // State
        user: readonly(user),
        token: readonly(token),
        isLoading: readonly(isLoading),
        error: readonly(error),

        // Getters
        isAuthenticated,
        userRole,
        isDirector,
        isVicePrincipal,
        isTeacher,
        isAdmin,

        // Actions
        login,
        logout,
        setAuth,
        clearAuth,
        hasRole,
        hasPermission,
        initAuth
    }
})