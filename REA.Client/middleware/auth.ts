// middleware/auth.ts
import { useAuthStore } from "~/stores/auth";

export default defineNuxtRouteMiddleware((to, from) => {
    // Only run on client side
    if (!process.client) {
        return
    }

    const authStore = useAuthStore()

    // Check for dev mode
    const isDevMode = localStorage.getItem('dev_mode') === 'true'
    const hasToken = localStorage.getItem('auth_token')
    const hasUser = localStorage.getItem('auth_user')

    console.log('🔐 Auth middleware check:', {
        path: to.path,
        isDevMode,
        hasToken: !!hasToken,
        hasUser: !!hasUser,
        storeAuthenticated: authStore.isAuthenticated
    })

    // If in dev mode and has basic auth data, ensure store is initialized
    if (isDevMode && hasToken && hasUser) {
        if (!authStore.isAuthenticated) {
            console.log('🔄 Initializing auth store from localStorage...')
            authStore.initAuth()
        }
        // Allow access
        return
    }

    // Normal authentication check
    if (!authStore.isAuthenticated) {
        console.log('❌ Not authenticated, redirecting to login')
        return navigateTo('/login')
    }

    // Optional: Check for specific role permissions
    const requiredRoles = to.meta.requiredRoles as string[]
    if (requiredRoles && !authStore.hasRole(requiredRoles)) {
        console.log('❌ Insufficient permissions')
        throw createError({
            statusCode: 403,
            statusMessage: 'Unauthorized Access'
        })
    }
})