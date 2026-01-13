<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const displayName = ref('')
const error = ref('')
const isLoading = ref(false)
const router = useRouter()

async function handleRegister() {
    if (password.value !== confirmPassword.value) {
        error.value = 'Passwords do not match'
        return
    }
    
    isLoading.value = true
    error.value = ''
    try {
        await axios.post('http://localhost:5194/api/auth/register', {
            email: email.value,
            password: password.value,
            displayName: displayName.value
        })
        router.push('/login?registered=true')
    } catch (err: any) {
        error.value = err.response?.data?.message || 'Registration failed'
    } finally {
        isLoading.value = false
    }
}
</script>

<template>
  <div class="flex items-center justify-center min-h-screen bg-finance-bg p-4 font-sans antialiased">
    <div class="w-full max-w-md bg-finance-card border border-white/10 shadow-[0_20px_50px_rgba(0,0,0,0.5)] rounded-3xl p-8 md:p-10">
      <div class="text-center mb-10">
        <h1 class="text-4xl font-extrabold text-white tracking-tight mb-3">Create Account</h1>
        <p class="text-finance-muted font-medium">Start your journey to financial clarity</p>
      </div>

      <form @submit.prevent="handleRegister" class="space-y-5">
        <div>
          <label for="displayName" class="block text-sm font-semibold text-finance-muted mb-2 ml-1">Full Name</label>
          <input 
            id="displayName"
            v-model="displayName" 
            type="text" 
            placeholder="John Doe"
            required
            class="w-full bg-finance-bg border border-white/10 rounded-2xl px-5 py-4 text-white placeholder:text-white/20 focus:outline-none focus:ring-2 focus:ring-finance-green/40 focus:border-finance-green transition-all duration-200"
          >
        </div>

        <div>
          <label for="email" class="block text-sm font-semibold text-finance-muted mb-2 ml-1">Email Address</label>
          <input 
            id="email"
            v-model="email" 
            type="email" 
            placeholder="name@example.com"
            required
            class="w-full bg-finance-bg border border-white/10 rounded-2xl px-5 py-4 text-white placeholder:text-white/20 focus:outline-none focus:ring-2 focus:ring-finance-green/40 focus:border-finance-green transition-all duration-200"
          >
        </div>

        <div>
          <label for="password" class="block text-sm font-semibold text-finance-muted mb-2 ml-1">Password</label>
          <input 
            id="password"
            v-model="password" 
            type="password" 
            placeholder="••••••••"
            required
            class="w-full bg-finance-bg border border-white/10 rounded-2xl px-5 py-4 text-white placeholder:text-white/20 focus:outline-none focus:ring-2 focus:ring-finance-green/40 focus:border-finance-green transition-all duration-200"
          >
        </div>

        <div>
          <label for="confirmPassword" class="block text-sm font-semibold text-finance-muted mb-2 ml-1">Confirm Password</label>
          <input 
            id="confirmPassword"
            v-model="confirmPassword" 
            type="password" 
            placeholder="••••••••"
            required
            class="w-full bg-finance-bg border border-white/10 rounded-2xl px-5 py-4 text-white placeholder:text-white/20 focus:outline-none focus:ring-2 focus:ring-finance-green/40 focus:border-finance-green transition-all duration-200"
          >
        </div>

        <div v-if="error" class="bg-finance-red/10 border border-finance-red/20 text-finance-red px-5 py-3 rounded-2xl text-sm font-medium">
          {{ error }}
        </div>

        <button 
          type="submit" 
          :disabled="isLoading"
          class="w-full bg-finance-green hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed text-black font-black uppercase tracking-wider py-4 px-6 rounded-2xl transition-all duration-300 shadow-xl shadow-finance-green/10 active:scale-95 flex items-center justify-center space-x-3 mt-4"
        >
          <span v-if="isLoading" class="animate-spin rounded-full h-5 w-5 border-[3px] border-black border-t-transparent"></span>
          <span>{{ isLoading ? 'Creating account...' : 'Sign Up' }}</span>
        </button>

        <p class="text-center text-finance-muted text-sm mt-8">
          Already have an account? 
          <router-link to="/login" class="text-finance-green font-semibold hover:underline">Log in</router-link>
        </p>
      </form>
    </div>
  </div>
</template>
