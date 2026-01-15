<script setup lang="ts">
defineProps<{
  title: string
}>()
</script>

<template>
  <div class="bg-finance-card rounded-lg p-6 shadow-lg border border-gray-800">
    <div class="flex justify-between items-center mb-2">
      <h3 class="text-finance-muted text-sm font-medium uppercase tracking-wider">{{ title }}</h3>
      <slot name="extra"></slot>
    </div>
    <slot></slot>
  </div>
</template>
