import { ofetch } from 'ofetch'
import {useAuthStore} from "~/stores/auth";

export default defineNuxtPlugin(() => {
    const config = useRuntimeConfig()
    const authStore = useAuthStore()

    const api = ofetch.create({
        baseURL: config.public.apiBase as string,

        async onRequest({ options }) {
            const token = authStore.token

            if (token) {
                options.headers = {
                    ...options.headers,
                    Authorization: `Bearer ${token}`
                }
            }
        },

        async onResponseError({ response }) {
            if (response.status === 401) {
                // Try to refresh token
                const refreshed = await authStore.refreshTokenAction()

                if (!refreshed) {
                    authStore.logout()
                    navigateTo('/login')
                }
            }
        }
    })

    return {
        provide: {
            api
        }
    }
})