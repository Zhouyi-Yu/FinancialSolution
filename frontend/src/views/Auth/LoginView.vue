<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '../../stores/auth'
import { useRouter } from 'vue-router'
import axios from 'axios'

const email = ref('')
const password = ref('')
const error = ref('')
const auth = useAuthStore()
const router = useRouter()

async function handleLogin() {
    try {
        const response = await axios.post('http://localhost:5194/api/auth/login', {
            email: email.value,
            password: password.value
        })
        auth.setAuth(response.data)
        router.push('/')
    } catch (err: any) {
        error.value = err.response?.data?.message || 'Login failed'
    }
}
</script>

<template>
  <div class="flex items-center justify-center min-h-screen bg-gray-100">
    <div class="px-8 py-6 mt-4 text-left bg-white shadow-lg rounded-lg w-96">
      <h3 class="text-2xl font-bold text-center text-blue-600">Login to Your Budget</h3>
      <form @submit.prevent="handleLogin">
        <div class="mt-4">
          <label class="block" for="email">Email</label>
          <input v-model="email" type="text" placeholder="Email"
                 class="w-full px-4 py-2 mt-2 border rounded-md focus:outline-none focus:ring-1 focus:ring-blue-600">
        </div>
        <div class="mt-4">
          <label class="block">Password</label>
          <input v-model="password" type="password" placeholder="Password"
                 class="w-full px-4 py-2 mt-2 border rounded-md focus:outline-none focus:ring-1 focus:ring-blue-600">
        </div>
        <div class="flex items-baseline justify-between">
          <button class="px-6 py-2 mt-4 text-white bg-blue-600 rounded-lg hover:bg-blue-900">Login</button>
        </div>
        <p v-if="error" class="mt-2 text-red-500 text-sm">{{ error }}</p>
      </form>
    </div>
  </div>
</template>
