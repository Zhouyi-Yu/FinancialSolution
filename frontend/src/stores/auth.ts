import { defineStore } from 'pinia'
import axios from 'axios'
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'

export const useAuthStore = defineStore('auth', () => {
    const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))
    const token = ref(localStorage.getItem('token'))
    const router = useRouter()

    const isAuthenticated = computed(() => !!token.value)

    // Initialize axios header if token exists
    if (token.value) {
        axios.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
    }

    // Add interceptor to handle 401s
    axios.interceptors.response.use(
        response => response,
        error => {
            if (error.response?.status === 401) {
                logout()
                router.push('/login')
            }
            return Promise.reject(error)
        }
    )

    function setAuth(data: { token: string; user: any }) {
        token.value = data.token
        user.value = data.user
        localStorage.setItem('token', data.token)
        localStorage.setItem('user', JSON.stringify(data.user))
        axios.defaults.headers.common['Authorization'] = `Bearer ${data.token}`
    }

    function logout() {
        token.value = null
        user.value = null
        localStorage.removeItem('token')
        localStorage.removeItem('user')
        delete axios.defaults.headers.common['Authorization']
        // Redirect to login handled by component or router
    }

    return { user, token, isAuthenticated, setAuth, logout }
})
