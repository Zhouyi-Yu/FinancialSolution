<script setup lang="ts">
import { RouterView } from 'vue-router'
import DebugConsole from './components/DebugConsole.vue'
</script>

<template>
  <div class="min-h-screen bg-finance-bg text-finance-text antialiased font-sans">
    <RouterView />
    <DebugConsole />
  </div>
</template>
