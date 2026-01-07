<script setup lang="ts">
defineProps<{
  title: string
}>()
</script>

<template>
  <div class="bg-finance-card rounded-lg p-6 shadow-lg border border-gray-800">
    <h3 class="text-finance-muted text-sm font-medium uppercase tracking-wider mb-2">{{ title }}</h3>
    <slot></slot>
  </div>
</template>
