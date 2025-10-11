import { ofetch } from 'ofetch'

export default defineNuxtPlugin(() => {
    const config = useRuntimeConfig()

    const api = ofetch.create({
        baseURL: config.public.apiBase,
        onRequest({ options }) {
            const headers = new Headers(options.headers)
            headers.set('Content-Type', 'application/json')

            const token = useAuth().getToken()
            if (token) {
                headers.set('Authorization', `Bearer ${token}`)
            }

            options.headers = headers
        },
        onResponseError({ response }) {
            if (response.status === 401) {
                useAuth().logout()
                navigateTo('/login')
            }
        }
    })

    return {
        provide: {
            api
        }
    }
})