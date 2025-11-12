// middleware/auth.ts
import { useAuthStore } from "~/stores/auth";

export default defineNuxtRouteMiddleware((to, from) => {
    const authStore = useAuthStore()

    // Check for dev mode in client-side
    const isDevMode = process.client && localStorage.getItem('dev_mode') === 'true'

    // If in dev mode and has basic auth data, allow access
    if (isDevMode) {
        const hasToken = process.client && localStorage.getItem('auth_token')
        const hasUser = process.client && localStorage.getItem('auth_user')

        if (hasToken && hasUser) {
            // Initialize auth store if not already done
            if (!authStore.isAuthenticated) {
                authStore.initAuth()
            }
            return // Allow access
        }
    }

    // Check if user is authenticated (normal flow)
    if (!authStore.isAuthenticated) {
        return navigateTo('/login')
    }

    // Optional: Check for specific role permissions
    const requiredRoles = to.meta.requiredRoles as string[]
    if (requiredRoles && !authStore.hasRole(requiredRoles)) {
        throw createError({
            statusCode: 403,
            statusMessage: 'Unauthorized Access'
        })
    }
})