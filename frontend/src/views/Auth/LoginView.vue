<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '../../stores/auth'
import { useRouter } from 'vue-router'
import axios from 'axios'

const email = ref('')
const password = ref('')
const error = ref('')
const isLoading = ref(false)
const auth = useAuthStore()
const router = useRouter()

async function handleLogin() {
    if (!email.value || !password.value) {
        error.value = 'Please enter both email and password.'
        return
    }
    
    isLoading.value = true
    error.value = ''
    try {
        const response = await axios.post('http://localhost:5194/api/auth/login', {
            email: email.value,
            password: password.value
        })
        auth.setAuth(response.data)
        router.push('/')
    } catch (err: any) {
        error.value = err.response?.data?.message || 'Login failed. Please check your credentials.'
    } finally {
        isLoading.value = false
    }
}
</script>

<template>
  <div class="flex items-center justify-center min-h-screen bg-finance-bg p-4 font-sans antialiased">
    <div class="w-full max-w-md bg-finance-card border border-white/10 shadow-[0_20px_50px_rgba(0,0,0,0.5)] rounded-3xl p-8 md:p-10">
      <div class="text-center mb-10">
        <h1 class="text-4xl font-extrabold text-white tracking-tight mb-3">Welcome Back</h1>
        <p class="text-finance-muted font-medium">Securely access your financial insights</p>
      </div>

      <form @submit.prevent="handleLogin" class="space-y-6">
        <div>
          <label for="email" class="block text-sm font-semibold text-finance-muted mb-2 ml-1">Email Address</label>
          <input 
            id="email"
            v-model="email" 
            type="email" 
            placeholder="name@example.com"
            required
            class="w-full bg-finance-bg border border-white/10 rounded-2xl px-5 py-4 text-white placeholder:text-white/20 focus:outline-none focus:ring-2 focus:ring-finance-cyan/40 focus:border-finance-cyan transition-all duration-200"
          >
        </div>

        <div>
          <div class="flex items-center justify-between mb-2 ml-1">
            <label for="password" class="block text-sm font-semibold text-finance-muted">Password</label>
            <a href="#" class="text-xs font-bold text-finance-cyan hover:text-white transition-colors duration-200">Forgot password?</a>
          </div>
          <input 
            id="password"
            v-model="password" 
            type="password" 
            placeholder="••••••••"
            required
            class="w-full bg-finance-bg border border-white/10 rounded-2xl px-5 py-4 text-white placeholder:text-white/20 focus:outline-none focus:ring-2 focus:ring-finance-cyan/40 focus:border-finance-cyan transition-all duration-200"
          >
        </div>

        <div v-if="error" class="bg-finance-red/10 border border-finance-red/20 text-finance-red px-5 py-3 rounded-2xl text-sm font-medium animate-in fade-in slide-in-from-top-2">
          {{ error }}
        </div>

        <button 
          type="submit" 
          :disabled="isLoading"
          class="w-full bg-finance-cyan hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed text-black font-black uppercase tracking-wider py-4 px-6 rounded-2xl transition-all duration-300 shadow-xl shadow-finance-cyan/10 active:scale-95 flex items-center justify-center space-x-3"
        >
          <span v-if="isLoading" class="animate-spin rounded-full h-5 w-5 border-[3px] border-black border-t-transparent"></span>
          <span>{{ isLoading ? 'Authenticating...' : 'Login' }}</span>
        </button>

        <div class="relative my-8">
          <div class="absolute inset-0 flex items-center">
            <div class="w-full border-t border-white/5"></div>
          </div>
          <div class="relative flex justify-center text-xs uppercase">
            <span class="bg-finance-card px-3 text-finance-muted font-bold">New to Budget?</span>
          </div>
        </div>

        <router-link 
          to="/register" 
          class="w-full flex items-center justify-center py-4 px-6 rounded-2xl border-2 border-white/10 text-white font-bold hover:bg-white/5 transition-all duration-200"
        >
          Create an Account
        </router-link>
      </form>
    </div>
  </div>
</template>
