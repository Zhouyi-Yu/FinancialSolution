<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from 'vue'

interface LogEntry {
  id: number
  timestamp: string
  type: 'log' | 'info' | 'warn' | 'error'
  message: string
  args: any[]
}

const logs = ref<LogEntry[]>([])
const isOpen = ref(false)
const logContainer = ref<HTMLElement | null>(null)
let originalConsole: any = {}

function formatArgs(args: any[]) {
  return args.map(arg => {
    if (typeof arg === 'object') {
      try {
        return JSON.stringify(arg, null, 2)
      } catch (e) {
        return String(arg)
      }
    }
    return String(arg)
  }).join(' ')
}

function captureLog(type: 'log' | 'info' | 'warn' | 'error', args: any[]) {
  const now = new Date()
  const timestamp = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}:${now.getSeconds().toString().padStart(2, '0')}`
  
  logs.value.push({
    id: Date.now() + Math.random(),
    timestamp,
    type,
    message: formatArgs(args),
    args
  })

  // Keep last 100 logs
  if (logs.value.length > 100) {
    logs.value.shift()
  }

  // Auto-scroll
  if (isOpen.value) {
    nextTick(() => {
      if (logContainer.value) {
        logContainer.value.scrollTop = logContainer.value.scrollHeight
      }
    })
  }

  // Call original
  if (originalConsole[type]) {
    originalConsole[type](...args)
  }
}

onMounted(() => {
  // Monkey patch console
  const types = ['log', 'info', 'warn', 'error'] as const
  types.forEach(type => {
    originalConsole[type] = console[type]
    console[type] = (...args) => captureLog(type, args)
  })
  
  // Add initial log
  console.log('Debugger initialized')
})

onUnmounted(() => {
  // Restore console
  const types = ['log', 'info', 'warn', 'error'] as const
  types.forEach(type => {
    if (originalConsole[type]) {
      console[type] = originalConsole[type]
    }
  })
})

function clearLogs() {
  logs.value = []
}

function toggle() {
  isOpen.value = !isOpen.value
}
</script>

<template>
  <div class="fixed bottom-4 right-4 z-[9999] font-mono text-xs">
    <!-- Toggle Button -->
    <button 
      @click="toggle"
      class="bg-gray-900 border border-gray-700 text-finance-cyan px-3 py-2 rounded-lg shadow-lg hover:bg-gray-800 transition-colors flex items-center space-x-2"
    >
      <span class="w-2 h-2 rounded-full" :class="logs.some(l => l.type === 'error') ? 'bg-red-500 animate-pulse' : 'bg-green-500'"></span>
      <span>Debug Console ({{ logs.length }})</span>
    </button>

    <!-- Console Window -->
    <div 
      v-if="isOpen"
      class="absolute bottom-12 right-0 w-[500px] h-[400px] bg-gray-900 border border-gray-700 rounded-lg shadow-2xl flex flex-col overflow-hidden"
    >
      <!-- Header -->
      <div class="flex items-center justify-between px-3 py-2 bg-gray-800 border-b border-gray-700">
        <span class="font-bold text-gray-300">Console Output</span>
        <div class="flex space-x-2">
          <button @click="clearLogs" class="text-gray-400 hover:text-white">Clear</button>
          <button @click="toggle" class="text-gray-400 hover:text-white">✕</button>
        </div>
      </div>

      <!-- Logs -->
      <div 
        ref="logContainer"
        class="flex-1 overflow-y-auto p-3 space-y-1 bg-black/90 text-gray-300"
      >
        <div v-if="logs.length === 0" class="text-gray-600 italic">No logs yet...</div>
        <div 
          v-for="log in logs" 
          :key="log.id" 
          class="flex space-x-2 font-mono hover:bg-white/5 p-0.5 rounded"
          :class="{
            'text-red-400': log.type === 'error',
            'text-yellow-400': log.type === 'warn',
            'text-blue-400': log.type === 'info',
            'text-gray-300': log.type === 'log'
          }"
        >
          <span class="text-gray-600 shrink-0">[{{ log.timestamp }}]</span>
          <span class="break-all whitespace-pre-wrap">{{ log.message }}</span>
        </div>
      </div>
    </div>
  </div>
</template>
