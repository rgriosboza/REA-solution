import {useAuthStore} from "~/stores/auth";

export default defineNuxtRouteMiddleware((to, from) => {
    const authStore = useAuthStore()

    // Check if user is authenticated
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