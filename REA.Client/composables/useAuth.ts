export const useAuth = () => {
    const user = ref(null)
    const isAuthenticated = ref(false)

    const login = async (username: string, password: string) => {
        // TODO: Implement login logic
        try {
            console.log('Login attempt:', username)
            // Simulate API call
            const token = 'mock-jwt-token-' + Date.now()
            setToken(token)
            isAuthenticated.value = true
            return true
        } catch (error) {
            console.error('Login failed:', error)
            return false
        }
    }

    const logout = () => {
        user.value = null
        isAuthenticated.value = false
        // Safe client-side check
        if (typeof window !== 'undefined') {
            localStorage.removeItem('auth_token')
        }
    }

    const getToken = (): string | null => {
        // Safe client-side check
        if (typeof window !== 'undefined') {
            return localStorage.getItem('auth_token')
        }
        return null
    }

    const setToken = (token: string) => {
        // Safe client-side check
        if (typeof window !== 'undefined') {
            localStorage.setItem('auth_token', token)
            isAuthenticated.value = true
        }
    }

    // Check if user is logged in on composable initialization
    if (getToken()) {
        isAuthenticated.value = true
    }

    return {
        user,
        isAuthenticated,
        login,
        logout,
        getToken,
        setToken
    }
}