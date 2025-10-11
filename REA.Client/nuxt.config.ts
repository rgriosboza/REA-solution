export default defineNuxtConfig({
    compatibilityDate: '2025-10-11',
    devtools: { enabled: true },
    modules: [
        '@nuxtjs/tailwindcss',
        '@pinia/nuxt'
    ],
    css: [
        'primevue/resources/themes/lara-light-blue/theme.css',
        'primevue/resources/primevue.css',
        'primeicons/primeicons.css'
    ],
    build: {
        transpile: ['primevue']
    },
    runtimeConfig: {
        public: {
            apiBase: process.env.API_BASE_URL || 'http://localhost:8080/api'
        }
    },
    typescript: {
        typeCheck: false // Temporarily disable type checking to avoid issues
    }
})