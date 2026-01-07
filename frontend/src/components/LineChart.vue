<script setup lang="ts">
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js'
import { Line } from 'vue-chartjs'
import { computed } from 'vue'

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
)

const props = defineProps<{
  data: number[]
  labels: string[]
  color?: string
}>()

const chartData = computed(() => ({
  labels: props.labels,
  datasets: [
    {
      label: 'Value',
      backgroundColor: props.color || '#00D1FF',
      borderColor: props.color || '#00D1FF',
      data: props.data,
      tension: 0.4,
      pointRadius: 0
    }
  ]
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: { 
      mode: 'index',
      intersect: false,
      backgroundColor: '#161B30',
      titleColor: '#FFFFFF',
      bodyColor: '#8A94A6',
      borderColor: '#374151',
      borderWidth: 1
    }
  },
  scales: {
    x: { 
      display: true, 
      grid: { display: false, drawBorder: false },
      ticks: { color: '#8A94A6', font: { size: 10 } }
    },
    y: { 
      display: true, 
      grid: { color: '#2B3544', drawBorder: false },
      ticks: { color: '#8A94A6', font: { size: 10 } }
    }
  },
  interaction: {
    mode: 'nearest',
    axis: 'x',
    intersect: false
  }
}
</script>

<template>
  <div class="h-64">
    <Line :data="chartData" :options="chartOptions" />
  </div>
</template>
